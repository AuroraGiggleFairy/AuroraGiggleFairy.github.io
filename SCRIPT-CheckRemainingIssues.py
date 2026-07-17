import os, re

draft = r'c:\GitHub\7D2D-Mods\01_Draft'
for folder in sorted(os.listdir(draft)):
    path = os.path.join(draft, folder, 'README.txt')
    if not os.path.isdir(os.path.join(draft, folder)) or not os.path.exists(path):
        continue
    with open(path, encoding='utf-8') as f:
        content = f.read()
    
    impl = len(re.findall(r'Implementation edits used to make this work:', content))
    stray = len(re.findall(r'^\s*-\s*$', content, re.MULTILINE))
    core_dye = content.count('Core dye loop')
    core_feature = content.count('Feature set:')
    core_dye_loop = content.count('- Core dye loop:')
    issues = []
    if impl > 1:
        issues.append(f'{impl}x impl')
    if stray > 0:
        issues.append(f'{stray} stray dashes')
    if core_dye > 1:
        issues.append(f'{core_dye}x Core dye')
    if core_feature > 1:
        issues.append(f'{core_feature}x Feature set')
    if core_dye_loop > 1:
        issues.append(f'{core_dye_loop}x Core dye loop')
    if issues:
        print(f'{folder}: {" | ".join(issues)}')
    else:
        print(f'{folder}: OK')