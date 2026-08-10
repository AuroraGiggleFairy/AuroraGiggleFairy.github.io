import re
path = r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\blocks.xml"
with open(path, encoding="utf-8") as f:
    data = f.read()

names = [
    "ironDoorWhite",
    "ironDoorGrey",
    "ironDoorArmyGreen",
    "ironDoorBrown",
    "cntGunSafe",
    "cntGunSafeWhite",
    "cntGunSafeVariantHelper",
]
keys = (
    "Extends",
    "CreativeMode",
    "SortOrder1",
    "SortOrder2",
    "CustomIcon",
    "CustomIconTint",
    "TintColor",
    "SelectAlternates",
    "PlaceAltBlockValue",
)
pat = re.compile(
    r'<property name="(' + "|".join(keys) + r')"([^>]*)/>'
)
for name in names:
    m = re.search(rf'<block name="{name}">(.*?)</block>', data, re.S)
    print("===", name, "===")
    if not m:
        print("  NOT FOUND")
        continue
    body = m.group(1)
    for p in pat.finditer(body):
        print(" ", p.group(0)[:200])
    if "CreativeMode" not in body:
        print("  [NO CreativeMode property]")
