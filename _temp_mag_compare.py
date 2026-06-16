import re
import pathlib
import xml.etree.ElementTree as ET

prog = pathlib.Path('c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/progression.xml')
win = pathlib.Path('c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/XUi_InGame/windows.xml')

root = ET.parse(prog).getroot()
skills = []
for cs in root.iter('crafting_skill'):
    n = cs.get('name','')
    if n.startswith('crafting'):
        skills.append((n, int(cs.get('max_level','100'))))

text = win.read_text(encoding='utf-8', errors='ignore')
# checks from magazine strip and zoom tabs
strip = set(re.findall(r'cvar\((crafting[A-Za-z0-9_]+)Check\)', text))
zoom = set(re.findall(r'name="zoomed([A-Za-z0-9_]+)"', text))
skill_names = set(n for n,_ in skills)
zoom_expected = set(n.replace('crafting','') for n,_ in skills)

missing_strip = sorted(skill_names - strip)
extra_strip = sorted(strip - skill_names)
missing_zoom = sorted(zoom_expected - zoom)
extra_zoom = sorted(zoom - zoom_expected)

print('crafting_skills_in_game', len(skill_names))
print('mag_strip_cvars_in_windows', len(strip))
print('zoom_tabs_in_windows', len(zoom))
print('missing_mag_strip_skills', missing_strip)
print('extra_mag_strip_skills', extra_strip)
print('missing_zoom_tabs', missing_zoom)
print('extra_zoom_tabs', extra_zoom)
