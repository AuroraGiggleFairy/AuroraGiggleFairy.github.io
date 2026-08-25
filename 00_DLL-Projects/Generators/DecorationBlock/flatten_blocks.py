"""
Flatten blocks.xml Extends chains into self-contained block definitions.

Matches 7D2D v3.1 engine behavior from BlocksFromXml / DynamicProperties:
  - Extends param1 is a comma-separated EXCLUSION list (on the Extends property)
  - CreativeMode is always excluded from inheritance
  - Property classes (class="RepairItems", etc.) can be excluded by class name
  - Nested exclusions like RepairItems.resourceWood are supported
  - <dropextendsoff/> skips parent drop inheritance; otherwise parent drops merge
    when the same event+name is not already on the child

Decoration-oriented output still strips Extends, drops, RepairItems, and
UpgradeBlock from the final XML (intentional for DecorationBlock).

See: 00_Support/WorkspaceData/Research/7D2D-v3.1-BlocksXml-Extends-Flatten.md
"""

from __future__ import annotations

import copy
import xml.etree.ElementTree as ET

INPUT_FILE = "blocks.xml"
OUTPUT_FILE = "blocks_flattened.xml"

# Always excluded by engine when copying from Extends parent.
ALWAYS_EXCLUDE = frozenset({"CreativeMode"})

# Decoration pipeline: omit these from flattened output even if present.
STRIP_PROPERTY_CLASSES = frozenset({"RepairItems", "UpgradeBlock"})
STRIP_DROPS_FROM_OUTPUT = True


block_map: dict[str, ET.Element] = {}
flattened_cache: dict[str, ET.Element] = {}


def get_extends_info(block: ET.Element) -> tuple[str | None, set[str]]:
    """Return (parent_name, exclusion_set) from the Extends property."""
    exclusions = set(ALWAYS_EXCLUDE)
    for prop in block.findall("property"):
        if prop.get("name") != "Extends":
            continue
        parent = prop.get("value")
        param1 = prop.get("param1") or ""
        for part in param1.split(","):
            part = part.strip()
            if part:
                exclusions.add(part)
        return parent, exclusions
    return None, exclusions


def drop_key(drop: ET.Element) -> tuple[str, str]:
    """Engine merges drops per (event, name)."""
    return (drop.get("event") or "Destroy", drop.get("name") or "")


def is_excluded_property(elem: ET.Element, exclusions: set[str]) -> bool:
    if elem.tag != "property":
        return False
    name = elem.get("name")
    cls = elem.get("class")
    if name and name in exclusions:
        return True
    if cls and cls in exclusions:
        return True
    if name and any(
        name.startswith(ex + ".") for ex in exclusions if "." not in ex
    ):
        # Excluding "Foo" also skips "Foo.Bar" flat keys if any exist.
        return True
    return False


def apply_nested_class_exclusions(
    class_elem: ET.Element, exclusions: set[str]
) -> ET.Element | None:
    """
    Deep-copy a property class, removing nested children excluded via
    ClassName.ChildName. Returns None if the whole class is excluded.
    """
    cls = class_elem.get("class")
    if not cls:
        return copy.deepcopy(class_elem)
    if cls in exclusions:
        return None

    prefix = cls + "."
    nested_excludes = {
        ex[len(prefix) :] for ex in exclusions if ex.startswith(prefix)
    }
    new_elem = copy.deepcopy(class_elem)
    if nested_excludes:
        for child in list(new_elem):
            child_name = child.get("name")
            if child_name and child_name in nested_excludes:
                new_elem.remove(child)
    return new_elem


def should_keep_output_elem(elem: ET.Element) -> bool:
    if elem.tag == "property" and elem.get("name") == "Extends":
        return False
    if elem.tag == "dropextendsoff":
        return False
    if STRIP_DROPS_FROM_OUTPUT and elem.tag == "drop":
        return False
    if elem.tag == "property" and elem.get("class") in STRIP_PROPERTY_CLASSES:
        return False
    return True


def collect_named_props(container: ET.Element) -> dict[str, ET.Element]:
    props: dict[str, ET.Element] = {}
    for elem in container:
        if elem.tag == "property" and elem.get("name") and elem.get("name") != "Extends":
            props[elem.get("name")] = copy.deepcopy(elem)
    return props


def collect_class_props(container: ET.Element) -> dict[str, ET.Element]:
    classes: dict[str, ET.Element] = {}
    for elem in container:
        if elem.tag == "property" and elem.get("class") and not elem.get("name"):
            classes[elem.get("class")] = copy.deepcopy(elem)
    return classes


def collect_drops(container: ET.Element) -> dict[tuple[str, str], ET.Element]:
    drops: dict[tuple[str, str], ET.Element] = {}
    for elem in container:
        if elem.tag == "drop":
            drops[drop_key(elem)] = copy.deepcopy(elem)
    return drops


def collect_other(container: ET.Element) -> list[ET.Element]:
    """Elements that are not named props, class props, drops, or dropextendsoff."""
    others: list[ET.Element] = []
    for elem in container:
        if elem.tag == "dropextendsoff":
            continue
        if elem.tag == "drop":
            continue
        if elem.tag == "property" and elem.get("name") == "Extends":
            continue
        if elem.tag == "property" and elem.get("name"):
            continue
        if elem.tag == "property" and elem.get("class") and not elem.get("name"):
            continue
        others.append(copy.deepcopy(elem))
    return others


def reset_flatten_state(blocks_root: ET.Element) -> None:
    """Rebuild block_map / cache from a <blocks> root (or document root)."""
    block_map.clear()
    flattened_cache.clear()
    for block in blocks_root.iter("block"):
        name = block.get("name")
        if name:
            block_map[name] = block


def flatten_block(block: ET.Element, visited: set[str] | None = None) -> ET.Element:
    if visited is None:
        visited = set()
    name = block.get("name")
    if name in flattened_cache:
        return copy.deepcopy(flattened_cache[name])
    if name in visited:
        raise Exception(f"Circular inheritance detected for block: {name}")
    visited.add(name)

    extends, exclusions = get_extends_info(block)

    if extends:
        parent = block_map.get(extends)
        if parent is None:
            raise Exception(f"Parent block not found: {extends} (from {name})")

        flat_parent = flatten_block(parent, visited)

        parent_props = collect_named_props(flat_parent)
        parent_classes = collect_class_props(flat_parent)
        parent_drops = collect_drops(flat_parent)
        other_parent = collect_other(flat_parent)

        # Apply exclusions to inherited named properties.
        for ex in list(parent_props):
            if is_excluded_property(parent_props[ex], exclusions):
                del parent_props[ex]

        # Apply exclusions to inherited property classes (and nested keys).
        filtered_classes: dict[str, ET.Element] = {}
        for cls_name, cls_elem in parent_classes.items():
            kept = apply_nested_class_exclusions(cls_elem, exclusions)
            if kept is not None:
                filtered_classes[cls_name] = kept
        parent_classes = filtered_classes

        # dropextendsoff → no parent drops.
        if block.find("dropextendsoff") is not None:
            parent_drops = {}

        child_props = collect_named_props(block)
        child_classes = collect_class_props(block)
        child_drops = collect_drops(block)
        other_child = collect_other(block)

        # Child named props override parent.
        merged_props = dict(parent_props)
        merged_props.update(child_props)

        # Child property classes replace same-named parent classes.
        merged_classes = dict(parent_classes)
        merged_classes.update(child_classes)

        # Drop merge: child first; parent fills missing (event, name) keys.
        merged_drops = dict(child_drops)
        for key, drop in parent_drops.items():
            if key not in merged_drops:
                merged_drops[key] = drop

        new_attribs = {
            k: v
            for k, v in block.attrib.items()
            if not k.startswith("param") and k != "Extends"
        }
        new_block = ET.Element("block", new_attribs)

        elements_to_add: list[ET.Element] = []
        elements_to_add.extend(other_parent)
        elements_to_add.extend(merged_props.values())
        elements_to_add.extend(merged_classes.values())
        elements_to_add.extend(other_child)
        if not STRIP_DROPS_FROM_OUTPUT:
            elements_to_add.extend(merged_drops.values())

        properties = [
            e
            for e in elements_to_add
            if e.tag == "property" and e.get("name") and should_keep_output_elem(e)
        ]
        properties.sort(key=lambda e: e.get("name") or "")
        others = [
            e
            for e in elements_to_add
            if not (e.tag == "property" and e.get("name")) and should_keep_output_elem(e)
        ]
        for prop in properties:
            new_block.append(prop)
        for elem in others:
            new_block.append(elem)

        flattened_cache[name] = copy.deepcopy(new_block)
        return new_block

    # No Extends — copy self, strip Extends/dropextendsoff (and decoration strips).
    new_block = ET.Element(
        "block",
        {
            k: v
            for k, v in block.attrib.items()
            if not k.startswith("param") and k != "Extends"
        },
    )
    elements_to_add = []
    for elem in block:
        if elem.tag == "dropextendsoff":
            continue
        if should_keep_output_elem(elem):
            elements_to_add.append(copy.deepcopy(elem))
    properties = [
        e for e in elements_to_add if e.tag == "property" and e.get("name")
    ]
    properties.sort(key=lambda e: e.get("name") or "")
    others = [
        e for e in elements_to_add if not (e.tag == "property" and e.get("name"))
    ]
    for prop in properties:
        new_block.append(prop)
    for elem in others:
        new_block.append(elem)

    flattened_cache[name] = copy.deepcopy(new_block)
    return new_block


def build_all_block_names(blocks_root: ET.Element) -> set[str]:
    names: set[str] = set()
    for block in blocks_root.iter("block"):
        name = block.get("name", "")
        if name.startswith("trader"):
            names.add(name[6:])
        else:
            names.add(name)
        if name.endswith("TraderOnly"):
            names.add(name[:-10])
        if "player" in name:
            names.add(name.replace("player", ""))
        if name.endswith("POI"):
            names.add(name[:-3])
    return names


def should_emit_block(block: ET.Element, all_block_names: set[str]) -> bool:
    name = block.get("name", "")
    nl = name.lower()
    if "master" in nl or "loothelper" in nl or "varianthelper" in nl:
        return False
    if any(
        elem.tag == "property" and elem.get("class") == "TrapDoor" for elem in block
    ):
        return False
    if name.endswith("Shapes"):
        return False
    if name.startswith("trader") and name[6:] in all_block_names:
        return False
    if name.endswith("TraderOnly") and name[:-10] in all_block_names:
        return False
    if name.endswith("POI") and name[:-3] in all_block_names:
        return False
    if "player" in nl:
        for token in ("_Player", "_player", "Player", "player"):
            cand = name.replace(token, "")
            if cand != name and cand in all_block_names:
                return False
    return True


def remove_comments(elem: ET.Element) -> None:
    for child in list(elem):
        if isinstance(child.tag, str):
            remove_comments(child)
        else:
            elem.remove(child)


def flatten_document(
    blocks_root: ET.Element, *, filter_blocks: bool = True
) -> ET.Element:
    reset_flatten_state(blocks_root)
    all_names = build_all_block_names(blocks_root)
    top = ET.Element("blocks", blocks_root.attrib)
    for block in blocks_root.iter("block"):
        if filter_blocks and not should_emit_block(block, all_names):
            continue
        top.append(flatten_block(block))
    remove_comments(top)
    return top


def main() -> None:
    tree = ET.parse(INPUT_FILE)
    root = tree.getroot()
    sample = [b.get("name") for b in list(root.iter("block"))[:5] if b.get("name")]
    count = sum(1 for _ in root.iter("block"))
    if count == 0:
        print("WARNING: No <block> elements found in the input file!")
    else:
        print(f"Found {count} blocks. First 5: {sample}")

    top = flatten_document(root, filter_blocks=True)
    processed = sum(1 for _ in top.iter("block"))
    print(f"Processed {processed} blocks.")

    ET.indent(top, space="    ")
    ET.ElementTree(top).write(OUTPUT_FILE, encoding="utf-8", xml_declaration=True)
    print(f"Flattened blocks written to {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
