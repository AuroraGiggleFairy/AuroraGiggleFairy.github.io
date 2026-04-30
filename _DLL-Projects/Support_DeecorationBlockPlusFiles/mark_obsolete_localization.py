import csv

# Input and output file paths
INPUT_FILE = 'OLD Localization.txt'
OUTPUT_FILE = 'OLD_Localization_obsolete.txt'

# Read the old localization file
with open(INPUT_FILE, encoding='utf-8') as infile:
    reader = csv.DictReader(infile)
    header = reader.fieldnames
    rows = list(reader)

# Identify language columns (exclude known non-language columns)
non_language_cols = {'Key', 'File', 'Type', 'UsedInMainMenu', 'NoTranslate', 'Context / Alternate Text'}
language_cols = [col for col in header if col not in non_language_cols]


# Translations for 'Obsolete' in each language
obsolete_translations = {
    'english': 'Obsolete',
    'german': 'Veraltet',
    'spanish': 'Obsoleto',
    'french': 'Obsolète',
    'italian': 'Obsoleto',
    'japanese': '廃止',
    'koreana': '사용되지 않음',
    'polish': 'Przestarzałe',
    'brazilian': 'Obsoleto',
    'russian': 'Устаревшее',
    'turkish': 'Eski',
    'schinese': '废弃',
    'tchinese': '廢棄',
}

with open(OUTPUT_FILE, 'w', encoding='utf-8', newline='') as outfile:
    writer = csv.DictWriter(outfile, fieldnames=header)
    writer.writeheader()
    for row in rows:
        for lang in language_cols:
            # Use the translation if available, else default to 'Obsolete'
            translation = obsolete_translations.get(lang, 'Obsolete')
            row[lang] = f'[ff0000]{translation}[-]'
        writer.writerow(row)

print(f'Wrote obsolete localization to {OUTPUT_FILE}')
