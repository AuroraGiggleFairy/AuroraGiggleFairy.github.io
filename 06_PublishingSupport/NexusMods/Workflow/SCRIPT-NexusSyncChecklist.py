"""Nexus Mods Checklist-Driven Batch Automation (Brave Browser CDP).

Dynamically reads `06_PublishingSupport/Publish-Checklist.md` to:
1. Update existing mods (General info/BBCode, Media thumbnail/banner, Files zip + changelog)
2. Upload and stage missing mods (Draft create -> General -> Media -> Files)
3. Update `Publish-Checklist.md` in real-time as each mod finishes.

Connects to Brave via DevTools CDP (--remote-debugging-port=9222).
"""
from __future__ import annotations

import argparse
import datetime as dt
import json
import os
import re
import socket
import subprocess
import sys
import time
from pathlib import Path
from typing import Dict, List, Optional, Tuple

HERE = Path(__file__).resolve().parent
WORKFLOW_DIR = HERE
NEXUS_ROOT = HERE.parent
REPO_ROOT = HERE.parents[2]
CHECKLIST_PATH = REPO_ROOT / "06_PublishingSupport" / "Publish-Checklist.md"
CONFIG_PATH = HERE / "nexusmods-config.json"
STATUS_PATH = HERE / "Nexus-Status.json"
PUBLISH_HELP = NEXUS_ROOT / "PublishHelp"
IMAGES_DIR = REPO_ROOT / "00_Images" / "02_ImagesFinal"
ZIPS_DIR = REPO_ROOT / "04_DownloadZips"

DEFAULT_CDP = "http://127.0.0.1:9222"
CDP_PORT = 9222
SITE = "https://www.nexusmods.com"
GAME_DOMAIN = "7daystodie"

SPECIAL_MAP = {
    "1hudplus": "1main",
    "helpfulrenames": "zhelpfulrenames",
    "lawntractorv3fix": "lawntractorv3fix",
    "lawntractorpatchguard": "lawntractorpatchguard",
}


def port_open(host: str = "127.0.0.1", port: int = CDP_PORT) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(0.5)
        return s.connect_ex((host, port)) == 0


def clean_key(s: str) -> str:
    s = s.lower()
    s = re.sub(r"^(?:agf-|zzzagf-|zzzzzagf-)?", "", s)
    s = re.sub(r"^(?:v3-)?", "", s)
    s = re.sub(r"^(?:server-|client-|both-|admin-|compat-|vp-|vps-|noeac-|requested-|4modders-|hudplus-)?", "", s)
    s = re.sub(r"^[0-9]+", "", s)
    s = s.replace("-", "").replace("_", "")
    return s


def find_matching_config_entry(base: str, catalog: dict) -> Tuple[Optional[str], Optional[dict]]:
    ck = clean_key(base)
    core_alias = None
    for k_alias, v_alias in SPECIAL_MAP.items():
        if k_alias in base.lower():
            core_alias = v_alias
            break

    for k, v in catalog.items():
        if not isinstance(v, dict):
            continue
        cnk = clean_key(k)
        if ck == cnk or (core_alias and core_alias == cnk):
            return k, v
        if base == "AGF-V3-SERVER-1HUDPlus" and "1main" in k.lower():
            return k, v
    return None, None


def parse_publish_help(path_or_name: str | Path) -> dict:
    if isinstance(path_or_name, str):
        md_file = PUBLISH_HELP / f"{path_or_name}.md"
        if not md_file.is_file():
            candidates = list(PUBLISH_HELP.glob(f"*{path_or_name}*.md"))
            if candidates:
                md_file = candidates[0]
            else:
                raise FileNotFoundError(f"PublishHelp markdown not found for {path_or_name}")
        path = md_file
    else:
        path = path_or_name

    text = path.read_text(encoding="utf-8")
    pattern = re.compile(r"```text\r?\n(.*?)```", re.DOTALL)
    blocks = [m.group(1).strip() for m in pattern.finditer(text)]

    result = {
        "mod_name": blocks[0] if len(blocks) > 0 else "",
        "version": blocks[1] if len(blocks) > 1 else "",
        "short_description": blocks[2] if len(blocks) > 2 else "",
        "full_description_bbcode": blocks[3] if len(blocks) > 3 else "",
        "file_display_name": blocks[4] if len(blocks) > 4 else "",
        "file_version": blocks[5] if len(blocks) > 5 else "",
        "file_description": blocks[6] if len(blocks) > 6 else "",
        "changelog_blocks": blocks[7:] if len(blocks) > 7 else [],
        "total_blocks": len(blocks),
        "source_file": path.name,
    }
    return result


def fill_file_options_description(page, desc_text: str) -> bool:
    """Expand 'File options' and reliably sync description to SCEditor and DOM events."""
    desc_ta = page.locator('textarea[placeholder*="Provide additional information"]')
    if not desc_ta.count() or not desc_ta.first.is_visible():
        fo_btn = page.locator('button:has-text("File options")')
        if fo_btn.count():
            fo_btn.first.click()
            page.wait_for_timeout(500)

    page.evaluate("""(desc) => {
        const textareas = document.querySelectorAll('textarea');
        for (let ta of textareas) {
            const ph = ta.getAttribute('placeholder') || '';
            if (ph.includes('Provide additional information')) {
                if (window.sceditor) {
                    const inst = window.sceditor.instance(ta);
                    if (inst) {
                        inst.val(desc);
                        if (inst.updateOriginal) inst.updateOriginal();
                    }
                }
                ta.value = desc;
                ta.dispatchEvent(new Event('input', { bubbles: true }));
                ta.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }
    }""", desc_text)

    desc_ta = page.locator('textarea[placeholder*="Provide additional information"]')
    for idx in range(desc_ta.count()):
        if desc_ta.nth(idx).is_visible():
            desc_ta.nth(idx).fill(desc_text)
            page.wait_for_timeout(200)

    return True


def read_checklist_items() -> List[dict]:
    """Parse Publish-Checklist.md table into structured items."""
    if not CHECKLIST_PATH.is_file():
        return []

    lines = CHECKLIST_PATH.read_text(encoding="utf-8").splitlines()
    items = []
    for line in lines:
        if not line.startswith("|") or line.startswith("| Mod") or line.startswith("|---"):
            continue
        parts = [p.strip() for p in line.split("|")]
        if len(parts) >= 7:
            items.append({
                "mod": parts[1],
                "ver": parts[2],
                "screenshot": parts[3],
                "beta": parts[4].lower() == "yes",
                "7dtdmods": parts[5],
                "nexus": parts[6],
                "raw_line": line,
            })
    return items


def mark_mod_done_in_checklist(mod_name: str) -> None:
    """Update a specific mod's Nexus status to 'done' in Publish-Checklist.md and refresh summary."""
    if not CHECKLIST_PATH.is_file():
        return

    content = CHECKLIST_PATH.read_text(encoding="utf-8")
    lines = content.splitlines()

    new_lines = []
    updated = False
    for line in lines:
        if line.startswith("|") and not line.startswith("| Mod") and not line.startswith("|---"):
            parts = [p.strip() for p in line.split("|")]
            if len(parts) >= 7 and parts[1] == mod_name:
                parts[6] = "done"
                line = "| " + " | ".join(parts[1:7]) + " |"
                updated = True
        new_lines.append(line)

    if not updated:
        return

    # Recompute summary
    n_done = 0
    n_update = 0
    n_upload = 0
    s_done = 0
    s_update = 0
    s_upload = 0
    total_mods = 0

    for line in new_lines:
        if line.startswith("|") and not line.startswith("| Mod") and not line.startswith("|---"):
            parts = [p.strip() for p in line.split("|")]
            if len(parts) >= 7:
                total_mods += 1
                n_col = parts[6]
                s_col = parts[5]
                if n_col == "done":
                    n_done += 1
                elif "update" in n_col:
                    n_update += 1
                elif "upload" in n_col:
                    n_upload += 1

                if s_col == "done":
                    s_done += 1
                elif "update" in s_col:
                    s_update += 1
                elif "upload" in s_col:
                    s_upload += 1

    summary_re = re.compile(r"^\*\*Summary:\*\*.*$", re.MULTILINE)
    new_summary = (
        f"**Summary:** Total Mods: **{total_mods}** | "
        f"Nexus: **{n_done}** done, **{n_update}** update, **{n_upload}** upload | "
        f"7dtdmods: **{s_done}** done, **{s_update}** update, **{s_upload}** upload"
    )

    final_text = "\n".join(new_lines)
    final_text = summary_re.sub(new_summary, final_text)
    CHECKLIST_PATH.write_text(final_text, encoding="utf-8")
    print(f"  [Checklist] Marked {mod_name} as done in Publish-Checklist.md")


def update_existing_mod(page, mod_name: str, mod_id: int, packet: dict, steps: List[str], force: bool = False) -> bool:
    """Run update steps (general, media, files) on an existing Nexus mod page."""
    print("=" * 60)
    print(f"NEXUS UPDATE: {mod_name} (ID: {mod_id})")
    print("=" * 60)
    print(f"  Target Version: {packet['version']}")
    print(f"  Steps in scope: {', '.join(steps)}")

    # 1. General Tab
    if "general" in steps or "all" in steps:
        print("\n--- [Step 1] Updating General Tab ---")
        url_general = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/general"
        if f"/mods/{mod_id}/edit/general" not in page.url:
            page.goto(url_general, wait_until="domcontentloaded")
        page.wait_for_selector("#mod-name", timeout=15000)
        page.wait_for_timeout(1000)

        page.locator("#mod-name").fill(packet["mod_name"])
        page.locator("#mod-version").fill(packet["version"])
        page.locator("#short-description").fill(packet["short_description"])

        editor_res = page.evaluate("""(bbcode) => {
            const textareas = document.querySelectorAll('textarea');
            for (let i = 0; i < textareas.length; i++) {
                if (window.sceditor) {
                    const inst = window.sceditor.instance(textareas[i]);
                    if (inst) {
                        inst.val('');
                        textareas[i].value = '';
                        if (inst.updateOriginal) inst.updateOriginal();
                        inst.val(bbcode);
                        if (inst.updateOriginal) inst.updateOriginal();
                        return true;
                    }
                }
            }
            return false;
        }""", packet["full_description_bbcode"])
        print(f"  SCEditor BBCode set: {editor_res}")

        save_btn = page.locator('button:has-text("Save")').first
        if save_btn.count():
            save_btn.click()
            page.wait_for_timeout(2500)
            print("  General tab saved.")

    # 2. Media Tab
    if "media" in steps or "all" in steps:
        print("\n--- [Step 2] Updating Media Tab ---")
        url_media = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/media"
        page.goto(url_media, wait_until="domcontentloaded")
        page.wait_for_timeout(2000)

        # Dismiss any lingering overlays
        page.keyboard.press("Escape")
        page.wait_for_timeout(300)

        slot_files = sorted([p for p in IMAGES_DIR.glob(f"{mod_name}_*.png") if not p.name.endswith("_Under2MB.png")])
        if not slot_files:
            clean_name = re.sub(r"-v\d+.*$", "", mod_name)
            slot_files = sorted([p for p in IMAGES_DIR.glob(f"{clean_name}_*.png") if not p.name.endswith("_Under2MB.png")])

        if not slot_files:
            print(f"  [WARN] No slot images found for {mod_name} in {IMAGES_DIR}. Skipping media upload.")
        else:
            file_inputs = page.locator('input[type="file"]')
            if file_inputs.count() >= 2:
                gallery_input = file_inputs.nth(1)

                def get_cards():
                    return page.locator('ul.grid > li, ul[class*="grid"] > li')

                cards_before = get_cards()
                initial_count = cards_before.count()
                old_card_srcs = []
                for i in range(initial_count):
                    c = cards_before.nth(i)
                    src = c.locator("img").get_attribute("src") or ""
                    old_card_srcs.append(src)

                # Upload _01.png
                img01 = slot_files[0]
                print(f"  Uploading {img01.name}...")
                gallery_input.set_input_files(str(img01))

                # Poll for completion
                new_card_index = None
                for _ in range(40):
                    page.wait_for_timeout(1000)
                    cards = get_cards()
                    curr_count = cards.count()
                    if curr_count > initial_count:
                        last_c = cards.last
                        if "Loading" not in last_c.inner_text():
                            print("  Upload complete (not Loading)!")
                            new_card_index = curr_count - 1
                            break

                if new_card_index is not None:
                    cards = get_cards()
                    new_card = cards.nth(new_card_index)
                    if "Thumbnail" not in new_card.inner_text():
                        print("  Setting newly uploaded card as Thumbnail...")
                        menu_btn = new_card.locator('button[aria-haspopup="menu"]')
                        if menu_btn.count():
                            menu_btn.click()
                            page.wait_for_timeout(500)
                            set_thumb = page.locator('[role="menu"] [role="menuitem"]:has-text("Set as thumbnail")')
                            if set_thumb.count() and set_thumb.is_enabled():
                                set_thumb.click()
                                page.wait_for_timeout(1500)
                            else:
                                page.keyboard.press("Escape")

                    # Delete superseded cards
                    if old_card_srcs:
                        print(f"  Deleting {len(old_card_srcs)} old card(s)...")
                        for old_src in old_card_srcs:
                            cards = get_cards()
                            for i in range(cards.count()):
                                c = cards.nth(i)
                                src = c.locator("img").get_attribute("src") or ""
                                if old_src and src == old_src:
                                    menu_btn = c.locator('button[aria-haspopup="menu"]')
                                    if menu_btn.count():
                                        menu_btn.click()
                                        page.wait_for_timeout(500)
                                        del_item = page.locator('[role="menu"] [role="menuitem"]:has-text("Delete")')
                                        if del_item.count():
                                            del_item.click()
                                            page.wait_for_timeout(800)
                                            dialog = page.locator('[role="dialog"]')
                                            if dialog.count() and dialog.first.is_visible():
                                                confirm_btn = dialog.first.locator('button:has-text("Delete")')
                                                if confirm_btn.count():
                                                    confirm_btn.click()
                                                    page.wait_for_timeout(1500)
                                    break

                # Upload subsequent slot images
                if len(slot_files) > 1:
                    for idx, extra_file in enumerate(slot_files[1:], start=2):
                        print(f"  Uploading slot {idx:02d}: {extra_file.name}...")
                        before_count = get_cards().count()
                        gallery_input.set_input_files(str(extra_file))
                        for _ in range(40):
                            page.wait_for_timeout(1000)
                            cards = get_cards()
                            if cards.count() > before_count and "Loading" not in cards.last.inner_text():
                                break
            else:
                print("  [ERROR] Gallery file input not found on Media tab.")

    # 3. Files Tab (via Headless Nexus REST API)
    if "files" in steps or "all" in steps:
        print("\n--- [Step 3] Updating Files Tab (via Headless REST API) ---")
        update_script = WORKFLOW_DIR / "SCRIPT-NexusUpdate.py"
        cmd = [
            sys.executable,
            str(update_script),
            "--mod", mod_name,
        ]
        if force:
            cmd.append("--force")
        print(f"  Running: {' '.join(cmd)}")
        res = subprocess.run(cmd, check=False)
        if res.returncode != 0:
            print(f"  [ERROR] File update via REST API failed with exit code {res.returncode}")
            return False
        print("  File update via REST API completed successfully.")

    print(f"\n[DONE] Update completed for {mod_name}!")
    return True


def upload_new_mod(page, mod_name: str, packet: dict, category: str = "Gameplay") -> Optional[int]:
    """Create draft, stage details, upload media, upload file, and register in config."""
    print("=" * 60)
    print(f"NEXUS NEW MOD UPLOAD: {mod_name}")
    print("=" * 60)
    print(f"  Mod Name: {packet['mod_name']}")
    print(f"  Version: {packet['version']}")
    print(f"  Category: {category}")

    # Step 1: Open draft modal
    print("\n[Step 1] Opening Draft Creation modal...")
    page.keyboard.press("Escape")
    page.wait_for_timeout(300)
    page.keyboard.press("Escape")
    page.wait_for_timeout(300)

    top_upload = page.locator('button').filter(has_text=re.compile(r"^Upload$"))
    if top_upload.count():
        top_upload.first.click()
        page.wait_for_timeout(600)

    upload_mod_btn = page.locator('button:has-text("Upload mod")')
    if upload_mod_btn.count():
        upload_mod_btn.first.click()
        page.wait_for_timeout(1500)

    draft_dialog = page.locator('[role="dialog"]:has-text("Create your draft mod page")').first
    if not draft_dialog.count() or not draft_dialog.is_visible():
        print("  [ERROR] Draft creation dialog not found.")
        return None

    print("  Filling Mod Name and Short Description...")
    draft_dialog.locator('#mod-name').fill(packet["mod_name"])
    draft_dialog.locator('#short-description').fill(packet["short_description"])

    print(f"  Selecting Category: {category}...")
    cat_input = draft_dialog.locator('input[placeholder="Select category"]')
    cat_input.click()
    page.wait_for_timeout(300)
    cat_input.fill(category)
    page.wait_for_timeout(300)
    opt = page.locator(f'[role="option"]:has-text("{category}")').first
    if opt.count() and opt.is_visible():
        opt.click()
    else:
        cat_input.press("Enter")
    page.wait_for_timeout(500)

    submit_btn = draft_dialog.locator('button:has-text("Create draft")')
    if submit_btn.count() and not submit_btn.get_attribute("disabled"):
        print("  Clicking 'Create draft' button...")
        submit_btn.click()
    else:
        print("  [ERROR] 'Create draft' button disabled or missing.")
        return None

    # Wait for new mod ID
    mod_id = None
    for _ in range(30):
        page.wait_for_timeout(1000)
        m = re.search(r"/mods/(\d+)", page.url)
        if m:
            mod_id = int(m.group(1))
            break

    if not mod_id:
        print("  [ERROR] Failed to detect newly created mod ID.")
        return None

    print(f"  Draft created! Nexus Mod ID: {mod_id}")

    # Register in nexusmods-config.json
    if CONFIG_PATH.is_file():
        try:
            with open(CONFIG_PATH, "r", encoding="utf-8") as f:
                cfg_data = json.load(f)
            if "mods" in cfg_data and mod_name not in cfg_data["mods"]:
                cfg_data["mods"][mod_name] = {
                    "intent": "update",
                    "nexus_mod_id": mod_id,
                    "update_group_id": "",
                    "update_group_name": packet.get("file_display_name", mod_name),
                    "file_category": "main",
                    "legacy_latest_file_id": 0,
                    "legacy_name_hint": "",
                    "page_url": f"https://www.nexusmods.com/7daystodie/mods/{mod_id}",
                    "tested_game_version_override": "",
                    "brief_overview_override": "",
                    "detailed_description_override": "",
                    "file_description_override": "",
                    "summary_override": "",
                    "description_override": "",
                }
                with open(CONFIG_PATH, "w", encoding="utf-8") as f:
                    json.dump(cfg_data, f, indent=2)
                print(f"  Registered Mod ID {mod_id} in nexusmods-config.json")
        except Exception as ex:
            print(f"  [WARN] Failed to update config: {ex}")

    # Fill General Tab
    print("\n[Step 2] Staging and saving General Tab...")
    page.wait_for_selector("#mod-name", timeout=15000)
    page.locator("#mod-name").fill(packet["mod_name"])
    page.locator("#mod-version").fill(packet["version"])
    page.locator("#short-description").fill(packet["short_description"])

    page.evaluate("""(bbcode) => {
        const textareas = document.querySelectorAll('textarea');
        for (let i = 0; i < textareas.length; i++) {
            if (window.sceditor) {
                const inst = window.sceditor.instance(textareas[i]);
                if (inst) {
                    inst.val('');
                    textareas[i].value = '';
                    if (inst.updateOriginal) inst.updateOriginal();
                    inst.val(bbcode);
                    if (inst.updateOriginal) inst.updateOriginal();
                }
            }
        }
    }""", packet["full_description_bbcode"])

    save_btn = page.locator('button:has-text("Save")').first
    if save_btn.count():
        save_btn.click()
        page.wait_for_timeout(2500)
        print("  General tab saved.")

    # Media Tab
    print("\n[Step 3] Staging Media Tab...")
    media_url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/media"
    page.goto(media_url, wait_until="domcontentloaded")
    page.wait_for_timeout(2000)

    slot_files = sorted([p for p in IMAGES_DIR.glob(f"{mod_name}_*.png") if not p.name.endswith("_Under2MB.png")])
    if not slot_files:
        clean_name = re.sub(r"-v\d+.*$", "", mod_name)
        slot_files = sorted([p for p in IMAGES_DIR.glob(f"{clean_name}_*.png") if not p.name.endswith("_Under2MB.png")])

    if slot_files:
        gallery_input = page.locator('input[type="file"]').nth(1)
        print(f"  Uploading {slot_files[0].name}...")
        gallery_input.set_input_files(str(slot_files[0]))
        for _ in range(40):
            page.wait_for_timeout(1000)
            cards = page.locator('ul.grid > li, ul[class*="grid"] > li')
            if cards.count() > 0 and "Loading" not in cards.last.inner_text():
                print("  Upload complete (not Loading)!")
                break

        cards = page.locator('ul.grid > li, ul[class*="grid"] > li')
        if cards.count() and "Thumbnail" not in cards.last.inner_text():
            menu_btn = cards.last.locator('button[aria-haspopup="menu"]')
            if menu_btn.count():
                menu_btn.click()
                page.wait_for_timeout(500)
                t_item = page.locator('[role="menu"] [role="menuitem"]:has-text("Set as thumbnail")')
                if t_item.count():
                    t_item.click()
                    page.wait_for_timeout(1000)

    # Files Tab
    print("\n[Step 4] Staging Files Tab...")
    files_url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/files"
    page.goto(files_url, wait_until="domcontentloaded")
    page.wait_for_timeout(2000)

    zip_path = ZIPS_DIR / f"{mod_name}.zip"
    if not zip_path.is_file():
        candidates = list(ZIPS_DIR.glob(f"*{mod_name}*.zip"))
        if candidates:
            zip_path = candidates[0]

    if zip_path.is_file():
        print(f"  Uploading release zip: {zip_path.name}...")
        page.locator('input[type="file"]').first.set_input_files(str(zip_path))
        page.wait_for_selector("#display-name", timeout=25000)
        page.wait_for_timeout(1000)

        page.locator("#display-name").fill(packet["file_display_name"])
        page.locator("#file-version").fill(packet["file_version"])

        print(f"  Filling File Description ({len(packet['file_description'])} chars)...")
        fill_file_options_description(page, packet["file_description"])

        save_file_btn = page.locator('button:has-text("Save file")')
        if save_file_btn.count():
            save_file_btn.click()
            page.wait_for_timeout(3000)
            print("  File saved to release table.")

        # Changelog
        page.wait_for_timeout(1000)
        more_btn = page.locator('button[aria-label="More actions"]').first
        if more_btn.count():
            more_btn.click()
            page.wait_for_timeout(500)
            cl_menu = page.locator('[role="menu"] [role="menuitem"]').filter(has_text=re.compile(r"(Add|Edit) changelog"))
            if cl_menu.count():
                cl_menu.first.click()
                page.wait_for_timeout(1000)
                dialog = page.locator('[role="dialog"]')
                if dialog.count() and dialog.first.is_visible():
                    ver_input = dialog.locator('#changelog-version')
                    if ver_input.count() and ver_input.input_value() != packet["file_version"]:
                        ver_input.fill(packet["file_version"])
                    cl_text = packet["changelog_blocks"][0] if packet["changelog_blocks"] else "Initial release of this mod."
                    dialog.locator('#changelog-text').fill(cl_text)
                    page.wait_for_timeout(500)
                    cl_save = dialog.locator('button:has-text("Save")')
                    if cl_save.count():
                        cl_save.click()
                        page.wait_for_timeout(2000)
                        print("  Changelog entry added and saved.")

    print(f"\n[DONE] New mod staged! Mod ID: {mod_id}")
    return mod_id


def build_parser() -> argparse.ArgumentParser:
    ap = argparse.ArgumentParser(
        description="Nexus Mods Checklist-Driven Batch Automation (Brave Browser CDP)"
    )
    ap.add_argument(
        "--only",
        choices=["all", "update", "upload"],
        default="all",
        help="Action type to process from checklist (default: all)",
    )
    ap.add_argument(
        "--steps",
        default="all",
        help="Steps to run for updates: all, general, media, files (comma-separated, default: all)",
    )
    ap.add_argument(
        "--mod",
        default="",
        help="Target only a single specific mod base name (e.g. AGF-V3-SERVER-2Weekday)",
    )
    ap.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Max number of mods to process (0 = all matching, default: 0)",
    )
    ap.add_argument(
        "--include-beta",
        action="store_true",
        help="Include mods with BETA = yes (skipped by default unless explicitly named)",
    )
    ap.add_argument(
        "--dry-run",
        action="store_true",
        help="Scan and report planned actions without modifying Brave or Nexus",
    )
    ap.add_argument(
        "--force",
        action="store_true",
        help="Force file upload via REST API even if local version matches live version",
    )
    ap.add_argument(
        "--cdp",
        default=DEFAULT_CDP,
        help=f"CDP URL for Brave (default: {DEFAULT_CDP})",
    )
    return ap


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    steps = [s.strip().lower() for s in args.steps.split(",") if s.strip()]
    if not steps:
        steps = ["all"]

    checklist_items = read_checklist_items()
    if not checklist_items:
        print(f"ERROR: Could not read items from {CHECKLIST_PATH}")
        return 1

    # Load catalog config
    config_mods = {}
    if CONFIG_PATH.is_file():
        with open(CONFIG_PATH, "r", encoding="utf-8") as f:
            config_mods = json.load(f).get("mods", {})

    # Filter items
    planned = []
    for item in checklist_items:
        m_name = item["mod"]
        n_stat = item["nexus"]
        is_beta = item["beta"]

        # If user targeted a single mod
        if args.mod and args.mod.lower() != m_name.lower():
            continue

        # Skip non-actionable statuses
        if n_stat in ("done", "skip", "packaged"):
            continue

        # Skip BETA by default unless flag or single mod explicitly passed
        if is_beta and not args.include_beta and not args.mod:
            continue

        action = "update" if "update" in n_stat else ("upload" if "upload" in n_stat else "unknown")
        if action == "unknown":
            continue

        if args.only != "all" and args.only != action:
            continue

        # Resolve Nexus ID
        mod_id = None
        if action == "update":
            _, cfg_entry = find_matching_config_entry(m_name, config_mods)
            if cfg_entry and cfg_entry.get("nexus_mod_id"):
                mod_id = int(cfg_entry["nexus_mod_id"])

        planned.append({
            "mod": m_name,
            "ver": item["ver"],
            "action": action,
            "nexus_status": n_stat,
            "nexus_mod_id": mod_id,
            "beta": is_beta,
        })

    if args.limit > 0:
        planned = planned[:args.limit]

    print("=" * 70)
    print("  NEXUS MODS CHECKLIST SYNC RUNNER")
    print("=" * 70)
    print(f"Checklist file: {CHECKLIST_PATH}")
    print(f"Action filter:  {args.only}")
    print(f"Steps:          {', '.join(steps)}")
    print(f"Include BETA:   {args.include_beta}")
    print(f"Dry Run:        {args.dry_run}")
    print(f"Mods in plan:   {len(planned)}")
    print("=" * 70)

    for idx, p in enumerate(planned, start=1):
        id_str = f"ID: {p['nexus_mod_id']}" if p["nexus_mod_id"] else "New Upload"
        print(f"  {idx:02d}. [{p['action'].upper()}] {p['mod']} (v{p['ver']}) — {id_str}")

    if not planned:
        print("\nNo mods pending synchronization matching current filters.")
        return 0

    if args.dry_run:
        print("\n[DRY RUN] Completed plan scan. No changes made.")
        return 0

    # Verify Brave CDP is open
    if not port_open(port=CDP_PORT):
        print(f"\nERROR: Brave remote debugging port {CDP_PORT} is not open.")
        print("Please run RUN-Nexus-StartBraveDebug.bat and ensure Brave is open and logged in.")
        return 1

    from playwright.sync_api import sync_playwright

    print(f"\nConnecting to Brave on {args.cdp}...")
    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        context = browser.contexts[0]
        page = context.pages[0] if context.pages else context.new_page()

        success_count = 0
        failure_count = 0

        for idx, item in enumerate(planned, start=1):
            m_name = item["mod"]
            action = item["action"]
            mod_id = item["nexus_mod_id"]

            print(f"\n[{idx}/{len(planned)}] Processing {m_name} ({action.upper()})...")

            try:
                packet = parse_publish_help(m_name)
            except Exception as ex:
                print(f"  [ERROR] Failed to load PublishHelp for {m_name}: {ex}")
                failure_count += 1
                continue

            if action == "update":
                if not mod_id:
                    print(f"  [ERROR] No Nexus mod ID resolved for {m_name}. Cannot update.")
                    failure_count += 1
                    continue
                ok = update_existing_mod(page, m_name, mod_id, packet, steps, force=args.force)
                if ok:
                    success_count += 1
                    mark_mod_done_in_checklist(m_name)
                else:
                    failure_count += 1

            elif action == "upload":
                new_id = upload_new_mod(page, m_name, packet)
                if new_id:
                    success_count += 1
                    mark_mod_done_in_checklist(m_name)
                else:
                    failure_count += 1

            # Brief cool-down between mods
            page.wait_for_timeout(2000)

        browser.close()

    print("\n" + "=" * 70)
    print(f"RUN COMPLETE: {success_count} succeeded, {failure_count} failed out of {len(planned)} planned.")
    print("=" * 70)
    return 0 if failure_count == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
