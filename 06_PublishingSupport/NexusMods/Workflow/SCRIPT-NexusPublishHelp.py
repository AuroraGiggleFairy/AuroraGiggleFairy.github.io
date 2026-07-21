import datetime as dt
import os
import re
import shutil
import sys
import xml.etree.ElementTree as ET
from typing import Dict, List, Tuple

sys.dont_write_bytecode = True  # never leave a __pycache__ behind

NEXUS_WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
NEXUS_ROOT_DIR = os.path.dirname(NEXUS_WORKFLOW_DIR)
VS_CODE_ROOT = os.path.dirname(os.path.dirname(NEXUS_ROOT_DIR))
RELEASE_SOURCE_DIR = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
PUBLISH_HELP_DIR = os.path.join(NEXUS_ROOT_DIR, "PublishHelp")
DEFAULT_DOWNLOAD_BASE_URL = (
    "https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips"
)
AGF_PREFIXES = ("AGF-", "zzzAGF-")

# --- Brand colors ---
AGF_COLOR_LINE = "#5F5980"
AGF_COLOR_HEADING = "#8DB580"
AGF_COLOR_HIGHLIGHT = "#DDCDFA"
AGF_DIVIDER = "[color=#5F5980]──────────────────────────────────────────────────────────────[/color]"

# --- Helper functions ---

def normalize_multiline_text(text: str) -> str:
    if not text:
        return ""
    return text.replace("\r\n", "\n").replace("\r", "\n").strip()


def normalize_single_line_text(text: str) -> str:
    return re.sub(r"\s+", " ", str(text or "")).strip()


def load_text_file(path: str) -> str:
    if not os.path.isfile(path):
        return ""
    try:
        with open(path, "r", encoding="utf-8") as handle:
            return handle.read()
    except Exception:
        return ""


def safe_int(raw_value: object) -> int:
    try:
        return int(raw_value)
    except Exception:
        return 0


def sanitize_filename(value: str) -> str:
    cleaned = re.sub(r"[^A-Za-z0-9._-]+", "_", value or "")
    cleaned = cleaned.strip("._")
    return cleaned or "mod"


def scan_mod_folders(root_dir: str) -> Dict[str, str]:
    results: Dict[str, str] = {}
    if not os.path.isdir(root_dir):
        return results
    for entry in sorted(os.listdir(root_dir)):
        full_path = os.path.join(root_dir, entry)
        if not os.path.isdir(full_path):
            continue
        if not entry.startswith(AGF_PREFIXES):
            continue
        results[entry] = full_path
    return results


def get_base_mod_name(name: str) -> str:
    return re.sub(r"-v\d+(?:\.\d+)*$", "", name)


def parse_modinfo(modinfo_path: str, fallback_name: str) -> Tuple[str, str, str]:
    if not os.path.isfile(modinfo_path):
        return fallback_name, "0.0.0", ""
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
    except Exception:
        return fallback_name, "0.0.0", ""
    mod_name = fallback_name
    mod_version = "0.0.0"
    description = ""
    for child in root:
        tag_name = child.tag.lower()
        value = child.attrib.get("value", "").strip()
        if tag_name == "name" and value:
            mod_name = value
        elif tag_name == "version" and value:
            mod_version = value
        elif tag_name == "description" and value:
            description = value
    return mod_name, mod_version, description


def extract_game_version_from_text(text: str) -> str:
    normalized = normalize_multiline_text(text)
    if not normalized:
        return ""
    for line in normalized.splitlines()[:20]:
        stripped = line.strip().lstrip("- ")
        match = re.match(r"^7d2d\s+Version\s+(.+?)\s*$", stripped, re.IGNORECASE)
        if match:
            return normalize_single_line_text(match.group(1))
    return ""


def extract_changelog_blocks(text: str) -> List[Tuple[str, List[str]]]:
    start_marker = "<!-- CHANGELOG START -->"
    if start_marker in text:
        text = text.split(start_marker, 1)[1]
    lines = text.splitlines()
    blocks: List[Tuple[str, List[str]]] = []
    current_version = ""
    current_lines: List[str] = []
    for raw_line in lines:
        line = raw_line.rstrip()
        version_match = re.match(r"^v(\d+(?:\.\d+)+)\s*$", line.strip(), re.IGNORECASE)
        if version_match:
            if current_version:
                blocks.append((current_version, current_lines))
            current_version = version_match.group(1)
            current_lines = []
            continue
        if current_version:
            current_lines.append(line)
    if current_version:
        blocks.append((current_version, current_lines))
    return blocks


def parse_readme_sections(readme_text: str) -> Dict[str, str]:
    """Extract MOD SCOPE, FEATURES, and OTHER DETAILS from a standard AGF README (txt format)."""
    sections: Dict[str, str] = {}
    if not readme_text:
        return sections
    text = normalize_multiline_text(readme_text)
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
    return sections


def build_display_name(mod_name: str, game_ver: str) -> str:
    """Build the Nexus display name from mod code and game version.
    Preserves original casing (e.g. HUDPlus, 1Main, BMCounter)."""
    parts = mod_name.split("-", 2)
    if len(parts) >= 3:
        name_part = parts[2].replace("-", " ")
        name_display = f"{parts[1]} - {name_part}"
    else:
        name_display = mod_name.replace("-", " ")
    return f"AGF - V3 - {name_display}"


# --- Static BBCode footer (never changes between mods) ---
STATIC_BBCODE_FOOTER = (
    AGF_DIVIDER + "\n" +
    AGF_DIVIDER + "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][heading][size=5][b]Ask AuroraGiggleFairy for Help[/b][/size][/heading][/color]\n" +
    "[list=1]\n" +
    "[*]Join AGF's [url=https://discord.gg/Vm5eyW6N4r]Discord[/url].\n" +
    "[list]\n" +
    "[*]AGF checks website messages often, but Discord is the fastest and best way to get help.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*]Find [color=#DDCDFA][b]#ask-for-help-here[/b][/color] under the [color=#DDCDFA][b]NEED HELP?[/b][/color] section.\n" +
    "[list]\n" +
    "[*]All questions are welcome, whether you are new or experienced.[/*]\n" +
    "[*]This includes mod conflicts, features not working as expected, server or admin issues, translation errors, and other mod-related problems.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*]Post your help request in [color=#DDCDFA][b]#ask-for-help-here[/b][/color]:\n" +
    "[list]\n" +
    "[*]Share a brief message about what is happening.[/*]\n" +
    "[*]Attach your latest log file.\n" +
    "[list]\n" +
    "[*]Enter the game, then press [color=#DDCDFA][b]F1[/b][/color] to open the console.[/*]\n" +
    "[*]Click [color=#DDCDFA][b]Open logs folder[/b][/color] in the top-right.[/*]\n" +
    "[*]The correct log file should already be selected. Drag and drop it into [color=#DDCDFA][b]#ask-for-help-here[/b][/color].[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*]A screenshot can also help.\n" +
    "[list]\n" +
    "[*]Use [color=#DDCDFA][b]PrtSc[/b][/color] (Print Screen) or your system screenshot tool, then paste the image into Discord chat.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*]If preferred, DMs are open and you are welcome to message AGF directly.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[/list]\n" +
    "\n" +
    AGF_DIVIDER + "\n" +
    AGF_DIVIDER + "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][heading][size=5][b]AGF Mod Guide[/b][/size][/heading][/color]\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][b][size=4]A. Install Mods[/size][/b][/color]\n" +
    "[list=1]\n" +
    "[*]Close the game.[/*]\n" +
    "[*]In Steam, right-click [color=#DDCDFA][b]7 Days to Die[/b][/color] -> [color=#DDCDFA][b]Manage[/b][/color] -> [color=#DDCDFA][b]Browse local files[/b][/color], then open [color=#DDCDFA][b]Mods[/b][/color].[/*]\n" +
    "[*]Extract the zip into the Mods folder. Make sure it ends up as [color=#DDCDFA][b]Mods/<ModName>/ModInfo.xml[/b][/color].[/*]\n" +
    "[*]Restart the game.[/*]\n" +
    "[/list]\n" +
    "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][b][size=4]B. Backups[/size][/b][/color]\n" +
    "[list]\n" +
    "[*][b]To Create:[/b] Open [color=#DDCDFA][b]%appdata%[/b][/color] -> [color=#DDCDFA][b]Roaming[/b][/color] -> [color=#DDCDFA][b]7DaysToDie[/b][/color] -> [color=#DDCDFA][b]Saves[/b][/color], then open your World Name folder (for example, Navezgane). Copy your Game Name folder (for example, MyGame) to a safe place.[/*]\n" +
    "[*][b]To Restore:[/b] Copy that saved Game Name folder back into the same World Name folder in [color=#DDCDFA][b]Saves[/b][/color]. Replace the current folder if asked.[/*]\n" +
    "[/list]\n" +
    "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][b][size=4]C. Update Mods[/size][/b][/color]\n" +
    "[list=1]\n" +
    "[*]Close the game.[/*]\n" +
    "[*]Make a backup first (see section B).[/*]\n" +
    "[*]Install the new version in Mods.\n" +
    "[list]\n" +
    "[*]If asked, allow overwrite or replace.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*]If both old and new folders are there, keep the newer one and delete the older one.[/*]\n" +
    "[*]Start the game and confirm your save loads.[/*]\n" +
    "[/list]\n" +
    "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][b][size=4]D. Remove Mods[/size][/b][/color]\n" +
    "[list]\n" +
    "[*][b]Warning:[/b] Removing a mod from an active save can destroy your saved game. Back up first.[/*]\n" +
    "[*]Never delete [color=#DDCDFA][b]0_TFP_Harmony[/b][/color]; it comes with the game.[/*]\n" +
    "[/list]\n" +
    "[list=1]\n" +
    "[*]Close the game.[/*]\n" +
    "[*]In Mods, delete each mod folder you are removing, except [color=#DDCDFA][b]0_TFP_Harmony[/b][/color].[/*]\n" +
    "[/list]\n" +
    "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][b][size=4]E. The 0_TFP_Harmony Mod (Do Not Remove)[/size][/b][/color]\n" +
    "[list]\n" +
    "[*]Never delete [color=#DDCDFA][b]0_TFP_Harmony[/b][/color]; it comes with the game.[/*]\n" +
    "[*]If it is missing, restore it by verifying game files in Steam:\n" +
    "[list=1]\n" +
    "[*]In Steam, right-click [color=#DDCDFA][b]7 Days to Die[/b][/color].[/*]\n" +
    "[*]Select [color=#DDCDFA][b]Properties[/b][/color].[/*]\n" +
    "[*]Select [color=#DDCDFA][b]Installed Files[/b][/color].[/*]\n" +
    "[*]Click [color=#DDCDFA][b]Verify integrity of game files[/b][/color] and wait for completion.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[/list]\n" +
    "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][b][size=4]F. EAC[/size][/b][/color]\n" +
    "[list]\n" +
    "[*]EAC stands for Easy Anti-Cheat and helps protect multiplayer sessions from cheating.[/*]\n" +
    "[*]Some mods require EAC to be turned off so they can work.[/*]\n" +
    "[*][color=#DDCDFA][b]How to launch 7 Days to Die with EAC off:[/b][/color]\n" +
    "[list=1]\n" +
    "[*]In Steam Library, select 7 Days to Die.[/*]\n" +
    "[*]Click Play.[/*]\n" +
    "[*]In the launch popup, select Launch game without EAC.[/*]\n" +
    "[*]Click Play.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*][color=#DDCDFA][b]If the launch popup does not appear:[/b][/color]\n" +
    "[list=1]\n" +
    "[*]In Steam Library, select 7 Days to Die.[/*]\n" +
    "[*]Click the gear icon on the right, then click Properties.[/*]\n" +
    "[*]Under Launch Options, open the Selected Launch Option dropdown.[/*]\n" +
    "[*]Choose Ask when starting game or Launch game without EAC.[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*][color=#DDCDFA][b]If you run multiplayer with EAC off, use these safety practices:[/b][/color]\n" +
    "[list]\n" +
    "[*]Simplest method: keep your server password private and have people ask for it.[/*]\n" +
    "[*]If you want tighter security on who joins, use the whitelist system.[/*]\n" +
    "[*]Admin tools such as Server Tools have security options.[/*]\n" +
    "[*]Talk to other server hosts, for example AGF in [url=https://discord.gg/Vm5eyW6N4r]Discord[/url].[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[/list]\n" +
    "\n" +
    AGF_DIVIDER + "\n" +
    AGF_DIVIDER + "\n" +
    "\n" +
    "[color=" + AGF_COLOR_HEADING + "][heading][size=5][b]Support AuroraGiggleFairy[/b][/size][/heading][/color]\n" +
    "[list]\n" +
    "[*]I have been actively creating and supporting 7 Days to Die mods since Alpha 18 (2019), and I genuinely love doing this work.[/*]\n" +
    "[*]I spend a lot of time fixing complex issues, keeping everything up to date, and helping players, modders, and server communities.[/*]\n" +
    "[*]If my work helps you, here are ways to support me:\n" +
    "[list]\n" +
    "[*]Help spread my mods by sharing them with others, creating content, or sharing my [url=https://auroragigglefairy.github.io/]GitHub page[/url].[/*]\n" +
    "[*]Join my [url=https://discord.gg/Vm5eyW6N4r]Discord[/url] to share feedback, keep up with updates, or volunteer as a tester.[/*]\n" +
    "[*]Support me on [url=https://www.twitch.tv/auroragigglefairy]Twitch[/url].[/*]\n" +
    "[*]Need hosting? Use my [url=https://pingperfect.com/aff.php?aff=1834]PingPerfect Referral Link[/url].[/*]\n" +
    "[*]Support me directly by donating to my [url=https://www.paypal.com/donate/?hosted_button_id=3B7BCQAZ6KHXC]PayPal[/url].[/*]\n" +
    "[/list]\n" +
    "[/*]\n" +
    "[*]From the bottom of my heart, thank you. [img]https://static.nexusmods.com/mods/130/images/emoji/200x200/2764.png[/img][/*]\n" +
    "[/list]\n" +
    "\n" +
    AGF_DIVIDER + "\n" +
    "\n" +
    "[i][size=3]This mod is part of [color=" + AGF_COLOR_HIGHLIGHT + "][b]The Giggle Pack[/b][/color]. For the full catalog, visit [url=https://auroragigglefairy.github.io/]https://auroragigglefairy.github.io/[/url][/size][/i]\n"
)


# --- BBCode generation helpers ---

def _join_wrapped_bullets(text: str) -> str:
    """Join word-wrapped continuation lines to their parent bullets.
    Preserves sub-bullets by tracking indent levels.
    A continuation line is one without a leading '-' that starts with text (indented)."""
    result: List[str] = []
    for line in text.splitlines():
        stripped = line.strip()
        if not stripped:
            continue
        indent = len(line) - len(line.lstrip())
        if stripped.startswith("-"):
            # Bullet line - track indent level
            result.append((" " * indent) + stripped)
        elif result and indent > 0:
            # Indented continuation - join to previous line
            result[-1] = result[-1] + " " + stripped
        else:
            result.append(stripped)
    return "\n".join(result)


def _bbcode_title(display_name: str, one_liner: str) -> str:
    return (
        AGF_DIVIDER + "\n" +
        AGF_DIVIDER + "\n" +
        f"[color={AGF_COLOR_HEADING}][size=6][b]AGF - V3 - {display_name}[/b][/size][/color]\n" +
        "\n" +
        f"[size=4]{one_liner}[/size]\n" +
        "\n" +
        AGF_DIVIDER + "\n"
    )


def _apply_color_terms_to_bbcode(text: str) -> str:
    """Wrap known UI/Steam terms in [color=#DDCDFA] tags for Nexus BBCode.
    Uses a single regex pass to avoid nesting issues when terms overlap (e.g.
    Mods/0_TFP_Harmony and 0_TFP_Harmony). Terms are sorted longest-first so
    that longer, more specific matches take priority over shorter sub-matches."""
    terms = [
        "Mods/0_TFP_Harmony",
        "Verify integrity of game files",
        "Installed Files",
        "0_TFP_Harmony",
        "7 Days to Die",
        "Properties",
    ]
    # Build a pattern that matches any of the terms, longest first
    escaped = [re.escape(t) for t in terms]
    combined_pattern = "|".join(escaped)

    def _wrap_match(m: re.Match) -> str:
        matched = m.group(0)
        # Only wrap if not already inside a [color=...] tag
        start = m.start()
        prefix = text[max(0, start - 20):start]
        if re.search(r"\[color=[^\]]+\]$", prefix):
            return matched
        return f"[color={AGF_COLOR_HIGHLIGHT}]{matched}[/color]"

    return re.sub(combined_pattern, _wrap_match, text)


def _bbcode_mod_scope(text: str) -> str:
    text = _join_wrapped_bullets(text)
    lines: List[str] = []
    lines.append(f"[color={AGF_COLOR_HEADING}][heading][size=5][b]Mod Scope[/b][/size][/heading][/color]")
    lines.append("[list]")
    in_sub = False
    pending_parent_close = False
    for raw_line in text.splitlines():
        indent = len(raw_line) - len(raw_line.lstrip())
        s = raw_line.strip()
        if not s or s.startswith("- Mod Version:"):
            continue
        if not s.startswith("- "):
            if in_sub:
                lines.append(f"[*]{s}[/*]")
            continue
        content = s[2:]
        if ":" in content:
            if indent >= 4:
                # Sub-bullet inside a parent's sub-list — apply color to terms
                if not in_sub:
                    lines.append("[list]")
                    in_sub = True
                lines.append(f"[*]{_apply_color_terms_to_bbcode(content)}[/*]")
            else:
                # Top-level item — close previous sub-list if needed
                if in_sub:
                    lines.append("[/list]")
                    lines.append("[/*]")
                    in_sub = False
                    pending_parent_close = False
                label, value = content.split(":", 1)
                ls = label.strip()
                vs = value.strip()
                if ls == "Website":
                    continue
                # Add the parent line WITHOUT [/*] — we add it after the sub-list
                lines.append(f"[*][color={AGF_COLOR_HIGHLIGHT}][b]{ls}:[/b][/color] {vs}")
                pending_parent_close = True
        elif in_sub:
            # Sub-bullet without colon (continuation plain text)
            lines.append(f"[*]{_apply_color_terms_to_bbcode(content)}[/*]")
        elif pending_parent_close:
            # A sub-bullet after a parent — open sub-list first
            lines.append("[list]")
            in_sub = True
            pending_parent_close = False
            lines.append(f"[*]{_apply_color_terms_to_bbcode(content)}[/*]")
    if in_sub:
        lines.append("[/list]")
        lines.append("[/*]")
        pending_parent_close = False
    if pending_parent_close:
        lines[-1] += "[/*]"
    lines.append("[/list]")
    lines.append("")
    lines.append(AGF_DIVIDER)
    return "\n".join(lines)


def _bbcode_features(text: str) -> str:
    text = _join_wrapped_bullets(text)
    lines: List[str] = []
    lines.append(f"[color={AGF_COLOR_HEADING}][heading][size=5][b]Features[/b][/size][/heading][/color]")
    lines.append("[list]")
    for raw_line in text.splitlines():
        s = raw_line.strip()
        if s.startswith("- "):
            bullet = s[2:]
            if bullet:
                lines.append(f"[*][size=4]{bullet}[/size][/*]")
    lines.append("[/list]")
    return "\n".join(lines)


def _bbcode_other_details(text: str) -> str:
    text = _join_wrapped_bullets(text)
    lines: List[str] = []
    lines.append(AGF_DIVIDER)
    lines.append("")
    lines.append(f"[color={AGF_COLOR_HEADING}][heading][size=4][b]Other Details[/b][/size][/heading][/color]")
    lines.append("[list]")
    in_sub = False
    for raw_line in text.splitlines():
        indent = len(raw_line) - len(raw_line.lstrip())
        s = raw_line.strip()
        if s.startswith("- "):
            content = s[2:]
            # Sub-bullet: indent >= 4 spaces from section, starts as "- Client-side behavior:"
            if indent >= 4:
                if not in_sub:
                    lines.append("[list]")
                    in_sub = True
                if ":" in content:
                    label, value = content.split(":", 1)
                    lines.append(f"[*][size=3][color={AGF_COLOR_HIGHLIGHT}][b]{label.strip()}:[/b][/color] {value.strip()}[/size][/*]")
                else:
                    lines.append(f"[*][size=3]{content}[/size][/*]")
            else:
                if in_sub:
                    lines.append("[/list]")
                    in_sub = False
                if ":" in content:
                    label, value = content.split(":", 1)
                    lines.append(f"[*][size=3][color={AGF_COLOR_HIGHLIGHT}][b]{label.strip()}:[/b][/color] {value.strip()}[/size][/*]")
                else:
                    lines.append(f"[*][size=3]{content}[/size][/*]")
        elif s:
            # Non-bullet continuation
            if in_sub:
                lines[-1] = lines[-1].rstrip("[/size][/*]") + " " + s + "[/size][/*]"
            else:
                lines[-1] = lines[-1].rstrip("[/size][/*]") + " " + s + "[/size][/*]"
    if in_sub:
        lines.append("[/list]")
    lines.append("[/list]")
    return "\n".join(lines)


def generate_bbcode_full_description(plan_entry: Dict[str, object], sections: Dict[str, str]) -> str:
    game_ver = str(plan_entry.get("tested_game_version", ""))
    mod_code = str(plan_entry.get("mod_name", ""))
    desc = str(plan_entry.get("description", ""))
    one_liner = desc.split(".")[0] + "." if "." in desc else desc

    parts = mod_code.split("-", 2)
    if len(parts) >= 3:
        display_name = f"{parts[1]} - {parts[2].replace('-', ' ')}"
    elif len(parts) == 2:
        display_name = parts[1].replace("-", " ")
    else:
        display_name = mod_code.replace("-", " ")

    top: List[str] = [_bbcode_title(display_name, one_liner)]

    scope = sections.get("mod_scope", "")
    if scope:
        top.append(_bbcode_mod_scope(scope))

    features = sections.get("features", "")
    if features:
        top.append(_bbcode_features(features))

    other = sections.get("other_details", "")
    if other:
        top.append(_bbcode_other_details(other))

    return "\n\n".join(top) + "\n\n" + STATIC_BBCODE_FOOTER


# --- Mod Type lookup from snippet ---
MODTYPE_LOOKUP = {
    "Server-Side (EAC-Friendly)": "Mod Type: Server-Side (EAC-Friendly) — Server install works for all joining players, EAC can be on or off, and it also works in singleplayer.",
    "Server-Side (EAC Off)": "Mod Type: Server-Side (EAC Off) — EAC off is required, server install works for all joining players, and it also works in singleplayer.",
    "Server/Client-Side (Required)": "Mod Type: Server/Client-Side (Required) — EAC off is required, the host and all joining players must install it, and it also works in singleplayer.",
    "Client-Side (Only)": "Mod Type: Client-Side (Only) — EAC off is required, server install has no effect, each player installs it on their own PC, and it also works in singleplayer.",
}


def _get_mod_type_sentence(entry: Dict[str, object]) -> str:
    """Get the full mod type sentence for a mod."""
    readme_text = load_text_file(str(entry.get("readme_path", "")))
    if not readme_text:
        return ""
    for line in readme_text.splitlines():
        stripped = line.strip()
        if "- Mod Type:" in stripped or "Mod Type:" in stripped:
            mod_type = stripped.split(":", 1)[1].strip()
            if mod_type in MODTYPE_LOOKUP:
                return MODTYPE_LOOKUP[mod_type]
            return f"Mod Type: {mod_type}"
    return ""


def build_details_markdown(entry: Dict[str, object]) -> str:
    """Simplified Details.md — only what's needed for Nexus copy/paste."""
    mod_name = str(entry.get("mod_name", ""))
    version = str(entry.get("version", "0.0.0"))
    game_ver = str(entry.get("tested_game_version", ""))
    description = str(entry.get("description", ""))

    short_desc = normalize_single_line_text(description)
    mod_type_sentence = _get_mod_type_sentence(entry)
    file_desc = f"{short_desc}\n\n{mod_type_sentence}" if mod_type_sentence else short_desc
    display_name = build_display_name(mod_name, game_ver)

    readme_text = load_text_file(str(entry.get("readme_path", "")))
    changelog_blocks = extract_changelog_blocks(readme_text) if readme_text else []
    changelog_parts: List[str] = []
    for cver, clines in changelog_blocks:
        bullets = [normalize_single_line_text(line.strip().lstrip("- ")) for line in clines if line.strip().startswith("-")]
        if bullets:
            changelog_parts.append(f"### v{cver}")
            changelog_parts.append("```text")
            for b in bullets:
                changelog_parts.append(b)
            changelog_parts.append("```")
            changelog_parts.append("")

    return f"""# Nexus Mod Details - {mod_name} v{version}

Generated: {dt.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

---

## 1) Copy/Paste: Nexus Mod Name

```text
{display_name}
```

---

## 2) Short Description (Copy/Paste)
**Target:** Nexus → General → Short description *(350 character limit)*

Length: `{len(short_desc)}` / 350

```text
{short_desc}
```

---

## 3) File Details (Copy/Paste)
**Target:** Nexus → Files → Edit File

### 3.1 Display Name
```text
{display_name}
```

### 3.2 File Version
```text
{version}
```

### 3.3 File Description / Notes
**Target:** Nexus → Files → File options → Description

```text
{file_desc}
```

---

## 4) Changelog (Copy/Paste)
**Target:** Nexus → Files → Add changelog modal

{chr(10).join(changelog_parts) if changelog_parts else "*No changelog entries found.*"}
"""


# --- Main generator ---

def gather_mod_data() -> Dict[str, Dict[str, object]]:
    mods: Dict[str, Dict[str, object]] = {}
    for folder_name, folder_path in scan_mod_folders(RELEASE_SOURCE_DIR).items():
        modinfo_path = os.path.join(folder_path, "ModInfo.xml")
        readme_path = os.path.join(folder_path, "README.md")
        readable_readme_path = os.path.join(folder_path, "ReadableReadMe.txt")
        _, version, description = parse_modinfo(modinfo_path, folder_name)
        base_name = get_base_mod_name(folder_name)

        readme_txt_path = os.path.join(folder_path, "README.txt")
        readme_text = load_text_file(readable_readme_path) or load_text_file(readme_path) or load_text_file(readme_txt_path)
        game_ver = ""
        if readme_text:
            game_ver = extract_game_version_from_text(readme_text)

        actual_readme = ""
        if os.path.isfile(readable_readme_path):
            actual_readme = readable_readme_path
        elif os.path.isfile(readme_path):
            actual_readme = readme_path
        elif os.path.isfile(readme_txt_path):
            actual_readme = readme_txt_path

        mods[base_name] = {
            "mod_name": base_name,
            "folder_name": folder_name,
            "folder_path": folder_path,
            "version": version,
            "description": description,
            "tested_game_version": game_ver,
            "readme_path": actual_readme,
            "readable_readme_path": "",
        }
    return mods


# Path for mod release zips. Images are NOT copied into PublishHelp — upload
# them to Nexus directly from 00_Images/02_ImagesFinal instead.
ZIP_DIR = os.path.join(VS_CODE_ROOT, "04_DownloadZips")


def generate_publish_help() -> int:
    mods = gather_mod_data()
    if not mods:
        print("No mods found in 03_ReleaseSource.")
        return 1

    total = len(mods)
    zip_total = 0
    details_total = 0
    bbcode_total = 0
    skipped = 0

    print(f"Generating PublishHelp for {total} mods...")
    print(f"Output: {PUBLISH_HELP_DIR}")
    print()

    for mod_name in sorted(mods.keys()):
        entry = mods[mod_name]
        version = str(entry.get("version", "0.0.0"))
        target_dir = os.path.join(PUBLISH_HELP_DIR, mod_name)
        os.makedirs(target_dir, exist_ok=True)

        zip_name = f"{mod_name}.zip"
        src_zip = os.path.join(ZIP_DIR, zip_name)
        if os.path.isfile(src_zip):
            dst_zip = os.path.join(target_dir, zip_name)
            shutil.copy2(src_zip, dst_zip)
            zip_total += 1

        details_md = build_details_markdown(entry)
        details_path = os.path.join(target_dir, "Details.md")
        with open(details_path, "w", encoding="utf-8") as handle:
            handle.write(details_md)
            if not details_md.endswith("\n"):
                handle.write("\n")
        details_total += 1

        readme_text = load_text_file(str(entry.get("readme_path", "")))
        if readme_text:
            sections = parse_readme_sections(readme_text)
            bbcode = generate_bbcode_full_description(entry, sections)
            bbcode_path = os.path.join(target_dir, "FullDesc.md")
            with open(bbcode_path, "w", encoding="utf-8") as handle:
                handle.write(bbcode)
                if not bbcode.endswith("\n"):
                    handle.write("\n")
            bbcode_total += 1
        else:
            print(f"  [SKIP FullDesc] {mod_name}: no README found")
            skipped += 1

        print(f"  [OK] {mod_name} v{version}")

    print()
    print("=== PublishHelp Generation Complete ===")
    print(f"Mods processed: {total}")
    print(f"Details files:  {details_total}")
    print(f"FullDesc files: {bbcode_total}")
    print(f"Zips copied:    {zip_total}")
    if skipped:
        print(f"Skipped:        {skipped} (no README)")
    print(f"Output folder:  {PUBLISH_HELP_DIR}")
    return 0


def main() -> int:
    import argparse
    parser = argparse.ArgumentParser(
        description="Generate Nexus PublishHelp folder structure with Details, FullDesc, and the release zip."
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Preview what would be generated without writing files.",
    )
    args = parser.parse_args()

    if args.dry_run:
        mods = gather_mod_data()
        print(f"[DRYRUN] Would generate PublishHelp for {len(mods)} mods in: {PUBLISH_HELP_DIR}")
        for mod_name in sorted(mods.keys()):
            entry = mods[mod_name]
            version = str(entry.get("version", "0.0.0"))
            print(f"  [DRYRUN] {mod_name} v{version}")
        return 0

    return generate_publish_help()


if __name__ == "__main__":
    sys.exit(main())