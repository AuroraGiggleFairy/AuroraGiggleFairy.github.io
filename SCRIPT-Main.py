import argparse
import csv
import datetime as dt
import hashlib
import json
import os
import re
import shutil
import sys
import threading
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

COMPAT_CSV = os.path.join(VS_CODE_ROOT, "HELPER_ModCompatibility.csv")
MOD_README_TEMPLATE = os.path.join(VS_CODE_ROOT, "TEMPLATE-ModReadMes.md")
MAIN_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "TEMPLATE-MainReadMe.md")
MAIN_README_PATH = os.path.join(VS_CODE_ROOT, "README.md")

AGF_PREFIXES = ("AGF-", "zzzAGF-")
BASE_DOWNLOAD_URL = "https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips"
BACKPACK_DEFAULT_ACTIVE_TOKEN = "084Slots"
GAME_OPTIONALS_BACKPACK_DIR = ".Optionals-Backpack"
GAME_OPTIONALS_HUDPLUS_DIR = ".Optionals-HUDPlus"
RELEASE_META_DIR_NAME = ".release"
GIGGLEPACK_CANONICAL_ZIP = "00_GigglePack_All.zip"
GIGGLEPACK_LATEST_ZIP = "AGF-GigglePack-latest.zip"
GIGGLEPACK_VERSIONED_ZIP_PREFIX = "AGF-GigglePack-v"
GIGGLEPACK_MAJOR_BUMP_MARKER = "gigglepack-major-bump.txt"
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
                    log.stats.sync_conflicts += 1
                    log.warn(
                        f"Version tie conflict for {base_name}: both v{ws_ver} but content differs. "
                        "No overwrite performed."
                    )
            except Exception as ex:
                log.warn(f"Could not hash compare tied versions for {base_name}: {ex}")

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
    rows.sort(key=lambda r: r.get("MOD_NAME", "").lower())
    if dry_run:
        log.info(f"[DRYRUN] Would write compatibility CSV: {COMPAT_CSV} ({len(rows)} rows)")
        return

    with open(COMPAT_CSV, "w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def normalize_compat_csv(folder_renames: List[Tuple[str, str, str]], dry_run: bool, log: Logger) -> List[Dict[str, str]]:
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

    mods_now: set[str] = set()
    for mod_dir in (PUBLISH_READY, IN_PROGRESS):
        for folder_name in scan_mod_folders(mod_dir):
            mods_now.add(get_base_mod_name(folder_name))

    for row in rows:
        old = row.get("MOD_NAME", "")
        if old in rename_base_map:
            row["MOD_NAME"] = rename_base_map[old]

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
            if content.strip().lower() == "none":
                if dry_run:
                    log.info(f"[DRYRUN] Would blank quote file containing 'None': {quote_path}")
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
            readme_content = readme_content.replace("{{MOD_VERSION}}", mod_version)
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
    _, csv_rows = load_compat_csv()
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

    for mod_name, ws_path in mods_pulled_from_game:
        if not os.path.exists(ws_path) and not dry_run:
            log.warn(f"Pushback skipped for {mod_name}: workspace path missing {ws_path}")
            continue

        dest_path = os.path.join(GAME_MODS, os.path.basename(ws_path))
        if maybe_remove_dir(dest_path, dry_run, log) and maybe_copytree(ws_path, dest_path, dry_run, log):
            log.stats.pushed_back_to_game += 1
            log.info(f"Pushback complete: {mod_name}")


def sync_staging_and_game(dry_run: bool, log: Logger) -> None:
    """Sync only between staging and game lanes.

    This mode is for day-to-day test syncs and does not touch PublishReady.
    """
    log.info("Mode sync-work: Sync Staging <-> Game by version")
    log.info(
        "Policy: keep one active BackpackPlus in game root, keep all BackpackPlus in .Optionals-Backpack, "
        "and mirror all HUDPlus/HUDPluszOther in .Optionals-HUDPlus (without forcing HUDPluszOther root removal)."
    )

    staging_folders = scan_mod_folders(STAGING)
    game_folders = scan_mod_folders(GAME_MODS)

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

    # Keep game root clean for backpack policy only.
    for game_folder, game_path in game_folders.items():
        if is_backpack_mod(game_folder) and active_backpack and game_folder != active_backpack:
            if maybe_remove_dir(game_path, dry_run, log):
                log.info(f"sync-work cleanup: removed non-active backpack from game root: {game_folder}")

    # Re-scan after cleanup decisions for consistent sync maps.
    game_folders = scan_mod_folders(GAME_MODS)

    # Sync allowed game-root mods by version.
    allowed_staging_root: Dict[str, str] = {}
    for folder, path in staging_folders.items():
        if is_backpack_mod(folder) and active_backpack and folder != active_backpack:
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

        if st_ver is None or game_ver is None:
            log.warn(
                f"Skipping sync-work for {base_name}: missing/unreadable ModInfo.xml "
                f"(staging={st_ver}, game={game_ver})"
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
                    log.stats.sync_conflicts += 1
                    log.warn(
                        f"sync-work conflict for {base_name}: both v{st_ver} but content differs. "
                        "No overwrite performed."
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

    # Mirror optionals folders in game space.
    optionals_backpack_path = os.path.join(GAME_MODS, GAME_OPTIONALS_BACKPACK_DIR)
    optionals_hudplus_path = os.path.join(GAME_MODS, GAME_OPTIONALS_HUDPLUS_DIR)
    if dry_run:
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_backpack_path}")
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_hudplus_path}")
    else:
        os.makedirs(optionals_backpack_path, exist_ok=True)
        os.makedirs(optionals_hudplus_path, exist_ok=True)

    for folder, st_path in staging_folders.items():
        if is_backpack_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_backpack_path, folder), dry_run, log):
                log.info(f"sync-work mirror: backpack optional updated: {folder}")
            continue
        if is_hudplus_mod(folder) or is_hudpluszother_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_hudplus_path, folder), dry_run, log):
                log.info(f"sync-work mirror: HUD optional updated: {folder}")

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

        if base_name not in publish_by_base:
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
            log.info(f"Promote skipped for {st_folder}: same version as publish-ready ({st_ver})")
        else:
            log.warn(
                f"Promote skipped for {st_folder}: staging version {st_ver} is lower than publish-ready {pub_ver}"
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


def load_json_file(path: str) -> Dict[str, object]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
        return data if isinstance(data, dict) else {}
    except Exception:
        return {}


def write_text_file(path: str, content: str, dry_run: bool, log: Logger) -> None:
    if dry_run:
        log.info(f"[DRYRUN] Would write file: {path}")
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


def generate_gigglepack_release_artifacts(dry_run: bool, log: Logger) -> None:
    """Create versioned/latest GigglePack zips and release notes for Discord/GitHub usage."""
    log.info("Step 6.5: Generate GigglePack release metadata + changelog outputs")

    canonical_zip_path = os.path.join(ZIP_OUTPUT, GIGGLEPACK_CANONICAL_ZIP)
    if not os.path.isfile(canonical_zip_path) and not dry_run:
        log.warn(f"GigglePack release metadata skipped: missing {canonical_zip_path}")
        return

    release_meta_dir = os.path.join(ZIP_OUTPUT, RELEASE_META_DIR_NAME)
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
        return

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

    is_baseline_release = not prev_state

    if is_baseline_release:
        release_version = "1.0.0"
    elif major_bump_requested:
        prev_major, _, _ = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major + 1, 0, 0)
    elif added_mods:
        prev_major, prev_minor, _ = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major, prev_minor + 1, 0)
    elif updated_existing_mods or removed_mods:
        prev_major, prev_minor, prev_patch = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major, prev_minor, prev_patch + 1)
    else:
        release_version = prev_version

    versioned_zip_name = f"{GIGGLEPACK_VERSIONED_ZIP_PREFIX}{release_version}.zip"
    versioned_zip_path = os.path.join(ZIP_OUTPUT, versioned_zip_name)
    latest_zip_path = os.path.join(ZIP_OUTPUT, GIGGLEPACK_LATEST_ZIP)

    if dry_run:
        log.info(f"[DRYRUN] Would copy {GIGGLEPACK_CANONICAL_ZIP} -> {versioned_zip_name}")
        log.info(f"[DRYRUN] Would copy {GIGGLEPACK_CANONICAL_ZIP} -> {GIGGLEPACK_LATEST_ZIP}")
    else:
        shutil.copy2(canonical_zip_path, versioned_zip_path)
        shutil.copy2(canonical_zip_path, latest_zip_path)

    new_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "to": new_ver}
        for mod_name, new_ver in added_mods
    ]
    updated_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "from": old_ver, "to": new_ver}
        for mod_name, old_ver, new_ver in updated_existing_mods
    ]
    removed_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "from": old_ver}
        for mod_name, old_ver in removed_mods
    ]

    # v1.0.0 special baseline: keep changelog intentionally focused and readable.
    if release_version == "1.0.0":
        focused_entries: List[Dict[str, str]] = []
        for mod_name in GIGGLEPACK_V100_FOCUS_MODS:
            if mod_name in giggle_mod_versions:
                focused_entries.append({"mod": mod_name, "to": giggle_mod_versions[mod_name]})
        if focused_entries:
            new_mod_entries = focused_entries
            updated_mod_entries = []
            removed_mod_entries = []

    new_mod_lines = [f"- {entry['mod']} (new: v{entry['to']})" for entry in new_mod_entries]
    updated_existing_lines = [
        f"- {entry['mod']} (v{entry['from']} -> v{entry['to']})"
        for entry in updated_mod_entries
    ]
    removed_lines = [f"- {entry['mod']} (was v{entry['from']})" for entry in removed_mod_entries]
    now_iso = dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    def capped(lines: List[str], cap: int) -> List[str]:
        if len(lines) <= cap:
            return lines
        return lines[:cap] + [f"- ... and {len(lines) - cap} more"]

    new_mod_lines_short = capped(new_mod_lines, 15)
    updated_existing_lines_short = capped(updated_existing_lines, 20)
    removed_lines_short = capped(removed_lines, 10)

    discord_chunks: List[str] = [
        f"- GigglePack v{release_version}",
        f"  - Released: {now_iso}",
        f"  - Download (versioned): {zip_download_link(versioned_zip_name)}",
        f"  - Download (latest): {zip_download_link(GIGGLEPACK_LATEST_ZIP)}",
        f"  - Change summary: +{len(new_mod_lines)} new, ~{len(updated_existing_lines)} updated, -{len(removed_lines)} removed",
        "",
        "Version rules:",
        "- X increases for major game-cycle updates (manual marker trigger).",
        "- Y increases when NEW mods are added to GigglePack.",
        "- Z increases when existing GigglePack mods are updated.",
        "",
    ]
    if release_version == "1.0.0":
        discord_chunks.append("  - New mods:")
        for line in new_mod_lines_short:
            discord_chunks.append(f"    {line}")
    elif is_baseline_release:
        discord_chunks.append(
            f"Baseline release snapshot created with {len(giggle_mod_versions)} mods. "
            "Detailed per-mod baseline list omitted for readability."
        )
    else:
        if new_mod_lines_short:
            discord_chunks.append("New mods since previous GigglePack release:")
            discord_chunks.extend(new_mod_lines_short)
            discord_chunks.append("")

        if updated_existing_lines_short:
            discord_chunks.append("Updated existing mods since previous GigglePack release:")
            discord_chunks.extend(updated_existing_lines_short)
            discord_chunks.append("")

        if removed_lines_short:
            discord_chunks.append("Removed from GigglePack since previous release:")
            discord_chunks.extend(removed_lines_short)

        if not (new_mod_lines_short or updated_existing_lines_short or removed_lines_short):
            discord_chunks.append("No mod changes since previous GigglePack release.")

    discord_text = "\n".join(discord_chunks).strip() + "\n"

    markdown_lines: List[str] = [
        f"# GigglePack v{release_version}",
        "",
        f"- Released: {now_iso}",
        f"- Versioned: {zip_download_link(versioned_zip_name)}",
        f"- Latest: {zip_download_link(GIGGLEPACK_LATEST_ZIP)}",
        "",
        "## Version Rules",
        "- X increases for major game-cycle updates (manual marker trigger).",
        "- Y increases when NEW mods are added to GigglePack.",
        "- Z increases when existing GigglePack mods are updated.",
        "",
        "## Change Summary",
        f"- New mods: {len(new_mod_lines)}",
        f"- Updated existing mods: {len(updated_existing_lines)}",
        f"- Removed mods: {len(removed_lines)}",
        "",
        "## New Mods",
    ]
    if release_version == "1.0.0":
        markdown_lines.extend(new_mod_lines_short)
    elif is_baseline_release:
        markdown_lines.append(
            f"- Baseline release snapshot with {len(giggle_mod_versions)} mods. "
            "Detailed per-mod baseline list intentionally omitted."
        )
    elif new_mod_lines_short:
        markdown_lines.extend(new_mod_lines_short)
    else:
        markdown_lines.append("- No new mods in this release.")

    markdown_lines.append("")
    markdown_lines.append("## Updated Existing Mods")
    if is_baseline_release:
        markdown_lines.append("- Baseline release (no previous release to compare existing updates).")
    elif updated_existing_lines_short:
        markdown_lines.extend(updated_existing_lines_short)
    else:
        markdown_lines.append("- No existing mod version increases in this release.")

    if removed_lines:
        markdown_lines.append("")
        markdown_lines.append("## Removed Mods")
        markdown_lines.extend(removed_lines_short)

    markdown_text = "\n".join(markdown_lines).strip() + "\n"

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
        "removed_mods": removed_mod_entries,
        "versioned_zip": versioned_zip_name,
        "latest_zip": GIGGLEPACK_LATEST_ZIP,
        "change_counts": {
            "new_mods": len(new_mod_entries),
            "updated_existing_mods": len(updated_mod_entries),
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
    desc = extract_mod_description_from_modinfo(modinfo_path)
    link = zip_download_link(f"{get_base_mod_name(folder_name)}.zip")

    features = extract_readme_block(readme_path, "<!-- FEATURES START -->", "<!-- FEATURES END -->").strip("\n")

    block = [f"> ### **{name}** *-v{version}* - [Download]({link})", f"> *{desc}*"]
    if features:
        features_html = markdown_features_to_html(features)
        block.append(
            "> <details> <summary>*Show detailed features*</summary>\n>\n"
            f"> {features_html}\n>\n> </details>"
        )
    block.append("> \n---\n")
    return "\n".join(block)


def load_gigglepack_release_state() -> Dict[str, object]:
    state_path = os.path.join(ZIP_OUTPUT, RELEASE_META_DIR_NAME, "gigglepack-release-state.json")
    return load_json_file(state_path)


def build_gigglepack_readme_release_lines(state: Dict[str, object]) -> List[str]:
    if not state:
        return []

    release_version = str(state.get("gigglepack_version", "")).strip() or "unknown"
    previous_release_version = str(state.get("previous_gigglepack_version", "")).strip()
    is_baseline_release = bool(state.get("is_baseline_release", False))
    released_at = str(state.get("released_at", "")).strip()
    new_mods = state.get("new_mods", [])
    updated_mods = state.get("updated_mods", [])
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

    lines: List[str] = []
    release_stamp = format_release_stamp(released_at)
    header_line = f">  - GigglePack v{release_version}"
    if release_stamp:
        header_line += f" - *{release_stamp}*"

    lines.extend([
        ">",
        "> <details><summary><i>Changelog</i></summary>",
        ">",
        header_line,
    ])

    new_count = int(change_counts.get("new_mods", len(new_mods)))
    updated_count = int(change_counts.get("updated_existing_mods", len(updated_mods)))
    removed_count = int(change_counts.get("removed_mods", len(removed_mods)))
    lines.append(f">     - Change summary: +{new_count} new, ~{updated_count} updated, -{removed_count} removed")

    if release_version == "1.0.0":
        lines.append(">     - New mods:")
        if isinstance(new_mods, list):
            for entry in new_mods[:10]:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    to_ver = str(entry.get("to", "")).strip()
                    if mod_name:
                        lines.append(f">         - {mod_name} (new: v{to_ver})")
        lines.extend([">", "> </details>"])
        return lines

    if is_baseline_release:
        lines.append(">     - Baseline release snapshot: detailed per-mod list hidden for readability.")
        lines.extend([">", "> </details>"])
        return lines

    if previous_release_version:
        lines.append(f">     - Previous GigglePack version: v{previous_release_version}")

    if isinstance(new_mods, list) and new_mods:
        lines.append(">     - New mods since previous GigglePack release:")
        for entry in new_mods[:12]:
            if isinstance(entry, dict):
                mod_name = str(entry.get("mod", "")).strip()
                to_ver = str(entry.get("to", "")).strip()
                if mod_name:
                    lines.append(f">         - {mod_name} (new: v{to_ver})")
        if len(new_mods) > 12:
            lines.append(f">         - ... and {len(new_mods) - 12} more")

    if isinstance(updated_mods, list) and updated_mods:
        lines.append(">     - Updated existing mods since previous GigglePack release:")
        for entry in updated_mods[:15]:
            if isinstance(entry, dict):
                mod_name = str(entry.get("mod", "")).strip()
                from_ver = str(entry.get("from", "")).strip()
                to_ver = str(entry.get("to", "")).strip()
                if not mod_name:
                    continue
                lines.append(f">         - {mod_name} (v{from_ver} -> v{to_ver})")
        if len(updated_mods) > 15:
            lines.append(f">         - ... and {len(updated_mods) - 15} more")
    else:
        lines.append(">     - No existing mod version increases since previous GigglePack release.")

    if isinstance(removed_mods, list) and removed_mods:
        lines.append(">")
        lines.append(">     - Removed from GigglePack:")
        for entry in removed_mods[:10]:
            if isinstance(entry, dict):
                mod_name = str(entry.get("mod", "")).strip()
                from_ver = str(entry.get("from", "")).strip()
                if mod_name:
                    lines.append(f">         - {mod_name} (was v{from_ver})")
        if len(removed_mods) > 10:
            lines.append(f">         - ... and {len(removed_mods) - 10} more")

    lines.extend([">", "> </details>"])

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
            desc = extract_mod_description_from_modinfo(modinfo_path)
            link = zip_download_link(f"{get_base_mod_name(mod)}.zip")
            md.append(f"| {name} | {version} | [Download]({link}) | {desc} |")
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
        required_dirs = [PUBLISH_READY, IN_PROGRESS, GAME_MODS]
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
            sync_staging_and_game(args.dry_run, log)
        elif args.mode == "prep-work":
            prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
        elif args.mode == "promote":
            prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            promote_staging_to_publish_ready(args.dry_run, log)
        elif args.mode == "package":
            prep_names_and_readmes_for_dirs((PUBLISH_READY,), args.dry_run, log)
            create_all_zips(args.dry_run, args.workers, log)
            generate_gigglepack_release_artifacts(args.dry_run, log)
            generate_main_readme(args.dry_run, log)
        else:
            mods_pulled = sync_workspace_and_game(args.dry_run, log)
            move_mods_by_major_version(args.dry_run, log)

            folder_renames = rename_mod_folders_to_modinfo(args.dry_run, log)
            csv_rows = normalize_compat_csv(folder_renames, args.dry_run, log)
            normalize_quote_files(csv_rows, folder_renames, args.dry_run, log)
            generate_mod_readmes(csv_rows, args.dry_run, log)

            push_back_pulled_mods(mods_pulled, args.dry_run, log)

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
