"""
Fix script for Config/XUi/windows.xml conversion.
The original script mis-handled nested <conditional> blocks.
This script:
  1. Extracts correct patch content for each nested-section mod from the backup
  2. Overwrites WMM12SlotToolbelt/windows.xml, creates 15SlotToolbelt/ and Dewtas18SlotToolbelt/
  3. Overwrites RecipeStatsTab/windows.xml
  4. Rewrites the dispatcher Config/XUi/windows.xml cleanly

Toolbelt mods are kept in a SINGLE <conditional> block (else-if chain) in the dispatcher,
preserving the original mutually-exclusive behavior.
"""

import re, os

BASE     = r"c:\GitHub\7D2D-Mods\02_ActiveBuild\zzzAGF-Special-Compatibilities-v4.1.1"
BACKUP   = os.path.join(BASE, "Config_BACKUP", "XUi", "windows.xml")
XUI_DIR  = os.path.join(BASE, "Config", "XUi")
PATCH_DIR = os.path.join(XUI_DIR, "ModPatches")

# ---------------------------------------------------------------------------
# Helper: extract balanced <if cond="...mod_name...">...</if>
# ---------------------------------------------------------------------------

def extract_if_block(text, mod_name):
    """Return the raw content INSIDE <if cond="...mod_name...">...</if>.
    Uses depth-tracking to handle nested <if> tags.
    Returns None if not found.
    """
    pat = re.compile(r'<if\s+cond="[^"]*' + re.escape(mod_name) + r'[^"]*">')
    m = pat.search(text)
    if not m:
        return None
    # inner content starts right after the opening <if...>
    inner_start = m.end()
    pos   = inner_start
    depth = 1
    while depth > 0 and pos < len(text):
        next_open  = text.find('<if ', pos)
        next_close = text.find('</if>', pos)
        if next_close == -1:
            raise ValueError(f"Unmatched <if> for mod '{mod_name}'")
        if next_open != -1 and next_open < next_close:
            depth += 1
            pos = next_open + 4
        else:
            depth -= 1
            if depth == 0:
                inner_end = next_close
                return text[inner_start:inner_end]
            pos = next_close + 5
    raise ValueError(f"Could not find matching </if> for mod '{mod_name}'")


# ---------------------------------------------------------------------------
# Read backup
# ---------------------------------------------------------------------------

with open(BACKUP, encoding="utf-8") as fh:
    backup_text = fh.read()

print("Backup loaded, length:", len(backup_text))


# ---------------------------------------------------------------------------
# Section 1: Toolbelt mods
# Each is gated by AGF-HUDPlus-1Main (outer) + individual mod (inner)
# Extract each mod's inner content and wrap with AGF-HUDPlus-1Main condition
# ---------------------------------------------------------------------------

TOOLBELT_MODS = [
    ("WMM12SlotToolbelt",   "AGF-HUDPlus-1Main"),
    ("15SlotToolbelt",      "AGF-HUDPlus-1Main"),
    ("Dewtas18SlotToolbelt","AGF-HUDPlus-1Main"),
]

for mod_name, agf_mod in TOOLBELT_MODS:
    inner = extract_if_block(backup_text, mod_name)
    if inner is None:
        print(f"  WARNING: Could not find <if> block for {mod_name}")
        continue

    patch_path = os.path.join(PATCH_DIR, mod_name)
    os.makedirs(patch_path, exist_ok=True)
    out_file = os.path.join(patch_path, "windows.xml")

    content = (
        f"<zzzAGF-Compatibilities>\n"
        f"<!-- {mod_name} -->\n"
        f"\n"
        f"<conditional>\n"
        f"\t<if cond=\"mod_loaded('{agf_mod}')\">"
        f"{inner}"
        f"</if>\n"
        f"</conditional>\n"
        f"\n"
        f"</zzzAGF-Compatibilities>"
    )
    with open(out_file, "w", encoding="utf-8") as fh:
        fh.write(content)
    print(f"  Written: ModPatches/{mod_name}/windows.xml")


# ---------------------------------------------------------------------------
# Section 2: RecipeStatsTab
# Gated by AGF-BackpackPlus (outer) + RecipeStatsTab (inner)
# Extract RecipeStatsTab content and wrap with BackpackPlus OR condition
# ---------------------------------------------------------------------------

backpack_cond = (
    "mod_loaded('AGF-BackpackPlus-060Slots') or "
    "mod_loaded('AGF-BackpackPlus-072Slots') or "
    "mod_loaded('AGF-BackpackPlus-084Slots') or "
    "mod_loaded('AGF-BackpackPlus-119Slots')"
)

recipe_inner = extract_if_block(backup_text, "RecipeStatsTab")
if recipe_inner is None:
    print("  WARNING: Could not find <if> block for RecipeStatsTab")
else:
    patch_path = os.path.join(PATCH_DIR, "RecipeStatsTab")
    os.makedirs(patch_path, exist_ok=True)
    out_file = os.path.join(patch_path, "windows.xml")

    content = (
        f"<zzzAGF-Compatibilities>\n"
        f"<!-- RecipeStatsTab -->\n"
        f"\n"
        f"<conditional>\n"
        f"\t<if cond=\"{backpack_cond}\">"
        f"{recipe_inner}"
        f"</if>\n"
        f"</conditional>\n"
        f"\n"
        f"</zzzAGF-Compatibilities>"
    )
    with open(out_file, "w", encoding="utf-8") as fh:
        fh.write(content)
    print("  Written: ModPatches/RecipeStatsTab/windows.xml")


# ---------------------------------------------------------------------------
# Section 3: Build the clean dispatcher windows.xml
#
# Toolbelt mods → single <conditional> with else-if chain (preserves mutual exclusion)
# All other mods → individual <conditional> blocks (as the script generated)
#
# Mods that already have correct patch files (from the original script run):
#   Bdubs_Vehicles, DishongTowerChallenge, IZY_*, IZY_melee, MoreQuestsNoScrollbar,
#   V2_OakravenAmmoPress, VanillaExtended, WMMVehicleCruiseControl
# Also now fixed: WMM12SlotToolbelt, 15SlotToolbelt, Dewtas18SlotToolbelt, RecipeStatsTab
# ---------------------------------------------------------------------------

NOTE = "<!--NOTE: In notepad++ I use \"linearize\" from the xml tool plugin to make a collection of code to go on a single line.-->"

TOOLBELT_NAMES = ["WMM12SlotToolbelt", "15SlotToolbelt", "Dewtas18SlotToolbelt"]

# All other mods with include entries (sorted as they currently appear + new ones)
OTHER_MODS = [
    "Bdubs_Vehicles",
    "DishongTowerChallenge",
    "IZY_RMP_44magnum",
    "IZY_RMP_45ACP",
    "IZY_RMP_556pack",
    "IZY_RMP_Demopack",
    "IZY_RMP_Miscpack",
    "IZY_RMP_SG",
    "IZY_RMP_Tech",
    "IZY_melee",
    "MoreQuestsNoScrollbar",
    "RecipeStatsTab",
    "V2_OakravenAmmoPress",
    "VanillaExtended",
    "WMMVehicleCruiseControl",
]

lines = ["<zzzAGF-Compatibilities>\n"]
lines.append(f"\n{NOTE}\n")

# Toolbelt else-if block
lines.append("\n<!-- Toolbelt Slots Mods & AGF HUDPlus-1Main -->\n")
lines.append("<conditional>\n")
for mod in TOOLBELT_NAMES:
    lines.append(f"\t<if cond=\"mod_loaded('{mod}')\">\n")
    lines.append(f"\t\t<include filename=\"ModPatches/{mod}/windows.xml\"/>\n")
    lines.append(f"\t</if>\n")
lines.append("</conditional>\n")

# Individual blocks for all other mods
for mod in OTHER_MODS:
    lines.append(f"\n<conditional>\n")
    lines.append(f"\t<if cond=\"mod_loaded('{mod}')\">\n")
    lines.append(f"\t\t<include filename=\"ModPatches/{mod}/windows.xml\"/>\n")
    lines.append(f"\t</if>\n")
    lines.append(f"</conditional>\n")

lines.append("\n</zzzAGF-Compatibilities>")

dispatcher_path = os.path.join(XUI_DIR, "windows.xml")
with open(dispatcher_path, "w", encoding="utf-8") as fh:
    fh.write("".join(lines))
print(f"\nDispatcher rewritten: Config/XUi/windows.xml")
print("Done.")
