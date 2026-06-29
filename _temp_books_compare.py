import re
import pathlib
import xml.etree.ElementTree as ET

prog = pathlib.Path('c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/progression.xml')
win = pathlib.Path('c:/GitHub/7D2D-Mods/_DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/Config/XUi_InGame/windows.xml')

root = ET.parse(prog).getroot()
series = []
for bg in root.iter('book_group'):
    name = bg.get('name','')
    if not name.startswith('skill'):
        continue
    books=[]
    comp=''
    for b in root.iter('book'):
        if b.get('parent','') != name:
            continue
        pname=b.get('name','')
        if not pname.startswith('perk'):
            continue
        if 'Complete' in pname:
            comp=pname
        else:
            books.append(pname)
    if not comp and books:
        comp=books[-1]
        books=books[:-1]
    if books:
        series.append((name, books, comp))

text = win.read_text(encoding='utf-8', errors='ignore')
start = text.find('name="booksTab"')
end = text.find('name="unlockablesTab"')
frag = text[start:end] if start!=-1 and end!=-1 else ''
cvars=set(re.findall(r'cvar\(([^)]+)\)', frag))
skillcvars=set([c for c in cvars if c.startswith('AGFskill')])
expected_skillcvars=set(['AGF'+name for name,_,_ in series])
print('book_series_expected', len(expected_skillcvars))
print('book_series_in_books_tab', len(skillcvars))
print('missing_series', sorted(expected_skillcvars - skillcvars))
print('extra_series', sorted(skillcvars - expected_skillcvars))
