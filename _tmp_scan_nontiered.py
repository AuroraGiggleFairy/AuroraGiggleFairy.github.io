"""One-shot scan: enumerate every QSection in the non-tiered zoomed categories.

Reports per QSection:
  - outer-grid cell width/height the section sits in
  - subcell widths (backgroundSection sprites inside this entry)
  - the cvar threshold each filledsprite gates on (and its width)
  - the threshold labels rendered (Q-chip "text=N")
  - icon count
"""
from __future__ import annotations
import re
from pathlib import Path

W = Path(r"c:\GitHub\7D2D-Mods\02_ActiveBuild\AGF-HUDPlus-PurpleBook-v2.0.1\Config\XUi\windows.xml")
NON_TIERED = ["Seeds", "Food", "Workstations", "Medical", "Traps",
              "Vehicles", "Electrician", "Explosives", "RepairTools"]

text = W.read_text(encoding="utf-8")
lines = text.splitlines()

def find_zoomed_block(cat: str) -> tuple[int, int]:
    """Return (start_idx, end_idx) of zoomed<Cat> rect, half-open, 0-based."""
    open_re = re.compile(rf'<rect name="zoomed{cat}"')
    any_zoomed_re = re.compile(r'<rect name="zoomed[A-Za-z]+"')
    start = None
    for i, ln in enumerate(lines):
        if open_re.search(ln):
            start = i
            break
    if start is None:
        return (-1, -1)
    end = len(lines)
    for j in range(start + 1, len(lines)):
        if any_zoomed_re.search(lines[j]):
            end = j
            break
    return (start, end)

# Patterns
re_grid_check = re.compile(r'<grid name="checklist([A-Za-z]+)"\s+depth="\d+"\s+rows="(\d+)"\s+cols="(\d+)"\s+pos="(-?\d+,-?\d+)"\s+cell_width="(\d+)"\s+cell_height="(\d+)"')
re_entry      = re.compile(r'<entry name="(QSection[A-Za-z]*)"')
re_close      = re.compile(r'</entry>')
re_bg         = re.compile(r'name="backgroundSection".*?width="(\d+)"\s+height="(\d+)"')
re_bar        = re.compile(r'name="yesUnlocked".*?width="(\d+)".*?fill="\{cvar\(crafting\w+Check(\d+)\)\}"')
re_label      = re.compile(r'name="checkunlock".*?text="(\d+)"')
re_icon       = re.compile(r'name="itemIcon".*?pos="(-?\d+,-?\d+)"')

for cat in NON_TIERED:
    s, e = find_zoomed_block(cat)
    if s < 0:
        print(f"[skip] {cat}: zoomed block not found")
        continue
    print(f"\n===== {cat} (lines {s+1}..{e}) =====")
    # outer checklist grid
    for i in range(s, e):
        m = re_grid_check.search(lines[i])
        if m and m.group(1) == cat:
            print(f"  grid: rows={m.group(2)} cols={m.group(3)} pos={m.group(4)} cell={m.group(5)}x{m.group(6)}")
            break
    # entries
    in_entry = False
    cur_name = None
    bgs: list[str] = []
    bars: list[str] = []
    labels: list[str] = []
    icons: list[str] = []
    for i in range(s, e):
        ln = lines[i]
        m = re_entry.search(ln)
        if m:
            in_entry = True
            cur_name = m.group(1)
            bgs, bars, labels, icons = [], [], [], []
            continue
        if in_entry and re_close.search(ln):
            print(f"    {cur_name:<15} bgs=[{','.join(bgs)}] bars=[{','.join(bars)}] labels=[{','.join(labels)}] icons={len(icons)}@{icons}")
            in_entry = False
            cur_name = None
            continue
        if in_entry:
            m = re_bg.search(ln)
            if m:
                bgs.append(f"{m.group(1)}x{m.group(2)}")
                continue
            m = re_bar.search(ln)
            if m:
                bars.append(f"{m.group(1)}px@Check{m.group(2)}")
                continue
            m = re_label.search(ln)
            if m:
                labels.append(m.group(1))
                continue
            m = re_icon.search(ln)
            if m:
                icons.append(m.group(1))
