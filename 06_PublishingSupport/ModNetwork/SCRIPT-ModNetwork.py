#!/usr/bin/env python3
"""SCRIPT-ModNetwork.py

Automates publishing and updating AGF mods on The Mod Network (themodnetwork.com).

Modes:
  init-config      Create modnetwork-config.json populated with all scanned mods
  build-plan       Scan release mods and generate upload plan JSON
  check-live       Query live TMN pages for mods that have a known tmn_slug
  prepare-upload   Preview the exact fields that would be sent to TMN (no API calls)
  update-page     PATCH /api/creator-mods/{mod_id} to update page metadata

Field sources (see FieldMapping.txt for full documentation):
  title           ModInfo.xml <Name>
  version         ModInfo.xml <Version>
  description     ModInfo.xml <Description>  (short plain-text)
  long_description README.md sections 3, 4+5+6, 7, 1+2  →  HTML
  install text    README.md sections 9–13  →  HTML  (How To Install tab)
  changelog       README.md last 3 version blocks  →  HTML
    game_version    Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv TESTED_GAME_VERSION
    mod_side        Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv MOD_TYPE_ID  →  mapped
  requirements    Static: "None."
  release_type    Static: "stable"  (or config override)
  file            04_DownloadZips/{base_mod_name}.zip
  slug            modnetwork-config.json per-mod tmn_slug (stored after first publish)
  game_id         modnetwork-config.json global tmn_game_id (numeric, resolve manually)
"""

import argparse
import csv
import datetime as dt
import getpass
import json
import os
import re
import sys
import urllib.error
import urllib.request
import uuid
import xml.etree.ElementTree as ET
from typing import Dict, List, Optional, Tuple

try:
    import markdown as markdown_lib
    MARKDOWN_AVAILABLE = True
except ImportError:
    MARKDOWN_AVAILABLE = False
    print(
        "WARNING: 'markdown' library not installed. Run: pip install markdown",
        file=sys.stderr,
    )

# ---------------------------------------------------------------------------
# Paths and constants
# ---------------------------------------------------------------------------

MODNETWORK_DATA_DIR = os.path.dirname(os.path.abspath(__file__))
VS_CODE_ROOT = os.path.dirname(os.path.dirname(MODNETWORK_DATA_DIR))
RELEASE_SOURCE_DIR = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
ZIP_OUTPUT_DIR = os.path.join(VS_CODE_ROOT, "04_DownloadZips")
DEFAULT_CONFIG_PATH = os.path.join(MODNETWORK_DATA_DIR, "modnetwork-config.json")
DEFAULT_PLAN_OUTPUT_PATH = os.path.join(MODNETWORK_DATA_DIR, "modnetwork-plan.json")
COMPAT_CSV_PATH = os.path.join(
    VS_CODE_ROOT,
    "Workflow",
    "ReadmeSystem",
    "Data",
    "HELPER_ModCompatibility.csv",
)

DEFAULT_API_BASE_URL = "https://themodnetwork.com/api/v1"
DEFAULT_API_KEY_ENV_VAR = "AGF_TMN_API_KEY"
AGF_PREFIXES = ("AGF-", "zzzAGF-")

DEFAULT_REQUIREMENTS = "None."
DEFAULT_RELEASE_TYPE = "stable"
CHANGELOG_KEEP_COUNT = 3

# Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv MOD_TYPE_ID → TMN mod_side
# 0 = TBD/Unknown → needs manual review
# 1 = Server-Side (EAC-Friendly)
# 2 = Server-Side (EAC Off)
# 3 = Server-Side (Dedicated Only, EAC Off)
# 4 = Hybrid (EAC Off)
# 5 = Server/Client-Side (Required)
# 6 = Client-Side (Only)
MOD_TYPE_TO_SIDE: Dict[int, Optional[str]] = {
    0: None,        # unknown — flag for manual review
    1: "server",
    2: "server",
    3: "server",
    4: "both",
    5: "both",
    6: "client",
}

# ---------------------------------------------------------------------------
# Utility functions
# ---------------------------------------------------------------------------

def load_json_file(path: str) -> Dict:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
        return data if isinstance(data, dict) else {}
    except Exception:
        return {}


def write_json_file(path: str, payload: Dict, dry_run: bool) -> None:
    if dry_run:
        print(f"[DRYRUN] Would write: {path}")
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        json.dump(payload, f, indent=2, ensure_ascii=False)
        f.write("\n")


def load_text_file(path: str) -> str:
    if not os.path.isfile(path):
        return ""
    try:
        with open(path, "r", encoding="utf-8") as f:
            return f.read()
    except Exception:
        return ""


def normalize_text(text: str) -> str:
    return text.replace("\r\n", "\n").replace("\r", "\n")


def normalize_single_line(text: str) -> str:
    return re.sub(r"\s+", " ", str(text or "")).strip()


def safe_int(value) -> int:
    try:
        return int(value)
    except Exception:
        return 0


def normalize_intent(raw) -> str:
    value = str(raw or "review").strip().lower()
    if value in {"publish", "update", "skip", "review"}:
        return value
    return "review"


# ---------------------------------------------------------------------------
# Compatibility CSV
# ---------------------------------------------------------------------------

def load_compat_csv() -> Dict[str, Dict[str, str]]:
    """Load Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv keyed by MOD_NAME."""
    result: Dict[str, Dict[str, str]] = {}
    if not os.path.isfile(COMPAT_CSV_PATH):
        return result
    try:
        with open(COMPAT_CSV_PATH, "r", encoding="utf-8", newline="") as f:
            reader = csv.DictReader(f)
            for row in reader:
                mod_name = str(row.get("MOD_NAME", "")).strip()
                if mod_name:
                    result[mod_name] = dict(row)
    except Exception:
        pass
    return result


def map_mod_type_to_side(mod_type_id: int) -> Optional[str]:
    """Return TMN mod_side for a MOD_TYPE_ID, or None if unknown/needs review."""
    return MOD_TYPE_TO_SIDE.get(mod_type_id, None)


# ---------------------------------------------------------------------------
# ModInfo / folder scanning
# ---------------------------------------------------------------------------

def get_base_mod_name(folder_name: str) -> str:
    return re.sub(r"-v\d+(?:\.\d+)*$", "", folder_name)


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
        tag = child.tag.lower()
        value = child.attrib.get("value", "").strip()
        if tag == "name" and value:
            mod_name = value
        elif tag == "version" and value:
            mod_version = value
        elif tag == "description" and value:
            description = value
    return mod_name, mod_version, description


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


def gather_release_mods() -> Dict[str, Dict]:
    mods: Dict[str, Dict] = {}
    for folder_name, folder_path in scan_mod_folders(RELEASE_SOURCE_DIR).items():
        modinfo_path = os.path.join(folder_path, "ModInfo.xml")
        readme_path = os.path.join(folder_path, "README.md")
        mod_name, version, description = parse_modinfo(modinfo_path, folder_name)
        base_name = get_base_mod_name(folder_name)
        mods[base_name] = {
            "base_name": base_name,
            "folder_name": folder_name,
            "folder_path": folder_path,
            "mod_name": mod_name,
            "version": version,
            "description": description,
            "readme_path": readme_path if os.path.isfile(readme_path) else "",
        }
    return mods


def gather_mod_zip_paths() -> Dict[str, str]:
    results: Dict[str, str] = {}
    if not os.path.isdir(ZIP_OUTPUT_DIR):
        return results
    for entry in sorted(os.listdir(ZIP_OUTPUT_DIR)):
        if not entry.endswith(".zip"):
            continue
        zip_base = entry[:-4]
        if not zip_base.startswith(AGF_PREFIXES):
            continue
        results[zip_base] = os.path.join(ZIP_OUTPUT_DIR, entry)
    return results


# ---------------------------------------------------------------------------
# README section extraction
# ---------------------------------------------------------------------------

def extract_readme_section(text: str, section_num: int) -> str:
    """Extract ## N. Title section content, including all sub-sections within it."""
    normalized = normalize_text(text)
    pattern = re.compile(r"^##\s+(\d+)\.\s+", re.MULTILINE)
    matches = list(pattern.finditer(normalized))
    for i, match in enumerate(matches):
        if int(match.group(1)) == section_num:
            start = match.start()
            end = matches[i + 1].start() if i + 1 < len(matches) else len(normalized)
            return normalized[start:end].strip()
    return ""


def extract_readme_sections(text: str, section_nums: List[int]) -> str:
    """Extract and concatenate multiple numbered sections, stripping trailing --- from each."""
    parts = [extract_readme_section(text, n) for n in section_nums]
    parts = [re.sub(r"\n+---+\s*$", "", p.rstrip()) for p in parts if p]
    return "\n\n".join(parts)


def extract_changelog_last_n(text: str, n: int = CHANGELOG_KEEP_COUNT) -> str:
    """
    Extract the last N version blocks from the CHANGELOG section.
    Version lines are prefixed with ### for proper HTML heading rendering.
    The oldest block gets a 'See README for full changelog' note appended.
    """
    normalized = normalize_text(text)
    start_m = re.search(r"<!--\s*CHANGELOG\s+START\s*-->", normalized, re.IGNORECASE)
    end_m = re.search(r"<!--\s*CHANGELOG\s+END\s*-->", normalized, re.IGNORECASE)
    if not start_m:
        return ""
    block = normalized[start_m.end(): end_m.start() if end_m else len(normalized)].strip()

    # Version lines look like "v5.4.7" or "V2.0.0" — standalone on their own line
    version_pattern = re.compile(r"^[Vv]?\d+\.\d+(?:\.\d+)*\s*$", re.MULTILINE)
    version_matches = list(version_pattern.finditer(block))
    if not version_matches:
        return block

    blocks: List[str] = []
    for i, vm in enumerate(version_matches):
        end = version_matches[i + 1].start() if i + 1 < len(version_matches) else len(block)
        raw = block[vm.start():end].strip()
        lines = raw.splitlines()
        if lines:
            lines[0] = f"### {lines[0].strip()}"
        blocks.append("\n".join(lines))

    selected = blocks[:n]
    if selected and len(blocks) > n:
        selected[-1] += "\n\n*See the README for the full changelog.*"

    return "\n\n".join(selected)


# ---------------------------------------------------------------------------
# HTML content builders
# ---------------------------------------------------------------------------

def _preprocess_md_for_html(text: str) -> str:
    """Shared preprocessing before markdown conversion."""
    # Strip HTML comments (<!-- FEATURES-SUMMARY START --> etc.)
    text = re.sub(r"<!--.*?-->", "", text, flags=re.DOTALL)
    # Strip leading section numbers from headings: ## 3. Title -> ## Title
    text = re.sub(r"^(#{1,6})\s+\d+\.\s+", r"\1 ", text, flags=re.MULTILINE)
    # Auto-linkify bare URLs so they render as clickable links
    text = re.sub(
        r"(?<!\]\()(?<!\[)(https?://[^\s)>\]]+)",
        r"[\1](\1)",
        text,
    )
    return text


def markdown_to_html(text: str) -> str:
    if not text:
        return ""
    text = _preprocess_md_for_html(text)
    if not MARKDOWN_AVAILABLE:
        return text
    return markdown_lib.markdown(text, extensions=["tables"], tab_length=2)


def _md_section_to_html(text: str) -> str:
    """Convert one preprocessed markdown section to HTML with correct indentation."""
    if not MARKDOWN_AVAILABLE:
        return text
    return markdown_lib.markdown(text, extensions=["tables"], tab_length=2)


def _apply_html_polish(html: str) -> str:
    """Post-process combined HTML: add separators, indent tables, style tables."""
    # Convert <hr> tags (from --- in markdown) to spacing divs
    html = re.sub(r"\s*<hr\s*/?>\s*", '<div style="margin-top:1.5em"></div>', html)

    # Add left margin to bullet and numbered lists for better indentation
    html = html.replace("<ul>", '<ul style="margin-left:1.5em">')
    html = html.replace("<ol>", '<ol style="margin-left:1.5em">')

    # Style tables — indent them and add cell padding
    html = html.replace("<table>", '<table style="border-collapse:collapse;margin:0.5em 0 0 1.5em">')
    html = html.replace("</table>", '</table><div style="height:0.6em"></div>')
    html = html.replace("</table>", "</table>")
    html = re.sub(r"<(th|td)>", r'<\1 style="padding:4px 10px;border:1px solid #555;">', html)

    # Split on h2/h3 opening tags, keeping the tags as tokens
    tokens = re.split(r"(<h[23][^>]*>)", html)
    result: List[str] = []
    first_heading = True
    prev_level = 0

    i = 0
    if tokens and not re.match(r"<h[23]", tokens[0]):
        result.append(tokens[0])
        i = 1

    while i < len(tokens):
        tag = tokens[i]
        content = tokens[i + 1] if i + 1 < len(tokens) else ""
        level = int(re.match(r"<h(\d)", tag).group(1))

        if first_heading:
            first_heading = False
            if level == 2:
                # First h2: thick line, no top margin (it's at the top of the page)
                sep = '<div style="border-top:3px solid #777;margin:0 0 0.6em 0"></div>'
            else:
                # First h3: thin line, no top margin
                sep = '<div style="border-top:1px solid #555;margin:0 0 0.4em 0"></div>'
        elif level == 2:
            # Subsequent h2: thick line + extra breathing room above
            sep = '<div style="border-top:3px solid #777;margin:2.8em 0 0.6em 0"></div>'
        else:
            # h3 (A/B/C): thin line + extra breathing room above
            sep = '<div style="border-top:1px solid #555;margin:2em 0 0.4em 0"></div>'

        if level == 3:
            # TMN's h3 CSS border can't be overridden with inline styles — use a styled <p> instead
            close_tag = re.search(r"</h3>", content)
            if close_tag:
                heading_text = content[:close_tag.start()]
                rest = content[close_tag.end():]
            else:
                heading_text = ""
                rest = content
            result.append(f'{sep}<p style="font-weight:bold;font-size:1.05em;margin:0 0 0.3em 0">{heading_text}</p>{rest}')
        else:
            result.append(f"{sep}{tag}{content}")
        prev_level = level
        i += 2

    return "".join(result)


def build_full_description_html(readme_text: str) -> str:
    """
    Build full description HTML from README sections in display order:
      1. Need Help? (section 3)
      2. Mod Type + Compatibility + Features Summary (sections 4, 5, 6)
      3. Features Details (section 7)
      4. About Author + Mod Philosophy (sections 1, 2)
    """
    parts: List[str] = []
    for section_nums in [[3], [4, 5, 6], [7], [1, 2]]:
        block = extract_readme_sections(readme_text, section_nums)
        if block:
            parts.append(_preprocess_md_for_html(block))
    if not parts:
        return ""
    combined_html = "".join(_md_section_to_html(p) for p in parts)
    return _apply_html_polish(combined_html)


def build_install_description_html(readme_text: str) -> str:
    """
    Build How To Install HTML from README sections 9–13:
      9.  Important Mod Details (mod type table, EAC explainer)
      10. Installation Guide
      11. Removal Guide
      12. Update Guide
      13. Backup Guide
    """
    block = extract_readme_sections(readme_text, [9, 10, 11, 12, 13])
    if not block:
        return ""
    # Strip trailing --- separators, split into per-section blocks and join with <hr>
    sections: List[str] = []
    for n in [9, 10, 11, 12, 13]:
        s = extract_readme_section(readme_text, n)
        if s:
            s = re.sub(r"\n+---+\s*$", "", s.rstrip())
            sections.append(_preprocess_md_for_html(s))
    if not sections:
        return ""
    combined_html = "".join(_md_section_to_html(s) for s in sections)
    return _apply_html_polish(combined_html)


def build_changelog_text(readme_text: str) -> str:
    """Build plain-text changelog from the last CHANGELOG_KEEP_COUNT version blocks.
    TMN release changelog field expects plain text/markdown, not HTML."""
    return extract_changelog_last_n(readme_text, CHANGELOG_KEEP_COUNT)


# ---------------------------------------------------------------------------
# HTTP helpers
# ---------------------------------------------------------------------------

def encode_multipart(
    fields: Dict[str, str],
    files: Optional[Dict[str, Tuple[str, bytes, str]]] = None,
) -> Tuple[bytes, str]:
    """Encode fields and optional files as multipart/form-data.
    Returns (body_bytes, content_type_header_value).
    """
    boundary = uuid.uuid4().hex
    body_parts: List[bytes] = []

    for name, value in fields.items():
        part = (
            f"--{boundary}\r\n"
            f'Content-Disposition: form-data; name="{name}"\r\n'
            f"\r\n"
            f"{value}\r\n"
        ).encode("utf-8")
        body_parts.append(part)

    if files:
        for name, (filename, data, content_type) in files.items():
            header = (
                f"--{boundary}\r\n"
                f'Content-Disposition: form-data; name="{name}"; filename="{filename}"\r\n'
                f"Content-Type: {content_type}\r\n"
                f"\r\n"
            ).encode("utf-8")
            body_parts.append(header + data + b"\r\n")

    body_parts.append(f"--{boundary}--\r\n".encode("utf-8"))
    return b"".join(body_parts), f"multipart/form-data; boundary={boundary}"


def tmn_get_json(url: str) -> Dict:
    req = urllib.request.Request(url, headers={"Accept": "application/json"})
    with urllib.request.urlopen(req, timeout=20) as response:
        return json.loads(response.read().decode("utf-8"))


def tmn_publish_request(
    api_base_url: str,
    api_key: str,
    fields: Dict[str, str],
    zip_path: str,
    dry_run: bool,
) -> Dict:
    """POST /publish to TMN. Returns the parsed JSON response."""
    if dry_run:
        print(f"  [DRYRUN] Would POST {api_base_url}/publish")
        for k, v in fields.items():
            preview = v[:120].replace("\n", " ") + ("..." if len(v) > 120 else "")
            print(f"    {k}: {preview}")
        print(f"    file: {os.path.basename(zip_path)}")
        return {"success": True, "dry_run": True}

    with open(zip_path, "rb") as f:
        zip_data = f.read()

    body, content_type = encode_multipart(
        fields=fields,
        files={"file": (os.path.basename(zip_path), zip_data, "application/zip")},
    )
    req = urllib.request.Request(
        f"{api_base_url}/publish",
        data=body,
        headers={
            "Authorization": f"Bearer {api_key}",
            "Content-Type": content_type,
            "Accept": "application/json",
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
            "Accept-Language": "en-US,en;q=0.9",
            "Origin": "https://themodnetwork.com",
            "Referer": "https://themodnetwork.com/upload",
        },
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=60) as response:
        return json.loads(response.read().decode("utf-8"))


# ---------------------------------------------------------------------------
# Config helpers
# ---------------------------------------------------------------------------

def get_env_api_key(config: Dict) -> Tuple[str, str]:
    env_var = str(config.get("api_key_env_var", DEFAULT_API_KEY_ENV_VAR)).strip() or DEFAULT_API_KEY_ENV_VAR
    return env_var, os.getenv(env_var, "").strip()


def resolve_action(intent: str, tmn_slug: str) -> Tuple[str, List[str]]:
    notes: List[str] = []
    if intent == "skip":
        return "skip", notes
    if intent == "publish":
        if tmn_slug:
            notes.append("Publish intent but tmn_slug already set; consider changing to 'update'.")
        return "publish", notes
    if intent == "update":
        if not tmn_slug:
            notes.append("Update intent requires tmn_slug to be set in config.")
            return "review", notes
        return "update", notes
    notes.append("Intent is 'review'; set to 'publish' or 'update' to include in upload.")
    return "review", notes


# ---------------------------------------------------------------------------
# Plan building
# ---------------------------------------------------------------------------

def build_plan(config: Dict) -> Dict:
    release_mods = gather_release_mods()
    zip_paths = gather_mod_zip_paths()
    compat_data = load_compat_csv()
    config_mods = config.get("mods", {})
    if not isinstance(config_mods, dict):
        config_mods = {}

    api_base_url = str(config.get("api_base_url", DEFAULT_API_BASE_URL)).rstrip("/")
    global_game_version = str(config.get("game_version_default", "")).strip()
    global_release_type = (
        str(config.get("release_type_default", DEFAULT_RELEASE_TYPE)).strip() or DEFAULT_RELEASE_TYPE
    )
    tmn_game_id = config.get("tmn_game_id", None)

    mod_entries: List[Dict] = []
    summary = {
        "total": 0, "publish": 0, "update": 0, "review": 0, "skip": 0,
        "missing_zip": 0, "missing_compat": 0, "missing_config": 0,
    }

    for base_name in sorted(release_mods.keys()):
        release_entry = release_mods[base_name]
        config_entry = config_mods.get(base_name, {})
        if not isinstance(config_entry, dict):
            config_entry = {}
        compat_row = compat_data.get(base_name, {})

        intent = normalize_intent(config_entry.get("intent", "review"))
        tmn_slug = str(config_entry.get("tmn_slug", "")).strip()
        tmn_mod_id = str(config_entry.get("tmn_mod_id", "")).strip()
        action, notes = resolve_action(intent, tmn_slug)

        zip_path = zip_paths.get(base_name, "")
        if not zip_path:
            notes = list(notes) + ["Missing zip in 04_DownloadZips."]
            summary["missing_zip"] += 1

        if not compat_row:
            notes = list(notes) + [
                "Not found in Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv."
            ]
            summary["missing_compat"] += 1

        if not config_entry:
            summary["missing_config"] += 1

        # Resolve mod_side
        mod_side_override = str(config_entry.get("mod_side_override", "")).strip()
        if mod_side_override:
            mod_side = mod_side_override
        else:
            mod_type_id = safe_int(compat_row.get("MOD_TYPE_ID", -1))
            mod_side = map_mod_type_to_side(mod_type_id)
            if mod_side is None:
                mod_side = ""
                notes = list(notes) + [
                    f"MOD_TYPE_ID={mod_type_id} cannot be auto-mapped; set mod_side_override."
                ]

        # Resolve game_version
        game_version = (
            str(config_entry.get("game_version_override", "")).strip()
            or str(compat_row.get("TESTED_GAME_VERSION", "")).strip()
            or global_game_version
        )

        # Resolve release_type
        release_type = (
            str(config_entry.get("release_type_override", "")).strip()
            or global_release_type
        )

        mod_entries.append({
            "base_name": base_name,
            "folder_name": release_entry["folder_name"],
            "mod_name": release_entry["mod_name"],
            "version": release_entry["version"],
            "description": release_entry["description"],
            "readme_path": release_entry["readme_path"],
            "zip_path": zip_path,
            "zip_name": os.path.basename(zip_path) if zip_path else f"{base_name}.zip",
            "intent": intent,
            "action": action,
            "tmn_slug": tmn_slug,
            "tmn_mod_id": tmn_mod_id,
            "page_url": str(config_entry.get("page_url", "")).strip(),
            "mod_side": mod_side,
            "game_version": game_version,
            "release_type": release_type,
            "tmn_category": str(config_entry.get("tmn_category", "")).strip(),
            "tmn_thumbnail_path": str(config_entry.get("tmn_thumbnail_path", "")).strip(),
            "tmn_screenshot_paths": config_entry.get("tmn_screenshot_paths", []),
            "long_description_override": str(config_entry.get("long_description_override", "")).strip(),
            "install_description_override": str(config_entry.get("install_description_override", "")).strip(),
            "tmn_game_id": tmn_game_id,
            "notes": notes,
        })
        summary["total"] += 1
        summary[action] += 1

    return {
        "generated_at": dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "api_base_url": api_base_url,
        "summary": summary,
        "mods": mod_entries,
    }


# ---------------------------------------------------------------------------
# Upload field builder
# ---------------------------------------------------------------------------

def build_upload_fields(entry: Dict, is_new: bool) -> Tuple[Dict[str, str], List[str]]:
    """Build the POST /publish form fields for one mod entry.
    Returns (fields_dict, warnings_list).
    """
    warnings: List[str] = []
    fields: Dict[str, str] = {}
    readme_text = load_text_file(entry.get("readme_path", ""))

    # slug — update only (identifies the existing mod page)
    if not is_new:
        fields["slug"] = entry["tmn_slug"]

    # title — new mods only
    if is_new:
        title = entry.get("mod_name", "").strip()
        if title:
            fields["title"] = title
        else:
            warnings.append("mod_name is empty; title not sent.")

    # version
    fields["version"] = entry.get("version", "0.0.0")

    # description (short — from ModInfo.xml)
    short_desc = normalize_single_line(entry.get("description", ""))
    if short_desc:
        fields["description"] = short_desc

    # long_description (full page body — HTML)
    if entry.get("long_description_override"):
        fields["long_description"] = entry["long_description_override"]
    elif readme_text:
        html = build_full_description_html(readme_text)
        if html:
            fields["long_description"] = html
        else:
            warnings.append("Could not build long_description from README.")
    else:
        warnings.append("No README.md found; long_description not sent.")

    # changelog (release notes — plain markdown, not HTML)
    if readme_text:
        changelog_text = build_changelog_text(readme_text)
        if changelog_text:
            fields["changelog"] = changelog_text
        else:
            warnings.append("Could not extract changelog from README.")

    # game_id (numeric TMN identifier — must be set in config)
    tmn_game_id = entry.get("tmn_game_id")
    if tmn_game_id is not None:
        fields["game_id"] = str(tmn_game_id)
    else:
        warnings.append("tmn_game_id not set in config; TMN may reject new mod creation.")

    # game_version
    game_version = entry.get("game_version", "")
    if game_version:
        fields["game_version"] = game_version

    # mod_side
    mod_side = entry.get("mod_side", "")
    if mod_side:
        fields["mod_side"] = mod_side
    else:
        warnings.append("mod_side not resolved; not sending.")

    # requirements — all AGF mods have none
    fields["requirements"] = DEFAULT_REQUIREMENTS

    # release_type
    fields["release_type"] = entry.get("release_type", DEFAULT_RELEASE_TYPE)

    return fields, warnings


# ---------------------------------------------------------------------------
# Mode: init-config
# ---------------------------------------------------------------------------

def cmd_init_config(args) -> int:
    config_path = args.config or DEFAULT_CONFIG_PATH
    if os.path.isfile(config_path):
        print(f"Config already exists: {config_path}")
        print("Delete or rename it before running init-config again.")
        return 1

    release_mods = gather_release_mods()
    mods_section: Dict[str, Dict] = {}
    for base_name in sorted(release_mods.keys()):
        mods_section[base_name] = {
            "intent": "review",
            "tmn_slug": "",
            "tmn_mod_id": "",
            "page_url": "",
            "tmn_category": "",
            "tmn_thumbnail_path": "",
            "tmn_screenshot_paths": [],
            "mod_side_override": "",
            "game_version_override": "",
            "release_type_override": "",
            "long_description_override": "",
            "install_description_override": "",
        }

    config = {
        "api_base_url": DEFAULT_API_BASE_URL,
        "api_key_env_var": DEFAULT_API_KEY_ENV_VAR,
        "tmn_game_id": None,
        "game_version_default": "2.6",
        "release_type_default": DEFAULT_RELEASE_TYPE,
        "mods": mods_section,
    }
    write_json_file(config_path, config, dry_run=False)
    print(f"Created: {config_path}")
    print(f"  {len(mods_section)} mods added with intent='review'")
    print("  Next steps:")
    print("    1. Set tmn_game_id once known (numeric 7D2D game ID)")
    print("    2. Set intent='publish' for each mod you want to upload")
    print("    3. Set tmn_category per mod")
    print("    4. Run build-plan, then prepare-upload to preview")
    return 0


# ---------------------------------------------------------------------------
# Mode: build-plan
# ---------------------------------------------------------------------------

def cmd_build_plan(args) -> int:
    config_path = args.config or DEFAULT_CONFIG_PATH
    output_path = getattr(args, "output", None) or DEFAULT_PLAN_OUTPUT_PATH

    config = load_json_file(config_path)
    if not config:
        print(f"Config not found or empty: {config_path}")
        print("Run 'init-config' first.")
        return 1

    plan = build_plan(config)
    write_json_file(output_path, plan, dry_run=False)

    s = plan["summary"]
    print("=== TMN PLAN SUMMARY ===")
    print(f"Generated:      {plan['generated_at']}")
    print(f"Total mods:     {s['total']}")
    print(f"  publish:      {s['publish']}")
    print(f"  update:       {s['update']}")
    print(f"  review:       {s['review']}")
    print(f"  skip:         {s['skip']}")
    print(f"  missing zip:      {s['missing_zip']}")
    print(f"  missing compat:   {s['missing_compat']}")
    print(f"  missing config:   {s['missing_config']}")
    print(f"\nPlan written to: {output_path}")

    for entry in plan["mods"]:
        if entry["notes"]:
            print(f"\n[{entry['action'].upper()}] {entry['base_name']} v{entry['version']}")
            for note in entry["notes"]:
                print(f"  ! {note}")
    return 0


# ---------------------------------------------------------------------------
# Mode: check-live
# ---------------------------------------------------------------------------

def cmd_check_live(args) -> int:
    config_path = args.config or DEFAULT_CONFIG_PATH
    config = load_json_file(config_path)
    if not config:
        print(f"Config not found: {config_path}")
        return 1

    release_mods = gather_release_mods()
    config_mods = config.get("mods", {})
    if not isinstance(config_mods, dict):
        config_mods = {}
    api_base_url = str(config.get("api_base_url", DEFAULT_API_BASE_URL)).rstrip("/")
    only = (getattr(args, "only", None) or "").strip()

    print("=== TMN LIVE CHECK ===")
    checked = 0
    for base_name in sorted(config_mods.keys()):
        if only and base_name != only:
            continue
        entry = config_mods[base_name]
        tmn_slug = str(entry.get("tmn_slug", "")).strip()
        if not tmn_slug:
            continue
        local_version = str(release_mods.get(base_name, {}).get("version", "?"))
        try:
            data = tmn_get_json(f"{api_base_url}/mod/{tmn_slug}")
            mod = data.get("mod", {})
            live_version = str(mod.get("version", "?"))
            live_title = str(mod.get("title", "?"))
            release_count = mod.get("release_count", "?")
            status = "UP TO DATE" if live_version == local_version else "OUTDATED"
            print(f"[{status}] {base_name}")
            print(f"  slug={tmn_slug} | title={live_title}")
            print(f"  local v{local_version} | live v{live_version} | releases={release_count}")
            checked += 1
        except urllib.error.HTTPError as ex:
            print(f"[HTTP {ex.code}] {base_name} (slug: {tmn_slug})")
        except urllib.error.URLError as ex:
            print(f"[NETWORK ERROR] {base_name}: {ex}")

    if checked == 0 and not only:
        print("No mods with tmn_slug in config. Run execute-upload (publish) first.")
    return 0


# ---------------------------------------------------------------------------
# Mode: prepare-upload
# ---------------------------------------------------------------------------

def cmd_prepare_upload(args) -> int:
    plan_path = getattr(args, "plan", None) or DEFAULT_PLAN_OUTPUT_PATH
    plan = load_json_file(plan_path)
    if not plan:
        print(f"Plan not found: {plan_path}")
        print("Run 'build-plan' first.")
        return 1

    only = (getattr(args, "only", None) or "").strip()
    mods = plan.get("mods", [])
    shown = 0

    print("=== TMN PREPARE UPLOAD ===")
    for entry in mods:
        action = entry.get("action", "review")
        base_name = entry.get("base_name", "")
        if only and base_name != only:
            continue
        if action not in ("publish", "update"):
            continue

        is_new = action == "publish"
        fields, warnings = build_upload_fields(entry, is_new)

        print(f"\n[{action.upper()}] {base_name} v{entry['version']}")
        print(f"  zip:   {entry.get('zip_name', '(missing)')}")
        for k, v in fields.items():
            preview = v[:200].replace("\n", " ") + ("..." if len(v) > 200 else "")
            print(f"  {k}: {preview}")
        for w in warnings:
            print(f"  ! WARNING: {w}")
        for n in entry.get("notes", []):
            print(f"  ! NOTE: {n}")
        shown += 1

    if shown == 0:
        msg = "No mods with action=publish or action=update."
        if only:
            msg += f" (filter: --only {only})"
        print(msg)
    return 0


# ---------------------------------------------------------------------------
# Mode: update-page
# ---------------------------------------------------------------------------

CREATOR_API_BASE = "https://themodnetwork.com/api"

def tmn_update_page_request(
    mod_id: str,
    api_key: str,
    fields: Dict,
    dry_run: bool,
    session_token: str = "",
) -> Dict:
    """PATCH /api/creator-mods/{mod_id} with JSON body."""
    url = f"{CREATOR_API_BASE}/creator-mods/{mod_id}"
    if dry_run:
        print(f"  [DRYRUN] Would PATCH {url}")
        for k, v in fields.items():
            preview = str(v)[:120].replace("\n", " ") + ("..." if len(str(v)) > 120 else "")
            print(f"    {k}: {preview}")
        return {"success": True, "dry_run": True}

    bearer = session_token if session_token else api_key
    body = json.dumps(fields).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=body,
        headers={
            "Authorization": f"Bearer {bearer}",
            "Content-Type": "application/json",
            "Accept": "application/json",
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
            "Origin": "https://themodnetwork.com",
            "Referer": "https://themodnetwork.com/manage-mods",
        },
        method="PATCH",
    )
    with urllib.request.urlopen(req, timeout=60) as response:
        return json.loads(response.read().decode("utf-8"))


def cmd_update_page(args) -> int:
    config_path = args.config or DEFAULT_CONFIG_PATH
    plan_path = getattr(args, "plan", None) or DEFAULT_PLAN_OUTPUT_PATH
    dry_run = getattr(args, "dry_run", False)
    only = (getattr(args, "only", None) or "").strip()

    config = load_json_file(config_path)
    plan = load_json_file(plan_path)
    if not plan:
        print(f"Plan not found: {plan_path}")
        print("Run 'build-plan' first.")
        return 1

    env_var, api_key = get_env_api_key(config)
    cli_key = (getattr(args, "api_key", None) or "").strip()
    if cli_key:
        api_key = cli_key
    session_token = (getattr(args, "session_token", None) or "").strip()
    if not dry_run and not api_key and not session_token:
        print(f"Missing auth. Set env var {env_var}, pass --api-key KEY, or pass --session-token JWT.")
        return 1

    mods = plan.get("mods", [])
    processed = 0
    failures = 0

    print("=== TMN UPDATE PAGE ===")
    if dry_run:
        print("[DRY RUN — no API calls will be made]")
    print()

    for entry in mods:
        base_name = entry.get("mod_name", "")
        if only and base_name != only:
            continue
        action = entry.get("action", "review")
        if action not in ("update", "publish"):
            continue

        mod_id = entry.get("tmn_mod_id", "").strip()
        if not mod_id:
            print(f"[SKIP] {base_name} — no tmn_mod_id set")
            continue

        readme_text = load_text_file(entry.get("readme_path", ""))
        fields: Dict = {}

        # long_description
        ld_override = entry.get("long_description_override", "").strip()
        if ld_override:
            fields["long_description"] = ld_override
        elif readme_text:
            ld = build_full_description_html(readme_text)
            if ld:
                fields["long_description"] = ld

        # install_guide
        ig_override = entry.get("install_description_override", "").strip()
        if ig_override:
            fields["install_guide"] = ig_override
        elif readme_text:
            ig = build_install_description_html(readme_text)
            if ig:
                fields["install_guide"] = ig

        # basic metadata
        if entry.get("description"):
            fields["description"] = entry["description"]
        if entry.get("version"):
            fields["version"] = entry["version"]
        if entry.get("game_version"):
            fields["game_version"] = entry["game_version"]
        if entry.get("mod_side"):
            fields["mod_side"] = entry["mod_side"]
        if entry.get("requirements"):
            fields["requirements"] = entry["requirements"]

        print(f"[UPDATE-PAGE] {base_name} ...")
        try:
            result = tmn_update_page_request(mod_id, api_key, fields, dry_run, session_token=session_token)
            if dry_run or result.get("success") or result.get("mod") or result.get("id"):
                print(f"  [OK]")
                processed += 1
            else:
                error = result.get("error", result.get("message", str(result)))
                print(f"  [FAILED] {error}")
                failures += 1
        except urllib.error.HTTPError as ex:
            body = ""
            try:
                body = ex.read().decode("utf-8", errors="replace")[:400]
            except Exception:
                pass
            print(f"  [HTTP {ex.code}] {body}")
            failures += 1
        except urllib.error.URLError as ex:
            print(f"  [NETWORK ERROR] {ex}")
            failures += 1
        except Exception as ex:
            print(f"  [ERROR] {ex}")
            failures += 1

    if processed == 0 and failures == 0:
        print("No mods with action=update or action=publish and a tmn_mod_id.")
    print(f"\nDone. Processed: {processed} | Failures: {failures}")
    return 0 if failures == 0 else 1


# ---------------------------------------------------------------------------
# Mode: execute-upload
# ---------------------------------------------------------------------------

def cmd_execute_upload(args) -> int:
    config_path = args.config or DEFAULT_CONFIG_PATH
    plan_path = getattr(args, "plan", None) or DEFAULT_PLAN_OUTPUT_PATH
    dry_run = getattr(args, "dry_run", False)
    only = (getattr(args, "only", None) or "").strip()

    config = load_json_file(config_path)
    plan = load_json_file(plan_path)
    if not plan:
        print(f"Plan not found: {plan_path}")
        print("Run 'build-plan' first.")
        return 1

    env_var, api_key = get_env_api_key(config)
    cli_key = (getattr(args, "api_key", None) or "").strip()
    if cli_key:
        api_key = cli_key
    if not api_key and not dry_run:
        print(f"Missing TMN API key. Set env var {env_var} or pass --api-key KEY.")
        return 1

    api_base_url = str(plan.get("api_base_url", DEFAULT_API_BASE_URL)).rstrip("/")
    config_mods = config.get("mods", {})
    if not isinstance(config_mods, dict):
        config_mods = {}

    mods = plan.get("mods", [])
    processed = 0
    failures = 0

    print("=== TMN EXECUTE UPLOAD ===")
    if dry_run:
        print("[DRY RUN — no API calls will be made]\n")

    for entry in mods:
        action = entry.get("action", "review")
        base_name = entry.get("base_name", "")
        if only and base_name != only:
            continue
        if action not in ("publish", "update"):
            continue

        zip_path = entry.get("zip_path", "")
        if not zip_path or not os.path.isfile(zip_path):
            print(f"[SKIP] {base_name}: zip not found at '{zip_path}'")
            failures += 1
            continue

        is_new = action == "publish"
        fields, warnings = build_upload_fields(entry, is_new)
        for w in warnings:
            print(f"  ! {base_name}: {w}")

        print(f"[{action.upper()}] {base_name} v{entry['version']} ...", flush=True)
        try:
            result = tmn_publish_request(api_base_url, api_key, fields, zip_path, dry_run)

            if result.get("success") or result.get("dry_run"):
                returned_slug = str(result.get("slug", "")).strip()
                returned_mod_id = str(result.get("mod_id", "")).strip()
                returned_url = str(result.get("url", "")).strip()
                is_new_result = bool(result.get("is_new", False))
                tag = "CREATED" if is_new_result else "UPDATED"
                print(f"  [{tag}] slug={returned_slug or '?'} | url={returned_url or '?'}")

                # Persist slug and mod_id back to config
                if not dry_run and (returned_slug or returned_mod_id or returned_url):
                    if base_name not in config_mods:
                        config_mods[base_name] = {}
                    if returned_slug:
                        config_mods[base_name]["tmn_slug"] = returned_slug
                    if returned_mod_id:
                        config_mods[base_name]["tmn_mod_id"] = returned_mod_id
                    if returned_url:
                        config_mods[base_name]["page_url"] = returned_url
                    config["mods"] = config_mods
                    write_json_file(config_path, config, dry_run=False)

                processed += 1
            else:
                error = result.get("error", result.get("message", str(result)))
                print(f"  [FAILED] {error}")
                failures += 1

        except urllib.error.HTTPError as ex:
            body = ""
            try:
                body = ex.read().decode("utf-8", errors="replace")[:400]
            except Exception:
                pass
            print(f"  [HTTP {ex.code}] {body}")
            failures += 1
        except urllib.error.URLError as ex:
            print(f"  [NETWORK ERROR] {ex}")
            failures += 1
        except Exception as ex:
            print(f"  [ERROR] {ex}")
            failures += 1

    print(f"\nDone. Processed: {processed} | Failures: {failures}")
    return 0 if failures == 0 else 1


# ---------------------------------------------------------------------------
# main
# ---------------------------------------------------------------------------

def main() -> int:
    parser = argparse.ArgumentParser(
        description="Automate AGF mod publishing on The Mod Network.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument(
        "--config", metavar="PATH",
        help=f"Config file path (default: {DEFAULT_CONFIG_PATH})",
    )

    sub = parser.add_subparsers(dest="mode", required=True)

    sub.add_parser("init-config", help="Create modnetwork-config.json from scanned mods")

    bp = sub.add_parser("build-plan", help="Scan mods and generate upload plan JSON")
    bp.add_argument("--output", metavar="PATH", help=f"Plan output path (default: {DEFAULT_PLAN_OUTPUT_PATH})")

    cl = sub.add_parser("check-live", help="Query live TMN pages for mods with a known tmn_slug")
    cl.add_argument("--only", metavar="MOD_NAME", help="Limit to one mod")

    pu = sub.add_parser("prepare-upload", help="Preview fields that would be sent (no API calls)")
    pu.add_argument("--plan", metavar="PATH", help=f"Plan file (default: {DEFAULT_PLAN_OUTPUT_PATH})")
    pu.add_argument("--only", metavar="MOD_NAME", help="Limit to one mod")

    eu = sub.add_parser("execute-upload", help="POST /publish to TMN for publish/update mods")
    eu.add_argument("--plan", metavar="PATH", help=f"Plan file (default: {DEFAULT_PLAN_OUTPUT_PATH})")
    eu.add_argument("--only", metavar="MOD_NAME", help="Limit to one mod")
    eu.add_argument("--dry-run", action="store_true", help="Preview without making API calls")
    eu.add_argument("--api-key", metavar="KEY", help="TMN API key (overrides env var)")

    up = sub.add_parser("update-page", help="PATCH page metadata (long_description, install_guide, etc.)")
    up.add_argument("--plan", metavar="PATH", help=f"Plan file (default: {DEFAULT_PLAN_OUTPUT_PATH})")
    up.add_argument("--only", metavar="MOD_NAME", help="Limit to one mod")
    up.add_argument("--dry-run", action="store_true", help="Preview without making API calls")
    up.add_argument("--api-key", metavar="KEY", help="TMN API key (overrides env var)")
    up.add_argument("--session-token", metavar="JWT", help="Browser JWT session token (from DevTools) — overrides --api-key for creator API")

    args = parser.parse_args()
    handlers = {
        "init-config": cmd_init_config,
        "build-plan": cmd_build_plan,
        "check-live": cmd_check_live,
        "prepare-upload": cmd_prepare_upload,
        "execute-upload": cmd_execute_upload,
        "update-page": cmd_update_page,
    }
    return handlers[args.mode](args)


if __name__ == "__main__":
    sys.exit(main())
