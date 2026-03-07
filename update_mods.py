# Add missing import
import os
# --- Category Descriptions ---
def load_category_descriptions(path='TEMPLATE-CategoryDescriptions.md'):
    descriptions = {}
    if not os.path.exists(path):
        return descriptions
    with open(path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    in_section = False
    for line in lines:
        line = line.strip()
        if line.startswith('[category_descriptions]'):
            in_section = True
            continue
        if in_section and '=' in line:
            key, val = line.split('=', 1)
            descriptions[key.strip().upper()] = val.strip()
    return descriptions

category_descriptions = load_category_descriptions()

import re
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path
import os

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
        pattern = re.compile(rf'{re.escape(label)}:? ?(.*)')
        match = pattern.search(existing)
        if match:
            return match.group(1).strip()
        return ''
    # Always define these variables, even if README is missing

    # Features and Changelog sections
    def extract_section(content, start_marker, end_marker):
        pattern = re.compile(rf'{re.escape(start_marker)}(.*?){re.escape(end_marker)}', re.DOTALL)
        match = pattern.search(content)
        if match:
            return match.group(1).strip()
        return ''

    features = extract_section(existing, '<!-- FEATURES START -->', '<!-- FEATURES END -->')
    changelog = extract_section(existing, '<!-- CHANGELOG START -->', '<!-- CHANGELOG END -->')

    # Fill template
    readme = template
    readme = readme.replace('{{MOD_NAME}}', name)
    readme = readme.replace('{{MOD_VERSION}}', version)

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
    download_link = f'https://AuroraGiggleFairy.github.io/zips/{folder}.zip'

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

    # Format features as a bulleted list with proper indentation
    features_lines = features.splitlines()
    formatted_features = ''
    for line in features_lines:
        if line.strip().startswith('- '):
            formatted_features += f"{line.strip()}\n"
        elif line.strip().startswith('  - '):
            formatted_features += f"  {line.strip()}\n"
        elif line.strip():
            formatted_features += f"- {line.strip()}\n"

    # Special handling for AGF Compatibility Mod (SPECIAL category)
    if folder.startswith('zzzAGF-'):
        # Use only the category description for SPECIAL section, formatted to match other categories
        special_desc = category_descriptions.get('SPECIAL', '')
        summary = ''
        if special_desc:
            summary += f"**{special_desc}**\n\n---\n---"
        else:
            summary += '---\n---'
        summary += f"\n| Version: {version} | [Download]({download_link}) |\n|---|---|\n"
        formatted_features = formatted_features.rstrip() + "\n<br>\n<br>\n"
        summary += f"\n{formatted_features}"
        return summary
    # Default for all other mods
    summary = f"---\n### **{name}**\n"
    if description:
        summary += f"*{description}*\n"
    summary += f"\n| Version: {version} | [Download]({download_link}) |\n|---|---|\n"
    formatted_features = formatted_features.rstrip() + "\n<br>\n<br>\n"
    summary += f"\n{formatted_features}"
    return summary

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
ZIPS_DIR = Path('zips')
ZIPS_DIR.mkdir(exist_ok=True)
for mod in folders:
    zip_path = ZIPS_DIR / f'{mod}.zip'
    print(f'Zipping {mod} -> {zip_path}')
    zip_folder(Path(mod), zip_path)

# --- Zipping mod packs ---


# Custom logic for HUDPlus_All and GigglePack_All
HUDPLUS_MODS = [d for d in folders if 'HUDPlus' in d]
OTHER_MODS = [d for d in folders if d.startswith('AGF-Other-')]


# Separate BackpackPlus 84 slot from others
BACKPACKPLUS_84 = 'AGF-BackpackPlus-84Slots-v3.2.0'
BACKPACKPLUS_MODS = [d for d in folders if 'BackpackPlus' in d and d != BACKPACKPLUS_84]

PACKS = {
    'HUDPlus_All': (HUDPLUS_MODS, OTHER_MODS, '.Optionals - Other'),
    'BackpackPlus_All': (BACKPACKPLUS_MODS + [BACKPACKPLUS_84], [], None),
    'VP_All': ([d for d in folders if d.startswith('AGF-VP-')], [], None),
    'NoEAC_All': (NOEAC_MODS, [], None),
    'Other_All': (OTHER_MODS, [], None),
    'GigglePack_All': (
        [d for d in folders if not d.startswith('AGF-Other-') and (d not in BACKPACKPLUS_MODS) and (d not in NOEAC_MODS)],
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


giggle_pack_link = '[**⬇️ DOWNLOAD ALL AGF MODS**](https://AuroraGiggleFairy.github.io/zips/GigglePack_All.zip)'

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

category_order = ['HUDPLUS', 'BACKPACKPLUS', 'SPECIAL', 'VP', 'NOEAC', 'OTHER']
category_headers = {
    'HUDPLUS': '## **HUDPLUS MODS**',
    'BACKPACKPLUS': '## **BACKPACKPLUS MODS**',
    'SPECIAL': '## **SPECIAL MODS**',
    'VP': '## **VANILLA PLUS MODS**',
    'NOEAC': '## **NOEAC MODS**',
    'OTHER': '## **OTHER MODS**',
}
def get_category_download_link(category, folders):
    if not folders:
        return ''
    zip_name = f'{category}_All.zip' if not category.endswith('_All') else f'{category}.zip'
    return f'[**\u2B07\uFE0F DOWNLOAD ALL {category.upper()} MODS**](https://AuroraGiggleFairy.github.io/zips/{zip_name})'

# Update all individual readmes
for folder in folders:
    update_readme(folder, template)

mod_list_block = '\n\n# Mod List\n\n## **GIGGLE PACK**\n[**\u2B07\uFE0F DOWNLOAD ALL AGF MODS**](https://AuroraGiggleFairy.github.io/zips/GigglePack_All.zip)'
if 'GIGGLE PACK' in category_descriptions:
    mod_list_block += f"\n*{category_descriptions['GIGGLE PACK']}*"

for cat in category_order:
    if cat in categories and categories[cat]:
        if cat == 'SPECIAL':
            # Custom header, no download all, no description, just the mod details
            mod_list_block += "\n\n## **AGF Compatibility Mod**\n"
            for folder in categories[cat]:
                try:
                    mod_list_block += '\n' + get_mod_summary(folder) + '\n'
                except Exception as e:
                    print(f"Skipping {folder} for summary: {e}")
            continue
        else:
            mod_list_block += f"\n\n{category_headers[cat]}\n{get_category_download_link(cat, categories[cat])}\n\n"
            if cat in category_descriptions:
                mod_list_block += f"**{category_descriptions[cat]}**\n\n---"
            else:
                mod_list_block += "---"
            cat_folders = categories[cat]
            # For BackpackPlus, move 119Slots mod to the end
            if cat == 'BACKPACKPLUS':
                backpack119 = [f for f in cat_folders if '119Slots' in f]
                others = [f for f in cat_folders if '119Slots' not in f]
                cat_folders = others + backpack119
            # For HUDPLUS, list HUDPlus mods, then a subsection for 'Other' mods
            if cat == 'HUDPLUS':
                hudplus_mods = [f for f in cat_folders if 'HUDPlus' in f]
                other_mods = [f for f in categories['OTHER']]
                for folder in hudplus_mods:
                    try:
                        mod_list_block += '\n' + get_mod_summary(folder) + '\n'
                    except Exception as e:
                        print(f"Skipping {folder} for summary: {e}")
                if other_mods:
                    mod_list_block += '\n### **Optional HUDPlus Tweaks**'
                    mod_list_block += '\n| Display Name | Version | Download | Description |\n|---|---|---|---|'
                    for folder in other_mods:
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
            for folder in cat_folders:
                try:
                    mod_list_block += '\n' + get_mod_summary(folder) + '\n'
                except Exception as e:
                    print(f"Skipping {folder} for summary: {e}")

main_readme_path = 'README.md'
with open('TEMPLATE-1Main.md', 'r', encoding='utf-8') as f:
    main_readme_content = f.read()
print("\n--- DEBUG: TEMPLATE-1Main.md CONTENT ---\n")
print(main_readme_content[:500])  # Print first 500 chars for brevity
print("\n--- END DEBUG ---\n")

if '<!-- MOD_LIST_START -->' in main_readme_content:
    new_content = main_readme_content.replace('<!-- MOD_LIST_START -->', mod_list_block)
else:
    # Fallback: append at the end
    new_content = main_readme_content.rstrip() + mod_list_block

with open(main_readme_path, 'w', encoding='utf-8') as f:
    f.write(new_content)
