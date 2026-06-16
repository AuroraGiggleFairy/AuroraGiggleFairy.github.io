import re
import pathlib
import xml.etree.ElementTree as ET

repo = pathlib.Path('c:/GitHub/7D2D-Mods')
recipes = pathlib.Path('c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/recipes.xml')
win = repo / '01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/XUi_InGame/windows.xml'

rroot = ET.parse(recipes).getroot()
learnable = set()
for rec in rroot.iter('recipe'):
    name = (rec.get('name') or '').strip()
    if not name:
        continue
    tags = {t.strip().lower() for t in (rec.get('tags', '').split(',')) if t.strip()}
    req_text = ' '.join((req.get('name', '') + ' ' + req.get('value', '')) for req in rec.findall('./effect_group/requirement'))
    if 'learnable' in tags or 'RecipeTagUnlocked' in req_text:
        learnable.add(name)

text = win.read_text(encoding='utf-8', errors='ignore')
start = text.find('name="unlockablesTab"')
end = text.find('<rect name="armorsTab"')
frag = text[start:end] if start != -1 else ''
tips = set(re.findall(r'tooltip_key="([A-Za-z0-9_]+)"', frag))
covered = set([n for n in learnable if n in tips])
missing = sorted(learnable - covered)

print('learnable_recipes', len(learnable))
print('learnables_with_matching_tooltip_in_unlock_tab', len(covered))
print('missing_by_name_match', len(missing))
print('sample_missing', missing[:120])
