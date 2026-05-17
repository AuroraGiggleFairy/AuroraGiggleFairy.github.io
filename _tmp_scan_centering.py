"""Map the layout coordinates of every zoomed<Cat> view so we can derive the
centering rule.

We want, per category:
  - magazine-strip pos (the 23-icon row at the top)         -> always (5, -10)?
  - title grid pos + cell size                              -> the big book icon
  - main checklist grid pos + cell size + cols              -> the rows of QSections
  - implied total checklist width  = cols * cell_width + (cols-1) * gutter

Then we'll cross-check whether the title and the checklist are centered around
the same x midline inside the Schematics window (width=1390 from the parent
window), accounting for the title block on the left and the checklist on the
right.
"""
from __future__ import annotations
import re
from pathlib import Path

W = Path(r"c:\GitHub\7D2D-Mods\02_ActiveBuild\AGF-HUDPlus-PurpleBook-v2.0.1\Config\XUi\windows.xml")
ALL_CATS = [
    "Robotics","Archery","Pistols","Rifles","MachineGuns","Shotguns",
    "HarvestingTools","Armors","Blades","Clubs","Knuckles","Sledges",
    "Spears","SalvagingTools","RepairTools","Traps","Medical","Vehicles",
    "Workstations","Seeds","Electrician","Explosives","Food",
]

text = W.read_text(encoding="utf-8")
lines = text.splitlines()

re_open = re.compile(r'<rect name="zoomed([A-Za-z]+)"')
re_grid = re.compile(
    r'<grid name="([A-Za-z]+)"\s+(?:controller="[^"]*"\s+)?depth="\d+"\s+rows="(\d+)"\s+cols="(\d+)"\s+pos="(-?\d+,-?\d+)"\s+cell_width="(\d+)"\s+cell_height="(\d+)"'
)

# Build map of zoomed<Cat> -> (start, end) line ranges
zoomed = {}
opens = []
for i, ln in enumerate(lines):
    m = re_open.search(ln)
    if m:
        opens.append((i, m.group(1)))
for idx, (i, cat) in enumerate(opens):
    j = opens[idx+1][0] if idx+1 < len(opens) else len(lines)
    zoomed[cat] = (i, j)

print(f"{'Cat':<16} {'TitleGrid':<22} {'TitleCell':<10} {'MainGrid':<22} {'MainCell':<10} {'Cols':<5} {'TotalW':<7}")
print("-" * 100)
for cat in ALL_CATS:
    if cat not in zoomed:
        print(f"[skip] {cat}")
        continue
    s, e = zoomed[cat]
    title_pos = title_cell = None
    main_pos = main_cell = main_cols = None
    for k in range(s, e):
        m = re_grid.search(lines[k])
        if not m: continue
        gname, rows, cols, pos, cw, ch = m.groups()
        if gname.endswith("Title"):
            title_pos = pos; title_cell = f"{cw}x{ch}"
        elif gname == f"checklist{cat}":
            main_pos = pos; main_cell = f"{cw}x{ch}"; main_cols = cols
    # implied total width = cols * cell_width  (assuming 0 gutter; sub-cells use 2px gutter inside)
    total_w = ""
    if main_cell and main_cols:
        cw_i = int(main_cell.split("x")[0])
        total_w = str(cw_i * int(main_cols))
    print(f"{cat:<16} {str(title_pos):<22} {str(title_cell):<10} {str(main_pos):<22} {str(main_cell):<10} {str(main_cols):<5} {total_w:<7}")
