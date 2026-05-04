import argparse
import os
import subprocess
import sys
from typing import List

VS_CODE_ROOT = os.path.dirname(os.path.abspath(__file__))
WORKFLOW_DIR = os.path.join(VS_CODE_ROOT, "Workflow")
CHAIN_SCRIPT = os.path.join(WORKFLOW_DIR, "04_run_chain.py")
ENGINE_SCRIPT = os.path.join(WORKFLOW_DIR, "05_pipeline_engine.py")
BANNER_SCRIPT = os.path.join(VS_CODE_ROOT, "SCRIPT-GenerateModBanners.py")


def add_common_cli_args(parser: argparse.ArgumentParser) -> None:
    parser.add_argument("--dry-run", action="store_true", help="Show what would happen without changing files")
    parser.add_argument("--verbose", action="store_true", help="Show detailed logs")
    parser.add_argument("--strict", action="store_true", help="Stop if required folders or templates are missing")
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
        help="How many files can be zipped at the same time",
    )


def append_common_flags(command: List[str], args: argparse.Namespace) -> List[str]:
    if args.dry_run:
        command.append("--dry-run")
    if args.verbose:
        command.append("--verbose")
    if args.strict:
        command.append("--strict")
    command.append("--fail-fast" if args.fail_fast else "--no-fail-fast")
    command.append("--transaction-rollback" if args.transaction_rollback else "--no-transaction-rollback")
    command.extend(["--quarantine-retention-days", str(args.quarantine_retention_days)])
    command.append("--enforce-agf-csv" if args.enforce_agf_csv else "--no-enforce-agf-csv")
    command.append("--preflight-write-check" if args.preflight_write_check else "--no-preflight-write-check")
    if args.workers is not None:
        command.extend(["--workers", str(args.workers)])
    return command


def run_chain(args: argparse.Namespace) -> int:
    if not os.path.isfile(CHAIN_SCRIPT):
        print(f"Missing workflow chain script: {CHAIN_SCRIPT}")
        return 1

    command = [sys.executable, CHAIN_SCRIPT]
    append_common_flags(command, args)
    if args.continue_on_error:
        command.append("--continue-on-error")

    print(f"[SCRIPT-Main] Launching workflow chain: {' '.join(command)}")
    return int(subprocess.run(command, check=False).returncode)


def run_single_mode(args: argparse.Namespace) -> int:
    if args.mode == "banners":
        if not os.path.isfile(BANNER_SCRIPT):
            print(f"Missing banner script: {BANNER_SCRIPT}")
            return 1
        command = [sys.executable, BANNER_SCRIPT]
        if args.dry_run:
            command.append("--dry-run")
        print(f"[SCRIPT-Main] Running single mode 'banners': {' '.join(command)}")
        return int(subprocess.run(command, check=False).returncode)

    if not os.path.isfile(ENGINE_SCRIPT):
        print(f"Missing workflow engine script: {ENGINE_SCRIPT}")
        return 1

    command = [sys.executable, ENGINE_SCRIPT, "--mode", args.mode]
    append_common_flags(command, args)

    print(f"[SCRIPT-Main] Running single mode '{args.mode}': {' '.join(command)}")
    engine_exit = int(subprocess.run(command, check=False).returncode)

    if args.mode == "update" and os.path.isfile(BANNER_SCRIPT):
        banner_cmd = [sys.executable, BANNER_SCRIPT, "--changed-only"]
        if args.dry_run:
            banner_cmd.append("--dry-run")
        print(f"[SCRIPT-Main] Generating update media (post-engine): {' '.join(banner_cmd)}")
        banner_exit = int(subprocess.run(banner_cmd, check=False).returncode)
        if banner_exit != 0:
            print(f"[SCRIPT-Main] Banner generation failed ({banner_exit})")

    if args.mode == "package" and os.path.isfile(BANNER_SCRIPT):
        banner_cmd = [sys.executable, BANNER_SCRIPT]
        if args.dry_run:
            banner_cmd.append("--dry-run")
        print(f"[SCRIPT-Main] Generating package media (post-engine): {' '.join(banner_cmd)}")
        banner_exit = int(subprocess.run(banner_cmd, check=False).returncode)
        if banner_exit != 0:
            print(f"[SCRIPT-Main] Banner generation failed ({banner_exit})")

    return engine_exit


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Main starter script for the AGF workflow."
    )
    parser.add_argument(
        "--mode",
        choices=["full", "update", "sync-work", "prep-work", "promote", "package", "self-test", "banners"],
        help=(
            "Run one specific part only. "
            "If omitted, SCRIPT-Main runs everything in order: sync-work -> promote -> package."
        ),
    )
    parser.add_argument(
        "--continue-on-error",
        action="store_true",
        help="When running the full chain, continue to the next step even if one fails",
    )
    add_common_cli_args(parser)
    return parser


def main() -> int:
    parser = build_arg_parser()
    args = parser.parse_args()

    if args.mode:
        return run_single_mode(args)
    return run_chain(args)


if __name__ == "__main__":
    sys.exit(main())
