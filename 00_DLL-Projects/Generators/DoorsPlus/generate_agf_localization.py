import csv
import os
import xml.etree.ElementTree as ET

# Paths
GAME_LOCALIZATION = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\Localization.csv"
WORKSPACE_LOCALIZATION = "Localization.csv"
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
        elif 'ShapeSpacer' in name:
            continue
        elif name.startswith('miscDoorChallengeHelper'):
            continue
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
    'german': 'Schlicht',
    'spanish': 'Lisa',
    'french': 'Simple',
    'italian': 'Semplice',
    'japanese': 'プレーン',
    'koreana': '플레인',
    'polish': 'Zwykłe',
    'brazilian': 'Simples',
    'russian': 'Обычная',
    'turkish': 'Düz',
    'schinese': '素面',
    'tchinese': '素面',
}
boarded_translations = {
    'english': 'Boarded',
    'german': 'Bretter',
    'spanish': 'Entablada',
    'french': 'Planches',
    'italian': 'Assi',
    'japanese': '板張り',
    'koreana': '판자',
    'polish': 'Deskowane',
    'brazilian': 'Tábuas',
    'russian': 'Доски',
    'turkish': 'Tahta Kaplı',
    'schinese': '木板',
    'tchinese': '木板',
}

# Only doors with a Clean twin should be labeled Boarded.
block_name_set = set(block_names)

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
        has_clean_twin = (not name.endswith('Clean')) and ((name + 'Clean') in block_name_set)
        for h in header:
            if h not in {"Key", "File", "Type", "UsedInMainMenu", "NoTranslate", "KeepLoaded", "Context / Alternate Text"}:
                val = row.get(h, '')
                material_translation = indicator_row.get(h, '')
                indicator = f'[ddcdfa]({material_translation})[-]' if material_translation else '[ddcdfa]()[-]'
                if name.endswith('Clean'):
                    style = plain_translations.get(h, 'Plain')
                    row[h] = f'{indicator} {val} [decea3]{style}[-]' if val else f'{indicator} [decea3]{style}[-]'
                elif has_clean_twin:
                    style = boarded_translations.get(h, 'Boarded')
                    row[h] = f'{indicator} {val} [decea3]{style}[-]' if val else f'{indicator} [decea3]{style}[-]'
                else:
                    row[h] = f'{indicator} {val}' if val else indicator
    # Ensure File/Type for AGF CSV shape
    row['File'] = row.get('File') or 'blocks'
    row['Type'] = row.get('Type') or 'Block'
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

# Metadata stays unquoted. Every language cell and Context is always quoted.
META_UNQUOTED = {"Key", "File", "Type", "UsedInMainMenu", "NoTranslate", "KeepLoaded"}
LANG_COLS = {
    "english", "german", "spanish", "french", "italian", "japanese", "koreana",
    "polish", "brazilian", "russian", "turkish", "schinese", "tchinese",
}


def format_csv_row(row, header):
    cells = []
    for h in header:
        val = row.get(h, "") or ""
        if h in META_UNQUOTED:
            cells.append(val)
            continue
        # Context + all 13 languages: always quoted, including empty -> ""
        escaped = val.replace('"', '""')
        cells.append(f'"{escaped}"')
    return ",".join(cells)


def helper_material_type(name):
    n = name.lower()
    if "powered" in n:
        return "powered"
    if "wood" in n:
        return "wood"
    if "iron" in n:
        return "iron"
    if "steel" in n:
        return "steel"
    return None


HELPER_COLOR_TAGS = {
    "wood": "[aaaaaa][-]",
    "iron": "[aaaaab][-]",
    "steel": "[aaaaac][-]",
    "powered": "[aaaaad][-]",
}
HELPER_MATERIAL_NAMES = {
    "wood": {
        "english": "Wood Quality",
        "german": "Holzqualität",
        "spanish": "calidad de madera",
        "french": "qualité du bois",
        "italian": "qualità del legno",
        "japanese": "木材の品質",
        "koreana": "목재 품질",
        "polish": "jakość drewna",
        "brazilian": "qualidade da madeira",
        "russian": "качество дерева",
        "turkish": "Ahşap Kalitesi",
        "schinese": "木材质量",
        "tchinese": "木材品質",
    },
    "iron": {
        "english": "Iron Quality",
        "german": "Eisenqualität",
        "spanish": "calidad de hierro",
        "french": "qualité du fer",
        "italian": "qualità del ferro",
        "japanese": "鉄の品質",
        "koreana": "철제 품질",
        "polish": "jakość żelaza",
        "brazilian": "qualidade do ferro",
        "russian": "качество железа",
        "turkish": "Demir Kalitesi",
        "schinese": "铁质量",
        "tchinese": "鐵品質",
    },
    "steel": {
        "english": "Steel Quality",
        "german": "Stahlqualität",
        "spanish": "calidad de acero",
        "french": "qualité de l’acier",
        "italian": "qualità dell’acciaio",
        "japanese": "鋼の品質",
        "koreana": "강철 품질",
        "polish": "jakość stali",
        "brazilian": "qualidade do aço",
        "russian": "качество стали",
        "turkish": "Çelik Kalitesi",
        "schinese": "钢质量",
        "tchinese": "鋼品質",
    },
    "powered": {
        "english": "Powered Steel",
        "german": "Elektrisch betriebener Stahl",
        "spanish": "Acero motorizado",
        "french": "Acier motorisé",
        "italian": "Acciaio motorizzato",
        "japanese": "電動鋼製",
        "koreana": "전동 강철",
        "polish": "Stalowe, zasilane elektrycznie",
        "brazilian": "Aço motorizado",
        "russian": "Стальные с электроприводом",
        "turkish": "Elektrikli Çelik",
        "schinese": "电动钢制门",
        "tchinese": "電動鋼製門",
    },
}


def make_helper_row(name, header):
    row = {h: "" for h in header}
    row["Key"] = name
    row["File"] = "blocks"
    row["Type"] = "Block"
    mat_type = helper_material_type(name)
    color_tag = HELPER_COLOR_TAGS.get(mat_type, "")
    names = HELPER_MATERIAL_NAMES.get(mat_type, {})
    for h in header:
        if h not in LANG_COLS:
            continue
        material_name = names.get(h, "")
        row[h] = f"All Doors {color_tag}[ddcdfa]({material_name})[-]"
    return row


def make_challenge_helper_row(header):
    """Challenge UI uses Localization.Get(expectedBlock); keep vanilla Wood Door text."""
    src = game_loc.get("oldWoodDoor") or {}
    row = {h: "" for h in header}
    row["Key"] = "miscDoorChallengeHelperAGF"
    row["File"] = "blocks"
    row["Type"] = "Block"
    for h in header:
        if h in LANG_COLS:
            row[h] = src.get(h, "") or ""
    return row


with open(WORKSPACE_LOCALIZATION, "w", encoding="utf-8", newline="") as f:
    f.write(",".join(header) + "\n")

    for row in new_rows:
        f.write(format_csv_row(row, header) + "\n")

    f.write(format_csv_row(get_xuiAllDoors_row(header), header) + "\n")
    f.write(format_csv_row(make_challenge_helper_row(header), header) + "\n")

    for name in variant_helper_names:
        f.write(format_csv_row(make_helper_row(name, header), header) + "\n")

print(f"Wrote {len(new_rows)} rows to {WORKSPACE_LOCALIZATION}")
