from typing import Dict, Type
import json
import datetime

from attr import dataclass

from server.spec import spec
from server.game import Player, RolledItem


__all__ = [
    # Player Messages
    "PlayerHello",
    "PlayerRoll",
    "PlayerAcceptRoll",
    "PlayerDeclineRoll",

    # Server Messages
    "ServerHello",
    "ServerAuthFailed",
    "ServerRollSuccess",
    "ServerRollFailed",
    "ServerGoldUpdated",
    "ServerRollDecided",

    # ABC
    "PlayerMessage",
    "ServerMessage",

    # Conversion
    "str_to_player_message",
    "str_to_server_message",
    "server_message_to_str",
    "player_message_to_str",
]


class PlayerMessage:
    pass


class ServerMessage:
    pass


_PLAYER_MESSAGE_TYPES: Dict[str, Type[PlayerMessage]] = {}
_SERVER_MESSAGE_TYPES: Dict[str, Type[ServerMessage]] = {}


def register_message(name: str):
    def _inner(cls):
        if issubclass(cls, PlayerMessage):
            _PLAYER_MESSAGE_TYPES[name] = cls
        elif issubclass(cls, ServerMessage):
            _SERVER_MESSAGE_TYPES[name] = cls
        else:
            raise TypeError(f"Unknown type: {cls}")
        return cls
    return _inner


@register_message("hello")
@dataclass(slots=True)
class PlayerHello(PlayerMessage):
    username: str
    token: str


@register_message("roll")
@dataclass(slots=True)
class PlayerRoll(PlayerMessage):
    merchant: str


@register_message("accept")
@dataclass(slots=True)
class PlayerAcceptRoll(PlayerMessage):
    pass


@register_message("accept")
@dataclass(slots=True)
class PlayerDeclineRoll(PlayerMessage):
    pass


@register_message("hello")
@dataclass(slots=True)
class ServerHello(ServerMessage):
    player_state: Player


@register_message("auth_failed")
@dataclass(slots=True)
class ServerAuthFailed(ServerMessage):
    reason: int


@register_message("gold_updated")
@dataclass(slots=True)
class ServerGoldUpdated(ServerMessage):
    old_gold: int
    new_gold: int
    next_update_time: datetime.datetime


@register_message("roll_failed")
@dataclass(slots=True)
class ServerRollFailed(ServerMessage):
    reason: str


@register_message("roll_success")
@dataclass(slots=True)
class ServerRollSuccess(ServerMessage):
    rolled_item: RolledItem
    player_state: Player


@register_message("decided")
@dataclass(slots=True)
class ServerRollDecided(ServerMessage):
    player_state: Player


_REVERSED_PLAYER_MESSAGE_TYPES = {v: k for k, v in _PLAYER_MESSAGE_TYPES.items()}
_REVERSED_SERVER_MESSAGE_TYPES = {v: k for k, v in _SERVER_MESSAGE_TYPES.items()}


def str_to_player_message(message: str) -> PlayerMessage:
    data = json.loads(message)
    message_type = data.pop("type")
    model_cls = _PLAYER_MESSAGE_TYPES[message_type]
    return spec.load(model_cls, data)


def str_to_server_message(message: str) -> ServerMessage:
    data = json.loads(message)
    message_type = data.pop("type")
    model_cls = _SERVER_MESSAGE_TYPES[message_type]
    return spec.load(model_cls, data)


def player_message_to_str(message: PlayerMessage) -> str:
    data = spec.dump(message)
    data["type"] = _REVERSED_PLAYER_MESSAGE_TYPES[type(message)]
    return json.dumps(data)


def server_message_to_str(message: ServerMessage) -> str:
    data = spec.dump(message)
    data["type"] = _REVERSED_SERVER_MESSAGE_TYPES[type(message)]
    return json.dumps(data)