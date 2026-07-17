import os, re

draft = r'c:\GitHub\7D2D-Mods\01_Draft'

found_any = False
for folder in sorted(os.listdir(draft)):
    path = os.path.join(draft, folder, 'README.txt')
    if not os.path.isdir(os.path.join(draft, folder)) or not os.path.exists(path):
        continue
    with open(path, encoding='utf-8') as f:
        content = f.read()
    
    issues = []
    
    # 1. Check for placeholder changelog
    m = re.search(r'={2,}\s*CHANGELOG\s*={2,}', content)
    if m:
        after = content[m.end():].strip()
        if re.match(r'^Notes\s*(?:\n\s*- Notes\s*)*\s*(?:\n\s*- Add changelog entries here\.)?', after):
            issues.append('PLACEHOLDER CHANGELOG')
    
    # 2. Check for broken section headers
    for bm in re.finditer(r'(-{60,})([A-Za-z])', content):
        issues.append('BROKEN HEADER (dashes merged with "{0}")'.format(bm.group(2)))
    
    # 3. Check for stray dash lines
    stray = len(re.findall(r'^\s*-\s*$', content, re.MULTILINE))
    if stray > 0:
        issues.append('{0} stray dash lines'.format(stray))
    
    if issues:
        found_any = True
        print('ISSUE: {0}: {1}'.format(folder, ' | '.join(issues)))

if not found_any:
    print('All 49 draft README.txt files look clean!')
else:
    print('\nCheck complete - issues remain')