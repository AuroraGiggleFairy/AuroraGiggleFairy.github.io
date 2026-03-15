"""
create_new_mod.py

Script to automate creation of a new mod folder with correct naming and initial files.
- Prompts for mod name (e.g., AGF-BackpackPlus-84Slots or zzzAGF-Special-Compatibilities)
- Creates folder as [mod name]-v1.0.0
- Generates ModInfo.xml and README.md with mod name and version
"""
import os
WORKSPACE_ROOT = os.path.dirname(os.path.abspath(__file__))
INPROGRESS_DIR = os.path.join(WORKSPACE_ROOT, "_In-Progress")
TEMPLATE_README_PATH = os.path.join(WORKSPACE_ROOT, "TEMPLATE-ModReadMe.md")
TEMPLATE_MODINFO = '''<?xml version="1.0" encoding="UTF-8" ?>\n<xml>\n    <Name value=\"{mod_name}\"/>\n    <DisplayName value=\"{display_name}\"/>\n    <Version value=\"{version}\"/>\n    <Description value=\"\"/>\n    <Author value=\"AuroraGiggleFairy (AGF)\"/>\n    <Website value=\"https://auroragigglefairy.github.io/\"/>\n</xml>\n'''

def main():
    mod_name = input("Enter new mod name (e.g., AGF-BackpackPlus-84Slots): ").strip()
    import re
    version = "0.0.1"
    def to_display_name(name):
        s = name.replace('-', ' ')
        s = re.sub(r'(?<=[a-z])(?=[A-Z])', ' ', s)
        s = re.sub(r'(?<=[A-Z])(?=[A-Z][a-z])', ' ', s)
        return s.strip()
    display_name = to_display_name(mod_name)
    folder_name = f"{mod_name}-v{version}"
    mod_path = os.path.join(INPROGRESS_DIR, folder_name)
    if os.path.exists(mod_path):
        print(f"Folder {folder_name} already exists in _In-Progress!")
        return
    os.makedirs(mod_path)
    # Add entry to HELPER_ModCompatibility.csv
    csv_path = os.path.join(WORKSPACE_ROOT, "HELPER_ModCompatibility.csv")
    if os.path.exists(csv_path):
        with open(csv_path, "a", encoding="utf-8") as csvfile:
            csvfile.write(f"\n{mod_name},MISSINGDATA,MISSINGDATA,MISSINGDATA,MISSINGDATA,MISSINGDATA,MISSINGDATA")
    # Create ModInfo.xml
    with open(os.path.join(mod_path, "ModInfo.xml"), "w", encoding="utf-8") as f:
        f.write(TEMPLATE_MODINFO.format(mod_name=mod_name, display_name=display_name, version=version))
    # Create README.md from template
    if os.path.exists(TEMPLATE_README_PATH):
        with open(TEMPLATE_README_PATH, "r", encoding="utf-8") as t:
            readme_content = t.read()
        # Replace placeholders
        readme_content = readme_content.replace("{{MOD_NAME}}", mod_name)
        readme_content = readme_content.replace("{{MOD_VERSION}}", version)
        readme_content = readme_content.replace("{{DOWNLOAD_LINK}}", "[Add link later]")
        readme_content = readme_content.replace("{{QUOTE}}", "[Add quote later]")
        # Set all compatibility fields to MISSINGDATA
        for field in [
            "EAC_FRIENDLY", "SERVER_SIDE", "CLIENT_REQUIRED",
            "SAFE_TO_INSTALL", "SAFE_TO_REMOVE", "UNIQUE"
        ]:
            readme_content = readme_content.replace(f"{{{{{field}}}}}", "MISSINGDATA")
        # Remove template feature/changelog placeholder lines
        for remove_line in [
            '[Add or keep features here. Automation should preserve this section.]',
            '[Add or keep changelog entries here. Automation should preserve this section.]'
        ]:
            readme_content = readme_content.replace(remove_line + '\n', '')
            readme_content = readme_content.replace('\n' + remove_line, '')
            readme_content = readme_content.replace(remove_line, '')

        # Insert initial changelog entry
        changelog_marker = "<!-- CHANGELOG START -->"
        changelog_entry = "v0.0.1\n- Mod first created.\n"
        if changelog_marker in readme_content:
            parts = readme_content.split(changelog_marker)
            before = parts[0] + changelog_marker + "\n"
            after = parts[1]
            # Only insert if not already present
            if changelog_entry not in after:
                after = changelog_entry + after.lstrip('\n')
            readme_content = before + after
        with open(os.path.join(mod_path, "README.md"), "w", encoding="utf-8") as f:
            f.write(readme_content)
    else:
        with open(os.path.join(mod_path, "README.md"), "w", encoding="utf-8") as f:
            f.write(f"# {mod_name}\n\nVersion: {version}\n")

    # Create Config and XUi folders (case sensitive)
    config_path = os.path.join(mod_path, "Config")
    xui_path = os.path.join(config_path, "XUi")
    os.makedirs(xui_path)
    # Create Localization.txt with correct header row
    localization_path = os.path.join(config_path, "Localization.txt")
    localization_header = "Key,File,Type,UsedInMainMenu,NoTranslate,english,Context / Alternate Text,german,spanish,french,italian,japanese,koreana,polish,brazilian,russian,turkish,schinese,tchinese\n"
    with open(localization_path, "w", encoding="utf-8") as f:
        f.write(localization_header)

    print(f"Created new mod folder: {folder_name}")
    print("You can now edit ModInfo.xml, README.md, and Config files as needed.")

if __name__ == "__main__":
    main()

