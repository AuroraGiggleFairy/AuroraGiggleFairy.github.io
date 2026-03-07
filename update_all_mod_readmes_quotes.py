import os
import re

# List all mod folders (exclude root README.md and templates)
folders = [f for f in os.listdir('.') if os.path.isdir(f) and not f.startswith('_') and not f.startswith('.')]
readme_files = []
for folder in folders:
    readme_path = os.path.join(folder, 'README.md')
    if os.path.exists(readme_path):
        readme_files.append(readme_path)

def update_readme_with_quote(readme_path):
    with open(readme_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    # Find version line
    version_idx = None
    for i, line in enumerate(lines):
        if line.strip().startswith('**Version:'):
            version_idx = i
            break
    if version_idx is None:
        return False
    # Check if next non-blank line is already a quote
    insert_idx = version_idx + 1
    while insert_idx < len(lines) and lines[insert_idx].strip() == '':
        insert_idx += 1
    already_quote = False
    if insert_idx < len(lines) and lines[insert_idx].strip().startswith('>'):
        already_quote = True
        # Optionally, fix formatting: ensure single * around quote if not blank
        quote_line = lines[insert_idx].strip()
        quote_content = quote_line[1:].strip().split('<!--')[0].strip()
        if quote_content and not (quote_content.startswith('*') and quote_content.endswith('*')):
            quote_content = f'*{quote_content.strip('* ')}*'
        # Preserve comment if present
        comment = ''
        if '<!--' in quote_line:
            comment = ' ' + quote_line[quote_line.index('<!--'):].rstrip()
        lines[insert_idx] = f'> {quote_content}{comment}\n'
    else:
        # Insert quote line after version
        lines.insert(version_idx+1, '>  <!-- Leave blank if no quote. If present, surround with single * for italics. -->\n')
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    return True

if __name__ == '__main__':
    updated = 0
    for readme in readme_files:
        if update_readme_with_quote(readme):
            updated += 1
    print(f'Updated {updated} mod README.md files.')
