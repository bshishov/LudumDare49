from typing import Set, Optional
import asyncio
import logging
from contextlib import asynccontextmanager
import datetime

import server.messages as msg
from server.db import IPlayerDatabase, PlayerDbEntry
from server.game import *

__all__ = [
    "PlayerConnection",
    "Application",
]


_logger = logging.getLogger(__name__)


class PlayerConnection:
    def __init__(self, socket_identifier: str):
        self._socket_identifier = socket_identifier
        self.message_queue: asyncio.Queue[msg.ServerMessage] = asyncio.Queue()
        self._player_entry: Optional[PlayerDbEntry] = None

    def set_player_entry(self, player: PlayerDbEntry):
        self._player_entry = player

    @property
    def player(self) -> Optional[Player]:
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
        self._player_db = db
        self._message_handlers = {
            msg.PlayerHello: self.on_player_hello,
            msg.PlayerRoll: self.on_player_roll,
            msg.PlayerAcceptRoll: self.on_player_accept,
            msg.PlayerDeclineRoll: self.on_player_decline,
        }

    @asynccontextmanager
    async def player_change(self, connection: PlayerConnection) -> Player:
        player = connection.player
        yield player
        await self._player_db.save(connection.player_entry)

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

    async def on_player_hello(self, connection: PlayerConnection, message: msg.PlayerHello):
        entry = await self._player_db.find(message.token)
        if entry is not None:
            player = entry.player
        else:
            player = self._game.create_new_player(message.token)
            entry = PlayerDbEntry(
                key=message.token,
                auth_token=message.token,
                player=player
            )

        connection.set_player_entry(entry)
        await self._player_db.save(connection.player_entry)
        connection.send(msg.ServerHello(player))

    async def on_player_roll(self, connection: PlayerConnection, message: msg.PlayerRoll):
        try:
            async with self.player_change(connection) as player:
                self._game.roll_item_for_player(player, message.merchant)
                connection.send(msg.ServerRollSuccess(
                    rolled_item=player.current_undecided_roll_item,
                    player_state=player
                ))
        except GameError as err:
            _logger.warning(err)
            connection.send(msg.ServerRollFailed(reason=err.error_code))

    async def on_player_accept(self, connection: PlayerConnection, message: msg.PlayerAcceptRoll):
        try:
            async with self.player_change(connection) as player:
                self._game.accept_roll(player)
                connection.send(msg.ServerRollDecided(player))
        except GameError as err:
            _logger.warning(err)

    async def on_player_decline(self, connection: PlayerConnection, message: msg.PlayerDeclineRoll):
        try:
            async with self.player_change(connection) as player:
                self._game.accept_roll(player)
                connection.send(msg.ServerRollDecided(player))
        except GameError as err:
            _logger.warning(err)

    async def gold_update_routine(self):
        while True:
            await asyncio.sleep(self._game.settings.income_period_seconds)
            next_update_time = (
                datetime.datetime.now()
                + datetime.timedelta(seconds=self._game.settings.income_period_seconds)
            )
            for connection in self._active_connections:
                player = connection.player
                if not player:
                    continue

                async with self.player_change(connection) as player:
                    old_gold = player.gold
                    self._game.update_player_gold(player)
                    new_gold = player.gold

                    connection.send(msg.ServerGoldUpdated(
                        old_gold=old_gold,
                        new_gold=new_gold,
                        next_update_time=next_update_time
                    ))
                    await self._player_db.save(connection.player_entry)
