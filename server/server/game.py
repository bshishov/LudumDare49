from typing import Optional, List, ClassVar
from datetime import datetime
from enum import Enum

import numpy as np
from attr import dataclass

from server.data import (
    Merchant,
    MerchantItemData,
    ItemData,
    GameSettingsData,
    Slot
)

__all__ = [
    "GameError",
    "Player",
    "RolledItem",
    "Game",
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


QUALITY_MULTIPLIER = {"shitty": 0.7, "bad": 0.85, "normal": 1, "good": 1.2, "divine": 1.4}
QUALITY_DISTRIBUTION = {"shitty": 0.35, "bad": 0.25, "normal": 0.25, "good": 0.1, "divine": 0.05}


@dataclass
class RolledItem:
    item: ItemData
    quality: Quality
    obtained_at: datetime
    merchant: str

    @property
    def power(self):
        return self.item.power * QUALITY_MULTIPLIER[self.quality.value]


@dataclass
class Player:
    username: str
    gold: int
    last_gold_update_time: datetime
    items: List[RolledItem]
    current_undecided_roll_item: Optional[RolledItem]

    @property
    def total_power(self) -> int:
        power = 0
        for item in self.items:
            power += item.power
        return power + 10

    def get_item_in_slot(self, slot: Slot) -> Optional[RolledItem]:
        for rolled_item in self.items:
            if rolled_item.item.slot == slot:
                return rolled_item
        return None


class Game:
    def __init__(
            self,
            settings: GameSettingsData,
            merchants: List[Merchant],
            items: List[ItemData]
    ):
        self._settings = settings
        self._items = {item.id: item for item in items}
        self._merchants = {m.id: m for m in merchants}

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

    def roll_item(self, merchant: Merchant) -> RolledItem:
        item_probabilities = np.array(list(i.probability for i in merchant.items), np.float32)
        item_probabilities /= np.sum(item_probabilities)

        item: MerchantItemData = np.random.choice(merchant.items, 1, p=item_probabilities)[0]

        quality_name = np.random.choice(
            list(QUALITY_DISTRIBUTION.keys()), 1,
            p=list(QUALITY_DISTRIBUTION.values())
        )[0]

        return RolledItem(
            item=self._items[item.item],
            quality=Quality(quality_name),
            obtained_at=datetime.now(),
            merchant=merchant.id
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
