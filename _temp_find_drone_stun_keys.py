import pathlib, re, xml.etree.ElementTree as ET
cfg = pathlib.Path(r'c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config')

items_xml = cfg / 'items.xml'
prog_xml = cfg / 'progression.xml'
recipes_xml = cfg / 'recipes.xml'

pat_drone = re.compile(r'drone|robotic', re.I)
pat_stun = re.compile(r'stun', re.I)

print('=== ITEMS: names containing drone/robotic and/or stun ===')
root = ET.parse(items_xml).getroot()
item_names = []
for item in root.findall('.//item'):
    n = item.get('name','')
    if pat_drone.search(n) or pat_stun.search(n):
        item_names.append(n)
for n in sorted(set(item_names)):
    print(n)

print('\n=== ITEMS: mods that look drone-related ===')
mods = sorted({n for n in item_names if n.lower().startswith('mod') and ('drone' in n.lower() or 'robotic' in n.lower() or 'stun' in n.lower())})
for n in mods:
    print(n)

print('\n=== PROGRESSION: unlock_entry item contains drone/robotic/stun ===')
proot = ET.parse(prog_xml).getroot()
seen = set()
for ue in proot.findall('.//unlock_entry'):
    item_attr = ue.get('item','')
    if not item_attr:
        continue
    items = [x.strip() for x in item_attr.split(',') if x.strip()]
    hit = [x for x in items if pat_drone.search(x) or pat_stun.search(x)]
    if hit:
        parent = ue.getparent() if hasattr(ue, 'getparent') else None
        for h in hit:
            seen.add(h)
for h in sorted(seen):
    print(h)

print('\n=== RECIPES: craft outputs contains drone/robotic/stun ===')
rroot = ET.parse(recipes_xml).getroot()
rseen = set()
for rec in rroot.findall('.//recipe'):
    n = rec.get('name','')
    if pat_drone.search(n) or pat_stun.search(n):
        rseen.add(n)
for n in sorted(rseen):
    print(n)
