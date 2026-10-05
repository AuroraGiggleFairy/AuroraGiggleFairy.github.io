"""
Workflow Step 6b — 7DaysToDieMods PublishHelp + missing satellite repos.

Packets are local only. Repo create uses `gh` if logged in.
Does not create GitHub Releases (that is RUN-7DaysToDieMods-Update.bat).
"""

from __future__ import annotations

import argparse
import importlib.util
import os
import sys

sys.dont_write_bytecode = True

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
VS_CODE_ROOT = os.path.normpath(os.path.join(WORKFLOW_DIR, "..", "..", ".."))
SITE_WORKFLOW = os.path.join(
    VS_CODE_ROOT, "06_PublishingSupport", "7DaysToDieMods", "Workflow"
)
PUBLISHHELP_PATH = os.path.join(SITE_WORKFLOW, "SCRIPT-7DaysToDieModsPublishHelp.py")
UPDATE_PATH = os.path.join(SITE_WORKFLOW, "SCRIPT-7DaysToDieModsUpdate.py")


def load_script(path: str, module_name: str):
    spec = importlib.util.spec_from_file_location(module_name, path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load {path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Step 6b — 7DaysToDieMods PublishHelp and satellite repos"
    )
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--skip-repos", action="store_true")
    parser.add_argument("--verbose", action="store_true")
    parser.add_argument("--strict", action="store_true")
    parser.add_argument("--workers", type=int, default=None)
    args = parser.parse_args()

    print("=" * 60)
    print("  STEP 6b — 7DAYSTODIEMODS PUBLISHHELP")
    print("  Local packets; optional empty satellite repos")
    print("=" * 60)

    publishhelp = load_script(PUBLISHHELP_PATH, "dtd_publishhelp")
    help_code = publishhelp.generate_publish_help(dry_run=args.dry_run)
    if help_code != 0:
        return help_code

    if args.skip_repos:
        print("  Skipping satellite repo create (--skip-repos).")
        return 0

    print()
    print("  Creating missing satellite repos (no Releases)...")
    updater = load_script(UPDATE_PATH, "dtd_update")
    try:
        repo_code = updater.process_mods(
            dry_run=args.dry_run,
            repos_only=True,
            releases_only=False,
            only_mod="",
        )
    except Exception as exc:
        print(f"  [WARN] Satellite repo step skipped: {exc}")
        return 0
    if repo_code != 0:
        print("  [WARN] Satellite repo step had errors; PublishHelp packets were still written.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
