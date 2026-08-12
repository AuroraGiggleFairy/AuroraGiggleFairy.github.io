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
    """Return {base_lower: (step1, step2, sort1_string)}."""
    sort_map = {}
    with open(csv_path, newline='', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            base = row['Name']
            step1 = int(row['Sort Step 1'])
            step2 = int(row['Sort Step 2'])
            # Zero-pad both so creative-menu string concat stays ordered.
            sort1 = f"AGF{step1:02d}{step2:03d}"
            sort_map[base.lower()] = (step1, step2, sort1)
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

SHAPE_GRID_COLS = 12
# One blank look per helper so Destroy returns the matching door helper.
# Compact: pad only at section end (no Boarded/Plain split, no per-family pads).
# Boarded-then-Plain: all Boarded in section first, pad, then all Plain (no per-family pads).
# Other sections: pad after color-heavy families; Boarded row(s) then Plain per family.
COMPACT_SECTIONS = {1, 3, 5, 9, 10, 15}
BOARD_PLAIN_SECTIONS = {}
# Insert one full empty grid row before these sections (after padding prior section).
BLANK_ROW_BEFORE_SECTIONS = {6, 7, 12}
COLOR_FAMILY_PAD_MIN = 6  # pad after a family if it used this many alts (color sets)

def strip_agf_tier(name):
    n = name or ''
    if n.endswith('Clean'):
        n = n[:-5]
    for tier in ('AGFWood', 'AGFIron', 'AGFSteel', 'AGFPowered'):
        if tier in n:
            return n.replace(tier, '')
    return n

def family_key_for_alt(name):
    return get_base_name_for_sort(strip_agf_tier(name))


def build_padded_place_alts(sorted_blocks, tier_token):
    """Insert shape spacer *tokens* so sections/families align to the 12-wide shape grid.

    Spacers are marked as the literal string '__SPACER__' and expanded to unique
    blocks later (shape UI sorts by Block.SortOrder, so shared spacers collapse).
    """
    out = []
    prev_section = None
    prev_family = None
    prev_clean = None
    family_count = 0

    def pad():
        rem = len(out) % SHAPE_GRID_COLS
        if rem:
            out.extend(['__SPACER__'] * (SHAPE_GRID_COLS - rem))

    for block in sorted_blocks:
        name = block.get('name') or ''
        if tier_token not in name:
            continue
        try:
            section = int(block.get('agf_step1', '99'))
        except Exception:
            section = 99
        family = family_key_for_alt(name)

        try:
            clean = int(block.get('agf_clean', '1' if name.endswith('Clean') else '0'))
        except Exception:
            clean = 1 if name.endswith('Clean') else 0

        if prev_section is not None and section != prev_section:
            pad()
            if section in BLANK_ROW_BEFORE_SECTIONS:
                out.extend(['__SPACER__'] * SHAPE_GRID_COLS)
            family_count = 0
        elif (
            prev_family is not None
            and family != prev_family
            and prev_section not in COMPACT_SECTIONS
            and prev_section not in BOARD_PLAIN_SECTIONS
            and family_count >= COLOR_FAMILY_PAD_MIN
        ):
            pad()
            family_count = 0
        elif (
            # Whole-section Boarded row(s) then Plain row(s).
            prev_section == section
            and prev_section in BOARD_PLAIN_SECTIONS
            and prev_clean == 0
            and clean == 1
        ):
            pad()
        elif (
            # Color families: finish all Boarded on their row(s), then Plain on the next.
            prev_family == family
            and prev_section == section
            and prev_section not in COMPACT_SECTIONS
            and prev_section not in BOARD_PLAIN_SECTIONS
            and prev_clean == 0
            and clean == 1
        ):
            pad()

        out.append(name)
        if prev_family == family and prev_section == section:
            family_count += 1
        else:
            family_count = 1
        prev_section = section
        prev_family = family
        prev_clean = clean

    pad()
    return out

def make_shape_spacer_block(name, helper_drop, sort1):
    """Blank shape-menu spacer: empty CustomIcon in UI; placeable wood cube in-world.

    Matches the old blankAGF pattern (Shape=New + Cube.fbx + wood texture 241).
    Do not use Shape=Cube (NRE) or Shape=Invisible (unseen / non-hittable).
    CustomIcon must be empty (value="") so the shape menu shows a blank tile.
    """
    block = ET.Element('block', {'name': name})
    ET.SubElement(block, 'property', {'name': 'CreativeMode', 'value': 'None'})
    ET.SubElement(block, 'property', {'name': 'CustomIcon', 'value': ''})
    ET.SubElement(block, 'property', {'name': 'DescriptionKey', 'value': 'miscDoorShapeSpacerAGFDesc'})
    ET.SubElement(block, 'property', {'name': 'Material', 'value': 'Mwood_weak'})
    ET.SubElement(block, 'property', {'name': 'Shape', 'value': 'New'})
    ET.SubElement(block, 'property', {'name': 'Model', 'value': '@:Shapes/Cube.fbx'})
    ET.SubElement(block, 'property', {'name': 'Texture', 'value': '241'})
    ET.SubElement(block, 'property', {'name': 'WaterFlow', 'value': 'permitted'})
    ET.SubElement(block, 'property', {'name': 'MaxDamage', 'value': '5'})
    ET.SubElement(block, 'property', {'name': 'EconomicValue', 'value': '1'})
    ET.SubElement(block, 'property', {'name': 'SellableToTrader', 'value': 'false'})
    ET.SubElement(block, 'property', {'name': 'Group', 'value': 'Building'})
    ET.SubElement(block, 'property', {'name': 'SortOrder1', 'value': sort1})
    ET.SubElement(block, 'property', {'name': 'SortOrder2', 'value': '0'})
    ET.SubElement(block, 'property', {'name': 'ShapeMenu', 'value': 'true'})
    ET.SubElement(block, 'drop', {'event': 'Destroy', 'name': helper_drop, 'count': '1'})
    ET.SubElement(block, 'drop', {'event': 'Fall', 'name': helper_drop, 'count': '1', 'prob': '1', 'stick_chance': '1'})
    return block

def apply_placealt_sort_orders(blocks_by_name, alt_names, tier_token, helper_drop, spacer_prefix):
    """Expand __SPACER__ tokens, assign sequential SortOrder so shape UI matches PlaceAlt."""
    resolved = []
    spacers = []
    spacer_i = 0
    for idx, name in enumerate(alt_names):
        sort1 = f"AGF{idx:05d}"
        if name == '__SPACER__':
            sname = f"{spacer_prefix}{spacer_i:04d}"
            spacer_i += 1
            spacers.append(make_shape_spacer_block(sname, helper_drop, sort1))
            resolved.append(sname)
            continue
        block = blocks_by_name.get(name)
        if block is not None:
            for prop in block.findall("property[@name='SortOrder1']"):
                block.remove(prop)
            for prop in block.findall("property[@name='SortOrder2']"):
                block.remove(prop)
            ET.SubElement(block, 'property', {'name': 'SortOrder1', 'value': sort1})
            ET.SubElement(block, 'property', {'name': 'SortOrder2', 'value': '0'})
        resolved.append(name)
    return resolved, spacers

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

# Dev/test door models (e.g. elevatorTest) — do not ship in DoorsPlus.
TEST_DOOR_NAME_SUBSTR = (
    'test',
    'debug',
    'prototype',
    'dummy',
    'placeholder',
)

def is_test_door_name(name):
    n = (name or '').lower()
    return any(s in n for s in TEST_DOOR_NAME_SUBSTR)

def is_powered_twin_door_name(name):
    """Vanilla *_Powered models duplicate unpowered meshes; skip (AGF Powered tier covers it)."""
    return '_Powered' in (name or '')

def is_porta_potty_unit_name(name):
    """Full porta-potty cabinets — keep only portaPottyDoor* in DoorsPlus."""
    n = (name or '').lower()
    if 'portapottydoor' in n:
        return False
    return 'portapotty' in n

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
        if is_test_door_name(orig_name) or is_powered_twin_door_name(orig_name) or is_porta_potty_unit_name(orig_name):
            continue
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
        # Upgrade chain stops at Steel. Powered is a standalone steel-quality tier.
        variants = [
            ('Wood', 'Mwood_regular', '1000', 'resourceWood', '10', 'Iron', 'resourceForgedIron', '10', '5', 'miscwoodDoorVariantHelper'),
            ('Iron', 'Mmetal', '5000', 'resourceForgedIron', '10', 'Steel', 'resourceForgedSteel', '10', '5', 'miscironDoorVariantHelper'),
            ('Steel', 'Msteel', '15000', 'resourceForgedSteel', '10', None, None, None, None, 'miscsteelDoorVariantHelper'),
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
                # PoweredDoor still exists in v3.1 (vanilla powered garages/vaults).
                for prop in new_block.findall("property[@name='Class']"):
                    new_block.remove(prop)
                for prop in new_block.findall("property[@name='ItemTypeIcon']"):
                    new_block.remove(prop)
                for prop in new_block.findall("property[@name='Tags']"):
                    new_block.remove(prop)
                ET.SubElement(new_block, 'property', {'name': 'Class', 'value': 'PoweredDoor'})
                ET.SubElement(new_block, 'property', {'name': 'ItemTypeIcon', 'value': 'electric_power'})
                ET.SubElement(new_block, 'property', {'name': 'Tags', 'value': 'door,electricianSkill'})
            else:
                # Keep CompositeTileEntity from flattened vanilla source.
                for prop in new_block.findall("property[@name='Class']"):
                    new_block.remove(prop)
                ET.SubElement(new_block, 'property', {'name': 'Class', 'value': 'CompositeTileEntity'})
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
                    if prop.get('name') == 'Class':
                        continue
                    if 'MeshDamage' in (prop.get('name') or ''):
                        continue
                new_block.append(copy.deepcopy(prop))

            # Set SortOrder1 using the CSV mapping for all color variants, append 0 for regular, 1 for Clean
            base_name = get_base_name_for_sort(orig_name)
            step1, step2, sort1 = sort_map.get(base_name.lower(), (99, 999, "AGF99999"))
            # Determine if this is a Clean variant
            is_clean = new_block.get('name', '').endswith('Clean')
            sort1_full = f"{sort1}{'1' if is_clean else '0'}"
            # Stash numeric keys for PlaceAlt ordering (not written to XML).
            new_block.set('agf_step1', str(step1))
            new_block.set('agf_step2', str(step2))
            new_block.set('agf_clean', '1' if is_clean else '0')
            # Remove any existing SortOrder1 property
            for prop in new_block.findall("property[@name='SortOrder1']"):
                new_block.remove(prop)
            ET.SubElement(new_block, 'property', {'name': 'SortOrder1', 'value': sort1_full})

            blocks.append(new_block)
    # 2. Add Clean variants for any with DMG1
    new_blocks = []
    clean_map = {'Wood': [], 'Iron': [], 'Steel': [], 'Powered': []}
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
            is_test_door_name(name) or
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
            # Inherit section keys; mark as Clean for PlaceAlt ordering.
            new_block.set('agf_step1', block.get('agf_step1', '99'))
            new_block.set('agf_step2', block.get('agf_step2', '999'))
            new_block.set('agf_clean', '1')
            new_blocks.append(new_block)
            # Add Clean block to correct helper group
            if 'AGFWood' in block.get('name'):
                clean_map['Wood'].append(new_block.get('name'))
            elif 'AGFIron' in block.get('name'):
                clean_map['Iron'].append(new_block.get('name'))
            elif 'AGFSteel' in block.get('name'):
                clean_map['Steel'].append(new_block.get('name'))
            elif 'AGFPowered' in block.get('name'):
                clean_map['Powered'].append(new_block.get('name'))
    # Add ItemTypeIcon property to all Clean variants before sorting properties
    for clean_block in new_blocks:
        # Remove any existing ItemTypeIcon property
        for prop in clean_block.findall("property[@name='ItemTypeIcon']"):
            clean_block.remove(prop)
        # Add the new ItemTypeIcon property
        clean_block.append(ET.Element('property', {'name': 'ItemTypeIcon', 'value': 'paint_bucket'}))
    blocks.extend(new_blocks)
    # 3. Sort by helper material group, then CSV section/order, SortOrder2, Boarded before Plain
    def get_material_group(name):
        if 'AGFWood' in name:
            return 0
        elif 'AGFIron' in name:
            return 1
        elif 'AGFSteel' in name:
            return 2
        elif 'AGFPowered' in name:
            return 3
        else:
            return 4

    def get_sort_order(block):
        name = block.get('name', '')
        group = get_material_group(name)
        try:
            step1 = int(block.get('agf_step1', '99'))
        except Exception:
            step1 = 99
        try:
            step2 = int(block.get('agf_step2', '999'))
        except Exception:
            step2 = 999
        try:
            clean = int(block.get('agf_clean', '1' if name.endswith('Clean') else '0'))
        except Exception:
            clean = 1 if name.endswith('Clean') else 0
        sort2 = None
        for prop in block.findall('property'):
            if prop.get('name') == 'SortOrder2':
                sort2 = prop.get('value')
                break
        try:
            sort2_n = int(float(sort2)) if sort2 is not None else 0
        except Exception:
            sort2_n = 0
        # Boarded (clean=0) of a family before Plain (clean=1), then by color.
        # Compact: keep each color as Boarded then Plain (B,P,B,P…) on one row.
        # Boarded-then-Plain sections: all Boarded in the section before any Plain.
        if step1 in BOARD_PLAIN_SECTIONS:
            return (group, step1, clean, step2, sort2_n, name)
        if step1 in COMPACT_SECTIONS:
            return (group, step1, step2, sort2_n, clean, name)
        return (group, step1, step2, clean, sort2_n, name)

    blocks.sort(key=get_sort_order)

    def ordered_names(tier_token):
        return [b.get('name') for b in blocks if tier_token in (b.get('name') or '')]

    # 4. Variant helpers — PlaceAlt + sequential SortOrder (shape UI sorts by SortOrder)
    blocks_by_name = {b.get('name'): b for b in blocks}
    wood_raw = build_padded_place_alts(blocks, 'AGFWood')
    iron_raw = build_padded_place_alts(blocks, 'AGFIron')
    steel_raw = build_padded_place_alts(blocks, 'AGFSteel')
    powered_raw = build_padded_place_alts(blocks, 'AGFPowered')
    wood_doors, wood_spacers = apply_placealt_sort_orders(
        blocks_by_name, wood_raw, 'AGFWood', wood_helper, 'miscDoorShapeSpacerWoodAGF_')
    iron_doors, iron_spacers = apply_placealt_sort_orders(
        blocks_by_name, iron_raw, 'AGFIron', iron_helper, 'miscDoorShapeSpacerIronAGF_')
    steel_doors, steel_spacers = apply_placealt_sort_orders(
        blocks_by_name, steel_raw, 'AGFSteel', steel_helper, 'miscDoorShapeSpacerSteelAGF_')
    powered_doors, powered_spacers = apply_placealt_sort_orders(
        blocks_by_name, powered_raw, 'AGFPowered', powered_helper, 'miscDoorShapeSpacerPoweredAGF_')
    spacers = wood_spacers + iron_spacers + steel_spacers + powered_spacers
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

    # SortOrder1 is assigned sequentially from PlaceAlt positions (shape UI sorts by SortOrder).

    # Write only <block> elements, no XML declaration or <blocks> parent
    def strip_agf_stash(block):
        for key in ('agf_step1', 'agf_step2', 'agf_clean'):
            if key in block.attrib:
                del block.attrib[key]

    all_blocks = []
    for b in blocks:
        strip_agf_stash(b)
        sort_block_properties(b)
        all_blocks.append(b)
    for s in spacers:
        sort_block_properties(s)
        all_blocks.append(s)
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
