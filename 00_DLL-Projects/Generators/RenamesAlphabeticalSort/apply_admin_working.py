"""Set visible [FF5555][Admin][-] on Dev/Admin names, add missing Dev keys, write buckets.csv."""
from __future__ import annotations

import csv
import re
import sys

from paths import (
    BUCKETS,
    ITEM_MODIFIERS_XML,
    ITEMS_XML,
    MOD_LOC,
    VANILLA_LOC,
)

sys.stdout.reconfigure(encoding="utf-8")

LANGS = [
    "english",
    "german",
    "spanish",
    "french",
    "italian",
    "japanese",
    "koreana",
    "polish",
    "brazilian",
    "russian",
    "turkish",
    "schinese",
    "tchinese",
]
ADMIN_WORD = {
    "english": "Admin",
    "german": "Administrator",
    "spanish": "Admin",
    "french": "Admin",
    "italian": "Amministratore",
    "japanese": "管理",
    "koreana": "관리",
    "polish": "Administrator",
    "brazilian": "Administrador",
    "russian": "Администратор",
    "turkish": "Yönetici",
    "schinese": "管理员",
    "tchinese": "管理員",
}
LEAD_RE = re.compile(
    r"^(?:"
    r"\[[0-9A-Fa-f]{6}\]\[[^\]]+\]\[-\]\s*"
    r"|\[\[[0-9A-Fa-f]{6}\][^\[]*\[-\]\]\s*"
    r"|\[[0-9A-Fa-f]{6}\][^\[]*\[-\]\s*"
    r")+"
)


def is_name_key(k: str) -> bool:
    if not k:
        return False
    if k.endswith(("Desc", "Tooltip", "PromptTitle")):
        return False
    return True


def parse_entries(path, tag: str):
    text = path.read_text(encoding="utf-8")
    entries = []
    for m in re.finditer(rf"<{tag}\s+name=\"([^\"]+)\"[^>]*>", text):
        name = m.group(1)
        close = f"</{tag}>"
        end = text.find(close, m.end())
        body = text[m.start() : end] if end > 0 else text[m.start() : m.start() + 4000]
        extends = None
        em = re.search(r'name="Extends"\s+value="([^"]+)"', body)
        if em:
            extends = em.group(1)
        cm = None
        for cm_m in re.finditer(
            r'<property name="CreativeMode" value="([^"]+)"\s*/>', body
        ):
            line_start = body.rfind("\n", 0, cm_m.start()) + 1
            line = body[line_start : body.find("\n", cm_m.start())]
            if "<!--" in line:
                continue
            cm = cm_m.group(1)
        entries.append({"name": name, "extends": extends, "cm": cm})
    return entries


def resolve_cm(entries):
    by = {e["name"]: e for e in entries}
    out: dict[str, str | None] = {}

    def cm_of(name, seen=None):
        if name in out:
            return out[name]
        e = by.get(name)
        if not e:
            return None
        if e["cm"]:
            out[name] = e["cm"]
            return e["cm"]
        if e["extends"] and e["extends"] not in (seen or set()):
            s = set(seen or ())
            s.add(name)
            val = cm_of(e["extends"], s)
            out[name] = val
            return val
        out[name] = None
        return None

    for e in entries:
        cm_of(e["name"])
    return out


def loc_rows(path):
    with path.open(encoding="utf-8-sig", newline="") as f:
        r = csv.DictReader(f)
        return r.fieldnames, list(r)


def strip_lead(s: str) -> str:
    return LEAD_RE.sub("", s or "", count=1)


def apply_prefix(lang: str, text: str, vr: dict[str, str], key: str) -> str:
    if not (text or "").strip():
        return text or ""
    rest = strip_lead(text).strip()
    if rest.lower() in ("0", "n/a"):
        rest = (
            (vr.get(lang) or "").strip()
            or (vr.get("english") or "").strip()
            or key
        )
    word = ADMIN_WORD[lang]
    return f"[FF5555][{word}][-] {rest}"


def working_keys(mod, van_map, item_cm, mod_cm) -> list[str]:
    keys: list[str] = []
    seen: set[str] = set()

    def add(k: str):
        if k and k not in seen and is_name_key(k):
            seen.add(k)
            keys.append(k)

    for r in mod:
        k = r.get("Key") or ""
        en = r.get("english") or ""
        if (r.get("Type") or "") == "Admin" or "Mod-Admin" in en or "[ff0000]Admin" in en:
            add(k)
    for name, cm in {**item_cm, **mod_cm}.items():
        if cm == "Dev" and (
            name in {r.get("Key") for r in mod}
            or (van_map.get(name) or {}).get("english")
        ):
            add(name)
    return keys


def main() -> None:
    fields, mod = loc_rows(MOD_LOC)
    _, van = loc_rows(VANILLA_LOC)
    van_map = {r["Key"]: r for r in van}
    item_cm = resolve_cm(parse_entries(ITEMS_XML, "item"))
    modifier_cm = resolve_cm(parse_entries(ITEM_MODIFIERS_XML, "item_modifier"))
    keys = working_keys(mod, van_map, item_cm, modifier_cm)
    file_of = {
        n: "items"
        for n, cm in item_cm.items()
        if cm == "Dev"
    }
    file_of.update(
        {n: "item_modifiers" for n, cm in modifier_cm.items() if cm == "Dev"}
    )

    by_key: dict[str, list[dict[str, str]]] = {}
    for r in mod:
        by_key.setdefault(r.get("Key") or "", []).append(r)

    added = 0
    insert_at = next(
        (i + 1 for i, r in enumerate(mod) if r.get("Key") == "meleeToolBlockReplaceTool"),
        len(mod),
    )
    new_rows: list[dict[str, str]] = []
    for k in keys:
        existing = by_key.get(k)
        vr = van_map.get(k) or {}
        if existing:
            for row in existing:
                row["Type"] = "Admin"
                if not (row.get("File") or "").strip():
                    row["File"] = vr.get("File") or file_of.get(k) or row.get("File") or ""
                for lang in LANGS:
                    row[lang] = apply_prefix(lang, row.get(lang) or "", vr, k)
            continue
        if not (vr.get("english") or "").strip():
            continue
        row = {f: "" for f in fields}
        row["Key"] = k
        row["File"] = vr.get("File") or file_of.get(k) or "items"
        row["Type"] = "Admin"
        for lang in LANGS:
            src = vr.get(lang) or ""
            if src.strip():
                row[lang] = apply_prefix(lang, src, vr, k)
            elif lang == "english" and (vr.get("english") or "").strip():
                row[lang] = apply_prefix(lang, vr["english"], vr, k)
        new_rows.append(row)
        added += 1

    if new_rows:
        mod[insert_at:insert_at] = new_rows

    with MOD_LOC.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields, lineterminator="\n")
        w.writeheader()
        w.writerows(mod)

    with BUCKETS.open("w", encoding="utf-8", newline="") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["key", "bucket"])
        for k in keys:
            w.writerow([k, "working"])

    print(f"working keys {len(keys)}  added loc rows {added}  buckets {BUCKETS.name}")
    for k in keys:
        print(f"  {k}")


if __name__ == "__main__":
    main()
