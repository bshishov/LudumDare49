from typing import Optional
from abc import ABCMeta, abstractmethod

from attr import dataclass
import pickledb

from server.game import Player
from server.spec import spec


__all__ = [
    "PlayerDbEntry",
    "IPlayerDatabase",
    "PickleDb",
]


@dataclass(slots=True)
class PlayerDbEntry:
    key: str
    auth_token: str
    player: Player


class IPlayerDatabase(metaclass=ABCMeta):
    @abstractmethod
    async def find(self, key: str) -> Optional[PlayerDbEntry]: ...

    @abstractmethod
    async def save(self, entry: PlayerDbEntry): ...


class PickleDb(IPlayerDatabase):
    def __init__(self, path: str):
        self._db = pickledb.load(path, auto_dump=True)

    async def find(self, uid: str) -> Optional[PlayerDbEntry]:
        data = self._db.get(uid)
        if not data:
            return None

        return spec.load(PlayerDbEntry, data)

    async def save(self, entry: PlayerDbEntry):
        key = entry.key
        data = spec.dump(entry)
        self._db[key] = data
