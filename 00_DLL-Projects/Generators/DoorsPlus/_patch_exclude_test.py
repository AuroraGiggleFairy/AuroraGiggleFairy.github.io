from pathlib import Path

path = Path("generate_doorsecure_agf_all.py")
text = path.read_text(encoding="utf-8")

helper_fn = """def get_helper_for_blockname(name):
    if 'AGFWood' in name:
        return wood_helper
    elif 'AGFIron' in name:
        return iron_helper
    elif 'AGFPowered' in name:
        return powered_helper
    elif 'AGFSteel' in name:
        return steel_helper
    return None
"""

helper_fn_new = """def get_helper_for_blockname(name):
    if 'AGFWood' in name:
        return wood_helper
    elif 'AGFIron' in name:
        return iron_helper
    elif 'AGFPowered' in name:
        return powered_helper
    elif 'AGFSteel' in name:
        return steel_helper
    return None

# Dev/test door models (e.g. elevatorTest) — do not ship in DoorsPlus.
TEST_DOOR_NAME_SUBSTR = (
    'test',
    'debug',
    'prototype',
    'dummy',
    'placeholder',
)

def is_test_door_name(name):
    n = (name or '').lower()
    return any(s in n for s in TEST_DOOR_NAME_SUBSTR)
"""

if "def is_test_door_name(" not in text:
    if helper_fn not in text:
        raise SystemExit("helper fn block not found")
    text = text.replace(helper_fn, helper_fn_new, 1)
else:
    print("is_test_door_name already present")

old_loop = """    # 1. Generate AGF variants
    for block in root.findall('block'):
        orig_name = block.get('name')
        customicon = orig_name
"""
new_loop = """    # 1. Generate AGF variants
    for block in root.findall('block'):
        orig_name = block.get('name')
        if is_test_door_name(orig_name):
            continue
        customicon = orig_name
"""
if "if is_test_door_name(orig_name):" not in text:
    if old_loop not in text:
        raise SystemExit("main loop block not found")
    text = text.replace(old_loop, new_loop, 1)
else:
    print("main loop skip already present")

# Also broaden Clean-loop elevatortest check to use shared helper
old_clean = """            'elevatortest' in name or
            ('garage' in name and 'irongarage' not in name)
"""
new_clean = """            is_test_door_name(name) or
            ('garage' in name and 'irongarage' not in name)
"""
if old_clean in text:
    text = text.replace(old_clean, new_clean, 1)
elif "is_test_door_name(name) or" in text:
    print("clean loop already updated")
else:
    print("WARN: clean-loop elevatortest pattern not found")

path.write_text(text, encoding="utf-8", newline="\n")
print("patched", path)
