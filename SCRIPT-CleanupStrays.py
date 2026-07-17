import os, re

draft = r'c:\GitHub\7D2D-Mods\01_Draft'

for folder in sorted(os.listdir(draft)):
    path = os.path.join(draft, folder, 'README.txt')
    if not os.path.isdir(os.path.join(draft, folder)) or not os.path.exists(path):
        continue
    
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original = content
    
    # 1. Remove lone dash lines (lines that are just "-" or " -" or "  -")
    content = re.sub(r'^[ \t]*-\s*$\n?', '', content, flags=re.MULTILINE)
    
    # 2. For DyesPlus specifically - remove duplicate "Core dye loop" blocks
    if 'Core dye loop' in content:
        # Find the OTHER DETAILS section
        section = re.search(r'OTHER DETAILS\n-{2,}\s*\n(.*?)(?=\n\s*\n\s*\n\s*={2,})', content, re.DOTALL)
        if section:
            details = section.group(1)
            # Count occurrences of "- Core dye loop:"
            count = len(re.findall(r'- Core dye loop:', details))
            if count > 1:
                # Keep only the first occurrence
                parts = details.split('- Core dye loop:')
                # parts[0] is lead text, parts[1] is first block, parts[2+] are duplicates
                lead = parts[0].strip()
                first_block = parts[1].strip()
                # Remove very end artifact if it matches pattern
                # Also remove any trailing "- " artifacts
                first_block = re.sub(r'\n\s*-\s*$', '', first_block)
                lead = re.sub(r'\n\s*-\s*$', '', lead)
                
                deduped = lead + '\n\n- Core dye loop:\n' + first_block
                # Remove any empty list item artifacts left
                deduped = re.sub(r'\n\s*-\s*\n', '\n', deduped)
                content = content.replace(section.group(1), '\n\n' + deduped + '\n\n')
    
    # 3. Clean up any " -\n" artifacts (dash followed by nothing)
    content = re.sub(r'\n[ \t]*-\s*\n', '\n', content)
    
    # 4. Handle stray "Implementation edits" prefix artifact lines
    # Sometimes a bare "-" ends up right before "Implementation edits"
    content = re.sub(r'-\s*\nImplementation edits', 'Implementation edits', content)
    
    # 5. Remove empty list items "- " at the end of lines before blank lines
    content = re.sub(r'\n\s*-\s*\n\n', '\n\n', content)
    
    # 6. Collapse excessive blank lines to max 2
    content = re.sub(r'\n{4,}', '\n\n\n', content)
    
    # 7. Ensure OTHER DETAILS content starts with a dash for the first bullet
    # Handle case where a bare "-" appears right after the header
    content = re.sub(r'(OTHER DETAILS\n-{2,}\s*\n)\s*-\n', r'\1  - ', content)
    
    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed: {folder}')
    else:
        print(f'OK: {folder}')