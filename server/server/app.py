from typing import Set, Optional, List, Dict
import asyncio
import logging
from contextlib import asynccontextmanager
import datetime
from uuid import uuid4
from collections import defaultdict

import server.messages as msg
from server.db import *
from server.game import *

__all__ = [
    "PlayerConnection",
    "Application",
]


_logger = logging.getLogger(__name__)


def random_id() -> str:
    return str(uuid4())[:8]


LEAGUE_LOCK = asyncio.Lock()


class NotAuthorizedError(Exception):
    def __init__(self):
        self.error_code = "not_authorized"
        super().__init__(self.error_code)


class PlayerConnection:
    def __init__(self, socket_identifier: str):
        self._socket_identifier = socket_identifier
        self.message_queue: asyncio.Queue[msg.ServerMessage] = asyncio.Queue()
        self._player_entry: Optional[PlayerDbEntry] = None

    def set_player_entry(self, player: PlayerDbEntry):
        self._player_entry = player

    @property
    def player(self) -> Optional[Player]:
        if not self._player_entry:
            return None
        return self._player_entry.player

    @property
    def player_entry(self) -> Optional[PlayerDbEntry]:
        return self._player_entry

    def send(self, message: msg.ServerMessage):
        self.message_queue.put_nowait(message)

    async def deque_message(self) -> msg.ServerMessage:
        return await self.message_queue.get()

    def __hash__(self):
        return hash(self._socket_identifier)

    def __str__(self):
        return f"Player {self._socket_identifier}"


class Application:
    def __init__(self, game: Game, db: IPlayerDatabase):
        self._game = game
        self._active_connections: Set[PlayerConnection] = set()
        self._db = db
        self._message_handlers = {
            msg.ClientHello: self.on_player_hello,
            msg.ClientRoll: self.on_player_roll,
            msg.ClientAcceptRoll: self.on_player_accept,
            msg.ClientDeclineRoll: self.on_player_decline,
            msg.ClientDivisionInfoRequest: self.on_client_division_info_request,
        }
        self._next_league_update_at = (
                datetime.datetime.now() +
                datetime.timedelta(seconds=self._game.settings.league_update_interval_seconds)
        )

    @asynccontextmanager
    async def player_change(self, connection: PlayerConnection) -> Player:
        if not connection.player_entry:
            raise NotAuthorizedError
        player = connection.player
        yield player
        self._db.save(connection.player_entry)

    async def on_player_connected(self, connection: PlayerConnection):
        _logger.info(f"{connection} connected")
        self._active_connections.add(connection)

    async def on_player_left(self, player: PlayerConnection):
        _logger.info(f"{player} left")
        self._active_connections.remove(player)

    async def on_player_message(self, connection: PlayerConnection, message: msg.PlayerMessage):
        _logger.info(f"{connection} sent: {message}")

        message_type = type(message)
        handler = self._message_handlers.get(message_type)
        if handler:
            async with LEAGUE_LOCK:
                await handler(connection, message)
        else:
            _logger.warning(f"Invalid player message {message_type}")
            connection.send(msg.ServerError(error="invalid_message"))

    async def on_player_hello(self, connection: PlayerConnection, message: msg.ClientHello):
        if connection.player_entry is not None:
            connection.send(msg.ServerError(error="already_authorized"))
            return

        username = message.username.strip()
        token = message.token.strip()

        if not username:
            connection.send(msg.ServerError(error="empty_username"))
            return

        if not token:
            connection.send(msg.ServerError(error="empty_token"))
            return

        # Authorize
        entry = self._db.find(token)
        if entry is not None:
            player = entry.player
            player.username = username  # Renaming
        else:
            # Assign division
            start_league_id = self._game.settings.starting_league
            player_id = token

            found_division = None
            for division in self._db.iter_league_divisions(start_league_id):
                if len(division.player_ids) < self._game.settings.max_players_per_division:
                    found_division = division
                    break

            if not found_division:
                division = DivisionDbEntry(
                    id=random_id(),
                    player_ids=[],
                    league_id=start_league_id
                )
                _logger.info(f"New division {division}")
            else:
                division = found_division

            division.player_ids.append(player_id)
            _logger.info(f"Assigned player {player_id} to division {division.id}")
            self._db.save_division(division)

            player = self._game.create_new_player(username, division.id, start_league_id)
            entry = PlayerDbEntry(
                key=player_id,
                auth_token=token,
                player=player
            )

        connection.set_player_entry(entry)
        self._db.save(connection.player_entry)
        connection.send(msg.ServerHello(player))

    async def on_player_roll(self, connection: PlayerConnection, message: msg.ClientRoll):
        try:
            async with self.player_change(connection) as player:
                self._game.roll_item_for_player(player, message.merchant)
                connection.send(msg.ServerRollSuccess(
                    rolled_item=player.current_undecided_roll_item,
                    player=player
                ))
        except (NotAuthorizedError, GameError) as err:
            _logger.warning(err)
            connection.send(msg.ServerError(error=err.error_code))

    async def on_player_accept(
            self,
            connection: PlayerConnection,
            message: msg.ClientAcceptRoll
    ):
        try:
            async with self.player_change(connection) as player:
                self._game.accept_roll(player)
                connection.send(msg.ServerRollDecided(player, accepted=True))
        except (NotAuthorizedError, GameError) as err:
            _logger.warning(err)
            connection.send(msg.ServerError(error=err.error_code))

    async def on_player_decline(
            self,
            connection: PlayerConnection,
            message: msg.ClientDeclineRoll
    ):
        try:
            async with self.player_change(connection) as player:
                self._game.decline_roll(player)
                connection.send(msg.ServerRollDecided(player, accepted=False))
        except (NotAuthorizedError, GameError) as err:
            _logger.warning(err)
            connection.send(msg.ServerError(error=err.error_code))

    async def on_client_division_info_request(
            self,
            connection: PlayerConnection,
            message: msg.ClientDivisionInfoRequest
    ):
        player = connection.player
        if player is None:
            connection.send(msg.ServerError(error=NotAuthorizedError().error_code))
            return

        try:
            division_info = get_division_info(player, self._db, self._next_league_update_at)
            connection.send(msg.ServerDivisionInfo(division_info))
        except Exception as err:
            _logger.error(f"Failed to calculate division info {err}", exc_info=True)
            connection.send(msg.ServerError(error="division_info_error"))

    async def gold_update_routine(self):
        interval = self._game.income_update_interval

        while True:
            await asyncio.sleep(interval)
            _logger.info("Gold update")
            next_update_time = datetime.datetime.utcnow() + datetime.timedelta(seconds=interval)
            for connection in self._active_connections:
                player = connection.player
                if not player:
                    continue

                try:
                    async with self.player_change(connection) as player:
                        old_gold = player.gold
                        self._game.update_player_gold(player)
                        new_gold = player.gold

                        connection.send(msg.ServerGoldUpdated(
                            old_gold=old_gold,
                            new_gold=new_gold,
                            next_update_time=next_update_time
                        ))
                except (NotAuthorizedError, GameError) as err:
                    _logger.warning(err)
                    connection.send(msg.ServerError(error=err.error_code))
                except Exception as err:
                    _logger.error(err)

    async def league_update_routine(self):
        interval_seconds = self._game.settings.league_update_interval_seconds

        while True:
            await asyncio.sleep(interval_seconds)
            self._next_league_update_at = (
                    datetime.datetime.now() +
                    datetime.timedelta(seconds=interval_seconds)
            )
            _logger.info("League update")

            async with LEAGUE_LOCK:
                _logger.info("Calculating leagues")
                try:
                    update_leagues(self._game, self._db)
                except Exception as err:
                    _logger.error(f"Failed to calculate league: {err}", exc_info=True)

                _logger.info("Notifying connected players")
                for connection in self._active_connections:
                    # Not authorized yet
                    db_player = connection.player_entry
                    if not db_player:
                        continue

                    # Update player since we changed it
                    db_player = self._db.find(db_player.key)
                    connection.set_player_entry(db_player)

                    if not db_player.player:
                        continue

                    try:
                        division_info = get_division_info(
                            player=db_player.player,
                            db=self._db,
                            next_update_at=self._next_league_update_at
                        )
                        connection.send(msg.ServerDivisionInfo(division_info))
                    except Exception as err:
                        _logger.error(f"Failed to calculate division info: {err}", exc_info=True)
                        connection.send(msg.ServerError(error="division_info_error"))


def get_division_info(
        player: Player,
        db: IPlayerDatabase,
        next_update_at: datetime.datetime
) -> DivisionInfo:
    players_in_division: List[Player] = []
    division = db.get_division(player.division_id)
    if division:
        for player_id in division.player_ids:
            p = db.find(player_id)
            if p is not None:  # Might be none if invalid key
                players_in_division.append(p.player)

    players_in_division = sorted(
        players_in_division,
        key=lambda i: i.total_power,
        reverse=True
    )
    division_standing_players = []
    for rank, p in enumerate(players_in_division):
        division_standing_players.append(DivisionPlayer(
            username=p.username,
            rank=rank + 1,
            power=p.total_power
        ))

    return DivisionInfo(
        division_id=division.id,
        league_id=player.league_id,
        players=division_standing_players,
        next_update_at=next_update_at
    )


def update_leagues(game: Game, db: IPlayerDatabase):
    _logger.info("Started calculation update")
    default_league = game.get_league(game.settings.starting_league)
    players_by_league: Dict[str, List[PlayerDbEntry]] = defaultdict(list)

    for division in db.iter_all_divisions():
        current_league = game.get_league(division.league_id) or default_league
        players_in_division: List[PlayerDbEntry] = []

        for player_id in division.player_ids:
            db_player = db.find(player_id)
            if not db_player:
                continue  # invalid / unknown player
            players_in_division.append(db_player)

        # Sort by total power descending
        players_in_division: List[PlayerDbEntry] = sorted(
            players_in_division,
            key=lambda p: p.player.total_power,
            reverse=False
        )

        for rank, db_player in enumerate(players_in_division):
            target_league_id = None  # same league

            if rank < current_league.n_best_players:
                # Upgrade
                target_league_id = current_league.next_league_id
            elif rank > len(players_in_division) - current_league.n_worst_players:
                # Downgrade
                target_league_id = current_league.prev_league_id

            # Rewards
            if rank < len(current_league.gold_rewards_for_rank):
                reward = current_league.gold_rewards_for_rank[rank]
                db_player.player.gold += reward

            if target_league_id is None:
                target_league_id = current_league.id

            players_by_league[target_league_id].append(db_player)

    _logger.info(f"Deleting divisions")
    db.clear_all_divisions()

    for league_id, db_players in players_by_league.items():
        league = game.get_league(league_id) or default_league
        _logger.info(f"processing league: {league.id} with {len(db_players)} players")

        division = DivisionDbEntry(id=random_id(), league_id=league.id, player_ids=[])

        for db_player in db_players:
            db_player.player.league_id = league.id
            db_player.player.division_id = division.id
            db.save(db_player)

            division.player_ids.append(db_player.key)

            if len(division.player_ids) >= game.settings.max_players_per_division:
                db.save_division(division)
                division = DivisionDbEntry(id=random_id(), league_id=league.id, player_ids=[])

        db.save_division(division)
