"""
Build Decoration Block XML.

File order:
  1. Masters
  2. Clones (each kind uses its own emit function)
  3. Variant helper

Default writes a small example set to Draft and the live game copy.
Use --all later to process every flattened block.
"""

from __future__ import annotations

import argparse
import csv
import re
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))

from flatten_blocks import (
    build_all_block_names,
    flatten_block,
    reset_flatten_state,
    should_emit_block,
)

GAME_BLOCKS = Path(
    r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\blocks.xml"
)
GAME_LOC = Path(
    r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\Localization.csv"
)
LEGACY_LOC = Path(
    r"c:\GitHub\7D2D-Mods\00_Support\Archive\AGF-VP-DecorationBlock-v3.0.3-ExcelBaseline-20260822\Config\Localization.csv"
)
DRAFT_ROOT = Path(r"c:\GitHub\7D2D-Mods\01_Draft\AGF-VP-DecorationBlock-v3.0.3")
LIVE_ROOT = Path(
    r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\AGF-VP-DecorationBlock-v3.0.3"
)
OUTPUT_ROOTS = (DRAFT_ROOT, LIVE_ROOT)
DRAFT_BLOCKS = DRAFT_ROOT / "Config" / "blocks.xml"
LIVE_BLOCKS = LIVE_ROOT / "Config" / "blocks.xml"
OUTPUT_BLOCKS = (DRAFT_BLOCKS, LIVE_BLOCKS)

CAMPFIRE_CATEGORIES = (
    ("CFFood/Cooking", "ui_game_symbol_fork", "lblCategoryFood"),
    ("CFDrink/Cooking", "ui_game_symbol_thirst", "lblCategoryDrink"),
    ("CFMedical", "ui_game_symbol_medical", "lblCategoryMedicine"),
    ("CFChemicals", "ui_game_symbol_chemistry", "lblCategoryChemicals"),
)
CAMPFIRE_WINDOWS = (
    "windowCraftingList",
    "craftingInfoPanel",
    "windowCraftingQueue",
    "windowToolsCampfire",
    "windowFuel",
    "windowOutput",
    "windowNonPagingHeader",
)

# Look and place properties kept from the flattened vanilla block.
VISUAL_PROPS = frozenset(
    {
        "Shape",
        "Model",
        "ModelOffset",
        "Texture",
        "Path",
        "Place",
        "WaterFlow",
        "MultiBlockDim",
        "Collide",
        "ImposterExchange",
        "ImposterDontBlock",
        "LightOpacity",
        "OnlySimpleRotations",
        "AllowAllRotations",
        "AllowedRotations",
        "PlacementDistance",
        "OversizedBounds",
        "GndAlign",
        "HandleFace",
        "LODCullScale",
        "Mesh",
        "UseGlobalUV",
        "MeshDamage",
        "OpenSound",
        "CloseSound",
        "UpgradeSound",
        "ShowModelOnFall",
        "VehicleHitScale",
        "CustomIconTint",
        "TintColor",
        "StabilitySupport",
    }
)

# Trees/crops: keep plant flags, never the distant-deco radius.
FLORA_EXTRA_PROPS = frozenset(
    {
        "IsPlant",
        "CanPlayersSpawnOn",
        "ImposterExclude",
    }
)

# Housing paint only. These do not change the emitted light color.
LIGHT_COLOR_SUFFIXES = (
    "ArmyGreen",
    "White",
    "Black",
    "Brown",
    "Yellow",
    "Green",
    "Grey",
    "Gray",
    "Silver",
    "Orange",
    "Purple",
    "Pink",
    "Brass",
    "Red",
)

# Powered clones must not copy housing tint. All emit the same light.
POWERED_SKIP_VISUALS = frozenset({"CustomIconTint", "TintColor"})

# Cooking twins keep the 3D TintColor, but the helper icon is always red.
CAMPFIRE_SKIP_VISUALS = frozenset({"CustomIconTint"})
CAMPFIRE_ICON_TINT = "FF0000"

COOK_NAME_MARKERS = (
    "coffeemaker",
    "grill",
    "oven",
    "stove",
    "microwave",
    "gasrange",
    "cookingpot",
)
KITCHEN_APPLIANCE_MARKERS = COOK_NAME_MARKERS + ("dishwasher",)

SIGNABLE = {
    "FontSize": "90",
    "LineCount": "3",
    "LineWidth": "0.55",
    "LineSpacing": "1",
}

LOOT_TIERS = (
    {
        "suffix": "",
        "insecure": "Insecure",
        "master": "agfDecoMasterWood",
        "loot": "playerWoodWritableStorage",
        "upgrade_to": "Iron",
        "upgrade_item": "resourceForgedIron",
    },
    {
        "suffix": "Iron",
        "insecure": "IronInsecure",
        "master": "agfDecoMasterIron",
        "loot": "playerIronWritableStorage",
        "upgrade_to": "Steel",
        "upgrade_item": "resourceForgedSteel",
    },
    {
        "suffix": "Steel",
        "insecure": "SteelInsecure",
        "master": "agfDecoMasterSteel",
        "loot": "playerSteelWritableStorage",
        "upgrade_to": None,
        "upgrade_item": None,
    },
)

EXAMPLE_SOURCES = (
    ("cntCoffeeMaker", "loot"),
    ("cntBinTrashPlasticEmptyWhite", "loot"),
    ("lightPorchWhite", "powered"),
    ("lightIndustrial", "powered"),
    ("banditWallCorrugated1x3", "upgradeable"),
    ("wineBarrel", "deco"),
)


def deco_name(source: str, suffix: str = "") -> str:
    return f"agfDeco{source}{suffix}"


def prop_value(block: ET.Element, name: str) -> str | None:
    for p in block.findall("property"):
        if p.get("name") == name:
            return p.get("value")
    return None


def has_class(block: ET.Element, class_name: str) -> bool:
    for p in block.iter("property"):
        if p.get("class") == class_name:
            return True
    return False


def add_prop(block: ET.Element, name: str, value: str, **extra: str) -> None:
    attrs = {"name": name, "value": value}
    attrs.update(extra)
    ET.SubElement(block, "property", attrs)


def add_visuals(
    block: ET.Element, flat: ET.Element, skip: frozenset[str] = frozenset()
) -> None:
    flora = is_flora_source(flat.get("name") or "")
    for p in flat.findall("property"):
        name = p.get("name")
        keep = name in VISUAL_PROPS or (flora and name in FLORA_EXTRA_PROPS)
        if not keep or name in skip:
            continue
        # Distant/gimbal trees do not render as player deco. Keep the mesh, drop the radius.
        if name in {"BigDecorationRadius", "IsDecoration", "IsDistantDecoration"}:
            continue
        if name == "Shape" and p.get("value") in {"DistantDecoTree", "DistantDeco"}:
            add_prop(block, "Shape", "ModelEntity")
            continue
        block.append(ET.Element("property", dict(p.attrib)))


def deco_icon(source: str, flat: ET.Element) -> str:
    nl = source.lower()
    if nl.startswith("concreteplateround"):
        return "concreteNoUpgradeMaster"
    if source == "flagWallHungSign":
        return "flagWallHungUSA"
    icon = prop_value(flat, "CustomIcon") or ""
    if not icon or icon == "missingIcon":
        return source
    return icon


def comment(text: str) -> ET.Element:
    return ET.Comment(f" {text} ")


def emit_masters() -> list[ET.Element]:
    master = ET.Element("block", {"name": "agfDecoMaster"})
    add_prop(master, "CreativeMode", "None")
    add_prop(master, "DescriptionKey", "agfDecoBlockDesc")
    add_prop(master, "Material", "DecoMaterial")
    add_prop(master, "MaxDamage", "500")
    add_prop(master, "Texture", "241")
    add_prop(master, "Shape", "ModelEntity")
    add_prop(master, "Model", "@:Entities/Misc/block_missingPrefab")
    add_prop(master, "StabilitySupport", "true")
    add_prop(master, "WaterFlow", "permitted")
    add_prop(master, "SellableToTrader", "false")
    add_prop(master, "MultiBlockDim", "1,1,1")
    repair = ET.SubElement(master, "property", {"class": "RepairItems"})
    add_prop(repair, "resourceClayLump", "5")
    ET.SubElement(
        master,
        "drop",
        {"event": "Destroy", "name": "agfDecorationsVariantHelper", "count": "1"},
    )
    ET.SubElement(
        master,
        "drop",
        {
            "event": "Fall",
            "name": "agfDecorationsVariantHelper",
            "count": "1",
            "prob": ".9",
        },
    )

    def tier_master(name: str, material: str, hp: str, repair_item: str) -> ET.Element:
        b = ET.Element("block", {"name": name})
        add_prop(b, "Extends", "agfDecoMaster", param1="Material,RepairItems")
        add_prop(b, "CreativeMode", "None")
        add_prop(b, "Material", material)
        add_prop(b, "MaxDamage", hp)
        r = ET.SubElement(b, "property", {"class": "RepairItems"})
        add_prop(r, repair_item, "10")
        return b

    return [
        master,
        tier_master("agfDecoMasterWood", "MwoodReinforced", "500", "resourceWood"),
        tier_master("agfDecoMasterIron", "Mmetal", "2500", "resourceForgedIron"),
        tier_master("agfDecoMasterSteel", "Msteel", "5000", "resourceForgedSteel"),
    ]


def emit_deco_copy(source: str, flat: ET.Element, sort1: str) -> list[ET.Element]:
    """Deco Copy: model only. Uses agfDecoMaster."""
    b = ET.Element("block", {"name": deco_name(source)})
    add_prop(b, "CreativeMode", "None")
    add_prop(b, "Extends", "agfDecoMaster")
    add_prop(b, "CustomIcon", deco_icon(source, flat))
    add_visuals(b, flat)
    if (prop_value(flat, "Class") or "") == "Ladder":
        add_prop(b, "Class", "Ladder")
    add_prop(b, "SortOrder1", sort1)
    return [b]


def emit_powered(source: str, flat: ET.Element, sort1: str) -> list[ET.Element]:
    """Powered: one fixture, same light color. Flashlights and lanterns keep housing tint."""
    b = ET.Element("block", {"name": deco_name(source)})
    add_prop(b, "CreativeMode", "None")
    add_prop(b, "Extends", "agfDecoMaster")
    add_prop(b, "CustomIcon", deco_icon(source, flat))
    skip = set(POWERED_SKIP_VISUALS)
    if keep_light_housing_colors(source):
        skip -= {"CustomIconTint", "TintColor"}
    add_visuals(b, flat, skip=frozenset(skip))
    add_prop(b, "Class", "PoweredLight")
    add_prop(b, "RuntimeSwitch", "true")
    add_prop(b, "RequiredPower", "5")
    add_prop(b, "ItemTypeIcon", "electric_power")
    add_prop(b, "SortOrder1", sort1)
    return [b]


def emit_campfire(source: str, flat: ET.Element, sort1: str) -> list[ET.Element]:
    """Campfire Like: same model, campfire station."""
    b = ET.Element("block", {"name": deco_name(source, "Cooking")})
    add_prop(b, "CreativeMode", "None")
    add_prop(b, "Extends", "agfDecoMaster")
    add_prop(b, "CustomIcon", deco_icon(source, flat))
    add_visuals(b, flat, skip=CAMPFIRE_SKIP_VISUALS)
    add_prop(b, "CustomIconTint", CAMPFIRE_ICON_TINT)
    add_prop(b, "Class", "Campfire")
    ws = ET.SubElement(b, "property", {"class": "Workstation"})
    add_prop(ws, "Modules", "tools,output,fuel,input")
    add_prop(ws, "CraftingAreaRecipes", "campfire")
    add_prop(b, "WorkstationIcon", "ui_game_symbol_campfire")
    add_prop(b, "CraftIcon", "ui_game_symbol_spatula")
    add_prop(b, "CraftActionName", "lblContextActionCook")
    add_prop(b, "HeatMapStrength", "5")
    add_prop(b, "HeatMapTime", "5000")
    add_prop(b, "HeatMapFrequency", "1000")
    add_prop(b, "OpenSound", "campfire_open")
    add_prop(b, "CloseSound", "campfire_close")
    add_prop(b, "CraftSound", "campfire_cook_click")
    add_prop(b, "CraftCompleteSound", "campfire_complete_item")
    add_prop(b, "SortOrder1", sort1)
    return [b]


def _storage_features(loot_list: str, lockable: bool) -> ET.Element:
    feats = ET.Element("property", {"class": "CompositeFeatures"})
    storage = ET.SubElement(feats, "property", {"class": "TEFeatureStorage"})
    add_prop(storage, "LootList", loot_list)
    if lockable:
        ET.SubElement(feats, "property", {"class": "TEFeatureLockable"})
    sign = ET.SubElement(feats, "property", {"class": "TEFeatureSignable"})
    for key, val in SIGNABLE.items():
        add_prop(sign, key, val)
    return feats


def emit_loot_family(source: str, flat: ET.Element, sort1: str) -> list[ET.Element]:
    """Player Loot: wood / iron / steel, each with an insecure twin."""
    out: list[ET.Element] = []
    icon = deco_icon(source, flat)
    for tier in LOOT_TIERS:
        secure_name = deco_name(source, tier["suffix"])
        insecure_name = deco_name(source, tier["insecure"])

        secure = ET.Element("block", {"name": secure_name})
        add_prop(secure, "CreativeMode", "None")
        add_prop(secure, "Extends", tier["master"])
        add_prop(secure, "CustomIcon", icon)
        add_visuals(secure, flat)
        add_prop(secure, "Class", "CompositeTileEntity")
        secure.append(_storage_features(tier["loot"], lockable=True))
        add_prop(secure, "DowngradeBlock", insecure_name)
        if tier["upgrade_to"]:
            up = ET.SubElement(secure, "property", {"class": "UpgradeBlock"})
            add_prop(up, "ToBlock", deco_name(source, tier["upgrade_to"]))
            add_prop(up, "Item", tier["upgrade_item"])
            add_prop(up, "ItemCount", "10")
            add_prop(up, "UpgradeHitCount", "4")
        ET.SubElement(secure, "dropextendsoff")
        ET.SubElement(secure, "drop", {"event": "Destroy", "count": "0"})
        add_prop(secure, "SortOrder1", sort1)
        out.append(secure)

        insecure = ET.Element("block", {"name": insecure_name})
        add_prop(insecure, "CreativeMode", "None")
        add_prop(insecure, "Extends", tier["master"])
        add_prop(insecure, "CustomIcon", icon)
        add_visuals(insecure, flat)
        add_prop(insecure, "Class", "CompositeTileEntity")
        insecure.append(_storage_features(tier["loot"], lockable=False))
        add_prop(insecure, "SortOrder1", sort1)
        out.append(insecure)
    return out


def emit_upgradeable_family(source: str, flat: ET.Element, sort1: str) -> list[ET.Element]:
    """Upgradeable: Deco -> Iron -> Steel. Same model."""
    icon = deco_icon(source, flat)
    rows = (
        ("", "agfDecoMaster", "Iron", "resourceForgedIron"),
        ("Iron", "agfDecoMasterIron", "Steel", "resourceForgedSteel"),
        ("Steel", "agfDecoMasterSteel", None, None),
    )
    out: list[ET.Element] = []
    for suffix, master, nxt, item in rows:
        b = ET.Element("block", {"name": deco_name(source, suffix)})
        add_prop(b, "CreativeMode", "None")
        add_prop(b, "Extends", master)
        add_prop(b, "CustomIcon", icon)
        skip = frozenset({"StabilitySupport"}) if source == "helipad" else frozenset()
        add_visuals(b, flat, skip=skip)
        if source == "helipad":
            add_prop(b, "StabilitySupport", "true")
            add_prop(b, "Path", "solid")
            if not prop_value(b, "Collide"):
                add_prop(b, "Collide", "movement,melee,bullet,arrow,rocket")
        if nxt:
            up = ET.SubElement(b, "property", {"class": "UpgradeBlock"})
            add_prop(up, "ToBlock", deco_name(source, nxt))
            add_prop(up, "Item", item)
            add_prop(up, "ItemCount", "10")
            add_prop(up, "UpgradeHitCount", "4")
        add_prop(b, "SortOrder1", sort1)
        out.append(b)
    return out


def emit_helper(picker_names: list[str]) -> ET.Element:
    b = ET.Element("block", {"name": "agfDecorationsVariantHelper"})
    add_prop(b, "Extends", "agfDecoMaster")
    add_prop(b, "CustomIcon", "cntRetroFridgeVer1Closed")
    add_prop(b, "CreativeMode", "Player")
    add_prop(b, "DescriptionKey", "agfDecorationsDesc")
    add_prop(b, "ItemTypeIcon", "all_blocks")
    add_prop(b, "Stacknumber", "5000")
    add_prop(b, "SelectAlternates", "true")
    add_prop(b, "PlaceAltBlockValue", ",".join(picker_names))
    add_prop(b, "EconomicValue", "1")
    add_prop(b, "SortOrder1", "U200")
    add_prop(b, "SortOrder2", "0000")
    return b


def is_cook_source(name: str) -> bool:
    n = name.lower()
    return any(m in n for m in COOK_NAME_MARKERS)


def is_vanilla_light(flat: ET.Element) -> bool:
    return (prop_value(flat, "Class") or "") in {"Light", "PoweredLight"}


def normalize_model(model: str) -> str:
    """Treat color/light prefab twins as the same mesh, even across folders."""
    name = model.replace("\\", "/").rsplit("/", 1)[-1]
    return (
        name.replace("RedPrefab.prefab", "Prefab.prefab")
        .replace("LightPrefab.prefab", "Prefab.prefab")
        .lower()
    )


def normalize_light_model(model: str) -> str:
    """Treat a *RedPrefab as the same fixture as the matching *Prefab."""
    return normalize_model(model)


def light_fixture_key(flat: ET.Element) -> tuple[str, ...]:
    return (
        normalize_light_model(prop_value(flat, "Model") or ""),
        prop_value(flat, "MultiBlockDim") or "1,1,1",
        prop_value(flat, "Place") or "",
        prop_value(flat, "HandleFace") or "",
        prop_value(flat, "ModelOffset") or "",
    )


def keep_light_housing_colors(name: str) -> bool:
    nl = name.lower()
    return nl.startswith("flashlight") or nl.startswith("lanterndecor")


def is_light_helper(name: str) -> bool:
    return "Helper" in name or "helper" in name


def is_player_dup(name: str) -> bool:
    n = name.lower()
    return n.endswith("_player") or n.endswith("player")


def light_pick_rank(name: str) -> tuple[int, str]:
    if is_light_helper(name):
        return (9, name)
    if is_player_dup(name):
        return (8, name)
    if name.endswith("White"):
        return (0, name)
    if any(name.endswith(suf) for suf in LIGHT_COLOR_SUFFIXES):
        return (2, name)
    return (1, name)


def pick_canonical_light(names: list[str]) -> str:
    usable = [n for n in names if not is_light_helper(n)] or list(names)
    pool = [n for n in usable if not is_player_dup(n)] or usable
    return sorted(pool, key=light_pick_rank)[0]


def group_light_fixtures(
    flat_map: dict[str, ET.Element],
) -> dict[str, list[str]]:
    """Canonical vanilla name -> every vanilla name in that fixture family."""
    buckets: dict[tuple[str, ...], list[str]] = defaultdict(list)
    for name, flat in flat_map.items():
        if not is_vanilla_light(flat):
            continue
        key = light_fixture_key(flat)
        if keep_light_housing_colors(name):
            tint = (prop_value(flat, "TintColor") or prop_value(flat, "CustomIconTint") or "")
            key = key + (tint.replace(" ", "").upper(),)
        buckets[key].append(name)
    return {pick_canonical_light(members): sorted(members) for members in buckets.values()}


SKIP_NAME_PARTS = (
    "infested",
    "cursed",
    "sleeper",
    "test",
    "debug",
    "dummy",
    "hidden",
    "imposter",
    "gore",
    "quest",
    "loothelper",
    "randomhelper",
    "varianthelper",
    "wreckage",
    "rubble",
    "texture",
    "twitch",
    "noloot",
    "pickedlock",
    "noose",
    "invisible",
)

# Intact stations. Broken / busted / collapsed cnt* stay loot via the cnt rule.
INTACT_WORKSTATIONS = frozenset(
    {
        "campfire",
        "forge",
        "workbench",
        "cementMixer",
        "chemistryStation",
        "tableSaw",
    }
)

SHAPE_KEEP_PREFIXES = ("glass", "ibeam", "concreteplateround")

UPGRADE_NAME_MARKERS = (
    "bandit",
    "shippingcontainer",
    "chainlink",
    "sandbag",
    "guardrail",
    "bollard",
    "parkingblock",
    "ibeam",
    "concreteplateround",
    "tarpfence",
    "jailbar",
    "helipad",
)

KIND_RANK = {"loot": 0, "upgradeable": 1, "powered": 2, "deco": 3}

_TOKEN_RE = re.compile(r"[^a-z0-9]+")


def name_tokens(name: str) -> set[str]:
    spaced = re.sub(r"([a-z])([A-Z])", r"\1 \2", name)
    spaced = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1 \2", spaced)
    spaced = re.sub(r"([a-zA-Z])([0-9])", r"\1 \2", spaced)
    return {t for t in _TOKEN_RE.split(spaced.lower()) if t}


def has_token(name: str, token: str) -> bool:
    return token.lower() in name_tokens(name)


def is_shape_model(model: str) -> bool:
    return "/Shapes/" in model or model.startswith("@:Shapes/")


def is_flora_source(name: str) -> bool:
    nl = name.lower()
    if nl.startswith("mushroombiome"):
        return False
    return nl.startswith("planted") or nl.startswith("tree")


def has_deco_model(flat: ET.Element, name: str) -> bool:
    model = prop_value(flat, "Model") or ""
    shape = prop_value(flat, "Shape") or ""
    if shape in {"BillboardPlant", "Grass", "GrassShort"}:
        return True
    if not model or "missingPrefab" in model or "block_missing" in model:
        return False
    if is_shape_model(model):
        nl = name.lower()
        return any(nl.startswith(p) for p in SHAPE_KEEP_PREFIXES)
    if model.startswith("@:") or "Entities/" in model:
        return True
    return False


def is_door_family(name: str) -> bool:
    if "shippingcontainer" in name.lower():
        return False
    return has_token(name, "door") or has_token(name, "hatch") or has_token(name, "gate")


def is_vanilla_lock_twin(name: str) -> bool:
    nl = name.lower()
    return "insecure" in nl or has_token(name, "player")


def is_shape_menu_junk(name: str) -> bool:
    nl = name.lower()
    if "escalator" in nl:
        return False
    return any(s in nl for s in ("ladder", "catwalk", "stair"))


def is_upgradeable_structure(name: str) -> bool:
    nl = name.lower()
    if "tarphanging" in nl:
        return False
    if any(m in nl for m in UPGRADE_NAME_MARKERS):
        return True
    if "fence" in nl:
        return True
    if nl.startswith("barrier") or "barrierplastic" in nl or "barrierconcrete" in nl:
        return True
    if nl.startswith("glass") and not any(
        s in nl for s in ("debris", "broken", "trap")
    ):
        return True
    return False


def visual_identity_key(flat: ET.Element) -> tuple[str, ...]:
    name = flat.get("name") or ""
    glass_family = ""
    for prefix in (
        "glassBusiness",
        "glassIndustrial",
        "glassOpaque",
        "glassBulletproof",
        "glassBroken",
        "glassDebris",
        "glassRamp",
        "stainedGlass",
        "opaqueBusinessGlass",
    ):
        if name.startswith(prefix):
            glass_family = prefix
            break
    return (
        normalize_model(prop_value(flat, "Model") or ""),
        (prop_value(flat, "TintColor") or "").replace(" ", "").upper(),
        (prop_value(flat, "CustomIconTint") or "").replace(" ", "").upper(),
        (prop_value(flat, "Texture") or "").replace(" ", ""),
        (prop_value(flat, "Mesh") or "").lower(),
        (prop_value(flat, "Material") or ""),
        glass_family,
    )


def visual_pick_rank(source: str, kind: str) -> tuple:
    nl = source.lower()
    return (
        1 if "twitch" in nl else 0,
        1 if has_token(source, "poi") else 0,
        1 if "alarmunlocked" in nl or "pickedlock" in nl else 0,
        1 if "constructionsupplies" in nl else 0,
        1 if "signcanvascorrugatedmetal" in nl else 0,
        KIND_RANK.get(kind, 9),
        1 if "player" in nl or "insecure" in nl else 0,
        0 if source_status(source) else 1,
        0 if source.endswith("White") else 1,
        len(source),
        source,
    )


def pick_canonical_visual(
    members: list[tuple[str, str, ET.Element]],
) -> tuple[str, str, ET.Element]:
    return min(members, key=lambda m: visual_pick_rank(m[0], m[1]))


def classify(flat: ET.Element) -> str | None:
    name = flat.get("name") or ""
    nl = name.lower()
    cls = prop_value(flat, "Class") or ""
    place = prop_value(flat, "Place") or ""
    tag = prop_value(flat, "BlockTag") or ""
    shape = prop_value(flat, "Shape") or ""

    if "master" in nl or "helper" in nl:
        return None
    if name.endswith("Shapes"):
        return None
    if nl.startswith("terr") or name in {"air", "water"}:
        return None
    if "filler" in nl and "escalator" not in nl:
        return None
    if any(s in nl for s in SKIP_NAME_PARTS):
        return None
    if name in {"flagWallHungSign", "spawnTrader"}:
        return None
    if nl.startswith("signcanvas") or nl.startswith("signdecal") or nl.startswith("canvas"):
        return None
    if "writablecrate" in nl:
        return None
    if nl.startswith("pipefirehazard") or nl.startswith("pipesfeedrekt"):
        return None
    if nl.startswith("cntshippingcrateconstruction"):
        return None
    if nl.startswith("mushroombiome"):
        return None
    if nl.startswith("trap"):
        return None
    if nl.startswith("spawn") or cls == "SpawnEntity":
        return None
    if (prop_value(flat, "CreativeMode") or "") == "Test":
        return None
    if tag == "Door" or cls in {"DoorSecure", "PoweredDoor"}:
        return None
    if place == "Door" and not is_flora_source(name):
        return None
    if has_class(flat, "TEFeatureDoor"):
        return None
    if is_door_family(name):
        return None
    if is_vanilla_lock_twin(name):
        return None
    if cls in {"Trap", "Mine"}:
        return None
    model = prop_value(flat, "Model") or ""
    if "NoShow" in model:
        return None
    if not is_flora_source(name):
        if "StudioGimbal" in model or model.startswith("#Entities/Trees"):
            return None
        if cls == "ModelTree":
            return None
        if shape in {"DistantDecoTree", "DistantDeco"} and not model.startswith("@:Entities/"):
            return None
    if is_shape_menu_junk(name):
        return None
    if not has_deco_model(flat, name):
        return None

    if name in INTACT_WORKSTATIONS:
        return "loot"
    if cls in {"SecureLoot", "Loot"} or has_class(flat, "TEFeatureStorage"):
        return "loot"
    # Empty / open looks often have no loot Class. Keep them as Player Loot.
    if name.startswith("cnt") or name.startswith("laundryCart"):
        return "loot"
    if cls in {"Light", "PoweredLight"}:
        return "powered"
    if is_upgradeable_structure(name):
        return "upgradeable"
    return "deco"


def load_flat_map() -> tuple[dict[str, ET.Element], list[str], set[str]]:
    root = ET.parse(GAME_BLOCKS).getroot()
    reset_flatten_state(root)
    all_names = build_all_block_names(root)
    order: list[str] = []
    flat_map: dict[str, ET.Element] = {}
    for raw in root.iter("block"):
        name = raw.get("name")
        if not name:
            continue
        order.append(name)
        flat_map[name] = flatten_block(raw)
    return flat_map, order, all_names


def load_legacy_helper_order() -> dict[str, int]:
    """Old Excel helper order. Known names keep that rank. New names go last."""
    path = HERE / "legacy_helper_order.txt"
    ranks: dict[str, int] = {}
    i = 0
    for line in path.read_text(encoding="utf-8").splitlines():
        name = line.strip()
        if name and name not in ranks:
            ranks[name] = i
            i += 1
    return ranks


SIZE_TOKEN_RANKS = (
    ("ExtraSmall", 0),
    ("ExtraLarge", 4),
    ("Small", 1),
    ("Medium", 2),
    ("Large", 3),
)


def split_size_token(name: str) -> tuple[str, int, str]:
    for token, rank in SIZE_TOKEN_RANKS:
        for start, end in _source_token_spans(name, token):
            return name[:start], rank, name[end:]
    return name, 50, ""


def _shared_camel_prefix(left: str, right: str) -> str:
    n = 0
    for a, b in zip(left, right):
        if a != b:
            break
        n += 1
    while n > 0:
        left_ok = n == len(left) or left[n].isupper() or left[n].isdigit()
        right_ok = n == len(right) or right[n].isupper() or right[n].isdigit()
        if left_ok and right_ok:
            break
        n -= 1
    return left[:n]


def legacy_or_prefix_rank(source: str, ranks: dict[str, int]) -> int:
    if source in ranks:
        return ranks[source]
    stem = strip_source_colors(source)
    best_len = 0
    best_rank = 10_000_000
    for key, rank in ranks.items():
        if len(key) < 4:
            continue
        if source.startswith(key):
            after = source[len(key) : len(key) + 1]
            if after == "" or after.isupper() or after.isdigit():
                if len(key) > best_len:
                    best_len = len(key)
                    best_rank = rank
                elif len(key) == best_len:
                    best_rank = min(best_rank, rank)
                continue
        shared = _shared_camel_prefix(stem, strip_source_colors(key))
        n = len(shared)
        if n < 8:
            continue
        if n > best_len:
            best_len = n
            best_rank = rank
        elif n == best_len:
            best_rank = min(best_rank, rank)
    return best_rank


def status_sort_rank(source: str) -> int:
    if _source_token_spans(source, "Working"):
        return 0
    statuses = source_status(source)
    if "Stopped" in statuses:
        return 1
    if "Broken" in statuses:
        return 8
    if "Closed" in statuses:
        return 3
    if "Open" in statuses:
        return 4
    if "Empty" in statuses:
        return 5
    if "Full" in statuses:
        return 6
    return 2


def color_sort_rank(source: str) -> int:
    color = source_color(source)
    if not color:
        return 50
    order = [label for _token, label in _source_color_pairs()]
    seen: list[str] = []
    for label in order:
        if label not in seen:
            seen.append(label)
    try:
        return seen.index(color)
    except ValueError:
        return 50


def position_sort_rank(source: str) -> int:
    pos = source_position(source)
    return {"Corner": 0, "": 1, "Side": 2, "Offset": 3, "Left": 4, "Right": 5}.get(pos, 9)


def structure_glass_sort_rank(source: str) -> int:
    if source.startswith("glassBusiness"):
        return 0
    if source.startswith("glassIndustrial"):
        return 1
    if source.startswith("glassOpaque"):
        return 3
    if source.startswith("glassBulletproof") or source.startswith("glassRamp"):
        return 2
    return 9


# Shared helper family. First matching prefix wins. Color variants still join
# through strip_source_colors when they are not in this list.
HELPER_FAMILY_PREFIXES: tuple[tuple[str, tuple[str, ...]], ...] = (
    ("pipe", ("pipeSmall", "metalPipe")),
    (
        "poster",
        ("poster", "targetPoster", "signPoster", "signSnackPoster"),
    ),
    ("sign", ("sign",)),
    (
        "couch",
        ("couchUgly", "couchModern", "sectionalLeather", "sectionalPlaid"),
    ),
    ("chair", ("chair", "oldChair", "officeChair", "barStool", "wheelchair")),
    (
        "bed",
        (
            "bed02",
            "bedMadeNoFrame",
            "bedMessyNoFrame",
            "bunkBed",
            "bedroll",
            "mattress",
            "gurneyBed",
            "hospitalBed",
        ),
    ),
    ("fern", ("plantFern",)),
    ("cooler", ("cntCooler",)),
    ("locker", ("cntLockers",)),
    (
        "workstation",
        (
            "workbench",
            "forge",
            "campfire",
            "chemistryStation",
            "cementMixer",
            "tableSaw",
            "combineStation",
            "cntApiary",
            "cntChickenCoop",
        ),
    ),
    ("wine", ("decoWineBottle", "wineBarrel", "decoSodaCan")),
    (
        "tvMounted",
        ("tvSmallWall", "tvLargeWall", "tvExtraSmallWall"),
    ),
    (
        "tvStand",
        ("tvSmallStand", "tvLargeStand", "tvExtraSmallStand", "tvCRT"),
    ),
    ("electronics", ("decoLaptop", "decoHeadphones", "speaker")),
    (
        "boxes",
        (
            "cntLootCrateShamway",
            "cntCardboardBox",
            "cntGarageStorage",
            "cntShippingCrateHero",
            "cntShippingCrate",
        ),
    ),
    (
        "garage",
        (
            "cntToolBox",
            "cntRollingToolBox",
            "decoToolSet",
            "decoBucket",
            "decoCaulk",
            "decoCoronaWipes",
            "decoSprayCans",
        ),
    ),
    (
        "traps",
        (
            "electricalBox",
            "autoTurret",
            "bladeTrap",
            "dartTrap",
            "flamethrowerTrap",
            "m60Turret",
            "shotgunTurret",
            "tripwire",
            "electricfence",
        ),
    ),
    ("hanginglog", ("corpseHanging", "noCorpseHanging")),
    ("escalator", ("escalator",)),
    ("elevator", ("elevator",)),
    ("flashlight", ("flashlight",)),
    ("drape", ("curtainDrapes",)),
    ("crop", ("planted",)),
    ("tree", ("treePlanted", "treeOak", "tree",)),
    (
        "structure",
        (
            "iBeam",
            "bandit",
            "shippingContainer",
            "chainlink",
            "sandbag",
            "guardRail",
            "bollard",
            "parkingBlock",
            "jailBar",
            "tarpFence",
            "concretePlateRound",
            "helipad",
        ),
    ),
    (
        "structureglass",
        (
            "glassBusiness",
            "glassIndustrial",
            "glassBulletproof",
            "glassOpaqueBulletproof",
            "glassRamp",
        ),
    ),
    ("glass", ("glassBroken", "glassDebris")),
    ("painting", ("painting", "picture")),
    ("mushroom", ("mushroom",)),
)

# Within a shared family, force a stable user order. Unlisted sources stay at 50.
HELPER_MEMBER_ORDER: dict[str, int] = {
    "cntLootCrateShamway": 0,
    "cntCardboardBox": 1,
    "cntGarageStorage": 2,
    "cntShippingCrateHero": 3,
    "posterCalendarWorkingStiff": 0,
    "targetPoster1": 1,
    "targetPoster2": 2,
    "posterBlueprintPistol": 3,
    "posterBlueprintRifle": 4,
    "posterCat": 5,
    "posterCats": 6,
    "posterSparky": 7,
}


def strip_source_colors(source: str) -> str:
    """Drop color tokens so bed02ArmyGreen sits with bed02."""
    used = [False] * len(source)
    for token, _label in _source_color_pairs():
        for at, end in _source_token_spans(source, token):
            if any(used[i] for i in range(at, end)):
                continue
            for i in range(at, end):
                used[i] = True
    stripped = "".join(ch for i, ch in enumerate(source) if not used[i])
    return stripped or source


def helper_family_id(source: str, all_sources: list[str] | None = None) -> str:
    for group_id, prefixes in HELPER_FAMILY_PREFIXES:
        if any(source.startswith(p) for p in prefixes):
            return group_id
    if "Chair" in source:
        return "chair"
    stem = strip_source_colors(source)
    if all_sources:
        return helper_family_map(all_sources).get(source, stem)
    return stem


def helper_family_map(all_sources: list[str]) -> dict[str, str]:
    """One pass of shared-prefix grouping. Unique sources only."""
    unique = list(dict.fromkeys(all_sources))
    stripped = {s: strip_source_colors(s) for s in unique}
    out: dict[str, str] = {}
    for source in unique:
        family = None
        for group_id, prefixes in HELPER_FAMILY_PREFIXES:
            if any(source.startswith(p) for p in prefixes):
                family = group_id
                break
        if family is None and "Chair" in source:
            family = "chair"
        if family is None:
            stem = stripped[source]
            best = ""
            for other in unique:
                if other == source:
                    continue
                shared = _shared_camel_prefix(stem, stripped[other])
                if len(shared) >= 8 and len(shared) > len(best):
                    best = shared
            family = best if best else stem
        out[source] = family
    return out


def sort_helper_rows(
    kept_rows: list[tuple[str, str, ET.Element]],
    ranks: dict[str, int],
) -> list[tuple[str, str, ET.Element]]:
    parts: list[tuple[str, str, ET.Element, str, int, str, int, str]] = []
    size_prefixes: dict[str, set[int]] = defaultdict(set)
    family_min: dict[str, int] = {}
    all_sources = [source for source, _kind, _flat in kept_rows]
    family_ids = helper_family_map(all_sources)
    for source, kind, flat in kept_rows:
        prefix, size_rank, rest = split_size_token(source)
        rank = legacy_or_prefix_rank(source, ranks)
        family_id = family_ids.get(source) or helper_family_id(source)
        parts.append((source, kind, flat, prefix, size_rank, rest, rank, family_id))
        family_min[family_id] = min(family_min.get(family_id, 10_000_000), rank)
        if size_rank < 50:
            size_prefixes[prefix].add(size_rank)

    group_rank: dict[str, int] = {}
    for source, kind, flat, prefix, size_rank, rest, rank, family_id in parts:
        if size_rank < 50 and len(size_prefixes.get(prefix, ())) >= 2:
            group_rank[prefix] = min(group_rank.get(prefix, 10_000_000), rank)

    def sort_key(
        item: tuple[str, str, ET.Element, str, int, str, int, str],
    ):
        source, _kind, _flat, prefix, size_rank, rest, rank, family_id = item
        family = family_min[family_id]
        if size_rank < 50 and prefix in group_rank:
            family = min(family, group_rank[prefix])
        return (
            family,
            family_id,
            HELPER_MEMBER_ORDER.get(source, 50),
            1 if "Dirty" in source else 0,
            structure_glass_sort_rank(source),
            color_sort_rank(source),
            position_sort_rank(source),
            source_part_sort_rank(source),
            status_sort_rank(source),
            size_rank,
            rank,
            rest,
            source,
        )

    parts.sort(key=sort_key)
    return [(source, kind, flat) for source, kind, flat, *_ in parts]


def emit_kind(
    kind: str,
    source: str,
    flat: ET.Element,
    sort1: str,
    light_groups: dict[str, list[str]],
    include_cooking: bool = True,
) -> tuple[list[ET.Element], list[str]]:
    blocks: list[ET.Element] = []
    picker: list[str] = []
    if kind == "loot":
        blocks.extend(emit_loot_family(source, flat, sort1))
        picker.append(deco_name(source))
        if include_cooking and is_cook_source(source):
            blocks.extend(emit_campfire(source, flat, sort1))
            picker.append(deco_name(source, "Cooking"))
    elif kind == "powered":
        blocks.extend(emit_powered(source, flat, sort1))
        picker.append(deco_name(source))
    elif kind == "upgradeable":
        blocks.extend(emit_upgradeable_family(source, flat, sort1))
        picker.append(deco_name(source))
    else:
        blocks.extend(emit_deco_copy(source, flat, sort1))
        picker.append(deco_name(source))
        if include_cooking and is_cook_source(source):
            blocks.extend(emit_campfire(source, flat, sort1))
            picker.append(deco_name(source, "Cooking"))
    return blocks, picker


def build_all(
    flat_map: dict[str, ET.Element],
    order: list[str],
    all_names: set[str],
    light_groups: dict[str, list[str]],
) -> tuple[list[ET.Element], list[str], dict[str, int]]:
    source_to_canonical = {
        member: canon for canon, members in light_groups.items() for member in members
    }
    counts: dict[str, int] = defaultdict(int)
    classified: list[tuple[str, str, ET.Element]] = []
    for source in order:
        flat = flat_map.get(source)
        if flat is None:
            continue
        if not should_emit_block(flat, all_names):
            counts["flatten_skip"] += 1
            continue
        kind = classify(flat)
        if kind is None:
            counts["rewrite_skip"] += 1
            continue
        if kind == "powered":
            kept = source_to_canonical.get(source)
            if kept is None or kept != source:
                counts["light_collapse"] += 1
                continue
        classified.append((source, kind, flat))

    buckets: dict[tuple[str, ...], list[tuple[str, str, ET.Element]]] = defaultdict(list)
    for source, kind, flat in classified:
        buckets[visual_identity_key(flat)].append((source, kind, flat))
    kept_sources = set()
    for members in buckets.values():
        pick = pick_canonical_visual(members)
        kept_sources.add(pick[0])
        if len(members) > 1:
            counts["visual_collapse"] += len(members) - 1

    legacy_rank = load_legacy_helper_order()
    kept_rows = [
        (source, kind, flat)
        for source, kind, flat in classified
        if source in kept_sources
    ]
    kept_rows = sort_helper_rows(kept_rows, legacy_rank)

    blocks: list[ET.Element] = []
    picker: list[str] = []
    sort_i = 1
    for source, kind, flat in kept_rows:
        if not (is_cook_source(source) and kind in {"loot", "deco"}):
            continue
        blocks.extend(emit_campfire(source, flat, f"{sort_i:05d}"))
        picker.append(deco_name(source, "Cooking"))
        counts["campfire"] += 1
        sort_i += 1
    for source, kind, flat in kept_rows:
        family, picks = emit_kind(
            kind, source, flat, f"{sort_i:05d}", light_groups, include_cooking=False
        )
        blocks.extend(family)
        picker.extend(picks)
        counts[kind] += 1
        sort_i += 1
    return blocks, picker, counts


def build_examples(
    flat_map: dict[str, ET.Element],
    light_groups: dict[str, list[str]],
) -> tuple[list[ET.Element], list[str]]:
    blocks: list[ET.Element] = []
    picker: list[str] = []
    sort_i = 1
    source_to_canonical = {
        member: canon for canon, members in light_groups.items() for member in members
    }

    for source, kind in EXAMPLE_SOURCES:
        if kind == "powered":
            kept = source_to_canonical.get(source, source)
            if kept != source:
                blocks.append(
                    comment(f"KIND powered skip {source}; same fixture as {kept}")
                )
                continue
            source = kept
        flat = flat_map.get(source)
        if flat is None:
            raise SystemExit(f"Vanilla block not found: {source}")
        sort1 = f"{sort_i:04d}"
        sort_i += 1

        if kind == "loot":
            blocks.append(comment(f"KIND {kind} source={source}"))
            family = emit_loot_family(source, flat, sort1)
            blocks.extend(family)
            picker.append(deco_name(source))
            if is_cook_source(source):
                blocks.append(comment(f"KIND campfire twin source={source}"))
                cooking = emit_campfire(source, flat, sort1)
                blocks.extend(cooking)
                picker.append(deco_name(source, "Cooking"))
        elif kind == "powered":
            skipped = [n for n in light_groups.get(source, [source]) if n != source]
            skip_note = f"; skip {', '.join(skipped)}" if skipped else ""
            blocks.append(comment(f"KIND powered source={source}{skip_note}"))
            family = emit_powered(source, flat, sort1)
            blocks.extend(family)
            picker.append(deco_name(source))
        elif kind == "upgradeable":
            blocks.append(comment(f"KIND {kind} source={source}"))
            family = emit_upgradeable_family(source, flat, sort1)
            blocks.extend(family)
            picker.append(deco_name(source))
        else:
            blocks.append(comment(f"KIND {kind} source={source}"))
            family = emit_deco_copy(source, flat, sort1)
            blocks.extend(family)
            picker.append(deco_name(source))

    return blocks, picker


def cooking_block_names(middle: list[ET.Element]) -> list[str]:
    names: list[str] = []
    for el in middle:
        if el.tag != "block":
            continue
        for p in el.findall("property"):
            if p.get("name") == "Class" and p.get("value") == "Campfire":
                names.append(el.get("name") or "")
                break
    return [n for n in names if n]


LOC_HEADER = [
    "Key",
    "File",
    "Type",
    "UsedInMainMenu",
    "NoTranslate",
    "KeepLoaded",
    "english",
    "Context / Alternate Text",
    "german",
    "spanish",
    "french",
    "italian",
    "japanese",
    "koreana",
    "polish",
    "brazilian",
    "russian",
    "turkish",
    "schinese",
    "tchinese",
]
LOC_LANGS = (
    "english",
    "german",
    "spanish",
    "french",
    "italian",
    "japanese",
    "koreana",
    "polish",
    "brazilian",
    "russian",
    "turkish",
    "schinese",
    "tchinese",
)
LOC_LATIN = frozenset(
    {
        "english",
        "german",
        "spanish",
        "french",
        "italian",
        "polish",
        "brazilian",
        "turkish",
    }
)
DECO_PREFIX = "[ddcdfa]Deco [-]"
MASTER_NAMES = frozenset(
    {
        "agfDecoMaster",
        "agfDecoMasterWood",
        "agfDecoMasterIron",
        "agfDecoMasterSteel",
    }
)
LOC_COLOR_WORDS = (
    "Army Green",
    "ArmyGreen",
    "White",
    "Black",
    "Brown",
    "Yellow",
    "Green",
    "Grey",
    "Gray",
    "Silver",
    "Orange",
    "Purple",
    "Pink",
    "Brass",
    "Blue",
    "Red",
)
LOC_ENGLISH_OVERRIDES = {
    "cntLootCrateShamway": "Cardboard Box, 1",
    "cntCardboardBox": "Cardboard Box, 2",
    "cntGarageStorage": "Cardboard Box, 3",
    "cntShippingCrateHero": "Shipping Crate",
}
# Uncolored vanilla parents still have a default paint.
LOC_IMPLIED_COLORS = {
    "cntGunSafe": "Black",
    "cntGunSafeOpen": "Black",
    "cntLockersShortOpen": "Green",
    "cntLockersShortClosed": "Green",
    "cntLockersTallOpen": "Green",
    "cntLockersTallClosed": "Green",
    "cntLockersTallDoubleOpen": "Green",
    "cntLockersTallDoubleClosed": "Green",
    "cntLockersTallMixOpen": "Green",
    "cntLockersTallMixClosed": "Green",
    "cntLockersTallMixLinkOpen": "Green",
    "cntLockersTallMixLinkClosed": "Green",
    "cntBuriedFoodStashChest": "Blue",
    "cntBuriedWeaponChest": "Red",
    "cntBuriedLootStashChest": "Green",
    "cntIntroBuriedFoodChest": "Brown",
}
# Vanilla English says Luggage on the medium open/closed twins of cntSuitcase.
LOC_ENGLISH_BASE_ALIASES = {
    "cntLuggageMediumOpen": "Suitcase",
    "cntLuggageMediumClosed": "Suitcase",
    "cntBuriedFoodStashChest": "Buried Supplies",
    "cntBuriedWeaponChest": "Buried Supplies",
    "cntBuriedLootStashChest": "Buried Supplies",
    "cntIntroBuriedFoodChest": "Buried Supplies",
}
LOC_DECO_TAGS = {
    "english": "(Deco)",
    "german": "(Deko)",
    "spanish": "(Deco)",
    "french": "(Déco)",
    "italian": "(Deco)",
    "japanese": "(デコ)",
    "koreana": "(데코)",
    "polish": "(Deco)",
    "brazilian": "(Deco)",
    "russian": "(Деко)",
    "turkish": "(Deco)",
    "schinese": "(装饰)",
    "tchinese": "(裝飾)",
}
LOC_TAGS = {
    "loot": {
        "english": "(Loot)",
        "german": "(Beute)",
        "spanish": "(Saqueo)",
        "french": "(Butin)",
        "italian": "(Bottino)",
        "japanese": "(戦利品)",
        "koreana": "(전리품)",
        "polish": "(Lupy)",
        "brazilian": "(Espólios)",
        "russian": "(Добыча)",
        "turkish": "(Ganimet)",
        "schinese": "(战利品)",
        "tchinese": "(戰利品)",
    },
    "campfire": {
        "english": "(Campfire)",
        "german": "(Lagerfeuer)",
        "spanish": "(Fogatas)",
        "french": "(Feu de camp)",
        "italian": "(Falò)",
        "japanese": "(焚き火)",
        "koreana": "(모닥불)",
        "polish": "(Ognisko)",
        "brazilian": "(Fogueira)",
        "russian": "(Костёр)",
        "turkish": "(Kamp Ateşi)",
        "schinese": "(篝火)",
        "tchinese": "(營火)",
    },
    "structure": {
        "english": "(Structure)",
        "german": "(Struktur)",
        "spanish": "(Estructura)",
        "french": "(Structure)",
        "italian": "(Struttura)",
        "japanese": "(構造)",
        "koreana": "(구조)",
        "polish": "(Konstrukcja)",
        "brazilian": "(Estrutura)",
        "russian": "(Конструкция)",
        "turkish": "(Yapı)",
        "schinese": "(结构)",
        "tchinese": "(結構)",
    },
    "electric": {
        "english": "(Electric)",
        "german": "(Elektrisch)",
        "spanish": "(Eléctrico)",
        "french": "(Électrique)",
        "italian": "(Elettrico)",
        "japanese": "(電気)",
        "koreana": "(전기)",
        "polish": "(Elektryczny)",
        "brazilian": "(Elétrico)",
        "russian": "(Электрический)",
        "turkish": "(Elektrik)",
        "schinese": "(电力)",
        "tchinese": "(電力)",
    },
}
LOC_MATERIALS = {
    "iron": {
        "english": "Iron",
        "german": "Eisen",
        "spanish": "Hierro",
        "french": "Fer",
        "italian": "Ferro",
        "japanese": "鉄",
        "koreana": "철",
        "polish": "Żelazo",
        "brazilian": "Ferro",
        "russian": "Железо",
        "turkish": "Demir",
        "schinese": "铁",
        "tchinese": "鐵",
    },
    "steel": {
        "english": "Steel",
        "german": "Stahl",
        "spanish": "Acero",
        "french": "Acier",
        "italian": "Acciaio",
        "japanese": "鋼",
        "koreana": "강철",
        "polish": "Stal",
        "brazilian": "Aço",
        "russian": "Сталь",
        "turkish": "Çelik",
        "schinese": "钢",
        "tchinese": "鋼",
    },
}
HELPER_NAMES = {
    "english": "[ddcdfa]Deco [-]Block",
    "german": "[ddcdfa]Deco [-]Block",
    "spanish": "[ddcdfa]Deco [-]Bloque",
    "french": "[ddcdfa]Deco [-]Bloc",
    "italian": "[ddcdfa]Deco [-]Blocco",
    "japanese": "[ddcdfa]Deco [-]ブロック",
    "koreana": "[ddcdfa]Deco [-]블록",
    "polish": "[ddcdfa]Deco [-]Blok",
    "brazilian": "[ddcdfa]Deco [-]Bloco",
    "russian": "[ddcdfa]Deco [-]Блок",
    "turkish": "[ddcdfa]Deco [-]Blok",
    "schinese": "[ddcdfa]Deco [-]方块",
    "tchinese": "[ddcdfa]Deco [-]方塊",
}
DESC_ROWS = {
    "agfDecorationsDesc": {
        "english": "Crafted decoration helper. Select a model, then place it.",
        "german": "Herstellbarer Deko-Helfer. Wähle ein Modell und platziere es.",
        "spanish": "Ayudante de decoración. Elige un modelo y colócalo.",
        "french": "Assistant de décoration. Choisissez un modèle, puis placez-le.",
        "italian": "Assistente decorazioni. Seleziona un modello e piazzalo.",
        "japanese": "クラフトしたデコヘルパー。モデルを選んで設置します。",
        "koreana": "제작한 데코 헬퍼. 모델을 선택한 뒤 설치하세요.",
        "polish": "Wytworzony pomocnik dekoracji. Wybierz model i postaw go.",
        "brazilian": "Auxiliar de decoração. Selecione um modelo e coloque-o.",
        "russian": "Созданный помощник декора. Выберите модель и установите её.",
        "turkish": "Üretilmiş dekor yardımcısı. Bir model seçin, ardından yerleştirin.",
        "schinese": "制作出的装饰助手。先选择模型，再放置。",
        "tchinese": "製作出的裝飾助手。先選擇模型，再放置。",
    },
    "agfDecoBlockDesc": {
        "english": "A placed decoration. Break it to recover the Deco helper.",
        "german": "Ein platziertes Deko-Objekt. Zerstöre es, um den Deko-Helfer zurückzuerhalten.",
        "spanish": "Una decoración colocada. Rómpela para recuperar el ayudante Deco.",
        "french": "Une décoration placée. Cassez-la pour récupérer l'assistant Déco.",
        "italian": "Una decorazione piazzata. Rompila per recuperare l'assistente Deco.",
        "japanese": "設置した装飾ブロック。壊すとデコヘルパーが戻ります。",
        "koreana": "설치된 장식 블록입니다. 부수면 데코 헬퍼가 돌아옵니다.",
        "polish": "Umieszczona dekoracja. Zniszcz ją, aby odzyskać pomocnika Deco.",
        "brazilian": "Uma decoração colocada. Quebre-a para recuperar o auxiliar Deco.",
        "russian": "Установленное украшение. Разрушьте его, чтобы вернуть помощник Deco.",
        "turkish": "Yerleştirilmiş bir dekorasyon. Deco yardımcısını geri almak için kırın.",
        "schinese": "已放置的装饰方块。破坏后可收回装饰助手。",
        "tchinese": "已放置的裝飾方塊。破壞後可收回裝飾助手。",
    },
}


def load_loc_map(path: Path) -> dict[str, dict[str, str]]:
    out: dict[str, dict[str, str]] = {}
    with path.open(encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            key = (row.get("Key") or "").strip()
            if key and key not in out:
                out[key] = row
    return out


def loc_text_broken(text: str) -> bool:
    if not text:
        return True
    return "??" in text or "\ufffd" in text


def extract_base_name(text: str) -> str:
    if not text:
        return ""
    name = re.sub(r"^\[ddcdfa\](?:Deco|AGF) \[-\]", "", text)
    name = re.sub(r" \[[0-9a-fA-F]{6}\]\([^)]*\)\[-\]\s*$", "", name)
    return name.strip()


LOC_SOURCE_COLORS = (
    ("ArmyGreen", "Army Green"),
    ("White", "White"),
    "Black",
    "Brown",
    "Yellow",
    "Green",
    ("Grey", "Grey"),
    ("Gray", "Grey"),
    "Silver",
    "Orange",
    "Purple",
    "Pink",
    "Brass",
    "Blue",
    "Red",
)


def _source_color_pairs() -> list[tuple[str, str]]:
    pairs: list[tuple[str, str]] = []
    for item in LOC_SOURCE_COLORS:
        if isinstance(item, tuple):
            pairs.append(item)
        else:
            pairs.append((item, item))
    pairs.sort(key=lambda pair: len(pair[0]), reverse=True)
    return pairs


def peel_color(name: str) -> tuple[str, str]:
    text = re.sub(r"\s+", " ", name).strip()
    lower = text.lower()
    for color in sorted(LOC_COLOR_WORDS, key=len, reverse=True):
        cl = color.lower()
        if lower.startswith(cl + " ") or lower.startswith(cl + ", "):
            return text[len(color) :].strip(" ,"), text[: len(color)]
        if lower.endswith(" " + cl) or lower.endswith(", " + cl):
            return text[: -len(color)].strip(" ,"), text[-len(color) :]
    return text, ""


def source_color(source: str) -> str:
    used = [False] * len(source)
    found: list[tuple[int, str]] = []
    for token, label in _source_color_pairs():
        start = 0
        while True:
            at = source.find(token, start)
            if at < 0:
                break
            end = at + len(token)
            start = at + 1
            if any(used[i] for i in range(at, end)):
                continue
            before = source[at - 1] if at else ""
            after = source[end] if end < len(source) else ""
            ok_before = at == 0 or before.islower() or before.isdigit()
            ok_after = end == len(source) or after.isupper() or after.isdigit()
            if not (ok_before and ok_after):
                continue
            found.append((at, label))
            for i in range(at, end):
                used[i] = True
    if not found:
        return ""
    found.sort(key=lambda item: item[0])
    return found[-1][1]


LOC_SOURCE_POSITIONS = (
    ("SideCentered", "Side"),
    ("Side Centered", "Side"),
    ("UnderCounter", "Offset"),
    ("Under Counter", "Offset"),
    ("Offset", "Offset"),
    ("Corner", "Corner"),
    ("Left", "Left"),
    ("Right", "Right"),
)


def _label_pairs(items: tuple) -> list[tuple[str, str]]:
    pairs: list[tuple[str, str]] = []
    for item in items:
        if isinstance(item, tuple):
            pairs.append(item)
        else:
            pairs.append((item, item))
    pairs.sort(key=lambda pair: len(pair[0]), reverse=True)
    return pairs


def source_position(source: str) -> str:
    used = [False] * len(source)
    found: list[tuple[int, str]] = []
    for token, label in _label_pairs(LOC_SOURCE_POSITIONS):
        if " " in token:
            continue
        start = 0
        while True:
            at = source.find(token, start)
            if at < 0:
                break
            end = at + len(token)
            start = at + 1
            if any(used[i] for i in range(at, end)):
                continue
            before = source[at - 1] if at else ""
            after = source[end] if end < len(source) else ""
            ok_before = at == 0 or before.islower() or before.isdigit()
            ok_after = end == len(source) or after.isupper() or after.isdigit()
            if not (ok_before and ok_after):
                continue
            found.append((at, label))
            for i in range(at, end):
                used[i] = True
    if not found:
        if "wallOven" in source:
            return "Offset"
        return ""
    found.sort(key=lambda item: item[0])
    return found[-1][1]


def peel_position(name: str, source: str) -> tuple[str, str]:
    position = source_position(source)
    text = name
    for raw, label in _label_pairs(LOC_SOURCE_POSITIONS):
        pattern = re.compile(rf"(?:(?<=^)|(?<=,)|(?<=\s)){re.escape(raw)}(?=,|\s|$)", re.I)
        if pattern.search(text):
            text = pattern.sub("", text)
            text = re.sub(r"\s+,", ",", text)
            text = re.sub(r",\s*,", ",", text)
            text = re.sub(r"\s+", " ", text).strip(" ,")
            if not position:
                position = label
    return text, position


def source_pose(source: str) -> str:
    for token, label in _label_pairs(LOC_SOURCE_POSES):
        if " " in token:
            continue
        for start, end in _source_token_spans(source, token):
            if _rest_is_name_slots(source, end):
                return label
    return ""


def peel_pose(name: str, source: str) -> tuple[str, str]:
    pose = source_pose(source)
    text = name
    for raw, label in _label_pairs(LOC_SOURCE_POSES):
        pattern = re.compile(rf"(?:(?<=^)|(?<=,)|(?<=\s)){re.escape(raw)}(?=,|\s|$)", re.I)
        if pattern.search(text):
            text = pattern.sub("", text)
            text = re.sub(r"\s+,", ",", text)
            text = re.sub(r",\s*,", ",", text)
            text = re.sub(r"\s+", " ", text).strip(" ,")
            if not pose:
                pose = label
    return text, pose


LOC_SOURCE_STATUSES = (
    ("BrokenDoor", ("Open", "Broken")),
    ("Broken", ("Broken",)),
    ("Closed", ("Closed",)),
    ("Open", ("Open",)),
    ("Empty", ("Empty",)),
    ("Full", ("Full",)),
    ("Working", ("Working",)),
    ("Stopped", ("Stopped",)),
)
LOC_SOURCE_STATUS_EXACT = {
    "wallClock": ("Stopped",),
    "cntSuitcase": ("Closed",),
    "cntLuggageMediumClosed": ("Closed",),
}
LOC_STATUS_ORDER = ("Empty", "Full", "Open", "Closed", "Working", "Stopped", "Broken")
LOC_SOURCE_POSES = (
    ("Crooked1", "Crooked 1"),
    ("Crooked2", "Crooked 2"),
    ("Crooked 1", "Crooked 1"),
    ("Crooked 2", "Crooked 2"),
    ("Dirty", "Dirty"),
)
LOC_SOURCE_PARTS = (
    ("TallMixLink", ("Tall", "Mixed 2")),
    ("TallMix", ("Tall", "Mixed 1")),
    ("TallDouble", ("Tall", "6 Small")),
    ("Tall", ("Tall",)),
    ("Short", ("Short",)),
)
LOC_PART_TEXT_PEELS = (
    "Tall Double",
    "Mixed 1",
    "Mixed 2",
    "Double",
    "Short",
    "Tall",
)
LOC_STATUS_FOLLOW = frozenset(
    {token for token, _ in _source_color_pairs()}
    | {token for token, _ in _label_pairs(LOC_SOURCE_POSITIONS) if " " not in token}
    | {token for token, _ in LOC_SOURCE_STATUSES}
    | {token for token, _ in LOC_SOURCE_POSES if " " not in token}
    | {token for token, _ in LOC_SOURCE_PARTS}
    | {"Door"}
)


def _source_token_spans(source: str, token: str) -> list[tuple[int, int]]:
    spans: list[tuple[int, int]] = []
    start = 0
    while True:
        at = source.find(token, start)
        if at < 0:
            break
        end = at + len(token)
        start = at + 1
        before = source[at - 1] if at else ""
        after = source[end] if end < len(source) else ""
        ok_before = at == 0 or before.islower() or before.isdigit()
        ok_after = end == len(source) or after.isupper() or after.isdigit()
        if ok_before and ok_after:
            spans.append((at, end))
    return spans


def _rest_is_name_slots(source: str, end: int) -> bool:
    rest = source[end:]
    while rest:
        if rest[0].isdigit():
            rest = rest.lstrip("0123456789")
            if rest.startswith("m") and (len(rest) == 1 or rest[1].isupper() or rest[1].isdigit()):
                rest = rest[1:]
            elif rest[:1] in {"x", "X"}:
                rest = rest[1:]
            continue
        if len(rest) == 1 and rest.isalpha() and rest.isupper():
            rest = ""
            continue
        matched = False
        for token in sorted(LOC_STATUS_FOLLOW, key=len, reverse=True):
            if rest.startswith(token):
                after = rest[len(token) : len(token) + 1]
                if after == "" or after.isupper() or after.isdigit():
                    rest = rest[len(token) :]
                    matched = True
                    break
        if not matched:
            return False
    return True


# Vanilla Empty/Full on these is door pose (open/closed), not fill.
LOC_STATUS_REMAP_FAMILIES = (
    "DeskMetal",
    "DeskWood",
    "ArmoireDrawer",
    "MorticianDrawer",
    "wallOven",
    "Dishwasher",
)


def remap_status_words(source: str, statuses: list[str]) -> list[str]:
    if any(family in source for family in LOC_STATUS_REMAP_FAMILIES):
        swapped: list[str] = []
        for word in statuses:
            if word == "Empty":
                swapped.append("Open")
            elif word == "Full":
                swapped.append("Closed")
            else:
                swapped.append(word)
        statuses = swapped
    return [word for word in LOC_STATUS_ORDER if word in statuses]


def source_status(source: str) -> list[str]:
    labels: list[str] = []
    for word in LOC_SOURCE_STATUS_EXACT.get(source, ()):
        if word not in labels:
            labels.append(word)
    for token, words in sorted(LOC_SOURCE_STATUSES, key=lambda item: len(item[0]), reverse=True):
        for start, end in _source_token_spans(source, token):
            if not _rest_is_name_slots(source, end):
                continue
            for word in words:
                if word not in labels:
                    labels.append(word)
            break
    if source.startswith("cntGunSafe") and not _source_token_spans(source, "Open"):
        if "Closed" not in labels:
            labels.append("Closed")
    if source.startswith("cntCooler") and not _source_token_spans(source, "Open"):
        if "Closed" not in labels:
            labels.append("Closed")
    if source.startswith("cntFridgeStainlessSteel") and not _source_token_spans(
        source, "Open"
    ):
        if "Closed" not in labels:
            labels.append("Closed")
    return remap_status_words(source, labels)


def peel_status(name: str, source: str) -> tuple[str, list[str]]:
    statuses = source_status(source)
    text = re.sub(
        r"\s*[\(\uff08](?:Open|Closed|Empty|Full|Working|Stopped|Broken)[\)\uff09]",
        "",
        name,
        flags=re.I,
    )
    for raw in ("Broken Door", "Broken", "Closed", "Open", "Empty", "Full", "Working", "Stopped"):
        pattern = re.compile(rf"(?:(?<=^)|(?<=,)|(?<=\s)){re.escape(raw)}(?=,|\s|$)", re.I)
        if pattern.search(text):
            text = pattern.sub("", text)
            text = re.sub(r"\s+,", ",", text)
            text = re.sub(r",\s*,", ",", text)
            text = re.sub(r"\s+", " ", text).strip(" ,")
            if raw == "Broken Door":
                for word in ("Open", "Broken"):
                    if word not in statuses:
                        statuses.append(word)
            elif raw not in statuses:
                statuses.append(raw)
    return text, remap_status_words(source, statuses)


def source_part(source: str) -> list[str]:
    for token, labels in sorted(LOC_SOURCE_PARTS, key=lambda item: len(item[0]), reverse=True):
        for start, end in _source_token_spans(source, token):
            if _rest_is_name_slots(source, end):
                return list(labels)
    return []


def peel_part(name: str, source: str) -> tuple[str, list[str]]:
    labels = source_part(source)
    text = name
    for raw in LOC_PART_TEXT_PEELS:
        pattern = re.compile(
            rf"(?:(?<=^)|(?<=,)|(?<=\s)){re.escape(raw)}(?=,|\s|$)",
            re.I,
        )
        if pattern.search(text):
            text = pattern.sub("", text)
            text = re.sub(r"\s+,", ",", text)
            text = re.sub(r",\s*,", ",", text)
            text = re.sub(r"\s+", " ", text).strip(" ,")
    return text, labels


def source_part_sort_rank(source: str) -> int:
    if _source_token_spans(source, "TallMixLink"):
        return 4
    if _source_token_spans(source, "TallMix"):
        return 3
    if _source_token_spans(source, "TallDouble"):
        return 2
    if _source_token_spans(source, "Tall"):
        return 1
    if _source_token_spans(source, "Short"):
        return 0
    return 50


MEASURE_NAME_RE = re.compile(
    r"(?:(?<=^)|(?<=,)|(?<=\s))(?:\d+\s*[xX×х]\s*\d+|\d+\s*m)(?=,|\s|$)",
    re.I,
)
MEASURE_SOURCE_GRID_RE = re.compile(r"(\d+)x(\d+)", re.I)
MEASURE_SOURCE_METER_RE = re.compile(r"(\d+)m")


def _norm_measure(raw: str) -> str:
    text = raw.replace("×", "x").replace("х", "x")
    text = re.sub(r"\s+", "", text)
    grid = MEASURE_SOURCE_GRID_RE.fullmatch(text)
    if grid:
        return f"{grid.group(1)}x{grid.group(2)}"
    if text.lower().endswith("m"):
        return text[:-1] + "m"
    return text


def source_measure(source: str) -> tuple[str, str]:
    grid = MEASURE_SOURCE_GRID_RE.search(source)
    if grid:
        measure = f"{grid.group(1)}x{grid.group(2)}"
        rest = source[grid.end() :]
    else:
        meter = MEASURE_SOURCE_METER_RE.search(source)
        if not meter:
            return "", ""
        measure = f"{meter.group(1)}m"
        rest = source[meter.end() :]
    for token, _words in sorted(LOC_SOURCE_STATUSES, key=lambda item: len(item[0]), reverse=True):
        if rest.startswith(token):
            after = rest[len(token) : len(token) + 1]
            if after == "" or after.isupper() or after.isdigit():
                rest = rest[len(token) :]
    letter = rest if len(rest) == 1 and rest.isalpha() and rest.isupper() else ""
    return measure, letter


def peel_measure(name: str, source: str) -> tuple[str, str, str]:
    measure, letter = source_measure(source)
    text = name
    found = MEASURE_NAME_RE.search(text)
    if found:
        measure = _norm_measure(found.group(0))
        text = MEASURE_NAME_RE.sub("", text)
        text = re.sub(r"\s+,", ",", text)
        text = re.sub(r",\s*,", ",", text)
        text = re.sub(r"\s+", " ", text).strip(" ,")
    elif not MEASURE_SOURCE_METER_RE.search(source):
        measure = ""
    if letter:
        pattern = re.compile(rf"(?:(?<=^)|(?<=,)|(?<=\s)){re.escape(letter)}(?=,|\s|$)", re.I)
        if pattern.search(text):
            text = pattern.sub("", text)
            text = re.sub(r"\s+,", ",", text)
            text = re.sub(r",\s*,", ",", text)
            text = re.sub(r"\s+", " ", text).strip(" ,")
    return text, measure, letter


def humanize_source(source: str) -> str:
    name = source
    if name.startswith("cnt"):
        name = name[3:]
    name = re.sub(r"([a-z])([A-Z])", r"\1 \2", name)
    name = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1 \2", name)
    name = re.sub(r"([A-Za-z])([0-9])", r"\1 \2", name)
    name = re.sub(r"([0-9])([A-Za-z])", r"\1 \2", name)
    return name.strip() or source


def split_deco_name(name: str, name_set: set[str]) -> tuple[str, str, str]:
    rest = name[len("agfDeco") :]
    if rest.endswith("Cooking"):
        source = rest[: -len("Cooking")]
        if f"agfDeco{source}" in name_set:
            return source, "campfire", ""
    for suffix, tier in (
        ("SteelInsecure", "steel"),
        ("IronInsecure", "iron"),
        ("Insecure", ""),
        ("Steel", "steel"),
        ("Iron", "iron"),
    ):
        if not rest.endswith(suffix):
            continue
        source = rest[: -len(suffix)]
        if f"agfDeco{source}" not in name_set:
            continue
        if f"agfDeco{source}Insecure" in name_set:
            return source, "loot", tier
        return source, "structure", tier
    if f"{name}Insecure" in name_set:
        return rest, "loot", ""
    if f"{name}Iron" in name_set:
        return rest, "structure", ""
    return rest, "plain", ""


LOC_TAG_COLORS = {
    "plain": "ddcdfa",
    "loot": "c8e6be",
    "campfire": "f0beb9",
    "structure": "f0cdaa",
    "electric": "e6dca5",
}


def loc_category_tag(family: str, lang: str) -> str:
    if family == "plain":
        word = LOC_DECO_TAGS.get(lang) or LOC_DECO_TAGS["english"]
    else:
        word = LOC_TAGS.get(family, {}).get(lang, "")
    color = LOC_TAG_COLORS.get(family, "")
    if word and color:
        return f"[{color}]{word}[-]"
    return ""


def compose_loc_name(base: str, family: str, tier: str, lang: str, source: str = "") -> str:
    parts: list[str] = []
    if lang == "english":
        override = LOC_ENGLISH_OVERRIDES.get(source)
        if override:
            parts.extend(part.strip() for part in override.split(",") if part.strip())
        else:
            text = LOC_ENGLISH_BASE_ALIASES.get(source) or re.sub(
                r"\s*\((?:POI|PDI|PDT)\)", "", base
            ).strip()
            text, color = peel_color(text)
            if not color:
                color = source_color(source)
            if not color:
                color = LOC_IMPLIED_COLORS.get(source, "")
            text, pose = peel_pose(text, source)
            text, part_labels = peel_part(text, source)
            text, position = peel_position(text, source)
            text, statuses = peel_status(text, source)
            text, measure, letter = peel_measure(text, source)
            if text:
                parts.append(text)
            if pose and pose.lower() not in {p.lower() for p in parts}:
                parts.append(pose)
            for part in part_labels:
                if part.lower() not in {p.lower() for p in parts}:
                    parts.append(part)
            if position and position.lower() not in {p.lower() for p in parts}:
                parts.append(position)
            for status in statuses:
                if status.lower() not in {p.lower() for p in parts}:
                    parts.append(status)
            if color and color.lower() not in {p.lower() for p in parts}:
                parts.append(color)
            if measure and measure.lower() not in {p.lower() for p in parts}:
                parts.append(measure)
            if letter and letter.lower() not in {p.lower() for p in parts}:
                parts.append(letter)
    else:
        if base:
            parts.append(base)
    if tier:
        word = LOC_MATERIALS[tier].get(lang) or LOC_MATERIALS[tier]["english"]
        if word.lower() not in {p.lower() for p in parts}:
            parts.append(word)
    text = ", ".join(parts)
    tag = loc_category_tag(family, lang)
    if tag:
        return f"{text} {tag}".strip()
    return text


LOC_QUOTED_COLS = frozenset(LOC_LANGS)


def format_loc_row(row: dict[str, str]) -> str:
    cells: list[str] = []
    for col in LOC_HEADER:
        if col == "Context / Alternate Text":
            cells.append("")
            continue
        val = row.get(col, "") or ""
        if col in LOC_QUOTED_COLS:
            cells.append('"' + val.replace('"', '""') + '"')
        else:
            cells.append(val)
    return ",".join(cells)


def loc_row(key: str, names: dict[str, str], file_name: str = "blocks", typ: str = "Block") -> dict[str, str]:
    row = {col: "" for col in LOC_HEADER}
    row["Key"] = key
    row["File"] = file_name
    row["Type"] = typ
    for lang in LOC_LANGS:
        row[lang] = names.get(lang, names.get("english", ""))
    return row


def write_text_both(rel_path: str, text: str) -> None:
    for base in OUTPUT_ROOTS:
        path = base / rel_path
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        print(f"Wrote {path}")


def write_localization(middle: list[ET.Element], helper: ET.Element) -> None:
    vanilla = load_loc_map(GAME_LOC)
    legacy = load_loc_map(LEGACY_LOC) if LEGACY_LOC.is_file() else {}

    names: list[str] = []
    name_set: set[str] = set()
    light_names: set[str] = set()
    for el in middle:
        if el.tag != "block":
            continue
        name = el.get("name") or ""
        if not name or name in MASTER_NAMES:
            continue
        names.append(name)
        name_set.add(name)
        if prop_value(el, "Class") == "PoweredLight":
            light_names.add(name)
    helper_name = helper.get("name") or "agfDecorationsVariantHelper"

    rows: list[dict[str, str]] = []
    reused = 0
    built = 0
    humanized = 0

    rows.append(loc_row(helper_name, dict(HELPER_NAMES)))

    for key, texts in DESC_ROWS.items():
        rows.append(loc_row(key, texts))

    for name in names:
        source, family, tier = split_deco_name(name, name_set)
        if family == "plain" and name in light_names:
            family = "electric"
        vanilla_row = vanilla.get(source, {})
        old_row = legacy.get(name, {})
        lang_names: dict[str, str] = {}
        bases: dict[str, str] = {}
        used_old = False
        used_human = False
        for lang in LOC_LANGS:
            base = ""
            if lang in LOC_LATIN and old_row:
                old_text = old_row.get(lang) or ""
                if old_text and not loc_text_broken(old_text):
                    base = extract_base_name(old_text)
                    used_old = True
            if not base:
                vanilla_text = vanilla_row.get(lang) or ""
                if vanilla_text and not loc_text_broken(vanilla_text):
                    base = vanilla_text
            if not base and lang != "english":
                base = bases.get("english", "")
            if not base:
                base = humanize_source(source)
                used_human = True
            bases[lang] = base
            lang_names[lang] = compose_loc_name(base, family, tier, lang, source)
        if used_old:
            reused += 1
        else:
            built += 1
        if used_human and not vanilla_row.get("english"):
            humanized += 1
        rows.append(loc_row(name, lang_names))

    lines = [",".join(LOC_HEADER)]
    lines.extend(format_loc_row(row) for row in rows)
    write_text_both("Config/Localization.csv", "\n".join(lines) + "\n")
    print(
        f"Localization keys {len(rows)} "
        f"(reused {reused}, new {built}, humanized {humanized})",
        flush=True,
    )


def write_xml_both(rel_path: str, root: ET.Element) -> None:
    ET.indent(root, space="\t")
    xml = '<?xml version="1.0" encoding="UTF-8"?>\n' + ET.tostring(root, encoding="unicode") + "\n"
    for base in OUTPUT_ROOTS:
        path = base / rel_path
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(xml, encoding="utf-8")
        print(f"Wrote {path}", flush=True)


def write_cooking_ui(cooking_names: list[str]) -> None:
    ui = ET.Element("AGFVanillaPlus-DecorationBlock")
    ui.append(comment("Campfire categories. One list per Cooking twin."))
    ui_append = ET.SubElement(ui, "append", {"xpath": "/ui_display_info/crafting_category_display"})
    for name in cooking_names:
        clist = ET.SubElement(ui_append, "crafting_category_list", {"display_type": name})
        for cat, icon, label in CAMPFIRE_CATEGORIES:
            ET.SubElement(
                clist,
                "crafting_category",
                {"name": cat, "icon": icon, "display_name": label},
            )
    write_xml_both("Config/ui_display.xml", ui)

    xui = ET.Element("AGFVanillaPlus-DecorationBlock")
    xui.append(comment("v3.1 has no /xui/ruleset. Append window groups to /xui."))
    xui_append = ET.SubElement(xui, "append", {"xpath": "/xui"})
    for name in cooking_names:
        group = ET.SubElement(
            xui_append,
            "window_group",
            {
                "name": f"workstation_{name}",
                "controller": "XUiC_WorkstationWindowGroup",
                "open_backpack_on_open": "true",
                "close_compass_on_open": "true",
                "defaultselected": "bp.content",
            },
        )
        for win in CAMPFIRE_WINDOWS:
            ET.SubElement(group, "window", {"name": win})
    write_xml_both("Config/XUi_InGame/xui.xml", xui)


# Look-based tags. Kind (loot/campfire/powered) is not the category.
FAMILY_LOOK: dict[str, tuple[str, str]] = {
    "pipe": ("Pipes", "Pipes"),
    "sign": ("Wall Art", "Signs"),
    "poster": ("Wall Art", "Posters"),
    "painting": ("Wall Art", "Paintings"),
    "couch": ("Furniture", "Couches"),
    "chair": ("Furniture", "Chairs"),
    "bed": ("Furniture", "Beds"),
    "drape": ("Furniture", "Drapes"),
    "fern": ("Plants", "Ferns"),
    "crop": ("Plants", "Crops"),
    "tree": ("Plants", "Trees"),
    "mushroom": ("Plants", "Mushrooms"),
    "cooler": ("Kitchen", "Coolers"),
    "wine": ("Kitchen", "Drinks"),
    "locker": ("Storage", "Lockers"),
    "boxes": ("Storage", "Boxes"),
    "workstation": ("Work", "Workstations"),
    "garage": ("Work", "Garage"),
    "electronics": ("Electronics", "Devices"),
    "tvStand": ("Electronics", "TVs Stand"),
    "tvMounted": ("Electronics", "TVs Mounted"),
    "flashlight": ("Lights", "Flashlights"),
    "traps": ("Work", "Traps"),
    "hanginglog": ("Outdoor", "Hanging Logs"),
    "escalator": ("Work", "Transit"),
    "elevator": ("Work", "Transit"),
    "structureglass": ("Structure", "Glass"),
    "glass": ("Structure", "Broken Glass"),
}

# First matching needle in source or family wins. Specific tokens before short ones.
LOOK_RULES: tuple[tuple[tuple[str, ...], str, str], ...] = (
    (("toaster", "dishwasher", "hoodrange", "microwave", "walloven", "gasrange",
      "charcoalgrill", "gasgrill", "coffeemaker", "cookingpot", "woodburningstove",
      "stovemodern", "stoveold", "sodafountain"), "Kitchen", "Appliances"),
    (("minibeverage", "watercooler", "cntfridge", "retrofridge", "cntfreezer",
      "icemachine"), "Kitchen", "Coolers"),
    (("sinkkitchen", "countermountedsink", "utilitysink"), "Kitchen", "Sinks"),
    (("cnttoilet", "urinal", "bathstall", "bathroomstall", "portapotty",
      "drinkingfountain", "clawfoot", "cntbathtub", "showerhandle", "showerhead",
      "pedestalsink", "wallmountsink", "faucet"), "Bathroom", "Fixtures"),
    (("waterheater", "cntwasher", "cntdryer", "laundrycart", "laundrypile",
      "laundryloose", "cntbasketlaundry"), "Bathroom", "Utilities"),
    (("aircondition", "radiatorhouse", "capchimney"), "Bathroom", "HVAC"),
    (("verticalblinds", "miniblind", "screencurtain"), "Furniture", "Blinds"),
    (("restaurantbooth", "fastfoodbooth", "theaterseat", "churchpew", "parkbench",
      "schoolseat", "schooldesk"), "Furniture", "Chairs"),
    (("cntdesk", "officedesk", "officetable", "endtable", "restauranttable",
      "diningtable", "picnictable", "tablecommun", "decotableround", "cntnightstand",
      "pooltable"), "Furniture", "Tables"),
    (("cntarmoire", "modularcloset", "cntfilecabinet", "medicinecabinet"),
     "Furniture", "Cabinets"),
    (("cntgunsafe", "cntfootlocker", "cntdesksafe", "cntwallsafe", "cntatms"),
     "Storage", "Safes"),
    (("cntwoodenchest", "cntoldwestchest", "cnthardenedchest", "cntlootchest",
      "cntburied", "introburied", "cntcasket", "cntcoffin", "cntmunitions",
      "militarygoods"), "Storage", "Chests"),
    (("cnttrash", "bintrash", "cntdomedtrash", "cntdumpster", "rubbish", "garbage_decor",
      "garbagecan", "emberpile"), "Storage", "Trash"),
    (("sportsbag", "cntbackpack", "cntsuitcase", "cntluggage", "cntduffle",
      "cntpurse", "cntweaponsbag"), "Storage", "Bags"),
    (("cntmailbox", "apartmentmailbox", "apartmentsmallmailbox", "cntpostmailbox"),
     "Office", "Mail"),
    (("cntbookshelf", "cntshelf", "cntstoreshelf", "cntshelves", "cntbookpile",
      "calendar2034"), "Office", "Shelves"),
    (("pallet", "brickstack", "bricksstack", "drywallstack", "osbwood", "plywoodstack",
      "woodplanksstack"), "Outdoor", "Pallets"),
    (("tentbiohazard", "cntcampingtent"), "Outdoor", "Tents"),
    (("awning", "umbrellatable"), "Outdoor", "Awnings"),
    (("flagwallhung", "flagpole"), "Outdoor", "Flags"),
    (("gravestone", "bodybag", "cobweb", "hangingmoss"), "Outdoor", "Graveyard"),
    (("planthouse", "plantaloe", "plantcherry", "planthydro", "planthanging",
      "planthedge", "plantshrub", "planter", "birdbath", "forestflower"),
     "Plants", "Potted"),
    (("cntsedan", "crumpledsedan", "cntminivan", "cntpickup", "cntsuv", "cntcar03", "crushedcars",
      "abandonedsuv", "abandonedpickup", "burntsedan", "cntpolicecar", "decoCar",
      "hubcap", "bicycle", "minibike", "motorcycle", "4x4static", "gyrocopter"),
     "Vehicles", "Cars"),
    (("cntservicetruck", "cntboxtruck", "cntsemitruck", "semiflatbed", "cntfiretruck",
      "cntarmytruck", "cntfarmtruck", "cntbusschool", "cntbuscity", "cntbusshuttle",
      "cntambulance", "backhoe", "excavator", "tractor", "forklift", "wildwestwagon"),
     "Vehicles", "Trucks"),
    (("lantern", "streetlight", "tablelamp", "desklamp", "ceilinglight",
      "candelabra", "chandelier", "fluorescent", "industriallight", "lightporch",
      "lightsconce", "lightwall", "recessedlight", "tracklight", "ceilingfan",
      "candle", "walltorch", "burningbarrel", "jackolantern", "roadbarricade",
      "lightpanel", "lightceiling", "lightindustrial", "lightdisplay"),
     "Lights", "Fixtures"),
    (("laptop", "headphone", "speaker", "decoComputer", "microphone", "projector",
      "radioham", "studiocamera", "loudspeaker", "serverrack"), "Electronics", "Devices"),
    (("cntvending", "cntgaspump", "cntcashregister", "cashregister", "cntstoreproduce",
      "cntclothesrack", "cntclothesshelf", "cntshoespile", "cntclothes", "mannequin",
      "cntdisplaycase", "cntgunrack", "cntmilitarymetal", "cntstoreelectronics"),
     "Store", "Fixtures"),
    (("conduit", "duct", "breakerbox", "powerswitch", "pushbutton", "pressureplate",
      "electric", "motionsensor", "commercial", "utilitygas", "utilitytrans",
      "residentialsingle", "generatorbank", "batterybank", "solarbank",
      "controlpanel", "satellite"), "Work", "Electrical"),
    (("factoryconveyor", "lifthydraulic", "decohoist", "tankpropane", "cntbarrel",
      "coneconstruction", "cntutilitycart", "cntjanitor", "cnttilttruck",
      "handtruck", "keystone"), "Work", "Industrial"),
    (("shippingcontainer",), "Structure", "Containers"),
    (("chainlink", "sandbag", "tarphanging", "tarpfence", "guardrail", "jailbar",
      "bollard", "parkingblock", "parkingmeter", "barrier", "barbed", "ironwrought",
      "logwall"), "Structure", "Fences"),
    (("ibeam", "bandit", "helipad", "concreteplate"), "Structure", "Beams"),
    (("haybale", "cnttrough", "horseshoe", "driftwood", "resourcerock", "ore",
      "rock0", "cinderblock", "firewood", "cowskull"), "Outdoor", "Farm / Nature"),
    (("decometal", "swingset", "basketball", "bleachers", "decobenchpress",
      "stationarybike", "decotreadmill", "decoweight", "shootingrange"),
     "Outdoor", "Recreation"),
    (("doghouse", "petcage", "reptile", "cntbirdnest"), "Outdoor", "Pets"),
    (("cntcollapsed", "dewcollector", "forgeworkstation"), "Work", "Workstations"),
    (("wallclock", "wallmirror", "keyrack", "rugbear", "trophy", "woodenbear",
      "standiv", "piano", "newspaper", "churchbell", "mortician"), "Furniture", "Decor"),
    (("blooddecor",), "Outdoor", "Gore"),
    (("cntfoodpile", "cntmedic", "cntpillcase", "cntchempile", "cntliquorpile",
      "cntammopile", "cntlootcrate"), "Storage", "Piles"),
    (("cntshopping",), "Store", "Carts"),
    (("firehydrant",), "Outdoor", "Street"),
    (("modularrope",), "Outdoor", "Farm / Nature"),
    (("newsdispense", "newspaperdispenser"), "Office", "Mail"),
    (("utilityinsulator",), "Work", "Electrical"),
    (("torchwallholder",), "Lights", "Fixtures"),
)


def look_category(source: str, family_id: str, kind: str) -> tuple[str, str]:
    """Parent and child from what the model is, not loot class."""
    if kind == "campfire":
        return "Kitchen", "Campfires"
    s = source.lower()
    f = family_id.lower()
    blob = f"{s} {f}"
    if family_id == "structure":
        if "shippingcontainer" in s:
            return "Structure", "Containers"
        if any(
            x in s
            for x in (
                "chainlink",
                "fence",
                "sandbag",
                "tarp",
                "guardrail",
                "jail",
                "bollard",
                "parking",
                "barrier",
                "barbed",
            )
        ):
            return "Structure", "Fences"
        return "Structure", "Beams"
    mapped = FAMILY_LOOK.get(family_id)
    if mapped:
        return mapped
    for needles, parent, child in LOOK_RULES:
        if any(n.lower() in blob for n in needles):
            return parent, child
    if kind in {"powered", "electric"}:
        return "Lights", "Fixtures"
    if kind == "structure":
        return "Structure", "Beams"
    if s == "switch" or f == "switch":
        return "Work", "Electrical"
    return "Unsorted", "Review"


def suggested_category(source: str, family_id: str, kind: str) -> str:
    parent, child = look_category(source, family_id, kind)
    return f"{parent} / {child}"


def write_helper_catalog(middle: list[ET.Element], helper: ET.Element) -> None:
    """Helper picker list for category and order edits. Not shipped with the game copy."""
    loc = load_loc_map(DRAFT_ROOT / "Config" / "Localization.csv")
    picker = [n for n in (prop_value(helper, "PlaceAltBlockValue") or "").split(",") if n]
    picker_set = set(picker)
    name_set = {
        el.get("name") or ""
        for el in middle
        if el.tag == "block" and el.get("name")
    }
    by_name = {
        el.get("name"): el
        for el in middle
        if el.tag == "block" and el.get("name")
    }
    sources = []
    for name in picker:
        source, _kind, _tier = split_deco_name(name, name_set)
        sources.append(source)
    family_ids = helper_family_map(list(dict.fromkeys(sources)))
    rows: list[dict[str, str]] = []
    for name in picker:
        el = by_name.get(name)
        if el is None:
            continue
        source, kind, _tier = split_deco_name(name, name_set)
        family_id = family_ids.get(source) or helper_family_id(source)
        english = (loc.get(name) or {}).get("english") or ""
        parent, child = look_category(source, family_id, kind)
        rows.append(
            {
                "BlockName": name,
                "EnglishName": english,
                "SortOrder1": prop_value(el, "SortOrder1") or "",
                "SortOrder2": prop_value(el, "SortOrder2") or "",
                "Kind": kind,
                "HelperFamily": family_id,
                "Parent": parent,
                "Child": child,
            }
        )
    rows.sort(key=lambda r: (r["SortOrder1"], r["BlockName"]))
    path = HERE / "helper_categories.csv"
    with path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(
            f,
            fieldnames=[
                "BlockName",
                "EnglishName",
                "SortOrder1",
                "SortOrder2",
                "Kind",
                "HelperFamily",
                "Parent",
                "Child",
            ],
        )
        writer.writeheader()
        writer.writerows(rows)
    print(f"Wrote {path} ({len(rows)} helper rows)", flush=True)


def write_draft(masters: list[ET.Element], middle: list[ET.Element], helper: ET.Element) -> None:
    top = ET.Element("AGFVanillaPlus-DecorationBlock")
    top.append(comment("Masters"))
    append_masters = ET.SubElement(top, "append", {"xpath": "/blocks"})
    for b in masters:
        append_masters.append(b)

    top.append(comment("Clones by kind"))
    append_mid = ET.SubElement(top, "append", {"xpath": "/blocks"})
    for b in middle:
        append_mid.append(b)

    top.append(comment("Variant helper last"))
    append_help = ET.SubElement(top, "append", {"xpath": "/blocks"})
    append_help.append(helper)

    add_larger_storage_loot_switch(top)
    write_xml_both("Config/blocks.xml", top)


def add_larger_storage_loot_switch(top: ET.Element) -> None:
    """Point Deco loot at Larger Storage *Large lists when that mod is loaded."""
    top.append(
        comment(
            "LootList switch. Sizes live in vanilla or Larger Storage Option. "
            "Not in this mod loot.xml."
        )
    )
    cond = ET.SubElement(top, "conditional")
    iff = ET.SubElement(
        cond,
        "if",
        {"cond": "mod_loaded('AGF-VP-LargerStorageOption')"},
    )
    pairs = (
        ("playerWoodWritableStorage", "playerWoodWritableStorageLarge"),
        ("playerIronWritableStorage", "playerIronWritableStorageLarge"),
        ("playerSteelWritableStorage", "playerSteelWritableStorageLarge"),
    )
    for src, dst in pairs:
        xpath = (
            "/blocks/block[starts-with(@name,'agfDeco')]"
            "/property[@class='CompositeFeatures']"
            "/property[@class='TEFeatureStorage']"
            f"/property[@name='LootList' and @value='{src}']/@value"
        )
        set_el = ET.SubElement(iff, "set", {"xpath": xpath})
        set_el.text = dst


def write_loot_stub() -> None:
    top = ET.Element("AGFVanillaPlus-DecorationBlock")
    top.append(
        comment(
            "No loot containers here. Deco uses vanilla writable lists, "
            "or Larger Storage Option *Large lists via the blocks.xml conditional."
        )
    )
    write_xml_both("Config/loot.xml", top)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--all",
        action="store_true",
        help="Generate every kept flattened block. Default is the example set.",
    )
    args = parser.parse_args()
    try:
        sys.stdout.reconfigure(line_buffering=True)
    except Exception:
        pass

    print("Flatten vanilla blocks for visuals...", flush=True)
    flat_map, order, all_names = load_flat_map()
    light_groups = group_light_fixtures(flat_map)
    light_in = sum(len(v) for v in light_groups.values())
    print(f"Light fixtures: {light_in} vanilla lights -> {len(light_groups)} clones")
    masters = emit_masters()
    if args.all:
        middle, picker, counts = build_all(flat_map, order, all_names, light_groups)
        print("Kind counts:")
        for key in (
            "loot",
            "campfire",
            "powered",
            "upgradeable",
            "deco",
            "flatten_skip",
            "rewrite_skip",
            "light_collapse",
            "visual_collapse",
        ):
            print(f"  {key}: {counts.get(key, 0)}")
    else:
        middle, picker = build_examples(flat_map, light_groups)
    helper = emit_helper(picker)
    write_draft(masters, middle, helper)
    write_localization(middle, helper)
    write_helper_catalog(middle, helper)
    write_loot_stub()
    cooking = cooking_block_names(middle)
    write_cooking_ui(cooking)
    print(f"Masters {len(masters)}; middle nodes {len(middle)}; helper picks {len(picker)}", flush=True)
    if not args.all:
        print("Cooking UI:", ", ".join(cooking) if cooking else "(none)")
        print("Picker:", ", ".join(picker))
    else:
        print(f"Cooking window groups: {len(cooking)}")


if __name__ == "__main__":
    main()
