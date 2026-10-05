"""Local Playwright runner for 7DaysToDieMods listings (no Cursor Agent).

Uses **Google Chrome** (not bundled Chromium) and prefers your already-logged-in
Chrome via DevTools CDP (port 9222).

Commands:
  login                         Optional: log in inside a Chrome automation profile
  create <modBase>              Prep + create draft + fill → stop at publish glance
  update <modBase>              Prep + refresh description/images (optional zip/changelog)

Typical use (already logged into Chrome):
  1) Close Chrome, then RUN-7dtdmods-StartChromeDebug.bat
  2) RUN-7dtdmods-Create.bat AGF-VP-CraftStackEngBattCells

Or if Chrome is already running with --remote-debugging-port=9222, just run create/update.

By default never clicks final Publish / agree.
Pass --publish only when the user explicitly asked to publish that run.
Requires: pip install playwright
"""
from __future__ import annotations

import argparse
import json
import os
import re
import socket
import subprocess
import sys
import time
from dataclasses import dataclass
from pathlib import Path

HERE = Path(__file__).resolve().parent
UPLOAD = HERE / "_listing_upload"
PROFILE = HERE / "_playwright_profile"
CONFIG_PATH = HERE / "7dtdmods-config.json"
PREP = HERE / "SCRIPT-PrepListingUpload.py"

SITE = "https://7daystodiemods.com"
API = "https://api.7daystodiemods.com"
GAME_ID = "01KJ17JZEVXHAX9SH1VNMKZFMC"
DEFAULT_CDP = "http://127.0.0.1:9222"
CDP_PORT = 9222


def load_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def save_json(path: Path, data: dict) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def port_open(host: str = "127.0.0.1", port: int = CDP_PORT) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(0.4)
        return s.connect_ex((host, port)) == 0


def find_chrome_exe() -> Path | None:
    candidates = [
        Path(os.environ.get("PROGRAMFILES", r"C:\Program Files"))
        / "Google/Chrome/Application/chrome.exe",
        Path(os.environ.get("PROGRAMFILES(X86)", r"C:\Program Files (x86)"))
        / "Google/Chrome/Application/chrome.exe",
        Path(os.environ.get("LOCALAPPDATA", "")) / "Google/Chrome/Application/chrome.exe",
    ]
    for p in candidates:
        if p.is_file():
            return p
    return None


def chrome_user_data_dir() -> Path:
    return Path(os.environ.get("LOCALAPPDATA", "")) / "Google/Chrome/User Data"


def run_prep(mod: str) -> dict:
    cmd = [sys.executable, str(PREP), mod]
    print("prep:", " ".join(cmd))
    subprocess.check_call(cmd, cwd=str(HERE))
    fill_path = UPLOAD / "fill.json"
    if not fill_path.is_file():
        raise SystemExit("prep did not write fill.json")
    return load_json(fill_path)


def load_fill(mod: str | None, *, do_prep: bool) -> dict:
    if do_prep:
        if not mod:
            raise SystemExit("mod base required for prep")
        return run_prep(mod)
    fill_path = UPLOAD / "fill.json"
    if not fill_path.is_file():
        raise SystemExit(f"Missing {fill_path} — run with prep or pass --prep")
    return load_json(fill_path)


def ensure_playwright():
    try:
        from playwright.sync_api import sync_playwright  # noqa: F401
    except ImportError as e:
        raise SystemExit(
            "Playwright not installed. Run:\n"
            "  python -m pip install playwright"
        ) from e


@dataclass
class BrowserSession:
    """Owns Playwright browser/context. close() only kills browsers we launched."""

    playwright: object
    context: object
    page: object
    mode: str  # "cdp" | "launch"
    browser: object | None = None

    def close(self) -> None:
        if self.mode == "cdp":
            # Leave the user's Chrome running; only disconnect.
            try:
                if self.browser is not None:
                    self.browser.close()
            except Exception:
                pass
            return
        try:
            self.context.close()
        except Exception:
            pass


def try_connect_cdp(playwright, cdp_url: str) -> BrowserSession | None:
    if not port_open():
        return None
    try:
        browser = playwright.chromium.connect_over_cdp(cdp_url)
        context = browser.contexts[0] if browser.contexts else browser.new_context()
        page = context.pages[0] if context.pages else context.new_page()
        print(f"attached to Chrome via CDP {cdp_url}")
        return BrowserSession(
            playwright=playwright,
            context=context,
            page=page,
            mode="cdp",
            browser=browser,
        )
    except Exception as e:
        print(f"CDP connect failed ({e})")
        return None


def start_system_chrome_debug() -> bool:
    """Launch system Chrome with remote debugging using the real user profile."""
    exe = find_chrome_exe()
    if not exe:
        print("Chrome.exe not found")
        return False
    user_data = chrome_user_data_dir()
    if not user_data.is_dir():
        print("Chrome user data dir not found:", user_data)
        return False
    # If Chrome is already running without CDP, this usually fails to bind the profile.
    cmd = [
        str(exe),
        f"--remote-debugging-port={CDP_PORT}",
        "--remote-allow-origins=*",
        f"--user-data-dir={user_data}",
        "--profile-directory=Default",
        "--no-first-run",
        "--no-default-browser-check",
        "--disable-background-mode",
        f"{SITE}/mods",
    ]
    print("starting Chrome with debugging:", " ".join(cmd[:3]), "...")
    subprocess.Popen(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    for _ in range(40):
        if port_open():
            return True
        time.sleep(0.25)
    return False


def open_session(playwright, *, headless: bool, cdp_url: str, use_system_profile: bool) -> BrowserSession:
    # 1) Prefer already-running Chrome with CDP
    sess = try_connect_cdp(playwright, cdp_url)
    if sess:
        return sess

    # 2) Try starting system Chrome (your login) with CDP
    if use_system_profile and not headless:
        if start_system_chrome_debug():
            sess = try_connect_cdp(playwright, cdp_url)
            if sess:
                return sess
        print(
            "Could not attach to your Chrome profile.\n"
            "Close ALL Chrome windows, then run RUN-7dtdmods-StartChromeDebug.bat\n"
            "and retry. Falling back to a separate Chrome automation profile…"
        )

    # 3) Fallback: dedicated Chrome channel profile (needs login once)
    PROFILE.mkdir(parents=True, exist_ok=True)
    print("launching Chrome channel with profile", PROFILE)
    context = playwright.chromium.launch_persistent_context(
        user_data_dir=str(PROFILE),
        channel="chrome",
        headless=headless,
        viewport={"width": 1400, "height": 900},
        accept_downloads=True,
    )
    page = context.pages[0] if context.pages else context.new_page()
    return BrowserSession(
        playwright=playwright,
        context=context,
        page=page,
        mode="launch",
        browser=None,
    )


def wait_logged_in(page, timeout_ms: int = 120_000) -> None:
    page.goto(f"{SITE}/mods", wait_until="domcontentloaded")
    deadline = time.time() + timeout_ms / 1000
    while time.time() < deadline:
        text = page.locator("body").inner_text(timeout=2000)
        if "New Draft" in text or "AuroraGiggleFairy" in text or "Back to drafts" in text:
            print("logged in OK")
            return
        if "Sign in" in text or "Log in" in text:
            time.sleep(1)
            continue
        time.sleep(0.5)
    raise SystemExit(
        "Not logged in (timed out).\n"
        "Close Chrome, run RUN-7dtdmods-StartChromeDebug.bat, log in if needed, retry."
    )


def cmd_login(args: argparse.Namespace) -> None:
    ensure_playwright()
    from playwright.sync_api import sync_playwright

    with sync_playwright() as p:
        sess = open_session(
            p,
            headless=False,
            cdp_url=args.cdp,
            use_system_profile=not args.automation_profile,
        )
        page = sess.page
        page.goto(f"{SITE}/mods", wait_until="domcontentloaded")
        print("Log in in the Chrome window if needed.")
        print("When you see your drafts / New Draft, return here and press Enter.")
        input()
        wait_logged_in(page, timeout_ms=10_000)
        print("OK")
        sess.close()


def cmd_start_chrome_debug(args: argparse.Namespace) -> None:
    if port_open():
        print(f"Chrome CDP already listening on {CDP_PORT}")
        return
    if start_system_chrome_debug():
        print(f"Chrome started with remote debugging on {CDP_PORT}")
        print("Keep this Chrome window open, then run Create/Update.")
        return
    raise SystemExit(
        "Failed to start Chrome with debugging.\n"
        "Fully quit Chrome (tray too), then run this again."
    )


def set_input_value(page, selector: str, value: str) -> None:
    page.locator(selector).first.fill(value)


def set_comments_off(page) -> None:
    switches = page.locator('[role="switch"]')
    for i in range(switches.count()):
        sw = switches.nth(i)
        aria = sw.get_attribute("aria-label") or ""
        parent_text = ""
        try:
            parent_text = sw.evaluate("el => (el.parentElement && el.parentElement.innerText) || ''")
        except Exception:
            pass
        if aria == "Comments" or parent_text.startswith("Comments"):
            if sw.get_attribute("aria-checked") == "true":
                sw.click()
            return


def open_combobox_by_index(page, index: int) -> None:
    """Reka comboboxes: 0=categories, 1=versions, 2=server-side (typical Details order)."""
    page.locator('[role="combobox"]').nth(index).click(timeout=5_000)
    page.wait_for_timeout(120)


def select_combobox_option(page, combo_index: int, option_text: str, *, search: str | None = None) -> None:
    open_combobox_by_index(page, combo_index)
    if search:
        search_box = page.locator(
            'input[placeholder*="Search categories"], input[placeholder*="Search versions"]'
        )
        if search_box.count():
            search_box.first.fill(search)
            page.wait_for_timeout(100)
    page.get_by_role("option", name=option_text, exact=True).first.click()
    page.keyboard.press("Escape")
    page.wait_for_timeout(100)


def select_server_side(page, side: str) -> None:
    # Prefer labeled combo; fall back to Details order index 2
    try:
        page.get_by_role("combobox", name="Server-side").click(timeout=1200)
    except Exception:
        open_combobox_by_index(page, 2)
    page.wait_for_timeout(120)
    page.get_by_role("option", name=side, exact=True).click()
    page.keyboard.press("Escape")
    page.wait_for_timeout(100)


def clear_and_import_description(page, html: str) -> None:
    # Clear existing description (Import appends)
    pm = page.locator(".ProseMirror").first
    if pm.count():
        pm.click()
        page.keyboard.press("Control+a")
        page.keyboard.press("Backspace")
        page.wait_for_timeout(80)

    bb = page.get_by_role("tab", name="BBCode")
    bb.scroll_into_view_if_needed()
    bb.click()
    page.wait_for_timeout(120)

    page.get_by_role("button", name="Import", exact=True).first.click()
    ta = page.locator('[role="dialog"] textarea').first
    ta.wait_for(state="visible", timeout=10_000)
    # insertFromPaste pattern (plain fill can leave Import disabled)
    ta.evaluate(
        """(el, html) => {
          const setter = Object.getOwnPropertyDescriptor(
            HTMLTextAreaElement.prototype, 'value').set;
          setter.call(el, html);
          el.dispatchEvent(new InputEvent('input', {
            bubbles: true, inputType: 'insertFromPaste', data: html
          }));
          el.dispatchEvent(new Event('change', { bubbles: true }));
        }""",
        html,
    )
    confirm = page.locator('[role="dialog"]').get_by_role("button", name="Import", exact=True)
    for _ in range(40):
        if confirm.is_enabled():
            break
        page.wait_for_timeout(80)
    if not confirm.is_enabled():
        raise SystemExit("Import confirm stayed disabled — paste recipe failed")
    confirm.click()
    page.wait_for_timeout(350)

    edit = page.get_by_role("tab", name="Edit")
    if edit.count():
        edit.click()
        page.wait_for_timeout(120)


def fill_details(page, fill: dict, html: str) -> None:
    page.wait_for_selector("#mod-title", timeout=15_000)
    print("  title/summary/comments...")
    set_input_value(page, "#mod-title", fill["title"])
    summary = page.get_by_placeholder("A short description shown in mod listings")
    summary.fill(fill["summary"])
    set_comments_off(page)

    print("  categories...")
    for cat in fill.get("categories") or []:
        # Always open by index — accessible name changes after selection and caused 1.5s×N hangs
        open_combobox_by_index(page, 0)
        search_box = page.locator('input[placeholder*="Search categories"]')
        if search_box.count():
            search_box.first.fill(cat.split()[0][:8])
            page.wait_for_timeout(80)
        opt = page.get_by_role("option", name=cat, exact=True).first
        # Skip re-click if already selected
        aria = opt.get_attribute("aria-selected")
        if aria != "true":
            opt.click()
        page.keyboard.press("Escape")
        page.wait_for_timeout(100)

    print("  game version...")
    select_combobox_option(page, 1, fill["game_version"], search="V3")

    print("  server-side...")
    select_server_side(page, fill["server_side"])

    print("  description Import...")
    clear_and_import_description(page, html)

    # Restore summary/title if wiped by Import/editor quirks
    if not summary.input_value():
        summary.fill(fill["summary"])
    if page.locator("#mod-title").input_value() != fill["title"]:
        set_input_value(page, "#mod-title", fill["title"])
    print("  details done")


def media_image_inputs(page):
    """Image file inputs on Media page: [0]=Thumbnail, [1]=Featured, [2]=Gallery (+ more)."""
    return page.locator('input[type="file"][accept*="image"]')


def wait_cdn_images(page, minimum: int, timeout_ms: int = 20_000) -> None:
    page.wait_for_function(
        """(min) => [...document.querySelectorAll('img')]
            .filter(i => /\\/images\\//.test(i.src)).length >= min""",
        arg=minimum,
        timeout=timeout_ms,
    )


def count_gallery_cdn_images(page) -> int:
    """Visible CDN imgs that follow the Gallery Images heading (not thumb/featured)."""
    return page.evaluate(
        """() => {
          const gh = [...document.querySelectorAll('h3,h4')]
            .find(h => (h.textContent || '').includes('Gallery Images'));
          if (!gh) return 0;
          return [...document.querySelectorAll('img')]
            .filter(i => /\\/images\\//.test(i.src))
            .filter(img => {
              const r = img.getBoundingClientRect();
              if (r.width < 8 || r.height < 8) return false;
              return (gh.compareDocumentPosition(img) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0;
            }).length;
        }"""
    )


def confirm_delete_menu(page) -> bool:
    for name in ("Delete", "Remove"):
        btn = page.get_by_role("menuitem", name=name)
        if btn.count():
            btn.first.click()
            page.wait_for_timeout(250)
            return True
        b = page.get_by_role("button", name=name, exact=True)
        if b.count() and b.last.is_visible():
            b.last.click()
            page.wait_for_timeout(250)
            return True
    return False


def clear_featured_slot(page) -> None:
    """Best-effort remove Featured image cards."""
    page.evaluate(
        """() => {
          const labels = [...document.querySelectorAll('p,span,div,label,h4,h5')];
          const feat = labels.find(el => {
            const t = (el.textContent || '').trim();
            return t === 'Featured (1280x720)' || t.startsWith('Featured (1280');
          });
          if (!feat) return;
          let n = feat.parentElement;
          for (let d = 0; d < 8 && n; d++) {
            const hasImg = [...n.querySelectorAll('img')].some(i => /\\/images\\//.test(i.src));
            if (!hasImg) { n = n.parentElement; continue; }
            const btns = [...n.querySelectorAll('button')].filter(b => !b.textContent.trim());
            if (btns.length) { btns[0].click(); return; }
            n = n.parentElement;
          }
        }"""
    )
    page.wait_for_timeout(300)
    confirm_delete_menu(page)


def clear_gallery_images(page, *, max_rounds: int = 20) -> None:
    """Delete all Gallery Images cards so update can replace instead of append."""
    for _ in range(max_rounds):
        before = count_gallery_cdn_images(page)
        if before == 0:
            print("  gallery cleared (0)")
            return
        opened = page.evaluate(
            """() => {
              const gh = [...document.querySelectorAll('h3,h4')]
                .find(h => (h.textContent || '').includes('Gallery Images'));
              if (!gh) return false;
              const imgs = [...document.querySelectorAll('img')]
                .filter(i => /\\/images\\//.test(i.src))
                .filter(img => {
                  const r = img.getBoundingClientRect();
                  if (r.width < 8 || r.height < 8) return false;
                  return (gh.compareDocumentPosition(img) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0;
                });
              if (!imgs.length) return false;
              const img = imgs[0];
              let n = img.parentElement;
              for (let d = 0; d < 10 && n; d++) {
                const btns = [...n.querySelectorAll('button')].filter(b => !(b.textContent || '').trim());
                if (btns.length) { btns[0].click(); return true; }
                n = n.parentElement;
              }
              return false;
            }"""
        )
        if not opened:
            print(f"  gallery clear: could not open menu (still {before})")
            return
        page.wait_for_timeout(200)
        if not confirm_delete_menu(page):
            print(f"  gallery clear: Delete menu missing (still {before})")
            return
        page.wait_for_timeout(400)
    print(f"  gallery clear stopped with {count_gallery_cdn_images(page)} left")


def fill_media(page, fill: dict) -> None:
    page.locator('input[type="file"]').first.wait_for(state="attached", timeout=15_000)

    # Proven Media slot map (do not use heading walk — Featured can steal):
    #   [0] accept without gif -> Thumbnail
    #   [1] accept+gif, single -> Featured (NEVER)
    #   [2] accept+gif, multiple -> Gallery
    inputs = page.locator('input[type="file"]')
    n = inputs.count()
    print(f"  file inputs: {n}")
    if n < 3:
        raise SystemExit(f"Expected >=3 file inputs on Media, got {n}")

    clear_featured_slot(page)
    print("  clearing existing gallery (replace, not append)...")
    clear_gallery_images(page)

    thumb_path = UPLOAD / fill["thumb"]
    print("  thumb -> input[0] set_input_files")
    inputs.nth(0).set_input_files(str(thumb_path))
    wait_cdn_images(page, 1)

    before = count_gallery_cdn_images(page)
    for gname in fill.get("gallery") or []:
        gpath = UPLOAD / gname
        print(f"  gallery -> input[2] set_input_files ({gname})")
        inputs.nth(2).set_input_files(str(gpath))
        page.wait_for_function(
            """(prev) => {
              const gh = [...document.querySelectorAll('h3,h4')]
                .find(h => (h.textContent || '').includes('Gallery Images'));
              if (!gh) return false;
              const following = [...document.querySelectorAll('img')]
                .filter(i => /\\/images\\//.test(i.src))
                .filter(img => {
                  const r = img.getBoundingClientRect();
                  if (r.width < 8 || r.height < 8) return false;
                  return (gh.compareDocumentPosition(img) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0;
                });
              return following.length > prev;
            }""",
            arg=before,
            timeout=45_000,
        )
        before = count_gallery_cdn_images(page)

    got = count_gallery_cdn_images(page)
    need = len(fill.get("gallery") or [])
    print(f"  gallery CDN images: {got} (need {need})")
    if need and got < need:
        raise SystemExit(f"Gallery still empty after upload (got {got}, need {need}).")
    if need and got > need:
        print(f"  WARNING: gallery has extras ({got} > {need}) — delete extras manually")


def fill_changelog(page, fill: dict) -> None:
    page.get_by_role("button", name="New Entry").click()
    ver = page.get_by_placeholder("2.0.0")
    if ver.count() == 0:
        ver = page.locator("#new-version")
    ver.first.wait_for(state="visible", timeout=10_000)
    ver.first.fill(fill["version"])
    create = page.get_by_role("button", name="Create Entry").last
    page.wait_for_function(
        """() => {
          const btns = [...document.querySelectorAll('button')]
            .filter(b => b.textContent.trim() === 'Create Entry');
          return btns.some(b => !b.disabled);
        }""",
        timeout=10_000,
    )
    create.click()
    page.wait_for_timeout(400)

    # Prefer typing into the empty editor via Playwright keyboard
    empty = page.locator(".ProseMirror.is-empty").first
    if empty.count():
        empty.click()
        page.keyboard.type(fill["changelog_body"], delay=5)
    else:
        page.evaluate(
            """(body) => {
              let editor = [...document.querySelectorAll('.ProseMirror')]
                .find(p => !p.innerText.includes('Mod Scope') && p.innerText.length < 200);
              if (!editor) throw new Error('changelog editor not found');
              editor.focus();
              document.execCommand('selectAll');
              document.execCommand('delete');
              document.execCommand('insertText', false, body);
            }""",
            fill["changelog_body"],
        )


def open_zip_row_editor(page, zip_name: str) -> None:
    """Click the edit (pencil) icon on the existing zip row — never the delete icon."""
    opened = page.evaluate(
        """(zipName) => {
          let zipEl = null;
          const walk = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
          while (walk.nextNode()) {
            if ((walk.currentNode.textContent || '').includes(zipName)) {
              zipEl = walk.currentNode.parentElement;
              break;
            }
          }
          if (!zipEl) return false;
          let n = zipEl;
          for (let d = 0; d < 12 && n; d++) {
            const text = n.innerText || '';
            if (text.includes(zipName) && text.length < 900) {
              const btns = [...n.querySelectorAll('button')].filter(b => !(b.textContent || '').trim());
              // First icon = edit; last often = delete (text-destructive)
              const edit = btns.find(b => !/destructive|danger/i.test(b.className || ''))
                || btns[0];
              if (edit) { edit.click(); return true; }
            }
            n = n.parentElement;
          }
          return false;
        }""",
        zip_name,
    )
    if not opened:
        raise SystemExit("Could not open existing zip row editor (pencil)")
    page.wait_for_timeout(400)


def fill_existing_zip_meta(page, fill: dict) -> None:
    """Open the existing Main Mod row pencil and set Label / Description / Version.

    Does not upload a new zip (update path). Uses Playwright fill, not CDP .value=
    alone — empty saves were a known bug.
    """
    print("  zip meta (existing file)...")
    open_zip_row_editor(page, fill["zip"])

    label = page.get_by_placeholder("Name of your file")
    desc = page.get_by_placeholder("Any special instructions relating to the mod file")
    ver = page.get_by_placeholder("e.g. v2.5")
    label.first.wait_for(state="visible", timeout=10_000)

    label.first.fill(fill["zip_label"])
    desc.first.fill(fill["zip_description"])
    ver.first.fill(fill["zip_version"])

    main = page.get_by_role("button", name="Main Mod", exact=True)
    if main.count():
        main.first.click()

    saves = page.locator("button:text-is('Save')")
    clicked = False
    for i in range(saves.count()):
        btn = saves.nth(i)
        try:
            if btn.is_visible() and btn.is_enabled():
                btn.click()
                clicked = True
                break
        except Exception:
            continue
    if not clicked:
        raise SystemExit("Zip row Save button not found")
    page.wait_for_timeout(500)

    # Re-open and verify full description
    open_zip_row_editor(page, fill["zip"])
    got = desc.first.input_value() if desc.count() else ""
    need = fill["zip_description"]
    if got.strip() != need.strip():
        print("  zip description mismatch — retrying fill")
        label.first.fill(fill["zip_label"])
        desc.first.fill(need)
        ver.first.fill(fill["zip_version"])
        saves = page.locator("button:text-is('Save')")
        for i in range(saves.count()):
            btn = saves.nth(i)
            if btn.is_visible() and btn.is_enabled():
                btn.click()
                break
        page.wait_for_timeout(400)
        open_zip_row_editor(page, fill["zip"])
        got = desc.first.input_value() if desc.count() else ""
    print(f"  zip description lines: {len(got.splitlines())} (need {len(need.splitlines())})")
    if got.strip() != need.strip():
        print("  WARNING: zip description still does not match packet exactly")
        print("  got:", repr(got[:180]))
    else:
        print("  zip meta OK")
    page.keyboard.press("Escape")


def set_permissions_disallow(page) -> None:
    """Show permissions on mod page ON, then Disallow all four rows."""
    print("  permissions -> Show on, Disallow x4...")
    show = page.get_by_role("switch", name="Show permissions on mod page")
    if show.count():
        if show.get_attribute("aria-checked") != "true":
            show.click()
            page.wait_for_timeout(300)
    else:
        page.evaluate(
            """() => {
              const sw = [...document.querySelectorAll('[role=switch]')].find(s => {
                const t = (s.parentElement?.innerText || '') + (s.getAttribute('aria-label') || '');
                return /permission/i.test(t) || /Spell out what others/i.test(t);
              });
              if (sw && sw.getAttribute('aria-checked') !== 'true') sw.click();
            }"""
        )
        page.wait_for_timeout(300)

    for heading in ("Reuploading", "Editing", "Asset Use", "Commercial"):
        ok = page.evaluate(
            """(h) => {
              const el = [...document.querySelectorAll('h3,h4,div,p,span,label')]
                .find(e => (e.textContent||'').trim().startsWith(h));
              if (!el) return 'missing-heading';
              let row = el;
              for (let i = 0; i < 8 && row; i++) {
                const d = [...row.querySelectorAll('button')]
                  .find(b => b.textContent.trim() === 'Disallow');
                if (d) {
                  d.click();
                  // Selected Disallow uses bg-destructive
                  const selected = /bg-destructive/.test(d.className || '');
                  return selected ? 'disallow' : 'clicked';
                }
                row = row.parentElement;
              }
              return 'missing-btn';
            }""",
            heading,
        )
        print(f"    {heading}: {ok}")
        page.wait_for_timeout(80)


def fill_files_and_perms(page, fill: dict) -> None:
    zip_input = page.locator('input[type="file"][accept*="zip"]').first
    if zip_input.count() == 0:
        zip_input = page.locator('input[type="file"]').last
    zip_input.wait_for(state="attached", timeout=15_000)
    zip_input.set_input_files(str(UPLOAD / fill["zip"]))

    page.get_by_text(fill["zip"], exact=False).first.wait_for(state="visible", timeout=30_000)

    page.get_by_placeholder("Name of your file").fill(fill["zip_label"])
    page.get_by_placeholder("Any special instructions relating to the mod file").fill(
        fill["zip_description"]
    )
    page.get_by_placeholder("e.g. v2.5").fill(fill["zip_version"])
    page.get_by_role("button", name="Main Mod", exact=True).click()
    # File-row Save — prefer enabled Save near the zip editor
    page.locator("button:text-is('Save')").nth(1).click()
    page.wait_for_timeout(500)

    # Re-open pencil and confirm non-empty meta
    page.evaluate(
        """() => {
          const upload = [...document.querySelectorAll('button')]
            .find(b => b.textContent.trim()==='Upload File');
          if (!upload) return;
          let n = upload.parentElement;
          for (let i=0;i<8 && n;i++) {
            const cands = [...n.querySelectorAll('button')].filter(b => !b.textContent.trim());
            if (cands.length) { cands[0].click(); return; }
            n = n.parentElement;
          }
        }"""
    )
    page.wait_for_timeout(300)
    label = page.get_by_placeholder("Name of your file")
    if label.count() and label.input_value() != fill["zip_label"]:
        label.fill(fill["zip_label"])
        page.get_by_placeholder("Any special instructions relating to the mod file").fill(
            fill["zip_description"]
        )
        page.get_by_placeholder("e.g. v2.5").fill(fill["zip_version"])
        page.locator("button:text-is('Save')").nth(1).click()
        page.wait_for_timeout(300)

    # Always verify full description (summary + blank + Mod Type)
    desc = page.get_by_placeholder("Any special instructions relating to the mod file")
    if desc.count():
        got = desc.first.input_value()
        if got.strip() != fill["zip_description"].strip():
            desc.first.fill(fill["zip_description"])
            page.locator("button:text-is('Save')").nth(1).click()
            page.wait_for_timeout(300)

    set_permissions_disallow(page)


def fill_credits_on_publish(page, credits: str) -> None:
    page.wait_for_timeout(500)
    page.evaluate(
        """(credits) => {
          const heads = [...document.querySelectorAll('h1,h2,h3,h4,label,legend,div,span,p')];
          const credHead = heads.find(h => h.textContent.trim() === 'Credits');
          if (!credHead) throw new Error('Credits heading not found');
          let editor = null;
          let n = credHead.parentElement;
          for (let d=0; d<8 && n && !editor; d++) {
            const pms = [...n.querySelectorAll('.ProseMirror')];
            editor = pms.find(pm => {
              const t = pm.innerText.trim();
              return t.length < 80 && !t.includes('Download');
            }) || pms[0];
            n = n.parentElement;
          }
          if (!editor) throw new Error('Credits editor not found');
          editor.focus();
          document.execCommand('selectAll');
          document.execCommand('delete');
          document.execCommand('insertText', false, credits);
        }""",
        credits,
    )


def click_final_publish(page) -> None:
    """Agree to terms + Publish. Only call when user explicitly requested publish."""
    print("  agree + Publish...")
    agreed = False
    boxes = page.locator('input[type="checkbox"]')
    if boxes.count() == 0:
        boxes = page.locator('[role="checkbox"]')
    for i in range(boxes.count()):
        box = boxes.nth(i)
        try:
            label = box.evaluate(
                """el => {
                  const id = el.id;
                  if (id) {
                    const lab = document.querySelector('label[for=\"' + id + '\"]');
                    if (lab) return (lab.textContent || '').trim();
                  }
                  const wrap = el.closest('label');
                  if (wrap) return (wrap.innerText || '').trim();
                  return (el.parentElement && el.parentElement.innerText) || '';
                }"""
            )
        except Exception:
            label = ""
        # Publish page: prefer agree/terms; if only one checkbox, use it
        if boxes.count() > 1 and not re.search(r"agree|terms|confirm", label or "", re.I):
            continue
        try:
            if box.get_attribute("role") == "checkbox":
                if box.get_attribute("aria-checked") != "true":
                    box.click()
            else:
                if not box.is_checked():
                    box.check(force=True)
            agreed = True
            break
        except Exception:
            continue
    if not agreed:
        for pat in (re.compile(r"agree", re.I), re.compile(r"terms", re.I)):
            lab = page.get_by_text(pat)
            if lab.count():
                lab.first.click()
                agreed = True
                break
    if not agreed:
        raise SystemExit("Could not find Terms/agree checkbox on publish page")

    page.wait_for_timeout(300)
    pub = page.get_by_role("button", name=re.compile(r"^Publish$", re.I))
    if pub.count() == 0:
        pub = page.get_by_role("button", name=re.compile(r"Publish", re.I))
    if pub.count() == 0:
        raise SystemExit("Publish button not found")
    btn = pub.last
    for _ in range(40):
        if btn.is_enabled():
            break
        page.wait_for_timeout(100)
    if not btn.is_enabled():
        raise SystemExit("Publish button stayed disabled after agree")
    btn.click()
    page.wait_for_timeout(1500)
    print("  Publish clicked")


def api_create_draft(page, title: str) -> tuple[str, str | None]:
    result = page.evaluate(
        """async ({ api, gameId, title }) => {
          const res = await fetch(api + '/v1/mods', {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
            body: JSON.stringify({ game_id: gameId, title })
          });
          const data = await res.json();
          if (!res.ok) throw new Error('create failed ' + res.status + ' ' + JSON.stringify(data));
          return { mod_id: data.mod_id, slug: data.slug || null };
        }""",
        {"api": API, "gameId": GAME_ID, "title": title},
    )
    print("created draft", result["mod_id"], result.get("slug"))
    return result["mod_id"], result.get("slug")


def remember_mod_id(base: str, mod_id: str, slug: str | None = None) -> None:
    cfg = load_json(CONFIG_PATH) if CONFIG_PATH.is_file() else {"mods": {}}
    mods = cfg.setdefault("mods", {})
    entry = mods.setdefault(base, {})
    entry["site_mod_id"] = mod_id
    if slug:
        entry["site_slug"] = slug
    save_json(CONFIG_PATH, cfg)
    print("saved site_mod_id to 7dtdmods-config.json ->", base)


def resolve_mod_id(base: str, override: str | None) -> str:
    if override:
        return override
    cfg = load_json(CONFIG_PATH)
    entry = (cfg.get("mods") or {}).get(base) or {}
    mid = entry.get("site_mod_id")
    if not mid:
        raise SystemExit(
            f"No site_mod_id for {base} in 7dtdmods-config.json.\n"
            "Pass --mod-id … or run create first."
        )
    return mid


def goto_edit(page, mod_id: str, section: str) -> None:
    page.goto(f"{SITE}/mods/{mod_id}/edit/{section}", wait_until="domcontentloaded")
    if section == "details":
        page.wait_for_selector("#mod-title", timeout=20_000)
    elif section in ("media", "files"):
        # Site file inputs are class=hidden — wait attached, not visible
        page.wait_for_selector('input[type="file"]', state="attached", timeout=20_000)
        page.wait_for_timeout(400)
    else:
        page.wait_for_timeout(800)


def clear_upload_scratch() -> None:
    if not UPLOAD.is_dir():
        return
    for p in UPLOAD.iterdir():
        if p.is_file() and p.name not in (
            "SCRIPT-PrepListingUpload.py",
            "SCRIPT-LocalCorsFileServer.py",
            "SCRIPT-7dtdmods-Playwright.py",
        ):
            # only clear listing copies
            if p.suffix.lower() in {".json", ".html", ".png", ".zip", ".jpg", ".jpeg", ".webp"}:
                p.unlink()


def cmd_create(args: argparse.Namespace) -> None:
    ensure_playwright()
    from playwright.sync_api import sync_playwright

    fill = load_fill(args.mod, do_prep=not args.no_prep)
    html = (UPLOAD / "description.html").read_text(encoding="utf-8")
    base = fill.get("base") or args.mod

    with sync_playwright() as p:
        sess = open_session(
            p,
            headless=args.headless,
            cdp_url=args.cdp,
            use_system_profile=not args.automation_profile,
        )
        page = sess.page
        wait_logged_in(page)
        if args.mod_id:
            mod_id = args.mod_id
            print("using existing draft", mod_id)
            remember_mod_id(base, mod_id)
        else:
            mod_id, slug = api_create_draft(page, fill["title"])
            remember_mod_id(base, mod_id, slug)

        goto_edit(page, mod_id, "details")
        print("details...")
        fill_details(page, fill, html)

        goto_edit(page, mod_id, "media")
        print("media...")
        fill_media(page, fill)

        goto_edit(page, mod_id, "changelog")
        print("changelog...")
        fill_changelog(page, fill)

        goto_edit(page, mod_id, "files")
        print("files...")
        fill_files_and_perms(page, fill)

        goto_edit(page, mod_id, "publish")
        print("publish glance...")
        fill_credits_on_publish(page, fill.get("credits") or "AuroraGiggleFairy")
        page.wait_for_timeout(500)

        url = f"{SITE}/mods/{mod_id}/edit/publish"
        live = f"{SITE}/mods/{mod_id}"
        if getattr(args, "publish", False):
            click_final_publish(page)
            print()
            print("PUBLISHED - double-check live page:")
            print(" ", live)
            print(" ", url)
        else:
            print()
            print("DONE - review publish page (do not auto-Publish):")
            print(" ", url)
        if args.keep_open:
            print("Press Enter here when finished (Chrome stays open if attached via CDP).")
            try:
                input()
            except EOFError:
                page.wait_for_timeout(60_000)
        clear_upload_scratch()
        sess.close()


def cmd_update(args: argparse.Namespace) -> None:
    ensure_playwright()
    from playwright.sync_api import sync_playwright

    fill = load_fill(args.mod, do_prep=not args.no_prep)
    html = (UPLOAD / "description.html").read_text(encoding="utf-8")
    base = fill.get("base") or args.mod
    mod_id = resolve_mod_id(base, args.mod_id)
    if args.mod_id:
        remember_mod_id(base, mod_id)

    with sync_playwright() as p:
        sess = open_session(
            p,
            headless=args.headless,
            cdp_url=args.cdp,
            use_system_profile=not args.automation_profile,
        )
        page = sess.page
        wait_logged_in(page)

        if getattr(args, "files_only", False):
            # Skip details/media; files block below handles zip meta + perms
            pass
        else:
            if not args.images_only:
                goto_edit(page, mod_id, "details")
                print("update details/description...")
                page.wait_for_selector("#mod-title", timeout=15_000)
                set_input_value(page, "#mod-title", fill["title"])
                page.get_by_placeholder("A short description shown in mod listings").fill(fill["summary"])
                set_comments_off(page)
                clear_and_import_description(page, html)

            if not args.desc_only:
                goto_edit(page, mod_id, "media")
                print("update media...")
                fill_media(page, fill)

        if args.changelog:
            goto_edit(page, mod_id, "changelog")
            print("add changelog...")
            fill_changelog(page, fill)

        # Files: default update refreshes zip Label/Description/Version + Disallow perms.
        # --zip also uploads a new zip. --files-only skips desc/media.
        do_files = (
            args.zip
            or getattr(args, "files_only", False)
            or (
                not args.desc_only
                and not args.images_only
                and not getattr(args, "no_files", False)
            )
        )
        if do_files:
            goto_edit(page, mod_id, "files")
            if args.zip:
                print("upload zip + meta/perms...")
                fill_files_and_perms(page, fill)
            else:
                print("update zip meta + permissions...")
                fill_existing_zip_meta(page, fill)
                set_permissions_disallow(page)

        goto_edit(page, mod_id, "publish")
        fill_credits_on_publish(page, fill.get("credits") or "AuroraGiggleFairy")
        url = f"{SITE}/mods/{mod_id}/edit/publish"
        print()
        print("DONE - review:", url)
        if args.keep_open:
            print("Press Enter here when finished.")
            try:
                input()
            except EOFError:
                page.wait_for_timeout(60_000)
        clear_upload_scratch()
        sess.close()


def add_browser_args(p: argparse.ArgumentParser) -> None:
    p.add_argument(
        "--cdp",
        default=DEFAULT_CDP,
        help=f"Chrome DevTools URL (default {DEFAULT_CDP})",
    )
    p.add_argument(
        "--automation-profile",
        action="store_true",
        help="Use Workflow/_playwright_profile instead of your real Chrome login",
    )


def build_parser() -> argparse.ArgumentParser:
    ap = argparse.ArgumentParser(description="7dtdmods Playwright listing helper (Chrome)")
    sub = ap.add_subparsers(dest="cmd", required=True)

    p_chrome = sub.add_parser(
        "start-chrome-debug",
        help="Start system Chrome with --remote-debugging-port=9222 (close Chrome first)",
    )
    p_chrome.set_defaults(func=cmd_start_chrome_debug)

    p_login = sub.add_parser("login", help="Confirm login in Chrome")
    add_browser_args(p_login)
    p_login.set_defaults(func=cmd_login)

    p_create = sub.add_parser("create", help="Create + fill draft; stop at publish unless --publish")
    p_create.add_argument("mod", help="Mod base, e.g. AGF-VP-FuelBurnPlus")
    p_create.add_argument("--mod-id", help="Fill an existing draft instead of POST /v1/mods")
    p_create.add_argument("--no-prep", action="store_true", help="Reuse existing _listing_upload")
    p_create.add_argument("--headless", action="store_true")
    p_create.add_argument("--no-keep-open", dest="keep_open", action="store_false", default=True)
    p_create.add_argument(
        "--publish",
        action="store_true",
        help="After fill: check Terms and click Publish (only when user asked)",
    )
    add_browser_args(p_create)
    p_create.set_defaults(func=cmd_create)

    p_update = sub.add_parser("update", help="Update description/images/zip-meta/perms")
    p_update.add_argument("mod", help="Mod base")
    p_update.add_argument("--mod-id", help="Override site_mod_id")
    p_update.add_argument("--no-prep", action="store_true")
    p_update.add_argument("--headless", action="store_true")
    p_update.add_argument("--desc-only", action="store_true", help="Skip media and files")
    p_update.add_argument("--images-only", action="store_true", help="Skip description and files")
    p_update.add_argument(
        "--files-only",
        action="store_true",
        help="Only refresh zip Label/Description/Version + permissions",
    )
    p_update.add_argument(
        "--no-files",
        action="store_true",
        help="Skip zip meta/permissions on a full update",
    )
    p_update.add_argument("--changelog", action="store_true", help="Also add changelog entry")
    p_update.add_argument("--zip", action="store_true", help="Upload a new zip then set meta/perms")
    p_update.add_argument("--no-keep-open", dest="keep_open", action="store_false", default=True)
    add_browser_args(p_update)
    p_update.set_defaults(func=cmd_update)

    p_pub = sub.add_parser(
        "publish",
        help="Agree + Publish an already-filled draft (only when user asked)",
    )
    p_pub.add_argument("mod", help="Mod base")
    p_pub.add_argument("--mod-id", help="Override site_mod_id")
    p_pub.add_argument("--headless", action="store_true")
    p_pub.add_argument("--no-keep-open", dest="keep_open", action="store_false", default=True)
    add_browser_args(p_pub)
    p_pub.set_defaults(func=cmd_publish)

    return ap


def cmd_publish(args: argparse.Namespace) -> None:
    ensure_playwright()
    from playwright.sync_api import sync_playwright

    base = args.mod
    mod_id = resolve_mod_id(base, args.mod_id)
    with sync_playwright() as p:
        sess = open_session(
            p,
            headless=args.headless,
            cdp_url=args.cdp,
            use_system_profile=not args.automation_profile,
        )
        page = sess.page
        wait_logged_in(page)
        goto_edit(page, mod_id, "publish")
        print("publish...")
        click_final_publish(page)
        live = f"{SITE}/mods/{mod_id}"
        print()
        print("PUBLISHED - double-check live page:")
        print(" ", live)
        if args.keep_open:
            try:
                input()
            except EOFError:
                page.wait_for_timeout(30_000)
        sess.close()


def main() -> None:
    ap = build_parser()
    args = ap.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
