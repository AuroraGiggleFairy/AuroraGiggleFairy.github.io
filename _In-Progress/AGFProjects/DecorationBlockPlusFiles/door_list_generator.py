import xml.etree.ElementTree as ET

# Path to your XML file
xml_path = "blocks_doorsecure.xml"
output_path = "Door List.txt"

# List of color suffixes to filter out (except white)

# Add 'Black' and 'Oak' to color suffixes for filtering
color_suffixes = [
    "Blue", "Brown", "Green", "Grey", "Orange", "Pink", "Purple", "Red", "Yellow", "Black", "Oak"
]


def get_color_suffix(name):
    for color in color_suffixes:
        if name.endswith(color):
            return color
    return None

def is_white_variant(name):
    return name.endswith("White")


def get_base_name(name):
    # Remove all color suffixes from the end to get the true base
    base = name
    while True:
        for color in color_suffixes:
            if base.endswith(color):
                base = base[:-len(color)]
                break
        else:
            break
    return base


def main():
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
    except Exception as e:
        print(f"Error reading/parsing {xml_path}: {e}")
        return

    # Group all block names by their base name
    base_to_variants = {}
    for block in root.findall("block"):
        name = block.get("name")
        base = get_base_name(name)
        base_to_variants.setdefault(base, []).append(name)

    # For each group, only include the White variant if it exists, otherwise skip all colored variants
    selected_names = []
    for base, variants in base_to_variants.items():
        white_variant = next((v for v in variants if v.endswith("White")), None)
        if white_variant:
            selected_names.append(white_variant)
        else:
            # Only include non-colored (base) variant if it exists
            non_colored = [v for v in variants if get_color_suffix(v) is None]
            if non_colored:
                selected_names.append(sorted(non_colored)[0])
    unique_names = sorted(selected_names)
    try:
        with open(output_path, "w", encoding="utf-8") as f:
            for name in unique_names:
                f.write(name + "\n")
        print(f"Wrote {len(unique_names)} block names to {output_path}")
    except Exception as e:
        print(f"Error writing to {output_path}: {e}")
    print("Block names:")
    for name in unique_names:
        print(name)

if __name__ == "__main__":
    main()
