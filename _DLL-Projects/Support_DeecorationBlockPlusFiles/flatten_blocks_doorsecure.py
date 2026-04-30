import xml.etree.ElementTree as ET
import copy
import xml.dom.minidom

INPUT_FILE = 'blocks.xml'
OUTPUT_FILE = 'blocks_flattened.xml'
DOORSECURE_FILE = 'blocks_doorsecure.xml'

tree = ET.parse(INPUT_FILE)
root = tree.getroot()

# Collect all block names for deduplication logic
all_block_names = set()
for block in root.iter('block'):
    name = block.get('name', '')
    if name.startswith('trader'):
        all_block_names.add(name[6:])
    else:
        all_block_names.add(name)
    if name.endswith('TraderOnly'):
        all_block_names.add(name[:-10])
    if name.endswith('POI'):
        all_block_names.add(name[:-3])
    if 'player' in name:
        all_block_names.add(name.replace('player', ''))

def should_keep_block(block):
    name = block.get('name', '')
    if 'master' in name or 'LootHelper' in name or 'VariantHelper' in name:
        return False
    if name.startswith('trader') and name[6:] in all_block_names:
        return False
    if name.endswith('TraderOnly') and name[:-10] in all_block_names:
        return False
    if name.endswith('POI') and name[:-3] in all_block_names:
        return False
    if 'player' in name and name.replace('player', '') in all_block_names:
        return False
    if any(elem.tag == 'property' and elem.get('class') == 'TrapDoor' for elem in block):
        return False
    if name.endswith('Shapes'):
        return False
    return True

def should_keep_elem(elem):
    if elem.tag == 'property' and elem.get('name') == 'Extends':
        return False
    if elem.tag == 'drop':
        return False
    if elem.tag == 'property' and elem.get('class') in ('RepairItems', 'UpgradeBlock'):
        return False
    return True

def flatten_block(block, visited=None):
    if visited is None:
        visited = set()
    name = block.get('name')
    if name in flatten_block.cache:
        return copy.deepcopy(flatten_block.cache[name])
    if name in visited:
        raise Exception(f'Circular inheritance detected for block: {name}')
    visited.add(name)
    # Find Extends property
    extends = None
    for prop in block.findall('property'):
        if prop.get('name') == 'Extends':
            extends = prop.get('value')
            break
    if extends:
        parent = block_map.get(extends)
        if parent is None:
            raise Exception(f'Parent block not found: {extends}')
        exclusions = set()
        for attr in block.attrib:
            if attr.startswith('param'):
                exclusions.add(block.attrib[attr])
        flat_parent = flatten_block(parent, visited)
        parent_props = {}
        other_parent = []
        for elem in flat_parent:
            if elem.tag == 'property' and elem.get('name') and elem.get('name') != 'Extends':
                parent_props[elem.get('name')] = copy.deepcopy(elem)
            else:
                other_parent.append(copy.deepcopy(elem))
        for ex in exclusions:
            if ex in parent_props:
                del parent_props[ex]
        child_props = {}
        other_child = []
        for elem in block:
            if elem.tag == 'property' and elem.get('name') and elem.get('name') != 'Extends':
                child_props[elem.get('name')] = copy.deepcopy(elem)
            elif elem.tag == 'dropextendsoff':
                continue
            else:
                other_child.append(copy.deepcopy(elem))
        merged_props = dict(parent_props)
        merged_props.update(child_props)
        new_attribs = {k: v for k, v in block.attrib.items() if not k.startswith('param') and k != 'Extends'}
        new_block = ET.Element('block', new_attribs)
        elements_to_add = []
        for elem in other_parent:
            if should_keep_elem(elem):
                elements_to_add.append(elem)
        for pname, prop in parent_props.items():
            if pname not in child_props and should_keep_elem(prop):
                elements_to_add.append(prop)
        for prop in child_props.values():
            if should_keep_elem(prop):
                elements_to_add.append(prop)
        for elem in other_child:
            if should_keep_elem(elem):
                elements_to_add.append(elem)
        properties = [e for e in elements_to_add if e.tag == 'property' and e.get('name')]
        properties.sort(key=lambda e: e.get('name'))
        others = [e for e in elements_to_add if not (e.tag == 'property' and e.get('name'))]
        for prop in properties:
            new_block.append(prop)
        for elem in others:
            new_block.append(elem)
        flatten_block.cache[name] = copy.deepcopy(new_block)
        return new_block
    else:
        new_block = ET.Element('block', {k: v for k, v in block.attrib.items() if not k.startswith('param') and k != 'Extends'})
        elements_to_add = []
        for elem in block:
            if elem.tag == 'dropextendsoff':
                continue
            if should_keep_elem(elem):
                elements_to_add.append(copy.deepcopy(elem))
        properties = [e for e in elements_to_add if e.tag == 'property' and e.get('name')]
        properties.sort(key=lambda e: e.get('name'))
        others = [e for e in elements_to_add if not (e.tag == 'property' and e.get('name'))]
        for prop in properties:
            new_block.append(prop)
        for elem in others:
            new_block.append(elem)
        flatten_block.cache[name] = copy.deepcopy(new_block)
        return new_block
flatten_block.cache = {}

# Build block map for flattening
block_map = {}
for block in root.iter('block'):
    name = block.get('name')
    if name:
        block_map[name] = block

# Write all filtered/flattened blocks
all_blocks_root = ET.Element('blocks', root.attrib)

doorsecure_root = ET.Element('blocks', root.attrib)
doorsecure_blocks = []
for block in root.iter('block'):
    if not should_keep_block(block):
        continue
    flat = flatten_block(block)
    all_blocks_root.append(flat)
    # If block has property Class value DoorSecure, add to doorsecure_blocks
    for elem in flat:
        if elem.tag == 'property' and elem.get('name') == 'Class' and elem.get('value') == 'DoorSecure':
            doorsecure_blocks.append(copy.deepcopy(flat))
            break
import re
# List of known color/suffixes (add more as needed)
COLORS = [
    'Blue', 'Brown', 'Green', 'Grey', 'Orange', 'Pink', 'Purple', 'Red', 'White', 'Yellow', 'Oak'
]
def split_main_color(name):
    # Try to match the longest color/suffix at the end
    for color in sorted(COLORS, key=len, reverse=True):
        if name.endswith(color):
            return (name[:-len(color)], color)
    return (name, '')
doorsecure_blocks.sort(key=lambda b: split_main_color(b.get('name', '')))
for b in doorsecure_blocks:
    doorsecure_root.append(b)
ET.indent(all_blocks_root, space='    ')
ET.ElementTree(all_blocks_root).write(OUTPUT_FILE, encoding='utf-8', xml_declaration=True)
ET.indent(doorsecure_root, space='    ')
ET.ElementTree(doorsecure_root).write(DOORSECURE_FILE, encoding='utf-8', xml_declaration=True)
print(f'Flattened blocks written to {OUTPUT_FILE}')
print(f'DoorSecure blocks written to {DOORSECURE_FILE}')
