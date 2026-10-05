"""Apply English tag skeleton + translated [Word] + native remainder.

  python apply_other_langs.py
"""
from __future__ import annotations

import csv
import re
import sys

from paths import MOD_LOC, VANILLA_LOC

sys.stdout.reconfigure(encoding="utf-8")

LANGS = [
    "german",
    "spanish",
    "french",
    "italian",
    "japanese",
    "koreana",
    "polish",
    "brazilian",
    "russian",
    "turkish",
    "schinese",
    "tchinese",
]


def L(*words: str) -> dict[str, str]:
    if len(words) != len(LANGS):
        raise ValueError(f"expected {len(LANGS)} words, got {len(words)}")
    return dict(zip(LANGS, words))


# Visible [Word] after orange wrap. Hidden tags stay English-identical.
PREFIX = {
    "Admin": L(
        "Administrator", "Admin", "Admin", "Amministratore", "管理", "관리",
        "Administrator", "Administrador", "Администратор", "Yönetici", "管理员", "管理員",
    ),
    "Ammo": L(
        "Munition", "Municiones", "Munitions", "Munizioni", "弾薬", "탄약",
        "Amunicja", "Munição", "Боеприпасы", "Mühimmat", "弹药", "彈藥",
    ),
    "Ammo-I": L(
        "Munition-Zutat", "Municiones-Ingrediente", "Munitions-Composant",
        "Munizioni-Ingrediente", "弾薬-材料", "탄약-요소",
        "Amunicja-Składnik", "Munição-Ingrediente", "Боеприпасы-Ингредиент",
        "Mühimmat-Bileşen", "弹药-原料", "彈藥-配料",
    ),
    "Armor": L(
        "Rüstung", "Armadura", "Armure", "Armatura", "アーマー", "방어구",
        "Pancerz", "Armadura", "Броня", "Zırh", "护甲", "護甲",
    ),
    "Clothing": L(
        "Kleidung", "Ropa", "Vêtements", "Vestiti", "衣服", "의류",
        "Ubranie", "Roupas", "Одежда", "Giyim", "衣物", "服裝",
    ),
    "Biome": L(
        "Biom", "Bioma", "Biome", "Bioma", "バイオーム", "바이옴",
        "Biom", "Bioma", "Биом", "Biyom", "群系", "群系",
    ),
    "Book": L(
        "Buch", "Libro", "Livre", "Libro", "本", "책",
        "Książka", "Livro", "Книга", "Kitap", "书籍", "書籍",
    ),
    "Magazine": L(
        "Magazin", "Revista", "Magazine", "Rivista", "マガジン", "잡지",
        "Magazyn", "Revista", "Журнал", "Dergi", "杂志", "雜誌",
    ),
    "Schematic": L(
        "Bauplan", "Esquema", "Schéma", "Schema", "設計図", "도면",
        "Schemat", "Diagrama", "Схема", "Şema", "设计图", "原理圖",
    ),
    "Candy": L(
        "Süßigkeit", "Dulce", "Bonbon", "Caramella", "キャンディ", "사탕",
        "Słodycze", "Doce", "Сладость", "Şeker", "糖果", "糖果",
    ),
    "Food": L(
        "Nahrung", "Comida", "Nourriture", "Cibo", "食料", "음식",
        "Żywność", "Comida", "Еда", "Gıda", "食物", "食物",
    ),
    "Crops": L(
        "Ernte", "Cultivos", "Cultures", "Colture", "作物", "작물",
        "Plony", "Cultivos", "Урожай", "Mahsul", "作物", "作物",
    ),
    "Drink": L(
        "Getränk", "Bebida", "Boisson", "Bevanda", "飲み物", "음료",
        "Napój", "Bebida", "Напиток", "İçecek", "饮料", "飲料",
    ),
    "Canned": L(
        "Konserve", "Enlatado", "Conserve", "Scatoletta", "缶詰", "통조림",
        "Konserwa", "Enlatado", "Консервы", "Konserve", "罐头", "罐頭",
    ),
    "Special": L(
        "Spezial", "Especial", "Spécial", "Speciale", "スペシャル", "스페셜",
        "Specjalne", "Especial", "Особое", "Özel", "特殊", "特殊",
    ),
    "Medical": L(
        "Medizin", "Médico", "Médical", "Medicina", "医療", "의료",
        "Medyczne", "Médico", "Медицина", "Medikal", "医疗", "醫療",
    ),
    "Parts": L(
        "Teile", "Partes", "Pièces", "Parti", "パーツ", "부품",
        "Części", "Peças", "Части", "Parçaları", "部件", "零件",
    ),
    "Reward": L(
        "Belohnung", "Recompensa", "Récompense", "Ricompensa", "報酬", "보상",
        "Nagroda", "Recompensa", "Награда", "Ödül", "报酬", "獎勵",
    ),
    "Station": L(
        "Station", "Estación", "Station", "Stazione", "装置", "작업대",
        "Stacja", "Estação", "Станция", "İstasyon", "设施", "設施",
    ),
    "Mod": L(
        "Mod", "Mod", "Modification", "Mod", "MOD", "모드",
        "Modyfikacja", "Mod", "Модификация", "Mod", "模组", "模組",
    ),
    "Mod-Armor": L(
        "Mod-Rüstung", "Mod-Armadura", "Mod-Armure", "Mod-Armatura", "MOD-アーマー", "모드-방어구",
        "Modyfikacja-Pancerz", "Mod-Armadura", "Мод-Броня", "Mod-Zırh", "模组-护甲", "模組-護甲",
    ),
    "Mod-Drone": L(
        "Mod-Drohne", "Mod-Dron", "Mod-Drone", "Mod-Drone", "MOD-ドローン", "모드-드론",
        "Modyfikacja-Dron", "Mod-Drone", "Мод-Дрон", "Mod-Drone", "模组-无人机", "模組-無人機",
    ),
    "Mod-Vehicle": L(
        "Mod-Fahrzeug", "Mod-Vehículo", "Mod-Véhicule", "Mod-Veicolo", "MOD-車両", "모드-차량",
        "Modyfikacja-Pojazd", "Mod-Veículo", "Мод-Транспорт", "Mod-Araç", "模组-载具", "模組-車輛",
    ),
    "Seed": L(
        "Samen", "Semilla", "Graine", "Seme", "種子", "씨앗",
        "Nasiono", "Semente", "Семя", "Tohum", "种子", "種子",
    ),
    "Build": L(
        "Bau", "Construcción", "Construction", "Costruzione", "建築", "건축",
        "Budowa", "Construção", "Строй", "Yapı", "建造", "建造",
    ),
    "Ore": L(
        "Erz", "Mineral", "Minerai", "Minerale", "鉱石", "광석",
        "Ruda", "Minério", "Руда", "Cevher", "矿石", "礦石",
    ),
    "Gem": L(
        "Edelstein", "Gema", "Gemme", "Gemma", "宝石", "보석",
        "Klejnot", "Gema", "Самоцвет", "Mücevher", "宝石", "寶石",
    ),
    "Resource": L(
        "Ressource", "Recurso", "Ressource", "Risorsa", "資源", "자원",
        "Zasób", "Recurso", "Ресурс", "Kaynak", "资源", "資源",
    ),
    "Vehicle": L(
        "Fahrzeug", "Vehículo", "Véhicule", "Veicolo", "車両", "차량",
        "Pojazd", "Veículo", "Транспорт", "Araç", "载具", "載具",
    ),
    "Electric": L(
        "Elektro", "Eléctrico", "Électrique", "Elettrico", "電気", "전기",
        "Elektryka", "Elétrico", "Электрика", "Elektrik", "电力", "電力",
    ),
    "Smelt": L(
        "Schmelz", "Fundición", "Fonte", "Fusione", "精錬", "제련",
        "Wytop", "Fundição", "Плавка", "Eritme", "熔炼", "熔煉",
    ),
}

GROWING = L(
    "Anbau", "Creciendo", "Pousse", "In crescita", "生育中", "성장 중",
    "Rosnący", "Cultivo", "Растет", "Yetişen", "生长中", "生長中",
)
HARVEST = L(
    "Sammeln", "Cosechar", "Récolter", "Raccogli", "採取", "수확",
    "Pozyskaj", "Colher", "Добыть", "Topla", "收集", "收穫",
)

HOST = {
    "Campfire": L(
        "Lagerfeuer", "Fogata", "Feu de camp", "Falò", "キャンプファイヤー", "캠프파이어",
        "Ognisko", "Fogueira", "Костер", "Kamp Ateşi", "篝火", "營火",
    ),
    "Forge": L(
        "Schmiede", "Fragua", "Forge", "Fucina", "炉", "화덕",
        "Kuźnia", "Forja", "Кузница", "Demirci Ocağı", "锻造炉", "鍛造爐",
    ),
    "Dew Collector": L(
        "Tausammler", "Recolector de rocío", "Ramasseur de rosée", "Raccoglitore di rugiada",
        "水滴収集器", "이슬 수집기", "Kolektor rosy", "Coletor de orvalho", "Коллектор росы",
        "Çiy Toplayıcı", "露水收集器", "露水收集器",
    ),
    "Apiary": L(
        "Bienenhaus", "Colmenar", "Rucher", "Apiario", "養蜂箱", "양봉장",
        "Pasieka", "Apiário", "Пасека", "Kovanlık", "蜂房", "養蜂箱",
    ),
    "Chicken Coop": L(
        "Hühnerstall", "Gallinero", "Poulailler", "Pollaio", "ニワトリ小屋", "닭장",
        "Kurnik", "Galinheiro", "Курятник", "Tavuk Kümesi", "鸡舍", "雞舍",
    ),
}

LEAD_RE = re.compile(
    r"^(?:"
    r"(?:\[[0-9A-Fa-f]{6,8}\]\[-\])*\[FFA94D\]\[[^\]]+\]\[-\](?:\[[0-9A-Fa-f]{6,8}\]\[-\])*\s*"
    r"|(?:\[[0-9A-Fa-f]{6,8}\])*\[FFA94D\]\[[^\]]+\]\[-\](?:\[[^\]]+\])*\s*"
    r"|\[[0-9A-Fa-f]{6}\]\[[^\]]+\]\[-\]\s*"
    r"|\[[0-9A-Fa-f]{6}\]\[-\]\s*"
    r"|\[[0-9A-Fa-f]{6}\][^\[]*?\[-\]\s*"
    r"|\(\[[0-9A-Fa-f]{6}\][^\)]*\[-\]\)\s*"
    r"|\[(?:bbbb|cccc|bbb)[0-9a-fA-F]{2,3}\]"
    r")+"
)
WRAP_RE = re.compile(
    r"^(?P<head>(?:\[[0-9A-Fa-f]{6,8}\]\[-\])*)"
    r"\[FFA94D\]\[(?P<word>[^\]]+)\]\[-\]"
    r"(?P<sort>(?:\[[0-9A-Fa-f]{6,8}\]\[-\])*)"
    r" (?P<rest>.*)$",
    re.DOTALL,
)
GEAR_RE = re.compile(
    r"^(?P<head>\[(?:bbbb|cccc)\d{2}\]\[-\]\[eeee\d{2}\]\[-\]) (?P<rest>.*)$"
)
STAT_TAIL_RE = re.compile(
    r"(?P<stats>(?:\s*\[[0-9A-Fa-f]{6}\]\([^)]*\)(?:\[-\])?)+)\s*$"
)
HOST_TAIL_RE = re.compile(
    r"^(?P<name>.*?)\s+\[DECEA3\]\((?P<host>[^)]+)\)\[-\]\s*$"
)
STAGE_RE = re.compile(
    r"^\[(?P<tag>a0000[2-5]|aa000[23])\]\[-\]"
    r"\[(?P<col>DECEA3|00ff00)\]"
    r"(?P<label>Growing|Harvest)"
    r"(?P<frac> \(\d/5\))?\[-\] (?P<name>.*)$"
)
ING_TAIL_RE = re.compile(
    r"\s*\(\[(?:DECEA3|decea3)\]"
    r"(?:Zutat|Ingrediente|Composant|材料|요소|Składnik|Ингредиент|Bileşen|原料|配料|Ingredient)"
    r"\[-\]\)\s*$",
    re.I,
)
MOD_TRAIL_RE = re.compile(
    r"(?:\s*[-—–]?\s*)"
    r"(?:Modifikation|Modification|Modificación|Modifica(?:zione)?|"
    r"Modyfikacja|Modificação|Модификация|模组|模組|개조(?:\s*부품)?|"
    r"改造パーツ|改造|Mod|MOD)\s*$",
    re.I,
)
SEED_LEAD_RE = re.compile(
    r"^(?:Semilla de |Graine d'|Graine de |Ghianda di |Pigna di |"
    r"Semente de |Семя |Nasiono |Tohum |种子|種子)\s*",
    re.I,
)
SEED_TAIL_RE = re.compile(
    r"(?:\s+Tohumu| Samen| の種|（種子）|种子|種子|samen)$",
    re.I,
)
JUNK_PAREN = re.compile(
    r"[（(](?:種子|生育中|Seed|Growing|Harvest)[）)]"
)
PLACEHOLDER_RE = re.compile(r"^(?:0|n/a)$", re.I)
OLD_UNBR_EN = re.compile(r"^\[ffb400\]([^\[]+)\[-\]\s*(.*)$", re.I)
TYPE_WORD = {
    "Ammo": "Ammo",
    "Ammo-Thrown": "Ammo",
    "Reward": "Reward",
}

SAMPLE_KEYS = [
    "ammo9mmBulletAP",
    "ammoGasCan",
    "armorPrimitiveOutfit",
    "clothingFeetT1",
    "drinkJarBeer",
    "foodCanChili",
    "foodHoney",
    "gunHandgunT1Pistol",
    "modGunBarrelExtender",
    "modGunBarrelExtenderSchematic",
    "plantedAloe1",
    "plantedAloe2",
    "resourceCement",
    "resourceWood",
    "resourceWoodBundle",
    "resourceScrapIronBundle",
    "ammoGasCanBundle",
    "foodRawMeatBundle",
    "resourceLockPickBundle",
    "toolForgeCrucible",
    "treePlantedOak08m",
    "treePlantedOak41m",
]


def loc_rows(path):
    with path.open(encoding="utf-8-sig", newline="") as f:
        r = csv.DictReader(f)
        return list(r.fieldnames or []), list(r)


def after_ffffff(text: str) -> str:
    idx = text.rfind("[ffffff]")
    if idx < 0:
        idx = text.rfind("[FFFFFF]")
    if idx >= 0:
        return text[idx + 8 :].strip()
    return ""


def native_name(src: str, van: str) -> str:
    for raw in (src, van):
        s = (raw or "").strip()
        if not s or PLACEHOLDER_RE.match(s):
            continue
        colored = after_ffffff(s)
        if colored:
            s = colored
        else:
            s = LEAD_RE.sub("", s).strip()
        s = re.sub(r"^[\-–—]\s*", "", s)
        s = JUNK_PAREN.sub("", s).strip()
        s = ING_TAIL_RE.sub("", s).strip()
        s = STAT_TAIL_RE.sub("", s).strip()
        s = HOST_TAIL_RE.sub(lambda m: m.group("name").strip(), s).strip()
        if s and not PLACEHOLDER_RE.match(s):
            return s
    return ""


def split_stats(rest: str) -> tuple[str, str]:
    m = STAT_TAIL_RE.search(rest)
    if not m:
        return rest.strip(), ""
    return rest[: m.start()].strip(), m.group("stats")


def strip_mod_trail(name: str) -> str:
    n = MOD_TRAIL_RE.sub("", (name or "").strip()).strip(" -—–")
    return n or name


def strip_seed_affix(name: str) -> str:
    n = SEED_LEAD_RE.sub("", (name or "").strip()).strip()
    n = SEED_TAIL_RE.sub("", n).strip(" -—–")
    return n or name


def stage_seed_key(key: str) -> str | None:
    m = re.match(r"^(treePlanted[A-Za-z]+)\d+m$", key)
    if m:
        return m.group(1) + "1m"
    m = re.match(r"^(planted[A-Za-z]+)\d", key)
    if m:
        return m.group(1) + "1"
    m = re.match(r"^(planted[A-Za-z]+)(?:Harvest|HarvestDesc)$", key)
    if m:
        return m.group(1) + "1"
    return None


BUNDLE_TAIL_RE = re.compile(
    r" (?:\[ffffff\])?Bundle \[DECEA3\]\([^)]+\)(?:\[-\])?\s*$"
)

LOOSE_BUNDLE = {
    "resourceLeadBundle": "resourceScrapLead",
}


def loose_item_key(key: str) -> str | None:
    if key in LOOSE_BUNDLE:
        return LOOSE_BUNDLE[key]
    if key.endswith("Bundle"):
        return key[: -len("Bundle")]
    return None


def split_bundle_tail(rest: str) -> tuple[str, str]:
    m = BUNDLE_TAIL_RE.search(rest or "")
    if not m:
        return (rest or "").strip(), ""
    return rest[: m.start()].rstrip(), m.group(0)


def attach_native(rest_en: str, name: str, lang: str, word_en: str) -> str:
    if word_en in ("Mod", "Mod-Armor", "Mod-Drone", "Mod-Vehicle", "Schematic"):
        name = strip_mod_trail(name)

    rest_core, bundle_tail = split_bundle_tail(rest_en)
    ff = rest_core.rfind("[ffffff]")
    if ff >= 0:
        prefix = rest_core[: ff + 8]
        after = rest_core[ff + 8 :]
        lead_space = " " if after[:1] == " " else ""
        core, stats = split_stats(after.lstrip())
        nat = split_stats(split_bundle_tail(name or core)[0])[0]
        host_m = HOST_TAIL_RE.search(rest_en)
        if host_m:
            host = (HOST.get(host_m.group("host")) or {}).get(lang) or host_m.group("host")
            return f"{prefix}{lead_space}{nat} [DECEA3]({host})[-]"
        return f"{prefix}{lead_space}{nat}{stats}{bundle_tail}"

    host_m = HOST_TAIL_RE.match(rest_core)
    if host_m:
        host = (HOST.get(host_m.group("host")) or {}).get(lang) or host_m.group("host")
        return f"{name or host_m.group('name')} [DECEA3]({host})[-]{bundle_tail}"

    core, stats = split_stats(rest_core)
    nat = split_stats(split_bundle_tail(name or core)[0])[0]
    return f"{nat}{stats}{bundle_tail}"


def translate_row(
    en: str,
    src: str,
    van: str,
    lang: str,
    seed_src: str = "",
    seed_van: str = "",
) -> str:
    if not (en or "").strip():
        return src or van or ""

    m = WRAP_RE.match(en)
    if m:
        word_en = m.group("word")
        word = (PREFIX.get(word_en) or {}).get(lang) or word_en
        rest_en = m.group("rest")
        name = native_name(src, van)
        if bundle_tail := split_bundle_tail(rest_en)[1]:
            name = split_stats(split_bundle_tail(name or "")[0])[0] or name
        if not name:
            name = split_stats(rest_en)[0]
            if rest_en.rfind("[ffffff]") >= 0:
                after = rest_en[rest_en.rfind("[ffffff]") + 8 :]
                name = split_stats(after.lstrip())[0]
        rest = attach_native(rest_en, name, lang, word_en)
        return f"{m.group('head')}[FFA94D][{word}][-]{m.group('sort')} {rest}"

    m = GEAR_RE.match(en)
    if m:
        rest_core, bundle_tail = split_bundle_tail(m.group("rest"))
        name = native_name(src, van) or rest_core
        name, _ = split_bundle_tail(name)
        name = split_stats(name)[0]
        return f"{m.group('head')} {name}{bundle_tail}"

    m = STAGE_RE.match(en)
    if m:
        label_en = m.group("label")
        label = (GROWING if label_en == "Growing" else HARVEST).get(lang) or label_en
        name = native_name(src, "")
        if not name:
            name = strip_seed_affix(native_name(seed_src, seed_van))
        if not name:
            name = m.group("name")
        frac = m.group("frac") or ""
        return f"[{m.group('tag')}][-][{m.group('col')}]{label}{frac}[-] {name}"

    core_en, stats_en = split_stats(en)
    if "[ffb400]" in (src or "").lower() or OLD_UNBR_EN.match(src or ""):
        name = native_name(src, van) or core_en
        return f"{name}{stats_en}"
    return src or van or en


def upgrade_old_english(row: dict[str, str]) -> bool:
    en = row.get("english") or ""
    m = OLD_UNBR_EN.match(en)
    if not m:
        return False
    rest = m.group(2)
    t = row.get("Type") or ""
    if t == "Bundle":
        row["english"] = rest
        return True
    word = TYPE_WORD.get(t) or m.group(1)
    row["english"] = f"[FFA94D][{word}][-] {rest}"
    return True


def clear_stale_context(row: dict[str, str]) -> bool:
    ctx_key = "Context / Alternate Text"
    if ctx_key not in row:
        return False
    c = row.get(ctx_key) or ""
    if not c.strip():
        return False
    if re.search(r"n/a", c, re.I) or "[ffb400]" in c.lower():
        row[ctx_key] = ""
        return True
    return False


def main() -> None:
    fields, mod = loc_rows(MOD_LOC)
    _, van_rows = loc_rows(VANILLA_LOC)
    van_map = {r.get("Key"): r for r in van_rows}
    by_key = {r.get("Key"): r for r in mod}

    unknown: dict[str, int] = {}
    unmatched = 0
    changed = 0
    skipped = 0
    upgraded_en = 0
    cleared_ctx = 0
    for row in mod:
        if clear_stale_context(row):
            cleared_ctx += 1
        if upgrade_old_english(row):
            upgraded_en += 1
        if (row.get("Type") or "") == "Admin":
            skipped += 1
            continue
        key = row.get("Key") or ""
        en = row.get("english") or ""
        vr = van_map.get(key) or {}
        loose_k = loose_item_key(key)
        loose_row = by_key.get(loose_k) if loose_k else None
        loose_vr = van_map.get(loose_k) if loose_k else None
        seed_k = stage_seed_key(key)
        seed_row = by_key.get(seed_k) if seed_k else None
        seed_vr = van_map.get(seed_k) if seed_k else None
        wm = WRAP_RE.match(en)
        if wm:
            w = wm.group("word")
            if w not in PREFIX:
                unknown[w] = unknown.get(w, 0) + 1
        elif en.strip() and not GEAR_RE.match(en) and not STAGE_RE.match(en):
            if "[FFA94D]" in en or re.match(r"^\[(?:bbbb|cccc|aa|a0)", en):
                unmatched += 1
        for lang in LANGS:
            seed_src = (seed_row or {}).get(lang) or ""
            seed_van = (seed_vr or {}).get(lang) or ""
            src = (
                (loose_row or {}).get(lang)
                or row.get(lang)
                or ""
            )
            van = (loose_vr or vr).get(lang) or vr.get(lang) or ""
            new = translate_row(
                en, src, van, lang, seed_src, seed_van
            )
            if new != (row.get(lang) or ""):
                changed += 1
            row[lang] = new

    with MOD_LOC.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields, lineterminator="\n")
        w.writeheader()
        w.writerows(mod)

    print(
        f"language cells rewritten {changed}  admin rows skipped {skipped}  "
        f"old english upgraded {upgraded_en}  stale context cleared {cleared_ctx}"
    )
    if unknown:
        print("unknown wrap words", unknown)
    if unmatched:
        print(f"unmatched english tag rows {unmatched}")
    print("--- samples german ---")
    for k in SAMPLE_KEYS:
        r = by_key.get(k)
        if not r:
            print(k, "MISSING")
            continue
        print(f"{k}\n  en {r.get('english')}\n  de {r.get('german')}")


if __name__ == "__main__":
    main()
