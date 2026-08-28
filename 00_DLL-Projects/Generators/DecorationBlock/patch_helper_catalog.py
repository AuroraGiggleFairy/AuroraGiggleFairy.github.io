"""Apply batched deco-helper catalog edits without regenerating icons.

CSV is the source of truth. This patches helper_categories.csv, the live canvas
RAW/PARENTS/CHILDREN (no ICONS rewrite), English loc in draft+live, PARENT_ORDER,
and prepends LOOK_RULES so a later generate_decoblocks.py run keeps the mapping.

  python patch_helper_catalog.py ops.json
  python patch_helper_catalog.py ops.json --dry-run
"""
from __future__ import annotations

import argparse
import ast
import csv
import json
import re
from collections import defaultdict
from pathlib import Path

HERE = Path(__file__).resolve().parent
CSV_PATH = HERE / "helper_categories.csv"
DECOBLOCKS = HERE / "generate_decoblocks.py"
GEN_CANVAS = Path(
    r"C:\Users\rft30\.cursor\projects\c-GitHub-7D2D-Mods\agent-tools\generate_deco_helper_canvas.py"
)
CANVAS = Path(
    r"C:\Users\rft30\.cursor\projects\c-GitHub-7D2D-Mods\canvases\deco-helper-block-list.canvas.tsx"
)
LOC_PATHS = (
    Path(r"c:\GitHub\7D2D-Mods\01_Draft\AGF-VP-DecorationBlock-v3.0.3\Config\Localization.csv"),
    Path(
        r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\AGF-VP-DecorationBlock-v3.0.3\Config\Localization.csv"
    ),
)
KIND_SUFFIX_RE = re.compile(r"(\s*\[[0-9a-fA-F]{6}\].*\[-\]\s*)+$")


def pad_sort(n: int) -> str:
    return f"{n:05d}"


def load_csv() -> tuple[list[dict[str, str]], list[str]]:
    with CSV_PATH.open(encoding="utf-8", newline="") as f:
        rows = list(csv.DictReader(f))
        fields = list(rows[0].keys())
    return rows, fields


def save_csv(rows: list[dict[str, str]], fields: list[str]) -> None:
    with CSV_PATH.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(rows)


def by_id(rows: list[dict[str, str]]) -> dict[str, dict[str, str]]:
    return {r["BlockName"]: r for r in rows}


def filtered_view(
    rows: list[dict[str, str]],
    parent: str | None,
    child: str | None,
) -> list[dict[str, str]]:
    out = []
    for r in rows:
        if parent and r["Parent"] != parent:
            continue
        if child and r["Child"] != child:
            continue
        out.append(r)
    out.sort(key=lambda r: (r["SortOrder1"], r["BlockName"]))
    return out


def select_rows(rows: list[dict[str, str]], op: dict) -> list[dict[str, str]]:
    if op.get("ids"):
        idx = by_id(rows)
        missing = [i for i in op["ids"] if i not in idx]
        if missing:
            raise SystemExit(f"unknown ids: {missing[:8]}")
        return [idx[i] for i in op["ids"]]
    parent = op.get("filter_parent")
    child = op.get("filter_child")
    view = filtered_view(rows, parent, child)
    if "from" in op or "to" in op:
        a = int(op.get("from", 1))
        b = int(op.get("to", len(view)))
        if a < 1 or b < a or b > len(view):
            raise SystemExit(
                f"range {a}-{b} out of 1..{len(view)} "
                f"(filter_parent={parent!r} filter_child={child!r})"
            )
        return view[a - 1 : b]
    if op.get("family"):
        needle = op["family"].lower()
        return [r for r in view if needle in r["HelperFamily"].lower()]
    if op.get("name_contains"):
        needle = op["name_contains"].lower()
        return [r for r in view if needle in r["EnglishName"].lower() or needle in r["BlockName"].lower()]
    raise SystemExit(f"op needs ids, from/to, family, or name_contains: {op}")


def extract_js_value(text: str, name: str, start: str) -> tuple[object, int, int]:
    if name == "RAW":
        j = text.rfind("RAW = [")
        if j < 0:
            raise SystemExit("canvas missing RAW = [")
        j = text.find("[", j)
        obj, n = json.JSONDecoder().raw_decode(text[j:])
        return obj, j, j + n
    patterns = (f"const {name}:", f"let {name}:", f"{name} =")
    i = -1
    for p in patterns:
        i = text.find(p)
        if i >= 0:
            break
    if i < 0:
        raise SystemExit(f"canvas missing {name}")
    eq = text.find("=", i)
    if eq < 0:
        raise SystemExit(f"canvas {name} has no =")
    j = text.find(start, eq)
    if j < 0:
        raise SystemExit(f"canvas {name} missing {start}")
    obj, n = json.JSONDecoder().raw_decode(text[j:])
    return obj, j, j + n


def splice(text: str, lo: int, hi: int, value: object) -> str:
    blob = json.dumps(value, ensure_ascii=False, separators=(",", ":"))
    return text[:lo] + blob + text[hi:]


def patch_canvas(
    rows: list[dict[str, str]],
    parents: list[str],
    children: dict[str, list[str]],
    dry: bool,
) -> None:
    text = CANVAS.read_text(encoding="utf-8")
    pobj, plo, phi = extract_js_value(text, "PARENTS", "[")
    text = splice(text, plo, phi, parents)
    cobj, clo, chi = extract_js_value(text, "CHILDREN", "{")
    text = splice(text, clo, chi, children)
    raw, rlo, rhi = extract_js_value(text, "RAW", "[")
    idx = {row[0]: i for i, row in enumerate(raw)}
    miss = 0
    for r in rows:
        i = idx.get(r["BlockName"])
        if i is None:
            miss += 1
            continue
        cell = raw[i]
        # id, name, sort, cat1, cat2, icon, tint
        cell[1] = r["EnglishName"]
        cell[2] = r["SortOrder1"]
        cell[3] = r["Parent"]
        cell[4] = r["Child"]
    text = splice(text, rlo, rhi, raw)
    if miss:
        print(f"canvas RAW miss {miss} ids (csv-only)")
    if dry:
        print(f"canvas would write {CANVAS} ({len(text)} chars)")
        return
    CANVAS.write_text(text, encoding="utf-8")
    print(f"wrote {CANVAS.name}")


def ensure_parent_order(name: str, after: str | None, dry: bool) -> None:
    text = GEN_CANVAS.read_text(encoding="utf-8")
    m = re.search(r"PARENT_ORDER = (\[[\s\S]*?\n\])", text)
    if not m:
        raise SystemExit("PARENT_ORDER missing")
    order = list(ast.literal_eval(m.group(1)))
    if name in order:
        return
    if after and after in order:
        order.insert(order.index(after) + 1, name)
    else:
        order.append(name)
    blob = "[\n    " + ",\n    ".join(f'"{p}"' for p in order) + ",\n]"
    text = text[: m.start(1)] + blob + text[m.end(1) :]
    if dry:
        print(f"PARENT_ORDER would add {name}")
        return
    GEN_CANVAS.write_text(text, encoding="utf-8")
    print(f"PARENT_ORDER += {name}")


def prepend_look_rule(needles: list[str], parent: str, child: str, dry: bool) -> None:
    if not needles:
        return
    text = DECOBLOCKS.read_text(encoding="utf-8")
    marker = "LOOK_RULES: tuple[tuple[tuple[str, ...], str, str], ...] = ("
    i = text.find(marker)
    if i < 0:
        raise SystemExit("LOOK_RULES missing")
    tup = "(" + ", ".join(json.dumps(n) for n in needles) + f'), "{parent}", "{child}"'
    insert = f"{marker}\n    ({tup}),"
    if tup in text:
        print("LOOK_RULES already has this mapping")
        return
    text = text[:i] + insert + text[i + len(marker) :]
    if dry:
        print(f"LOOK_RULES would prepend {needles} -> {parent}/{child}")
        return
    DECOBLOCKS.write_text(text, encoding="utf-8")
    print(f"LOOK_RULES += {needles} -> {parent}/{child}")


def rename_english(row: dict[str, str], new: str) -> str:
    old = row["EnglishName"]
    m = KIND_SUFFIX_RE.search(old)
    suffix = m.group(0) if m else ""
    if KIND_SUFFIX_RE.search(new):
        return new
    return (new.rstrip() + suffix).strip()


def _csv_line(fields: list[str]) -> str:
    out = []
    for i, val in enumerate(fields):
        if i == 7:
            out.append("")
            continue
        if i >= 6:
            out.append('"' + val.replace('"', '""') + '"')
        else:
            out.append(val)
    return ",".join(out)


def patch_loc_english(keys: dict[str, str], dry: bool) -> None:
    if not keys:
        return
    for path in LOC_PATHS:
        if not path.exists():
            print(f"skip loc {path} (missing)")
            continue
        raw = path.read_text(encoding="utf-8").splitlines()
        n = 0
        new_lines = [raw[0]]
        for line in raw[1:]:
            if not line.strip():
                new_lines.append(line)
                continue
            key = line.split(",", 1)[0]
            if key not in keys:
                new_lines.append(line)
                continue
            row = next(csv.reader([line]))
            row[6] = keys[key]
            new_lines.append(_csv_line(row))
            n += 1
        if dry:
            print(f"loc {path.name} would update {n}")
            continue
        path.write_text("\n".join(new_lines) + "\n", encoding="utf-8")
        print(f"loc {path.name} updated {n}")


def rebuild_children(rows: list[dict[str, str]], parents: list[str], extra: dict[str, list[str]]) -> dict[str, list[str]]:
    seen: dict[str, list[str]] = defaultdict(list)
    have: dict[str, set[str]] = defaultdict(set)
    for r in rows:
        p, c = r["Parent"], r["Child"]
        if c not in have[p]:
            have[p].add(c)
            seen[p].append(c)
    out: dict[str, list[str]] = {}
    for p in parents:
        kids = list(extra.get(p, []))
        for c in seen.get(p, []):
            if c not in kids:
                kids.append(c)
        out[p] = kids
    return out


def reorder_by_category(rows: list[dict[str, str]], parents: list[str], children: dict[str, list[str]]) -> None:
    def rank(r: dict[str, str]) -> tuple:
        p = r["Parent"]
        try:
            pi = parents.index(p)
        except ValueError:
            pi = len(parents)
        kids = children.get(p, [])
        try:
            ci = kids.index(r["Child"])
        except ValueError:
            ci = len(kids)
        return (pi, p, ci, r["Child"], r["SortOrder1"], r["BlockName"])

    rows.sort(key=rank)
    for i, r in enumerate(rows, 1):
        r["SortOrder1"] = pad_sort(i)


def apply_ops(ops: list[dict], dry: bool) -> None:
    rows, fields = load_csv()
    text = CANVAS.read_text(encoding="utf-8")
    parents, _, _ = extract_js_value(text, "PARENTS", "[")
    children, _, _ = extract_js_value(text, "CHILDREN", "{")
    parents = list(parents)
    children = {k: list(v) for k, v in children.items()}
    extra_children: dict[str, list[str]] = {k: list(v) for k, v in children.items()}
    loc_updates: dict[str, str] = {}
    do_reorder = False
    touched: list[str] = []

    for op in ops:
        kind = op["op"]
        if kind == "add_parent":
            name = op["name"]
            if name not in parents:
                after = op.get("after")
                if after and after in parents:
                    parents.insert(parents.index(after) + 1, name)
                else:
                    parents.append(name)
            kids = op.get("children") or []
            extra_children.setdefault(name, [])
            for c in kids:
                if c not in extra_children[name]:
                    extra_children[name].append(c)
            ensure_parent_order(name, op.get("after"), dry)
            print(f"parent {name} children={extra_children[name]}")
        elif kind == "add_child":
            p = op["parent"]
            c = op["name"]
            extra_children.setdefault(p, [])
            if c not in extra_children[p]:
                extra_children[p].append(c)
            if p not in parents:
                parents.append(p)
            print(f"child {p}/{c}")
        elif kind == "set_cat":
            target_p = op["parent"]
            target_c = op["child"]
            extra_children.setdefault(target_p, [])
            if target_c not in extra_children[target_p]:
                extra_children[target_p].append(target_c)
            if target_p not in parents:
                raise SystemExit(f"unknown parent {target_p!r}; add_parent first")
            picked = select_rows(rows, op)
            for r in picked:
                r["Parent"] = target_p
                r["Child"] = target_c
                touched.append(r["BlockName"])
            print(f"set_cat {len(picked)} -> {target_p}/{target_c}")
            if op.get("needles"):
                prepend_look_rule(list(op["needles"]), target_p, target_c, dry)
        elif kind == "rename":
            picked = select_rows(rows, op)
            if len(picked) != 1 and not op.get("english"):
                raise SystemExit("rename needs one row or shared english")
            new = op["english"]
            for r in picked:
                r["EnglishName"] = rename_english(r, new)
                loc_updates[r["BlockName"]] = r["EnglishName"]
                touched.append(r["BlockName"])
            print(f"rename {len(picked)} -> {new}")
        elif kind == "reorder_by_category":
            do_reorder = True
        else:
            raise SystemExit(f"unknown op {kind}")

    children = rebuild_children(rows, parents, extra_children)
    if do_reorder:
        print(
            "skip reorder_by_category: that would rewrite SortOrder1; "
            "category grouping is a canvas view only"
        )

    if dry:
        print(f"csv would write {len(rows)} rows; loc keys {len(loc_updates)}")
        patch_canvas(rows, parents, children, dry=True)
        return
    save_csv(rows, fields)
    print(f"wrote {CSV_PATH.name} ({len(rows)} rows)")
    patch_loc_english(loc_updates, dry=False)
    patch_canvas(rows, parents, children, dry=False)


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("ops", type=Path, help="JSON file with {ops: [...]}")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()
    payload = json.loads(args.ops.read_text(encoding="utf-8"))
    ops = payload["ops"] if isinstance(payload, dict) else payload
    apply_ops(ops, dry=args.dry_run)


if __name__ == "__main__":
    main()
