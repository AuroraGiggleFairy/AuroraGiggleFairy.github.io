import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import importlib.util
spec = importlib.util.spec_from_file_location('', os.path.join(os.path.dirname(os.path.abspath(__file__)), '06_nexus.py'))
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

mods = [
    ('AGF-HUDPlus-1Main', 'AGF-HUDPlus-1Main-v6.5.1'),
    ('AGF-NoEAC-Toolbelt12Slots', 'AGF-NoEAC-Toolbelt12Slots-v2.2.1'),
]

for base_name, folder_name in mods:
    folder_path = os.path.join(mod.VS_CODE_ROOT, '03_ReleaseSource', folder_name)
    readme = mod.load_readme_text(folder_path)
    sections = mod.parse_readme_sections(readme)
    mod_info = mod.load_modinfo_xml(folder_path)
    nexus_name = mod.format_nexus_mod_name(base_name, '3')
    description = mod_info.get('description', '')
    bbcode = mod.generate_bbcode_full_description(nexus_name, '3', description, sections)
    
    help_dir = os.path.join(mod.PUBLISHHELP_DIR, base_name)
    os.makedirs(help_dir, exist_ok=True)
    
    with open(os.path.join(help_dir, 'FullDesc_Test.md'), 'w', encoding='utf-8') as f:
        f.write(bbcode)
        if not bbcode.endswith('\n'):
            f.write('\n')
    
    print(f'{base_name}: {len(bbcode.splitlines())} lines')