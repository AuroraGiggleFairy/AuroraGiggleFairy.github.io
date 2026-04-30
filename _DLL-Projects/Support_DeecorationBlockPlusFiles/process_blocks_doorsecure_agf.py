"""
AGF DoorSecure AGF XML Processor

This script processes blocks_doorsecure_agf.xml for further transformation or analysis.
Add your custom logic below as needed.
"""
import xml.etree.ElementTree as ET
import copy
import re

INPUT_FILE = 'blocks_doorsecure_agf.xml'
OUTPUT_FILE = 'blocks_doorsecure_agf_processed.xml'

def main():
    tree = ET.parse(INPUT_FILE)
    root = tree.getroot()
    # Example: Copy all blocks to a new file (customize as needed)
    out_root = ET.Element('blocks')
    for block in root.findall('block'):
        out_root.append(copy.deepcopy(block))
        # Find mesh damage properties
        mesh_props = [p for p in block.findall('property') if 'MeshDamage' in p.get('name','')]
        # Find DMG1
        has_dmg1 = any('DMG1' in (p.get('value') or '') for p in mesh_props) or any('DMG1' in (p.text or '') for p in mesh_props)
        if mesh_props and has_dmg1:
            # Get max damage
            maxdmg = None
            for p in block.findall('property'):
                if p.get('name') == 'MaxDamage':
                    try:
                        maxdmg = int(float(p.get('value')))
                    except Exception:
                        pass
            if maxdmg is None:
                continue
            # Build new block
            new_block = copy.deepcopy(block)
            new_block.set('name', block.get('name') + 'Clean')
            # Rewrite mesh damage
            for p in new_block.findall('property'):
                if 'MeshDamage' in p.get('name',''):
                    mesh = p.get('value')
                    if mesh:
                        # Parse mesh string into DMG stages
                        parts = re.split(r'(,| )', mesh)
                        stages = []
                        i = 0
                        while i < len(parts):
                            part = parts[i].strip()
                            if part.startswith('DMG'):
                                stage = part
                                # Find value
                                j = i+1
                                while j < len(parts) and not parts[j].strip().replace('-','').replace('.','').isdigit():
                                    j += 1
                                if j < len(parts):
                                    val = parts[j].strip()
                                    stages.append((stage, val))
                                i = j
                            i += 1
                        # Rebuild mesh: DMG0 = max+1, DMG1 = max, rest rescaled
                        new_stages = [('DMG0', str(maxdmg+1)), ('DMG1', str(maxdmg))]
                        orig_hp = None
                        if len(stages) > 1:
                            try:
                                orig_hp = int(stages[1][1])
                            except Exception:
                                orig_hp = maxdmg
                        for idx, (stage, val) in enumerate(stages):
                            if idx < 2:
                                continue
                            try:
                                v = int(val)
                                scaled = max(1, int(round(v * maxdmg / orig_hp))) if orig_hp else v
                                new_stages.append((stage, str(scaled)))
                            except Exception:
                                new_stages.append((stage, val))
                        mesh_str = '   ' + ',   '.join([f'Door/{s}, {v}' for s,v in new_stages]) + '   '
                        p.set('value', mesh_str)
            out_root.append(new_block)
    ET.indent(out_root, space='    ')
    ET.ElementTree(out_root).write(OUTPUT_FILE, encoding='utf-8', xml_declaration=True)
    print(f'Wrote {OUTPUT_FILE}')

if __name__ == "__main__":
    main()
