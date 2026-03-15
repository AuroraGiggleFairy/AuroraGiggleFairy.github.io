# --- Helper Functions ---
def get_quote_file_name(mod_name):
    return f"{mod_name}.txt"

def ensure_quote_file_exists(quotes_dir, quote_file):
    quote_path = os.path.join(quotes_dir, quote_file)
    if not os.path.exists(quote_path):
        with open(quote_path, 'w', encoding='utf-8') as f:
            f.write("")
        print(f"[QUOTE] Created missing quote file: {quote_file}")
    return quote_path

def parse_modinfo(modinfo_path, fallback_name):
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        name_tag = root.find('Name')
        mod_name = name_tag.attrib['value'] if name_tag is not None and 'value' in name_tag.attrib else fallback_name
        version_tag = root.find('Version')
        mod_version = version_tag.attrib['value'] if version_tag is not None and 'value' in version_tag.attrib else '0.0.0'
    except Exception:
        mod_name = fallback_name
        mod_version = '0.0.0'
    return mod_name, mod_version

def format_blockquote(text):
    if not text.strip():
        return ''
    lines = text.splitlines()
    return '\n'.join([f'> {line}' if line.strip() else '>' for line in lines])
import os
import shutil
import xml.etree.ElementTree as ET
import concurrent.futures

# --- Readable ReadMe Helpers ---
import re
def markdown_to_text(md):
    md = re.sub(r'```[\s\S]*?```', '', md)
    md = re.sub(r'!\[[^\]]*\]\([^\)]*\)', '', md)
    md = re.sub(r'\[([^\]]+)\]\(([^\)]+)\)', r'\1: \2', md)
    md = re.sub(r'[`*_~]', '', md)
    md = re.sub(r'^---+$', '\n' + '='*40 + '\n', md, flags=re.MULTILINE)
    md = re.sub(r'^#+\s*', '', md, flags=re.MULTILINE)
    md = re.sub(r'^>\s?', '', md, flags=re.MULTILINE)
    md = re.sub(r'<[^>]+>', '', md)
    md = re.sub(r'\n{3,}', '\n\n', md)
    return md.strip()

def remove_blank_between_dividers(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    new_lines = []
    i = 0
    while i < len(lines):
        if (
            lines[i].lstrip().startswith('=') and
            i + 2 < len(lines) and
            lines[i+1].strip() == '' and
            lines[i+2].lstrip().startswith('=')
        ):
            new_lines.append(lines[i])
            i += 1
        else:
            new_lines.append(lines[i])
        i += 1
    with open(filepath, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)

# --- CONFIG ---

VS_CODE_ROOT = os.path.dirname(os.path.abspath(__file__))
PUBLISH_READY = os.path.join(VS_CODE_ROOT, '_Mods1.PublishReady')
IN_PROGRESS = os.path.join(VS_CODE_ROOT, '_Mods2.In-Progress')
GAME_MODS = r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods'
AGF_PREFIXES = ('AGF-', 'zzzAGF-')

# --- HELPERS ---
def is_agf_mod(folder):
    return folder.startswith(AGF_PREFIXES)

def get_modinfo_version(folder_path):
    modinfo_path = os.path.join(folder_path, 'ModInfo.xml')
    if not os.path.exists(modinfo_path):
        return None
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        version_tag = root.find('Version')
        if version_tag is not None and 'value' in version_tag.attrib:
            return version_tag.attrib['value']
        for prop in root.findall('.//property'):
            if prop.get('name') == 'Version':
                return prop.get('value')
    except Exception:
        return None
    return None

def compare_versions(v1, v2):
    try:
        def to_tuple(v):
            return tuple(int(x) for x in (v or '0.0.0').split('.') if x.isdigit())
        t1, t2 = to_tuple(v1), to_tuple(v2)
        maxlen = max(len(t1), len(t2))
        t1 += (0,) * (maxlen - len(t1))
        t2 += (0,) * (maxlen - len(t2))
        return (t1 > t2) - (t1 < t2)
    except Exception:
        return 0

def copy_mod(src, dst):
    if os.path.exists(dst):
        shutil.rmtree(dst)
    shutil.copytree(src, dst)

def main():

    # Step 1: Find and Match Your Mods


    pub_folders = {f: os.path.join(PUBLISH_READY, f) for f in os.listdir(PUBLISH_READY) if os.path.isdir(os.path.join(PUBLISH_READY, f)) and is_agf_mod(f)}
    inprog_folders = {f: os.path.join(IN_PROGRESS, f) for f in os.listdir(IN_PROGRESS) if os.path.isdir(os.path.join(IN_PROGRESS, f)) and is_agf_mod(f)}
    game_folders = {f: os.path.join(GAME_MODS, f) for f in os.listdir(GAME_MODS) if os.path.isdir(os.path.join(GAME_MODS, f)) and is_agf_mod(f)}
    mods_pulled_from_game = []

    # Step 2: Sync Mods by Version Number
    
    for folder_name, game_path in game_folders.items():
        pub_path = pub_folders.get(folder_name)
        inprog_path = inprog_folders.get(folder_name)
        game_ver = get_modinfo_version(game_path)
        pub_ver = get_modinfo_version(pub_path) if pub_path else None
        inprog_ver = get_modinfo_version(inprog_path) if inprog_path else None
        try:
            cmp_pub = compare_versions(game_ver, pub_ver) if pub_path and game_ver and pub_ver else None
        except Exception:
            cmp_pub = None
        try:
            cmp_inprog = compare_versions(game_ver, inprog_ver) if inprog_path and game_ver and inprog_ver else None
        except Exception:
            cmp_inprog = None
        if cmp_pub is not None and cmp_pub > 0:
            print(f"[SYNC] Game mod {folder_name} (v{game_ver}) is newer than PublishReady (v{pub_ver}). Overwriting PublishReady version.")
            copy_mod(game_path, pub_path)
            mods_pulled_from_game.append((folder_name, pub_path))
            try:
                shutil.rmtree(game_path)
                print(f"[CLEANUP] Removed {folder_name} from game mods folder after sync.")
            except Exception as e:
                print(f"[ERROR] Failed to remove {folder_name} from game mods folder: {e}")
        if cmp_inprog is not None and cmp_inprog > 0:
            print(f"[SYNC] Game mod {folder_name} (v{game_ver}) is newer than _In-Progress (v{inprog_ver}). Overwriting _In-Progress version.")
            copy_mod(game_path, inprog_path)
            mods_pulled_from_game.append((folder_name, inprog_path))
            try:
                shutil.rmtree(game_path)
                print(f"[CLEANUP] Removed {folder_name} from game mods folder after sync.")
            except Exception as e:
                print(f"[ERROR] Failed to remove {folder_name} from game mods folder: {e}")

    # Step 3: Move Mods Based on Major Version
    pub_folders = {f: os.path.join(PUBLISH_READY, f) for f in os.listdir(PUBLISH_READY) if os.path.isdir(os.path.join(PUBLISH_READY, f)) and is_agf_mod(f)}
    inprog_folders = {f: os.path.join(IN_PROGRESS, f) for f in os.listdir(IN_PROGRESS) if os.path.isdir(os.path.join(IN_PROGRESS, f)) and is_agf_mod(f)}
    all_mods = set(list(pub_folders.keys()) + list(inprog_folders.keys()))
    for folder_name in all_mods:
        pub_path = pub_folders.get(folder_name)
        inprog_path = inprog_folders.get(folder_name)
        mod_path = pub_path if pub_path else inprog_path
        if not mod_path:
            continue
        version = get_modinfo_version(mod_path)
        if not version or '.' not in version:
            continue
        major = version.split('.', 1)[0]
        try:
            major_num = int(major)
        except ValueError:
            continue
        if major_num == 0:
            if pub_path:
                dest = os.path.join(IN_PROGRESS, folder_name)
                print(f"[MOVE] Mod {folder_name} (v{version}) is dev (major=0). Moving to _Mods2.In-Progress.")
                if os.path.exists(dest):
                    shutil.rmtree(dest)
                shutil.move(pub_path, dest)
        elif major_num >= 1:
            if inprog_path:
                dest = os.path.join(PUBLISH_READY, folder_name)
                print(f"[MOVE] Mod {folder_name} (v{version}) is publish-ready (major>=1). Moving to _Mods1.PublishReady.")
                if os.path.exists(dest):
                    shutil.rmtree(dest)
                shutil.move(inprog_path, dest)

    # =============================
    # Step 4: Special Handling (Renaming, Compatibility, Quotes, README)
    # =============================

    # --- 4.1 Folder Renaming ---
    folder_renames = []
    for mod_dir in [PUBLISH_READY, IN_PROGRESS]:
        for folder_name in os.listdir(mod_dir):
            if not is_agf_mod(folder_name):
                continue
            mod_path = os.path.join(mod_dir, folder_name)
            modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
            if not os.path.exists(modinfo_path):
                continue
            modinfo_name, modinfo_version = parse_modinfo(modinfo_path, folder_name)
            target_name = f"{modinfo_name}-v{modinfo_version}"
            if target_name != folder_name and is_agf_mod(modinfo_name):
                new_path = os.path.join(mod_dir, target_name)
                if not os.path.exists(new_path):
                    print(f"[RENAME] Renaming folder {folder_name} -> {target_name}")
                    shutil.move(mod_path, new_path)
                    folder_renames.append((folder_name, target_name, mod_dir))
                    for i, (old_name, ws_path) in enumerate(mods_pulled_from_game):
                        if old_name == folder_name:
                            mods_pulled_from_game[i] = (target_name, new_path)

    # --- 4.2 Update mod_compatibility.csv (or HELPER_ModCompatibility.csv) ---
    # NOTE: This assumes HELPER_ModCompatibility.csv is used, as per your current workflow.
    import csv
    COMPAT_CSV = os.path.join(VS_CODE_ROOT, 'HELPER_ModCompatibility.csv')
    def get_base_mod_name(name):
        return re.sub(r'-v\d+\.\d+(\.\d+)?$', '', name)

    mods_now = set()
    folder_name_to_base = {}
    for mod_dir in [PUBLISH_READY, IN_PROGRESS]:
        for folder_name in os.listdir(mod_dir):
            if is_agf_mod(folder_name):
                base_name = get_base_mod_name(folder_name)
                mods_now.add(base_name)
                folder_name_to_base[folder_name] = base_name
    csv_rows = []
    csv_fieldnames = ['MOD_NAME', 'QUOTE_FILE']
    if os.path.exists(COMPAT_CSV):
        with open(COMPAT_CSV, 'r', encoding='utf-8', newline='') as f:
            reader = csv.DictReader(f)
            csv_fieldnames = reader.fieldnames if reader.fieldnames else csv_fieldnames
            for row in reader:
                csv_rows.append(row)
    # Remove entries for mods that no longer exist
    csv_rows = [row for row in csv_rows if row.get('MOD_NAME') in mods_now]
    # Add new mods and update renamed mods
    existing_mods = {row['MOD_NAME'] for row in csv_rows}
    for mod in mods_now:
        if mod not in existing_mods:
            csv_rows.append({fn: 'MISSINGDATA' for fn in csv_fieldnames})
            csv_rows[-1]['MOD_NAME'] = mod
            csv_rows[-1]['QUOTE_FILE'] = f'{mod}.txt'
    # Update QUOTE_FILE for renamed mods
    for row in csv_rows:
        row['QUOTE_FILE'] = f"{row['MOD_NAME']}.txt"
        for fn in csv_fieldnames:
            if not row.get(fn):
                row[fn] = 'MISSINGDATA'
    # Write back to CSV
    with open(COMPAT_CSV, 'w', encoding='utf-8', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=csv_fieldnames)
        writer.writeheader()
        writer.writerows(csv_rows)

    # --- 4.3 Quote Files ---
    QUOTES_DIR = os.path.join(VS_CODE_ROOT, '_Quotes')
    os.makedirs(QUOTES_DIR, exist_ok=True)
    # Ensure quote file exists and is named correctly for every mod in the CSV
    for row in csv_rows:
        mod_name = row['MOD_NAME']
        quote_file = row['QUOTE_FILE']
        quote_path = os.path.join(QUOTES_DIR, quote_file)
        # If mod was renamed, rename quote file if old one exists (using base names)
        for old_name, new_name, mod_dir in folder_renames:
            old_base = get_base_mod_name(old_name)
            new_base = get_base_mod_name(new_name)
            old_quote = os.path.join(QUOTES_DIR, f'{old_base}.txt')
            new_quote = os.path.join(QUOTES_DIR, f'{new_base}.txt')
            if os.path.exists(old_quote) and not os.path.exists(new_quote):
                os.rename(old_quote, new_quote)
                print(f"[QUOTE] Renamed quote file: {old_base}.txt -> {new_base}.txt")
        # Always overwrite with latest quote (if you have a source, here just ensure file exists)
        if not os.path.exists(quote_path):
            with open(quote_path, 'w', encoding='utf-8') as f:
                f.write("")
            print(f"[QUOTE] Created missing quote file: {quote_file}")

    # --- 4.4 README.md and ReadableReadMe.txt ---
    # For each mod, create README.md from template and ReadableReadMe.txt
    MOD_README_TEMPLATE = os.path.join(VS_CODE_ROOT, 'TEMPLATE-ModReadMe.md')
    # --- Load compatibility data from CSV for all mods ---
    compat_data = {}
    if os.path.exists(COMPAT_CSV):
        with open(COMPAT_CSV, 'r', encoding='utf-8', newline='') as f:
            reader = csv.DictReader(f)
            for row in reader:
                compat_data[row['MOD_NAME']] = row

    for mod_dir in [PUBLISH_READY, IN_PROGRESS]:
        for folder_name in os.listdir(mod_dir):
            if not is_agf_mod(folder_name):
                continue
            mod_path = os.path.join(mod_dir, folder_name)
            modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
            if not os.path.exists(modinfo_path):
                continue
            mod_name, mod_version = parse_modinfo(modinfo_path, folder_name)
            base_name = get_base_mod_name(folder_name)
            download_link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_Mods3.zip/{base_name}.zip'
            # Get compatibility fields from CSV
            compat = compat_data.get(base_name, {})
            eac_friendly = compat.get('EAC_FRIENDLY', 'MISSINGDATA')
            server_side = compat.get('SERVER_SIDE', 'MISSINGDATA')
            client_required = compat.get('CLIENT_REQUIRED', 'MISSINGDATA')
            safe_to_install = compat.get('SAFE_TO_INSTALL', 'MISSINGDATA')
            safe_to_remove = compat.get('SAFE_TO_REMOVE', 'MISSINGDATA')
            unique = compat.get('UNIQUE', 'MISSINGDATA')
            quote_file = os.path.join(QUOTES_DIR, f'{base_name}.txt')
            quote_md = ''
            if os.path.exists(quote_file):
                with open(quote_file, 'r', encoding='utf-8') as f:
                    quote_text = f.read().strip()
                if quote_text:
                    quote_md = format_blockquote(quote_text)
            # Fill template, but preserve existing Features and Changelog blocks if present
            if os.path.exists(MOD_README_TEMPLATE):
                with open(MOD_README_TEMPLATE, 'r', encoding='utf-8') as f:
                    template = f.read()
                readme_content = template.replace('{{MOD_NAME}}', mod_name)
                readme_content = readme_content.replace('{{MOD_VERSION}}', mod_version)
                readme_content = readme_content.replace('{{DOWNLOAD_LINK}}', download_link)
                readme_content = readme_content.replace('{{QUOTE}}', quote_md)
                readme_content = readme_content.replace('{{EAC_FRIENDLY}}', eac_friendly)
                readme_content = readme_content.replace('{{SERVER_SIDE}}', server_side)
                readme_content = readme_content.replace('{{CLIENT_REQUIRED}}', client_required)
                readme_content = readme_content.replace('{{SAFE_TO_INSTALL}}', safe_to_install)
                readme_content = readme_content.replace('{{SAFE_TO_REMOVE}}', safe_to_remove)
                readme_content = readme_content.replace('{{UNIQUE}}', unique)
                readme_path = os.path.join(mod_path, 'README.md')
                # --- PRESERVE FEATURES/CHANGELOG ---
                features_block = ''
                changelog_block = ''
                if os.path.exists(readme_path):
                    with open(readme_path, 'r', encoding='utf-8') as f:
                        old_content = f.read()
                    # Extract features
                    f_start = old_content.find('<!-- FEATURES START -->')
                    f_end = old_content.find('<!-- FEATURES END -->')
                    if f_start != -1 and f_end != -1:
                        features_block = old_content[f_start+21:f_end]
                    # Extract changelog
                    c_start = old_content.find('<!-- CHANGELOG START -->')
                    c_end = old_content.find('<!-- CHANGELOG END -->')
                    if c_start != -1 and c_end != -1:
                        changelog_block = old_content[c_start+21:c_end]
                # Replace in new content
                if features_block:
                    readme_content = re.sub(r'(<!-- FEATURES START -->)([\s\S]*?)(<!-- FEATURES END -->)', r'\1' + features_block + r'\3', readme_content, flags=re.MULTILINE)
                if changelog_block:
                    readme_content = re.sub(r'(<!-- CHANGELOG START -->)([\s\S]*?)(<!-- CHANGELOG END -->)', r'\1' + changelog_block + r'\3', readme_content, flags=re.MULTILINE)
                with open(readme_path, 'w', encoding='utf-8') as f:
                    f.write(readme_content)
                # Create ReadableReadMe.txt
                txt_path = os.path.join(mod_path, 'ReadableReadMe.txt')
                txt_content = markdown_to_text(readme_content)
                with open(txt_path, 'w', encoding='utf-8') as f:
                    f.write(txt_content)
    # --- 4.5 Preserve Important Info ---
    # (Handled in earlier steps: version/changelog from game mods folder is preserved if newer)

    # Step 5: Push Updated Mods Back to the Game Mods Folder (If Pulled Earlier)
    for mod_name, ws_path in mods_pulled_from_game:
        if not os.path.exists(ws_path):
            print(f"[PUSHBACK] Skipping {mod_name}: workspace path not found.")
            continue
        dest_path = os.path.join(GAME_MODS, os.path.basename(ws_path))
        if os.path.exists(dest_path):
            try:
                shutil.rmtree(dest_path)
            except Exception as e:
                print(f"[PUSHBACK] Failed to remove old {mod_name} in game mods folder: {e}")
                continue
        try:
            shutil.copytree(ws_path, dest_path)
            print(f"[PUSHBACK] Updated {mod_name} pushed back to game mods folder.")
        except Exception as e:
            print(f"[PUSHBACK] Failed to copy {mod_name} to game mods folder: {e}")

    # Step 6: Create Zip Files for Mods and Categories (After All Other Steps)
    import zipfile
    ZIP_OUTPUT = os.path.join(VS_CODE_ROOT, '_Mods3.zip')
    os.makedirs(ZIP_OUTPUT, exist_ok=True)
    for file in os.listdir(ZIP_OUTPUT):
        if file.lower().endswith('.zip'):
            try:
                os.remove(os.path.join(ZIP_OUTPUT, file))
                print(f'[CLEANUP] Deleted old zip: {file}')
            except Exception as e:
                print(f'[CLEANUP] Failed to delete {file}: {e}')
    def zip_mod_folder(mod_folder):
        mod_path = os.path.join(PUBLISH_READY, mod_folder)
        modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
        mod_name, _ = parse_modinfo(modinfo_path, mod_folder)
        zip_name = mod_folder.split('-v')[0] + '.zip' if '-v' in mod_folder else mod_folder + '.zip'
        zip_path = os.path.join(ZIP_OUTPUT, zip_name)
        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
            for root, _, files in os.walk(mod_path):
                for file in files:
                    file_path = os.path.join(root, file)
                    arcname = os.path.join(mod_folder, os.path.relpath(file_path, mod_path))
                    zipf.write(file_path, arcname)
        print(f'[ZIP] Created {zip_name}')
    for mod_folder in os.listdir(PUBLISH_READY):
        if is_agf_mod(mod_folder) and os.path.isdir(os.path.join(PUBLISH_READY, mod_folder)):
            zip_mod_folder(mod_folder)
    def zip_category(pack_name, root_mods, optionals_map=None):
        zip_path = os.path.join(ZIP_OUTPUT, f'{pack_name}.zip')
        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
            for mod_folder in root_mods:
                mod_path = os.path.join(PUBLISH_READY, mod_folder)
                for root, _, files in os.walk(mod_path):
                    for file in files:
                        file_path = os.path.join(root, file)
                        arcname = os.path.join(mod_folder, os.path.relpath(file_path, mod_path))
                        zipf.write(file_path, arcname)
            if optionals_map:
                for opt_folder, opt_mods in optionals_map.items():
                    for mod_folder in opt_mods:
                        mod_path = os.path.join(PUBLISH_READY, mod_folder)
                        for root, _, files in os.walk(mod_path):
                            for file in files:
                                file_path = os.path.join(root, file)
                                arcname = os.path.join(opt_folder, mod_folder, os.path.relpath(file_path, mod_path))
                                zipf.write(file_path, arcname)
        print(f'[ZIP] Created {pack_name}.zip')
    all_folders = [f for f in os.listdir(PUBLISH_READY) if is_agf_mod(f) and os.path.isdir(os.path.join(PUBLISH_READY, f))]
    backpackplus_mods = [f for f in all_folders if f.startswith('AGF-BackpackPlus-')]
    hudplus_mods = [f for f in all_folders if f.startswith('AGF-HUDPlus-')]
    hudpluszother_mods = [f for f in all_folders if f.startswith('AGF-HUDPluszOther-')]
    noeac_mods = [f for f in all_folders if f.startswith('AGF-NoEAC-')]
    vp_mods = [f for f in all_folders if f.startswith('AGF-VP-')]
    special_mods = [f for f in all_folders if f.startswith('zzzAGF-Special')]
    backpackplus_84 = next((f for f in backpackplus_mods if '84Slots' in f), None)
    zip_category('BackpackPlus_All', backpackplus_mods)
    giggle_root = hudplus_mods + vp_mods + special_mods + ([backpackplus_84] if backpackplus_84 else [])
    giggle_optionals = {
        '.Optionals-BackpackPlus': backpackplus_mods,
        '.Optionals-HUDPlus': hudplus_mods + hudpluszother_mods,
        '.Optionals-NoEAC': noeac_mods,
    }
    zip_category('GigglePack_All', giggle_root, giggle_optionals)
    hudplus_all_root = hudplus_mods + special_mods
    hudplus_all_optionals = {
        '.Optionals-NoEAC': noeac_mods,
        '.Optionals-HUDPluszOther': hudpluszother_mods,
    }
    zip_category('HUDPlus_All', hudplus_all_root, hudplus_all_optionals)
    zip_category('HUDPluszOther_All', hudpluszother_mods)
    zip_category('AGF-NoEAC_All', noeac_mods)
    vp_all_root = vp_mods + special_mods
    vp_all_optionals = {'.Optionals-NoEAC': noeac_mods}
    zip_category('VP_All', vp_all_root, vp_all_optionals)

if __name__ == '__main__':
    main()

    # Step 7: Update Main README.md and ReadableReadMe.txt
    # (All code for updating main README.md and ReadableReadMe.txt must be done here, after all other steps)
    import datetime
    MAIN_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, 'TEMPLATE-MainReadMe.md')
    MAIN_README_PATH = os.path.join(VS_CODE_ROOT, 'README.md')
    MAIN_README_TXT_PATH = os.path.join(VS_CODE_ROOT, 'ReadableReadMe.txt')
    CATEGORY_DESC_PATH = os.path.join(VS_CODE_ROOT, 'TEMPLATE-CategoryDescriptions.md')
    def get_now_str():
        now = datetime.datetime.now()
        return now.strftime('%B %d, %Y, %-I:%M %p %Z') if hasattr(now, 'strftime') else now.strftime('%B %d, %Y')
    def load_category_descriptions():
        descs = {}
        if os.path.exists(CATEGORY_DESC_PATH):
            with open(CATEGORY_DESC_PATH, 'r', encoding='utf-8') as f:
                lines = f.readlines()
            current = None
            buf = []
            for line in lines:
                if line.strip().startswith('[') and line.strip().endswith(']'):
                    if current and buf:
                        descs[current] = '\n'.join(buf).strip()
                    current = line.strip()[1:-1]
                    buf = []
                elif current is not None:
                    buf.append(line.rstrip())
            if current and buf:
                descs[current] = '\n'.join(buf).strip()
        return descs
    def extract_mod_features(readme_path):
        if not os.path.exists(readme_path):
            return ''
        with open(readme_path, 'r', encoding='utf-8') as f:
            content = f.read()
        start = content.find('<!-- FEATURES START -->')
        end = content.find('<!-- FEATURES END -->')
        if start != -1 and end != -1:
            features_block = content[start+21:end].strip('\n')
            return features_block
        return ''

    # Always use the <Description> value from ModInfo.xml for the main README description
    def extract_mod_description_from_modinfo(modinfo_path):
        if not os.path.exists(modinfo_path):
            return ''
        try:
            import xml.etree.ElementTree as ET
            tree = ET.parse(modinfo_path)
            root = tree.getroot()
            desc_tag = root.find('Description')
            if desc_tag is not None and 'value' in desc_tag.attrib:
                return desc_tag.attrib['value']
        except Exception:
            pass
        return ''
    def extract_mod_details_block(readme_path):
        if not os.path.exists(readme_path):
            return ''
        with open(readme_path, 'r', encoding='utf-8') as f:
            content = f.read()
        start = content.find('<details>')
        if start == -1:
            return ''
        return content[start:]
    # Load template
    if not os.path.exists(MAIN_TEMPLATE_PATH):
        print('[MAIN README] TEMPLATE-MainReadMe.md not found, skipping main README generation.')
    else:
        with open(MAIN_TEMPLATE_PATH, 'r', encoding='utf-8') as f:
            main_template = f.read()
        # Update last updated field
        # Use Windows-compatible format string (no %-I)
        now_str = datetime.datetime.now().strftime('%B %d, %Y, %I:%M %p EST').lstrip('0').replace(' 0', ' ')
        main_content = main_template.replace('{{LAST_UPDATED}}', now_str)
        # Gather mod data
        # Only include mods from _Mods1.PublishReady in the main README
        all_mods = [f for f in os.listdir(PUBLISH_READY) if is_agf_mod(f) and os.path.isdir(os.path.join(PUBLISH_READY, f))]
        # Categorize mods
        def in_group(name, prefix):
            return name.startswith(prefix)
        backpackplus_mods = [f for f in all_mods if f.startswith('AGF-BackpackPlus-')]
        hudplus_mods = [f for f in all_mods if f.startswith('AGF-HUDPlus-')]
        hudpluszother_mods = [f for f in all_mods if f.startswith('AGF-HUDPluszOther-')]
        noeac_mods = [f for f in all_mods if f.startswith('AGF-NoEAC-')]
        vp_mods = [f for f in all_mods if f.startswith('AGF-VP-')]
        special_mods = [f for f in all_mods if f.startswith('zzzAGF-Special')]
        # Compose mod list markdown
        modlist_md = []
        # Example: Add Giggle Pack, HUD Plus, Backpack Plus, Special, Vanilla Plus, No EAC, HUDPluszOther
        # (A) GIGGLE PACK
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('<br>')
        modlist_md.append('')
        modlist_md.append('## **A. GIGGLE PACK**')
        modlist_md.append('')
        modlist_md.append('*[(Back to Top)](#agf-7-days-to-die-mods)*')
        modlist_md.append('')
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('[**⬇️ DOWNLOAD ALL AGF MODS**](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_Mods3.zip/GigglePack_All.zip)')
        modlist_md.append('')
        modlist_md.append('> - *All AGF mods in one convenient download.*')
        modlist_md.append('> - *Direct set-up is AGF preference and only server side mods*')
        modlist_md.append('> - *Client Side enhancements are in the NoEAC folder*')
        modlist_md.append('')
        modlist_md.append('---')
        modlist_md.append('')
        # (B) HUD PLUS MODS
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('<br>')
        modlist_md.append('')
        modlist_md.append('## **B. HUD PLUS MODS**')
        modlist_md.append('')
        modlist_md.append('*[(Back to Top)](#agf-7-days-to-die-mods)*')
        modlist_md.append('')
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('[**⬇️ Download All HUD Plus Mods**](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_Mods3.zip/HUDPlus_All.zip)')
        modlist_md.append('')
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('*Quality-of-life HUD enhancements and visual tweaks.*')
        modlist_md.append('')
        for mod in hudplus_mods:
            mod_path = os.path.join(PUBLISH_READY, mod)
            modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
            readme_path = os.path.join(mod_path, 'README.md')
            name, version = parse_modinfo(modinfo_path, mod)
            # Prettify mod name for display
            pretty_name = name.replace('-', ' ').replace('AGF ', 'AGF-').replace('  ', ' ').replace('AGF-', 'AGF ')
            link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_Mods3.zip/{mod.split("-v")[0]}.zip'
            # Use modinfo.xml description for summary
            desc = extract_mod_description_from_modinfo(modinfo_path)
            features = extract_mod_features(readme_path)
            # Compose blockquote with details if features exist
            modlist_md.append(f'> ### **{pretty_name}** *-v{version}* - [Download]({link})\n> *{desc}*')
            if features:
                try:
                    from update_mods import markdown_features_to_html
                except ImportError:
                    def markdown_features_to_html(features_text):
                        lines = [line.rstrip() for line in features_text.strip().splitlines() if line.strip()]
                        if not lines:
                            return ''
                        html = ''
                        stack = []
                        for line in lines:
                            indent = len(line) - len(line.lstrip(' '))
                            indent = indent + 4 * (len(line) - len(line.lstrip('\t')))
                            content = line.lstrip('-* ').strip()
                            while stack and indent < stack[-1]:
                                html += '</ul>'
                                stack.pop()
                            if not stack or indent > (stack[-1] if stack else 0):
                                html += '<ul>'
                                stack.append(indent)
                            html += f'<li>{content}</li>'
                        while stack:
                            html += '</ul>'
                            stack.pop()
                        return html
                features_html = markdown_features_to_html(features)
                modlist_md.append(f'> <details> <summary>*Show detailed features*</summary>\n>\n> {features_html}\n> \n> </details>')
            modlist_md.append('> \n---\n\n')
        # HUDPluszOther as table
        if hudpluszother_mods:
            # Add divider/spacing pattern before the table header
            modlist_md.append('---')
            modlist_md.append('')
            modlist_md.append('<br>')
            modlist_md.append('')
            modlist_md.append('### **Optional HUDPlus Tweaks** – [Download All](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_Mods3.zip/HUDPluszOther_All.zip)')
            modlist_md.append('')
            modlist_md.append('| Display Name | Version | Download | Description |')
            modlist_md.append('|---|---|---|---|')
            for mod in hudpluszother_mods:
                mod_path = os.path.join(PUBLISH_READY, mod)
                modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
                readme_path = os.path.join(mod_path, 'README.md')
                name, version = parse_modinfo(modinfo_path, mod)
                link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_Mods3.zip/{mod.split("-v")[0]}.zip'
                desc = extract_mod_description_from_modinfo(modinfo_path)
                modlist_md.append(f'| {name} | {version} | [Download]({link}) | {desc} |')
            # Add a blank line after the table
            modlist_md.append('')
            modlist_md.append('---')
        # (C) BACKPACK PLUS MODS
        # Add two --- and a <br> before the section header
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('<br>')
        modlist_md.append('')
        modlist_md.append('## **C. BACKPACK PLUS MODS**\n')
        modlist_md.append('*[(Back to Top)](#agf-7-days-to-die-mods)*')
        modlist_md.append('')
        modlist_md.append('[**⬇️ Download All Backpack Plus Mods**](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/BackpackPlus_All.zip)')
        modlist_md.append('')
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('*Download all above or select one below.*')
        modlist_md.append('')
        for mod in backpackplus_mods:
            mod_path = os.path.join(PUBLISH_READY, mod)
            modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
            readme_path = os.path.join(mod_path, 'README.md')
            name, version = parse_modinfo(modinfo_path, mod)
            link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/{mod.split("-v")[0]}.zip'
            desc = extract_mod_description_from_modinfo(modinfo_path)
            features = extract_mod_features(readme_path)
            modlist_md.append(f'> ### **{name}** *-v{version}* - [Download]({link})\n> *{desc}*')
            if features:
                try:
                    from update_mods import markdown_features_to_html
                except ImportError:
                    def markdown_features_to_html(features_text):
                        lines = [line.rstrip() for line in features_text.strip().splitlines() if line.strip()]
                        if not lines:
                            return ''
                        html = ''
                        stack = []
                        for line in lines:
                            indent = len(line) - len(line.lstrip(' '))
                            indent = indent + 4 * (len(line) - len(line.lstrip('\t')))
                            content = line.lstrip('-* ').strip()
                            while stack and indent < stack[-1]:
                                html += '</ul>'
                                stack.pop()
                            if not stack or indent > (stack[-1] if stack else 0):
                                html += '<ul>'
                                stack.append(indent)
                            html += f'<li>{content}</li>'
                        while stack:
                            html += '</ul>'
                            stack.pop()
                        return html
                features_html = markdown_features_to_html(features)
                modlist_md.append(f'> <details> <summary>*Show detailed features*</summary>\n>\n> {features_html}\n> \n> </details>')
            modlist_md.append('> \n---\n')
        # (D) SPECIAL COMPATIBILITY MOD
        if special_mods:
            modlist_md.append('---')
            modlist_md.append('')
            modlist_md.append('<br>')
            modlist_md.append('')
            modlist_md.append('## **D. SPECIAL COMPATIBILITY MOD**')
            modlist_md.append('*[(Back to Top)](#agf-7-days-to-die-mods)*')
            modlist_md.append('')
            modlist_md.append('---')
            modlist_md.append('')
            for mod in special_mods:
                mod_path = os.path.join(PUBLISH_READY, mod)
                modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
                readme_path = os.path.join(mod_path, 'README.md')
                name, version = parse_modinfo(modinfo_path, mod)
                link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/{mod.split("-v")[0]}.zip'
                desc = extract_mod_description_from_modinfo(modinfo_path)
                features = extract_mod_features(readme_path)
                modlist_md.append(f'> ### **{name}** *-v{version}* - [Download]({link})\n> *{desc}*')
                if features:
                    try:
                        from update_mods import markdown_features_to_html
                    except ImportError:
                        def markdown_features_to_html(features_text):
                            lines = [line.rstrip() for line in features_text.strip().splitlines() if line.strip()]
                            if not lines:
                                return ''
                            html = ''
                            stack = []
                            for line in lines:
                                indent = len(line) - len(line.lstrip(' '))
                                indent = indent + 4 * (len(line) - len(line.lstrip('\t')))
                                content = line.lstrip('-* ').strip()
                                while stack and indent < stack[-1]:
                                    html += '</ul>'
                                    stack.pop()
                                if not stack or indent > (stack[-1] if stack else 0):
                                    html += '<ul>'
                                    stack.append(indent)
                                html += f'<li>{content}</li>'
                            while stack:
                                html += '</ul>'
                                stack.pop()
                            return html
                    features_html = markdown_features_to_html(features)
                    modlist_md.append(f'> <details> <summary>*Show detailed features*</summary>\n>\n> {features_html}\n> \n> </details>')
                modlist_md.append('> \n---\n')
        # (E) VANILLA PLUS MODS
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('<br>')
        modlist_md.append('')
        modlist_md.append('## **E. VANILLA PLUS MODS**')
        modlist_md.append('*[(Back to Top)](#agf-7-days-to-die-mods)*')
        modlist_md.append('')
        modlist_md.append('[**⬇️ Download All VP Mods**](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/VP_All.zip)')
        modlist_md.append('')
        modlist_md.append('---')
        modlist_md.append('')
        modlist_md.append('*Vanilla Plus: gameplay tweaks and new features.*')
        modlist_md.append('')
        for mod in vp_mods:
            mod_path = os.path.join(PUBLISH_READY, mod)
            modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
            readme_path = os.path.join(mod_path, 'README.md')
            name, version = parse_modinfo(modinfo_path, mod)
            link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_Mods3.zip/{mod.split("-v")[0]}.zip'
            desc = extract_mod_description_from_modinfo(modinfo_path)
            features = extract_mod_features(readme_path)
            modlist_md.append(f'> ### **{name}** *-v{version}* - [Download]({link})\n> *{desc}*')
            if features:
                try:
                    from update_mods import markdown_features_to_html
                except ImportError:
                    def markdown_features_to_html(features_text):
                        lines = [line.rstrip() for line in features_text.strip().splitlines() if line.strip()]
                        if not lines:
                            return ''
                        html = ''
                        stack = []
                        for line in lines:
                            indent = len(line) - len(line.lstrip(' '))
                            indent = indent + 4 * (len(line) - len(line.lstrip('\t')))
                            content = line.lstrip('-* ').strip()
                            while stack and indent < stack[-1]:
                                html += '</ul>'
                                stack.pop()
                            if not stack or indent > (stack[-1] if stack else 0):
                                html += '<ul>'
                                stack.append(indent)
                            html += f'<li>{content}</li>'
                        while stack:
                            html += '</ul>'
                            stack.pop()
                        return html
                features_html = markdown_features_to_html(features)
                modlist_md.append(f'> <details> <summary>*Show detailed features*</summary>\n>\n> {features_html}\n> \n> </details>')
            modlist_md.append('> \n---\n')
        # (F) NO EAC MODS
        if noeac_mods:
            modlist_md.append('---')
            modlist_md.append('')
            modlist_md.append('<br>')
            modlist_md.append('')
            modlist_md.append('## **F. NO EAC MODS**')
            modlist_md.append('*[(Back to Top)](#agf-7-days-to-die-mods)*')
            modlist_md.append('')
            modlist_md.append('[**⬇️ Download All NoEAC Mods**](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/AGF-NoEAC_All.zip)')
            modlist_md.append('')
            modlist_md.append('---')
            modlist_md.append('')
            modlist_md.append('*Mods that require EAC to be off or are client-side only.*')
            modlist_md.append('')
            for mod in noeac_mods:
                mod_path = os.path.join(PUBLISH_READY, mod)
                modinfo_path = os.path.join(mod_path, 'ModInfo.xml')
                readme_path = os.path.join(mod_path, 'README.md')
                name, version = parse_modinfo(modinfo_path, mod)
                link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/{mod.split("-v")[0]}.zip'
                desc = extract_mod_description_from_modinfo(modinfo_path)
                features = extract_mod_features(readme_path)
                modlist_md.append(f'> ### **{name}** *-v{version}* - [Download]({link})\n> *{desc}*')
                if features:
                    try:
                        from update_mods import markdown_features_to_html
                    except ImportError:
                        def markdown_features_to_html(features_text):
                            lines = [line.rstrip() for line in features_text.strip().splitlines() if line.strip()]
                            if not lines:
                                return ''
                            html = ''
                            stack = []
                            for line in lines:
                                indent = len(line) - len(line.lstrip(' '))
                                indent = indent + 4 * (len(line) - len(line.lstrip('\t')))
                                content = line.lstrip('-* ').strip()
                                while stack and indent < stack[-1]:
                                    html += '</ul>'
                                    stack.pop()
                                if not stack or indent > (stack[-1] if stack else 0):
                                    html += '<ul>'
                                    stack.append(indent)
                                html += f'<li>{content}</li>'
                            while stack:
                                html += '</ul>'
                                stack.pop()
                            return html
                    features_html = markdown_features_to_html(features)
                    modlist_md.append(f'> <details> <summary>*Show detailed features*</summary>\n>\n> {features_html}\n> \n> </details>')
                modlist_md.append('> \n---\n')
        # Insert mod list into template
        modlist_str = '\n'.join(modlist_md)
        import re
        main_content = re.sub(r'<!-- MOD_LIST_START -->(.*?)<!-- MOD_LIST_END -->', f'<!-- MOD_LIST_START -->\n{modlist_str}\n<!-- MOD_LIST_END -->', main_content, flags=re.DOTALL)
        # Write README.md
        with open(MAIN_README_PATH, 'w', encoding='utf-8') as f:
            f.write(main_content)
        # Remove all <li>></li> lines from README.md
        with open(MAIN_README_PATH, 'r', encoding='utf-8') as f:
            content = f.read()
        # Remove all <li>></li> substrings from the file
        content = content.replace('<li>></li>', '')
        # Ensure every '---' line has a blank line above it (unless already present)
        lines = content.splitlines()
        new_lines = []
        for i, line in enumerate(lines):
            if line.strip() == '---':
                if i > 0 and lines[i-1].strip() != '':
                    new_lines.append('')
            new_lines.append(line)
        content = '\n'.join(new_lines)
        with open(MAIN_README_PATH, 'w', encoding='utf-8') as f:
            f.write(content)
        print('[MAIN README] Main README.md generated and cleaned of <li>></li>.')
        # Do NOT generate ReadableReadMe.txt for the main README
        # (If you want to generate ReadableReadMe.txt for individual mods, that logic should be elsewhere)