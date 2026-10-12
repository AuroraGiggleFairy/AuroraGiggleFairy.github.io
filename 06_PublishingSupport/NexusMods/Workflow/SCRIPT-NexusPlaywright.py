"""Nexus Mods Playwright Automation Helper (Brave Browser).

Connects to Brave via DevTools CDP (--remote-debugging-port=9222).
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
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO_ROOT = HERE.parents[2]
NEXUS_ROOT = HERE.parent
PUBLISH_HELP = NEXUS_ROOT / "PublishHelp"

DEFAULT_CDP = "http://127.0.0.1:9222"
CDP_PORT = 9222
SITE = "https://www.nexusmods.com"
GAME_DOMAIN = "7daystodie"

BRAVE_CANDIDATES = [
    Path(os.environ.get("PROGRAMFILES", r"C:\Program Files"))
    / "BraveSoftware/Brave-Browser/Application/brave.exe",
    Path(os.environ.get("PROGRAMFILES(X86)", r"C:\Program Files (x86)"))
    / "BraveSoftware/Brave-Browser/Application/brave.exe",
    Path(os.environ.get("LOCALAPPDATA", ""))
    / "BraveSoftware/Brave-Browser/Application/brave.exe",
]


def find_brave_exe() -> Path | None:
    for p in BRAVE_CANDIDATES:
        if p.is_file():
            return p
    return None


def brave_user_data_dir() -> Path:
    return Path(os.environ.get("LOCALAPPDATA", "")) / "BraveSoftware/Brave-Browser/User Data"


def port_open(host: str = "127.0.0.1", port: int = CDP_PORT) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(0.4)
        return s.connect_ex((host, port)) == 0


def get_nexus_page(browser):
    context = browser.contexts[0]
    for p in context.pages:
        if "nexusmods.com" in p.url:
            p.bring_to_front()
            return p
    for p in context.pages:
        if p.url == "about:blank":
            p.goto(f"{SITE}/games/{GAME_DOMAIN}", wait_until="domcontentloaded")
            p.wait_for_timeout(2000)
            p.bring_to_front()
            return p
    p = context.new_page()
    p.goto(f"{SITE}/games/{GAME_DOMAIN}", wait_until="domcontentloaded")
    p.wait_for_timeout(2000)
    p.bring_to_front()
    return p


def start_system_brave_debug(target_url: str = f"{SITE}/7daystodie") -> bool:
    exe = find_brave_exe()
    if not exe:
        print(f"ERROR: brave.exe not found in candidates: {[str(c) for c in BRAVE_CANDIDATES]}")
        return False
    user_data = brave_user_data_dir()
    if not user_data.is_dir():
        print(f"ERROR: Brave user data dir not found: {user_data}")
        return False

    cmd = [
        str(exe),
        f"--remote-debugging-port={CDP_PORT}",
        "--remote-allow-origins=*",
        f"--user-data-dir={user_data}",
        "--profile-directory=Default",
        "--no-first-run",
        "--no-default-browser-check",
        "--disable-background-mode",
        "--start-minimized",
        target_url,
    ]
    print(f"Starting Brave with debugging on port {CDP_PORT}...")
    startupinfo = None
    if sys.platform == "win32":
        startupinfo = subprocess.STARTUPINFO()
        startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startupinfo.wShowWindow = 7  # SW_SHOWMINNOACTIVE: minimized, does not steal window focus
    subprocess.Popen(
        cmd,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
        startupinfo=startupinfo,
    )
    for _ in range(40):
        if port_open(port=CDP_PORT):
            return True
        time.sleep(0.25)
    return False


def cmd_start_brave_debug(args: argparse.Namespace) -> None:
    if port_open(port=CDP_PORT):
        print(f"Brave CDP is already listening on port {CDP_PORT}.")
        return
    if start_system_brave_debug():
        print(f"Brave started successfully with remote debugging on port {CDP_PORT}.")
        print("Keep Brave open, log into Nexus if needed (via 1Password), then proceed.")
        return
    raise SystemExit(
        "Failed to start Brave with debugging port 9222.\n"
        "Ensure all Brave windows and background processes are completely closed, then retry."
    )


def cmd_check_connection(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    from playwright.sync_api import sync_playwright

    print(f"Connecting to Brave on {args.cdp}...")
    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        contexts = browser.contexts
        print(f"Connected! Found {len(contexts)} browser context(s).")
        pages = []
        for ctx in contexts:
            pages.extend(ctx.pages)
        print(f"Open tabs ({len(pages)}):")
        for i, page in enumerate(pages):
            try:
                print(f"  [{i}] {page.title()} -> {page.url}")
            except Exception:
                print(f"  [{i}] <failed to read title> -> {page.url}")

        # Check Nexus tab
        nexus_page = None
        for p in pages:
            if "nexusmods.com" in p.url:
                nexus_page = p
                break

        if nexus_page:
            print("\nChecking Nexus login status on tab:", nexus_page.url)
            # Evaluate login state
            login_info = nexus_page.evaluate("""() => {
                const loginBtn = document.querySelector('a[href*="/login"], a[href*="users.nexusmods.com"], button[aria-label*="Log in"]');
                const userLink = document.querySelector('a[href*="/users/"]');
                const avatar = document.querySelector('img[alt*="avatar" i], .avatar, .user-avatar');
                const navLinks = Array.from(document.querySelectorAll('nav a, header a')).map(a => ({
                    text: a.innerText.trim(),
                    href: a.getAttribute('href')
                })).filter(a => a.text);
                return {
                    url: window.location.href,
                    title: document.title,
                    hasLoginBtn: !!loginBtn,
                    userHref: userLink ? userLink.getAttribute('href') : null,
                    userText: userLink ? userLink.innerText.trim() : null,
                    avatarFound: !!avatar,
                    navCount: navLinks.length,
                    sampleNav: navLinks.slice(0, 10)
                };
            }""")
            print(f"  Page URL: {login_info.get('url')}")
            print(f"  User Link: {login_info.get('userHref')} ({login_info.get('userText')})")
            print(f"  Has Login Button: {login_info.get('hasLoginBtn')}")
            print(f"  Nav Links found: {login_info.get('navCount')}")
        browser.close()
    print("Connection check SUCCESSFUL.")


def cmd_inspect_mod(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    from playwright.sync_api import sync_playwright

    mod_id = args.mod_id
    edit_tab = args.tab
    if edit_tab:
        url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/{edit_tab}"
    else:
        url = f"{SITE}/{GAME_DOMAIN}/mods/{mod_id}"
    print(f"Connecting to Brave and inspecting: {url}")
    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        page.goto(url, wait_until="domcontentloaded")
        page.wait_for_timeout(2500)
        print("Page Title:", page.title())
        print("Final URL:", page.url)

        # Inspect form fields, inputs, textareas, editors
        form_info = page.evaluate("""() => {
            const inputs = Array.from(document.querySelectorAll('input, textarea, select, [contenteditable="true"]')).map(el => {
                return {
                    tag: el.tagName,
                    type: el.getAttribute('type') || '',
                    name: el.getAttribute('name') || '',
                    id: el.getAttribute('id') || '',
                    placeholder: el.getAttribute('placeholder') || '',
                    ariaLabel: el.getAttribute('aria-label') || '',
                    className: el.className || '',
                    value: (el.value || el.innerText || '').slice(0, 100),
                    offsetParent: !!el.offsetParent
                };
            });
            const buttons = Array.from(document.querySelectorAll('button, input[type="submit"]')).map(b => ({
                text: (b.innerText || b.value || '').trim(),
                type: b.getAttribute('type') || '',
                id: b.getAttribute('id') || '',
                className: b.className || ''
            })).filter(b => b.text);
            return { inputs, buttons };
        }""")
        print(f"\nForm inputs found ({len(form_info['inputs'])}):")
        for i, inp in enumerate(form_info["inputs"][:25]):
            safe_val = inp['value'].encode('ascii', errors='replace').decode('ascii')
            print(f"  [{i}] <{inp['tag']}> id={inp['id']} name={inp['name']} type={inp['type']} visible={inp['offsetParent']} val={repr(safe_val)}")
        print(f"\nButtons found ({len(form_info['buttons'])}):")
        for i, b in enumerate(form_info["buttons"][:25]):
            safe_text = b['text'].encode('ascii', errors='replace').decode('ascii')
            print(f"  [{i}] <BUTTON> text={repr(safe_text)} id={b['id']}")

        # Inspect SCEditor and Import description button
        sceditor_info = page.evaluate("""() => {
            const hasSceditorGlobal = typeof window.sceditor !== 'undefined';
            const hasJQuery = typeof window.$ !== 'undefined' || typeof window.jQuery !== 'undefined';
            
            // Check textareas associated with sceditor
            const textareas = Array.from(document.querySelectorAll('textarea')).map(t => ({
                id: t.id,
                className: t.className,
                hasVal: !!t.value,
                valLength: (t.value || '').length
            }));
            
            // Check editor mode buttons (SCEditor has a source mode button, usually icon or title 'View source')
            const sceditorButtons = Array.from(document.querySelectorAll('.sceditor-toolbar a, .sceditor-toolbar button')).map(b => ({
                title: b.getAttribute('title') || b.getAttribute('data-sceditor-command') || b.innerText || '',
                command: b.getAttribute('data-sceditor-command') || ''
            }));
            
            return { hasSceditorGlobal, hasJQuery, textareas, sceditorButtons };
        }""")
        print("\nSCEditor Info:")
        print("  Global sceditor:", sceditor_info["hasSceditorGlobal"])
        print("  Global jQuery:", sceditor_info["hasJQuery"])
        print("  Textareas:", sceditor_info["textareas"])
        print("  SCEditor Toolbar Buttons:", sceditor_info["sceditorButtons"][:15])

        browser.close()

        browser.close()


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
    }
    return result


def cmd_parse(args: argparse.Namespace) -> None:
    mod_name = args.mod
    md_file = PUBLISH_HELP / f"{mod_name}.md"
    if not md_file.is_file():
        # Try finding case-insensitively or with suffix
        candidates = list(PUBLISH_HELP.glob(f"*{mod_name}*.md"))
        if candidates:
            md_file = candidates[0]
        else:
            print(f"ERROR: Could not find PublishHelp file for {mod_name} in {PUBLISH_HELP}")
            sys.exit(1)

    print(f"Parsing PublishHelp: {md_file.name}")
    data = parse_publish_help(md_file)
    print(f"  Mod Name: {data['mod_name']}")
    print(f"  Version: {data['version']}")
    print(f"  Short Description ({len(data['short_description'])} chars): {data['short_description']}")
    print(f"  Full BBCode length: {len(data['full_description_bbcode'])} chars")
    print(f"  File Display Name: {data['file_display_name']}")
    print(f"  File Version: {data['file_version']}")
    print(f"  File Description ({len(data['file_description'])} chars): {data['file_description'][:60]}...")
    print(f"  Changelog block count: {len(data['changelog_blocks'])}")


def cmd_fill_general(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_name = args.mod
    md_file = PUBLISH_HELP / f"{mod_name}.md"
    if not md_file.is_file():
        candidates = list(PUBLISH_HELP.glob(f"*{mod_name}*.md"))
        if candidates:
            md_file = candidates[0]
        else:
            print(f"ERROR: Could not find PublishHelp file for {mod_name} in {PUBLISH_HELP}")
            sys.exit(1)

    packet = parse_publish_help(md_file)
    mod_id = args.mod_id or "11221"

    url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/general"
    print(f"Connecting to Brave on {args.cdp}...")
    print(f"Navigating to {url}...")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        if f"/mods/{mod_id}/edit/general" not in page.url:
            page.goto(url, wait_until="domcontentloaded")
        page.wait_for_selector("#mod-name", timeout=15000)
        page.wait_for_timeout(1000)

        # 1. Fill Mod Name
        print(f"Filling Mod Name: {packet['mod_name']!r}")
        page.locator("#mod-name").fill(packet["mod_name"])

        # 2. Fill Mod Version
        print(f"Filling Mod Version: {packet['version']!r}")
        page.locator("#mod-version").fill(packet["version"])

        # 3. Fill Short Description
        print(f"Filling Short Description ({len(packet['short_description'])} chars)...")
        page.locator("#short-description").fill(packet["short_description"])

        # 4. Fill Full Description via SCEditor (explicit full clear, then populate)
        print(f"Filling Full BBCode Description ({len(packet['full_description_bbcode'])} chars) via SCEditor...")
        editor_res = page.evaluate("""(bbcode) => {
            const textareas = document.querySelectorAll('textarea');
            for (let i = 0; i < textareas.length; i++) {
                if (window.sceditor) {
                    const inst = window.sceditor.instance(textareas[i]);
                    if (inst) {
                        // 1. Explicitly clear both editor and underlying textarea
                        inst.val('');
                        textareas[i].value = '';
                        if (inst.updateOriginal) {
                            inst.updateOriginal();
                        }
                        // 2. Set new content
                        inst.val(bbcode);
                        if (inst.updateOriginal) {
                            inst.updateOriginal();
                        }
                        return {
                            success: true,
                            valLength: (inst.val() || '').length,
                            textareaLength: (textareas[i].value || '').length
                        };
                    }
                }
            }
            return { success: false, reason: 'SCEditor instance not found' };
        }""", packet["full_description_bbcode"])

        print(f"SCEditor update result: {editor_res}")

        # Check values currently in DOM to verify
        current_vals = page.evaluate("""() => {
            const nameEl = document.querySelector('#mod-name');
            const verEl = document.querySelector('#mod-version');
            const shortEl = document.querySelector('#short-description');
            return {
                name: nameEl ? nameEl.value : null,
                version: verEl ? verEl.value : null,
                shortDesc: shortEl ? shortEl.value : null
            };
        }""")
        print("\nVerification of filled DOM values:")
        print(f"  Mod Name: {current_vals['name']!r}")
        print(f"  Mod Version: {current_vals['version']!r}")
        print(f"  Short Description: {current_vals['shortDesc']!r}")

        # If --save flag is provided or requested, click Save
        if getattr(args, "save", False):
            save_btn = page.locator('button:has-text("Save")').first
            if save_btn.count():
                print("\nClicking 'Save' on General tab...")
                save_btn.click()
                page.wait_for_timeout(2500)
                print("Saved General tab!")
        else:
            print("\n[NOTE] As requested: Did NOT click 'Save'. The form is filled in your Brave window.")
        browser.close()


def cmd_inspect_media(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_id = args.mod_id or "11221"
    url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/media"
    print(f"Connecting to Brave on {args.cdp}...")
    print(f"Navigating to {url}...")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        page.goto(url, wait_until="domcontentloaded")
        page.wait_for_timeout(2500)

        # Expand accordions if collapsed
        for btn_text in ["Your thumbnail", "Images and header"]:
            btn = page.locator(f"button:has-text('{btn_text}')").first
            if btn.count() and btn.get_attribute("aria-expanded") == "false":
                btn.click()
                page.wait_for_timeout(600)

        data = page.evaluate("""() => {
            const fileInputs = Array.from(document.querySelectorAll('input[type="file"]')).map((el, i) => {
                let contextText = '';
                let node = el.parentElement;
                while (node && node !== document.body && contextText.length < 150) {
                    const text = (node.innerText || '').trim().replace(/\\s+/g, ' ');
                    if (text && text.length > contextText.length) {
                        contextText = text;
                    }
                    node = node.parentElement;
                }
                return {
                    index: i,
                    id: el.id,
                    accept: el.accept,
                    multiple: el.multiple,
                    contextText: contextText.slice(0, 120)
                };
            });

            // Find all buttons containing 'Your thumbnail' or 'Images and header'
            const disclosures = Array.from(document.querySelectorAll('button')).filter(b => {
                const t = b.innerText || '';
                return t.includes('Your thumbnail') || t.includes('Images and header');
            }).map(b => {
                const title = b.innerText.trim();
                // The panel content is usually the sibling or controlled by aria-controls
                const panelId = b.getAttribute('aria-controls');
                const panel = panelId ? document.getElementById(panelId) : b.nextElementSibling;
                const panelText = panel ? panel.innerText.trim().replace(/\\n+/g, ' ').slice(0, 500) : '<no panel>';
                const allElements = panel ? Array.from(panel.querySelectorAll('*')).map(el => ({
                    tag: el.tagName,
                    id: el.id,
                    className: el.className ? el.className.slice(0, 40) : '',
                    text: (el.innerText || '').slice(0, 30)
                })) : [];
                const imgs = panel ? Array.from(panel.querySelectorAll('img')).map(img => ({
                    src: img.src.slice(0, 120),
                    alt: img.alt || '',
                    outer: img.outerHTML.slice(0, 150)
                })) : [];
                return { title, panelId, panelText, elementCount: allElements.length, imgs };
            });

            // Find where staticdelivery images are in DOM
            const staticImgs = Array.from(document.querySelectorAll('img')).filter(img => {
                return img.src.includes('staticdelivery') || img.src.includes('11221');
            }).map(img => {
                const parents = [];
                let curr = img.parentElement;
                while (curr && curr !== document.body && parents.length < 5) {
                    parents.push({ tag: curr.tagName, id: curr.id, className: curr.className, text: (curr.innerText || '').slice(0, 40) });
                    curr = curr.parentElement;
                }
                const card = img.closest('div[class*="relative"], div[class*="card"], li, div[class*="group"], figure') || img.parentElement;
                const btns = card ? Array.from(card.querySelectorAll('button, a')).map(b => ({
                    text: (b.innerText || '').trim(),
                    ariaLabel: b.getAttribute('aria-label') || '',
                    title: b.getAttribute('title') || '',
                    className: b.className
                })) : [];
                return {
                    src: img.src,
                    alt: img.alt,
                    parents,
                    cardText: card ? (card.innerText || '').replace(/\\n+/g, ' ').slice(0, 100) : '',
                    cardButtons: btns
                };
            });

            // Inspect the card LI in detail
            const galleryCards = Array.from(document.querySelectorAll('ul li')).filter(li => {
                return (li.innerText || '').includes('AuroraGiggleFairy') || li.querySelector('img');
            }).map(li => {
                const img = li.querySelector('img');
                const btns = Array.from(li.querySelectorAll('button')).map(b => ({
                    text: b.innerText.trim(),
                    ariaLabel: b.getAttribute('aria-label') || '',
                    title: b.getAttribute('title') || '',
                    svg: b.querySelector('svg') ? b.querySelector('svg').outerHTML.slice(0, 100) : '',
                    className: b.className
                }));
                const spans = Array.from(li.querySelectorAll('span, p')).map(s => s.innerText.trim()).filter(Boolean);
                return {
                    imgSrc: img ? img.src : '',
                    alt: img ? img.alt : '',
                    fullText: li.innerText.replace(/\\n+/g, ' | '),
                    buttons: btns,
                    spans
                };
            });

            return { fileInputs, disclosures, staticImgs, galleryCards };
        }""")

        print(f"\nFile inputs found ({len(data['fileInputs'])}):")
        for fi in data["fileInputs"]:
            print(f"  [{fi['index']}] accept={fi['accept']} multiple={fi['multiple']}")
            print(f"       context={repr(fi['contextText'])}")

        print(f"\nGallery Cards in UL LI ({len(data['galleryCards'])}):")
        for gc in data["galleryCards"]:
            print(f"  Img: {gc['imgSrc']}")
            print(f"    Text: {gc['fullText']!r}")
            print(f"    Spans: {gc['spans']}")
            print(f"    Buttons: ({len(gc['buttons'])})")
            for b in gc["buttons"]:
                print(f"      text={b['text']!r} aria={b['ariaLabel']!r} class={b['className'][:50]!r}")
                print(f"      svg: {b['svg']}")

        # Click the 3-dots kebab menu button using Playwright locator
        page.wait_for_timeout(500)
        card = page.locator('ul li').first
        if card.count():
            menu_btn = card.locator('button[aria-haspopup="menu"]')
            if menu_btn.count():
                print(f"Found menu button (id={menu_btn.get_attribute('id')}). Clicking...")
                menu_btn.click()
                page.wait_for_timeout(800)

                # Look for open menu items
                menu_items = page.locator('[role="menu"] [role="menuitem"], [role="menu"] button, [role="menu"] a')
                print(f"Menu items count: {menu_items.count()}")
                for i in range(menu_items.count()):
                    item = menu_items.nth(i)
                    print(f"  Item {i}: text={item.inner_text()!r} role={item.get_attribute('role')!r}")

        browser.close()


def cmd_stage_media(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_name = args.mod
    mod_id = args.mod_id or "11221"
    images_dir = REPO_ROOT / "00_Images" / "02_ImagesFinal"

    # Find slot files: _01.png, _02.png, ...
    slot_files = sorted([
        p for p in images_dir.glob(f"{mod_name}_*.png")
        if not p.name.endswith("_Under2MB.png")
    ])

    if not slot_files:
        # Try matching by suffix or removing version
        clean_name = re.sub(r"-v\d+.*$", "", mod_name)
        slot_files = sorted([
            p for p in images_dir.glob(f"{clean_name}_*.png")
            if not p.name.endswith("_Under2MB.png")
        ])

    print(f"Mod: {mod_name} (ID: {mod_id})")
    print(f"Found {len(slot_files)} local image(s) to stage:")
    for f in slot_files:
        print(f"  {f.name} ({f.stat().st_size:,} bytes)")

    if not slot_files:
        print("ERROR: No images found to upload.")
        sys.exit(1)

    url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/media"
    print(f"\nConnecting to Brave and navigating to {url}...")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        if f"/mods/{mod_id}/edit/media" not in page.url:
            page.goto(url, wait_until="domcontentloaded")
            page.wait_for_timeout(2000)

        # Dismiss any open dialog/overlay
        cancel_btn = page.locator('button:has-text("Cancel")')
        if cancel_btn.count() and cancel_btn.first.is_visible():
            cancel_btn.first.click()
            page.wait_for_timeout(500)
        page.keyboard.press("Escape")
        page.wait_for_timeout(300)

        # Ensure gallery file input is present
        file_inputs = page.locator('input[type="file"]')
        if file_inputs.count() < 2:
            print("ERROR: Gallery file input (index 1) not found!")
            browser.close()
            sys.exit(1)
        gallery_input = file_inputs.nth(1)

        def get_gallery_cards():
            return page.locator('ul.grid > li, ul[class*="grid"] > li')

        # Snapshot cards before any action
        cards_before = get_gallery_cards()
        initial_count = cards_before.count()
        print(f"Gallery cards currently in DOM: {initial_count}")
        old_card_srcs = []
        for i in range(initial_count):
            c = cards_before.nth(i)
            src = c.locator("img").get_attribute("src") or ""
            txt = c.inner_text().replace("\n", " | ")
            print(f"  Existing Card {i}: {txt!r} (src: {src[:70]})")
            old_card_srcs.append(src)

        # 1. Upload _01.png
        img01 = slot_files[0]
        print(f"\n[Step 1] Uploading image 01: {img01.name}...")
        gallery_input.set_input_files(str(img01))
        print("  File assigned to dropzone. Waiting for upload...")

        # Poll for new card to appear
        new_card_index = None
        for attempt in range(40):
            page.wait_for_timeout(1000)
            cards = get_gallery_cards()
            curr_count = cards.count()
            if curr_count > initial_count:
                print(f"  New card appeared (count: {curr_count})!")
                # Wait until it is no longer loading
                last_card = cards.last
                txt = last_card.inner_text()
                if "Loading" not in txt:
                    print("  Upload complete (not Loading)!")
                    new_card_index = curr_count - 1
                    break
                else:
                    print(f"  [Attempt {attempt+1}] Still loading...")

        if new_card_index is None:
            cards = get_gallery_cards()
            new_card_index = cards.count() - 1

        page.wait_for_timeout(1000)
        cards = get_gallery_cards()
        new_card = cards.nth(new_card_index)
        new_card_text = new_card.inner_text().replace("\n", " | ")
        print(f"New card: {new_card_text!r}")

        # 2. Set new image as Thumbnail if it isn't already
        if "Thumbnail" not in new_card_text:
            print("\n[Step 2] Setting newly uploaded image as Thumbnail...")
            menu_btn = new_card.locator('button[aria-haspopup="menu"]')
            if menu_btn.count():
                menu_btn.click()
                page.wait_for_timeout(500)
                set_thumb_item = page.locator('[role="menu"] [role="menuitem"]:has-text("Set as thumbnail")')
                if set_thumb_item.count() and set_thumb_item.is_enabled():
                    set_thumb_item.click()
                    page.wait_for_timeout(1500)
                    print("  Clicked 'Set as thumbnail'.")
                else:
                    print("  'Set as thumbnail' not clickable or already set.")
                    page.keyboard.press("Escape")
        else:
            print("\n[Step 2] Newly uploaded card already has Thumbnail badge.")

        # 3. Delete old superseded cards (cards that were present before this upload)
        if old_card_srcs:
            print(f"\n[Step 3] Deleting {len(old_card_srcs)} old superseded image card(s)...")
            for old_src in old_card_srcs:
                cards = get_gallery_cards()
                found = False
                for i in range(cards.count()):
                    c = cards.nth(i)
                    src = c.locator("img").get_attribute("src") or ""
                    # Match old src or check if not thumbnail
                    if old_src and src == old_src:
                        print(f"  Found old card {i} (matching src). Deleting...")
                        menu_btn = c.locator('button[aria-haspopup="menu"]')
                        menu_btn.click()
                        page.wait_for_timeout(500)
                        del_item = page.locator('[role="menu"] [role="menuitem"]:has-text("Delete")')
                        if del_item.count():
                            del_item.click()
                            page.wait_for_timeout(800)
                            dialog = page.locator('[role="dialog"]')
                            if dialog.count() and dialog.first.is_visible():
                                confirm_btn = dialog.first.locator('button:has-text("Delete")')
                                confirm_btn.click()
                                page.wait_for_timeout(1500)
                                print("  Confirmed deletion in modal.")
                        found = True
                        break
                if not found:
                    print(f"  Old card src {old_src[:50]} not found in current gallery (already deleted).")

        # 4. Upload any subsequent slot files (_02.png, _03.png, ...) sequentially
        if len(slot_files) > 1:
            print(f"\n[Step 4] Sequentially uploading {len(slot_files) - 1} extra image(s)...")
            for idx, extra_file in enumerate(slot_files[1:], start=2):
                print(f"  Uploading slot {idx:02d}: {extra_file.name}...")
                cards = get_gallery_cards()
                before_extra = cards.count()
                gallery_input.set_input_files(str(extra_file))

                # Poll for completion
                for attempt in range(40):
                    page.wait_for_timeout(1000)
                    cards = get_gallery_cards()
                    if cards.count() > before_extra:
                        last_c = cards.last
                        if "Loading" not in last_c.inner_text():
                            print(f"    Slot {idx:02d} upload complete!")
                            break

        # Final summary of cards currently in gallery
        page.wait_for_timeout(1000)
        final_cards = get_gallery_cards()
        print(f"\nFinal staged gallery cards count: {final_cards.count()}")
        for i in range(final_cards.count()):
            c = final_cards.nth(i)
            print(f"  Card {i}: {c.inner_text().replace(chr(10), ' | ')!r}")

        print("\n[NOTE] As requested: Did NOT click 'Save'. Changes remain uncommitted in your Brave window.")
        browser.close()


def cmd_inspect_files(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_id = args.mod_id or "11221"
    url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/files"
    print(f"Connecting to Brave on {args.cdp}...")
    print(f"Navigating to {url}...")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        page.goto(url, wait_until="domcontentloaded")
        page.wait_for_timeout(2500)

        info = page.evaluate("""() => {
            const fileInputs = Array.from(document.querySelectorAll('input[type="file"]')).map((el, i) => {
                const parent = el.closest('div[class*="drop"], form, section') || el.parentElement;
                return {
                    index: i,
                    accept: el.accept,
                    multiple: el.multiple,
                    parentText: (parent ? parent.innerText : '').slice(0, 150)
                };
            });

            const inputs = Array.from(document.querySelectorAll('input, textarea, select')).map(el => ({
                tag: el.tagName,
                id: el.id,
                name: el.name,
                type: el.type,
                val: (el.value || '').slice(0, 50)
            })).filter(el => el.type !== 'hidden');

            const buttons = Array.from(document.querySelectorAll('button')).map(b => ({
                text: (b.innerText || '').trim(),
                ariaLabel: b.getAttribute('aria-label') || '',
                id: b.id
            })).filter(b => b.text || b.ariaLabel);

            return { fileInputs, inputs, buttons };
        }""")

        print(f"\nFile inputs found ({len(info['fileInputs'])}):")
        for fi in info["fileInputs"]:
            print(f"  [{fi['index']}] accept={fi['accept']} multiple={fi['multiple']}")
            print(f"       parentText={repr(fi['parentText'][:80])}")

        print(f"\nInputs found ({len(info['inputs'])}):")
        for inp in info["inputs"]:
            print(f"  {inp['tag']} id={inp['id']!r} name={inp['name']!r} type={inp['type']!r} val={inp['val']!r}")

        print(f"\nButtons found ({len(info['buttons'])}):")
        for b in info["buttons"]:
            txt = b['text'].replace('\n', ' ')
            if txt:
                print(f"  Button: {txt!r} (id={b['id']!r}, ariaLabel={b['ariaLabel']!r})")

        # Inspect table rows and action elements
        row_details = page.evaluate("""() => {
            const rows = Array.from(document.querySelectorAll('table tbody tr, [role="row"]')).map((r, i) => {
                const cells = Array.from(r.querySelectorAll('td, th')).map(c => c.innerText.trim().replace(/\\n+/g, ' | '));
                const interactive = Array.from(r.querySelectorAll('button, a, [role="button"]')).map(el => ({
                    tag: el.tagName,
                    text: el.innerText.trim(),
                    ariaLabel: el.getAttribute('aria-label') || '',
                    title: el.getAttribute('title') || '',
                    className: el.className
                }));
                return { index: i, cells, interactive };
            });
            return rows;
        }""")
        print(f"\nTable row details:\n{json.dumps(row_details, indent=2)}")

        # Also inspect what tabs exist on the mod edit page (is there a separate changelog tab?)
        edit_nav = page.evaluate("""() => {
            return Array.from(document.querySelectorAll('nav a, header a, ul a')).filter(a => (a.getAttribute('href') || '').includes('/edit/')).map(a => ({
                text: a.innerText.trim().replace(/\\n+/g, ' | '),
                href: a.getAttribute('href')
            }));
        }""")
        print(f"\nEdit nav links:\n{json.dumps(edit_nav, indent=2)}")

        page.keyboard.press("Escape")
        browser.close()


def fill_file_options_description(page, desc_text: str) -> bool:
    """Expand 'File options' and reliably sync description to SCEditor and DOM events."""
    desc_ta = page.locator('textarea[placeholder*="Provide additional information"]')
    if not desc_ta.count() or not desc_ta.first.is_visible():
        fo_btn = page.locator('button:has-text("File options")')
        if fo_btn.count():
            fo_btn.first.click()
            page.wait_for_timeout(500)

    # Sync via SCEditor instance and dispatch DOM events on all matching textareas
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

    # Also fill visible textarea directly via Playwright
    desc_ta = page.locator('textarea[placeholder*="Provide additional information"]')
    for idx in range(desc_ta.count()):
        if desc_ta.nth(idx).is_visible():
            desc_ta.nth(idx).fill(desc_text)
            page.wait_for_timeout(200)

    return True


def cmd_update_file_options_and_changelog(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_name = args.mod
    mod_id = args.mod_id or "13090"
    packet = parse_publish_help(mod_name)

    url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/files"
    print(f"Connecting to Brave on {args.cdp}...")
    print(f"Navigating to {url}...")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        if f"/mods/{mod_id}/edit/files" not in page.url:
            page.goto(url, wait_until="domcontentloaded")
            page.wait_for_timeout(2000)

        # Dismiss any open popups/dialogs
        page.keyboard.press("Escape")
        page.wait_for_timeout(300)
        page.keyboard.press("Escape")
        page.wait_for_timeout(300)

        # 1. Update File Options (Description)
        more_btn = page.locator('button[aria-label="More actions"]').first
        if not more_btn.count():
            print("ERROR: 'More actions' button on file row not found!")
            browser.close()
            sys.exit(1)

        print("\n[Step 1] Opening File Edit to fill Description...")
        more_btn.click()
        page.wait_for_timeout(500)
        edit_item = page.get_by_role("menuitem", name="Edit", exact=True)
        edit_item.click()
        page.wait_for_timeout(1000)

        print(f"  Filling File Description ({len(packet['file_description'])} chars)...")
        fill_file_options_description(page, packet["file_description"])
        page.wait_for_timeout(500)

        save_file_btn = page.locator('button:has-text("Save file")')
        if save_file_btn.count():
            save_file_btn.click()
            page.wait_for_timeout(2500)
            print("  File options (Description) saved successfully!")

        # 2. Add or Edit Changelog Entry
        print("\n[Step 2] Adding / Checking Changelog Entry...")
        page.wait_for_timeout(1000)
        more_btn = page.locator('button[aria-label="More actions"]').first
        more_btn.click()
        page.wait_for_timeout(500)
        cl_menu = page.locator('[role="menu"] [role="menuitem"]').filter(has_text=re.compile(r"(Add|Edit) changelog"))
        if cl_menu.count():
            menu_label = cl_menu.first.inner_text().strip()
            print(f"  Found changelog menu item: {menu_label!r}")
            cl_menu.first.click()
            page.wait_for_timeout(1000)

            dialog = page.locator('[role="dialog"]')
            if dialog.count() and dialog.first.is_visible():
                # Verify / set version
                ver_input = dialog.locator('#changelog-version')
                if ver_input.count() and ver_input.input_value() != packet["file_version"]:
                    ver_input.fill(packet["file_version"])

                cl_text = packet["changelog_blocks"][0] if packet["changelog_blocks"] else "Initial release of this mod."
                current_cl_val = dialog.locator('#changelog-text').input_value()
                print(f"  Current Changelog in dialog: {current_cl_val!r}")
                if current_cl_val != cl_text:
                    print(f"  Filling Changelog text: {cl_text!r}...")
                    dialog.locator('#changelog-text').fill(cl_text)
                    page.wait_for_timeout(500)

                cl_save_btn = dialog.locator('button:has-text("Save")')
                if cl_save_btn.count():
                    cl_save_btn.click()
                    page.wait_for_timeout(2000)
                    print("  Changelog entry confirmed/saved!")
            else:
                print("  WARNING: Add/Edit changelog dialog not visible!")
        else:
            print("  WARNING: 'Add/Edit changelog' item not found in menu!")

        # 3. Verify final state
        page.wait_for_timeout(1500)
        
        # Check logs tab / changelog state
        page.goto(f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}?tab=logs", wait_until="domcontentloaded")
        page.wait_for_timeout(2000)
        logs_text = page.evaluate("""() => {
            const bodyText = document.body.innerText;
            const logItems = Array.from(document.querySelectorAll('[class*="changelog"], [class*="log"], [class*="version"]')).map(el => el.innerText.trim().replace(/\\n+/g, ' | ')).filter(t => t.length > 5 && t.length < 300);
            return {
                url: window.location.href,
                hasInitialRelease: bodyText.includes('Initial release'),
                has100: bodyText.includes('1.0.0'),
                logItems: logItems.slice(0, 10)
            };
        }""")
        print(f"\nPublic Logs Tab check (?tab=logs):\n{json.dumps(logs_text, indent=2)}")

        page.goto(f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/general", wait_until="domcontentloaded")
        page.wait_for_timeout(2000)
        pub_btn = page.locator('button:has-text("Publish")').first
        is_ready = pub_btn.count() and not pub_btn.is_disabled()
        print(f"\nFinal publish readiness: {is_ready}")
        browser.close()


def cmd_stage_file(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_name = args.mod
    mod_id = args.mod_id or "13090"
    packet = parse_publish_help(mod_name)

    # Locate zip file in 04_DownloadZips
    zips_dir = REPO_ROOT / "04_DownloadZips"
    zip_path = zips_dir / f"{mod_name}.zip"
    if not zip_path.is_file():
        candidates = list(zips_dir.glob(f"*{mod_name}*.zip"))
        if candidates:
            zip_path = candidates[0]
        else:
            print(f"ERROR: Zip not found for {mod_name} in {zips_dir}")
            sys.exit(1)

    print(f"Mod: {mod_name} (ID: {mod_id})")
    print(f"Zip: {zip_path.name} ({zip_path.stat().st_size:,} bytes)")
    print(f"File Display Name: {packet['file_display_name']!r}")
    print(f"File Version: {packet['file_version']!r}")
    print(f"File Description: {len(packet['file_description'])} chars")

    url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/files"
    print(f"\nConnecting to Brave and navigating to {url}...")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        if f"/mods/{mod_id}/edit/files" not in page.url:
            page.goto(url, wait_until="domcontentloaded")
            page.wait_for_timeout(2000)

        file_inputs = page.locator('input[type="file"]')
        if not file_inputs.count():
            print("ERROR: No file inputs found on page!")
            browser.close()
            sys.exit(1)

        upload_input = file_inputs.first
        print("\nAssigning zip file to upload input...")
        upload_input.set_input_files(str(zip_path))
        print("Zip assigned! Waiting for form/card to populate...")

        # Poll for new inputs, cards, or progress
        for attempt in range(30):
            page.wait_for_timeout(1000)
            visible_inputs = page.evaluate("""() => {
                const inputs = Array.from(document.querySelectorAll('input:not([type="hidden"]):not([type="file"]), textarea, select')).map(el => ({
                    id: el.id,
                    name: el.name,
                    tagName: el.tagName,
                    type: el.type,
                    placeholder: el.placeholder,
                    value: el.value,
                    ariaLabel: el.getAttribute('aria-label') || '',
                    labels: Array.from(el.labels || []).map(l => l.innerText)
                }));
                const buttons = Array.from(document.querySelectorAll('button:not(#profile-menu)')).map(b => ({
                    id: b.id,
                    text: b.innerText.trim().replace(/\\n+/g, ' | '),
                    disabled: b.disabled
                })).filter(b => b.text && !['Games', 'Mods', 'Collections', 'Media', 'Community', 'Support', 'Search /', 'Upload'].includes(b.text));
                
                const cards = Array.from(document.querySelectorAll('[class*="card"], [class*="file-row"], tr, [role="row"]')).map(c => c.innerText.trim().replace(/\\n+/g, ' | ')).filter(t => t.length > 5 && t.length < 300);

                return { inputs, buttons, cards };
            }""")

            if visible_inputs["inputs"] or len(visible_inputs["buttons"]) > 2:
                print(f"  [Attempt {attempt+1}] Controls appeared!")
                print(f"\nVisible inputs ({len(visible_inputs['inputs'])}):")
                print(json.dumps(visible_inputs["inputs"], indent=2))
                print(f"\nVisible buttons ({len(visible_inputs['buttons'])}):")
                print(json.dumps(visible_inputs["buttons"], indent=2))
                break
            else:
                print(f"  [Attempt {attempt+1}] Waiting... (inputs: {len(visible_inputs['inputs'])})")

        # Fill File Display Name
        print(f"\nFilling File Display Name: {packet['file_display_name']!r}...")
        page.locator("#display-name").fill(packet["file_display_name"])

        # Fill File Version
        print(f"Filling File Version: {packet['file_version']!r}...")
        page.locator("#file-version").fill(packet["file_version"])

        # Fill File Description
        print(f"Filling File Description ({len(packet['file_description'])} chars)...")
        fill_file_options_description(page, packet["file_description"])

        page.wait_for_timeout(1000)

        # Check values in DOM
        staged_vals = page.evaluate("""() => {
            const nameEl = document.querySelector('#display-name');
            const verEl = document.querySelector('#file-version');
            const descEl = document.querySelector('textarea[placeholder*="Provide additional information"]');
            const saveBtn = Array.from(document.querySelectorAll('button')).find(b => b.innerText.trim() === 'Save file');
            return {
                name: nameEl ? nameEl.value : null,
                version: verEl ? verEl.value : null,
                desc: descEl ? descEl.value : null,
                saveBtnDisabled: saveBtn ? saveBtn.disabled : null
            };
        }""")
        print(f"\nStaged values in form:\n{json.dumps(staged_vals, indent=2)}")

        # Click Save file
        save_file_btn = page.locator('button:has-text("Save file")')
        if save_file_btn.count() and not staged_vals.get("saveBtnDisabled"):
            print("\nClicking 'Save file' button...")
            save_file_btn.click()
            page.wait_for_timeout(3000)
            print("Clicked 'Save file'!")

        # Add Changelog Entry via file row menu
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
                        print("Changelog entry added and saved.")

        # Check files list / table now present on page
        page.wait_for_timeout(2000)
        files_table_info = page.evaluate("""() => {
            const cards = Array.from(document.querySelectorAll('[class*="file-row"], tr, [role="row"], table tbody tr')).map(r => r.innerText.trim().replace(/\\n+/g, ' | '));
            const saveBtns = Array.from(document.querySelectorAll('button')).filter(b => b.innerText.trim().includes('Save') || b.innerText.trim().includes('Publish')).map(b => ({
                text: b.innerText.trim().replace(/\\n+/g, ' | '),
                disabled: b.disabled
            }));
            return { cards, saveBtns };
        }""")
        print(f"\nFiles table rows ({len(files_table_info['cards'])}):\n{json.dumps(files_table_info['cards'], indent=2)}")
        print(f"Save/Publish buttons state:\n{json.dumps(files_table_info['saveBtns'], indent=2)}")

        browser.close()


def cmd_publish_new_mod(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"Port {CDP_PORT} not open — starting Brave with debugging...")
        if not start_system_brave_debug():
            print(f"FAIL: Port {CDP_PORT} is not open and failed to start Brave.")
            sys.exit(1)
        time.sleep(2)

    mod_name = args.mod
    category = args.category or "Gameplay"
    packet = parse_publish_help(mod_name)

    mod_id = getattr(args, "mod_id", None)
    if not mod_id:
        cfg_file = REPO_ROOT / "06_PublishingSupport" / "NexusMods" / "Workflow" / "nexusmods-config.json"
        if cfg_file.is_file():
            try:
                with open(cfg_file, "r", encoding="utf-8") as f:
                    cfg_data = json.load(f)
                configured_id = cfg_data.get("mods", {}).get(mod_name, {}).get("nexus_mod_id")
                if configured_id and int(configured_id) > 0:
                    mod_id = str(configured_id)
            except Exception:
                pass

    print("=" * 60)
    print(f"AUTOMATED NEW MOD PUBLICATION PIPELINE: {mod_name}")
    print("=" * 60)
    print(f"  Mod Name: {packet['mod_name']}")
    print(f"  Version: {packet['version']}")
    print(f"  Category: {category}")
    print(f"  Mod ID: {mod_id or '(Will be created)'}")
    print(f"  File Display Name: {packet['file_display_name']}")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = get_nexus_page(browser)

        # [Step 1] Open 'Create your draft mod page' modal (if not already known)
        if mod_id:
            print(f"\n[Step 1] Mod already has Nexus Mod ID {mod_id}. Skipping draft creation modal...")
        else:
            print("\n[Step 1] Opening Draft Creation modal...")
            page.goto(f"{SITE}/games/{GAME_DOMAIN}", wait_until="domcontentloaded")
            page.wait_for_timeout(2000)

            # Dismiss any open dialog/overlay
            cancel_btn = page.locator('button:has-text("Cancel")')
            if cancel_btn.count() and cancel_btn.first.is_visible():
                cancel_btn.first.click()
                page.wait_for_timeout(500)
            page.keyboard.press("Escape")
            page.wait_for_timeout(300)

            top_upload = page.locator('button').filter(has_text=re.compile(r"^Upload$"))
            if top_upload.count():
                top_upload.first.click()
                page.wait_for_timeout(600)

            upload_mod_btn = page.locator('button:has-text("Upload mod"), [role="menuitem"]:has-text("Upload mod")')
            if upload_mod_btn.count():
                upload_mod_btn.first.click()
                page.wait_for_timeout(1500)

            draft_dialog = page.locator('[role="dialog"]:has-text("Create your draft mod page")').first
            if not draft_dialog.count() or not draft_dialog.is_visible():
                print("ERROR: Draft creation dialog not found.")
                browser.close()
                sys.exit(1)

            print("  Filling Mod Name and Short Description...")
            draft_dialog.locator('#mod-name').fill(packet["mod_name"])
            draft_dialog.locator('#short-description').fill(packet["short_description"])

            print(f"  Selecting Category: {category}...")
            show_cat_btn = draft_dialog.locator('button[aria-label="Show categories"]')
            if show_cat_btn.count() and show_cat_btn.is_visible():
                show_cat_btn.click()
                page.wait_for_timeout(500)
            else:
                cat_input = draft_dialog.locator('input[placeholder="Select category"]')
                cat_input.click()
                page.wait_for_timeout(300)

            opt = page.locator(f'[role="option"]:has-text("{category}")')
            if opt.count() > 0:
                opt.first.click()
            else:
                cat_input = draft_dialog.locator('input[placeholder="Select category"]')
                cat_input.fill(category)
                page.wait_for_timeout(300)
                opt2 = page.locator(f'[role="option"]:has-text("{category}")')
                if opt2.count() > 0:
                    opt2.first.click()
                else:
                    cat_input.press("Enter")
            page.wait_for_timeout(500)

            submit_btn = draft_dialog.locator('button:has-text("Create draft")')
            for _ in range(10):
                if submit_btn.is_enabled():
                    break
                page.wait_for_timeout(500)
            print(f"  Submitting Draft Creation modal (is_enabled={submit_btn.is_enabled()})...")
            submit_btn.click()

            # Wait for new mod ID
            for _ in range(30):
                page.wait_for_timeout(1000)
                m = re.search(r"/mods/(\d+)", page.url)
                if m and m.group(1) not in ("11221", "13090", "13091", "13093"):
                    mod_id = m.group(1)
                    break

            if not mod_id:
                print("ERROR: Failed to detect newly created mod ID.")
                browser.close()
                sys.exit(1)

            print(f"\n  Draft created! Nexus Mod ID: {mod_id}")
            print(f"  Draft URL: {page.url}")

        # Update nexusmods-config.json
        cfg_file = REPO_ROOT / "06_PublishingSupport" / "NexusMods" / "Workflow" / "nexusmods-config.json"
        if cfg_file.is_file():
            try:
                with open(cfg_file, "r", encoding="utf-8") as f:
                    cfg_data = json.load(f)
                if "mods" in cfg_data:
                    if mod_name not in cfg_data["mods"]:
                        cfg_data["mods"][mod_name] = {}
                    cfg_data["mods"][mod_name].update({
                        "intent": "update",
                        "nexus_mod_id": int(mod_id),
                        "update_group_id": "",
                        "update_group_name": packet.get("file_display_name", mod_name),
                        "file_category": "main",
                        "legacy_latest_file_id": 0,
                        "legacy_name_hint": "",
                        "page_url": f"https://www.nexusmods.com/7daystodie/mods/{mod_id}",
                    })
                    with open(cfg_file, "w", encoding="utf-8") as f:
                        json.dump(cfg_data, f, indent=2)
                    print(f"  Saved Mod ID {mod_id} to nexusmods-config.json")
            except Exception as e:
                print(f"  Warning: failed to update nexusmods-config.json: {e}")

        # Update Nexus-Status.json as well
        status_file = REPO_ROOT / "06_PublishingSupport" / "NexusMods" / "Workflow" / "Nexus-Status.json"
        if status_file.is_file():
            try:
                with open(status_file, "r", encoding="utf-8") as f:
                    st_data = json.load(f)
                for item in st_data.get("mods", []):
                    if item.get("base_name") == mod_name:
                        item["configured_nexus_mod_id"] = int(mod_id)
                        item["live_mod_id"] = int(mod_id)
                        item["live_version"] = packet["version"]
                        item["status"] = "Matches"
                        item["intent"] = "update"
                        item["notes"] = []
                        break
                with open(status_file, "w", encoding="utf-8") as f:
                    json.dump(st_data, f, indent=2)
                print(f"  Saved Mod ID {mod_id} to Nexus-Status.json")
            except Exception as e:
                print(f"  Warning: failed to update Nexus-Status.json: {e}")

        # [Step 2] Fill General Tab and Save
        print("\n[Step 2] Staging and saving General Tab...")
        gen_url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/general"
        if f"/mods/{mod_id}/edit/general" not in page.url:
            page.goto(gen_url, wait_until="domcontentloaded")
            page.wait_for_timeout(2000)
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
        if save_btn.count() and save_btn.is_enabled():
            save_btn.click()
            page.wait_for_timeout(2500)
            print("  General tab saved.")
        else:
            print("  General tab already up to date (Save button disabled).")

        # [Step 3] Stage Media Tab
        print("\n[Step 3] Staging Media Tab...")
        media_url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/media"
        page.goto(media_url, wait_until="domcontentloaded")
        page.wait_for_timeout(2000)

        images_dir = REPO_ROOT / "00_Images" / "02_ImagesFinal"
        slot_files = sorted([p for p in images_dir.glob(f"{mod_name}_*.png") if not p.name.endswith("_Under2MB.png")])
        if not slot_files:
            clean_name = re.sub(r"-v\d+.*$", "", mod_name)
            slot_files = sorted([p for p in images_dir.glob(f"{clean_name}_*.png") if not p.name.endswith("_Under2MB.png")])

        if slot_files:
            gallery_input = page.locator('input[type="file"]').nth(1)
            print(f"  Uploading {slot_files[0].name}...")
            gallery_input.set_input_files(str(slot_files[0]))
            
            # Wait for upload to complete
            for _ in range(40):
                page.wait_for_timeout(1000)
                cards = page.locator('ul.grid > li, ul[class*="grid"] > li')
                if cards.count() > 0 and "Loading" not in cards.last.inner_text():
                    print("  Upload complete (not Loading)!")
                    break

            # Confirm thumbnail badge
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

        # [Step 4] Stage Files Tab
        print("\n[Step 4] Staging Files Tab...")
        files_url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/files"
        page.goto(files_url, wait_until="domcontentloaded")
        page.wait_for_timeout(2000)

        zips_dir = REPO_ROOT / "04_DownloadZips"
        zip_path = zips_dir / f"{mod_name}.zip"
        if not zip_path.is_file():
            candidates = list(zips_dir.glob(f"*{mod_name}*.zip"))
            if candidates:
                zip_path = candidates[0]

        if zip_path.is_file():
            print(f"  Uploading release zip: {zip_path.name}...")
            page.locator('input[type="file"]').first.set_input_files(str(zip_path))
            page.wait_for_selector("#display-name", timeout=20000)
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

            # Add Changelog Entry via file row menu
            try:
                page.wait_for_selector('button[aria-label="More actions"]', timeout=15000)
            except Exception:
                pass
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

            # Ensure File Description is saved on the file row
            page.wait_for_timeout(1000)
            more_btn = page.locator('button[aria-label="More actions"]').first
            if more_btn.count():
                more_btn.click()
                page.wait_for_timeout(500)
                edit_item = page.get_by_role("menuitem", name="Edit", exact=True)
                if edit_item.count():
                    edit_item.click()
                    page.wait_for_timeout(1000)
                    fill_file_options_description(page, packet["file_description"])
                    save_file_btn = page.locator('button:has-text("Save file")')
                    if save_file_btn.count():
                        save_file_btn.click()
                        page.wait_for_timeout(2500)
                        print("  File options (Description) verified & saved on file row.")

        # [Step 5] Verify Publish status
        page.goto(f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/general", wait_until="domcontentloaded")
        page.wait_for_timeout(2000)
        pub_btn = page.locator('button.nxm-button-success:has-text("Publish"), button:has-text("Publish")').first
        is_ready = pub_btn.count() and not pub_btn.is_disabled()
        print(f"\n[Step 5] Mod draft preparation complete! Ready to publish: {is_ready}")
        print(f"  Review URL in Brave: {page.url}")

        if getattr(args, "publish", False) and is_ready:
            print("  Clicking Publish...")
            pub_btn.click()
            page.wait_for_timeout(1500)
            dialog = page.locator('[role="dialog"]:has-text("Publishing")')
            if dialog.count() and dialog.first.is_visible():
                confirm_btn = dialog.first.locator('button:has-text("Publish mod")')
                if confirm_btn.count():
                    confirm_btn.click()
                    page.wait_for_timeout(3500)
                    print(f"  PUBLISHED LIVE: {page.url}")

        browser.close()

        # Incremental sync: refresh checklist and matrix canvas
        try:
            import subprocess
            subprocess.run([sys.executable, str(REPO_ROOT / "06_PublishingSupport" / "SCRIPT-GeneratePublishChecklist.py")], check=False)
            subprocess.run([sys.executable, str(REPO_ROOT / "06_PublishingSupport" / "SCRIPT-SyncPublishingMatrix.py")], check=False)
        except Exception:
            pass


def cmd_check_publish_status(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_id = args.mod_id or "13090"
    url = f"{SITE}/games/{GAME_DOMAIN}/mods/{mod_id}/edit/general"

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]
        if f"/mods/{mod_id}/edit" not in page.url:
            page.goto(url, wait_until="domcontentloaded")
            page.wait_for_timeout(2000)

        # Inspect tabs and their status
        tabs_info = page.evaluate("""() => {
            const tabs = Array.from(document.querySelectorAll('a[href*="/edit/"], nav a')).map(a => ({
                text: a.innerText.trim().replace(/\\n+/g, ' | '),
                href: a.getAttribute('href') || ''
            })).filter(t => t.text && t.href.includes('/edit/'));

            const publishBtns = Array.from(document.querySelectorAll('button')).filter(b => b.innerText.trim() === 'Publish').map(b => ({
                text: b.innerText.trim(),
                disabled: b.disabled,
                ariaDisabled: b.getAttribute('aria-disabled')
            }));

            const banners = Array.from(document.querySelectorAll('[role="alert"], [class*="banner"], [class*="notice"]')).map(el => el.innerText.trim().replace(/\\n+/g, ' | ')).filter(t => t.length > 5 && t.length < 300);

            return { tabs, publishBtns, banners };
        }""")
        print(f"Tabs info:\n{json.dumps(tabs_info['tabs'], indent=2)}")
        print(f"\nPublish button info:\n{json.dumps(tabs_info['publishBtns'], indent=2)}")
        print(f"\nBanners / Notices:\n{json.dumps(tabs_info['banners'][:5], indent=2)}")

        browser.close()


def cmd_check_media_cards(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]

        # First, dismiss any open dialogs/overlays with Escape or Cancel
        cancel_btn = page.locator('button:has-text("Cancel")')
        if cancel_btn.count() and cancel_btn.first.is_visible():
            cancel_btn.first.click()
            page.wait_for_timeout(500)
        page.keyboard.press("Escape")
        page.wait_for_timeout(300)

        gallery_items = page.locator('ul.grid > li, ul[class*="grid"] > li')
        count = gallery_items.count()
        print(f"Gallery cards count: {count}")

        for i in range(count):
            item = gallery_items.nth(i)
            text = item.inner_text().replace("\n", " | ")
            img_src = item.locator("img").get_attribute("src") or ""
            print(f"\nCard {i}: {text!r}")
            print(f"  img src: {img_src[:80]}")

            menu_btn = item.locator('button[aria-haspopup="menu"]')
            if menu_btn.count():
                menu_btn.click()
                page.wait_for_timeout(600)
                opts = page.locator('[role="menu"] [role="menuitem"]').all_inner_texts()
                print(f"  menu options: {opts}")
                page.keyboard.press("Escape")
                page.wait_for_timeout(300)

        browser.close()


def cmd_inspect_upload_menu(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]

        # Check if "Create your draft mod page" dialog is already open
        draft_dialog = page.locator('[role="dialog"]:has-text("Create your draft mod page")')
        if not draft_dialog.count() or not draft_dialog.first.is_visible():
            # Dismiss any popup/menu first
            page.keyboard.press("Escape")
            page.wait_for_timeout(300)
            page.keyboard.press("Escape")
            page.wait_for_timeout(300)

            # Click top upload button
            top_upload = page.locator('button').filter(has_text=re.compile(r"^Upload$"))
            if top_upload.count():
                top_upload.first.click()
                page.wait_for_timeout(600)

            # Click "Upload mod" button in dropdown
            upload_mod_btn = page.locator('button:has-text("Upload mod")')
            if upload_mod_btn.count():
                print("Clicking 'Upload mod' button...")
                upload_mod_btn.first.click()
                page.wait_for_timeout(1500)

        draft_dialog = page.locator('[role="dialog"]:has-text("Create your draft mod page")').first
        if not draft_dialog.count() or not draft_dialog.is_visible():
            print("ERROR: Draft creation dialog not found.")
            browser.close()
            return

        print("Draft dialog found!")
        d1_info = draft_dialog.evaluate("""dialog => {
            const inputs = Array.from(dialog.querySelectorAll('input, textarea, select')).map(el => ({
                id: el.id,
                name: el.name,
                tagName: el.tagName,
                type: el.type,
                placeholder: el.placeholder,
                value: el.value,
                ariaLabel: el.getAttribute('aria-label') || '',
                labels: Array.from(el.labels || []).map(l => l.innerText)
            }));
            const buttons = Array.from(dialog.querySelectorAll('button')).map(b => ({
                id: b.id,
                text: b.innerText.trim().replace(/\\n+/g, ' | '),
                type: b.type,
                disabled: b.disabled,
                ariaHaspopup: b.getAttribute('aria-haspopup') || '',
                role: b.getAttribute('role') || ''
            }));
            return { inputs, buttons };
        }""")
        print(f"\nDialog Controls:\n{json.dumps(d1_info, indent=2)}")

        # Check Category container and toggle button
        cat_parent = draft_dialog.locator('input[placeholder="Select category"]').locator('..')
        print(f"Cat parent HTML: {cat_parent.evaluate('e => e.parentElement.innerHTML')[:400]}")

        # Click the toggle button for category
        cat_btn = cat_parent.locator('button[aria-haspopup="listbox"]')
        if not cat_btn.count():
            cat_btn = draft_dialog.locator('button[aria-haspopup="listbox"]').nth(1)

        print(f"Clicking Category toggle button...")
        cat_btn.click()
        page.wait_for_timeout(600)

        # Look in portal or page for options
        all_options = page.locator('[role="option"], li[role="option"], [data-headlessui-state*="option"]').all_inner_texts()
        print(f"All options found ({len(all_options)}):\n{all_options}")
        page.keyboard.press("Escape")

        browser.close()


def cmd_stage_new_mod_draft(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    mod_name = args.mod
    packet = parse_publish_help(mod_name)
    print(f"Loaded PublishHelp for: {mod_name}")
    print(f"  Mod Name: {packet['mod_name']}")
    print(f"  Short Description: {packet['short_description']}")

    category = args.category or "Gameplay"
    print(f"  Target Category: {category}")

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]

        # Check if draft dialog is already open
        draft_dialog = page.locator('[role="dialog"]:has-text("Create your draft mod page")')
        if not draft_dialog.count() or not draft_dialog.first.is_visible():
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
                print("Clicking 'Upload mod' button...")
                upload_mod_btn.first.click()
                page.wait_for_timeout(1500)

        draft_dialog = page.locator('[role="dialog"]:has-text("Create your draft mod page")').first
        if not draft_dialog.count() or not draft_dialog.is_visible():
            print("ERROR: Draft creation dialog not found.")
            browser.close()
            sys.exit(1)

        print("\nFilling Mod Name...")
        name_input = draft_dialog.locator('#mod-name')
        name_input.fill(packet["mod_name"])

        print("Filling Short Description...")
        short_input = draft_dialog.locator('#short-description')
        short_input.fill(packet["short_description"])

        print(f"Selecting Category: {category}...")
        cat_input = draft_dialog.locator('input[placeholder="Select category"]')
        cat_input.click()
        page.wait_for_timeout(400)
        cat_input.fill(category)
        page.wait_for_timeout(400)

        # Click the matching option in the dropdown list
        option = page.locator(f'[role="option"]:has-text("{category}")').first
        if option.count() and option.is_visible():
            option.click()
            print(f"Selected category '{category}' from dropdown.")
        else:
            print(f"Dropdown option not found; pressing Enter on input...")
            cat_input.press("Enter")
        page.wait_for_timeout(500)

        # Check the 'Create draft' button status
        submit_btn = draft_dialog.locator('button:has-text("Create draft")')
        is_disabled = submit_btn.get_attribute("disabled") is not None
        print(f"\n'Create draft' button disabled: {is_disabled}")

        # Check values currently in dialog
        vals = draft_dialog.evaluate("""dialog => {
            const nameEl = dialog.querySelector('#mod-name');
            const shortEl = dialog.querySelector('#short-description');
            const catEl = dialog.querySelector('input[placeholder="Select category"]');
            const gameEl = dialog.querySelector('input[placeholder="Type game"]');
            return {
                name: nameEl ? nameEl.value : null,
                short: shortEl ? shortEl.value : null,
                game: gameEl ? gameEl.value : null,
                category: catEl ? catEl.value : null
            };
        }""")
        print(f"Dialog field values currently staged:\n{json.dumps(vals, indent=2)}")

        print("\n[NOTE] As requested: Did NOT click 'Create draft'. Changes remain uncommitted in your Brave window.")
        browser.close()


def cmd_submit_create_draft(args: argparse.Namespace) -> None:
    if not port_open(port=CDP_PORT):
        print(f"FAIL: Port {CDP_PORT} is not open. Start Brave with debugging first.")
        sys.exit(1)

    from playwright.sync_api import sync_playwright

    with sync_playwright() as pw:
        browser = pw.chromium.connect_over_cdp(args.cdp)
        page = browser.contexts[0].pages[0]

        draft_dialog = page.locator('[role="dialog"]:has-text("Create your draft mod page")')
        if not draft_dialog.count() or not draft_dialog.first.is_visible():
            print("ERROR: Draft creation dialog is not currently visible in Brave.")
            browser.close()
            sys.exit(1)

        submit_btn = draft_dialog.first.locator('button:has-text("Create draft")')
        if not submit_btn.count():
            print("ERROR: 'Create draft' button not found.")
            browser.close()
            sys.exit(1)

        print("Clicking 'Create draft' button in modal...")
        submit_btn.click()

        print("Waiting for draft page navigation/creation...")
        # Poll for URL change or navigation
        new_url = ""
        for attempt in range(30):
            page.wait_for_timeout(1000)
            curr_url = page.url
            print(f"  [Poll {attempt+1}] URL: {curr_url}")
            match = re.search(r"/mods/(\d+)", curr_url)
            if match and match.group(1) != "11221": # not the old Weekday mod
                new_url = curr_url
                mod_id = match.group(1)
                print(f"\nSUCCESS! New draft mod created!")
                print(f"  Nexus Mod ID: {mod_id}")
                print(f"  Draft URL: {new_url}")
                print(f"  Page Title: {page.title()}")
                break

        if not new_url:
            print(f"Final URL after timeout: {page.url}")

        browser.close()


def build_parser() -> argparse.ArgumentParser:
    ap = argparse.ArgumentParser(description="Nexus Mods Playwright Automation Helper (Brave)")
    sub = ap.add_subparsers(dest="cmd", required=True)

    p_debug = sub.add_parser("start-brave-debug", help="Start Brave with remote debugging on port 9222")
    p_debug.set_defaults(func=cmd_start_brave_debug)

    p_check = sub.add_parser("check-connection", help="Verify Playwright can attach to Brave via CDP")
    p_check.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_check.set_defaults(func=cmd_check_connection)

    p_parse = sub.add_parser("parse", help="Parse a PublishHelp markdown file")
    p_parse.add_argument("mod", help="Mod base name (e.g. AGF-V3-SERVER-2Weekday)")
    p_parse.set_defaults(func=cmd_parse)

    p_inspect = sub.add_parser("inspect-mod", help="Inspect a live Nexus mod page for edit links")
    p_inspect.add_argument("mod_id", default="11221", nargs="?", help="Nexus mod ID (default 11221)")
    p_inspect.add_argument("--tab", default="", help="Sub-tab, e.g. general, media, files, permissions")
    p_inspect.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_inspect.set_defaults(func=cmd_inspect_mod)

    p_inspect_media = sub.add_parser("inspect-media", help="Inspect Media tab in detail")
    p_inspect_media.add_argument("mod_id", default="11221", nargs="?", help="Nexus mod ID (default 11221)")
    p_inspect_media.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_inspect_media.set_defaults(func=cmd_inspect_media)

    p_stage_media = sub.add_parser("stage-media", help="Stage media upload without saving")
    p_stage_media.add_argument("mod", default="AGF-V3-SERVER-2Weekday", nargs="?", help="Mod base name")
    p_stage_media.add_argument("--mod-id", default="11221", help="Nexus mod ID (default 11221)")
    p_stage_media.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_stage_media.set_defaults(func=cmd_stage_media)

    p_check_media = sub.add_parser("check-media-cards", help="Check current media cards and menu options")
    p_check_media.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_check_media.set_defaults(func=cmd_check_media_cards)

    p_inspect_upload = sub.add_parser("inspect-upload-menu", help="Inspect top-level Upload menu")
    p_inspect_upload.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_inspect_upload.set_defaults(func=cmd_inspect_upload_menu)

    p_inspect_files = sub.add_parser("inspect-files", help="Inspect Files tab in detail")
    p_inspect_files.add_argument("mod_id", default="11221", nargs="?", help="Nexus mod ID (default 11221)")
    p_inspect_files.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_inspect_files.set_defaults(func=cmd_inspect_files)

    p_fill_gen = sub.add_parser("fill-general", help="Fill General tab without saving")
    p_fill_gen.add_argument("mod", default="AGF-V3-SERVER-2Weekday", nargs="?", help="Mod base name")
    p_fill_gen.add_argument("--mod-id", default="11221", help="Nexus mod ID (default 11221)")
    p_fill_gen.add_argument("--save", action="store_true", help="Save the general tab after filling")
    p_fill_gen.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_fill_gen.set_defaults(func=cmd_fill_general)

    p_stage_draft = sub.add_parser("stage-new-mod-draft", help="Stage new mod draft modal without submitting")
    p_stage_draft.add_argument("mod", default="AGF-V3-SERVER-POISwitchesUnlockDoors", nargs="?", help="Mod base name")
    p_stage_draft.add_argument("--category", default="Gameplay", help="Category name (default: Gameplay)")
    p_stage_draft.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_stage_draft.set_defaults(func=cmd_stage_new_mod_draft)

    p_submit_draft = sub.add_parser("submit-create-draft", help="Click Create draft button in the open modal")
    p_submit_draft.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_submit_draft.set_defaults(func=cmd_submit_create_draft)

    p_stage_file = sub.add_parser("stage-file", help="Stage zip upload on Files tab")
    p_stage_file.add_argument("mod", default="AGF-V3-SERVER-POISwitchesUnlockDoors", nargs="?", help="Mod base name")
    p_stage_file.add_argument("--mod-id", default="13090", help="Nexus mod ID (default 13090)")
    p_stage_file.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_stage_file.set_defaults(func=cmd_stage_file)

    p_check_publish = sub.add_parser("check-publish-status", help="Check publish button and validation status")
    p_check_publish.add_argument("mod_id", default="13090", nargs="?", help="Nexus mod ID (default 13090)")
    p_check_publish.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_check_publish.set_defaults(func=cmd_check_publish_status)

    p_fix_file = sub.add_parser("update-file-options-and-changelog", help="Fill File options description and changelog on staged file")
    p_fix_file.add_argument("mod", default="AGF-V3-SERVER-POISwitchesUnlockDoors", nargs="?", help="Mod base name")
    p_fix_file.add_argument("--mod-id", default="13090", help="Nexus mod ID (default 13090)")
    p_fix_file.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_fix_file.set_defaults(func=cmd_update_file_options_and_changelog)

    p_pub_new = sub.add_parser("publish-new-mod", help="Automated end-to-end staging of a brand new mod up to Publish")
    p_pub_new.add_argument("mod", help="Mod base name (e.g. AGF-V3-SERVER-POISwitchesUnlockDoors)")
    p_pub_new.add_argument("--mod-id", help="Nexus mod ID if already created")
    p_pub_new.add_argument("--category", default="Gameplay", help="Category name (default: Gameplay)")
    p_pub_new.add_argument("--publish", action="store_true", help="Publish mod immediately if ready")
    p_pub_new.add_argument("--cdp", default=DEFAULT_CDP, help=f"CDP URL (default {DEFAULT_CDP})")
    p_pub_new.set_defaults(func=cmd_publish_new_mod)

    return ap


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
