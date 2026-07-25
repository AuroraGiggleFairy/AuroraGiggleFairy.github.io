"""Audit all release source mods against Nexus and write a durable status report.

Outputs:
- Console tables (same actionable buckets as the markdown report)
- Nexus-Status.md  (human command center)
- Nexus-Status.json (machine-readable for upload tooling)
- Optional: Nexus-Config-Suggestions.json + merge into nexusmods-config.json

Buckets:
- Ready to update (local > Nexus, configured)
- In sync
- On Nexus but missing from config (with paste-ready JSON)
- Not on Nexus (manual create list)
- Needs attention (fetch errors / Nexus newer)
"""
from __future__ import annotations

import argparse
import datetime as dt
import getpass
import json
import os
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import xml.etree.ElementTree as ET
from typing import Dict, List, Optional, Tuple

sys.dont_write_bytecode = True

# ── Paths ────────────────────────────────────────────────────────────────
NEXUS_WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
NEXUS_ROOT_DIR = os.path.dirname(NEXUS_WORKFLOW_DIR)
VS_CODE_ROOT = os.path.dirname(os.path.dirname(NEXUS_ROOT_DIR))
RELEASE_SOURCE_DIR = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
ZIP_OUTPUT_DIR = os.path.join(VS_CODE_ROOT, "04_DownloadZips")
CONFIG_PATH = os.path.join(NEXUS_WORKFLOW_DIR, "nexusmods-config.json")
# Human-facing status lives at NexusMods root; machine JSON stays in Workflow.
STATUS_MD_PATH = os.path.join(NEXUS_ROOT_DIR, "Nexus-Status.md")
STATUS_JSON_PATH = os.path.join(NEXUS_WORKFLOW_DIR, "Nexus-Status.json")
SUGGESTIONS_PATH = os.path.join(NEXUS_WORKFLOW_DIR, "Nexus-Config-Suggestions.json")
PRIVATE_API_KEY_PATH = os.path.join(NEXUS_ROOT_DIR, "nexus-api-key.private.txt")
PUBLISHHELP_DIR = os.path.join(NEXUS_ROOT_DIR, "PublishHelp")

# ── API setup ────────────────────────────────────────────────────────────
API_KEY_ENV_VAR = "AGF_NEXUSMODS_API_KEY"
HEADERS = {
    "accept": "application/json",
    "apikey": "",
    "Application-Name": "AGF-NexusMods-Automation",
    "Application-Version": "0.1.0",
    "User-Agent": "AGF-NexusMods-Automation/0.1.0",
}
GAME_DOMAIN = "7daystodie"
API_V1_BASE = "https://api.nexusmods.com/v1"
API_V3_BASE = "https://api.nexusmods.com/v3"
API_GRAPHQL_URL = "https://api.nexusmods.com/v2/graphql"
AGF_AUTHOR_NAMES = ("AuroraGiggleFairy", "auroragigglefairy", "AuroraGiggleFairyAGF", "GiggleFairy")
AGF_PREFIXES = ("AGF-", "zzzAGF-")
HIGH_CONFIDENCE_SCORE = 0.8


# ── Helpers ──────────────────────────────────────────────────────────────

def get_base_mod_name(name: str) -> str:
    return re.sub(r"-v\d+(?:\.\d+)*$", "", name)


def parse_version_from_folder(folder: str) -> str:
    match = re.search(r"-v(\d+(?:\.\d+)+)$", folder)
    return match.group(1) if match else "?"


def parse_local_version(folder_path: str, folder_name: str) -> str:
    modinfo_path = os.path.join(folder_path, "ModInfo.xml")
    try:
        root = ET.parse(modinfo_path).getroot()
        for child in root:
            if child.tag.lower() == "version":
                version = child.attrib.get("value", "").strip()
                if version:
                    return version
    except (OSError, ET.ParseError):
        pass
    return parse_version_from_folder(folder_name)


def parse_version_parts(version: str) -> Tuple[int, int, int]:
    nums = [int(part) for part in re.findall(r"\d+", version or "")]
    while len(nums) < 3:
        nums.append(0)
    return nums[0], nums[1], nums[2]


def compare_versions(left: str, right: str) -> int:
    left_parts = parse_version_parts(left)
    right_parts = parse_version_parts(right)
    if left_parts < right_parts:
        return -1
    if left_parts > right_parts:
        return 1
    return 0


def safe_int(val) -> int:
    try:
        return int(val)
    except Exception:
        return 0


def request_json(url: str, method: str = "GET", body: Optional[dict] = None):
    payload = None if body is None else json.dumps(body).encode("utf-8")
    headers = dict(HEADERS)
    if payload is not None:
        headers["content-type"] = "application/json"
    req = urllib.request.Request(url, headers=headers, method=method, data=payload)
    with urllib.request.urlopen(req, timeout=30) as resp:
        raw = resp.read()
    return json.loads(raw.decode("utf-8")) if raw else {}


def extract_data_payload(payload: object) -> object:
    if isinstance(payload, dict) and "data" in payload:
        return payload.get("data")
    return payload


def author_matches(result: dict) -> bool:
    for field in ("author", "username", "uploader", "uploaded_by", "author_name", "uploaded_by_name"):
        raw = result.get(field, None)
        if raw is not None:
            text = str(raw).strip()
            if text and any(name.lower() in text.lower() for name in AGF_AUTHOR_NAMES):
                return True
    return False


def find_zip_path(base_name: str) -> str:
    candidate = os.path.join(ZIP_OUTPUT_DIR, f"{base_name}.zip")
    if os.path.isfile(candidate):
        return candidate
    if not os.path.isdir(ZIP_OUTPUT_DIR):
        return ""
    prefix = f"{base_name}".lower()
    for entry in os.listdir(ZIP_OUTPUT_DIR):
        if not entry.lower().endswith(".zip"):
            continue
        stem = entry[:-4]
        if get_base_mod_name(stem).lower() == prefix:
            return os.path.join(ZIP_OUTPUT_DIR, entry)
    return ""


def page_url_for(nexus_mod_id: int) -> str:
    if nexus_mod_id <= 0:
        return ""
    return f"https://www.nexusmods.com/{GAME_DOMAIN}/mods/{nexus_mod_id}"


def fetch_known_mod_info(nexus_mod_id: int) -> dict:
    result: Dict[str, object] = {}
    try:
        data = request_json(f"{API_V3_BASE}/games/{GAME_DOMAIN}/mods/{nexus_mod_id}")
        payload = extract_data_payload(data)
        if isinstance(payload, dict):
            result.update(payload)
    except Exception:
        pass
    try:
        data = request_json(f"{API_V1_BASE}/games/{GAME_DOMAIN}/mods/{nexus_mod_id}.json")
        if isinstance(data, dict):
            result.update(data)
    except Exception:
        pass
    return result


def fetch_mod_files(mod_id: str) -> List[dict]:
    """Return mod files / update groups for a mod (v3 first)."""
    if not mod_id:
        return []
    try:
        payload = request_json(f"{API_V3_BASE}/mods/{mod_id}/files")
        data = extract_data_payload(payload)
        if isinstance(data, dict):
            mod_files = data.get("mod_files", [])
            if isinstance(mod_files, list):
                return [item for item in mod_files if isinstance(item, dict)]
    except urllib.error.HTTPError as ex:
        if ex.code not in (404, 403):
            print(f"  [FILES ERROR] HTTP {ex.code} for mod_id={mod_id}")
    except Exception as ex:
        print(f"  [FILES ERROR] {ex} for mod_id={mod_id}")
    return []


def pick_update_group(mod_files: List[dict], preferred_id: str = "") -> Tuple[str, str]:
    if preferred_id:
        for item in mod_files:
            if str(item.get("id", "")).strip() == preferred_id:
                return preferred_id, str(item.get("name", "")).strip()
    if not mod_files:
        return "", ""
    active = next((item for item in mod_files if bool(item.get("is_active", False))), None)
    chosen = active or mod_files[0]
    return str(chosen.get("id", "")).strip(), str(chosen.get("name", "")).strip()


def build_publishhelp_display_name(mod_name: str, game_ver: str = "3") -> str:
    """Same naming rules as SCRIPT-NexusPublishHelp.build_display_name / Details.md titles."""
    parts = mod_name.split("-", 2)
    if len(parts) >= 3:
        name_part = parts[2].replace("-", " ")
        name_display = f"{parts[1]} - {name_part}"
    else:
        name_display = mod_name.replace("-", " ")
    game_token = str(game_ver or "3").strip() or "3"
    return f"AGF - V{game_token} - {name_display}"


def read_publishhelp_title(base_name: str) -> str:
    """Read the Nexus page/file title from PublishHelp/<mod>/Details.md when present."""
    details_path = os.path.join(PUBLISHHELP_DIR, base_name, "Details.md")
    if not os.path.isfile(details_path):
        return ""
    try:
        with open(details_path, "r", encoding="utf-8") as handle:
            text = handle.read()
    except OSError:
        return ""
    match = re.search(r"```text\s*\n([^\n]+)\n```", text)
    if not match:
        return ""
    title = match.group(1).strip()
    return title if title.upper().startswith("AGF") else ""


def expected_nexus_title(base_name: str) -> str:
    return read_publishhelp_title(base_name) or build_publishhelp_display_name(base_name)


def normalize_nexus_title(value: str) -> str:
    """Collapse PublishHelp/Nexus titles for comparison: keep alphanumerics only."""
    text = str(value or "").lower()
    text = re.sub(r"^agf\s*-\s*v\d+\s*-\s*", "", text)
    text = re.sub(r"^agf\s+", "", text)
    return re.sub(r"[^a-z0-9]+", "", text)


def request_graphql(query: str, variables: Optional[dict] = None) -> dict:
    body = {"query": query, "variables": variables or {}}
    payload = request_json(API_GRAPHQL_URL, method="POST", body=body)
    return payload if isinstance(payload, dict) else {}


def graphql_list_agf_mods() -> List[dict]:
    """List AGF 7DTD mods via GraphQL (author filter + AGF name wildcard)."""
    query = """
    query AGFMods($filter: ModsFilter, $count: Int, $offset: Int) {
      mods(filter: $filter, count: $count, offset: $offset) {
        totalCount
        nodes {
          modId
          name
          version
          author
          uploader { name }
        }
      }
    }
    """
    collected: List[dict] = []
    seen_ids: set = set()

    filter_sets = [
        {
            "gameDomainName": [{"value": GAME_DOMAIN, "op": "EQUALS"}],
            "author": [{"value": "AuroraGiggleFairy", "op": "EQUALS"}],
        },
        {
            "gameDomainName": [{"value": GAME_DOMAIN, "op": "EQUALS"}],
            "name": [{"value": "AGF*", "op": "WILDCARD"}],
        },
        {
            "gameDomainName": [{"value": GAME_DOMAIN, "op": "EQUALS"}],
            "nameStemmed": [{"value": "AGF", "op": "MATCHES"}],
        },
    ]

    for filter_body in filter_sets:
        offset = 0
        page_size = 50
        while offset < 500:
            try:
                payload = request_graphql(
                    query,
                    {"filter": filter_body, "count": page_size, "offset": offset},
                )
            except Exception as ex:
                print(f"  [GRAPHQL ERROR] {ex}")
                break
            errors = payload.get("errors")
            if isinstance(errors, list) and errors:
                msg = str(errors[0].get("message", errors[0]))[:200]
                print(f"  [GRAPHQL ERROR] {msg}")
                break
            data = payload.get("data", {})
            mods_page = data.get("mods", {}) if isinstance(data, dict) else {}
            nodes = mods_page.get("nodes", []) if isinstance(mods_page, dict) else []
            if not isinstance(nodes, list) or not nodes:
                break
            for node in nodes:
                if not isinstance(node, dict):
                    continue
                author = str(node.get("author", "") or "")
                uploader = ""
                uploader_obj = node.get("uploader")
                if isinstance(uploader_obj, dict):
                    uploader = str(uploader_obj.get("name", "") or "")
                title = str(node.get("name", "") or "").strip()
                if not any(
                    name.lower() in author.lower() or name.lower() in uploader.lower()
                    for name in AGF_AUTHOR_NAMES
                ):
                    if not title.upper().startswith("AGF"):
                        continue
                mod_id = safe_int(node.get("modId", node.get("id", 0)))
                if mod_id <= 0 or mod_id in seen_ids:
                    continue
                seen_ids.add(mod_id)
                collected.append(
                    {
                        "mod_id": mod_id,
                        "id": mod_id,
                        "name": title,
                        "version": str(node.get("version", "") or "").strip(),
                        "author": author or uploader,
                        "uploaded_by": uploader or author,
                    }
                )
            if len(nodes) < page_size:
                break
            offset += page_size
            time.sleep(0.2)

    return collected


def search_nexus(query: str) -> list:
    """v1 search using the current `search=` parameter (not deprecated `name=`)."""
    encoded = urllib.parse.quote(query)
    # Current Nexus v1 search param is `search=` (modde/docs). Old `name=` returns HTTP 422.
    urls = [
        f"{API_V1_BASE}/games/{GAME_DOMAIN}/mods/search.json?search={encoded}",
        f"{API_V1_BASE}/games/{GAME_DOMAIN}/mods/search.json?search={encoded}&page=1",
    ]
    for url in urls:
        try:
            data = request_json(url)
            results = data if isinstance(data, list) else (
                data.get("data", []) if isinstance(data, dict) and "data" in data else []
            )
            if not isinstance(results, list):
                continue
            return [row for row in results if isinstance(row, dict) and author_matches(row)]
        except urllib.error.HTTPError as ex:
            if ex.code in (404, 422, 501):
                continue
            print(f"  [SEARCH ERROR] HTTP {ex.code} for '{query}'")
            return []
        except Exception as ex:
            print(f"  [SEARCH ERROR] {ex} for '{query}'")
            return []
    return []


def score_match(result_name: str, mod_name: str, expected_title: str = "") -> float:
    """Score Nexus page title against folder base name and PublishHelp title."""
    rn = str(result_name or "").strip()
    expected = (expected_title or build_publishhelp_display_name(mod_name)).strip()

    rn_norm = normalize_nexus_title(rn)
    expected_norm = normalize_nexus_title(expected)
    if not rn_norm or not expected_norm:
        return 0.0
    if rn_norm == expected_norm:
        return 1.0
    if expected_norm in rn_norm or rn_norm in expected_norm:
        len_ratio = min(len(expected_norm), len(rn_norm)) / max(len(expected_norm), len(rn_norm))
        return 0.85 + (0.14 * len_ratio)

    expected_tokens = set(re.findall(r"[a-z0-9]+", expected.lower()))
    result_tokens = set(re.findall(r"[a-z0-9]+", rn.lower()))
    skip = {"agf", "v", "v3", "v2", "v1", "plus", "the", "a", "an", "and", "or", "of", "for", "to", "in", "mod"}
    expected_tokens -= skip
    result_tokens -= skip
    if expected_tokens and result_tokens:
        overlap = expected_tokens & result_tokens
        recall = len(overlap) / len(expected_tokens)
        precision = len(overlap) / len(result_tokens)
        if recall > 0 and precision > 0:
            f1 = 2 * (precision * recall) / (precision + recall)
            return f1 * 0.9

    mod_base = mod_name.lower()
    for prefix in ("zzzagf-", "agf-"):
        if mod_base.startswith(prefix):
            mod_base = mod_base[len(prefix):]
            break
    mod_base_norm = re.sub(r"[^a-z0-9]+", "", mod_base)
    if mod_base_norm and mod_base_norm in rn_norm:
        len_ratio = min(len(mod_base_norm), len(rn_norm)) / max(len(mod_base_norm), len(rn_norm))
        return 0.7 + (0.2 * len_ratio)
    return 0.0


def generate_search_keywords(base_name: str) -> List[str]:
    """Prefer PublishHelp Nexus titles, then short distinctive fragments."""
    keywords: List[str] = []
    title = expected_nexus_title(base_name)
    if title:
        keywords.append(title)
        stripped = re.sub(r"^AGF\s*-\s*V\d+\s*-\s*", "", title, flags=re.IGNORECASE).strip()
        if stripped and stripped not in keywords:
            keywords.append(stripped)
    built = build_publishhelp_display_name(base_name)
    if built not in keywords:
        keywords.append(built)

    text = base_name
    for prefix in ("zzzAGF-", "AGF-"):
        if text.startswith(prefix):
            text = text[len(prefix):]
            break
    parts = [part for part in re.split(r"[-_/\\]+", text) if part]
    if len(parts) >= 2:
        keywords.append(f"{parts[0]} {parts[-1]}")
    if parts:
        keywords.append(parts[-1])

    seen = set()
    unique: List[str] = []
    for item in keywords:
        key = item.lower()
        if key in seen or not item.strip():
            continue
        seen.add(key)
        unique.append(item)
    return unique


def load_api_key() -> str:
    api_key = os.getenv(API_KEY_ENV_VAR, "").strip()
    if api_key:
        return api_key
    if os.path.isfile(PRIVATE_API_KEY_PATH):
        try:
            with open(PRIVATE_API_KEY_PATH, "r", encoding="utf-8") as handle:
                api_key = handle.readline().strip()
            if api_key:
                return api_key
        except OSError:
            pass
    print("Nexus API key is not configured.")
    print("Paste it below for this check only; it will not be displayed or saved.")
    try:
        return getpass.getpass("Nexus API key: ").strip()
    except (EOFError, KeyboardInterrupt):
        print("\nVersion check cancelled.")
        return ""


STATUS_NEEDS_UPDATE = "Needs Update"
STATUS_MATCHES = "Matches"
STATUS_FIRST_UPLOAD = "First Upload Needed"

STATUS_SORT_ORDER = {
    STATUS_NEEDS_UPDATE: 0,
    STATUS_MATCHES: 1,
    STATUS_FIRST_UPLOAD: 2,
}


def format_generated_at(when: Optional[dt.datetime] = None) -> str:
    """Month-day-year with 12-hour time, no seconds. Example: 07-25-2026 2:54 AM"""
    stamp = when or dt.datetime.now()
    hour = stamp.strftime("%I").lstrip("0") or "12"
    return f"{stamp.strftime('%m-%d-%Y')} {hour}:{stamp.strftime('%M %p')}"


def classify_mod(mod: dict) -> str:
    """Return one of: Needs Update | Matches | First Upload Needed."""
    on_nexus = bool(mod.get("configured")) or bool(mod.get("found"))
    if not on_nexus:
        return STATUS_FIRST_UPLOAD

    local_ver = str(mod.get("local_version", "?"))
    nexus_ver = str(mod.get("live_version", "-"))
    if nexus_ver in ("?", "ERROR", "", "-", "—"):
        return STATUS_NEEDS_UPDATE
    cmp = compare_versions(local_ver, nexus_ver)
    if cmp == 0:
        return STATUS_MATCHES
    return STATUS_NEEDS_UPDATE


def status_label(bucket: str, mod: dict) -> str:
    return bucket


def recommended_config_snippet(mod: dict) -> dict:
    nexus_id = safe_int(mod.get("nexus_mod_id", 0))
    update_group_id = str(mod.get("update_group_id", "") or "").strip()
    update_group_name = str(mod.get("update_group_name", "") or "").strip()
    return {
        "intent": "update",
        "nexus_mod_id": nexus_id,
        "update_group_id": update_group_id,
        "update_group_name": update_group_name,
        "file_category": "main",
        "legacy_latest_file_id": 0,
        "legacy_name_hint": "",
        "page_url": page_url_for(nexus_id),
        "tested_game_version_override": "",
        "brief_overview_override": "",
        "detailed_description_override": "",
        "file_description_override": "",
        "summary_override": "",
        "description_override": "",
    }


def enrich_with_update_group(mod: dict) -> None:
    """Fetch mod files and fill update_group_id/name when possible."""
    config_entry = mod.get("config_entry") if isinstance(mod.get("config_entry"), dict) else {}
    preferred = str(config_entry.get("update_group_id", "") or "").strip()
    if preferred and not mod.get("update_group_id"):
        mod["update_group_id"] = preferred
        mod["update_group_name"] = str(config_entry.get("update_group_name", "") or "").strip()

    nexus_mod_id = safe_int(mod.get("nexus_mod_id", 0))
    if nexus_mod_id <= 0:
        return

    # Prefer live/game-scoped id for /mods/{id}/files when available.
    live_id = str(mod.get("live_id", "") or "").strip() or str(nexus_mod_id)
    files = fetch_mod_files(live_id)
    if not files and live_id != str(nexus_mod_id):
        files = fetch_mod_files(str(nexus_mod_id))
    group_id, group_name = pick_update_group(files, preferred)
    if group_id:
        mod["update_group_id"] = group_id
        mod["update_group_name"] = group_name
        mod["mod_files_count"] = len(files)
    else:
        mod.setdefault("update_group_id", preferred)
        mod.setdefault("update_group_name", str(config_entry.get("update_group_name", "") or "").strip())
        mod["mod_files_count"] = len(files)


def build_status_rows(mods: List[dict]) -> List[dict]:
    rows: List[dict] = []
    for mod in mods:
        bucket = classify_mod(mod)
        nexus_id = safe_int(mod.get("nexus_mod_id", 0))
        zip_path = str(mod.get("zip_path", "") or "")
        row = {
            "base_name": mod["base_name"],
            "folder": mod["folder"],
            "local_version": mod["local_version"],
            "expected_title": str(mod.get("expected_title", "") or expected_nexus_title(mod["base_name"])),
            "configured": bool(mod.get("configured")),
            "found": bool(mod.get("found")) if not mod.get("configured") else True,
            "nexus_mod_id": nexus_id if nexus_id > 0 else 0,
            "live_name": str(mod.get("live_name", "") or ""),
            "live_version": str(mod.get("live_version", "-") or "-"),
            "live_id": str(mod.get("live_id", "") or ""),
            "page_url": page_url_for(nexus_id) if nexus_id > 0 else "",
            "score": float(mod.get("score", 0) or 0),
            "update_group_id": str(mod.get("update_group_id", "") or ""),
            "update_group_name": str(mod.get("update_group_name", "") or ""),
            "zip_path": zip_path,
            "zip_present": bool(zip_path and os.path.isfile(zip_path)),
            "bucket": bucket,
            "status": status_label(bucket, mod),
        }
        rows.append(row)
    return rows


def write_status_json(rows: List[dict], summary: dict) -> None:
    payload = {
        "generated_at": format_generated_at(),
        "game_domain": GAME_DOMAIN,
        "release_source_dir": RELEASE_SOURCE_DIR,
        "zip_output_dir": ZIP_OUTPUT_DIR,
        "config_path": CONFIG_PATH,
        "summary": summary,
        "mods": rows,
        "buckets": {
            "needs_update": [row for row in rows if row["status"] == STATUS_NEEDS_UPDATE],
            "matches": [row for row in rows if row["status"] == STATUS_MATCHES],
            "first_upload_needed": [row for row in rows if row["status"] == STATUS_FIRST_UPLOAD],
        },
    }
    with open(STATUS_JSON_PATH, "w", encoding="utf-8") as handle:
        json.dump(payload, handle, indent=2, ensure_ascii=True)
        handle.write("\n")


def write_status_markdown(rows: List[dict], summary: dict) -> None:
    lines: List[str] = []
    lines.append("# Nexus Status Report")
    lines.append("")
    lines.append(f"Generated: {format_generated_at()}")
    lines.append("")
    lines.append("## Summary")
    lines.append("")
    lines.append(f"- Total: **{summary['total']}**")
    lines.append(f"- Needs Update: **{summary['needs_update']}**")
    lines.append(f"- Matches: **{summary['matches']}**")
    lines.append(f"- First Upload Needed: **{summary['first_upload_needed']}**")
    if summary.get("missing_from_config", 0):
        lines.append(
            f"- Still unsaved / low-confidence matches: **{summary['missing_from_config']}** "
            "(re-check names, or add IDs manually)"
        )
    lines.append("")
    lines.append("## Status")
    lines.append("")
    lines.append("| Mod | Local | Nexus | Status |")
    lines.append("|---|---|---|---|")

    sorted_rows = sorted(
        rows,
        key=lambda row: (
            STATUS_SORT_ORDER.get(str(row.get("status", "")), 99),
            str(row.get("base_name", "")),
        ),
    )
    for row in sorted_rows:
        nexus_ver = row["live_version"] if row["status"] != STATUS_FIRST_UPLOAD else "-"
        lines.append(
            f"| `{row['base_name']}` | {row['local_version']} | {nexus_ver} | {row['status']} |"
        )
    lines.append("")

    with open(STATUS_MD_PATH, "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines))


def write_config_suggestions(rows: List[dict]) -> dict:
    suggestions: Dict[str, object] = {
        "generated_at": format_generated_at(),
        "high_confidence_threshold": HIGH_CONFIDENCE_SCORE,
        "mods": {},
    }
    mods_out: Dict[str, object] = {}
    for row in rows:
        # Found on Nexus but not yet saved in nexusmods-config.json
        if (not row["configured"]) and row["found"] and row["nexus_mod_id"]:
            mods_out[row["base_name"]] = {
                "action": "add",
                "score": row["score"],
                "live_name": row["live_name"],
                "live_version": row["live_version"],
                "config": recommended_config_snippet(row),
            }
            continue
        # Fill missing update_group_id for already-configured mods.
        if row["configured"] and row["nexus_mod_id"] and row["update_group_id"]:
            mods_out[row["base_name"]] = {
                "action": "patch_update_group",
                "score": 1.0,
                "config": {
                    "update_group_id": row["update_group_id"],
                    "update_group_name": row["update_group_name"],
                    "page_url": row["page_url"] or page_url_for(row["nexus_mod_id"]),
                },
            }
    suggestions["mods"] = mods_out
    with open(SUGGESTIONS_PATH, "w", encoding="utf-8") as handle:
        json.dump(suggestions, handle, indent=2, ensure_ascii=True)
        handle.write("\n")
    return suggestions


def apply_config_suggestions(min_score: float = HIGH_CONFIDENCE_SCORE, dry_run: bool = False) -> int:
    if not os.path.isfile(SUGGESTIONS_PATH):
        print(f"Suggestions file not found: {SUGGESTIONS_PATH}")
        print("Run the status audit first (RUN-Nexus-Status.bat).")
        return 1
    with open(SUGGESTIONS_PATH, "r", encoding="utf-8") as handle:
        suggestions = json.load(handle)
    config = {"mods": {}}
    if os.path.isfile(CONFIG_PATH):
        with open(CONFIG_PATH, "r", encoding="utf-8") as handle:
            loaded = json.load(handle)
        if isinstance(loaded, dict):
            config = loaded
    mods = config.setdefault("mods", {})
    if not isinstance(mods, dict):
        mods = {}
        config["mods"] = mods

    suggestion_mods = suggestions.get("mods", {})
    if not isinstance(suggestion_mods, dict):
        print("Suggestions file has no mods.")
        return 1

    added = 0
    patched = 0
    skipped = 0
    for base_name, suggestion in sorted(suggestion_mods.items()):
        if not isinstance(suggestion, dict):
            continue
        action = str(suggestion.get("action", "")).strip()
        score = float(suggestion.get("score", 0) or 0)
        cfg = suggestion.get("config", {})
        if not isinstance(cfg, dict):
            continue

        if action == "add":
            if score < min_score:
                skipped += 1
                print(f"[SKIP] {base_name}: score {score:.2f} < {min_score:.2f}")
                continue
            if base_name in mods and safe_int(mods[base_name].get("nexus_mod_id", 0)) > 0:
                skipped += 1
                print(f"[SKIP] {base_name}: already configured")
                continue
            print(f"[ADD] {base_name}: nexus_mod_id={cfg.get('nexus_mod_id')} score={score:.2f}")
            if not dry_run:
                mods[base_name] = cfg
            added += 1
            continue

        if action == "patch_update_group":
            existing = mods.get(base_name)
            if not isinstance(existing, dict):
                skipped += 1
                continue
            current_group = str(existing.get("update_group_id", "") or "").strip()
            new_group = str(cfg.get("update_group_id", "") or "").strip()
            if not new_group or current_group == new_group:
                skipped += 1
                continue
            print(f"[PATCH] {base_name}: update_group_id {current_group or '(empty)'} -> {new_group}")
            if not dry_run:
                existing["update_group_id"] = new_group
                if cfg.get("update_group_name"):
                    existing["update_group_name"] = cfg["update_group_name"]
                if cfg.get("page_url") and not str(existing.get("page_url", "")).strip():
                    existing["page_url"] = cfg["page_url"]
            patched += 1

    print()
    print(f"Added: {added} | Patched update groups: {patched} | Skipped: {skipped}")
    if dry_run:
        print("[DRYRUN] No changes written to nexusmods-config.json")
        return 0

    # Preserve top-level keys; ensure api_base_url is v3.
    if not str(config.get("api_base_url", "")).strip():
        config["api_base_url"] = API_V3_BASE
    with open(CONFIG_PATH, "w", encoding="utf-8") as handle:
        json.dump(config, handle, indent=2, ensure_ascii=True)
        handle.write("\n")
    print(f"Wrote {CONFIG_PATH}")
    return 0


def print_console_report(rows: List[dict], summary: dict) -> None:
    print(f"\n{'=' * 100}")
    print(f"  NEXUS STATUS — {summary['total']} mods  |  {format_generated_at()}")
    print(f"{'=' * 100}")
    print(f"\n{'Mod Name':<50} {'Local':<12} {'Nexus':<12} {'Status'}")
    print("-" * 100)
    sorted_rows = sorted(
        rows,
        key=lambda row: (
            STATUS_SORT_ORDER.get(str(row.get("status", "")), 99),
            str(row.get("base_name", "")),
        ),
    )
    for row in sorted_rows:
        nexus_ver = row["live_version"] if row["status"] != STATUS_FIRST_UPLOAD else "-"
        print(
            f"{row['base_name']:<50} {row['local_version']:<12} {str(nexus_ver):<12} {row['status']}"
        )
    print("-" * 100)
    print("\n  SUMMARY:")
    print(f"    Total:                 {summary['total']}")
    print(f"    Needs Update:          {summary['needs_update']}")
    print(f"    Matches:               {summary['matches']}")
    print(f"    First Upload Needed:   {summary['first_upload_needed']}")
    if summary.get("missing_from_config", 0):
        print(f"    Not saved in config:   {summary['missing_from_config']}")
    print()
    print(f"  Wrote: {STATUS_MD_PATH}")
    print(f"  Wrote: {STATUS_JSON_PATH}")
    print(f"  Wrote: {SUGGESTIONS_PATH}")


def run_audit(save_config: bool = True) -> int:
    api_key = load_api_key()
    if not api_key:
        print("No API key entered; version check cancelled.")
        return 1
    HEADERS["apikey"] = api_key

    config = {"mods": {}}
    if os.path.isfile(CONFIG_PATH):
        with open(CONFIG_PATH, "r", encoding="utf-8") as handle:
            loaded = json.load(handle)
        if isinstance(loaded, dict):
            config = loaded
    config_mods = config.get("mods", {})
    if not isinstance(config_mods, dict):
        config_mods = {}

    mods: List[dict] = []
    if not os.path.isdir(RELEASE_SOURCE_DIR):
        print(f"Release source directory not found: {RELEASE_SOURCE_DIR}")
        return 1

    for entry in sorted(os.listdir(RELEASE_SOURCE_DIR)):
        folder_path = os.path.join(RELEASE_SOURCE_DIR, entry)
        if not os.path.isdir(folder_path):
            continue
        if not entry.startswith(AGF_PREFIXES):
            continue
        base_name = get_base_mod_name(entry)
        local_version = parse_local_version(folder_path, entry)
        config_entry = config_mods.get(base_name, {})
        if not isinstance(config_entry, dict):
            config_entry = {}
        nexus_mod_id = safe_int(config_entry.get("nexus_mod_id", 0))
        zip_path = find_zip_path(base_name)
        mods.append(
            {
                "folder": entry,
                "base_name": base_name,
                "local_version": local_version,
                "configured": nexus_mod_id > 0,
                "nexus_mod_id": nexus_mod_id,
                "config_entry": config_entry,
                "zip_path": zip_path,
                "update_group_id": str(config_entry.get("update_group_id", "") or "").strip(),
                "update_group_name": str(config_entry.get("update_group_name", "") or "").strip(),
                "expected_title": expected_nexus_title(base_name),
            }
        )

    total = len(mods)
    print(f"\n{'=' * 100}")
    print(f"  NEXUS AUDIT: Scanning {total} AGF release source mods against Nexus")
    print(f"{'=' * 100}")

    print(f"\n-- Phase 1: Fetching live data for {sum(1 for m in mods if m['configured'])} configured mods --\n")
    for mod in mods:
        if not mod["configured"]:
            continue
        nexus_id = mod["nexus_mod_id"]
        print(f"  Fetching mod_id={nexus_id} ({mod['base_name']})...", end=" ")
        try:
            live = fetch_known_mod_info(nexus_id)
            live_version = str(live.get("version", live.get("mod_version", ""))).strip() or "?"
            live_name = str(live.get("name", "")).strip() or "?"
            live_id = str(live.get("id", live.get("mod_id", nexus_id))).strip()
            mod["live_version"] = live_version
            mod["live_name"] = live_name
            mod["live_id"] = live_id
            print(f"OK - '{live_name}' v{live_version}")
        except Exception as ex:
            mod["live_version"] = "ERROR"
            mod["live_name"] = "ERROR"
            mod["live_id"] = str(nexus_id)
            print(f"FAILED: {ex}")
        enrich_with_update_group(mod)

    unconfigured = [mod for mod in mods if not mod["configured"]]
    print(f"\n-- Phase 2: Discovering Nexus pages for {len(unconfigured)} unconfigured mods --")
    print("  Matching against PublishHelp Details.md titles (e.g. 'AGF - V3 - VP - BedrollPlus').\n")

    print("  GraphQL author/name discovery...", end=" ")
    unique_broad = graphql_list_agf_mods()
    print(f"{len(unique_broad)} mods")

    if len(unique_broad) < 5:
        print("  GraphQL returned few results; trying v1 search= fallback...")
        for query in ("AGF", "AGF - V3", "AuroraGiggleFairy"):
            print(f"  v1 search '{query}'...", end=" ")
            try:
                results = search_nexus(query)
                print(f"{len(results)} results")
                for row in results:
                    rid = safe_int(row.get("mod_id", row.get("id", 0)))
                    if rid <= 0:
                        continue
                    if any(safe_int(existing.get("mod_id", 0)) == rid for existing in unique_broad):
                        continue
                    unique_broad.append(row)
            except Exception as ex:
                print(f"ERROR: {ex}")
        print(f"  Total unique discovery results: {len(unique_broad)}")

    # Best match per local mod from the discovery pool.
    broad_matched: Dict[str, dict] = {}
    used_nexus_ids: set = set()
    for mod in unconfigured:
        expected = str(mod.get("expected_title", "") or expected_nexus_title(mod["base_name"]))
        best_score = 0.0
        best_row = None
        for row in unique_broad:
            rid = safe_int(row.get("mod_id", row.get("id", 0)))
            if rid <= 0 or rid in used_nexus_ids:
                continue
            rname = str(row.get("name", "")).strip()
            score = score_match(rname, mod["base_name"], expected_title=expected)
            if score > best_score:
                best_score = score
                best_row = row
        if best_row and best_score >= 0.7:
            rid = safe_int(best_row.get("mod_id", best_row.get("id", 0)))
            used_nexus_ids.add(rid)
            broad_matched[mod["base_name"]] = {
                "nexus_mod_id": rid,
                "live_name": str(best_row.get("name", "")).strip(),
                "live_version": str(best_row.get("version", best_row.get("mod_version", ""))).strip(),
                "score": best_score,
                "expected_title": expected,
            }
            print(
                f"  [MATCH] {mod['base_name']} -> '{best_row.get('name')}' "
                f"(id={rid}, score={best_score:.2f}) | expected '{expected}'"
            )

    still_needed = [mod for mod in unconfigured if mod["base_name"] not in broad_matched]
    if still_needed:
        print(f"\n  Per-mod search for {len(still_needed)} unmatched mods (PublishHelp titles)...\n")
    for mod in still_needed:
        expected = str(mod.get("expected_title", "") or expected_nexus_title(mod["base_name"]))
        keywords = generate_search_keywords(mod["base_name"])
        best_score = 0.0
        best_result = None
        for keyword in keywords:
            try:
                results = search_nexus(keyword)
            except Exception as ex:
                print(f"  [SEARCH ERROR] {ex} for '{keyword}'")
                results = []
            for row in results:
                rid = safe_int(row.get("mod_id", row.get("id", 0)))
                if rid in used_nexus_ids:
                    continue
                score = score_match(str(row.get("name", "")).strip(), mod["base_name"], expected_title=expected)
                if score > best_score:
                    best_score = score
                    best_result = row
            if best_score >= HIGH_CONFIDENCE_SCORE:
                break
            time.sleep(0.35)

        if best_result and best_score >= 0.5:
            rname = str(best_result.get("name", "")).strip()
            rid = safe_int(best_result.get("mod_id", best_result.get("id", 0)))
            rversion = str(best_result.get("version", best_result.get("mod_version", ""))).strip()
            used_nexus_ids.add(rid)
            broad_matched[mod["base_name"]] = {
                "nexus_mod_id": rid,
                "live_name": rname,
                "live_version": rversion,
                "score": best_score,
                "expected_title": expected,
            }
            label = "STRONG" if best_score >= HIGH_CONFIDENCE_SCORE else "POSSIBLE"
            print(f"  [{label}] {mod['base_name']} -> '{rname}' (id={rid}, v{rversion}, score={best_score:.2f})")
        else:
            print(f"  [NOT FOUND] {mod['base_name']} | expected '{expected}'")

    for mod in unconfigured:
        match = broad_matched.get(mod["base_name"])
        if match:
            mod["nexus_mod_id"] = match["nexus_mod_id"]
            mod["live_version"] = match["live_version"] or "?"
            mod["live_name"] = match["live_name"]
            mod["live_id"] = str(match["nexus_mod_id"])
            mod["found"] = True
            mod["score"] = match["score"]
            # Resolve the API mod id (may differ from game-scoped id) before file-group lookup.
            try:
                live = fetch_known_mod_info(mod["nexus_mod_id"])
                if live:
                    live_version = str(live.get("version", live.get("mod_version", ""))).strip()
                    if live_version:
                        mod["live_version"] = live_version
                    live_name = str(live.get("name", "")).strip()
                    if live_name:
                        mod["live_name"] = live_name
                    live_id = str(live.get("id", live.get("mod_id", mod["nexus_mod_id"]))).strip()
                    if live_id:
                        mod["live_id"] = live_id
            except Exception:
                pass
            enrich_with_update_group(mod)
        else:
            mod["found"] = False
            mod["live_version"] = "-"
            mod["live_name"] = "-"
            mod["live_id"] = ""

    rows = build_status_rows(mods)
    write_config_suggestions(rows)

    if save_config:
        print("\n-- Saving high-confidence discoveries into nexusmods-config.json --\n")
        apply_config_suggestions(min_score=HIGH_CONFIDENCE_SCORE, dry_run=False)
        # Refresh configured flags from the updated config file.
        refreshed = {"mods": {}}
        if os.path.isfile(CONFIG_PATH):
            with open(CONFIG_PATH, "r", encoding="utf-8") as handle:
                loaded = json.load(handle)
            if isinstance(loaded, dict):
                refreshed = loaded
        refreshed_mods = refreshed.get("mods", {})
        if not isinstance(refreshed_mods, dict):
            refreshed_mods = {}
        for mod in mods:
            entry = refreshed_mods.get(mod["base_name"], {})
            if not isinstance(entry, dict):
                continue
            nexus_id = safe_int(entry.get("nexus_mod_id", 0))
            if nexus_id > 0:
                mod["configured"] = True
                mod["nexus_mod_id"] = nexus_id
                group_id = str(entry.get("update_group_id", "") or "").strip()
                if group_id:
                    mod["update_group_id"] = group_id
                group_name = str(entry.get("update_group_name", "") or "").strip()
                if group_name:
                    mod["update_group_name"] = group_name
        rows = build_status_rows(mods)

    summary = {
        "total": total,
        "configured": sum(1 for row in rows if row["configured"]),
        "needs_update": sum(1 for row in rows if row["status"] == STATUS_NEEDS_UPDATE),
        "matches": sum(1 for row in rows if row["status"] == STATUS_MATCHES),
        "first_upload_needed": sum(1 for row in rows if row["status"] == STATUS_FIRST_UPLOAD),
        "missing_from_config": sum(1 for row in rows if (not row["configured"]) and row["found"]),
        "missing_zip": sum(1 for row in rows if not row["zip_present"]),
    }

    write_status_json(rows, summary)
    write_status_markdown(rows, summary)
    print_console_report(rows, summary)
    return 0


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Audit ReleaseSource vs Nexus and write Nexus-Status.*")
    parser.add_argument(
        "--no-save-config",
        action="store_true",
        help="Read-only status: do not write discovered Nexus IDs into nexusmods-config.json",
    )
    parser.add_argument(
        "--apply-config-suggestions",
        action="store_true",
        help="Only merge existing suggestions into config (no live audit)",
    )
    parser.add_argument(
        "--min-score",
        type=float,
        default=HIGH_CONFIDENCE_SCORE,
        help=f"Minimum match score when applying suggestions (default {HIGH_CONFIDENCE_SCORE})",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="With --apply-config-suggestions, preview merges without writing config",
    )
    return parser


def configure_stdio() -> None:
    """Avoid Windows cp1252 crashes on console prints."""
    for stream_name in ("stdout", "stderr"):
        stream = getattr(sys, stream_name, None)
        if stream is not None and hasattr(stream, "reconfigure"):
            try:
                stream.reconfigure(encoding="utf-8", errors="replace")
            except Exception:
                pass


def main() -> int:
    configure_stdio()
    args = build_arg_parser().parse_args()
    if args.apply_config_suggestions:
        return apply_config_suggestions(min_score=args.min_score, dry_run=args.dry_run)
    return run_audit(save_config=not args.no_save_config)


if __name__ == "__main__":
    sys.exit(main())
