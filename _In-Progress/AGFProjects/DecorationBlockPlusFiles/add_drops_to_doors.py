import xml.etree.ElementTree as ET

INPUT = 'blocks_doorsecure.xml'
OUTPUT = 'blocks_doorsecure_drops.xml'

def add_drops_to_doors(input_path, output_path):
    tree = ET.parse(input_path)
    root = tree.getroot()
    for block in root.findall('block'):
        name = block.get('name')
        # Remove existing drop elements
        for drop in block.findall('drop'):
            block.remove(drop)
        # Add new drop elements
        drop_destroy = ET.Element('drop', {'event': 'Destroy', 'name': name})
        drop_fall = ET.Element('drop', {'event': 'Fall', 'name': name, 'count': '1', 'prob': '0.75', 'stick_chance': '1'})
        block.append(drop_destroy)
        block.append(drop_fall)
    tree.write(output_path, encoding='utf-8', xml_declaration=True)

if __name__ == '__main__':
    add_drops_to_doors(INPUT, OUTPUT)
    print(f'Updated drops written to {OUTPUT}')
