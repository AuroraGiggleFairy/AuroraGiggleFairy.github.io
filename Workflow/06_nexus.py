"""
Workflow Step 6 — Nexus Mods PublishHelp artifact generation.

All data from local sources (README, ModInfo.xml, config).
No API calls needed.
"""

import argparse
import datetime as dt
import json
import os
import re
import shutil
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True  # never leave a __pycache__ behind

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
VS_CODE_ROOT = os.path.normpath(os.path.join(WORKFLOW_DIR, ".."))
# NEXUS_ROOT_DIR holds the API key + generated output (PublishHelp/ModDetails).
# NEXUS_WORKFLOW_DIR holds the scripts/config/templates (06_PublishingSupport/NexusMods/Workflow).
NEXUS_ROOT_DIR = os.path.join(VS_CODE_ROOT, "06_PublishingSupport", "NexusMods")
NEXUS_WORKFLOW_DIR = os.path.join(NEXUS_ROOT_DIR, "Workflow")
NEXUS_CONFIG = os.path.join(NEXUS_WORKFLOW_DIR, "nexusmods-config.json")
RELEASE_SOURCE_DIR = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
PUBLISHHELP_DIR = os.path.join(NEXUS_ROOT_DIR, "PublishHelp")
MODDETAILS_DIR = os.path.join(NEXUS_ROOT_DIR, "ModDetails")
TEMPLATE_DETAILS_PATH = os.path.join(NEXUS_WORKFLOW_DIR, "TEMPLATE-Details.md")
NEXUS_MODGUIDE_PATH = os.path.join(
    VS_CODE_ROOT, "05_GigglePackReleaseData", "ReadmeSystem", "Snippets", "Nexus-MODGUIDE-md-Snippet.md"
)

AGF_PREFIXES = ("AGF-", "zzzAGF-")

# BBCode color formatting constants
AGF_COLOR_LINE = "#5F5980"
AGF_COLOR_HEADING = "#8DB580"
AGF_COLOR_HIGHLIGHT = "#DDCDFA"
AGF_DIVIDER = "[color=#5F5980]──────────────────────────────────────────────────────────────[/color]"


def get_base_mod_name(name: str) -> str:
    return re.sub(r"-v\d+(?:\.\d+)*$", "", name)


def extract_version(folder_path: str) -> str:
    """Extract version from ModInfo.xml."""
    path = os.path.join(folder_path, "ModInfo.xml")
    if not os.path.isfile(path):
        return "?"
    try:
        tree = ET.parse(path)
        for child in tree.getroot():
            if child.tag.lower() == "version":
                return child.attrib.get("value", "?")
    except Exception:
        pass
    return "?"


def load_modinfo_xml(folder_path: str) -> dict:
    """Load ModInfo.xml and return tag values as a dict."""
    result = {"name": "", "display_name": "", "version": "?", "description": ""}
    path = os.path.join(folder_path, "ModInfo.xml")
    if not os.path.isfile(path):
        return result
    try:
        tree = ET.parse(path)
        for child in tree.getroot():
            tag = child.tag.lower()
            val = child.attrib.get("value", "").strip()
            if tag == "name":
                result["name"] = val
            elif tag == "displayname":
                result["display_name"] = val
            elif tag == "version":
                result["version"] = val
            elif tag == "description":
                result["description"] = val
    except Exception:
        pass
    return result


def load_readme_text(folder_path: str) -> str:
    """Load README.txt from the mod folder, preferring readable version."""
    paths = [
        os.path.join(folder_path, "README.txt"),
    ]
    for p in paths:
        if os.path.isfile(p):
            try:
                with open(p, "r", encoding="utf-8-sig") as f:
                    return f.read()
            except Exception:
                pass
    return ""


def extract_mod_type_from_readme(readme_text: str) -> str:
    """Extract 'Mod Type' line from the MOD SCOPE section."""
    if not readme_text:
        return ""
    match = re.search(
        r"MOD SCOPE\s*[-=]+\s*(.*?)(?=\n\s*[-=]+\s*\n\s*(?:FEATURES|OTHER DETAILS))",
        readme_text, re.IGNORECASE | re.DOTALL
    )
    if not match:
        return ""
    scope_block = match.group(1)
    for line in scope_block.splitlines():
        stripped = line.strip().lstrip("- ")
        if stripped.lower().startswith("mod type:"):
            return stripped.split(":", 1)[1].strip()
    return ""


def extract_changelog_entries(readme_text: str) -> str:
    """Extract changelog as formatted text for the template.
    Format: ### header followed by ```text code block.
    Lines that don't start with - are wrapped continuations of the previous bullet."""
    if not readme_text:
        return ""
    # Find CHANGELOG section header
    match = re.search(
        r"CHANGELOG\s*[-=]+", readme_text, re.IGNORECASE
    )
    if not match:
        return ""
    changelog_text = readme_text[match.end():]
    # Parse version blocks (handle both v and V prefixes)
    blocks = re.split(r"\n(?=\s*(?:v|V)\d+(?:\.\d+)+)", changelog_text)
    result_lines = []
    for block in blocks:
        block = block.strip()
        if not block:
            continue
        # Check if it starts with a version
        version_match = re.match(r"([vV]\d+(?:\.\d+)+)", block)
        if not version_match:
            continue
        version_tag = version_match.group(1).lower()
        result_lines.append(f"### {version_tag}")
        result_lines.append("```text")
        # Get lines after the version
        body_lines = block[len(version_match.group(0)):].strip().splitlines()
        bullets: list[str] = []
        current = ""
        for line in body_lines:
            stripped = line.strip()
            if not stripped or re.match(r"^-{2,}$", stripped):
                # Skip pure divider lines like "-------"
                continue
            if stripped.startswith("- "):
                if current:
                    bullets.append(current)
                current = stripped[2:].strip()
            elif current:
                # Continuation of previous bullet - join with space
                current = current + " " + stripped
            else:
                current = stripped
        if current:
            bullets.append(current)
        if bullets:
            for b in bullets:
                result_lines.append(b)
        result_lines.append("```")
        result_lines.append("")
    return "\n".join(result_lines).strip()


def format_nexus_mod_name(base_name: str, game_version: str) -> str:
    """Format eg 'AGF-HUDPlus-1Main' -> 'AGF - V3 - HUDPlus - 1Main'."""
    clean = base_name
    if clean.startswith("AGF-"):
        clean = clean[4:]
    elif clean.startswith("zzzAGF-"):
        clean = clean[7:]
    parts = [p for p in clean.split("-") if p]
    display_parts = [p for p in parts if p]
    joined = " - ".join(display_parts)
    if game_version:
        return f"AGF - V{game_version} - {joined}"
    return f"AGF - {joined}"


def load_mod_type_map() -> dict:
    """Load the Nexus MOD TYPE snippet file and build a lookup dict.
    The snippet format is:
      MOD TYPE 1
      Server-Side (EAC-Friendly): full description...
    Maps the label before ':' (e.g. 'Server-Side (EAC-Friendly)') to the full description line."""
    snippet_path = os.path.join(VS_CODE_ROOT, "05_GigglePackReleaseData", "ReadmeSystem", "Snippets", "Nexus-MODTYPE-md-Snippet.md")
    mod_type_map = {}
    if not os.path.isfile(snippet_path):
        return mod_type_map
    try:
        with open(snippet_path, "r", encoding="utf-8") as f:
            content = f.read()
        for block in content.strip().split("\n\n"):
            lines = block.strip().splitlines()
            if len(lines) >= 2:
                # Second line has the full description; extract label before ":"
                desc = lines[1].strip()
                label = desc.split(":")[0].strip() if ":" in desc else desc
                if label:
                    mod_type_map[label] = desc
    except Exception:
        pass
    return mod_type_map


def resolve_file_description(short_desc: str, mod_type_label: str, mod_type_map: dict) -> str:
    """Build the file description string using Mod Type description from the snippet if available."""
    if mod_type_label:
        full_desc = mod_type_map.get(mod_type_label, "")
        if full_desc:
            return f"{short_desc}\n\nMod Type: {full_desc}"
        return f"{short_desc}\n\nMod Type: {mod_type_label}"
    return short_desc


def generate_details_md(template_text: str, mod_info: dict, nexus_mod_name: str, short_desc: str, file_desc: str, changelog: str) -> str:
    """Substitute placeholders in the template with actual values."""
    now = dt.datetime.now().strftime("%Y-%m-%d  %I:%M %p")
    replacements = {
        "{{MOD_NAME}}": mod_info.get("name", ""),
        "{{MOD_VERSION}}": mod_info.get("version", "?"),
        "{{GENERATED_TIMESTAMP}}": now,
        "{{NEXUS_MOD_NAME}}": nexus_mod_name,
        "{{SHORT_DESCRIPTION}}": short_desc,
        "{{SHORT_DESC_LENGTH}}": str(len(short_desc)),
        "{{FILE_DESCRIPTION}}": file_desc,
        "{{FILE_DESC_LENGTH}}": str(len(file_desc)),
        "{{CHANGELOG_ENTRIES}}": changelog,
    }
    result = template_text
    for placeholder, value in replacements.items():
        result = result.replace(placeholder, value)
    return result


def parse_readme_sections(readme_text: str) -> dict:
    """Extract MOD SCOPE, FEATURES, OTHER DETAILS, and AGF MOD GUIDE from a standard AGF README."""
    sections: dict = {}
    if not readme_text:
        return sections
    text = readme_text.replace("\r\n", "\n").replace("\r", "\n").strip()
    scope_match = re.search(
        r"MOD SCOPE\s*[-=]+\s*(.*?)(?=\n\s*[-=]+\s*\n\s*(?:FEATURES|OTHER DETAILS))",
        text, re.IGNORECASE | re.DOTALL
    )
    if scope_match:
        sections["mod_scope"] = scope_match.group(1).strip()
    features_match = re.search(
        r"FEATURES\s*[-=]+\s*(.*?)(?=\n\s*[-=]+\s*\n\s*(?:OTHER DETAILS|AGF MOD GUIDE))",
        text, re.IGNORECASE | re.DOTALL
    )
    if features_match:
        sections["features"] = features_match.group(1).strip()
    other_match = re.search(
        r"OTHER DETAILS\s*[-=]+\s*(.*?)(?=\n\s*[-=]+\s*\n\s*(?:AGF MOD GUIDE))",
        text, re.IGNORECASE | re.DOTALL
    )
    if other_match:
        sections["other_details"] = other_match.group(1).strip()
    guide_match = re.search(
        r"AGF MOD GUIDE\s*[-=]+\s*(.*?)(?:\n\s*[-=]+\s*\n\s*(?:CHANGELOG)|$)",
        text, re.IGNORECASE | re.DOTALL
    )
    if guide_match:
        sections["mod_guide"] = guide_match.group(1).strip()
    return sections


def generate_bbcode_full_description(nexus_mod_name: str, game_ver: str, description: str, sections: dict) -> str:
    """Generate a branded BBCode full description for a mod."""
    one_liner = description.split(".")[0] + "." if "." in description else description
    lines: list = []

    def w(text: str = ""):
        lines.append(text)

    # nexus_mod_name already includes full formatted name like "AGF - V3 - HUDPlus - 1Main"
    w(AGF_DIVIDER)
    w(AGF_DIVIDER)
    w(f"[color={AGF_COLOR_HEADING}][size=6][b]{nexus_mod_name}[/b][/size][/color]")
    w()
    w(f"[size=4]{one_liner}[/size]")
    w()
    w(AGF_DIVIDER)

    # MOD SCOPE
    scope = sections.get("mod_scope", "")
    if scope:
        w(f"[heading][color={AGF_COLOR_HEADING}][size=5][b]Mod Scope[/b][/size][/color][/heading]")
        w("[list]")
        pending_sub_items: list = []
        collecting_deps = False  # Flag: collecting sub-items for Dependencies
        for line in scope.splitlines():
            stripped = line.strip()
            if not stripped or stripped.startswith("- Mod Version:"):
                continue
            if stripped.lstrip("- ").startswith("To restore, use Steam Verify:"):
                # This is a sub-item of Dependencies with a colon; treat as sub-item
                content = stripped.lstrip("- ").strip()
                if content:
                    pending_sub_items.append(content)
                continue
            if collecting_deps and stripped.startswith("- ") and ":" in stripped.lstrip("- "):
                # While collecting deps, treat colon lines as sub-items, not labels
                content = stripped.lstrip("- ").strip()
                if content:
                    pending_sub_items.append(content)
                continue
            if ":" in stripped.lstrip("- "):
                # Flush any pending sub-items before starting a new entry
                if pending_sub_items:
                    w("[list]")
                    for si in pending_sub_items:
                        w(f"[*]{si}[/*]")
                    w("[/list]")
                    w("[/*]")
                    pending_sub_items = []
                label, value = stripped.lstrip("- ").split(":", 1)
                label_s = label.strip()
                value_s = value.strip()
                if label_s == "Website":
                    continue
                elif label_s == "Mod Type":
                    w(f"[*][color={AGF_COLOR_HIGHLIGHT}][b]{label_s}:[/b][/color] {value_s}")
                    # Mod Type sub-items follow; collect them
                elif label_s == "Dependencies":
                    collecting_deps = True
                    w(f"[*][color={AGF_COLOR_HIGHLIGHT}][b]{label_s}:[/b][/color] {value_s}")
                    continue
                else:
                    collecting_deps = False
                    w(f"[*][color={AGF_COLOR_HIGHLIGHT}][b]{label_s}:[/b][/color] {value_s}[/*]")
            elif stripped.startswith("- "):
                # Sub-item under Mod Type or Dependencies
                content = stripped.lstrip("- ").strip()
                if content:
                    pending_sub_items.append(content)
        # Flush any remaining sub-items at end of list
        if pending_sub_items:
            w("[list]")
            for si in pending_sub_items:
                w(f"[*]{si}[/*]")
            w("[/list]")
            w("[/*]")
        w("[/list]")
        w()
        w(AGF_DIVIDER)

    # OTHER DETAILS (extracted early so FEATURES can check if it exists)
    other = sections.get("other_details", "")

    # FEATURES
    features = sections.get("features", "")
    if features:
        w(f"[heading][color={AGF_COLOR_HEADING}][size=5][b]Features[/b][/size][/color][/heading]")
        w("[list]")
        feat_current = ""
        for raw_line in features.splitlines():
            stripped = raw_line.strip()
            if not stripped:
                continue
            if stripped.startswith("- ") or stripped.startswith("-"):
                if feat_current:
                    w(f"[*]{feat_current.strip()}[/*]")
                feat_current = stripped.lstrip("- ")
            elif feat_current:
                feat_current += " " + stripped
            else:
                feat_current = stripped
        if feat_current:
            w(f"[*][size=4]{feat_current.strip()}[/size][/*]")
        w("[/list]")
        # Only add separator divider when OTHER DETAILS follows (otherwise MODGUIDE snippet provides its own dividers)
        if other:
            w()
            w(AGF_DIVIDER)

    # OTHER DETAILS
    other = sections.get("other_details", "")
    if other:
        w(f"[heading][color={AGF_COLOR_HEADING}][size=4][b]Other Details[/b][/size][/color][/heading]")
        w("[list]")
        current_bullet = ""
        for raw_line in other.splitlines():
            stripped = raw_line.strip()
            if not stripped:
                continue
            if stripped.startswith("- ") or stripped.startswith("-"):
                content = stripped.lstrip("- ")
                if content.startswith("- "):
                    current_bullet += (" " + content.lstrip("- "))
                elif current_bullet:
                    w(f"[*]{current_bullet.strip()}[/*]")
                    current_bullet = content
                else:
                    current_bullet = content
            elif current_bullet:
                # Continuation of previous bullet - join with space
                current_bullet += " " + stripped
            else:
                current_bullet = stripped
        if current_bullet:
            w(f"[*]{current_bullet.strip()}[/*]")
        w("[/list]")

    # Append the MODGUIDE snippet (Ask for Help, Mod Guide, Support, and footer)
    # Add blank line before MODGUIDE snippet's two divider lines
    w()
    # Note: snippet already starts with its own divider lines, so no lead-in divider is added
    if os.path.isfile(NEXUS_MODGUIDE_PATH):
        with open(NEXUS_MODGUIDE_PATH, "r", encoding="utf-8") as f:
            guide_snippet = f.read()
        w(guide_snippet.strip())
    else:
        print(f"  [WARN] Mod Guide snippet not found: {NEXUS_MODGUIDE_PATH}")
    return "\n".join(lines)


def find_mod_entries() -> list:
    """Scan 03_ReleaseSource for AGF mods with Nexus config intents."""
    config = {}
    if os.path.isfile(NEXUS_CONFIG):
        with open(NEXUS_CONFIG, "r", encoding="utf-8") as f:
            cfg = json.load(f)
            config = cfg.get("mods", {}) if isinstance(cfg, dict) else {}

    entries = []
    if not os.path.isdir(RELEASE_SOURCE_DIR):
        return entries

    for folder in sorted(os.listdir(RELEASE_SOURCE_DIR)):
        full_path = os.path.join(RELEASE_SOURCE_DIR, folder)
        if not os.path.isdir(full_path) or not folder.startswith(AGF_PREFIXES):
            continue
        base_name = get_base_mod_name(folder)
        cfg_entry = config.get(base_name, {})
        if not isinstance(cfg_entry, dict):
            cfg_entry = {}
        intent = str(cfg_entry.get("intent", "review")).strip().lower()
        entries.append({
            "base_name": base_name,
            "folder_path": full_path,
            "folder_name": folder,
            "intent": intent,
        })
    return entries


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Step 6 — Generate Nexus Mods PublishHelp documentation files"
    )
    parser.add_argument(
        "--only",
        choices=["all", "publish", "update", "review", "skip"],
        default="all",
    )
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    print("=" * 60)
    print("  STEP 6 — NEXUS MODS PUBLISHHELP GENERATION")
    print("  Local sources only — no API calls needed")
    print("=" * 60)

    mods = find_mod_entries()
    if not mods:
        print("  No mods found in 03_ReleaseSource.")
        return 0

    if args.only != "all":
        mods = [m for m in mods if m["intent"] == args.only]

    print(f"\n  Found {len(mods)} mod(s) to process.")
    if args.dry_run:
        for m in mods:
            print(f"    [DRYRUN] {m['base_name']} ({m['intent']})")
        return 0

    # Stage 0: Wipe the PublishHelp directory entirely before regenerating
    print("\n--- Stage 0: Wiping PublishHelp directory ---")
    if os.path.isdir(PUBLISHHELP_DIR):
        shutil.rmtree(PUBLISHHELP_DIR)
        print("  PublishHelp directory deleted.")
    os.makedirs(PUBLISHHELP_DIR, exist_ok=True)
    print("  PublishHelp directory recreated.")

    # Images are NOT copied into PublishHelp — upload them to Nexus directly
    # from 00_Images/02_ImagesFinal instead. PublishHelp only holds the
    # text/zip artifacts below (see WORKSPACE-ORGANIZATION-PLAN.md Progress Log).

    # Stage 1: Generate BBCode FullDesc.md directly (moved from SCRIPT-NexusMods.py)
    print("\n--- Stage 1: BBCode Full Descriptions ---")
    game_ver = "3"
    for m in mods:
        base_name = m["base_name"]
        folder_path = m["folder_path"]
        help_dir = os.path.join(PUBLISHHELP_DIR, base_name)
        os.makedirs(help_dir, exist_ok=True)

        readme = load_readme_text(folder_path)
        if not readme:
            print(f"  [SKIP] {base_name}: no README.txt found")
            continue
        sections = parse_readme_sections(readme)
        mod_info = load_modinfo_xml(folder_path)
        nexus_name = format_nexus_mod_name(base_name, game_ver)
        description = mod_info.get("description", "")
        bbcode = generate_bbcode_full_description(nexus_name, game_ver, description, sections)
        full_desc_path = os.path.join(help_dir, "FullDesc.md")
        with open(full_desc_path, "w", encoding="utf-8") as f:
            f.write(bbcode)
            if not bbcode.endswith("\n"):
                f.write("\n")
        print(f"  [FullDesc] {base_name}")
    print("  BBCode Full Description generation complete.")

    # Stage 2: Generate Details.md from template
    print("\n--- Stage 2: Generating Details.md ---")
    template_text = ""
    if os.path.isfile(TEMPLATE_DETAILS_PATH):
        with open(TEMPLATE_DETAILS_PATH, "r", encoding="utf-8") as f:
            template_text = f.read()
    else:
        print(f"  [WARN] Template not found: {TEMPLATE_DETAILS_PATH}")

    if template_text:
        # Load Mod Type description map once for all mods
        mod_type_map = load_mod_type_map()
        for m in mods:
            base_name = m["base_name"]
            folder_path = m["folder_path"]
            help_dir = os.path.join(PUBLISHHELP_DIR, base_name)
            os.makedirs(help_dir, exist_ok=True)

            # Gather data
            mod_info = load_modinfo_xml(folder_path)
            readme = load_readme_text(folder_path)
            mod_type = extract_mod_type_from_readme(readme)
            # Game version is hardcoded to 3 for current 7d2d compatibility
            game_ver = "3"

            nexus_name = format_nexus_mod_name(base_name, game_ver)
            short_desc = mod_info.get("description", "")
            file_desc = resolve_file_description(short_desc, mod_type, mod_type_map)
            changelog = extract_changelog_entries(readme)

            details_text = generate_details_md(template_text, mod_info, nexus_name, short_desc, file_desc, changelog)
            details_path = os.path.join(help_dir, "Details.md")
            with open(details_path, "w", encoding="utf-8") as f:
                f.write(details_text)
                if not details_text.endswith("\n"):
                    f.write("\n")
            print(f"  [Details.md] {base_name}")

    print("\n  PublishHelp files updated in:", PUBLISHHELP_DIR)
    print("=" * 60)
    return 0


if __name__ == "__main__":
    sys.exit(main())