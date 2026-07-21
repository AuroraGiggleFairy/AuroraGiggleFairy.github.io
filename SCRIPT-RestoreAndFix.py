import os, re, subprocess, sys

draft = r'c:\GitHub\7D2D-Mods\01_Draft'
x2 = r'c:\GitHub\7D2D-Mods\90_Archive\old-game-versions\_x2.6'
workspace = r'c:\GitHub\7D2D-Mods'

# Step 1: Restore all README.txt files from git
print("=" * 70)
print("STEP 1: Restoring all README.txt from git...")
print("=" * 70)

for folder in sorted(os.listdir(draft)):
    path = os.path.join(draft, folder, 'README.txt')
    if not os.path.isdir(os.path.join(draft, folder)):
        continue
    if os.path.exists(path):
        result = subprocess.run(
            ['git', 'checkout', '--', path],
            cwd=workspace,
            capture_output=True,
            text=True
        )
        if result.returncode != 0:
            print(f"  {folder} restore error: {result.stderr.strip()}")

print("  All restored. Verifying changelogs are placeholder...")

# Step 2: Run the FixDraftChangelogs script
print("\n" + "=" * 70)
print("STEP 2: Running changelog transfer and duplication fix...")
print("=" * 70)

result = subprocess.run(
    ['python', 'SCRIPT-FixDraftChangelogs.py'],
    cwd=workspace,
    capture_output=True,
    text=True
)
print(result.stdout)
if result.stderr:
    print(f"STDERR: {result.stderr}")

# Step 3: Re-check and clean up remaining artifacts
print("\n" + "=" * 70)
print("STEP 3: Cleaning up remaining artifacts...")
print("=" * 70)

# Fix the specific damaged section headers first (cleanup caused dashes to merge)
for folder in sorted(os.listdir(draft)):
    path = os.path.join(draft, folder, 'README.txt')
    if not os.path.isdir(os.path.join(draft, folder)) or not os.path.exists(path):
        continue
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original = content
    
    # Fix broken section headers (dashes merged with "OTHER DETAILS")
    content = re.sub(r'(-{60,})([Ii])', r'\1\n\2', content)
    
    # Fix OTHER DETAILS section: remove stray dashes just before "Implementation edits"
    content = re.sub(r'\n\s*-\s*\nImplementation edits', '\nImplementation edits', content)
    
    # Fix OTHER DETAILS section: remove trailing "  -\n" before section close
    content = re.sub(r'\n\s*-\s*\n\n\n\n======', '\n\n\n======', content)
    
    # Fix: stray dash line right after header
    content = re.sub(r'(OTHER DETAILS\n-{2,}\s*\n)\s*-\s*\n', r'\1', content)
    
    # Fix DyesPlus: remove duplicated "- Core dye loop:" blocks
    if folder == 'AGF-VP-DyesPlus-v3.1.1':
        # Find the OTHER DETAILS section content
        m = re.search(r'(OTHER DETAILS\n-{2,}\s*\n)(.*?)(?=\n\s*\n\s*\n\s*={2,})', content, re.DOTALL)
        if m:
            header = m.group(1)
            body = m.group(2)
            
            # Get all unique blocks separated by blank lines
            blocks = re.split(r'\n{2,}', body)
            seen = []
            unique_blocks = []
            for b in blocks:
                norm = re.sub(r'\s+', ' ', b.strip())
                if norm not in seen:
                    seen.append(norm)
                    unique_blocks.append(b.strip())
            
            new_body = '\n\n'.join(unique_blocks)
            new_section = header + new_body + '\n\n'
            content = content[:m.start()] + new_section + content[m.end():]
    
    # Clean trailing "  -" lines
    content = re.sub(r'\n  -\n\n', '\n\n\n', content)
    
    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"  {folder}: Fixed")
    else:
        print(f"  {folder}: OK")

# Step 4: Final verification
print("\n" + "=" * 70)
print("FINAL VERIFICATION:")
print("=" * 70)
for folder in sorted(os.listdir(draft)):
    path = os.path.join(draft, folder, 'README.txt')
    if not os.path.isdir(os.path.join(draft, folder)) or not os.path.exists(path):
        continue
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    issues = []
    
    # Check for placeholder changelog
    changelog_section = re.search(r'={2,}\s*CHANGELOG\s*={2,}(.*?)(?:\Z|={2,})', content, re.DOTALL)
    if changelog_section:
        text = changelog_section.group(1).strip()
        placeholder_pattern = r'^Notes\s*(?:\n\s*- Notes\s*)*\s*(?:\n\s*- Add changelog entries here\.\s*)?$'
        if re.match(placeholder_pattern, text):
            issues.append("STILL placeholder changelog!")
    
    # Check for broken section headers
    for m in re.finditer(r'(-{10,})([^ \n\r\t=-])', content):
        issues.append(f"broken header near '...{m.group(2)}...'")
    
    # Check for stray dashes
    stray = len(re.findall(r'^\s*-\s*$', content, re.MULTILINE))
    if stray > 0:
        issues.append(f"{stray} stray dashes")
    
    if issues:
        print(f"  !! {folder}: {'; '.join(issues)}")
    else:
        print(f"  OK {folder}")