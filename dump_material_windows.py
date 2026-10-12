import xml.etree.ElementTree as ET

tree = ET.parse(r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\XUi_InGame\windows.xml')
root = tree.getroot()

for w in root.findall('.//window'):
    if w.get('name') in ('windowMaterials', 'windowMaterialInfo', 'windowToolbelt'):
        print(ET.tostring(w, encoding='unicode'))
