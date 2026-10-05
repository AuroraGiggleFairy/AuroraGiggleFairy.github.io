"""Print loc + vanilla XML snippet for one or more keys.

  python lookup.py foodBaconAndEggs modArmorBandolier plantedCorn1
"""
from __future__ import annotations

import csv
import re
import sys
from pathlib import Path

from paths import (
    BLOCKS_XML,
    CATALOG,
    ITEM_MODIFIERS_XML,
    ITEMS_XML,
    MOD_LOC,
    VANILLA_LOC,
)

XML_BY_FILE = {
    "items": ITEMS_XML,
    "weapon": ITEMS_XML,
    "blocks": BLOCKS_XML,
    "item_modifiers": ITEM_MODIFIERS_XML,
}
OPEN_TAG = {
    "items": "item",
    "weapon": "item",
    "blocks": "block",
    "item_modifiers": "item_modifier",
}


def loc_index(path: Path) -> dict[str, dict[str, str]]:
    with path.open(encoding="utf-8-sig", newline="") as f:
        return {(r.get("Key") or ""): r for r in csv.DictReader(f)}


def catalog_row(key: str) -> dict[str, str] | None:
    if not CATALOG.exists():
        return None
    with CATALOG.open(encoding="utf-8", newline="") as f:
        for r in csv.DictReader(f):
            if r.get("key") == key:
                return r
    return None


def xml_snippet(xml_path: Path, tag: str, key: str, max_chars: int = 2500) -> str:
    text = xml_path.read_text(encoding="utf-8")
    m = re.search(rf"<{tag}\s+name=\"{re.escape(key)}\"[^>]*>", text)
    if not m:
        return ""
    start = m.start()
    close = f"</{tag}>"
    end = text.find(close, m.end())
    if end < 0:
        return text[start : start + max_chars]
    chunk = text[start : end + len(close)]
    if len(chunk) > max_chars:
        return chunk[:max_chars] + "\n..."
    return chunk


def show(key: str, mod: dict[str, dict[str, str]], van: dict[str, dict[str, str]]) -> None:
    mr = mod.get(key)
    vr = van.get(key)
    cr = catalog_row(key)
    print("=" * 60)
    print(key)
    if cr:
        print(f"  category  {cr.get('category')}")
        print(f"  vanilla   {cr.get('vanilla')}")
        print(f"  mod       {cr.get('mod')}")
    if mr:
        print(f"  loc File  {mr.get('File')}  Type={mr.get('Type')}")
    if vr:
        print(f"  vanilla   {vr.get('english')}")
    loc_file = (mr or {}).get("File") or (vr or {}).get("File") or ""
    xml_path = XML_BY_FILE.get(loc_file)
    tag = OPEN_TAG.get(loc_file)
    if xml_path and tag:
        snip = xml_snippet(xml_path, tag, key)
        print(f"  xml       {xml_path.name} <{tag}>")
        print(snip or "  (no matching element)")
    print()


def main() -> None:
    keys = sys.argv[1:]
    if not keys:
        raise SystemExit("usage: python lookup.py KEY [KEY...]")
    mod = loc_index(MOD_LOC)
    van = loc_index(VANILLA_LOC)
    for k in keys:
        show(k, mod, van)


if __name__ == "__main__":
    main()
