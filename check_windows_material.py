import xml.etree.ElementTree as ET

tree = ET.parse(r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\XUi_InGame\windows.xml')
root = tree.getroot()

for w in root.findall('.//window'):
    s = ET.tostring(w, encoding='unicode')
    if 'material' in s.lower() or 'paint' in s.lower():
        print(f"Window: {w.get('name')} controller={w.get('controller')}")
