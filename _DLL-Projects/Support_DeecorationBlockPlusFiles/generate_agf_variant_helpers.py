"""
AGF Door Variant Helpers Generator

This script generates 3 variant helper blocks for all AGFWood, AGFIron, and AGFSteel doors found in blocks_doorsecure_agf.xml.
"""
import xml.etree.ElementTree as ET
import copy

INPUT_FILE = 'blocks_doorsecure_agf.xml'
OUTPUT_FILE = 'blocks_doorsecure_agf_helpers.xml'

def get_doors_by_type(root, type_str):
    return [b.get('name') for b in root.findall('block') if f'AGF{type_str}' in b.get('name','')]

def make_helper_block(name, extends, icon, place_values, sort1, sort2):
    block = ET.Element('block', {'name': name})
    ET.SubElement(block, 'property', {'name': 'Extends', 'value': extends})
    ET.SubElement(block, 'property', {'name': 'CustomIcon', 'value': icon})
    ET.SubElement(block, 'property', {'name': 'CreativeMode', 'value': 'Player'})
    ET.SubElement(block, 'property', {'name': 'DescriptionKey', 'value': 'blockVariantHelperGroupDesc'})
    ET.SubElement(block, 'property', {'name': 'ItemTypeIcon', 'value': 'all_blocks'})
    ET.SubElement(block, 'property', {'name': 'SelectAlternates', 'value': 'true'})
    ET.SubElement(block, 'property', {'name': 'PlaceAltBlockValue', 'value': ','.join(place_values)})
    ET.SubElement(block, 'property', {'name': 'Group', 'value': 'Basics,Building,advBuilding'})
    ET.SubElement(block, 'property', {'name': 'PickupJournalEntry', 'value': 'shapeMenuTip'})
    ET.SubElement(block, 'property', {'name': 'SortOrder1', 'value': sort1})
    ET.SubElement(block, 'property', {'name': 'SortOrder2', 'value': sort2})
    return block

def main():
    tree = ET.parse(INPUT_FILE)
    root = tree.getroot()
    # Gather all AGFWood, AGFIron, AGFSteel doors
    wood_doors = sorted(get_doors_by_type(root, 'Wood'))
    iron_doors = sorted(get_doors_by_type(root, 'Iron'))
    steel_doors = sorted(get_doors_by_type(root, 'Steel'))
    # Use the first door in each group as Extends and CustomIcon
    helpers = [
        make_helper_block('miscwoodDoorVariantHelper', wood_doors[0] if wood_doors else '', wood_doors[0] if wood_doors else '', wood_doors, 'U100', '0001'),
        make_helper_block('miscironDoorVariantHelper', iron_doors[0] if iron_doors else '', iron_doors[0] if iron_doors else '', iron_doors, 'U101', '0002'),
        make_helper_block('miscsteelDoorVariantHelper', steel_doors[0] if steel_doors else '', steel_doors[0] if steel_doors else '', steel_doors, 'U102', '0003'),
    ]
    out_root = ET.Element('blocks')
    for h in helpers:
        out_root.append(h)
    ET.indent(out_root, space='    ')
    ET.ElementTree(out_root).write(OUTPUT_FILE, encoding='utf-8', xml_declaration=True)
    print(f'Wrote {OUTPUT_FILE}')

if __name__ == "__main__":
    main()
