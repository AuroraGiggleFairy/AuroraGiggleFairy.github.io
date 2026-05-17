from pathlib import Path
import re

new_p = Path(r'01_Draft\AGF-PurpleBookTest-v0.0.1\Config\XUi\windows.xml')
old_p = Path(r'02_ActiveBuild\AGF-HUDPlus-PurpleBook-v2.0.1\Config\XUi\windows.xml')
new = new_p.read_text(encoding='utf-8')
old = old_p.read_text(encoding='utf-8')

print(f"           {'new':>15}   {'old':>15}")
print(f"bytes      {len(new):>15,}   {len(old):>15,}")
print(f"lines      {new.count(chr(10)):>15,}   {old.count(chr(10)):>15,}")

for label, pat in [
    ('zoomed rects', r'<rect name="zoomed[A-Za-z]+"'),
    ('checklist rects', r'<rect name="checklist[A-Za-z]+"'),
    ('QSection entries', r'<entry name="QSection'),
    ('Magazine entries', r'<entry name="Magazine'),
    ('itemIcon sprites', r'name="itemIcon"'),
    ('filledsprites', r'<filledsprite'),
    ('Q-chip labels', r'name="checkunlock"'),
    ('cvar references', r'\{cvar\(crafting'),
]:
    nn = len(re.findall(pat, new))
    no = len(re.findall(pat, old))
    print(f"{label:<22} {nn:>10}   {no:>15}")
