from typing import Optional, List, ClassVar
from datetime import datetime
from enum import Enum

import numpy as np
from attr import dataclass

__all__ = [
    "GameError",
    "Player",
    "RolledItem",
    "Game",
    "GameSettings"
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


class Slot(Enum):
    HEAD = "head"
    BODY = "body"
    WEAPON = "weapon"
    TRINKET = "trinket"
    BOOTS = "boots"


class Quality(Enum):
    SHITTY = "shitty"
    BAD = "bad"
    NORMAL = "normal"
    GOOD = "good"
    DIVINE = "divine"


QUALITY_MULTIPLIER = {"shitty": 0.7, "bad": 0.85, "normal": 1, "good": 1.2, "divine": 1.4}
QUALITY_DISTRIBUTION = {"shitty": 0.35, "bad": 0.25, "normal": 0.25, "good": 0.1, "divine": 0.05}


class Rarity(Enum):
    COMMON = "common"
    UNCOMMON = "uncommon"
    RARE = "rare"
    EPIC = "epic"
    LEGENDARY = "legendary"
    MYTHICAL = "mythical"


@dataclass
class BaseItem:
    id: str
    power: int
    slot: Slot
    rarity: Rarity


@dataclass
class MerchantItem:
    item_base: BaseItem
    probability: float


@dataclass
class Merchant:
    id: str
    items: List[MerchantItem]
    roll_price: int


@dataclass
class RolledItem:
    item: BaseItem
    quality: Quality
    obtained_at: datetime

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
        for i in self.items:
            if i.item.slot == slot:
                return i
        return None


@dataclass
class GameSettings:
    income_multiplier: float
    income_period_seconds: float
    initial_gold: int
    roll_price: int

    @staticmethod
    def from_json_file(path: str):
        import json
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
            return GameSettings(
                income_multiplier=data["INCOME_MULTIPLIER"],
                income_period_seconds=data["INCOME_PERIOD_MINUTES"] * 60,
                initial_gold=data["INITIAL_BALANCE"],
                roll_price =data["ROLLING_PRICE"],
            )


class Game:
    def __init__(self, settings: GameSettings):
        self.settings = settings
        self._merchants = {
            "default": Merchant(
                id="default",
                items=[
                    MerchantItem(
                        item_base=BaseItem(
                            id="generic_sword",
                            power=10,
                            slot=Slot.WEAPON,
                            rarity=Rarity.LEGENDARY
                        ),
                        probability=1
                    )
                ],
                roll_price=settings.roll_price
            )
        }

    def create_new_player(self, username: str) -> Player:
        return Player(
            username=username,
            gold=self.settings.initial_gold,
            items=[],
            last_gold_update_time=datetime.now(),
            current_undecided_roll_item=None
        )

    def update_player_gold(self, player: Player):
        player.last_gold_updated_time = datetime.now()
        player.gold += round(player.total_power * self.settings.income_multiplier)

    def roll_item(self, merchant: Merchant) -> RolledItem:
        item_probabilities = np.array(list(i.probability for i in merchant.items), np.float32)
        item_probabilities /= np.sum(item_probabilities)

        item: MerchantItem = np.random.choice(merchant.items, 1, p=item_probabilities)[0]

        quality_name = np.random.choice(
            list(QUALITY_DISTRIBUTION.keys()), 1,
            p=list(QUALITY_DISTRIBUTION.values())
        )[0]

        return RolledItem(
            item=item.item_base,
            quality=Quality(quality_name),
            obtained_at=datetime.now()
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
