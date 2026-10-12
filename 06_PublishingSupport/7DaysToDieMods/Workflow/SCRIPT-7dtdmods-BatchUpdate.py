"""Batch runner for 7dtdmods updates.

Reads Publish-Checklist.md for mods needing update, and runs
SCRIPT-7dtdmods-Playwright.py update <mod> --zip --changelog --no-keep-open [--publish]
sequentially.
"""

import argparse
import sys
import json
import subprocess
import time
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO_ROOT = HERE.parents[2]
CHECKLIST = REPO_ROOT / "06_PublishingSupport" / "Publish-Checklist.md"
STATE_FILE = REPO_ROOT / "06_PublishingSupport" / "Publish-State.json"
PLAYWRIGHT_SCRIPT = HERE / "SCRIPT-7dtdmods-Playwright.py"
GEN_CHECKLIST_SCRIPT = REPO_ROOT / "06_PublishingSupport" / "SCRIPT-GeneratePublishChecklist.py"
LOG_FILE = HERE / "_batch_update_log.txt"


def load_state() -> dict:
    if STATE_FILE.is_file():
        try:
            with open(STATE_FILE, "r", encoding="utf-8") as f:
                return json.load(f)
        except Exception:
            return {}
    return {}


def get_mods_needing_update(target_mod: str | None = None, include_done: bool = False, state: dict | None = None) -> list[tuple[str, str, str]]:
    mods = []
    if not CHECKLIST.exists():
        print(f"Checklist not found: {CHECKLIST}")
        return mods

    with open(CHECKLIST, "r", encoding="utf-8") as f:
        for line in f:
            if "|" in line and not line.strip().startswith("| Mod") and not line.strip().startswith("|---") and not line.strip().startswith("**Summary"):
                parts = [p.strip() for p in line.split("|")]
                if len(parts) >= 7:
                    mod_name = parts[1]
                    ver = parts[2]
                    beta = parts[4].lower()
                    status_7dtd = parts[5]
                    st_lower = status_7dtd.lower()
                    if st_lower in ("skip", "packaged"):
                        continue
                    if target_mod:
                        if target_mod.lower() in mod_name.lower():
                            mods.append((mod_name, ver, status_7dtd))
                    elif include_done:
                        if "upload" not in st_lower:
                            if state:
                                d7 = state.get(mod_name, {}).get("7dtd", {})
                                if d7.get("needs_title") or d7.get("needs_desc") or d7.get("needs_zip") or "update" in st_lower:
                                    mods.append((mod_name, ver, status_7dtd))
                            else:
                                mods.append((mod_name, ver, status_7dtd))
                    elif "update" in st_lower:
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
    parser = argparse.ArgumentParser(description="Batch update mods on 7daystodiemods.com")
    parser.add_argument("--publish", action="store_true", help="Agree and click Push Live / Publish automatically for each mod")
    parser.add_argument("--mod", help="Update only this specific mod")
    parser.add_argument("--limit", type=int, default=0, help="Limit number of mods to process")
    parser.add_argument("--delay", type=int, default=15, help="Pacing delay in seconds between mods to avoid site rate limits (default 15)")
    parser.add_argument("--files-only", action="store_true", help="Force files-only update (skip details and media for all mods)")
    parser.add_argument("--no-media", action="store_true", help="Force details + files update, skip media images for all mods")
    parser.add_argument("--force-full", action="store_true", help="Force full update (details, media, and files for all mods)")
    parser.add_argument("--all", action="store_true", help="Process all listed mods on 7dtdmods, including those marked 'done'")
    parser.add_argument("--dry-run", action="store_true", help="List mods that would be updated without running Playwright")
    args = parser.parse_args()

    state = load_state()
    mods = get_mods_needing_update(args.mod, include_done=args.all, state=state)
    if args.limit > 0:
        mods = mods[:args.limit]

    total = len(mods)
    print("==================================================")
    print(f"  7DAYSTODIEMODS BATCH UPDATE ({total} mods)")
    if args.publish:
        print("  Mode: AUTO-PUBLISH (Push Live)")
    else:
        print("  Mode: PREPARE DRAFTS (Leave on Publish Review page)")
    print("==================================================")

    if total == 0:
        print("No mods match the update criteria.")
        return

    if args.dry_run:
        print("\nDry-run mode. Mods to update:")
        for i, (m, v, s) in enumerate(mods, 1):
            m_state = state.get(m, {})
            mode = m_state.get("mode", "auto")
            d7 = m_state.get("7dtd", {})
            if mode == "skip":
                plan = "SKIP"
            elif args.files_only or mode == "files":
                plan = "Fast Delta (files + changelog only)"
            elif args.no_media:
                plan = "Delta (title + desc + files)"
            elif args.force_full or mode == "full":
                plan = "Full Update (details + media + files)"
            else:
                nt = d7.get("needs_title", False)
                nd = d7.get("needs_desc", False) or nt
                nm = d7.get("needs_media", False)
                if not nd and not nm:
                    plan = "Fast Delta (files + changelog only)"
                elif nd and not nm:
                    plan = "Delta (title + desc + files)"
                elif nm and not nd:
                    plan = "Delta (media + files)"
                else:
                    plan = "Full Update"
            print(f"  {i:2d}. {m} (target v{v}, current {s}) -> {plan}")
        return

    successes = []
    failures = []

    with open(LOG_FILE, "a", encoding="utf-8") as log:
        mode_str = "AUTO-PUBLISH" if args.publish else "DRAFTS"
        log.write(f"\n\n==== Batch Run [{mode_str}]: {time.strftime('%Y-%m-%d %H:%M:%S')} ({total} mods) ====\n")

    for i, (mod, ver, status) in enumerate(mods, 1):
        m_state = state.get(mod, {})
        mode = m_state.get("mode", "auto")
        if mode == "skip" and not args.mod:
            print(f"\n[{i}/{total}] Skipping {mod} (mode set to 'skip' in Publishing Matrix)", flush=True)
            continue

        if i > 1 and args.delay > 0:
            print(f"  Pacing delay: waiting {args.delay}s to respect site rate limits...", flush=True)
            time.sleep(args.delay)

        # Determine delta flags
        d7 = m_state.get("7dtd", {})
        delta_args = []
        needs_new_zip = d7.get("needs_zip", False) or ("update" in status.lower()) or ("upload" in status.lower())
        delta_plan_str = "fast delta (zip + changelog + perms)"
        if args.force_full or mode == "full":
            delta_args = []
            delta_plan_str = "full update (details + media + files)"
        elif args.files_only or mode == "files":
            delta_args = ["--files-only"]
            delta_plan_str = "fast delta (files only)"
        elif args.no_media:
            delta_args = ["--no-media"]
            delta_plan_str = "delta (title + desc + files)"
        else:
            nt = d7.get("needs_title", False)
            nd = d7.get("needs_desc", False) or nt
            nm = d7.get("needs_media", False)
            if not nd and not nm:
                delta_args = ["--files-only"]
                delta_plan_str = "fast delta (zip + changelog only)"
            elif nd and not nm:
                delta_args = ["--no-media"]
                delta_plan_str = "delta (title + desc + files)"
            elif nm and not nd:
                delta_args = ["--images-only"]
                delta_plan_str = "delta (media + files)"
            else:
                delta_args = []
                delta_plan_str = "full update"

        print(f"\n[{i}/{total}] Updating {mod} (v{ver}, was {status}) [{delta_plan_str}]...", flush=True)
        cmd = [
            sys.executable,
            str(PLAYWRIGHT_SCRIPT),
            "update",
            mod,
            "--no-keep-open",
        ] + delta_args
        if needs_new_zip:
            cmd.extend(["--zip", "--changelog"])
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
                proc.wait(timeout=210)
                combined_out = "".join(output_lines)
                if proc.returncode == 0:
                    success = True
                    break
                # Check for rate limit indicators in output
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
            except Exception as e:
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
    print("  BATCH RUN COMPLETE")
    print(f"  Success: {len(successes)} / {total}")
    print(f"  Failed:  {len(failures)} / {total}")
    print("==================================================")

    if args.publish and successes:
        print("\nRegenerating Publish-Checklist.md...")
        try:
            subprocess.run([sys.executable, str(GEN_CHECKLIST_SCRIPT)], check=True)
        except Exception as e:
            print(f"Failed to regenerate checklist: {e}")


if __name__ == "__main__":
    main()
