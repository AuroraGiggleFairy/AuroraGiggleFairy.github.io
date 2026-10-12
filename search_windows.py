import re

with open(r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\XUi_InGame\windows.xml', 'r', encoding='utf-8') as f:
    text = f.read()

matches = re.findall(r'<window[^>]+name="([^"]+)"[^>]*>', text)
for m in matches:
    if any(k in m.lower() for k in ('paint', 'texture', 'material', 'tool', 'brush', 'held')):
        print(m)
