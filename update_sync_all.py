# New helper: build_modinfo_index
def build_modinfo_index(path):
    """Return {modinfo_name: (folder_path, version)} for all AGF/zzzAGF mods in path."""
    index = {}
    AGF_PREFIXES = ('AGF-', 'zzzAGF-')
    for folder in os.listdir(path):
        folder_path = os.path.join(path, folder)
        if not os.path.isdir(folder_path):
            continue
        modinfo_path = os.path.join(folder_path, 'ModInfo.xml')
        if not os.path.exists(modinfo_path):
            continue
        try:
            tree = ET.parse(modinfo_path)
            root = tree.getroot()
            name_tag = root.find('Name')
            if name_tag is not None and 'value' in name_tag.attrib:
                modinfo_name = name_tag.attrib['value']
            else:
                prop_tag = next((p for p in root.findall('.//property') if p.get('name') == 'Name'), None)
                modinfo_name = prop_tag.get('value') if prop_tag is not None else None
            version_tag = root.find('Version')
            if version_tag is not None and 'value' in version_tag.attrib:
                modinfo_version = version_tag.attrib['value']
            else:
                prop_tag = next((p for p in root.findall('.//property') if p.get('name') == 'Version'), None)
                modinfo_version = prop_tag.get('value') if prop_tag is not None else None
            if modinfo_name and modinfo_name.startswith(AGF_PREFIXES):
                index[modinfo_name] = (folder_path, modinfo_version)
        except Exception:
            continue
    return index
def compare_versions(v1, v2):
    """Compare two version strings (e.g., '1.0.10' vs '1.0.2'). Returns 1 if v1>v2, -1 if v1<v2, 0 if equal."""
    def to_tuple(v):
        return tuple(int(x) for x in (v or '0.0.0').split('.'))
    t1, t2 = to_tuple(v1), to_tuple(v2)
    # Pad shorter tuple with zeros
    maxlen = max(len(t1), len(t2))
    t1 += (0,) * (maxlen - len(t1))
    t2 += (0,) * (maxlen - len(t2))
    return (t1 > t2) - (t1 < t2)
import os
import shutil
import subprocess
import xml.etree.ElementTree as ET
import re

"""
update_sync_all.py

Automates syncing of AGF-/zzzAGF- mods between workspace, _StagingArea, and the game Mods folder.
- Only updates/removes mods in the game folder if a newer version is available in _StagingArea.
- Pulls newer mods from the game folder into the workspace if ModInfo.xml version is higher.
- Calls update_mods.py after any pull to update workspace and _StagingArea.
- Ensures no in-progress mods in the game folder are deleted unless replaced by a newer version.

Edit the GAME_MODS_PATH below if your game folder changes.
"""

WORKSPACE_ROOT = os.path.dirname(os.path.abspath(__file__))
INPROGRESS_DIR = os.path.join(WORKSPACE_ROOT, '_In-Progress')
STAGING_AREA = os.path.join(WORKSPACE_ROOT, '_StagingArea')
GAME_MODS_PATH = r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods'
UPDATE_MODS_SCRIPT = os.path.join(WORKSPACE_ROOT, 'update_mods.py')

AGF_PREFIXES = ('AGF-', 'zzzAGF-')
BACKPACKPLUS_PREFIX = 'AGF-BackpackPlus-'
HUDPLUSOTHER_PREFIX = 'AGF-HUDPlusOther-'

# Helper functions
def get_mod_version_from_foldername(foldername):
    match = re.search(r'-v([\d.]+)$', foldername)
    return match.group(1) if match else None

def move_inprogress_to_publish_ready():
    # Move mods from _In-Progress to workspace root if ModInfo.xml version >= 1.0.0
    if not os.path.exists(INPROGRESS_DIR):
        return
    for mod in os.listdir(INPROGRESS_DIR):
        mod_path = os.path.join(INPROGRESS_DIR, mod)
        if not os.path.isdir(mod_path):
            continue
        modinfo_version = get_modinfo_version(mod_path)
        try:
            version_tuple = tuple(map(int, (modinfo_version or '0.0.0').split('.')))
        except Exception:
            version_tuple = (0, 0, 0)
        if version_tuple >= (1, 0, 0):
            dest_path = os.path.join(WORKSPACE_ROOT, mod)
            if os.path.exists(dest_path):
                shutil.rmtree(dest_path)
            shutil.move(mod_path, dest_path)
            print(f"Moved {mod} to publish-ready workspace root.")

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
    except Exception:
        return None
    return None

def list_mods(path):
    return [f for f in os.listdir(path)
            if os.path.isdir(os.path.join(path, f)) and f.startswith(AGF_PREFIXES)]

def filter_mods(mods):
    # Only one BackpackPlus (always 84 if present), exclude HUDPlusOther
    backpackplus_84 = [m for m in mods if m.startswith('AGF-BackpackPlus-84Slots')]
    others = [m for m in mods if not m.startswith(BACKPACKPLUS_PREFIX) and not m.startswith(HUDPLUSOTHER_PREFIX)]
    result = others
    if backpackplus_84:
        result.append(backpackplus_84[0])
    return result

def copy_mod(src, dst):
    if os.path.exists(dst):
        shutil.rmtree(dst)
    shutil.copytree(src, dst)

def remove_mod(path):
    if os.path.exists(path):
        shutil.rmtree(path)

def sync_workspace_to_staging():
    # Clear _StagingArea
    for mod in os.listdir(STAGING_AREA):
        remove_mod(os.path.join(STAGING_AREA, mod))
    # Copy all mods from workspace to _StagingArea (canonicalized by update_mods.py)
    ws_index = build_modinfo_index(WORKSPACE_ROOT)
    for modinfo_name, (folder_path, _) in ws_index.items():
        dest = os.path.join(STAGING_AREA, os.path.basename(folder_path))
        copy_mod(folder_path, dest)

def sync_staging_to_game():
    # Only push to game Mods folder if _StagingArea version is higher
    staging_index = build_modinfo_index(STAGING_AREA)
    game_index = build_modinfo_index(GAME_MODS_PATH)
    for modinfo_name, (staging_path, staging_version) in staging_index.items():
        game_entry = game_index.get(modinfo_name)
        push = False
        # Only push AGF-HUDPlusOther-* if it already exists in the game folder
        if os.path.basename(staging_path).startswith('AGF-HUDPlusOther-'):
            if not game_entry:
                continue  # skip if not present in game
        if not game_entry:
            push = True
        else:
            _, game_version = game_entry
            cmp = compare_versions(staging_version, game_version)
            if cmp > 0:
                push = True
        if push:
            # Remove all game mods with this modinfo_name
            for folder, (gname, _) in [(f, build_modinfo_index(GAME_MODS_PATH)[f]) for f in build_modinfo_index(GAME_MODS_PATH) if f == modinfo_name]:
                remove_mod(gname)
            dest = os.path.join(GAME_MODS_PATH, os.path.basename(staging_path))
            copy_mod(staging_path, dest)

def sync_game_to_workspace():
    # Pull newer mods from game folder if ModInfo.xml version is higher
    game_index = build_modinfo_index(GAME_MODS_PATH)
    ws_index = build_modinfo_index(WORKSPACE_ROOT)
    for modinfo_name, (game_path, game_version) in game_index.items():
        ws_entry = ws_index.get(modinfo_name)
        pull = False
        if not ws_entry:
            pull = True
        else:
            _, ws_version = ws_entry
            cmp = compare_versions(game_version, ws_version)
            if cmp > 0:
                pull = True
        if pull:
            # Remove all workspace mods with this modinfo_name
            for folder, (wname, _) in [(f, build_modinfo_index(WORKSPACE_ROOT)[f]) for f in build_modinfo_index(WORKSPACE_ROOT) if f == modinfo_name]:
                remove_mod(wname)
            dest = os.path.join(WORKSPACE_ROOT, os.path.basename(game_path))
            copy_mod(game_path, dest)
    run_update_mods()

def run_update_mods():
    if os.path.exists(UPDATE_MODS_SCRIPT):
        subprocess.run(['python', UPDATE_MODS_SCRIPT], check=True)

def main():
    print('Moving publish-ready mods from _In-Progress to workspace root...')
    move_inprogress_to_publish_ready()

    # --- Folder renaming logic ---
    print('Checking and renaming mod folders to match ModInfo.xml Name...')
    workspace_mods = list_mods(WORKSPACE_ROOT)
    for mod in workspace_mods:
        mod_path = os.path.join(WORKSPACE_ROOT, mod)
        modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
        if not os.path.exists(modinfo_path):
            continue
        try:
            tree = ET.parse(modinfo_path)
            root = tree.getroot()
            # Try both <Name value="..."/> and <property name="Name" value="..."/>
            name_tag = root.find('Name')
            if name_tag is not None and 'value' in name_tag.attrib:
                modinfo_name = name_tag.attrib['value']
            else:
                prop_tag = next((p for p in root.findall('.//property') if p.get('name') == 'Name'), None)
                modinfo_name = prop_tag.get('value') if prop_tag is not None else None
            if not modinfo_name:
                continue
            # Remove version suffix from folder name
            version_match = re.search(r'(-v[\d.]+)$', mod)
            version_suffix = version_match.group(1) if version_match else ''
            folder_base = mod[:-len(version_suffix)] if version_suffix else mod
            if folder_base != modinfo_name:
                new_folder = modinfo_name + version_suffix
                new_path = os.path.join(WORKSPACE_ROOT, new_folder)
                if not os.path.exists(new_path):
                    print(f'Renaming {mod} -> {new_folder}')
                    os.rename(mod_path, new_path)
                else:
                    print(f'Cannot rename {mod} to {new_folder}: destination exists.')
        except Exception as e:
            print(f'Error checking/renaming {mod}: {e}')

    # Remove non-AGF/zzzAGF mods from workspace root
    for folder in os.listdir(WORKSPACE_ROOT):
        folder_path = os.path.join(WORKSPACE_ROOT, folder)
        if not os.path.isdir(folder_path):
            continue
        if not (folder.startswith('AGF-') or folder.startswith('zzzAGF-')):
            # Only remove folders that look like mods (have ModInfo.xml)
            if os.path.exists(os.path.join(folder_path, 'ModInfo.xml')):
                print(f"[CLEANUP] Removing non-AGF mod from workspace: {folder}")
                shutil.rmtree(folder_path)

    print('Syncing workspace to _StagingArea...')
    sync_workspace_to_staging()
    print('Syncing game Mods folder to workspace (pull newer mods)...')
    sync_game_to_workspace()
    print('Running update_mods.py...')
    run_update_mods()
    print('Rebuilding _StagingArea from workspace...')
    sync_workspace_to_staging()
    print('Syncing _StagingArea to game Mods folder...')
    sync_staging_to_game()
    print('Sync complete.')

if __name__ == '__main__':
    main()
