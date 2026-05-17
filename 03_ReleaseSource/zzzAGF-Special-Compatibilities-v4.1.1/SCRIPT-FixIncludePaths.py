import os, glob

config_dir = r"c:\GitHub\7D2D-Mods\02_ActiveBuild\zzzAGF-Special-Compatibilities-v4.1.1\Config"
old = 'filename="ModPatches/'
new = 'filename="XUi/ModPatches/'

for path in glob.glob(os.path.join(config_dir, "*.xml")):
    with open(path, encoding="utf-8") as f:
        content = f.read()
    if old in content:
        updated = content.replace(old, new)
        with open(path, "w", encoding="utf-8") as f:
            f.write(updated)
        print(f"Updated: {os.path.basename(path)}")
    else:
        print(f"No change: {os.path.basename(path)}")

print("Done.")
