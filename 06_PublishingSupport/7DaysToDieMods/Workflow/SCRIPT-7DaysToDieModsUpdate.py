"""Create satellite GitHub repos and Releases for 7DaysToDieMods sync.

Does not create site pages or change descriptions/images.
Does not run on every git push — only when you run this script.

Usage:
    python SCRIPT-7DaysToDieModsUpdate.py --dry-run
    python SCRIPT-7DaysToDieModsUpdate.py --repos-only
    python SCRIPT-7DaysToDieModsUpdate.py --mod AGF-BackpackPlus-072Slots
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import os
import re
import shutil
import subprocess
import sys

sys.dont_write_bytecode = True

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
SITE_ROOT = os.path.dirname(WORKFLOW_DIR)
VS_CODE_ROOT = os.path.normpath(os.path.join(SITE_ROOT, "..", ".."))
CONFIG_PATH = os.path.join(WORKFLOW_DIR, "7dtdmods-config.json")
ZIP_DIR = os.path.join(VS_CODE_ROOT, "04_DownloadZips")
NEXUS_STEP_PATH = os.path.join(
    VS_CODE_ROOT, "00_Support", "Automation", "workflow", "06_nexus.py"
)

REPO_DESCRIPTION = (
    "Release-only repo for 7DaysToDieMods sync. Source stays in the AGF workspace."
)


def load_nexus_step():
    spec = importlib.util.spec_from_file_location("nexus_step6", NEXUS_STEP_PATH)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load {NEXUS_STEP_PATH}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def find_gh() -> str:
    for candidate in (
        os.path.join(os.environ.get("ProgramFiles", r"C:\Program Files"), "GitHub CLI", "gh.exe"),
        os.path.join(os.environ.get("LOCALAPPDATA", ""), "GitHub CLI", "gh.exe"),
        shutil.which("gh") or "",
    ):
        if candidate and os.path.isfile(candidate):
            return candidate
    return ""


def run_gh(gh: str, args: list[str]) -> subprocess.CompletedProcess:
    return subprocess.run(
        [gh, *args],
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )


def load_config() -> dict:
    if not os.path.isfile(CONFIG_PATH):
        return {"github_owner": "AuroraGiggleFairy", "defaults": {}, "mods": {}}
    with open(CONFIG_PATH, "r", encoding="utf-8") as handle:
        data = json.load(handle)
    if not isinstance(data, dict):
        return {"github_owner": "AuroraGiggleFairy", "defaults": {}, "mods": {}}
    data.setdefault("defaults", {})
    data.setdefault("mods", {})
    return data


def save_config(config: dict) -> None:
    with open(CONFIG_PATH, "w", encoding="utf-8") as handle:
        json.dump(config, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def latest_changelog_plain(nexus, readme: str) -> str:
    formatted = nexus.extract_changelog_entries(readme)
    match = re.search(r"```text\n(.*?)```", formatted, re.DOTALL)
    if not match:
        return ""
    return match.group(1).strip()


def ensure_repo(gh: str, owner: str, repo: str, dry_run: bool) -> tuple[bool, str]:
    full = f"{owner}/{repo}"
    viewed = run_gh(gh, ["repo", "view", full, "--json", "name"])
    if viewed.returncode == 0:
        return False, f"exists {full}"
    if dry_run:
        return True, f"would create {full}"
    created = run_gh(
        gh,
        [
            "repo",
            "create",
            full,
            "--public",
            "--add-readme",
            "--description",
            REPO_DESCRIPTION,
        ],
    )
    if created.returncode != 0:
        return False, f"CREATE FAILED {full}: {(created.stderr or created.stdout).strip()}"
    return True, f"created {full}"


def release_exists(gh: str, owner: str, repo: str, tag: str) -> bool:
    checked = run_gh(gh, ["release", "view", tag, "--repo", f"{owner}/{repo}"])
    return checked.returncode == 0


def create_release(
    gh: str,
    owner: str,
    repo: str,
    tag: str,
    notes: str,
    zip_path: str,
    dry_run: bool,
) -> str:
    if release_exists(gh, owner, repo, tag):
        return f"release {tag} already exists"
    if not os.path.isfile(zip_path):
        return f"SKIP release {tag}: zip missing ({zip_path})"
    if dry_run:
        return f"would create release {tag} with {os.path.basename(zip_path)}"
    created = run_gh(
        gh,
        [
            "release",
            "create",
            tag,
            "--repo",
            f"{owner}/{repo}",
            "--title",
            tag,
            "--notes",
            notes or tag,
            zip_path,
        ],
    )
    if created.returncode != 0:
        return f"RELEASE FAILED {tag}: {(created.stderr or created.stdout).strip()}"
    return f"created release {tag}"


def process_mods(
    *,
    dry_run: bool,
    repos_only: bool,
    releases_only: bool,
    only_mod: str,
) -> int:
    nexus = load_nexus_step()
    config = load_config()
    defaults = config.get("defaults") if isinstance(config.get("defaults"), dict) else {}
    owner = str(defaults.get("github_owner") or config.get("github_owner") or "AuroraGiggleFairy")
    mods_cfg = config.get("mods")
    if not isinstance(mods_cfg, dict):
        mods_cfg = {}
        config["mods"] = mods_cfg

    gh = find_gh()
    if not gh:
        print("ERROR: GitHub CLI (gh.exe) not found.")
        return 1
    auth = run_gh(gh, ["auth", "status"])
    if auth.returncode != 0:
        print("ERROR: gh is not logged in. Run:")
        print('  & "C:\\Program Files\\GitHub CLI\\gh.exe" auth login')
        return 1

    entries = nexus.find_mod_entries()
    if only_mod:
        entries = [entry for entry in entries if entry["base_name"] == only_mod]
        if not entries:
            print(f"ERROR: {only_mod} not found in 03_ReleaseSource.")
            return 1

    print("=" * 60)
    print("  7DAYSTODIEMODS GITHUB SATELLITE UPDATE")
    print(f"  Owner: {owner}")
    if dry_run:
        print("  MODE: DRY RUN")
    if repos_only:
        print("  Repos only (no Releases)")
    if releases_only:
        print("  Releases only (no new repos)")
    print("=" * 60)

    created_repos = 0
    created_releases = 0
    errors = 0
    config_changed = False

    for entry in entries:
        base_name = entry["base_name"]
        cfg_entry = mods_cfg.get(base_name)
        if not isinstance(cfg_entry, dict):
            cfg_entry = {}
            mods_cfg[base_name] = cfg_entry
            config_changed = True
        repo = str(cfg_entry.get("github_repo") or base_name)
        if cfg_entry.get("github_repo") != repo:
            cfg_entry["github_repo"] = repo
            config_changed = True

        print(f"\n  {base_name}")
        if not releases_only:
            made, message = ensure_repo(gh, owner, repo, dry_run)
            print(f"    repo: {message}")
            if "FAILED" in message:
                errors += 1
                continue
            if made:
                created_repos += 1

        if repos_only:
            continue

        mod_info = nexus.load_modinfo_xml(entry["folder_path"])
        version = str(mod_info.get("version") or "").strip()
        if not version or version == "?" or version.startswith("0."):
            print(f"    release: SKIP (version {version or '?'})")
            continue
        readme = nexus.load_readme_text(entry["folder_path"])
        notes = latest_changelog_plain(nexus, readme) or version
        zip_path = os.path.join(ZIP_DIR, f"{base_name}.zip")
        message = create_release(gh, owner, repo, version, notes, zip_path, dry_run)
        print(f"    release: {message}")
        if "FAILED" in message:
            errors += 1
        elif message.startswith("created release") or message.startswith("would create release"):
            created_releases += 1
            if not dry_run:
                cfg_entry["last_release_tag"] = version
                config_changed = True

    if config_changed and not dry_run:
        save_config(config)

    print()
    print("=" * 60)
    print(f"  Repos created: {created_repos}")
    print(f"  Releases created: {created_releases}")
    if errors:
        print(f"  Errors: {errors}")
    print("=" * 60)
    return 1 if errors else 0


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Create 7DaysToDieMods satellite GitHub repos and Releases."
    )
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--mod", default="", help="Single ReleaseSource base name")
    parser.add_argument(
        "--repos-only",
        action="store_true",
        help="Create missing empty repos only (used by Publish).",
    )
    parser.add_argument(
        "--releases-only",
        action="store_true",
        help="Create missing Releases only.",
    )
    parser.add_argument("--verbose", action="store_true")
    parser.add_argument("--strict", action="store_true")
    parser.add_argument("--workers", type=int, default=None)
    args = parser.parse_args()
    if args.repos_only and args.releases_only:
        print("ERROR: --repos-only and --releases-only cannot be combined.")
        return 1
    return process_mods(
        dry_run=args.dry_run,
        repos_only=args.repos_only,
        releases_only=args.releases_only,
        only_mod=args.mod.strip(),
    )


if __name__ == "__main__":
    sys.exit(main())
