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
COMPAT_CSV_PATH = os.path.join(
    WORKSPACE_ROOT,
    "Workflow",
    "ReadmeSystem",
    "Data",
    "HELPER_ModCompatibility.csv",
)
QUOTES_DIR = os.path.join(WORKSPACE_ROOT, "_Quotes")
TEMPLATE_MODINFO = '''<?xml version="1.0" encoding="UTF-8" ?>\n<xml>\n    <Name value=\"{mod_name}\"/>\n    <DisplayName value=\"{display_name}\"/>\n    <Version value=\"{version}\"/>\n    <Description value=\"\"/>\n    <Author value=\"AuroraGiggleFairy (AGF)\"/>\n    <Website value=\"https://auroragigglefairy.github.io/\"/>\n</xml>\n'''


VERSION_PATTERN = re.compile(r"^\d+\.\d+\.\d+$")


def build_initial_readme_md(mod_name: str, version: str) -> str:
    return f"""# {mod_name}
7d2d Version MISSINGDATA
**Version:** {version}
[Download]([Add link later])

[Add quote later]

---
---

## README TABLE OF CONTENTS
1. About AGF
2. Need Help?
3. Install Scope & EAC Requirement
4. Compatibility
5. Features Summary
6. Other Details
7. Changelog

---
---

## 1. About AGF
- My name is AuroraGiggleFairy (AGF).
- I have been modding 7 Days to Die for 7 years.
- I do my best to prioritize accessibility, user-friendliness, and localization where possible.
- I provide kind, comprehensive support to players, modders, and server communities, and I rely on community feedback to keep improving my mods.

---
---

## 2. Need Help?
- Join AGF's Discord for support: https://discord.gg/Vm5eyW6N4r

---
---

## 3. Install Scope & EAC Requirement
- TBD

---
---

## 4. Compatibility
- Last 7d2d Version tested on: MISSINGDATA
- \"{mod_name}\" is SAFE to install on an existing game: MISSINGDATA
- \"{mod_name}\" is SAFE to remove from an existing game: MISSINGDATA
- Unique Details: MISSINGDATA

---
---

## 5. Features Summary
<!-- FEATURES-SUMMARY START -->
- See this mod's README for full details.
<!-- FEATURES-SUMMARY END -->

---
---

## 6. Other Details
<!-- FEATURES-DETAILED START -->
- Add or remove as needed.
<!-- FEATURES-DETAILED END -->

---
---

## 7. Changelog
<!-- CHANGELOG START -->
v{version}
- Mod first created.
<!-- CHANGELOG END -->
"""


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
        new_row[mod_type_idx] = "TBD"
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

    # Add entry to Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv
    # (without duplicating existing MOD_NAME rows)
    append_compatibility_row(mod_name)
    # Create default quote file in _Quotes if it does not exist yet
    ensure_quote_file(mod_name)

    # Create ModInfo.xml
    with open(os.path.join(mod_path, "ModInfo.xml"), "w", encoding="utf-8") as f:
        f.write(TEMPLATE_MODINFO.format(mod_name=mod_name, display_name=display_name, version=version))

    # Create README.md scaffold used by readme migration/publish pipelines.
    with open(os.path.join(mod_path, "README.md"), "w", encoding="utf-8") as f:
        f.write(build_initial_readme_md(mod_name, version))

    # Create Config and XUi_InGame folders (case sensitive)
    config_path = os.path.join(mod_path, "Config")
    xui_path = os.path.join(config_path, "XUi_InGame")
    os.makedirs(xui_path)
    # Create Localization.csv with the current draft schema
    localization_path = os.path.join(config_path, "Localization.csv")
    localization_header = "Key,File,Type,UsedInMainMenu,NoTranslate,KeepLoaded,english,Context / Alternate Text,german,spanish,french,italian,japanese,koreana,polish,brazilian,russian,turkish,schinese,tchinese\n"
    with open(localization_path, "w", encoding="utf-8") as f:
        f.write(localization_header)

    print(f"Created new mod folder: {folder_name}")
    print(f"Location: {mod_path}")
    print("You can now edit ModInfo.xml, README.md, and Config files as needed.")

if __name__ == "__main__":
    main()

