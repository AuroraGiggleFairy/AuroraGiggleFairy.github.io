import pathlib
import xml.etree.ElementTree as ET

prog = pathlib.Path('c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/progression.xml')
root = ET.parse(prog).getroot()
cs = root.find('.//crafting_skill[@name="craftingArmor"]')
items = set()
if cs is not None:
    for de in cs.findall('display_entry'):
        item_attr = de.get('item','')
        if item_attr:
            for n in item_attr.split(','):
                n=n.strip()
                if n: items.add(n)
        for ue in de.findall('unlock_entry'):
            for n in (ue.get('item','').split(',')):
                n=n.strip()
                if n: items.add(n)
print('craftingArmor_unique_items', len(items))
print(sorted(items))
