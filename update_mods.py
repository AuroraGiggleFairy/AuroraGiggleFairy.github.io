import datetime
import re
# --- Quotes Loader ---
def update_last_updated(files, date_str=None):
    """
    Replace the *{{LAST_UPDATED}}* placeholder with today's date in the same italic style.
    """
    if date_str is None:
        # Format: Month D, YYYY (e.g., March 9, 2026)
        try:
            date_str = datetime.date.today().strftime('%B %-d, %Y')
        except:
            date_str = datetime.date.today().strftime('%B %#d, %Y')
        # Windows compatibility for day formatting
        if re.match(r'.*%[-#]d.*', date_str) is not None:
            date_str = datetime.date.today().strftime('%B %d, %Y').replace(' 0', ' ')
    replacement = f'*Last Updated {date_str}*'
    for file in files:
        try:
            with open(file, 'r', encoding='utf-8') as f:
                content = f.read()
            # Only replace the placeholder (with or without italics); do nothing if not found
            new_content, n = re.subn(r'\*\{\{LAST_UPDATED\}\}\*', replacement, content, flags=re.IGNORECASE)
            if n > 0:
                with open(file, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"[INFO] Updated *{{LAST_UPDATED}}* in {file}")
            else:
                print(f"[INFO] No *{{LAST_UPDATED}}* placeholder found in {file}; no changes made.")
        except Exception as e:
            print(f"[WARN] Could not update *{{LAST_UPDATED}}* in {file}: {e}")


def load_quotes(path='TEMPLATE-Quotes.md'):
    if not os.path.exists(path):
        return {}, {}
    with open(path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    section_quotes = {}
    mod_quotes = {}
    in_mod_section = False
    import re
    for line in lines:
        line = line.strip()
        if line.startswith('# Mod Quotes'):
            in_mod_section = True
            continue
        def extract_bracketed(text):
            match = re.search(r'\[(.*?)\]', text)
            return match.group(1).strip() if match else ''
        if not in_mod_section:
            if ':' in line and not line.startswith('#') and line.split(':',1)[0].isupper():
                key, val = line.split(':', 1)
                section_quotes[key.strip().upper()] = extract_bracketed(val)
        else:
            if ':' in line and not line.startswith('#'):
                key, val = line.split(':', 1)
                mod_quotes[key.strip()] = extract_bracketed(val)
    return section_quotes, mod_quotes
# Add missing import
import os
# --- Category Descriptions ---
# --- Section Quotes ---
def load_category_descriptions(path='TEMPLATE-CategoryDescriptions.md'):
    descriptions = {}
    if not os.path.exists(path):
        return descriptions
    with open(path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    in_section = False
    current_key = None
    current_lines = []
    for line in lines:
        line = line.rstrip('\n')
        if line.strip().startswith('[category_descriptions]'):
            in_section = True
            continue
        if in_section:
            cat_match = re.match(r'^\[(.+)\]$', line.strip())
            if cat_match:
                if current_key and current_lines:
                    descriptions[current_key] = '\n'.join(current_lines).strip()
                current_key = cat_match.group(1).strip().upper()
                current_lines = []
            elif current_key is not None:
                if line.strip() == '' and current_lines:
                    # Blank line signals end of current category
                    descriptions[current_key] = '\n'.join(current_lines).strip()
                    current_key = None
                    current_lines = []
                else:
                    current_lines.append(line)
    # Add last category if file doesn't end with blank line
    if current_key and current_lines:
        descriptions[current_key] = '\n'.join(current_lines).strip()
    return descriptions

category_descriptions = load_category_descriptions()

import re

import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path
import os
import csv

def load_compatibility_csv(csv_path='mod_compatibility.csv'):
    """
    Loads the compatibility CSV into a dictionary keyed by MOD_NAME (folder name).
    """
    compat = {}
    if not os.path.exists(csv_path):
        return compat
    with open(csv_path, newline='', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            name = row.get('MOD_NAME', '').strip()
            if name:
                compat[name] = row
    return compat

compat_data = load_compatibility_csv()

# Function to update each mod's README.md using the template
def update_readme(folder, template):
    xml_path = os.path.join(folder, 'ModInfo.xml')
    readme_path = os.path.join(folder, 'README.md')
    # Parse modinfo.xml
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
        name_tag = root.find('DisplayName')
        if name_tag is None:
            name_tag = root.find('Name')
        version_tag = root.find('Version')
        name = name_tag.attrib['value'] if name_tag is not None and 'value' in name_tag.attrib else folder
        version = version_tag.attrib['value'] if version_tag is not None and 'value' in version_tag.attrib else "0.0.0"
    except Exception:
        name = folder
        version = "0.0.0"

    # Read existing README.md if present
    existing = ''
    if os.path.exists(readme_path):
        with open(readme_path, 'r', encoding='utf-8') as f:
            existing = f.read()

    # Helper to get value after colon for a label
    def get_field(label):
        pattern = re.compile(rf'^{re.escape(label)}:? ?(.*)$', re.MULTILINE)
        match = pattern.search(existing)
        if match:
            return match.group(1).strip()
        return ''

    # Extract the quote line (the first blockquote after version)
    def extract_quote(existing):
        lines = existing.splitlines()
        for i, line in enumerate(lines):
            if line.strip().startswith('**Version:**'):
                # Look for the next non-empty line that starts with '>'
                for j in range(i+1, min(i+4, len(lines))):
                    l = lines[j].strip()
                    if l.startswith('>'):
                        # Remove leading '>' and whitespace
                        quote = l[1:].strip()
                        return quote
                break
        return ''

    # Features and Changelog sections
    def extract_section(content, start_marker, end_marker):
        pattern = re.compile(rf'{re.escape(start_marker)}(.*?){re.escape(end_marker)}', re.DOTALL)
        match = pattern.search(content)
        if match:
            return match.group(1).strip()
        return ''

    features = extract_section(existing, '<!-- FEATURES START -->', '<!-- FEATURES END -->')
    changelog = extract_section(existing, '<!-- CHANGELOG START -->', '<!-- CHANGELOG END -->')

    # Use CSV compatibility data if available, fallback to existing or 'UPDATE'
    compat_row = compat_data.get(folder, {})
    def get_csv_or_existing(key, label, fallback='MISSINGDATA'):
        val = compat_row.get(key, '').strip()
        if val:
            return val
        return get_field(label) or fallback

    eac_friendly = get_csv_or_existing('EAC_FRIENDLY', 'EAC Friendly')
    server_side = get_csv_or_existing('SERVER_SIDE', 'Server Side')
    client_required = get_csv_or_existing('CLIENT_REQUIRED', 'Client Required for Multiplayer')
    safe_install = get_csv_or_existing('SAFE_TO_INSTALL', 'Safe to install on existing games')
    safe_remove = get_csv_or_existing('SAFE_TO_REMOVE', 'Safe to remove from an existing game')
    unique = get_csv_or_existing('UNIQUE', 'Unique Details', '')
    # Extract or blank the quote
    quote = extract_quote(existing)
    # If quote is not blank, ensure it is wrapped in single * at both ends
    if quote:
        if not (quote.startswith('*') and quote.endswith('*')):
            quote = f'*{quote.strip('* ')}*'

    # Fill template
    base_zip_name = re.sub(r'-v[0-9.]+$', '', folder)
    download_link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/{base_zip_name}.zip'
    readme = template
    readme = readme.replace('{{MOD_NAME}}', name)
    readme = readme.replace('{{MOD_VERSION}}', version)
    readme = readme.replace('{{DOWNLOAD_LINK}}', download_link)

    readme = readme.replace('{{EAC_FRIENDLY}}', eac_friendly)
    readme = readme.replace('{{SERVER_SIDE}}', server_side)
    readme = readme.replace('{{CLIENT_REQUIRED}}', client_required)
    readme = readme.replace('{{SAFE_TO_INSTALL}}', safe_install)
    readme = readme.replace('{{SAFE_TO_REMOVE}}', safe_remove)
    readme = readme.replace('{{UNIQUE}}', unique)
    readme = readme.replace('{{QUOTE}}', quote)

    readme = re.sub(r'<!-- FEATURES START -->.*?<!-- FEATURES END -->', f'<!-- FEATURES START -->\n{features}\n<!-- FEATURES END -->', readme, flags=re.DOTALL)
    readme = re.sub(r'<!-- CHANGELOG START -->.*?<!-- CHANGELOG END -->', f'<!-- CHANGELOG START -->\n{changelog}\n<!-- CHANGELOG END -->', readme, flags=re.DOTALL)

    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write(readme)
import os
# Main Logic
def get_mod_summary(folder):
    import xml.etree.ElementTree as ET
    xml_path = os.path.join(folder, 'ModInfo.xml')
    readme_path = os.path.join(folder, 'README.md')
    # Parse modinfo.xml
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
        name_tag = root.find('DisplayName')
        if name_tag is None:
            name_tag = root.find('Name')
        version_tag = root.find('Version')
        name = name_tag.attrib['value'] if name_tag is not None and 'value' in name_tag.attrib else folder
        version = version_tag.attrib['value'] if version_tag is not None and 'value' in version_tag.attrib else "0.0.0"
    except Exception:
        name = folder
        version = "0.0.0"

    # Download link (full GitHub Pages URL)
    base_zip_name = re.sub(r'-v[0-9.]+$', '', folder)
    download_link = f'https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/{base_zip_name}.zip'

    # Description/summary from ModInfo.xml
    description = ''
    try:
        desc_tag = root.find('Description')
        if desc_tag is not None and 'value' in desc_tag.attrib:
            description = desc_tag.attrib['value']
    except Exception:
        description = ''

    # Features section from README.md
    features = ''
    if os.path.exists(readme_path):
        with open(readme_path, 'r', encoding='utf-8') as f:
            content = f.read()
            match = re.search(r'<!-- FEATURES START -->(.*?)<!-- FEATURES END -->', content, re.DOTALL)
            if match:
                features = match.group(1).strip()
    if not features:
        features = '_No features listed._'

    # Preserve the original indentation of each line in the features section
    features_lines = features.splitlines()
    formatted_features = ''
    for line in features_lines:
        formatted_features += f"{line}\n"

    # Use TEMPLATE-ModListEntry.md (lines 3-8) for each mod entry
    modlistentry_template = ''
    with open('TEMPLATE-ModListEntry.md', 'r', encoding='utf-8') as f:
        lines = f.readlines()
        # Lines 3-8 (1-based) = lines[2:8] (0-based)
        modlistentry_template = ''.join(lines[2:8])

    # Prepare replacements
    entry = modlistentry_template
    entry = entry.replace('{{MOD_NAME}}', name)
    entry = entry.replace('{{MOD_VERSION}}', version)
    entry = entry.replace('{{DOWNLOAD_LINK}}', download_link)
    entry = entry.replace('{{DESCRIPTION}}', description if description else '_No description available._')
    # Format features so only the first line is unquoted, rest are prefixed with '> '
    features_lines = formatted_features.rstrip().split('\n')
    if features_lines:
        quoted_features = features_lines[0] + '\n'
        if len(features_lines) > 1:
            quoted_features += '\n'.join('> ' + line if line.strip() else '>' for line in features_lines[1:])
    else:
        quoted_features = '_No features listed._'
    entry = entry.replace('{{FEATURES}}', quoted_features)
    return entry + '\n---'

with open('TEMPLATE-Mod_ReadMe.md', 'r', encoding='utf-8') as f:
    template = f.read()


# --- Zipping logic ---
def zip_folder(folder, zip_path):
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, _, files in os.walk(folder):
            for file in files:
                file_path = Path(root) / file
                arcname = file_path.relative_to(folder.parent)
                zipf.write(file_path, arcname)


# Get all mod folders
folders = sorted([f for f in os.listdir('.') if os.path.isdir(f) and (f.startswith('AGF-') or f.startswith('zzzAGF'))])
import os
# Identify NoEAC mods (must be after folders is defined)
NOEAC_MODS = [d for d in folders if d.startswith('AGF-NoEAC-')]

# Update all individual readmes
for folder in folders:
    update_readme(folder, template)

# --- Zipping individual mods ---
ZIPS_DIR = Path('_zip')
ZIPS_DIR.mkdir(exist_ok=True)
def get_versioned_folder_name(folder):
    xml_path = os.path.join(folder, 'ModInfo.xml')
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
        name_tag = root.find('DisplayName')
        if name_tag is None:
            name_tag = root.find('Name')
        version_tag = root.find('Version')
        name = name_tag.attrib['value'] if name_tag is not None and 'value' in name_tag.attrib else folder
        version = version_tag.attrib['value'] if version_tag is not None and 'value' in version_tag.attrib else "0.0.0"
    except Exception:
        name = folder
        version = "0.0.0"
    # Remove any existing version suffix from folder name
    base = re.sub(r'-v[0-9.]+$', '', folder)
    return f"{base}-v{version}"

for mod in folders:
    versioned_folder = get_versioned_folder_name(mod)
    # Always rename to the correct versioned folder if needed
    if mod != versioned_folder:
        # Remove any existing folder with the correct version (old version cleanup)
        if os.path.exists(versioned_folder):
            print(f"[INFO] Removing old versioned folder: {versioned_folder}")
            import shutil
            shutil.rmtree(versioned_folder)
        print(f"[INFO] Renaming {mod} -> {versioned_folder}")
        os.rename(mod, versioned_folder)
        # Update folders list so future pack zipping uses the new name
        folders[folders.index(mod)] = versioned_folder
        mod = versioned_folder
    zip_name = re.sub(r'-v[0-9.]+$', '', mod)  # Remove version for zip name
    zip_path = ZIPS_DIR / f'{zip_name}.zip'
    print(f'Zipping {mod} -> {zip_path}')
    zip_folder(Path(mod), zip_path)

# --- Zipping mod packs ---


# Custom logic for HUDPlus_All and GigglePack_All
HUDPLUS_MODS = [d for d in folders if 'HUDPlus' in d]
OTHER_MODS = [d for d in folders if d.startswith('AGF-HUDPlusOther-')]


# Separate BackpackPlus 84 slot from others
BACKPACKPLUS_84 = 'AGF-BackpackPlus-84Slots-v3.2.0'
BACKPACKPLUS_MODS = [d for d in folders if 'BackpackPlus' in d and d != BACKPACKPLUS_84]


# Pack definitions
PACKS = {
    'HUDPlus_All': (HUDPLUS_MODS, OTHER_MODS, '.Optionals - Other'),
    'BackpackPlus_All': (BACKPACKPLUS_MODS + [BACKPACKPLUS_84], [], None),
    'VP_All': ([d for d in folders if d.startswith('AGF-VP-')], [], None),
    'NoEAC_All': (NOEAC_MODS, [], None),
    # 'Other_All': (OTHER_MODS, [], None),
    'GigglePack_All': (
        [d for d in folders if not d.startswith('AGF-HUDPlusOther-') and (d not in BACKPACKPLUS_MODS) and (d not in NOEAC_MODS)],
        OTHER_MODS + BACKPACKPLUS_MODS + [BACKPACKPLUS_84] + NOEAC_MODS,
        None  # We'll handle optionals below
    ),
}
for pack_name, (main_mods, optional_mods, optional_folder) in PACKS.items():
    if not main_mods and not optional_mods:
        continue
    zip_path = ZIPS_DIR / f'{pack_name}.zip'
    print(f'Zipping pack {pack_name} -> {zip_path}')
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        # Add main mods at root
        for folder in main_mods:
            for root, _, files in os.walk(folder):
                for file in files:
                    file_path = Path(root) / file
                    arcname = Path(folder) / file_path.relative_to(folder)
                    zipf.write(file_path, arcname)
        # Add optional mods in the specified subfolder (for HUDPlus_All)
        if optional_mods and optional_folder:
            for folder in optional_mods:
                for root, _, files in os.walk(folder):
                    for file in files:
                        file_path = Path(root) / file
                        arcname = Path(optional_folder) / Path(folder) / file_path.relative_to(folder)
                        zipf.write(file_path, arcname)
        # Special handling for GigglePack_All: two optionals
        if pack_name == 'GigglePack_All':
            # Add HUDPlus mods and 'Other' mods in '.Optionals - HUDPlus'
            for folder in HUDPLUS_MODS + OTHER_MODS:
                for root, _, files in os.walk(folder):
                    for file in files:
                        file_path = Path(root) / file
                        arcname = Path('.Optionals - HUDPlus') / Path(folder) / file_path.relative_to(folder)
                        zipf.write(file_path, arcname)
            # Add BackpackPlus mods (including 84 slot) in '.Optionals - BackpackPlus'
            for folder in BACKPACKPLUS_MODS + [BACKPACKPLUS_84]:
                for root, _, files in os.walk(folder):
                    for file in files:
                        file_path = Path(root) / file
                        arcname = Path('.Optionals - BackpackPlus') / Path(folder) / file_path.relative_to(folder)
                        zipf.write(file_path, arcname)
            # Add NoEAC mods in '.Optionals - NoEAC'
            for folder in NOEAC_MODS:
                for root, _, files in os.walk(folder):
                    for file in files:
                        file_path = Path(root) / file
                        arcname = Path('.Optionals - NoEAC') / Path(folder) / file_path.relative_to(folder)
                        zipf.write(file_path, arcname)


giggle_pack_link = '[**⬇️ DOWNLOAD ALL AGF MODS**](https://AuroraGiggleFairy.github.io/_zip/GigglePack_All.zip)'

# Main Logic
with open('TEMPLATE-Mod_ReadMe.md', 'r', encoding='utf-8') as f:
    template = f.read()

# Categorize folders
folders = sorted([f for f in os.listdir('.') if os.path.isdir(f) and (f.startswith('AGF-') or f.startswith('zzzAGF'))])
categories = {'HUDPLUS': [], 'BACKPACKPLUS': [], 'SPECIAL': [], 'VP': [], 'NOEAC': [], 'OTHER': []}
for folder in folders:
    if folder.startswith('zzzAGF'):
        categories['SPECIAL'].append(folder)
    else:
        parts = folder.split('-')
        if len(parts) > 1:
            cat = parts[1].upper()
            if cat in categories:
                categories[cat].append(folder)
            else:
                categories['OTHER'].append(folder)
        else:
            categories['OTHER'].append(folder)

category_order = ['HUDPLUS', 'BACKPACKPLUS', 'SPECIAL', 'VP', 'NOEAC']
category_headers = {
    'HUDPLUS': '## **B. HUD PLUS MODS**',
    'BACKPACKPLUS': '## **C. BACKPACK PLUS MODS**',
    'SPECIAL': '## **D. SPECIAL COMPATIBILITY MOD**',
    'VP': '## **E. VANILLA PLUS MODS**',
    'NOEAC': '## **F. NO EAC MODS**',
    # 'OTHER': '## **OTHER MODS**',
}
def get_category_download_link(category, folders):
    if not folders:
        return ''
    zip_name = f'{category}_All.zip' if not category.endswith('_All') else f'{category}.zip'
    return f'[**\u2B07\uFE0F DOWNLOAD ALL {category.upper()} MODS**](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/{zip_name})'





# Load quotes for use in README generation
import sys
section_quotes, mod_quotes = load_quotes('TEMPLATE-Quotes.md')
print(f"[DEBUG] section_quotes['GIGGLE PACK']: {section_quotes.get('GIGGLE PACK')}")
sys.stdout.flush()

# Update all individual readmes
for folder in folders:
    update_readme(folder, template)


mod_list_block = '\n\n## **A. GIGGLE PACK**\n\n*[(Back to Top)](#agf-7-days-to-die-mods)*\n\n---\n\n[**\u2B07\uFE0F DOWNLOAD ALL AGF MODS**](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/GigglePack_All.zip)'
# Add category description for Giggle Pack if present
if 'GIGGLE PACK' in category_descriptions:
    mod_list_block += f"\n\n{category_descriptions['GIGGLE PACK']}"
# Add quote for Giggle Pack if present
if 'GIGGLE PACK' in section_quotes:
    quote = section_quotes['GIGGLE PACK']
    if quote:
        mod_list_block += f"\n\n> {quote}\n\n---"
    else:
        mod_list_block += "\n\n---"
else:
    mod_list_block += "\n\n---"


for cat in category_order:
    if cat in categories and categories[cat]:
        if cat == 'SPECIAL':
            # Custom header, no download all, no description, just the mod details
            mod_list_block += "\n\n---\n\n<br>\n\n## **D. SPECIAL COMPATIBILITY MOD**\n\n*[(Back to Top)](#agf-7-days-to-die-mods)*\n\n---\n\n"
            for folder in categories[cat]:
                try:
                    mod_list_block += '\n' + get_mod_summary(folder) + '\n'
                except Exception as e:
                    print(f"Skipping {folder} for summary: {e}")
            # Ensure a single blank line before the ending --- (no extra ---)
            # Remove any trailing whitespace and --- lines, then add exactly one blank line and one ---
            mod_list_block = re.sub(r'(\n*---\n*)+$', '', mod_list_block)
            mod_list_block = mod_list_block.rstrip() + '\n\n---\n'
            continue
        else:
            # HUDPLUSOTHER logic: insert after HUDPLUS
            if cat == 'HUDPLUS' and 'OTHER_MODS' in globals() and OTHER_MODS:
                # Insert HUDPLUS mods, then HUDPLUSOTHER as a sub-section
                mod_list_block += f"\n---\n\n<br>\n\n{category_headers[cat]}\n\n*[(Back to Top)](#agf-7-days-to-die-mods)*\n\n---\n\n{get_category_download_link(cat, categories[cat])}\n\n---\n\n"
                if cat in category_descriptions:
                    mod_list_block += f"\n{category_descriptions[cat]}\n"
                quote = ''
                header_key = category_headers[cat].replace('## **','').replace(' MODS**','').upper().strip()
                fallback_key = cat.replace('_',' ') + ' MODS'
                fallback_key2 = cat.replace('_',' ')
                # Normalize keys for section_quotes
                def norm_key(k):
                    return k.upper().replace('_', ' ').strip()
                quote = (
                    section_quotes.get(norm_key(header_key), '') or
                    section_quotes.get(norm_key(fallback_key), '') or
                    section_quotes.get(norm_key(fallback_key2), '') or
                    section_quotes.get(norm_key(cat), '') or
                    section_quotes.get(cat, '')
                )
                if quote:
                    mod_list_block += f"> {quote}\n"
                # Main HUDPLUS mods
                for folder in categories[cat]:
                    try:
                        mod_list_block += '\n' + get_mod_summary(folder) + '\n'
                    except Exception as e:
                        print(f"Skipping {folder} for summary: {e}")
                # HUDPLUSOTHER sub-section with required formatting
                mod_list_block += '\n---\n<br>\n\n### **Optional HUDPlus Tweaks**\n\n'
                mod_list_block += '| Display Name | Version | Download | Description |\n|---|---|---|---|'
                for folder in OTHER_MODS:
                    try:
                        import xml.etree.ElementTree as ET
                        xml_path = os.path.join(folder, 'ModInfo.xml')
                        name = folder
                        version = ''
                        description = ''
                        if os.path.exists(xml_path):
                            tree = ET.parse(xml_path)
                            root = tree.getroot()
                            name_tag = root.find('DisplayName')
                            if name_tag is None:
                                name_tag = root.find('Name')
                            if name_tag is not None and 'value' in name_tag.attrib:
                                name = name_tag.attrib['value']
                            version_tag = root.find('Version')
                            if version_tag is not None and 'value' in version_tag.attrib:
                                version = version_tag.attrib['value']
                            desc_tag = root.find('Description')
                            if desc_tag is not None and 'value' in desc_tag.attrib:
                                description = desc_tag.attrib['value']
                        download_link = f'https://AuroraGiggleFairy.github.io/zips/{folder}.zip'
                        mod_list_block += f"\n| {name} | {version} | [Download]({download_link}) | {description} |"
                    except Exception as e:
                        print(f"Skipping {folder} for condensed optionals: {e}")
                mod_list_block += '\n'
                continue
            # BackpackPlus custom sort: 119 slot last
            if cat == 'BACKPACKPLUS':
                sorted_folders = [f for f in categories[cat] if '119' not in f]
                sorted_folders += [f for f in categories[cat] if '119' in f]
            else:
                sorted_folders = categories[cat]
            mod_list_block += f"\n---\n\n<br>\n\n{category_headers[cat]}\n\n*[(Back to Top)](#agf-7-days-to-die-mods)*\n\n{get_category_download_link(cat, categories[cat])}\n\n---\n"
            if cat in category_descriptions:
                mod_list_block += f"\n{category_descriptions[cat]}\n"
            quote = ''
            header_key = category_headers[cat].replace('## **','').replace(' MODS**','').upper().strip()
            fallback_key = cat.replace('_',' ') + ' MODS'
            fallback_key2 = cat.replace('_',' ')
            quote = (
                section_quotes.get(header_key, '') or
                section_quotes.get(fallback_key.upper(), '') or
                section_quotes.get(fallback_key2.upper(), '') or
                section_quotes.get(cat.upper(), '') or
                section_quotes.get(cat, '')
            )
            if cat == 'GIGGLE PACK':
                # Insert triple-star line, then blockquote, then dashes
                mod_list_block += "\n***All AGF mods in one convenient download.***\n"
                # Normalize keys for section_quotes
                def norm_key(k):
                    return k.upper().replace('_', ' ').strip()
                gpack_quote = (
                    section_quotes.get(norm_key(header_key), '') or
                    section_quotes.get(norm_key(fallback_key), '') or
                    section_quotes.get(norm_key(fallback_key2), '') or
                    section_quotes.get(norm_key(cat), '') or
                    section_quotes.get(cat, '')
                )
                if gpack_quote:
                    mod_list_block += f"> {gpack_quote}\n"
                mod_list_block += "\n---\n---\n"
                continue
            if quote:
                mod_list_block += f"> {quote}\n"
            for folder in sorted_folders:
                try:
                    mod_list_block += '\n' + get_mod_summary(folder) + '\n'
                except Exception as e:
                    print(f"Skipping {folder} for summary: {e}")


# --- Generate README.md from template, replacing the date placeholder ---
main_readme_path = 'README.md'
with open('TEMPLATE-1Main.md', 'r', encoding='utf-8') as f:
    main_readme_content = f.read()


# Replace the placeholder with today's date and time in EST (Month D, YYYY, HH:MM AM/PM EST)
import pytz
from datetime import datetime
est = pytz.timezone('US/Eastern')
now_est = datetime.now(est)
# Always format as 'Month D, YYYY, HH:MM AM/PM EST' (remove leading zero from day)
month = now_est.strftime('%B')
day = str(now_est.day)
year = now_est.strftime('%Y')
time_str = now_est.strftime('%I:%M %p').lstrip('0')
date_str = f"{month} {day}, {year}, {time_str} EST"
date_line = f'*Last Updated {date_str}*'
main_readme_content = re.sub(r'\*\{\{LAST_UPDATED\}\}\*', date_line, main_readme_content, flags=re.IGNORECASE)

if '<!-- MOD_LIST_START -->' in main_readme_content:
    new_content = main_readme_content.replace('<!-- MOD_LIST_START -->', mod_list_block + '\n<!-- MOD_LIST_END -->')
else:
    new_content = main_readme_content.rstrip() + mod_list_block + '\n<!-- MOD_LIST_END -->'

with open(main_readme_path, 'w', encoding='utf-8') as f:
    f.write(new_content)

# --- Ensure TEMPLATE-Quotes.md is up to date with all mod names ---
def sync_mod_quotes_template(quotes_path='TEMPLATE-Quotes.md', folders=None):
    if folders is None:
        return
    # Read existing quotes file
    if not os.path.exists(quotes_path):
        lines = []
    else:
        with open(quotes_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
    # Find the start of the mod quotes section
    mod_quotes_start = None
    for i, line in enumerate(lines):
        if line.strip().startswith('# Mod Quotes'):
            mod_quotes_start = i
            break
    if mod_quotes_start is None:
        # Add section if missing
        lines.append('\n---\n\n# Mod Quotes\n# (Use the exact mod name as it appears in the README header)\n')
        mod_quotes_start = len(lines) - 1
    # Build a dict of existing mod quotes (ignore '# ...add more mods as needed' and comments)
    mod_quotes = {}
    for line in lines[mod_quotes_start+1:]:
        if ':' in line and not line.strip().startswith('#'):
            key, val = line.split(':', 1)
            mod_quotes[key.strip()] = val.rstrip('\n')
    # Get all mod display names
    import xml.etree.ElementTree as ET
    mod_names = []
    for folder in folders:
        xml_path = os.path.join(folder, 'ModInfo.xml')
        try:
            tree = ET.parse(xml_path)
            root = tree.getroot()
            name_tag = root.find('DisplayName')
            if name_tag is None:
                name_tag = root.find('Name')
            name = name_tag.attrib['value'] if name_tag is not None and 'value' in name_tag.attrib else folder
            mod_names.append(name.strip())
        except Exception:
            mod_names.append(folder.strip())
    # Update mod_quotes with any new mods
    updated = False
    for name in mod_names:
        if name not in mod_quotes:
            mod_quotes[name] = ''
            updated = True
    # Rebuild the mod quotes section (remove any trailing comments or old lines)
    new_mod_quotes_lines = []
    for name in sorted(mod_names):
        # Preserve existing quote if present
        quote = mod_quotes.get(name, '')
        new_mod_quotes_lines.append(f'{name}: {quote}\n')
    # Write back the updated file if needed
    before = lines[:mod_quotes_start+1]
    with open(quotes_path, 'w', encoding='utf-8') as f:
        f.writelines(before)
        f.write(''.join(new_mod_quotes_lines))
    print("[DEBUG] Mod names parsed for quotes:")
    for name in mod_names:
        print(f"  - {name}")
