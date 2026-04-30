import csv
import os
import xml.etree.ElementTree as ET

# Paths
GAME_LOCALIZATION = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\Localization.txt"
WORKSPACE_LOCALIZATION = "Localization.txt"
BLOCKS_XML = "blocks_doorsecure_agf_all.xml"


# Read header from game localization (always present and correct)
with open(GAME_LOCALIZATION, encoding="utf-8") as f:
    header = f.readline().strip().split(",")

# Read game localization into a dict: {Key: row_dict}
game_loc = {}
with open(GAME_LOCALIZATION, encoding="utf-8") as f:
    reader = csv.DictReader(f, delimiter=',')
    for row in reader:
        game_loc[row['Key']] = row



# Parse all block names and collect VariantHelperAGF blocks separately
block_names = []
variant_helper_names = []
with open(BLOCKS_XML, encoding="utf-8") as f:
    xml_content = f.read()
wrapped_xml = f"<blocks>\n{xml_content}\n</blocks>"
root = ET.fromstring(wrapped_xml)
for block in root.findall('block'):
    name = block.get('name')
    if name:
        if 'VariantHelperAGF' in name:
            variant_helper_names.append(name)
        else:
            block_names.append(name)

# Helper: get base name (strip AGF* and Clean)
def get_base_name(name):
    base = name
    if 'AGF' in base:
        base = base.split('AGF')[0]
    if base.endswith('Clean'):
        base = base[:-5]
    return base

# Prepare new localization rows
new_rows = []
plain_translations = {
    'english': 'Plain',
    'german': 'Klar',
    'spanish': 'Simple',
    'french': 'Simple',
    'italian': 'Semplice',
    'japanese': 'プレーン',
    'koreana': '플레인',
    'polish': 'Prosta',
    'brazilian': 'Simples',
    'russian': 'Простой',
    'turkish': 'Sade',
    'schinese': '简洁',
    'tchinese': '簡潔',
}
for name in block_names:
    base = get_base_name(name)
    base_row = game_loc.get(base)
    if not base_row:
        row = {h: '' for h in header}
        row['Key'] = name
        row['english'] = name
    else:
        row = dict(base_row)
        row['Key'] = name
    # Append material indicator for each block type
    indicator_key = None
    if 'AGFWood' in name:
        indicator_key = 'xuifWood'
    elif 'AGFIron' in name:
        indicator_key = 'xuifIron'
    elif 'AGFSteel' in name:
        indicator_key = 'xuifSteel'
    elif 'AGFPowered' in name:
        indicator_key = 'xuiPower'
    if indicator_key:
        indicator_row = game_loc.get(indicator_key, {})
        for h in header:
            if h not in {"Key", "File", "Type", "UsedInMainMenu", "NoTranslate", "Context / Alternate Text"}:
                val = row.get(h, '')
                material_translation = indicator_row.get(h, '')
                indicator = f'[ddcdfa]({material_translation})[-]' if material_translation else '[ddcdfa]()[-]'
                # If this is a Clean variant, insert Plain before the indicator
                if name.endswith('Clean'):
                    plain = plain_translations.get(h, 'Plain')
                    # Move indicator to the start, no space after [-]
                    row[h] = f'{indicator} {val} [decea3]{plain}[-]' if val else f'{indicator} [decea3]{plain}[-]'
                else:
                    # Move indicator to the start for regular blocks too
                    row[h] = f'{indicator} {val}' if val else indicator
    new_rows.append(row)

# Append to workspace Localization.txt


def get_xuiAllDoors_row(header):
    translations = {
        'english': 'All Doors',
        'german': 'Alle Türen',
        'spanish': 'Todas las puertas',
        'french': 'Toutes les portes',
        'italian': 'Tutte le porte',
        'japanese': 'すべてのドア',
        'koreana': '모든 문',
        'polish': 'Wszystkie drzwi',
        'brazilian': 'Todas as portas',
        'russian': 'Все двери',
        'turkish': 'Tüm Kapılar',
        'schinese': '所有门',
        'tchinese': '所有門',
    }
    row = {h: '' for h in header}
    row['Key'] = 'xuiAllDoors'
    for h in header:
        if h in translations:
            row[h] = translations[h]
    return row

def quote_row_entries(row, header):
    quoted = {}
    for h in header:
        # Only quote non-empty entries
        val = row.get(h, '')
        quoted[h] = f'"{val}"' if val != '' else ''
    return quoted


# Write rows with all entries quoted, comma-separated

# Columns that should NOT be quoted
no_quote = {"Key", "File", "Type", "UsedInMainMenu", "NoTranslate", "Context / Alternate Text"}




with open(WORKSPACE_LOCALIZATION, 'w', encoding="utf-8", newline='') as f:
    f.write(','.join(header) + '\n')

    # Write all other rows first
    for row in new_rows:
        quoted = []
        for h in header:
            val = row.get(h, '')
            if h in no_quote:
                quoted.append(val)
            else:
                quoted.append(f'"{val}"')
        f.write(','.join(quoted) + '\n')

    # Add xuiAllDoors key at the end
    xuiAllDoors_row = get_xuiAllDoors_row(header)
    quoted = []
    for h in header:
        val = xuiAllDoors_row.get(h, '')
        if h in no_quote:
            quoted.append(val)
        else:
            quoted.append(f'"{val}"')
    f.write(','.join(quoted) + '\n')

    # Add VariantHelperAGF localization rows at the very end
    material_keys = {
        'wood': 'xuifWood',
        'iron': 'xuifIron',
        'steel': 'xuifSteel',
        'powered': 'xuiPower',
    }
    for name in variant_helper_names:
        row = {h: '' for h in header}
        row['Key'] = name
        # Determine material type from block name
        if 'wood' in name.lower():
            material_key = material_keys['wood']
        elif 'iron' in name.lower():
            material_key = material_keys['iron']
        elif 'steel' in name.lower():
            material_key = material_keys['steel']
        elif 'powered' in name.lower():
            material_key = material_keys['powered']
        else:
            material_key = None
        # New format: 'All Doors [color][-][ddcdfa](Material Quality)[-]'
        color_tags = {
            'wood': '[aaaaaa][-]',
            'iron': '[aaaaab][-]',
            'steel': '[aaaaac][-]',
            'powered': '[aaaaad][-]',
        }
        material_names = {
            'wood': {
                'english': 'Wood Quality',
                'german': 'Holzqualität',
                'spanish': 'calidad de madera',
                'french': 'qualité du bois',
                'italian': 'qualità del legno',
                'japanese': '木材の品質',
                'koreana': '목재 품질',
                'polish': 'jakość drewna',
                'brazilian': 'qualidade da madeira',
                'russian': 'качество дерева',
                'turkish': 'Ahşap Kalitesi',
                'schinese': '木材质量',
                'tchinese': '木材品質',
            },
            'iron': {
                'english': 'Iron Quality',
                'german': 'Eisenqualität',
                'spanish': 'calidad de hierro',
                'french': 'qualité du fer',
                'italian': 'qualità del ferro',
                'japanese': '鉄の品質',
                'koreana': '철제 품질',
                'polish': 'jakość żelaza',
                'brazilian': 'qualidade do ferro',
                'russian': 'качество железа',
                'turkish': 'Demir Kalitesi',
                'schinese': '铁质量',
                'tchinese': '鐵品質',
            },
            'steel': {
                'english': 'Steel Quality',
                'german': 'Stahlqualität',
                'spanish': 'calidad de acero',
                'french': 'qualité de l’acier',
                'italian': 'qualità dell’acciaio',
                'japanese': '鋼の品質',
                'koreana': '강철 품질',
                'polish': 'jakość stali',
                'brazilian': 'qualidade do aço',
                'russian': 'качество стали',
                'turkish': 'Çelik Kalitesi',
                'schinese': '钢质量',
                'tchinese': '鋼品質',
            },
            'powered': {
                'english': 'Powered Steel',
                'german': 'Elektrisch betriebener Stahl',
                'spanish': 'Acero motorizado',
                'french': 'Acier motorisé',
                'italian': 'Acciaio motorizzato',
                'japanese': '電動鋼製',
                'koreana': '전동 강철',
                'polish': 'Stalowe, zasilane elektrycznie',
                'brazilian': 'Aço motorizado',
                'russian': 'Стальные с электроприводом',
                'turkish': 'Elektrikli Çelik',
                'schinese': '电动钢制门',
                'tchinese': '電動鋼製門',
            },
        }
        if 'wood' in name.lower():
            mat_type = 'wood'
        elif 'iron' in name.lower():
            mat_type = 'iron'
        elif 'steel' in name.lower() and 'powered' not in name.lower():
            mat_type = 'steel'
        elif 'powered' in name.lower():
            mat_type = 'powered'
        else:
            mat_type = None
        for h in header:
            if h in {'Key', 'File', 'Type', 'UsedInMainMenu', 'NoTranslate', 'Context / Alternate Text'}:
                continue
            color_tag = color_tags.get(mat_type, '')
            material_name = material_names.get(mat_type, {}).get(h, '')
            row[h] = f'All Doors {color_tag}[ddcdfa]({material_name})[-]'
        quoted = []
        for h in header:
            val = row.get(h, '')
            if h in no_quote:
                quoted.append(val)
            else:
                quoted.append(f'"{val}"')
        f.write(','.join(quoted) + '\n')

print(f"Wrote {len(new_rows)} rows to {WORKSPACE_LOCALIZATION}")
