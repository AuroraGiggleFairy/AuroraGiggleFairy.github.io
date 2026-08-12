"""Convert DoorSecure-style door XML to v3.1 CompositeTileEntity + TEFeatureDoor."""

from __future__ import annotations

import copy
import xml.etree.ElementTree as ET


def prop_value(block: ET.Element, name: str) -> str | None:
    for prop in block.findall("property"):
        if prop.get("name") == name:
            return prop.get("value")
    return None


def normalize_composite_door_features(block: ET.Element) -> bool:
    """Ensure CompositeFeatures match working doors: empty TEFeatureDoor, sounded
    TEFeatureDoor, then TEFeatureLockable.

    Vanilla elevatorDoorDouble (and copies) omit the empty TEFeatureDoor and put
    Lockable first — activate then toggles lock instead of opening.
    """
    features = None
    for prop in block.findall("property"):
        if prop.get("class") == "CompositeFeatures":
            features = prop
            break
    if features is None:
        return False

    sounds: dict[str, str] = {}
    for child in list(features):
        cls = child.get("class")
        if cls == "TEFeatureDoor":
            for sp in child.findall("property"):
                n, v = sp.get("name"), sp.get("value")
                if n and v:
                    sounds[n] = v
            features.remove(child)
        elif cls == "TEFeatureLockable":
            features.remove(child)

    # Rebuild in the working order used by oldWoodDoor / elevatorDoor.
    ET.SubElement(features, "property", {"class": "TEFeatureDoor"})
    door_feat = ET.SubElement(features, "property", {"class": "TEFeatureDoor"})
    for key in ("OpenSound", "CloseSound", "LockedSound"):
        if key in sounds:
            ET.SubElement(door_feat, "property", {"name": key, "value": sounds[key]})
    ET.SubElement(features, "property", {"class": "TEFeatureLockable"})
    return True


def ensure_composite_door(block: ET.Element) -> ET.Element:
    """
    If block uses removed DoorSecure (or flat Open/Close sounds without
    CompositeFeatures), rewrite to CompositeTileEntity + TEFeatureDoor/Lockable.
    PoweredDoor is left alone aside from ensuring it is not DoorSecure.
    """
    cls = prop_value(block, "Class")
    has_features = any(p.get("class") == "CompositeFeatures" for p in block.findall("property"))

    sounds: dict[str, str] = {}
    for key in ("OpenSound", "CloseSound", "LockedSound"):
        val = prop_value(block, key)
        if val:
            sounds[key] = val

    if cls == "PoweredDoor":
        # Keep PoweredDoor; if no features but flat sounds exist, still wrap them.
        if has_features or not sounds:
            return block
        # fall through to attach features while keeping PoweredDoor
        target_class = "PoweredDoor"
    elif cls == "DoorSecure" or (not has_features and sounds):
        target_class = "CompositeTileEntity"
    elif cls == "CompositeTileEntity" and has_features:
        normalize_composite_door_features(block)
        return block
    elif cls is None and sounds and not has_features:
        target_class = "CompositeTileEntity"
    else:
        return block

    # Remove Class + flat sound props + old CompositeFeatures (rebuild cleanly)
    for prop in list(block.findall("property")):
        if prop.get("name") == "Class":
            block.remove(prop)
        elif prop.get("name") in ("OpenSound", "CloseSound", "LockedSound"):
            block.remove(prop)
        elif prop.get("class") == "CompositeFeatures":
            # Keep existing features if already present and we're only fixing class
            if has_features and cls != "DoorSecure":
                continue
            block.remove(prop)

    ET.SubElement(block, "property", {"name": "Class", "value": target_class})

    if not has_features or cls == "DoorSecure":
        features = ET.SubElement(block, "property", {"class": "CompositeFeatures"})
        ET.SubElement(features, "property", {"class": "TEFeatureDoor"})
        door_feat = ET.SubElement(features, "property", {"class": "TEFeatureDoor"})
        for key, val in sounds.items():
            ET.SubElement(door_feat, "property", {"name": key, "value": val})
        ET.SubElement(features, "property", {"class": "TEFeatureLockable"})
    else:
        normalize_composite_door_features(block)

    return block


def convert_tree_doorsecure(root: ET.Element) -> int:
    """Convert all DoorSecure blocks under root. Returns count converted."""
    n = 0
    for block in root.iter("block"):
        before = prop_value(block, "Class")
        ensure_composite_door(block)
        after = prop_value(block, "Class")
        if before == "DoorSecure" and after != "DoorSecure":
            n += 1
    return n
