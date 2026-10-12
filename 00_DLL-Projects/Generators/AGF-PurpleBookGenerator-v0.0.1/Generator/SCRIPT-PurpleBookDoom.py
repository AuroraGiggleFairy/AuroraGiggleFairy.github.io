"""PurpleBook-Doom variant.

Reads the Doom Learn by Doing progression and the Doom book series and
writes Learnings plus a Books page in the original Purple Book shape.
No armor page.

Output stays under this generator. A second copy is written into the Doom
snapshot mod folder so that install can load it.
"""

from __future__ import annotations

import shutil
import xml.etree.ElementTree as ET
from collections import defaultdict
from pathlib import Path

GENERATOR_DIR = Path(__file__).resolve().parent
GENERATOR_MOD = GENERATOR_DIR.parent
REPO = GENERATOR_MOD.parents[2]
OUT_DIR = GENERATOR_MOD / "PurpleBook-Doom"
DOOM_MOD = REPO / "00_Support" / "DoomSurvival" / "Mods" / "AGF-PurpleBook-Doom"
PROGRESSION = REPO / "00_Support" / "DoomSurvival" / "Mods" / "zzz-LearnByDoing" / "Config" / "progression.xml"
ITEMS = REPO / "00_Support" / "DoomSurvival" / "Mods" / "ZZZ_DoomMod_Standalone" / "Config" / "items.xml"
BOOKS_XML = REPO / "00_Support" / "DoomSurvival" / "Mods" / "ZZZ_DoomMod_Standalone" / "Config" / "progression.xml"
DOOM_MODS = REPO / "00_Support" / "DoomSurvival" / "Mods"
# These append onto craftingVehicles after the standalone list.
VEHICLE_EXTRA_PROGRESSION = [
    DOOM_MODS / "ZZZ_OcbLawnTractor" / "Config" / "progression.xml",
    DOOM_MODS / "zzzz-UH-60" / "Config" / "progression.xml",
]
VEHICLE_EXTRA_ITEMS = [
    DOOM_MODS / "ZZZ_OcbLawnTractor" / "Config" / "items.xml",
    DOOM_MODS / "zzzz-UH-60" / "Config" / "items.xml",
]
# The lawn-tractor fix moves that unlock from 10 to 20.
VEHICLE_LEVEL_PATCH = {
    "vehicleLawnTractorChassis,vehicleLawnTractorAccessories": 20,
}

# Weapons are two columns. Medical sits under Plasma Rifle. Cooking is full width.
COLUMN_LEFT = ["lbu_Gunslinger", "lbu_Archery", "lbu_Robotics", "lbu_Boomstick"]
COLUMN_RIGHT = ["lbu_MachineGunner", "lbu_Demolition", "lbu_DeadEye", "lbu_Medical"]
FULL_ROWS = ["lbu_Cooking"]
ICON_SIZE = 34
CHIP_H = 28
SECTION_GAP = 14
# Inner height of the book, below the tabs and above the bottom border.
PAGE_BODY = 716
BAR_H = 26
HEADER_W = 360
BAR_W = HEADER_W - 20
# Icon, gap, and the xp bar. The left cluster is centered in the row.
HEADER_BLOCK = 74
# Shared border only. Colored fills stay inset by this so they cannot cross it.
TILE_OVERLAP = 3
FILL_INSET = 3

QCOLORS = [
    "156,136,103",
    "207,119,41",
    "162,164,19",
    "50,194,52",
    "49,93,206",
    "164,42,204",
]

SKILL_KEYS = {
    "lbu_Cooking": "agfDoomSkillCooking",
    "lbu_Medical": "agfDoomSkillMedical",
    "lbu_Gunslinger": "agfDoomSkillHandgun",
    "lbu_Archery": "agfDoomSkillCrossbow",
    "lbu_Robotics": "agfDoomSkillFlamethrower",
    "lbu_Boomstick": "agfDoomSkillShotguns",
    "lbu_MachineGunner": "agfDoomSkillChaingun",
    "lbu_Demolition": "agfDoomSkillRocket",
    "lbu_DeadEye": "agfDoomSkillPlasma",
}

# english, german, spanish, french, italian, japanese, koreana, polish,
# brazilian, russian, turkish, schinese, tchinese
LOC_ROWS = [
    (
        "xuiSchematics",
        "Knowledge",
        "Wissen",
        "Conocimiento",
        "Connaissance",
        "Conoscenza",
        "知識",
        "지식",
        "Wiedza",
        "Conhecimento",
        "Знания",
        "Bilgi",
        "知识",
        "知識",
    ),
    (
        "agfDoomPbTitle",
        "Knowledge",
        "Wissen",
        "Conocimiento",
        "Connaissance",
        "Conoscenza",
        "知識",
        "지식",
        "Wiedza",
        "Conhecimento",
        "Знания",
        "Bilgi",
        "知识",
        "知識",
    ),
    (
        "agfDoomPbTooltip",
        "Knowledge",
        "Wissen",
        "Conocimiento",
        "Connaissance",
        "Conoscenza",
        "知識",
        "지식",
        "Wiedza",
        "Conhecimento",
        "Знания",
        "Bilgi",
        "知识",
        "知識",
    ),
    (
        "agfDoomTabLearnings",
        "Learnings",
        "Lernen",
        "Aprendizajes",
        "Apprentissages",
        "Apprendimenti",
        "習得",
        "습득",
        "Nauka",
        "Aprendizados",
        "Знания",
        "Öğrenimler",
        "习得",
        "習得",
    ),
    (
        "agfDoomTabBooks",
        "Books",
        "Bücher",
        "Libros",
        "Livres",
        "Libri",
        "本",
        "책",
        "Książki",
        "Livros",
        "Книги",
        "Kitaplar",
        "书",
        "書籍",
    ),
    (
        "agfDoomTabOverview",
        "Overview",
        "Übersicht",
        "Resumen",
        "Aperçu",
        "Panoramica",
        "概要",
        "개요",
        "Przegląd",
        "Visão geral",
        "Обзор",
        "Genel Bakış",
        "概览",
        "總覽",
    ),
    (
        "agfDoomTabOverviewTooltip",
        "Return to full page overview.",
        "Zur vollständigen Seitenübersicht zurückkehren.",
        "Volver a la vista general de página completa.",
        "Revenir à la vue d'ensemble de la page complète.",
        "Torna alla panoramica completa della pagina.",
        "ページ全体の概要に戻ります。",
        "전체 페이지 개요로 돌아갑니다.",
        "Wróć do pełnego podglądu strony.",
        "Retorna à visão geral completa da página.",
        "Вернуться к полному обзору страницы.",
        "Tam sayfa genel görünümüne dön.",
        "返回完整页面概览。",
        "返回完整頁面總覽。",
    ),
    (
        "agfDoomTabCrafting",
        "Crafting",
        "Herstellung",
        "Fabricación",
        "Fabrication",
        "Creazione",
        "クラフト",
        "제작",
        "Wytwarzanie",
        "Criação",
        "Крафт",
        "Üretim",
        "制作",
        "製作",
    ),
    (
        "agfDoomAmmoStack",
        "Stack Size",
        "Stapelgröße",
        "Tamaño de pila",
        "Taille de pile",
        "Dimensione pila",
        "スタックサイズ",
        "묶음 크기",
        "Rozmiar stosu",
        "Tamanho da pilha",
        "Размер стопки",
        "Yığın boyutu",
        "堆叠大小",
        "堆疊大小",
    ),
    ("agfDoomSkillCooking", "Cooking", "Kochen", "Cocina", "Cuisine", "Cucina", "料理", "요리", "Gotowanie", "Culinária", "Готовка", "Yemek", "烹饪", "烹飪"),
    ("agfDoomSkillMedical", "Medical", "Medizin", "Medicina", "Médecine", "Medicina", "医療", "의료", "Medycyna", "Medicina", "Медицина", "Tıp", "医疗", "醫療"),
    ("agfDoomSkillHandgun", "Handgun", "Pistole", "Pistola", "Pistolet", "Pistola", "ピストル", "피스톨", "Pistolet", "Pistola", "Пистолет", "Tabanca", "手枪", "手槍"),
    ("agfDoomSkillCrossbow", "Crossbow", "Armbrust", "Ballesta", "Arbalète", "Balestra", "クロスボウ", "석궁", "Kusza", "Besta", "Арбалет", "Arbalet", "弩", "十字弓"),
    ("agfDoomSkillFlamethrower", "Flamethrower", "Flammenwerfer", "Lanzallamas", "Lance-flammes", "Lanciafiamme", "火炎放射器", "화염방사기", "Miotacz ognia", "Lança-chamas", "Огнемёт", "Alev silahı", "火焰喷射器", "火焰噴射器"),
    ("agfDoomSkillShotguns", "Shotguns", "Schrotflinten", "Escopetas", "Fusils à pompe", "Fucili a pompa", "ショットガン", "샷건", "Strzelby", "Espingardas", "Дробовики", "Pompalılar", "霰弹枪", "散彈槍"),
    ("agfDoomSkillChaingun", "Chaingun", "Maschinengewehr", "Ametralladora", "Minigun", "Mitragliatrice", "チェーンガン", "체인건", "Karabin maszynowy", "Metralhadora", "Пулемёт", "Makineli tüfek", "链枪", "鏈槍"),
    ("agfDoomSkillRocket", "Rocket Launcher", "Raketenwerfer", "Lanzacohetes", "Lance-roquettes", "Lanciarazzi", "ロケットランチャー", "로켓탄 발사기", "Wyrzutnia rakiet", "Lança-foguetes", "Гранатомет", "Roketatar", "火箭发射器", "火箭發射器"),
    ("agfDoomSkillPlasma", "Plasma Rifle", "Plasmarifle", "Rifle de plasma", "Fusil à plasma", "Fucile al plasma", "プラズマライフル", "플라즈마 라이플", "Karabin plazmowy", "Rifle de plasma", "Плазменная винтовка", "Plazma tüfeği", "等离子步枪", "電漿步槍"),
]

# Ammo pouch crafting skills. Stack size is the item stack times (1 + level / 10).
AMMO_POUCHES = [
    ("perkHandgunAmmoPouch", "HandgunAmmoMagazine", 100),
    ("perkCrossbowAmmoPouch", "ammoArrowSteelAP", 40),
    ("perkTurretAmmoPouch", "ammoJunkTurretRegular", 200),
    ("perkShotgunAmmoPouch", "ShotgunBoxedAmmo", 60),
    ("perkChaingunAmmoPouch", "ChaingunBoxedAmmo", 300),
    ("perkFuelAmmoPouch", "ammoGasCan", 2000),
    ("perkRocketAmmoPouch", "RocketLauncherRocket", 30),
    ("perkPlasmaAmmoPouch", "PlasmaCellBattery", 400),
]
AMMO_MAX = 40

# Crafting page order. The first three are the left column, the last three the right.
# Knuckles and Electrician are swapped from the skills window.
CRAFT_SKILL_ORDER = [
    "craftingHarvestingTools",
    "craftingRepairTools",
    "craftingElectrician",
    "craftingBlades",
    "craftingKnuckles",
    "craftingVehicles",
]
CRAFT_HEADER_W = 240


def xml_attr(value: str) -> str:
    return (
        value.replace("&", "&amp;")
        .replace('"', "&quot;")
        .replace("<", "&lt;")
    )


def hex_to_rgb(raw: str) -> str:
    text = (raw or "").strip().lstrip("#")
    if len(text) == 8:
        text = text[:6]
    if len(text) != 6:
        return ""
    try:
        r = int(text[0:2], 16)
        g = int(text[2:4], 16)
        b = int(text[4:6], 16)
    except ValueError:
        return ""
    return f"{r},{g},{b}"


def section_ys(heights: list[int]) -> list[int]:
    """Equal gap above, between, and below the rows, filling the book."""
    gaps = len(heights) + 1
    gap = (PAGE_BODY - sum(heights)) // gaps
    y = -gap
    rows: list[int] = []
    for height in heights:
        rows.append(y)
        y -= height + gap
    return rows


def quality_color(item_name: str) -> str:
    if len(item_name) >= 2 and item_name[-2] == "Q" and item_name[-1].isdigit():
        index = int(item_name[-1]) - 1
        if 0 <= index < len(QCOLORS):
            return QCOLORS[index]
    return QCOLORS[0]


def parse_skills(path: Path) -> list[dict]:
    root = ET.parse(path).getroot()
    skills: list[dict] = []
    for perk in root.iter("perk"):
        name = perk.get("name") or ""
        if name not in SKILL_KEYS:
            continue
        levels: dict[int, list[str]] = defaultdict(list)
        for effect in perk.iter("passive_effect"):
            if effect.get("name") != "RecipeTagUnlocked":
                continue
            start = (effect.get("level") or "1").split(",")[0]
            try:
                level = int(start)
            except ValueError:
                continue
            for tag in (effect.get("tags") or "").split(","):
                tag = tag.strip()
                if tag and tag not in levels[level]:
                    levels[level].append(tag)
        skills.append(
            {
                "name": name,
                "icon": perk.get("icon") or "ui_game_symbol_book",
                "max_level": int(perk.get("max_level") or "50"),
                "levels": sorted(levels),
                "items": levels,
            }
        )
    return skills


def parse_craft_skills(path: Path) -> list[dict]:
    """Unlock tiles from RecipeTagUnlocked. A long list uses that level's display icon."""
    root = ET.parse(path).getroot()
    found: dict[str, dict] = {}
    for node in root.iter("crafting_skill"):
        name = node.get("name") or ""
        if name not in CRAFT_SKILL_ORDER:
            continue
        display_icon: dict[int, str] = {}
        for entry in node.findall("display_entry"):
            icon = (entry.get("icon") or entry.get("item") or "").split(",")[0].strip()
            raw = (entry.get("unlock_level") or "").split(",")[0].strip()
            if icon and raw.isdigit():
                display_icon.setdefault(int(raw), icon)
        levels: dict[int, list[str]] = defaultdict(list)
        for effect in node.iter("passive_effect"):
            if effect.get("name") != "RecipeTagUnlocked":
                continue
            raw = (effect.get("level") or "1").split(",")[0].strip()
            if not raw.isdigit():
                continue
            level = int(raw)
            tags = [tag.strip() for tag in (effect.get("tags") or "").split(",") if tag.strip()]
            if len(tags) > 4:
                icon = display_icon.get(level)
                if icon and icon not in levels[level]:
                    levels[level].append(icon)
            else:
                for tag in tags:
                    if tag not in levels[level]:
                        levels[level].append(tag)
        found[name] = {
            "name": name,
            "icon": node.get("icon") or "ui_game_symbol_tool",
            "max_level": int(node.get("max_level") or "1"),
            "levels": sorted(levels),
            "items": levels,
        }
    vehicles = found.get("craftingVehicles")
    if vehicles:
        _merge_vehicle_extras(vehicles)
    missing = [skill_name for skill_name in CRAFT_SKILL_ORDER if skill_name not in found]
    if missing:
        raise SystemExit(f"missing craft skills: {missing}")
    return [found[skill_name] for skill_name in CRAFT_SKILL_ORDER]


def _merge_vehicle_extras(skill: dict) -> None:
    """Lawn tractor and UH-60 are appended onto Vehicles by their own mods."""
    levels: dict[int, list[str]] = skill["items"]
    for path in VEHICLE_EXTRA_PROGRESSION:
        root = ET.parse(path).getroot()
        for effect in root.iter("passive_effect"):
            if effect.get("name") != "RecipeTagUnlocked":
                continue
            tags = [tag.strip() for tag in (effect.get("tags") or "").split(",") if tag.strip()]
            raw = (effect.get("level") or "1").split(",")[0].strip()
            if not raw.isdigit() or not tags:
                continue
            level = VEHICLE_LEVEL_PATCH.get(",".join(tags), int(raw))
            for tag in tags:
                if tag not in levels[level]:
                    levels[level].append(tag)
    skill["levels"] = sorted(levels)


def parse_icons(path: Path, wanted: set[str]) -> dict[str, tuple[str, str]]:
    icons: dict[str, tuple[str, str]] = {}
    # The Doom items file is one document. Iterparse keeps the icon pass light.
    for _event, elem in ET.iterparse(path, events=("end",)):
        if elem.tag != "item":
            continue
        name = elem.get("name") or ""
        if name in wanted:
            sprite = name
            tint = ""
            for prop in elem.findall("property"):
                if prop.get("name") == "CustomIcon" and prop.get("value"):
                    sprite = prop.get("value") or name
                elif prop.get("name") == "CustomIconTint" and prop.get("value"):
                    tint = hex_to_rgb(prop.get("value") or "")
            icons[name] = (sprite, tint)
        elem.clear()
    return icons


class Xml:
    def __init__(self) -> None:
        self.lines: list[str] = []
        self.depth = 0

    def open(self, tag: str, **attrs: object) -> None:
        self.lines.append(self._tag(tag, attrs, close=False))
        self.depth += 1

    def leaf(self, tag: str, **attrs: object) -> None:
        self.lines.append(self._tag(tag, attrs, close=True))

    def close(self, tag: str) -> None:
        self.depth -= 1
        self.lines.append(f"{'    ' * self.depth}</{tag}>")

    def text_elem(self, tag: str, body: str, **attrs: object) -> None:
        parts = [f"{'    ' * self.depth}<{tag}"]
        for key, value in attrs.items():
            if value is None or value == "":
                continue
            parts.append(f' {key}="{xml_attr(str(value))}"')
        parts.append(f">{xml_attr(body)}</{tag}>")
        self.lines.append("".join(parts))

    def _tag(self, tag: str, attrs: dict, close: bool) -> str:
        parts = [f"{'    ' * self.depth}<{tag}"]
        for key, value in attrs.items():
            if value is None or value == "":
                continue
            parts.append(f' {key}="{xml_attr(str(value))}"')
        parts.append(" />" if close else ">")
        return "".join(parts)

    def text(self) -> str:
        return "\n".join(self.lines) + "\n"


def _icon_slots(count: int, box_w: int, box_h: int, chip_h: int) -> list[tuple[int, int, int]]:
    """Two icons stack. A third sits beside the top one. Keep space inside the side borders."""
    icon = min(ICON_SIZE, max(22, box_w - 16))
    gap = 4
    icon_top = chip_h + 4
    area_h = max(icon, box_h - icon_top - 4)
    if count <= 1:
        x = max(2, (box_w - icon) // 2)
        y = -(icon_top + max(0, (area_h - icon) // 2))
        return [(x, y, icon)]
    if count == 2:
        group_h = icon * 2 + gap
        if group_h > area_h:
            icon = max(18, (area_h - gap) // 2)
            group_h = icon * 2 + gap
        top = icon_top + max(0, (area_h - group_h) // 2)
        x = max(2, (box_w - icon) // 2)
        return [(x, -top, icon), (x, -(top + icon + gap), icon)]
    if count == 4:
        pair = icon * 2 + gap
        if pair > box_w - 8:
            icon = max(14, (box_w - 8 - gap) // 2)
            pair = icon * 2 + gap
        group_h = icon * 2 + gap
        if group_h > area_h:
            icon = max(14, (area_h - gap) // 2)
            pair = icon * 2 + gap
            group_h = icon * 2 + gap
        left = max(4, (box_w - pair) // 2)
        top = icon_top + max(0, (area_h - group_h) // 2)
        slots = [(0, 0), (1, 0), (0, 1), (1, 1)]
        return [
            (left + col * (icon + gap), -(top + row * (icon + gap)), icon)
            for col, row in slots
        ]
    pair = icon * 2 + gap
    if pair > box_w - 4:
        icon = max(18, (box_w - 4 - gap) // 2)
        pair = icon * 2 + gap
    group_h = icon * 2 + gap
    if group_h > area_h:
        icon = max(18, (area_h - gap) // 2)
        pair = icon * 2 + gap
        group_h = icon * 2 + gap
    left = max(2, (box_w - pair) // 2)
    top = icon_top + max(0, (area_h - group_h) // 2)
    slots = [(0, 0), (1, 0), (0, 1)]
    return [
        (left + col * (icon + gap), -(top + row * (icon + gap)), icon)
        for col, row in slots[:count]
    ]


def emit_tiles(w: Xml, skill: dict, icons: dict[str, tuple[str, str]], x0: int, y0: int, total_w: int, cell_h: int) -> None:
    """Tiles touch and fill total_w. The level chip is the full top of each tile."""
    chip_h = CHIP_H
    levels = skill["levels"]
    count = max(1, len(levels))
    overlap = TILE_OVERLAP if count > 1 else 0
    span = total_w + overlap * (count - 1)
    base = span // count
    extra = span % count
    x = x0
    for index, level in enumerate(levels):
        items = skill["items"][level]
        box_w = base + (1 if index >= count - extra else 0)
        box_h = cell_h
        chip_w = box_w
        check = f"pbDoomCheck_{skill['name']}_L{level}"
        font = 16 if chip_w < 48 else 20
        w.open("rect", name=f"learnTile{skill['name']}{level}", pos=f"{x},{y0}", width=box_w, height=box_h)
        inset = FILL_INSET
        inner_w = box_w - inset * 2
        inner_h = box_h - inset * 2
        w.leaf(
            "sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
            pos=f"{inset},{-inset}", width=inner_w, height=inner_h, color="[darkGrey]",
            type="sliced", fillcenter="true",
        )
        w.leaf(
            "filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
            pos=f"{inset},{-inset}", width=inner_w, height=inner_h, fillcenter="true",
            type="filled", fill=f"{{cvar({check})}}",
        )
        for item_name, (ix, iy, icon) in zip(items, _icon_slots(len(items), box_w, box_h, chip_h)):
            sprite, tint = icons.get(item_name, (item_name, ""))
            w.leaf(
                "sprite", name="itemIcon", depth=5, pos=f"{ix},{iy}",
                size=f"{icon},{icon}", foregroundlayer="true", atlas="ItemIconAtlas",
                sprite=sprite, color=tint, tooltip_key=item_name,
            )
        w.leaf(
            "sprite", depth=6, name="backgroundBorderSection", sprite="menu_empty3px",
            foregroundlayer="true", pos="0,0", width=box_w, height=box_h,
            color="0,0,0,255", type="sliced", fillcenter="false",
        )
        w.leaf(
            "sprite", depth=7, name="levelChip", sprite="menu_empty3px",
            pos=f"{inset},{-inset}", width=chip_w - inset * 2, height=chip_h - inset * 2,
            color=quality_color(items[0]), type="sliced", fillcenter="true",
        )
        w.leaf(
            "sprite", depth=8, name="levelChipBorder", sprite="menu_empty3px",
            foregroundlayer="true", pos="0,0", width=chip_w, height=chip_h,
            color="0,0,0,255", type="sliced", fillcenter="false",
        )
        w.leaf(
            "label", depth=9, name="checkunlock", pos=f"{chip_w // 2},{-chip_h // 2}",
            pivot="center", width=chip_w, height=chip_h, justify="center",
            text=str(level), color="[black]", font_size=font,
        )
        w.close("rect")
        x += box_w - (overlap if index < count - 1 else 0)


def _emit_level_pair(w: Xml, x: int, y: int, level_text: str, max_text: str, prefix: str) -> None:
    """Larger current level and smaller /max on one line, sharing a baseline."""
    w.leaf(
        "label", depth=3, name=f"{prefix}Level", pos=f"{x},{y}", width="52", height="28",
        font_size="26", justify="right", text=level_text,
    )
    w.leaf(
        "label", depth=3, name=f"{prefix}LevelMax", pos=f"{x + 54},{y - 4}", width="40", height="18",
        font_size="16", justify="left", color="[white]", text=max_text,
    )


def _emit_skill_header(w: Xml, skill: dict, y0: int) -> None:
    """Icon, title, and level share one line. The xp bar fills the left section under them."""
    name = skill["name"]
    icon = 40
    w.leaf(
        "sprite", name="skillIcon", depth=3, pos=f"10,{y0}", size=f"{icon},{icon}",
        foregroundlayer="true", atlas="ItemIconAtlas", sprite=skill["icon"],
        tooltip_key=SKILL_KEYS[name],
    )
    w.leaf(
        "label", depth=3, name="skillName", pos=f"58,{y0 - 20}", width="170", height="32",
        pivot="left", font_size="26", justify="left", overflow="shrinkcontent",
        text_key=SKILL_KEYS[name], color="[white]",
    )
    _emit_level_pair(
        w, HEADER_W - 120, y0 - 6,
        f"[fc8c03]{{cvar({name}_Lvl:0)}}[-]",
        f"/{skill['max_level']}",
        "skill",
    )
    bar_y = y0 - 48
    w.leaf(
        "sprite", depth=3, name="xpTrack", sprite="menu_empty3px",
        pos=f"10,{bar_y}", width=BAR_W, height=BAR_H, color="20,20,20,255", type="sliced", fillcenter="true",
    )
    w.leaf(
        "filledsprite", depth=4, name="xpFill", color="252,140,3,255",
        pos=f"{10 + FILL_INSET},{bar_y - FILL_INSET}", width=BAR_W - FILL_INSET * 2,
        height=BAR_H - FILL_INSET * 2, fillcenter="true", type="filled",
        fill=f"{{cvar(pbDoomFill_{name})}}",
    )
    w.leaf(
        "sprite", depth=6, name="xpBorder", sprite="menu_empty3px", foregroundlayer="true",
        pos=f"10,{bar_y}", width=BAR_W, height=BAR_H, color="0,0,0,255", type="sliced", fillcenter="false",
    )
    w.leaf(
        "label", depth=7, name="xpText", pos=f"{10 + BAR_W // 2},{bar_y - BAR_H // 2}",
        pivot="center", width=BAR_W, height=BAR_H, justify="center",
        font_size=16, color="255,255,255,255",
        effect="Outline8", effect_color="0,0,0,255", effect_distance="1,1",
        text=f"{{cvar({name}_XP:0)}}/{{cvar({name}_XPIncrement:0)}}",
    )


def emit_skill_row(
    w: Xml,
    skill: dict,
    icons: dict[str, tuple[str, str]],
    x: int,
    y: int,
    row_w: int,
    row_h: int,
) -> None:
    """Header on the left, unlock tiles flush to the top, bottom, and right."""
    name = skill["name"]
    w.open("rect", name=f"learn{name}", pos=f"{x},{y}", height=row_h, width=row_w)
    w.leaf(
        "sprite", depth=2, name="rowBackground", sprite="menu_empty3px",
        pos="3,-3", width=row_w - 6, height=row_h - 6, color="[darkGrey]",
        type="sliced", fillcenter="true",
    )
    _emit_skill_header(w, skill, -((row_h - HEADER_BLOCK) // 2))
    emit_tiles(w, skill, icons, HEADER_W, 0, row_w - HEADER_W, row_h)
    w.leaf(
        "sprite", depth=10, name="sectionBorder", sprite="menu_empty3px",
        foregroundlayer="true", pos="0,0", width=row_w, height=row_h,
        color="0,0,0,255", type="sliced", fillcenter="false",
    )
    w.close("rect")


def book_icon(book: ET.Element) -> str:
    for effect in book.iter("passive_effect"):
        if effect.get("name") != "LootProb":
            continue
        for tag in (effect.get("tags") or "").split(","):
            tag = tag.strip()
            if tag.startswith("book"):
                return tag
    name = book.get("name") or ""
    if name.startswith("perk"):
        return "book" + name[4:]
    return name


def parse_books(path: Path) -> list[dict]:
    root = ET.parse(path).getroot()
    series: list[dict] = []
    index: dict[str, dict] = {}
    for group in root.iter("book_group"):
        name = group.get("name") or ""
        if not name.startswith("skill"):
            continue
        entry = {
            "name": name,
            "name_key": group.get("name_key") or name,
            "icon": group.get("icon") or "ui_game_symbol_book",
            "books": [],
        }
        series.append(entry)
        index[name] = entry
    for book in root.iter("book"):
        parent = book.get("parent") or ""
        if parent not in index:
            continue
        tip = book.get("desc_key") or book.get("name") or ""
        icon = book_icon(book)
        index[parent]["books"].append(
            {
                "name": book.get("name") or "",
                "icon": icon,
                "sprite": icon,
                "tint": "",
                "tip": tip,
                "complete": "Complete" in (book.get("name") or ""),
            }
        )
    return series


def series_parts(group: dict) -> tuple[list[dict], dict | None]:
    volumes = [book for book in group["books"] if not book["complete"]]
    complete = next((book for book in group["books"] if book["complete"]), None)
    return volumes, complete


def apply_book_sprites(path: Path, series: list[dict]) -> int:
    """Use the item CustomIcon when a book does not use its own item name."""
    wanted = {book["icon"] for group in series for book in group["books"]}
    found = parse_icons(path, wanted)
    replaced = 0
    for group in series:
        for book in group["books"]:
            sprite, tint = found.get(book["icon"], (book["icon"], ""))
            if sprite != book["icon"]:
                replaced += 1
            book["sprite"] = sprite
            book["tint"] = tint
        volumes, _complete = series_parts(group)
        cover = volumes[0] if volumes else (group["books"][0] if group["books"] else None)
        group["cover"] = cover["sprite"] if cover else group["icon"]
        group["cover_tint"] = cover["tint"] if cover else ""
    return replaced


def _atlas_for(sprite: str) -> str:
    if sprite.startswith("ui_game_"):
        return ""
    return "ItemIconAtlas"


def _sprite(w: Xml, sprite: str, **attrs: object) -> None:
    atlas = _atlas_for(sprite)
    if atlas:
        attrs["atlas"] = atlas
    w.leaf("sprite", sprite=sprite, **attrs)


def _overview_volume(w: Xml, number: str, tip: str, read: str) -> None:
    w.open("entry", name=number)
    w.leaf(
        "sprite", depth=2, name="background", sprite="menu_empty3px", pos="0,0",
        width=60, height=50, color="[darkGrey]", type="sliced", fillcenter="true",
    )
    w.leaf(
        "sprite", depth=2, name="backgroundBorder", foregroundlayer="true", sprite="menu_empty3px",
        width=60, height=50, color="[black]", type="sliced", fillcenter="false",
    )
    w.leaf(
        "label", depth=3, name="volNumber", pos="0,-11", width=60, height=50, justify="center",
        text=number, color="[white]", font_size=30, tooltip_key=tip,
    )
    w.leaf(
        "filledsprite", depth=8, name="background", color="0,255,0,120", pos="3,-3",
        width=54, height=44, fillcenter="true", type="filled", fill=f"{{cvar({read})}}",
    )
    w.close("entry")
    for _pad in range(3):
        w.leaf("entry")


def _overview_complete(w: Xml, book: dict) -> None:
    read = f"pbDoomRead_{book['name']}"
    locked = f"pbDoomNot_{book['name']}"
    w.open("entry", name="8")
    w.leaf(
        "sprite", depth=2, name="background", sprite="menu_empty3px", pos="0,0",
        width=60, height=50, color="[darkGrey]", type="sliced", fillcenter="true",
    )
    w.leaf(
        "sprite", depth=2, name="backgroundBorder", foregroundlayer="true", sprite="menu_empty3px",
        width=60, height=50, color="[black]", type="sliced", fillcenter="false",
    )
    w.leaf(
        "filledsprite", depth=8, name="background", color="0,255,0,120", pos="3,-3",
        width=54, height=44, fillcenter="true", type="filled",
        fill=f"{{cvar({read})}}", tooltip_key=book["tip"],
    )
    w.leaf(
        "filledsprite", depth=8, name="unlocked", color="255,255,255,255",
        sprite="ui_game_symbol_check", width=40, height=40, pos="10,-5",
        fillcenter="true", type="filled", fill=f"{{cvar({read})}}", tooltip_key=book["tip"],
    )
    w.leaf(
        "filledsprite", depth=8, name="locked", color="180,180,180,255",
        sprite="ui_game_symbol_lock", width=30, height=30, pos="15,-10",
        fillcenter="true", type="filled", fill=f"{{cvar({locked})}}", tooltip_key=book["tip"],
    )
    w.close("entry")


def _zoomed_volume_row(w: Xml, index: int, book: dict, y0: int) -> None:
    ry = y0 - 72 * index
    read = f"pbDoomRead_{book['name']}"
    number = str(index + 1)
    w.leaf(
        "sprite", depth=2, name=f"rowBg{number}", sprite="menu_empty3px",
        pos=f"0,{ry}", width=860, height=68, color="[darkGrey]", type="sliced", fillcenter="true",
    )
    w.leaf(
        "sprite", depth=3, name=f"rowBorder{number}", sprite="menu_empty3px",
        pos=f"0,{ry}", width=860, height=68, color="[black]", type="sliced", fillcenter="false",
    )
    w.leaf(
        "filledsprite", depth=2, name=f"rowFill{number}", color="75,140,75,255",
        pos=f"3,{ry - 3}", width=854, height=62, fillcenter="true", type="filled",
        fill=f"{{cvar({read})}}",
    )
    w.leaf(
        "label", depth=3, name=f"idx{number}", pos=f"44,{ry - 34}", width=88, height=68,
        pivot="center", justify="center", text=number, font_size=24, tooltip_key=book["tip"],
    )
    w.leaf(
        "label", depth=3, name=f"desc{number}", pos=f"100,{ry - 34}", width=752, height=68,
        pivot="left", justify="left", text_key=book["tip"], font_size=22, tooltip_key=book["tip"],
    )


def _zoomed_complete_row(w: Xml, index: int, book: dict, y0: int) -> None:
    ry = y0 - 72 * index
    read = f"pbDoomRead_{book['name']}"
    locked = f"pbDoomNot_{book['name']}"
    w.leaf(
        "sprite", depth=2, name="rowBgComplete", sprite="menu_empty3px",
        pos=f"0,{ry}", width=860, height=68, color="[darkGrey]", type="sliced", fillcenter="true",
    )
    w.leaf(
        "sprite", depth=3, name="rowBorderComplete", sprite="menu_empty3px",
        pos=f"0,{ry}", width=860, height=68, color="[black]", type="sliced", fillcenter="false",
    )
    w.leaf(
        "filledsprite", depth=2, name="rowFillComplete", color="75,140,75,255",
        pos=f"3,{ry - 3}", width=854, height=62, fillcenter="true", type="filled",
        fill=f"{{cvar({read})}}",
    )
    w.leaf(
        "filledsprite", depth=4, name="unlockedComplete", color="255,255,255,255",
        sprite="ui_game_symbol_check", width=40, height=40, pos=f"24,{ry - 14}",
        fillcenter="true", type="filled", fill=f"{{cvar({read})}}", tooltip_key=book["tip"],
    )
    w.leaf(
        "filledsprite", depth=4, name="lockedComplete", color="180,180,180,255",
        sprite="ui_game_symbol_lock", width=30, height=30, pos=f"29,{ry - 19}",
        fillcenter="true", type="filled", fill=f"{{cvar({locked})}}", tooltip_key=book["tip"],
    )
    w.leaf(
        "label", depth=3, name="descComplete", pos=f"100,{ry - 34}", width=752, height=68,
        pivot="left", justify="left", text_key=book["tip"], font_size=22, tooltip_key=book["tip"],
    )


def emit_books_tab(w: Xml, series: list[dict]) -> None:
    """Original books header: brown Overview, cover strip, numbered volumes, completion last."""
    count = len(series)
    w.open("rect", name="booksTab", controller="TabSelectorTab", tab_key="agfDoomTabBooks")
    w.open("rect", name="tabs", controller="TabSelector", select_tab_contents_on_open="true")
    w.open("rect", name="tabsHeader")
    w.open("rect", name="allTabButton", controller="TabSelectorButton")
    w.leaf(
        "simplebutton", name="tabButton", depth=8, pos="168,33", width=120, height=29,
        font_size=17, bordercolor="[black]", defaultcolor="130, 88, 48",
        selectedsprite="menu_empty", selectedcolor="74, 33, 150", foregroundlayer="false",
        caption="{tab_name_localized}", tooltip_key="agfDoomTabOverviewTooltip",
    )
    w.close("rect")
    w.open(
        "grid", name="tabButtons", pos="12,-10", depth=3, rows="1", cols=count,
        cell_width="72", cell_height="100", repeat_content="true", arrangement="horizontal",
    )
    w.open("rect", controller="TabSelectorButton")
    w.leaf(
        "simplebutton", name="tabButton", depth=8, pos="5,0", width=60, height=100,
        font_size=20, bordercolor="[black]", defaultcolor="[darkestGrey]",
        selectedsprite="menu_empty", selectedcolor="74, 33, 150", foregroundlayer="false",
        caption="{tab_name_localized}",
    )
    w.close("rect")
    w.close("grid")
    w.open(
        "grid", name="booksDecor", pos="17,-10", depth=10, rows="1", cols=count,
        cell_width="72", cell_height="100", repeat_content="false", arrangement="horizontal",
        controller="MapStats",
    )
    for group in series:
        volumes, complete = series_parts(group)
        total = len(volumes)
        done = f"pbDoomRead_{complete['name']}" if complete else f"pbDoomDone_{group['name']}"
        w.open("entry")
        w.open("rect", depth=10, pos="0,0", width=60, height=100)
        _sprite(
            w, group["cover"], name="itemIcon", depth=10, pos="3,-9", width=60, height=50,
            size="54,54", foregroundlayer="true", color=group["cover_tint"],
        )
        w.leaf(
            "label", depth=10, name="check", pos="0,-66", width=30, height=25, justify="right",
            text=f"[fc8c03]{{cvar(pbDoomBooks_{group['name']}:0)}}[-] ",
            font_size=22, foregroundlayer="true",
        )
        w.leaf(
            "label", depth=10, name="check", pos="30,-70", width=30, height=25, justify="left",
            text=f"/{total}", font_size=16, foregroundlayer="true",
        )
        w.leaf(
            "filledsprite", depth=9, name="yesUnlocked", color="10,255,10,65",
            pos="3,-1", width=54, height=98, fillcenter="true", type="filled",
            foregroundlayer="true", fill=f"{{cvar({done})}}",
        )
        w.close("rect")
        w.close("entry")
    w.close("grid")
    w.close("rect")
    w.open("rect", name="tabsContents")
    w.open("rect", name="allBooks", controller="TabSelectorTab", tab_key="agfDoomTabOverview")
    w.open("rect", name="booksAllBody", controller="MapStats")
    w.open("rect", pos="2,-100")
    for index, group in enumerate(series):
        volumes, complete = series_parts(group)
        w.open(
            "grid", name=f"checklist{group['name']}", depth=3, rows="120", cols="2",
            pos=f"{15 + index * 72},-11", cell_width="30", cell_height="25",
            repeat_content="false", arrangement="horizontal",
        )
        for book_index, book in enumerate(volumes):
            _overview_volume(w, str(book_index + 1), book["tip"], f"pbDoomRead_{book['name']}")
        if complete:
            _overview_complete(w, complete)
        w.close("grid")
    w.close("rect")
    w.close("rect")
    w.close("rect")
    for group in series:
        volumes, complete = series_parts(group)
        total = len(volumes)
        done = f"pbDoomRead_{complete['name']}" if complete else f"pbDoomDone_{group['name']}"
        tip = complete["tip"] if complete else group["name_key"]
        w.open("rect", name=f"zoomedBooks{group['name']}", controller="TabSelectorTab", tab_key="")
        w.open("rect", name=f"zoomedBooksBody{group['name']}", controller="MapStats")
        w.leaf(
            "label", name=f"zoomedBooksName{group['name']}", depth=3, pos="120,-210",
            width=250, height=30, justify="center", font_size=26, text_key=group["name_key"],
        )
        w.open(
            "grid", name=f"zoomedBooksCard{group['name']}", depth=3, rows="2", cols="1",
            pos="120,-240", cell_width="250", cell_height="250",
            repeat_content="false", arrangement="horizontal",
        )
        w.open("entry", name="Magazine")
        w.leaf(
            "sprite", depth=2, name="background", sprite="menu_empty3px",
            width=250, height=350, color="[black]", type="sliced", fillcenter="true",
        )
        w.leaf(
            "sprite", depth=2, name="backgroundBorder", foregroundlayer="true", sprite="menu_empty3px",
            width=250, height=350, color="[black]", type="sliced", fillcenter="false",
        )
        w.leaf(
            "filledsprite", depth=1, name="yesUnlocked", color="0,255,0,200",
            pos="0,0", width=250, height=350, fillcenter="true", type="filled",
            fill=f"{{cvar({done})}}",
        )
        _sprite(
            w, group["cover"], name="itemIcon", depth=3, pos="2,-14", width=250, height=250,
            size="246,246", foregroundlayer="true", color=group["cover_tint"],
            tooltip_key=group["name_key"],
        )
        w.close("entry")
        w.open("entry", name="level")
        w.leaf(
            "label", depth=3, name="check", pos="0,-32", width=125, height=100, justify="right",
            text=f"[fc8c03]{{cvar(pbDoomBooks_{group['name']}:0)}}[-] ",
            font_size=40, tooltip_key=tip,
        )
        w.leaf(
            "label", depth=3, name="check", pos="125,-36", width=125, height=100, justify="left",
            text=f"/{total}", font_size=34, tooltip_key=tip,
        )
        w.close("entry")
        w.close("grid")
        table_h = 572
        row_count = len(volumes) + (1 if complete else 0)
        used = (row_count - 1) * 72 + 68 if row_count else 0
        row_y = -max(0, (table_h - used) // 2)
        w.open("rect", name=f"zoomedBooksTable{group['name']}", depth=3, pos="410,-129", width=860, height=table_h)
        for book_index, book in enumerate(volumes):
            _zoomed_volume_row(w, book_index, book, row_y)
        if complete:
            _zoomed_complete_row(w, len(volumes), complete, row_y)
        w.close("rect")
        w.close("rect")
        w.close("rect")
    w.close("rect")
    w.close("rect")
    w.close("rect")


def _craft_level_cvar(name: str) -> str:
    if name.startswith("perk") and name.endswith("AmmoPouch"):
        return f"pbDoomAmmoLvl_{name}"
    return f"pbDoomCraftLvl_{name}"


def _emit_craft_header(w: Xml, skill: dict, row_h: int) -> None:
    """Icon centered in the row. The name is centered above the centered level."""
    name = skill["name"]
    icon = 40
    icon_y = -((row_h - icon) // 2)
    text_x = 54
    text_w = CRAFT_HEADER_W - text_x - 8
    text_h = 62
    text_y = -((row_h - text_h) // 2)
    pair_span = 94
    pair_x = text_x + (text_w - pair_span) // 2
    _sprite(
        w, skill["icon"], name="craftIcon", depth=3, pos=f"8,{icon_y}", size=f"{icon},{icon}",
        foregroundlayer="true", tooltip_key=f"{name}Name",
    )
    w.leaf(
        "label", depth=3, name="craftName", pos=f"{text_x},{text_y}", width=text_w, height="28",
        font_size="22", justify="center", overflow="shrinkcontent",
        text_key=f"{name}Name", color="[white]",
    )
    _emit_level_pair(
        w, pair_x, text_y - 32,
        f"[fc8c03]{{cvar({_craft_level_cvar(name)}:0)}}[-]",
        f"/{skill['max_level']}",
        name,
    )


def emit_craft_skill_row(
    w: Xml,
    skill: dict,
    icons: dict[str, tuple[str, str]],
    x: int,
    y: int,
    row_w: int,
    row_h: int,
) -> None:
    """Same card as Learnings: header on the left, unlock tiles across the rest."""
    w.open("rect", name=f"craft{skill['name']}", pos=f"{x},{y}", height=row_h, width=row_w)
    w.leaf(
        "sprite", depth=2, name="rowBackground", sprite="menu_empty3px",
        pos="3,-3", width=row_w - 6, height=row_h - 6, color="[darkGrey]",
        type="sliced", fillcenter="true",
    )
    _emit_craft_header(w, skill, row_h)
    emit_tiles(w, skill, icons, CRAFT_HEADER_W, 0, row_w - CRAFT_HEADER_W, row_h)
    w.leaf(
        "sprite", depth=10, name="sectionBorder", sprite="menu_empty3px",
        foregroundlayer="true", pos="0,0", width=row_w, height=row_h,
        color="0,0,0,255", type="sliced", fillcenter="false",
    )
    w.close("rect")


def emit_ammo_group(w: Xml, x: int, row_ys: list[int], width: int) -> None:
    """Compact ammo rows. row_ys is one y per ammo row.

    x is the book-edge inset. The same inset is the gap between cards, and the
    span ends with that inset before the right edge of the book.
    """
    cols = 4
    col_gap = SECTION_GAP
    row_h = 58
    cell_w = (width - col_gap * (cols - 1)) // cols
    icon = 36
    label_gap = 4
    for index, (perk, sprite, _base) in enumerate(AMMO_POUCHES):
        col = index % cols
        row = index // cols
        cx = x + col * (cell_w + col_gap)
        cy = row_ys[row]
        top = -((row_h - icon) // 2)
        center = top - icon // 2
        w.open("rect", name=f"ammo{perk}", pos=f"{cx},{cy}", height=row_h, width=cell_w)
        w.leaf(
            "sprite", depth=2, name="rowBackground", sprite="menu_empty3px",
            pos="3,-3", width=cell_w - 6, height=row_h - 6, color="[darkGrey]",
            type="sliced", fillcenter="true",
        )
        _sprite(
            w, sprite, name="ammoIcon", depth=3, pos=f"8,{top}", size=f"{icon},{icon}",
            foregroundlayer="true", tooltip_key=f"{perk}Name",
        )
        level_x = cell_w - 100
        zone_left = 8 + icon + 8
        zone_right = level_x - 4
        zone_w = max(0, zone_right - zone_left)
        num_w = 68
        label_w = 108
        if num_w + label_gap + label_w > zone_w:
            label_w = max(48, zone_w - num_w - label_gap)
        group_w = num_w + label_gap + label_w
        group_x = zone_left + max(0, (zone_w - group_w) // 2)
        w.leaf(
            "label", depth=3, name="ammoStack", pos=f"{group_x},{center}", width=num_w, height="28",
            pivot="left", font_size="24", justify="right", color="[white]",
            text=f"{{cvar(pbDoomAmmoStack_{perk}:0)}}",
        )
        w.leaf(
            "label", depth=3, name="ammoStackCaption", pos=f"{group_x + num_w + label_gap},{center}",
            width=label_w, height="28", pivot="left", font_size="18", justify="left",
            overflow="shrinkcontent", color="[white]", text_key="agfDoomAmmoStack",
        )
        _emit_level_pair(
            w, level_x, center + 12,
            f"[fc8c03]{{cvar(pbDoomAmmoLvl_{perk}:0)}}[-]",
            f"/{AMMO_MAX}",
            perk,
        )
        w.leaf(
            "sprite", depth=10, name="sectionBorder", sprite="menu_empty3px",
            foregroundlayer="true", pos="0,0", width=cell_w, height=row_h,
            color="0,0,0,255", type="sliced", fillcenter="false",
        )
        w.close("rect")


def emit_crafting_tab(w: Xml, craft_skills: list[dict], icons: dict[str, tuple[str, str]]) -> None:
    """Skill rows with unlock tiles. Ammo stays in one shorter group under them."""
    margin = SECTION_GAP
    col_w = (1390 - margin * 3) // 2
    row_h = 112
    ammo_h = 58
    full_w = col_w * 2 + margin
    ys = section_ys([row_h, row_h, row_h, ammo_h, ammo_h])
    left = craft_skills[:3]
    right = craft_skills[3:]
    w.open("rect", name="craftingTab", controller="TabSelectorTab", tab_key="agfDoomTabCrafting")
    for index, skill in enumerate(left):
        emit_craft_skill_row(w, skill, icons, margin, ys[index], col_w, row_h)
    for index, skill in enumerate(right):
        emit_craft_skill_row(
            w, skill, icons, margin + col_w + margin, ys[index], col_w, row_h,
        )
    emit_ammo_group(w, margin, ys[3:], full_w)
    w.close("rect")


def emit_windows(skills: list[dict], icons: dict[str, tuple[str, str]], books: list[dict], craft_skills: list[dict]) -> str:
    w = Xml()
    w.open("AGF-Doom")
    w.open("append", xpath="windows/window[@name='windowPagingHeader']")
    w.leaf("sprite", name="background", sprite="ui_game_filled_circle", depth=2, pos="-39,-3", height=36, width=36, color="[lightGrey]", type="sliced")
    w.leaf("sprite", name="background", sprite="ui_game_filled_circle", depth=1, pos="-41,-1", height=40, width=40, color="0,0,0", type="sliced")
    w.leaf(
        "iconbutton", pos="-21,-21", depth=3, selectable="true", collider_scale="1.5",
        name="Schematics", sprite="ui_game_symbol_book_read", tooltip_key="agfDoomPbTooltip",
        snap="false", gamepad_selectable="false",
        color_default="221,205,250,255", color_hovered="221,205,250,255",
        color_selected="221,205,250,255", color_disabled="170,170,170,255",
    )
    w.close("append")
    w.text_elem("set", "-58,-20", xpath="windows/window[@name='windowPagingHeader']/gamepad_icon[@name='LB_Icon']/@pos")
    w.open("append", xpath="/windows")
    w.open(
        "window", name="Schematics", panel="Left", pos="0,0", height="720", width="1390",
        cursor_area="true", globalopacitymod="1.35", controller="MapStats",
    )
    w.open("rect", name="agfPbFallbackTitle", pos="480,102", width="430", height="46", depth=10)
    w.leaf("sprite", name="backgroundDark", sprite="ui_game_header_fill", pos="-3,3", width="436", height="52", color="0,0,0,255", type="sliced", pivot="topleft")
    w.leaf("sprite", name="backgroundGrey", sprite="ui_game_header_fill", pos="0,0", width="430", height="46", color="96,96,96,255", type="sliced", pivot="topleft", globalopacity="false")
    w.leaf("label", name="title", depth=12, text_key="agfDoomPbTitle", pos="35,-7", width="360", height="34", font_size=34, justify="center", style="outline", color="[white]", foregroundlayer="true")
    w.close("rect")
    w.open("rect", name="header")
    w.leaf("sprite", name="background", depth=2, pos="0,43", height="45", color="[black]", type="sliced", fillcenter="true")
    w.close("rect")
    w.open("rect", name="Background")
    w.leaf("sprite", name="background", depth=2, color="[mediumGrey]", type="sliced", fillcenter="true", foregroundlayer="false")
    w.leaf("sprite", name="backgroundBorder", depth=2, sprite="menu_empty3px", color="[black]", type="sliced", fillcenter="false", foregroundlayer="false")
    w.close("rect")
    w.open("rect", name="tabs", controller="TabSelector", select_tab_contents_on_open="true")
    w.open("rect", name="tabsHeader")
    w.open("grid", name="tabButtons", pos="515,40", depth="2", rows="1", cols="3", cell_width="120", cell_height="40", repeat_content="true", arrangement="horizontal")
    w.open("rect", controller="TabSelectorButton")
    w.leaf(
        "simplebutton", name="tabButton", depth=8, width="120", height="40", font_size="20",
        bordercolor="[black]", defaultcolor="[darkGrey]", selectedsprite="menu_empty",
        selectedcolor="74, 33, 150", foregroundlayer="false", caption="{tab_name_localized}",
    )
    w.close("rect")
    w.close("grid")
    w.close("rect")
    w.open("rect", name="tabsContents")
    by_name = {skill["name"]: skill for skill in skills}
    margin = SECTION_GAP
    col_w = (1390 - margin * 3) // 2
    left_x = margin
    right_x = margin + col_w + margin
    weapon_h = 112
    ys = section_ys([weapon_h] * (len(COLUMN_LEFT) + len(FULL_ROWS)))
    w.open("rect", name="learningsTab", controller="TabSelectorTab", tab_key="agfDoomTabLearnings")
    for index, skill_name in enumerate(COLUMN_LEFT):
        emit_skill_row(
            w, by_name[skill_name], icons, left_x, ys[index],
            col_w, weapon_h,
        )
    for index, skill_name in enumerate(COLUMN_RIGHT):
        emit_skill_row(
            w, by_name[skill_name], icons, right_x, ys[index],
            col_w, weapon_h,
        )
    for offset, skill_name in enumerate(FULL_ROWS):
        emit_skill_row(
            w, by_name[skill_name], icons, left_x, ys[len(COLUMN_LEFT) + offset],
            col_w * 2 + margin, weapon_h,
        )
    w.close("rect")
    emit_crafting_tab(w, craft_skills, icons)
    emit_books_tab(w, books)
    w.close("rect")
    w.close("rect")
    w.close("window")
    w.close("append")
    w.close("AGF-Doom")
    return w.text()


def emit_buffs(skills: list[dict], books: list[dict], craft_skills: list[dict]) -> str:
    w = Xml()
    w.open("AGF-Doom")
    w.open("append", xpath="/buffs")
    w.open("buff", name="pbDoomLearnings", hidden="true", icon="ui_game_symbol_book")
    w.leaf("stack_type", value="ignore")
    w.leaf("duration", value="0")
    w.leaf("update_rate", value="0.05")
    for trigger in ("onSelfBuffStart", "onSelfBuffUpdate"):
        for skill in skills:
            name = skill["name"]
            fill = f"pbDoomFill_{name}"
            w.open("effect_group")
            w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=fill, operation="set", value="0")
            w.open(
                "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=fill,
                operation="set", value=f"@{name}_XP",
            )
            w.leaf("requirement", name="CVarCompare", cvar=f"{name}_XPIncrement", operation="GT", value="0")
            w.close("triggered_effect")
            w.open(
                "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=fill,
                operation="divide", value=f"@{name}_XPIncrement",
            )
            w.leaf("requirement", name="CVarCompare", cvar=f"{name}_XPIncrement", operation="GT", value="0")
            w.close("triggered_effect")
            w.open(
                "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=fill,
                operation="set", value="1",
            )
            w.leaf("requirement", name="CVarCompare", cvar=f"{name}_Lvl", operation="GTE", value=str(skill["max_level"]))
            w.close("triggered_effect")
            for level in skill["levels"]:
                check = f"pbDoomCheck_{name}_L{level}"
                w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=check, operation="set", value="0")
                w.open(
                    "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=check,
                    operation="set", value="1",
                )
                w.leaf("requirement", name="CVarCompare", cvar=f"{name}_Lvl", operation="GTE", value=str(level))
                w.close("triggered_effect")
            w.close("effect_group")
        for group in books:
            volumes, complete = series_parts(group)
            count_cvar = f"pbDoomBooks_{group['name']}"
            w.open("effect_group")
            w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=count_cvar, operation="set", value="0")
            for book in group["books"]:
                read = f"pbDoomRead_{book['name']}"
                w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=read, operation="set", value="0")
                w.open("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=read, operation="set", value="1")
                w.leaf("requirement", name="ProgressionLevel", progression_name=book["name"], operation="GTE", value="1")
                w.close("triggered_effect")
            for book in volumes:
                w.open("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=count_cvar, operation="add", value="1")
                w.leaf("requirement", name="ProgressionLevel", progression_name=book["name"], operation="GTE", value="1")
                w.close("triggered_effect")
            if complete:
                locked = f"pbDoomNot_{complete['name']}"
                w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=locked, operation="set", value="1")
                w.open("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=locked, operation="set", value="0")
                w.leaf("requirement", name="ProgressionLevel", progression_name=complete["name"], operation="GTE", value="1")
                w.close("triggered_effect")
            else:
                done = f"pbDoomDone_{group['name']}"
                w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=done, operation="set", value="0")
                w.open("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=done, operation="set", value="1")
                for book in volumes:
                    w.leaf("requirement", name="ProgressionLevel", progression_name=book["name"], operation="GTE", value="1")
                w.close("triggered_effect")
            w.close("effect_group")
        for perk, _icon, base in AMMO_POUCHES:
            lvl = f"pbDoomAmmoLvl_{perk}"
            stack = f"pbDoomAmmoStack_{perk}"
            mul = f"pbDoomAmmoMul_{perk}"
            w.open("effect_group")
            w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=lvl, operation="set", value="0")
            for level in range(1, AMMO_MAX + 1):
                w.open(
                    "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=lvl,
                    operation="set", value=str(level),
                )
                w.leaf("requirement", name="ProgressionLevel", progression_name=perk, operation="GTE", value=str(level))
                w.close("triggered_effect")
            w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=mul, operation="set", value="10")
            w.leaf(
                "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=mul,
                operation="add", value=f"@{lvl}",
            )
            w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=stack, operation="set", value=str(base))
            w.leaf(
                "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=stack,
                operation="multiply", value=f"@{mul}",
            )
            w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=stack, operation="divide", value="10")
            w.close("effect_group")
        for skill in craft_skills:
            name = skill["name"]
            cvar = _craft_level_cvar(name)
            w.open("effect_group")
            w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=cvar, operation="set", value="0")
            for level in range(1, skill["max_level"] + 1):
                w.open(
                    "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=cvar,
                    operation="set", value=str(level),
                )
                w.leaf("requirement", name="ProgressionLevel", progression_name=name, operation="GTE", value=str(level))
                w.close("triggered_effect")
            w.close("effect_group")
            w.open("effect_group")
            for level in skill["levels"]:
                check = f"pbDoomCheck_{name}_L{level}"
                w.leaf("triggered_effect", trigger=trigger, action="ModifyCVar", cvar=check, operation="set", value="0")
                w.open(
                    "triggered_effect", trigger=trigger, action="ModifyCVar", cvar=check,
                    operation="set", value="1",
                )
                w.leaf("requirement", name="CVarCompare", cvar=cvar, operation="GTE", value=str(level))
                w.close("triggered_effect")
            w.close("effect_group")
    w.close("buff")
    w.close("append")
    w.close("AGF-Doom")
    return w.text()


def emit_gameevents() -> str:
    sequences = [
        "game_first_spawn",
        "game_on_spawn",
        "game_on_respawn_none",
        "game_on_respawn_default",
        "game_on_respawn_injured",
        "game_on_respawn_permanent",
    ]
    w = Xml()
    w.open("AGF-Doom")
    for sequence in sequences:
        w.open("append", xpath=f"/gameevents/action_sequence[@name='{sequence}']")
        w.open("action", **{"class": "AddBuff"})
        w.leaf("property", name="buff_name", value="pbDoomLearnings")
        w.close("action")
        w.close("append")
    w.close("AGF-Doom")
    return w.text()


def emit_xui() -> str:
    return """<AGF-Doom>
    <insertbefore xpath="/xui/window_group[@name='crafting']">
        <window_group name="schematics" open_backpack_on_open="false" close_compass_on_open="false" stack_panel_y_offset="397">
            <window name="Schematics"/>
        </window_group>
    </insertbefore>
</AGF-Doom>
"""


def emit_localization() -> str:
    header = "Key,File,Type,UsedInMainMenu,NoTranslate,KeepLoaded,english,Context / Alternate Text,german,spanish,french,italian,japanese,koreana,polish,brazilian,russian,turkish,schinese,tchinese"
    lines = [header]
    for row in LOC_ROWS:
        key = row[0]
        cells = [f'"{text}"' for text in row[1:]]
        lines.append(",".join([key, "", "", "", "", "", cells[0], '""', *cells[1:]]))
    return "\n".join(lines) + "\n"


def emit_modinfo() -> str:
    return """<?xml version="1.0" encoding="UTF-8" ?>
<xml>
    <Name value="AGF-PurpleBook-Doom" />
    <DisplayName value="Purple Book Doom" />
    <Version value="0.1.0" />
    <Description value="PurpleBook-Doom. Learnings rows and the books page." />
    <Author value="AuroraGiggleFairy" />
    <Website value="" />
</xml>
"""


def write_tree(root: Path, files: dict[str, str]) -> None:
    for rel, text in files.items():
        path = root / rel
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8", newline="\n")


def main() -> None:
    skills = parse_skills(PROGRESSION)
    if len(skills) != len(SKILL_KEYS):
        found = {skill["name"] for skill in skills}
        missing = sorted(set(SKILL_KEYS) - found)
        raise SystemExit(f"missing skills: {missing}")
    craft_skills = parse_craft_skills(BOOKS_XML)
    wanted: set[str] = set()
    for skill in skills:
        for items in skill["items"].values():
            wanted.update(items)
    for skill in craft_skills:
        for items in skill["items"].values():
            wanted.update(items)
    icons = parse_icons(ITEMS, wanted)
    for path in VEHICLE_EXTRA_ITEMS:
        icons.update(parse_icons(path, wanted))
    books = parse_books(BOOKS_XML)
    custom_icons = apply_book_sprites(ITEMS, books)
    missing_icons = sorted(wanted - set(icons))
    files = {
        "ModInfo.xml": emit_modinfo(),
        "Config/XUi_InGame/windows.xml": emit_windows(skills, icons, books, craft_skills),
        "Config/XUi_InGame/xui.xml": emit_xui(),
        "Config/buffs.xml": emit_buffs(skills, books, craft_skills),
        "Config/gameevents.xml": emit_gameevents(),
        "Config/Localization.csv": emit_localization(),
    }
    write_tree(OUT_DIR, files)
    if DOOM_MOD.exists():
        shutil.rmtree(DOOM_MOD)
    shutil.copytree(OUT_DIR, DOOM_MOD)
    for rel in files:
        ET.parse(OUT_DIR / rel) if rel.endswith(".xml") else None
    print(f"[ok] {len(skills)} skills, {len(wanted)} crafts, {len(books)} book series, {custom_icons} custom book icons")
    print(f"[ok] {OUT_DIR}")
    print(f"[ok] {DOOM_MOD}")
    if missing_icons:
        print("[warn] items with no icon entry: " + ", ".join(missing_icons))


if __name__ == "__main__":
    main()
