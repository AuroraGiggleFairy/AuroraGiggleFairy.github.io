import argparse
import os
import subprocess
import sys
from typing import List, Tuple

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))

STEP_SCRIPTS: List[Tuple[str, str]] = [
    ("sync-work", os.path.join(WORKFLOW_DIR, "01_sync_work.py")),
    ("promote", os.path.join(WORKFLOW_DIR, "02_promote.py")),
    ("package", os.path.join(WORKFLOW_DIR, "03_package.py")),
]


def build_step_command(step_script: str, args: argparse.Namespace) -> List[str]:
    command = [sys.executable, step_script]
    if args.dry_run:
        command.append("--dry-run")
    if args.verbose:
        command.append("--verbose")
    if args.strict:
        command.append("--strict")
    if args.workers is not None:
        command.extend(["--workers", str(args.workers)])
    return command


def main() -> int:
    parser = argparse.ArgumentParser(description="Run all workflow steps in order")
    parser.add_argument("--dry-run", action="store_true", help="Show what would happen without changing files")
    parser.add_argument("--verbose", action="store_true", help="Show detailed logs")
    parser.add_argument("--strict", action="store_true", help="Stop if required folders or templates are missing")
    parser.add_argument(
        "--workers",
        type=int,
        default=max(2, min(8, os.cpu_count() or 2)),
        help="How many files can be zipped at the same time",
    )
    parser.add_argument(
        "--continue-on-error",
        action="store_true",
        help="Continue to the next step even if one step fails",
    )
    args = parser.parse_args()

    overall_exit_code = 0
    for step_name, step_script in STEP_SCRIPTS:
        command = build_step_command(step_script, args)
        print(f"[Workflow] Step '{step_name}' starting: {' '.join(command)}")
        completed = subprocess.run(command, check=False)
        if completed.returncode != 0:
            overall_exit_code = int(completed.returncode)
            print(f"[Workflow] Step '{step_name}' failed with code {completed.returncode}")
            if not args.continue_on_error:
                return overall_exit_code
        else:
            print(f"[Workflow] Step '{step_name}' completed")

    return overall_exit_code


if __name__ == "__main__":
    sys.exit(main())
