"""
SCRIPT-GenPurpleBookCompat.py — Phase 1 (report-only)

Reads vanilla 7D2D Config XMLs, applies a target mod's XML patches in-memory,
and reports what items/recipes/unlocks the target mod adds vs vanilla. This is
the input the Purple Book compatibility-patch generator will use to emit
windows.xml patches in Phase 2.

Scope (locked): target mods that add items unlocked by EXISTING vanilla
books/perks/schematics/magazine skills. New books/skills are out of scope.

Usage:
    python SCRIPT-GenPurpleBookCompat.py --report --target 7D1.0_Izayo_WeaponpackRemastered_45ACP
    python SCRIPT-GenPurpleBookCompat.py --report --target Bdubs_Vehicles
    python SCRIPT-GenPurpleBookCompat.py --report --target 7D1.0_Izayo_WeaponpackRemastered_45ACP,Bdubs_Vehicles
    python SCRIPT-GenPurpleBookCompat.py --dump-diff --dump-dir "C:\\Users\\...\\ConfigsDump"
    python SCRIPT-GenPurpleBookCompat.py --dump-diff-purplebook --dump-dir "C:\\Users\\...\\ConfigsDump"
    python SCRIPT-GenPurpleBookCompat.py --dump-generate-purplebook-rewrite --dump-dir "C:\\Users\\...\\ConfigsDump"
    python SCRIPT-GenPurpleBookCompat.py --dump-generate-purplebook-rewrite-from-generator --dump-dir "C:\\Users\\...\\ConfigsDump"
"""
from __future__ import annotations

import argparse
import os
import re
import shutil
import subprocess
import sys
from collections import defaultdict
from dataclasses import dataclass, field
from typing import Iterable

from lxml import etree

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------

VANILLA_CONFIG_DIR = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config"
MODS_DIR = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods"
HANDWRITTEN_COMPAT_DIR = (
    r"c:\GitHub\7D2D-Mods\02_ActiveBuild\zzzAGF-Special-Compatibilities-v4.1.1"
    r"\Config\XUi\ModPatches"
)
GENERATED_DIR = r"c:\GitHub\7D2D-Mods\_GeneratedCompat"
WORKSPACE_ROOT = os.path.dirname(os.path.abspath(__file__))
SOURCE_PB_GENERATOR_SCRIPT = os.path.join(
    WORKSPACE_ROOT,
    "_DLL-Projects",
    "AGF-PurpleBookGenerator-v0.0.1",
    "Generator",
    "SCRIPT-PurpleBookGenerator.py",
)
GENERATOR_OUT_BASE_DIR = os.path.join(WORKSPACE_ROOT, "_DLL-Projects")
FULLGEN_COMPARE_VANILLA_OUT_MOD = "AGF-PurpleBookGenerator-v0.0.1/temp/fullgen-compare/vanilla"
FULLGEN_COMPARE_DUMP_OUT_MOD = "AGF-PurpleBookGenerator-v0.0.1/temp/fullgen-compare/dump"

# Items whose CustomIcon contains any of these substrings are considered
# "not ready for display" and excluded from generation.
NOT_READY_ICON_MARKERS = ("Notready", "NotReady", "not_ready", "NOTREADY")

# Files we read & patch (only the ones the Purple Book UI cares about).
TRACKED_FILES = ("progression.xml", "items.xml", "recipes.xml", "item_modifiers.xml")

# Dump-diff entity selectors (tag, key-attribute) per tracked file.
# For non-unique keys (e.g. duplicate recipe names), the collector appends
# a deterministic #N suffix so one-to-one matching still works.
DUMP_ENTITY_SPECS: dict[str, tuple[tuple[str, str], ...]] = {
    "items.xml": (("item", "name"),),
    "recipes.xml": (("recipe", "name"),),
    "item_modifiers.xml": (("item_modifier", "name"),),
    "progression.xml": (
        ("crafting_skill", "name"),
        ("skill", "name"),
        ("perk", "name"),
        ("book", "name"),
        ("attribute", "name"),
    ),
}

_COMMENT_MOD_RE = re.compile(r"by:\s*['\"]([^'\"]+)['\"]", re.IGNORECASE)

# ---------------------------------------------------------------------------
# TARGET_PROFILES
#   Hardcoded per-mod (or per-mod-family) configuration for this private build
#   tool.  Each profile picks the rollup strategy and any per-mod overrides.
#
#   strategy:
#     "magazine_summary"  - default; one summary icon per (skill, level) bucket
#     "vehicle_family"    - group items by stem (strip part suffixes); show the
#                           family's *Placeable item as the section icon, with
#                           individual parts in the zoomed grid.
#
#   competing_groups: list of mod-folder lists that share UI sections.  Used
#     by the (future) layout-matrix emitter.  The alphabetically-first loaded
#     mod in each group owns the layout.
# ---------------------------------------------------------------------------

TARGET_PROFILES: dict[str, dict] = {
    # Izy Remastered Pistol family - all share checklistPistols
    "7D1.0_Izayo_WeaponpackRemastered_44mag":          {"compat": "IZY_RMP_44magnum",  "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_45ACP":          {"compat": "IZY_RMP_45ACP",     "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_9mmVAL":         {"compat": "IZY_RMP_9mmVAL",    "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_556pack":        {"compat": "IZY_RMP_556pack",   "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_762pack":        {"compat": "IZY_RMP_762pack",   "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_SHOTGUNpackVAL": {"compat": "IZY_RMP_SG",        "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_DemolitionPack": {"compat": "IZY_RMP_Demopack",  "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_Heavy_WeaponPack":{"compat": "IZY_RMP_HVW",       "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackRemastered_Technicalpack":  {"compat": "IZY_RMP_Tech",      "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackMisc_pack":                 {"compat": "IZY_RMP_Miscpack",  "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackSpecial_Melee":             {"compat": "IZY_melee",         "strategy": "magazine_summary"},
    "7D1.0_Izayo_WeaponpackVanillaReplacer":           {"compat": "IZY_MMVMV2",        "strategy": "magazine_summary"},
    # Bdubs vehicles - family rollup
    "Other-BdubsVehicles":                             {"compat": "Other-BdubsVehicles", "strategy": "vehicle_family"},
}

# Mod folders that share UI sections; alphabetical-first loaded mod owns layout.
COMPETING_GROUPS: list[list[str]] = [
    # Pistol packs all add to checklistPistols
    ["7D1.0_Izayo_WeaponpackRemastered_44mag",
     "7D1.0_Izayo_WeaponpackRemastered_45ACP",
     "7D1.0_Izayo_WeaponpackRemastered_9mmVAL"],
    # Rifle packs share checklistMachineGuns / checklistRifles
    ["7D1.0_Izayo_WeaponpackRemastered_556pack",
     "7D1.0_Izayo_WeaponpackRemastered_762pack"],
]

# Vehicle part-name suffixes to strip when grouping into families.
# Both PascalCase (vanilla / Izy) and lowercase (BDubs) variants are listed
# explicitly so stem detection is deterministic and order-sensitive.  Longest
# suffixes must come first to avoid false partial matches (e.g. "Handlebars"
# before "bars").
VEHICLE_PART_SUFFIXES = (
    "Handlebars", "handlebars",
    "Placeable", "placeable",
    "Accessories", "accessories",
    "Chassis", "chassis",
    "Body", "body",
    "Parts", "parts",
    "Bars", "bars",
    "Wheels", "wheels",
    "Engine", "engine",
)
# Suffixes that identify the canonical "family icon" item (case-insensitive
# match against either variant).
VEHICLE_FAMILY_ICON_SUFFIXES = ("Placeable", "placeable")


# ---------------------------------------------------------------------------
# 7D2D XPath patcher (subset)
# ---------------------------------------------------------------------------
# Supports the subset of operations a vanilla-style mod's XML patch file uses:
#   <append xpath="..."> ... </append>
#   <prepend xpath="..."> ... </prepend>
#   <insertbefore xpath="..."> ... </insertbefore>
#   <insertafter xpath="..."> ... </insertafter>
#   <set xpath="...">value</set>            (replaces text or attribute value)
#   <setattribute xpath="..." name="x">v</setattribute>
#   <remove xpath="..."/>
#   <csv xpath="..." op="add|remove">a,b,c</csv>
#   <conditional><if cond="...">...children...</if></conditional>
# Conditionals: by default we treat mod_loaded('AGF-HUDPlus-PurpleBook') as TRUE
# (we are computing patches that assume Purple Book is loaded), and other
# mod_loaded(X) as FALSE for X != target_mod and TRUE for X == target_mod.

PATCH_OPS = {"append", "prepend", "insertbefore", "insertafter",
             "set", "setattribute", "remove", "csv"}


def _xpath_select(tree: etree._ElementTree, xpath: str) -> list[etree._Element]:
    # 7D2D xpaths can use absolute "/configs/..." or relative; lxml handles both
    # against the tree.  Attribute references like ".../@tags" need special-case.
    try:
        return tree.xpath(xpath)
    except etree.XPathEvalError:
        return []


def _children_of(elem: etree._Element) -> list[etree._Element]:
    return [c for c in elem if isinstance(c.tag, str)]


def _eval_cond(cond: str, *, target_mod: str | None) -> bool:
    """Tiny evaluator for `mod_loaded('X')` / `and` / `or` / `not` / parens."""
    expr = cond.strip()
    # Replace each mod_loaded('X') token with True/False before eval.
    import re

    def repl(m: re.Match) -> str:
        name = m.group(1)
        if name == "AGF-HUDPlus-PurpleBook":
            return "True"
        if target_mod and name == target_mod:
            return "True"
        return "False"

    safe = re.sub(r"mod_loaded\(\s*['\"]([^'\"]+)['\"]\s*\)", repl, expr)
    safe = safe.replace("&&", " and ").replace("||", " or ").replace("!", " not ")
    try:
        return bool(eval(safe, {"__builtins__": {}}, {}))
    except Exception:
        return False


def apply_patch(tree: etree._ElementTree, patch_root: etree._Element,
                *, target_mod: str | None = None) -> int:
    """Apply all top-level patch ops in patch_root onto tree.  Returns op count."""
    count = 0
    for op in _children_of(patch_root):
        tag = op.tag
        if tag == "conditional":
            for child in _children_of(op):
                if child.tag == "if" and _eval_cond(child.get("cond", ""), target_mod=target_mod):
                    count += apply_patch(tree, child, target_mod=target_mod)
                # else/elif intentionally not modeled
            continue
        if tag not in PATCH_OPS:
            # Could be a comment or unknown wrapper; recurse for <configs> nesting
            if tag == "configs":
                count += apply_patch(tree, op, target_mod=target_mod)
            continue

        xpath = op.get("xpath", "")
        if not xpath:
            continue

        # Attribute-targeted xpath (".../@attr") with set/append
        if "/@" in xpath:
            host_xp, _, attr = xpath.rpartition("/@")
            hosts = _xpath_select(tree, host_xp)
            new_text = (op.text or "").strip()
            for host in hosts:
                cur = host.get(attr, "")
                if tag == "set":
                    host.set(attr, new_text)
                elif tag == "append":
                    host.set(attr, cur + new_text)
                elif tag == "prepend":
                    host.set(attr, new_text + cur)
                elif tag == "remove":
                    if attr in host.attrib:
                        del host.attrib[attr]
                count += 1
            continue

        targets = _xpath_select(tree, xpath)
        for target in targets:
            if tag == "append":
                for child in _children_of(op):
                    target.append(_clone(child))
            elif tag == "prepend":
                for i, child in enumerate(_children_of(op)):
                    target.insert(i, _clone(child))
            elif tag == "insertafter":
                parent = target.getparent()
                if parent is None:
                    continue
                idx = list(parent).index(target) + 1
                for child in _children_of(op):
                    parent.insert(idx, _clone(child))
                    idx += 1
            elif tag == "insertbefore":
                parent = target.getparent()
                if parent is None:
                    continue
                idx = list(parent).index(target)
                for child in _children_of(op):
                    parent.insert(idx, _clone(child))
                    idx += 1
            elif tag == "set":
                # set on an element: replace its children with op's children, or text
                for c in list(target):
                    target.remove(c)
                target.text = op.text
                for child in _children_of(op):
                    target.append(_clone(child))
            elif tag == "setattribute":
                target.set(op.get("name", ""), (op.text or "").strip())
            elif tag == "remove":
                parent = target.getparent()
                if parent is not None:
                    parent.remove(target)
            elif tag == "csv":
                csv_op = op.get("op", "add")
                items = [s.strip() for s in (op.text or "").split(",") if s.strip()]
                # csv targets a text node; treat as element text
                cur = [s.strip() for s in (target.text or "").split(",") if s.strip()]
                if csv_op == "add":
                    cur.extend(i for i in items if i not in cur)
                elif csv_op == "remove":
                    cur = [c for c in cur if c not in items]
                target.text = ",".join(cur)
            count += 1
    return count


def _clone(elem: etree._Element) -> etree._Element:
    return etree.fromstring(etree.tostring(elem))


# ---------------------------------------------------------------------------
# Loaders
# ---------------------------------------------------------------------------

def load_xml(path: str) -> etree._ElementTree:
    parser = etree.XMLParser(remove_blank_text=False, recover=True, huge_tree=True)
    return etree.parse(path, parser)


def load_vanilla() -> dict[str, etree._ElementTree]:
    out: dict[str, etree._ElementTree] = {}
    for name in TRACKED_FILES:
        p = os.path.join(VANILLA_CONFIG_DIR, name)
        if os.path.exists(p):
            out[name] = load_xml(p)
    return out


def load_mod_patches(mod_dir: str) -> dict[str, etree._Element]:
    """Return {filename: root_element} for the mod's Config XML files we track."""
    out: dict[str, etree._Element] = {}
    cfg = os.path.join(mod_dir, "Config")
    if not os.path.isdir(cfg):
        return out
    for name in TRACKED_FILES:
        p = os.path.join(cfg, name)
        if os.path.exists(p):
            try:
                out[name] = load_xml(p).getroot()
            except Exception as e:
                print(f"  ! failed to parse {p}: {e}", file=sys.stderr)
    return out


# ---------------------------------------------------------------------------
# Extractors  (run against the merged tree)
# ---------------------------------------------------------------------------

@dataclass
class CraftingSkill:
    name: str
    max_level: int
    # breakpoint level -> list of item tags unlocked at that level
    breakpoints: dict[int, list[str]] = field(default_factory=lambda: defaultdict(list))


@dataclass
class Item:
    name: str
    unlocked_by: str | None = None        # raw value of UnlockedBy property
    custom_icon: str | None = None
    # Tags property (used by RecipeTagUnlocked matching)
    tags: list[str] = field(default_factory=list)


@dataclass
class Recipe:
    name: str           # item name produced
    tags: list[str] = field(default_factory=list)


@dataclass
class Snapshot:
    skills: dict[str, CraftingSkill] = field(default_factory=dict)
    items: dict[str, Item] = field(default_factory=dict)
    recipes: dict[str, Recipe] = field(default_factory=dict)


def extract_progression(tree: etree._ElementTree) -> dict[str, CraftingSkill]:
    skills: dict[str, CraftingSkill] = {}
    root = tree.getroot()
    for cs in root.iter("crafting_skill"):
        name = cs.get("name")
        if not name:
            continue
        try:
            maxlvl = int(cs.get("max_level") or "100")
        except ValueError:
            maxlvl = 100
        skill = CraftingSkill(name=name, max_level=maxlvl)
        for pe in cs.iter("passive_effect"):
            if pe.get("name") != "RecipeTagUnlocked":
                continue
            level_attr = pe.get("level", "")
            tags_attr = pe.get("tags", "")
            if not level_attr or not tags_attr:
                continue
            # level="1,100" -> first number is the breakpoint
            try:
                bp = int(level_attr.split(",")[0])
            except ValueError:
                continue
            tags = [t.strip() for t in tags_attr.split(",") if t.strip()]
            skill.breakpoints[bp].extend(tags)
        skills[name] = skill
    return skills


def extract_items(tree: etree._ElementTree) -> dict[str, Item]:
    items: dict[str, Item] = {}
    root = tree.getroot()
    for it in root.iter("item"):
        name = it.get("name")
        if not name:
            continue
        item = Item(name=name)
        for prop in it.iter("property"):
            pname = prop.get("name")
            pval = prop.get("value", "")
            if pname == "UnlockedBy":
                item.unlocked_by = pval
            elif pname == "CustomIcon":
                item.custom_icon = pval
            elif pname == "Tags":
                item.tags = [t.strip() for t in pval.split(",") if t.strip()]
        items[name] = item
    return items


def extract_recipes(tree: etree._ElementTree) -> dict[str, Recipe]:
    """Return {item_name: Recipe} for every <recipe> element.  The recipe's
    `name` attribute is the produced item; recipes that share an item name
    overwrite (last wins) since we only need to know craftability, not detail."""
    out: dict[str, Recipe] = {}
    for r in tree.getroot().iter("recipe"):
        name = r.get("name")
        if not name:
            continue
        tags = [t.strip() for t in (r.get("tags") or "").split(",") if t.strip()]
        out[name] = Recipe(name=name, tags=tags)
    return out


def snapshot(trees: dict[str, etree._ElementTree]) -> Snapshot:
    snap = Snapshot()
    if "progression.xml" in trees:
        snap.skills = extract_progression(trees["progression.xml"])
    if "items.xml" in trees:
        snap.items = extract_items(trees["items.xml"])
    if "recipes.xml" in trees:
        snap.recipes = extract_recipes(trees["recipes.xml"])
    return snap


# ---------------------------------------------------------------------------
# Patching pipeline
# ---------------------------------------------------------------------------

def merge_mod_into(trees: dict[str, etree._ElementTree], mod_dir: str,
                   target_mod: str) -> dict[str, int]:
    """Apply mod_dir's Config patches onto trees in place. Returns op counts."""
    counts: dict[str, int] = {}
    patches = load_mod_patches(mod_dir)
    for fname, patch_root in patches.items():
        if fname not in trees:
            # Mod adds a file we don't track in vanilla baseline -> create empty
            trees[fname] = etree.ElementTree(etree.Element("configs"))
        n = apply_patch(trees[fname], patch_root, target_mod=target_mod)
        counts[fname] = n
    return counts


# ---------------------------------------------------------------------------
# Reporter
# ---------------------------------------------------------------------------

def diff_snapshots(base: Snapshot, after: Snapshot) -> dict:
    new_items = {n: it for n, it in after.items.items() if n not in base.items}

    # New (skill, level, tag) tuples
    new_skill_unlocks: list[tuple[str, int, list[str]]] = []
    new_breakpoints: list[tuple[str, int]] = []
    for sname, sk in after.skills.items():
        bsk = base.skills.get(sname)
        for lvl, tags in sorted(sk.breakpoints.items()):
            base_tags = set(bsk.breakpoints.get(lvl, [])) if bsk else set()
            added = [t for t in tags if t not in base_tags]
            if added:
                new_skill_unlocks.append((sname, lvl, added))
            if bsk and lvl not in bsk.breakpoints and lvl in sk.breakpoints:
                new_breakpoints.append((sname, lvl))

    return {
        "new_items": new_items,
        "new_skill_unlocks": new_skill_unlocks,
        "new_breakpoints": new_breakpoints,
    }


def _bucket_item(it: Item) -> str:
    ub = (it.unlocked_by or "").strip()
    if not ub:
        return "no UnlockedBy (likely schematic-only or always-unlocked)"
    if ub.startswith("crafting"):
        return f"magazine skill: {ub}"
    if "Complete" in ub:
        return f"book complete cvar: {ub}"
    if ub.startswith("perk"):
        return f"perk: {ub}"
    return f"other: {ub}"


def report(target_mod: str) -> None:
    mod_dir = os.path.join(MODS_DIR, target_mod)
    if not os.path.isdir(mod_dir):
        print(f"!! mod folder not found: {mod_dir}")
        return

    print(f"\n========== {target_mod} ==========")

    # Baseline snapshot
    vanilla_trees = load_vanilla()
    base_snap = snapshot(vanilla_trees)

    # Apply mod
    counts = merge_mod_into(vanilla_trees, mod_dir, target_mod)
    after_snap = snapshot(vanilla_trees)

    print(f"  patch ops applied: {counts}")
    print(f"  baseline items   : {len(base_snap.items)}")
    print(f"  after items      : {len(after_snap.items)}")

    delta = diff_snapshots(base_snap, after_snap)

    # --- New items grouped by UnlockedBy bucket ---
    print(f"\n  -- New items ({len(delta['new_items'])}) grouped by UnlockedBy --")
    by_bucket: dict[str, list[Item]] = defaultdict(list)
    for it in delta["new_items"].values():
        by_bucket[_bucket_item(it)].append(it)
    for bucket in sorted(by_bucket):
        items = by_bucket[bucket]
        print(f"    [{bucket}]  ({len(items)})")
        for it in sorted(items, key=lambda x: x.name)[:15]:
            icon = f"  icon={it.custom_icon}" if it.custom_icon else ""
            print(f"        - {it.name}{icon}")
        if len(items) > 15:
            print(f"        ... and {len(items)-15} more")

    # --- Magazine skill unlocks added by the mod ---
    print(f"\n  -- New magazine-skill RecipeTagUnlocked entries "
          f"({len(delta['new_skill_unlocks'])}) --")
    by_skill: dict[str, list[tuple[int, list[str]]]] = defaultdict(list)
    for sname, lvl, tags in delta["new_skill_unlocks"]:
        by_skill[sname].append((lvl, tags))
    for sname in sorted(by_skill):
        print(f"    skill {sname}:")
        for lvl, tags in sorted(by_skill[sname]):
            base_bp = sname in base_snap.skills and lvl in base_snap.skills[sname].breakpoints
            marker = "" if base_bp else "  ** NEW BREAKPOINT **"
            print(f"        L{lvl:>3}: {', '.join(tags)}{marker}")

    if delta["new_breakpoints"]:
        print(f"\n  !! new breakpoints (would need craftingChecker buffs.xml entry):")
        for sname, lvl in delta["new_breakpoints"]:
            print(f"      {sname} @ level {lvl}")


# ---------------------------------------------------------------------------
# Rollup: turn delta into UI-icon plan
# ---------------------------------------------------------------------------

def _is_not_ready(item: Item) -> bool:
    if not item.custom_icon:
        return False
    return any(m in item.custom_icon for m in NOT_READY_ICON_MARKERS)


def _find_breakpoint(skills: dict[str, CraftingSkill],
                     skill_name: str, item_name: str) -> int | None:
    sk = skills.get(skill_name)
    if not sk:
        return None
    for lvl in sorted(sk.breakpoints):
        if item_name in sk.breakpoints[lvl]:
            return lvl
    return None


@dataclass
class UnlockEntry:
    """One icon to insert under unlockablesTab/grid (single item per icon)."""
    item: str
    icon: str
    fill_cvar: str        # e.g. "AGFperkPistolPeteComplete" or "1"


@dataclass
class MagazineSummary:
    """One summary icon for a (skill, level) section. Aggregates all
    items the mod adds in that section."""
    skill: str
    level: int
    icon: str             # CustomIcon of alphabetically-first item
    icon_item: str        # name of that first item (for tooltip_key fallback)
    items: list[str]      # all member item names (alphabetical)
    loc_key: str          # generated localization key


@dataclass
class VehicleFamily:
    """One vehicle family - icon is the *Placeable; section drives off the
    highest breakpoint among its parts."""
    stem: str             # e.g. "vehicleTruck4x4OldBV"
    skill: str            # e.g. "craftingVehicleCheck"
    level: int
    icon_item: str        # the *Placeable item (or first part if no placeable)
    icon: str             # CustomIcon of icon_item (or item name fallback)
    parts: list[str]      # all part item names (alphabetical)
    loc_key: str


@dataclass
class RolledPlan:
    target_mod: str
    strategy: str = "magazine_summary"
    skipped_not_ready: list[str] = field(default_factory=list)
    skipped_no_unlock: list[str] = field(default_factory=list)
    skipped_not_craftable: list[str] = field(default_factory=list)
    skipped_unmapped: list[str] = field(default_factory=list)   # crafting skill but no breakpoint match
    unlock_entries: list[UnlockEntry] = field(default_factory=list)
    magazine_sections: list[MagazineSummary] = field(default_factory=list)
    vehicle_families: list[VehicleFamily] = field(default_factory=list)


# Map UnlockedBy values to a fill cvar for unlocks tab icons.
# Anything not matched falls back to "1" (always-filled, schematic style).
_FILL_PATTERNS = [
    # (suffix, transform) -> exact UnlockedBy value -> fill cvar
    # Direct cvar reference: UnlockedBy=perkXxxComplete -> AGFperkXxxComplete
    # UnlockedBy=perkXxx -> AGFperkXxx
]


def _pick_fill_for_unlock(unlocked_by: str) -> str:
    ub = (unlocked_by or "").strip()
    if not ub:
        return "1"
    if ub.startswith("perk"):
        # e.g. perkPistolPeteComplete -> AGFperkPistolPeteComplete
        return f"{{cvar(AGF{ub})}}"
    if ub.startswith("crafting"):
        # crafting magazine unlock — handled in magazine flow, not unlocks tab
        return "1"
    # Schematic / item / unknown -> always show
    return "1"


def _vehicle_stem(name: str) -> str:
    for suf in VEHICLE_PART_SUFFIXES:
        if name.endswith(suf):
            return name[: -len(suf)]
    return name


def _is_craftable(name: str, snap: Snapshot) -> bool:
    return name in snap.recipes


def rollup(target_mod: str, after_snap: Snapshot,
           delta: dict, base_snap: Snapshot,
           strategy: str = "magazine_summary") -> RolledPlan:
    plan = RolledPlan(target_mod=target_mod, strategy=strategy)
    new_items: dict[str, Item] = delta["new_items"]

    if strategy == "vehicle_family":
        return _rollup_vehicle_family(plan, new_items, after_snap)
    return _rollup_magazine_summary(plan, new_items, after_snap)


def _rollup_magazine_summary(plan: RolledPlan, new_items: dict[str, Item],
                              after_snap: Snapshot) -> RolledPlan:
    target_mod = plan.target_mod
    mag_buckets: dict[tuple[str, int], list[Item]] = defaultdict(list)

    for name in sorted(new_items):
        it = new_items[name]
        if _is_not_ready(it):
            plan.skipped_not_ready.append(name)
            continue
        ub = (it.unlocked_by or "").strip()
        craftable = _is_craftable(name, after_snap)

        if ub.startswith("crafting"):
            if not craftable:
                plan.skipped_not_craftable.append(f"{name} (UnlockedBy={ub})")
                continue
            lvl = _find_breakpoint(after_snap.skills, ub, name)
            if lvl is None:
                plan.skipped_unmapped.append(f"{name} (UnlockedBy={ub}, no breakpoint match)")
                continue
            mag_buckets[(ub, lvl)].append(it)
        elif ub.startswith("perk") or "Complete" in ub:
            if not craftable:
                plan.skipped_not_craftable.append(f"{name} (UnlockedBy={ub})")
                continue
            plan.unlock_entries.append(UnlockEntry(
                item=name,
                icon=it.custom_icon or name,
                fill_cvar=_pick_fill_for_unlock(ub),
            ))
        elif not ub:
            # No UnlockedBy: only show if craftable (vanilla recipe = always-unlocked)
            if craftable:
                plan.unlock_entries.append(UnlockEntry(
                    item=name, icon=it.custom_icon or name, fill_cvar="1",
                ))
            else:
                plan.skipped_no_unlock.append(name)
        else:
            plan.skipped_unmapped.append(f"{name} (UnlockedBy={ub})")

    for (skill, lvl), items in sorted(mag_buckets.items()):
        items_sorted = sorted(items, key=lambda i: i.name)
        first = items_sorted[0]
        plan.magazine_sections.append(MagazineSummary(
            skill=skill,
            level=lvl,
            icon=first.custom_icon or first.name,
            icon_item=first.name,
            items=[i.name for i in items_sorted],
            loc_key=f"AGFcompat_{target_mod}_{skill}_L{lvl}",
        ))
    return plan


def _rollup_vehicle_family(plan: RolledPlan, new_items: dict[str, Item],
                            after_snap: Snapshot) -> RolledPlan:
    """Group craftable items by stem; pick *Placeable as the family icon;
    the section level is the max breakpoint of any part in the family."""
    target_mod = plan.target_mod
    families: dict[str, list[Item]] = defaultdict(list)

    for name in sorted(new_items):
        it = new_items[name]
        if _is_not_ready(it):
            plan.skipped_not_ready.append(name)
            continue
        if not _is_craftable(name, after_snap):
            plan.skipped_not_craftable.append(name)
            continue
        stem = _vehicle_stem(name)
        families[stem].append(it)

    for stem, items in sorted(families.items()):
        items_sorted = sorted(items, key=lambda i: i.name)
        # Pick icon item: prefer *Placeable
        icon_item = next(
            (i for i in items_sorted
             if any(i.name.endswith(s) for s in VEHICLE_FAMILY_ICON_SUFFIXES)),
            items_sorted[0],
        )
        # Determine skill + level: scan each part's UnlockedBy / breakpoint;
        # pick the highest breakpoint across parts (gates the whole family).
        best_skill: str | None = None
        best_level: int = -1
        for it in items_sorted:
            ub = (it.unlocked_by or "").strip()
            if not ub.startswith("crafting"):
                continue
            lvl = _find_breakpoint(after_snap.skills, ub, it.name)
            if lvl is not None and lvl > best_level:
                best_level = lvl
                best_skill = ub
        if best_skill is None:
            # No magazine-skill gate found - skip family (likely placeable-only,
            # or unlocked through a perk we'd handle separately)
            for it in items_sorted:
                plan.skipped_unmapped.append(f"{it.name} (family={stem}, no breakpoint)")
            continue
        plan.vehicle_families.append(VehicleFamily(
            stem=stem,
            skill=best_skill,
            level=best_level,
            icon_item=icon_item.name,
            icon=icon_item.custom_icon or icon_item.name,
            parts=[i.name for i in items_sorted],
            loc_key=f"AGFcompat_{target_mod}_family_{stem}",
        ))
    return plan


# ---------------------------------------------------------------------------
# Draft writer: emit review YAML + Localization.txt to _GeneratedCompat
# ---------------------------------------------------------------------------

import yaml  # type: ignore

def _csv_escape(s: str) -> str:
    """7D2D Localization.txt uses CSV; quote and double-up internal quotes."""
    return '"' + s.replace('"', '""') + '"'


def write_draft(plan: RolledPlan) -> None:
    out_dir = os.path.join(GENERATED_DIR, plan.target_mod)
    os.makedirs(out_dir, exist_ok=True)

    # 1) review YAML — author-editable curation file
    yaml_path = os.path.join(out_dir, "review.yaml")
    payload = {
        "target_mod": plan.target_mod,
        "strategy": plan.strategy,
        "filters": {
            "not_ready_markers": list(NOT_READY_ICON_MARKERS),
        },
        "unlock_entries": [
            {"item": e.item, "icon": e.icon, "fill": e.fill_cvar}
            for e in plan.unlock_entries
        ],
        "magazine_sections": [
            {
                "skill": s.skill,
                "level": s.level,
                "loc_key": s.loc_key,
                "icon_item": s.icon_item,
                "icon": s.icon,
                "items": s.items,
            }
            for s in plan.magazine_sections
        ],
        "vehicle_families": [
            {
                "stem": v.stem,
                "skill": v.skill,
                "level": v.level,
                "loc_key": v.loc_key,
                "icon_item": v.icon_item,
                "icon": v.icon,
                "parts": v.parts,
            }
            for v in plan.vehicle_families
        ],
        "skipped": {
            "not_ready": plan.skipped_not_ready,
            "no_unlock": plan.skipped_no_unlock,
            "not_craftable": plan.skipped_not_craftable,
            "unmapped": plan.skipped_unmapped,
        },
    }
    with open(yaml_path, "w", encoding="utf-8") as f:
        yaml.safe_dump(payload, f, sort_keys=False, allow_unicode=True, width=200)

    # 2) Localization.txt — one row per magazine summary icon
    loc_path = os.path.join(out_dir, "Localization.txt")
    header = ("Key,File,Type,UsedInMainMenu,NoTranslate,english,"
              "Context / Alternate Text,german,spanish,french,italian,"
              "japanese,koreana,polish,brazilian,russian,turkish,"
              "schinese,tchinese\n")
    with open(loc_path, "w", encoding="utf-8") as f:
        f.write(header)
        for sec in plan.magazine_sections:
            display_name = f"{plan.target_mod} — {sec.skill} L{sec.level}"
            items_csv = ", ".join(sec.items)
            english = f"{display_name}:\\n{items_csv}"
            row = (
                f"{sec.loc_key},XUi,checklist,,,"
                f"{_csv_escape(english)},,,,,,,,,,,,,\n"
            )
            f.write(row)

    print(f"  draft written -> {yaml_path}")
    print(f"  loc written   -> {loc_path}")


def draft(target_mod: str) -> None:
    mod_dir = os.path.join(MODS_DIR, target_mod)
    if not os.path.isdir(mod_dir):
        print(f"!! mod folder not found: {mod_dir}")
        return
    profile = TARGET_PROFILES.get(target_mod, {})
    strategy = profile.get("strategy", "magazine_summary")
    print(f"\n========== DRAFT {target_mod}  [strategy={strategy}] ==========")

    vanilla_trees = load_vanilla()
    base_snap = snapshot(vanilla_trees)
    merge_mod_into(vanilla_trees, mod_dir, target_mod)
    after_snap = snapshot(vanilla_trees)
    delta = diff_snapshots(base_snap, after_snap)

    plan = rollup(target_mod, after_snap, delta, base_snap, strategy=strategy)

    print(f"  unlock entries     : {len(plan.unlock_entries)}")
    print(f"  magazine sections  : {len(plan.magazine_sections)}")
    if plan.magazine_sections:
        for s in plan.magazine_sections:
            print(f"      {s.skill} L{s.level}  icon={s.icon}  ({len(s.items)} items)")
    print(f"  vehicle families   : {len(plan.vehicle_families)}")
    if plan.vehicle_families:
        for v in plan.vehicle_families:
            print(f"      {v.stem}  skill={v.skill} L{v.level}  icon_item={v.icon_item}  ({len(v.parts)} parts)")
    print(f"  skipped not-ready    : {len(plan.skipped_not_ready)}")
    print(f"  skipped no-unlock    : {len(plan.skipped_no_unlock)}")
    print(f"  skipped not-craftable: {len(plan.skipped_not_craftable)}")
    print(f"  skipped unmapped     : {len(plan.skipped_unmapped)}")
    if plan.skipped_unmapped:
        for s in plan.skipped_unmapped[:5]:
            print(f"      ! {s}")
        if len(plan.skipped_unmapped) > 5:
            print(f"      ... and {len(plan.skipped_unmapped)-5} more")

    write_draft(plan)


# ---------------------------------------------------------------------------
# Cross-check: parse handwritten compat windows.xml -> set of inserted items
# ---------------------------------------------------------------------------

@dataclass
class HandwrittenInsert:
    item: str                  # tooltip_key or sprite name
    location: str              # human-readable section path
    fill: str | None = None    # filledsprite fill="" attribute
    cond: str = ""             # the <if cond=...> guarding this insert


def parse_handwritten_windows(path: str) -> list[HandwrittenInsert]:
    """Walk an existing handwritten zzzAGF compat windows.xml and pull out
    every item/sprite icon that gets inserted, along with the conditional
    branch and target section path."""
    if not os.path.exists(path):
        return []
    tree = load_xml(path)
    inserts: list[HandwrittenInsert] = []

    def walk(elem: etree._Element, cond: str) -> None:
        for child in _children_of(elem):
            if child.tag == "conditional":
                walk(child, cond)
            elif child.tag == "if":
                walk(child, child.get("cond", "").strip())
            elif child.tag in ("append", "insertafter", "insertbefore"):
                xp = child.get("xpath", "")
                section = _summarize_section(xp)
                # Look for <entry name="item"> blocks (unlock tab) or
                # <sprite name="itemIcon"> (checklist/zoomed)
                for inserted in child.iter():
                    if inserted is child:
                        continue
                    if inserted.tag == "entry" and inserted.get("name") == "item":
                        # Find the itemIcon sprite and the filledsprite inside
                        item = None
                        fill = None
                        for s in inserted.iter("sprite"):
                            if s.get("name") == "itemIcon":
                                item = s.get("tooltip_key") or s.get("sprite")
                                break
                        for fs in inserted.iter("filledsprite"):
                            fill = fs.get("fill")
                            break
                        if item:
                            inserts.append(HandwrittenInsert(
                                item=item, location=section, fill=fill, cond=cond))
                    elif inserted.tag == "sprite" and inserted.get("name") == "itemIcon":
                        # Direct sprite (in checklist rect[N] or zoomed entry[N])
                        # Don't double-count when nested inside an <entry>
                        if inserted.getparent() is child:
                            item = inserted.get("tooltip_key") or inserted.get("sprite")
                            if item:
                                inserts.append(HandwrittenInsert(
                                    item=item, location=section, cond=cond))
            else:
                walk(child, cond)

    walk(tree.getroot(), cond="")
    return inserts


_SECTION_PATTERNS = [
    (r"unlockablesTab/grid", "unlocks"),
    (r"allChecklists/rect\[@name='checklist(\w+)'\]/rect\[(\d+)\]", "compact:{0} rect[{1}]"),
    (r"zoomed(\w+)'\]/grid", "zoomed:{0}"),
]


def _summarize_section(xpath: str) -> str:
    import re
    for pat, fmt in _SECTION_PATTERNS:
        m = re.search(pat, xpath)
        if m:
            return fmt.format(*m.groups())
    return xpath[:80]


def cross_check(target_mod: str, handwritten_subfolder: str) -> None:
    """Compare Phase-1 delta items against the items inserted by the
    handwritten compat folder."""
    print(f"\n========== CROSS-CHECK {target_mod} <-> {handwritten_subfolder} ==========")
    mod_dir = os.path.join(MODS_DIR, target_mod)
    if not os.path.isdir(mod_dir):
        print(f"!! mod folder not found: {mod_dir}")
        return
    hand_path = os.path.join(HANDWRITTEN_COMPAT_DIR, handwritten_subfolder, "windows.xml")
    if not os.path.exists(hand_path):
        print(f"!! handwritten file not found: {hand_path}")
        return

    # Phase 1 delta
    vanilla_trees = load_vanilla()
    base_snap = snapshot(vanilla_trees)
    merge_mod_into(vanilla_trees, mod_dir, target_mod)
    after_snap = snapshot(vanilla_trees)
    delta = diff_snapshots(base_snap, after_snap)
    delta_items: set[str] = set(delta["new_items"].keys())

    inserts = parse_handwritten_windows(hand_path)
    print(f"  handwritten inserts: {len(inserts)} icon entries")
    print(f"  delta new items   : {len(delta_items)}")

    # Group inserts by location, dedupe by item per location
    by_loc: dict[str, list[HandwrittenInsert]] = defaultdict(list)
    for ins in inserts:
        by_loc[ins.location].append(ins)

    handwritten_items: set[str] = {ins.item for ins in inserts}

    only_in_delta = delta_items - handwritten_items
    only_in_handwritten = handwritten_items - delta_items
    matched = delta_items & handwritten_items

    print(f"\n  matched          : {len(matched)}")
    print(f"  in delta only    : {len(only_in_delta)}")
    print(f"  in handwritten only: {len(only_in_handwritten)}")

    if only_in_delta:
        print("\n  -- in delta but NOT inserted by handwritten patch --")
        for n in sorted(only_in_delta):
            it = delta["new_items"][n]
            print(f"      {n}  (UnlockedBy={it.unlocked_by})")

    if only_in_handwritten:
        print("\n  -- inserted by handwritten patch but NOT in our delta --")
        for n in sorted(only_in_handwritten):
            print(f"      {n}")

    print("\n  -- handwritten inserts grouped by location --")
    for loc in sorted(by_loc):
        items = by_loc[loc]
        print(f"    [{loc}]  ({len(items)})")
        for ins in items[:8]:
            fill = f"  fill={ins.fill}" if ins.fill else ""
            cond = f"   cond=\"{ins.cond[:60]}{'...' if len(ins.cond)>60 else ''}\"" if ins.cond else ""
            print(f"        - {ins.item}{fill}{cond}")
        if len(items) > 8:
            print(f"        ... and {len(items)-8} more")


# ---------------------------------------------------------------------------
# Dump-diff mode: compare ConfigsDump vs vanilla by entity key/signature
# ---------------------------------------------------------------------------

@dataclass
class DumpEntityRecord:
    tag: str
    key: str
    mods: list[str] = field(default_factory=list)


@dataclass
class DumpFileDiff:
    file_name: str
    changed: list[DumpEntityRecord] = field(default_factory=list)
    added: list[DumpEntityRecord] = field(default_factory=list)
    removed: list[DumpEntityRecord] = field(default_factory=list)
    mod_marker_counts: dict[str, int] = field(default_factory=dict)


def _normalize_text(text: str | None) -> str:
    if not text:
        return ""
    return " ".join(text.split())


def _element_signature(elem: etree._Element) -> tuple:
    children = tuple(
        _element_signature(child)
        for child in elem
        if isinstance(child.tag, str)
    )
    return (
        elem.tag,
        tuple(sorted(elem.attrib.items())),
        _normalize_text(elem.text),
        children,
    )


def _mods_from_comment_text(text: str | None) -> list[str]:
    if not text:
        return []
    out: list[str] = []
    for m in _COMMENT_MOD_RE.finditer(text):
        mod_name = m.group(1).strip()
        if mod_name:
            out.append(mod_name)
    return out


def _mods_for_element(elem: etree._Element) -> list[str]:
    mods: set[str] = set()
    for node in elem.iter():
        if isinstance(node, etree._Comment):
            mods.update(_mods_from_comment_text(node.text))
    return sorted(mods)


def _mod_marker_counts(root: etree._Element) -> dict[str, int]:
    counts: dict[str, int] = defaultdict(int)
    for node in root.iter():
        if isinstance(node, etree._Comment):
            for mod_name in _mods_from_comment_text(node.text):
                counts[mod_name] += 1
    return dict(sorted(counts.items(), key=lambda kv: (-kv[1], kv[0])))


def _collect_keyed_entities(
    tree: etree._ElementTree,
    specs: tuple[tuple[str, str], ...],
) -> dict[str, etree._Element]:
    out: dict[str, etree._Element] = {}
    for tag, key_attr in specs:
        seen: dict[str, int] = defaultdict(int)
        for elem in tree.getroot().iter(tag):
            base_key = (elem.get(key_attr) or "").strip()
            if not base_key:
                base_key = f"@missing-{key_attr}"
            seen[base_key] += 1
            ordinal = seen[base_key]
            stable_key = base_key if ordinal == 1 else f"{base_key}#{ordinal}"
            out[f"{tag}:{stable_key}"] = elem
    return out


def _record_from_entity(entity_id: str, elem: etree._Element | None) -> DumpEntityRecord:
    tag, _, key = entity_id.partition(":")
    mods = _mods_for_element(elem) if elem is not None else []
    return DumpEntityRecord(tag=tag, key=key, mods=mods)


def _diff_dump_file(
    file_name: str,
    vanilla_tree: etree._ElementTree,
    dump_tree: etree._ElementTree,
    specs: tuple[tuple[str, str], ...],
) -> DumpFileDiff:
    base = _collect_keyed_entities(vanilla_tree, specs)
    after = _collect_keyed_entities(dump_tree, specs)

    base_keys = set(base.keys())
    after_keys = set(after.keys())

    changed: list[DumpEntityRecord] = []
    added: list[DumpEntityRecord] = []
    removed: list[DumpEntityRecord] = []

    for entity_id in sorted(base_keys & after_keys):
        if _element_signature(base[entity_id]) != _element_signature(after[entity_id]):
            changed.append(_record_from_entity(entity_id, after[entity_id]))

    for entity_id in sorted(after_keys - base_keys):
        added.append(_record_from_entity(entity_id, after[entity_id]))

    for entity_id in sorted(base_keys - after_keys):
        removed.append(_record_from_entity(entity_id, None))

    return DumpFileDiff(
        file_name=file_name,
        changed=changed,
        added=added,
        removed=removed,
        mod_marker_counts=_mod_marker_counts(dump_tree.getroot()),
    )


def _print_entity_bucket(label: str, rows: list[DumpEntityRecord], detail_limit: int) -> None:
    print(f"    {label}: {len(rows)}")
    if detail_limit <= 0:
        return
    for row in rows[:detail_limit]:
        mods = ", ".join(row.mods) if row.mods else "unknown"
        print(f"      - {row.tag}:{row.key}  [mods={mods}]")
    if len(rows) > detail_limit:
        print(f"      ... and {len(rows) - detail_limit} more")


def dump_diff_report(dump_dir: str, vanilla_dir: str, detail_limit: int = 20) -> int:
    if not os.path.isdir(dump_dir):
        print(f"!! dump dir not found: {dump_dir}", file=sys.stderr)
        return 2
    if not os.path.isdir(vanilla_dir):
        print(f"!! vanilla dir not found: {vanilla_dir}", file=sys.stderr)
        return 2

    print("\n========== DUMP DIFF (vanilla -> merged dump) ==========")
    print(f"  vanilla dir: {vanilla_dir}")
    print(f"  dump dir   : {dump_dir}")

    total_changed = 0
    total_added = 0
    total_removed = 0

    for file_name in TRACKED_FILES:
        specs = DUMP_ENTITY_SPECS.get(file_name)
        if not specs:
            continue
        vanilla_path = os.path.join(vanilla_dir, file_name)
        dump_path = os.path.join(dump_dir, file_name)

        if not os.path.exists(vanilla_path):
            print(f"\n  !! missing vanilla file: {vanilla_path}")
            continue
        if not os.path.exists(dump_path):
            print(f"\n  !! missing dump file: {dump_path}")
            continue

        try:
            vanilla_tree = load_xml(vanilla_path)
            dump_tree = load_xml(dump_path)
        except Exception as ex:
            print(f"\n  !! failed to parse {file_name}: {ex}")
            continue

        file_diff = _diff_dump_file(file_name, vanilla_tree, dump_tree, specs)
        total_changed += len(file_diff.changed)
        total_added += len(file_diff.added)
        total_removed += len(file_diff.removed)

        print(f"\n  -- {file_name} --")
        _print_entity_bucket("changed", file_diff.changed, detail_limit)
        _print_entity_bucket("added", file_diff.added, detail_limit)
        _print_entity_bucket("removed", file_diff.removed, detail_limit)

        if file_diff.mod_marker_counts:
            top = list(file_diff.mod_marker_counts.items())[:5]
            top_text = ", ".join(f"{name}={count}" for name, count in top)
            print(f"    marker mods: {top_text}")
        else:
            print("    marker mods: none")

    print("\n  == totals ==")
    print(f"    changed: {total_changed}")
    print(f"    added  : {total_added}")
    print(f"    removed: {total_removed}")
    return 0


def _diff_projection(
    base: dict[str, tuple],
    after: dict[str, tuple],
) -> tuple[list[str], list[str], list[str]]:
    base_keys = set(base.keys())
    after_keys = set(after.keys())
    changed = sorted(k for k in (base_keys & after_keys) if base[k] != after[k])
    added = sorted(after_keys - base_keys)
    removed = sorted(base_keys - after_keys)
    return changed, added, removed


def _print_key_bucket(label: str, keys: list[str], detail_limit: int) -> None:
    print(f"    {label}: {len(keys)}")
    if detail_limit <= 0:
        return
    for key in keys[:detail_limit]:
        print(f"      - {key}")
    if len(keys) > detail_limit:
        print(f"      ... and {len(keys) - detail_limit} more")


def _progression_purplebook_projection(tree: etree._ElementTree) -> dict[str, tuple]:
    out: dict[str, tuple] = {}
    skills = extract_progression(tree)
    for name, sk in skills.items():
        breakpoints = tuple(
            (lvl, tuple(sorted(set(tags))))
            for lvl, tags in sorted(sk.breakpoints.items())
        )
        out[name] = (sk.max_level, breakpoints)
    return out


def _recipe_purplebook_projection(tree: etree._ElementTree) -> dict[str, tuple]:
    out: dict[str, tuple] = {}
    for rec in tree.getroot().iter("recipe"):
        name = (rec.get("name") or "").strip()
        if not name:
            continue
        tags = tuple(
            sorted(
                set(
                    t.strip()
                    for t in (rec.get("tags") or "").split(",")
                    if t.strip()
                )
            )
        )
        learnable = "1" if "learnable" in {t.lower() for t in tags} else "0"
        always_unlocked = (rec.get("always_unlocked") or "").strip().lower()
        out[name] = (tags, learnable, always_unlocked)
    return out


def _item_purplebook_projection(
    items_tree: etree._ElementTree,
    recipes_tree: etree._ElementTree,
) -> dict[str, tuple]:
    out: dict[str, tuple] = {}
    items = extract_items(items_tree)
    recipe_names = set(extract_recipes(recipes_tree).keys())
    for name, it in items.items():
        out[name] = (
            (it.unlocked_by or "").strip(),
            (it.custom_icon or "").strip(),
            tuple(sorted(set(it.tags))),
            "1" if name in recipe_names else "0",
        )
    return out


def _item_modifier_purplebook_projection(tree: etree._ElementTree) -> dict[str, tuple]:
    out: dict[str, tuple] = {}
    for mod in tree.getroot().iter("item_modifier"):
        name = (mod.get("name") or "").strip()
        if not name:
            continue
        unlocked_by = ""
        for prop in mod.iter("property"):
            if (prop.get("name") or "") == "UnlockedBy":
                unlocked_by = (prop.get("value") or "").strip()
        out[name] = (unlocked_by,)
    return out


@dataclass(frozen=True)
class PurpleBookSkillLayout:
    skill_name: str
    cvar_prefix: str
    checklist_rect: str
    zoomed_rect: str
    title_grid: str


PURPLEBOOK_SKILL_LAYOUTS: dict[str, PurpleBookSkillLayout] = {
    "craftingSeeds": PurpleBookSkillLayout(
        skill_name="craftingSeeds",
        cvar_prefix="craftingSeedsCheck",
        checklist_rect="checklistSeeds",
        zoomed_rect="zoomedSeeds",
        title_grid="checklistSeedsTitle",
    ),
    "craftingVehicles": PurpleBookSkillLayout(
        skill_name="craftingVehicles",
        cvar_prefix="craftingVehiclesCheck",
        checklist_rect="checklistVehicles",
        zoomed_rect="zoomedVehicles",
        title_grid="checklistVehiclesTitle",
    ),
    "craftingWorkstations": PurpleBookSkillLayout(
        skill_name="craftingWorkstations",
        cvar_prefix="craftingWorkstationsCheck",
        checklist_rect="checklistWorkstations",
        zoomed_rect="zoomedWorkstations",
        title_grid="checklistWorkstationsTitle",
    ),
    "craftingMedical": PurpleBookSkillLayout(
        skill_name="craftingMedical",
        cvar_prefix="craftingMedicalCheck",
        checklist_rect="checklistMedical",
        zoomed_rect="zoomedMedical",
        title_grid="checklistMedicalTitle",
    ),
}

_MAGAZINE_DECOR_ICON_TO_SKILL: dict[str, str] = {
    "bookSouthernFarming": "craftingSeeds",
    "bookVehicleAdventures": "craftingVehicles",
    "bookForgeAhead": "craftingWorkstations",
    "bookMedicalJournal": "craftingMedical",
}

_ZOOMED_LABEL_ATTRS = ("pos", "width", "height", "justify", "font_size", "text_key")
_ZOOMED_GRID_ATTRS = (
    "rows",
    "cols",
    "pos",
    "cell_width",
    "cell_height",
    "repeat_content",
    "arrangement",
    "controller",
)


def _first_xpath_text(node: etree._Element, xpath: str) -> str:
    vals = node.xpath(xpath)
    if not vals:
        return ""
    first = vals[0]
    return str(first).strip()


def _layout_label_name(layout: PurpleBookSkillLayout) -> str:
    return f"{layout.checklist_rect}MagName"


def _extract_magazine_decor_entries(tree: etree._ElementTree) -> list[dict[str, str]]:
    root = tree.getroot()
    entries = root.xpath(
        "//window[@name='Schematics']//rect[@name='craftinglistTab']//grid[@name='magazineDecor']/entry"
    )
    out: list[dict[str, str]] = []
    for idx, entry in enumerate(entries, start=1):
        out.append(
            {
                "index": str(idx),
                "icon_sprite": _first_xpath_text(entry, "./rect/sprite[@name='itemIcon'][1]/@sprite"),
                "icon_tooltip": _first_xpath_text(entry, "./rect/sprite[@name='itemIcon'][1]/@tooltip_key"),
                "label1_text": _first_xpath_text(entry, "./rect/label[1]/@text"),
                "label2_text": _first_xpath_text(entry, "./rect/label[2]/@text"),
                "fill": _first_xpath_text(entry, "./rect/filledsprite[@name='yesUnlocked']/@fill"),
            }
        )
    return out


def _extract_zoomed_slot_order(tree: etree._ElementTree) -> list[str]:
    root = tree.getroot()
    nodes = root.xpath(
        "//window[@name='Schematics']//rect[@name='craftinglistTab']//rect[@name='tabsContents']/rect[@controller='TabSelectorTab' and @tab_key='']"
    )
    out: list[str] = []
    for node in nodes:
        name = (node.get("name") or "").strip()
        if name:
            out.append(name)
    return out


def _parse_first_level(level_attr: str) -> int | None:
    try:
        return int((level_attr or "").split(",")[0].strip())
    except Exception:
        return None


def _extract_skill_sections(
    tree: etree._ElementTree,
    skill_name: str,
) -> tuple[int, list[tuple[int, list[str]]]] | None:
    root = tree.getroot()
    skill = root.find(f".//crafting_skill[@name='{skill_name}']")
    if skill is None:
        return None

    try:
        max_level = int((skill.get("max_level") or "100").strip())
    except Exception:
        max_level = 100

    sections: list[tuple[int, list[str]]] = []
    for pe in skill.iter("passive_effect"):
        if (pe.get("name") or "") != "RecipeTagUnlocked":
            continue
        if (pe.get("operation") or "") != "base_set":
            continue
        lvl = _parse_first_level(pe.get("level") or "")
        if lvl is None:
            continue
        tags = [t.strip() for t in (pe.get("tags") or "").split(",") if t.strip()]
        if not tags:
            continue
        sections.append((lvl, tags))

    # Keep source order from progression.xml so section ordering follows the
    # mod-authored progression rows, not a synthetic sort.
    return max_level, sections


def _render_windows_skill_rewrite(
    layout: PurpleBookSkillLayout,
    base_sections: list[tuple[int, list[str]]],
    max_level: int,
    sections: list[tuple[int, list[str]]],
    *,
    max_icon_slots: int = 8,
) -> list[str]:
    lines: list[str] = []

    base_count = len(base_sections)
    dump_count = len(sections)
    shared_count = min(base_count, dump_count)

    for idx in range(1, shared_count + 1):
        lvl, tags = sections[idx - 1]
        base_tags = base_sections[idx - 1][1]
        base_icon_count = max(1, min(max_icon_slots, len(base_tags)))
        dump_icon_count = max(1, min(max_icon_slots, len(tags)))
        icon_rewrite_count = max(base_icon_count, dump_icon_count)
        level_cvar = f"{layout.cvar_prefix}{lvl}"

        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/filledsprite[@name='yesUnlocked']/@fill\">{{cvar({level_cvar})}}</set>"
        )
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/label[@name='checkunlock']/@text\">{lvl}</set>"
        )
        for icon_idx, tag in enumerate(tags[:max_icon_slots], start=1):
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/sprite[@name='itemIcon'][{icon_idx}]/@sprite\">{tag}</set>"
            )
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/sprite[@name='itemIcon'][{icon_idx}]/@tooltip_key\">{tag}</set>"
            )
        # Clean stale icons if dump removed entries from this section.
        for icon_idx in range(dump_icon_count + 1, icon_rewrite_count + 1):
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/sprite[@name='itemIcon'][{icon_idx}]\"/>"
            )
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/sprite[@name='itemtypeicon'][{icon_idx}]\"/>"
            )

    lines.append("")

    for idx in range(1, shared_count + 1):
        lvl, tags = sections[idx - 1]
        base_tags = base_sections[idx - 1][1]
        base_icon_count = max(1, min(max_icon_slots, len(base_tags)))
        dump_icon_count = max(1, min(max_icon_slots, len(tags)))
        icon_rewrite_count = max(base_icon_count, dump_icon_count)
        level_cvar = f"{layout.cvar_prefix}{lvl}"

        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/filledsprite[@name='yesUnlocked']/@fill\">{{cvar({level_cvar})}}</set>"
        )
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/label[@name='checkunlock']/@text\">{lvl}</set>"
        )
        for icon_idx, tag in enumerate(tags[:max_icon_slots], start=1):
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/sprite[@name='itemIcon'][{icon_idx}]/@sprite\">{tag}</set>"
            )
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/sprite[@name='itemIcon'][{icon_idx}]/@tooltip_key\">{tag}</set>"
            )
        for icon_idx in range(dump_icon_count + 1, icon_rewrite_count + 1):
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/sprite[@name='itemIcon'][{icon_idx}]\"/>"
            )
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/sprite[@name='itemtypeicon'][{icon_idx}]\"/>"
            )

    # Remove stale sections/entries when the dump progression has fewer rows.
    if dump_count < base_count:
        for idx in range(dump_count + 1, base_count + 1):
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']\"/>"
            )
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]\"/>"
            )

    max_cvar = f"{layout.cvar_prefix}{max_level}"
    lines.append(
        f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.title_grid}']/entry[@name='Magazine']/filledsprite[@name='yesUnlocked']/@fill\">{{cvar({max_cvar})}}</set>"
    )
    lines.append(
        f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.title_grid}']/entry[@name='level']/label[2]/@text\">/{max_level}</set>"
    )
    return lines


def _render_buffs_new_level_cvars(
    layout: PurpleBookSkillLayout,
    levels: list[int],
) -> tuple[list[str], list[str]]:
    startup_lines: list[str] = []
    update_lines: list[str] = []
    progress_cvar = layout.cvar_prefix

    for lvl in levels:
        level_cvar = f"{layout.cvar_prefix}{lvl}"
        startup_lines.append(
            f"\t\t\t\t<triggered_effect trigger=\"onSelfBuffStart\" action=\"ModifyCVar\" cvar=\"{level_cvar}\" operation=\"set\" value=\"0\"/>"
        )
        update_lines.append(
            f"\t\t\t\t<triggered_effect trigger=\"onSelfBuffUpdate\" action=\"ModifyCVar\" cvar=\"{level_cvar}\" operation=\"set\" value=\"1\"><requirement name=\"CVarCompare\" cvar=\"{progress_cvar}\" operation=\"GTE\" value=\"{lvl}\"/><requirement name=\"CVarCompare\" cvar=\"{level_cvar}\" operation=\"LT\" value=\"1\"/></triggered_effect>"
        )
        update_lines.append(
            f"\t\t\t\t<triggered_effect trigger=\"onSelfBuffUpdate\" action=\"ModifyCVar\" cvar=\"{level_cvar}\" operation=\"set\" value=\"0\"><requirement name=\"CVarCompare\" cvar=\"{progress_cvar}\" operation=\"LT\" value=\"{lvl}\"/><requirement name=\"CVarCompare\" cvar=\"{level_cvar}\" operation=\"GTE\" value=\"1\"/></triggered_effect>"
        )

    return startup_lines, update_lines


def _safe_int(text: str | None) -> int | None:
    if text is None:
        return None
    try:
        return int(str(text).strip())
    except Exception:
        return None


def _extract_skill_sections_from_generated_windows(
    tree: etree._ElementTree,
    layout: PurpleBookSkillLayout,
) -> tuple[int, list[tuple[int, list[tuple[str, str]]]]] | None:
    root = tree.getroot()
    sections: list[tuple[int, list[tuple[str, str]]]] = []
    idx = 1
    while True:
        sec_nodes = root.xpath(
            f"//window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']"
        )
        if not sec_nodes:
            break

        sec = sec_nodes[0]
        lvl_text = (sec.xpath("./label[@name='checkunlock']/@text") or [""])[0]
        lvl = _safe_int(lvl_text)
        if lvl is not None:
            icons: list[tuple[str, str]] = []
            for icon in sec.xpath("./sprite[@name='itemIcon']"):
                sprite = (icon.get("sprite") or "").strip()
                tooltip = (icon.get("tooltip_key") or sprite).strip()
                if sprite or tooltip:
                    icons.append((sprite, tooltip))
            sections.append((lvl, icons))
        idx += 1

    if not sections:
        return None

    title_text = (
        root.xpath(
            f"//window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.title_grid}']/entry[@name='level']/label[2]/@text"
        )
        or [""]
    )[0].strip()
    max_level = _safe_int(title_text.lstrip("/"))
    if max_level is None:
        max_level = max(lvl for lvl, _ in sections)

    return max_level, sections


def _extract_zoomed_layout_payload(
    tree: etree._ElementTree,
    layout: PurpleBookSkillLayout,
) -> dict[str, object] | None:
    root = tree.getroot()
    label_name = _layout_label_name(layout)

    label_nodes = root.xpath(
        f"//window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/label[@name='{label_name}']"
    )
    title_nodes = root.xpath(
        f"//window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.title_grid}']"
    )
    checklist_nodes = root.xpath(
        f"//window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']"
    )
    if not label_nodes or not title_nodes or not checklist_nodes:
        return None

    label = label_nodes[0]
    title_grid = title_nodes[0]
    checklist_grid = checklist_nodes[0]

    return {
        "label_attrs": {
            a: (label.get(a) or "").strip()
            for a in _ZOOMED_LABEL_ATTRS
        },
        "title_grid_attrs": {
            a: (title_grid.get(a) or "").strip()
            for a in _ZOOMED_GRID_ATTRS
        },
        "checklist_grid_attrs": {
            a: (checklist_grid.get(a) or "").strip()
            for a in _ZOOMED_GRID_ATTRS
        },
        "title_fill": _first_xpath_text(
            title_grid,
            "./entry[@name='Magazine']/filledsprite[@name='yesUnlocked']/@fill",
        ),
        "title_icon_sprite": _first_xpath_text(
            title_grid,
            "./entry[@name='Magazine']/sprite[@name='itemIcon']/@sprite",
        ),
        "title_icon_tooltip": _first_xpath_text(
            title_grid,
            "./entry[@name='Magazine']/sprite[@name='itemIcon']/@tooltip_key",
        ),
        "level1_text": _first_xpath_text(
            title_grid,
            "./entry[@name='level']/label[1]/@text",
        ),
        "level1_tooltip": _first_xpath_text(
            title_grid,
            "./entry[@name='level']/label[1]/@tooltip_key",
        ),
        "level2_text": _first_xpath_text(
            title_grid,
            "./entry[@name='level']/label[2]/@text",
        ),
        "level2_tooltip": _first_xpath_text(
            title_grid,
            "./entry[@name='level']/label[2]/@tooltip_key",
        ),
        "checklist_entries_xml": [
            etree.tostring(e, encoding="unicode").strip()
            for e in checklist_grid.xpath("./entry")
        ],
    }


def _render_zoomed_slot_remap(
    target_layout: PurpleBookSkillLayout,
    source_skill: str,
    payload: dict[str, object],
    slot_index: int,
) -> list[str]:
    lines: list[str] = []
    lines.append(
        f"\t\t<!-- slot {slot_index}: remap {target_layout.skill_name} slot to {source_skill} content -->"
    )

    label_name = _layout_label_name(target_layout)
    label_attrs: dict[str, str] = payload.get("label_attrs", {})  # type: ignore[assignment]
    for attr, value in label_attrs.items():
        if value:
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/label[@name='{label_name}']/@{attr}\">{value}</set>"
            )

    title_grid_attrs: dict[str, str] = payload.get("title_grid_attrs", {})  # type: ignore[assignment]
    for attr, value in title_grid_attrs.items():
        if value:
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/@{attr}\">{value}</set>"
            )

    checklist_grid_attrs: dict[str, str] = payload.get("checklist_grid_attrs", {})  # type: ignore[assignment]
    for attr, value in checklist_grid_attrs.items():
        if value:
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.checklist_rect}']/@{attr}\">{value}</set>"
            )

    title_fill = str(payload.get("title_fill", "")).strip()
    if title_fill:
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/entry[@name='Magazine']/filledsprite[@name='yesUnlocked']/@fill\">{title_fill}</set>"
        )

    title_icon_sprite = str(payload.get("title_icon_sprite", "")).strip()
    if title_icon_sprite:
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/entry[@name='Magazine']/sprite[@name='itemIcon']/@sprite\">{title_icon_sprite}</set>"
        )
    title_icon_tooltip = str(payload.get("title_icon_tooltip", "")).strip()
    if title_icon_tooltip:
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/entry[@name='Magazine']/sprite[@name='itemIcon']/@tooltip_key\">{title_icon_tooltip}</set>"
        )

    level1_text = str(payload.get("level1_text", "")).strip()
    if level1_text:
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/entry[@name='level']/label[1]/@text\">{level1_text}</set>"
        )
    level1_tooltip = str(payload.get("level1_tooltip", "")).strip()
    if level1_tooltip:
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/entry[@name='level']/label[1]/@tooltip_key\">{level1_tooltip}</set>"
        )
    level2_text = str(payload.get("level2_text", "")).strip()
    if level2_text:
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/entry[@name='level']/label[2]/@text\">{level2_text}</set>"
        )
    level2_tooltip = str(payload.get("level2_tooltip", "")).strip()
    if level2_tooltip:
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.title_grid}']/entry[@name='level']/label[2]/@tooltip_key\">{level2_tooltip}</set>"
        )

    lines.append(
        f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.checklist_rect}']/entry\"/>"
    )

    entries_xml: list[str] = payload.get("checklist_entries_xml", [])  # type: ignore[assignment]
    if entries_xml:
        lines.append(
            f"\t\t\t<append xpath=\"/windows/window[@name='Schematics']//rect[@name='{target_layout.zoomed_rect}']/grid[@name='{target_layout.checklist_rect}']\">"
        )
        for entry_xml in entries_xml:
            lines.append(f"\t\t\t\t{entry_xml}")
        lines.append("\t\t\t</append>")

    return lines


def _render_slot_alignment_from_generator_diff(
    vanilla_windows_tree: etree._ElementTree,
    dump_windows_tree: etree._ElementTree,
    wanted_skills: list[str],
) -> tuple[list[str], list[str], list[str]]:
    lines: list[str] = []
    notes: list[str] = []
    warnings: list[str] = []

    base_entries = _extract_magazine_decor_entries(vanilla_windows_tree)
    dump_entries = _extract_magazine_decor_entries(dump_windows_tree)
    if not base_entries or not dump_entries:
        warnings.append("slot-alignment: missing magazineDecor entries in generated windows output.")
        return lines, notes, warnings

    base_zoomed_slots = _extract_zoomed_slot_order(vanilla_windows_tree)
    if not base_zoomed_slots:
        warnings.append("slot-alignment: missing zoomed slot order under craftinglistTab/tabsContents.")
        return lines, notes, warnings

    wanted = {s.strip() for s in wanted_skills if s.strip()}
    # Seeds slot movement has repeatedly required medical slot remap pairing.
    if "craftingSeeds" in wanted:
        wanted.add("craftingMedical")

    slot_count = min(len(base_entries), len(dump_entries), len(base_zoomed_slots))
    if slot_count == 0:
        return lines, notes, warnings

    rect_to_skill = {layout.zoomed_rect: skill for skill, layout in PURPLEBOOK_SKILL_LAYOUTS.items()}
    payload_cache: dict[str, dict[str, object]] = {}

    if (len(base_entries) != len(dump_entries)) or (len(base_entries) != len(base_zoomed_slots)):
        warnings.append(
            "slot-alignment: slot counts differ across vanilla/dump/top-strip; applied to shared range only."
        )

    for idx in range(1, slot_count + 1):
        base_entry = base_entries[idx - 1]
        dump_entry = dump_entries[idx - 1]

        base_skill = _MAGAZINE_DECOR_ICON_TO_SKILL.get(base_entry.get("icon_sprite", ""), "")
        dump_skill = _MAGAZINE_DECOR_ICON_TO_SKILL.get(dump_entry.get("icon_sprite", ""), "")

        if not ({base_skill, dump_skill} & wanted):
            continue

        entry_path = (
            "/windows/window[@name='Schematics']//rect[@name='craftinglistTab']"
            f"//grid[@name='magazineDecor']/entry[{idx}]/rect"
        )

        if base_entry.get("icon_sprite", "") != dump_entry.get("icon_sprite", "") and dump_entry.get("icon_sprite", ""):
            lines.append(
                f"\t\t\t<set xpath=\"{entry_path}/sprite[@name='itemIcon']/@sprite\">{dump_entry['icon_sprite']}</set>"
            )
        if base_entry.get("icon_tooltip", "") != dump_entry.get("icon_tooltip", "") and dump_entry.get("icon_tooltip", ""):
            lines.append(
                f"\t\t\t<set xpath=\"{entry_path}/sprite[@name='itemIcon']/@tooltip_key\">{dump_entry['icon_tooltip']}</set>"
            )
        if base_entry.get("label1_text", "") != dump_entry.get("label1_text", "") and dump_entry.get("label1_text", ""):
            lines.append(
                f"\t\t\t<set xpath=\"{entry_path}/label[1]/@text\">{dump_entry['label1_text']}</set>"
            )
        if base_entry.get("label2_text", "") != dump_entry.get("label2_text", "") and dump_entry.get("label2_text", ""):
            lines.append(
                f"\t\t\t<set xpath=\"{entry_path}/label[2]/@text\">{dump_entry['label2_text']}</set>"
            )
        if base_entry.get("fill", "") != dump_entry.get("fill", "") and dump_entry.get("fill", ""):
            lines.append(
                f"\t\t\t<set xpath=\"{entry_path}/filledsprite[@name='yesUnlocked']/@fill\">{dump_entry['fill']}</set>"
            )

        target_rect = base_zoomed_slots[idx - 1]
        target_skill = rect_to_skill.get(target_rect, "")
        if not target_skill or not dump_skill or target_skill == dump_skill:
            continue
        if not ({target_skill, dump_skill} & wanted):
            continue

        target_layout = PURPLEBOOK_SKILL_LAYOUTS.get(target_skill)
        source_layout = PURPLEBOOK_SKILL_LAYOUTS.get(dump_skill)
        if not target_layout or not source_layout:
            continue

        payload = payload_cache.get(dump_skill)
        if payload is None:
            payload = _extract_zoomed_layout_payload(dump_windows_tree, source_layout) or {}
            payload_cache[dump_skill] = payload
        if not payload:
            warnings.append(
                f"slot-alignment: missing source zoomed payload for {dump_skill}; skipped slot {idx}."
            )
            continue

        lines.extend(_render_zoomed_slot_remap(target_layout, dump_skill, payload, idx))
        notes.append(
            f"slot {idx}: {target_skill} slot remapped to {dump_skill} content"
        )

    if lines:
        lines.insert(0, "\t\t<!-- Auto-generated slot-alignment block: magazineDecor + zoomed slot remap -->")
        lines.append("")

    return lines, notes, warnings


def _append_icon_diffs(
    lines: list[str],
    warnings: list[str],
    base_icons: list[tuple[str, str]],
    dump_icons: list[tuple[str, str]],
    section_path: str,
    *,
    max_icon_slots: int,
) -> tuple[int, int]:
    set_ops = 0
    remove_ops = 0
    base_trim = base_icons[:max_icon_slots]
    dump_trim = dump_icons[:max_icon_slots]
    shared = min(len(base_trim), len(dump_trim))

    for icon_idx in range(1, shared + 1):
        base_sprite, base_tooltip = base_trim[icon_idx - 1]
        dump_sprite, dump_tooltip = dump_trim[icon_idx - 1]
        if base_sprite != dump_sprite and dump_sprite:
            lines.append(
                f"\t\t\t<set xpath=\"{section_path}/sprite[@name='itemIcon'][{icon_idx}]/@sprite\">{dump_sprite}</set>"
            )
            set_ops += 1
        if base_tooltip != dump_tooltip and dump_tooltip:
            lines.append(
                f"\t\t\t<set xpath=\"{section_path}/sprite[@name='itemIcon'][{icon_idx}]/@tooltip_key\">{dump_tooltip}</set>"
            )
            set_ops += 1

    for icon_idx in range(shared + 1, len(dump_trim) + 1):
        dump_sprite, dump_tooltip = dump_trim[icon_idx - 1]
        if dump_sprite:
            lines.append(
                f"\t\t\t<set xpath=\"{section_path}/sprite[@name='itemIcon'][{icon_idx}]/@sprite\">{dump_sprite}</set>"
            )
            set_ops += 1
        if dump_tooltip:
            lines.append(
                f"\t\t\t<set xpath=\"{section_path}/sprite[@name='itemIcon'][{icon_idx}]/@tooltip_key\">{dump_tooltip}</set>"
            )
            set_ops += 1
        warnings.append(
            f"{section_path}: dump has itemIcon[{icon_idx}] beyond vanilla output; verify the node exists in baseline before relying on <set>."
        )

    for icon_idx in range(len(dump_trim) + 1, len(base_trim) + 1):
        lines.append(
            f"\t\t\t<remove xpath=\"{section_path}/sprite[@name='itemIcon'][{icon_idx}]\"/>"
        )
        lines.append(
            f"\t\t\t<remove xpath=\"{section_path}/sprite[@name='itemtypeicon'][{icon_idx}]\"/>"
        )
        remove_ops += 2

    return set_ops, remove_ops


def _render_windows_skill_rewrite_diff(
    layout: PurpleBookSkillLayout,
    base_sections: list[tuple[int, list[tuple[str, str]]]],
    base_max: int,
    dump_sections: list[tuple[int, list[tuple[str, str]]]],
    dump_max: int,
    *,
    max_icon_slots: int = 8,
) -> tuple[list[str], dict[str, int], list[str]]:
    lines: list[str] = []
    warnings: list[str] = []
    stats = {"set": 0, "remove": 0, "warnings": 0}

    base_count = len(base_sections)
    dump_count = len(dump_sections)
    shared_count = min(base_count, dump_count)

    for idx in range(1, shared_count + 1):
        base_lvl, base_icons = base_sections[idx - 1]
        dump_lvl, dump_icons = dump_sections[idx - 1]

        if base_lvl != dump_lvl:
            level_cvar = f"{layout.cvar_prefix}{dump_lvl}"
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/filledsprite[@name='yesUnlocked']/@fill\">{{cvar({level_cvar})}}</set>"
            )
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']/label[@name='checkunlock']/@text\">{dump_lvl}</set>"
            )
            stats["set"] += 2

        checklist_path = (
            f"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']"
        )
        set_ops, remove_ops = _append_icon_diffs(
            lines,
            warnings,
            base_icons,
            dump_icons,
            checklist_path,
            max_icon_slots=max_icon_slots,
        )
        stats["set"] += set_ops
        stats["remove"] += remove_ops

    if shared_count > 0:
        lines.append("")

    for idx in range(1, shared_count + 1):
        base_lvl, base_icons = base_sections[idx - 1]
        dump_lvl, dump_icons = dump_sections[idx - 1]

        if base_lvl != dump_lvl:
            level_cvar = f"{layout.cvar_prefix}{dump_lvl}"
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/filledsprite[@name='yesUnlocked']/@fill\">{{cvar({level_cvar})}}</set>"
            )
            lines.append(
                f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]/label[@name='checkunlock']/@text\">{dump_lvl}</set>"
            )
            stats["set"] += 2

        zoomed_path = (
            f"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]"
        )
        set_ops, remove_ops = _append_icon_diffs(
            lines,
            warnings,
            base_icons,
            dump_icons,
            zoomed_path,
            max_icon_slots=max_icon_slots,
        )
        stats["set"] += set_ops
        stats["remove"] += remove_ops

    if dump_count < base_count:
        for idx in range(dump_count + 1, base_count + 1):
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.checklist_rect}']/rect[@name='Section{idx}']\"/>"
            )
            lines.append(
                f"\t\t\t<remove xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.checklist_rect}']/entry[{idx}]\"/>"
            )
            stats["remove"] += 2
    elif dump_count > base_count:
        warnings.append(
            f"{layout.skill_name}: dump output has {dump_count - base_count} extra section(s) beyond vanilla output; structural append entries may be required."
        )

    if base_max != dump_max:
        max_cvar = f"{layout.cvar_prefix}{dump_max}"
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.title_grid}']/entry[@name='Magazine']/filledsprite[@name='yesUnlocked']/@fill\">{{cvar({max_cvar})}}</set>"
        )
        lines.append(
            f"\t\t\t<set xpath=\"/windows/window[@name='Schematics']//rect[@name='{layout.zoomed_rect}']/grid[@name='{layout.title_grid}']/entry[@name='level']/label[2]/@text\">/{dump_max}</set>"
        )
        stats["set"] += 2

    stats["warnings"] = len(warnings)
    return lines, stats, warnings


def _out_mod_rel_to_abs(out_mod_rel: str) -> str:
    parts = [p for p in out_mod_rel.replace("\\", "/").split("/") if p]
    return os.path.join(GENERATOR_OUT_BASE_DIR, *parts)


def _run_source_generator_for_compare(
    config_dir: str,
    vanilla_dir: str,
    out_mod_rel: str,
) -> tuple[str, str] | None:
    if not os.path.exists(SOURCE_PB_GENERATOR_SCRIPT):
        print(f"!! source generator script not found: {SOURCE_PB_GENERATOR_SCRIPT}", file=sys.stderr)
        return None

    required = ("progression.xml", "qualityinfo.xml", "items.xml", "blocks.xml", "recipes.xml")
    for file_name in required:
        p = os.path.join(config_dir, file_name)
        if not os.path.exists(p):
            print(f"!! missing generator input file: {p}", file=sys.stderr)
            return None

    game_loc = os.path.join(vanilla_dir, "Localization.csv")
    if not os.path.exists(game_loc):
        print(f"!! missing vanilla Localization.csv for source generator: {game_loc}", file=sys.stderr)
        return None

    out_mod_abs = _out_mod_rel_to_abs(out_mod_rel)
    if os.path.isdir(out_mod_abs):
        shutil.rmtree(out_mod_abs, ignore_errors=True)
    os.makedirs(out_mod_abs, exist_ok=True)

    cmd = [
        sys.executable,
        SOURCE_PB_GENERATOR_SCRIPT,
        "--out-mod",
        out_mod_rel,
        "--progression",
        os.path.join(config_dir, "progression.xml"),
        "--qualityinfo",
        os.path.join(config_dir, "qualityinfo.xml"),
        "--items",
        os.path.join(config_dir, "items.xml"),
        "--blocks",
        os.path.join(config_dir, "blocks.xml"),
        "--recipes",
        os.path.join(config_dir, "recipes.xml"),
        "--game-localization",
        game_loc,
        "--no-sync-game-mod",
        "--no-sync-activebuild",
        "--no-autobackup",
    ]

    proc = subprocess.run(cmd, capture_output=True, text=True)
    if proc.returncode != 0:
        print("!! source generator run failed", file=sys.stderr)
        print(f"   config dir: {config_dir}", file=sys.stderr)
        print(f"   out-mod   : {out_mod_rel}", file=sys.stderr)
        if proc.stdout.strip():
            print("--- stdout ---", file=sys.stderr)
            print(proc.stdout.strip(), file=sys.stderr)
        if proc.stderr.strip():
            print("--- stderr ---", file=sys.stderr)
            print(proc.stderr.strip(), file=sys.stderr)
        return None

    windows_path = os.path.join(out_mod_abs, "Config", "XUi_InGame", "windows.xml")
    buffs_path = os.path.join(out_mod_abs, "Config", "buffs.xml")
    if not os.path.exists(windows_path):
        print(f"!! source generator did not produce windows.xml: {windows_path}", file=sys.stderr)
        return None
    if not os.path.exists(buffs_path):
        print(f"!! source generator did not produce buffs.xml: {buffs_path}", file=sys.stderr)
        return None
    return windows_path, buffs_path


def dump_generate_purplebook_rewrite_from_generator(
    dump_dir: str,
    vanilla_dir: str,
    skills_csv: str,
) -> int:
    if not os.path.isdir(dump_dir):
        print(f"!! dump dir not found: {dump_dir}", file=sys.stderr)
        return 2
    if not os.path.isdir(vanilla_dir):
        print(f"!! vanilla dir not found: {vanilla_dir}", file=sys.stderr)
        return 2

    wanted_skills = [s.strip() for s in skills_csv.split(",") if s.strip()]
    if not wanted_skills:
        print("!! no skills requested", file=sys.stderr)
        return 2

    print("\n========== GENERATED PURPLE BOOK REWRITE FROM SOURCE GENERATOR OUTPUT DIFF ==========")
    print(f"  vanilla dir: {vanilla_dir}")
    print(f"  dump dir   : {dump_dir}")
    print(f"  source generator: {SOURCE_PB_GENERATOR_SCRIPT}")

    vanilla_out = _run_source_generator_for_compare(
        config_dir=vanilla_dir,
        vanilla_dir=vanilla_dir,
        out_mod_rel=FULLGEN_COMPARE_VANILLA_OUT_MOD,
    )
    if not vanilla_out:
        return 2

    dump_out = _run_source_generator_for_compare(
        config_dir=dump_dir,
        vanilla_dir=vanilla_dir,
        out_mod_rel=FULLGEN_COMPARE_DUMP_OUT_MOD,
    )
    if not dump_out:
        return 2

    vanilla_windows_path, _vanilla_buffs_path = vanilla_out
    dump_windows_path, _dump_buffs_path = dump_out

    print(f"  generated vanilla windows: {vanilla_windows_path}")
    print(f"  generated dump windows   : {dump_windows_path}")

    vanilla_windows_tree = load_xml(vanilla_windows_path)
    dump_windows_tree = load_xml(dump_windows_path)

    windows_lines: list[str] = []
    buffs_startup: list[str] = []
    buffs_updates: list[str] = []
    changed_skills: list[str] = []
    warnings: list[str] = []

    for skill in wanted_skills:
        layout = PURPLEBOOK_SKILL_LAYOUTS.get(skill)
        if not layout:
            print(f"\n  !! unsupported skill for rewrite generator: {skill}")
            continue

        base_info = _extract_skill_sections_from_generated_windows(vanilla_windows_tree, layout)
        dump_info = _extract_skill_sections_from_generated_windows(dump_windows_tree, layout)
        if not base_info or not dump_info:
            print(f"\n  !! missing generated windows sections for skill: {skill}")
            continue

        base_max, base_sections = base_info
        dump_max, dump_sections = dump_info

        skill_lines, stats, skill_warnings = _render_windows_skill_rewrite_diff(
            layout,
            base_sections,
            base_max,
            dump_sections,
            dump_max,
        )

        base_levels = {lvl for lvl, _ in base_sections}
        dump_levels = {lvl for lvl, _ in dump_sections}
        new_levels = sorted(dump_levels - base_levels)

        if not skill_lines and not new_levels:
            print(f"\n  -- {skill}: no rewrite needed (source-generator output matches)")
            continue

        changed_skills.append(skill)
        print(f"\n  -- {skill}: rewrite needed")
        print(f"     vanilla max={base_max}, dump max={dump_max}")
        print(f"     vanilla sections={len(base_sections)}, dump sections={len(dump_sections)}")
        print(
            f"     operations: set={stats['set']} remove={stats['remove']} warnings={stats['warnings']}"
        )
        if new_levels:
            start, update = _render_buffs_new_level_cvars(layout, new_levels)
            buffs_startup.extend(start)
            buffs_updates.extend(update)
            print(f"     new cvar levels for buffs: {', '.join(str(v) for v in new_levels)}")
        else:
            print("     no new buff cvar levels needed")

        windows_lines.extend(skill_lines)
        windows_lines.append("")
        warnings.extend(skill_warnings)

    slot_lines, slot_notes, slot_warnings = _render_slot_alignment_from_generator_diff(
        vanilla_windows_tree,
        dump_windows_tree,
        wanted_skills,
    )
    if slot_lines:
        windows_lines.extend(slot_lines)
        print("\n  -- slot alignment rewrites generated")
        for note in slot_notes:
            print(f"     {note}")
    warnings.extend(slot_warnings)

    if not changed_skills and not slot_lines and not buffs_startup and not buffs_updates:
        print("\n  no rewrite blocks generated (all requested skills matched vanilla output)")
        return 0

    if windows_lines:
        print("\n========== WINDOWS CONDITIONAL (GENERATED) ==========")
        print("<conditional>")
        print("\t<if cond=\"mod_loaded('AGF-HUDPlus-PurpleBook')\">")
        print("\t\t<!-- Auto-generated from source-generator vanilla-vs-dump output diff (changed-only). -->")
        for line in windows_lines:
            print(line)
        print("\t</if>")
        print("</conditional>")

    if buffs_startup or buffs_updates:
        print("\n========== BUFFS CONDITIONAL (GENERATED, NEW LEVEL CVARS ONLY) ==========")
        print("<conditional>")
        print("\t<if cond=\"mod_loaded('AGF-HUDPlus-PurpleBook')\">")
        print("\t\t<insertafter xpath=\"/buffs/buff[@name='agfRecalculate']/effect_group[@name='recaculateChecklistPorgression']/triggered_effect[@cvar='craftingVehiclesCheck100']\">")
        for line in buffs_startup:
            print(line)
        print("\t\t</insertafter>")
        print("\t\t<append xpath=\"/buffs/buff[@name='buffStatusCheck02']/effect_group[@name='craftingChecker']\">")
        for line in buffs_updates:
            print(line)
        print("\t\t</append>")
        print("\t</if>")
        print("</conditional>")

    if warnings:
        print("\n========== NOTES ==========")
        seen: set[str] = set()
        for msg in warnings:
            if msg in seen:
                continue
            seen.add(msg)
            print(f"  - {msg}")

    return 0


def dump_generate_purplebook_rewrite(
    dump_dir: str,
    vanilla_dir: str,
    skills_csv: str,
) -> int:
    if not os.path.isdir(dump_dir):
        print(f"!! dump dir not found: {dump_dir}", file=sys.stderr)
        return 2
    if not os.path.isdir(vanilla_dir):
        print(f"!! vanilla dir not found: {vanilla_dir}", file=sys.stderr)
        return 2

    vanilla_progression = os.path.join(vanilla_dir, "progression.xml")
    dump_progression = os.path.join(dump_dir, "progression.xml")
    if not os.path.exists(vanilla_progression):
        print(f"!! missing vanilla progression.xml: {vanilla_progression}", file=sys.stderr)
        return 2
    if not os.path.exists(dump_progression):
        print(f"!! missing dump progression.xml: {dump_progression}", file=sys.stderr)
        return 2

    vanilla_tree = load_xml(vanilla_progression)
    dump_tree = load_xml(dump_progression)

    wanted_skills = [s.strip() for s in skills_csv.split(",") if s.strip()]
    if not wanted_skills:
        print("!! no skills requested", file=sys.stderr)
        return 2

    windows_lines: list[str] = []
    buffs_startup: list[str] = []
    buffs_updates: list[str] = []
    changed_skills: list[str] = []

    print("\n========== GENERATED PURPLE BOOK REWRITE FROM DUMP ==========")
    print(f"  vanilla dir: {vanilla_dir}")
    print(f"  dump dir   : {dump_dir}")

    for skill in wanted_skills:
        layout = PURPLEBOOK_SKILL_LAYOUTS.get(skill)
        if not layout:
            print(f"\n  !! unsupported skill for rewrite generator: {skill}")
            continue

        base_info = _extract_skill_sections(vanilla_tree, skill)
        dump_info = _extract_skill_sections(dump_tree, skill)
        if not base_info or not dump_info:
            print(f"\n  !! missing crafting_skill in progression.xml: {skill}")
            continue

        base_max, base_sections = base_info
        dump_max, dump_sections = dump_info

        if base_max == dump_max and base_sections == dump_sections:
            print(f"\n  -- {skill}: no progression rewrite needed")
            continue

        changed_skills.append(skill)
        print(f"\n  -- {skill}: rewrite needed")
        print(f"     vanilla max={base_max}, dump max={dump_max}")
        print(f"     vanilla sections={len(base_sections)}, dump sections={len(dump_sections)}")

        dump_levels_in_order = [lvl for lvl, _ in dump_sections]
        if dump_levels_in_order and len(set(dump_levels_in_order)) == 1 and len(dump_levels_in_order) > 1:
            print("     note: all section levels are identical; generator keeps per-section entries to preserve per-item visibility.")

        windows_lines.extend(_render_windows_skill_rewrite(layout, base_sections, dump_max, dump_sections))
        windows_lines.append("")

        base_levels = {lvl for lvl, _ in base_sections}
        dump_levels = {lvl for lvl, _ in dump_sections}
        new_levels = sorted(dump_levels - base_levels)
        if new_levels:
            start, update = _render_buffs_new_level_cvars(layout, new_levels)
            buffs_startup.extend(start)
            buffs_updates.extend(update)
            print(f"     new cvar levels for buffs: {', '.join(str(v) for v in new_levels)}")
        else:
            print("     no new buff cvar levels needed")

    if not changed_skills:
        print("\n  no rewrite blocks generated (all requested skills matched vanilla)")
        return 0

    print("\n========== WINDOWS CONDITIONAL (GENERATED) ==========")
    print("<conditional>")
    print("\t<if cond=\"mod_loaded('AGF-HUDPlus-PurpleBook')\">")
    print("\t\t<!-- Auto-generated from progression dump diff. Rewrites section levels/icons and removes stale sections/icons. -->")
    for line in windows_lines:
        print(line)
    print("\t</if>")
    print("</conditional>")

    if buffs_startup or buffs_updates:
        print("\n========== BUFFS CONDITIONAL (GENERATED, NEW LEVEL CVARS ONLY) ==========")
        print("<conditional>")
        print("\t<if cond=\"mod_loaded('AGF-HUDPlus-PurpleBook')\">")
        print("\t\t<insertafter xpath=\"/buffs/buff[@name='agfRecalculate']/effect_group[@name='recaculateChecklistPorgression']/triggered_effect[@cvar='craftingVehiclesCheck100']\">")
        for line in buffs_startup:
            print(line)
        print("\t\t</insertafter>")
        print("\t\t<append xpath=\"/buffs/buff[@name='buffStatusCheck02']/effect_group[@name='craftingChecker']\">")
        for line in buffs_updates:
            print(line)
        print("\t\t</append>")
        print("\t</if>")
        print("</conditional>")

    return 0


def dump_diff_purplebook_report(dump_dir: str, vanilla_dir: str, detail_limit: int = 20) -> int:
    if not os.path.isdir(dump_dir):
        print(f"!! dump dir not found: {dump_dir}", file=sys.stderr)
        return 2
    if not os.path.isdir(vanilla_dir):
        print(f"!! vanilla dir not found: {vanilla_dir}", file=sys.stderr)
        return 2

    print("\n========== PURPLE BOOK DIFF (vanilla -> merged dump) ==========")
    print("  scope: Purple-Book-relevant fields only")
    print(f"  vanilla dir: {vanilla_dir}")
    print(f"  dump dir   : {dump_dir}")

    required = ("progression.xml", "items.xml", "recipes.xml", "item_modifiers.xml")
    vanilla_trees: dict[str, etree._ElementTree] = {}
    dump_trees: dict[str, etree._ElementTree] = {}

    for file_name in required:
        vanilla_path = os.path.join(vanilla_dir, file_name)
        dump_path = os.path.join(dump_dir, file_name)

        if not os.path.exists(vanilla_path):
            print(f"\n  !! missing vanilla file: {vanilla_path}")
            continue
        if not os.path.exists(dump_path):
            print(f"\n  !! missing dump file: {dump_path}")
            continue
        try:
            vanilla_trees[file_name] = load_xml(vanilla_path)
            dump_trees[file_name] = load_xml(dump_path)
        except Exception as ex:
            print(f"\n  !! failed to parse {file_name}: {ex}")

    total_changed = 0
    total_added = 0
    total_removed = 0

    if "progression.xml" in vanilla_trees and "progression.xml" in dump_trees:
        base = _progression_purplebook_projection(vanilla_trees["progression.xml"])
        after = _progression_purplebook_projection(dump_trees["progression.xml"])
        changed, added, removed = _diff_projection(base, after)
        total_changed += len(changed)
        total_added += len(added)
        total_removed += len(removed)
        print("\n  -- progression.xml (crafting_skill RecipeTagUnlocked + max_level) --")
        _print_key_bucket("changed", changed, detail_limit)
        _print_key_bucket("added", added, detail_limit)
        _print_key_bucket("removed", removed, detail_limit)
        markers = _mod_marker_counts(dump_trees["progression.xml"].getroot())
        top = ", ".join(f"{n}={c}" for n, c in list(markers.items())[:5]) if markers else "none"
        print(f"    marker mods: {top}")

    if (
        "items.xml" in vanilla_trees
        and "items.xml" in dump_trees
        and "recipes.xml" in vanilla_trees
        and "recipes.xml" in dump_trees
    ):
        base = _item_purplebook_projection(vanilla_trees["items.xml"], vanilla_trees["recipes.xml"])
        after = _item_purplebook_projection(dump_trees["items.xml"], dump_trees["recipes.xml"])
        changed, added, removed = _diff_projection(base, after)
        total_changed += len(changed)
        total_added += len(added)
        total_removed += len(removed)
        print("\n  -- items.xml (UnlockedBy + CustomIcon + Tags + craftable presence) --")
        _print_key_bucket("changed", changed, detail_limit)
        _print_key_bucket("added", added, detail_limit)
        _print_key_bucket("removed", removed, detail_limit)
        markers = _mod_marker_counts(dump_trees["items.xml"].getroot())
        top = ", ".join(f"{n}={c}" for n, c in list(markers.items())[:5]) if markers else "none"
        print(f"    marker mods: {top}")

    if "recipes.xml" in vanilla_trees and "recipes.xml" in dump_trees:
        base = _recipe_purplebook_projection(vanilla_trees["recipes.xml"])
        after = _recipe_purplebook_projection(dump_trees["recipes.xml"])
        changed, added, removed = _diff_projection(base, after)
        total_changed += len(changed)
        total_added += len(added)
        total_removed += len(removed)
        print("\n  -- recipes.xml (tags + learnable + always_unlocked) --")
        _print_key_bucket("changed", changed, detail_limit)
        _print_key_bucket("added", added, detail_limit)
        _print_key_bucket("removed", removed, detail_limit)
        markers = _mod_marker_counts(dump_trees["recipes.xml"].getroot())
        top = ", ".join(f"{n}={c}" for n, c in list(markers.items())[:5]) if markers else "none"
        print(f"    marker mods: {top}")

    if "item_modifiers.xml" in vanilla_trees and "item_modifiers.xml" in dump_trees:
        base = _item_modifier_purplebook_projection(vanilla_trees["item_modifiers.xml"])
        after = _item_modifier_purplebook_projection(dump_trees["item_modifiers.xml"])
        changed, added, removed = _diff_projection(base, after)
        total_changed += len(changed)
        total_added += len(added)
        total_removed += len(removed)
        print("\n  -- item_modifiers.xml (UnlockedBy only) --")
        _print_key_bucket("changed", changed, detail_limit)
        _print_key_bucket("added", added, detail_limit)
        _print_key_bucket("removed", removed, detail_limit)
        markers = _mod_marker_counts(dump_trees["item_modifiers.xml"].getroot())
        top = ", ".join(f"{n}={c}" for n, c in list(markers.items())[:5]) if markers else "none"
        print(f"    marker mods: {top}")

    print("\n  == purple-book totals ==")
    print(f"    changed: {total_changed}")
    print(f"    added  : {total_added}")
    print(f"    removed: {total_removed}")
    return 0


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def main(argv: Iterable[str]) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--dump-diff", action="store_true",
                    help="Compare a merged ConfigsDump folder vs vanilla and report changed/added/removed entities.")
    ap.add_argument("--dump-diff-purplebook", action="store_true",
                    help="Compare ConfigsDump vs vanilla using Purple-Book-relevant fields only.")
    ap.add_argument("--dump-generate-purplebook-rewrite", action="store_true",
                    help="Generate Purple Book windows/buffs conditional rewrite blocks from progression diff (vanilla vs ConfigsDump).")
    ap.add_argument("--dump-generate-purplebook-rewrite-from-generator", action="store_true",
                    help="Run source PurpleBook generator on vanilla + ConfigsDump and generate changed-only rewrite blocks from output diff.")
    ap.add_argument("--dump-dir", default=None,
                    help="Path to ConfigsDump folder for --dump-diff mode.")
    ap.add_argument("--vanilla-dir", default=VANILLA_CONFIG_DIR,
                    help="Vanilla Config directory for --dump-diff mode.")
    ap.add_argument("--skills", default="craftingWorkstations,craftingVehicles,craftingSeeds",
                    help="Comma-separated crafting skills for --dump-generate-purplebook-rewrite mode.")
    ap.add_argument("--detail-limit", type=int, default=20,
                    help="Max detail rows per bucket for --dump-diff (0 = counts only).")
    ap.add_argument("--report", action="store_true",
                    help="Read-only report mode (no XML written).")
    ap.add_argument("--cross-check", action="store_true",
                    help="Compare delta vs. existing handwritten compat windows.xml.")
    ap.add_argument("--draft", action="store_true",
                    help="Write a review YAML + Localization.txt draft to _GeneratedCompat/<ModName>.")
    ap.add_argument("--target", required=False,
                    help="Comma-separated list of mod folder names under "
                         f"{MODS_DIR}.  If omitted with --all, runs every "
                         "entry in TARGET_PROFILES.")
    ap.add_argument("--all", action="store_true",
                    help="Run against every mod listed in TARGET_PROFILES.")
    ap.add_argument("--handwritten", default=None,
                    help="Subfolder under zzzAGF-Special-Compatibilities ModPatches "
                         "to compare against (default = profile compat name or --target).")
    args = ap.parse_args(list(argv))

    if args.dump_diff_purplebook:
        if not args.dump_dir:
            print("--dump-dir is required with --dump-diff-purplebook.", file=sys.stderr)
            return 2
        return dump_diff_purplebook_report(
            dump_dir=args.dump_dir,
            vanilla_dir=args.vanilla_dir,
            detail_limit=max(args.detail_limit, 0),
        )

    if args.dump_generate_purplebook_rewrite:
        if not args.dump_dir:
            print("--dump-dir is required with --dump-generate-purplebook-rewrite.", file=sys.stderr)
            return 2
        return dump_generate_purplebook_rewrite(
            dump_dir=args.dump_dir,
            vanilla_dir=args.vanilla_dir,
            skills_csv=args.skills,
        )

    if args.dump_generate_purplebook_rewrite_from_generator:
        if not args.dump_dir:
            print("--dump-dir is required with --dump-generate-purplebook-rewrite-from-generator.", file=sys.stderr)
            return 2
        return dump_generate_purplebook_rewrite_from_generator(
            dump_dir=args.dump_dir,
            vanilla_dir=args.vanilla_dir,
            skills_csv=args.skills,
        )

    if args.dump_diff:
        if not args.dump_dir:
            print("--dump-dir is required with --dump-diff.", file=sys.stderr)
            return 2
        return dump_diff_report(
            dump_dir=args.dump_dir,
            vanilla_dir=args.vanilla_dir,
            detail_limit=max(args.detail_limit, 0),
        )

    if not (args.report or args.cross_check or args.draft):
        print("Use --dump-diff, --dump-diff-purplebook, --dump-generate-purplebook-rewrite, --dump-generate-purplebook-rewrite-from-generator, or Phase 1 modes: --report, --cross-check, and/or --draft.", file=sys.stderr)
        return 2

    if args.all:
        targets = list(TARGET_PROFILES.keys())
    elif args.target:
        targets = [t.strip() for t in args.target.split(",") if t.strip()]
    else:
        print("--target or --all is required.", file=sys.stderr)
        return 2

    if args.handwritten:
        handwritten_list = [s.strip() for s in args.handwritten.split(",")]
    else:
        handwritten_list = [TARGET_PROFILES.get(t, {}).get("compat", t) for t in targets]

    if args.report:
        for t in targets:
            report(t)
    if args.cross_check:
        for t, h in zip(targets, handwritten_list):
            cross_check(t, h)
    if args.draft:
        for t in targets:
            draft(t)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
