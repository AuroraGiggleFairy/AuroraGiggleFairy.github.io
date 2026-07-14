"""
Standalone Nexus Mods upload script.

This is the ONLY script that handles uploading updates to Nexus Mods.
It runs independently of the publish chain and is triggered manually.

Usage:
    set AGF_NEXUSMODS_API_KEY=your_key_here
    python SCRIPT-NexusUpload.py --only update
    python SCRIPT-NexusUpload.py --dry-run    (preview without uploading)
"""

import argparse
import os
import subprocess
import sys

VS_CODE_ROOT = os.path.dirname(os.path.abspath(__file__))
NEXUS_SCRIPT = os.path.join(VS_CODE_ROOT, "SCRIPT-NexusMods.py")
NEXUS_CONFIG = os.path.join(
    VS_CODE_ROOT, "05_GigglePackReleaseData", "NexusMods", "nexusmods-config.json"
)
API_KEY_ENV_VAR = "AGF_NEXUSMODS_API_KEY"


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Upload mod updates to Nexus Mods (standalone)."
    )
    parser.add_argument(
        "--only",
        choices=["all", "publish", "update", "review", "skip"],
        default="update",
        help="Which mods to process (default: update)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Preview what would be uploaded without making changes",
    )
    args = parser.parse_args()

    api_key = os.getenv(API_KEY_ENV_VAR, "").strip()
    if not api_key:
        print("=" * 60)
        print("  NEXUS UPLOAD — API KEY REQUIRED")
        print("=" * 60)
        print(f"  Set the environment variable: {API_KEY_ENV_VAR}")
        print()
        print("  Example:")
        print(f"    set {API_KEY_ENV_VAR}=your_api_key_here")
        print("=" * 60)
        return 1

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
    if args.dry_run:
        command.append("--dry-run")

    print("=" * 60)
    print("  NEXUS MODS UPLOAD")
    print(f"  Filter: {args.only}")
    if args.dry_run:
        print("  MODE: DRY RUN (no files will be uploaded)")
    print("=" * 60)
    print()

    result = subprocess.run(command, check=False)

    print()
    print("=" * 60)
    if result.returncode == 0:
        print("  Upload complete.")
    else:
        print(f"  Upload failed (exit code {result.returncode}).")
    print("=" * 60)

    return result.returncode


if __name__ == "__main__":
    sys.exit(main())