import argparse
import os
import subprocess
import sys

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
ENGINE_SCRIPT = os.path.join(WORKFLOW_DIR, "05_pipeline_engine.py")


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


def main() -> int:
    parser = argparse.ArgumentParser(description="Step 02: promote tested mods to release")
    add_common_cli_args(parser)
    args = parser.parse_args()
    return run_engine_mode("promote", args)


if __name__ == "__main__":
    sys.exit(main())
