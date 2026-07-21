import os
import re

DRAFT_DIR = r"c:\GitHub\7D2D-Mods\01_Draft"
X2_DIR = r"c:\GitHub\7D2D-Mods\90_Archive\old-game-versions\_x2.6"

def get_mod_name(folder_name):
    """Get mod name by stripping the version suffix"""
    match = re.match(r'(.+)-v?\d[\d.-]*$', folder_name)
    if match:
        return match.group(1)
    return folder_name

def find_x2_match(draft_name):
    """Find matching mod in _x2.6 by name (ignoring version)"""
    draft_base = get_mod_name(draft_name)
    exact_path = os.path.join(X2_DIR, draft_name)
    if os.path.isdir(exact_path):
        return draft_name
    
    for x2_folder in sorted(os.listdir(X2_DIR)):
        x2_path = os.path.join(X2_DIR, x2_folder)
        if os.path.isdir(x2_path):
            x2_base = get_mod_name(x2_folder)
            if x2_base == draft_base:
                return x2_folder
    return None

def extract_changelog_from_md(md_path):
    with open(md_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    changelog_match = re.search(r'<!-- CHANGELOG START -->\s*(.*?)\s*<!-- CHANGELOG END -->', content, re.DOTALL)
    if changelog_match:
        return changelog_match.group(1).strip()
    
    changelog_section = re.search(r'## 8\.\s*Changelog\s*(.*?)(?=\n##\s*\d+\.|\Z)', content, re.DOTALL)
    if changelog_section:
        text = changelog_section.group(1).strip()
        if '<!--' in text:
            return None
        return text
    
    return None

def format_changelog_for_txt(changelog_text):
    if not changelog_text:
        return ""
    
    lines = changelog_text.split('\n')
    result_lines = []
    
    for line in lines:
        stripped = line.strip()
        if not stripped:
            result_lines.append('')
            continue
        
        if re.match(r'^v?\d[\d.]*', stripped):
            version = stripped
            if result_lines and result_lines[-1]:
                result_lines.append('')
            result_lines.append(version)
            result_lines.append('')
        elif stripped.startswith('- '):
            result_lines.append('    - ' + stripped[2:].strip())
        elif stripped.startswith('* '):
            result_lines.append('    - ' + stripped[2:].strip())
        elif stripped:
            result_lines.append('    - ' + stripped)
    
    return '\n'.join(result_lines)

def check_changelog_is_placeholder(readme_path):
    with open(readme_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    changelog_section = re.search(r'={2,}\s*CHANGELOG\s*={2,}(.*?)(?:\Z|={2,})', content, re.DOTALL)
    if not changelog_section:
        return True
    
    changelog_text = changelog_section.group(1).strip()
    placeholder_pattern = r'^Notes\s*(?:\n\s*- Notes\s*)*\s*(?:\n\s*- Add changelog entries here\.\s*)?$'
    if re.match(placeholder_pattern, changelog_text):
        return True
    return False

def check_duplicated_content(readme_path):
    with open(readme_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    impl_matches = re.findall(r'Implementation edits used to make this work:', content)
    return len(impl_matches) > 1

def fix_other_details_section(content):
    """
    Fix the OTHER DETAILS section by removing duplicated blocks.
    Returns the fixed content, or None if no fix was needed.
    """
    # Find the OTHER DETAILS section
    section_match = re.search(
        r'(OTHER DETAILS\n-{2,}\s*\n)(.*?)(?=\n\s*\n\s*\n\s*={2,})',
        content, 
        re.DOTALL
    )
    if not section_match:
        return None
    
    section_header = section_match.group(1)
    details_content = section_match.group(2)
    original_details = details_content
    
    # Strategy: Find all blocks starting with "Implementation edits" or feature descriptors
    # and keep only the first occurrence of each
    
    parts = re.split(r'(\n\s*-\s*\n)', details_content)
    if len(parts) > 1:
        # Remove the "-\n" separators entirely
        combined = ''.join(parts).strip()
        details_content = combined
    
    # Now split by "Implementation edits used to make this work:"
    impl_blocks = re.split(r'(?=Implementation edits used to make this work:\s*)', details_content)
    
    if len(impl_blocks) > 1:
        lead_text = impl_blocks[0].strip() if impl_blocks[0].strip() else ""
        first_block = impl_blocks[1].strip()
        
        if lead_text:
            deduplicated = lead_text + '\n\n' + first_block
        else:
            deduplicated = first_block
        
        if deduplicated != details_content.strip():
            new_content = content.replace(original_details, '\n\n' + deduplicated + '\n\n')
            return new_content
    
    return None

def fix_other_details_v2(content):
    """
    More aggressive fix: for sections that have repeated content,
    remove duplicate blocks entirely.
    """
    section_match = re.search(
        r'(OTHER DETAILS\n-{2,}\s*\n)(.*?)(?=\n\s*\n\s*\n\s*={2,})',
        content, 
        re.DOTALL
    )
    if not section_match:
        return None
    
    section_header = section_match.group(1)
    details_content = section_match.group(2)
    original_details = details_content
    
    # Remove any bare "-\n" lines or "-\n\n-" artifacts
    details_content = re.sub(r'^-\s*$', '', details_content, flags=re.MULTILINE)
    
    # Find duplicated blocks: look for repeated "Implementation edits" or repeated feature phrases
    # Simple approach: split on known section markers and keep unique ones
    
    # Split into logical blocks (separated by blank lines)
    blocks = re.split(r'\n{2,}', details_content)
    
    seen_blocks = set()
    unique_blocks = []
    for block in blocks:
        stripped = block.strip()
        if not stripped:
            continue
        # Normalize the block for comparison
        normalized = re.sub(r'\s+', ' ', stripped)
        if normalized not in seen_blocks:
            seen_blocks.add(normalized)
            unique_blocks.append(stripped)
    
    if len(unique_blocks) < len([b for b in blocks if b.strip()]):
        # Some blocks were removed
        deduplicated = '\n\n'.join(unique_blocks)
        new_content = content.replace(original_details, '\n\n' + deduplicated + '\n\n')
        
        # Clean up: remove lines that are just "-" or " -" or "  -"
        new_content = re.sub(r'^\s*-\s*$\n?', '', new_content, flags=re.MULTILINE)
        # Clean up multiple blank lines
        new_content = re.sub(r'\n{4,}', '\n\n\n', new_content)
        
        return new_content
    
    return None

def main():
    import sys
    dry_run = '--dry-run' in sys.argv
    
    print("=" * 70)
    print(f"{'DRY RUN - ' if dry_run else ''}Checking draft mods for changelog and duplication issues...")
    print("=" * 70)
    
    total_draft = 0
    needs_changelog = []
    needs_dup_fix = []
    matching_x2 = []
    
    for draft_folder in sorted(os.listdir(DRAFT_DIR)):
        draft_path = os.path.join(DRAFT_DIR, draft_folder)
        if not os.path.isdir(draft_path):
            continue
        
        readme_path = os.path.join(draft_path, "README.txt")
        if not os.path.exists(readme_path):
            continue
        
        total_draft += 1
        issues = []
        
        if check_changelog_is_placeholder(readme_path):
            issues.append("placeholder changelog")
            needs_changelog.append(draft_folder)
        
        if check_duplicated_content(readme_path):
            issues.append("duplicated content")
            needs_dup_fix.append(draft_folder)
        
        x2_match = find_x2_match(draft_folder)
        if x2_match:
            matching_x2.append((draft_folder, x2_match))
        
        if issues:
            x2_note = f" (has _x2.6 match: {x2_match})" if find_x2_match(draft_folder) else " (NO _x2.6 match)"
            print(f"  {draft_folder}: {', '.join(issues)}{x2_note}")
    
    print(f"\n{'='*70}")
    print(f"Summary:")
    print(f"  Total draft mods with README.txt: {total_draft}")
    print(f"  Draft mods with placeholder changelogs: {len(needs_changelog)}")
    print(f"  Draft mods with duplicated OTHER DETAILS: {len(needs_dup_fix)}")
    print(f"  Draft mods with _x2.6 counterparts: {len(matching_x2)}")
    
    # Step 1: Transfer changelogs from _x2.6
    if matching_x2:
        print(f"\n{'='*70}")
        print(f"{'DRY RUN - ' if dry_run else ''}Processing changelog transfers from _x2.6...")
        print(f"{'='*70}")
        
        for draft_folder, x2_folder in matching_x2:
            x2_md_path = os.path.join(X2_DIR, x2_folder, "README.md")
            draft_readme_path = os.path.join(DRAFT_DIR, draft_folder, "README.txt")
            
            if not os.path.exists(x2_md_path):
                print(f"  {draft_folder}: No README.md in _x2.6/{x2_folder}")
                continue
            
            changelog = extract_changelog_from_md(x2_md_path)
            if not changelog:
                print(f"  {draft_folder}: Could not extract changelog from _x2.6")
                continue
            
            formatted = format_changelog_for_txt(changelog)
            
            if dry_run:
                print(f"  {draft_folder}: Would transfer changelog ({len(formatted)} chars) ✓")
                continue
            
            with open(draft_readme_path, 'r', encoding='utf-8') as f:
                current_content = f.read()
            
            new_content = re.sub(
                r'(={2,}\s*CHANGELOG\s*={2,}\s*\n)(?:.*?)(?=\n\n=|\Z)',
                r'\1' + '\n' + formatted + '\n',
                current_content,
                flags=re.DOTALL
            )
            
            with open(draft_readme_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            
            print(f"  {draft_folder}: Changelog transferred ✓")
    
    # Step 2: Fix duplicated OTHER DETAILS sections
    print(f"\n{'='*70}")
    print(f"{'DRY RUN - ' if dry_run else ''}Fixing duplicated content in OTHER DETAILS sections...")
    print(f"{'='*70}")
    
    # Process ALL draft mods seeking content that needs dedup
    for draft_folder in sorted(os.listdir(DRAFT_DIR)):
        draft_path = os.path.join(DRAFT_DIR, draft_folder)
        if not os.path.isdir(draft_path):
            continue
        
        readme_path = os.path.join(draft_path, "README.txt")
        if not os.path.exists(readme_path):
            continue
        
        with open(readme_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        fixed = fix_other_details_v2(content)
        
        if fixed:
            if dry_run:
                print(f"  {draft_folder}: Needs OTHER DETAILS cleanup ✓")
                continue
            
            with open(readme_path, 'w', encoding='utf-8') as f:
                f.write(fixed)
            print(f"  {draft_folder}: OTHER DETAILS cleaned ✓")

if __name__ == "__main__":
    main()