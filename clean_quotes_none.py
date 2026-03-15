import os

QUOTES_DIR = '_Quotes'
changed = 0
for fname in os.listdir(QUOTES_DIR):
    if fname.endswith('.txt'):
        fpath = os.path.join(QUOTES_DIR, fname)
        with open(fpath, 'r', encoding='utf-8') as f:
            content = f.read().strip()
        if content == 'None':
            with open(fpath, 'w', encoding='utf-8') as f:
                f.write('')
            print(f'Cleared "None" from: {fname}')
            changed += 1
print(f'Checked all quote files. Cleared {changed} file(s).')
