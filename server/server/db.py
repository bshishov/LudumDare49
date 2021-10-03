from typing import Optional, Iterable, List, Type, TypeVar
from abc import ABCMeta, abstractmethod
import logging

from attr import dataclass
import pickledb

from server.game import Player
from server.spec import spec


__all__ = [
    "PlayerDbEntry",
    "IPlayerDatabase",
    "PickleDb",
    "DivisionDbEntry",
]


_logger = logging.getLogger(__name__)


@dataclass(slots=True)
class PlayerDbEntry:
    key: str
    auth_token: str
    player: Player


@dataclass(slots=True)
class DivisionDbEntry:
    id: str
    league_id: str
    player_ids: List[str]


class IPlayerDatabase(metaclass=ABCMeta):
    @abstractmethod
    def find(self, key: str) -> Optional[PlayerDbEntry]: ...

    @abstractmethod
    def save(self, entry: PlayerDbEntry): ...

    @abstractmethod
    def get_division(self, division_id: str) -> Optional[DivisionDbEntry]: ...

    @abstractmethod
    def iter_players_by_division(self, division_id: str) -> Iterable[PlayerDbEntry]: ...

    @abstractmethod
    def iter_league_divisions(self, league_id: str) -> Iterable[DivisionDbEntry]: ...

    @abstractmethod
    def save_division(self, division: DivisionDbEntry): ...


T = TypeVar("T")


def _try_get(db: pickledb.PickleDB, key: str, t: Type[T]) -> Optional[T]:
    data = db.get(key)
    if not data:
        return None

    try:
        return spec.load(t, data)
    except (TypeError, ValueError) as err:
        db.rem(key)
        _logger.warning(f"Corrupted data: {key}, {err}")
        return None


class PickleDb(IPlayerDatabase):
    def __init__(self, player_db_path: str, division_db_path: str):
        self._players_db = pickledb.load(player_db_path, auto_dump=True)
        self._division_db = pickledb.load(division_db_path, auto_dump=True)

    def find(self, uid: str) -> Optional[PlayerDbEntry]:
        return self._get_player(uid)

    def get_division(self, division_id: str) -> Optional[DivisionDbEntry]:
        return self._get_division(division_id)

    def _get_player(self, uid: str) -> Optional[PlayerDbEntry]:
        return _try_get(self._players_db, uid, PlayerDbEntry)

    def _get_division(self, division_id: str) -> Optional[DivisionDbEntry]:
        return _try_get(self._division_db, division_id, DivisionDbEntry)

    def iter_players_by_division(self, division_id: str) -> Iterable[PlayerDbEntry]:
        division = self._get_division(division_id)
        if division is None:
            return
        for player_id in division.player_ids:
            player = self._get_player(player_id)
            if player is not None:
                yield player

    def iter_league_divisions(self, league_id: str) -> Iterable[DivisionDbEntry]:
        for key in self._division_db.getall():
            division = self._get_division(key)
            if division:
                if division.league_id == league_id:
                    yield division

    def save(self, entry: PlayerDbEntry):
        key = entry.key
        self._players_db[key] = spec.dump(entry)

    def save_division(self, division: DivisionDbEntry):
        key = division.id
        self._division_db[key] = spec.dump(division)
