"""Refresh canvas RAW + catalog.csv from Localization.csv. Does not rewrite canvas UI.

  python sync_canvas.py
"""
from __future__ import annotations

import csv
import json

from paths import BUCKETS, CATALOG, CANVAS, MOD_LOC, TYPE_DISPLAY, VANILLA_LOC

RAW_PREFIX = "const RAW: RawRow[] = "


def vanilla_english() -> dict[str, str]:
    out: dict[str, str] = {}
    with VANILLA_LOC.open(encoding="utf-8-sig", newline="") as f:
        for r in csv.DictReader(f):
            k = (r.get("Key") or "").strip()
            if k:
                out[k] = r.get("english") or ""
    return out


def load_buckets() -> dict[str, str]:
    if not BUCKETS.exists():
        return {}
    out: dict[str, str] = {}
    with BUCKETS.open(encoding="utf-8", newline="") as f:
        for r in csv.DictReader(f):
            k = (r.get("key") or "").strip()
            b = (r.get("bucket") or "").strip()
            if k and b:
                out[k] = b
    return out


def build_catalog_and_raw() -> tuple[list[list[str]], list[list[str]]]:
    van = vanilla_english()
    buckets = load_buckets()
    catalog: list[list[str]] = []
    raw: list[list[str]] = []
    seen_raw: set[str] = set()
    with MOD_LOC.open(encoding="utf-8-sig", newline="") as f:
        for r in csv.DictReader(f):
            k = (r.get("Key") or "").strip()
            if not k:
                continue
            cat = TYPE_DISPLAY.get(r.get("Type") or "", r.get("Type") or "")
            bucket = buckets.get(k, "")
            catalog.append([bucket, cat, k, van.get(k, ""), r.get("english") or ""])
            if k not in seen_raw:
                seen_raw.add(k)
                raw.append([bucket, cat, k, van.get(k, ""), r.get("english") or ""])
    return catalog, raw


def write_catalog(rows: list[list[str]]) -> None:
    with CATALOG.open("w", encoding="utf-8", newline="") as f:
        w = csv.writer(f)
        w.writerow(["bucket", "category", "key", "vanilla", "mod"])
        w.writerows(rows)


def inject_canvas(rows: list[list[str]]) -> None:
    text = CANVAS.read_text(encoding="utf-8")
    start = text.find(RAW_PREFIX)
    if start < 0:
        raise SystemExit("canvas missing RAW_PREFIX")
    arr_at = start + len(RAW_PREFIX)
    if text[arr_at] != "[":
        raise SystemExit("canvas RAW is not a JSON array")
    _, end_rel = json.JSONDecoder().raw_decode(text[arr_at:])
    end = arr_at + end_rel
    if text[end : end + 1] != ";":
        raise SystemExit("canvas RAW not followed by semicolon")
    payload = RAW_PREFIX + json.dumps(rows, ensure_ascii=False) + ";"
    CANVAS.write_text(text[:start] + payload + text[end + 1 :], encoding="utf-8")


def main() -> None:
    catalog, raw = build_catalog_and_raw()
    write_catalog(catalog)
    inject_canvas(raw)
    print(f"synced catalog {len(catalog)}  canvas RAW {len(raw)} -> {CATALOG.name}")


if __name__ == "__main__":
    main()
