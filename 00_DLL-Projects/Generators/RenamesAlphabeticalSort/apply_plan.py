"""Write planned English names + Type from the canvas liveName() into Localization.csv.

  python apply_plan.py
"""
from __future__ import annotations

import csv
import json
import subprocess
import sys
from pathlib import Path

from paths import CANVAS, MOD_LOC, VANILLA_LOC

sys.stdout.reconfigure(encoding="utf-8")

GEN = Path(__file__).resolve().parent
EVAL_TS = GEN / "_eval_live.ts"
DUMP = GEN / "_live_dump.json"
RAW_PREFIX = "const RAW: RawRow[] = "


def extract_raw(canvas: str) -> str:
    start = canvas.find(RAW_PREFIX)
    if start < 0:
        raise SystemExit("canvas missing RAW")
    arr_at = start + len(RAW_PREFIX)
    raw, _ = json.JSONDecoder().raw_decode(canvas[arr_at:])
    return json.dumps(raw, ensure_ascii=False)


def extract_logic(canvas: str) -> str:
    start = canvas.find("const ROW_H")
    end = canvas.find("function PageTitle")
    if start < 0 or end < 0:
        raise SystemExit("canvas missing ROW_H / PageTitle")
    return canvas[start:end].rstrip() + "\n"


def write_eval_ts(canvas: str) -> None:
    types = """
type RawRow = [string, string, string, string, string];
type SortCol = "cat" | "live" | "key" | "vanilla";
type SortState = { col: SortCol; dir: "asc" | "desc" };
type LocPart = { text: string; hex: string | null };

type Row = {
  bucket: string;
  cat: string;
  key: string;
  vanilla: string;
  mod: string;
  i: number;
};
"""
    body = (
        types
        + f"const RAW: RawRow[] = {extract_raw(canvas)};\n"
        + extract_logic(canvas)
        + """
const dump: { key: string; live: string; cat: string; table: string }[] = [];
const seen = new Set<string>();
for (const row of rows()) {
  if (seen.has(row.key)) continue;
  seen.add(row.key);
  dump.push({
    key: row.key,
    live: liveName(row),
    cat: row.cat,
    table: tableName(row),
  });
}
console.log(JSON.stringify(dump));
"""
    )
    EVAL_TS.write_text(body, encoding="utf-8")


NODE = Path(
    r"c:\Users\rft30\AppData\Local\Programs\cursor\resources\app\resources\helpers\node.exe"
)


def run_eval() -> list[dict]:
    r = subprocess.run(
        [str(NODE), "--experimental-strip-types", str(EVAL_TS)],
        cwd=str(GEN),
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if r.returncode != 0:
        sys.stderr.write(r.stdout)
        sys.stderr.write(r.stderr)
        raise SystemExit(f"tsx failed ({r.returncode})")
    text = r.stdout.strip()
    # tsx may print npm notices before JSON
    bracket = text.find("[")
    if bracket < 0:
        raise SystemExit("no JSON dump from tsx")
    return json.loads(text[bracket:])


def loc_rows(path: Path):
    with path.open(encoding="utf-8-sig", newline="") as f:
        r = csv.DictReader(f)
        return list(r.fieldnames or []), list(r)


def main() -> None:
    canvas = CANVAS.read_text(encoding="utf-8")
    write_eval_ts(canvas)
    dump = run_eval()
    DUMP.write_text(json.dumps(dump, ensure_ascii=False, indent=0), encoding="utf-8")

    fields, mod = loc_rows(MOD_LOC)
    _, van = loc_rows(VANILLA_LOC)
    van_map = {r.get("Key"): r for r in van}
    by_key: dict[str, list[dict[str, str]]] = {}
    for row in mod:
        by_key.setdefault(row.get("Key") or "", []).append(row)

    updated = 0
    skipped_admin = 0
    added = 0
    new_rows: list[dict[str, str]] = []
    for item in dump:
        key = item["key"]
        live = item["live"]
        cat = item["cat"]
        existing = by_key.get(key)
        if not existing:
            if item["table"] not in ("Consumable", "Crop", "Biome"):
                continue
            vr = van_map.get(key) or {}
            row = {f: (vr.get(f) or "") for f in fields}
            row["Key"] = key
            row["File"] = vr.get("File") or "items"
            row["Type"] = cat
            row["english"] = live
            new_rows.append(row)
            by_key[key] = [row]
            added += 1
            continue
        for row in existing:
            if (row.get("Type") or "") == "Admin":
                skipped_admin += 1
                continue
            if row.get("english") != live:
                row["english"] = live
            if key.endswith("Bundle") and not key.startswith("ammoBundle"):
                row["Type"] = "Bundle"
            elif key.startswith("ammoBundle"):
                row["Type"] = "Ammo"
            elif item["table"] != "Resource":
                row["Type"] = cat
            updated += 1

    if new_rows:
        insert_at = next(
            (i + 1 for i, r in enumerate(mod) if r.get("Key") == "foodCropYuccaFruit"),
            len(mod),
        )
        mod[insert_at:insert_at] = new_rows

    with MOD_LOC.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields, lineterminator="\n")
        w.writeheader()
        w.writerows(mod)

    EVAL_TS.unlink(missing_ok=True)
    print(f"updated {updated}  added {added}  skipped_admin {skipped_admin}  dump {len(dump)}")


if __name__ == "__main__":
    main()
