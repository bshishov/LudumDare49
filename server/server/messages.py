from typing import Dict, Type
import json
from datetime import datetime

from attr import dataclass, attrib
from attr.validators import instance_of

from server.spec import spec
from server.game import Player, RolledItem


__all__ = [
    # Player Messages
    "ClientHello",
    "ClientRoll",
    "ClientAcceptRoll",
    "ClientDeclineRoll",

    # Server Messages
    "ServerHello",
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
class ClientHello(PlayerMessage):
    username: str = attrib(validator=instance_of(str))
    token: str = attrib(validator=instance_of(str))


@register_message("roll")
@dataclass(slots=True)
class ClientRoll(PlayerMessage):
    merchant: str = attrib(validator=instance_of(str))


@register_message("accept_roll")
@dataclass(slots=True)
class ClientAcceptRoll(PlayerMessage):
    pass


@register_message("decline_roll")
@dataclass(slots=True)
class ClientDeclineRoll(PlayerMessage):
    pass


@register_message("server_hello")
@dataclass(slots=True)
class ServerHello(ServerMessage):
    player: Player


@register_message("gold_updated")
@dataclass(slots=True)
class ServerGoldUpdated(ServerMessage):
    old_gold: int = attrib(validator=instance_of(int))
    new_gold: int = attrib(validator=instance_of(int))
    next_update_time: datetime = attrib(validator=instance_of(datetime))


@register_message("roll_failed")
@dataclass(slots=True)
class ServerRollFailed(ServerMessage):
    error: str


@register_message("roll_success")
@dataclass(slots=True)
class ServerRollSuccess(ServerMessage):
    rolled_item: RolledItem
    player: Player


@register_message("roll_decided")
@dataclass(slots=True)
class ServerRollDecided(ServerMessage):
    player: Player


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
