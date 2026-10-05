# Outback Roadies loc in HelpfulRenames / Renames Alphabetical consumable style.
import csv
import shutil
from pathlib import Path

REPO = Path(r"c:\GitHub\7D2D-Mods")
HR_LOC = (
    REPO / "02_ActiveBuild" / "AGF-VP-zHelpfulRenames-v3.0.1" / "Config" / "Localization.csv"
)
OUT = (
    REPO
    / "02_ActiveBuild"
    / "zzzAGF-Special-NoEACCompatibilities-v1.0.0"
    / "Config"
    / "ModPatches"
    / "HelpfulRenames-V3_OutbackRoadies"
    / "Localization.csv"
)
INGREDIENT_MARK = "[DECEA3](I)[-]"
# Vanilla HelpfulRenames foods/canned used as Outback ingredients that lack (I).
# Crops stay without (I). Keys that already have (I) in HelpfulRenames are omitted.
VANILLA_ADD_I = [
    "foodBlueberryPie",
    "foodEggBoiled",
    "foodMeatStew",
    "foodCanCatfood",
    "foodCanChicken",
    "foodCanMiso",
    "foodCanPears",
    "foodCanSoup",
]
COPIES = [
    REPO
    / "03_ReleaseSource"
    / "zzzAGF-Special-NoEACCompatibilities-v1.0.0"
    / "Config"
    / "ModPatches"
    / "HelpfulRenames-V3_OutbackRoadies"
    / "Localization.csv",
    Path(
        r"C:\Program Files (x86)\Steam\steamapps\common\7 Days to Die - Outback\Mods"
        r"\zzzAGF-Special-NoEACCompatibilities-v1.0.0\Config\ModPatches"
        r"\HelpfulRenames-V3_OutbackRoadies\Localization.csv"
    ),
]

LANGS = [
    "german", "spanish", "french", "italian", "japanese", "koreana",
    "polish", "brazilian", "russian", "turkish", "schinese", "tchinese",
]

PREFIX = {
    "Drink": ["Getränk", "Bebida", "Boisson", "Bevanda", "飲み物", "음료", "Napój", "Bebida", "Напиток", "İçecek", "饮料", "飲料"],
    "Food": ["Nahrung", "Comida", "Nourriture", "Cibo", "食料", "음식", "Żywność", "Comida", "Еда", "Gıda", "食物", "食物"],
    "Crops": ["Ernte", "Cultivos", "Cultures", "Colture", "作物", "작물", "Plony", "Cultivos", "Урожай", "Mahsul", "作物", "作物"],
    "Candy": ["Süßigkeit", "Dulce", "Bonbon", "Caramella", "キャンディ", "사탕", "Słodycze", "Doce", "Сладость", "Şeker", "糖果", "糖果"],
    "Special": ["Spezial", "Especial", "Spécial", "Speciale", "スペシャル", "스페셜", "Specjalne", "Especial", "Особое", "Özel", "特殊", "特殊"],
    "Medical": ["Medizin", "Médico", "Médical", "Medicina", "医療", "의료", "Medyczne", "Médico", "Медицина", "Medikal", "医疗", "醫療"],
    "Build": ["Bau", "Construcción", "Construction", "Costruzione", "建築", "건축", "Budowa", "Construção", "Строй", "Yapı", "建造", "建造"],
    "Ore": ["Erz", "Mineral", "Minerai", "Minerale", "鉱石", "광석", "Ruda", "Minério", "Руда", "Cevher", "矿石", "礦石"],
}

CONCRETE = [
    "Betonmischung", "Mezcla de hormigón", "Mélange à béton", "Miscela di calcestruzzo",
    "コンクリートミックス", "콘크리트 혼합물", "Mieszanka betonowa", "Cimento", "Бетон",
    "Beton Karışımı", "混凝土混合料", "混凝土混合物",
]
CLAY = [
    "Lehmboden", "Terreno de arcilla", "Sol en argile", "Argilla", "粘土", "식토",
    "Gleba gliniasta", "Solo argiloso", "Глинистый грунт", "Killi Toprak", "粘土", "黏土",
]


def pad4(n: int) -> str:
    return f"{n:04d}"


def food_stats(food, water, inf, ing):
    bits = []
    if food not in (None, 0):
        bits.append(f"[00de50]({food})[-]")
    if water not in (None, 0):
        bits.append(f"[00c8ff]({water})[-]")
    if inf:
        bits.append(f"[FFA94D]({inf}%)[-]")
    if ing:
        bits.append("[DECEA3](I)[-]")
    return "".join(bits)


def drink_stats(water, food, inf, ing):
    bits = []
    if water not in (None, 0):
        bits.append(f"[00c8ff]({water})[-]")
    if food not in (None, 0):
        bits.append(f"[00de50]({food})[-]")
    if inf:
        bits.append(f"[FFA94D]({inf}%)[-]")
    if ing:
        bits.append("[DECEA3](I)[-]")
    return "".join(bits)


def hid(code: str) -> str:
    return f"[{code.strip('[]')}][-]"


def consume(family, word, hidden, remainder, stats):
    body = f"{family}[FFA94D][{word}][-]{hidden} {remainder}"
    if stats:
        body += " " + stats
    return body


def langs_from_prefix(word, remainder_en, remainder_langs, hidden, family, stats):
    out = []
    for i, w in enumerate(PREFIX[word]):
        rem = remainder_langs[i] if remainder_langs else remainder_en
        out.append(consume(family, w, hidden, rem, stats))
    return out


def add_ingredient_mark(text: str) -> str:
    if not text or "(I)" in text:
        return text
    return text + INGREDIENT_MARK


def load_hr_rows():
    with HR_LOC.open(encoding="utf-8-sig", newline="") as f:
        return {r["Key"]: r for r in csv.DictReader(f)}


def overlay_vanilla_ingredient(hr_row):
    patched = dict(hr_row)
    patched["english"] = add_ingredient_mark(patched.get("english") or "")
    for lang in LANGS:
        patched[lang] = add_ingredient_mark(patched.get(lang) or "")
    return patched


def row(key, typ, english, others):
    return {
        "Key": key,
        "File": "items",
        "Type": typ,
        "UsedInMainMenu": "",
        "NoTranslate": "",
        "KeepLoaded": "",
        "english": english,
        "Context / Alternate Text": "",
        **dict(zip(LANGS, others)),
    }


# food, water, inf%, ingredient  — health omitted (HelpfulRenames consumable update)
FOODS = [
    ("foodWheatFlour", "Food", "Food", "Wheat Flour", 5, -5, 0, True),
    ("foodSnagSanga", "Food", "Food", "Snag Sanga", 30, 0, 0, True),
    ("foodWitchettyGrub", "Food", "Food", "Witchetty Grub", 5, 5, 1, True),
    ("foodWitchettyGrubSoupandSanga", "Food", "Food", "Witchetty Grub Soup and Sanga", 65, 25, 5, False),
    ("foodMeatPie", "Food", "Food", "Poco Meat Pie", 50, 0, 0, True),
    ("foodPocoTucker", "Food", "Food", "Poco Tucker", 60, 28, 3, True),
    ("foodZombimiteToast", "Food", "Food", "Zombimite on Toast", 30, 0, 1, False),
    ("foodDamper", "Food", "Food", "Damper", 18, -5, 0, True),
    ("foodSausageRoll", "Food", "Food", "Sausage Roll", 30, 0, 0, True),
    ("foodHordeGorge", "Food", "Food", "Horde Gorge", 200, 70, 10, False),
    ("foodBarramundi", "Food", "Food", "Sustainable Aussie Barra", 15, 0, 0, True),
    ("foodFishAndChips", "Food", "Food", "Fish and Chips", 65, 0, 0, False),
    ("foodSuperMeatStew", "Food", "Food", "Super Meat Stew", 75, 25, 0, False),
    ("foodBillyCanYabbies", "Food", "Food", "Billy Can Yabbies", 70, 25, 0, False),
    ("foodCouscousYabbies", "Food", "Food", "Couscous with Yabbies", 50, 0, 0, False),
    ("foodFishPie", "Food", "Food", "Fish Pie", 112, 20, 0, False),
    ("foodGnocchiChili", "Food", "Food", "Gnocchi Chili", 65, 15, 0, False),
    ("foodFishPieMashPeas", "Food", "Food", "Fish Pie Mountain", 120, 20, 0, False),
    ("foodGrilledYuccaFruit", "Food", "Food", "Grilled Yucca Fruit", 25, 0, 0, False),
    ("foodYuccaFruitPie", "Food", "Food", "Yucca Fruit Pie", 48, 0, 0, True),
    ("foodGrilledCactusPaddles", "Food", "Food", "Grilled Cactus Paddles", 25, 0, 0, False),
    ("foodCactusOmelette", "Food", "Food", "Cactus Dan Omelette", 36, 0, 0, False),
    ("foodSaguaroFruitPie", "Food", "Food", "Saguaro Fruit Pie", 48, 0, 0, True),
    ("foodFruitPieTrio", "Food", "Food", "Fruit Pie Trio", 150, 0, 0, False),
    ("foodCactusStew", "Food", "Food", "Cactus Stew", 40, 20, 0, False),
    ("foodSuperCornFluffyOmelette", "Food", "Food", "Super Fluffy Omelette", 105, 0, 5, False),
    ("foodEggQuiche", "Food", "Food", "Quiche", 55, 0, 0, False),
    ("foodEggSandwich", "Food", "Food", "Egg Sandwich", 45, 0, 0, False),
    ("foodChickenStew", "Food", "Food", "Chicken Stew", 50, 20, 0, False),
    ("foodRoastChicken", "Food", "Food", "Roast Chicken", 45, 0, 0, False),
    ("foodChickSticks", "Food", "Food", "Chick Sticks", 27, -5, 1, False),
    ("foodHoneyComb", "Food", "Food", "Honey Comb", 10, 0, 1, True),
    ("foodHoneyBread", "Food", "Food", "Honey Bread", 25, 0, 5, False),
    ("foodHoneyPumpkinPie", "Food", "Food", "Honey Pumpkin Pie", 50, 0, 5, True),
    ("foodBlueberryBreadPudding", "Food", "Food", "Blueberry Bread Pudding", 55, 0, 5, False),
    ("foodFairyBread", "Food", "Food", "Fairy Bread", 20, -5, 0, False),
    ("foodLamington", "Food", "Food", "Lamington", 20, -5, 0, False),
    ("foodSuperHoneyPopcorn", "Food", "Food", "Super Honey Popcorn", 28, -3, 1, False),
    ("foodHoneyGlazedDonuts", "Food", "Food", "Honey Glazed Donuts", 25, 0, 5, False),
    ("foodStimStam", "Food", "Food", "Stim Stam", 25, 5, 0, True),
    ("foodStimStamStem", "Food", "Food", "Stim Stam Stem", 30, 50, 0, False),
]

CROPS = [
    ("foodWildWheat", "Crop", "Crops", "Wild Wheat", 1, -1, 0, False),
    ("foodBillyTea", "Crop", "Crops", "Billy Tea Leaves", 2, -3, 0, False),
    ("foodCropCactusFruit", "Crop", "Crops", "Prickly Cactus Fruit", 1, 0, 0, False),
    ("foodCropCactusPaddles", "Crop", "Crops", "Cactus Paddles", 2, 0, 0, False),
    ("foodCropSaguaroFruit", "Crop", "Crops", "Saguaro Fruit", 2, 0, 0, False),
]

DRINKS = [
    ("drinkCactusFruitJuice", "Drink", "Drink", "Cactus Dan Fruit Juice", 35, 35, 0, False),
    ("drinkYuccaCactusAloeJuiceCooler", "Drink", "Drink", "Yucca Cactus Aloe Juice Storm Breaker", 85, 55, 0, False),
    ("drinkCanChromeSoda", "Drink", "Drink", "Chrome Soda", 50, 10, 0, False),
    ("drinkJarBillyTea", "Drink", "Drink", "Billy Tea", 40, 0, 0, True),
    ("drinkWombatChocoPoco", "Drink", "Drink", "Wombat Choco Poco", 25, 40, 0, False),
]

rows = []

for key, typ, word, rem, water, food, inf, ing in DRINKS:
    hidden = hid(f"ed{pad4(water)}") + hid(f"ee{pad4(1000 + food)}")
    stats = drink_stats(water, food, inf, ing)
    en = consume(hid("f00001"), word, hidden, rem, stats)
    rows.append(row(key, typ, en, langs_from_prefix(word, rem, None, hidden, hid("f00001"), stats)))

for key, typ, word, rem, food, water, inf, ing in FOODS:
    hidden = hid(f"fd{pad4(food)}") + hid(f"fe{pad4(1000 + water)}")
    stats = food_stats(food, water, inf, ing)
    en = consume(hid("f00002"), word, hidden, rem, stats)
    rows.append(row(key, typ, en, langs_from_prefix(word, rem, None, hidden, hid("f00002"), stats)))

for key, typ, word, rem, food, water, inf, ing in CROPS:
    hidden = hid(f"fb{pad4(food)}") + hid(f"fe{pad4(1000 + water)}")
    stats = food_stats(food, water, inf, False)
    en = consume(hid("f00003"), word, hidden, rem, stats)
    rows.append(row(key, typ, en, langs_from_prefix(word, rem, None, hidden, hid("f00003"), stats)))

hb_hidden = hid("a00009")
hb_stats = "[FFA94D](5%)[-][DECEA3](I)[-]"
hb_en = consume(hid("f00005"), "Special", hb_hidden, "Honey Beer", hb_stats)
rows.append(row(
    "drinkHoneyBeer", "Special", hb_en,
    langs_from_prefix("Special", "Honey Beer", None, hb_hidden, hid("f00005"), hb_stats),
))

def wrapped_bundle(family_hid, word, name, count, name_langs):
    def line(w, n):
        return f"{family_hid}[FFA94D][{w}][-] {n} Bundle [DECEA3]({count})[-]"

    en = line(word, name)
    others = [line(PREFIX[word][i], n) for i, n in enumerate(name_langs)]
    return en, others


conc_en, conc_others = wrapped_bundle(hid("dddd01") + hid("eeee03"), "Build", "Concrete Mix", 1000, CONCRETE)
clay_en, clay_others = wrapped_bundle(hid("dddd02") + hid("eeee01"), "Ore", "Clay Soil", 6000, CLAY)
rows.append(row("resourceCementMixerConcreteBundle", "Bundle", conc_en, conc_others))
rows.append(row("resourceClayLumpBundle", "Bundle", clay_en, clay_others))

header = [
    "Key", "File", "Type", "UsedInMainMenu", "NoTranslate", "KeepLoaded",
    "english", "Context / Alternate Text", *LANGS,
]
hr = load_hr_rows()
for key in VANILLA_ADD_I:
    src = hr.get(key)
    if not src:
        raise SystemExit(f"missing HelpfulRenames loc: {key}")
    patched = overlay_vanilla_ingredient(src)
    rows.append({col: patched.get(col, "") for col in header})

OUT.parent.mkdir(parents=True, exist_ok=True)
with OUT.open("w", encoding="utf-8", newline="") as f:
    w = csv.DictWriter(f, fieldnames=header, lineterminator="\n", quoting=csv.QUOTE_MINIMAL)
    w.writeheader()
    w.writerows(rows)

print(f"wrote {len(rows)} rows to {OUT}")
for dest in COPIES:
    if dest.parent.exists() or dest == COPIES[0]:
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(OUT, dest)
        print(f"copied {dest}")
    else:
        print(f"skip missing {dest.parent}")
