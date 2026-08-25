"""
Rebuild DoorsPlus from current vanilla blocks.xml:
  1) flatten + extract doors
  2) refresh sort CSV for new models
  3) generate AGF variants + helpers
  4) generate Boarded/Plain localization
  5) install into 02_ActiveBuild/AGF-VP-DoorsPlus-v4.0.0
"""

from __future__ import annotations

import csv
import re
import subprocess
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
DOORSPLUS = Path(r"c:\GitHub\7D2D-Mods\02_ActiveBuild\AGF-VP-DoorsPlus-v4.0.0")
SORT_CSV = HERE / "doorsecure_sortTypeSummary.csv"
AGF_ALL = HERE / "blocks_doorsecure_agf_all.xml"
LOC_OUT = HERE / "Localization.csv"

COLORS = [
    "ArmyGreen", "Blue", "Brown", "Green", "Grey", "Orange", "Pink",
    "Purple", "Red", "White", "Yellow", "Oak", "Black",
]


def run(script: str) -> None:
    print(f"==> {script}")
    subprocess.check_call([sys.executable, script], cwd=HERE)


def read_text_auto(path: Path) -> str:
    raw = path.read_bytes()
    if raw.startswith((b"\xff\xfe", b"\xfe\xff")) or raw.count(b"\x00") > max(100, len(raw)//10):
        return raw.decode("utf-16")
    try:
        return raw.decode("utf-8")
    except UnicodeDecodeError:
        return raw.decode("utf-16")


def white_base(name: str) -> str:
    for color in sorted(COLORS, key=len, reverse=True):
        if name.endswith(color):
            return name[: -len(color)] + "White"
    return name


def is_porta_potty_unit_name(name: str) -> bool:
    """Full porta-potty cabinets — keep only portaPottyDoor*."""
    n = (name or "").lower()
    if "portapottydoor" in n:
        return False
    return "portapotty" in n


def classify_sort_section(name: str) -> int:
    """Map unknown door bases into plan sections (see ShapeMenu-Order.md)."""
    n = name.lower()
    if "rollup" in n:
        return 7
    if "garage" in n:
        return 6
    if "woodenfence" in n:
        return 4
    if any(k in n for k in ("gate", "chainlink")) or (
        "fence" in n and "woodenfence" not in n
    ):
        return 5
    if "bathroomstall" in n:
        return 15
    if "portapotty" in n:
        return 16
    if any(k in n for k in ("jail", "elevator", "trailer")):
        return 14
    # Screen doors (not *NoScreen* sliding) share glass row
    if ("screen" in n and "noscreen" not in n) or "glass" in n:
        return 9
    if "sliding" in n:
        return 10
    if "commercial" in n:
        return 13
    if any(k in n for k in ("closet", "pantry", "armoire", "tallcabinet")):
        return 12
    if "interior" in n:
        return 11
    if any(k in n for k in ("exterior", "french")):
        return 8
    return 8


def refresh_sort_csv(door_xml: Path) -> None:
    import xml.etree.ElementTree as ET

    root = ET.parse(door_xml).getroot()
    door_names = [b.get("name") for b in root.findall("block") if b.get("name")]
    bases = sorted({white_base(n) for n in door_names})

    existing = {}
    rows = []
    if SORT_CSV.exists():
        sort_text = read_text_auto(SORT_CSV).lstrip("\ufeff")
        reader = csv.DictReader(sort_text.splitlines())
        for row in reader:
            existing[row["Name"]] = row
            rows.append(row)

    # Max Sort Step 2 per section (1..6)
    max_step2: dict[int, int] = {}
    for row in rows:
        try:
            s1 = int(row["Sort Step 1"])
            s2 = int(row["Sort Step 2"])
            max_step2[s1] = max(max_step2.get(s1, 0), s2)
        except Exception:
            pass

    # Drop vanilla powered twins (AGF Powered tier covers them).
    # Drop full porta-potty cabinets (doors only).
    rows = [
        r
        for r in rows
        if "_Powered" not in (r.get("Name") or "")
        and not is_porta_potty_unit_name(r.get("Name") or "")
    ]
    existing = {r["Name"]: r for r in rows}
    max_step2 = {}
    for row in rows:
        try:
            s1 = int(row["Sort Step 1"])
            s2 = int(row["Sort Step 2"])
            max_step2[s1] = max(max_step2.get(s1, 0), s2)
        except Exception:
            pass

    added = []
    for base in bases:
        if "_Powered" in base or is_porta_potty_unit_name(base):
            continue
        if base in existing:
            continue
        section = classify_sort_section(base)
        max_step2[section] = max_step2.get(section, 0) + 1
        row = {
            "Name": base,
            "Sort Step 1": str(section),
            "Sort Step 2": str(max_step2[section]),
        }
        rows.append(row)
        existing[base] = row
        added.append(f"{base}(s{section})")

    rows.sort(
        key=lambda r: (
            int(r.get("Sort Step 1") or 99),
            int(r.get("Sort Step 2") or 999),
            r.get("Name") or "",
        )
    )

    with SORT_CSV.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=["Name", "Sort Step 1", "Sort Step 2"])
        writer.writeheader()
        writer.writerows(rows)

    print(f"Sort CSV doors: {len(bases)}; newly added: {len(added)}")
    if added:
        print("  new:", ", ".join(added[:30]), ("..." if len(added) > 30 else ""))


def apply_spacer_localization(loc_path: Path, blocks_path: Path | None = None) -> None:
    """One loc row per spacer block name; every language cell is a quoted ASCII space.

    Required CSV form (14 text columns after KeepLoaded, including Context):
      key,blocks,Block,,,," "," "," "," "," "," "," "," "," "," "," "," "," "," "

    Shape UI falls back to the raw block name when Key != block name, so shared
    keys alone are not enough — each miscDoorShapeSpacer* needs its own row.
    """
    spacer_names: list[str] = []
    src = blocks_path if blocks_path and blocks_path.exists() else DOORSPLUS / "Config" / "blocks.xml"
    if src.exists():
        spacer_names = sorted(
            set(re.findall(r'<block name="(miscDoorShapeSpacer[^"]+)"', src.read_text(encoding="utf-8")))
        )

    keys = list(spacer_names)
    # Shared description key referenced by every spacer block.
    if "miscDoorShapeSpacerAGFDesc" not in keys:
        keys.append("miscDoorShapeSpacerAGFDesc")

    # english + Context / Alternate Text + 12 other languages = 14 quoted spaces
    quoted_spaces = ",".join(['" "'] * 14)

    def spacer_line(key: str) -> str:
        return f"{key},blocks,Block,,,,{quoted_spaces}"

    text = loc_path.read_text(encoding="utf-8")
    lines = text.splitlines()
    if not lines:
        raise RuntimeError(f"Empty localization file: {loc_path}")
    kept = [ln for ln in lines if not ln.startswith("miscDoorShapeSpacer")]
    # Drop trailing blank lines so append is clean
    while kept and not kept[-1].strip():
        kept.pop()
    kept.extend(spacer_line(k) for k in keys)
    loc_path.write_text("\n".join(kept) + "\n", encoding="utf-8")
    print(f"Spacer localization: {len(spacer_names)} block keys + desc -> {loc_path}")



def install_into_doorsplus() -> None:
    blocks_path = DOORSPLUS / "Config" / "blocks.xml"
    loc_path = DOORSPLUS / "Config" / "Localization.csv"

    agf_blocks = AGF_ALL.read_text(encoding="utf-8").rstrip() + "\n"
    # Sanity: AGF output must not still emit removed DoorSecure.
    if 'value="DoorSecure"' in agf_blocks or "value='DoorSecure'" in agf_blocks:
        raise SystemExit("AGF output still contains DoorSecure — generator not patched?")

    # Current All Doors append only. Do not keep leftover helper or AGF* sections.
    out = (
        "<AGFVanillaPlus-DoorsPlus>\n\n"
        '<append xpath="/blocks">\n'
        + agf_blocks
        + "</append>\n\n\n\n\n\n</AGFVanillaPlus-DoorsPlus>\n"
    )
    blocks_path.write_text(out, encoding="utf-8", newline="\n")
    print(f"Wrote {blocks_path} ({blocks_path.stat().st_size} bytes)")

    loc_path.write_text(LOC_OUT.read_text(encoding="utf-8"), encoding="utf-8", newline="\n")
    apply_spacer_localization(loc_path, blocks_path)
    print(f"Wrote {loc_path} ({loc_path.stat().st_size} bytes)")


def summarize() -> None:
    import xml.etree.ElementTree as ET

    # Count AGF blocks from generated file
    wrapped = "<blocks>\n" + AGF_ALL.read_text(encoding="utf-8") + "\n</blocks>"
    root = ET.fromstring(wrapped)
    names = [b.get("name") for b in root.findall("block") if b.get("name")]
    helpers = [n for n in names if "VariantHelperAGF" in n]
    tiers = {
        t: sum(1 for n in names if re.search(rf"AGF{t}(Clean)?$", n or ""))
        for t in ("Wood", "Iron", "Steel", "Powered")
    }
    print("AGF total blocks", len(names), "helpers", helpers)
    print("tier counts", tiers)

    loc_lines = LOC_OUT.read_text(encoding="utf-8").splitlines()
    boarded = sum(1 for ln in loc_lines if "Boarded" in ln)
    plain = sum(1 for ln in loc_lines if "[decea3]Plain[-]" in ln or "Plain[-]" in ln)
    print(f"Localization rows {len(loc_lines)-1}; english Boarded marks ~{boarded}; Plain marks ~{plain}")


def main() -> None:
    run("flatten_blocks_doorsecure.py")
    # Editable order doc → CSV + COMPACT_SECTIONS, then append any brand-new models.
    run("sync_doorsplus_shape_order.py")
    refresh_sort_csv(HERE / "blocks_doorsecure.xml")
    run("generate_doorsecure_agf_all.py")
    run("generate_agf_localization.py")
    install_into_doorsplus()
    summarize()
    print("DONE")


if __name__ == "__main__":
    main()
