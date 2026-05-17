"""
Pretty-print all XML files under Config/ (excluding Config_BACKUP/).
Uses xml.dom.minidom for formatting. Preserves existing root tags.
"""

import os, re
from xml.dom import minidom
from xml.parsers.expat import ExpatError

BASE       = r"c:\GitHub\7D2D-Mods\02_ActiveBuild\zzzAGF-Special-Compatibilities-v4.1.1"
CONFIG_DIR = os.path.join(BASE, "Config")
BACKUP_DIR = os.path.join(BASE, "Config_BACKUP")

INDENT = "\t"

def strip_whitespace_nodes(node):
    """Recursively remove text nodes that are purely whitespace."""
    for child in list(node.childNodes):
        if child.nodeType == child.TEXT_NODE and not child.nodeValue.strip():
            node.removeChild(child)
        else:
            strip_whitespace_nodes(child)


def pretty_print_xml(filepath):
    with open(filepath, encoding="utf-8") as f:
        content = f.read()

    try:
        parsed = minidom.parseString(content.encode("utf-8"))
    except ExpatError as e:
        print(f"  SKIP (parse error: {e}): {filepath}")
        return

    # Remove existing whitespace-only text nodes so minidom doesn't double-indent
    strip_whitespace_nodes(parsed)

    pretty = parsed.toprettyxml(indent=INDENT, encoding=None)

    # Remove the <?xml ...?> declaration minidom adds
    pretty = re.sub(r"^<\?xml[^?]*\?>\n?", "", pretty)

    # Collapse runs of blank lines to a single blank line
    pretty = re.sub(r"\n{3,}", "\n\n", pretty)

    pretty = pretty.strip() + "\n"

    with open(filepath, "w", encoding="utf-8") as f:
        f.write(pretty)

count = 0
for root, dirs, files in os.walk(CONFIG_DIR):
    # Skip backup folder
    dirs[:] = [d for d in dirs if os.path.join(root, d) != BACKUP_DIR]
    for fname in files:
        if fname.lower().endswith(".xml"):
            fpath = os.path.join(root, fname)
            rel    = os.path.relpath(fpath, BASE)
            pretty_print_xml(fpath)
            print(f"  Formatted: {rel}")
            count += 1

print(f"\nDone. {count} files formatted.")
