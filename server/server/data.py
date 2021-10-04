import json
from typing import List, Optional
from enum import Enum

from attr import dataclass, attrib, validators

from server.spec import spec

__all__ = [
    "Slot",
    "Rarity",
    "GameSettingsData",
    "MerchantData",
    "MerchantItemData",
    "MerchantQualityDistribution",
    "ItemData",
    "LeagueData",
    "load_merchants",
    "load_items",
    "load_game_settings",
    "load_leagues"
]


def positive(instance, attribute, value):
    if value < 0:
        raise ValueError(f"Expected positive value for attribute {attribute}, got {value}")


def positive_float(instance, attribute, value):
    validators.instance_of(float)(instance, attribute, value)
    positive(instance, attribute, value)


def positive_int(instance, attribute, value):
    validators.instance_of(int)(instance, attribute, value)
    positive(instance, attribute, value)


def non_empty_string(instance, attribute, value):
    validators.instance_of(str)(instance, attribute, value)
    if not value:
        raise ValueError("Non empty string expected")


class Slot(Enum):
    HEAD = "head"
    BODY = "body"
    WEAPON = "weapon"
    TRINKET = "trinket"
    BOOTS = "boots"


class Rarity(Enum):
    COMMON = "common"
    UNCOMMON = "uncommon"
    RARE = "rare"
    EPIC = "epic"
    LEGENDARY = "legendary"
    MYTHICAL = "mythical"


@dataclass
class GameSettingsData:
    income_period_seconds: float = attrib(validator=positive_float)
    power_income_multiplier: float = attrib(validator=positive_float)
    initial_gold: int = attrib(validator=positive_int)
    league_update_interval_seconds: float = attrib(validator=positive_float)
    max_players_per_division: int = attrib(validator=positive_int)
    starting_league: str = attrib(validator=non_empty_string)


@dataclass
class MerchantItemData:
    item: str = attrib(validator=non_empty_string)
    probability: float = attrib(validator=positive_float)


@dataclass
class MerchantQualityDistribution:
    shitty: float = attrib(validator=positive_float)
    bad: float = attrib(validator=positive_float)
    normal: float = attrib(validator=positive_float)
    good: float = attrib(validator=positive_float)
    divine: float = attrib(validator=positive_float)


@dataclass
class MerchantData:
    id: str = attrib(validator=non_empty_string)
    roll_price: int = attrib(validator=positive_int)
    items: List[MerchantItemData]
    quality_distribution: MerchantQualityDistribution


@dataclass
class ItemData:
    id: str = attrib(validator=non_empty_string)
    power: int = attrib(validator=positive_int)
    slot: Slot
    rarity: Rarity


@dataclass
class LeagueData:
    id: str = attrib(validator=non_empty_string)
    gold_rewards_for_rank: List[int]
    n_best_players: int = attrib(validator=positive_int)
    n_worst_players: int = attrib(validator=positive_int)
    next_league_id: Optional[str]
    prev_league_id: Optional[str]


def load_merchants(filename: str) -> List[MerchantData]:
    with open(filename, "r", encoding="utf-8") as f:
        raw_data = json.load(f)
        return spec.load(List[MerchantData], raw_data)


def load_items(filename: str) -> List[ItemData]:
    with open(filename, "r", encoding="utf-8") as f:
        raw_data = json.load(f)
        return spec.load(List[ItemData], raw_data)


def load_game_settings(filename: str) -> GameSettingsData:
    with open(filename, "r", encoding="utf-8") as f:
        raw_data = json.load(f)
        return spec.load(GameSettingsData, raw_data)


def load_leagues(filename: str) -> List[LeagueData]:
    with open(filename, "r", encoding="utf-8") as f:
        raw_data = json.load(f)
        return spec.load(List[LeagueData], raw_data)
