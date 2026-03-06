import xml.etree.ElementTree as ET
import copy
import re

INPUT_FILE = 'blocks_doorsecure.xml'
OUTPUT_FILE = 'blocks_doorsecure_agf.xml'

# List of known color/suffixes (add more as needed)
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


def make_clean_mesh_damage(orig_mesh, max_damage, orig_hp, new_hp):
    """
    Cleanly parse and scale mesh damage string for door blocks.
    """
    # Parse mesh string into DMG stages
    # Example: 'Door/DMG0, 1000, Door/DMG1, 750, Door/DMG2, 500, Door/DMG3, 250, -, 1'
    parts = re.split(r'(,| )', orig_mesh)
    stages = []
    i = 0
    while i < len(parts):
        p = parts[i].strip()
        if p.startswith('DMG'):
            stage = p
            # Find value
            j = i + 1
            while j < len(parts) and not parts[j].strip().replace('-', '').replace('.', '').isdigit():
                j += 1
            if j < len(parts):
                val = parts[j].strip()
                stages.append((stage, val))
            i = j
        i += 1
    # Build new mesh: DMG0 = max_damage+1, DMG1 = max_damage, rest shifted/scaled
    new_stages = []
    new_stages.append(('DMG0', str(max_damage + 1)))
    new_stages.append(('DMG1', str(max_damage)))
    for idx, (stage, val) in enumerate(stages):
        if idx == 0 or idx == 1:
            continue
        # Scale remaining stages
        try:
            v = int(val)
            scaled = max(1, int(round(v * new_hp / orig_hp)))
            new_stages.append((stage, str(scaled)))
        except Exception:
            new_stages.append((stage, val))
    # Rebuild mesh string
    mesh_str = '   ' + ',   '.join([f'Door/{s}, {v}' for s, v in new_stages]) + '   '
    return mesh_str
def main():
    tree = ET.parse(INPUT_FILE)
    root = tree.getroot()

    blocks = []
    for block in root.findall('block'):
        orig_name = block.get('name')
        # Find customicon value
        customicon = orig_name
        for prop in block.findall('property'):
            if prop.get('name') == 'CustomIcon':
                customicon = prop.get('value')
                break
        # Gather mesh damage and special properties
        mesh_damage_props = [p for p in block.findall('property') if 'MeshDamage' in p.get('name','')]
        repair_mesh = any(p.get('class') == 'RepairItemsMeshDamage' for p in block.findall('property'))
        start_damage = any(p.get('name') == 'StartDamage' for p in block.findall('property'))
        stage2_health = any(p.get('name') == 'Stage2Health' for p in block.findall('property'))
        mesh_damage_ends_1 = any(p.get('name','').endswith('1') for p in mesh_damage_props)
        # Helper to copy and update mesh damage
        def copy_mesh_damage(scale):
            new_props = []
            for p in mesh_damage_props:
                new_p = copy.deepcopy(p)
                if not (repair_mesh or start_damage or stage2_health or p.get('name','').endswith('1')):
                    # Scale value
                    try:
                        new_val = int(float(p.get('value')) * scale)
                        new_p.set('value', str(new_val))
                    except Exception:
                        pass
                new_props.append(new_p)
            return new_props
        # Generate variants
        variants = [
            ('Wood', 'Mwood_regular', '1000', 'resourceWood', '10', 'Iron', 'resourceForgedIron', '10', '5'),
            ('Iron', 'Mmetal', '5000', 'resourceForgedIron', '10', 'Steel', 'resourceForgedSteel', '10', '5'),
            ('Steel', 'Msteel', '15000', 'resourceForgedSteel', '10', None, None, None, None)
        ]
        prev_name = None
        for idx, (mat, matval, maxdmg, repitem, repcount, upg_to, upg_item, upg_count, upg_hits) in enumerate(variants):
            new_block = ET.Element('block', block.attrib)
            new_name = orig_name + 'AGF' + mat
            new_block.set('name', new_name)
            # CustomIcon
            ET.SubElement(new_block, 'property', {'name': 'CustomIcon', 'value': customicon})
            # Material and MaxDamage
            ET.SubElement(new_block, 'property', {'name': 'Material', 'value': matval})
            ET.SubElement(new_block, 'property', {'name': 'MaxDamage', 'value': maxdmg})
            # MeshDamage (scaled)
            scale = float(maxdmg) / 1000.0
            for p in copy_mesh_damage(scale):
                new_block.append(p)
            # RepairItems
            rep = ET.SubElement(new_block, 'property', {'class': 'RepairItems'})
            rep.set(repitem, repcount)
            # Drops
            ET.SubElement(new_block, 'drop', {'event': 'Destroy', 'name': new_name})
            ET.SubElement(new_block, 'drop', {'event': 'Fall', 'name': new_name, 'count': '1', 'prob': '0.75', 'stick_chance': '1'})
            # Upgrades
            if upg_to:
                upg = ET.SubElement(new_block, 'property', {'class': 'UpgradeBlock'})
                ET.SubElement(upg, 'property', {'name': 'ToBlock', 'value': orig_name + 'AGF' + upg_to})
                ET.SubElement(upg, 'property', {'name': 'Item', 'value': upg_item})
                ET.SubElement(upg, 'property', {'name': 'ItemCount', 'value': upg_count})
                ET.SubElement(upg, 'property', {'name': 'UpgradeHitCount', 'value': upg_hits})
            # Copy over all other properties
            for prop in block:
                if prop.tag == 'property' and prop.get('name') not in ('CustomIcon', 'Material', 'MaxDamage') and prop.get('class') not in ('UpgradeBlock', 'RepairItems') and 'MeshDamage' not in prop.get('name','') and prop.get('name') not in ('StartDamage', 'Stage2Health'):
                    new_block.append(copy.deepcopy(prop))
            blocks.append(new_block)
            prev_name = new_name
    # Sort blocks using the same logic as blocks_doorsecure
    blocks.sort(key=lambda b: split_main_color(b.get('name', '')))
    # Write output
    out_root = ET.Element('blocks')
    for b in blocks:
        out_root.append(b)
    ET.indent(out_root, space='    ')
    ET.ElementTree(out_root).write(OUTPUT_FILE, encoding='utf-8', xml_declaration=True)
    print(f'Wrote {OUTPUT_FILE}')

if __name__ == "__main__":
    main()
                except:
                    new_stages.append((stage, val))
            # Rebuild mesh string
            mesh_str = '   ' + ',   '.join([f'Door/{s}, {v}' for s,v in new_stages]) + '   '
            return mesh_str

            if j < len(parts):
                val = parts[j].strip()
                stages.append((stage, val))
            i = j
        i += 1
    # Build new mesh: DMG0 = max_damage+1, DMG1 = max_damage, rest shifted/scaled
    new_stages = []
    new_stages.append(('DMG0', str(max_damage+1)))
    new_stages.append(('DMG1', str(max_damage)))
    for idx, (stage, val) in enumerate(stages):
        if idx == 0 or idx == 1:
            continue
        # Scale remaining stages
        try:
            v = int(val)
            scaled = max(1, int(round(v * new_hp / orig_hp)))
            new_stages.append((stage, str(scaled)))
        except:
            new_stages.append((stage, val))
    # Rebuild mesh string
    mesh_str = '   ' + ',   '.join([f'Door/{s}, {v}' for s,v in new_stages]) + '   '
    return mesh_str
def main():

# --- NEW LOGIC FOR AGF VARIANTS ---
import xml.etree.ElementTree as ET
import copy
import re

INPUT_FILE = 'blocks_doorsecure.xml'
OUTPUT_FILE = 'blocks_doorsecure_agf.xml'

# List of known color/suffixes (add more as needed)
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

tree = ET.parse(INPUT_FILE)
root = tree.getroot()

blocks = []
for block in root.findall('block'):
    orig_name = block.get('name')
    # Find customicon value
    customicon = orig_name
    for prop in block.findall('property'):
        if prop.get('name') == 'CustomIcon':
            customicon = prop.get('value')
            break
    # Gather mesh damage and special properties
    mesh_damage_props = [p for p in block.findall('property') if 'MeshDamage' in p.get('name','')]
    repair_mesh = any(p.get('class') == 'RepairItemsMeshDamage' for p in block.findall('property'))
    start_damage = any(p.get('name') == 'StartDamage' for p in block.findall('property'))
    stage2_health = any(p.get('name') == 'Stage2Health' for p in block.findall('property'))
    mesh_damage_ends_1 = any(p.get('name','').endswith('1') for p in mesh_damage_props)
    # Helper to copy and update mesh damage
    def copy_mesh_damage(scale):
        new_props = []
        for p in mesh_damage_props:
            new_p = copy.deepcopy(p)
            if not (repair_mesh or start_damage or stage2_health or p.get('name','').endswith('1')):
                # Scale value
                try:
                    new_val = int(float(p.get('value')) * scale)
                    new_p.set('value', str(new_val))
                except Exception:
                    pass
            new_props.append(new_p)
        return new_props
    # Generate variants
    variants = [
        ('Wood', 'Mwood_regular', '1000', 'resourceWood', '10', 'Iron', 'resourceForgedIron', '10', '5'),
        ('Iron', 'Mmetal', '5000', 'resourceForgedIron', '10', 'Steel', 'resourceForgedSteel', '10', '5'),
        ('Steel', 'Msteel', '15000', 'resourceForgedSteel', '10', None, None, None, None)
    ]
    prev_name = None
    for idx, (mat, matval, maxdmg, repitem, repcount, upg_to, upg_item, upg_count, upg_hits) in enumerate(variants):
        new_block = ET.Element('block', block.attrib)
        new_name = orig_name + 'AGF' + mat
        new_block.set('name', new_name)
        # CustomIcon
        ET.SubElement(new_block, 'property', {'name': 'CustomIcon', 'value': customicon})
        # Material and MaxDamage
        ET.SubElement(new_block, 'property', {'name': 'Material', 'value': matval})
        ET.SubElement(new_block, 'property', {'name': 'MaxDamage', 'value': maxdmg})
        # MeshDamage (scaled)
        scale = float(maxdmg) / 1000.0
        for p in copy_mesh_damage(scale):
            new_block.append(p)
        # RepairItems
        rep = ET.SubElement(new_block, 'property', {'class': 'RepairItems'})
        rep.set(repitem, repcount)
        # Drops
        ET.SubElement(new_block, 'drop', {'event': 'Destroy', 'name': new_name})
        ET.SubElement(new_block, 'drop', {'event': 'Fall', 'name': new_name, 'count': '1', 'prob': '0.75', 'stick_chance': '1'})
        # Upgrades
        if upg_to:
            upg = ET.SubElement(new_block, 'property', {'class': 'UpgradeBlock'})
            ET.SubElement(upg, 'property', {'name': 'ToBlock', 'value': orig_name + 'AGF' + upg_to})
            ET.SubElement(upg, 'property', {'name': 'Item', 'value': upg_item})
            ET.SubElement(upg, 'property', {'name': 'ItemCount', 'value': upg_count})
            ET.SubElement(upg, 'property', {'name': 'UpgradeHitCount', 'value': upg_hits})
        # Copy over all other properties
        for prop in block:
            if prop.tag == 'property' and prop.get('name') not in ('CustomIcon', 'Material', 'MaxDamage') and prop.get('class') not in ('UpgradeBlock', 'RepairItems') and 'MeshDamage' not in prop.get('name','') and prop.get('name') not in ('StartDamage', 'Stage2Health'):
                new_block.append(copy.deepcopy(prop))
        blocks.append(new_block)
        prev_name = new_name
# Sort blocks using the same logic as blocks_doorsecure
blocks.sort(key=lambda b: split_main_color(b.get('name', '')))
# Write output
out_root = ET.Element('blocks')
for b in blocks:
    out_root.append(b)
ET.indent(out_root, space='    ')
ET.ElementTree(out_root).write(OUTPUT_FILE, encoding='utf-8', xml_declaration=True)
print(f'Wrote {OUTPUT_FILE}')
