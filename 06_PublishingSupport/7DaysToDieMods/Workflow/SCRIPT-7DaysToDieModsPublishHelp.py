"""Generate 7DaysToDieMods PublishHelp packets from 03_ReleaseSource.

One file per ReleaseSource mod: PublishHelp/{ModName}.md
Description is Import BBCode: HTML headings with span colors, plus [list]/[*].
"""

from __future__ import annotations

import argparse
import datetime as dt
import importlib.util
import json
import os
import re
import shutil
import sys

sys.dont_write_bytecode = True

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
SITE_ROOT = os.path.dirname(WORKFLOW_DIR)
VS_CODE_ROOT = os.path.normpath(os.path.join(SITE_ROOT, "..", ".."))
PUBLISH_HELP_DIR = os.path.join(SITE_ROOT, "PublishHelp")
CONFIG_PATH = os.path.join(WORKFLOW_DIR, "7dtdmods-config.json")
IMAGES_FINAL_DIR = os.path.join(VS_CODE_ROOT, "00_Images", "02_ImagesFinal")
SLOT_PNG_RE = re.compile(r"_\d{2}\.png$", re.IGNORECASE)
NEXUS_STEP_PATH = os.path.join(
    VS_CODE_ROOT, "00_Support", "Automation", "workflow", "06_nexus.py"
)

SIDE_MAP = {
    "Server-Side (EAC-Friendly)": "Server Side Only",
    "Server-Side (EAC Off)": "Server Side Only",
    "Server/Client-Side (Required)": "Server & Client Side",
    "Client-Side (Only)": "Client Side Only",
}


def load_nexus_step():
    spec = importlib.util.spec_from_file_location("nexus_step6", NEXUS_STEP_PATH)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load {NEXUS_STEP_PATH}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def load_config() -> dict:
    if not os.path.isfile(CONFIG_PATH):
        return {}
    with open(CONFIG_PATH, "r", encoding="utf-8") as handle:
        data = json.load(handle)
    return data if isinstance(data, dict) else {}


def infer_category(base_name: str) -> str:
    if "HUDPlus" in base_name:
        return "UI"
    if "BackpackPlus" in base_name:
        return "Quality of Life"
    if "MapPlus" in base_name or base_name.endswith("-Map"):
        return "Map"
    if "DoorsPlus" in base_name:
        return "Building"
    if "Vehicle" in base_name or "Honk" in base_name:
        return "Vehicle"
    if "Audio" in base_name:
        return "Sound"
    return "Quality of Life"


def infer_side(mod_type_label: str) -> str:
    return SIDE_MAP.get(mod_type_label, "Server Side Only")


def format_images_section(base_name: str) -> str:
    lines = [
        "Thumbnail max is 2MB. Only `_01` needs an Under2MB sibling when the original is over 2MB.",
        "Gallery images (`_02` and later) can stay at full size.",
        "",
    ]
    names = []
    if os.path.isdir(IMAGES_FINAL_DIR):
        for name in sorted(os.listdir(IMAGES_FINAL_DIR)):
            if not name.startswith(f"{base_name}_"):
                continue
            if name.lower().endswith("_under2mb.png"):
                continue
            if not SLOT_PNG_RE.search(name):
                continue
            names.append(name)

    if not names:
        lines.append("No slot images found yet. Upload from `00_Images/02_ImagesFinal` when they exist.")
        return "\n".join(lines)

    if len(names) == 1:
        lines.append("Same image on **thumbnail** and **gallery** unless `_01` is over 2MB:")
    else:
        lines.append("Upload these:")

    for name in names:
        is_thumb = name.lower().endswith("_01.png")
        stem, ext = os.path.splitext(name)
        under_name = f"{stem}_Under2MB{ext}"
        under_path = os.path.join(IMAGES_FINAL_DIR, under_name)
        if is_thumb and os.path.isfile(under_path):
            lines.append(f"- Thumbnail: `00_Images/02_ImagesFinal/{under_name}`")
            lines.append(f"- Gallery: `00_Images/02_ImagesFinal/{name}`")
        elif is_thumb:
            lines.append(f"- Thumbnail + gallery: `00_Images/02_ImagesFinal/{name}`")
        else:
            lines.append(f"- Gallery: `00_Images/02_ImagesFinal/{name}`")
    return "\n".join(lines)


def _bbcode_lists_to_html(text: str) -> str:
    """Innermost [list]/[list=1] → <ul>/<ol><li>. Skips empty [*] items.

    Import's BBob [*] parser inserts a blank first <li> on [list=1], so
    numbering starts at 2 and nested items can duplicate. Real HTML lists
    are kept as blocks and Turndown numbers them from 1.
    """
    converted = text
    while True:
        end = converted.find("[/list]")
        if end < 0:
            break
        start = converted.rfind("[list", 0, end)
        if start < 0:
            break
        open_end = converted.find("]", start)
        if open_end < 0 or open_end > end:
            break
        numbered = converted[start : open_end + 1].lower().startswith("[list=1")
        body = converted[open_end + 1 : end]
        bullets = [part.strip() for part in re.split(r"\[\*\]", body)[1:]]
        items = [f"<li>{bullet}</li>" for bullet in bullets if bullet]
        if items:
            tag = "ol" if numbered else "ul"
            replacement = f"<{tag}>\n" + "\n".join(items) + f"\n</{tag}>"
        else:
            replacement = ""
        converted = converted[:start] + replacement + converted[end + len("[/list]") :]
    return converted


def nexus_bbcode_to_7dtd(text: str) -> str:
    """Nexus BBCode → Import-safe HTML/BBCode mix for 7DaysToDieMods.

    Headings: HTML + span (not [color] inside <h2>).
    Inline color/bold/links: HTML so they survive inside list items.
    Lists: HTML ul/ol so Import does not invent a blank first item.
    """
    converted = text.replace("[/*]", "")
    converted = converted.replace("Mods/<ModName>/ModInfo.xml", "Mods/(ModName)/ModInfo.xml")
    converted = re.sub(
        r"\[color=(#[0-9A-Fa-f]+)\]\[size=6\]\[b\](.*?)\[/b\]\[/size\]\[/color\]",
        r'\n<h1><span style="color:\1">\2</span></h1>\n',
        converted,
    )
    converted = re.sub(
        r"\[heading\]\[color=(#[0-9A-Fa-f]+)\]\[size=5\]\[b\](.*?)\[/b\]\[/size\]\[/color\]\[/heading\]",
        r'\n<h2><span style="color:\1">\2</span></h2>\n',
        converted,
    )
    converted = re.sub(
        r"\[heading\]\[color=(#[0-9A-Fa-f]+)\]\[size=4\]\[b\](.*?)\[/b\]\[/size\]\[/color\]\[/heading\]",
        r'\n<h3><span style="color:\1">\2</span></h3>\n',
        converted,
    )
    converted = re.sub(
        r"\[color=(#[0-9A-Fa-f]+)\]\[b\]\[size=4\](.*?)\[/size\]\[/b\]\[/color\]",
        r'\n<h3><span style="color:\1">\2</span></h3>\n',
        converted,
    )
    converted = re.sub(r"\[size=\d+\]", "", converted)
    converted = converted.replace("[/size]", "")
    converted = converted.replace("[heading]", "").replace("[/heading]", "")
    converted = re.sub(
        r"\[url=([^\]]+)\](.*?)\[/url\]",
        r'<a href="\1">\2</a>',
        converted,
        flags=re.DOTALL,
    )
    converted = re.sub(
        r"\[color=(#[0-9A-Fa-f]+)\]\[b\](.*?)\[/b\]\[/color\]",
        r'<span style="color:\1"><strong>\2</strong></span>',
        converted,
        flags=re.DOTALL,
    )
    converted = re.sub(
        r"\[color=(#[0-9A-Fa-f]+)\](.*?)\[/color\]",
        r'<span style="color:\1">\2</span>',
        converted,
        flags=re.DOTALL,
    )
    converted = converted.replace("[b]", "<strong>").replace("[/b]", "</strong>")
    converted = converted.replace("[i]", "<em>").replace("[/i]", "</em>")
    converted = _bbcode_lists_to_html(converted)
    converted = re.sub(r"\n{3,}", "\n\n", converted)
    return converted


def latest_changelog_plain(nexus, readme: str) -> str:
    formatted = nexus.extract_changelog_entries(readme)
    match = re.search(r"```text\n(.*?)```", formatted, re.DOTALL)
    if not match:
        return ""
    lines = [line.strip() for line in match.group(1).splitlines() if line.strip()]
    # Site changelog is markdown: one Enter joins lines. Blank line = two entries.
    return "\n\n".join(lines)


def build_packet(nexus, entry: dict, cfg_entry: dict, defaults: dict) -> str:
    folder_path = entry["folder_path"]
    base_name = entry["base_name"]
    readme = nexus.load_readme_text(folder_path)
    mod_info = nexus.load_modinfo_xml(folder_path)
    sections = nexus.parse_readme_sections(readme)
    mod_type = nexus.extract_mod_type_from_readme(readme)
    game_ver = "3"
    title = str(cfg_entry.get("site_title") or nexus.format_nexus_mod_name(base_name, game_ver))
    summary = str(cfg_entry.get("summary") or mod_info.get("description", "")).strip()
    category = str(cfg_entry.get("category") or infer_category(base_name))
    game_version = str(cfg_entry.get("game_version") or defaults.get("game_version") or "V3 Mods")
    server_side = str(cfg_entry.get("server_side") or infer_side(mod_type))
    credits = str(cfg_entry.get("credits") or defaults.get("credits") or "AuroraGiggleFairy")
    owner = str(defaults.get("github_owner") or "AuroraGiggleFairy")
    repo = str(cfg_entry.get("github_repo") or base_name)
    version = str(mod_info.get("version") or "?")
    mod_type_map = nexus.load_mod_type_map()
    file_desc = nexus.resolve_file_description(summary, mod_type, mod_type_map)
    changelog = latest_changelog_plain(nexus, readme)
    nexus_bbcode = nexus.generate_bbcode_full_description(title, game_ver, summary, sections)
    description = nexus_bbcode_to_7dtd(nexus_bbcode).rstrip() + "\n"
    images_section = format_images_section(base_name)
    now = dt.datetime.now().strftime("%Y-%m-%d  %I:%M %p")
    return f"""# {base_name} v{version}
### 7DaysToDieMods Details
Generated: {now}

Description is for **Import**, not Edit paste.

1. Select all in the description and delete it. Import **appends** if anything is left.
2. **BBCode** tab → **Import**.
3. Paste only the description fence contents (not the ` ``` ` lines).
4. Import, then check Preview. Numbered lists should start at 1.

---

## 1) Title

```text
{title}
```

---

## 2) Summary

```text
{summary}
```

---

## 3) Category, game version, side

Category:

```text
{category}
```

Game version:

```text
{game_version}
```

Server / client / both:

```text
{server_side}
```

---

## 4) Description

```text
{description}```

---

## 5) Images

{images_section}

---

## 6) Changelog

Version:

```text
{version}
```

Text field (plain text only — do not copy a code fence; that becomes a code block on the site).
The site joins a single Enter into one line. Keep the blank line between entries:

{changelog}

---

## 7) Download — first zip only

Upload `{base_name}.zip` once so the page can publish. File Type: **Main Mod**. Skip External link. After this, GitHub Releases replace the file — do not upload again.

GitHub sync fills **version** (tag) and **changelog** (release notes) plus the zip. It does **not** keep this Label / file Description. After the first synced update, check the new file and paste Label + Description again if they went blank or became the zip name.

Label:

```text
{title}
```

Description (same as Nexus file description: summary, blank line, Mod Type):

```text
{file_desc}
```

Version of Mod:

```text
{version}
```

---

## 8) Permissions

Editing & Alterations: your call (allow others to modify, or not).

---

## 9) Final page

GitHub sync:

```text
{owner}/{repo}
```

Credits:

```text
{credits}
```
"""


def generate_publish_help(dry_run: bool = False) -> int:
    nexus = load_nexus_step()
    config = load_config()
    defaults = config.get("defaults") if isinstance(config.get("defaults"), dict) else {}
    mods_cfg = config.get("mods") if isinstance(config.get("mods"), dict) else {}
    entries = nexus.find_mod_entries()
    if not entries:
        print("  No mods found in 03_ReleaseSource.")
        return 0

    print(f"  Found {len(entries)} ReleaseSource mod(s).")
    if dry_run:
        for entry in entries:
            print(f"    [DRYRUN] {entry['base_name']}")
        return 0

    if os.path.isdir(PUBLISH_HELP_DIR):
        shutil.rmtree(PUBLISH_HELP_DIR)
    os.makedirs(PUBLISH_HELP_DIR, exist_ok=True)

    for entry in entries:
        base_name = entry["base_name"]
        cfg_entry = mods_cfg.get(base_name, {})
        if not isinstance(cfg_entry, dict):
            cfg_entry = {}
        packet = build_packet(nexus, entry, cfg_entry, defaults)
        path = os.path.join(PUBLISH_HELP_DIR, f"{base_name}.md")
        with open(path, "w", encoding="utf-8") as handle:
            handle.write(packet)
            if not packet.endswith("\n"):
                handle.write("\n")
        print(f"  [PublishHelp] {base_name}")

    print("  PublishHelp files updated in:", PUBLISH_HELP_DIR)
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Generate 7DaysToDieMods PublishHelp packets from ReleaseSource."
    )
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--verbose", action="store_true")
    parser.add_argument("--strict", action="store_true")
    parser.add_argument("--workers", type=int, default=None)
    args = parser.parse_args()

    print("=" * 60)
    print("  7DAYSTODIEMODS PUBLISHHELP GENERATION")
    print("  Local sources only — no API calls")
    print("=" * 60)
    return generate_publish_help(dry_run=args.dry_run)


if __name__ == "__main__":
    sys.exit(main())
