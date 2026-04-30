"""
AGF DoorSecure Full Pipeline

This script:
1. Reads blocks_doorsecure.xml
2. Generates AGFWood, AGFIron, AGFSteel variants for each block
3. For each with DMG1, creates a Clean variant with mesh damage rewritten
4. Appends 3 variant helper blocks (one for each AGF type)
5. Outputs all to blocks_doorsecure_agf_all.xml
"""

import xml.etree.ElementTree as ET
import copy
import re
import csv
import os
def load_sort_orders(csv_path):
    sort_map = {}
    with open(csv_path, newline='', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            base = row['Name']
            step1 = int(row['Sort Step 1'])
            step2 = int(row['Sort Step 2'])
            sort1 = f"AGF{step1:02d}{step2}"
            sort_map[base.lower()] = sort1
    return sort_map

def get_base_name_for_sort(name):
    # Always map all color variants to the White variant for lookup
    colors = [
        'ArmyGreen', 'Blue', 'Brown', 'Green', 'Grey', 'Orange', 'Pink', 'Purple', 'Red', 'Yellow', 'Oak', 'Black', 'White'
    ]
    for color in colors:
        if name.endswith(color):
            return name[:-len(color)] + 'White'
    return name

INPUT_FILE = 'blocks_doorsecure.xml'
OUTPUT_FILE = 'blocks_doorsecure_agf_all.xml'

COLORS = [
    'Blue', 'Brown', 'Green', 'Grey', 'Orange', 'Pink', 'Purple', 'Red', 'White', 'Yellow', 'Oak'
]

def split_main_color(name):
    for color in sorted(COLORS, key=len, reverse=True):
        if name.endswith('AGFWood'+color):
            return (name[:-len('AGFWood'+color)], 'Wood'+color)
        if name.endswith('AGFIron'+color):
            return (name[:-len('AGFIron'+color)], 'Iron'+color)
        if name.endswith('AGFSteel'+color):
            return (name[:-len('AGFSteel'+color)], 'Steel'+color)
        if name.endswith(color):
            return (name[:-len(color)], color)
    return (name, '')

def parse_mesh_stages(mesh, return_prefixes=False):
    parts = [p.strip() for p in mesh.split(',') if p.strip()]
    stages = []
    prefixes = []
    i = 0
    while i < len(parts) - 1:
        stage = parts[i]
        prefix = ''
        if '/' in stage:
            prefix, stage = stage.split('/', 1)
            prefix += '/'
        val = parts[i+1]
        if stage.startswith('DMG') and val.replace('-', '').isdigit():
            stages.append((stage, val))
            prefixes.append(prefix)
        i += 2
    dash_one = False
    if len(parts) >= 2 and parts[-2] == '-' and parts[-1] == '1':
        dash_one = True
    if return_prefixes:
        return stages, dash_one, prefixes
    return stages, dash_one

def scale_mesh_damage(mesh, new_maxdmg, orig_maxdmg):
    stages, dash_one, orig_prefixes = parse_mesh_stages(mesh, return_prefixes=True)
    new_stages = []
    for idx, (stage, val) in enumerate(stages):
        try:
            v = int(val)
            scaled = max(1, int(round(v * float(new_maxdmg) / orig_maxdmg)))
            prefix = orig_prefixes[idx] if idx < len(orig_prefixes) else ''
            new_stages.append((prefix + stage, str(scaled)))
        except Exception:
            prefix = orig_prefixes[idx] if idx < len(orig_prefixes) else ''
            new_stages.append((prefix + stage, val))
    mesh_str = '   ' + ',   '.join([f'{s}, {v}' for s, v in new_stages])
    if dash_one:
        mesh_str += ',   -, 1'
    mesh_str += '   '
    return mesh_str

def make_clean_mesh(mesh, maxdmg):
    stages, dash_one, orig_prefixes = parse_mesh_stages(mesh, return_prefixes=True)
    if len(stages) < 2:
        return mesh
    try:
        orig_hp = int(stages[1][1])
    except Exception:
        orig_hp = maxdmg
    new_stages = [(orig_prefixes[0] + 'DMG0', str(maxdmg+1)), (orig_prefixes[1] + 'DMG1', str(maxdmg))]
    for idx, (stage, val) in enumerate(stages):
        if idx < 2:
            continue
        try:
            v = int(val)
            scaled = max(1, int(round(v * maxdmg / orig_hp))) if orig_hp else v
            prefix = orig_prefixes[idx] if idx < len(orig_prefixes) else ''
            new_stages.append((prefix + stage, str(scaled)))
        except Exception:
            prefix = orig_prefixes[idx] if idx < len(orig_prefixes) else ''
            new_stages.append((prefix + stage, val))
    mesh_str = '   ' + ',   '.join([f'{s}, {v}' for s, v in new_stages])
    if dash_one:
        mesh_str += ',   -, 1'
    mesh_str += '   '
    return mesh_str

# Helper block names
wood_helper = 'miscwoodDoorVariantHelperAGF'
iron_helper = 'miscironDoorVariantHelperAGF'
steel_helper = 'miscsteelDoorVariantHelperAGF'
powered_helper = 'miscpoweredDoorVariantHelperAGF'

def get_helper_for_blockname(name):
    if 'AGFWood' in name:
        return wood_helper
    elif 'AGFIron' in name:
        return iron_helper
    elif 'AGFPowered' in name:
        return powered_helper
    elif 'AGFSteel' in name:
        return steel_helper
    return None

def make_helper_block(name, extends, icon, place_values, sort1, sort2):
    block = ET.Element('block', {'name': name})
    ET.SubElement(block, 'property', {'name': 'Extends', 'value': extends, 'param1': 'CustomIconTint'})
    ET.SubElement(block, 'property', {'name': 'CustomIcon', 'value': icon})
    ET.SubElement(block, 'property', {'name': 'CreativeMode', 'value': 'Player'})
    ET.SubElement(block, 'property', {'name': 'DescriptionKey', 'value': 'blockVariantHelperGroupDesc'})
    # Set ItemTypeIcon based on helper name
    if name == 'miscwoodDoorVariantHelperAGF':
        ET.SubElement(block, 'property', {'name': 'ItemTypeIcon', 'value': 'wood'})
    elif name == 'miscironDoorVariantHelperAGF':
        ET.SubElement(block, 'property', {'name': 'ItemTypeIcon', 'value': 'challenge_harvesting_wrench_vending_machine'})
    elif name == 'miscsteelDoorVariantHelperAGF':
        ET.SubElement(block, 'property', {'name': 'ItemTypeIcon', 'value': 'ibeam'})
    elif name == 'miscpoweredDoorVariantHelperAGF':
        ET.SubElement(block, 'property', {'name': 'ItemTypeIcon', 'value': 'electric_power'})
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
    blocks = []

    # Load SortOrder1 mapping from CSV
    sort_csv = os.path.join(os.path.dirname(__file__), 'doorsecure_sortTypeSummary.csv')
    sort_map = load_sort_orders(sort_csv)
    # 1. Generate AGF variants
    for block in root.findall('block'):
        orig_name = block.get('name')
        customicon = orig_name
        for prop in block.findall('property'):
            if prop.get('name') == 'CustomIcon':
                customicon = prop.get('value')
                break
        mesh_damage_props = [p for p in block.findall('property') if 'MeshDamage' in p.get('name','')]
        repair_mesh = any(p.get('class') == 'RepairItemsMeshDamage' for p in block.findall('property'))
        start_damage = any(p.get('name') == 'StartDamage' for p in block.findall('property'))
        stage2_health = any(p.get('name') == 'Stage2Health' for p in block.findall('property'))
        def copy_mesh_damage(scale):
            new_props = []
            for p in mesh_damage_props:
                new_p = copy.deepcopy(p)
                if not (repair_mesh or start_damage or stage2_health or p.get('name','').endswith('1')):
                    try:
                        new_val = int(float(p.get('value')) * scale)
                        new_p.set('value', str(new_val))
                    except Exception:
                        pass
                new_props.append(new_p)
            return new_props
        variants = [
            ('Wood', 'Mwood_regular', '1000', 'resourceWood', '10', 'Iron', 'resourceForgedIron', '10', '5', 'miscwoodDoorVariantHelper'),
            ('Iron', 'Mmetal', '5000', 'resourceForgedIron', '10', 'Steel', 'resourceForgedSteel', '10', '5', 'miscironDoorVariantHelper'),
            ('Steel', 'Msteel', '15000', 'resourceForgedSteel', '10', 'Powered', 'resourceForgedSteel', '10', '5', 'miscsteelDoorVariantHelper'),
            ('Powered', 'Msteel', '15000', 'resourceForgedSteel', '10', None, None, None, None, 'miscpoweredDoorVariantHelper')
        ]
        for idx, (mat, matval, maxdmg, repitem, repcount, upg_to, upg_item, upg_count, upg_hits, helper_name) in enumerate(variants):
            new_block = ET.Element('block', block.attrib)
            new_name = orig_name + 'AGF' + mat
            new_block.set('name', new_name)
            ET.SubElement(new_block, 'property', {'name': 'CustomIcon', 'value': customicon})
            ET.SubElement(new_block, 'property', {'name': 'Material', 'value': matval})
            ET.SubElement(new_block, 'property', {'name': 'MaxDamage', 'value': maxdmg})
            # Powered-specific properties
            if mat == 'Powered':
                # Remove any existing Class, ItemTypeIcon, Tags
                for prop in new_block.findall("property[@name='Class']"):
                    new_block.remove(prop)
                for prop in new_block.findall("property[@name='ItemTypeIcon']"):
                    new_block.remove(prop)
                for prop in new_block.findall("property[@name='Tags']"):
                    new_block.remove(prop)
                ET.SubElement(new_block, 'property', {'name': 'Class', 'value': 'PoweredDoor'})
                ET.SubElement(new_block, 'property', {'name': 'ItemTypeIcon', 'value': 'electric_power'})
                ET.SubElement(new_block, 'property', {'name': 'Tags', 'value': 'electricianSkill'})
            # Set or replace CreativeMode to None for all non-VariantHelperAGF blocks
            if not new_name.endswith('VariantHelperAGF'):
                # Remove any existing CreativeMode property
                for prop in new_block.findall("property[@name='CreativeMode']"):
                    new_block.remove(prop)
                ET.SubElement(new_block, 'property', {'name': 'CreativeMode', 'value': 'None'})
            # Get original MaxDamage for scaling
            orig_maxdmg = None
            for prop in block.findall('property'):
                if prop.get('name') == 'MaxDamage':
                    try:
                        orig_maxdmg = float(prop.get('value'))
                    except Exception:
                        orig_maxdmg = None
                    break
            if orig_maxdmg is None:
                orig_maxdmg = 1000.0  # fallback
            for p in mesh_damage_props:
                new_p = copy.deepcopy(p)
                mesh = new_p.get('value')
                if mesh:
                    mesh_str = scale_mesh_damage(mesh, int(maxdmg), orig_maxdmg)
                    new_p.set('value', mesh_str)
                new_block.append(new_p)
            if mat == 'Wood':
                rep = ET.SubElement(new_block, 'property', {'class': 'RepairItems'})
                ET.SubElement(rep, 'property', {'name': 'resourceWood', 'value': '10'})
            elif mat == 'Iron':
                rep = ET.SubElement(new_block, 'property', {'class': 'RepairItems'})
                ET.SubElement(rep, 'property', {'name': 'resourceForgedIron', 'value': '9'})
            elif mat in ('Steel', 'Powered'):
                rep = ET.SubElement(new_block, 'property', {'class': 'RepairItems'})
                ET.SubElement(rep, 'property', {'name': 'resourceForgedSteel', 'value': '11'})
            # Drop events now use the variant helper name
            helper = get_helper_for_blockname(new_name)
            if helper:
                # Remove any existing Drop property or <drop> elements
                for prop in new_block.findall("property[@name='Drop']"):
                    new_block.remove(prop)
                for drop in new_block.findall('drop'):
                    new_block.remove(drop)
                # Add correct <drop> elements
                drop_destroy = ET.Element('drop', {'event': 'Destroy', 'name': helper, 'count': '1'})
                drop_fall = ET.Element('drop', {'event': 'Fall', 'name': helper, 'count': '1', 'prob': '0.75', 'stick_chance': '1'})
                new_block.append(drop_destroy)
                new_block.append(drop_fall)
            if upg_to:
                upg = ET.SubElement(new_block, 'property', {'class': 'UpgradeBlock'})
                ET.SubElement(upg, 'property', {'name': 'ToBlock', 'value': orig_name + 'AGF' + upg_to})
                ET.SubElement(upg, 'property', {'name': 'Item', 'value': upg_item})
                ET.SubElement(upg, 'property', {'name': 'ItemCount', 'value': upg_count})
                ET.SubElement(upg, 'property', {'name': 'UpgradeHitCount', 'value': upg_hits})
            # Set MaxDamage
            new_block.find("property[@name='MaxDamage']").set('value', maxdmg)
            # Copy all properties except those handled above or already present
            manual_props = set()
            for p in new_block.findall('property'):
                if p.get('name'):
                    manual_props.add(p.get('name'))
                if p.get('class'):
                    manual_props.add(p.get('class'))
            for prop in block:
                if prop.tag == 'property':
                    if (prop.get('name') and prop.get('name') in manual_props) or (prop.get('class') and prop.get('class') in manual_props):
                        continue
                    # Exclude Stage2Health, StartDamage, and RepairItemsMeshDamage
                    if prop.get('name') in ('StartDamage', 'Stage2Health'):
                        continue
                    if prop.get('class') == 'RepairItemsMeshDamage':
                        continue
                    if 'MeshDamage' in (prop.get('name') or ''):
                        continue
                new_block.append(copy.deepcopy(prop))

            # Set SortOrder1 using the CSV mapping for all color variants, append 0 for regular, 1 for Clean
            base_name = get_base_name_for_sort(orig_name)
            sort1 = sort_map.get(base_name.lower(), "AGF000")
            # Determine if this is a Clean variant
            is_clean = new_block.get('name', '').endswith('Clean')
            sort1_full = f"{sort1}{'1' if is_clean else '0'}"
            # Remove any existing SortOrder1 property
            for prop in new_block.findall("property[@name='SortOrder1']"):
                new_block.remove(prop)
            ET.SubElement(new_block, 'property', {'name': 'SortOrder1', 'value': sort1_full})

            blocks.append(new_block)
    # 2. Add Clean variants for any with DMG1
    new_blocks = []
    clean_map = {'Wood': [], 'Iron': [], 'Steel': []}
    for block in blocks:
        name = block.get('name', '').lower()
        # Exclude jail, chainlinkfencedoor, rollup, cellar, vault, woodenfencedoor (all colors), elevatorTest, shutters, screen doors, and garage doors except irongarage
        if (
            'jail' in name or
            'chainlinkfencedoor' in name or
            'rollup' in name or
            'cellar' in name or
            'vault' in name or
            'woodenfencedoor' in name or
            'shutter' in name or
            'screen' in name or
            'elevatortest' in name or
            ('garage' in name and 'irongarage' not in name)
        ):
            continue
        mesh_props = [p for p in block.findall('property') if 'MeshDamage' in p.get('name','')]
        has_dmg1 = any('DMG1' in (p.get('value') or '') for p in mesh_props) or any('DMG1' in (p.text or '') for p in mesh_props)
        if mesh_props and has_dmg1:
            maxdmg = None
            orig_sort2 = None
            for p in block.findall('property'):
                if p.get('name') == 'MaxDamage':
                    try:
                        maxdmg = int(float(p.get('value')))
                    except Exception:
                        pass
                if p.get('name') == 'SortOrder2':
                    try:
                        orig_sort2 = int(p.get('value'))
                    except Exception:
                        pass
            if maxdmg is None:
                continue
            new_block = copy.deepcopy(block)
            new_block.set('name', block.get('name') + 'Clean')
            for p in new_block.findall('property'):
                if 'MeshDamage' in p.get('name',''):
                    mesh = p.get('value')
                    if mesh:
                        mesh_str = make_clean_mesh(mesh, maxdmg)
                        p.set('value', mesh_str)
            # Update UpgradeBlock ToBlock to point to Clean variant if present
            for upg in new_block.findall("property[@class='UpgradeBlock']"):
                for toblock in upg.findall("property[@name='ToBlock']"):
                    val = toblock.get('value')
                    if val and val.endswith(('AGFWood', 'AGFIron', 'AGFSteel')):
                        toblock.set('value', val + 'Clean')
            new_blocks.append(new_block)
            # Add Clean block to correct helper group
            if 'AGFWood' in block.get('name'):
                clean_map['Wood'].append(new_block.get('name'))
            elif 'AGFIron' in block.get('name'):
                clean_map['Iron'].append(new_block.get('name'))
            elif 'AGFSteel' in block.get('name'):
                clean_map['Steel'].append(new_block.get('name'))
    # Add ItemTypeIcon property to all Clean variants before sorting properties
    for clean_block in new_blocks:
        # Remove any existing ItemTypeIcon property
        for prop in clean_block.findall("property[@name='ItemTypeIcon']"):
            clean_block.remove(prop)
        # Add the new ItemTypeIcon property
        clean_block.append(ET.Element('property', {'name': 'ItemTypeIcon', 'value': 'paint_bucket'}))
    blocks.extend(new_blocks)
    # 3. Sort blocks by material group, then SortOrder1, then SortOrder2
    def get_material_group(name):
        if 'AGFWood' in name:
            return 0
        elif 'AGFIron' in name:
            return 1
        elif 'AGFSteel' in name:
            return 2
        else:
            return 3

    def get_sort_order(block):
        name = block.get('name', '')
        group = get_material_group(name)
        sort1 = None
        sort2 = None
        for prop in block.findall('property'):
            if prop.get('name') == 'SortOrder1':
                sort1 = prop.get('value')
            if prop.get('name') == 'SortOrder2':
                sort2 = prop.get('value')
        return (group, sort1 or '', sort2 or '')
    blocks.sort(key=get_sort_order)
    # 4. Add 3 variant helpers, including Clean blocks
    wood_doors = [b.get('name') for b in blocks if 'AGFWood' in b.get('name','')]
    iron_doors = [b.get('name') for b in blocks if 'AGFIron' in b.get('name','')]
    steel_doors = [b.get('name') for b in blocks if 'AGFSteel' in b.get('name','')]
    # Add Clean blocks to each group only if not already present
    wood_doors += [n for n in clean_map['Wood'] if n not in wood_doors]
    iron_doors += [n for n in clean_map['Iron'] if n not in iron_doors]
    steel_doors += [n for n in clean_map['Steel'] if n not in steel_doors]
    powered_doors = [b.get('name') for b in blocks if 'AGFPowered' in b.get('name','')]
    helpers = [
        make_helper_block('miscwoodDoorVariantHelperAGF', 'oldWoodDoor', 'oldWoodDoor', wood_doors, 'U100', '0001'),
        make_helper_block('miscironDoorVariantHelperAGF', 'ironDoorWhite', 'ironDoorWhite', iron_doors, 'U101', '0002'),
        make_helper_block('miscsteelDoorVariantHelperAGF', 'vaultDoor01', 'vaultDoor01', steel_doors, 'U102', '0003'),
        make_helper_block('miscpoweredDoorVariantHelperAGF', 'vaultDoor01', 'vaultDoor01', powered_doors, 'U103', '0004'),
    ]
    # 5. Alphabetize <property> elements by 'name' within each <block>
    def sort_block_properties(block):
        # Only sort direct <property> children, leave others (like <drop>, <UpgradeBlock>) untouched
        props = [child for child in block if child.tag == 'property']
        others = [child for child in block if child.tag != 'property']
        # Sort properties with a 'name' attribute alphabetically, then those without at the end
        props.sort(key=lambda p: (p.get('name') is None, p.get('name') or ''))
        # Remove all properties
        for p in props:
            block.remove(p)
        # Re-add in sorted order
        for p in props:
            block.append(p)
        # Ensure other children remain in original order after properties
        for o in others:
            block.remove(o)
            block.append(o)

    # After all blocks and helpers are created, ensure SortOrder1 last digit is 1 for Clean, 0 for others
    for block in blocks + helpers:
        name = block.get('name', '')
        for prop in block.findall("property[@name='SortOrder1']"):
            val = prop.get('value', '')
            if name.endswith('Clean'):
                if not val.endswith('1'):
                    prop.set('value', val[:-1] + '1')
            else:
                if not val.endswith('0'):
                    prop.set('value', val[:-1] + '0')

    # Write only <block> elements, no XML declaration or <blocks> parent
    all_blocks = []
    for b in blocks:
        sort_block_properties(b)
        all_blocks.append(b)
    for h in helpers:
        sort_block_properties(h)
        all_blocks.append(h)
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        for block in all_blocks:
            ET.indent(block, space='    ')
            f.write(ET.tostring(block, encoding='unicode'))
            f.write('\n')
    print(f'Wrote {OUTPUT_FILE}')

if __name__ == "__main__":
    main()
