"""Prep one 7dtdmods listing scratch folder for Cursor browser File-assign.

Usage:
  python SCRIPT-PrepListingUpload.py AGF-VPS-GlobalStormTracker
  python SCRIPT-PrepListingUpload.py AGF-VPS-GlobalStormTracker --ensure-cors

Copies into Workflow/_listing_upload/ (clears prior files first):
  - thumbnail (Under2MB if present, else _01)
  - gallery slot pngs (_01, _02, ...) never Under2MB
  - zip from 04_DownloadZips/
  - description.html from PublishHelp packet §4
  - fill.json  (all form fields — agent reads this once; do not re-skim the .md)

Does not touch ModInfo/README. Keep this script; scratch copies are disposable.
"""
from __future__ import annotations

import argparse
import json
import re
import shutil
import socket
import subprocess
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
# HERE = .../7DaysToDieMods/Workflow
# parents[0]=7DaysToDieMods, [1]=06_PublishingSupport, [2]=repo
REPO = HERE.parents[2]
UPLOAD = HERE / "_listing_upload"
PACKET_DIR = HERE.parent / "PublishHelp"
IMAGES = REPO / "00_Images" / "02_ImagesFinal"
ZIPS = REPO / "04_DownloadZips"


def fence_after(md: str, heading_pat: str) -> str:
    m = re.search(
        heading_pat + r".*?```(?:text)?\r?\n(.*?)```",
        md,
        re.S | re.I,
    )
    if not m:
        raise SystemExit(f"Could not extract fence for: {heading_pat}")
    return m.group(1).strip()


def extract_description(md: str) -> str:
    m = re.search(
        r"## 4\) Description.*?```(?:text)?\r?\n(.*?)```\r?\n\r?\n---\r?\n\r?\n## 5\)",
        md,
        re.S,
    )
    if not m:
        raise SystemExit("Could not extract ## 4) Description from packet")
    return m.group(1)


def extract_section3(md: str) -> tuple[str, str, str]:
    cat = fence_after(md, r"Category:\s*")
    ver = fence_after(md, r"Game version:\s*")
    side = fence_after(md, r"Server / client / both:\s*")
    return cat, ver, side


def extract_changelog_body(md: str) -> str:
    m = re.search(r"## 6\) Changelog(.*?)## 7\)", md, re.S)
    if not m:
        raise SystemExit("Could not find ## 6) Changelog")
    block = m.group(1)
    # Drop the Version fence; take remaining non-instruction plain lines
    block = re.sub(r"```(?:text)?\r?\n.*?\r?\n```", "", block, count=1, flags=re.S)
    lines = []
    for line in block.splitlines():
        s = line.strip()
        if not s:
            if lines and lines[-1] != "":
                lines.append("")
            continue
        if s.startswith("Version:") or s.startswith("Text field") or s.startswith("The site joins"):
            continue
        if s.startswith("Keep the blank") or s.startswith("---"):
            continue
        lines.append(s)
    while lines and lines[0] == "":
        lines.pop(0)
    while lines and lines[-1] == "":
        lines.pop()
    body = "\n".join(lines).strip()
    if not body:
        raise SystemExit("Empty changelog body")
    return body


def extract_zip_desc(md: str) -> str:
    m = re.search(
        r"## 7\) Download.*?Description \(.*?\)\s*:\s*```(?:text)?\r?\n(.*?)```",
        md,
        re.S,
    )
    if not m:
        # fallback: summary + blank + first Mod Type line style from label section
        raise SystemExit("Could not extract zip Description from ## 7)")
    return m.group(1).strip()


def port_open(port: int = 8765) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(0.3)
        return s.connect_ex(("127.0.0.1", port)) == 0


def build_fill(
    *,
    base: str,
    md: str,
    thumb_name: str,
    gallery_names: list[str],
    zip_name: str,
) -> dict:
    title = fence_after(md, r"## 1\) Title\s*")
    summary = fence_after(md, r"## 2\) Summary\s*")
    category, game_version, server_side = extract_section3(md)
    version = fence_after(md, r"## 6\) Changelog.*?Version:\s*")
    changelog_body = extract_changelog_body(md)
    zip_label = fence_after(md, r"## 7\) Download.*?Label:\s*")
    zip_desc = extract_zip_desc(md)
    zip_version = fence_after(md, r"Version of Mod:\s*")
    credits = fence_after(md, r"## 9\) Final page.*?Credits:\s*")

    categories = [category]
    # VP / VPS packets always also get Quality of Life (rule)
    if re.search(r"\bVP[S]?\b", title) or "-VP-" in base or "-VPS-" in base:
        if "Quality of Life" not in categories:
            categories.append("Quality of Life")

    return {
        "base": base,
        "title": title,
        "summary": summary,
        "categories": categories,
        "game_version": game_version,
        "server_side": server_side,
        "comments": False,
        "version": version,
        "changelog_body": changelog_body,
        "zip_label": zip_label,
        "zip_description": zip_desc,
        "zip_version": zip_version,
        "zip_file_type": "Main Mod",
        "credits": credits,
        "thumb": thumb_name,
        "gallery": gallery_names,
        "zip": zip_name,
        "cors_base": "http://127.0.0.1:8765",
        "description_url": "http://127.0.0.1:8765/description.html",
    }


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("mod", help="Folder/base name, e.g. AGF-VPS-GlobalStormTracker")
    ap.add_argument("--ensure-cors", action="store_true", help="Start CORS server if 8765 is free")
    args = ap.parse_args()
    base = args.mod.strip().removesuffix(".md")

    packet = PACKET_DIR / f"{base}.md"
    if not packet.is_file():
        alt = PACKET_DIR / base / f"{base}.md"
        packet = alt if alt.is_file() else packet
    if not packet.is_file():
        raise SystemExit(f"Missing packet: {PACKET_DIR / (base + '.md')}")

    zip_src = ZIPS / f"{base}.zip"
    if not zip_src.is_file():
        raise SystemExit(f"Missing zip: {zip_src}")

    slots = sorted(IMAGES.glob(f"{base}_*.png"))
    slots = [p for p in slots if "_Under2MB" not in p.name]
    if not slots:
        raise SystemExit(f"No images matching {IMAGES / (base + '_*.png')}")

    under = IMAGES / f"{base}_01_Under2MB.png"
    thumb = under if under.is_file() else IMAGES / f"{base}_01.png"
    if not thumb.is_file():
        raise SystemExit(f"Missing thumbnail: {thumb}")

    UPLOAD.mkdir(parents=True, exist_ok=True)
    for old in UPLOAD.iterdir():
        if old.is_file():
            old.unlink()

    md = packet.read_text(encoding="utf-8")
    (UPLOAD / "description.html").write_text(extract_description(md), encoding="utf-8")

    shutil.copy2(thumb, UPLOAD / thumb.name)
    for p in slots:
        shutil.copy2(p, UPLOAD / p.name)
    shutil.copy2(zip_src, UPLOAD / zip_src.name)

    fill = build_fill(
        base=base,
        md=md,
        thumb_name=thumb.name,
        gallery_names=[p.name for p in slots],
        zip_name=zip_src.name,
    )
    (UPLOAD / "fill.json").write_text(
        json.dumps(fill, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )

    print("packet", packet.name)
    print("thumb", thumb.name)
    print("gallery", ", ".join(p.name for p in slots))
    print("zip", zip_src.name)
    print("fill.json", fill["title"], fill["version"], fill["server_side"])
    print("upload", UPLOAD)
    for p in sorted(UPLOAD.iterdir()):
        print(f"  {p.name}\t{p.stat().st_size}")

    if args.ensure_cors:
        if port_open(8765):
            print("cors already on 8765")
        else:
            script = HERE / "SCRIPT-LocalCorsFileServer.py"
            subprocess.Popen(
                [sys.executable, str(script)],
                cwd=str(HERE),
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
            )
            print("cors started on 8765")


if __name__ == "__main__":
    main()
