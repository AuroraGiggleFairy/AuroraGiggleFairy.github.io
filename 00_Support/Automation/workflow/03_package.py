import argparse
import os
import subprocess
import sys

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
ENGINE_SCRIPT = os.path.join(WORKFLOW_DIR, "05_pipeline_engine.py")
VS_CODE_ROOT = os.path.dirname(WORKFLOW_DIR)
BANNER_SCRIPT = os.path.join(VS_CODE_ROOT, "SCRIPT-GenerateModBanners.py")


def add_common_cli_args(parser: argparse.ArgumentParser) -> None:
    parser.add_argument("--dry-run", action="store_true", help="Show what would happen without changing files")
    parser.add_argument("--verbose", action="store_true", help="Show detailed logs")
    parser.add_argument("--strict", action="store_true", help="Stop if required folders or templates are missing")
    parser.add_argument(
        "--workers",
        type=int,
        default=max(2, min(8, os.cpu_count() or 2)),
        help="How many files can be zipped at the same time",
    )


def run_engine_mode(mode: str, args: argparse.Namespace) -> int:
    command = [sys.executable, ENGINE_SCRIPT, "--mode", mode]
    if args.dry_run:
        command.append("--dry-run")
    if args.verbose:
        command.append("--verbose")
    if args.strict:
        command.append("--strict")
    if args.workers is not None:
        command.extend(["--workers", str(args.workers)])

    print(f"[Workflow] Running mode '{mode}': {' '.join(command)}")
    completed = subprocess.run(command, check=False)
    return int(completed.returncode)


def run_banner_generation(args: argparse.Namespace) -> int:
    if not os.path.isfile(BANNER_SCRIPT):
        print(f"[Workflow] Banner script not found, skipping: {BANNER_SCRIPT}")
        return 0

    command = [sys.executable, BANNER_SCRIPT]
    if args.dry_run:
        command.append("--dry-run")

    print(f"[Workflow] Generating README banners: {' '.join(command)}")
    completed = subprocess.run(command, check=False)
    return int(completed.returncode)


def main() -> int:
    parser = argparse.ArgumentParser(description="Step 03: package release files")
    add_common_cli_args(parser)
    args = parser.parse_args()
    banner_exit = run_banner_generation(args)
    if banner_exit != 0:
        print(f"[Workflow] Banner generation failed ({banner_exit}); continuing package step")
    return run_engine_mode("package", args)


if __name__ == "__main__":
    sys.exit(main())
