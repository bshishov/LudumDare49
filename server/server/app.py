from typing import Set, Optional
import asyncio
import logging
from contextlib import asynccontextmanager
import datetime
from uuid import uuid4

import server.messages as msg
from server.db import IPlayerDatabase, PlayerDbEntry, DivisionDbEntry
from server.game import *

__all__ = [
    "PlayerConnection",
    "Application",
]


_logger = logging.getLogger(__name__)


def random_id() -> str:
    return str(uuid4())[:8]


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
            await handler(connection, message)
        else:
            _logger.warning(f"Invalid player message {message_type}")
            connection.send(msg.ServerError(error="invalid_message"))

    async def on_player_hello(self, connection: PlayerConnection, message: msg.ClientHello):
        if connection.player_entry is not None:
            connection.send(msg.ServerError(error="already_authorized"))
            return

        # Authorize
        entry = self._db.find(message.token)
        if entry is not None:
            player = entry.player
        else:
            # Assign division
            start_league_id = "wooden"
            player_id = message.token

            found_division = None
            for division in self._db.iter_league_divisions(start_league_id):
                if len(division.player_ids) < 10:
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

            player = self._game.create_new_player(message.username.strip(), division.id)
            entry = PlayerDbEntry(
                key=player_id,
                auth_token=message.token,
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

    async def on_player_accept(self, connection: PlayerConnection, message: msg.ClientAcceptRoll):
        try:
            async with self.player_change(connection) as player:
                self._game.accept_roll(player)
                connection.send(msg.ServerRollDecided(player, accepted=True))
        except (NotAuthorizedError, GameError) as err:
            _logger.warning(err)
            connection.send(msg.ServerError(error=err.error_code))

    async def on_player_decline(self, connection: PlayerConnection, message: msg.ClientDeclineRoll):
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
        standings = self._game.generate_division_standings()
        player = connection.player
        if player is None:
            connection.send(msg.ServerError(error=NotAuthorizedError().error_code))
            return

        players_in_division = []
        division = self._db.get_division(player.division_id)
        if division:
            for player_id in division.player_ids:
                p = self._db.find(player_id)
                if p is not None:  # Might be none if invalid key
                    players_in_division.append(p.player)

        players_in_division = sorted(players_in_division, key=lambda i: i.power, reverse=True)
        division_standing_players = []
        for rank, p in enumerate(players_in_division):
            division_standing_players.append(DivisionPlayer(
                username=p.username,
                rank=rank+1,
                power=p.total_power
            ))

        connection.send(msg.ServerDivisionInfo(standings=standings))

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
        interval_seconds = 10

        while True:
            await asyncio.sleep(interval_seconds)
            _logger.info("League update")

            for connection in self._active_connections:
                player = connection.player
                if not player:
                    continue

                # TODO: UPDATE
                _logger.info("League update not implemented yet")
