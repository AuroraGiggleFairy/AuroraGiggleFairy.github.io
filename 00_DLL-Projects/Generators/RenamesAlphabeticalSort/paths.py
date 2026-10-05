from pathlib import Path

REPO = Path(r"c:\GitHub\7D2D-Mods")
GAME_CONFIG = Path(r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config")
MOD_DIR = REPO / "02_ActiveBuild" / "AGF-VP-zHelpfulRenames-v3.0.1"
MOD_LOC = MOD_DIR / "Config" / "Localization.csv"
VANILLA_LOC = GAME_CONFIG / "Localization.csv"
ITEMS_XML = GAME_CONFIG / "items.xml"
BLOCKS_XML = GAME_CONFIG / "blocks.xml"
ITEM_MODIFIERS_XML = GAME_CONFIG / "item_modifiers.xml"
CANVAS = Path(
    r"C:\Users\rft30\.cursor\projects\c-GitHub-7D2D-Mods\canvases\renames-alphabetical-sort-patterns.canvas.tsx"
)
CATALOG = Path(__file__).resolve().parent / "catalog.csv"
BUCKETS = Path(__file__).resolve().parent / "buckets.csv"
TYPE_DISPLAY = {"": "Apiary"}
