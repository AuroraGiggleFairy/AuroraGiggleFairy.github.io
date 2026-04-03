import argparse
import csv
import datetime as dt
import difflib
import hashlib
import json
import os
import re
import shutil
import sys
import threading
import urllib.error
import urllib.request
import zipfile
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Tuple
import xml.etree.ElementTree as ET

# =============================================================
# CONFIG
# =============================================================
VS_CODE_ROOT = os.path.dirname(os.path.abspath(__file__))
LANE_DRAFT_PREFERRED = os.path.join(VS_CODE_ROOT, "01_Draft")
LANE_ACTIVE_BUILD_PREFERRED = os.path.join(VS_CODE_ROOT, "02_ActiveBuild")
LANE_RELEASE_SOURCE_PREFERRED = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
LANE_DOWNLOAD_ZIPS_PREFERRED = os.path.join(VS_CODE_ROOT, "04_DownloadZips")

LANE_DRAFT_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods2.In-Progress")
LANE_ACTIVE_BUILD_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods0.Staging")
LANE_RELEASE_SOURCE_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods1.PublishReady")
LANE_DOWNLOAD_ZIPS_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods3.zip")


def resolve_lane_path(preferred: str, legacy: str) -> str:
    """Prefer the new lane folder name; fall back to legacy names for compatibility."""
    if os.path.isdir(preferred):
        return preferred
    if os.path.isdir(legacy):
        return legacy
    return preferred


IN_PROGRESS = resolve_lane_path(LANE_DRAFT_PREFERRED, LANE_DRAFT_LEGACY)
STAGING = resolve_lane_path(LANE_ACTIVE_BUILD_PREFERRED, LANE_ACTIVE_BUILD_LEGACY)
PUBLISH_READY = resolve_lane_path(LANE_RELEASE_SOURCE_PREFERRED, LANE_RELEASE_SOURCE_LEGACY)
GAME_MODS = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods"
ZIP_OUTPUT = resolve_lane_path(LANE_DOWNLOAD_ZIPS_PREFERRED, LANE_DOWNLOAD_ZIPS_LEGACY)
QUOTES_DIR = os.path.join(VS_CODE_ROOT, "_Quotes")
LOGS_DIR = os.path.join(VS_CODE_ROOT, "Logs")
MAIN_LOG_MAX_FILES = 10

COMPAT_CSV = os.path.join(VS_CODE_ROOT, "HELPER_ModCompatibility.csv")
MOD_README_TEMPLATE = os.path.join(VS_CODE_ROOT, "TEMPLATE-ModReadMes.md")
MAIN_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "TEMPLATE-MainReadMe.md")
DISCORD_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "05_GigglePackReleaseData", "TEMPLATE-DiscordUpdate.md")
MAIN_README_PATH = os.path.join(VS_CODE_ROOT, "README.md")

AGF_PREFIXES = ("AGF-", "zzzAGF-")
BASE_DOWNLOAD_URL = "https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips"
BACKPACK_DEFAULT_ACTIVE_TOKEN = "084Slots"
GAME_OPTIONALS_BACKPACK_DIR = ".Optionals-Backpack"
GAME_OPTIONALS_HUDPLUS_DIR = ".Optionals-HUDPlus"
GAME_OPTIONALS_4MODDERS_DIR = ".Optionals-4Modders"
RELEASE_META_DIR_NAME = ".release"
GIGGLEPACK_RELEASE_DATA_DIR = "GigglePack"
GIGGLEPACK_RELEASE_ROOT_DIR = os.path.join(VS_CODE_ROOT, "05_GigglePackReleaseData")
GIGGLEPACK_CANONICAL_ZIP = "00_GigglePack_All.zip"
GIGGLEPACK_VERSIONED_ZIP_PREFIX = "AGF-GigglePack-v"
GIGGLEPACK_MAJOR_BUMP_MARKER = "gigglepack-major-bump.txt"
DISCORD_WEBHOOK_ENV_VAR = "AGF_DISCORD_WEBHOOK_URL"
GIGGLEPACK_V100_FOCUS_MODS = (
    "AGF-NoEAC-ExpandedInteractionPrompts",
    "AGF-NoEAC-ScreamerAlert",
)
README_COMPAT_FIELDS = (
    "EAC_FRIENDLY",
    "SERVER_SIDE",
    "CLIENT_REQUIRED",
    "SAFE_TO_INSTALL",
    "SAFE_TO_REMOVE",
    "UNIQUE",
    "QUOTE_FILE",
)


# =============================================================
# LOGGING
# =============================================================
@dataclass
class RunStats:
    scanned_workspace_mods: int = 0
    scanned_game_mods: int = 0
    synced_pull_from_game: int = 0
    synced_push_to_game: int = 0
    sync_conflicts: int = 0
    moved_to_publish_ready: int = 0
    moved_to_in_progress: int = 0
    renamed_folders: int = 0
    csv_added_rows: int = 0
    csv_removed_rows: int = 0
    quote_files_created: int = 0
    quote_files_renamed: int = 0
    quote_files_blanked_none: int = 0
    readmes_written: int = 0
    readable_txt_written: int = 0
    pushed_back_to_game: int = 0
    mod_zips_created: int = 0
    pack_zips_created: int = 0
    promoted_to_publish_ready: int = 0
    warnings: int = 0
    errors: int = 0


class Logger:
    def __init__(self, verbose: bool = False, dry_run: bool = False) -> None:
        self.verbose = verbose
        self.dry_run = dry_run
        self.stats = RunStats()
        self._messages: List[str] = []
        self._lock = threading.Lock()

    def _emit(self, level: str, message: str) -> None:
        ts = dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        line = f"[{ts}] [{level}] {message}"
        with self._lock:
            self._messages.append(line)
        if level in ("WARN", "ERROR") or self.verbose:
            print(line)

    def info(self, message: str) -> None:
        self._emit("INFO", message)

    def warn(self, message: str) -> None:
        self.stats.warnings += 1
        self._emit("WARN", message)

    def error(self, message: str) -> None:
        self.stats.errors += 1
        self._emit("ERROR", message)

    def _extract_mod_changes(self) -> List[Tuple[str, str]]:
        """Build a deduplicated list of mod changes detected in this run's log messages."""
        action_by_mod: Dict[str, List[str]] = {}

        action_map: List[Tuple[re.Pattern[str], str]] = [
            (re.compile(r"sync-work repair:"), "repaired in game"),
            (re.compile(r"sync-work push:"), "pushed to game"),
            (re.compile(r"sync-work pull:"), "pulled from game"),
            (re.compile(r"sync-work tie resolved"), "tie resolved (game updated)"),
            (re.compile(r"Promoted new mod:"), "promoted (new)"),
            (re.compile(r"Promoted update:"), "promoted (updated)"),
            (re.compile(r"Promoted refresh:"), "promoted (refresh)"),
            (re.compile(r"Moved to PublishReady:"), "moved to publish-ready"),
            (re.compile(r"Moved to In-Progress:"), "moved to in-progress"),
            (re.compile(r"Workspace is newer for"), "workspace newer (pushed to game)"),
            (re.compile(r"Game is newer for"), "game newer (pulled to workspace)"),
            (re.compile(r"Version tie resolved for"), "version tie resolved"),
        ]

        mod_pattern = re.compile(r"\b(AGF-[A-Za-z0-9-]+-v[0-9][0-9A-Za-z\.-]*)\b")

        for line in self._messages:
            message_match = re.search(r"\[[^\]]+\]\s\[[^\]]+\]\s(.+)$", line)
            message = message_match.group(1) if message_match else line

            action = None
            for pattern, label in action_map:
                if pattern.search(message):
                    action = label
                    break
            if not action:
                continue

            mod_match = mod_pattern.search(message)
            if not mod_match:
                continue

            mod_name = mod_match.group(1)
            if mod_name not in action_by_mod:
                action_by_mod[mod_name] = []
            if action not in action_by_mod[mod_name]:
                action_by_mod[mod_name].append(action)

        changes: List[Tuple[str, str]] = []
        for mod_name in sorted(action_by_mod.keys(), key=lambda name: name.lower()):
            actions = ", ".join(action_by_mod[mod_name])
            changes.append((mod_name, actions))
        return changes

    def get_mod_change_summary_lines(self) -> List[str]:
        changes = self._extract_mod_changes()
        if not changes:
            return ["none"]
        return [f"{mod_name} | {actions}" for mod_name, actions in changes]

    def write_log_file(self) -> Optional[str]:
        try:
            os.makedirs(LOGS_DIR, exist_ok=True)
            stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
            mode = "dryrun" if self.dry_run else "live"
            path = os.path.join(LOGS_DIR, f"main-script-{mode}-{stamp}.log")
            with open(path, "w", encoding="utf-8") as f:
                for line in self._messages:
                    f.write(line + "\n")
                f.write("\n=== SUMMARY ===\n")
                for key, value in self.stats.__dict__.items():
                    f.write(f"{key}: {value}\n")

                f.write("\n=== MOD CHANGES ===\n")
                for line in self.get_mod_change_summary_lines():
                    f.write(f"{line}\n")

            # Keep only the newest main-script logs to avoid unbounded growth.
            log_candidates: List[Tuple[float, str]] = []
            for name in os.listdir(LOGS_DIR):
                if not (name.startswith("main-script-") and name.endswith(".log")):
                    continue
                full_path = os.path.join(LOGS_DIR, name)
                if os.path.isfile(full_path):
                    log_candidates.append((os.path.getmtime(full_path), full_path))

            log_candidates.sort(key=lambda item: item[0], reverse=True)
            for _, old_path in log_candidates[MAIN_LOG_MAX_FILES:]:
                try:
                    os.remove(old_path)
                except Exception:
                    pass

            return path
        except Exception:
            return None


# =============================================================
# HELPERS
# =============================================================
def is_agf_mod(folder: str) -> bool:
    return folder.startswith(AGF_PREFIXES)


def is_backpack_mod(folder: str) -> bool:
    return folder.startswith("AGF-BackpackPlus-")


def is_hudplus_mod(folder: str) -> bool:
    return folder.startswith("AGF-HUDPlus-")


def is_hudpluszother_mod(folder: str) -> bool:
    return folder.startswith("AGF-HUDPluszOther-")


def is_4modders_mod(folder: str) -> bool:
    return folder.startswith("AGF-4Modders-")


def get_base_mod_name(name: str) -> str:
    return re.sub(r"-v\d+\.\d+(\.\d+)*$", "", name)


def parse_modinfo(modinfo_path: str, fallback_name: str) -> Tuple[str, str]:
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        name_tag = root.find("Name")
        version_tag = root.find("Version")
        mod_name = name_tag.attrib.get("value", fallback_name) if name_tag is not None else fallback_name
        mod_version = version_tag.attrib.get("value", "0.0.0") if version_tag is not None else "0.0.0"
        return mod_name, mod_version
    except Exception:
        return fallback_name, "0.0.0"


def get_modinfo_display_name(modinfo_path: str, fallback_name: str) -> str:
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        display_tag = root.find("DisplayName")
        if display_tag is not None:
            display_name = display_tag.attrib.get("value", "").strip()
            if display_name:
                return display_name
    except Exception:
        pass
    return fallback_name


def format_version_for_display(version: str, display_name: str) -> str:
    version_text = (version or "0.0.0").strip()
    if "BETA" in (display_name or "") and not version_text.endswith("-BETA"):
        return f"{version_text}-BETA"
    return version_text


def get_modinfo_version(folder_path: str) -> Optional[str]:
    modinfo_path = os.path.join(folder_path, "ModInfo.xml")
    if not os.path.exists(modinfo_path):
        return None
    _, version = parse_modinfo(modinfo_path, "")
    return version or None


def compare_versions(v1: Optional[str], v2: Optional[str]) -> int:
    def to_tuple(v: Optional[str]) -> Tuple[int, ...]:
        parts = re.findall(r"\d+", v or "0.0.0")
        return tuple(int(p) for p in parts)

    t1 = to_tuple(v1)
    t2 = to_tuple(v2)
    maxlen = max(len(t1), len(t2))
    t1 += (0,) * (maxlen - len(t1))
    t2 += (0,) * (maxlen - len(t2))
    return (t1 > t2) - (t1 < t2)


def hash_file(path: str) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(65536), b""):
            h.update(chunk)
    return h.hexdigest()


def hash_directory(path: str) -> str:
    h = hashlib.sha256()
    for root, _, files in os.walk(path):
        rel_root = os.path.relpath(root, path).replace("\\", "/")
        for file in sorted(files):
            full_path = os.path.join(root, file)
            rel_path = os.path.join(rel_root, file).replace("\\", "/")
            h.update(rel_path.encode("utf-8", errors="replace"))
            h.update(hash_file(full_path).encode("ascii"))
    return h.hexdigest()


def scan_mod_folders(base_dir: str) -> Dict[str, str]:
    if not os.path.isdir(base_dir):
        return {}
    return {
        name: os.path.join(base_dir, name)
        for name in os.listdir(base_dir)
        if os.path.isdir(os.path.join(base_dir, name)) and is_agf_mod(name)
    }


def maybe_remove_dir(path: str, dry_run: bool, log: Logger) -> bool:
    if not os.path.exists(path):
        return True
    if dry_run:
        log.info(f"[DRYRUN] Would remove directory: {path}")
        return True
    try:
        shutil.rmtree(path)
        return True
    except Exception as ex:
        log.error(f"Failed to remove directory {path}: {ex}")
        return False


def maybe_copytree(src: str, dst: str, dry_run: bool, log: Logger) -> bool:
    if dry_run:
        log.info(f"[DRYRUN] Would copy directory: {src} -> {dst}")
        return True
    try:
        if os.path.exists(dst):
            shutil.rmtree(dst)
        shutil.copytree(src, dst)
        return True
    except Exception as ex:
        log.error(f"Failed to copy directory {src} -> {dst}: {ex}")
        return False


def maybe_move(src: str, dst: str, dry_run: bool, log: Logger) -> bool:
    if dry_run:
        log.info(f"[DRYRUN] Would move directory: {src} -> {dst}")
        return True
    try:
        if os.path.exists(dst):
            shutil.rmtree(dst)
        shutil.move(src, dst)
        return True
    except Exception as ex:
        log.error(f"Failed to move directory {src} -> {dst}: {ex}")
        return False


def markdown_to_text(md: str) -> str:
    md = re.sub(r"```[\s\S]*?```", "", md)
    md = re.sub(r"!\[[^\]]*\]\([^\)]*\)", "", md)
    md = re.sub(r"\[([^\]]+)\]\(([^\)]+)\)", r"\1: \2", md)
    md = re.sub(r"[`*_~]", "", md)
    md = re.sub(r"^---+$", "\n" + "=" * 40 + "\n", md, flags=re.MULTILINE)
    md = re.sub(r"^#+\s*", "", md, flags=re.MULTILINE)
    md = re.sub(r"^>\s?", "", md, flags=re.MULTILINE)
    md = re.sub(r"<[^>]+>", "", md)
    md = re.sub(r"\n{3,}", "\n\n", md)
    return md.strip()


def format_blockquote(text: str) -> str:
    if not text.strip():
        return ""
    lines = text.splitlines()
    return "\n".join(f"> {line}" if line.strip() else ">" for line in lines)


def markdown_features_to_html(features_text: str) -> str:
    lines = [line.rstrip() for line in features_text.strip().splitlines() if line.strip()]
    if not lines:
        return ""
    html = ""
    stack: List[int] = []
    for line in lines:
        indent = len(line) - len(line.lstrip(" "))
        indent += 4 * (len(line) - len(line.lstrip("\t")))
        content = line.lstrip("-* \t").strip()
        while stack and indent < stack[-1]:
            html += "</ul>"
            stack.pop()
        if not stack or indent > stack[-1]:
            html += "<ul>"
            stack.append(indent)
        html += f"<li>{content}</li>"
    while stack:
        html += "</ul>"
        stack.pop()
    return html


def extract_readme_block(readme_path: str, start_marker: str, end_marker: str) -> str:
    if not os.path.exists(readme_path):
        return ""
    try:
        with open(readme_path, "r", encoding="utf-8") as f:
            content = f.read()
        start = content.find(start_marker)
        end = content.find(end_marker)
        if start != -1 and end != -1:
            return content[start + len(start_marker):end]
    except Exception:
        return ""
    return ""


def sanitize_preserved_readme_block(block: str) -> str:
    """Clean malformed legacy markers from preserved README sections."""
    if not block:
        return block

    # Some legacy files contain leftover chevrons right after section markers.
    cleaned = re.sub(r"^[ \t]*>+[ \t]*", "", block)
    cleaned = re.sub(r"^\s*>+\s*$", "", cleaned, flags=re.MULTILINE)
    cleaned = re.sub(r"\n{3,}", "\n\n", cleaned)
    if cleaned and not cleaned.startswith("\n"):
        cleaned = "\n" + cleaned
    return cleaned


def extract_mod_description_from_modinfo(modinfo_path: str) -> str:
    if not os.path.exists(modinfo_path):
        return ""
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        desc_tag = root.find("Description")
        if desc_tag is not None and "value" in desc_tag.attrib:
            return desc_tag.attrib["value"]
    except Exception:
        return ""
    return ""


def zip_download_link(zip_name: str) -> str:
    return f"{BASE_DOWNLOAD_URL}/{zip_name}"


def mod_download_markdown_link(mod_name: str) -> str:
    base_name = get_base_mod_name(mod_name)
    return f"[{mod_name}]({zip_download_link(base_name + '.zip')})"


def extract_markdown_section(markdown_text: str, heading: str, next_heading: Optional[str] = None) -> str:
    start_match = re.search(rf"^##\s*{re.escape(heading)}\s*$", markdown_text, re.MULTILINE)
    if not start_match:
        return markdown_text.strip()

    body_start = start_match.end()
    if not next_heading:
        return markdown_text[body_start:].strip()

    end_match = re.search(
        rf"^##\s*{re.escape(next_heading)}\s*$",
        markdown_text[body_start:],
        re.MULTILINE,
    )
    if not end_match:
        return markdown_text[body_start:].strip()

    body_end = body_start + end_match.start()
    return markdown_text[body_start:body_end].strip()


def render_discord_post_from_template(
    release_version: str,
    now_iso: str,
    versioned_zip_name: str,
    previous_release_version: Optional[str],
    new_mod_entries: List[Dict[str, str]],
    updated_mod_entries: List[Dict[str, str]],
    renamed_mod_entries: List[Dict[str, str]],
    removed_mod_entries: List[Dict[str, str]],
    log: Logger,
) -> str:
    if not os.path.exists(DISCORD_TEMPLATE_PATH):
        log.warn(f"Discord template missing: {DISCORD_TEMPLATE_PATH}. Using fallback format.")
        return ""

    try:
        with open(DISCORD_TEMPLATE_PATH, "r", encoding="utf-8") as f:
            template_text = f.read()
    except Exception as ex:
        log.warn(f"Could not read Discord template {DISCORD_TEMPLATE_PATH}: {ex}. Using fallback format.")
        return ""

    template_start_marker = "<!-- DISCORD_TEMPLATE_START -->"
    template_end_marker = "<!-- DISCORD_TEMPLATE_END -->"
    if template_start_marker in template_text and template_end_marker in template_text:
        body = template_text.split(template_start_marker, 1)[1].split(template_end_marker, 1)[0].strip()
    else:
        body = extract_markdown_section(template_text, "Copy/Paste Format", "Notes")
    canonical_download_url = zip_download_link(GIGGLEPACK_CANONICAL_ZIP)
    release_file_label = f"GigglePack-v{release_version}.zip"
    static_map = {
        "GIGGLEPACK_VERSION": release_version,
        "RELEASE_STAMP": now_iso,
        "VERSIONED_ZIP_URL": canonical_download_url,
        # Compatibility aliases for older/custom templates.
        # latest zip is deprecated, so map these to the canonical download URL.
        "LATEST_ZIP_URL": canonical_download_url,
        "DOWNLOAD_URL": canonical_download_url,
        "DOWNLOAD_FILE_LABEL": release_file_label,
        "NEW_COUNT": str(len(new_mod_entries)),
        "UPDATED_COUNT": str(len(updated_mod_entries)),
        "RENAMED_COUNT": str(len(renamed_mod_entries)),
        "REMOVED_COUNT": str(len(removed_mod_entries)),
        "PREVIOUS_GIGGLEPACK_VERSION": previous_release_version or "None",
        "NEW_MOD_LINES": "\n".join(
            f"  - {mod_download_markdown_link(e['mod'])} (new: v{e.get('to_display', e['to'])})"
            for e in new_mod_entries
        )
        or "  - None",
        "UPDATED_MOD_LINES": "\n".join(
            f"  - {mod_download_markdown_link(e['mod'])} "
            f"(v{e.get('from_display', e['from'])} -> v{e.get('to_display', e['to'])})"
            for e in updated_mod_entries
        )
        or "  - None",
        "RENAMED_MOD_LINES": "\n".join(
            f"  - {mod_download_markdown_link(e['to_mod'])} "
            f"(renamed from {e['from_mod']}, v{e.get('version_display', e['version'])})"
            for e in renamed_mod_entries
        )
        or "  - None",
        "REMOVED_MOD_LINES": "\n".join(
            f"  - {e['mod']} (was v{e.get('from_display', e['from'])})" for e in removed_mod_entries
        )
        or "  - None",
    }

    def token_value(token: str) -> str:
        if token in static_map:
            return static_map[token]

        new_mod_match = re.fullmatch(r"NEW_MOD_(\d+)", token)
        if new_mod_match:
            idx = int(new_mod_match.group(1)) - 1
            return new_mod_entries[idx]["mod"] if 0 <= idx < len(new_mod_entries) else "None"

        new_mod_ver_match = re.fullmatch(r"NEW_MOD_(\d+)_VERSION", token)
        if new_mod_ver_match:
            idx = int(new_mod_ver_match.group(1)) - 1
            return (
                new_mod_entries[idx].get("to_display", new_mod_entries[idx]["to"])
                if 0 <= idx < len(new_mod_entries)
                else "None"
            )

        updated_mod_match = re.fullmatch(r"UPDATED_MOD_(\d+)", token)
        if updated_mod_match:
            idx = int(updated_mod_match.group(1)) - 1
            return updated_mod_entries[idx]["mod"] if 0 <= idx < len(updated_mod_entries) else "None"

        updated_old_match = re.fullmatch(r"UPDATED_MOD_(\d+)_OLD", token)
        if updated_old_match:
            idx = int(updated_old_match.group(1)) - 1
            return (
                updated_mod_entries[idx].get("from_display", updated_mod_entries[idx]["from"])
                if 0 <= idx < len(updated_mod_entries)
                else "None"
            )

        updated_new_match = re.fullmatch(r"UPDATED_MOD_(\d+)_NEW", token)
        if updated_new_match:
            idx = int(updated_new_match.group(1)) - 1
            return (
                updated_mod_entries[idx].get("to_display", updated_mod_entries[idx]["to"])
                if 0 <= idx < len(updated_mod_entries)
                else "None"
            )

        removed_mod_match = re.fullmatch(r"REMOVED_MOD_(\d+)", token)
        if removed_mod_match:
            idx = int(removed_mod_match.group(1)) - 1
            return removed_mod_entries[idx]["mod"] if 0 <= idx < len(removed_mod_entries) else "None"

        removed_ver_match = re.fullmatch(r"REMOVED_MOD_(\d+)_VERSION", token)
        if removed_ver_match:
            idx = int(removed_ver_match.group(1)) - 1
            return (
                removed_mod_entries[idx].get("from_display", removed_mod_entries[idx]["from"])
                if 0 <= idx < len(removed_mod_entries)
                else "None"
            )

        return "None"

    rendered = re.sub(r"\{\{([A-Z0-9_]+)\}\}", lambda m: token_value(m.group(1)), body)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(new:\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(vNone\s*->\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(renamed from None,\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(was\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"\n{3,}", "\n\n", rendered).strip()
    return rendered + "\n"


def get_mod_bases_for_dirs(mod_dirs: Tuple[str, ...]) -> set[str]:
    mod_bases: set[str] = set()
    for mod_dir in mod_dirs:
        for folder_name in scan_mod_folders(mod_dir):
            mod_bases.add(get_base_mod_name(folder_name))
    return mod_bases


def build_readme_metadata_index(
    csv_rows: List[Dict[str, str]],
    target_mod_bases: set[str],
    log: Logger,
) -> Dict[str, Dict[str, str]]:
    csv_map: Dict[str, Dict[str, str]] = {}
    for row in csv_rows:
        mod_name = row.get("MOD_NAME", "").strip()
        if mod_name:
            csv_map[mod_name] = dict(row)

    compat_data: Dict[str, Dict[str, str]] = {}
    missing_in_csv: List[str] = []
    for base_name in sorted(target_mod_bases):
        row = dict(csv_map.get(base_name, {}))
        if not row:
            missing_in_csv.append(base_name)

        row["MOD_NAME"] = base_name
        for field_name in README_COMPAT_FIELDS:
            if not row.get(field_name):
                if field_name == "QUOTE_FILE":
                    row[field_name] = f"{base_name}.txt"
                else:
                    row[field_name] = "MISSINGDATA"
        compat_data[base_name] = row

    if missing_in_csv:
        preview = ", ".join(missing_in_csv[:5])
        extra = "" if len(missing_in_csv) <= 5 else f" (+{len(missing_in_csv) - 5} more)"
        log.warn(
            "README metadata is missing in HELPER_ModCompatibility.csv for: "
            f"{preview}{extra}. Using MISSINGDATA defaults."
        )

    return compat_data


# =============================================================
# STEP 1 + 2: SCAN + SYNC
# =============================================================
def sync_workspace_and_game(dry_run: bool, log: Logger) -> List[Tuple[str, str]]:
    log.info("Step 1/2: Scan mod folders and sync by version")

    pub_folders = scan_mod_folders(PUBLISH_READY)
    inprog_folders = scan_mod_folders(IN_PROGRESS)
    game_folders = scan_mod_folders(GAME_MODS)

    log.stats.scanned_workspace_mods = len(pub_folders) + len(inprog_folders)
    log.stats.scanned_game_mods = len(game_folders)

    ws_by_base: Dict[str, Tuple[str, str]] = {}
    for folder, path in {**pub_folders, **inprog_folders}.items():
        ws_by_base[get_base_mod_name(folder)] = (folder, path)

    game_by_base: Dict[str, Tuple[str, str]] = {}
    for folder, path in game_folders.items():
        game_by_base[get_base_mod_name(folder)] = (folder, path)

    mods_pulled_from_game: List[Tuple[str, str]] = []

    for base_name, (ws_folder, ws_path) in ws_by_base.items():
        if base_name not in game_by_base:
            continue

        game_folder, game_path = game_by_base[base_name]
        if is_4modders_mod(ws_folder) or is_4modders_mod(game_folder):
            log.info(
                f"Skipping game-root auto-sync for optional 4Modders mod: {ws_folder} / {game_folder}"
            )
            continue

        ws_ver = get_modinfo_version(ws_path)
        game_ver = get_modinfo_version(game_path)

        if ws_ver is None or game_ver is None:
            log.warn(
                f"Skipping sync for {base_name}: missing/unreadable ModInfo.xml "
                f"(workspace={ws_ver}, game={game_ver})"
            )
            continue

        cmp_value = compare_versions(ws_ver, game_ver)

        if cmp_value < 0:
            log.info(
                f"Game is newer for {base_name}: game={game_folder} v{game_ver}, "
                f"workspace={ws_folder} v{ws_ver}. Pulling from game."
            )
            if maybe_copytree(game_path, ws_path, dry_run, log):
                mods_pulled_from_game.append((ws_folder, ws_path))
                log.stats.synced_pull_from_game += 1
                if maybe_remove_dir(game_path, dry_run, log):
                    log.info(f"Removed game mod after pull: {game_folder}")

        elif cmp_value > 0:
            new_game_dest = os.path.join(GAME_MODS, ws_folder)
            log.info(
                f"Workspace is newer for {base_name}: workspace={ws_folder} v{ws_ver}, "
                f"game={game_folder} v{game_ver}. Pushing to game."
            )
            if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(ws_path, new_game_dest, dry_run, log):
                log.stats.synced_push_to_game += 1

        else:
            try:
                ws_hash = hash_directory(ws_path)
                game_hash = hash_directory(game_path)
                if ws_hash != game_hash:
                    # Same version but different bytes: keep game folder aligned to workspace source-of-truth.
                    new_game_dest = os.path.join(GAME_MODS, ws_folder)
                    if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(ws_path, new_game_dest, dry_run, log):
                        log.stats.synced_push_to_game += 1
                        log.info(
                            f"Version tie resolved for {base_name}: both v{ws_ver} but content differed. "
                            "Pushed workspace copy to game."
                        )
                    else:
                        log.stats.sync_conflicts += 1
                        log.warn(
                            f"Version tie conflict for {base_name}: both v{ws_ver} but content differs. "
                            "Auto-push to game failed."
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare tied versions for {base_name}: {ex}")

    # Mirror 4Modders mods into game optionals folder in full mode as non-root installs.
    optionals_4modders_path = os.path.join(GAME_MODS, GAME_OPTIONALS_4MODDERS_DIR)
    if dry_run:
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_4modders_path}")
    else:
        os.makedirs(optionals_4modders_path, exist_ok=True)

    workspace_sources = {**pub_folders, **inprog_folders}
    for folder, ws_path in workspace_sources.items():
        if not is_4modders_mod(folder):
            continue
        if maybe_copytree(ws_path, os.path.join(optionals_4modders_path, folder), dry_run, log):
            log.info(f"sync mirror: 4Modders optional updated: {folder}")

    return mods_pulled_from_game


# =============================================================
# STEP 3: MOVE BY MAJOR VERSION
# =============================================================
def move_mods_by_major_version(dry_run: bool, log: Logger) -> None:
    log.info("Step 3: Move mods between In-Progress and PublishReady by major version")

    pub_folders = scan_mod_folders(PUBLISH_READY)
    inprog_folders = scan_mod_folders(IN_PROGRESS)
    all_mods = set(pub_folders) | set(inprog_folders)

    for folder_name in all_mods:
        pub_path = pub_folders.get(folder_name)
        inprog_path = inprog_folders.get(folder_name)
        mod_path = pub_path or inprog_path
        if not mod_path:
            continue

        version = get_modinfo_version(mod_path)
        if not version or "." not in version:
            log.warn(f"Skipping major-version move for {folder_name}: invalid version '{version}'")
            continue

        try:
            major = int(version.split(".", 1)[0])
        except ValueError:
            log.warn(f"Skipping major-version move for {folder_name}: non-numeric major '{version}'")
            continue

        if major == 0 and pub_path:
            dest = os.path.join(IN_PROGRESS, folder_name)
            if maybe_move(pub_path, dest, dry_run, log):
                log.stats.moved_to_in_progress += 1
                log.info(f"Moved to In-Progress: {folder_name} (v{version})")

        elif major >= 1 and inprog_path:
            dest = os.path.join(PUBLISH_READY, folder_name)
            if maybe_move(inprog_path, dest, dry_run, log):
                log.stats.moved_to_publish_ready += 1
                log.info(f"Moved to PublishReady: {folder_name} (v{version})")


def sync_publishready_to_staging_latest(dry_run: bool, log: Logger) -> None:
    """Ensure ActiveBuild has the latest released versions by base mod name.

    Rules:
    - If a release mod is missing in ActiveBuild, copy it in.
    - If release version is higher than ActiveBuild version, replace ActiveBuild copy.
    - If ActiveBuild is higher, keep ActiveBuild and warn.
    - If versions tie and content differs, keep ActiveBuild and warn.
    """
    log.info("Step 3.5: Ensure latest ReleaseSource mods are in ActiveBuild")

    publish_folders = scan_mod_folders(PUBLISH_READY)
    staging_folders = scan_mod_folders(STAGING)

    publish_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in publish_folders.items()
    }
    staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in staging_folders.items()
    }

    for base_name in sorted(publish_by_base.keys()):
        pub_folder, pub_path = publish_by_base[base_name]
        pub_ver = get_modinfo_version(pub_path)
        if pub_ver is None:
            log.warn(f"ActiveBuild sync skipped for {pub_folder}: unreadable release ModInfo.xml")
            continue

        if base_name not in staging_by_base:
            dest = os.path.join(STAGING, pub_folder)
            if maybe_copytree(pub_path, dest, dry_run, log):
                log.info(f"ActiveBuild sync add: {pub_folder} v{pub_ver}")
            continue

        st_folder, st_path = staging_by_base[base_name]
        st_ver = get_modinfo_version(st_path)
        if st_ver is None:
            log.warn(f"ActiveBuild sync skipped for {st_folder}: unreadable active ModInfo.xml")
            continue

        cmp_value = compare_versions(pub_ver, st_ver)
        if cmp_value > 0:
            dest = os.path.join(STAGING, pub_folder)
            if maybe_remove_dir(st_path, dry_run, log) and maybe_copytree(pub_path, dest, dry_run, log):
                log.info(f"ActiveBuild sync update: {st_folder} v{st_ver} -> {pub_folder} v{pub_ver}")
        elif cmp_value < 0:
            log.warn(
                f"ActiveBuild sync kept newer active mod for {base_name}: "
                f"active v{st_ver} > release v{pub_ver}"
            )
        else:
            try:
                pub_hash = hash_directory(pub_path)
                st_hash = hash_directory(st_path)
                if pub_hash != st_hash:
                    log.warn(
                        f"ActiveBuild sync conflict for {base_name}: both v{pub_ver} but content differs. "
                        "Keeping ActiveBuild copy."
                    )
            except Exception as ex:
                log.warn(f"Could not hash compare release/active tie for {base_name}: {ex}")


def sync_game_and_draft(dry_run: bool, log: Logger) -> None:
    """Sync updates from Game into Draft for AGF mods present in both lanes.

    This keeps Draft current when a mod was originally tracked in Draft but got
    updated while testing in the game Mods folder.
    """
    log.info("Step 0.25: Compare Game <-> Draft and pull newer game updates into Draft")

    draft_folders = scan_mod_folders(IN_PROGRESS)
    game_folders = scan_mod_folders(GAME_MODS)

    draft_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in draft_folders.items()
    }
    game_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in game_folders.items()
    }

    overlap = sorted(set(draft_by_base.keys()) & set(game_by_base.keys()))
    for base_name in overlap:
        draft_folder, draft_path = draft_by_base[base_name]
        game_folder, game_path = game_by_base[base_name]

        draft_ver = get_modinfo_version(draft_path)
        game_ver = get_modinfo_version(game_path)

        if draft_ver is None or game_ver is None:
            log.warn(
                f"Draft/game sync skipped for {base_name}: missing/unreadable ModInfo.xml "
                f"(draft={draft_ver}, game={game_ver})"
            )
            continue

        cmp_value = compare_versions(game_ver, draft_ver)
        if cmp_value > 0:
            if maybe_copytree(game_path, draft_path, dry_run, log):
                log.stats.synced_pull_from_game += 1
                log.info(
                    f"Draft/game pull: {game_folder} v{game_ver} -> {draft_folder} v{draft_ver}"
                )
        elif cmp_value == 0:
            try:
                draft_hash = hash_directory(draft_path)
                game_hash = hash_directory(game_path)
                if draft_hash != game_hash:
                    if maybe_copytree(game_path, draft_path, dry_run, log):
                        log.stats.synced_pull_from_game += 1
                        log.info(
                            f"Draft/game tie refresh for {base_name}: both v{game_ver} but content differed. "
                            "Pulled game copy into Draft."
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare game/draft tie for {base_name}: {ex}")


def enforce_staging_major_policy(dry_run: bool, log: Logger) -> None:
    """Ensure ActiveBuild only contains major-version >= 1 mods.

    Any ActiveBuild mod on major 0 is moved back to Draft. If Draft already has
    that base mod, keep whichever version is newer there and remove the other copy.
    """
    log.info("Lane policy: keep major v0.x mods in Draft, not ActiveBuild")

    staging_folders = scan_mod_folders(STAGING)
    draft_folders = scan_mod_folders(IN_PROGRESS)
    draft_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in draft_folders.items()
    }

    for st_folder, st_path in staging_folders.items():
        st_ver = get_modinfo_version(st_path)
        if st_ver is None:
            log.warn(f"Lane policy skipped for {st_folder}: unreadable active ModInfo.xml")
            continue

        try:
            st_major = int((st_ver or "0.0.0").split(".", 1)[0])
        except Exception:
            st_major = 0

        if st_major >= 1:
            continue

        base_name = get_base_mod_name(st_folder)
        draft_match = draft_by_base.get(base_name)

        if not draft_match:
            dest = os.path.join(IN_PROGRESS, st_folder)
            if maybe_move(st_path, dest, dry_run, log):
                log.info(f"Lane policy move: {st_folder} v{st_ver} moved ActiveBuild -> Draft")
            continue

        draft_folder, draft_path = draft_match
        draft_ver = get_modinfo_version(draft_path)
        if draft_ver is None:
            log.warn(f"Lane policy skipped merge for {st_folder}: unreadable draft ModInfo.xml")
            continue

        cmp_value = compare_versions(st_ver, draft_ver)
        if cmp_value > 0:
            replacement_dest = os.path.join(IN_PROGRESS, st_folder)
            if maybe_remove_dir(draft_path, dry_run, log) and maybe_move(st_path, replacement_dest, dry_run, log):
                log.info(
                    f"Lane policy replace: kept newer v0 draft copy from ActiveBuild "
                    f"({st_folder} v{st_ver} > {draft_folder} v{draft_ver})"
                )
        elif cmp_value < 0:
            if maybe_remove_dir(st_path, dry_run, log):
                log.info(
                    f"Lane policy cleanup: removed ActiveBuild v0 copy {st_folder} v{st_ver}; "
                    f"Draft already newer ({draft_folder} v{draft_ver})"
                )
        else:
            try:
                st_hash = hash_directory(st_path)
                draft_hash = hash_directory(draft_path)
                if st_hash != draft_hash:
                    replacement_dest = os.path.join(IN_PROGRESS, st_folder)
                    if maybe_remove_dir(draft_path, dry_run, log) and maybe_move(st_path, replacement_dest, dry_run, log):
                        log.info(
                            f"Lane policy tie refresh: replaced Draft copy with ActiveBuild copy for {base_name} "
                            f"at v{st_ver}, then removed ActiveBuild copy"
                        )
                else:
                    if maybe_remove_dir(st_path, dry_run, log):
                        log.info(
                            f"Lane policy cleanup: removed duplicate ActiveBuild v0 copy {st_folder} "
                            f"(matching Draft v{draft_ver})"
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare lane-policy tie for {base_name}: {ex}")


def sync_draft_to_staging_latest(dry_run: bool, log: Logger) -> None:
    """Ensure ActiveBuild contains the latest Draft copy for each base mod name.

    Rules:
    - If a draft mod is missing in ActiveBuild, copy it in.
    - If draft version is higher than ActiveBuild version, replace ActiveBuild copy.
    - If versions tie but content differs, replace ActiveBuild with Draft copy.
    - If ActiveBuild version is higher, keep ActiveBuild and warn.
    """
    log.info("Step 0.5: Ensure latest Draft mods are in ActiveBuild")

    draft_folders = scan_mod_folders(IN_PROGRESS)
    staging_folders = scan_mod_folders(STAGING)

    draft_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in draft_folders.items()
    }
    staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in staging_folders.items()
    }

    for base_name in sorted(draft_by_base.keys()):
        draft_folder, draft_path = draft_by_base[base_name]
        draft_ver = get_modinfo_version(draft_path)
        if draft_ver is None:
            log.warn(f"Draft sync skipped for {draft_folder}: unreadable draft ModInfo.xml")
            continue

        try:
            draft_major = int((draft_ver or "0.0.0").split(".", 1)[0])
        except Exception:
            draft_major = 0
        if draft_major < 1:
            log.info(
                f"Draft sync skipped for {draft_folder}: version {draft_ver} is draft-only (major < 1)"
            )
            continue

        remove_draft_after_sync = False

        if base_name not in staging_by_base:
            dest = os.path.join(STAGING, draft_folder)
            if maybe_copytree(draft_path, dest, dry_run, log):
                log.info(f"Draft sync add: {draft_folder} v{draft_ver}")
                remove_draft_after_sync = True
        else:
            st_folder, st_path = staging_by_base[base_name]
            st_ver = get_modinfo_version(st_path)
            if st_ver is None:
                log.warn(f"Draft sync skipped for {st_folder}: unreadable active ModInfo.xml")
                continue

            cmp_value = compare_versions(draft_ver, st_ver)
            if cmp_value > 0:
                dest = os.path.join(STAGING, draft_folder)
                if maybe_remove_dir(st_path, dry_run, log) and maybe_copytree(draft_path, dest, dry_run, log):
                    log.info(f"Draft sync update: {st_folder} v{st_ver} -> {draft_folder} v{draft_ver}")
                    remove_draft_after_sync = True
            elif cmp_value < 0:
                log.warn(
                    f"Draft sync kept newer ActiveBuild mod for {base_name}: "
                    f"active v{st_ver} > draft v{draft_ver}"
                )
                remove_draft_after_sync = True
            else:
                try:
                    draft_hash = hash_directory(draft_path)
                    st_hash = hash_directory(st_path)
                    if draft_hash != st_hash:
                        dest = os.path.join(STAGING, draft_folder)
                        if maybe_remove_dir(st_path, dry_run, log) and maybe_copytree(draft_path, dest, dry_run, log):
                            log.info(
                                f"Draft sync refresh: {st_folder} and {draft_folder} are both v{draft_ver} "
                                "but content differed. Replaced ActiveBuild with Draft copy."
                            )
                            remove_draft_after_sync = True
                    else:
                        remove_draft_after_sync = True
                except Exception as ex:
                    log.warn(f"Could not hash compare draft/active tie for {base_name}: {ex}")

        if remove_draft_after_sync and maybe_remove_dir(draft_path, dry_run, log):
            log.info(
                f"Draft promotion cleanup: removed {draft_folder} v{draft_ver} from Draft "
                "after syncing to ActiveBuild"
            )


# =============================================================
# STEP 4: RENAME + CSV + QUOTES + MOD README
# =============================================================
def rename_mod_folders_to_modinfo(
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
) -> List[Tuple[str, str, str]]:
    log.info("Rename folders to match ModInfo Name+Version when safe")

    folder_renames: List[Tuple[str, str, str]] = []
    target_dirs = mod_dirs if mod_dirs is not None else (PUBLISH_READY, IN_PROGRESS)
    for mod_dir in target_dirs:
        folders = scan_mod_folders(mod_dir)
        for folder_name, folder_path in folders.items():
            modinfo_path = os.path.join(folder_path, "ModInfo.xml")
            if not os.path.exists(modinfo_path):
                continue

            mod_name, mod_version = parse_modinfo(modinfo_path, folder_name)
            if not is_agf_mod(mod_name):
                log.warn(f"Rename skipped for {folder_name}: ModInfo Name does not start with AGF/zzzAGF")
                continue

            target_name = f"{mod_name}-v{mod_version}"
            if target_name == folder_name:
                continue

            src = folder_path
            dst = os.path.join(mod_dir, target_name)
            if os.path.exists(dst):
                log.warn(f"Rename skipped due to collision: {folder_name} -> {target_name}")
                continue

            if maybe_move(src, dst, dry_run, log):
                folder_renames.append((folder_name, target_name, mod_dir))
                log.stats.renamed_folders += 1
                log.info(f"Renamed folder: {folder_name} -> {target_name}")

    return folder_renames


def load_compat_csv() -> Tuple[List[str], List[Dict[str, str]]]:
    default_fields = ["MOD_NAME", "QUOTE_FILE"]
    if not os.path.exists(COMPAT_CSV):
        return default_fields, []

    def normalize_header_name(header: str) -> str:
        return header.lstrip("\ufeff").strip().strip('"')

    with open(COMPAT_CSV, "r", encoding="utf-8", newline="") as f:
        reader = csv.DictReader(f)
        raw_fieldnames = list(reader.fieldnames) if reader.fieldnames else default_fields
        fieldnames = [normalize_header_name(fn) for fn in raw_fieldnames]

        rows: List[Dict[str, str]] = []
        for raw_row in reader:
            clean_row: Dict[str, str] = {}
            for key, value in raw_row.items():
                if key is None:
                    continue
                clean_key = normalize_header_name(key)
                clean_row[clean_key] = value
            rows.append(clean_row)

    return fieldnames, rows


def save_compat_csv(fieldnames: List[str], rows: List[Dict[str, str]], dry_run: bool, log: Logger) -> None:
    def row_has_missingdata(row: Dict[str, str]) -> bool:
        for fn in fieldnames:
            value = str(row.get(fn, ""))
            if "missingdata" in value.lower():
                return True
        return False

    rows.sort(key=lambda r: (0 if row_has_missingdata(r) else 1, r.get("MOD_NAME", "").lower()))
    if dry_run:
        log.info(f"[DRYRUN] Would write compatibility CSV: {COMPAT_CSV} ({len(rows)} rows)")
        return

    with open(COMPAT_CSV, "w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def normalize_compat_csv(
    folder_renames: List[Tuple[str, str, str]],
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
    prune_to_mods_now: bool = True,
) -> List[Dict[str, str]]:
    log.info("Step 4.2: Normalize HELPER_ModCompatibility.csv")

    fieldnames, rows = load_compat_csv()
    if "MOD_NAME" not in fieldnames:
        fieldnames.insert(0, "MOD_NAME")
    if "QUOTE_FILE" not in fieldnames:
        fieldnames.append("QUOTE_FILE")

    rename_base_map = {
        get_base_mod_name(old): get_base_mod_name(new)
        for old, new, _ in folder_renames
    }

    scan_dirs = mod_dirs if mod_dirs is not None else (PUBLISH_READY, IN_PROGRESS)
    mods_now: set[str] = set()
    for mod_dir in scan_dirs:
        for folder_name in scan_mod_folders(mod_dir):
            mods_now.add(get_base_mod_name(folder_name))

    for row in rows:
        old = row.get("MOD_NAME", "")
        if old in rename_base_map:
            row["MOD_NAME"] = rename_base_map[old]

    removed = 0
    if prune_to_mods_now:
        before = len(rows)
        rows = [row for row in rows if row.get("MOD_NAME") in mods_now]
        removed = before - len(rows)
        log.stats.csv_removed_rows += removed

    existing = {row.get("MOD_NAME") for row in rows}
    for mod in sorted(mods_now):
        if mod not in existing:
            new_row = {fn: "MISSINGDATA" for fn in fieldnames}
            new_row["MOD_NAME"] = mod
            new_row["QUOTE_FILE"] = f"{mod}.txt"
            rows.append(new_row)
            log.stats.csv_added_rows += 1

    for row in rows:
        row["QUOTE_FILE"] = f"{row.get('MOD_NAME', 'MISSINGDATA')}.txt"
        for fn in fieldnames:
            if not row.get(fn):
                row[fn] = "MISSINGDATA"

    save_compat_csv(fieldnames, rows, dry_run, log)
    return rows


def normalize_quote_files(csv_rows: List[Dict[str, str]], folder_renames: List[Tuple[str, str, str]], dry_run: bool, log: Logger) -> None:
    log.info("Step 4.3: Normalize quote files")

    if dry_run:
        log.info(f"[DRYRUN] Would ensure quote directory exists: {QUOTES_DIR}")
    else:
        os.makedirs(QUOTES_DIR, exist_ok=True)

    for old_name, new_name, _ in folder_renames:
        old_base = get_base_mod_name(old_name)
        new_base = get_base_mod_name(new_name)
        old_quote = os.path.join(QUOTES_DIR, f"{old_base}.txt")
        new_quote = os.path.join(QUOTES_DIR, f"{new_base}.txt")
        if os.path.exists(old_quote) and not os.path.exists(new_quote):
            if dry_run:
                log.info(f"[DRYRUN] Would rename quote file: {old_quote} -> {new_quote}")
            else:
                os.rename(old_quote, new_quote)
            log.stats.quote_files_renamed += 1

    for row in csv_rows:
        quote_file = row.get("QUOTE_FILE", "")
        if not quote_file:
            continue
        quote_path = os.path.join(QUOTES_DIR, quote_file)

        if not os.path.exists(quote_path):
            if dry_run:
                log.info(f"[DRYRUN] Would create quote file: {quote_path}")
            else:
                with open(quote_path, "w", encoding="utf-8") as f:
                    f.write("")
            log.stats.quote_files_created += 1
            continue

        try:
            with open(quote_path, "r", encoding="utf-8") as f:
                content = f.read()
            normalized = content.strip().lower()
            if normalized in {"none", "missingdata"}:
                if dry_run:
                    log.info(f"[DRYRUN] Would blank quote file containing placeholder text: {quote_path}")
                else:
                    with open(quote_path, "w", encoding="utf-8") as f:
                        f.write("")
                log.stats.quote_files_blanked_none += 1
        except Exception as ex:
            log.warn(f"Failed reading/normalizing quote file {quote_path}: {ex}")


def generate_mod_readmes(
    csv_rows: List[Dict[str, str]],
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
) -> None:
    log.info("Generate per-mod README.md and ReadableReadMe.txt")

    if not os.path.exists(MOD_README_TEMPLATE):
        log.error(f"Missing required template: {MOD_README_TEMPLATE}")
        raise FileNotFoundError(MOD_README_TEMPLATE)

    with open(MOD_README_TEMPLATE, "r", encoding="utf-8") as f:
        template = f.read()

    target_dirs = mod_dirs if mod_dirs is not None else (PUBLISH_READY, IN_PROGRESS)
    target_mod_bases = get_mod_bases_for_dirs(target_dirs)
    compat_data = build_readme_metadata_index(csv_rows, target_mod_bases, log)

    for mod_dir in target_dirs:
        folders = scan_mod_folders(mod_dir)
        for folder_name, mod_path in folders.items():
            modinfo_path = os.path.join(mod_path, "ModInfo.xml")
            if not os.path.exists(modinfo_path):
                log.warn(f"Skipping README for {folder_name}: missing ModInfo.xml")
                continue

            mod_name, mod_version = parse_modinfo(modinfo_path, folder_name)
            mod_display_name = get_modinfo_display_name(modinfo_path, mod_name)
            mod_version_display = format_version_for_display(mod_version, mod_display_name)
            base_name = get_base_mod_name(folder_name)
            zip_name = f"{base_name}.zip"
            download_link = zip_download_link(zip_name)

            compat = compat_data.get(base_name, {})
            eac_friendly = compat.get("EAC_FRIENDLY", "MISSINGDATA")
            server_side = compat.get("SERVER_SIDE", "MISSINGDATA")
            client_required = compat.get("CLIENT_REQUIRED", "MISSINGDATA")
            safe_to_install = compat.get("SAFE_TO_INSTALL", "MISSINGDATA")
            safe_to_remove = compat.get("SAFE_TO_REMOVE", "MISSINGDATA")
            unique = compat.get("UNIQUE", "MISSINGDATA")

            quote_file_name = compat.get("QUOTE_FILE", f"{base_name}.txt")
            quote_file_path = os.path.join(QUOTES_DIR, quote_file_name)
            fallback_quote_path = os.path.join(QUOTES_DIR, f"{base_name}.txt")
            if not os.path.exists(quote_file_path) and quote_file_name != f"{base_name}.txt" and os.path.exists(fallback_quote_path):
                quote_file_path = fallback_quote_path

            quote_md = ""
            if os.path.exists(quote_file_path):
                try:
                    with open(quote_file_path, "r", encoding="utf-8") as f:
                        quote_text = f.read().strip()
                    if quote_text:
                        quote_md = format_blockquote(quote_text)
                except Exception as ex:
                    log.warn(f"Failed reading quote for {folder_name}: {ex}")

            readme_content = template
            readme_content = readme_content.replace("{{MOD_NAME}}", mod_name)
            readme_content = readme_content.replace("{{MOD_VERSION}}", mod_version_display)
            readme_content = readme_content.replace("{{DOWNLOAD_LINK}}", download_link)
            readme_content = readme_content.replace("{{QUOTE}}", quote_md)
            readme_content = readme_content.replace("{{EAC_FRIENDLY}}", eac_friendly)
            readme_content = readme_content.replace("{{SERVER_SIDE}}", server_side)
            readme_content = readme_content.replace("{{CLIENT_REQUIRED}}", client_required)
            readme_content = readme_content.replace("{{SAFE_TO_INSTALL}}", safe_to_install)
            readme_content = readme_content.replace("{{SAFE_TO_REMOVE}}", safe_to_remove)
            readme_content = readme_content.replace("{{UNIQUE}}", unique)

            readme_path = os.path.join(mod_path, "README.md")
            features_block = extract_readme_block(readme_path, "<!-- FEATURES START -->", "<!-- FEATURES END -->")
            changelog_block = extract_readme_block(readme_path, "<!-- CHANGELOG START -->", "<!-- CHANGELOG END -->")

            features_block = sanitize_preserved_readme_block(features_block)
            changelog_block = sanitize_preserved_readme_block(changelog_block)

            if features_block:
                readme_content = re.sub(
                    r"(<!-- FEATURES START -->)([\s\S]*?)(<!-- FEATURES END -->)",
                    r"\1" + features_block + r"\3",
                    readme_content,
                    flags=re.MULTILINE,
                )
            if changelog_block:
                readme_content = re.sub(
                    r"(<!-- CHANGELOG START -->)([\s\S]*?)(<!-- CHANGELOG END -->)",
                    r"\1" + changelog_block + r"\3",
                    readme_content,
                    flags=re.MULTILINE,
                )

            txt_path = os.path.join(mod_path, "ReadableReadMe.txt")
            txt_content = markdown_to_text(readme_content)

            if dry_run:
                log.info(f"[DRYRUN] Would write README + ReadableReadMe for {folder_name}")
            else:
                try:
                    with open(readme_path, "w", encoding="utf-8") as f:
                        f.write(readme_content)
                    with open(txt_path, "w", encoding="utf-8") as f:
                        f.write(txt_content)
                except Exception as ex:
                    log.warn(f"Failed writing README files for {folder_name}: {ex}")
                    continue

            log.stats.readmes_written += 1
            log.stats.readable_txt_written += 1


def prep_names_and_readmes_for_dirs(mod_dirs: Tuple[str, ...], dry_run: bool, log: Logger) -> None:
    """Refresh folder naming and README files for specific mod directories."""
    log.info("Pre-release prep: refresh folder names + README files")

    folder_renames = rename_mod_folders_to_modinfo(dry_run, log, mod_dirs=mod_dirs)
    csv_rows = normalize_compat_csv(
        folder_renames,
        dry_run,
        log,
        mod_dirs=mod_dirs,
        prune_to_mods_now=False,
    )
    if not csv_rows:
        log.warn("Compatibility CSV has no rows; README generation will use fallback values.")

    for row in csv_rows:
        mod_name = row.get("MOD_NAME", "")
        if not row.get("QUOTE_FILE") and mod_name:
            row["QUOTE_FILE"] = f"{mod_name}.txt"

    normalize_quote_files(csv_rows, folder_renames, dry_run, log)
    generate_mod_readmes(csv_rows, dry_run, log, mod_dirs=mod_dirs)


# =============================================================
# STEP 5: PUSH BACK PULLED MODS
# =============================================================
def push_back_pulled_mods(mods_pulled_from_game: List[Tuple[str, str]], dry_run: bool, log: Logger) -> None:
    log.info("Step 5: Push updated pulled mods back to game folder")

    workspace_by_base: Dict[str, str] = {}
    for folder, path in scan_mod_folders(PUBLISH_READY).items():
        workspace_by_base[get_base_mod_name(folder)] = path
    for folder, path in scan_mod_folders(IN_PROGRESS).items():
        workspace_by_base[get_base_mod_name(folder)] = path

    for mod_name, ws_path in mods_pulled_from_game:
        resolved_ws_path = ws_path
        if not os.path.exists(resolved_ws_path):
            base_name = get_base_mod_name(mod_name)
            resolved_ws_path = workspace_by_base.get(base_name, "")

        if (not resolved_ws_path or not os.path.exists(resolved_ws_path)) and not dry_run:
            log.warn(f"Pushback skipped for {mod_name}: workspace path missing {ws_path}")
            continue

        dest_path = os.path.join(GAME_MODS, os.path.basename(resolved_ws_path))
        if maybe_remove_dir(dest_path, dry_run, log) and maybe_copytree(resolved_ws_path, dest_path, dry_run, log):
            log.stats.pushed_back_to_game += 1
            log.info(f"Pushback complete: {mod_name}")


def sync_staging_and_game(dry_run: bool, log: Logger) -> None:
    """Sync only between staging and game lanes.

    This mode is for day-to-day test syncs and does not touch PublishReady.
    """
    log.info("Mode sync-work: Sync Staging <-> Game by version")
    log.info(
        "Policy: keep one active BackpackPlus in game root, keep all BackpackPlus in .Optionals-Backpack, "
        "mirror all HUDPlus/HUDPluszOther in .Optionals-HUDPlus and keep HUDPluszOther out of game root, "
        "and mirror AGF-4Modders into .Optionals-4Modders without auto-pushing them into game root."
    )

    staging_folders = scan_mod_folders(STAGING)
    game_folders = scan_mod_folders(GAME_MODS)
    renamed_suffixes: set[str] = set()
    for folder_name in staging_folders:
        match = re.match(r"^AGF-4Modders-(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if match:
            renamed_suffixes.add(match.group(1))

    log.stats.scanned_workspace_mods = len(staging_folders)
    log.stats.scanned_game_mods = len(game_folders)

    backpack_mods = sorted([f for f in staging_folders if is_backpack_mod(f)])
    active_backpack = next((f for f in backpack_mods if BACKPACK_DEFAULT_ACTIVE_TOKEN in f), None)
    if active_backpack is None and backpack_mods:
        active_backpack = backpack_mods[0]
        log.warn(
            f"Default backpack token '{BACKPACK_DEFAULT_ACTIVE_TOKEN}' not found. "
            f"Using '{active_backpack}' as active backpack."
        )

    # Keep game root clean for root-policy-only mods.
    for game_folder, game_path in game_folders.items():
        if is_backpack_mod(game_folder) and active_backpack and game_folder != active_backpack:
            if maybe_remove_dir(game_path, dry_run, log):
                log.info(f"sync-work cleanup: removed non-active backpack from game root: {game_folder}")
            continue
        if is_hudpluszother_mod(game_folder):
            if maybe_remove_dir(game_path, dry_run, log):
                log.info(f"sync-work cleanup: removed HUDPluszOther optional from game root: {game_folder}")

    cleanup_legacy_4modders_replacements_with_suffixes(
        GAME_MODS,
        renamed_suffixes,
        dry_run,
        log,
        "game root",
    )

    # Re-scan after cleanup decisions for consistent sync maps.
    game_folders = scan_mod_folders(GAME_MODS)

    # Sync allowed game-root mods by version.
    allowed_staging_root: Dict[str, str] = {}
    for folder, path in staging_folders.items():
        if is_backpack_mod(folder) and active_backpack and folder != active_backpack:
            continue
        if is_hudpluszother_mod(folder):
            continue
        if is_4modders_mod(folder):
            continue
        allowed_staging_root[folder] = path

    staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in allowed_staging_root.items()
    }
    game_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in game_folders.items()
    }

    overlap = sorted(set(staging_by_base.keys()) & set(game_by_base.keys()))
    for base_name in overlap:
        st_folder, st_path = staging_by_base[base_name]
        game_folder, game_path = game_by_base[base_name]
        st_ver = get_modinfo_version(st_path)
        game_ver = get_modinfo_version(game_path)

        if st_ver is None:
            log.warn(
                f"Skipping sync-work for {base_name}: missing/unreadable ModInfo.xml "
                f"(staging={st_ver}, game={game_ver})"
            )
            continue

        if game_ver is None:
            target = os.path.join(GAME_MODS, st_folder)
            if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(st_path, target, dry_run, log):
                log.stats.synced_push_to_game += 1
                log.info(
                    f"sync-work repair: replaced malformed game copy for {base_name} "
                    f"with staging v{st_ver}"
                )
            continue

        cmp_value = compare_versions(st_ver, game_ver)
        if cmp_value > 0:
            target = os.path.join(GAME_MODS, st_folder)
            if maybe_copytree(st_path, target, dry_run, log):
                log.stats.synced_push_to_game += 1
                log.info(f"sync-work push: {st_folder} v{st_ver} -> game")
        elif cmp_value < 0:
            target = os.path.join(STAGING, game_folder)
            if maybe_copytree(game_path, target, dry_run, log):
                log.stats.synced_pull_from_game += 1
                log.info(f"sync-work pull: {game_folder} v{game_ver} -> staging")
        else:
            try:
                st_hash = hash_directory(st_path)
                game_hash = hash_directory(game_path)
                if st_hash != game_hash:
                    # In sync-work mode, staging is authoritative for game copies on tied versions.
                    target = os.path.join(GAME_MODS, st_folder)
                    if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(st_path, target, dry_run, log):
                        log.stats.synced_push_to_game += 1
                        log.info(
                            f"sync-work tie resolved for {base_name}: both v{st_ver} but content differed. "
                            "Pushed staging copy to game."
                        )
                    else:
                        log.stats.sync_conflicts += 1
                        log.warn(
                            f"sync-work conflict for {base_name}: both v{st_ver} but content differs. "
                            "Auto-push to game failed."
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare tied versions for {base_name}: {ex}")

    # Ensure active backpack exists in game root even if it did not overlap.
    if active_backpack:
        active_staging_path = staging_folders.get(active_backpack)
        active_game_path = os.path.join(GAME_MODS, active_backpack)
        if active_staging_path and active_backpack not in game_folders:
            if maybe_copytree(active_staging_path, active_game_path, dry_run, log):
                log.stats.synced_push_to_game += 1
                log.info(f"sync-work push: ensured active backpack in game root: {active_backpack}")

    # Push all other allowed staging mods that are missing from game root by base name.
    missing_in_game = sorted(set(staging_by_base.keys()) - set(game_by_base.keys()))
    for base_name in missing_in_game:
        st_folder, st_path = staging_by_base[base_name]
        st_ver = get_modinfo_version(st_path)
        if st_ver is None:
            log.warn(f"sync-work missing push skipped for {base_name}: staging ModInfo.xml unreadable")
            continue
        target = os.path.join(GAME_MODS, st_folder)
        if maybe_copytree(st_path, target, dry_run, log):
            log.stats.synced_push_to_game += 1
            log.info(f"sync-work push: added missing game mod {st_folder} v{st_ver}")

    # Mirror optionals folders in game space.
    optionals_backpack_path = os.path.join(GAME_MODS, GAME_OPTIONALS_BACKPACK_DIR)
    optionals_hudplus_path = os.path.join(GAME_MODS, GAME_OPTIONALS_HUDPLUS_DIR)
    optionals_4modders_path = os.path.join(GAME_MODS, GAME_OPTIONALS_4MODDERS_DIR)
    if dry_run:
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_backpack_path}")
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_hudplus_path}")
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_4modders_path}")
    else:
        os.makedirs(optionals_backpack_path, exist_ok=True)
        os.makedirs(optionals_hudplus_path, exist_ok=True)
        os.makedirs(optionals_4modders_path, exist_ok=True)

    cleanup_legacy_4modders_replacements_with_suffixes(
        optionals_hudplus_path,
        renamed_suffixes,
        dry_run,
        log,
        GAME_OPTIONALS_HUDPLUS_DIR,
    )

    for folder, st_path in staging_folders.items():
        if is_backpack_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_backpack_path, folder), dry_run, log):
                log.info(f"sync-work mirror: backpack optional updated: {folder}")
            continue
        if is_hudplus_mod(folder) or is_hudpluszother_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_hudplus_path, folder), dry_run, log):
                log.info(f"sync-work mirror: HUD optional updated: {folder}")
            continue
        if is_4modders_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_4modders_path, folder), dry_run, log):
                log.info(f"sync-work mirror: 4Modders optional updated: {folder}")

    # Normalize active-build folder names after pulls so names track ModInfo versions.
    rename_mod_folders_to_modinfo(dry_run, log, mod_dirs=(STAGING,))


def promote_staging_to_publish_ready(dry_run: bool, log: Logger) -> None:
    """Promote staging mods to publish-ready only when version is higher or missing in publish-ready."""
    log.info("Mode promote: Promote Staging -> PublishReady (version-gated)")

    staging_folders = scan_mod_folders(STAGING)
    publish_folders = scan_mod_folders(PUBLISH_READY)

    publish_by_base: Dict[str, Tuple[str, str]] = {}
    for folder, path in publish_folders.items():
        publish_by_base[get_base_mod_name(folder)] = (folder, path)

    for st_folder, st_path in staging_folders.items():
        base_name = get_base_mod_name(st_folder)
        st_ver = get_modinfo_version(st_path)

        if st_ver is None:
            log.warn(f"Promote skipped for {st_folder}: missing/unreadable ModInfo.xml")
            continue

        try:
            st_major = int((st_ver or "0.0.0").split(".", 1)[0])
        except Exception:
            st_major = 0
        if st_major < 1:
            log.info(
                f"Promote skipped for {st_folder}: staging version {st_ver} is draft-only (major < 1)"
            )
            continue

        if base_name not in publish_by_base:
            legacy_replacement = re.match(
                r"^(AGF-NoEAC-|AGF-HUDPluszOther-)(.+)-v([0-9][0-9a-zA-Z\.-]*)$",
                st_folder,
            )
            if legacy_replacement:
                suffix = legacy_replacement.group(2)
                version = legacy_replacement.group(3)
                old_4modders_name = f"AGF-4Modders-{suffix}-v{version}"
                old_4modders_path = publish_folders.get(old_4modders_name)
                if old_4modders_path and maybe_remove_dir(old_4modders_path, dry_run, log):
                    log.info(
                        "Promote rename replacement: removed stale 4Modders release folder "
                        f"{old_4modders_name} before promoting {st_folder}"
                    )

            dest = os.path.join(PUBLISH_READY, st_folder)
            if maybe_copytree(st_path, dest, dry_run, log):
                log.stats.promoted_to_publish_ready += 1
                log.info(f"Promoted new mod: {st_folder} v{st_ver}")
            continue

        pub_folder, pub_path = publish_by_base[base_name]
        pub_ver = get_modinfo_version(pub_path)
        if pub_ver is None:
            log.warn(f"Promote skipped for {st_folder}: publish copy has unreadable ModInfo.xml")
            continue

        cmp_value = compare_versions(st_ver, pub_ver)
        if cmp_value > 0:
            dest = os.path.join(PUBLISH_READY, st_folder)
            if pub_folder != st_folder:
                if not maybe_remove_dir(pub_path, dry_run, log):
                    log.warn(f"Promote skipped for {st_folder}: could not remove old publish folder {pub_folder}")
                    continue
            if maybe_copytree(st_path, dest, dry_run, log):
                log.stats.promoted_to_publish_ready += 1
                log.info(f"Promoted update: {st_folder} v{st_ver} (was {pub_folder} v{pub_ver})")
        elif cmp_value == 0:
            try:
                st_hash = hash_directory(st_path)
                pub_hash = hash_directory(pub_path)
                if st_hash == pub_hash:
                    log.info(f"Promote skipped for {st_folder}: same version/content as publish-ready ({st_ver})")
                else:
                    dest = os.path.join(PUBLISH_READY, st_folder)
                    if pub_folder != st_folder:
                        if not maybe_remove_dir(pub_path, dry_run, log):
                            log.warn(
                                f"Promote skipped for {st_folder}: could not remove old publish folder {pub_folder}"
                            )
                            continue
                    if maybe_copytree(st_path, dest, dry_run, log):
                        log.stats.promoted_to_publish_ready += 1
                        log.info(
                            f"Promoted refresh: {st_folder} v{st_ver} (same version, content/name changed from {pub_folder})"
                        )
            except Exception as ex:
                log.warn(f"Promote hash compare failed for {st_folder}: {ex}")
        else:
            log.warn(
                f"Promote skipped for {st_folder}: staging version {st_ver} is lower than publish-ready {pub_ver}"
            )


def cleanup_release_legacy_4modders_renames(dry_run: bool, log: Logger) -> None:
    """Remove legacy ReleaseSource folders that were renamed into AGF-4Modders equivalents."""
    log.info("Cleanup: Remove legacy ReleaseSource folders replaced by AGF-4Modders renames")

    publish_folders = scan_mod_folders(PUBLISH_READY)
    staging_folders = scan_mod_folders(STAGING)
    for folder_name in sorted(publish_folders.keys()):
        if not folder_name.startswith("AGF-4Modders-"):
            continue

        match = re.match(r"^AGF-4Modders-(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if not match:
            continue

        suffix = match.group(1)
        version = match.group(2)
        for legacy_prefix in ("AGF-NoEAC-", "AGF-HUDPluszOther-"):
            legacy_folder = f"{legacy_prefix}{suffix}-v{version}"
            legacy_path = os.path.join(PUBLISH_READY, legacy_folder)
            if os.path.isdir(legacy_path):
                if legacy_folder in staging_folders:
                    # ActiveBuild is the source-of-truth. Keep the legacy-named release folder
                    # when that naming still exists in ActiveBuild.
                    log.info(
                        "Skipped legacy release cleanup because ActiveBuild still has: "
                        f"{legacy_folder}"
                    )
                    continue
                if maybe_remove_dir(legacy_path, dry_run, log):
                    log.info(
                        f"Removed legacy release folder after 4Modders rename: {legacy_folder} -> {folder_name}"
                    )


def cleanup_legacy_4modders_renames_in_dir(base_dir: str, dry_run: bool, log: Logger) -> None:
    """Remove legacy NoEAC/HUDPluszOther folders that were replaced by AGF-4Modders in a lane."""
    lane_name = os.path.basename(base_dir.rstrip("\\/"))
    log.info(f"Cleanup: Remove legacy 4Modders-replaced folders in {lane_name}")

    folders = scan_mod_folders(base_dir)
    for folder_name in sorted(folders.keys()):
        if not folder_name.startswith("AGF-4Modders-"):
            continue

        match = re.match(r"^AGF-4Modders-(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if not match:
            continue

        suffix = match.group(1)
        for legacy_prefix in ("AGF-NoEAC-", "AGF-HUDPluszOther-"):
            legacy_pattern = re.compile(rf"^{re.escape(legacy_prefix + suffix)}-v.+$")
            for candidate_name, candidate_path in list(folders.items()):
                if candidate_name == folder_name:
                    continue
                if not legacy_pattern.match(candidate_name):
                    continue
                if maybe_remove_dir(candidate_path, dry_run, log):
                    log.info(f"Removed legacy folder in {lane_name}: {candidate_name} -> replaced by {folder_name}")


def cleanup_legacy_4modders_replacements_with_suffixes(
    base_dir: str,
    renamed_suffixes: set[str],
    dry_run: bool,
    log: Logger,
    lane_label: str,
) -> None:
    """Remove legacy NoEAC/HUDPluszOther folders whose suffix now exists as AGF-4Modders."""
    if not os.path.isdir(base_dir) or not renamed_suffixes:
        return

    folders = scan_mod_folders(base_dir)
    for folder_name, folder_path in list(folders.items()):
        match = re.match(r"^(AGF-NoEAC-|AGF-HUDPluszOther-)(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if not match:
            continue

        suffix = match.group(2)
        if suffix not in renamed_suffixes:
            continue

        replacement_name = f"AGF-4Modders-{suffix}"
        if maybe_remove_dir(folder_path, dry_run, log):
            log.info(f"Removed legacy folder in {lane_label}: {folder_name} -> replaced by {replacement_name}")


def cleanup_older_versions_in_dir(base_dir: str, dry_run: bool, log: Logger) -> None:
    """Keep only the newest folder per base mod name in a lane."""
    lane_name = os.path.basename(base_dir.rstrip("\\/"))
    log.info(f"Cleanup: Remove older duplicate versions in {lane_name}")

    folders = scan_mod_folders(base_dir)
    by_base: Dict[str, List[Tuple[str, str, str]]] = {}
    for folder_name, folder_path in folders.items():
        base_name = get_base_mod_name(folder_name)
        version = get_modinfo_version(folder_path)
        if not version:
            version_match = re.search(r"-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
            version = version_match.group(1) if version_match else "0.0.0"
        by_base.setdefault(base_name, []).append((folder_name, folder_path, version))

    for base_name, entries in by_base.items():
        if len(entries) <= 1:
            continue

        sorted_entries = sorted(entries, key=lambda x: (tuple(int(p) for p in re.findall(r"\d+", x[2]) or [0]), x[0]))
        keep_name, keep_path, keep_ver = sorted_entries[-1]
        for folder_name, folder_path, version in sorted_entries[:-1]:
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    f"Removed older duplicate in {lane_name}: {folder_name} v{version} "
                    f"(kept {keep_name} v{keep_ver})"
                )


# =============================================================
# STEP 6: ZIP MODS + PACKS
# =============================================================
def collect_publishready_folders() -> List[str]:
    return sorted(scan_mod_folders(PUBLISH_READY).keys())


def zip_mod_folder(mod_folder: str, dry_run: bool, log: Logger) -> Tuple[str, bool]:
    mod_path = os.path.join(PUBLISH_READY, mod_folder)
    zip_name = f"{get_base_mod_name(mod_folder)}.zip"
    zip_path = os.path.join(ZIP_OUTPUT, zip_name)

    if dry_run:
        log.info(f"[DRYRUN] Would create mod zip: {zip_name}")
        return zip_name, True

    try:
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zipf:
            for root, _, files in os.walk(mod_path):
                for file in files:
                    file_path = os.path.join(root, file)
                    arcname = os.path.join(mod_folder, os.path.relpath(file_path, mod_path))
                    zipf.write(file_path, arcname)
        return zip_name, True
    except Exception as ex:
        log.error(f"Failed creating mod zip {zip_name}: {ex}")
        return zip_name, False


def zip_category(pack_name: str, root_mods: List[str], optionals_map: Optional[Dict[str, List[str]]], dry_run: bool, log: Logger) -> bool:
    zip_path = os.path.join(ZIP_OUTPUT, f"{pack_name}.zip")
    if dry_run:
        log.info(f"[DRYRUN] Would create pack zip: {pack_name}.zip")
        return True

    try:
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zipf:
            for mod_folder in root_mods:
                mod_path = os.path.join(PUBLISH_READY, mod_folder)
                if not os.path.isdir(mod_path):
                    continue
                for root, _, files in os.walk(mod_path):
                    for file in files:
                        file_path = os.path.join(root, file)
                        arcname = os.path.join(mod_folder, os.path.relpath(file_path, mod_path))
                        zipf.write(file_path, arcname)

            if optionals_map:
                for opt_folder, opt_mods in optionals_map.items():
                    for mod_folder in opt_mods:
                        mod_path = os.path.join(PUBLISH_READY, mod_folder)
                        if not os.path.isdir(mod_path):
                            continue
                        for root, _, files in os.walk(mod_path):
                            for file in files:
                                file_path = os.path.join(root, file)
                                arcname = os.path.join(opt_folder, mod_folder, os.path.relpath(file_path, mod_path))
                                zipf.write(file_path, arcname)
        return True
    except Exception as ex:
        log.error(f"Failed creating pack zip {pack_name}.zip: {ex}")
        return False


def build_pack_definitions(all_folders: List[str]) -> List[Tuple[str, List[str], Optional[Dict[str, List[str]]]]]:
    backpackplus_mods = [f for f in all_folders if f.startswith("AGF-BackpackPlus-")]
    hudplus_mods = [f for f in all_folders if f.startswith("AGF-HUDPlus-")]
    hudpluszother_mods = [f for f in all_folders if f.startswith("AGF-HUDPluszOther-")]
    noeac_mods = [f for f in all_folders if f.startswith("AGF-NoEAC-")]
    modders_mods = [f for f in all_folders if f.startswith("AGF-4Modders-")]
    vp_mods = [f for f in all_folders if f.startswith("AGF-VP-")]
    special_mods = [f for f in all_folders if f.startswith("zzzAGF-Special")]

    backpackplus_84 = next((f for f in backpackplus_mods if "84Slots" in f), None)

    packs: List[Tuple[str, List[str], Optional[Dict[str, List[str]]]]] = []
    packs.append(("00_BackpackPlus_All", backpackplus_mods, None))

    giggle_root = hudplus_mods + vp_mods + special_mods + ([backpackplus_84] if backpackplus_84 else [])
    giggle_optionals = {
        ".Optionals-BackpackPlus": backpackplus_mods,
        ".Optionals-HUDPlus": hudplus_mods + hudpluszother_mods,
        ".Optionals-NoEAC": noeac_mods,
        ".Optionals-4Modders": modders_mods,
    }
    packs.append(("00_GigglePack_All", giggle_root, giggle_optionals))

    hudplus_all_root = hudplus_mods + special_mods
    hudplus_all_optionals = {
        ".Optionals-NoEAC": noeac_mods,
        ".Optionals-HUDPluszOther": hudpluszother_mods,
    }
    packs.append(("00_HUDPlus_All", hudplus_all_root, hudplus_all_optionals))
    packs.append(("00_HUDPluszOther_All", hudpluszother_mods, None))
    packs.append(("00_NoEAC_All", noeac_mods, None))
    packs.append(("00_4Modders_All", modders_mods, None))

    vp_all_root = vp_mods + special_mods
    vp_all_optionals = {".Optionals-NoEAC": noeac_mods}
    packs.append(("00_VP_All", vp_all_root, vp_all_optionals))

    return packs


def gather_mod_versions_by_base() -> Dict[str, str]:
    versions: Dict[str, str] = {}
    for folder_name, folder_path in scan_mod_folders(PUBLISH_READY).items():
        base_name = get_base_mod_name(folder_name)
        version = get_modinfo_version(folder_path) or "0.0.0"
        versions[base_name] = version
    return versions


def bump_patch_version(version: str) -> str:
    numbers = [int(p) for p in re.findall(r"\d+", version)]
    while len(numbers) < 3:
        numbers.append(0)
    numbers[2] += 1
    return f"{numbers[0]}.{numbers[1]}.{numbers[2]}"


def parse_three_part_version(version: str) -> Tuple[int, int, int]:
    numbers = [int(p) for p in re.findall(r"\d+", version)]
    while len(numbers) < 3:
        numbers.append(0)
    return numbers[0], numbers[1], numbers[2]


def format_three_part_version(major: int, minor: int, patch: int) -> str:
    return f"{major}.{minor}.{patch}"


def format_release_stamp(dt_value: dt.datetime) -> str:
    stamp = dt_value.strftime("%B %d, %Y %I:%M%p")
    stamp = stamp.lstrip("0").replace(" 0", " ")
    return stamp.replace("AM", "am").replace("PM", "pm")


def extract_mod_suffix_for_rename(mod_name: str) -> str:
    """Return the mod-name tail after AGF category for rename matching heuristics."""
    if mod_name.startswith("AGF-"):
        rest = mod_name[len("AGF-"):]
        if "-" in rest:
            return rest.split("-", 1)[1]
    return mod_name


def detect_renamed_mods(
    added_mods: List[Tuple[str, str]],
    removed_mods: List[Tuple[str, str]],
) -> Tuple[List[Tuple[str, str, str]], List[Tuple[str, str]], List[Tuple[str, str]]]:
    """Detect renamed mods and remove them from added/removed buckets.

    Primary match: same version and same mod suffix (category changed).
    Secondary match: same version and best similarity score.
    """
    remaining_added = list(added_mods)
    remaining_removed = list(removed_mods)
    renamed_mods: List[Tuple[str, str, str]] = []

    removed_by_key: Dict[Tuple[str, str], List[Tuple[str, str]]] = {}
    for old_name, old_ver in remaining_removed:
        key = (old_ver, extract_mod_suffix_for_rename(old_name).lower())
        removed_by_key.setdefault(key, []).append((old_name, old_ver))

    unmatched_added: List[Tuple[str, str]] = []
    for new_name, new_ver in remaining_added:
        key = (new_ver, extract_mod_suffix_for_rename(new_name).lower())
        matches = removed_by_key.get(key, [])
        if matches:
            old_name, old_ver = matches.pop(0)
            renamed_mods.append((old_name, new_name, old_ver))
        else:
            unmatched_added.append((new_name, new_ver))

    remaining_added = unmatched_added
    remaining_removed = []
    for entries in removed_by_key.values():
        remaining_removed.extend(entries)

    if remaining_added and remaining_removed:
        unmatched_removed = list(remaining_removed)
        final_added: List[Tuple[str, str]] = []
        for new_name, new_ver in remaining_added:
            candidates = [(old_name, old_ver) for old_name, old_ver in unmatched_removed if old_ver == new_ver]
            if not candidates:
                final_added.append((new_name, new_ver))
                continue

            best = max(
                candidates,
                key=lambda item: difflib.SequenceMatcher(
                    None,
                    extract_mod_suffix_for_rename(item[0]).lower(),
                    extract_mod_suffix_for_rename(new_name).lower(),
                ).ratio(),
            )
            best_ratio = difflib.SequenceMatcher(
                None,
                extract_mod_suffix_for_rename(best[0]).lower(),
                extract_mod_suffix_for_rename(new_name).lower(),
            ).ratio()

            if best_ratio >= 0.8:
                renamed_mods.append((best[0], new_name, best[1]))
                unmatched_removed.remove(best)
            else:
                final_added.append((new_name, new_ver))

        remaining_added = final_added
        remaining_removed = unmatched_removed

    renamed_mods.sort(key=lambda item: (item[0].lower(), item[1].lower()))
    remaining_added.sort(key=lambda item: item[0].lower())
    remaining_removed.sort(key=lambda item: item[0].lower())
    return renamed_mods, remaining_added, remaining_removed


def load_json_file(path: str) -> Dict[str, object]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
        return data if isinstance(data, dict) else {}
    except Exception:
        return {}


def get_gigglepack_release_dir_for_write() -> str:
    return GIGGLEPACK_RELEASE_ROOT_DIR


def get_gigglepack_release_dir_for_read() -> str:
    preferred = GIGGLEPACK_RELEASE_ROOT_DIR
    legacy = os.path.join(ZIP_OUTPUT, RELEASE_META_DIR_NAME, GIGGLEPACK_RELEASE_DATA_DIR)
    if os.path.isdir(preferred):
        return preferred
    if os.path.isdir(legacy):
        return legacy
    return preferred


def write_text_file(path: str, content: str, dry_run: bool, log: Logger) -> None:
    if dry_run:
        log.info(f"[DRYRUN] Would write file: {path}")
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


def split_discord_message(content: str, max_len: int = 2000) -> List[str]:
    if len(content) <= max_len:
        return [content]

    lines = content.splitlines(keepends=True)
    chunks: List[str] = []
    current = ""
    for line in lines:
        if len(current) + len(line) <= max_len:
            current += line
            continue

        if current:
            chunks.append(current.rstrip("\n"))
            current = ""

        while len(line) > max_len:
            chunks.append(line[:max_len])
            line = line[max_len:]
        current = line

    if current:
        chunks.append(current.rstrip("\n"))
    return chunks


def post_discord_webhook_message(webhook_url: str, content: str, dry_run: bool, log: Logger) -> bool:
    parts = split_discord_message(content)
    if dry_run:
        log.info(f"[DRYRUN] Would post Discord message in {len(parts)} part(s)")
        return True

    for idx, part in enumerate(parts, start=1):
        payload = json.dumps({"content": part}).encode("utf-8")
        request = urllib.request.Request(
            webhook_url,
            data=payload,
            headers={"Content-Type": "application/json", "User-Agent": "AGF-GigglePack-Automation"},
            method="POST",
        )
        try:
            with urllib.request.urlopen(request, timeout=20) as response:
                status = response.getcode()
                if status not in (200, 204):
                    log.warn(f"Discord webhook returned HTTP {status} on part {idx}/{len(parts)}")
                    return False
        except urllib.error.URLError as ex:
            log.warn(f"Failed to post Discord webhook part {idx}/{len(parts)}: {ex}")
            return False
        except Exception as ex:
            log.warn(f"Unexpected Discord webhook error on part {idx}/{len(parts)}: {ex}")
            return False

    return True


def maybe_post_discord_release_update(
    release_result: Dict[str, object],
    webhook_url: str,
    dry_run: bool,
    log: Logger,
) -> None:
    if not release_result:
        return

    has_update = bool(release_result.get("has_update", False))
    if not has_update:
        log.info("GigglePack update not detected; skipping Discord auto-post")
        return

    webhook = webhook_url.strip()
    if not webhook:
        log.info(
            f"Discord auto-post skipped: set --discord-webhook-url or env {DISCORD_WEBHOOK_ENV_VAR} to enable"
        )
        return

    discord_text = str(release_result.get("discord_text", "")).strip()
    if not discord_text:
        log.warn("Discord auto-post skipped: generated Discord text is empty")
        return

    if post_discord_webhook_message(webhook, discord_text, dry_run, log):
        log.info("Discord webhook post completed")


def generate_gigglepack_release_artifacts(dry_run: bool, log: Logger) -> Dict[str, object]:
    """Create versioned GigglePack zips and release notes for Discord/GitHub usage."""
    log.info("Step 6.5: Generate GigglePack release metadata + changelog outputs")

    canonical_zip_path = os.path.join(ZIP_OUTPUT, GIGGLEPACK_CANONICAL_ZIP)
    if not os.path.isfile(canonical_zip_path) and not dry_run:
        log.warn(f"GigglePack release metadata skipped: missing {canonical_zip_path}")
        return {"has_update": False, "discord_text": ""}

    release_meta_dir = get_gigglepack_release_dir_for_write()
    state_path = os.path.join(release_meta_dir, "gigglepack-release-state.json")
    discord_path = os.path.join(release_meta_dir, "latest-gigglepack-discord.txt")
    markdown_path = os.path.join(release_meta_dir, "latest-gigglepack-release.md")

    prev_state = load_json_file(state_path)
    prev_version = str(prev_state.get("gigglepack_version", "1.0.0"))
    prev_mods = prev_state.get("mods", {})
    if not isinstance(prev_mods, dict):
        prev_mods = {}

    marker_path = os.path.join(release_meta_dir, GIGGLEPACK_MAJOR_BUMP_MARKER)
    major_bump_requested = os.path.isfile(marker_path)

    all_folders = collect_publishready_folders()
    packs = build_pack_definitions(all_folders)
    giggle_pack = next((pack for pack in packs if pack[0] == "00_GigglePack_All"), None)
    if not giggle_pack:
        log.warn("GigglePack release metadata skipped: could not resolve pack definition")
        return {"has_update": False, "discord_text": ""}

    _, giggle_root, giggle_optionals = giggle_pack
    giggle_mod_folder_names = set(giggle_root)
    if giggle_optionals:
        for opt_mods in giggle_optionals.values():
            giggle_mod_folder_names.update(opt_mods)

    current_versions = gather_mod_versions_by_base()
    giggle_mod_versions: Dict[str, str] = {}
    for folder_name in sorted(giggle_mod_folder_names):
        base_name = get_base_mod_name(folder_name)
        if base_name in current_versions:
            giggle_mod_versions[base_name] = current_versions[base_name]

    updated_existing_mods: List[Tuple[str, str, str]] = []
    added_mods: List[Tuple[str, str]] = []
    removed_mods: List[Tuple[str, str]] = []

    for mod_name in sorted(giggle_mod_versions):
        current_ver = giggle_mod_versions[mod_name]
        previous_ver = str(prev_mods.get(mod_name, ""))
        if not previous_ver:
            added_mods.append((mod_name, current_ver))
        elif compare_versions(current_ver, previous_ver) > 0:
            updated_existing_mods.append((mod_name, previous_ver, current_ver))

    for mod_name in sorted(prev_mods):
        if mod_name not in giggle_mod_versions:
            removed_mods.append((mod_name, str(prev_mods.get(mod_name, ""))))

    renamed_mods, added_mods, removed_mods = detect_renamed_mods(added_mods, removed_mods)

    is_baseline_release = not prev_state

    if is_baseline_release:
        release_version = "1.0.0"
    elif major_bump_requested:
        prev_major, _, _ = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major + 1, 0, 0)
    elif added_mods:
        prev_major, prev_minor, _ = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major, prev_minor + 1, 0)
    elif updated_existing_mods or renamed_mods or removed_mods:
        prev_major, prev_minor, prev_patch = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major, prev_minor, prev_patch + 1)
    else:
        release_version = prev_version

    has_update = bool(
        is_baseline_release
        or major_bump_requested
        or added_mods
        or updated_existing_mods
        or renamed_mods
        or removed_mods
    )

    if not has_update:
        existing_discord_text = ""
        if os.path.isfile(discord_path):
            try:
                with open(discord_path, "r", encoding="utf-8") as f:
                    existing_discord_text = f.read().strip()
            except Exception:
                existing_discord_text = ""

        # Do not rewrite latest files/state on no-change runs.
        return {
            "has_update": False,
            "release_version": prev_version,
            "discord_text": existing_discord_text,
            "discord_path": discord_path,
        }

    previous_release_version = prev_version

    versioned_zip_name = f"{GIGGLEPACK_VERSIONED_ZIP_PREFIX}{release_version}.zip"
    versioned_zip_path = os.path.join(ZIP_OUTPUT, versioned_zip_name)

    if dry_run:
        log.info(f"[DRYRUN] Would copy {GIGGLEPACK_CANONICAL_ZIP} -> {versioned_zip_name}")
    else:
        shutil.copy2(canonical_zip_path, versioned_zip_path)

    new_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "to": new_ver}
        for mod_name, new_ver in added_mods
    ]
    updated_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "from": old_ver, "to": new_ver}
        for mod_name, old_ver, new_ver in updated_existing_mods
    ]
    renamed_mod_entries: List[Dict[str, str]] = [
        {"from_mod": old_mod_name, "to_mod": new_mod_name, "version": ver}
        for old_mod_name, new_mod_name, ver in renamed_mods
    ]
    removed_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "from": old_ver}
        for mod_name, old_ver in removed_mods
    ]

    beta_display_by_base: Dict[str, bool] = {}
    for folder_name, folder_path in scan_mod_folders(PUBLISH_READY).items():
        modinfo_path = os.path.join(folder_path, "ModInfo.xml")
        mod_name, _ = parse_modinfo(modinfo_path, folder_name)
        display_name = get_modinfo_display_name(modinfo_path, mod_name)
        beta_display_by_base[get_base_mod_name(folder_name)] = "BETA" in display_name

    def maybe_beta_display(mod_name: str, version_value: str) -> str:
        if beta_display_by_base.get(mod_name, False) and not str(version_value).endswith("-BETA"):
            return f"{version_value}-BETA"
        return version_value

    new_mod_entries_display: List[Dict[str, str]] = [
        {
            "mod": entry["mod"],
            "to": entry["to"],
            "to_display": maybe_beta_display(entry["mod"], entry["to"]),
        }
        for entry in new_mod_entries
    ]
    updated_mod_entries_display: List[Dict[str, str]] = [
        {
            "mod": entry["mod"],
            "from": entry["from"],
            "to": entry["to"],
            "from_display": maybe_beta_display(entry["mod"], entry["from"]),
            "to_display": maybe_beta_display(entry["mod"], entry["to"]),
        }
        for entry in updated_mod_entries
    ]
    renamed_mod_entries_display: List[Dict[str, str]] = [
        {
            "from_mod": entry["from_mod"],
            "to_mod": entry["to_mod"],
            "version": entry["version"],
            "version_display": maybe_beta_display(entry["to_mod"], entry["version"]),
        }
        for entry in renamed_mod_entries
    ]
    removed_mod_entries_display: List[Dict[str, str]] = [
        {
            "mod": entry["mod"],
            "from": entry["from"],
            "from_display": maybe_beta_display(entry["mod"], entry["from"]),
        }
        for entry in removed_mod_entries
    ]

    # v1.0.0 special baseline: keep changelog intentionally focused and readable.
    if release_version == "1.0.0":
        focused_entries: List[Dict[str, str]] = []
        for mod_name in GIGGLEPACK_V100_FOCUS_MODS:
            if mod_name in giggle_mod_versions:
                focused_entries.append({"mod": mod_name, "to": giggle_mod_versions[mod_name]})
        if focused_entries:
            new_mod_entries = focused_entries
            new_mod_entries_display = [
                {
                    "mod": entry["mod"],
                    "to": entry["to"],
                    "to_display": maybe_beta_display(entry["mod"], entry["to"]),
                }
                for entry in focused_entries
            ]
            updated_mod_entries = []
            updated_mod_entries_display = []
            renamed_mod_entries = []
            renamed_mod_entries_display = []
            removed_mod_entries = []
            removed_mod_entries_display = []

    new_mod_lines = [
        f"- {mod_download_markdown_link(entry['mod'])} (new: v{entry.get('to_display', entry['to'])})"
        for entry in new_mod_entries_display
    ]
    updated_existing_lines = [
        f"- {mod_download_markdown_link(entry['mod'])} "
        f"(v{entry.get('from_display', entry['from'])} -> v{entry.get('to_display', entry['to'])})"
        for entry in updated_mod_entries_display
    ]
    renamed_lines = [
        f"- {mod_download_markdown_link(entry['to_mod'])} "
        f"(renamed from {entry['from_mod']}, v{entry.get('version_display', entry['version'])})"
        for entry in renamed_mod_entries_display
    ]
    removed_lines = [
        f"- {entry['mod']} (was v{entry.get('from_display', entry['from'])})"
        for entry in removed_mod_entries_display
    ]
    if has_update:
        now_dt = dt.datetime.now()
    else:
        previous_release_stamp = str(prev_state.get("released_at", "")).strip()
        try:
            now_dt = dt.datetime.strptime(previous_release_stamp, "%Y-%m-%d %H:%M:%S")
        except Exception:
            now_dt = dt.datetime.now()
    now_iso = now_dt.strftime("%Y-%m-%d %H:%M:%S")
    now_display = format_release_stamp(now_dt)

    discord_text = render_discord_post_from_template(
        release_version=release_version,
        now_iso=now_display,
        versioned_zip_name=versioned_zip_name,
        previous_release_version=previous_release_version,
        new_mod_entries=new_mod_entries_display,
        updated_mod_entries=updated_mod_entries_display,
        renamed_mod_entries=renamed_mod_entries_display,
        removed_mod_entries=removed_mod_entries_display,
        log=log,
    )

    if not discord_text:
        fallback_chunks: List[str] = [
            f"GigglePack v{release_version} - {now_display}",
            f"Download: {zip_download_link(GIGGLEPACK_CANONICAL_ZIP)}",
            (
                f"Change summary: +{len(new_mod_lines)} new, ~{len(updated_existing_lines)} updated, "
                f"={len(renamed_lines)} renamed, -{len(removed_lines)} removed"
            ),
            "",
            "**New mods:**",
        ]
        fallback_chunks.extend(new_mod_lines or ["- None"])
        fallback_chunks.extend(["", "**Updated existing mods:**"])
        fallback_chunks.extend(updated_existing_lines or ["- None"])
        fallback_chunks.extend(["", "**Renamed mods:**"])
        fallback_chunks.extend(renamed_lines or ["- None"])
        fallback_chunks.extend(["", "**Removed mods:**"])
        fallback_chunks.extend(removed_lines or ["- None"])
        discord_text = "\n".join(fallback_chunks).strip() + "\n"

    markdown_lines: List[str] = [
        f"## GigglePack v{release_version} ({now_display})",
        (
            f"### Summary: +{len(new_mod_lines)} new, ~{len(updated_existing_lines)} updated, "
            f"={len(renamed_lines)} renamed, -{len(removed_lines)} removed"
        ),
        "- **New Mods**",
    ]
    if new_mod_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in new_mod_lines])
    else:
        markdown_lines.append("  - None")

    markdown_lines.append("- **Updated Existing Mods**")
    if updated_existing_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in updated_existing_lines])
    else:
        markdown_lines.append("  - None")

    markdown_lines.append("- **Renamed Mods**")
    if renamed_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in renamed_lines])
    else:
        markdown_lines.append("  - None")

    markdown_lines.append("- **Removed Mods**")
    if removed_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in removed_lines])
    else:
        markdown_lines.append("  - None")

    new_markdown_entry = "\n".join(markdown_lines).strip()

    existing_entries: List[str] = []
    if os.path.isfile(markdown_path):
        try:
            with open(markdown_path, "r", encoding="utf-8") as f:
                existing_content = f.read().strip()
            if existing_content:
                if existing_content.startswith("# GigglePack Release Changelog"):
                    existing_body = existing_content.split("\n", 1)[1].strip() if "\n" in existing_content else ""
                else:
                    existing_body = existing_content
                if existing_body:
                    existing_entries = [
                        chunk.strip() for chunk in re.split(r"\n\n---\n\n", existing_body) if chunk.strip()
                    ]
        except Exception:
            existing_entries = []

    def entry_version(entry_text: str) -> str:
        match = re.search(r"^#{1,2}\s*GigglePack\s+v([0-9]+\.[0-9]+\.[0-9]+)", entry_text, re.MULTILINE)
        return match.group(1) if match else ""

    filtered_entries = [entry for entry in existing_entries if entry_version(entry) != release_version]
    all_entries = [new_markdown_entry] + filtered_entries
    markdown_text = (
        "# GigglePack Release Changelog\n"
        "Newest entries appear at the top.\n\n"
        + "\n\n---\n\n".join(all_entries).strip()
        + "\n"
    )

    write_text_file(discord_path, discord_text, dry_run, log)
    write_text_file(markdown_path, markdown_text, dry_run, log)

    state_payload = {
        "gigglepack_version": release_version,
        "previous_gigglepack_version": prev_version,
        "is_baseline_release": is_baseline_release,
        "released_at": now_iso,
        "mods": giggle_mod_versions,
        "updated_mods": updated_mod_entries,
        "new_mods": new_mod_entries,
        "renamed_mods": renamed_mod_entries,
        "removed_mods": removed_mod_entries,
        "versioned_zip": versioned_zip_name,
        "change_counts": {
            "new_mods": len(new_mod_entries),
            "updated_existing_mods": len(updated_mod_entries),
            "renamed_mods": len(renamed_mod_entries),
            "removed_mods": len(removed_mod_entries),
        },
    }

    if dry_run:
        log.info(f"[DRYRUN] Would write state file: {state_path}")
    else:
        os.makedirs(release_meta_dir, exist_ok=True)
        with open(state_path, "w", encoding="utf-8") as f:
            json.dump(state_payload, f, indent=2, ensure_ascii=True)
        if major_bump_requested:
            try:
                os.remove(marker_path)
                log.info(f"Consumed major bump marker: {marker_path}")
            except Exception as ex:
                log.warn(f"Could not remove major bump marker {marker_path}: {ex}")

    return {
        "has_update": has_update,
        "release_version": release_version,
        "discord_text": discord_text,
        "discord_path": discord_path,
    }

    log.info(
        f"GigglePack release v{release_version}: {len(updated_existing_lines)} updated existing mods, "
        f"{len(new_mod_entries)} new mods, {len(removed_lines)} removed mods"
    )


def create_all_zips(dry_run: bool, workers: int, log: Logger) -> List[str]:
    log.info("Step 6: Create all mod and category zip files")

    if dry_run:
        log.info(f"[DRYRUN] Would ensure zip output directory exists: {ZIP_OUTPUT}")
    else:
        os.makedirs(ZIP_OUTPUT, exist_ok=True)

    existing_zips = []
    if os.path.isdir(ZIP_OUTPUT):
        existing_zips = [f for f in os.listdir(ZIP_OUTPUT) if f.lower().endswith(".zip")]

    for file in existing_zips:
        path = os.path.join(ZIP_OUTPUT, file)
        if dry_run:
            log.info(f"[DRYRUN] Would delete old zip: {file}")
        else:
            try:
                os.remove(path)
            except Exception as ex:
                log.warn(f"Failed to delete old zip {file}: {ex}")

    all_folders = collect_publishready_folders()

    created_mod_zips: List[str] = []
    if all_folders:
        with ThreadPoolExecutor(max_workers=max(1, workers)) as executor:
            futures = [executor.submit(zip_mod_folder, folder, dry_run, log) for folder in all_folders]
            for future in as_completed(futures):
                zip_name, ok = future.result()
                if ok:
                    created_mod_zips.append(zip_name)
                    log.stats.mod_zips_created += 1

    packs = build_pack_definitions(all_folders)

    for pack_name, root_mods, optionals_map in packs:
        ok = zip_category(pack_name, root_mods, optionals_map, dry_run, log)
        if ok:
            log.stats.pack_zips_created += 1
            created_mod_zips.append(f"{pack_name}.zip")

    return sorted(set(created_mod_zips))


# =============================================================
# STEP 7: MAIN README
# =============================================================
def build_mod_entry(folder_name: str) -> str:
    mod_path = os.path.join(PUBLISH_READY, folder_name)
    modinfo_path = os.path.join(mod_path, "ModInfo.xml")
    readme_path = os.path.join(mod_path, "README.md")

    name, version = parse_modinfo(modinfo_path, folder_name)
    display_name = get_modinfo_display_name(modinfo_path, name)
    version_display = format_version_for_display(version, display_name)
    desc = extract_mod_description_from_modinfo(modinfo_path)
    link = zip_download_link(f"{get_base_mod_name(folder_name)}.zip")

    features = extract_readme_block(readme_path, "<!-- FEATURES START -->", "<!-- FEATURES END -->").strip("\n")

    block = [f"> ### **{name}** *-v{version_display}* - [Download]({link})", f"> *{desc}*"]
    if features:
        features_html = markdown_features_to_html(features)
        block.append(
            "> <details> <summary>*Show detailed features*</summary>\n>\n"
            f"> {features_html}\n>\n> </details>"
        )
    block.append("> \n---\n")
    return "\n".join(block)


def load_gigglepack_release_state() -> Dict[str, object]:
    state_path = os.path.join(get_gigglepack_release_dir_for_read(), "gigglepack-release-state.json")
    return load_json_file(state_path)


def load_recent_gigglepack_release_entries(limit: int = 3) -> List[Dict[str, object]]:
    if limit <= 0:
        return []

    markdown_path = os.path.join(get_gigglepack_release_dir_for_read(), "latest-gigglepack-release.md")
    if not os.path.isfile(markdown_path):
        return []

    try:
        with open(markdown_path, "r", encoding="utf-8") as f:
            lines = f.read().splitlines()
    except Exception:
        return []

    entries: List[Dict[str, object]] = []
    current: Optional[Dict[str, object]] = None
    section: Optional[str] = None

    for raw_line in lines:
        line = raw_line.strip()
        if not line:
            continue

        if line in {"# GigglePack Release Changelog", "Newest entries appear at the top.", "---"}:
            continue

        header_match = re.match(r"^##\s+GigglePack\s+v([0-9]+\.[0-9]+\.[0-9]+)\s+\((.+)\)$", line)
        if header_match:
            if current:
                entries.append(current)
            current = {
                "version": header_match.group(1).strip(),
                "stamp": header_match.group(2).strip(),
                "new": [],
                "updated": [],
                "renamed": [],
                "removed": [],
                "new_count": 0,
                "updated_count": 0,
                "renamed_count": 0,
                "removed_count": 0,
            }
            section = None
            continue

        if not current:
            continue

        summary_match = re.match(
            r"^###\s+Summary:\s*\+(\d+)\s+new,\s*~(\d+)\s+updated,\s*=(\d+)\s+renamed,\s*-(\d+)\s+removed$",
            line,
        )
        if summary_match:
            current["new_count"] = int(summary_match.group(1))
            current["updated_count"] = int(summary_match.group(2))
            current["renamed_count"] = int(summary_match.group(3))
            current["removed_count"] = int(summary_match.group(4))
            continue

        old_summary_match = re.match(
            r"^###\s+Summary:\s*\+(\d+)\s+new,\s*~(\d+)\s+updated,\s*-(\d+)\s+removed$",
            line,
        )
        if old_summary_match:
            current["new_count"] = int(old_summary_match.group(1))
            current["updated_count"] = int(old_summary_match.group(2))
            current["renamed_count"] = 0
            current["removed_count"] = int(old_summary_match.group(3))
            continue

        if line == "- **New Mods**":
            section = "new"
            continue
        if line == "- **Updated Existing Mods**":
            section = "updated"
            continue
        if line == "- **Renamed Mods**":
            section = "renamed"
            continue
        if line == "- **Removed Mods**":
            section = "removed"
            continue

        if section and line.startswith("- "):
            item_text = line[2:].strip()
            current_list = current.get(section)
            if isinstance(current_list, list):
                current_list.append(item_text)

    if current:
        entries.append(current)

    return entries[:limit]


def build_gigglepack_readme_release_lines(state: Dict[str, object]) -> List[str]:
    history_entries = load_recent_gigglepack_release_entries(limit=3)
    if not history_entries and not state:
        return []

    def escape_html(text: str) -> str:
        return text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

    def mod_download_html_link(mod_name: str) -> str:
        href = escape_html(zip_download_link(f"{get_base_mod_name(mod_name)}.zip"))
        return f"<a href=\"{href}\">{escape_html(mod_name)}</a>"

    def markdown_links_to_html(text: str) -> str:
        parts: List[str] = []
        last_idx = 0
        for match in re.finditer(r"\[([^\]]+)\]\(([^\)]+)\)", text):
            parts.append(escape_html(text[last_idx:match.start()]))
            label = escape_html(match.group(1).strip())
            href = escape_html(match.group(2).strip())
            parts.append(f"<a href=\"{href}\">{label}</a>")
            last_idx = match.end()
        parts.append(escape_html(text[last_idx:]))
        return "".join(parts)

    def render_release_item_html(item_text: str, section_name: str) -> str:
        if section_name == "removed":
            plain_text = re.sub(r"\[([^\]]+)\]\(([^\)]+)\)", r"\1", item_text)
            return escape_html(plain_text)

        if "[" in item_text and "](" in item_text:
            return markdown_links_to_html(item_text)

        if section_name in ("new", "updated"):
            match = re.match(r"^([A-Za-z0-9\-]+)(\s*\(.+\))$", item_text)
            if match:
                mod_name = match.group(1).strip()
                suffix = match.group(2)
                if is_agf_mod(mod_name):
                    return f"{mod_download_html_link(mod_name)}{escape_html(suffix)}"

        if section_name == "renamed":
            match = re.match(r"^([A-Za-z0-9\-]+)(\s*\(renamed from .+\))$", item_text)
            if match:
                mod_name = match.group(1).strip()
                suffix = match.group(2)
                if is_agf_mod(mod_name):
                    return f"{mod_download_html_link(mod_name)}{escape_html(suffix)}"

        return escape_html(item_text)

    parsed_entries: List[Dict[str, object]] = []
    if history_entries:
        for entry in history_entries:
            version = str(entry.get("version", "")).strip() or "unknown"
            stamp = str(entry.get("stamp", "")).strip()
            new_items = [render_release_item_html(str(item).strip(), "new") for item in entry.get("new", []) if str(item).strip()]
            updated_items = [render_release_item_html(str(item).strip(), "updated") for item in entry.get("updated", []) if str(item).strip()]
            renamed_items = [render_release_item_html(str(item).strip(), "renamed") for item in entry.get("renamed", []) if str(item).strip()]
            removed_items = [render_release_item_html(str(item).strip(), "removed") for item in entry.get("removed", []) if str(item).strip()]
            parsed_entries.append({
                "header": f"GigglePack v{escape_html(version)}" + (f" - {escape_html(stamp)}" if stamp else ""),
                "new_count": int(entry.get("new_count", len(new_items))),
                "updated_count": int(entry.get("updated_count", len(updated_items))),
                "renamed_count": int(entry.get("renamed_count", len(renamed_items))),
                "removed_count": int(entry.get("removed_count", len(removed_items))),
                "new_items": new_items,
                "updated_items": updated_items,
                "renamed_items": renamed_items,
                "removed_items": removed_items,
            })
    else:
        release_version = str(state.get("gigglepack_version", "")).strip() or "unknown"
        previous_release_version = str(state.get("previous_gigglepack_version", "")).strip()
        released_at = str(state.get("released_at", "")).strip()
        new_mods = state.get("new_mods", [])
        updated_mods = state.get("updated_mods", [])
        renamed_mods = state.get("renamed_mods", [])
        removed_mods = state.get("removed_mods", [])
        change_counts = state.get("change_counts", {}) if isinstance(state.get("change_counts", {}), dict) else {}

        def format_release_stamp(raw: str) -> str:
            if not raw:
                return ""
            try:
                parsed = dt.datetime.strptime(raw, "%Y-%m-%d %H:%M:%S")
                stamp = parsed.strftime("%B %d, %Y %I:%M%p")
                stamp = stamp.lstrip("0").replace(" 0", " ")
                return stamp.replace("AM", "am").replace("PM", "pm")
            except Exception:
                return raw

        new_items: List[str] = []
        if isinstance(new_mods, list):
            for entry in new_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    to_ver = str(entry.get("to", "")).strip()
                    if mod_name:
                        new_items.append(f"{mod_download_html_link(mod_name)} (new: v{escape_html(to_ver)})")

        updated_items: List[str] = []
        if isinstance(updated_mods, list):
            for entry in updated_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    from_ver = str(entry.get("from", "")).strip()
                    to_ver = str(entry.get("to", "")).strip()
                    if mod_name:
                        updated_items.append(
                            f"{mod_download_html_link(mod_name)} (v{escape_html(from_ver)} -&gt; v{escape_html(to_ver)})"
                        )

        renamed_items: List[str] = []
        if isinstance(renamed_mods, list):
            for entry in renamed_mods:
                if isinstance(entry, dict):
                    from_mod = str(entry.get("from_mod", "")).strip()
                    to_mod = str(entry.get("to_mod", "")).strip()
                    version = str(entry.get("version", "")).strip()
                    if from_mod and to_mod:
                        renamed_items.append(
                            f"{mod_download_html_link(to_mod)} (renamed from {escape_html(from_mod)}, v{escape_html(version)})"
                        )

        removed_items: List[str] = []
        if isinstance(removed_mods, list):
            for entry in removed_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    from_ver = str(entry.get("from", "")).strip()
                    if mod_name:
                        removed_items.append(f"{escape_html(mod_name)} (was v{escape_html(from_ver)})")

        header = f"GigglePack v{escape_html(release_version)}"
        release_stamp = format_release_stamp(released_at)
        if release_stamp:
            header += f" - {escape_html(release_stamp)}"

        parsed_entries.append({
            "header": header,
            "new_count": int(change_counts.get("new_mods", len(new_items))),
            "updated_count": int(change_counts.get("updated_existing_mods", len(updated_items))),
            "renamed_count": int(change_counts.get("renamed_mods", len(renamed_items))),
            "removed_count": int(change_counts.get("removed_mods", len(removed_items))),
            "new_items": new_items,
            "updated_items": updated_items,
            "renamed_items": renamed_items,
            "removed_items": removed_items,
            "previous_release_version": previous_release_version,
        })

    lines: List[str] = []

    def build_inner_list(items: List[str]) -> str:
        if not items:
            return "<ul><li>None</li></ul>"
        return "<ul>" + "".join(f"<li>{item}</li>" for item in items) + "</ul>"

    detail_bits: List[str] = ["<ul>"]
    for idx, entry in enumerate(parsed_entries):
        header_line = str(entry.get("header", "")).strip() or "GigglePack"
        new_count = int(entry.get("new_count", 0))
        updated_count = int(entry.get("updated_count", 0))
        renamed_count = int(entry.get("renamed_count", 0))
        removed_count = int(entry.get("removed_count", 0))
        new_items = entry.get("new_items", []) if isinstance(entry.get("new_items", []), list) else []
        updated_items = entry.get("updated_items", []) if isinstance(entry.get("updated_items", []), list) else []
        renamed_items = entry.get("renamed_items", []) if isinstance(entry.get("renamed_items", []), list) else []
        removed_items = entry.get("removed_items", []) if isinstance(entry.get("removed_items", []), list) else []

        detail_bits.extend([
            f"<li>{header_line}",
            "<ul>",
            (
                f"<li>Change summary: +{new_count} new, ~{updated_count} updated, "
                f"={renamed_count} renamed, -{removed_count} removed</li>"
            ),
        ])
        previous_release_version = str(entry.get("previous_release_version", "")).strip()
        entry_version_match = re.search(r"v([0-9]+\.[0-9]+\.[0-9]+)", header_line)
        entry_version = entry_version_match.group(1) if entry_version_match else ""
        if previous_release_version and previous_release_version != entry_version:
            detail_bits.append(f"<li>Previous GigglePack version: v{escape_html(previous_release_version)}</li>")

        detail_bits.append(f"<li>New mods:{build_inner_list(new_items)}</li>")
        detail_bits.append(f"<li>Updated existing mods:{build_inner_list(updated_items)}</li>")
        detail_bits.append(f"<li>Renamed mods:{build_inner_list(renamed_items)}</li>")
        detail_bits.append(f"<li>Removed mods:{build_inner_list(removed_items)}</li>")
        detail_bits.extend(["</ul>", "</li>"])

    detail_bits.append("</ul>")
    changelog_html = "".join(detail_bits)

    lines.extend([
        "> <details> <summary><i>Changelog (latest 3 releases)</i></summary>",
        ">",
        f"> {changelog_html}",
        ">",
        "> </details>",
    ])

    return lines


def generate_main_readme(dry_run: bool, log: Logger) -> None:
    log.info("Step 7: Generate main README.md")

    if not os.path.exists(MAIN_TEMPLATE_PATH):
        log.error(f"Missing required template: {MAIN_TEMPLATE_PATH}")
        raise FileNotFoundError(MAIN_TEMPLATE_PATH)

    with open(MAIN_TEMPLATE_PATH, "r", encoding="utf-8") as f:
        main_template = f.read()

    now_str = dt.datetime.now().strftime("%B %d, %Y, %I:%M %p EST").lstrip("0").replace(" 0", " ")
    main_content = main_template.replace("{{LAST_UPDATED}}", now_str)

    all_mods = collect_publishready_folders()
    backpackplus_mods = [f for f in all_mods if f.startswith("AGF-BackpackPlus-")]
    hudplus_mods = [f for f in all_mods if f.startswith("AGF-HUDPlus-")]
    hudpluszother_mods = [f for f in all_mods if f.startswith("AGF-HUDPluszOther-")]
    noeac_mods = [f for f in all_mods if f.startswith("AGF-NoEAC-")]
    modders_mods = [f for f in all_mods if f.startswith("AGF-4Modders-")]
    vp_mods = [f for f in all_mods if f.startswith("AGF-VP-")]
    special_mods = [f for f in all_mods if f.startswith("zzzAGF-Special")]

    md: List[str] = []
    giggle_release_state = load_gigglepack_release_state()
    giggle_release_version = str(giggle_release_state.get("gigglepack_version", "")).strip()
    giggle_download_label = f"[**⬇️ DOWNLOAD ALL AGF MODS**]({zip_download_link('00_GigglePack_All.zip')})"
    if giggle_release_version:
        giggle_download_label += f" **(GigglePack v{giggle_release_version})**"

    giggle_release_lines = build_gigglepack_readme_release_lines(giggle_release_state)

    md.extend([
        "---", "", "<br>", "", "## **A. GIGGLE PACK**", "",
        "*[(Back to Top)](#agf-7-days-to-die-mods)*", "", "---", "",
        giggle_download_label, "",
        "> - *All AGF mods in one convenient download.*",
        "> - *Direct set-up is AGF preference and only server side mods*",
        "> - *Client Side enhancements are in the NoEAC folder*", "", "---", "",
    ])
    if giggle_release_lines:
        md.extend(giggle_release_lines)
        md.extend(["", "---", ""])

    md.extend([
        "---", "", "<br>", "", "## **B. HUD PLUS MODS**", "",
        "*[(Back to Top)](#agf-7-days-to-die-mods)*", "", "---", "",
        f"[**⬇️ Download All HUD Plus Mods**]({zip_download_link('00_HUDPlus_All.zip')})", "", "---", "",
        "*Quality-of-life HUD enhancements and visual tweaks.*", "",
    ])
    for mod in hudplus_mods:
        md.append(build_mod_entry(mod))

    if hudpluszother_mods:
        md.extend([
            "---", "", "<br>", "",
            f"### **Optional HUDPlus Tweaks** - [Download All]({zip_download_link('00_HUDPluszOther_All.zip')})", "",
            "| Display Name | Version | Download | Description |",
            "|---|---|---|---|",
        ])
        for mod in hudpluszother_mods:
            mod_path = os.path.join(PUBLISH_READY, mod)
            modinfo_path = os.path.join(mod_path, "ModInfo.xml")
            name, version = parse_modinfo(modinfo_path, mod)
            display_name = get_modinfo_display_name(modinfo_path, name)
            version_display = format_version_for_display(version, display_name)
            desc = extract_mod_description_from_modinfo(modinfo_path)
            link = zip_download_link(f"{get_base_mod_name(mod)}.zip")
            md.append(f"| {name} | {version_display} | [Download]({link}) | {desc} |")
        md.extend(["", "---"])

    md.extend([
        "---", "", "<br>", "", "## **C. BACKPACK PLUS MODS**", "",
        "*[(Back to Top)](#agf-7-days-to-die-mods)*", "",
        f"[**⬇️ Download All Backpack Plus Mods**]({zip_download_link('00_BackpackPlus_All.zip')})", "", "---", "",
        "*Download all above or select one below.*", "",
    ])

    preferred_last = "AGF-BackpackPlus-119Slots"
    backpack_sorted = sorted(backpackplus_mods, key=lambda x: (get_base_mod_name(x) == preferred_last, x))
    for mod in backpack_sorted:
        md.append(build_mod_entry(mod))

    if special_mods:
        md.extend([
            "---", "", "<br>", "", "## **D. SPECIAL COMPATIBILITY MOD**", "",
            "*[(Back to Top)](#agf-7-days-to-die-mods)*", "", "---", "",
        ])
        for mod in special_mods:
            md.append(build_mod_entry(mod))

    md.extend([
        "---", "", "<br>", "", "## **E. VANILLA PLUS MODS**", "",
        "*[(Back to Top)](#agf-7-days-to-die-mods)*", "",
        f"[**⬇️ Download All VP Mods**]({zip_download_link('00_VP_All.zip')})", "", "---", "",
        "*Vanilla Plus: gameplay tweaks and new features.*", "",
    ])
    for mod in vp_mods:
        md.append(build_mod_entry(mod))

    if noeac_mods:
        md.extend([
            "---", "", "<br>", "", "## **F. NO EAC MODS**", "",
            "*[(Back to Top)](#agf-7-days-to-die-mods)*", "",
            f"[**⬇️ Download All NoEAC Mods**]({zip_download_link('00_NoEAC_All.zip')})", "", "---", "",
            "*Mods that require EAC to be off or are client-side only.*", "",
        ])
        for mod in noeac_mods:
            md.append(build_mod_entry(mod))

    md.extend([
        "---", "", "<br>", "", "## **G. 4MODDERS MODS**", "",
        "*[(Back to Top)](#agf-7-days-to-die-mods)*", "",
        f"[**⬇️ Download All 4Modders Mods**]({zip_download_link('00_4Modders_All.zip')})", "", "---", "",
        "*Optional modder-focused tools and helpers. Not auto-pushed to game root.*", "",
    ])
    if modders_mods:
        for mod in modders_mods:
            md.append(build_mod_entry(mod))
    else:
        md.append("*No AGF-4Modders mods are currently published.*")

    modlist_str = "\n".join(md)
    main_content = re.sub(
        r"<!-- MOD_LIST_START -->(.*?)<!-- MOD_LIST_END -->",
        f"<!-- MOD_LIST_START -->\n{modlist_str}\n<!-- MOD_LIST_END -->",
        main_content,
        flags=re.DOTALL,
    )

    main_content = main_content.replace("<li>></li>", "")

    lines = main_content.splitlines()
    cleaned: List[str] = []
    for i, line in enumerate(lines):
        if line.strip() == "---" and i > 0 and lines[i - 1].strip() != "":
            cleaned.append("")
        cleaned.append(line)

    final_content = "\n".join(cleaned)

    if dry_run:
        log.info(f"[DRYRUN] Would write main README: {MAIN_README_PATH}")
        return

    with open(MAIN_README_PATH, "w", encoding="utf-8") as f:
        f.write(final_content)


# =============================================================
# VALIDATION
# =============================================================
def validate_required_paths(strict: bool, log: Logger, mode: str) -> bool:
    ok = True
    required_dirs = []
    required_files = []

    if mode == "sync-work":
        required_dirs = [STAGING, GAME_MODS]
    elif mode == "prep-work":
        required_dirs = [STAGING]
        required_files = [MOD_README_TEMPLATE]
    elif mode == "promote":
        required_dirs = [STAGING, PUBLISH_READY]
        required_files = [MOD_README_TEMPLATE]
    elif mode == "package":
        required_dirs = [PUBLISH_READY]
        required_files = [MOD_README_TEMPLATE, MAIN_TEMPLATE_PATH]
    else:
        required_dirs = [STAGING, PUBLISH_READY, IN_PROGRESS, GAME_MODS]
        required_files = [MOD_README_TEMPLATE, MAIN_TEMPLATE_PATH]

    for path in required_dirs:
        if not os.path.isdir(path):
            message = f"Required directory not found: {path}"
            if strict:
                log.error(message)
                ok = False
            else:
                log.warn(message)

    for path in required_files:
        if not os.path.isfile(path):
            message = f"Required template not found: {path}"
            if strict:
                log.error(message)
                ok = False
            else:
                log.warn(message)

    return ok


# =============================================================
# MAIN
# =============================================================
def run_pipeline(args: argparse.Namespace) -> int:
    log = Logger(verbose=args.verbose, dry_run=args.dry_run)
    log.info("Starting SCRIPT-Main automation pipeline")
    log.info(f"Selected mode: {args.mode}")
    if args.dry_run:
        log.info("Dry-run mode enabled: no file system changes will be written")

    if not validate_required_paths(strict=args.strict, log=log, mode=args.mode):
        log.error("Path validation failed in strict mode")
        log_path = log.write_log_file()
        if log_path:
            print(f"Log file: {log_path}")
        return 1

    try:
        if args.mode == "sync-work":
            enforce_staging_major_policy(args.dry_run, log)
            sync_staging_and_game(args.dry_run, log)
        elif args.mode == "prep-work":
            prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
        elif args.mode == "promote":
            enforce_staging_major_policy(args.dry_run, log)
            prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            promote_staging_to_publish_ready(args.dry_run, log)
        elif args.mode == "package":
            prep_names_and_readmes_for_dirs((PUBLISH_READY,), args.dry_run, log)
            create_all_zips(args.dry_run, args.workers, log)
            generate_gigglepack_release_artifacts(args.dry_run, log)
            generate_main_readme(args.dry_run, log)
        else:
            log.info(
                "Mode full: sync Game<->Draft for tracked draft mods, ingest Draft->ActiveBuild, "
                "sync ActiveBuild<->Game, then finalize/promote/package."
            )

            # 0.25) Pull newer game updates into Draft for overlapping AGF mods.
            sync_game_and_draft(args.dry_run, log)

            # 0.5) Ensure ActiveBuild includes latest Draft copies before game sync.
            sync_draft_to_staging_latest(args.dry_run, log)

            # 0.75) Enforce lane policy after ingest so v0.x stays in Draft.
            enforce_staging_major_policy(args.dry_run, log)

            # 1) Keep ActiveBuild and Game synchronized first.
            sync_staging_and_game(args.dry_run, log)

            # 2) Apply naming/readme updates in ActiveBuild.
            prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(STAGING, args.dry_run, log)
            cleanup_older_versions_in_dir(STAGING, args.dry_run, log)

            # 3) Push finalized ActiveBuild changes back to game (version/hash aware).
            sync_staging_and_game(args.dry_run, log)

            # 4) Promote finalized ActiveBuild content to ReleaseSource.
            promote_staging_to_publish_ready(args.dry_run, log)

            # 4.5) Remove stale legacy folder names replaced by AGF-4Modders naming.
            cleanup_release_legacy_4modders_renames(args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(PUBLISH_READY, args.dry_run, log)
            cleanup_older_versions_in_dir(PUBLISH_READY, args.dry_run, log)

            # 5) Ensure ReleaseSource + Draft metadata/quotes/readmes are normalized before packaging.
            release_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(PUBLISH_READY,))
            draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            all_renames = release_renames + draft_renames
            csv_rows = normalize_compat_csv(
                all_renames,
                args.dry_run,
                log,
                mod_dirs=(PUBLISH_READY, IN_PROGRESS),
                prune_to_mods_now=True,
            )
            normalize_quote_files(csv_rows, all_renames, args.dry_run, log)
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(PUBLISH_READY,))
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(IN_PROGRESS,))

            # 6) Package and regenerate main README.
            create_all_zips(args.dry_run, args.workers, log)
            generate_gigglepack_release_artifacts(args.dry_run, log)
            generate_main_readme(args.dry_run, log)

    except Exception as ex:
        log.error(f"Pipeline aborted due to unhandled exception: {ex}")
        log_path = log.write_log_file()
        if log_path:
            print(f"Log file: {log_path}")
        return 1

    print("\n=== RUN SUMMARY ===")
    for key, value in log.stats.__dict__.items():
        print(f"{key}: {value}")

    print("\n=== MOD CHANGES ===")
    for line in log.get_mod_change_summary_lines():
        print(line)

    log_path = log.write_log_file()
    if log_path:
        print(f"Log file: {log_path}")

    return 0 if log.stats.errors == 0 else 1


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Main AGF mod automation pipeline")
    parser.add_argument(
        "--mode",
        choices=["full", "sync-work", "prep-work", "promote", "package"],
        default="full",
        help="Workflow mode: full pipeline, staging sync, prep active-build readmes, promote, or package output",
    )
    parser.add_argument("--dry-run", action="store_true", help="Preview actions without writing changes")
    parser.add_argument("--verbose", action="store_true", help="Show INFO logs in console")
    parser.add_argument("--strict", action="store_true", help="Fail when required directories/templates are missing")
    parser.add_argument(
        "--workers",
        type=int,
        default=max(2, min(8, os.cpu_count() or 2)),
        help="Worker count for parallel mod zip creation",
    )
    return parser


if __name__ == "__main__":
    arg_parser = build_arg_parser()
    cli_args = arg_parser.parse_args()
    sys.exit(run_pipeline(cli_args))
