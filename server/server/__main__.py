from typing import Callable
import argparse
import asyncio
import logging
import logging.config
import os

from websockets import WebSocketServerProtocol, serve, ConnectionClosedError

from server.app import *
from server.messages import (
    PlayerMessage,
    ServerMessage,
    str_to_player_message,
    server_message_to_str
)
from server.db import PickleDb
from server.game import Game
from server.data import (
    load_merchants,
    load_items,
    load_game_settings,
    load_leagues
)

_logger = logging.getLogger(__name__)


class WebsocketHandler:
    def __init__(
            self,
            app: Application,
            player_message_converter: Callable[[str], PlayerMessage],
            server_message_converter: Callable[[ServerMessage], str],
    ):
        self.app = app
        self.player_message_converter = player_message_converter
        self.server_message_converter = server_message_converter

    async def handle(self, websocket: WebSocketServerProtocol, path: str):
        _logger.debug(f"Connected {websocket}: {path}")
        player = PlayerConnection(str(websocket.remote_address))
        await self.app.on_player_connected(player)
        try:
            consumer_task = asyncio.ensure_future(self._consumer_handler(websocket, player))
            producer_task = asyncio.ensure_future(self._producer_handler(websocket, player))
            done, pending = await asyncio.wait(
                (consumer_task, producer_task),
                return_when=asyncio.FIRST_COMPLETED
            )
            for task in pending:
                task.cancel()
        except ConnectionClosedError:
            pass
        except Exception as err:
            _logger.exception(err, exc_info=True)
        await self.app.on_player_left(player)

    async def _consumer_handler(self, websocket: WebSocketServerProtocol, player: PlayerConnection):
        try:
            async for message in websocket:
                if isinstance(message, bytes):
                    message = message.decode(encoding="utf-8")
                try:
                    player_message = self.player_message_converter(message)
                    await self.app.on_player_message(player, player_message)
                except Exception as err:
                    logging.exception(err, exc_info=True)
        except ConnectionClosedError:
            return

    async def _producer_handler(self, websocket: WebSocketServerProtocol, player: PlayerConnection):
        while True:
            server_message = await player.deque_message()
            message = self.server_message_converter(server_message)
            await websocket.send(message)


def main(host: str, port: int, data_path: str):
    logging_config = {
        'version': 1,
        'disable_existing_loggers': False,
        'formatters': {
            'detailed': {
                'class': 'logging.Formatter',
                'format': '[%(levelname)s] %(asctime)-15s %(module)s %(funcName)s: %(message)s'
            }
        },
        'handlers': {
            'console': {
                'class': 'logging.StreamHandler',
                'level': 'DEBUG',
                'formatter': 'detailed'
            },
        },
        'loggers': {},
        'root': {
            'level': 'DEBUG',
            'handlers': ['console'],
            'formatter': 'detailed'
        },
    }
    logging.config.dictConfig(logging_config)
    logging.getLogger('asyncio').setLevel(logging.ERROR)
    logging.getLogger('asyncio.coroutines').setLevel(logging.ERROR)
    logging.getLogger('websockets.server').setLevel(logging.ERROR)
    logging.getLogger('websockets.protocol').setLevel(logging.ERROR)

    _logger.info(f"Loading data from {data_path}")

    items = load_items(os.path.join(data_path, "items.json"))
    merchants = load_merchants(os.path.join(data_path, "merchants.json"))
    game_settings = load_game_settings(os.path.join(data_path, "game.json"))
    leagues = load_leagues(os.path.join(data_path, "leagues.json"))

    game = Game(
        settings=game_settings,
        merchants=merchants,
        items=items,
        leagues=leagues
    )

    players_db_path = os.environ.get("PLAYERS_DB_PATH", "players.db")
    divisions_db_path = os.environ.get("DIVISIONS_DB_PATH", "divisions.db")

    _logger.info(f"Loading db")
    application = Application(
        db=PickleDb(players_db_path, divisions_db_path),
        game=game
    )
    handler = WebsocketHandler(
        application,
        str_to_player_message,
        server_message_to_str
    )
    websocket_server = serve(handler.handle, host, port)

    _logger.info(f'Running server on {host}:{port}')
    asyncio.get_event_loop().run_until_complete(websocket_server)
    asyncio.get_event_loop().create_task(application.gold_update_routine())
    asyncio.get_event_loop().create_task(application.league_update_routine())
    _logger.info(f'Server started')
    asyncio.get_event_loop().run_forever()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=6789, help='Port')
    parser.add_argument('--host', type=str, default='0.0.0.0', help='WebSockets host')
    parser.add_argument('--data', type=str, default='data/', help='path to game data')
    arguments = parser.parse_args()
    main(arguments.host, arguments.port, arguments.data)
