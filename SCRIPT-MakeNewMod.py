"""
create_new_mod.py

Script to automate creation of a new mod folder with correct naming and initial files.
- Prompts for mod name (e.g., AGF-BackpackPlus-84Slots or zzzAGF-Special-Compatibilities)
- Creates folder as [mod name]-v1.0.0
- Generates ModInfo.xml and README.md with mod name and version
"""
import csv
import argparse
import os
import re


WORKSPACE_ROOT = os.path.dirname(os.path.abspath(__file__))


def resolve_lane_path(preferred: str, legacy: str) -> str:
    if os.path.isdir(preferred):
        return preferred
    if os.path.isdir(legacy):
        return legacy
    return preferred


INPROGRESS_DIR = resolve_lane_path(
    os.path.join(WORKSPACE_ROOT, "01_Draft"),
    os.path.join(WORKSPACE_ROOT, "_Mods2.In-Progress"),
)
TEMPLATE_README_PATH = os.path.join(WORKSPACE_ROOT, "TEMPLATE-ModReadMes.md")
COMPAT_CSV_PATH = os.path.join(WORKSPACE_ROOT, "HELPER_ModCompatibility.csv")
QUOTES_DIR = os.path.join(WORKSPACE_ROOT, "_Quotes")
TEMPLATE_MODINFO = '''<?xml version="1.0" encoding="UTF-8" ?>\n<xml>\n    <Name value=\"{mod_name}\"/>\n    <DisplayName value=\"{display_name}\"/>\n    <Version value=\"{version}\"/>\n    <Description value=\"\"/>\n    <Author value=\"AuroraGiggleFairy (AGF)\"/>\n    <Website value=\"https://auroragigglefairy.github.io/\"/>\n</xml>\n'''


VERSION_PATTERN = re.compile(r"^\d+\.\d+\.\d+$")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Create a new mod folder scaffold in the in-progress lane."
    )
    parser.add_argument(
        "--name",
        help="Mod name, e.g. AGF-BackpackPlus-84Slots or zzzAGF-Special-Compatibilities",
    )
    parser.add_argument(
        "--version",
        default="0.0.1",
        help="Initial mod version in SemVer format (default: 0.0.1)",
    )
    parser.add_argument(
        "--non-interactive",
        action="store_true",
        help="Fail instead of prompting when required inputs are missing",
    )
    return parser.parse_args()


def to_display_name(name: str) -> str:
    s = name.replace('-', ' ')
    s = re.sub(r'(?<=[a-z])(?=[A-Z])', ' ', s)
    s = re.sub(r'(?<=[A-Z])(?=[A-Z][a-z])', ' ', s)
    return s.strip()


def is_valid_mod_name(mod_name: str) -> bool:
    return mod_name.startswith("AGF-") or mod_name.startswith("zzzAGF-")


def ensure_section_line(content: str, start_marker: str, end_marker: str, line: str, at_start: bool) -> str:
    pattern = re.compile(
        rf"({re.escape(start_marker)}\r?\n)(.*?)?(\r?\n{re.escape(end_marker)})",
        re.DOTALL,
    )
    match = pattern.search(content)
    if not match:
        return content

    body = match.group(2) or ""
    lines = body.splitlines()
    filtered = [ln for ln in lines if ln.strip() != line]

    if at_start:
        new_body_lines = [line] + filtered
    else:
        new_body_lines = filtered + [line]

    new_body = "\r\n".join(new_body_lines)
    replacement = f"{match.group(1)}{new_body}{match.group(3)}"
    return content[: match.start()] + replacement + content[match.end() :]


def append_compatibility_row(mod_name: str) -> None:
    if not os.path.exists(COMPAT_CSV_PATH):
        return

    with open(COMPAT_CSV_PATH, "r", encoding="utf-8-sig", newline="") as f:
        reader = csv.reader(f)
        header = next(reader, None)
        rows = list(reader)

    if not header:
        return

    fieldnames = [str(name).lstrip("\ufeff").strip() for name in header]
    if not fieldnames or "MOD_NAME" not in fieldnames:
        return

    mod_name_idx = fieldnames.index("MOD_NAME")
    existing_mods = {
        str(row[mod_name_idx]).strip()
        for row in rows
        if len(row) > mod_name_idx
    }
    if mod_name in existing_mods:
        return

    new_row = ["TBD" for _ in fieldnames]
    new_row[mod_name_idx] = mod_name
    if "MOD_TYPE_ID" in fieldnames:
        mod_type_idx = fieldnames.index("MOD_TYPE_ID")
        new_row[mod_type_idx] = "0"
    if "QUOTE_FILE" in fieldnames:
        quote_idx = fieldnames.index("QUOTE_FILE")
        new_row[quote_idx] = f"{mod_name}.txt"

    with open(COMPAT_CSV_PATH, "a", encoding="utf-8", newline="") as f:
        writer = csv.writer(f, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(new_row)


def ensure_quote_file(mod_name: str) -> None:
    os.makedirs(QUOTES_DIR, exist_ok=True)
    quote_file_path = os.path.join(QUOTES_DIR, f"{mod_name}.txt")
    if os.path.exists(quote_file_path):
        return

    with open(quote_file_path, "w", encoding="utf-8", newline="") as f:
        f.write("")


def main():
    args = parse_args()

    mod_name = (args.name or "").strip()
    if not mod_name:
        if args.non_interactive:
            print("--name is required when --non-interactive is set.")
            return
        mod_name = input("Enter new mod name (e.g., AGF-BackpackPlus-84Slots): ").strip()

    if not mod_name:
        print("Mod name cannot be empty.")
        return

    if not is_valid_mod_name(mod_name):
        print("Mod name must start with AGF- or zzzAGF-.")
        return

    version = args.version.strip()
    if not VERSION_PATTERN.match(version):
        print("Version must be in SemVer format: X.Y.Z")
        return

    display_name = to_display_name(mod_name)
    folder_name = f"{mod_name}-v{version}"
    mod_path = os.path.join(INPROGRESS_DIR, folder_name)

    if os.path.exists(mod_path):
        append_compatibility_row(mod_name)
        ensure_quote_file(mod_name)
        print(f"Folder {folder_name} already exists in {INPROGRESS_DIR}.")
        print("Compatibility CSV row and quote file were verified/backfilled.")
        return

    os.makedirs(INPROGRESS_DIR, exist_ok=True)
    os.makedirs(mod_path)

    # Add entry to HELPER_ModCompatibility.csv (without duplicating existing MOD_NAME rows)
    append_compatibility_row(mod_name)
    # Create default quote file in _Quotes if it does not exist yet
    ensure_quote_file(mod_name)

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
            "TESTED_GAME_VERSION", "EAC_FRIENDLY", "SERVER_SIDE_PLAYER", "SERVER_SIDE_DEDICATED", "CLIENT_SIDE",
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

        # Enforce default README section lines for newly created mods.
        readme_content = ensure_section_line(
            readme_content,
            "<!-- FEATURES-SUMMARY START -->",
            "<!-- FEATURES-SUMMARY END -->",
            "- See this mod's README for full details.",
            at_start=False,
        )
        readme_content = ensure_section_line(
            readme_content,
            "<!-- FEATURES-DETAILED START -->",
            "<!-- FEATURES-DETAILED END -->",
            "- Works standalone.",
            at_start=True,
        )

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
    print(f"Location: {mod_path}")
    print("You can now edit ModInfo.xml, README.md, and Config files as needed.")

if __name__ == "__main__":
    main()

