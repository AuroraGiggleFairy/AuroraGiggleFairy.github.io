"""
Standalone Nexus Mods update script.

Pushes newer zip file versions to existing Nexus mod pages.
Cannot create brand-new mod pages — that stays manual on the site.

Usage:
    python SCRIPT-NexusUpdate.py --dry-run
    python SCRIPT-NexusUpdate.py --mod AGF-NoEAC-Toolbelt12Slots
"""

import argparse
import os
import subprocess
import sys

sys.dont_write_bytecode = True

NEXUS_DATA_DIR = os.path.dirname(os.path.abspath(__file__))
NEXUS_ROOT_DIR = os.path.dirname(NEXUS_DATA_DIR)
NEXUS_SCRIPT = os.path.join(NEXUS_DATA_DIR, "SCRIPT-NexusMods.py")
NEXUS_CONFIG = os.path.join(NEXUS_DATA_DIR, "nexusmods-config.json")
PRIVATE_API_KEY_PATH = os.path.join(NEXUS_ROOT_DIR, "nexus-api-key.private.txt")
API_KEY_ENV_VAR = "AGF_NEXUSMODS_API_KEY"


def load_api_key() -> str:
    api_key = os.getenv(API_KEY_ENV_VAR, "").strip()
    if api_key:
        return api_key
    if os.path.isfile(PRIVATE_API_KEY_PATH):
        try:
            with open(PRIVATE_API_KEY_PATH, "r", encoding="utf-8") as handle:
                return handle.readline().strip()
        except OSError:
            return ""
    return ""


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Update existing Nexus mod pages with newer file versions."
    )
    parser.add_argument(
        "--only",
        choices=["all", "publish", "update", "review", "skip"],
        default="update",
        help="Which mods to process (default: update)",
    )
    parser.add_argument(
        "--mod",
        default="",
        help="Optional single base mod name (e.g. AGF-NoEAC-Toolbelt12Slots)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Validate what would be updated without changing Nexus",
    )
    args = parser.parse_args()

    api_key = load_api_key()
    if not api_key:
        print("=" * 60)
        print("  NEXUS UPDATE — API KEY REQUIRED")
        print("=" * 60)
        print(f"  Set the environment variable: {API_KEY_ENV_VAR}")
        print(f"  Or put the key in: {PRIVATE_API_KEY_PATH}")
        print("=" * 60)
        return 1
    os.environ[API_KEY_ENV_VAR] = api_key

    if not os.path.isfile(NEXUS_SCRIPT):
        print(f"ERROR: Missing {NEXUS_SCRIPT}")
        return 1
    if not os.path.isfile(NEXUS_CONFIG):
        print(f"ERROR: Missing {NEXUS_CONFIG}")
        return 1

    command = [
        sys.executable, NEXUS_SCRIPT,
        "--mode", "upload",
        "--only", args.only,
        "--config", NEXUS_CONFIG,
    ]
    if args.mod:
        command.extend(["--mod", args.mod])
    if args.dry_run:
        command.append("--dry-run")

    print("=" * 60)
    print("  NEXUS MODS UPDATE")
    print("  Existing pages only (no new mod-page creation)")
    print(f"  Filter: {args.only}")
    if args.mod:
        print(f"  Mod: {args.mod}")
    if args.dry_run:
        print("  MODE: DRY RUN (validate only)")
    print("=" * 60)
    print()

    result = subprocess.run(command, check=False)

    print()
    print("=" * 60)
    if result.returncode == 0:
        print("  Update complete." if not args.dry_run else "  Dry-run validation complete.")
    else:
        print(f"  Update finished with issues (exit code {result.returncode}).")
    print("=" * 60)
    return result.returncode


if __name__ == "__main__":
    sys.exit(main())
