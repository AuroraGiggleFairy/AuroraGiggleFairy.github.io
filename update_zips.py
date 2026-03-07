import os
import zipfile
from pathlib import Path

# Directories to include as individual zips (top-level mod folders)
MOD_FOLDERS = [
    d for d in os.listdir('.')
    if os.path.isdir(d) and d.startswith('AGF-')
]

# Mod pack definitions (group zips)
MOD_PACKS = {
    'HUDPlus_All': [d for d in MOD_FOLDERS if 'HUDPlus' in d],
    'BackpackPlus_All': [d for d in MOD_FOLDERS if 'BackpackPlus' in d],
    'VP_All': [d for d in MOD_FOLDERS if d.startswith('AGF-VP-')],
    'NoEAC_All': [d for d in MOD_FOLDERS if d.startswith('AGF-NoEAC-')],
    # 'Other_All': [d for d in MOD_FOLDERS if d.startswith('AGF-HUDPlusOther-')],
}

ZIPS_DIR = Path('zips')
ZIPS_DIR.mkdir(exist_ok=True)

def zip_folder(folder, zip_path):
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, _, files in os.walk(folder):
            for file in files:
                file_path = Path(root) / file
                arcname = file_path.relative_to(folder.parent)
                zipf.write(file_path, arcname)

def main():
    # Zip each individual mod
    for mod in MOD_FOLDERS:
        zip_path = ZIPS_DIR / f'{mod}.zip'
        print(f'Zipping {mod} -> {zip_path}')
        zip_folder(Path(mod), zip_path)

    # Zip mod packs
    for pack_name, folders in MOD_PACKS.items():
        if not folders:
            continue
        zip_path = ZIPS_DIR / f'{pack_name}.zip'
        print(f'Zipping pack {pack_name} -> {zip_path}')
        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
            for folder in folders:
                for root, _, files in os.walk(folder):
                    for file in files:
                        file_path = Path(root) / file
                        arcname = Path(folder) / file_path.relative_to(folder)
                        zipf.write(file_path, arcname)

if __name__ == '__main__':
    main()
