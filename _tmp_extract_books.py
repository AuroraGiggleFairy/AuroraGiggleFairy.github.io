"""Extract book-sprite -> crafting-skill mapping from existing windows.xml."""
from pathlib import Path
import re

W = Path(r"c:\GitHub\7D2D-Mods\02_ActiveBuild\AGF-HUDPlus-PurpleBook-v2.0.1\Config\XUi\windows.xml")
text = W.read_text(encoding="utf-8")
lines = text.splitlines()

mapping: dict[str, str] = {}
re_sprite = re.compile(r'sprite="(book[A-Za-z0-9]+)"')
re_skill  = re.compile(r'crafting([A-Za-z]+)Check\)')

for i, ln in enumerate(lines):
    msp = re_sprite.search(ln)
    if not msp:
        continue
    # peek a few lines around for the skill name
    for j in range(max(0, i-2), min(len(lines), i+4)):
        msk = re_skill.search(lines[j])
        if msk:
            mapping.setdefault(msk.group(1), msp.group(1))
            break

for k in sorted(mapping):
    print(f"  {k:<18} -> {mapping[k]}")
print(f"\ntotal: {len(mapping)}")
