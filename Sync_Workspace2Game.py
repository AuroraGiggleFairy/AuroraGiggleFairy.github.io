import os
import shutil

"""
Sync_Workspace2Game.py
Copies selected AGF mods from workspace to game Mods folder, with rules:
- Only AGF-BackpackPlus-84Slots-v3.2.1 (not other BackpackPlus mods)
- Exclude all AGF-HUDPlusOther-* mods
- Copy all other AGF-/zzzAGF- mods
"""

PUBLISH_READY_DIR = os.path.abspath('.')
GAME_MODS_DIR = r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods'

# Only copy this BackpackPlus mod
INCLUDE_BACKPACK = 'AGF-BackpackPlus-84Slots-v3.2.1'
# Exclude all HUDPlusOther mods
EXCLUDE_PREFIXES = ('AGF-HUDPlusOther-',)

# Only include AGF- and zzzAGF- mods
MOD_PREFIXES = ('AGF-', 'zzzAGF-')

def should_copy(mod_name):
    if mod_name == INCLUDE_BACKPACK:
        return True
    if mod_name.startswith('AGF-BackpackPlus-') and mod_name != INCLUDE_BACKPACK:
        return False
    if any(mod_name.startswith(prefix) for prefix in EXCLUDE_PREFIXES):
        return False
    return mod_name.startswith(MOD_PREFIXES)

def copy_mod(mod_name):
    src = os.path.join(PUBLISH_READY_DIR, mod_name)
    dst = os.path.join(GAME_MODS_DIR, mod_name)
    if os.path.exists(dst):
        confirm = input(f"Mod '{mod_name}' already exists in game folder. Overwrite? (y/n): ").strip().lower()
        if confirm != 'y':
            print(f"Skipped {mod_name}")
            return
        shutil.rmtree(dst)
    shutil.copytree(src, dst)
    print(f"Copied {mod_name} to game Mods folder.")

def main():
    mods = [d for d in os.listdir(PUBLISH_READY_DIR)
            if os.path.isdir(os.path.join(PUBLISH_READY_DIR, d))]
    mods_to_copy = [m for m in mods if should_copy(m)]
    if not mods_to_copy:
        print('No mods to copy.')
        return
    print('Mods to be copied:')
    for m in mods_to_copy:
        print(f'  {m}')
    confirm = input('Proceed with copying these mods to the game folder? (y/n): ').strip().lower()
    if confirm != 'y':
        print('Aborted.')
        return
    for m in mods_to_copy:
        copy_mod(m)
    print('Sync complete.')

if __name__ == '__main__':
    main()
