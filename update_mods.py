import os
import re
import xml.etree.ElementTree as ET

def get_mod_info(folder):
    xml_path = os.path.join(folder, 'ModInfo.xml')
    readme_path = os.path.join(folder, 'README.md')
    
    tree = ET.parse(xml_path)
    root = tree.getroot()
    
    name_tag = root.find(".//*[@name='DisplayName']") or root.find(".//*[@name='Name']")
    version_tag = root.find(".//*[@name='Version']")
    
    name = name_tag.attrib['value'] if name_tag is not None else folder
    version = version_tag.attrib['value'] if version_tag is not None else "0.0.0"
    
    features = ""
    if os.path.exists(readme_path):
        with open(readme_path, 'r', encoding='utf-8') as f:
            content = f.read()
            if "4.  FEATURES" in content:
                # Grabs everything after the header
                after_features = content.split("4.  FEATURES")[-1]
                # Stops at the first line of 10+ underscores
                parts = re.split(r'_{10,}', after_features)
                # .strip() removes any weird leading/trailing blank lines
                features = parts[0].strip()
            else:
                features = "*No features listed.*"
    
    return f"---\n\n### **{name}**\n\n**Version:** v{version} | [**Direct Download**](https://github.com{folder}.zip)\n\n{features}\n\n"

# Main Logic
mod_list_markdown = ""
folders = sorted([f for f in os.listdir('.') if os.path.isdir(f) and f.startswith('AGF-')])

for folder in folders:
    try:
        mod_list_markdown += get_mod_info(folder)
    except Exception as e:
        print(f"Skipping {folder}: {e}")

with open('TEMPLATE.md', 'r', encoding='utf-8') as f:
    template = f.read()

with open('README.md', 'w', encoding='utf-8') as f:
    f.write(template + "\n" + mod_list_markdown)
