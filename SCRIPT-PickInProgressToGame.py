"""
sync_inprogress_to_game.py

Interactive script to copy selected draft mods to the game Mods folder for testing.
- Lists all mods in the draft lane (prefers 01_Draft/)
- Prompts user to select which mods to copy to the game Mods folder
- If a mod with the same base name exists in the game folder, compares ModInfo.xml version and folder version
- Only copies if the draft version is newer or not present
- Optionally, can remove selected mods from the game Mods folder
- Updates README and ModInfo.xml in the game folder to match the draft version if copied
"""
import os
import shutil
import xml.etree.ElementTree as ET
import re

WORKSPACE_ROOT = os.path.dirname(os.path.abspath(__file__))


def resolve_lane_path(*candidates):
    for path in candidates:
        if os.path.isdir(path):
            return path
    return candidates[0]


INPROGRESS_DIR = resolve_lane_path(
    os.path.join(WORKSPACE_ROOT, "01_Draft"),
    os.path.join(WORKSPACE_ROOT, "_Mods2.In-Progress"),
    os.path.join(WORKSPACE_ROOT, "_In-Progress"),
)
GAME_MODS_PATH = r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods'

def get_mod_version_from_foldername(foldername):
    match = re.search(r'-v([\d.]+)$', foldername)
    return match.group(1) if match else None


def version_key(version_text):
    if not version_text:
        return tuple()
    parts = [p for p in str(version_text).strip().split('.') if p != '']
    key = []
    for part in parts:
        if part.isdigit():
            key.append(int(part))
        else:
            numeric = re.match(r'^(\d+)', part)
            key.append(int(numeric.group(1)) if numeric else 0)
    return tuple(key)

def get_modinfo_version(mod_folder):
    modinfo_path = os.path.join(mod_folder, 'ModInfo.xml')
    if not os.path.exists(modinfo_path):
        return None
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        for prop in root.findall('.//property'):
            if prop.get('name') == 'Version':
                return prop.get('value')
        # fallback for <Version value="..." />
        for elem in root.iter():
            if elem.tag == 'Version' and 'value' in elem.attrib:
                return elem.attrib['value']
    except Exception:
        return None
    return None

def list_mods(path):
    return [f for f in os.listdir(path) if os.path.isdir(os.path.join(path, f))]

def prompt_select_mods(mods):
    print("Select mods to copy to the game Mods folder (comma separated numbers):")
    for i, mod in enumerate(mods):
        print(f"{i+1}. {mod}")
    sel = input("Enter numbers (e.g. 1,3,5): ").strip()
    idxs = [int(x)-1 for x in sel.split(',') if x.strip().isdigit() and 0 < int(x) <= len(mods)]
    return [mods[i] for i in idxs]

def copy_mod(src, dst):
    if os.path.exists(dst):
        shutil.rmtree(dst)
    shutil.copytree(src, dst)

def main():
    mods = list_mods(INPROGRESS_DIR)
    # Only allow AGF-HUDPluszOther-* mods if they already exist in the game Mods folder
    game_mods = list_mods(GAME_MODS_PATH)
    filtered_mods = []
    for mod in mods:
        if mod.startswith('AGF-HUDPluszOther-'):
            # Only include if a matching mod exists in the game folder (by base name)
            mod_base = re.sub(r'-v[\d.]+$', '', mod)
            if any(g.startswith(mod_base) for g in game_mods):
                filtered_mods.append(mod)
        else:
            filtered_mods.append(mod)
    mods = filtered_mods
    if not mods:
        print(f"No in-progress mods found in: {INPROGRESS_DIR}")
        return
    selected = prompt_select_mods(mods)
    if not selected:
        print("No mods selected.")
        return
    game_mods = list_mods(GAME_MODS_PATH)
    for mod in selected:
        src = os.path.join(INPROGRESS_DIR, mod)
        mod_base = re.sub(r'-v[\d.]+$', '', mod)
        inprog_version = get_mod_version_from_foldername(mod)
        inprog_modinfo_version = get_modinfo_version(src)
        # Determine major version (prefer ModInfo.xml, fallback to folder)
        version_str = inprog_modinfo_version or inprog_version or '0.0.0'
        try:
            major_version = int(version_str.split('.')[0])
        except Exception:
            major_version = 0
        if major_version < 1:
            print(
                f"Warning: {mod} has major version {major_version}. "
                "Continuing copy because this tool is for draft testing."
            )
        # Find matching mod in game folder
        gmod = next((g for g in game_mods if re.sub(r'-v[\d.]+$', '', g) == mod_base), None)
        if gmod:
            gmod_path = os.path.join(GAME_MODS_PATH, gmod)
            game_version = get_mod_version_from_foldername(gmod)
            game_modinfo_version = get_modinfo_version(gmod_path)
            # Compare versions (prefer ModInfo.xml, fallback to folder)
            inprog_v = inprog_modinfo_version or inprog_version
            game_v = game_modinfo_version or game_version
            if inprog_v and game_v and version_key(inprog_v) > version_key(game_v):
                print(f"Updating {gmod} in game folder with newer in-progress version {inprog_v}...")
                copy_mod(src, os.path.join(GAME_MODS_PATH, mod))
            elif inprog_v and game_v and version_key(inprog_v) == version_key(game_v):
                print(f"{gmod} in game folder is up to date. Skipping.")
            else:
                print(f"Game folder has newer or same version for {gmod}. Skipping.")
        else:
            print(f"Copying {mod} to game folder...")
            copy_mod(src, os.path.join(GAME_MODS_PATH, mod))
    print("Done.")

if __name__ == "__main__":
    main()
