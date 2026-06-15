"""
Simulates 7D2D Localization load sequence from BJAYLog to find the exact key
causing IndexOutOfRangeException in WriteCsv.
"""
import csv, io, os

MODS_ROOT = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods"
MODS_ROOT_ALT = r"D:\SteamLibrary\steamapps\common\7 Days To Die\Mods"
HEADER_KEY = "KEY"
USED_IN_MAIN_MENU_KEY = "usedinmainmenu"

# Exact load order from BJAYLog.txt
LOAD_ORDER = [
    "Gears",
    "Quartz",
    "PUBGPickup",
    "MoreDroneModifications",
    "AGF-HUDPlus-1Main",
    "AGF-NoEAC-AudioOptionsPlus",
    "AGF-NoEAC-AutoRun",
    "AGF-NoEAC-GlobalStormTracker",
    "AGF-NoEAC-OpenAllButton",
    "AGF-NoEAC-ScreamerAlert",
    "AGF-NoEAC-Toolbelt12Slots",
    "AGF-VP-AmmoDisassembly",
    "AGF-VP-ApiaryPlus",
    "AGF-VP-ArmorHarvestMods",
    "AGF-VP-AutomobilesRespawn",
    "AGF-VP-BedrollPlus",
    "AGF-VP-BreakItGetIt",
    "AGF-VP-BuyTraderVendingMachines",
    "AGF-VP-CraftSewingKits",
    "AGF-VP-CraftStackEngBattCells",
    "AGF-VP-CraftVitamins",
    "AGF-VP-DecorationBlock",
    "AGF-VP-DewsPlus",
    "AGF-VP-DoorsPlus",
    "AGF-VP-DrinkableAcid",
    "AGF-VP-DyesPlus",
    "AGF-VP-FloraHarvester",
    "AGF-VP-FuelBurnPlus",
    "AGF-VP-MasterTool",
    "AGF-VP-MedicationNoInsectSlow",
    "AGF-VP-MiningPlus",
    "AGF-VP-Mod988",
    "AGF-VP-ModBundling",
    "AGF-VP-PumpkinsPlus",
    "AGF-VP-RenamesAlphabeticalSort",
    "AGF-VP-RestorePowerAnyTime",
    "AGF-VP-ScrapBatts4Acid",
    "AGF-VP-SimplifiedStacks",
    "AGF-VP-SmeltingPlus",
    "AGF-VP-WriteStoryOnCrate",
    "Boat_Modlet_For_V2",
    "DrillingMachine",
    "FNK_UAZ452",
    "fs_brick_textures",
    "fs_cloth_and_carpet_texture",
    "fs_concrete_texture",
    "fs_earth_textures",
    "fs_metal_textures",
    "fs_plaster_and_paints_textures",
    "fs_tile_texture",
    "fs_wallpaper_textures",
    "fs_wood_texture",
    "Industrial_Generators",
    "LittleRedSonja_BarbieDavidson",
    "LittleRedSonja_ZombiePack",
    "MD-500",
    "GW71_FoundationBlock",
    "Telrics_Horses_V2",
    "War3zuk_FarmLife",
    "zzzAGF-Special-Compatibilities",
]

def find_loc_file(mod_name):
    """Find the Localization.txt for a given mod name in either mods root."""
    for mods_root in [MODS_ROOT, MODS_ROOT_ALT]:
        if not os.path.isdir(mods_root):
            continue
        for root, dirs, files in os.walk(mods_root):
            parts = root.lower()
            if "_backup" in parts or "_localization temp" in parts or "other mods" in parts:
                dirs[:] = []
                continue
            if "localization.txt" in [f.lower() for f in files]:
                if mod_name.lower() in root.lower():
                    full = os.path.join(root, "Localization.txt")
                    if os.path.exists(full):
                        return full
    return None

def read_csv_rows(filepath):
    rows = []
    try:
        with open(filepath, encoding="utf-8-sig", errors="replace") as f:
            content = f.read()
        reader = csv.reader(io.StringIO(content))
        for row in reader:
            if row:
                rows.append(row)
    except Exception as e:
        print(f"  ERROR reading {filepath}: {e}")
    return rows

def simulate_load_csv_patch(mod_name, filepath, m_dict, patched_cells):
    rows = read_csv_rows(filepath)
    if not rows or len(rows[0]) < 2:
        return []

    header_row = rows[0]
    array2 = list(m_dict[HEADER_KEY])
    new_length = len(array2)

    col_translation = [0] * len(header_row)
    new_cols = []
    for i in range(1, len(header_row)):
        col_name = header_row[i].strip()
        found = False
        for j, canon in enumerate(array2):
            if col_name.lower() == canon.lower():
                col_translation[i] = j
                found = True
                break
        if not found:
            col_translation[i] = new_length
            new_cols.append(col_name)
            new_length += 1

    if new_cols:
        print(f"  [{mod_name}] NEW columns introduced: {new_cols} -> newLength={new_length}")

    orig_col_uimm = -1
    for k, col in enumerate(array2):
        if col.lower() == USED_IN_MAIN_MENU_KEY:
            orig_col_uimm = k
            break

    mismatches = []

    for row in rows:
        if not row or not row[0]:
            continue
        key = row[0]

        if key in m_dict:
            value = list(m_dict[key])
            if len(value) < new_length:
                value = value + [""] * (new_length - len(value))

            if key in patched_cells:
                value2 = list(patched_cells[key])
                old_bool_len = len(value2)
                if len(value2) < new_length:
                    value2 = value2 + [False] * (new_length - len(value2))
            else:
                value2 = [False] * new_length
                old_bool_len = None

            for j in range(1, min(len(row), len(col_translation))):
                if row[j].strip():
                    idx = col_translation[j]
                    if idx < len(value):
                        value[idx] = row[j]
                    if idx < len(value2):
                        value2[idx] = True

            uimm_val = value[orig_col_uimm] if (orig_col_uimm >= 0 and orig_col_uimm < len(value)) else ""
            if not uimm_val:
                patched_cells[key] = value2
            else:
                # UsedInMainMenu is set - bool[] resize is discarded if key already in patchedCells
                if old_bool_len is not None and old_bool_len < new_length:
                    # The resized value2 is NOT saved back - mismatch created!
                    mismatches.append((key, old_bool_len, new_length, uimm_val))
            m_dict[key] = value
        else:
            value = [""] * new_length
            value2 = [False] * new_length
            for j in range(1, min(len(row), len(col_translation))):
                if row[j].strip():
                    idx = col_translation[j]
                    if idx < new_length:
                        value[idx] = row[j]
                        value2[idx] = True
            patched_cells[key] = value2
            m_dict[key] = value

    # Update canonical KEY array
    if new_cols:
        m_dict[HEADER_KEY] = array2 + new_cols

    return mismatches

def get_local_load_order():
    """Get mod folders from local Mods root in alphabetical order (how game loads them)."""
    skip = {"_backup", "_localization temp", "_myservertemp", "other mods",
            "server mods", "b19jay mod package", "update mod package", ".optionals"}
    results = []
    for entry in sorted(os.scandir(MODS_ROOT), key=lambda e: e.name.lower()):
        if not entry.is_dir():
            continue
        if any(s in entry.name.lower() for s in skip):
            continue
        loc = os.path.join(entry.path, "Config", "Localization.txt")
        if os.path.exists(loc):
            results.append((entry.name, loc))
    return results

def main():
    base_cols = ["File","Type","UsedInMainMenu","NoTranslate","english",
                 "Context / Alternate Text","german","spanish","french","italian",
                 "japanese","koreana","polish","brazilian","russian","turkish",
                 "schinese","tchinese"]

    # --- MINIMAL TEST: just FarmLife + RenamesAlphabeticalSort ---
    print("=== MINIMAL: RenamesAlphabeticalSort + War3zuk FarmLife ===")
    m_dict = {HEADER_KEY: base_cols[:]}
    patched_cells = {}
    minimal = [
        ("AGF-VP-RenamesAlphabeticalSort", find_loc_file("AGF-VP-RenamesAlphabeticalSort")),
        ("War3zuk FarmLife",               find_loc_file("War3zuk FarmLife")),
    ]
    for mod_name, loc_file in minimal:
        if not loc_file:
            print(f"[{mod_name}] NOT FOUND"); continue
        print(f"Loading [{mod_name}] from {loc_file}")
        mismatches = simulate_load_csv_patch(mod_name, loc_file, m_dict, patched_cells)
        if mismatches:
            print(f"  *** MISMATCH CREATED:")
            for key, bool_len, str_len, uimm in mismatches:
                print(f"    Key='{key}' UsedInMainMenu='{uimm}' -> bool[{bool_len}] but string now [{str_len}]")
    print(f"\nFinal KEY columns ({len(m_dict[HEADER_KEY])}): {m_dict[HEADER_KEY]}")
    crashes = [(k, len(v), len(m_dict[k])) for k,v in patched_cells.items() if k in m_dict and len(v) < len(m_dict[k])]
    print(f"CRASH candidates: {len(crashes)}")
    for k, bl, sl in crashes:
        print(f"  key='{k}' bool[{bl}] < string[{sl}]")

    # --- MINIMAL TEST: just FarmingPlus + War3zuk FarmLife ---
    print("\n=== MINIMAL: FarmingPlus + War3zuk FarmLife ===")
    m_dict = {HEADER_KEY: base_cols[:]}
    patched_cells = {}
    minimal = [
        ("AGF-VP-FarmingPlus", find_loc_file("AGF-VP-FarmingPlus")),
        ("War3zuk FarmLife",   find_loc_file("War3zuk FarmLife")),
    ]
    for mod_name, loc_file in minimal:
        if not loc_file:
            print(f"[{mod_name}] NOT FOUND"); continue
        print(f"Loading [{mod_name}] from {loc_file}")
        mismatches = simulate_load_csv_patch(mod_name, loc_file, m_dict, patched_cells)
        if mismatches:
            print(f"  *** MISMATCH CREATED:")
            for key, bool_len, str_len, uimm in mismatches:
                print(f"    Key='{key}' UsedInMainMenu='{uimm}' -> bool[{bool_len}] but string now [{str_len}]")
    print(f"\nFinal KEY columns ({len(m_dict[HEADER_KEY])}): {m_dict[HEADER_KEY]}")
    crashes = [(k, len(v), len(m_dict[k])) for k,v in patched_cells.items() if k in m_dict and len(v) < len(m_dict[k])]
    print(f"CRASH candidates: {len(crashes)}")
    for k, bl, sl in crashes:
        print(f"  key='{k}' bool[{bl}] < string[{sl}]")

    print("\n=== BJAY LOG ORDER ===")
    m_dict = {HEADER_KEY: base_cols[:]}
    patched_cells = {}
    for mod_name in LOAD_ORDER:
        loc_file = find_loc_file(mod_name)
        if not loc_file:
            print(f"[{mod_name}] No Localization.txt found - skipping")
            continue

        mismatches = simulate_load_csv_patch(mod_name, loc_file, m_dict, patched_cells)
        if mismatches:
            print(f"*** MISMATCH CREATED by [{mod_name}]:")
            for key, bool_len, str_len, uimm in mismatches:
                print(f"    Key='{key}' UsedInMainMenu='{uimm}' -> patchedCells.bool[{bool_len}] but string now [{str_len}]")

    print(f"\nFinal KEY columns ({len(m_dict[HEADER_KEY])}): {m_dict[HEADER_KEY]}")
    print(f"Total patchedCells entries: {len(patched_cells)}")

    print("\n--- Keys that WILL crash WriteCsv (bool[] < string[]) ---")
    crash_count = 0
    for key, bool_arr in sorted(patched_cells.items()):
        if key in m_dict:
            str_len = len(m_dict[key])
            bool_len = len(bool_arr)
            if bool_len < str_len:
                print(f"  CRASH: key='{key}' bool[{bool_len}] < string[{str_len}]")
                crash_count += 1
    if crash_count == 0:
        print("  None (simulation shows no crash with this load order)")
    else:
        print(f"\n  Total crash candidates: {crash_count}")

if __name__ == "__main__":
    main()
