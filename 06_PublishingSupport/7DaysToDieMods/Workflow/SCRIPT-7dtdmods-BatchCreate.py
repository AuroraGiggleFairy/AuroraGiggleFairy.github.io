"""Batch runner for 7dtdmods new mod creation.

Reads Publish-Checklist.md for mods needing first-time upload, and runs:
SCRIPT-7dtdmods-Playwright.py create <mod> --no-keep-open [--publish]
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
PLAYWRIGHT_SCRIPT = HERE / "SCRIPT-7dtdmods-Playwright.py"
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
                    status_7dtd = parts[5]
                    mode = state.get(mod_name, {}).get("mode", "auto")
                    if (mod_name in EXCLUDE_MODS or mode == "skip") and not target_mod:
                        continue
                    if target_mod:
                        if target_mod.lower() in mod_name.lower():
                            mods.append((mod_name, ver, status_7dtd))
                    elif "upload" in status_7dtd.lower():
                        mods.append((mod_name, ver, status_7dtd))
    return mods


def cleanup_browser_tabs() -> None:
    try:
        import urllib.request, json
        with urllib.request.urlopen("http://127.0.0.1:9222/json/list", timeout=2) as resp:
            tabs = json.load(resp)
        for t in tabs:
            if "edit" in t.get("url", ""):
                urllib.request.urlopen(f"http://127.0.0.1:9222/json/close/{t['id']}", timeout=2)
    except Exception:
        pass


def main() -> None:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(line_buffering=True)
    parser = argparse.ArgumentParser(description="Batch create new mods on 7daystodiemods.com")
    parser.add_argument("--publish", action="store_true", help="Agree and click Push Live / Publish automatically for each mod")
    parser.add_argument("--mod", help="Create only this specific mod")
    parser.add_argument("--limit", type=int, default=0, help="Limit number of mods to process")
    parser.add_argument("--delay", type=int, default=15, help="Pacing delay in seconds between mods to avoid site rate limits (default 15)")
    parser.add_argument("--dry-run", action="store_true", help="List mods that would be created without running Playwright")
    args = parser.parse_args()

    mods = get_mods_needing_create(args.mod)
    if args.limit > 0:
        mods = mods[:args.limit]

    total = len(mods)
    print("==================================================")
    print(f"  7DAYSTODIEMODS BATCH CREATE ({total} mods)")
    if args.publish:
        print("  Mode: AUTO-PUBLISH (Push Live)")
    else:
        print("  Mode: PREPARE DRAFTS (Leave on Publish Review page)")
    print("==================================================")

    if total == 0:
        print("No mods match the create criteria.")
        return

    if args.dry_run:
        print("\nDry-run mode. Mods to create:")
        for i, (m, v, s) in enumerate(mods, 1):
            print(f"  {i:2d}. {m} (v{v}, status {s})")
        return

    successes = []
    failures = []

    with open(LOG_FILE, "a", encoding="utf-8") as log:
        mode_str = "AUTO-PUBLISH" if args.publish else "DRAFTS"
        log.write(f"\n\n==== Batch Create [{mode_str}]: {time.strftime('%Y-%m-%d %H:%M:%S')} ({total} mods) ====\n")

    for i, (mod, ver, status) in enumerate(mods, 1):
        if i > 1 and args.delay > 0:
            print(f"  Pacing delay: waiting {args.delay}s to respect site rate limits...", flush=True)
            time.sleep(args.delay)

        print(f"\n[{i}/{total}] Creating {mod} (v{ver})...", flush=True)
        cmd = [
            sys.executable,
            str(PLAYWRIGHT_SCRIPT),
            "create",
            mod,
            "--no-keep-open",
        ]
        if args.publish:
            cmd.append("--publish")

        cleanup_browser_tabs()
        start_t = time.time()
        combined_out = ""
        success = False
        for attempt in range(1, 3):
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
                proc.wait(timeout=240)
                combined_out = "".join(output_lines)
                if proc.returncode == 0:
                    success = True
                    break
                if attempt == 1 and any(sig in combined_out.lower() for sig in ["too fast", "429", "rate limit", "going too fast"]):
                    print("  [Rate Limit Detected] Pausing 65s for site cooldown before retry...", flush=True)
                    time.sleep(65)
                    cleanup_browser_tabs()
                    start_t = time.time()
                    continue
            except subprocess.TimeoutExpired:
                proc.kill()
                if attempt == 1:
                    print("  [Timeout Detected] Pausing 65s for site cooldown before retry...", flush=True)
                    time.sleep(65)
                    cleanup_browser_tabs()
                    start_t = time.time()
                    continue
                break
            except Exception:
                break

        elapsed = time.time() - start_t
        if success:
            status_label = "PUBLISHED" if args.publish else "DRAFT READY"
            print(f"  -> SUCCESS: {status_label} ({elapsed:.1f}s)", flush=True)
            successes.append(mod)
            with open(LOG_FILE, "a", encoding="utf-8") as log:
                log.write(f"[{i}/{total}] SUCCESS {mod} ({elapsed:.1f}s)\n")
            if args.publish:
                try:
                    subprocess.run([sys.executable, str(GEN_CHECKLIST_SCRIPT)], capture_output=True, check=False)
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
    print("  BATCH CREATE COMPLETE")
    print(f"  Success: {len(successes)} / {total}")
    print(f"  Failed:  {len(failures)} / {total}")
    print("==================================================")

    if args.publish and successes:
        print("\nRegenerating Publish-Checklist.md...")
        try:
            subprocess.run([sys.executable, str(GEN_CHECKLIST_SCRIPT)], check=True)
        except Exception as e:
            print(f"Warning: could not regenerate checklist: {e}")


if __name__ == "__main__":
    main()
