import os
import re

def markdown_to_text(md):
    # Remove code blocks
    md = re.sub(r'```[\s\S]*?```', '', md)
    # Remove images
    md = re.sub(r'!\[[^\]]*\]\([^\)]*\)', '', md)
    # Convert links to 'text: url' format
    md = re.sub(r'\[([^\]]+)\]\(([^\)]+)\)', r'\1: \2', md)
    # Remove bold/italic/inline code
    md = re.sub(r'[`*_~]', '', md)
    # Convert all --- to dividers (single newline after each)
    md = re.sub(r'^---+$', '\n' + '='*40 + '\n', md, flags=re.MULTILINE)
    # Remove headings
    md = re.sub(r'^#+\s*', '', md, flags=re.MULTILINE)
    # Remove blockquotes
    md = re.sub(r'^>\s?', '', md, flags=re.MULTILINE)
    # Remove HTML tags
    md = re.sub(r'<[^>]+>', '', md)
    # Remove extra whitespace
    md = re.sub(r'\n{3,}', '\n\n', md)
    return md.strip()

def postprocess_remove_dividers(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    compat_indices = [i for i, l in enumerate(lines) if l.strip().startswith('4. Compatibility')]
    if len(compat_indices) >= 2:
        start = compat_indices[1] + 1
        removed = 0
        i = start
        while i < len(lines) and removed < 3:
            if lines[i].lstrip().startswith('='):
                lines.pop(i)
                removed += 1
                continue
            i += 1
    with open(filepath, 'w', encoding='utf-8') as f:
        f.writelines(lines)

def convert_all_readmes():
    # Convert main README.md
    if os.path.exists('README.md'):
        with open('README.md', 'r', encoding='utf-8') as f:
            md = f.read()
        txt = markdown_to_text(md)
        with open('ReadableReadMe.txt', 'w', encoding='utf-8') as f:
            f.write(txt)
        remove_blank_between_dividers('ReadableReadMe.txt')

    # Convert all AGF-/zzzAGF- mod readmes
    for folder in os.listdir('.'):
        if os.path.isdir(folder) and (folder.startswith('AGF-') or folder.startswith('zzzAGF-')):
            readme_path = os.path.join(folder, 'README.md')
            txt_path = os.path.join(folder, 'ReadableReadMe.txt')
            if os.path.exists(readme_path):
                with open(readme_path, 'r', encoding='utf-8') as f:
                    md = f.read()
                txt = markdown_to_text(md)
                with open(txt_path, 'w', encoding='utf-8') as f:
                    f.write(txt)
                remove_blank_between_dividers(txt_path)

def remove_blank_between_dividers(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    new_lines = []
    i = 0
    while i < len(lines):
        if (
            lines[i].lstrip().startswith('=') and
            i + 2 < len(lines) and
            lines[i+1].strip() == '' and
            lines[i+2].lstrip().startswith('=')
        ):
            new_lines.append(lines[i])
            # skip the blank line
            i += 1
        else:
            new_lines.append(lines[i])
        i += 1
    with open(filepath, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)

if __name__ == '__main__':
    convert_all_readmes()