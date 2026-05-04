import argparse
import csv
import datetime as dt
import difflib
import hashlib
import io
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
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
VS_CODE_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
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
MOD_TYPE_DESCRIPTIONS_TEMPLATE = os.path.join(VS_CODE_ROOT, "TEMPLATE-ModTypeDescriptions.md")
MAIN_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "TEMPLATE-MainReadMe.md")
MAIN_MOD_CATEGORY_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "TEMPLATE-MainReadMe-1ModCategory")
MAIN_MOD_ENTRY_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "TEMPLATE-MainReadMe-2ModEntry")
GIGGLE_PACK_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "TEMPLATE-MainReadMe-0GigglePack")
CATEGORY_DESCRIPTIONS_PATH = os.path.join(VS_CODE_ROOT, "TEMPLATE-CategoryDescriptions.md")
IMAGES_ROOT = os.path.join(VS_CODE_ROOT, "00_Images")
IMAGES_SOURCE_ROOT = os.path.join(IMAGES_ROOT, "source")
IMAGES_GENERATED_ROOT = os.path.join(IMAGES_ROOT, "_generated")
DISCORD_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "05_GigglePackReleaseData", "Discord", "TEMPLATE-DiscordUpdate.md")
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
GIGGLEPACK_PENDING_CHANGES_PATH = os.path.join(
    GIGGLEPACK_RELEASE_ROOT_DIR,
    "gigglepack-pending-changes.json",
)
RUN_LOCK_PATH = os.path.join(VS_CODE_ROOT, ".script-main.lock")
RUN_MANIFEST_MAX_FILES = 20
GAME_REMOVALS_QUARANTINE_DIR = os.path.join(VS_CODE_ROOT, "_Quarantine-GameRemovals")
TRANSACTION_ROLLBACK_DIR = os.path.join(VS_CODE_ROOT, "_TransactionRollback")
GIGGLEPACK_CANONICAL_ZIP = "00_GigglePack_All.zip"
GIGGLEPACK_VERSIONED_ZIP_PREFIX = "AGF-GigglePack-v"
GIGGLEPACK_MAJOR_BUMP_MARKER = "gigglepack-major-bump.txt"
DISCORD_WEBHOOK_ENV_VAR = "AGF_DISCORD_WEBHOOK_URL"
GIGGLEPACK_V100_FOCUS_MODS = (
    "AGF-NoEAC-ExpandedInteractionPrompts",
    "AGF-NoEAC-ScreamerAlert",
)
README_COMPAT_FIELDS = (
    "TESTED_GAME_VERSION",
    "EAC_FRIENDLY",
    "SERVER_SIDE_PLAYER",
    "SERVER_SIDE_DEDICATED",
    "CLIENT_SIDE",
    "MOD_TYPE_ID",
    "SAFE_TO_INSTALL",
    "SAFE_TO_REMOVE",
    "UNIQUE",
    "QUOTE_FILE",
)

COMPAT_CSV_FIELD_ORDER = [
    "MOD_NAME",
    "MOD_TYPE_ID",
    "TESTED_GAME_VERSION",
    "SAFE_TO_INSTALL",
    "SAFE_TO_REMOVE",
    "UNIQUE",
    "QUOTE_FILE",
    "EAC_FRIENDLY",
    "SERVER_SIDE_PLAYER",
    "SERVER_SIDE_DEDICATED",
    "CLIENT_SIDE",
]

DEFAULT_MOD_TYPE_LINE_BY_ID = {
    "1": "Server-side (EAC-friendly): Server install works for all joining players; EAC on or off.",
    "2": "Server-side (EAC Off): EAC off required; server install works for all joining players.",
    "3": "Server/Client-side (Required): EAC off required; host and joining players must install it.",
    "4": "Client-side (Only): EAC off required; server install has no effect; only the installing player gets the feature.",
    "5": "Hybrid: EAC off required; server install works for all joining players; client install is optional for extra features.",
    "6": "Server-side (Dedicated Only, EAC Off): EAC off required; dedicated uses server install only, but player-hosted requires host and joining players to install the mod.",
}

MOD_TYPE_COMPAT_BY_ID = {
    "1": {
        "EAC_FRIENDLY": "Yes",
        "SERVER_SIDE_PLAYER": "Yes",
        "SERVER_SIDE_DEDICATED": "Yes",
        "CLIENT_SIDE": "None",
    },
    "2": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "Yes",
        "SERVER_SIDE_DEDICATED": "Yes",
        "CLIENT_SIDE": "None",
    },
    "3": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "No",
        "SERVER_SIDE_DEDICATED": "No",
        "CLIENT_SIDE": "Required",
    },
    "4": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "N/A",
        "SERVER_SIDE_DEDICATED": "N/A",
        "CLIENT_SIDE": "Only",
    },
    "5": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "Hybrid",
        "SERVER_SIDE_DEDICATED": "Hybrid",
        "CLIENT_SIDE": "Optional",
    },
    "6": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "No",
        "SERVER_SIDE_DEDICATED": "Yes",
        "CLIENT_SIDE": "Required",
    },
}

FAIL_FAST_ENABLED = True


@dataclass
class RunTransaction:
    enabled: bool
    rollback_root: str
    actions: List[Dict[str, str]] = field(default_factory=list)
    rolling_back: bool = False


CURRENT_TRANSACTION: Optional[RunTransaction] = None


def load_mod_type_lines_from_template(log: "Logger") -> Dict[str, str]:
    """Load MOD_TYPE_ID wording from TEMPLATE-ModTypeDescriptions.md.

    Expected format under the heading "Mod Types with Simple Descriptions":
    `1 Server-side ...`
    `2 Server-side ...`
    etc.
    """
    if not os.path.exists(MOD_TYPE_DESCRIPTIONS_TEMPLATE):
        log.warn(
            "Missing TEMPLATE-ModTypeDescriptions.md; using built-in MOD_TYPE_ID wording defaults"
        )
        return dict(DEFAULT_MOD_TYPE_LINE_BY_ID)

    try:
        with open(MOD_TYPE_DESCRIPTIONS_TEMPLATE, "r", encoding="utf-8") as f:
            text = f.read()
    except Exception as ex:
        log.warn(
            f"Failed to read {MOD_TYPE_DESCRIPTIONS_TEMPLATE}: {ex}; using built-in MOD_TYPE_ID wording defaults"
        )
        return dict(DEFAULT_MOD_TYPE_LINE_BY_ID)

    heading_match = re.search(
        r"^###\s+Mod Types with Simple Descriptions\s*$",
        text,
        flags=re.MULTILINE,
    )
    if not heading_match:
        log.warn(
            "Could not find 'Mod Types with Simple Descriptions' heading; using built-in MOD_TYPE_ID wording defaults"
        )
        return dict(DEFAULT_MOD_TYPE_LINE_BY_ID)

    section_text = text[heading_match.end():]
    parsed: Dict[str, str] = {}
    for raw_line in section_text.splitlines():
        line = raw_line.strip()
        if not line:
            continue
        if line.startswith("### "):
            break

        entry_match = re.match(r"^(\d+)\s+(.+)$", line)
        if entry_match:
            mod_type_id, wording = entry_match.groups()
            parsed[mod_type_id.strip()] = wording.strip()

    if not parsed:
        log.warn(
            "No MOD_TYPE_ID lines parsed from TEMPLATE-ModTypeDescriptions.md; using built-in wording defaults"
        )
        return dict(DEFAULT_MOD_TYPE_LINE_BY_ID)

    return parsed


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
        self._action_needed: List[str] = []
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

    def action_needed(self, message: str) -> None:
        with self._lock:
            if message not in self._action_needed:
                self._action_needed.append(message)
        self.warn(f"ACTION NEEDED: {message}")

    def get_action_needed_lines(self) -> List[str]:
        with self._lock:
            return list(self._action_needed)

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
            lines: List[str] = []
            lines.extend(self._messages)
            lines.append("")
            lines.append("=== SUMMARY ===")
            for key, value in self.stats.__dict__.items():
                lines.append(f"{key}: {value}")
            lines.append("")
            lines.append("=== MOD CHANGES ===")
            lines.extend(self.get_mod_change_summary_lines())
            atomic_write_text(path, "\n".join(lines) + "\n", encoding="utf-8")

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


def atomic_write_text(path: str, content: str, encoding: str = "utf-8", newline: Optional[str] = None) -> None:
    """Write file content atomically to reduce partial-write corruption risk."""
    target_dir = os.path.dirname(path)
    if target_dir:
        os.makedirs(target_dir, exist_ok=True)

    fd, tmp_path = tempfile.mkstemp(prefix=".tmp-", suffix=".tmp", dir=target_dir or None)
    try:
        with os.fdopen(fd, "w", encoding=encoding, newline=newline) as f:
            f.write(content)
        os.replace(tmp_path, path)
    except Exception:
        try:
            os.remove(tmp_path)
        except Exception:
            pass
        raise


def atomic_write_json(path: str, payload: Dict[str, object], ensure_ascii: bool = False, indent: int = 2) -> None:
    text = json.dumps(payload, ensure_ascii=ensure_ascii, indent=indent)
    atomic_write_text(path, text + "\n", encoding="utf-8")


def is_path_within(path: str, parent: str) -> bool:
    try:
        return os.path.commonpath([os.path.abspath(path), os.path.abspath(parent)]) == os.path.abspath(parent)
    except Exception:
        return False


def remove_dir_force(path: str) -> None:
    if not os.path.exists(path):
        return
    if os.path.isdir(path):
        shutil.rmtree(path)
    else:
        os.remove(path)


def create_transaction(enabled: bool) -> Optional[RunTransaction]:
    if not enabled:
        return None
    stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
    tx_root = os.path.join(TRANSACTION_ROLLBACK_DIR, f"tx-{stamp}-{os.getpid()}")
    os.makedirs(tx_root, exist_ok=True)
    return RunTransaction(enabled=True, rollback_root=tx_root)


def transaction_snapshot_dir(path: str) -> str:
    if not CURRENT_TRANSACTION or not CURRENT_TRANSACTION.enabled:
        raise RuntimeError("Transaction snapshot requested without active transaction")
    snap_id = len(CURRENT_TRANSACTION.actions) + 1
    backup_path = os.path.join(CURRENT_TRANSACTION.rollback_root, f"snapshot-{snap_id}")
    shutil.copytree(path, backup_path)
    return backup_path


def rollback_transaction(log: Logger) -> None:
    if not CURRENT_TRANSACTION or not CURRENT_TRANSACTION.enabled:
        return

    txn = CURRENT_TRANSACTION
    txn.rolling_back = True
    rollback_failures = 0
    log.warn("Attempting transactional rollback of filesystem changes")

    for action in reversed(txn.actions):
        kind = action.get("kind", "")
        try:
            if kind == "undo_copy":
                dst = action.get("dst", "")
                dst_backup = action.get("dst_backup", "")
                if dst and os.path.exists(dst):
                    remove_dir_force(dst)
                if dst and dst_backup and os.path.exists(dst_backup):
                    os.makedirs(os.path.dirname(dst), exist_ok=True)
                    shutil.copytree(dst_backup, dst)
            elif kind == "undo_move":
                src = action.get("src", "")
                dst = action.get("dst", "")
                dst_backup = action.get("dst_backup", "")
                if dst and os.path.exists(dst):
                    os.makedirs(os.path.dirname(src), exist_ok=True)
                    if os.path.exists(src):
                        remove_dir_force(src)
                    shutil.move(dst, src)
                if dst and dst_backup and os.path.exists(dst_backup):
                    os.makedirs(os.path.dirname(dst), exist_ok=True)
                    shutil.copytree(dst_backup, dst)
            elif kind == "restore_removed":
                path = action.get("path", "")
                backup = action.get("backup", "")
                if path and os.path.exists(path):
                    remove_dir_force(path)
                if path and backup and os.path.exists(backup):
                    os.makedirs(os.path.dirname(path), exist_ok=True)
                    shutil.copytree(backup, path)
            elif kind == "restore_quarantined":
                path = action.get("path", "")
                quarantine = action.get("quarantine", "")
                if path and os.path.exists(path):
                    remove_dir_force(path)
                if path and quarantine and os.path.exists(quarantine):
                    os.makedirs(os.path.dirname(path), exist_ok=True)
                    shutil.move(quarantine, path)
        except Exception as ex:
            rollback_failures += 1
            log.error(f"Rollback action failed ({kind}): {ex}")

    txn.rolling_back = False
    if rollback_failures == 0:
        log.info("Transactional rollback completed successfully")
    else:
        log.error(f"Transactional rollback finished with {rollback_failures} failure(s)")


def finalize_transaction(success: bool, log: Logger) -> None:
    global CURRENT_TRANSACTION
    if not CURRENT_TRANSACTION:
        return
    txn = CURRENT_TRANSACTION
    if not success:
        rollback_transaction(log)
    try:
        if os.path.isdir(txn.rollback_root):
            shutil.rmtree(txn.rollback_root)
    except Exception as ex:
        log.warn(f"Could not clean transaction temp dir {txn.rollback_root}: {ex}")
    CURRENT_TRANSACTION = None


def cleanup_game_quarantine(retention_days: int, dry_run: bool, log: Logger) -> None:
    if retention_days < 0:
        return
    if not os.path.isdir(GAME_REMOVALS_QUARANTINE_DIR):
        return

    cutoff = dt.datetime.now().timestamp() - (retention_days * 86400)
    for entry in os.listdir(GAME_REMOVALS_QUARANTINE_DIR):
        entry_path = os.path.join(GAME_REMOVALS_QUARANTINE_DIR, entry)
        if not os.path.isdir(entry_path):
            continue
        try:
            mtime = os.path.getmtime(entry_path)
        except Exception:
            continue
        if mtime >= cutoff:
            continue
        if dry_run:
            log.info(f"[DRYRUN] Would remove old game quarantine entry: {entry_path}")
        else:
            try:
                shutil.rmtree(entry_path)
                log.info(f"Removed old game quarantine entry: {entry_path}")
            except Exception as ex:
                log.warn(f"Failed to remove old game quarantine entry {entry_path}: {ex}")


def validate_agf_rows_in_csv(log: Logger) -> bool:
    if not os.path.isfile(COMPAT_CSV):
        return True

    _, rows = load_compat_csv()
    invalid_rows: List[str] = []
    for row in rows:
        mod_name = row.get("MOD_NAME", "").strip()
        if mod_name and not is_agf_mod(mod_name):
            invalid_rows.append(mod_name)

    if not invalid_rows:
        return True

    preview = ", ".join(sorted(set(invalid_rows))[:8])
    extra = "" if len(set(invalid_rows)) <= 8 else f" (+{len(set(invalid_rows)) - 8} more)"
    log.error(
        "Non-AGF rows found in HELPER_ModCompatibility.csv. "
        f"Remove these rows before running: {preview}{extra}"
    )
    return False


def check_directory_write_access(path: str) -> Tuple[bool, str]:
    if not os.path.isdir(path):
        return False, "directory missing"
    try:
        fd, probe_path = tempfile.mkstemp(prefix=".write-test-", dir=path)
        os.close(fd)
        os.remove(probe_path)
        return True, ""
    except Exception as ex:
        return False, str(ex)


def run_writeability_preflight(mode: str, dry_run: bool, log: Logger) -> bool:
    probe_dirs: List[str] = [LOGS_DIR, os.path.dirname(COMPAT_CSV), QUOTES_DIR]

    if mode in ("update", "full", "sync-work"):
        probe_dirs.extend([STAGING, GAME_MODS])
    if mode in ("update", "full"):
        probe_dirs.append(IN_PROGRESS)
    if mode in ("promote", "full", "package"):
        probe_dirs.append(PUBLISH_READY)
    if mode in ("package", "full"):
        probe_dirs.extend([ZIP_OUTPUT, GIGGLEPACK_RELEASE_ROOT_DIR])

    seen: set[str] = set()
    failures: List[str] = []
    for directory in probe_dirs:
        normalized = os.path.normpath(directory)
        if normalized in seen:
            continue
        seen.add(normalized)

        ok, reason = check_directory_write_access(normalized)
        if not ok:
            failures.append(f"{normalized}: {reason}")

    if not failures:
        log.info("Preflight writeability check passed")
        return True

    for item in failures:
        if dry_run:
            log.warn(f"Preflight writeability issue (dry-run only): {item}")
        else:
            log.error(f"Preflight writeability issue: {item}")

    return dry_run


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


def has_version_drift(folder_name: str, mod_version: str) -> bool:
    version_match = re.search(r"-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
    folder_version = version_match.group(1) if version_match else ""
    return folder_version != (mod_version or "")


def is_notepadpp_running() -> bool:
    try:
        result = subprocess.run(
            ["tasklist", "/FI", "IMAGENAME eq notepad++.exe"],
            capture_output=True,
            text=True,
            check=False,
        )
        output = (result.stdout or "") + "\n" + (result.stderr or "")
        return "notepad++.exe" in output.lower()
    except Exception:
        return False


def extract_folder_version(folder_name: str) -> str:
    version_match = re.search(r"-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
    return version_match.group(1) if version_match else ""


def ensure_notepadpp_closed_for_version_bumps(
    folder_renames: List[Tuple[str, str, str]],
    dry_run: bool,
    log: Logger,
) -> bool:
    """When version bumps are about to be applied, prompt user to close Notepad++ if running."""
    has_version_bump = any(
        extract_folder_version(old) != extract_folder_version(new) for old, new, _ in folder_renames
    )
    if not has_version_bump:
        return True

    if not is_notepadpp_running():
        return True

    message = (
        "Notepad++ is currently open. It may be locking files that need to be updated. "
        "Please close Notepad++ and press Enter to continue, or type 'skip' to abort."
    )

    if dry_run or not sys.stdin or not sys.stdin.isatty():
        log.warn(message + " Proceeding because this run is non-interactive.")
        return True

    while True:
        print(message)
        try:
            response = input().strip().lower()
        except EOFError:
            response = "skip"

        if response == "skip":
            log.error("Run aborted by user due to Notepad++ pre-flight check for version bumps")
            return False

        if not is_notepadpp_running():
            return True

        log.warn("Notepad++ is still running. Close it, then press Enter, or type 'skip' to abort.")


def ensure_notepadpp_closed_for_game_sync(dry_run: bool, log: Logger) -> bool:
    """Before touching game Mods data, require Notepad++ closure in interactive runs."""
    if not is_notepadpp_running():
        return True

    message = (
        "Notepad++ is currently open. Game-folder sync/push/pull is about to run and may fail due to file locks. "
        "Please close Notepad++ and press Enter to continue, or type 'skip' to abort."
    )

    if dry_run or not sys.stdin or not sys.stdin.isatty():
        log.warn(message + " Proceeding because this run is non-interactive.")
        return True

    while True:
        print(message)
        try:
            response = input().strip().lower()
        except EOFError:
            response = "skip"

        if response == "skip":
            log.error("Run aborted by user due to Notepad++ pre-flight check for game-folder sync")
            return False

        if not is_notepadpp_running():
            return True

        log.warn("Notepad++ is still running. Close it, then press Enter, or type 'skip' to abort.")


def acquire_run_lock() -> bool:
    try:
        fd = os.open(RUN_LOCK_PATH, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
        with os.fdopen(fd, "w", encoding="utf-8") as f:
            f.write(f"pid={os.getpid()}\n")
            f.write(f"started={dt.datetime.now().isoformat()}\n")
        return True
    except FileExistsError:
        return False
    except Exception:
        return False


def release_run_lock() -> None:
    try:
        if os.path.exists(RUN_LOCK_PATH):
            os.remove(RUN_LOCK_PATH)
    except Exception:
        pass


def write_run_manifest(log: Logger, mode: str, dry_run: bool, exit_code: int, log_path: Optional[str]) -> Optional[str]:
    """Write a machine-readable run manifest for audits and troubleshooting."""
    try:
        os.makedirs(LOGS_DIR, exist_ok=True)
        stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
        manifest_path = os.path.join(LOGS_DIR, f"run-manifest-{stamp}.json")
        payload = {
            "timestamp": dt.datetime.now().isoformat(),
            "mode": mode,
            "dry_run": dry_run,
            "exit_code": exit_code,
            "log_path": log_path,
            "stats": log.stats.__dict__,
            "action_needed": log.get_action_needed_lines(),
            "mod_changes": log.get_mod_change_summary_lines(),
        }
        atomic_write_json(manifest_path, payload, ensure_ascii=False, indent=2)

        manifest_candidates: List[Tuple[float, str]] = []
        for name in os.listdir(LOGS_DIR):
            if not (name.startswith("run-manifest-") and name.endswith(".json")):
                continue
            full_path = os.path.join(LOGS_DIR, name)
            if os.path.isfile(full_path):
                manifest_candidates.append((os.path.getmtime(full_path), full_path))

        manifest_candidates.sort(key=lambda item: item[0], reverse=True)
        for _, old_path in manifest_candidates[RUN_MANIFEST_MAX_FILES:]:
            try:
                os.remove(old_path)
            except Exception:
                pass

        return manifest_path
    except Exception:
        return None


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
        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back:
            if is_path_within(path, GAME_MODS):
                rel = os.path.relpath(path, GAME_MODS)
                stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
                quarantine_target = os.path.join(GAME_REMOVALS_QUARANTINE_DIR, stamp, rel)
                os.makedirs(os.path.dirname(quarantine_target), exist_ok=True)
                if os.path.exists(quarantine_target):
                    remove_dir_force(quarantine_target)
                shutil.move(path, quarantine_target)
                CURRENT_TRANSACTION.actions.append(
                    {
                        "kind": "restore_quarantined",
                        "path": path,
                        "quarantine": quarantine_target,
                    }
                )
                log.info(f"Quarantined game folder instead of delete: {path} -> {quarantine_target}")
                return True

            backup_path = transaction_snapshot_dir(path)
            remove_dir_force(path)
            CURRENT_TRANSACTION.actions.append(
                {
                    "kind": "restore_removed",
                    "path": path,
                    "backup": backup_path,
                }
            )
            return True

        if is_path_within(path, GAME_MODS):
            rel = os.path.relpath(path, GAME_MODS)
            stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
            quarantine_target = os.path.join(GAME_REMOVALS_QUARANTINE_DIR, stamp, rel)
            os.makedirs(os.path.dirname(quarantine_target), exist_ok=True)
            if os.path.exists(quarantine_target):
                remove_dir_force(quarantine_target)
            shutil.move(path, quarantine_target)
            log.info(f"Quarantined game folder instead of delete: {path} -> {quarantine_target}")
            return True
        remove_dir_force(path)
        return True
    except Exception as ex:
        log.error(f"Failed to remove directory {path}: {ex}")
        if FAIL_FAST_ENABLED:
            raise RuntimeError(f"Failed to remove directory: {path}") from ex
        return False


def maybe_copytree(src: str, dst: str, dry_run: bool, log: Logger) -> bool:
    if dry_run:
        log.info(f"[DRYRUN] Would copy directory: {src} -> {dst}")
        return True
    try:
        dst_backup = ""
        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back and os.path.exists(dst):
            dst_backup = transaction_snapshot_dir(dst)

        if os.path.exists(dst):
            remove_dir_force(dst)
        shutil.copytree(src, dst)

        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back:
            CURRENT_TRANSACTION.actions.append(
                {
                    "kind": "undo_copy",
                    "dst": dst,
                    "dst_backup": dst_backup,
                }
            )
        return True
    except Exception as ex:
        log.error(f"Failed to copy directory {src} -> {dst}: {ex}")
        if FAIL_FAST_ENABLED:
            raise RuntimeError(f"Failed to copy directory: {src} -> {dst}") from ex
        return False


def maybe_move(src: str, dst: str, dry_run: bool, log: Logger) -> bool:
    if dry_run:
        log.info(f"[DRYRUN] Would move directory: {src} -> {dst}")
        return True
    try:
        dst_backup = ""
        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back and os.path.exists(dst):
            dst_backup = transaction_snapshot_dir(dst)

        if os.path.exists(dst):
            remove_dir_force(dst)
        shutil.move(src, dst)

        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back:
            CURRENT_TRANSACTION.actions.append(
                {
                    "kind": "undo_move",
                    "src": src,
                    "dst": dst,
                    "dst_backup": dst_backup,
                }
            )
        return True
    except Exception as ex:
        log.error(f"Failed to move directory {src} -> {dst}: {ex}")
        if FAIL_FAST_ENABLED:
            raise RuntimeError(f"Failed to move directory: {src} -> {dst}") from ex
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


def sync_game_and_draft(dry_run: bool, log: Logger) -> List[Tuple[str, str]]:
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

    mods_pulled_from_game: List[Tuple[str, str]] = []

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
                mods_pulled_from_game.append((draft_folder, draft_path))
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
                        mods_pulled_from_game.append((draft_folder, draft_path))
                        log.info(
                            f"Draft/game tie refresh for {base_name}: both v{game_ver} but content differed. "
                            "Pulled game copy into Draft."
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare game/draft tie for {base_name}: {ex}")

    return mods_pulled_from_game


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

            folder_base_name = get_base_mod_name(folder_name)
            name_changed = folder_base_name != mod_name
            version_changed = has_version_drift(folder_name, mod_version)

            # Policy: name changes are only applied when accompanied by a version bump/change.
            if name_changed and not version_changed:
                log.warn(
                    f"Rename skipped for {folder_name}: ModInfo Name changed but version did not. "
                    "Bump version to apply name change."
                )
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


def plan_mod_folder_renames(mod_dirs: Tuple[str, ...], log: Logger) -> List[Tuple[str, str, str]]:
    """Plan folder renames without modifying files. Mirrors rename policy and collision guard."""
    planned: List[Tuple[str, str, str]] = []
    for mod_dir in mod_dirs:
        folders = scan_mod_folders(mod_dir)
        for folder_name, folder_path in folders.items():
            modinfo_path = os.path.join(folder_path, "ModInfo.xml")
            if not os.path.exists(modinfo_path):
                continue

            mod_name, mod_version = parse_modinfo(modinfo_path, folder_name)
            if not is_agf_mod(mod_name):
                continue

            folder_base_name = get_base_mod_name(folder_name)
            name_changed = folder_base_name != mod_name
            version_changed = has_version_drift(folder_name, mod_version)

            if name_changed and not version_changed:
                continue

            target_name = f"{mod_name}-v{mod_version}"
            if target_name == folder_name:
                continue

            dst = os.path.join(mod_dir, target_name)
            if os.path.exists(dst):
                continue

            planned.append((folder_name, target_name, mod_dir))

    return planned


def load_compat_csv() -> Tuple[List[str], List[Dict[str, str]]]:
    default_fields = list(COMPAT_CSV_FIELD_ORDER)
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
            normalized = value.strip().lower()
            if "missingdata" in normalized or normalized == "tbd":
                return True
            if fn == "MOD_TYPE_ID" and normalized == "0":
                return True
        return False

    rows.sort(key=lambda r: (0 if row_has_missingdata(r) else 1, r.get("MOD_NAME", "").lower()))
    if dry_run:
        log.info(f"[DRYRUN] Would write compatibility CSV: {COMPAT_CSV} ({len(rows)} rows)")
        return

    csv_buffer = io.StringIO(newline="")
    writer = csv.DictWriter(csv_buffer, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(rows)
    atomic_write_text(COMPAT_CSV, csv_buffer.getvalue(), encoding="utf-8", newline="")


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
    if "TESTED_GAME_VERSION" not in fieldnames:
        insert_at = 1 if "MOD_NAME" in fieldnames else 0
        fieldnames.insert(insert_at, "TESTED_GAME_VERSION")
    if "QUOTE_FILE" not in fieldnames:
        fieldnames.append("QUOTE_FILE")

    compatibility_fields = [
        "EAC_FRIENDLY",
        "SERVER_SIDE_PLAYER",
        "SERVER_SIDE_DEDICATED",
        "CLIENT_SIDE",
        "MOD_TYPE_ID",
        "SAFE_TO_INSTALL",
        "SAFE_TO_REMOVE",
        "UNIQUE",
    ]
    for compat_field in compatibility_fields:
        if compat_field not in fieldnames:
            fieldnames.append(compat_field)

    # Keep a stable canonical CSV layout.
    ordered_known_fields = [fn for fn in COMPAT_CSV_FIELD_ORDER if fn in fieldnames]
    ordered_extra_fields = [fn for fn in fieldnames if fn not in COMPAT_CSV_FIELD_ORDER]
    fieldnames = ordered_known_fields + ordered_extra_fields

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

    before_non_agf_filter = len(rows)
    rows = [row for row in rows if is_agf_mod(row.get("MOD_NAME", "").strip())]
    dropped_non_agf = before_non_agf_filter - len(rows)
    if dropped_non_agf > 0:
        log.warn(f"Removed {dropped_non_agf} non-AGF rows from HELPER_ModCompatibility.csv")

    removed = 0
    if prune_to_mods_now:
        before = len(rows)
        rows = [row for row in rows if row.get("MOD_NAME") in mods_now]
        removed = before - len(rows)
        log.stats.csv_removed_rows += removed

    existing = {row.get("MOD_NAME") for row in rows}
    for mod in sorted(mods_now):
        if mod not in existing:
            new_row = {fn: "TBD" for fn in fieldnames}
            new_row["MOD_NAME"] = mod
            if "MOD_TYPE_ID" in fieldnames:
                new_row["MOD_TYPE_ID"] = "0"
            new_row["QUOTE_FILE"] = f"{mod}.txt"
            rows.append(new_row)
            log.stats.csv_added_rows += 1

    for row in rows:
        row["QUOTE_FILE"] = f"{row.get('MOD_NAME', 'TBD')}.txt"
        for fn in fieldnames:
            if not row.get(fn):
                row[fn] = "0" if fn == "MOD_TYPE_ID" else "TBD"

    def _is_missing(value: str) -> bool:
        text = str(value or "").strip()
        return not text or text.upper() in {"MISSINGDATA", "TBD", "0"}

    for row in rows:
        legacy_server_side = str(row.get("SERVER_SIDE", "")).strip()
        server_side_player = str(row.get("SERVER_SIDE_PLAYER", "")).strip()
        server_side_dedicated = str(row.get("SERVER_SIDE_DEDICATED", "")).strip()

        # Legacy mapping: one SERVER_SIDE value feeds both new host columns.
        if _is_missing(server_side_player) and not _is_missing(legacy_server_side):
            row["SERVER_SIDE_PLAYER"] = legacy_server_side
            server_side_player = legacy_server_side
        if _is_missing(server_side_dedicated) and not _is_missing(legacy_server_side):
            row["SERVER_SIDE_DEDICATED"] = legacy_server_side

        client_required = str(row.get("CLIENT_REQUIRED", "")).strip()
        client_side = str(row.get("CLIENT_SIDE", "")).strip()

        if _is_missing(client_side) and not _is_missing(client_required):
            row["CLIENT_SIDE"] = client_required
            client_side = client_required

        mod_type_id = str(row.get("MOD_TYPE_ID", "") or "").strip()
        if not mod_type_id:
            row["MOD_TYPE_ID"] = "0"
            mod_type_id = "0"
        if _is_missing(mod_type_id):
            continue

        derived_compat = MOD_TYPE_COMPAT_BY_ID.get(mod_type_id)
        if not derived_compat:
            log.warn(
                f"Invalid MOD_TYPE_ID '{mod_type_id}' for {row.get('MOD_NAME', 'UNKNOWN')}; keeping existing compatibility values"
            )
            continue

        for compat_field, compat_value in derived_compat.items():
            row[compat_field] = compat_value

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
                atomic_write_text(quote_path, "", encoding="utf-8")
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
                    atomic_write_text(quote_path, "", encoding="utf-8")
                log.stats.quote_files_blanked_none += 1
        except Exception as ex:
            log.warn(f"Failed reading/normalizing quote file {quote_path}: {ex}")


def update_mod_loaded_references_for_renames(
    folder_renames: List[Tuple[str, str, str]],
    dry_run: bool,
    log: Logger,
    scan_dirs: Tuple[str, ...] = (IN_PROGRESS, STAGING),
) -> None:
    """Update mod_loaded('old') -> mod_loaded('new') across workspace XML files for name renames."""
    rename_pairs: List[Tuple[str, str]] = []
    seen_pairs: set[Tuple[str, str]] = set()
    for old_name, new_name, _ in folder_renames:
        old_base = get_base_mod_name(old_name)
        new_base = get_base_mod_name(new_name)
        if old_base == new_base:
            continue
        pair = (old_base, new_base)
        if pair in seen_pairs:
            continue
        seen_pairs.add(pair)
        rename_pairs.append(pair)

    if not rename_pairs:
        return

    log.info("Updating mod_loaded references for renamed mods")
    renamed_bases: set[str] = set()
    for old_base, new_base in rename_pairs:
        renamed_bases.add(old_base)
        renamed_bases.add(new_base)
    touched_other_mod_bases: set[str] = set()

    for scan_dir in scan_dirs:
        if not os.path.isdir(scan_dir):
            continue

        for root, _, files in os.walk(scan_dir):
            for filename in files:
                if not filename.lower().endswith(".xml"):
                    continue

                file_path = os.path.join(root, filename)
                try:
                    with open(file_path, "r", encoding="utf-8") as f:
                        content = f.read()
                except Exception as ex:
                    log.warn(f"Failed reading XML for mod_loaded update: {file_path}: {ex}")
                    continue

                updated_content = content
                replacement_count = 0
                for old_base, new_base in rename_pairs:
                    old_token = f"mod_loaded('{old_base}')"
                    new_token = f"mod_loaded('{new_base}')"
                    count = updated_content.count(old_token)
                    if count:
                        updated_content = updated_content.replace(old_token, new_token)
                        replacement_count += count

                if replacement_count == 0:
                    continue

                rel_path = os.path.relpath(file_path, VS_CODE_ROOT).replace("\\", "/")

                owner_base = ""
                rel_parts = rel_path.split("/")
                if rel_parts and rel_parts[0] in (
                    os.path.basename(IN_PROGRESS),
                    os.path.basename(STAGING),
                ) and len(rel_parts) >= 2:
                    owner_base = get_base_mod_name(rel_parts[1])
                if owner_base and owner_base not in renamed_bases:
                    touched_other_mod_bases.add(owner_base)

                if dry_run:
                    log.info(f"[DRYRUN] Would update mod_loaded ref in: {rel_path} ({replacement_count} occurrences)")
                    continue

                try:
                    atomic_write_text(file_path, updated_content, encoding="utf-8")
                    log.info(f"Updated mod_loaded ref in: {rel_path} ({replacement_count} occurrences)")
                except Exception as ex:
                    log.warn(f"Failed writing XML for mod_loaded update: {file_path}: {ex}")

    for mod_base in sorted(touched_other_mod_bases):
        log.action_needed(
            f"'{mod_base}' had mod_loaded references updated. Bump its ModInfo version and review its README."
        )


def gather_mod_versions_by_base_in_dirs(mod_dirs: Tuple[str, ...]) -> Dict[str, str]:
    versions: Dict[str, str] = {}
    for mod_dir in mod_dirs:
        for folder_name, folder_path in scan_mod_folders(mod_dir).items():
            base_name = get_base_mod_name(folder_name)
            version = get_modinfo_version(folder_path) or "0.0.0"
            versions[base_name] = version
    return versions


def update_gigglepack_pending_changes(
    dry_run: bool,
    log: Logger,
    consolidate_updates_label: str = "",
) -> Dict[str, object]:
    """Compute/update pending changes since last published GigglePack state from ActiveBuild."""
    release_state = load_gigglepack_release_state()
    prev_version = str(release_state.get("gigglepack_version", "1.0.0"))
    prev_mods_raw = release_state.get("mods", {})
    prev_mods = prev_mods_raw if isinstance(prev_mods_raw, dict) else {}

    current_mods = gather_mod_versions_by_base_in_dirs((STAGING,))

    added_mods: List[Tuple[str, str]] = []
    updated_existing_mods: List[Tuple[str, str, str]] = []
    removed_mods: List[Tuple[str, str]] = []

    for mod_name in sorted(current_mods):
        current_ver = current_mods[mod_name]
        previous_ver = str(prev_mods.get(mod_name, ""))
        if not previous_ver:
            added_mods.append((mod_name, current_ver))
        elif compare_versions(current_ver, previous_ver) > 0:
            updated_existing_mods.append((mod_name, previous_ver, current_ver))

    for mod_name in sorted(prev_mods):
        if mod_name not in current_mods:
            removed_mods.append((mod_name, str(prev_mods.get(mod_name, ""))))

    renamed_mods, added_mods, removed_mods = detect_renamed_mods(added_mods, removed_mods)

    updated_mods_payload = [
        [mod_name, old_ver, new_ver]
        for mod_name, old_ver, new_ver in updated_existing_mods
    ]

    payload: Dict[str, object] = {
        "computed_at": dt.datetime.now().isoformat(timespec="seconds"),
        "since_gigglepack_version": prev_version,
        "new_mods": [[mod_name, ver] for mod_name, ver in added_mods],
        "updated_mods": updated_mods_payload,
        "renamed_mods": [[old_name, new_name, ver] for old_name, new_name, ver in renamed_mods],
        "removed_mods": [[mod_name, old_ver] for mod_name, old_ver in removed_mods],
    }

    collapse_label = (consolidate_updates_label or "").strip()
    if collapse_label and updated_mods_payload:
        payload["updated_mods_expanded_count"] = len(updated_mods_payload)
        payload["updated_mods_expanded"] = updated_mods_payload
        payload["updated_mods"] = [[collapse_label, "multiple", "multiple"]]
        payload["one_time_consolidation_note"] = (
            "Pending updates were intentionally consolidated into one bulk entry for this run."
        )
        log.info(
            "Pending changes consolidation active: "
            f"collapsed {len(updated_mods_payload)} updated mods into '{collapse_label}'"
        )

    if dry_run:
        log.info(f"[DRYRUN] Would write pending changes file: {GIGGLEPACK_PENDING_CHANGES_PATH}")
    else:
        os.makedirs(os.path.dirname(GIGGLEPACK_PENDING_CHANGES_PATH), exist_ok=True)
        atomic_write_json(GIGGLEPACK_PENDING_CHANGES_PATH, payload, ensure_ascii=False, indent=2)

    log.info(
        "Pending GigglePack changes updated: "
        f"new={len(added_mods)}, updated={len(updated_existing_mods)}, "
        f"renamed={len(renamed_mods)}, removed={len(removed_mods)}"
    )
    return payload


def generate_mod_readmes(
    csv_rows: List[Dict[str, str]],
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
) -> None:
    log.info("Generate per-mod README.md and ReadableReadMe.txt")
    mod_type_line_by_id = load_mod_type_lines_from_template(log)

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
            tested_game_version = compat.get("TESTED_GAME_VERSION", "MISSINGDATA")
            eac_friendly = compat.get("EAC_FRIENDLY", "MISSINGDATA")
            server_side_player = compat.get("SERVER_SIDE_PLAYER", "MISSINGDATA")
            if not server_side_player or server_side_player == "MISSINGDATA":
                server_side_player = compat.get("SERVER_SIDE", "MISSINGDATA")
            server_side_dedicated = compat.get("SERVER_SIDE_DEDICATED", "MISSINGDATA")
            if not server_side_dedicated or server_side_dedicated == "MISSINGDATA":
                server_side_dedicated = compat.get("SERVER_SIDE", "MISSINGDATA")
            client_side = compat.get("CLIENT_SIDE", "MISSINGDATA")
            if not client_side or client_side == "MISSINGDATA":
                client_side = compat.get("CLIENT_REQUIRED", "MISSINGDATA")
            mod_type_id = str(compat.get("MOD_TYPE_ID", "MISSINGDATA") or "").strip()
            mod_type_line = mod_type_line_by_id.get(mod_type_id)
            if not mod_type_line:
                if mod_type_id and mod_type_id not in {"MISSINGDATA", "TBD"}:
                    log.warn(f"Invalid MOD_TYPE_ID '{mod_type_id}' for {base_name}; using MISSINGDATA")
                mod_type_line = "MISSINGDATA"
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
            readme_content = readme_content.replace("{{TESTED_GAME_VERSION}}", tested_game_version)
            readme_content = readme_content.replace("{{EAC_FRIENDLY}}", eac_friendly)
            readme_content = readme_content.replace("{{SERVER_SIDE_PLAYER}}", server_side_player)
            readme_content = readme_content.replace("{{SERVER_SIDE_DEDICATED}}", server_side_dedicated)
            readme_content = readme_content.replace("{{CLIENT_SIDE}}", client_side)
            readme_content = readme_content.replace("{{MOD_TYPE_ID}}", mod_type_id)
            readme_content = readme_content.replace("{{MOD_TYPE_LINE}}", mod_type_line)
            # Backward compatibility for older templates.
            readme_content = readme_content.replace("{{SERVER_SIDE}}", server_side_player)
            readme_content = readme_content.replace("{{CLIENT_REQUIRED}}", client_side)
            readme_content = readme_content.replace("{{MOD_TYPE}}", mod_type_line)
            readme_content = readme_content.replace("{{SAFE_TO_INSTALL}}", safe_to_install)
            readme_content = readme_content.replace("{{SAFE_TO_REMOVE}}", safe_to_remove)
            readme_content = readme_content.replace("{{UNIQUE}}", unique)

            readme_path = os.path.join(mod_path, "README.md")
            features_summary_block = extract_readme_block(readme_path, "<!-- FEATURES-SUMMARY START -->", "<!-- FEATURES-SUMMARY END -->")
            features_detailed_block = extract_readme_block(readme_path, "<!-- FEATURES-DETAILED START -->", "<!-- FEATURES-DETAILED END -->")
            changelog_block = extract_readme_block(readme_path, "<!-- CHANGELOG START -->", "<!-- CHANGELOG END -->")

            features_summary_block = sanitize_preserved_readme_block(features_summary_block)
            features_detailed_block = sanitize_preserved_readme_block(features_detailed_block)
            changelog_block = sanitize_preserved_readme_block(changelog_block)

            if features_summary_block:
                readme_content = re.sub(
                    r"(<!-- FEATURES-SUMMARY START -->)([\s\S]*?)(<!-- FEATURES-SUMMARY END -->)",
                    r"\1" + features_summary_block + r"\3",
                    readme_content,
                    flags=re.MULTILINE,
                )
            if features_detailed_block:
                readme_content = re.sub(
                    r"(<!-- FEATURES-DETAILED START -->)([\s\S]*?)(<!-- FEATURES-DETAILED END -->)",
                    r"\1" + features_detailed_block + r"\3",
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
                    atomic_write_text(readme_path, readme_content, encoding="utf-8")
                    atomic_write_text(txt_path, txt_content, encoding="utf-8")
                except Exception as ex:
                    log.warn(f"Failed writing README files for {folder_name}: {ex}")
                    continue

            log.stats.readmes_written += 1
            log.stats.readable_txt_written += 1


def prep_names_and_readmes_for_dirs(mod_dirs: Tuple[str, ...], dry_run: bool, log: Logger) -> List[Tuple[str, str, str]]:
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
    return folder_renames


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
    for folder, path in scan_mod_folders(STAGING).items():
        workspace_by_base[get_base_mod_name(folder)] = path

    for mod_name, ws_path in mods_pulled_from_game:
        resolved_ws_path = ws_path
        if not os.path.exists(resolved_ws_path):
            base_name = get_base_mod_name(mod_name)
            resolved_ws_path = workspace_by_base.get(base_name, "")

        if (not resolved_ws_path or not os.path.exists(resolved_ws_path)) and not dry_run:
            log.warn(f"Pushback skipped for {mod_name}: workspace path missing {ws_path}")
            continue

        pushed_folder_name = os.path.basename(resolved_ws_path)
        pushed_version = get_modinfo_version(resolved_ws_path)
        if pushed_version is None:
            log.warn(f"Pushback skipped for {mod_name}: unreadable ModInfo.xml at {resolved_ws_path}")
            continue

        try:
            pushed_major = int((pushed_version or "0.0.0").split(".", 1)[0])
        except Exception:
            pushed_major = 0

        if pushed_major < 1:
            log.info(
                f"Pushback skipped for {mod_name}: version {pushed_version} is draft-only (major < 1)"
            )
            continue

        if is_4modders_mod(pushed_folder_name):
            existing_game_root_path = os.path.join(GAME_MODS, pushed_folder_name)
            if not os.path.isdir(existing_game_root_path):
                log.info(
                    f"Pushback skipped for {mod_name}: 4Modders mods only push when already present in game root"
                )
                continue

        if mod_name != pushed_folder_name:
            old_game_path = os.path.join(GAME_MODS, mod_name)
            if maybe_remove_dir(old_game_path, dry_run, log):
                log.info(
                    f"Pushback cleanup: removed old game folder name {mod_name} before pushing {pushed_folder_name}"
                )

        dest_path = os.path.join(GAME_MODS, pushed_folder_name)
        if maybe_remove_dir(dest_path, dry_run, log) and maybe_copytree(resolved_ws_path, dest_path, dry_run, log):
            log.stats.pushed_back_to_game += 1
            log.info(f"Pushback complete: {mod_name}")


def remap_pulled_mods_after_renames(
    mods_pulled_from_game: List[Tuple[str, str]],
    folder_renames: List[Tuple[str, str, str]],
    log: Logger,
) -> List[Tuple[str, str]]:
    """Remap pulled-mod tracking entries when folder names changed during the run."""
    if not mods_pulled_from_game or not folder_renames:
        return mods_pulled_from_game

    rename_by_name: Dict[str, Tuple[str, str]] = {}
    rename_by_name_and_dir: Dict[Tuple[str, str], Tuple[str, str]] = {}
    for old_name, new_name, mod_dir in folder_renames:
        new_path = os.path.join(mod_dir, new_name)
        rename_by_name[old_name] = (new_name, new_path)
        rename_by_name_and_dir[(old_name, mod_dir)] = (new_name, new_path)

    remapped: List[Tuple[str, str]] = []
    seen: set[Tuple[str, str]] = set()
    for mod_name, ws_path in mods_pulled_from_game:
        current_name = mod_name
        current_path = ws_path

        mod_dir = os.path.dirname(ws_path)
        mapped = rename_by_name_and_dir.get((mod_name, mod_dir))
        if mapped is None:
            mapped = rename_by_name.get(mod_name)

        if mapped is not None:
            current_name, current_path = mapped
            log.info(
                f"Pushback remap: {mod_name} -> {current_name} (using post-rename workspace path)"
            )

        key = (current_name, current_path)
        if key in seen:
            continue
        seen.add(key)
        remapped.append(key)

    return remapped


def get_managed_mod_names_from_csv(log: Logger) -> set[str]:
    """Return managed mod base names from HELPER_ModCompatibility.csv."""
    managed: set[str] = set()
    try:
        _, rows = load_compat_csv()
        for row in rows:
            mod_name = row.get("MOD_NAME", "").strip()
            if mod_name and is_agf_mod(mod_name):
                managed.add(mod_name)
            elif mod_name:
                log.warn(f"Ignoring non-AGF CSV row in managed roster: {mod_name}")
    except Exception as ex:
        log.warn(f"Could not load managed mods from CSV; unknown orphans will be preserved: {ex}")
    return managed


def sync_staging_and_game(dry_run: bool, log: Logger) -> List[Tuple[str, str]]:
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
    mods_pulled_from_game: List[Tuple[str, str]] = []
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
    game_by_base_all: Dict[str, List[Tuple[str, str]]] = {}
    for folder, path in game_folders.items():
        base_name = get_base_mod_name(folder)
        game_by_base_all.setdefault(base_name, []).append((folder, path))

    # If multiple game folders exist for the same base, keep only the ActiveBuild target name.
    for base_name, entries in game_by_base_all.items():
        if len(entries) <= 1:
            continue
        keep_folder = staging_by_base.get(base_name, ("", ""))[0]
        for game_folder, game_path in entries:
            if keep_folder and game_folder == keep_folder:
                continue
            if maybe_remove_dir(game_path, dry_run, log):
                log.info(
                    "sync-work cleanup: removed duplicate/old game version for base "
                    f"{base_name}: {game_folder}"
                )

    game_folders = scan_mod_folders(GAME_MODS)
    game_by_base_all = {}
    for folder, path in game_folders.items():
        base_name = get_base_mod_name(folder)
        game_by_base_all.setdefault(base_name, []).append((folder, path))
    managed_mods = get_managed_mod_names_from_csv(log)

    # Remove AGF mods from game root that are no longer represented in ActiveBuild.
    # This handles renamed mods and removed mods so stale folders do not linger in game.
    stale_game_bases = sorted(set(game_by_base_all.keys()) - set(staging_by_base.keys()))
    for base_name in stale_game_bases:
        for game_folder, game_path in game_by_base_all.get(base_name, []):
            if base_name in managed_mods:
                if maybe_remove_dir(game_path, dry_run, log):
                    log.info(
                        f"sync-work cleanup: removed managed stale game mod not present in ActiveBuild: {game_folder}"
                    )
            else:
                log.warn(
                    f"sync-work orphan preserved: unknown game mod not in ActiveBuild and not in CSV: {game_folder}"
                )

    # Re-scan game root after stale cleanup so overlap/missing calculations are accurate.
    game_folders = scan_mod_folders(GAME_MODS)
    game_by_base_all = {}
    for folder, path in game_folders.items():
        base_name = get_base_mod_name(folder)
        game_by_base_all.setdefault(base_name, []).append((folder, path))

    overlap = sorted(set(staging_by_base.keys()) & set(game_by_base_all.keys()))
    for base_name in overlap:
        st_folder, st_path = staging_by_base[base_name]
        game_entries = list(game_by_base_all.get(base_name, []))
        preferred = next(((name, path) for name, path in game_entries if name == st_folder), None)
        game_folder, game_path = preferred if preferred else game_entries[0]

        # Remove any extra versions for this base in game root.
        for extra_folder, extra_path in game_entries:
            if extra_folder == game_folder:
                continue
            if maybe_remove_dir(extra_path, dry_run, log):
                log.info(
                    f"sync-work cleanup: removed extra game version for {base_name}: {extra_folder}"
                )
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
                mods_pulled_from_game.append((game_folder, target))
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
    missing_in_game = sorted(set(staging_by_base.keys()) - set(game_by_base_all.keys()))
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

    # Remove stale optionals entries that no longer match ActiveBuild folder names.
    expected_backpack = {folder for folder in staging_folders if is_backpack_mod(folder)}
    expected_hudplus = {
        folder
        for folder in staging_folders
        if is_hudplus_mod(folder) or is_hudpluszother_mod(folder)
    }
    expected_4modders = {folder for folder in staging_folders if is_4modders_mod(folder)}

    optionals_cleanup_targets = (
        (optionals_backpack_path, expected_backpack, GAME_OPTIONALS_BACKPACK_DIR),
        (optionals_hudplus_path, expected_hudplus, GAME_OPTIONALS_HUDPLUS_DIR),
        (optionals_4modders_path, expected_4modders, GAME_OPTIONALS_4MODDERS_DIR),
    )
    for optionals_dir, expected_names, label in optionals_cleanup_targets:
        if not os.path.isdir(optionals_dir):
            continue
        existing = scan_mod_folders(optionals_dir)
        for folder_name, folder_path in existing.items():
            if folder_name in expected_names:
                continue
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    f"sync-work cleanup: removed stale optional from {label}: {folder_name}"
                )

    # Normalize active-build folder names after pulls so names track ModInfo versions.
    rename_mod_folders_to_modinfo(dry_run, log, mod_dirs=(STAGING,))

    # Extra cleanup pass after renames: ensure game root and optionals reflect final ActiveBuild names.
    log.info("sync-work cleanup: post-rename cleanup pass")

    final_staging_folders = scan_mod_folders(STAGING)
    final_allowed_staging_root: Dict[str, str] = {}
    final_backpack_mods = sorted([f for f in final_staging_folders if is_backpack_mod(f)])
    final_active_backpack = next((f for f in final_backpack_mods if BACKPACK_DEFAULT_ACTIVE_TOKEN in f), None)
    if final_active_backpack is None and final_backpack_mods:
        final_active_backpack = final_backpack_mods[0]

    for folder, path in final_staging_folders.items():
        if is_backpack_mod(folder) and final_active_backpack and folder != final_active_backpack:
            continue
        if is_hudpluszother_mod(folder):
            continue
        if is_4modders_mod(folder):
            continue
        final_allowed_staging_root[folder] = path

    final_staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in final_allowed_staging_root.items()
    }

    final_game_folders = scan_mod_folders(GAME_MODS)
    final_game_by_base_all: Dict[str, List[Tuple[str, str]]] = {}
    for folder, path in final_game_folders.items():
        base_name = get_base_mod_name(folder)
        final_game_by_base_all.setdefault(base_name, []).append((folder, path))

    # Final pass: enforce one game-root folder per ActiveBuild base name.
    for base_name, entries in final_game_by_base_all.items():
        expected_folder = final_staging_by_base.get(base_name, ("", ""))[0]
        if not expected_folder:
            continue
        for folder_name, folder_path in entries:
            if folder_name == expected_folder:
                continue
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    "sync-work cleanup: post-rename removed extra game version for base "
                    f"{base_name}: {folder_name}"
                )

    final_game_folders = scan_mod_folders(GAME_MODS)
    final_game_by_base_all = {}
    for folder, path in final_game_folders.items():
        base_name = get_base_mod_name(folder)
        final_game_by_base_all.setdefault(base_name, []).append((folder, path))

    final_stale_game_bases = sorted(set(final_game_by_base_all.keys()) - set(final_staging_by_base.keys()))
    for base_name in final_stale_game_bases:
        for game_folder, game_path in final_game_by_base_all.get(base_name, []):
            if base_name in managed_mods:
                if maybe_remove_dir(game_path, dry_run, log):
                    log.info(
                        "sync-work cleanup: post-rename removed managed stale game mod "
                        f"not present in ActiveBuild: {game_folder}"
                    )
            else:
                log.warn(
                    "sync-work orphan preserved: post-rename unknown game mod not in "
                    f"ActiveBuild and not in CSV: {game_folder}"
                )

    final_expected_backpack = {folder for folder in final_staging_folders if is_backpack_mod(folder)}
    final_expected_hudplus = {
        folder
        for folder in final_staging_folders
        if is_hudplus_mod(folder) or is_hudpluszother_mod(folder)
    }
    final_expected_4modders = {folder for folder in final_staging_folders if is_4modders_mod(folder)}

    final_optionals_cleanup_targets = (
        (optionals_backpack_path, final_expected_backpack, GAME_OPTIONALS_BACKPACK_DIR),
        (optionals_hudplus_path, final_expected_hudplus, GAME_OPTIONALS_HUDPLUS_DIR),
        (optionals_4modders_path, final_expected_4modders, GAME_OPTIONALS_4MODDERS_DIR),
    )
    for optionals_dir, expected_names, label in final_optionals_cleanup_targets:
        if not os.path.isdir(optionals_dir):
            continue
        existing = scan_mod_folders(optionals_dir)
        for folder_name, folder_path in existing.items():
            if folder_name in expected_names:
                continue
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    f"sync-work cleanup: post-rename removed stale optional from {label}: {folder_name}"
                )

    return mods_pulled_from_game


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


def cleanup_legacy_4modders_renames_in_dir(base_dir: str, dry_run: bool, log: Logger, staging_dir: Optional[str] = None) -> None:
    """Remove legacy NoEAC/HUDPluszOther folders that were replaced by AGF-4Modders in a lane.

    When ``staging_dir`` is provided (recommended for PUBLISH_READY), staging is used as the
    source-of-truth to decide which side of a 4Modders/NoEAC pair is stale:
    - If staging still has AGF-4Modders-X  -> NoEAC-X in base_dir is legacy, delete it.
    - If staging has AGF-NoEAC-X (not 4Modders-X) -> 4Modders-X in base_dir is stale, delete it.
    Without a staging_dir the function keeps its original behaviour (always deletes the NoEAC side).
    """
    lane_name = os.path.basename(base_dir.rstrip("\\/"))
    log.info(f"Cleanup: Remove legacy 4Modders-replaced folders in {lane_name}")

    folders = scan_mod_folders(base_dir)
    staging_folders: Dict[str, str] = scan_mod_folders(staging_dir) if staging_dir else {}

    for folder_name in sorted(folders.keys()):
        if not folder_name.startswith("AGF-4Modders-"):
            continue

        match = re.match(r"^AGF-4Modders-(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if not match:
            continue

        suffix = match.group(1)
        folder_path = folders[folder_name]

        if staging_dir:
            staging_has_4modders = any(
                n.startswith(f"AGF-4Modders-{suffix}-v") for n in staging_folders
            )
            if not staging_has_4modders:
                # Staging no longer uses the 4Modders name for this suffix.
                # The 4Modders folder here is stale; delete it and keep the NoEAC counterpart.
                for legacy_prefix in ("AGF-NoEAC-", "AGF-HUDPluszOther-"):
                    legacy_pattern = re.compile(rf"^{re.escape(legacy_prefix + suffix)}-v.+$")
                    has_counterpart = any(legacy_pattern.match(n) for n in folders if n != folder_name)
                    if has_counterpart:
                        if maybe_remove_dir(folder_path, dry_run, log):
                            log.info(
                                f"Removed stale 4Modders folder in {lane_name}: {folder_name} "
                                f"(staging now uses a non-4Modders name for suffix '{suffix}')"
                            )
                        break
                continue

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
    atomic_write_text(path, content, encoding="utf-8")


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


def generate_mod_images(dry_run: bool, log: Logger) -> None:
    """Invoke SCRIPT-GenerateModImages.py to regenerate mod images from current mod data."""
    banner_script = os.path.join(os.path.dirname(os.path.abspath(__file__)), "SCRIPT-GenerateModImages.py")
    if not os.path.isfile(banner_script):
        log.warn(f"ModImage script not found, skipping: {banner_script}")
        return
    python_exe = sys.executable
    cmd = [python_exe, banner_script, "--changed-only"]
    if dry_run:
        cmd.append("--dry-run")
    log.info(f"Generating mod images: {' '.join(cmd)}")
    if not dry_run:
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, check=False)
            for line in (result.stdout or "").splitlines():
                if line.strip():
                    log.info(f"  [modimages] {line}")
            for line in (result.stderr or "").splitlines():
                if line.strip():
                    log.warn(f"  [modimages] {line}")
            if result.returncode != 0:
                log.warn(f"ModImage script exited with code {result.returncode}")
        except Exception as ex:
            log.warn(f"ModImage script failed: {ex}")


def copy_mod_images_to_mod_folders(dry_run: bool, log: Logger, mod_dirs: Optional[Tuple[str, ...]] = None) -> None:
    """Copy each mod's generated ModImage PNG into the root of its mod folder."""
    if mod_dirs is None:
        mod_dirs = (STAGING,)
    if not os.path.isdir(IMAGES_GENERATED_ROOT):
        log.warn(f"Generated images folder not found, skipping ModImage copy: {IMAGES_GENERATED_ROOT}")
        return
    copied = 0
    for mod_dir in mod_dirs:
        if not os.path.isdir(mod_dir):
            continue
        for folder_name in sorted(os.listdir(mod_dir)):
            folder_path = os.path.join(mod_dir, folder_name)
            if not os.path.isdir(folder_path):
                continue
            if not is_agf_mod(folder_name):
                continue
            base_name = get_base_mod_name(folder_name)
            banner_src = os.path.join(IMAGES_GENERATED_ROOT, f"ModImage_{base_name}.png")
            if not os.path.isfile(banner_src):
                continue
            banner_dst = os.path.join(folder_path, f"ModImage_{base_name}.png")
            try:
                src_mtime = os.path.getmtime(banner_src)
                dst_mtime = os.path.getmtime(banner_dst) if os.path.isfile(banner_dst) else 0
                if src_mtime <= dst_mtime:
                    continue
            except OSError:
                pass
            log.info(f"Copy banner -> {os.path.relpath(banner_dst, VS_CODE_ROOT)}")
            if not dry_run:
                try:
                    import shutil
                    shutil.copy2(banner_src, banner_dst)
                    copied += 1
                except Exception as ex:
                    log.warn(f"Could not copy banner to {banner_dst}: {ex}")
            else:
                copied += 1
    log.info(f"ModImage copy: {copied} file(s) {'would be ' if dry_run else ''}updated")


def generate_gigglepack_release_artifacts(dry_run: bool, log: Logger) -> Dict[str, object]:
    """Create versioned GigglePack zips and release notes for Discord/GitHub usage."""
    log.info("Step 6.5: Generate GigglePack release metadata + changelog outputs")

    canonical_zip_path = os.path.join(ZIP_OUTPUT, GIGGLEPACK_CANONICAL_ZIP)
    if not os.path.isfile(canonical_zip_path) and not dry_run:
        log.warn(f"GigglePack release metadata skipped: missing {canonical_zip_path}")
        return {"has_update": False, "discord_text": ""}

    release_meta_dir = get_gigglepack_release_dir_for_write()
    state_path = os.path.join(release_meta_dir, "gigglepack-release-state.json")
    discord_path = os.path.join(release_meta_dir, "Discord", "discord-post.txt")
    markdown_path = os.path.join(release_meta_dir, "gigglepack-release-history.md")

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
                    existing_entries = []
                    for chunk in re.split(r"\n\n---\n\n", existing_body):
                        chunk = re.sub(r'^Newest entries appear at the top\.\s*', '', chunk.strip())
                        if chunk:
                            existing_entries.append(chunk)
        except Exception:
            existing_entries = []

    def entry_version(entry_text: str) -> str:
        match = re.search(r"^#{1,2}\s*GigglePack\s+v([0-9]+\.[0-9]+\.[0-9]+)", entry_text, re.MULTILINE)
        return match.group(1) if match else ""

    filtered_entries = [entry for entry in existing_entries if entry_version(entry) != release_version]
    all_entries = [new_markdown_entry] + filtered_entries
    markdown_text = (
        "# GigglePack Release Changelog\n\n"
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
        "updated_mods": updated_mod_entries_display,
        "new_mods": new_mod_entries_display,
        "renamed_mods": renamed_mod_entries_display,
        "removed_mods": removed_mod_entries_display,
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
        atomic_write_json(state_path, state_payload, ensure_ascii=True, indent=2)
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
DEFAULT_MAIN_README_CATEGORY_TEMPLATE = (
    "---\n\n"
    "<br>\n\n"
    "## **{{CATEGORY_TITLE}}**\n\n"
    "{{CATEGORY_DOWNLOAD_LINE}} - {{CATEGORY_DESCRIPTION}}\n\n"
    "*[(Back to Top)](#agf-7-days-to-die-mods)*\n\n"
    "---\n\n"
    "---\n"
)

DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE = (
    '<table><tr>\n'
    '<td width="160">{{MOD_BANNER}}</td>\n'
    '<td valign="top">\n'
    '<b>{{MOD_NAME}}</b> &nbsp;·&nbsp; v{{MOD_VERSION}} &nbsp;·&nbsp; <a href="{{DOWNLOAD_LINK}}">Download</a><br>\n'
    '{{MOD_DESCRIPTION}}\n'
    '{{MOD_FEATURES_BLOCK}}'
    '</td>\n'
    '</tr></table>\n'
    '\n---\n'
)


def render_main_readme_category_block(
    category_template: str,
    category_title: str,
    category_download_line: str,
    category_description: str,
) -> List[str]:
    block = re.sub(r"<!--.*?-->", "", category_template, flags=re.DOTALL)
    block = block.replace("{{CATEGORY_TITLE}}", category_title)
    block = block.replace("{{CATEGORY_DOWNLOAD_LINE}}", category_download_line)
    block = block.replace("{{CATEGORY_DESCRIPTION}}", category_description)
    # If no download line, collapse the resulting blank line it leaves behind
    block = re.sub(r"\n{3,}", "\n\n", block).strip("\n")
    return block.splitlines()


def build_mod_entry(
    folder_name: str,
    mod_entry_template: Optional[str] = None,
    compat_map: Optional[Dict[str, Dict[str, str]]] = None,
    mod_type_lines: Optional[Dict[str, str]] = None,
) -> str:
    mod_path = os.path.join(PUBLISH_READY, folder_name)
    modinfo_path = os.path.join(mod_path, "ModInfo.xml")

    name, version = parse_modinfo(modinfo_path, folder_name)
    display_name = get_modinfo_display_name(modinfo_path, name)
    version_display = format_version_for_display(version, display_name)
    desc = extract_mod_description_from_modinfo(modinfo_path)
    link = zip_download_link(f"{get_base_mod_name(folder_name)}.zip")

    features_block = ""
    base_mod = get_base_mod_name(folder_name)
    if compat_map is not None:
        mod_type_id = (compat_map.get(base_mod) or {}).get("MOD_TYPE_ID", "").strip()
        type_line_map = mod_type_lines if mod_type_lines is not None else DEFAULT_MOD_TYPE_LINE_BY_ID
        mod_type_text = type_line_map.get(mod_type_id, "")
        if mod_type_text:
            features_block = f"<ul><li><em>{mod_type_text}</em></li></ul>\n"

    base_mod_name = get_base_mod_name(folder_name)
    banner_file = f"ModImage_{base_mod_name}.png"
    banner_abs = os.path.join(IMAGES_GENERATED_ROOT, banner_file)
    banner_html = ""
    if os.path.isfile(banner_abs):
        banner_url = f"https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/blob/main/00_Images/_generated/{banner_file}?raw=true"
        banner_html = f'<a href="{banner_url}"><img src="{banner_url}" width="150"></a>'

    template = re.sub(r"<!--.*?-->", "", (mod_entry_template or DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE), flags=re.DOTALL).strip("\n")
    entry = template
    entry = entry.replace("{{MOD_NAME}}", display_name)
    entry = entry.replace("{{MOD_VERSION}}", version_display)
    entry = entry.replace("{{DOWNLOAD_LINK}}", link)
    entry = entry.replace("{{MOD_DESCRIPTION}}", desc)
    entry = entry.replace("{{MOD_BANNER}}", banner_html)
    entry = entry.replace("{{MOD_FEATURES_BLOCK}}", features_block)
    entry = re.sub(r"\n{3,}", "\n\n", entry).strip()
    return entry


def load_gigglepack_release_state() -> Dict[str, object]:
    state_path = os.path.join(get_gigglepack_release_dir_for_read(), "gigglepack-release-state.json")
    return load_json_file(state_path)


def load_recent_gigglepack_release_entries(limit: int = 3) -> List[Dict[str, object]]:
    if limit <= 0:
        return []

    markdown_path = os.path.join(get_gigglepack_release_dir_for_read(), "gigglepack-release-history.md")
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

        if line in {"# GigglePack Release Changelog", "Newest entries appear at the top.", "---", ""}:
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
                    to_ver = str(entry.get("to_display", entry.get("to", ""))).strip()
                    if mod_name:
                        new_items.append(f"{mod_download_html_link(mod_name)} (new: v{escape_html(to_ver)})")

        updated_items: List[str] = []
        if isinstance(updated_mods, list):
            for entry in updated_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    from_ver = str(entry.get("from_display", entry.get("from", ""))).strip()
                    to_ver = str(entry.get("to_display", entry.get("to", ""))).strip()
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
                    version = str(entry.get("version_display", entry.get("version", ""))).strip()
                    if from_mod and to_mod:
                        renamed_items.append(
                            f"{mod_download_html_link(to_mod)} (renamed from {escape_html(from_mod)}, v{escape_html(version)})"
                        )

        removed_items: List[str] = []
        if isinstance(removed_mods, list):
            for entry in removed_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    from_ver = str(entry.get("from_display", entry.get("from", ""))).strip()
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


def load_category_descriptions(log: Optional[Logger] = None) -> Dict[str, str]:
    """Parse TEMPLATE-CategoryDescriptions.md into a dict keyed by section name."""
    result: Dict[str, str] = {}
    if not os.path.isfile(CATEGORY_DESCRIPTIONS_PATH):
        if log:
            log.warn(f"Category descriptions file not found: {CATEGORY_DESCRIPTIONS_PATH}")
        return result
    try:
        with open(CATEGORY_DESCRIPTIONS_PATH, "r", encoding="utf-8") as f:
            content = f.read()
        # Split on [SECTION] markers
        parts = re.split(r"^\[([^\]]+)\]", content, flags=re.MULTILINE)
        # parts = [preamble, key1, value1, key2, value2, ...]
        it = iter(parts[1:])  # skip preamble
        for key in it:
            value = next(it, "").strip()
            if key.strip() and key.strip() != "category_descriptions":
                result[key.strip()] = value
    except Exception as ex:
        if log:
            log.warn(f"Could not parse category descriptions: {ex}")
    return result


def generate_main_readme(dry_run: bool, log: Logger) -> None:
    log.info("Step 7: Generate main README.md")

    if not os.path.exists(MAIN_TEMPLATE_PATH):
        log.error(f"Missing required template: {MAIN_TEMPLATE_PATH}")
        raise FileNotFoundError(MAIN_TEMPLATE_PATH)

    with open(MAIN_TEMPLATE_PATH, "r", encoding="utf-8") as f:
        main_template = f.read()

    category_template = DEFAULT_MAIN_README_CATEGORY_TEMPLATE
    if os.path.isfile(MAIN_MOD_CATEGORY_TEMPLATE_PATH):
        try:
            with open(MAIN_MOD_CATEGORY_TEMPLATE_PATH, "r", encoding="utf-8") as f:
                category_template = f.read().strip("\n") or DEFAULT_MAIN_README_CATEGORY_TEMPLATE
        except Exception as ex:
            log.warn(f"Could not read main mod-category template {MAIN_MOD_CATEGORY_TEMPLATE_PATH}: {ex}")

    mod_entry_template = DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE
    if os.path.isfile(MAIN_MOD_ENTRY_TEMPLATE_PATH):
        try:
            with open(MAIN_MOD_ENTRY_TEMPLATE_PATH, "r", encoding="utf-8") as f:
                mod_entry_template = f.read().strip("\n") or DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE
        except Exception as ex:
            log.warn(f"Could not read main mod-entry template {MAIN_MOD_ENTRY_TEMPLATE_PATH}: {ex}")

    cat_desc = load_category_descriptions(log)

    mod_type_lines = load_mod_type_lines_from_template(log)
    _, compat_rows = load_compat_csv()
    compat_map: Dict[str, Dict[str, str]] = {row.get("MOD_NAME", "").strip(): dict(row) for row in compat_rows if row.get("MOD_NAME", "").strip()}

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

    giggle_template = DEFAULT_MAIN_README_CATEGORY_TEMPLATE
    if os.path.isfile(GIGGLE_PACK_TEMPLATE_PATH):
        try:
            with open(GIGGLE_PACK_TEMPLATE_PATH, "r", encoding="utf-8") as f:
                giggle_template = f.read().strip("\n") or DEFAULT_MAIN_README_CATEGORY_TEMPLATE
        except Exception as ex:
            log.warn(f"Could not read GigglePack template {GIGGLE_PACK_TEMPLATE_PATH}: {ex}")

    giggle_description = cat_desc.get("GIGGLE PACK", "All AGF mods in one convenient download.")
    giggle_changelog = "\n".join(giggle_release_lines) if giggle_release_lines else ""

    giggle_block = re.sub(r"<!--.*?-->", "", giggle_template, flags=re.DOTALL)
    giggle_block = giggle_block.replace("{{CATEGORY_TITLE}}", "A. GIGGLE PACK")
    giggle_block = giggle_block.replace("{{CATEGORY_DOWNLOAD_LINE}}", giggle_download_label)
    giggle_block = giggle_block.replace("{{CATEGORY_DESCRIPTION}}", giggle_description)
    giggle_block = giggle_block.replace("{{GIGGLE_CHANGELOG}}", giggle_changelog)
    giggle_block = re.sub(r"\n{3,}", "\n\n", giggle_block).strip("\n")
    md.extend(giggle_block.splitlines())
    md.append("")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "B. HUD PLUS MODS",
            f"[**⬇️ Download All HUD Plus Mods**]({zip_download_link('00_HUDPlus_All.zip')})",
            cat_desc.get("HUDPLUS", "Quality-of-life HUD enhancements and visual tweaks."),
        )
    )
    md.append("")
    for mod in hudplus_mods:
        md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))

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
            md.append(f"| {display_name} | {version_display} | [Download]({link}) | {desc} |")
        md.extend(["", "---"])

    md.extend(
        render_main_readme_category_block(
            category_template,
            "C. BACKPACK PLUS MODS",
            f"[**⬇️ Download All Backpack Plus Mods**]({zip_download_link('00_BackpackPlus_All.zip')})",
            cat_desc.get("BACKPACKPLUS", "Increases backpack size. Choose the slot count that fits your needs."),
        )
    )
    md.append("")

    preferred_last = "AGF-BackpackPlus-119Slots"
    backpack_sorted = sorted(backpackplus_mods, key=lambda x: (get_base_mod_name(x) == preferred_last, x))
    for mod in backpack_sorted:
        md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))

    if special_mods:
        md.extend(
            render_main_readme_category_block(
                category_template,
                "D. SPECIAL COMPATIBILITY MOD",
                "",
                cat_desc.get("SPECIAL", "Compatibility patches between AGF mods and select third-party mods."),
            )
        )
        md.append("")
        for mod in special_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))

    md.extend(
        render_main_readme_category_block(
            category_template,
            "E. VANILLA PLUS MODS",
            f"[**⬇️ Download All VP Mods**]({zip_download_link('00_VP_All.zip')})",
            cat_desc.get("VP", "Gameplay tweaks and new features that expand on the base game."),
        )
    )
    md.append("")
    for mod in vp_mods:
        md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))

    if noeac_mods:
        md.extend(
            render_main_readme_category_block(
                category_template,
                "F. NO EAC MODS",
                f"[**⬇️ Download All NoEAC Mods**]({zip_download_link('00_NoEAC_All.zip')})",
                cat_desc.get("NOEAC", "Game enhancements that require a DLL. EAC must be off."),
            )
        )
        md.append("")
        for mod in noeac_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))

    md.extend(
        render_main_readme_category_block(
            category_template,
            "G. 4MODDERS MODS",
            f"[**⬇️ Download All 4Modders Mods**]({zip_download_link('00_4Modders_All.zip')})",
            cat_desc.get("4MODDERS", "Modder resources and niche mods. Read each description before installing."),
        )
    )
    md.append("")
    if modders_mods:
        for mod in modders_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
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

    atomic_write_text(MAIN_README_PATH, final_content, encoding="utf-8")


# =============================================================
# VALIDATION
# =============================================================
def run_self_tests(log: Logger) -> bool:
    """Run lightweight in-process reliability tests against temp fixtures."""
    log.info("Running self-test harness")
    passed = 0
    failed = 0

    def record(test_name: str, condition: bool) -> None:
        nonlocal passed, failed
        if condition:
            passed += 1
            log.info(f"self-test pass: {test_name}")
        else:
            failed += 1
            log.error(f"self-test fail: {test_name}")

    def write_modinfo(mod_dir: str, name: str, version: str) -> None:
        xml = (
            "<xml>\n"
            f"  <Name value=\"{name}\"/>\n"
            f"  <Version value=\"{version}\"/>\n"
            "</xml>\n"
        )
        atomic_write_text(os.path.join(mod_dir, "ModInfo.xml"), xml, encoding="utf-8")

    with tempfile.TemporaryDirectory() as temp_root:
        mods_dir = os.path.join(temp_root, "mods")
        os.makedirs(mods_dir, exist_ok=True)

        agf_dir = os.path.join(mods_dir, "AGF-VP-FilterTest-v1.0.0")
        ext_dir = os.path.join(mods_dir, "SomeExternalMod-v9.9.9")
        os.makedirs(agf_dir, exist_ok=True)
        os.makedirs(ext_dir, exist_ok=True)
        write_modinfo(agf_dir, "AGF-VP-FilterTest", "1.0.0")

        filtered = scan_mod_folders(mods_dir)
        record("scan_mod_folders filters non-AGF mods", "AGF-VP-FilterTest-v1.0.0" in filtered and "SomeExternalMod-v9.9.9" not in filtered)

        name_only_dir = os.path.join(mods_dir, "AGF-VP-NameOnly-v1.0.0")
        ver_only_dir = os.path.join(mods_dir, "AGF-VP-VersionOnly-v1.0.0")
        combined_dir = os.path.join(mods_dir, "AGF-VP-CombinedOld-v1.0.0")
        os.makedirs(name_only_dir, exist_ok=True)
        os.makedirs(ver_only_dir, exist_ok=True)
        os.makedirs(combined_dir, exist_ok=True)
        write_modinfo(name_only_dir, "AGF-VP-NameOnlyRenamed", "1.0.0")
        write_modinfo(ver_only_dir, "AGF-VP-VersionOnly", "1.0.1")
        write_modinfo(combined_dir, "AGF-VP-CombinedNew", "1.0.1")

        planned = plan_mod_folder_renames((mods_dir,), log)
        planned_names = {(old, new) for old, new, _ in planned}
        record(
            "name-only drift is skipped without version bump",
            ("AGF-VP-NameOnly-v1.0.0", "AGF-VP-NameOnlyRenamed-v1.0.0") not in planned_names,
        )
        record(
            "version-only drift is planned",
            ("AGF-VP-VersionOnly-v1.0.0", "AGF-VP-VersionOnly-v1.0.1") in planned_names,
        )
        record(
            "combined drift is planned",
            ("AGF-VP-CombinedOld-v1.0.0", "AGF-VP-CombinedNew-v1.0.1") in planned_names,
        )

    log.info(f"self-test summary: passed={passed}, failed={failed}")
    return failed == 0


def validate_required_paths(strict: bool, log: Logger, mode: str) -> bool:
    ok = True
    required_dirs = []
    required_files = []

    if mode == "sync-work":
        required_dirs = [STAGING, GAME_MODS]
    elif mode == "self-test":
        required_dirs = []
        required_files = []
    elif mode == "update":
        required_dirs = [STAGING, IN_PROGRESS, GAME_MODS]
        required_files = [MOD_README_TEMPLATE]
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
    global FAIL_FAST_ENABLED, CURRENT_TRANSACTION
    FAIL_FAST_ENABLED = args.fail_fast
    CURRENT_TRANSACTION = create_transaction(args.transaction_rollback and not args.dry_run)
    run_success = False

    log = Logger(verbose=args.verbose, dry_run=args.dry_run)
    log.info("Starting SCRIPT-Main automation pipeline")
    log.info(f"Selected mode: {args.mode}")
    log.info("Scope policy: only AGF-/zzzAGF-prefixed mods are managed in workspace and game folders")
    if args.dry_run:
        log.info("Dry-run mode enabled: no file system changes will be written")

    try:
        if args.mode == "self-test":
            ok = run_self_tests(log)
            log_path = log.write_log_file()
            exit_code = 0 if ok and log.stats.errors == 0 else 1
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, exit_code, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            run_success = exit_code == 0
            return exit_code

        if not validate_required_paths(strict=args.strict, log=log, mode=args.mode):
            log.error("Path validation failed in strict mode")
            log_path = log.write_log_file()
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            return 1

        cleanup_game_quarantine(args.quarantine_retention_days, args.dry_run, log)

        if args.enforce_agf_csv and not validate_agf_rows_in_csv(log):
            log_path = log.write_log_file()
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            return 1

        if args.preflight_write_check and not run_writeability_preflight(args.mode, args.dry_run, log):
            log.error("Preflight writeability check failed")
            log_path = log.write_log_file()
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            return 1

        if args.mode == "sync-work":
            enforce_staging_major_policy(args.dry_run, log)
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            sync_staging_and_game(args.dry_run, log)
        elif args.mode == "update":
            log.info(
                "Mode update: sync Draft<->Game, ingest Draft->ActiveBuild, "
                "sync ActiveBuild<->Game, normalize names/readmes — no promote or package."
            )

            pulled_mods_for_pushback: List[Tuple[str, str]] = []

            # 0.25) Pull newer game updates into Draft for overlapping AGF mods.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_game_and_draft(args.dry_run, log))

            # 0.5) Ensure ActiveBuild includes latest Draft copies before game sync.
            sync_draft_to_staging_latest(args.dry_run, log)

            # 0.75) Enforce lane policy after ingest so v0.x stays in Draft.
            enforce_staging_major_policy(args.dry_run, log)

            # 0.9) Resolve name/version folder drift first, then update mod_loaded refs before any game push.
            pre_sync_rename_plan = plan_mod_folder_renames((STAGING, IN_PROGRESS), log)
            if not ensure_notepadpp_closed_for_version_bumps(pre_sync_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pre_sync_staging_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(STAGING,))
            pre_sync_draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            pre_sync_renames = pre_sync_staging_renames + pre_sync_draft_renames
            update_mod_loaded_references_for_renames(pre_sync_renames, args.dry_run, log)

            # 1) Keep ActiveBuild and Game synchronized.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_staging_and_game(args.dry_run, log))

            # 2) Apply naming/readme updates in ActiveBuild.
            staging_renames = prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(STAGING, args.dry_run, log)
            cleanup_older_versions_in_dir(STAGING, args.dry_run, log)

            # 3) Normalize names/metadata in Draft as well.
            post_sync_draft_rename_plan = plan_mod_folder_renames((IN_PROGRESS,), log)
            if not ensure_notepadpp_closed_for_version_bumps(post_sync_draft_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            cleanup_older_versions_in_dir(IN_PROGRESS, args.dry_run, log)
            all_renames = pre_sync_renames + staging_renames + draft_renames
            post_sync_renames = staging_renames + draft_renames
            csv_rows = normalize_compat_csv(
                all_renames,
                args.dry_run,
                log,
                mod_dirs=(STAGING, IN_PROGRESS),
                prune_to_mods_now=True,
            )
            normalize_quote_files(csv_rows, all_renames, args.dry_run, log)
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(STAGING,))
            generate_mod_images(args.dry_run, log)
            copy_mod_images_to_mod_folders(args.dry_run, log, mod_dirs=(STAGING,))
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            update_mod_loaded_references_for_renames(post_sync_renames, args.dry_run, log)
            update_mod_loaded_references_for_renames(all_renames, args.dry_run, log)

            pulled_mods_for_pushback = remap_pulled_mods_after_renames(
                pulled_mods_for_pushback,
                all_renames,
                log,
            )

            # 4) Push back only pulled mods that qualify.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            push_back_pulled_mods(pulled_mods_for_pushback, args.dry_run, log)

            # 5) Recompute pending changes against last published GigglePack state.
            update_gigglepack_pending_changes(
                args.dry_run,
                log,
                args.pending_updates_consolidation_label,
            )

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
                "sync ActiveBuild<->Game, then apply targeted pushback for pulled mods, then finalize/promote/package."
            )

            pulled_mods_for_pushback: List[Tuple[str, str]] = []

            # 0.25) Pull newer game updates into Draft for overlapping AGF mods.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_game_and_draft(args.dry_run, log))

            # 0.5) Ensure ActiveBuild includes latest Draft copies before game sync.
            sync_draft_to_staging_latest(args.dry_run, log)

            # 0.75) Enforce lane policy after ingest so v0.x stays in Draft.
            enforce_staging_major_policy(args.dry_run, log)

            # 0.9) Resolve name/version folder drift first, then update mod_loaded refs before any game push.
            pre_sync_rename_plan = plan_mod_folder_renames((STAGING, IN_PROGRESS), log)
            if not ensure_notepadpp_closed_for_version_bumps(pre_sync_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pre_sync_staging_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(STAGING,))
            pre_sync_draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            pre_sync_renames = pre_sync_staging_renames + pre_sync_draft_renames
            update_mod_loaded_references_for_renames(pre_sync_renames, args.dry_run, log)

            # 1) Keep ActiveBuild and Game synchronized first.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_staging_and_game(args.dry_run, log))

            # 2) Apply naming/readme updates in ActiveBuild.
            staging_renames = prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            update_mod_loaded_references_for_renames(staging_renames, args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(STAGING, args.dry_run, log)
            cleanup_older_versions_in_dir(STAGING, args.dry_run, log)
            generate_mod_images(args.dry_run, log)
            copy_mod_images_to_mod_folders(args.dry_run, log, mod_dirs=(STAGING,))

            # 3) Promote finalized ActiveBuild content to ReleaseSource.
            promote_staging_to_publish_ready(args.dry_run, log)

            # 3.5) Remove stale legacy folder names replaced by AGF-4Modders naming.
            cleanup_release_legacy_4modders_renames(args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(PUBLISH_READY, args.dry_run, log, staging_dir=STAGING)
            cleanup_older_versions_in_dir(PUBLISH_READY, args.dry_run, log)

            # 4) Ensure ReleaseSource + Draft metadata/quotes/readmes are normalized before packaging.
            release_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(PUBLISH_READY,))
            draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            cleanup_older_versions_in_dir(IN_PROGRESS, args.dry_run, log)
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
            update_mod_loaded_references_for_renames(all_renames, args.dry_run, log)

            pulled_mods_for_pushback = remap_pulled_mods_after_renames(
                pulled_mods_for_pushback,
                all_renames,
                log,
            )

            # 5) After names/metadata are finalized, push back only pulled mods that qualify.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            push_back_pulled_mods(pulled_mods_for_pushback, args.dry_run, log)

            # 6) Package and regenerate main README.
            create_all_zips(args.dry_run, args.workers, log)
            generate_gigglepack_release_artifacts(args.dry_run, log)
            generate_main_readme(args.dry_run, log)

        # Mark successful completion before the finally block so transactional
        # rollback is not triggered on successful runs.
        run_success = log.stats.errors == 0
    except Exception as ex:
        log.error(f"Pipeline aborted due to unhandled exception: {ex}")
        log_path = log.write_log_file()
        manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
        if log_path:
            print(f"Log file: {log_path}")
        if manifest_path:
            print(f"Run manifest: {manifest_path}")
        return 1
    finally:
        finalize_transaction(success=run_success, log=log)

    print("\n=== RUN SUMMARY ===")
    for key, value in log.stats.__dict__.items():
        print(f"{key}: {value}")

    action_needed = log.get_action_needed_lines()
    if action_needed:
        print("\n=== ACTION NEEDED ===")
        for line in action_needed:
            print(f"- {line}")

    print("\n=== MOD CHANGES ===")
    for line in log.get_mod_change_summary_lines():
        print(line)

    log_path = log.write_log_file()
    exit_code = 0 if log.stats.errors == 0 else 1
    manifest_path = write_run_manifest(log, args.mode, args.dry_run, exit_code, log_path)
    if log_path:
        print(f"Log file: {log_path}")
    if manifest_path:
        print(f"Run manifest: {manifest_path}")

    run_success = exit_code == 0
    return exit_code


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Main AGF mod automation pipeline")
    parser.add_argument(
        "--mode",
        choices=["full", "update", "sync-work", "prep-work", "promote", "package", "self-test"],
        default="full",
        help="Workflow mode: full pipeline, staging sync, prep active-build readmes, promote, or package output",
    )
    parser.add_argument("--dry-run", action="store_true", help="Preview actions without writing changes")
    parser.add_argument("--verbose", action="store_true", help="Show INFO logs in console")
    parser.add_argument("--strict", action="store_true", help="Fail when required directories/templates are missing")
    parser.add_argument(
        "--fail-fast",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Abort immediately on critical filesystem operation failures",
    )
    parser.add_argument(
        "--transaction-rollback",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Attempt to rollback filesystem operations after an error",
    )
    parser.add_argument(
        "--quarantine-retention-days",
        type=int,
        default=7,
        help="Delete game-removal quarantine entries older than this many days",
    )
    parser.add_argument(
        "--enforce-agf-csv",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Fail if HELPER_ModCompatibility.csv contains non-AGF rows",
    )
    parser.add_argument(
        "--preflight-write-check",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Verify write access to key folders before running",
    )
    parser.add_argument(
        "--workers",
        type=int,
        default=max(2, min(8, os.cpu_count() or 2)),
        help="Worker count for parallel mod zip creation",
    )
    parser.add_argument(
        "--pending-updates-consolidation-label",
        default="",
        help=(
            "Optional one-time label to collapse pending updated_mods into a single bulk entry "
            "(example: AGF-Bulk-ReadMe-Format-Update)"
        ),
    )
    return parser


if __name__ == "__main__":
    arg_parser = build_arg_parser()
    cli_args = arg_parser.parse_args()
    if not acquire_run_lock():
        print(f"Another pipeline run is already active (lock file exists: {RUN_LOCK_PATH}).")
        sys.exit(1)
    try:
        sys.exit(run_pipeline(cli_args))
    finally:
        release_run_lock()
