import xml.etree.ElementTree as ET
import re
from collections import defaultdict

INPUT = 'blocks_doorsecure_agf.xml'
OUTPUT = 'blocks_doorsecure_agf_with_helpers.xml'

MATERIALS = [
    ('Wood', 'doorWood'),
    ('Iron', 'doorIron'),
    ('Steel', 'doorSteel'),
]

def collect_variant_blocks(input_path):
    tree = ET.parse(input_path)
    root = tree.getroot()
    # Group by base type (e.g., doorWoodOldWoodDoorAGF -> OldWoodDoor)
    material_blocks = {mat: defaultdict(list) for mat, _ in MATERIALS}
    for block in root.findall('block'):
        name = block.get('name', '')
        for mat, prefix in MATERIALS:
            if name.startswith(prefix):
                # Extract base type (between prefix and AGF)
                base = name[len(prefix):-3] if name.endswith('AGF') else name[len(prefix):]
                material_blocks[mat][base].append(name)
    return material_blocks

def make_variant_helper_block(material, block_names, extends_icon, custom_icon):
    block = ET.Element('block', {'name': f'misc{material.lower()}DoorVariantHelperAGF'})
    ET.SubElement(block, 'property', {'name': 'Extends', 'value': extends_icon})
    ET.SubElement(block, 'property', {'name': 'CustomIcon', 'value': custom_icon})
    ET.SubElement(block, 'property', {'name': 'CreativeMode', 'value': 'Player'})
    ET.SubElement(block, 'property', {'name': 'DescriptionKey', 'value': 'blockVariantHelperGroupDesc'})
    ET.SubElement(block, 'property', {'name': 'ItemTypeIcon', 'value': 'all_blocks'})
    ET.SubElement(block, 'property', {'name': 'SelectAlternates', 'value': 'true'})
    ET.SubElement(block, 'property', {'name': 'PlaceAltBlockValue', 'value': ','.join(block_names)})
    ET.SubElement(block, 'property', {'name': 'Group', 'value': 'Basics,Building,advBuilding'})
    ET.SubElement(block, 'property', {'name': 'PickupJournalEntry', 'value': 'shapeMenuTip'})
    sort_orders = {'Wood': ('U100', '0001'), 'Iron': ('U101', '0002'), 'Steel': ('U102', '0003')}
    so1, so2 = sort_orders.get(material, ('U199', '0099'))
    ET.SubElement(block, 'property', {'name': 'SortOrder1', 'value': so1})
    ET.SubElement(block, 'property', {'name': 'SortOrder2', 'value': so2})
    return block
    return block

def main():
    tree = ET.parse(INPUT)
    root = tree.getroot()
    material_blocks = collect_variant_blocks(INPUT)
    # Load original doors for CustomIcon lookup
    orig_tree = ET.parse('blocks_doorsecure.xml')
    orig_root = orig_tree.getroot()
    orig_doors = {b.get('name'): b for b in orig_root.findall('block')}
    # Add a single variantHelper block for each material
    for mat, _ in MATERIALS:
        all_agf_names = []
        for agf_names in material_blocks[mat].values():
            all_agf_names.extend(agf_names)
        # Use the first block as the icon, or fallback to 'Generic'
        extends_icon = custom_icon = 'Generic'
        if all_agf_names:
            first_block = all_agf_names[0]
            # Try to find the original block for icon
            orig_block = orig_doors.get(first_block.replace(f'door{mat}', '').replace('AGF', ''))
            if orig_block is not None:
                for prop in orig_block.findall('property'):
                    if prop.get('name') == 'CustomIcon':
                        custom_icon = prop.get('value')
                        break
                extends_icon = orig_block.get('name')
            else:
                extends_icon = first_block.replace('AGF', '')
                custom_icon = extends_icon
        helper = make_variant_helper_block(mat, all_agf_names, extends_icon, custom_icon)
        root.append(helper)
    tree.write(OUTPUT, encoding='utf-8', xml_declaration=True)
    print(f'Wrote {OUTPUT}')

if __name__ == '__main__':
    main()
