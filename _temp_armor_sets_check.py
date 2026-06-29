import re
import pathlib
import xml.etree.ElementTree as ET

items = pathlib.Path('c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/items.xml')
script = pathlib.Path('c:/GitHub/7D2D-Mods/_DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py')

root = ET.parse(items).getroot()
item_outfits = sorted({it.get('name') for it in root.iter('item') if re.match(r'^armor[A-Za-z0-9_]+Outfit$', it.get('name',''))})

text = script.read_text(encoding='utf-8', errors='ignore')
hardcoded = sorted(set(re.findall(r'"(armor[A-Za-z0-9_]+Outfit)"', text)))

missing_from_script = sorted([n for n in item_outfits if n not in hardcoded])
extra_in_script = sorted([n for n in hardcoded if n not in item_outfits])

print('outfit_items_in_game', len(item_outfits))
print('outfit_names_hardcoded_in_script', len(hardcoded))
print('missing_from_script', missing_from_script)
print('extra_in_script', extra_in_script)
