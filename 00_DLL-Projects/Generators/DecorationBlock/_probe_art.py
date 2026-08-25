from __future__ import annotations

import re
from pathlib import Path

BLOCKS = Path(
    r"c:\GitHub\7D2D-Mods\01_Draft\AGF-VP-DecorationBlock-v3.0.3\Config\blocks.xml"
)
text = BLOCKS.read_text(encoding="utf-8")
names = [
    n.removeprefix("agfDeco")
    for n in re.search(r'name="PlaceAltBlockValue" value="([^"]+)"', text).group(1).split(",")
]


def dump(label, pred):
    idxs = [i for i, n in enumerate(names) if pred(n)]
    runs = 1 + sum(1 for a, b in zip(idxs, idxs[1:]) if b != a + 1) if idxs else 0
    print(f"\n== {label} n={len(idxs)} runs={runs}")
    if not idxs:
        return
    prev = None
    for i in idxs:
        gap = "" if prev is None or i == prev + 1 else f"  [gap {i - prev - 1}]"
        print(f"  {i+1:4d} {names[i]}{gap}")
        prev = i


nl = lambda n: n.lower()
dump(
    "wall art (name contains)",
    lambda n: any(
        k in nl(n)
        for k in (
            "paint",
            "picture",
            "poster",
            "canvas",
            "frame",
            "calendar",
            "bulletin",
        )
    ),
)

print("\n--- neighbors around first painting/picture/poster ---")
hits = [
    i
    for i, n in enumerate(names)
    if n.startswith(("painting", "picture", "poster", "targetPoster"))
]
if hits:
    start, end = hits[0], hits[-1]
    print(f"span {start+1}..{end+1} width={end-start+1} items={len(hits)}")
