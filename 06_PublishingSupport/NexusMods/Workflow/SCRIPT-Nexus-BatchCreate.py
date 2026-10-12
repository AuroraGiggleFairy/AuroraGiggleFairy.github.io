"""Batch runner for Nexus Mods new mod creation.

Reads Publish-Checklist.md for mods needing first-time upload, and runs:
SCRIPT-NexusPlaywright.py publish-new-mod <mod>
sequentially.
"""

import argparse
import subprocess
import sys
import time
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO_ROOT = HERE.parents[2]
CHECKLIST = REPO_ROOT / "06_PublishingSupport" / "Publish-Checklist.md"
STATE_FILE = REPO_ROOT / "06_PublishingSupport" / "Publish-State.json"
PLAYWRIGHT_SCRIPT = HERE / "SCRIPT-NexusPlaywright.py"
GEN_CHECKLIST_SCRIPT = REPO_ROOT / "06_PublishingSupport" / "SCRIPT-GeneratePublishChecklist.py"
LOG_FILE = HERE / "_batch_create_log.txt"

EXCLUDE_MODS = {"zzzAGF-V3-SERVER-HelpfulRenames"}


def get_mods_needing_create(target_mod: str | None = None) -> list[tuple[str, str, str]]:
    mods = []
    if not CHECKLIST.exists():
        print(f"Checklist not found: {CHECKLIST}")
        return mods

    state = {}
    if STATE_FILE.is_file():
        try:
            import json
            with open(STATE_FILE, "r", encoding="utf-8") as f:
                state = json.load(f)
        except Exception:
            pass

    with open(CHECKLIST, "r", encoding="utf-8") as f:
        for line in f:
            if "|" in line and not line.strip().startswith("| Mod") and not line.strip().startswith("|---") and not line.strip().startswith("**Summary"):
                parts = [p.strip() for p in line.split("|")]
                if len(parts) >= 7:
                    mod_name = parts[1]
                    ver = parts[2]
                    status_nexus = parts[6]
                    mode = state.get(mod_name, {}).get("mode", "auto")
                    if (mod_name in EXCLUDE_MODS or mode == "skip") and not target_mod:
                        continue
                    if target_mod:
                        if target_mod.lower() in mod_name.lower():
                            mods.append((mod_name, ver, status_nexus))
                    elif "upload" in status_nexus.lower():
                        mods.append((mod_name, ver, status_nexus))
    return mods


def main() -> None:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(line_buffering=True)
    parser = argparse.ArgumentParser(description="Batch create new mods on Nexus Mods")
    parser.add_argument("--publish", action="store_true", help="Publish mod immediately if ready")
    parser.add_argument("--mod", help="Create only this specific mod")
    parser.add_argument("--limit", type=int, default=0, help="Limit number of mods to process")
    parser.add_argument("--delay", type=int, default=5, help="Pacing delay in seconds between mods (default 5)")
    parser.add_argument("--dry-run", action="store_true", help="List mods that would be created without running Playwright")
    args = parser.parse_args()

    mods = get_mods_needing_create(args.mod)
    if args.limit > 0:
        mods = mods[:args.limit]

    total = len(mods)
    print("==================================================")
    print(f"  NEXUS MODS BATCH CREATE ({total} mods)")
    if args.publish:
        print("  Mode: AUTO-PUBLISH (Create + Media + Files + Publish)")
    else:
        print("  Mode: STAGE NEW MODS (Draft + General + Media + Files)")
    print("==================================================")

    if total == 0:
        print("No mods match the create criteria.")
        return

    if args.dry_run:
        print("\nDry-run mode. Mods to create on Nexus:")
        for i, (m, v, s) in enumerate(mods, 1):
            print(f"  {i:2d}. {m} (v{v}, status {s})")
        return

    successes = []
    failures = []

    with open(LOG_FILE, "a", encoding="utf-8") as log:
        log.write(f"\n\n==== Nexus Batch Create: {time.strftime('%Y-%m-%d %H:%M:%S')} ({total} mods) ====\n")

    for i, (mod, ver, status) in enumerate(mods, 1):
        if i > 1 and args.delay > 0:
            print(f"  Pacing delay: waiting {args.delay}s...", flush=True)
            time.sleep(args.delay)

        print(f"\n[{i}/{total}] Staging Nexus mod {mod} (v{ver})...", flush=True)
        cmd = [
            sys.executable,
            str(PLAYWRIGHT_SCRIPT),
            "publish-new-mod",
            mod,
        ]
        if args.publish:
            cmd.append("--publish")

        start_t = time.time()
        combined_out = ""
        success = False
        try:
            proc = subprocess.Popen(
                cmd,
                cwd=str(HERE),
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                text=True,
                encoding="utf-8",
                errors="replace",
                bufsize=1,
            )
            output_lines = []
            for line in proc.stdout:
                print("    " + line, end="", flush=True)
                output_lines.append(line)
            proc.wait(timeout=300)
            combined_out = "".join(output_lines)
            if proc.returncode == 0:
                success = True
        except subprocess.TimeoutExpired:
            proc.kill()
        except Exception:
            pass

        elapsed = time.time() - start_t
        if success:
            print(f"  -> SUCCESS: STAGED ({elapsed:.1f}s)", flush=True)
            successes.append(mod)
            with open(LOG_FILE, "a", encoding="utf-8") as log:
                log.write(f"[{i}/{total}] SUCCESS {mod} ({elapsed:.1f}s)\n")
            try:
                subprocess.run([sys.executable, str(GEN_CHECKLIST_SCRIPT)], capture_output=True, check=False)
                sync_matrix_script = REPO_ROOT / "06_PublishingSupport" / "SCRIPT-SyncPublishingMatrix.py"
                if sync_matrix_script.is_file():
                    subprocess.run([sys.executable, str(sync_matrix_script)], capture_output=True, check=False)
            except Exception:
                pass
        else:
            err_msg = ""
            for line in combined_out.splitlines()[-10:]:
                if "error" in line.lower() or "exception" in line.lower() or "failed" in line.lower() or "traceback" in line.lower():
                    err_msg = line.strip()
            print(f"  -> FAILED ({elapsed:.1f}s): {err_msg or 'See log for details'}", flush=True)
            failures.append((mod, err_msg))
            with open(LOG_FILE, "a", encoding="utf-8") as log:
                log.write(f"[{i}/{total}] FAILED {mod} ({elapsed:.1f}s)\n")
                log.write(f"--- Output for {mod} ---\n{combined_out}\n---\n")

    print("\n==================================================")
    print("  NEXUS BATCH CREATE COMPLETE")
    print(f"  Success: {len(successes)} / {total}")
    print(f"  Failed:  {len(failures)} / {total}")
    print("==================================================")

    if successes:
        print("\nRegenerating Publish-Checklist.md and Matrix Canvas...")
        try:
            subprocess.run([sys.executable, str(GEN_CHECKLIST_SCRIPT)], check=True)
            sync_matrix_script = REPO_ROOT / "06_PublishingSupport" / "SCRIPT-SyncPublishingMatrix.py"
            if sync_matrix_script.is_file():
                subprocess.run([sys.executable, str(sync_matrix_script)], check=True)
        except Exception as e:
            print(f"Warning: could not regenerate checklist/matrix: {e}")


if __name__ == "__main__":
    main()
