"""Transfer changelogs from _x2.6/**/README.md to 01_Draft/README.txt

Searches _x2.6/ directly and inside all .Optionals-* folders.
Formats changelog to match the BackpackPlus style with --- separators.
"""
import os, re, subprocess

DRAFT_DIR = r"c:\GitHub\7D2D-Mods\01_Draft"
X2_DIR = r"c:\GitHub\7D2D-Mods\90_Archive\old-game-versions\_x2.6"
WORKSPACE = r"c:\GitHub\7D2D-Mods"

# Handle mods that were renamed between versions
# The _x2.6 has old folder names, draft has new folder names
NAME_MAP = {
    'AGF-4Modders-ItemTypeIconColor-v2.0.0': 'AGF-NoEAC-ItemTypeIconColor-v1.0.3',
}

def get_mod_name(folder_name):
    """Strip version suffix to get base mod name"""
    match = re.match(r'(.+)-v?\d[\d.-]*$', folder_name)
    return match.group(1) if match else folder_name

def find_x2_md(draft_name):
    """Find README.md for this mod anywhere in _x2.6 tree"""
    # Check name map first
    if draft_name in NAME_MAP:
        mapped = NAME_MAP[draft_name]
        search_paths = [
            os.path.join(X2_DIR, mapped, "README.md"),
        ]
        # Also check .Optionals
        for x2f in sorted(os.listdir(X2_DIR)):
            x2_sub = os.path.join(X2_DIR, x2f)
            if os.path.isdir(x2_sub) and x2f.startswith('.Optionals'):
                search_paths.append(os.path.join(x2_sub, mapped, "README.md"))
        for p in search_paths:
            if os.path.exists(p):
                return p
    
    draft_base = get_mod_name(draft_name)
    
    # Search _x2.6 root first
    for x2f in sorted(os.listdir(X2_DIR)):
        x2_path = os.path.join(X2_DIR, x2f, "README.md")
        if os.path.exists(x2_path):
            if get_mod_name(x2f) == draft_base:
                return x2_path
    
    # Then search .Optionals directories
    for x2f in sorted(os.listdir(X2_DIR)):
        x2_sub = os.path.join(X2_DIR, x2f)
        if os.path.isdir(x2_sub) and x2f.startswith('.Optionals'):
            for sub in sorted(os.listdir(x2_sub)):
                md_path = os.path.join(x2_sub, sub, "README.md")
                if os.path.exists(md_path):
                    if get_mod_name(sub) == draft_base:
                        return md_path
    
    return None

def extract_changelog(md_path):
    """Extract changelog text from <!-- CHANGELOG START --> markers"""
    with open(md_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    m = re.search(r'<!-- CHANGELOG START -->\s*(.*?)\s*<!-- CHANGELOG END -->', content, re.DOTALL)
    if m:
        return m.group(1).strip()
    return None

def format_changelog(text):
    """Format to match the BackpackPlus style with --- separators between versions"""
    lines = text.split('\n')
    
    result_lines = []
    version_count = 0
    
    for line in lines:
        stripped = line.strip()
        if not stripped:
            continue
        
        # Version headers like vX.Y.Z
        if re.match(r'^v?\d[\d.]*', stripped):
            if version_count > 0:
                result_lines.append('')
                result_lines.append('------------------------------------------------------------------------')
                result_lines.append('')
            else:
                result_lines.append('')
            
            result_lines.append(stripped)
            version_count += 1
        elif stripped.startswith('- '):
            result_lines.append('    - ' + stripped[2:].strip())
        elif stripped.startswith('* '):
            result_lines.append('    - ' + stripped[2:].strip())
        elif stripped:
            result_lines.append('    - ' + stripped)
    
    return '\n'.join(result_lines)

# Restore from git
print("Restoring from git...")
subprocess.run(['git', '-C', WORKSPACE, 'checkout', '--', '.'], capture_output=True)
print()

total_placeholders = 0
success = 0
no_match = []
no_changelog = []
already_had = 0

for draft_folder in sorted(os.listdir(DRAFT_DIR)):
    readme_path = os.path.join(DRAFT_DIR, draft_folder, "README.txt")
    if not os.path.isdir(os.path.join(DRAFT_DIR, draft_folder)) or not os.path.exists(readme_path):
        continue
    
    with open(readme_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Find changelog section header
    m = re.search(r'(={2,}\s*CHANGELOG\s*={2,})\s*\n', content)
    if not m:
        continue
    
    # Check if content after header is placeholder
    after = content[m.end():]
    if not re.search(r'Notes|Add changelog entries', after):
        already_had += 1
        continue
    
    total_placeholders += 1
    
    # Find the old README.md
    md_path = find_x2_md(draft_folder)
    if not md_path:
        no_match.append(draft_folder)
        continue
    
    changelog = extract_changelog(md_path)
    if not changelog:
        no_changelog.append(draft_folder)
        continue
    
    formatted = format_changelog(changelog)
    
    # Replace the placeholder section
    new_content = content[:m.end()] + formatted + '\n'
    
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write(new_content)
    
    x2_rel = os.path.relpath(md_path, X2_DIR)
    print(f"  {draft_folder}: Changelog transferred from {x2_rel} ✓")
    success += 1

print(f"\nResults:")
print(f"  Total placeholder changelogs found: {total_placeholders}")
print(f"  Transferred successfully: {success}")
print(f"  Already had real content: {already_had}")
print(f"  No _x2.6 match found: {len(no_match)}")
for f in no_match:
    print(f"    - {f}")
print(f"  Had _x2.6 match but no changelog extractable: {len(no_changelog)}")
for f in no_changelog:
    print(f"    - {f}")