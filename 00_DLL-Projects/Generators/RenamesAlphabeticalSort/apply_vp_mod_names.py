"""Apply Helpful Renames naming to other AGF-VP mods. Run from repo; loc only."""
from __future__ import annotations

import csv
import io
import re
from pathlib import Path

REPO = Path(r"c:\GitHub\7D2D-Mods")
HR = REPO / "02_ActiveBuild" / "AGF-VP-zHelpfulRenames-v3.0.1" / "Config" / "Localization.csv"
AB = REPO / "02_ActiveBuild"

LANGS = [
    "english",
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
# csv columns after Key,File,Type,UsedInMainMenu,NoTranslate,KeepLoaded
# english=6, context=7, then LANGS[1:]
PARTS = {
    "english": "Parts",
    "german": "Teile",
    "spanish": "Partes",
    "french": "Pièces",
    "italian": "Parti",
    "japanese": "パーツ",
    "koreana": "부품",
    "polish": "Części",
    "brazilian": "Peças",
    "russian": "Части",
    "turkish": "Parçaları",
    "schinese": "部件",
    "tchinese": "零件",
}
SCHEMATIC = {
    "english": "Schematic",
    "german": "Bauplan",
    "spanish": "Esquema",
    "french": "Schéma",
    "italian": "Schema",
    "japanese": "設計図",
    "koreana": "도면",
    "polish": "Schemat",
    "brazilian": "Diagrama",
    "russian": "Схема",
    "turkish": "Şema",
    "schinese": "设计图",
    "tchinese": "原理圖",
}
FUEL = {
    "english": "Fuel",
    "german": "Brennstoff",
    "spanish": "Combustible",
    "french": "Carburant",
    "italian": "Carburante",
    "japanese": "燃料",
    "koreana": "연료",
    "polish": "Paliwo",
    "brazilian": "Combustível",
    "russian": "Топливо",
    "turkish": "Yakıt",
    "schinese": "燃料",
    "tchinese": "燃料",
}
ORE = {
    "english": "Ore",
    "german": "Erz",
    "spanish": "Mineral",
    "french": "Minerai",
    "italian": "Minerale",
    "japanese": "鉱石",
    "koreana": "광석",
    "polish": "Ruda",
    "brazilian": "Minério",
    "russian": "Руда",
    "turkish": "Cevher",
    "schinese": "矿石",
    "tchinese": "礦石",
}

HEADER = [
    "Key",
    "File",
    "Type",
    "UsedInMainMenu",
    "NoTranslate",
    "KeepLoaded",
    "english",
    "Context / Alternate Text",
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


def load_rows(path: Path) -> tuple[list[str], dict[str, list[str]]]:
    text = path.read_text(encoding="utf-8")
    reader = csv.reader(io.StringIO(text))
    rows = list(reader)
    header = rows[0]
    by_key = {}
    for row in rows[1:]:
        if not row or not row[0] or row[0].startswith("<!--"):
            continue
        by_key[row[0]] = row
    return header, by_key


def lang_cells(row: list[str]) -> dict[str, str]:
    out = {"english": row[6] if len(row) > 6 else ""}
    names = LANGS[1:]
    for i, name in enumerate(names):
        idx = 8 + i
        out[name] = row[idx] if len(row) > idx else ""
    return out


def set_langs(row: list[str], langs: dict[str, str]) -> list[str]:
    while len(row) < 20:
        row.append("")
    row[6] = langs.get("english", row[6])
    for i, name in enumerate(LANGS[1:]):
        row[8 + i] = langs.get(name, row[8 + i] if len(row) > 8 + i else "")
    return row[:20]


def blank_row(key: str, file: str, typ: str) -> list[str]:
    return [key, file, typ, "", "", "", "", ""] + [""] * 12


TOOL_LEAD = re.compile(
    r"^(?:\[bbbb[0-9a-f]{2}\](?:\[-\])?\[eeee[0-9a-f]{2}\](?:\[-\])?(?:\[ffffff\](?:\[-\])?)? )+",
    re.I,
)
CAT_LEAD = re.compile(r"^(?:\[ffcc01\](?:\[-\])?)?(?:\[FFA94D\]\[[^\]]+\]\[-\] )+")
TAN_PAREN = re.compile(r"\[DECEA3\]\(([^)]*)\)(?:\[-\])?", re.I)


def hid(code: str) -> str:
    return f"[{code.strip('[]')}][-]"


def tan_qty(inner: str) -> str:
    return f"[DECEA3]({inner})[-]"


def close_tan_parens(cell: str) -> str:
    if not cell:
        return cell
    return TAN_PAREN.sub(lambda m: tan_qty(m.group(1)), cell)


def strip_tool_lead(val: str) -> str:
    return TOOL_LEAD.sub("", val).strip()


def strip_cat_lead(val: str) -> str:
    return CAT_LEAD.sub("", val).strip()


def wrap_cat(word: str, rest: str) -> str:
    return f"[FFA94D][{word}][-] {strip_cat_lead(rest)}"


def add_stack_suffix(cell: str, n: str) -> str:
    if not cell:
        return cell
    if re.search(r"\[DECEA3\]\(" + re.escape(n) + r"\)(?!\d)", cell):
        return close_tan_parens(cell)
    return cell.rstrip() + " " + tan_qty(n)


def add_bundle_stack(cell: str, n: str) -> str:
    if not cell:
        return cell
    if re.search(r" Bundle \[DECEA3\]\(" + re.escape(n) + r"\)(?!\d)", cell):
        return close_tan_parens(cell)
    cell = add_stack_suffix(cell, n)
    return re.sub(
        r" \[DECEA3\]\(" + re.escape(n) + r"\)(?!\d)(?:\[-\])?",
        f" Bundle {tan_qty(n)}",
        cell,
        count=1,
    )


def replace_stack(cell: str, old: str, new: str) -> str:
    return re.sub(
        r"\[DECEA3\]\(" + re.escape(old) + r"\)(?!\d)(?:\[-\])?",
        tan_qty(new),
        cell,
    )


def write_csv(path: Path, rows: list[list[str]]) -> None:
    buf = io.StringIO()
    w = csv.writer(buf, lineterminator="\n", quoting=csv.QUOTE_MINIMAL)
    w.writerow(HEADER)
    for row in rows:
        while len(row) < 20:
            row.append("")
        w.writerow(row[:20])
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(buf.getvalue(), encoding="utf-8")


DISA_OPEN = re.compile(
    r"\[DECEA3\]([^[]*?):(?!\[-\])(\s*)\[ffffff\]",
    re.I,
)
HIGHLIGHT_WHITE = re.compile(
    r"\[([0-9A-Fa-f]{6})\]((?:(?!\[-\]).)*?)\[ffffff\]",
    re.I,
)
HEX_OR_CLOSE = re.compile(r"\[[0-9A-Fa-f]{6}\]|\[-\]")


def unclosed_hex_count(text: str) -> int:
    n = 0
    for m in HEX_OR_CLOSE.finditer(text or ""):
        if m.group(0) == "[-]":
            n = max(0, n - 1)
        else:
            n += 1
    return n


def close_disassembled(val: str) -> str:
    if not val:
        return val
    val = DISA_OPEN.sub(r"[DECEA3]\1:[-]\2[ffffff]", val)
    if unclosed_hex_count(val):
        val = val.rstrip() + "[-]" * unclosed_hex_count(val)
    return val


def close_highlight_then_white(val: str) -> str:
    if not val:
        return val
    val = HIGHLIGHT_WHITE.sub(r"[\1]\2[-][ffffff]", val)
    n = unclosed_hex_count(val)
    if n:
        val = val.rstrip() + "[-]" * n
    return val


def close_open_hex_at_end(val: str) -> str:
    if not val:
        return val
    n = unclosed_hex_count(val)
    if n:
        return val.rstrip() + "[-]" * n
    return val


def patch_csv_langs(path: Path, fn) -> None:
    text = path.read_text(encoding="utf-8")
    rows = list(csv.reader(io.StringIO(text)))
    out = [rows[0]]
    for row in rows[1:]:
        if row and row[0] and not str(row[0]).startswith("<!--"):
            langs = lang_cells(row)
            for lang, val in langs.items():
                langs[lang] = fn(val)
            set_langs(row, langs)
        out.append(row)
    buf = io.StringIO()
    w = csv.writer(buf, lineterminator="\n", quoting=csv.QUOTE_MINIMAL)
    for row in out:
        w.writerow(row)
    path.write_text(buf.getvalue(), encoding="utf-8")


def patch_ammo_disassembly() -> None:
    path = AB / "AGF-VP-AmmoDisassembly-v2.0.0" / "Config" / "Localization.csv"
    patch_csv_langs(path, close_disassembled)


def patch_localization_patches() -> None:
    path = AB / "zzzAGF-Special-LocalizationPatches-v1.0.1" / "Config" / "Localization.csv"
    if not path.exists():
        return

    def fix(val: str) -> str:
        val = close_tan_parens(val)
        val = close_highlight_then_white(val)
        return val

    patch_csv_langs(path, fix)


def patch_drinkable_acid() -> None:
    path = AB / "AGF-VP-DrinkableAcid-v3.0.0" / "Config" / "Localization.csv"
    if not path.exists():
        return
    patch_csv_langs(path, close_open_hex_at_end)


def copy_hr_row(hr: dict[str, list[str]], key: str, file: str | None = None, typ: str | None = None) -> list[str]:
    row = list(hr[key])
    while len(row) < 20:
        row.append("")
    if file is not None:
        row[1] = file
    if typ is not None:
        row[2] = typ
    return row[:20]


def main() -> None:
    _, hr = load_rows(HR)

    # --- Flora Harvester ---
    flora_path = AB / "AGF-VP-FloraHarvester-v3.0.1" / "Config" / "Localization.csv"
    _, flora = load_rows(flora_path)
    flora_rows = []
    name_row = list(flora["meleeToolFloraHarvester"])
    langs = lang_cells(name_row)
    for k, v in langs.items():
        langs[k] = f"{hid('bbbb06')}{hid('eeee05')} {strip_tool_lead(v)}" if v else v
    name_row[1] = "items"
    name_row[2] = "Tool"
    set_langs(name_row, langs)
    flora_rows.append(name_row)
    flora_rows.append(flora["meleeToolFloraHarvesterDesc"])
    write_csv(flora_path, flora_rows)

    # --- Master Tool ---
    mt_path = AB / "AGF-VP-MasterTool-v7.1.1" / "Config" / "Localization.csv"
    _, mt = load_rows(mt_path)
    mt_out: list[list[str]] = []
    part_keys = [
        "resourceMasterToolButchering",
        "resourceMasterToolSalvaging",
        "resourceMasterToolHarvesting",
        "resourceMasterToolElectricals",
    ]
    for key in part_keys:
        row = list(mt[key])
        row[1] = ""
        row[2] = ""
        mt_out.append(row)
    for key in [
        "resourceMasterToolButcheringDesc",
        "resourceMasterToolSalvagingDesc",
        "resourceMasterToolHarvestingDesc",
        "resourceMasterToolElectricalsDesc",
    ]:
        mt_out.append(mt[key])

    tool_row = list(mt["meleeMasterTool"])
    langs = lang_cells(tool_row)
    for k, v in langs.items():
        langs[k] = f"{hid('bbbb07')}{hid('eeee01')} {strip_tool_lead(v)}" if v else v
    tool_row[1] = "items"
    tool_row[2] = "Tool"
    set_langs(tool_row, langs)
    mt_out.append(tool_row)
    mt_out.append(mt["meleeMasterTooldesc"])

    sch_row = list(mt["meleeMasterToolSchematic"])
    rest_langs = lang_cells(mt["meleeMasterTool"])
    langs = {
        lang: wrap_cat(SCHEMATIC[lang], strip_tool_lead(rest_langs[lang]))
        for lang in LANGS
    }
    langs = {lang: f"{hid('ffcc01')}{val}" for lang, val in langs.items()}
    sch_row[1] = "items"
    sch_row[2] = "Schematic"
    set_langs(sch_row, langs)
    mt_out.append(sch_row)
    mt_out.append(mt["meleeMasterToolSchematicDesc"])

    def patch_schematic_color(val: str, lang: str) -> str:
        # old [ffb400]Plural[-] -> hidden + orange [Schematic]
        val = re.sub(
            r"\[ffb400\][^\[]*\[-\]",
            f"{hid('ffcc01')}[FFA94D][{SCHEMATIC[lang]}][-]",
            val,
        )
        return re.sub(r"\[ffcc01\](?!\[-\])", hid("ffcc01"), val, flags=re.I)

    for key in [
        "noteAGFMasterTool",
        "noteAGFMasterToolDesc",
        "treasureMapMasterTool",
        "treasureMapMasterToolDesc",
        "treasureMapMasterToolSchematic",
        "treasureMapMasterToolSchematicDesc",
        "treasure_MasterTool",
        "treasure_MasterTool_offer",
        "treasure_MasterTool_completed",
        "modjobsstatement",
        "modjobsname",
    ]:
        row = list(mt[key])
        langs = lang_cells(row)
        for lang, val in langs.items():
            langs[lang] = patch_schematic_color(val, lang)
        set_langs(row, langs)
        mt_out.append(row)
    write_csv(mt_path, mt_out)

    # --- Fuel Burn Plus ---
    fuel_path = AB / "AGF-VP-FuelBurnPlus-v3.1.1" / "Config" / "Localization.csv"
    _, fuel = load_rows(fuel_path)
    fuel_rest = {
        "english": "Burn",
        "german": "Brennen",
        "spanish": "Quemadura",
        "french": "Calciner",
        "italian": "Ustione",
        "japanese": "燃焼",
        "koreana": "연소",
        "polish": "Spalanie",
        "brazilian": "Queima",
        "russian": "Горение",
        "turkish": "Yanma",
        "schinese": "燃烧",
        "tchinese": "燃燒",
    }
    times = [
        ("resourceWood10min", "aaaaaa", "10m"),
        ("resourceWood60min", "aaaaab", "60m"),
        ("resourceWood600min", "aaaaac", "600m"),
        ("resourceWood6000min", "aaaaad", "6000m"),
    ]
    fuel_out = []
    for key, sort_tag, t in times:
        row = blank_row(key, "items", "Resource")
        langs = {
            lang: f"[FFA94D][{FUEL[lang]}][-]{hid(sort_tag)} {fuel_rest[lang]} {tan_qty(t)}"
            for lang in LANGS
        }
        set_langs(row, langs)
        fuel_out.append(row)
    fuel_out.append(fuel["fuelAGFdesc"])
    write_csv(fuel_path, fuel_out)

    # --- Mining Plus (ore bundles use the matching [Ore] wrap + (6000)) ---
    mine_path = AB / "AGF-VP-MiningPlus-v2.0.0" / "Config" / "Localization.csv"
    _, mine = load_rows(mine_path)
    mine_out = [mine["resourceClayLumpBundleDesc"], mine["resourceCrushedSandBundledesc"]]
    for dst, src in (
        ("resourceClayLumpBundle", "resourceClayLump"),
        ("resourceCrushedSandBundle", "resourceCrushedSand"),
    ):
        row = copy_hr_row(hr, src, "items", "Bundle")
        row[0] = dst
        langs = lang_cells(row)
        for lang, val in langs.items():
            langs[lang] = add_bundle_stack(val, "6000")
        set_langs(row, langs)
        mine_out.append(row)
    write_csv(mine_path, mine_out)

    # --- Smelting Plus ---
    smelt_path = AB / "AGF-VP-SmeltingPlus-v3.0.0" / "Config" / "Localization.csv"
    _, smelt = load_rows(smelt_path)
    smelt_src = {
        "resourceScrapIronAdvSmelt": "resourceScrapIron",
        "resourceScrapBrassAdvSmelt": "resourceScrapBrass",
        "resourceScrapLeadAdvSmelt": "resourceScrapLead",
    }
    smelt_out = []
    for dst, src in smelt_src.items():
        row = copy_hr_row(hr, src, "items", "Resource")
        row[0] = dst
        langs = lang_cells(row)
        old = lang_cells(smelt[dst])
        for lang, val in langs.items():
            m = re.search(r"\[DECEA3\]\([^)]+\)", old.get(lang, ""))
            suffix = close_tan_parens(m.group(0) if m else "[DECEA3](Smelting 1:5)")
            langs[lang] = val.rstrip() + " " + suffix
        set_langs(row, langs)
        smelt_out.append(row)
    for key in (
        "resourceScrapIronAdvSmeltDesc",
        "resourceScrapBrassAdvSmeltDesc",
        "resourceScrapLeadAdvSmeltDesc",
    ):
        smelt_out.append(smelt[key])
    write_csv(smelt_path, smelt_out)

    # Craft Stack Eng/Batt/Cells: do not write loc names. It loads before
    # HelpfulRenames; stack suffixes are for bundles, not these items.
    # Rechargeable Battery is named in HelpfulRenames as Electric.

    # --- Simplified Stacks (loads after HelpfulRenames) ---
    ss_path = AB / "AGF-VP-SimplifiedStacks-v2.1.1" / "Config" / "Localization.csv"
    ammo_keys = [
        k
        for k in hr
        if k.startswith("ammoBundle") and k != "ammoBundleBlunderbuss"
    ]
    ss_out = []
    for key in ammo_keys:
        row = copy_hr_row(hr, key, "items", "Ammo")
        langs = lang_cells(row)
        for lang, val in langs.items():
            val = replace_stack(val, "75", "500")
            val = replace_stack(val, "100", "500")
            langs[lang] = val
        set_langs(row, langs)
        ss_out.append(row)
    gas = copy_hr_row(hr, "ammoGasCanBundle", "items", "Bundle")
    langs = lang_cells(gas)
    for lang, val in langs.items():
        langs[lang] = replace_stack(val, "5000", "30000")
    set_langs(gas, langs)
    ss_out.append(gas)
    gp = copy_hr_row(hr, "resourceGunPowderBundle", "items", "Bundle")
    langs = lang_cells(gp)
    for lang, val in langs.items():
        langs[lang] = replace_stack(val, "1000", "6000")
    set_langs(gp, langs)
    ss_out.append(gp)
    write_csv(ss_path, ss_out)

    # --- Rebundle Bundles (vanilla unpack counts; SS overrides later) ---
    rb_keys = [
        "ammoGasCanBundle",
        "resourceGunPowderBundle",
        "resourceLockPickBundle",
        "resourceRockSmallBundle",
        "resourceWoodBundle",
        "resourceScrapIronBundle",
        "resourcePotassiumNitratePowderBundle",
        "resourceLeadBundle",
        "resourceCoalBundle",
        "resourceOilShaleBundle",
    ] + ammo_keys
    rb_out = []
    seen = set()
    for key in rb_keys:
        if key in seen or key not in hr:
            continue
        seen.add(key)
        typ = "Ammo" if key.startswith("ammoBundle") else "Bundle"
        rb_out.append(copy_hr_row(hr, key, "items", typ))
    write_csv(AB / "AGF-VP-RebundleBundles-v2.1.1" / "Config" / "Localization.csv", rb_out)

    patch_ammo_disassembly()
    patch_localization_patches()
    patch_drinkable_acid()

    print(
        "wrote Flora, MasterTool, FuelBurn, MiningPlus, SmeltingPlus, "
        "SimplifiedStacks, Rebundle, AmmoDisassembly, LocalizationPatches, DrinkableAcid"
    )


if __name__ == "__main__":
    main()
