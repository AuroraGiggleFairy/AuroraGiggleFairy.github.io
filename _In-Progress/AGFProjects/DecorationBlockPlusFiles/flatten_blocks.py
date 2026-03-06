
import xml.etree.ElementTree as ET
import copy
import xml.dom.minidom

INPUT_FILE = 'blocks.xml'
OUTPUT_FILE = 'blocks_flattened.xml'

# Parse the XML
tree = ET.parse(INPUT_FILE)
root = tree.getroot()


# Build a map of all blocks by name (handle nested <blocks> root)
block_map = {}
block_count = 0
block_names = []
for block in root.iter('block'):
    name = block.get('name')
    if name:
        block_map[name] = block
        block_count += 1
        if len(block_names) < 5:
            block_names.append(name)
if block_count == 0:
    print("WARNING: No <block> elements found in the input file! Check file path, encoding, and structure.")
else:
    print(f"Found {block_count} blocks in the XML file. First 5 block names: {block_names}")

def get_param_exclusions(block):
    # Returns a set of property names to exclude from inheritance
    exclusions = set()
    for attr in block.attrib:
        if attr.startswith('param'):
            exclusions.add(block.attrib[attr])
    return exclusions


# Add a cache for flattened blocks
flattened_cache = {}

def flatten_block(block, visited=None):
    if visited is None:
        visited = set()
    name = block.get('name')
    print(f'Processing block: {name}, attributes: {block.attrib}')
    if name in flattened_cache:
        return copy.deepcopy(flattened_cache[name])
    if name in visited:
        raise Exception(f'Circular inheritance detected for block: {name}')
    visited.add(name)

    # Look for <property name="Extends" value="..."/> child
    extends = None
    for prop in block.findall('property'):
        if prop.get('name') == 'Extends':
            extends = prop.get('value')
            break
    if extends:
        parent = block_map.get(extends)
        if parent is None:
            raise Exception(f'Parent block not found: {extends}')
        exclusions = get_param_exclusions(block)
        # Recursively flatten parent
        flat_parent = flatten_block(parent, visited)
        # Build dictionaries for parent properties and drops
        parent_props = {}
        parent_drops = {}
        other_parent = []
        for elem in flat_parent:
            if elem.tag == 'property' and elem.get('name') and elem.get('name') != 'Extends':
                parent_props[elem.get('name')] = copy.deepcopy(elem)
            elif elem.tag == 'drop' and elem.get('event'):
                parent_drops[elem.get('event')] = copy.deepcopy(elem)
            else:
                other_parent.append(copy.deepcopy(elem))
        # Remove excluded properties
        for ex in exclusions:
            if ex in parent_props:
                del parent_props[ex]
        # If <dropextendsoff/>, remove all parent drops
        if block.find('dropextendsoff') is not None:
            parent_drops = {}
        # Build dictionaries for child properties and drops
        child_props = {}
        child_drops = {}
        other_child = []
        for elem in block:
            if elem.tag == 'property' and elem.get('name') and elem.get('name') != 'Extends':
                child_props[elem.get('name')] = copy.deepcopy(elem)
            elif elem.tag == 'drop' and elem.get('event'):
                child_drops[elem.get('event')] = copy.deepcopy(elem)
            elif elem.tag == 'dropextendsoff':
                continue
            else:
                other_child.append(copy.deepcopy(elem))
        # Debug: print parent and child properties for every block with Extends
        print(f'Flattening block: {name} (Extends: {extends})')
        print('  Parent properties:')
        for pname, prop in parent_props.items():
            print(f"    {pname}: {prop.attrib}")
        print('  Child properties:')
        for pname, prop in child_props.items():
            print(f"    {pname}: {prop.attrib}")
        # Merge: child overrides parent
        merged_props = dict(parent_props)
        merged_props.update(child_props)  # child overrides parent
        merged_drops = dict(parent_drops)
        merged_drops.update(child_drops)
        # Build new block, EXPLICITLY REMOVE Extends
        new_attribs = {k: v for k, v in block.attrib.items() if not k.startswith('param') and k != 'Extends'}
        new_block = ET.Element('block', new_attribs)
        # Add all parent/child elements except Extends, drops, RepairItems, UpgradeBlock
        def should_keep(elem):
            if elem.tag == 'property' and elem.get('name') == 'Extends':
                return False
            if elem.tag == 'drop':
                return False
            if elem.tag == 'property' and elem.get('class') in ('RepairItems', 'UpgradeBlock'):
                return False
            return True
        # Collect all elements to add
        elements_to_add = []
        for elem in other_parent:
            if should_keep(elem):
                elements_to_add.append(elem)
        for pname, prop in parent_props.items():
            if pname not in child_props and should_keep(prop):
                elements_to_add.append(prop)
        for prop in child_props.values():
            if should_keep(prop):
                elements_to_add.append(prop)
        for elem in other_child:
            if should_keep(elem):
                elements_to_add.append(elem)
        # Sort <property> elements by name, keep others in order
        properties = [e for e in elements_to_add if e.tag == 'property' and e.get('name')]
        properties.sort(key=lambda e: e.get('name'))
        others = [e for e in elements_to_add if not (e.tag == 'property' and e.get('name'))]
        for prop in properties:
            new_block.append(prop)
        for elem in others:
            new_block.append(elem)
        flattened_cache[name] = copy.deepcopy(new_block)
        return new_block
    else:
        # No Extends, just return a copy without Extends/param attributes, Extends property, drops, RepairItems, UpgradeBlock
        new_block = ET.Element('block', {k: v for k, v in block.attrib.items() if not k.startswith('param') and k != 'Extends'})
        def should_keep(elem):
            if elem.tag == 'property' and elem.get('name') == 'Extends':
                return False
            if elem.tag == 'drop':
                return False
            if elem.tag == 'property' and elem.get('class') in ('RepairItems', 'UpgradeBlock'):
                return False
            return True
        elements_to_add = []
        for elem in block:
            if elem.tag == 'dropextendsoff':
                continue
            if should_keep(elem):
                elements_to_add.append(copy.deepcopy(elem))
        properties = [e for e in elements_to_add if e.tag == 'property' and e.get('name')]
        properties.sort(key=lambda e: e.get('name'))
        others = [e for e in elements_to_add if not (e.tag == 'property' and e.get('name'))]
        for prop in properties:
            new_block.append(prop)
        for elem in others:
            new_block.append(elem)
        flattened_cache[name] = copy.deepcopy(new_block)
        return new_block




# Build new <blocks> root and only append flattened blocks


# First, collect all block names (without 'trader' prefix if present)

all_block_names = set()
for block in root.iter('block'):
    name = block.get('name', '')
    # For 'trader' prefix
    if name.startswith('trader'):
        all_block_names.add(name[6:])
    else:
        all_block_names.add(name)
    # For 'TraderOnly' suffix
    if name.endswith('TraderOnly'):
        all_block_names.add(name[:-10])
    # For 'POI' suffix
        # For 'player' substring
        if 'player' in name:
            all_block_names.add(name.replace('player', ''))
        # Remove 'player' blocks if a duplicate exists with 'player' removed
        if 'player' in name and name.replace('player', '') in all_block_names:
            continue
    if name.endswith('POI'):
        all_block_names.add(name[:-3])

top = ET.Element('blocks', root.attrib)
processed_count = 0
terrainFillerAdaptive_flat = None
for block in root.iter('block'):
    name = block.get('name', '')
    # Remove blocks with 'master', 'LootHelper', or 'VariantHelper' in their name
    if 'master' in name or 'LootHelper' in name or 'VariantHelper' in name:
        continue
    # Remove blocks with a <property class="TrapDoor"> element
    has_trapdoor = any(elem.tag == 'property' and elem.get('class') == 'TrapDoor' for elem in block)
    if has_trapdoor:
        continue
    # Remove blocks whose names end with 'Shapes'
    if name.endswith('Shapes'):
        continue
    # Remove 'trader' blocks if a non-trader duplicate exists
    if name.startswith('trader') and name[6:] in all_block_names:
        continue
    # Remove 'TraderOnly' blocks if a non-TraderOnly duplicate exists
    if name.endswith('TraderOnly') and name[:-10] in all_block_names:
        continue
    # Remove 'POI' blocks if a non-POI duplicate exists
    if name.endswith('POI') and name[:-3] in all_block_names:
        continue
    flat = flatten_block(block)
    if name == 'terrainFillerAdaptive':
        terrainFillerAdaptive_flat = flat
    top.append(flat)
    processed_count += 1
print(f"Processed {processed_count} blocks and wrote to output.")

# Debug: print the flattened terrainFillerAdaptive block and its properties
if terrainFillerAdaptive_flat is not None:
    print('Flattened terrainFillerAdaptive block:')
    print(ET.tostring(terrainFillerAdaptive_flat, encoding='unicode'))
    print('Properties in flattened terrainFillerAdaptive:')
    for elem in terrainFillerAdaptive_flat:
        if elem.tag == 'property':
            print(f"  {elem.attrib}")

# Remove all comments from the tree
def remove_comments(elem):
    for child in list(elem):
        if isinstance(child.tag, str):
            remove_comments(child)
        else:
            # This is a comment
            elem.remove(child)
remove_comments(top)

# Write output
ET.indent(top, space='    ')
ET.ElementTree(top).write(OUTPUT_FILE, encoding='utf-8', xml_declaration=True)
print(f'Flattened blocks written to {OUTPUT_FILE}')
