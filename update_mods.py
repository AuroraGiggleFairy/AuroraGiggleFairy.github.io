import re
import xml.etree.ElementTree as ET

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

    eac_friendly = get_field('EAC Friendly')
    server_side = get_field('Server Side')
    client_required = get_field('Client Required for Multiplayer')
    safe_install = get_field('Safe to install on existing games')
    safe_remove = get_field('Safe to remove from an existing game')

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
    readme = re.sub(r'EAC Friendly: *\n', f'EAC Friendly: {eac_friendly}\n', readme)
    readme = re.sub(r'Server Side: *\n', f'Server Side: {server_side}\n', readme)
    readme = re.sub(r'Client Required for Multiplayer: *\n', f'Client Required for Multiplayer: {client_required}\n', readme)
    readme = re.sub(r'Safe to install on existing games: *\n', f'Safe to install on existing games: {safe_install}\n', readme)
    readme = re.sub(r'Safe to remove from an existing game: *\n', f'Safe to remove from an existing game: {safe_remove}\n', readme)
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

    # Download link (assuming GitHub zip link)
    download_link = f'https://github.com/AuroraGiggleFairy/{folder}/archive/refs/heads/main.zip'

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

    summary = f"---\n### **{name}**\n"
    if description:
        summary += f"*{description}*\n"
    # Markdown table for version and download link (no 'Direct')
    summary += f"\n| Version: {version} | [Download]({download_link}) |\n|---|---|\n"
    # Ensure a true blank line after the features list
    formatted_features = formatted_features.rstrip() + "\n<br>\n<br>\n"
    summary += f"\n{formatted_features}"
    return summary

with open('MOD_README_TEMPLATE.md', 'r', encoding='utf-8') as f:
    template = f.read()

folders = sorted([f for f in os.listdir('.') if os.path.isdir(f) and (f.startswith('AGF-') or f.startswith('zzzAGF'))])
for folder in folders:
    update_readme(folder, template)

# After updating all individual readmes, generate the mod summary list
mod_summaries = []
for folder in folders:
    try:
        mod_summaries.append(get_mod_summary(folder))
    except Exception as e:
        print(f"Skipping {folder} for summary: {e}")

# Insert or append the mod list to the main README.md
main_readme_path = 'README.md'
with open('TEMPLATE.md', 'r', encoding='utf-8') as f:
    main_readme_content = f.read()

mod_list_block = '\n\n## Mod List\n' + '\n\n'.join(mod_summaries) + '\n'

if '<!-- MOD_LIST_START -->' in main_readme_content:
    new_content = main_readme_content.replace('<!-- MOD_LIST_START -->', mod_list_block)
else:
    # Fallback: append at the end
    new_content = main_readme_content.rstrip() + mod_list_block


    # Fill template
    readme = template
    readme = readme.replace('{{MOD_NAME}}', name)
    readme = readme.replace('{{MOD_VERSION}}', version)
    readme = re.sub(r'EAC Friendly: *\n', f'EAC Friendly: {eac_friendly}\n', readme)
    readme = re.sub(r'Server Side: *\n', f'Server Side: {server_side}\n', readme)
    readme = re.sub(r'Client Required for Multiplayer: *\n', f'Client Required for Multiplayer: {client_required}\n', readme)
    readme = re.sub(r'Safe to install on existing games: *\n', f'Safe to install on existing games: {safe_install}\n', readme)
    readme = re.sub(r'Safe to remove from an existing game: *\n', f'Safe to remove from an existing game: {safe_remove}\n', readme)
    readme = re.sub(r'<!-- FEATURES START -->.*?<!-- FEATURES END -->', f'<!-- FEATURES START -->\n{features}\n<!-- FEATURES END -->', readme, flags=re.DOTALL)
    readme = re.sub(r'<!-- CHANGELOG START -->.*?<!-- CHANGELOG END -->', f'<!-- CHANGELOG START -->\n{changelog}\n<!-- CHANGELOG END -->', readme, flags=re.DOTALL)

    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write(readme)

# Main Logic
with open('MOD_README_TEMPLATE.md', 'r', encoding='utf-8') as f:
    template = f.read()



 # Categorize folders
folders = sorted([f for f in os.listdir('.') if os.path.isdir(f) and (f.startswith('AGF-') or f.startswith('zzzAGF'))])
categories = {'HUDPlus': [], 'BackpackPlus': [], 'Special': [], 'VP': [], 'NoEAC': [], 'Other': []}
for folder in folders:
    if folder.startswith('zzzAGF'):
        categories['Special'].append(folder)
    else:
        parts = folder.split('-')
        if len(parts) > 1:
            cat = parts[1]
            if cat in categories:
                categories[cat].append(folder)
            else:
                categories['Other'].append(folder)
        else:
            categories['Other'].append(folder)

category_order = ['HUDPlus', 'BackpackPlus', 'Special', 'VP', 'NoEAC', 'Other']

category_headers = {
    'HUDPlus': '## HUDPlus Mods',
    'BackpackPlus': '## BackpackPlus Mods',
    'Special': '## Special Mods',
    'VP': '## VP Mods',
    'NoEAC': '## NoEAC Mods',
    'Other': '## Other Mods',
}

def get_category_download_link(category, folders):
    if not folders:
        return ''
    # Create a zip link for all folders in the category using GitHub's tree download (not natively supported for subfolders, so instruct user to download from repo or provide a script)
    # For now, provide a message and fallback to main repo zip
    return f'[**Download All {category} Mods**](https://github.com/AuroraGiggleFairy/7D2D-Mods/archive/refs/heads/main.zip)'

# Update all individual readmes
for folder in folders:
    update_readme(folder, template)


# Build mod summaries by category, with Giggle Pack at the top
giggle_pack_link = '[**Download All Mods (Giggle Pack)**](https://github.com/AuroraGiggleFairy/7D2D-Mods/archive/refs/heads/main.zip)'
mod_list_block = '\n\n# Mod List\n\n## Giggle Pack\nDownload all mods in one ZIP:\n' + giggle_pack_link + '\n'
for cat in category_order:
    if categories[cat]:
        mod_list_block += f"\n\n{category_headers[cat]}\nDownload all {cat} mods in one ZIP (contains all mods):\n{get_category_download_link(cat, categories[cat])}\n"
        cat_folders = categories[cat]
        # For BackpackPlus, move Backpack119 mod to the end
        if cat == 'BackpackPlus':
            backpack119 = [f for f in cat_folders if 'Backpack119' in f]
            others = [f for f in cat_folders if 'Backpack119' not in f]
            cat_folders = others + backpack119
        for folder in cat_folders:
            try:
                mod_list_block += '\n' + get_mod_summary(folder) + '\n'
            except Exception as e:
                print(f"Skipping {folder} for summary: {e}")

main_readme_path = 'README.md'
with open('TEMPLATE.md', 'r', encoding='utf-8') as f:
    main_readme_content = f.read()

if '<!-- MOD_LIST_START -->' in main_readme_content:
    new_content = main_readme_content.replace('<!-- MOD_LIST_START -->', mod_list_block)
else:
    # Fallback: append at the end
    new_content = main_readme_content.rstrip() + mod_list_block

with open(main_readme_path, 'w', encoding='utf-8') as f:
    f.write(new_content)
