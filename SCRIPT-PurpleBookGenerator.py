"""SCRIPT-GenPurpleBook.py
=============================
Generates the AGF Purple Book ``Schematics`` window from vanilla
``progression.xml`` (+ optional mod overlays) plus a small constants module
that supplies the book-cover sprite per crafting_skill.

This first pass produces ONLY the ``craftinglistTab`` (magazines view).
The other three tabs (booksTab/unlockablesTab/armorsTab) are emitted as empty
placeholders so the resulting mod loads in-game.

Output: ``01_Draft/AGF-PurpleBookTest-vX.Y.Z/Config/XUi/windows.xml``.

Usage::

    python SCRIPT-GenPurpleBook.py [--out-mod AGF-PurpleBookTest-v0.0.1]
                                   [--progression <path>]
"""
from __future__ import annotations

import argparse
import math
import os
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field
from pathlib import Path

# ---------------------------------------------------------------------------
# Paths & constants
# ---------------------------------------------------------------------------

WORKSPACE = Path(__file__).resolve().parent
GAME_CONFIG = Path(r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config")
DEFAULT_PROGRESSION = GAME_CONFIG / "progression.xml"
DEFAULT_ITEMS = GAME_CONFIG / "items.xml"
DEFAULT_BLOCKS = GAME_CONFIG / "blocks.xml"
DEFAULT_OUT_MOD = "AGF-PurpleBookTest-v0.0.1"

# Window frame
# panel="Left" auto-stacks windows: the <window>'s own pos attribute is
# IGNORED for layout (XUi.LayoutWindowsInPanel sets localPosition.y per
# window in stacking order). Vertical placement of the entire panel is
# controlled by <window_group stack_panel_y_offset="..."> in xui.xml,
# whose default is 457 (NGUI: smaller value = panel sits lower on screen).
WIN_WIDTH = 1390
WIN_HEIGHT = 720
STACK_PANEL_Y_OFFSET = 397  # default 457 minus 60 px to clear page-header band
MAG_STRIP_TOP_Y = -10
MAG_STRIP_HEIGHT = 100  # the 23-icon row at the top of every zoomed view
USABLE_TOP_Y = MAG_STRIP_TOP_Y - MAG_STRIP_HEIGHT  # -110
USABLE_CENTER_Y = (USABLE_TOP_Y - WIN_HEIGHT) // 2  # = -475

# Per-sprite tweaks for the Food compact section. Keys = sprite or item name.
# size: int (overrides 13). dx, dy: ints added to default position.
# Bottom-alignment to y=-30 is preserved automatically when size changes.
FOOD_ICON_OVERRIDES: dict[str, dict] = {
    "drinkJarBeer": dict(dy=-2),
}

# Force tooltip text on specific items (overrides entity-name fallback).
# Used for the armor row icons which are representatives for a whole class.
TOOLTIP_OVERRIDES: dict[str, str] = {
    "armorAthleticOutfit": "agfArmorLight",
    "armorAssassinOutfit": "agfArmorMedium",
    "armorMinerOutfit":    "agfArmorHeavy",
}

# Quality colors (Q1..Q6)
QCOLORS = [
    "156, 136, 103",  # Q1 brown
    "207, 119, 41",   # Q2 orange
    "162, 164, 19",   # Q3 yellow
    "50, 194, 52",    # Q4 green
    "49, 93, 206",    # Q5 blue
    "164, 42, 204",   # Q6 purple
]

# Per-skill book-cover sprite (lifted from existing Purple Book v2.0.1)
BOOK_SPRITE: dict[str, str] = {
    "craftingArmor":           "bookArmoredUp",
    "craftingBlades":          "bookKnifeGuy",
    "craftingBows":            "bookBowHunters",
    "craftingClubs":           "bookBigHitters",
    "craftingElectrician":     "bookWiring101",
    "craftingExplosives":      "bookExplosiveMagazine",
    "craftingFood":            "bookHomeCookingWeekly",
    "craftingHandguns":        "bookHandgunMagazine",
    "craftingHarvestingTools": "bookToolsDigest",
    "craftingKnuckles":        "bookFuriousFists",
    "craftingMachineGuns":     "bookTacticalWarfare",
    "craftingMedical":         "bookMedicalJournal",
    "craftingRepairTools":     "bookHandyLand",
    "craftingRifles":          "bookRifleWorld",
    "craftingRobotics":        "bookTechPlanet",
    "craftingSalvageTools":    "bookScrapping4Fun",
    "craftingSeeds":           "bookSouthernFarming",
    "craftingShotguns":        "bookShotgunWeekly",
    "craftingSledgehammers":   "bookGetHammered",
    "craftingSpears":          "bookSharpSticks",
    "craftingTraps":           "bookElectricalTraps",
    "craftingVehicles":        "bookVehicleAdventures",
    "craftingWorkstations":    "bookForgeAhead",
}

# Per-skill magazine item name. Used as text_key on the title label so the
# localized magazine name (e.g. "Tech Planet") shows above the cover sprite.
MAG_ITEM: dict[str, str] = {
    "craftingArmor":           "armorSkillMagazine",
    "craftingBlades":          "bladesSkillMagazine",
    "craftingBows":            "bowsSkillMagazine",
    "craftingClubs":           "clubsSkillMagazine",
    "craftingElectrician":     "electricianSkillMagazine",
    "craftingExplosives":      "explosivesSkillMagazine",
    "craftingFood":            "foodSkillMagazine",
    "craftingHandguns":        "handgunsSkillMagazine",
    "craftingHarvestingTools": "harvestingToolsSkillMagazine",
    "craftingKnuckles":        "knucklesSkillMagazine",
    "craftingMachineGuns":     "machineGunsSkillMagazine",
    "craftingMedical":         "medicalSkillMagazine",
    "craftingRepairTools":     "repairToolsSkillMagazine",
    "craftingRifles":          "riflesSkillMagazine",
    "craftingRobotics":        "roboticsSkillMagazine",
    "craftingSalvageTools":    "salvageToolsSkillMagazine",
    "craftingSeeds":           "seedSkillMagazine",
    "craftingShotguns":        "shotgunsSkillMagazine",
    "craftingSledgehammers":   "sledgehammersSkillMagazine",
    "craftingSpears":          "spearsSkillMagazine",
    "craftingTraps":           "trapsSkillMagazine",
    "craftingVehicles":        "vehiclesSkillMagazine",
    "craftingWorkstations":    "workstationSkillMagazine",
}

# Display order for tabs across the top row (file order != display order)
TAB_ORDER = [
    "craftingRobotics", "craftingBows", "craftingHandguns", "craftingRifles",
    "craftingMachineGuns", "craftingShotguns", "craftingHarvestingTools",
    "craftingBlades", "craftingClubs", "craftingKnuckles",
    "craftingSledgehammers", "craftingSpears", "craftingSalvageTools",
    "craftingRepairTools", "craftingArmor", "craftingTraps", "craftingMedical",
    "craftingVehicles", "craftingElectrician", "craftingSeeds",
    "craftingExplosives", "craftingWorkstations", "craftingFood",
]

# Short name (without the ``crafting`` prefix); used in rect names.
def short(skill_name: str) -> str:
    return skill_name[len("crafting"):]

# ---------------------------------------------------------------------------
# Data model
# ---------------------------------------------------------------------------

@dataclass
class DisplayRow:
    """One ``<display_entry>`` from a crafting_skill block.

    Two flavors:
      - tiered (1 item, 6 unlock_level numbers, has quality)
      - non-tiered (N items, M thresholds, has_quality="false")
    """
    items: list[str]                    # item or icon names (flattened)
    unlock_levels: list[int]            # threshold numbers (per quality OR per item)
    name_keys: list[str] = field(default_factory=list)  # tooltip/loc keys
    has_quality: bool = True
    # Non-tiered rows: items grouped by unlock_level position. Each list
    # corresponds to the items unlocked at unlock_levels[i]. For tiered rows
    # this is normally a single group repeated.
    level_items: list[list[str]] = field(default_factory=list)
    level_names: list[list[str]] = field(default_factory=list)
    # Tiered rows only: optional multi-row icon layout. When set, the zoomed
    # qsection becomes 50px taller per extra icon row, with icons stacked at
    # y=-40, y=-90, y=-140, ... Used by Armored Up to show all
    # Light/Medium/Heavy outfit variants in three rows.
    icon_rows: list[list[str]] = field(default_factory=list)
    icon_row_names: list[list[str]] = field(default_factory=list)

    @property
    def is_tiered(self) -> bool:
        # 6 quality thresholds + has_quality=true â†’ quality-tiered row
        # (number of items can vary: 1 for typical, 3+ for shared armor tiers)
        return self.has_quality and len(self.unlock_levels) == 6

@dataclass
class Skill:
    name: str            # craftingRifles
    max_level: int
    icon: str
    display: list[DisplayRow] = field(default_factory=list)

    @property
    def short_name(self) -> str:
        return short(self.name)

    @property
    def book_sprite(self) -> str:
        return BOOK_SPRITE.get(self.name, "ui_game_symbol_book")

# ---------------------------------------------------------------------------
# Parsing
# ---------------------------------------------------------------------------

@dataclass
class ItemIcon:
    """Resolved icon properties for a single item from items.xml.

    sprite : sprite name in ItemIconAtlas (defaults to the item name).
    tint   : NGUI tint as "R,G,B" (e.g. "160,160,255") or "" if none.
    type_icon : overlay sprite (CustomIconTint's challenge marker etc.) or "".
    """
    sprite: str
    tint: str = ""
    type_icon: str = ""


def _hex_to_rgb(hexstr: str) -> str:
    s = hexstr.strip().lstrip("#")
    if len(s) != 6:
        return ""
    try:
        r = int(s[0:2], 16); g = int(s[2:4], 16); b = int(s[4:6], 16)
    except ValueError:
        return ""
    return f"{r},{g},{b}"


def parse_items(path: Path, tag: str = "item") -> dict[str, ItemIcon]:
    """Read every ``<item name="...">`` (or ``<block>`` when ``tag="block"``)
    in the file and capture icon-related properties: CustomIcon,
    CustomIconTint, ItemTypeIcon. Inheritance via ``Extends`` IS followed and
    ``param1="prop1,prop2"`` lists EXCLUDED properties (so a child won't
    inherit those from its parent)."""
    out: dict[str, ItemIcon] = {}
    raw: dict[str, dict[str, str]] = {}
    try:
        tree = ET.parse(path)
    except (FileNotFoundError, ET.ParseError):
        return out
    root = tree.getroot()
    for it in root.iter(tag):
        nm = it.get("name", "")
        if not nm:
            continue
        d: dict[str, str] = {}
        ext = it.get("Extends") or ""
        ext_excl: list[str] = []  # properties listed in param1 are EXCLUDED from inheritance
        for prop in it.findall("property"):
            pname = prop.get("name", "")
            pval = prop.get("value", "")
            if pname == "Extends":
                ext = pval
                p1 = prop.get("param1", "")
                if p1:
                    ext_excl = [s.strip() for s in p1.split(",") if s.strip()]
            elif pname in ("CustomIcon", "CustomIconTint", "ItemTypeIcon"):
                d[pname] = pval
        if ext:
            d["__ext__"] = ext
        if ext_excl:
            d["__excl__"] = ",".join(ext_excl)
        raw[nm] = d

    def resolve(nm: str, depth: int = 0) -> dict[str, str]:
        if depth > 8 or nm not in raw:
            return {}
        own = raw[nm]
        parent_name = own.get("__ext__", "")
        excl = set((own.get("__excl__") or "").split(",")) if own.get("__excl__") else set()
        merged: dict[str, str] = {}
        if parent_name:
            inherited = resolve(parent_name, depth + 1)
            for k, v in inherited.items():
                if k in excl:
                    continue
                merged[k] = v
        for k, v in own.items():
            if k not in ("__ext__", "__excl__"):
                merged[k] = v
        return merged

    BUNDLE_OVERLAY = "ui_game_symbol_treasure"  # convention used by hand-made book
    for nm in raw:
        m = resolve(nm)
        sprite = m.get("CustomIcon", "") or nm
        tint = m.get("CustomIconTint", "")
        if tint:
            tint = _hex_to_rgb(tint) or tint
        type_icon_name = m.get("ItemTypeIcon", "")
        type_icon_sprite = ""
        if type_icon_name:
            # Hand-made book uses "treasure" sprite for bundle; everything else
            # follows the standard ui_game_symbol_<value> naming convention.
            if type_icon_name == "bundle":
                type_icon_sprite = BUNDLE_OVERLAY
            else:
                type_icon_sprite = f"ui_game_symbol_{type_icon_name}"
        out[nm] = ItemIcon(sprite=sprite, tint=tint, type_icon=type_icon_sprite)
    return out


# Global cache populated in main()
ITEM_ICONS: dict[str, ItemIcon] = {}


def resolve_item_icon(item: str) -> ItemIcon:
    return ITEM_ICONS.get(item, ItemIcon(sprite=item))


def parse_progression(path: Path) -> list[Skill]:
    tree = ET.parse(path)
    root = tree.getroot()
    skills: list[Skill] = []
    for cs in root.iter("crafting_skill"):
        name = cs.get("name", "")
        if not name.startswith("crafting"):
            continue
        sk = Skill(
            name=name,
            max_level=int(cs.get("max_level", "100")),
            icon=cs.get("icon", "ui_game_symbol_book"),
        )
        for de in cs.findall("display_entry"):
            has_q = de.get("has_quality", "true").lower() != "false"
            unlock = [int(x.strip()) for x in de.get("unlock_level", "").split(",") if x.strip()]
            # Per-level item lists. unlock_entry's unlock_tier="N" is 1-indexed
            # into the parent unlock_level list. A single unlock_entry with
            # multiple items applies them all to that level.
            level_items: list[list[str]] = [[] for _ in unlock]
            for u in de.findall("unlock_entry"):
                tier_attr = u.get("unlock_tier") or u.get("level")
                items_attr = u.get("item", "")
                items = [s.strip() for s in items_attr.split(",") if s.strip()]
                if tier_attr and tier_attr.strip().isdigit():
                    tidx = int(tier_attr) - 1
                    if 0 <= tidx < len(level_items):
                        level_items[tidx].extend(items)
                        continue
                # No usable tier: spread across all levels equally (tiered rows)
                for lst in level_items:
                    lst.extend(items)
            # Flat fallback list (used by zoomed view & tiered single-icon column)
            child_items: list[str] = []
            for lst in level_items:
                for it in lst:
                    if it not in child_items:
                        child_items.append(it)
            if child_items:
                icons = child_items
            elif de.get("item"):
                icons = [s.strip() for s in de.get("item", "").split(",") if s.strip()]
            elif de.get("icon"):
                icons = [s.strip() for s in de.get("icon", "").split(",") if s.strip()]
            else:
                icons = []
            # If no unlock_entry children populated level_items, fall back to
            # spreading the icon/item attribute list across each level slot
            # (one item per level, in order).
            if not any(level_items) and icons:
                if len(icons) == len(unlock):
                    level_items = [[ic] for ic in icons]
                else:
                    # Tiered single item: same set at every level
                    level_items = [list(icons) for _ in unlock]
            names_attr = de.get("name_key") or ""
            names = [s.strip() for s in names_attr.split(",") if s.strip()]
            if not names:
                names = list(icons)
            # Per-level name keys: align positionally with level_items where
            # possible, else reuse the global names list.
            level_names: list[list[str]] = []
            if len(names) == len(unlock):
                # icon-list-style: one name per level
                level_names = [[names[i]] * max(1, len(level_items[i])) for i in range(len(unlock))]
            else:
                level_names = [list(names) for _ in unlock]
            sk.display.append(DisplayRow(
                items=icons,
                unlock_levels=unlock,
                name_keys=names,
                has_quality=has_q,
                level_items=level_items,
                level_names=level_names,
            ))
        # Note: do NOT merge armor T2/T3/T4 rows even though they share
        # unlock_levels â€” the user wants them as 4 distinct categories
        # (Primitive/Light/Medium/Heavy), each with its own row icon.
        # Special case: replace craftingArmor's display with exactly TWO
        # containers: Primitive (1 outfit icon) and a merged Light/Med/Heavy
        # container holding three representative outfit icons.
        if sk.name == "craftingArmor":
            light = ["armorAthleticOutfit", "armorEnforcerOutfit", "armorLumberjackOutfit",
                     "armorPreacherOutfit", "armorRogueOutfit"]
            medium = ["armorAssassinOutfit", "armorBikerOutfit", "armorCommandoOutfit",
                      "armorFarmerOutfit", "armorRangerOutfit", "armorScavengerOutfit"]
            heavy = ["armorMinerOutfit", "armorNerdOutfit", "armorNomadOutfit",
                     "armorRaiderOutfit"]
            sk.display = [
                DisplayRow(
                    items=["armorPrimitiveOutfit"],
                    unlock_levels=[1, 2, 4, 6, 8, 10],
                    name_keys=["armorT1"],
                    has_quality=True,
                    level_items=[["armorPrimitiveOutfit"]] * 6,
                    level_names=[["armorT1"]] * 6,
                ),
                DisplayRow(
                    items=["armorAthleticOutfit", "armorAssassinOutfit", "armorMinerOutfit"],
                    unlock_levels=[11, 20, 40, 60, 80, 100],
                    name_keys=["agfArmorLight", "agfArmorMedium", "agfArmorHeavy"],
                    has_quality=True,
                    level_items=[["armorAthleticOutfit", "armorAssassinOutfit", "armorMinerOutfit"]] * 6,
                    level_names=[["agfArmorLight", "agfArmorMedium", "agfArmorHeavy"]] * 6,
                    icon_rows=[light, medium, heavy],
                    icon_row_names=[light, medium, heavy],
                ),
            ]
        skills.append(sk)
    return skills

# ---------------------------------------------------------------------------
# XML emission helpers
# ---------------------------------------------------------------------------

class W:
    """Tiny indent-aware XML writer for hand-shaped xui markup."""

    def __init__(self) -> None:
        self.lines: list[str] = []
        self.depth = 0

    def open(self, tag: str, **attrs) -> None:
        self.lines.append(self._indent() + self._tag(tag, attrs, self_close=False))
        self.depth += 1

    def close(self, tag: str) -> None:
        self.depth -= 1
        self.lines.append(self._indent() + f"</{tag}>")

    def leaf(self, tag: str, **attrs) -> None:
        self.lines.append(self._indent() + self._tag(tag, attrs, self_close=True))

    def comment(self, text: str) -> None:
        self.lines.append(self._indent() + f"<!--{text}-->")

    def raw(self, text: str) -> None:
        self.lines.append(self._indent() + text)

    def _indent(self) -> str:
        return "    " * self.depth

    def _tag(self, tag: str, attrs: dict, self_close: bool) -> str:
        parts = [tag]
        for k, v in attrs.items():
            k = k.rstrip("_").replace("__", ":")  # allow class_ etc
            parts.append(f'{k}="{v}"')
        body = " ".join(parts)
        return f"<{body}/>" if self_close else f"<{body}>"

    def render(self) -> str:
        return "\n".join(self.lines) + "\n"

# ---------------------------------------------------------------------------
# Layout primitives
# ---------------------------------------------------------------------------

def emit_qsection_tier6(w: W, skill: str, levels: list[int], items: list[str],
                        names: list[str] | None = None,
                        icon_rows: list[list[str]] | None = None,
                        icon_row_names: list[list[str]] | None = None) -> None:
    """One QSection (300Ã—100 by default) with 6 quality chips, 6-step gradient,
    and icons centered.

    If ``icon_rows`` is provided, the section becomes taller (50px per extra
    icon row) and icons are emitted left-aligned at 50-stride per row.
    """
    if names is None:
        names = list(items)
    has_multi = bool(icon_rows)
    n_icon_rows = len(icon_rows) if has_multi else 1
    sec_h = 100 if not has_multi else 50 + 50 * n_icon_rows
    w.open("entry", name="QSection")
    w.comment("Background of Section")
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=300, height=sec_h, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=300, height=sec_h, color="[black]",
           type="sliced", fillcenter="false")
    w.comment("If completed (cumulative gradient)")
    for i, lvl in enumerate(levels):
        width = 50 * (i + 1)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               globalopacity="false", pos="0,0", width=width, height=sec_h,
               fillcenter="true", type="filled",
               fill=f"{{cvar({skill}Check{lvl})}}")
    for i, lvl in enumerate(levels):
        x = 50 * i
        w.comment(f"Q{i+1}")
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"{x},0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"{x},0", width=50, height=30, color=QCOLORS[i], type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"{x+25},-15", pivot="center",
               width=50, height=30,
               justify="center", text=str(lvl), color="[black]", font_size=26)
    w.comment("Icon(s)")
    if has_multi:
        # Multi-row icon layout: each row left-aligned at 50-stride, packed.
        for r, row_items in enumerate(icon_rows):
            row_names = icon_row_names[r] if icon_row_names and r < len(icon_row_names) else list(row_items)
            iy = -(40 + r * 50)
            for i, it in enumerate(row_items):
                ix = i * 50  # leave 1px slop in hand-coded; close enough at 50
                tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (row_names[i] if i < len(row_names) and row_names[i] else it))
                ic = resolve_item_icon(it)
                w.leaf("sprite", name="itemIcon", depth=3, pos=f"{ix},{iy}", size="50,50",
                       foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                       color=ic.tint, tooltip_key=tk)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{ix},{iy}", width=20, height=20,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
    else:
        m = len(items) if items else 0
        if m > 0:
            # Pack icons as a tight group, centered horizontally in the 300-wide cell.
            icon_size = 50
            gap = 10
            group_w = m * icon_size + (m - 1) * gap
            start_x = (300 - group_w) // 2
            for i, it in enumerate(items):
                cx = start_x + i * (icon_size + gap)
                tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
                ic = resolve_item_icon(it)
                w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                       foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                       color=ic.tint, tooltip_key=tk)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{cx},-40", width=20, height=20,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("entry")


def emit_qsection_nup(w: W, skill: str, levels: list[int], items: list[str], names: list[str],
                      cell_width: int = 300,
                      level_items: list[list[str]] | None = None,
                      level_names: list[list[str]] | None = None) -> None:
    """N-up split (non-tiered) section.  Sub-cells share equal width with 2px gutter.

    If ``level_items``/``level_names`` are provided (list-per-slot), each
    sub-cell shows its own items centered horizontally inside the sub-cell;
    otherwise the flat ``items`` list is centered across the whole cell.
    """
    n = len(levels)
    if n <= 0:
        return
    gutter = 2
    sub_w = (cell_width - gutter * (n - 1)) // n
    used_w = sub_w * n + gutter * (n - 1)
    start_x = max(0, (cell_width - used_w) // 2)
    w.open("entry", name="QSection")
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=cell_width, height=100, color="[darkGrey]", type="sliced", fillcenter="true")
    for i, lvl in enumerate(levels):
        x = start_x + i * (sub_w + gutter)
        w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
               foregroundlayer="true", pos=f"{x},0", width=sub_w, height=100,
               color="[black]", type="sliced", fillcenter="false")
    for i, lvl in enumerate(levels):
        x = start_x + i * (sub_w + gutter)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos=f"{x},0", width=sub_w, height=100, fillcenter="true",
               type="filled", fill=f"{{cvar({skill}Check{lvl})}}")
    for i, lvl in enumerate(levels):
        x = start_x + i * (sub_w + gutter)
        # Chip is anchored to the top-left of the sub-cell.
        chip_x = x
        chip_color = QCOLORS[0]
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"{chip_x},0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"{chip_x},0", width=50, height=30, color=chip_color, type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"{chip_x+25},-15", pivot="center",
               width=50, height=30,
               justify="center", text=str(lvl), color="[black]", font_size=26)
    # Icon placement
    icon_size = 50
    gap = 10
    if level_items:
        # Per-sub-cell: center each slot's icons inside its own sub-cell.
        for i, lvl in enumerate(levels):
            slot_items = level_items[i] if i < len(level_items) else []
            slot_names = level_names[i] if level_names and i < len(level_names) else list(slot_items)
            m = len(slot_items)
            if m == 0:
                continue
            x = start_x + i * (sub_w + gutter)
            # Shrink the inter-icon gap if the group would overflow the sub-cell.
            slot_gap = gap
            if m > 1:
                max_gap = (sub_w - 4 - m * icon_size) // (m - 1)
                if max_gap < gap:
                    slot_gap = max(0, max_gap)
            group_w = m * icon_size + (m - 1) * slot_gap
            icon_start_x = x + (sub_w - group_w) // 2
            for j, it in enumerate(slot_items):
                cx = icon_start_x + j * (icon_size + slot_gap)
                tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (slot_names[j] if j < len(slot_names) and slot_names[j] else it))
                ic = resolve_item_icon(it)
                w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                       foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                       color=ic.tint, tooltip_key=tk)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{cx},-40", width=20, height=20,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
    elif items:
        m = len(items)
        group_w = m * icon_size + (m - 1) * gap
        start_x = (cell_width - group_w) // 2
        for i, it in enumerate(items):
            cx = start_x + i * (icon_size + gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},-40", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("entry")


def emit_qsection_single(w: W, skill: str, level: int, items: list[str],
                         names: list[str] | None = None,
                         cell_width: int = 300) -> None:
    """A single-slot non-tiered section: 1 chip (top-left) + icons (centered).

    Used by the auto-layout zoomed-view grid where each unlock threshold gets
    its own ``cell_width \u00d7 100`` container.
    """
    if names is None:
        names = list(items)
    w.open("entry", name="QSection")
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=cell_width, height=100, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=cell_width, height=100,
           color="[black]", type="sliced", fillcenter="false")
    w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
           pos="0,0", width=cell_width, height=100, fillcenter="true",
           type="filled", fill=f"{{cvar({skill}Check{level})}}")
    # Chip top-left, Q1 brown
    w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color=QCOLORS[0], type="sliced", fillcenter="true")
    w.leaf("label", depth=3, name="checkunlock", pos="25,-15", pivot="center",
           width=50, height=30, justify="center", text=str(level),
           color="[black]", font_size=26)
    # Icons centered
    m = len(items)
    if m > 0:
        icon_size = 50
        gap = 10
        slot_gap = gap
        if m > 1:
            max_gap = (cell_width - 4 - m * icon_size) // (m - 1)
            if max_gap < gap:
                slot_gap = max(0, max_gap)
        group_w = m * icon_size + (m - 1) * slot_gap
        start_x = (cell_width - group_w) // 2
        for j, it in enumerate(items):
            cx = start_x + j * (icon_size + slot_gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},-40", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("entry")


def _emit_single_section_rect(w: W, skill: str, level: int,
                              items: list[str], names: list[str],
                              x: int, y: int,
                              width: int = 300, height: int = 100,
                              row1_count: int | None = None,
                              row1_col_offset: int = 0,
                              force_cols: int | None = None,
                              row2_extra_drop: int = 0) -> None:
    """Render one non-tiered section as a positioned rect.

    When ``row1_count`` is provided, icons are packed into two rows inside the
    icon area to the right of the 50px chip.
    """
    w.open("rect", name=f"QSection{level}", pos=f"{x},{y}", width=width, height=height)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=width, height=height, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=width, height=height,
           color="[black]", type="sliced", fillcenter="false")
    w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
           pos="0,0", width=width, height=height, fillcenter="true",
           type="filled", fill=f"{{cvar({skill}Check{level})}}")
    w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color=QCOLORS[0], type="sliced", fillcenter="true")
    w.leaf("label", depth=3, name="checkunlock", pos="25,-15", pivot="center",
           width=50, height=30, justify="center", text=str(level),
           color="[black]", font_size=26)

    if not items:
        w.close("rect")
        return

    if row1_count is None:
        # Default single-row center for rect-based sections.
        icon_size = 50
        gap = 10
        m = len(items)
        group_w = m * icon_size + (m - 1) * gap
        start_x = (width - group_w) // 2
        y_pos = -40 if height <= 100 else -52
        for j, it in enumerate(items):
            cx = start_x + j * (icon_size + gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{y_pos}",
                   size="50,50", foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{y_pos}", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    else:
        split = max(0, min(len(items), row1_count))
        top_items = items[:split]
        bot_items = items[split:]
        top_names = names[:split]
        bot_names = names[split:]
        icon_size = 50
        type_sz = 20
        preferred_gap = 8
        row_gap = 8
        ncols = max(len(bot_items), len(top_items) + max(0, row1_col_offset))
        if force_cols is not None:
            ncols = max(ncols, force_cols)
        if ncols <= 0:
            w.close("rect")
            return
        area_left = 55
        area_right_pad = 5
        area_w = max(10, width - area_left - area_right_pad)
        if ncols <= 1:
            col_gap = 0
        else:
            col_gap = max(0, min(preferred_gap, (area_w - ncols * icon_size) // (ncols - 1)))
        grid_w = ncols * icon_size + (ncols - 1) * col_gap
        start_x = area_left + (area_w - grid_w) // 2
        two_row_h = icon_size * 2 + row_gap
        top_pad = max(0, (height - two_row_h) // 2)
        y_top = -top_pad
        y_bot = y_top - icon_size - row_gap - row2_extra_drop

        for j, it in enumerate(top_items):
            col = j + max(0, row1_col_offset)
            cx = start_x + col * (icon_size + col_gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (top_names[j] if j < len(top_names) and top_names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{y_top}",
                   size="50,50", foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{y_top}", width=type_sz, height=type_sz,
                       sprite=ic.type_icon, foregroundlayer="true", color="")

        for j, it in enumerate(bot_items):
            cx = start_x + j * (icon_size + col_gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (bot_names[j] if j < len(bot_names) and bot_names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{y_bot}",
                   size="50,50", foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{y_bot}", width=type_sz, height=type_sz,
                       sprite=ic.type_icon, foregroundlayer="true", color="")

    w.close("rect")


def _emit_tier6_section_rect(w: W, skill: str, levels: list[int],
                             items: list[str], names: list[str],
                             x: int, y: int,
                             width: int = 300, height: int = 100) -> None:
    """Render a 6-quality tiered section as a positioned rect."""
    w.open("rect", name="QSectionTier6", pos=f"{x},{y}", width=width, height=height)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=width, height=height, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=width, height=height,
           color="[black]", type="sliced", fillcenter="false")
    for i, lvl in enumerate(levels[:6]):
        fill_w = 50 * (i + 1)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               globalopacity="false", pos="0,0", width=fill_w, height=height,
               fillcenter="true", type="filled", fill=f"{{cvar({skill}Check{lvl})}}")
    for i, lvl in enumerate(levels[:6]):
        cx = 50 * i
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"{cx},0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"{cx},0", width=50, height=30, color=QCOLORS[i], type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"{cx+25},-15", pivot="center",
               width=50, height=30, justify="center", text=str(lvl), color="[black]", font_size=26)
    if items:
        m = len(items)
        icon_size = 50
        gap = 10
        group_w = m * icon_size + (m - 1) * gap
        start_x = (width - group_w) // 2
        for j, it in enumerate(items):
            cx = start_x + j * (icon_size + gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},-40", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")


def _emit_tier6_section_vertical_rect(w: W, skill: str, levels: list[int],
                                      items: list[str], names: list[str],
                                      x: int, y: int,
                                      width: int = 180, height: int = 375) -> None:
    """Render a vertical/transposed 6-quality section (ALL-page style, larger)."""
    w.open("rect", name="QSectionTier6Vertical", pos=f"{x},{y}", width=width, height=height)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=width, height=height, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=width, height=height,
           color="[black]", type="sliced", fillcenter="false")

    seg_h = max(1, height // 6)
    for i, lvl in enumerate(levels[:6]):
        fill_h = min(height, seg_h * (i + 1))
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               globalopacity="false", pos="0,0", width=width, height=fill_h,
               fillcenter="true", type="filled", fill=f"{{cvar({skill}Check{lvl})}}")

    chip_h = min(30, max(20, seg_h - 2))
    # Place chips so the final (100) chip bottom edge meets the section bottom.
    span = max(1, height - chip_h)
    for i, lvl in enumerate(levels[:6]):
        cy = -int(round(i * span / 5.0))
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"0,{cy}", width=50, height=chip_h, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"0,{cy}", width=50, height=chip_h, color=QCOLORS[i], type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"25,{cy - chip_h // 2}", pivot="center",
               width=50, height=chip_h, justify="center", text=str(lvl), color="[black]", font_size=26)

    if items:
        m = len(items)
        icon_size = 50
        gap = 10
        right_x = 50
        right_w = max(icon_size, width - right_x)
        content_h = m * icon_size + (m - 1) * gap
        start_y = -((height - content_h) // 2)
        for j, it in enumerate(items):
            cy = start_y - j * (icon_size + gap)
            cx = right_x + max(0, (right_w - icon_size) // 2)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{cy}", size="50,50",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{cy}", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")

# ---------------------------------------------------------------------------
# Magazine top-strip (shared across all 23 zoomed views)
# ---------------------------------------------------------------------------

def emit_magazine_strip(w: W, skills: list[Skill]) -> None:
    """The 23-icon row across the top of every zoomed<Cat> rect."""
    w.open("grid", name="tabButtons", pos="5,-10", depth=3, rows=1, cols=23,
           cell_width=60, cell_height=100, repeat_content="false",
           arrangement="horizontal", controller="MapStats")
    by_name = {s.name: s for s in skills}
    for tab_skill in TAB_ORDER:
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        w.open("entry")
        w.open("rect", depth=10, pos="5,0", width=50, height=100)
        w.leaf("sprite", depth=10, name="itemIcon", pos="2,-9", width=50, height=50,
               size="46,46", foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=sk.book_sprite, color="")
        w.leaf("label", depth=10, name="check", pos="0,-60", width=50, height=16,
               justify="center", text=f"[fc8c03]{{cvar({sk.name}Check)}}[-]", font_size=22)
        w.leaf("label", depth=10, name="check", pos="0,-77", width=50, height=16,
               justify="center", text=f"/{sk.max_level}", font_size=22)
        w.leaf("filledsprite", depth=1, name="yesUnlocked", color="10,255,10,65",
               pos="2,-1", width=46, height=98, fillcenter="true", type="filled",
               fill=f"{{cvar({sk.name}Check{sk.max_level})}}")
        w.close("rect")
        w.close("entry")
    w.close("grid")

# ---------------------------------------------------------------------------
# Compact strip + zoomed view
# ---------------------------------------------------------------------------

def emit_compact_strip(w: W, sk: Skill, x: int) -> None:
    """The narrow 50px column shown in the All-Checklists view.
    The Magazine header (icon + level/max + greenfill) lives in the
    tabsHeader buttons themselves, so the strip starts directly with
    Section1 just under the buttons."""
    short_n = sk.short_name
    # Pre-compute total column height as the sum of section heights.
    total_h = 0
    food_compact = (sk.name == "craftingFood")
    for row in sk.display:
        if row.has_quality and len(row.unlock_levels) == 6:
            n_icons = max(1, len(row.items))
            total_h += max(120, 10 + 25 * n_icons + 5)
        else:
            for i in range(len(row.unlock_levels)):
                items_here = row.level_items[i] if i < len(row.level_items) else row.items
                n = max(1, len(items_here))
                if food_compact and n <= 2:
                    total_h += 30  # compact side-by-side row (Food only)
                else:
                    total_h += _nontiered_height(n)
    # y=-110 places the strip directly below the 100px tab buttons (which
    # sit at y=-10..-110). Sections start at y=0 within this rect.
    w.open("rect", name=f"checklist{short_n}", depth=3, pos=f"{x},-110", width=50, height=total_h, controller="MapStats")
    # Section1..N â€” one per display row, with cumulative y-offset so taller
    # sections (multi-icon rows) push later sections down.
    y_cursor = 0
    section_idx = 0
    for row in sk.display:
        if row.has_quality and len(row.unlock_levels) == 6:
            section_idx += 1
            sec_h = _emit_tiered_section(w, sk, row, section_idx, y_cursor)
            y_cursor -= sec_h
        else:
            for li, lvl in enumerate(row.unlock_levels):
                section_idx += 1
                sec_h = _emit_nontiered_section(w, sk, row, li, lvl, section_idx, y_cursor)
                y_cursor -= sec_h
    w.close("rect")


def _icon_slots(n: int) -> list[tuple[int, int]]:
    """Return (x,y) positions for `n` icons in a non-tiered Section.
    Special-cases:
      n=1 -> single icon centered with shrunk container, at (26,-5)
      n=2 -> vertical stack at (26,-5), (26,-25) (single column, tight)
      n>=3 -> alternating left/right grid pattern matching the hand-built book.
    """
    if n <= 0:
        return []
    if n == 1:
        return [(26, -5)]
    if n == 2:
        return [(26, -5), (26, -25)]
    slots: list[tuple[int, int]] = [(26, -10)]
    for r in range(1, 16):
        y = -10 - r * 25
        slots.append((2, y))
        slots.append((26, y))
        if len(slots) >= n:
            break
    return slots[:n]


def _nontiered_height(n: int) -> int:
    """Container height (px) for a non-tiered Section with n icons.
    Includes ~5px of bottom padding so icons don't kiss the bottom border
    (matches the tiered-container look).
    """
    if n <= 1:
        return 30
    if n == 2:
        return 51
    if n == 3:
        return 61
    if n <= 5:
        return 86
    if n <= 7:
        return 111
    if n <= 9:
        return 136
    rows = (n + 2) // 2
    return 10 + rows * 25 + 6


def _emit_icon_grid(w: W, items: list[str], names: list[str]) -> int:
    """Emit icons inside a Section rect. Returns the grid's content height.
    See `_icon_slots` for the placement pattern."""
    n = len(items)
    slots = _icon_slots(n)
    for i in range(n):
        it = items[i]
        x, y = slots[i]
        # Prefer the entity name as tooltip_key (items.xml/blocks.xml entries
        # are also loc keys in vanilla). Fall back to name_key only when the
        # entity isn't in our parsed dicts. TOOLTIP_OVERRIDES wins.
        tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
        ic = resolve_item_icon(it)
        w.leaf("sprite", name="itemIcon", depth=3,
               pos=f"{x},{y}", size="21,21",
               foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
        if ic.type_icon:
            w.leaf("sprite", name="itemtypeicon", depth=8,
                   pos=f"{x},{y}", width=10, height=10,
                   sprite=ic.type_icon, foregroundlayer="true", color="")
    # Content height: handled by caller via _nontiered_height
    return _nontiered_height(n)


def _emit_tiered_section(w: W, sk: Skill, row: 'DisplayRow', idx: int, y: int) -> int:
    """Emit a 6-quality container at y offset `y`. Returns its height."""
    items = list(row.items)
    names = list(row.name_keys)
    n_icons = max(1, len(items))
    sec_h = max(120, 10 + 25 * n_icons + 5)
    w.open("rect", name=f"Section{idx}", pos=f"0,{y}",
           width=50, height=sec_h)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
           type="sliced", fillcenter="false")
    for i, lvl in enumerate(row.unlock_levels[:6]):
        h = 20 * (i + 1)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos="0,0", width=50, height=h, fillcenter="true", type="filled",
               fill=f"{{cvar({sk.name}Check{lvl})}}")
    for i, lvl in enumerate(row.unlock_levels[:6]):
        by = -2 - i * 20
        iy = -3 - i * 20
        color = QCOLORS[i]
        w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"2,{by}", width=21, height=18, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"3,{iy}", width=19, height=16, color=color,
               type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"0,{iy}",
               width=25, height=16, justify="center", text=str(lvl),
               color="[black]", font_size=16)
    # Tiered icon column: vertical stack starting at y=-10
    for i, it in enumerate(items):
        iy = -10 - i * 25
        tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
        ic = resolve_item_icon(it)
        w.leaf("sprite", name="itemIcon", depth=3,
               pos=f"26,{iy}", size="21,21",
               foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
        if ic.type_icon:
            w.leaf("sprite", name="itemtypeicon", depth=8,
                   pos=f"26,{iy}", width=10, height=10,
                   sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")
    return sec_h


def _walk_nontiered(row: 'DisplayRow'):
    """Walk a non-tiered row's unlock_levels and yield either:
      ("merged", [i, i+1, ...])  - run of >=2 consecutive single-icon levels
      ("single", i)              - one level (multi-icon, or a lone single)
    """
    n = len(row.unlock_levels)
    i = 0
    while i < n:
        items_here = row.level_items[i] if i < len(row.level_items) else row.items
        if len(items_here) == 1:
            j = i
            while j < n:
                items_j = row.level_items[j] if j < len(row.level_items) else row.items
                if len(items_j) != 1:
                    break
                j += 1
            run = list(range(i, j))
            if len(run) >= 2:
                yield ("merged", run)
            else:
                yield ("single", i)
            i = j
        else:
            yield ("single", i)
            i += 1


def _merged_singles_height(n: int) -> int:
    """Height of a merged-singles container with n tiers (1 icon per tier)."""
    return 22 * n + 5


def _emit_merged_singles_section(w: W, sk: Skill, row: 'DisplayRow',
                                 indices: list[int], idx: int, y: int) -> int:
    """Emit ONE container holding N consecutive single-icon tiers stacked
    vertically. Each row = (Q1 brown chip, level number, single icon) and has
    its own green-fill driven by ``cvar({sk}Check{lvl})``. Saves height vs
    emitting N separate 30-px containers.
    """
    pitch = 22
    n = len(indices)
    sec_h = _merged_singles_height(n)
    w.open("rect", name=f"Section{idx}", pos=f"0,{y}", width=50, height=sec_h)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
           type="sliced", fillcenter="false")
    # Pass 1: per-row green fills (drawn before chip backgrounds so chips win)
    for slot, li in enumerate(indices):
        lvl = row.unlock_levels[li]
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos=f"0,{-slot*pitch}", width=50, height=pitch, fillcenter="true",
               type="filled", fill=f"{{cvar({sk.name}Check{lvl})}}")
    # Pass 2: chips + icons
    for slot, li in enumerate(indices):
        lvl = row.unlock_levels[li]
        items_here = row.level_items[li] if li < len(row.level_items) else row.items
        names_here = row.level_names[li] if li < len(row.level_names) else row.name_keys
        it = items_here[0] if items_here else ""
        if it:
            tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names_here[0] if names_here and names_here[0] else it))
        else:
            tip = ""
        by = -2 - slot * pitch
        iy = -3 - slot * pitch
        w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"2,{by}", width=21, height=18, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"3,{iy}", width=19, height=16, color=QCOLORS[0],
               type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"0,{iy}",
               width=25, height=16, justify="center", text=str(lvl),
               color="[black]", font_size=16)
        if it:
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3,
                   pos=f"26,{iy}", size="21,21",
                   foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"26,{iy}", width=10, height=10,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")
    return sec_h


def _emit_nontiered_section(w: W, sk: Skill, row: 'DisplayRow',
                            li: int, lvl: int, idx: int, y: int) -> int:
    """Emit a single-Q-chip container holding row's items unlocked at this
    specific level (row.level_items[li]). Container height is sized to fit
    the icons. Returns the height used."""
    items = list(row.level_items[li]) if li < len(row.level_items) else list(row.items)
    if not items:
        items = list(row.items)
    names = list(row.level_names[li]) if li < len(row.level_names) else list(row.name_keys)
    n = max(1, len(items))
    # Compact layout for Food: 22-px tall row, chip + up to 2 icons side-by-side
    # at 14x14. Keeps one container per unlock level so organization is intact.
    food_compact = (sk.name == "craftingFood" and n <= 2)
    if food_compact:
        sec_h = 30
        w.open("rect", name=f"Section{idx}", pos=f"0,{y}", width=50, height=sec_h)
        w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
               width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
        w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
               foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos="0,0", width=50, height=sec_h, fillcenter="true", type="filled",
               fill=f"{{cvar({sk.name}Check{lvl})}}")
        # Chip pinned to top-left (consistent with all other sections)
        w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
               pos="2,-2", width=21, height=18, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos="3,-3", width=19, height=16, color=QCOLORS[0],
               type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos="0,-3",
               width=25, height=16, justify="center", text=str(lvl),
               color="[black]", font_size=16)
        # Right cell = x[24..50] = 26 wide, full 30 px tall.
        # n=1: full-size 21x21 icon centered in right cell.
        # n=2: vertical stack of two 14x14 icons, centered horizontally and
        # vertically inside the right cell (no border overlap).
        if n == 1:
            it = items[0]
            tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[0] if names and names[0] else it))
            ic = resolve_item_icon(it)
            # Match _icon_slots(1) exactly: pos=(26,-5), size=21x21
            ix, iy = 26, -5
            ov = FOOD_ICON_OVERRIDES.get(ic.sprite) or FOOD_ICON_OVERRIDES.get(it) or {}
            ix += ov.get("dx", 0); iy += ov.get("dy", 0)
            sz = ov.get("size", 21)
            w.leaf("sprite", name="itemIcon", depth=3,
                   pos=f"{ix},{iy}", size=f"{sz},{sz}",
                   foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{ix},{iy}", width=10, height=10,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
        else:
            # n=2 vertical stack inset from borders. Sprites have variable
            # built-in padding so use a conservative 11x11 with 4px top/bottom.
            # Container 30 tall: top icon y=-4..-15, bottom icon y=-15..-26.
            # Horizontally: 11 in 26 -> x = 24 + (26-11)/2 = 31 (rounded down).
            cascade = [(31, -4), (31, -15)]
            sz_default = 11
            for i in range(min(n, 2)):
                it = items[i]
                tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
                ic = resolve_item_icon(it)
                ix, iy = cascade[i]
                ov = FOOD_ICON_OVERRIDES.get(ic.sprite) or FOOD_ICON_OVERRIDES.get(it) or {}
                sz = ov.get("size", sz_default)
                ix += ov.get("dx", 0); iy += ov.get("dy", 0)
                w.leaf("sprite", name="itemIcon", depth=3,
                       pos=f"{ix},{iy}", size=f"{sz},{sz}",
                       foregroundlayer="true", atlas="ItemIconAtlas",
                       sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{ix},{iy}", width=8, height=8,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
        w.close("rect")
        return sec_h
    sec_h = _nontiered_height(n)
    w.open("rect", name=f"Section{idx}", pos=f"0,{y}", width=50, height=sec_h)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
           type="sliced", fillcenter="false")
    # Single full-height green fill driven by this level's cvar
    w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
           pos="0,0", width=50, height=sec_h, fillcenter="true", type="filled",
           fill=f"{{cvar({sk.name}Check{lvl})}}")
    # One Q1 brown chip
    w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
           pos="2,-2", width=21, height=18, color="[black]",
           type="sliced", fillcenter="false")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           pos="3,-3", width=19, height=16, color=QCOLORS[0],
           type="sliced", fillcenter="true")
    w.leaf("label", depth=3, name="checkunlock", pos="0,-3",
           width=25, height=16, justify="center", text=str(lvl),
           color="[black]", font_size=16)
    _emit_icon_grid(w, items, names)
    w.close("rect")
    return sec_h


def emit_zoomed_view(w: W, sk: Skill, all_skills: list[Skill]) -> None:
    """The full-width zoomed view shown when a skill tab is selected."""
    short_n = sk.short_name
    medical_slots: list[tuple[int, list[str], list[str]]] = []
    medical_mode = (sk.name == "craftingMedical")
    seeds_slots: list[tuple[int, list[str], list[str]]] = []
    seeds_mode = (sk.name == "craftingSeeds")
    food_slots: list[tuple[int, list[str], list[str]]] = []
    food_mode = (sk.name == "craftingFood")
    workstations_slots: list[tuple[int, list[str], list[str]]] = []
    workstations_mode = (sk.name == "craftingWorkstations")
    electrician_mode = (sk.name == "craftingElectrician")
    electrician_by_level: dict[int, tuple[list[str], list[str]]] = {}
    electrician_tiered: DisplayRow | None = None
    medical_cols = 2
    medical_rows = 4
    medical_cell_w = 150
    seeds_cols = 2
    seeds_rows = 5
    seeds_cell_w = 150
    food_cols = 5
    food_rows = 4
    food_cell_w = 150
    workstations_cols = 4
    workstations_rows = 4
    # Match current per-container width in Forge Ahead's 16/20/26 row.
    workstations_cell_w = 134

    if medical_mode:
        # Medical Journal: flatten to 8 single-slot containers and render as
        # 2 columns x 4 rows with equal width for visual consistency.
        for row in sk.display:
            if row.is_tiered:
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                medical_slots.append((lvl, items, names))
        # Keep exactly 8 visible entries (2x4) in level order.
        medical_slots = medical_slots[:medical_cols * medical_rows]
        cell_width = medical_cell_w
        total_grid_h = medical_rows * 100
        total_grid_w = medical_cols * cell_width
    elif seeds_mode:
        # Southern Farming: flatten to 10 single-slot containers and render as
        # 2 columns x 5 rows. Use wider containers than the old 100px slots.
        for row in sk.display:
            if row.is_tiered:
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                seeds_slots.append((lvl, items, names))
        # Keep exactly 10 visible entries (2x5) in level order.
        seeds_slots = seeds_slots[:seeds_cols * seeds_rows]
        cell_width = seeds_cell_w
        total_grid_h = seeds_rows * 100
        total_grid_w = seeds_cols * cell_width
    elif food_mode:
        # Home Cooking Weekly: flatten to 20 single-slot containers and render
        # as 5 columns x 4 rows. Match Seeds container width (150px).
        for row in sk.display:
            if row.is_tiered:
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                food_slots.append((lvl, items, names))
        food_slots = food_slots[:food_cols * food_rows]
        cell_width = food_cell_w
        total_grid_h = food_rows * 100
        total_grid_w = food_cols * cell_width
    elif workstations_mode:
        # Forge Ahead: flatten to 16 single-slot containers and render as
        # 4 columns x 4 rows. Container width matches the former 3-up row
        # container width used by levels 16/20/26.
        for row in sk.display:
            if row.is_tiered:
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                workstations_slots.append((lvl, items, names))
        workstations_slots = workstations_slots[:workstations_cols * workstations_rows]
        cell_width = workstations_cell_w
        total_grid_h = workstations_rows * 100
        total_grid_w = workstations_cols * cell_width
    elif electrician_mode:
        for row in sk.display:
            if row.is_tiered:
                electrician_tiered = row
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                electrician_by_level[lvl] = (items, names)
        col2_w = 380
        col3_w = 130
        col_gap = 50
        col2_h = 375  # 125 + 125 + 125
        col3_h = 250  # shorter vertical/transposed tiered block
        total_grid_h = max(col2_h, col3_h)
        total_grid_w = col2_w + col_gap + col3_w
        cell_width = col2_w
    else:
        rows_used = min(len(sk.display), 6)
        cell_width = 300  # default; widened automatically when N-up needs more
        # Find widest row -> cell width
        for row in sk.display:
            if not row.is_tiered:
                n = len(row.unlock_levels)
                if n > 0:
                    # Keep 3-way splits aligned with the 300px tiered width.
                    if n <= 3:
                        needed = 300
                    else:
                        needed = max(300, n * 100 + (n - 1) * 2)
                    if needed > cell_width:
                        cell_width = needed
        # Compute total vertical height of the main grid (some rows may be taller
        # than 100px when they use a multi-row icon layout, e.g. Armored Up).
        def _row_h(r):
            return 50 + 50 * len(r.icon_rows) if r.icon_rows else 100
        total_grid_h = sum(_row_h(r) for r in sk.display[:6])
        total_grid_w = cell_width
    # Vertical centering: usable area is y=-110..-720 (height 610, center -415).
    mag_label_y = -225
    title_pos_y = -255
    main_pos_y = -415 + total_grid_h // 2
    # Layout: [magazine 250] [gap 50] [main grid]
    visual_w = 250 + 50 + total_grid_w
    title_pos_x = (WIN_WIDTH - visual_w) // 2
    main_pos_x = title_pos_x + 300

    w.open("rect", name=f"zoomed{short_n}", controller="TabSelectorTab", tab_key="")
    # Magazine name label (sits above the cover sprite). text_key resolves to
    # the localized magazine name from items.xml (e.g. "Tech Planet").
    mag_item = MAG_ITEM.get(sk.name, "")
    if mag_item:
        w.leaf("label", name=f"checklist{short_n}MagName", depth=3,
               pos=f"{title_pos_x},{mag_label_y}", width=250, height=30,
               justify="center", font_size=26, text_key=mag_item)
    # Title block
    w.open("grid", name=f"checklist{short_n}Title", depth=3, rows=2, cols=1,
           pos=f"{title_pos_x},{title_pos_y}", cell_width=250, cell_height=250,
           repeat_content="false", arrangement="horizontal", controller="MapStats")
    w.open("entry", name="Magazine")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           width=250, height=350, color="[black]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=2, name="backgroundBorder", foregroundlayer="true",
           sprite="menu_empty3px", width=250, height=350, color="[black]",
           type="sliced", fillcenter="false")
    w.leaf("filledsprite", depth=1, name="yesUnlocked", color="0,255,0,200",
           pos="0,0", width=250, height=350, fillcenter="true", type="filled",
           fill=f"{{cvar({sk.name}Check{sk.max_level})}}")
    w.leaf("sprite", depth=3, name="itemIcon", pos="2,-2", width=250, height=250,
           size="246,246", foregroundlayer="true", atlas="ItemIconAtlas",
           sprite=sk.book_sprite, color="", tooltip_key=f"{sk.name}Name")
    w.close("entry")
    w.open("entry", name="level")
    w.leaf("label", depth=3, name="check", pos="0,-14", width=250, height=100,
           justify="center", text=f"[fc8c03]{{cvar({sk.name}Check)}}[-]", font_size=40,
           tooltip_key=f"{sk.name}Name")
    w.leaf("label", depth=3, name="check", pos="0,-50", width=250, height=100,
           justify="center", text=f"/{sk.max_level}", font_size=40,
           tooltip_key=f"{sk.name}Name")
    w.close("entry")
    w.close("grid")
    # Main checklist grid
    if medical_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=medical_rows, cols=medical_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in medical_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width)
        w.close("grid")
    elif seeds_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=seeds_rows, cols=seeds_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in seeds_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width)
        w.close("grid")
    elif food_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=food_rows, cols=food_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in food_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width)
        w.close("grid")
    elif workstations_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=workstations_rows, cols=workstations_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in workstations_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width)
        w.close("grid")
    elif electrician_mode:
        col2_w = 380
        col2_h = 375
        col3_w = 130
        col3_h = 220
        col_gap = 50
        col3_x = col2_w + col_gap
        col2_top = 0
        # Independent vertical centering: shorter tier block centered against
        # the taller stacked column (25/45/55).
        col3_top = -((col2_h - col3_h) // 2)
        w.open("rect", name=f"checklist{short_n}", depth=3,
               pos=f"{main_pos_x},{main_pos_y}", width=col2_w + col_gap + col3_w,
               height=col2_h, controller="MapStats")
        items25, names25 = electrician_by_level.get(25, ([], []))
        items45, names45 = electrician_by_level.get(45, ([], []))
        items55, names55 = electrician_by_level.get(55, ([], []))
        _emit_single_section_rect(w, sk.name, 25, items25, names25,
                                  x=0, y=col2_top, width=col2_w, height=125,
                                  row1_count=4)
        _emit_single_section_rect(w, sk.name, 45, items45, names45,
                                  x=0, y=col2_top - 125, width=col2_w, height=125,
                                  row1_count=3)
        _emit_single_section_rect(w, sk.name, 55, items55, names55,
                                  x=0, y=col2_top - 250, width=col2_w, height=125,
                                  row1_count=6, force_cols=6, row2_extra_drop=0)
        if electrician_tiered is not None:
            _emit_tier6_section_vertical_rect(
                w, sk.name,
                electrician_tiered.unlock_levels,
                electrician_tiered.items,
                electrician_tiered.name_keys,
                x=col3_x, y=col3_top, width=col3_w, height=col3_h,
            )
        w.close("rect")
    else:
        w.open("grid", name=f"checklist{short_n}", depth=3, rows=6, cols=3,
               pos=f"{main_pos_x},{main_pos_y}", cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="vertical", controller="MapStats")
        for row in sk.display[:6]:
            if row.is_tiered:
                emit_qsection_tier6(w, sk.name, row.unlock_levels, row.items, row.name_keys,
                                    icon_rows=row.icon_rows or None,
                                    icon_row_names=row.icon_row_names or None)
            else:
                emit_qsection_nup(w, sk.name, row.unlock_levels, row.items, row.name_keys, cell_width,
                                  level_items=row.level_items or None,
                                  level_names=row.level_names or None)
        w.close("grid")
    w.close("rect")

# ---------------------------------------------------------------------------
# Tabs shell + window
# ---------------------------------------------------------------------------

def emit_craftinglist_tab(w: W, skills: list[Skill]) -> None:
    by_name = {s.name: s for s in skills}
    w.open("rect", name="craftinglistTab", controller="TabSelectorTab", tab_key="agfChecklist")
    # Sub-tab selector with 1 (All) + 23 buttons
    w.open("rect", name="tabs", controller="TabSelector", select_tab_contents_on_open="true")
    # Header. Two stacked elements at the same screen position:
    #   1) tabButtons grid: 24 plain transparent simplebuttons (the click
    #      surfaces; All + 23 skills) using repeat_content="true" template.
    #   2) magazineDecor grid: 23 decorative magazine covers (sprite + level
    #      labels + greenfill) at depth=10 layered on top. NGUI sprites and
    #      labels have no colliders, so clicks pass through to the buttons
    #      below â€” exactly how the hand-written zoomed views work.
    w.open("rect", name="tabsHeader")
    # ---- (1) Click surfaces ----
    w.open("grid", name="tabButtons", pos="-55,-10", depth=3, rows=1, cols=24,
           cell_width=60, cell_height=100, repeat_content="true", arrangement="horizontal")
    w.open("rect", controller="TabSelectorButton")
    w.leaf("simplebutton", name="tabButton", depth=8, pos="5,0",
           width=50, height=100, font_size=20, bordercolor="[black]",
           defaultcolor="[darkestGrey]", selectedsprite="menu_empty",
           selectedcolor="74, 33, 150", foregroundlayer="false",
           caption="{tab_name_localized}")
    w.close("rect")
    w.close("grid")
    # ---- (2) Decorative magazine overlay ----
    # First cell (col 0) is reserved for the All button (already shows
    # "All" caption from the template above), so the magazine grid starts
    # at column 1: pos x = -55 + 60 = 5.
    # depth=10 (above button at depth=8) + foregroundlayer="true" so they
    # render visually on top of the button background. Sprites and labels
    # have NO NGUI collider, so clicks pass through to the simplebutton.
    # This is exactly the pattern the hand-written zoomed views use.
    w.open("grid", name="magazineDecor", pos="5,-10", depth=10, rows=1, cols=23,
           cell_width=60, cell_height=100, repeat_content="false",
           arrangement="horizontal", controller="MapStats")
    for tab_skill in TAB_ORDER:
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        w.open("entry")
        w.open("rect", depth=10, pos="5,0", width=50, height=100)
        # NOTE: No tooltip_key on these sprites/labels. tooltip_key turns a
        # widget into a hover/click target that blocks the simplebutton
        # underneath. The hand-written zoomed views deliberately omit it.
        w.leaf("sprite", depth=10, name="itemIcon", pos="2,-9", width=50, height=50,
               size="46,46", foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=sk.book_sprite, color="")
        w.leaf("label", depth=10, name="check", pos="0,-60", width=50, height=16,
               justify="center", text=f"[fc8c03]{{cvar({sk.name}Check)}}[-]", font_size=22,
               foregroundlayer="true")
        w.leaf("label", depth=10, name="check", pos="0,-77", width=50, height=16,
               justify="center", text=f"/{sk.max_level}", font_size=22,
               foregroundlayer="true")
        w.leaf("filledsprite", depth=9, name="yesUnlocked", color="10,255,10,65",
               pos="2,-1", width=46, height=98, fillcenter="true", type="filled",
               foregroundlayer="true",
               fill=f"{{cvar({sk.name}Check{sk.max_level})}}")
        w.close("rect")
        w.close("entry")
    w.close("grid")
    w.close("rect")
    # Contents
    w.open("rect", name="tabsContents")
    # All view (compact strips)
    w.open("rect", name="allChecklists", controller="TabSelectorTab", tab_key="lblAll")
    for i, tab_skill in enumerate(TAB_ORDER):
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        x = 10 + i * 60
        emit_compact_strip(w, sk, x)
    w.close("rect")
    # Per-skill zoomed views
    for tab_skill in TAB_ORDER:
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        emit_zoomed_view(w, sk, skills)
    w.close("rect")  # tabsContents
    w.close("rect")  # tabs
    w.close("rect")  # craftinglistTab


def emit_window(skills: list[Skill]) -> str:
    w = W()
    w.raw('<?xml version="1.0" encoding="UTF-8"?>')
    w.open("AGF-PurpleBookTest")
    # ---- Button on the paging header (this is what opens the window) ----
    w.comment("The Button to access Schematic Checklist")
    w.open("append", xpath="windows/window[@name='windowPagingHeader']")
    w.leaf("sprite", name="background", sprite="ui_game_filled_circle", depth=2,
           pos="-39,-3", height=36, width=36, color="[lightGrey]", type="sliced")
    w.leaf("sprite", name="background", sprite="ui_game_filled_circle", depth=1,
           pos="-41,-1", height=40, width=40, color="0, 0, 0", type="sliced")
    w.leaf("button", pos="-21,-21", name="Schematics",
           sprite="ui_game_symbol_book_read", defaultcolor="221, 205, 250",
           tooltip_key="checklistSchematicsTitle",
           style="press, hover, paging.window.icon")
    w.close("append")
    # Reposition the gamepad LB icon to make room for the new button
    w.raw('<set xpath="windows/window[@name=\'windowPagingHeader\']/gamepad_icon[@name=\'LB_Icon\']/@pos">-58,-20</set>')
    # The append into /windows holds the full Schematics window
    w.open("append", xpath="/windows")
    w.open("window", name="Schematics", panel="Left",
           pos="0,0",
           height=WIN_HEIGHT, width=WIN_WIDTH, cursor_area="true",
           globalopacitymod="1.35", controller="MapStats")
    # Header (just a thin black bar; the page title is owned by windowPagingHeader)
    w.open("rect", name="header")
    w.leaf("sprite", name="background", depth=2, pos="0,43", height=45,
           color="[black]", type="sliced", fillcenter="true")
    w.close("rect")
    # Background: filled center + black border (both must be emitted)
    w.open("rect", name="Background")
    w.leaf("sprite", name="background", depth=2, color="[mediumGrey]",
           type="sliced", fillcenter="true", foregroundlayer="false")
    w.leaf("sprite", name="backgroundBorder", depth=2, sprite="menu_empty3px",
           color="[black]", type="sliced", fillcenter="false",
           on_press="true", foregroundlayer="false")
    w.close("rect")
    # 4-tab shell â€” header is small + top-centered so it sits above the magazine strip
    w.open("rect", name="tabs", controller="TabSelector", select_tab_contents_on_open="true")
    w.open("rect", name="tabsHeader")
    w.open("grid", name="tabButtons", pos="455,40", depth=2, rows=1, cols=4,
           cell_width=120, cell_height=40, repeat_content="true", arrangement="horizontal")
    w.open("rect", controller="TabSelectorButton")
    w.leaf("simplebutton", name="tabButton", depth=8, width=120, height=40,
           font_size=20, bordercolor="[black]", defaultcolor="[darkGrey]",
           selectedsprite="menu_empty", selectedcolor="74, 33, 150",
           foregroundlayer="false", caption="{tab_name_localized}")
    w.close("rect")
    w.close("grid")
    w.close("rect")  # tabsHeader
    w.open("rect", name="tabsContents")
    emit_craftinglist_tab(w, skills)
    # Placeholder tabs
    for placeholder, key in (("booksTab", "agfBooks"), ("unlockablesTab", "agfUnlocks"), ("armorsTab", "agfArmors")):
        w.open("rect", name=placeholder, controller="TabSelectorTab", tab_key=key)
        w.leaf("label", name="placeholder", pos="0,-300", width=WIN_WIDTH, height=40,
               justify="center", text=f"({placeholder} - not yet generated)",
               font_size=24, color="[lightGrey]")
        w.close("rect")
    w.close("rect")  # tabsContents
    w.close("rect")  # tabs
    w.close("window")
    w.close("append")
    w.close("AGF-PurpleBookTest")
    return w.render()

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--out-mod", default=DEFAULT_OUT_MOD)
    ap.add_argument("--progression", default=str(DEFAULT_PROGRESSION))
    ap.add_argument("--items", default=str(DEFAULT_ITEMS))
    ap.add_argument("--blocks", default=str(DEFAULT_BLOCKS))
    args = ap.parse_args()

    prog_path = Path(args.progression)
    if not prog_path.exists():
        print(f"[err] progression.xml not found: {prog_path}")
        return 2

    items_path = Path(args.items)
    blocks_path = Path(args.blocks)
    global ITEM_ICONS
    ITEM_ICONS = parse_items(items_path, tag="item")
    print(f"[ok] parsed {len(ITEM_ICONS)} items from {items_path.name}")
    block_icons = parse_items(blocks_path, tag="block")
    # Block icons fill in for unlock entries that are blocks (e.g. tripwirepost,
    # ceilingLight01_player). Item entries take precedence when both exist.
    added = 0
    for nm, ic in block_icons.items():
        if nm not in ITEM_ICONS:
            ITEM_ICONS[nm] = ic
            added += 1
    print(f"[ok] parsed {len(block_icons)} blocks from {blocks_path.name} ({added} new icons)")

    skills = parse_progression(prog_path)
    print(f"[ok] parsed {len(skills)} crafting_skill blocks from {prog_path.name}")
    for s in skills:
        rows = len(s.display)
        kind = "tiered" if all(r.is_tiered for r in s.display) else "mixed"
        print(f"     {s.name:<28} max={s.max_level:<3} rows={rows} [{kind}]")

    # Reorder TAB_ORDER: keep all tiered (and Armor) in their current order,
    # then sort the non-tiered tail by computed column height descending so
    # the panel cascades tallest -> shortest like the tiered prefix.
    by_name = {s.name: s for s in skills}

    def _col_height(sk: Skill) -> int:
        h = 100
        for row in sk.display:
            if row.has_quality and len(row.unlock_levels) == 6:
                h += max(120, 10 + 25 * max(1, len(row.items)) + 5)
            else:
                for i in range(len(row.unlock_levels)):
                    items_here = row.level_items[i] if i < len(row.level_items) else row.items
                    n = max(1, len(items_here))
                    if sk.name == "craftingFood" and n <= 2:
                        h += 30
                    else:
                        h += _nontiered_height(n)
        return h

    head: list[str] = []
    tail: list[str] = []
    for nm in TAB_ORDER:
        sk = by_name.get(nm)
        if sk and all(r.is_tiered for r in sk.display):
            head.append(nm)
        else:
            tail.append(nm)
    tail.sort(key=lambda n: _col_height(by_name[n]) if n in by_name else 0)
    TAB_ORDER[:] = head + tail
    print("[ok] reordered TAB_ORDER (non-tiered tail sorted by height asc)")
    for nm in tail:
        if nm in by_name:
            print(f"     {nm:<28} h={_col_height(by_name[nm])}")

    out_mod_dir = WORKSPACE / "01_Draft" / args.out_mod
    if not out_mod_dir.exists():
        print(f"[err] target mod folder does not exist: {out_mod_dir}")
        print("      Create it first with: python SCRIPT-MakeNewMod.py --name AGF-PurpleBookTest")
        return 2

    out_xui_dir = out_mod_dir / "Config" / "XUi"
    out_xui_dir.mkdir(parents=True, exist_ok=True)
    out_path = out_xui_dir / "windows.xml"

    xml_text = emit_window(skills)
    out_path.write_text(xml_text, encoding="utf-8")
    print(f"[ok] wrote {out_path}  ({len(xml_text):,} bytes)")

    # xui.xml registers the schematics window_group so the button can open it
    xui_path = out_xui_dir / "xui.xml"
    xui_text = (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<AGF-PurpleBookTest>\n'
        '\t<insertbefore xpath="/xui/ruleset/window_group[@name=\'crafting\']">\n'
        f'\t\t<window_group name="schematics" open_backpack_on_open="false" close_compass_on_open="false" stack_panel_y_offset="{STACK_PANEL_Y_OFFSET}">\n'
        '\t\t\t<window name="Schematics"/>\n'
        '\t\t</window_group>\n'
        '\t</insertbefore>\n'
        '</AGF-PurpleBookTest>\n'
    )
    xui_path.write_text(xui_text, encoding="utf-8")
    print(f"[ok] wrote {xui_path}  ({len(xui_text):,} bytes)")

    # Localization for tab captions
    loc_path = out_mod_dir / "Config" / "Localization.txt"
    loc_text = (
        'Key,File,Type,UsedInMainMenu,NoTranslate,english\n'
        'agfChecklist,,,,,"Magazines"\n'
        'agfBooks,,,,,"Books"\n'
        'agfUnlocks,,,,,"Unlocks"\n'
        'agfArmors,,,,,"Armors"\n'
        'agfArmorLight,,,,,"Light Armors"\n'
        'agfArmorMedium,,,,,"Medium Armors"\n'
        'agfArmorHeavy,,,,,"Heavy Armors"\n'
        'lblAll,,,,,"All"\n'
        'checklistSchematicsTitle,,,,,"Crafting List"\n'
        'checklistZoomMagazine,,,,,"(Zoom In - Select Magazine)"\n'
    )
    loc_path.write_text(loc_text, encoding="utf-8")
    print(f"[ok] wrote {loc_path}  ({len(loc_text):,} bytes)")
    return 0

if __name__ == "__main__":
    sys.exit(main())
