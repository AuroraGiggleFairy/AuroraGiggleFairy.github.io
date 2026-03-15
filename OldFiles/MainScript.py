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
                # Always keep the folder with the highest ModInfo.xml version for each mod name
                if modinfo_name in index:
                    _, existing_version = index[modinfo_name]
                    cmp = compare_versions(modinfo_version, existing_version)
                    if cmp > 0:
                        index[modinfo_name] = (folder_path, modinfo_version)
                else:
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
HUDPLUSOTHER_PREFIX = 'AGF-HUDPluszOther-'

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
        # Check for <Version value="..."/>
        version_tag = root.find('Version')
        if version_tag is not None and 'value' in version_tag.attrib:
            return version_tag.attrib['value']
        # Fallback: check for <property name="Version" value="..."/>
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
        # Only push AGF-HUDPluszOther-* if it already exists in the game folder
        if os.path.basename(staging_path).startswith('AGF-HUDPluszOther-'):
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
    inprog_index = build_modinfo_index(INPROGRESS_DIR)
    for modinfo_name, (game_path, game_version) in game_index.items():
        # Update workspace root
        ws_entry = ws_index.get(modinfo_name)
        pull_ws = False
        if not ws_entry:
            pull_ws = True
        else:
            _, ws_version = ws_entry
            cmp = compare_versions(game_version, ws_version)
            if cmp > 0:
                pull_ws = True
        if pull_ws:
            # Remove all workspace mods with this modinfo_name
            for folder, (wname, _) in [(f, build_modinfo_index(WORKSPACE_ROOT)[f]) for f in build_modinfo_index(WORKSPACE_ROOT) if f == modinfo_name]:
                remove_mod(wname)
            dest = os.path.join(WORKSPACE_ROOT, os.path.basename(game_path))
            copy_mod(game_path, dest)

        # Update _In-Progress: always copy if game version is higher or not present
        inprog_entry = inprog_index.get(modinfo_name)
        pull_inprog = False
        if not inprog_entry:
            pull_inprog = True
        else:
            _, inprog_version = inprog_entry
            cmp = compare_versions(game_version, inprog_version)
            if cmp > 0:
                pull_inprog = True
        if pull_inprog:
            # Always remove the folder in _In-Progress with the same name as the game mod, regardless of version
            folder_name = os.path.basename(game_path)
            folder_path = os.path.join(INPROGRESS_DIR, folder_name)
            if os.path.exists(folder_path):
                remove_mod(folder_path)
            # Remove all _In-Progress mods with this modinfo_name (if any, even if folder name differs)
            for folder, (iname, _) in [(f, build_modinfo_index(INPROGRESS_DIR)[f]) for f in build_modinfo_index(INPROGRESS_DIR) if f == modinfo_name]:
                if os.path.abspath(iname) != os.path.abspath(folder_path):
                    remove_mod(iname)
            dest = os.path.join(INPROGRESS_DIR, folder_name)
            copy_mod(game_path, dest)
    run_update_mods()
    # After updating _In-Progress, move any publish-ready mods to workspace root
    move_inprogress_to_publish_ready()

def run_update_mods():
    if os.path.exists(UPDATE_MODS_SCRIPT):
        subprocess.run(['python', UPDATE_MODS_SCRIPT], check=True)

def main():
    # Move mods from workspace root back to _In-Progress if version < 1.0.0
    print('\n=== Checking for mods to move back to _In-Progress (version < 1.0.0)... ===')
    workspace_mods = [f for f in os.listdir(WORKSPACE_ROOT) if os.path.isdir(os.path.join(WORKSPACE_ROOT, f))]
    for mod in workspace_mods:
        mod_path = os.path.join(WORKSPACE_ROOT, mod)
        modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
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
            if not modinfo_name or not (modinfo_name.startswith('AGF-') or modinfo_name.startswith('zzzAGF-')):
                continue
            version_tag = root.find('Version')
            version = version_tag.attrib['value'] if version_tag is not None and 'value' in version_tag.attrib else "0.0.0"
            major_version = int(version.split('.')[0]) if version else 0
            if major_version < 1:
                dest_path = os.path.join(INPROGRESS_DIR, mod)
                if os.path.exists(dest_path):
                    print(f'[DEBUG] Removing existing folder in _In-Progress: {dest_path}')
                    shutil.rmtree(dest_path)
                print(f'[DEBUG] Moving {mod} from workspace root back to _In-Progress (version {version})')
                shutil.move(mod_path, dest_path)
        except Exception as e:
            print(f'[DEBUG] Error checking/moving {mod} back to _In-Progress: {e}')
    print('Moving publish-ready mods from _In-Progress to workspace root...')
    move_inprogress_to_publish_ready()

    # --- Folder renaming and README update logic for workspace root and _In-Progress ---
    def rename_and_update_readme_in_dir(target_dir):
        mods = [f for f in os.listdir(target_dir) if os.path.isdir(os.path.join(target_dir, f))]
        print(f"[DEBUG] Processing {len(mods)} folders in {target_dir}")
        for mod in mods:
            print(f"[DEBUG] Processing folder: {mod}")
            mod_path = os.path.join(target_dir, mod)
            modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
            if not os.path.exists(modinfo_path):
                print(f"[DEBUG] Skipping {mod}: No ModInfo.xml found.")
                continue
            try:
                tree = ET.parse(modinfo_path)
                root = tree.getroot()
                # Try both <Name value=...> and <property name=...>
                name_tag = root.find('Name')
                if name_tag is not None and 'value' in name_tag.attrib:
                    modinfo_name = name_tag.attrib['value']
                else:
                    prop_tag = next((p for p in root.findall('.//property') if p.get('name') == 'Name'), None)
                    modinfo_name = prop_tag.get('value') if prop_tag is not None else None
                if not modinfo_name or not (modinfo_name.startswith('AGF-') or modinfo_name.startswith('zzzAGF-')):
                    print(f"[DEBUG] Skipping {mod}: modinfo_name is not AGF- or zzzAGF-.")
                    continue
                # Remove version suffix from folder name
                version_tag = root.find('Version')
                version = version_tag.attrib['value'] if version_tag is not None and 'value' in version_tag.attrib else "0.0.0"
                canonical_folder = f"{modinfo_name}-v{version}"
                new_path = os.path.join(target_dir, canonical_folder)
                if mod != canonical_folder:
                    if not os.path.exists(new_path):
                        print(f'[DEBUG] Renaming {mod} -> {canonical_folder} in {target_dir}')
                        os.rename(mod_path, new_path)
                        mod_path = new_path
                    else:
                        print(f'[DEBUG] Cannot rename {mod} to {canonical_folder}: destination exists in {target_dir}.')
                # Update README if template exists
                template_path = os.path.join(WORKSPACE_ROOT, "TEMPLATE-Mod_ReadMe.md")
                if os.path.exists(template_path):
                    try:
                        import importlib.util
                        import sys
                        update_mods_path = os.path.join(WORKSPACE_ROOT, "update_mods.py")
                        spec = importlib.util.spec_from_file_location("update_mods", update_mods_path)
                        update_mods = importlib.util.module_from_spec(spec)
                        sys.modules["update_mods"] = update_mods
                        spec.loader.exec_module(update_mods)
                        update_mods.update_readme(mod_path, open(template_path, encoding="utf-8").read())
                        print(f'[DEBUG] Updated README for {canonical_folder} in {target_dir}')
                    except Exception as e:
                        print(f'[DEBUG] Error updating README for {canonical_folder} in {target_dir}: {e}')
            except Exception as e:
                print(f'[DEBUG] Error checking/renaming/updating {mod} in {target_dir}: {e}')

    print('\n=== Checking and renaming mod folders to match ModInfo.xml Name in _In-Progress... ===')
    try:
        rename_and_update_readme_in_dir(INPROGRESS_DIR)
        print('[DEBUG] Finished processing _In-Progress directory.')
    except Exception as e:
        print(f'[ERROR] Exception while processing _In-Progress: {e}')

    print('\n=== Checking and renaming mod folders to match ModInfo.xml Name in workspace root... ===')
    rename_and_update_readme_in_dir(WORKSPACE_ROOT)

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
