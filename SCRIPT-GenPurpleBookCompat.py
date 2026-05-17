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
"""
from __future__ import annotations

import argparse
import os
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

# Items whose CustomIcon contains any of these substrings are considered
# "not ready for display" and excluded from generation.
NOT_READY_ICON_MARKERS = ("Notready", "NotReady", "not_ready", "NOTREADY")

# Files we read & patch (only the ones the Purple Book UI cares about).
TRACKED_FILES = ("progression.xml", "items.xml", "recipes.xml")

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
# CLI
# ---------------------------------------------------------------------------

def main(argv: Iterable[str]) -> int:
    ap = argparse.ArgumentParser()
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

    if not (args.report or args.cross_check or args.draft):
        print("Phase 1 supports --report, --cross-check, and/or --draft.", file=sys.stderr)
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
