from typing import Optional, List, ClassVar, Mapping
from datetime import datetime, timedelta
from enum import Enum
import random

import numpy as np
from attr import dataclass

from server.data import (
    MerchantData,
    MerchantItemData,
    ItemData,
    GameSettingsData,
    Slot,
    LeagueData
)

__all__ = [
    "GameError",
    "Player",
    "RolledItem",
    "Game",
    "DivisionStandings",
    "DivisionPlayer"
]


class GameError(Exception):
    ERROR_CODE: ClassVar[str]

    def __init__(self):
        self.error_code: str = self.ERROR_CODE
        super().__init__(self.ERROR_CODE)


class UnknownMerchant(GameError):
    ERROR_CODE = "invalid_merchant"


class NotEnoughGold(GameError):
    ERROR_CODE = "not_enough_gold"


class RollIsAlreadyActive(GameError):
    ERROR_CODE = "roll_is_already_active"


class NoActiveRoll(GameError):
    ERROR_CODE = "no_active_roll"


class Quality(Enum):
    SHITTY = "shitty"
    BAD = "bad"
    NORMAL = "normal"
    GOOD = "good"
    DIVINE = "divine"


QUALITY_MULTIPLIER = {
    Quality.SHITTY: 0.7,
    Quality.BAD: 0.85,
    Quality.NORMAL: 1,
    Quality.GOOD: 1.2,
    Quality.DIVINE: 1.4
}


@dataclass
class RolledItem:
    item: ItemData
    quality: Quality
    obtained_at: datetime
    merchant: str
    total_power: int


@dataclass
class Player:
    username: str
    gold: int
    last_gold_update_time: datetime
    items: List[RolledItem]
    current_undecided_roll_item: Optional[RolledItem]
    # last_activity: datetime

    @property
    def total_power(self) -> int:
        power = 0
        for item in self.items:
            power += item.total_power
        return power

    def get_item_in_slot(self, slot: Slot) -> Optional[RolledItem]:
        for rolled_item in self.items:
            if rolled_item.item.slot == slot:
                return rolled_item
        return None


@dataclass(slots=True)
class DivisionPlayer:
    username: str
    rank: int
    power: int


@dataclass(slots=True)
class DivisionStandings:
    division_id: str
    league_id: str
    players: List[DivisionPlayer]
    next_update_at: datetime


class Game:
    def __init__(
            self,
            settings: GameSettingsData,
            merchants: List[MerchantData],
            items: List[ItemData],
            leagues: List[LeagueData]
    ):
        self._leagues = leagues
        self._settings = settings
        self._items: Mapping[str, ItemData] = {item.id: item for item in items}
        self._merchants: Mapping[str, MerchantData] = {m.id: m for m in merchants}

        for merchant in merchants:
            for item in merchant.items:
                assert item.item in self._items, f"Unknown item {item.item}"

    @property
    def income_update_interval(self):
        return self._settings.income_period_seconds

    def create_new_player(self, username: str) -> Player:
        return Player(
            username=username,
            gold=self._settings.initial_gold,
            items=[],
            last_gold_update_time=datetime.now(),
            current_undecided_roll_item=None
        )

    def update_player_gold(self, player: Player):
        player.last_gold_updated_time = datetime.now()
        player.gold += round(player.total_power * self._settings.power_income_multiplier)

    def roll_item(self, merchant: MerchantData) -> RolledItem:
        item_probabilities = np.array(list(i.probability for i in merchant.items), np.float32)
        item_probabilities /= np.sum(item_probabilities)

        merchant_item: MerchantItemData = np.random.choice(merchant.items, 1, p=item_probabilities)[0]

        quality_distribution = {
            Quality.SHITTY: merchant.quality_distribution.shitty,
            Quality.NORMAL: merchant.quality_distribution.normal,
            Quality.BAD: merchant.quality_distribution.bad,
            Quality.GOOD: merchant.quality_distribution.good,
            Quality.DIVINE: merchant.quality_distribution.divine,
        }

        quality: Quality = np.random.choice(
            list(quality_distribution.keys()), 1,
            p=list(quality_distribution.values())
        )[0]

        base_item = self._items[merchant_item.item]
        total_item_power = round(base_item.power * QUALITY_MULTIPLIER[quality])

        return RolledItem(
            item=base_item,
            quality=quality,
            obtained_at=datetime.now(),
            merchant=merchant.id,
            total_power=total_item_power
        )

    def roll_item_for_player(self, player: Player, merchant_id: str):
        merchant = self._merchants.get(merchant_id)
        if not merchant:
            raise UnknownMerchant

        if player.gold < merchant.roll_price:
            raise NotEnoughGold

        if player.current_undecided_roll_item is not None:
            raise RollIsAlreadyActive

        rolled_item = self.roll_item(merchant)
        player.current_undecided_roll_item = rolled_item
        player.gold -= merchant.roll_price

    def accept_roll(self, player: Player):
        rolled_item = player.current_undecided_roll_item

        if rolled_item is None:
            raise NoActiveRoll

        player.current_undecided_roll_item = None

        previous_item_in_same_slot = player.get_item_in_slot(rolled_item.item.slot)
        if previous_item_in_same_slot:
            player.items.remove(previous_item_in_same_slot)
        player.items.append(rolled_item)

    def decline_roll(self, player: Player):
        rolled_item = player.current_undecided_roll_item

        if rolled_item is None:
            raise NoActiveRoll

        player.current_undecided_roll_item = None
        player.gold += round(self._merchants[rolled_item.merchant].roll_price * 0.7)

    def generate_division_standings(self) -> DivisionStandings:
        league = random.choice(self._leagues)

        n = self._settings.max_players_per_division
        powers = list(sorted([random.randint(100, 2000) for _ in range(n)], reverse=True))
        division_players = []

        for rank, p in enumerate(powers):
            division_players.append(DivisionPlayer(
                username=f"random_player",
                power=p,
                rank=rank + 1
            ))

        return DivisionStandings(
            league_id=league.id,
            division_id=league.id + "_test",
            players=division_players,
            next_update_at=datetime.now() + timedelta(hours=1, minutes=45)
        )
