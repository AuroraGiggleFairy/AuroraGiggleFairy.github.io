import argparse
import datetime as dt
import json
import os
import re
import shutil
import sys
import urllib.error
import urllib.request
import xml.etree.ElementTree as ET
from typing import Dict, List, Optional, Tuple


VS_CODE_ROOT = os.path.dirname(os.path.abspath(__file__))
RELEASE_SOURCE_DIR = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
ZIP_OUTPUT_DIR = os.path.join(VS_CODE_ROOT, "04_DownloadZips")
RELEASE_META_DIR = os.path.join(VS_CODE_ROOT, "05_GigglePackReleaseData")
DEFAULT_CONFIG_PATH = os.path.join(RELEASE_META_DIR, "NexusMods", "nexusmods-config.json")
DEFAULT_TEMPLATE_PATH = os.path.join(RELEASE_META_DIR, "NexusMods", "TEMPLATE-NexusModsConfig.json")
DEFAULT_PLAN_OUTPUT_PATH = os.path.join(RELEASE_META_DIR, "NexusMods", "nexusmods-release-plan.json")
DEFAULT_UPLOAD_PLAN_OUTPUT_PATH = os.path.join(RELEASE_META_DIR, "NexusMods", "nexusmods-upload-plan.json")
DEFAULT_API_KEY_ENV_VAR = "AGF_NEXUSMODS_API_KEY"
DEFAULT_APPLICATION_NAME = "AGF-NexusMods-Automation"
DEFAULT_APPLICATION_VERSION = "0.1.0"
DEFAULT_API_BASE_URL = "https://api.nexusmods.com/v2"
DEFAULT_API_V1_BASE_URL = "https://api.nexusmods.com/v1"
DEFAULT_DOWNLOAD_BASE_URL = (
    "https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips"
)
AGF_PREFIXES = ("AGF-", "zzzAGF-")


def load_json_file(path: str) -> Dict[str, object]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as handle:
            data = json.load(handle)
        return data if isinstance(data, dict) else {}
    except Exception:
        return {}


def write_json_file(path: str, payload: Dict[str, object], dry_run: bool) -> None:
    if dry_run:
        print(f"[DRYRUN] Would write JSON file: {path}")
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as handle:
        json.dump(payload, handle, indent=2, ensure_ascii=True)
        handle.write("\n")


def load_text_file(path: str) -> str:
    if not os.path.isfile(path):
        return ""
    try:
        with open(path, "r", encoding="utf-8") as handle:
            return handle.read()
    except Exception:
        return ""


def normalize_multiline_text(text: str) -> str:
    if not text:
        return ""
    normalized = text.replace("\r\n", "\n").replace("\r", "\n").strip()
    return normalized


def normalize_single_line_text(text: str) -> str:
    return re.sub(r"\s+", " ", str(text or "")).strip()


def build_nexus_sentence_list(items: List[str], conjunction: str = "and") -> str:
    cleaned = [normalize_single_line_text(item).rstrip(".") for item in items if normalize_single_line_text(item)]
    if not cleaned:
        return ""
    if len(cleaned) == 1:
        return cleaned[0]
    if len(cleaned) == 2:
        return f"{cleaned[0]} {conjunction} {cleaned[1]}"
    return f"{', '.join(cleaned[:-1])}, {conjunction} {cleaned[-1]}"


def extract_game_version_from_text(text: str) -> str:
    normalized = normalize_multiline_text(text)
    if not normalized:
        return ""
    for line in normalized.splitlines()[:20]:
        stripped = line.strip()
        match = re.match(r"^7d2d\s+Version\s+(.+?)\s*$", stripped, re.IGNORECASE)
        if match:
            return normalize_single_line_text(match.group(1))
    return ""


def extract_feature_bullets(text: str) -> List[str]:
    normalized = normalize_multiline_text(text)
    if not normalized:
        return []

    marker_match = re.search(
        r"<!--\s*FEATURES-SUMMARY START\s*-->(.*?)<!--\s*FEATURES-SUMMARY END\s*-->",
        normalized,
        flags=re.IGNORECASE | re.DOTALL,
    )
    if marker_match:
        block = marker_match.group(1)
    else:
        section_match = re.search(
            r"(?:^|\n)(?:##\s*5\.\s*Features|5\.\s*Features)\s*(.*?)(?:\n(?:##\s*6\.|6\.\s*Changelog)|\Z)",
            normalized,
            flags=re.IGNORECASE | re.DOTALL,
        )
        if not section_match:
            return []
        block = section_match.group(1)

    bullets: List[str] = []
    for line in block.splitlines():
        stripped = line.strip()
        if not stripped.startswith("-"):
            continue
        bullet = normalize_single_line_text(stripped.lstrip("- "))
        if bullet:
            bullets.append(bullet)
    return bullets


def build_feature_driven_overview(base_summary: str, feature_bullets: List[str], tested_game_version: str) -> str:
    base = normalize_single_line_text(base_summary)
    selected_features = feature_bullets[:3]
    parts: List[str] = []
    if base:
        parts.append(f"{base.rstrip('.')}.")
    if selected_features:
        feature_lines = [normalize_single_line_text(item) for item in selected_features if normalize_single_line_text(item)]
        if feature_lines:
            feature_text = " ".join(feature_lines)
            parts.append(f"Highlights include: {feature_text} See below for the full features list.")
    if tested_game_version:
        parts.append(f"Last tested on 7d2d Version {tested_game_version}.")
    return " ".join(part.strip() for part in parts if part.strip())


def parse_version_parts(version: str) -> Tuple[int, int, int]:
    numbers = [int(part) for part in re.findall(r"\d+", version)]
    while len(numbers) < 3:
        numbers.append(0)
    return numbers[0], numbers[1], numbers[2]


def compare_versions(left: str, right: str) -> int:
    left_parts = parse_version_parts(left)
    right_parts = parse_version_parts(right)
    if left_parts < right_parts:
        return -1
    if left_parts > right_parts:
        return 1
    return 0


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


def gather_release_mods() -> Dict[str, Dict[str, str]]:
    mods: Dict[str, Dict[str, str]] = {}
    for folder_name, folder_path in scan_mod_folders(RELEASE_SOURCE_DIR).items():
        modinfo_path = os.path.join(folder_path, "ModInfo.xml")
        readme_path = os.path.join(folder_path, "README.md")
        readable_readme_path = os.path.join(folder_path, "ReadableReadMe.txt")
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
            "readable_readme_path": readable_readme_path if os.path.isfile(readable_readme_path) else "",
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


def normalize_intent(raw_value: object) -> str:
    value = str(raw_value or "review").strip().lower()
    if value in {"publish", "update", "skip", "review"}:
        return value
    return "review"


def safe_int(raw_value: object) -> int:
    try:
        return int(raw_value)
    except Exception:
        return 0


def resolve_action(intent: str, nexus_mod_id: int) -> Tuple[str, List[str]]:
    notes: List[str] = []

    if intent == "skip":
        return "skip", notes
    if intent == "publish":
        if nexus_mod_id > 0:
            notes.append("Publish intent has a Nexus mod id; verify this should not be an update.")
        return "publish", notes
    if intent == "update":
        if nexus_mod_id <= 0:
            notes.append("Update intent requires a valid nexus_mod_id.")
            return "review", notes
        return "update", notes
    notes.append("Review intent or missing config entry; decide publish vs update manually.")
    return "review", notes


def create_template_config(destination_path: str, dry_run: bool) -> int:
    if os.path.exists(destination_path):
        print(f"Config already exists: {destination_path}")
        return 1
    if not os.path.isfile(DEFAULT_TEMPLATE_PATH):
        print(f"Template config missing: {DEFAULT_TEMPLATE_PATH}")
        return 1

    if dry_run:
        print(f"[DRYRUN] Would create config from template: {destination_path}")
        return 0

    os.makedirs(os.path.dirname(destination_path), exist_ok=True)
    shutil.copy2(DEFAULT_TEMPLATE_PATH, destination_path)
    print(f"Created config: {destination_path}")
    return 0


def extract_data_payload(payload: object) -> object:
    if isinstance(payload, dict) and "data" in payload:
        return payload.get("data")
    return payload


def get_api_base_url(config: Dict[str, object]) -> str:
    return str(config.get("api_base_url", DEFAULT_API_BASE_URL)).rstrip("/")


def get_api_v1_base_url(config: Dict[str, object]) -> str:
    return str(config.get("api_v1_base_url", DEFAULT_API_V1_BASE_URL)).rstrip("/")


def build_release_plan(config: Dict[str, object]) -> Dict[str, object]:
    release_mods = gather_release_mods()
    zip_paths = gather_mod_zip_paths()
    config_mods = config.get("mods", {})
    if not isinstance(config_mods, dict):
        config_mods = {}

    game_domain = str(config.get("game_domain", "7daystodie")).strip() or "7daystodie"
    download_base_url = str(config.get("default_download_base_url", DEFAULT_DOWNLOAD_BASE_URL)).rstrip("/")

    mod_entries: List[Dict[str, object]] = []
    summary = {
        "total_mods": 0,
        "publish": 0,
        "update": 0,
        "review": 0,
        "skip": 0,
        "missing_zip": 0,
        "missing_config": 0,
    }

    for base_name in sorted(release_mods.keys()):
        release_entry = release_mods[base_name]
        config_entry = config_mods.get(base_name, {})
        if not isinstance(config_entry, dict):
            config_entry = {}

        intent = normalize_intent(config_entry.get("intent", "review"))
        nexus_mod_id = safe_int(config_entry.get("nexus_mod_id", 0))
        action, notes = resolve_action(intent, nexus_mod_id)

        zip_path = zip_paths.get(base_name, "")
        if not zip_path:
            notes = list(notes) + ["Missing individual zip artifact in 04_DownloadZips."]
            summary["missing_zip"] += 1

        if not config_entry:
            summary["missing_config"] += 1

        zip_name = os.path.basename(zip_path) if zip_path else f"{base_name}.zip"
        page_url = str(config_entry.get("page_url", "")).strip()
        summary_override = str(config_entry.get("summary_override", "")).strip()
        description_override = str(config_entry.get("description_override", "")).strip()
        brief_overview_override = str(config_entry.get("brief_overview_override", "")).strip()
        detailed_description_override = normalize_multiline_text(
            str(config_entry.get("detailed_description_override", ""))
        )
        file_description_override = normalize_multiline_text(
            str(config_entry.get("file_description_override", ""))
        )
        tested_game_version_override = normalize_single_line_text(config_entry.get("tested_game_version_override", ""))
        update_group_id = str(config_entry.get("update_group_id", "")).strip()
        update_group_name = str(config_entry.get("update_group_name", "")).strip()
        file_category = str(config_entry.get("file_category", "main")).strip().lower() or "main"
        legacy_latest_file_id = safe_int(config_entry.get("legacy_latest_file_id", 0))
        legacy_name_hint = str(config_entry.get("legacy_name_hint", "")).strip()
        markdown_readme_body = normalize_multiline_text(str(load_text_file(str(release_entry.get("readme_path", "")))))
        readable_readme_body = normalize_multiline_text(str(load_text_file(str(release_entry.get("readable_readme_path", "")))))
        feature_bullets = extract_feature_bullets(markdown_readme_body or readable_readme_body)
        tested_game_version = (
            tested_game_version_override
            or extract_game_version_from_text(readable_readme_body)
            or extract_game_version_from_text(markdown_readme_body)
        )
        base_summary = summary_override or description_override or release_entry["description"]
        generated_overview = build_feature_driven_overview(base_summary, feature_bullets, tested_game_version)
        brief_overview = brief_overview_override or generated_overview or base_summary
        detailed_description = detailed_description_override or readable_readme_body or markdown_readme_body or description_override or release_entry["description"]
        file_description = file_description_override or (
            f"{normalize_single_line_text(release_entry['description']).rstrip('.')}. Last tested on 7d2d Version {tested_game_version}."
            if tested_game_version and normalize_single_line_text(release_entry["description"])
            else description_override or release_entry["description"]
        )

        entry = {
            "mod_name": base_name,
            "folder_name": release_entry["folder_name"],
            "folder_path": release_entry["folder_path"],
            "version": release_entry["version"],
            "zip_name": zip_name,
            "zip_path": zip_path,
            "download_url": f"{download_base_url}/{zip_name}",
            "readme_path": release_entry["readme_path"],
            "readable_readme_path": release_entry["readable_readme_path"],
            "description": file_description,
            "summary": brief_overview,
            "brief_overview": brief_overview,
            "detailed_description": detailed_description,
            "file_description": file_description,
            "tested_game_version": tested_game_version,
            "intent": intent,
            "action": action,
            "nexus_mod_id": nexus_mod_id,
            "page_url": page_url,
            "game_domain": game_domain,
            "update_group_id": update_group_id,
            "update_group_name": update_group_name,
            "file_category": file_category,
            "legacy_latest_file_id": legacy_latest_file_id,
            "legacy_name_hint": legacy_name_hint,
            "notes": notes,
        }
        mod_entries.append(entry)
        summary["total_mods"] += 1
        summary[action] += 1

    return {
        "generated_at": dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "game_domain": game_domain,
        "config_path": os.path.abspath(DEFAULT_CONFIG_PATH),
        "summary": summary,
        "mods": mod_entries,
    }


def print_plan_summary(plan: Dict[str, object], only: str) -> None:
    summary = plan.get("summary", {})
    print("=== NEXUS PLAN SUMMARY ===")
    print(f"Generated: {plan.get('generated_at', '')}")
    print(f"Total mods: {summary.get('total_mods', 0)}")
    print(f"Publish: {summary.get('publish', 0)}")
    print(f"Update: {summary.get('update', 0)}")
    print(f"Review: {summary.get('review', 0)}")
    print(f"Skip: {summary.get('skip', 0)}")
    print(f"Missing zip: {summary.get('missing_zip', 0)}")
    print(f"Missing config: {summary.get('missing_config', 0)}")
    print()

    mods = plan.get("mods", [])
    if not isinstance(mods, list):
        return

    print("=== NEXUS TARGETS ===")
    for entry in mods:
        if not isinstance(entry, dict):
            continue
        action = str(entry.get("action", "review"))
        if only != "all" and action != only:
            continue
        mod_name = str(entry.get("mod_name", ""))
        version = str(entry.get("version", "0.0.0"))
        nexus_mod_id = safe_int(entry.get("nexus_mod_id", 0))
        update_group_id = str(entry.get("update_group_id", "")).strip()
        suffix_parts: List[str] = []
        if nexus_mod_id > 0:
            suffix_parts.append(f"mod_id={nexus_mod_id}")
        if update_group_id:
            suffix_parts.append(f"group_id={update_group_id}")
        suffix = f" | {' | '.join(suffix_parts)}" if suffix_parts else ""
        print(f"[{action.upper()}] {mod_name} v{version}{suffix}")
        notes = entry.get("notes", [])
        if isinstance(notes, list):
            for note in notes:
                print(f"  - {note}")


def build_request_headers(config: Dict[str, object], api_key: str) -> Dict[str, str]:
    application_name = str(config.get("application_name", DEFAULT_APPLICATION_NAME)).strip()
    application_version = str(config.get("application_version", DEFAULT_APPLICATION_VERSION)).strip()
    return {
        "accept": "application/json",
        "apikey": api_key,
        "Application-Name": application_name or DEFAULT_APPLICATION_NAME,
        "Application-Version": application_version or DEFAULT_APPLICATION_VERSION,
        "User-Agent": f"{application_name or DEFAULT_APPLICATION_NAME}/{application_version or DEFAULT_APPLICATION_VERSION}",
    }


def request_json(url: str, headers: Dict[str, str], method: str = "GET", body: Optional[Dict[str, object]] = None) -> object:
    payload = None if body is None else json.dumps(body).encode("utf-8")
    request_headers = dict(headers)
    if payload is not None:
        request_headers["content-type"] = "application/json"
    request = urllib.request.Request(url, headers=request_headers, method=method, data=payload)
    with urllib.request.urlopen(request, timeout=20) as response:
        response_bytes = response.read()
    if not response_bytes:
        return {}
    return json.loads(response_bytes.decode("utf-8"))


def fetch_nexus_mod_info(
    api_base_url: str,
    api_v1_base_url: str,
    game_domain: str,
    nexus_mod_id: int,
    headers: Dict[str, str],
) -> Dict[str, object]:
    url = f"{api_base_url}/games/{game_domain}/mods/{nexus_mod_id}"
    try:
        payload = request_json(url, headers)
        data = extract_data_payload(payload)
        return data if isinstance(data, dict) else {}
    except urllib.error.HTTPError as ex:
        if ex.code != 404:
            raise

    legacy_url = f"{api_v1_base_url}/games/{game_domain}/mods/{nexus_mod_id}.json"
    payload = request_json(legacy_url, headers)
    return payload if isinstance(payload, dict) else {}


def fetch_mod_update_groups(api_base_url: str, mod_id: str, headers: Dict[str, str]) -> List[Dict[str, object]]:
    url = f"{api_base_url}/mods/{mod_id}/file-update-groups"
    payload = request_json(url, headers)
    data = extract_data_payload(payload)
    if not isinstance(data, dict):
        return []
    groups = data.get("groups", [])
    if not isinstance(groups, list):
        return []
    return [group for group in groups if isinstance(group, dict)]


def fetch_group_versions(api_base_url: str, group_id: str, headers: Dict[str, str]) -> List[Dict[str, object]]:
    url = f"{api_base_url}/file-update-groups/{group_id}/versions"
    payload = request_json(url, headers)
    data = extract_data_payload(payload)
    if not isinstance(data, dict):
        return []
    versions = data.get("versions", [])
    if not isinstance(versions, list):
        return []
    return [version for version in versions if isinstance(version, dict)]


def fetch_legacy_mod_files(
    api_v1_base_url: str,
    game_domain: str,
    nexus_mod_id: int,
    headers: Dict[str, str],
) -> Dict[str, object]:
    url = f"{api_v1_base_url}/games/{game_domain}/mods/{nexus_mod_id}/files.json"
    payload = request_json(url, headers)
    return payload if isinstance(payload, dict) else {}


def build_mod_keywords(mod_name: str) -> List[str]:
    lowered = mod_name.lower()
    lowered = lowered.replace("zzzagf-", "").replace("agf-", "")
    parts = [part for part in re.split(r"[^a-z0-9]+", lowered) if part]
    keywords: List[str] = []
    for part in parts:
        if part not in keywords:
            keywords.append(part)
        if part.endswith("main") and "main" not in keywords:
            keywords.append("main")
    return keywords


def summarize_legacy_file_chains(legacy_payload: Dict[str, object], mod_name: str) -> List[Dict[str, object]]:
    files = legacy_payload.get("files", [])
    file_updates = legacy_payload.get("file_updates", [])
    if not isinstance(files, list) or not isinstance(file_updates, list):
        return []

    file_by_id: Dict[int, Dict[str, object]] = {}
    next_by_old: Dict[int, int] = {}
    new_ids: set[int] = set()
    old_ids: set[int] = set()

    for file_entry in files:
        if not isinstance(file_entry, dict):
            continue
        file_id = safe_int(file_entry.get("file_id", 0))
        if file_id > 0:
            file_by_id[file_id] = file_entry

    for edge in file_updates:
        if not isinstance(edge, dict):
            continue
        old_file_id = safe_int(edge.get("old_file_id", 0))
        new_file_id = safe_int(edge.get("new_file_id", 0))
        if old_file_id > 0 and new_file_id > 0:
            next_by_old[old_file_id] = new_file_id
            old_ids.add(old_file_id)
            new_ids.add(new_file_id)

    roots: List[int] = sorted(old_ids - new_ids)
    singleton_ids = sorted(set(file_by_id.keys()) - old_ids - new_ids)
    keywords = build_mod_keywords(mod_name)
    visited: set[int] = set()
    chain_summaries: List[Dict[str, object]] = []

    def summarize_chain(file_ids: List[int]) -> Dict[str, object]:
        latest_id = file_ids[-1]
        latest_file = file_by_id.get(latest_id, {})
        chain_text = " ".join(
            f"{file_by_id.get(file_id, {}).get('name', '')} {file_by_id.get(file_id, {}).get('file_name', '')}"
            for file_id in file_ids
        ).lower()
        keyword_score = sum(1 for keyword in keywords if keyword and keyword in chain_text)
        return {
            "latest_file_id": latest_id,
            "latest_name": str(latest_file.get("name", "")).strip(),
            "latest_file_name": str(latest_file.get("file_name", "")).strip(),
            "latest_version": str(latest_file.get("version", "")).strip(),
            "latest_mod_version": str(latest_file.get("mod_version", "")).strip(),
            "latest_uploaded_timestamp": safe_int(latest_file.get("uploaded_timestamp", 0)),
            "latest_uploaded_time": str(latest_file.get("uploaded_time", "")).strip(),
            "is_primary": bool(latest_file.get("is_primary", False)),
            "category_name": str(latest_file.get("category_name", "")).strip(),
            "chain_length": len(file_ids),
            "file_ids": file_ids,
            "keyword_score": keyword_score,
        }

    for root_id in roots:
        chain_ids: List[int] = []
        current = root_id
        while current and current not in visited:
            visited.add(current)
            chain_ids.append(current)
            current = next_by_old.get(current, 0)
        if chain_ids:
            chain_summaries.append(summarize_chain(chain_ids))

    for single_id in singleton_ids:
        if single_id in visited:
            continue
        visited.add(single_id)
        chain_summaries.append(summarize_chain([single_id]))

    chain_summaries.sort(
        key=lambda item: (
            item.get("keyword_score", 0),
            1 if item.get("is_primary", False) else 0,
            item.get("latest_uploaded_timestamp", 0),
        ),
        reverse=True,
    )
    return chain_summaries


def select_legacy_chain_candidate(
    candidates: List[Dict[str, object]],
    preferred_latest_file_id: int = 0,
    preferred_name_hint: str = "",
) -> Optional[Dict[str, object]]:
    if not candidates:
        return None

    if preferred_latest_file_id > 0:
        for candidate in candidates:
            candidate_file_ids = candidate.get("file_ids", [])
            if isinstance(candidate_file_ids, list) and preferred_latest_file_id in candidate_file_ids:
                return candidate

    hint = preferred_name_hint.strip().lower()
    if hint:
        for candidate in candidates:
            latest_name = str(candidate.get("latest_name", "")).strip().lower()
            latest_file_name = str(candidate.get("latest_file_name", "")).strip().lower()
            if hint in latest_name or hint in latest_file_name:
                return candidate

    best = candidates[0]
    if safe_int(best.get("keyword_score", 0)) <= 0 and len(candidates) > 1:
        return None
    return best


def print_legacy_chain_candidates(mod_name: str, candidates: List[Dict[str, object]]) -> None:
    print(f"  Legacy file-chain candidates for {mod_name}:")
    for candidate in candidates[:5]:
        latest_file_id = safe_int(candidate.get("latest_file_id", 0))
        latest_name = str(candidate.get("latest_name", "")).strip() or "<unnamed>"
        latest_version = str(candidate.get("latest_mod_version", "")).strip() or str(candidate.get("latest_version", "")).strip()
        chain_length = safe_int(candidate.get("chain_length", 0))
        keyword_score = safe_int(candidate.get("keyword_score", 0))
        primary_note = " primary" if bool(candidate.get("is_primary", False)) else ""
        print(
            f"    - file_id={latest_file_id} | v{latest_version} | {latest_name} | "
            f"chain={chain_length} | score={keyword_score}{primary_note}"
        )


def get_env_api_key(config: Dict[str, object]) -> Tuple[str, str]:
    env_var_name = str(config.get("api_key_env_var", DEFAULT_API_KEY_ENV_VAR)).strip() or DEFAULT_API_KEY_ENV_VAR
    return env_var_name, os.getenv(env_var_name, "").strip()


def build_group_line(group: Dict[str, object]) -> str:
    group_id = str(group.get("id", "")).strip()
    group_name = str(group.get("name", "")).strip() or "<unnamed>"
    is_active = bool(group.get("is_active", False))
    versions_count = safe_int(group.get("versions_count", 0))
    active_suffix = " active" if is_active else ""
    return f"{group_name} | id={group_id} | versions={versions_count}{active_suffix}"


def resolve_live_state_for_entry(entry: Dict[str, object], config: Dict[str, object], headers: Dict[str, str]) -> Dict[str, object]:
    api_base_url = get_api_base_url(config)
    api_v1_base_url = get_api_v1_base_url(config)
    mod_name = str(entry.get("mod_name", ""))
    game_domain = str(entry.get("game_domain", "7daystodie"))
    nexus_mod_id = safe_int(entry.get("nexus_mod_id", 0))
    legacy_latest_file_id = safe_int(entry.get("legacy_latest_file_id", 0))
    legacy_name_hint = str(entry.get("legacy_name_hint", "")).strip()

    payload = fetch_nexus_mod_info(api_base_url, api_v1_base_url, game_domain, nexus_mod_id, headers)
    live_version = str(payload.get("version", "0.0.0"))
    live_mod_id = str(payload.get("id", payload.get("mod_id", ""))).strip()
    update_groups: List[Dict[str, object]] = []
    update_groups_error = ""
    legacy_candidates: List[Dict[str, object]] = []
    selected_legacy_chain: Optional[Dict[str, object]] = None

    if live_mod_id:
        try:
            update_groups = fetch_mod_update_groups(api_base_url, live_mod_id, headers)
        except urllib.error.HTTPError as group_ex:
            update_groups_error = f"HTTP {group_ex.code}"
            if group_ex.code != 404:
                raise

    if not update_groups:
        legacy_payload = fetch_legacy_mod_files(api_v1_base_url, game_domain, nexus_mod_id, headers)
        legacy_candidates = summarize_legacy_file_chains(legacy_payload, mod_name)
        selected_legacy_chain = select_legacy_chain_candidate(
            legacy_candidates,
            preferred_latest_file_id=legacy_latest_file_id,
            preferred_name_hint=legacy_name_hint,
        )
        if selected_legacy_chain:
            live_version = (
                str(selected_legacy_chain.get("latest_mod_version", "")).strip()
                or str(selected_legacy_chain.get("latest_version", "")).strip()
                or live_version
            )

    return {
        "mod_id": live_mod_id,
        "mod_name": str(payload.get("name", "")).strip(),
        "version": live_version,
        "page_version": str(payload.get("version", "")).strip(),
        "page_summary": str(payload.get("summary", "")).strip(),
        "update_groups": update_groups,
        "update_groups_error": update_groups_error,
        "legacy_chain_candidates": legacy_candidates,
        "selected_legacy_chain": selected_legacy_chain,
    }


def extract_changelog_blocks(readme_path: str) -> List[Tuple[str, List[str]]]:
    text = load_text_file(readme_path)
    if not text:
        return []

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


def build_changelog_delta(readme_path: str, from_version: str, to_version: str) -> List[Dict[str, object]]:
    blocks = extract_changelog_blocks(readme_path)
    delta: List[Dict[str, object]] = []
    for version, lines in blocks:
        if compare_versions(version, from_version) <= 0:
            continue
        if compare_versions(version, to_version) > 0:
            continue
        bullets = [line.strip() for line in lines if line.strip().startswith("-")]
        delta.append({"version": version, "bullets": bullets})
    return delta


def build_file_changelog_summary(changelog_delta: List[Dict[str, object]]) -> str:
    if not changelog_delta:
        return ""

    lines: List[str] = []
    for block in changelog_delta:
        version = str(block.get("version", "")).strip()
        bullets = block.get("bullets", [])
        if version:
            lines.append(f"v{version}")
        if isinstance(bullets, list):
            for bullet in bullets:
                bullet_text = str(bullet).strip()
                if bullet_text:
                    lines.append(bullet_text)
        lines.append("")
    return "\n".join(lines).strip()


def discover_live_update_groups(plan: Dict[str, object], config: Dict[str, object], only_updates: bool) -> int:
    env_var_name, api_key = get_env_api_key(config)
    if not api_key:
        print(f"Missing Nexus API key. Set environment variable {env_var_name}.")
        return 1

    headers = build_request_headers(config, api_key)
    api_base_url = get_api_base_url(config)
    api_v1_base_url = get_api_v1_base_url(config)
    mods = plan.get("mods", [])
    if not isinstance(mods, list):
        return 1

    print("=== NEXUS UPDATE GROUP DISCOVERY ===")
    failures = 0
    for entry in mods:
        if not isinstance(entry, dict):
            continue
        if only_updates and str(entry.get("action", "")) != "update":
            continue

        nexus_mod_id = safe_int(entry.get("nexus_mod_id", 0))
        if nexus_mod_id <= 0:
            continue

        mod_name = str(entry.get("mod_name", ""))
        game_domain = str(entry.get("game_domain", "7daystodie"))
        legacy_latest_file_id = safe_int(entry.get("legacy_latest_file_id", 0))
        legacy_name_hint = str(entry.get("legacy_name_hint", "")).strip()
        try:
            mod_payload = fetch_nexus_mod_info(api_base_url, api_v1_base_url, game_domain, nexus_mod_id, headers)
            live_mod_id = str(mod_payload.get("id", "")).strip()
            live_mod_name = str(mod_payload.get("name", "")).strip()
            groups: List[Dict[str, object]] = []
            legacy_candidates: List[Dict[str, object]] = []
            selected_legacy_chain: Optional[Dict[str, object]] = None
            update_groups_error = ""

            if live_mod_id:
                try:
                    groups = fetch_mod_update_groups(api_base_url, live_mod_id, headers)
                except urllib.error.HTTPError as group_ex:
                    update_groups_error = f"HTTP {group_ex.code}"
                    if group_ex.code != 404:
                        raise

            if not groups:
                legacy_payload = fetch_legacy_mod_files(api_v1_base_url, game_domain, nexus_mod_id, headers)
                legacy_candidates = summarize_legacy_file_chains(legacy_payload, mod_name)
                selected_legacy_chain = select_legacy_chain_candidate(
                    legacy_candidates,
                    preferred_latest_file_id=legacy_latest_file_id,
                    preferred_name_hint=legacy_name_hint,
                )

            entry["live"] = {
                "mod_id": live_mod_id,
                "mod_name": live_mod_name,
                "update_groups": groups,
                "update_groups_error": update_groups_error,
                "legacy_chain_candidates": legacy_candidates,
                "selected_legacy_chain": selected_legacy_chain,
            }

            if groups:
                print(f"[{mod_name}] nexus_mod_id={nexus_mod_id} | live_id={live_mod_id or 'unknown'} | groups={len(groups)}")
                for group in groups:
                    print(f"  - {build_group_line(group)}")
            else:
                print(
                    f"[{mod_name}] nexus_mod_id={nexus_mod_id} | live_id={live_mod_id or 'unknown'} | "
                    f"groups unavailable ({update_groups_error or 'no groups'})"
                )
                print_legacy_chain_candidates(mod_name, legacy_candidates)
        except urllib.error.HTTPError as ex:
            failures += 1
            print(f"[HTTP {ex.code}] {mod_name}: could not discover update groups")
        except urllib.error.URLError as ex:
            failures += 1
            print(f"[NETWORK ERROR] {mod_name}: {ex}")
        except Exception as ex:
            failures += 1
            print(f"[ERROR] {mod_name}: {ex}")

    return 0 if failures == 0 else 1


def run_live_check(plan: Dict[str, object], config: Dict[str, object]) -> int:
    env_var_name, api_key = get_env_api_key(config)
    if not api_key:
        print(f"Missing Nexus API key. Set environment variable {env_var_name}.")
        return 1

    headers = build_request_headers(config, api_key)
    api_base_url = get_api_base_url(config)
    api_v1_base_url = get_api_v1_base_url(config)
    mods = plan.get("mods", [])
    if not isinstance(mods, list):
        return 1

    print("=== NEXUS LIVE CHECK ===")
    failures = 0
    for entry in mods:
        if not isinstance(entry, dict):
            continue
        if str(entry.get("action", "")) != "update":
            continue

        mod_name = str(entry.get("mod_name", ""))
        game_domain = str(entry.get("game_domain", "7daystodie"))
        nexus_mod_id = safe_int(entry.get("nexus_mod_id", 0))
        local_version = str(entry.get("version", "0.0.0"))
        configured_group_id = str(entry.get("update_group_id", "")).strip()
        legacy_latest_file_id = safe_int(entry.get("legacy_latest_file_id", 0))
        legacy_name_hint = str(entry.get("legacy_name_hint", "")).strip()

        try:
            payload = fetch_nexus_mod_info(api_base_url, api_v1_base_url, game_domain, nexus_mod_id, headers)
            live_version = str(payload.get("version", "0.0.0"))
            live_mod_id = str(payload.get("id", payload.get("mod_id", ""))).strip()
            update_groups: List[Dict[str, object]] = []
            update_groups_error = ""
            legacy_candidates: List[Dict[str, object]] = []
            selected_legacy_chain: Optional[Dict[str, object]] = None

            if live_mod_id:
                try:
                    update_groups = fetch_mod_update_groups(api_base_url, live_mod_id, headers)
                except urllib.error.HTTPError as group_ex:
                    update_groups_error = f"HTTP {group_ex.code}"
                    if group_ex.code != 404:
                        raise

            if not update_groups:
                legacy_payload = fetch_legacy_mod_files(api_v1_base_url, game_domain, nexus_mod_id, headers)
                legacy_candidates = summarize_legacy_file_chains(legacy_payload, mod_name)
                selected_legacy_chain = select_legacy_chain_candidate(
                    legacy_candidates,
                    preferred_latest_file_id=legacy_latest_file_id,
                    preferred_name_hint=legacy_name_hint,
                )
                if selected_legacy_chain:
                    live_version = (
                        str(selected_legacy_chain.get("latest_mod_version", "")).strip()
                        or str(selected_legacy_chain.get("latest_version", "")).strip()
                        or live_version
                    )

            matching_group = next(
                (group for group in update_groups if str(group.get("id", "")).strip() == configured_group_id),
                None,
            )
            comparison = compare_versions(local_version, live_version)
            if comparison > 0:
                status = "LOCAL_NEWER"
            elif comparison < 0:
                status = "NEXUS_NEWER"
            else:
                status = "IN_SYNC"
            group_note = ""
            if configured_group_id:
                group_note = " | group=FOUND" if matching_group else " | group=MISSING"
            elif update_groups:
                group_note = f" | groups={len(update_groups)}"
            elif selected_legacy_chain:
                group_note = f" | legacy_file_id={safe_int(selected_legacy_chain.get('latest_file_id', 0))}"

            entry["live"] = {
                "mod_id": live_mod_id,
                "mod_name": str(payload.get("name", "")).strip(),
                "version": live_version,
                "update_groups": update_groups,
                "update_groups_error": update_groups_error,
                "matching_update_group": matching_group,
                "legacy_chain_candidates": legacy_candidates,
                "selected_legacy_chain": selected_legacy_chain,
            }
            print(
                f"[{status}] {mod_name}: local v{local_version} | nexus v{live_version} | mod_id={nexus_mod_id}{group_note}"
            )
        except urllib.error.HTTPError as ex:
            failures += 1
            print(f"[HTTP {ex.code}] {mod_name}: live check failed for mod_id={nexus_mod_id}")
        except urllib.error.URLError as ex:
            failures += 1
            print(f"[NETWORK ERROR] {mod_name}: {ex}")
        except Exception as ex:
            failures += 1
            print(f"[ERROR] {mod_name}: {ex}")

    return 0 if failures == 0 else 1


def prepare_upload_plan(plan: Dict[str, object], config: Dict[str, object], only: str) -> Tuple[int, Dict[str, object]]:
    env_var_name, api_key = get_env_api_key(config)
    if not api_key:
        print(f"Missing Nexus API key. Set environment variable {env_var_name}.")
        return 1, {}

    headers = build_request_headers(config, api_key)
    mods = plan.get("mods", [])
    if not isinstance(mods, list):
        return 1, {}

    prepared_targets: List[Dict[str, object]] = []
    failures = 0
    print("=== NEXUS UPLOAD PREPARATION ===")

    for entry in mods:
        if not isinstance(entry, dict):
            continue
        action = str(entry.get("action", "review"))
        if only != "all" and action != only:
            continue
        if action != "update":
            continue

        mod_name = str(entry.get("mod_name", ""))
        zip_path = str(entry.get("zip_path", ""))
        if not os.path.isfile(zip_path):
            failures += 1
            print(f"[MISSING ZIP] {mod_name}: {zip_path}")
            continue

        try:
            live = resolve_live_state_for_entry(entry, config, headers)
            entry["live"] = live
            local_version = str(entry.get("version", "0.0.0"))
            live_version = str(live.get("version", "0.0.0"))
            selected_chain = live.get("selected_legacy_chain")
            latest_file_id = 0
            latest_file_name = ""
            if isinstance(selected_chain, dict):
                latest_file_id = safe_int(selected_chain.get("latest_file_id", 0))
                latest_file_name = str(selected_chain.get("latest_file_name", "")).strip()

            changelog_delta = build_changelog_delta(str(entry.get("readme_path", "")), live_version, local_version)
            file_size_bytes = os.path.getsize(zip_path)
            brief_overview = str(entry.get("brief_overview", "")).strip()
            detailed_description = normalize_multiline_text(str(entry.get("detailed_description", "")))
            file_description = normalize_multiline_text(str(entry.get("file_description", "")))
            tested_game_version = normalize_single_line_text(str(entry.get("tested_game_version", "")))
            file_changelog_summary = build_file_changelog_summary(changelog_delta)
            upload_target = {
                "mod_name": mod_name,
                "nexus_mod_id": safe_int(entry.get("nexus_mod_id", 0)),
                "page_url": str(entry.get("page_url", "")).strip(),
                "local_version": local_version,
                "live_version": live_version,
                "live_page_version": str(live.get("page_version", "")).strip(),
                "live_latest_file_id": latest_file_id,
                "live_latest_file_name": latest_file_name,
                "zip_path": zip_path,
                "zip_name": os.path.basename(zip_path),
                "size_bytes": file_size_bytes,
                "file_category": str(entry.get("file_category", "main")).strip().lower() or "main",
                "summary": brief_overview,
                "description": file_description,
                "brief_overview": brief_overview,
                "detailed_description": detailed_description,
                "file_description": file_description,
                "tested_game_version": tested_game_version,
                "file_changelog_summary": file_changelog_summary,
                "changelog_delta": changelog_delta,
                "selected_legacy_chain": selected_chain,
            }
            prepared_targets.append(upload_target)
            print(
                f"[READY] {mod_name}: local v{local_version} -> live v{live_version} | "
                f"file_id={latest_file_id or 'unknown'} | {file_size_bytes} bytes"
            )
        except Exception as ex:
            failures += 1
            print(f"[ERROR] {mod_name}: {ex}")

    return (
        0 if failures == 0 else 1,
        {
            "generated_at": dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "config_path": str(plan.get("config_path", "")),
            "prepared_targets": prepared_targets,
        },
    )


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Dedicated Nexus Mods planning and live-check workflow")
    parser.add_argument(
        "--mode",
        choices=["init-config", "build-plan", "check-live", "discover-groups", "prepare-upload"],
        default="build-plan",
        help="Create a config from template, build a local Nexus release plan, run live discovery/checks, or prepare upload payloads",
    )
    parser.add_argument(
        "--config",
        default=DEFAULT_CONFIG_PATH,
        help="Path to nexusmods-config.json",
    )
    parser.add_argument(
        "--plan-output",
        default=DEFAULT_PLAN_OUTPUT_PATH,
        help="Where to write the generated Nexus release plan JSON",
    )
    parser.add_argument(
        "--upload-plan-output",
        default=DEFAULT_UPLOAD_PLAN_OUTPUT_PATH,
        help="Where to write the prepared Nexus upload plan JSON",
    )
    parser.add_argument(
        "--only",
        choices=["all", "publish", "update", "review", "skip"],
        default="all",
        help="Filter the printed plan summary by action",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Preview file writes without changing files",
    )
    return parser


def main() -> int:
    parser = build_arg_parser()
    args = parser.parse_args()

    config_path = os.path.abspath(args.config)
    if args.mode == "init-config":
        return create_template_config(config_path, args.dry_run)

    if not os.path.isfile(config_path):
        print(f"Config not found: {config_path}")
        print("Run --mode init-config first, then fill in publish/update intents and Nexus mod ids.")
        return 1

    config = load_json_file(config_path)
    if not config:
        print(f"Could not read config: {config_path}")
        return 1

    plan = build_release_plan(config)
    plan["config_path"] = config_path

    plan_output = os.path.abspath(args.plan_output)
    write_json_file(plan_output, plan, args.dry_run)
    print_plan_summary(plan, args.only)

    if args.mode == "check-live":
        result = run_live_check(plan, config)
        write_json_file(plan_output, plan, args.dry_run)
        return result
    if args.mode == "discover-groups":
        result = discover_live_update_groups(plan, config, only_updates=True)
        write_json_file(plan_output, plan, args.dry_run)
        return result
    if args.mode == "prepare-upload":
        result, upload_plan = prepare_upload_plan(plan, config, args.only)
        write_json_file(plan_output, plan, args.dry_run)
        if upload_plan:
            write_json_file(os.path.abspath(args.upload_plan_output), upload_plan, args.dry_run)
        return result
    return 0


if __name__ == "__main__":
    sys.exit(main())