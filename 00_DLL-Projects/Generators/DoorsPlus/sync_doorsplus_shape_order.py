"""
Sync editable shape-order markdown → doorsecure_sortTypeSummary.csv
and COMPACT_SECTIONS / BOARD_PLAIN_SECTIONS in generate_doorsecure_agf_all.py.

Edit: 00_Support/WorkspaceData/DoorsPlus-ShapeMenu-Order.md
Then run this (or rebuild_doorsplus_from_vanilla.py, which calls it).
"""

from __future__ import annotations

import csv
import re
from pathlib import Path

HERE = Path(__file__).resolve().parent
ORDER_MD = Path(r"c:\GitHub\7D2D-Mods\00_Support\WorkspaceData\DoorsPlus-ShapeMenu-Order.md")
SORT_CSV = HERE / "doorsecure_sortTypeSummary.csv"
GENERATE_PY = HERE / "generate_doorsecure_agf_all.py"

SECTION_RE = re.compile(
    r"^##\s+Section\s+(\d+)\s*\|",
    re.MULTILINE,
)
COMPACT_RE = re.compile(
    r"##\s+Compact sections\s*\n.*?```\s*\n([0-9,\s]+)\n```",
    re.DOTALL,
)
BOARD_PLAIN_RE = re.compile(
    r"##\s+Boarded-then-Plain sections\s*\n.*?```\s*\n([0-9,\s]*)\n```",
    re.DOTALL,
)
BLANK_ROW_RE = re.compile(
    r"##\s+Full blank rows before sections\s*\n.*?```\s*\n([0-9,\s]*)\n```",
    re.DOTALL,
)
FENCE_RE = re.compile(r"```\s*\n(.*?)```", re.DOTALL)


def _parse_int_set(raw: str) -> set[int]:
    return {int(x.strip()) for x in raw.split(",") if x.strip()}


def parse_order_md(
    text: str,
) -> tuple[set[int], set[int], set[int], list[tuple[int, list[str]]]]:
    compact_m = COMPACT_RE.search(text)
    if not compact_m:
        raise SystemExit("Missing ## Compact sections fenced list (e.g. ```\\n1, 3, 4\\n```)")
    compact = _parse_int_set(compact_m.group(1))

    board_m = BOARD_PLAIN_RE.search(text)
    board_plain = _parse_int_set(board_m.group(1)) if board_m else set()

    blank_m = BLANK_ROW_RE.search(text)
    blank_before = _parse_int_set(blank_m.group(1)) if blank_m else set()

    overlap = compact & board_plain
    if overlap:
        raise SystemExit(f"Sections cannot be both Compact and Boarded-then-Plain: {sorted(overlap)}")

    # Ignore scratch notes
    cut = re.search(r"^##\s+Notes\s*/\s*scratch", text, re.MULTILINE | re.IGNORECASE)
    if cut:
        text = text[: cut.start()]

    sections: list[tuple[int, list[str]]] = []
    matches = list(SECTION_RE.finditer(text))
    if not matches:
        raise SystemExit("No ## Section N | ... headings found")

    for i, m in enumerate(matches):
        sec = int(m.group(1))
        start = m.end()
        end = matches[i + 1].start() if i + 1 < len(matches) else len(text)
        body = text[start:end]
        fence = FENCE_RE.search(body)
        if not fence:
            raise SystemExit(f"Section {sec}: expected a ``` fenced name list")
        names: list[str] = []
        seen: set[str] = set()
        for line in fence.group(1).splitlines():
            name = line.strip()
            if not name or name.startswith("#"):
                continue
            if name.startswith("`") and name.endswith("`"):
                name = name[1:-1].strip()
            if name in seen:
                raise SystemExit(f"Duplicate model in section {sec}: {name}")
            seen.add(name)
            names.append(name)
        if not names:
            raise SystemExit(f"Section {sec}: empty name list")
        sections.append((sec, names))

    sections.sort(key=lambda t: t[0])
    all_names = [n for _, ns in sections for n in ns]
    if len(all_names) != len(set(all_names)):
        raise SystemExit("Duplicate model name across sections")
    return compact, board_plain, blank_before, sections


def write_csv(sections: list[tuple[int, list[str]]]) -> None:
    rows = []
    for sec, names in sections:
        for i, name in enumerate(names, start=1):
            rows.append({"Name": name, "Sort Step 1": str(sec), "Sort Step 2": str(i)})
    with SORT_CSV.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=["Name", "Sort Step 1", "Sort Step 2"])
        w.writeheader()
        w.writerows(rows)
    print(f"Wrote {SORT_CSV} ({len(rows)} models)")


def _write_set_const(name: str, values: set[int]) -> None:
    text = GENERATE_PY.read_text(encoding="utf-8")
    new_lit = "{" + ", ".join(str(x) for x in sorted(values)) + "}"
    updated, n = re.subn(
        rf"^{name}\s*=\s*\{{[^}}]*\}}",
        f"{name} = {new_lit}",
        text,
        count=1,
        flags=re.MULTILINE,
    )
    if n != 1:
        raise SystemExit(f"Could not update {name} in generate_doorsecure_agf_all.py")
    GENERATE_PY.write_text(updated, encoding="utf-8", newline="\n")
    print(f"{name} = {new_lit}")


def main() -> None:
    if not ORDER_MD.exists():
        raise SystemExit(f"Missing order file: {ORDER_MD}")
    compact, board_plain, blank_before, sections = parse_order_md(
        ORDER_MD.read_text(encoding="utf-8")
    )
    write_csv(sections)
    _write_set_const("COMPACT_SECTIONS", compact)
    _write_set_const("BOARD_PLAIN_SECTIONS", board_plain)
    _write_set_const("BLANK_ROW_BEFORE_SECTIONS", blank_before)
    print("Sections:")
    for sec, names in sections:
        marks = []
        if sec in compact:
            marks.append("compact")
        if sec in board_plain:
            marks.append("boarded→plain")
        if sec in blank_before:
            marks.append("blank-row-before")
        suffix = f" ({', '.join(marks)})" if marks else ""
        print(f"  {sec}{suffix}: {len(names)} models")
    print("DONE sync")


if __name__ == "__main__":
    main()
