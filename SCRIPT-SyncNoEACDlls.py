import argparse
import hashlib
import json
import os
import re
import shutil
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from typing import Dict, List, Optional, Tuple


WORKSPACE_ROOT = os.path.dirname(os.path.abspath(__file__))
DLL_PROJECTS_DIR = os.path.join(WORKSPACE_ROOT, "_DLL-Projects")
LEGACY_NOEAC_PROJECTS_DIR = os.path.join(WORKSPACE_ROOT, "_NoEAC-Projects")
ACTIVE_BUILD_DIR = os.path.join(WORKSPACE_ROOT, "02_ActiveBuild")
RELEASE_SOURCE_DIR = os.path.join(WORKSPACE_ROOT, "03_ReleaseSource")
DRAFT_DIR = os.path.join(WORKSPACE_ROOT, "01_Draft")
DEFAULT_MAP_PATH = os.path.join(WORKSPACE_ROOT, "HELPER_NoEACDllSync.json")
DEFAULT_GAME_MODS_DIR = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods"


DEFAULT_PROJECT_TO_MOD_OVERRIDES: Dict[str, str] = {
    "stormtracker": "AGF-NoEAC-GlobalStormTracker",
    "enhancedagf": "AGF-NoEAC-EnhancedPatch",
    "screameralertvariant": "AGF-NoEAC-ScreamerAlert",
    "roboticinboxagffiles": "AGF-NoEAC-SortingBox",
}

DEFAULT_PROJECT_RENAME_OVERRIDES: Dict[str, str] = {
    "stormtracker": "GlobalStormTracker",
    "enhancedagf": "EnhancedAGF",
    "screameralertvariant": "ScreamerAlert",
    "roboticinboxagffiles": "SortingBox",
}

VALID_CATEGORIES = {"NoEAC", "Fixes", "Security", "4Modders"}

DEFAULT_PROJECT_CATEGORY_OVERRIDES: Dict[str, str] = {
    "anticm": "Security",
    "lockableworkstations": "Security",
    "damagetypefix": "Fixes",
    "destroybiomebadgefix": "Fixes",
    "removeitemsrepeatablemod": "Fixes",
    "windowenteringduration": "Fixes",
}


def normalize_key(text: str) -> str:
    return re.sub(r"[^a-z0-9]", "", (text or "").lower())


def project_key_from_folder_name(folder_name: str) -> str:
    # Keep override keys stable if folders are prefixed with DLL_.
    cleaned = re.sub(r"^dll[_-]+", "", (folder_name or ""), flags=re.IGNORECASE)
    return normalize_key(cleaned)


def mod_base_from_folder(folder_name: str) -> str:
    return re.sub(r"-v\d+(?:\.\d+)*$", "", folder_name)


def mod_short_name(mod_base: str) -> str:
    return re.sub(r"^AGF-NoEAC-", "", mod_base, flags=re.IGNORECASE)


def file_sha256(path: str) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as handle:
        while True:
            chunk = handle.read(1024 * 1024)
            if not chunk:
                break
            h.update(chunk)
    return h.hexdigest()


@dataclass
class ProjectInfo:
    project_path: str
    project_dir: str
    project_folder_name: str
    project_name: str
    assembly_name: str
    project_key: str


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Compare NoEAC project DLLs against installed game mod DLLs, "
            "then copy newer DLLs into game mod folders."
        )
    )
    parser.add_argument("--game-mods", default=DEFAULT_GAME_MODS_DIR, help="Path to game Mods folder")
    parser.add_argument("--map-file", default=DEFAULT_MAP_PATH, help="Optional JSON overrides file")
    parser.add_argument("--apply", action="store_true", help="Copy DLLs when source is newer")
    parser.add_argument("--suggest-renames", action="store_true", help="Print recommended _DLL-Projects folder renames")
    parser.add_argument("--apply-renames", action="store_true", help="Apply safe _DLL-Projects folder renames")
    parser.add_argument("--no-hash", action="store_true", help="Skip SHA256 comparison and use timestamps only")
    parser.add_argument("--verbose", action="store_true", help="Print additional debug details")
    return parser.parse_args()


def load_override_file(path: str) -> Tuple[Dict[str, str], Dict[str, str], Dict[str, str]]:
    if not os.path.isfile(path):
        return {}, {}, {}

    try:
        with open(path, "r", encoding="utf-8") as handle:
            payload = json.load(handle)
    except Exception:
        return {}, {}, {}

    project_to_mod_raw = payload.get("project_to_mod_overrides", {}) if isinstance(payload, dict) else {}
    rename_raw = payload.get("project_rename_overrides", {}) if isinstance(payload, dict) else {}
    category_raw = payload.get("project_category_overrides", {}) if isinstance(payload, dict) else {}

    project_to_mod: Dict[str, str] = {}
    rename_overrides: Dict[str, str] = {}
    category_overrides: Dict[str, str] = {}

    if isinstance(project_to_mod_raw, dict):
        for k, v in project_to_mod_raw.items():
            key = normalize_key(str(k))
            value = str(v).strip()
            if key and value:
                project_to_mod[key] = value

    if isinstance(rename_raw, dict):
        for k, v in rename_raw.items():
            key = normalize_key(str(k))
            value = str(v).strip()
            if key and value:
                rename_overrides[key] = value

    if isinstance(category_raw, dict):
        for k, v in category_raw.items():
            key = normalize_key(str(k))
            value = str(v).strip()
            if key and value in VALID_CATEGORIES:
                category_overrides[key] = value

    return project_to_mod, rename_overrides, category_overrides


def collect_csproj_projects(root_dir: str) -> List[ProjectInfo]:
    projects: List[ProjectInfo] = []
    excluded_parts = ("\\Decompiled DLLs\\", "\\.vscode\\")

    for dirpath, _, filenames in os.walk(root_dir):
        for filename in filenames:
            if not filename.lower().endswith(".csproj"):
                continue
            project_path = os.path.join(dirpath, filename)
            normalized_path = project_path.replace("/", "\\")
            if any(part in normalized_path for part in excluded_parts):
                continue

            project_dir = os.path.dirname(project_path)
            project_name = os.path.splitext(filename)[0]
            assembly_name = project_name

            try:
                tree = ET.parse(project_path)
                root = tree.getroot()
                for property_group in root.findall("PropertyGroup"):
                    assembly = property_group.find("AssemblyName")
                    if assembly is not None and (assembly.text or "").strip():
                        assembly_name = (assembly.text or "").strip()
                        break
            except Exception:
                pass

            rel_dir = os.path.relpath(project_dir, root_dir)
            top_folder = rel_dir.split(os.sep)[0]
            project_key = project_key_from_folder_name(top_folder)

            projects.append(
                ProjectInfo(
                    project_path=project_path,
                    project_dir=project_dir,
                    project_folder_name=top_folder,
                    project_name=project_name,
                    assembly_name=assembly_name,
                    project_key=project_key,
                )
            )

    # Prefer a shallower project file when multiple projects map to the same top-level folder.
    by_folder: Dict[str, ProjectInfo] = {}
    for project in projects:
        existing = by_folder.get(project.project_folder_name)
        if not existing:
            by_folder[project.project_folder_name] = project
            continue
        if len(project.project_path) < len(existing.project_path):
            by_folder[project.project_folder_name] = project

    return sorted(by_folder.values(), key=lambda p: p.project_folder_name.lower())


def collect_noeac_mod_bases() -> Dict[str, str]:
    bases: Dict[str, str] = {}
    for lane in (ACTIVE_BUILD_DIR, RELEASE_SOURCE_DIR, DRAFT_DIR):
        if not os.path.isdir(lane):
            continue
        for name in os.listdir(lane):
            full = os.path.join(lane, name)
            if not os.path.isdir(full):
                continue
            if not name.startswith("AGF-NoEAC-"):
                continue
            base = mod_base_from_folder(name)
            key = normalize_key(mod_short_name(base))
            if key not in bases:
                bases[key] = base
    return bases


def find_best_mod_base(project: ProjectInfo, mod_bases_by_key: Dict[str, str], overrides: Dict[str, str]) -> Optional[str]:
    if project.project_key in overrides:
        return overrides[project.project_key]

    candidates = []
    keys_to_try = {
        normalize_key(project.project_folder_name),
        normalize_key(project.project_name),
        normalize_key(project.assembly_name),
    }
    keys_to_try = {k for k in keys_to_try if k}

    for key in keys_to_try:
        if key in mod_bases_by_key:
            return mod_bases_by_key[key]

    for key, base in mod_bases_by_key.items():
        for source_key in keys_to_try:
            if source_key in key or key in source_key:
                candidates.append(base)

    unique = sorted(set(candidates))
    if len(unique) == 1:
        return unique[0]
    return None


def find_newest_source_dll(project_dir: str, assembly_name: str) -> Optional[str]:
    target_name = f"{assembly_name}.dll"
    matches: List[str] = []

    for dirpath, _, filenames in os.walk(project_dir):
        if "\\obj\\" in dirpath.replace("/", "\\"):
            continue
        for filename in filenames:
            if filename.lower() != target_name.lower():
                continue
            matches.append(os.path.join(dirpath, filename))

    if not matches:
        return None

    matches.sort(key=lambda p: os.path.getmtime(p), reverse=True)
    return matches[0]


def find_mod_folder_by_base(mods_root: str, mod_base: str) -> Optional[str]:
    if not os.path.isdir(mods_root):
        return None

    for name in os.listdir(mods_root):
        full = os.path.join(mods_root, name)
        if not os.path.isdir(full):
            continue
        if mod_base_from_folder(name).lower() == mod_base.lower():
            return full
    return None


def pick_target_dll_name(mod_base: str, source_assembly_name: str) -> str:
    # Prefer current lane DLL name when available to keep naming consistent.
    lane_mod_dir = find_mod_folder_by_base(RELEASE_SOURCE_DIR, mod_base) or find_mod_folder_by_base(ACTIVE_BUILD_DIR, mod_base)
    if lane_mod_dir and os.path.isdir(lane_mod_dir):
        lane_dlls = [name for name in os.listdir(lane_mod_dir) if name.lower().endswith(".dll")]
        if len(lane_dlls) == 1:
            return lane_dlls[0]
        for candidate in lane_dlls:
            if candidate.lower() == f"{source_assembly_name.lower()}.dll":
                return candidate
    return f"{source_assembly_name}.dll"


def suggest_rename(
    project: ProjectInfo,
    mod_base: str,
    rename_overrides: Dict[str, str],
    category_overrides: Dict[str, str],
) -> Optional[Tuple[str, str]]:
    source_name = project.project_folder_name
    if project.project_key in rename_overrides:
        base_target_name = rename_overrides[project.project_key]
    else:
        base_target_name = mod_short_name(mod_base)

    category = category_overrides.get(project.project_key, "NoEAC")
    target_name = f"DLL_{category}-{base_target_name}"

    if normalize_key(source_name) == normalize_key(target_name):
        return None

    source_path = os.path.join(DLL_PROJECTS_DIR, source_name)
    target_path = os.path.join(DLL_PROJECTS_DIR, target_name)
    if os.path.exists(target_path):
        return None

    return source_path, target_path


def main() -> int:
    args = parse_args()

    projects_root = DLL_PROJECTS_DIR if os.path.isdir(DLL_PROJECTS_DIR) else LEGACY_NOEAC_PROJECTS_DIR

    if not os.path.isdir(projects_root):
        print(f"DLL projects folder not found: {DLL_PROJECTS_DIR}")
        return 1
    if not os.path.isdir(args.game_mods):
        print(f"Game Mods folder not found: {args.game_mods}")
        return 1

    file_overrides, file_renames, file_categories = load_override_file(args.map_file)
    project_to_mod_overrides = dict(DEFAULT_PROJECT_TO_MOD_OVERRIDES)
    project_to_mod_overrides.update(file_overrides)
    rename_overrides = dict(DEFAULT_PROJECT_RENAME_OVERRIDES)
    rename_overrides.update(file_renames)
    category_overrides = dict(DEFAULT_PROJECT_CATEGORY_OVERRIDES)
    category_overrides.update(file_categories)

    global DLL_PROJECTS_DIR
    DLL_PROJECTS_DIR = projects_root
    projects = collect_csproj_projects(projects_root)
    mod_bases = collect_noeac_mod_bases()

    copy_count = 0
    skip_count = 0
    missing_count = 0
    rename_actions: List[Tuple[str, str]] = []

    for project in projects:
        mod_base = find_best_mod_base(project, mod_bases, project_to_mod_overrides)
        if not mod_base:
            print(f"[UNMAPPED] {project.project_folder_name} | {project.project_name} | {project.assembly_name}")
            missing_count += 1
            continue

        source_dll = find_newest_source_dll(project.project_dir, project.assembly_name)
        if not source_dll:
            print(f"[NO DLL] {project.project_folder_name} -> {mod_base} | expected {project.assembly_name}.dll")
            missing_count += 1
            continue

        game_mod_dir = find_mod_folder_by_base(args.game_mods, mod_base)
        if not game_mod_dir:
            print(f"[MISSING GAME MOD] {mod_base}")
            missing_count += 1
            continue

        target_dll_name = pick_target_dll_name(mod_base, project.assembly_name)
        target_dll = os.path.join(game_mod_dir, target_dll_name)

        source_mtime = os.path.getmtime(source_dll)
        target_exists = os.path.isfile(target_dll)
        target_mtime = os.path.getmtime(target_dll) if target_exists else 0

        should_copy = not target_exists or source_mtime > target_mtime

        if should_copy and target_exists and not args.no_hash:
            try:
                should_copy = file_sha256(source_dll) != file_sha256(target_dll)
            except Exception:
                pass

        rel_source = os.path.relpath(source_dll, WORKSPACE_ROOT)
        rel_target = os.path.relpath(target_dll, WORKSPACE_ROOT) if target_dll.startswith(WORKSPACE_ROOT) else target_dll

        if should_copy:
            if args.apply:
                os.makedirs(os.path.dirname(target_dll), exist_ok=True)
                shutil.copy2(source_dll, target_dll)
                print(f"[COPIED] {rel_source} -> {rel_target}")
            else:
                print(f"[DRYRUN COPY] {rel_source} -> {rel_target}")
            copy_count += 1
        else:
            print(f"[OK] {project.project_folder_name} -> {mod_base} | {target_dll_name}")
            skip_count += 1

        rename_plan = suggest_rename(project, mod_base, rename_overrides, category_overrides)
        if rename_plan:
            rename_actions.append(rename_plan)

    if args.suggest_renames:
        if not rename_actions:
            print("[RENAMES] no rename suggestions")
        else:
            print("[RENAMES] suggested project folder renames:")
            for src, dst in rename_actions:
                print(f"  - {os.path.relpath(src, WORKSPACE_ROOT)} -> {os.path.relpath(dst, WORKSPACE_ROOT)}")

    if args.apply_renames:
        applied = 0
        for src, dst in rename_actions:
            if os.path.exists(src) and not os.path.exists(dst):
                os.rename(src, dst)
                print(f"[RENAMED] {os.path.relpath(src, WORKSPACE_ROOT)} -> {os.path.relpath(dst, WORKSPACE_ROOT)}")
                applied += 1
        print(f"[RENAMES APPLIED] {applied}")

    print(
        "\nSummary: "
        f"copied_or_would_copy={copy_count} | up_to_date={skip_count} | missing_or_unmapped={missing_count}"
    )

    if args.verbose:
        print(f"Projects scanned: {len(projects)}")
        print(f"NoEAC mod bases discovered: {len(mod_bases)}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
