"""
Rebuild DoorsPlus from current vanilla blocks.xml:
  1) flatten + extract doors
  2) refresh sort CSV for new models
  3) generate AGF variants + helpers
  4) generate Boarded/Plain localization
  5) install into 01_Draft/AGF-VP-DoorsPlus-v3.0.1
"""

from __future__ import annotations

import csv
import re
import subprocess
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
DOORSPLUS = Path(r"c:\GitHub\7D2D-Mods\01_Draft\AGF-VP-DoorsPlus-v3.0.1")
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


def classify_sort_section(name: str) -> int:
    """Map unknown door bases into plan sections 4/5/6."""
    n = name.lower()
    if any(k in n for k in ("garage", "rollup")):
        return 5  # Garages
    if any(k in n for k in ("gate", "fence")):
        return 4  # Gates
    return 6  # Decorative (last)


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
    rows = [r for r in rows if "_Powered" not in (r.get("Name") or "")]
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
        if "_Powered" in base:
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


def convert_legacy_doorsecure(legacy_text: str) -> str:
    """Rewrite legacy append DoorSecure blocks to CompositeTileEntity."""
    import xml.etree.ElementTree as ET
    from door_v31_class import ensure_composite_door

    # Wrap fragments so ElementTree can parse multiple top-level appends.
    wrapped = f"<root>{legacy_text}</root>"
    try:
        root = ET.fromstring(wrapped)
    except ET.ParseError as e:
        raise SystemExit(f"Failed parsing legacy blocks for DoorSecure conversion: {e}") from e

    converted = 0
    for block in root.iter("block"):
        before = None
        for prop in block.findall("property"):
            if prop.get("name") == "Class":
                before = prop.get("value")
                break
        ensure_composite_door(block)
        after = None
        for prop in block.findall("property"):
            if prop.get("name") == "Class":
                after = prop.get("value")
                break
        if before == "DoorSecure" and after != "DoorSecure":
            converted += 1

    # Serialize children only (preserve original root wrappers outside).
    parts: list[str] = []
    for child in list(root):
        parts.append(ET.tostring(child, encoding="unicode"))
    # Also keep any leading non-element text? legacy usually starts with comment/root tag.
    # Our draft starts with <AGF...> then appends — parse may fail if root tag included.
    print(f"Legacy DoorSecure -> CompositeTileEntity conversions: {converted}")
    return "".join(parts)



def apply_spacer_localization(loc_path: Path) -> None:
    """Shared spacer label/desc: single ASCII space in every language cell."""
    import csv
    space = " "
    langs = [
        "english", "german", "spanish", "french", "italian", "japanese", "koreana",
        "polish", "brazilian", "russian", "turkish", "schinese", "tchinese",
    ]
    keys = [
        "miscDoorShapeSpacerAGF",
        "miscDoorShapeSpacerAGFDesc",
    ]
    with loc_path.open(encoding="utf-8", newline="") as f:
        reader = csv.DictReader(f)
        header = list(reader.fieldnames or [])
        rows = [
            r for r in reader
            if not str(r.get("Key", "")).startswith("miscDoorShapeSpacer")
        ]

    def space_row(key: str) -> dict:
        r = {h: "" for h in header}
        r["Key"] = key
        if "File" in r:
            r["File"] = "blocks"
        if "Type" in r:
            r["Type"] = "Block"
        if "Context / Alternate Text" in r:
            r["Context / Alternate Text"] = '""'
        for lang in langs:
            if lang in r:
                r[lang] = space
        return r

    for key in keys:
        rows.append(space_row(key))
    with loc_path.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=header, quoting=csv.QUOTE_MINIMAL)
        w.writeheader()
        for r in rows:
            w.writerow({h: r.get(h, "") for h in header})



def install_into_doorsplus() -> None:
    blocks_path = DOORSPLUS / "Config" / "blocks.xml"
    loc_path = DOORSPLUS / "Config" / "Localization.csv"
    text = blocks_path.read_text(encoding="utf-8")

    # Keep everything before the final AGF append; replace that append.
    matches = list(re.finditer(r'<append xpath="/blocks">', text))
    if len(matches) < 4:
        raise SystemExit(f"Expected >=4 append sections, found {len(matches)}")
    agf_start = matches[-1].start()

    # Find matching closing append for the last one: last </append> before root close
    # Use the final </append> in file.
    closes = [m.start() for m in re.finditer(r"</append>", text)]
    if not closes:
        raise SystemExit("No </append> found")
    agf_end = closes[-1] + len("</append>")

    legacy_raw = text[:agf_start]
    # Split off XML root open so we only convert append bodies + keep wrapper.
    root_open_m = re.match(
        r'(?s)(\s*<AGFVanillaPlus-DoorsPlus>\s*)(.*)$', legacy_raw
    )
    if not root_open_m:
        raise SystemExit("Expected <AGFVanillaPlus-DoorsPlus> root on legacy prefix")
    root_open, legacy_body = root_open_m.group(1), root_open_m.group(2)
    legacy_body = convert_legacy_doorsecure(legacy_body).rstrip() + "\n\n"
    legacy = root_open + legacy_body

    agf_blocks = AGF_ALL.read_text(encoding="utf-8").rstrip() + "\n"
    # Sanity: AGF output must not still emit removed DoorSecure.
    if 'value="DoorSecure"' in agf_blocks or "value='DoorSecure'" in agf_blocks:
        raise SystemExit("AGF output still contains DoorSecure — generator not patched?")

    new_agf = (
        '<append xpath="/blocks">\n'
        + agf_blocks
        + "</append>\n\n\n\n\n\n</AGFVanillaPlus-DoorsPlus>\n"
    )

    # Drop any trailing root after agf_end from legacy rebuild
    out = legacy + new_agf
    blocks_path.write_text(out, encoding="utf-8", newline="\n")
    print(f"Wrote {blocks_path} ({blocks_path.stat().st_size} bytes)")

    loc_path.write_text(LOC_OUT.read_text(encoding="utf-8"), encoding="utf-8", newline="\n")
    apply_spacer_localization(loc_path)
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
    refresh_sort_csv(HERE / "blocks_doorsecure.xml")
    run("generate_doorsecure_agf_all.py")
    run("generate_agf_localization.py")
    install_into_doorsplus()
    summarize()
    print("DONE")


if __name__ == "__main__":
    main()
