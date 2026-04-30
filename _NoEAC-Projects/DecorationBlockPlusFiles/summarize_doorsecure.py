import xml.etree.ElementTree as ET
import csv

INPUT_FILE = 'blocks_doorsecure.xml'
OUTPUT_CSV = 'doorsecure_summary.csv'

# Parse the DoorSecure XML
root = ET.parse(INPUT_FILE).getroot()

rows = []
for block in root.iter('block'):
    name = block.get('name', '')
    max_damage = ''
    mesh_damage = ''
    for prop in block:
        if prop.tag == 'property':
            if prop.get('name') == 'MaxDamage':
                max_damage = prop.get('value')
            elif prop.get('name') == 'MeshDamage':
                mesh_damage = prop.get('value')
    rows.append({'Block Name': name, 'MaxDamage': max_damage, 'MeshDamage': mesh_damage})

# Write to CSV
with open(OUTPUT_CSV, 'w', newline='', encoding='utf-8') as csvfile:
    writer = csv.DictWriter(csvfile, fieldnames=['Block Name', 'MaxDamage', 'MeshDamage'])
    writer.writeheader()
    for row in rows:
        writer.writerow(row)

print(f'Wrote summary to {OUTPUT_CSV}')
