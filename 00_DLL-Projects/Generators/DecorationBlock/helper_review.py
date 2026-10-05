"""Helper-list review pass. Excel is not edited."""
from __future__ import annotations

import re
from pathlib import Path

COLOR_LABEL = {
    "ArmyGreen": "Army Green",
    "DarkGreen": "Dark Green",
    "Aqua": "Aqua",
    "White": "White",
    "Black": "Black",
    "Brown": "Brown",
    "Yellow": "Yellow",
    "Green": "Green",
    "Grey": "Grey",
    "Gray": "Grey",
    "Silver": "Silver",
    "Orange": "Orange",
    "Purple": "Purple",
    "Pink": "Pink",
    "Brass": "Brass",
    "Blue": "Blue",
    "Red": "Red",
    "Tan": "Tan",
}

DEFAULT_COLOR = {
    "bed02": "Black",
    "bunkBedMade": "Black",
    "bunkBedMessy": "Black",
    "cntSportsBag01": "White",
    "cntSportsBag02": "Grey",
    "cntDumpster": "Green",
    "cntDumpsterFlies": "Green",
}

SHIP_CRATE = {
    "cntShippingCrateBookstore": "Crack-A-Book",
    "cntShippingCrateCarParts": "Pass-N-Gas",
    "cntShippingCrateHero": "Hero",
    "cntShippingCrateLabEquipment": "Pop-N-Pills",
    "cntShippingCrateMoPowerElectronics": "Mo Power Electronics",
    "cntShippingCrateSavageCountry": "Savage Country",
    "cntShippingCrateShamway": "Shamway",
    "cntShippingCrateShotgunMessiah": "Shotgun Messiah",
    "cntShippingCrateWorkingStiffs": "Working Stiffs",
}

BURIED = {
    "cntBuriedFoodStashChest": "Blue",
    "cntBuriedLootStashChest": "Green",
    "cntBuriedWeaponChest": "Red",
    "cntIntroBuriedFoodChest": "Brown",
}

CHILD_ORDER = {
    "Furniture": [
        "Armoire",
        "Bed",
        "Barstool",
        "Couch",
        "Camping",
        "Folding",
        "Oldchair",
        "Wheelchair",
        "Seat",
        "Booth",
        "Foodbooth",
        "Table",
    ],
    "Electrical": ["Stuff", "Defense", "Generator", "Station", "City"],
    "Entertainment": ["Video", "Sound", "Electrical"],
    "Death": ["Hanging", "Rope", "Climbable", "General"],
    "Vehicle": [
        "Sedan",
        "Police",
        "Suv",
        "Service",
        "Semi-Truck",
        "Excavator",
        "Claw",
        "Destroyed",
    ],
    "Building": ["Sidewalk", "Duct", "Elevator"],
    "Outdoor": ["Barbed", "News", "Potty"],
    "Flora": ["Mushroom", "Cactus", "Crop", "Flower", "Grass"],
}


def _base(english: str, p) -> str:
    return p.KIND_SUFFIX_RE.sub("", english).rstrip()


def _set_en(row: dict, base: str, p, loc: dict, kind: str | None = None) -> None:
    tag_kind = kind or row.get("Kind") or "plain"
    if tag_kind == "plain":
        tag_kind = "deco"
    row["EnglishName"] = p.apply_kind_tag(base, tag_kind)
    loc[row["BlockName"]] = row["EnglishName"]


def _camel_words(src: str) -> list[str]:
    return re.findall(r"[A-Z]?[a-z]+|[A-Z]+(?=[A-Z][a-z]|\d|$)|[0-9]+", src)


def apply_helper_review(rows: list[dict], p, dry: bool) -> tuple[list[str], dict, dict]:
    p.merge_canvas_edits(rows)
    loc: dict[str, str] = {}

    for r in rows:
        src = p.source_of(r["BlockName"])
        en = _base(r["EnglishName"], p)

        if src in SHIP_CRATE:
            _set_en(r, f"Shipping Crate Sealed, {SHIP_CRATE[src]}", p, loc)
            continue
        if src == "cntMunitionsBoxArmy":
            _set_en(r, "Munitions Box, Army Green", p, loc)
            continue
        if src in ("cntFootlockerClosedSuperCorn", "cntFootlockerOpenSuperCorn"):
            st = "Closed" if "Closed" in src else "Open"
            _set_en(r, f"Footlocker, {st}, Dark Green", p, loc)
            continue
        if src in BURIED:
            _set_en(r, f"Reinforced Chest, {BURIED[src]}", p, loc)
            continue
        if src == "cntLootChestHero":
            _set_en(r, "Reinforced Chest", p, loc)
            continue
        if src.startswith("cntDumpster"):
            flies = "Flies" in src
            col = p.color_of(src)
            if col == "_":
                col = "Green"
            label = COLOR_LABEL.get(col, col)
            name = f"Dumpster, Flies, {label}" if flies else f"Dumpster, {label}"
            _set_en(r, name, p, loc)
            continue
        if src.startswith("cntCasketModern"):
            st = "Open" if "Open" in src else "Closed"
            col = p.color_of(src)
            label = COLOR_LABEL.get(col, col) if col != "_" else ""
            name = f"Casket, {st}, {label}" if label else f"Casket, {st}"
            _set_en(r, name, p, loc)
            continue
        if src.startswith("cntBookShelf"):
            rest = src[len("cntBookShelf") :]
            cam = _camel_words(rest)
            core = []
            status = []
            i = 0
            while i < len(cam):
                w = cam[i]
                if w in ("Empty", "Full"):
                    if i + 1 < len(cam) and cam[i + 1].isdigit():
                        status.append(f"{w} {cam[i + 1]}")
                        i += 2
                        continue
                    status.append(w)
                    i += 1
                    continue
                core.append(w)
                i += 1
            head = ("Book Shelf " + " ".join(core)).strip()
            tail = ", ".join(status)
            _set_en(r, f"{head}, {tail}" if tail else head, p, loc)
            continue
        if src.startswith("cntBookPile"):
            _set_en(r, f"Book Pile {src.replace('cntBookPile', '')}", p, loc)
            continue
        if src.startswith("cntChemPile"):
            _set_en(r, f"Chem Pile, {src.replace('cntChemPile', '')}", p, loc)
            continue
        if src.startswith("cntMedicLootPile"):
            rest = src.replace("cntMedicLootPile", "")
            if rest.endswith("Corner"):
                _set_en(r, f"Medic Pile {rest[:-6]}, Corner", p, loc)
            else:
                _set_en(r, f"Medic Pile {rest}", p, loc)
            continue
        if src.startswith("tv"):
            ebs = src.endswith("EBS")
            body = src[:-3] if ebs else src
            size = ""
            for tok in ("3x2", "2x2", "2x1", "1x1"):
                if tok in body:
                    size = tok
                    body = body.replace(tok, "")
                    break
            fallen = "Fallen" in body
            wall = "Wall" in body
            scale = ""
            for tok, lab in (
                ("ExtraSmall", "Extra Small"),
                ("Small", "Small"),
                ("Large", "Large"),
            ):
                if tok in body:
                    scale = lab
                    break
            if src.startswith("tvCRT"):
                name = "TV"
            else:
                bits = []
                if scale:
                    bits.append(scale)
                if wall:
                    bits.append("Wall")
                if fallen:
                    bits.append("Fallen")
                bits.append("TV")
                name = " ".join(bits)
                if size and size != "1x1":
                    name = f"{name}, {size}"
            if ebs:
                name = f"{name}, EBS"
            _set_en(r, name, p, loc)
            continue
        if src == "cntServiceTruckMoPower":
            _set_en(r, "Service Truck, Mo Power", p, loc)
            continue
        if src == "cntServiceTruckWorkingStiffTools":
            _set_en(r, "Service Truck, Working Stiff Tools", p, loc)
            continue
        if (
            src.startswith("cntStoreShelf")
            or src.startswith("cntClothesShelf")
            or "StoreShelfElectronics" in src
        ):
            _set_en(r, _shelf_name(src), p, loc)
            continue
        if src.startswith("glassBroken") or src.startswith("glassDebris"):
            restore = {
                "glassBroken_01": "Broken Glass 01",
                "glassBroken_02": "Broken Glass 02",
                "glassBroken_03": "Broken Glass 03",
                "glassBrokenCapLeft": "Broken Glass Cap Left",
                "glassBrokenCapRight": "Broken Glass Cap Right",
                "glassDebris01": "Glass Debris 01",
            }.get(src)
            if restore:
                _set_en(r, restore, p, loc, kind="deco")
            continue
        if src.startswith("glass") or src.startswith("stainedGlass"):
            _set_en(r, _glass_name(src), p, loc, kind="structure")
            continue
        if src == "cntBathTubGore":
            r["Parent"], r["Child"] = "Household", "Bathroom"
            _set_en(r, "Bathtub, Gore", p, loc, kind="loot")
            continue
        if src.startswith("modularRope"):
            climb = "SideCentered" in src
            if src == "modularRopeTiled":
                name = "Rope"
            elif src == "modularRopeTiledSideCentered":
                name = "Rope, Side, Climbable"
            elif src == "modularRopeTopTied":
                name = "Rope, Top Tied"
            elif src == "modularRopeTopTiedSideCentered":
                name = "Rope, Side, Top Tied, Climbable"
            else:
                name = en
            r["Parent"] = "Death"
            r["Child"] = "Climbable" if climb else "Rope"
            _set_en(r, name, p, loc)
            continue
        if src in ("powerSwitch01", "powerSwitch02"):
            _set_en(r, f"Switch {src[-2:]}", p, loc)
            continue
        if src.startswith("pushButtonSwitch"):
            _set_en(r, f"Push Button Switch {src.replace('pushButtonSwitch', '')}", p, loc)
            continue
        if src == "switch":
            _set_en(r, "Switch, Player", p, loc)
            continue
        if src == "cntTiltTruckFullFlies":
            _set_en(r, "Tilt Truck, Full, Flies", p, loc)
            continue
        if src.startswith("mushroomRadiated"):
            _set_en(r, f"Radiated Mushrooms {src.replace('mushroomRadiated', '')}", p, loc)
            continue
        col = p.color_of(src)
        if col == "_" and src in DEFAULT_COLOR:
            label = COLOR_LABEL.get(DEFAULT_COLOR[src], DEFAULT_COLOR[src])
            if label.lower() not in en.lower():
                en = f"{en}, {label}"
        elif col != "_":
            label = COLOR_LABEL.get(col, col)
            if label.lower() not in en.lower():
                en = f"{en}, {label}"

        elif en != _base(r["EnglishName"], p):
            _set_en(r, en, p, loc)

    for r in rows:
        src = p.source_of(r["BlockName"])
        if src == "decoHeadphonesBroken":
            r["Parent"], r["Child"] = "Entertainment", "Sound"
        elif src == "loudspeaker":
            r["Parent"], r["Child"] = "Entertainment", "Sound"
        elif src.startswith("cntNews") or src.startswith("cntNewspaper"):
            r["Parent"], r["Child"] = "Outdoor", "News"
        elif src.startswith("barbed"):
            r["Parent"], r["Child"] = "Outdoor", "Barbed"
        elif src.startswith("duct"):
            r["Parent"], r["Child"] = "Building", "Duct"
        elif src.startswith("escalator") or src.startswith("elevator"):
            r["Parent"], r["Child"] = "Building", "Elevator"
        elif src.startswith("commercial") and "Meter" in src:
            r["Parent"], r["Child"] = "Electrical", "Stuff"
        elif src == "residentialSingleMeterDisconnect":
            r["Parent"], r["Child"] = "Electrical", "Stuff"
        elif src.startswith("breakerBox"):
            r["Parent"], r["Child"] = "Electrical", "Stuff"
        elif src in ("autoTurret", "shotgunTurret", "m60Turret", "flamethrowerTrap", "dartTrap"):
            r["Parent"], r["Child"] = "Electrical", "Defense"
        elif src.startswith("mushroom") or src.startswith("plantedMushroom"):
            r["Parent"], r["Child"] = "Flora", "Mushroom"
        elif src.startswith("noCorpseHangingLog") or src.startswith("corpseHangingLog"):
            r["Parent"], r["Child"] = "Death", "Hanging"
        elif src.startswith("tree") and r["Parent"] in ("Crop", "Tree"):
            r["Parent"], r["Child"] = "Tree", "General"
        elif src.startswith("cntCar03Sedan") and "v08" in src:
            r["Parent"], r["Child"] = "Vehicle", "Police"
        elif src.startswith("excavatorClaw"):
            r["Parent"], r["Child"] = "Vehicle", "Claw"
        elif src.startswith("excavator"):
            r["Parent"], r["Child"] = "Vehicle", "Excavator"
        elif r["Parent"] == "Seat":
            r["Parent"] = "Furniture"
        elif src.startswith("painting") and r["Parent"] == "Sign":
            r["Parent"], r["Child"] = "Painting", "Person"
        elif src.startswith("cntSemiTruck01ModularRearEnd"):
            r["Kind"] = "plain"
            _set_en(r, _base(r["EnglishName"], p), p, loc, kind="deco")
        elif src.startswith("cntDumpster"):
            r["Parent"], r["Child"] = "Household", "Trash"
        elif src.startswith("cntFootlocker"):
            r["Parent"], r["Child"] = "Household", "General"
        elif src.startswith("cntBuried") or src in ("cntIntroBuriedFoodChest", "cntLootChestHero"):
            r["Parent"], r["Child"] = "Decoration", "Loot"
        elif src.startswith("concretePlateRound13m"):
            r["Parent"], r["Child"] = "Building", "Sidewalk"

    parents: list[str] = []
    extra: dict[str, list[str]] = {}
    for r in rows:
        if r.get("Kind") == "campfire":
            parents.append("Kitchen")
            extra["Kitchen"] = ["Campfires"]
            break
    for r in rows:
        pnm, c = r["Parent"], r["Child"]
        if pnm not in parents:
            parents.append(pnm)
        extra.setdefault(pnm, [])
        if c not in extra[pnm]:
            extra[pnm].append(c)
    for pnm, order in CHILD_ORDER.items():
        if pnm not in extra:
            continue
        seen = extra[pnm]
        extra[pnm] = [c for c in order if c in seen] + [c for c in seen if c not in order]

    _sort_rows(rows, p, parents, extra)

    if not dry:
        _patch_draft_plate_icons()
        _copy_plate_icons(rows, p)

    p.set_parent_order(parents, dry)
    p.write_helper_look(rows, dry)
    p.write_canvas_edits(rows, dry)
    print(f"apply_helper_review rows={len(rows)} loc={len(loc)} parents={len(parents)}")
    return parents, extra, loc


def _shelf_name(src: str) -> str:
    for prefix, head in (
        ("cntStoreShelfElectronics", "Electronics Store Shelf"),
        ("cntClothesShelf", "Clothes Shelf"),
        ("cntStoreShelf", "Store Shelf"),
    ):
        if src.startswith(prefix):
            rest = src[len(prefix) :]
            words = _camel_words(rest) if rest else []
            core = []
            tail = []
            hit_status = False
            for w in words:
                if w in ("Empty", "Full", "Open", "Closed", "Screensaver") or (
                    hit_status and w.isdigit()
                ):
                    hit_status = True
                    tail.append(w)
                elif hit_status:
                    tail.append(w)
                else:
                    core.append(w)
            head_s = head + ((" " + " ".join(core)) if core else "")
            if not tail:
                return head_s
            pretty_tail = []
            i = 0
            while i < len(tail):
                w = tail[i]
                if w == "Screensaver" and i + 1 < len(tail) and tail[i + 1].isdigit():
                    pretty_tail.append(f"Screensaver {tail[i + 1]}")
                    i += 2
                    continue
                pretty_tail.append(w)
                i += 1
            return head_s + ", " + ", ".join(pretty_tail)
    return src


def _glass_name(src: str) -> str:
    kind = []
    if "Industrial" in src:
        kind.append("Industrial")
    if "Business" in src:
        kind.append("Business")
    if "Opaque" in src and "Bulletproof" in src:
        kind.append("Bulletproof Opaque")
    elif "Bulletproof" in src:
        kind.append("Bulletproof")
    if "Stained" in src:
        kind.append("Stained")
    shape = "Block"
    if "RampSheet" in src:
        shape = "Ramp Sheet"
    elif "Ramp" in src:
        shape = "Ramp"
    elif "Sheet" in src:
        shape = "Pane"
    elif "TubePlateCorner" in src:
        shape = "Tube Plate, Corner"
    elif "TubePlateCenter" in src:
        shape = "Tube Plate, Center"
    elif "CTRPlate" in src:
        shape = "Plate, Centered"
    elif "PlateCurved" in src:
        shape = "Plate, Curved"
    elif "Plate" in src:
        shape = "Plate"
    type_s = ", ".join(kind)
    if shape == "Block":
        return f"Glass Block, {type_s}" if type_s else "Glass Block"
    if type_s:
        return f"Glass {shape}, {type_s}"
    return f"Glass {shape}"


def _sort_rows(rows: list[dict], p, parents: list[str], extra: dict) -> None:
    orig = {r["BlockName"]: i for i, r in enumerate(rows)}
    stem_anchor: dict[str, int] = {}
    for r in rows:
        stem = p.model_stem(p.source_of(r["BlockName"]))
        stem_anchor[stem] = min(stem_anchor.get(stem, 10**9), orig[r["BlockName"]])

    def color_rank(src: str) -> tuple:
        col = p.color_of(src)
        if col == "_" and src in DEFAULT_COLOR:
            col = DEFAULT_COLOR[src]
        if col == "_":
            return ("",)
        return (COLOR_LABEL.get(col, col),)

    def extra_key(r: dict) -> tuple:
        src = p.source_of(r["BlockName"])
        st = p.status_bucket(src, r.get("EnglishName") or "")
        col = color_rank(src)
        stem = p.model_stem(src)
        if src.startswith("tv"):
            ebs = 1 if src.endswith("EBS") else 0
            size = 50
            for tok, rk in (("ExtraSmall", 0), ("Small", 1), ("Large", 2)):
                if tok in src:
                    size = rk
                    break
            dim = 0
            for i, tok in enumerate(("1x1", "2x1", "2x2", "3x2")):
                if tok in src:
                    dim = i
                    break
            wall = 1 if "Wall" in src else 0
            fallen = 1 if "Fallen" in src else 0
            return (stem_anchor[stem], size, wall, fallen, dim, ebs, st, col)
        if src.startswith("abandonedSUV"):
            doors = 1 if "Doors" in src else 0
            base = src.replace("Doors", "")
            return (stem_anchor.get(p.model_stem(base), orig[r["BlockName"]]), doors, st, col)
        if src.startswith("cntDumpster"):
            flies = 1 if "Flies" in src else 0
            return (0, flies, col, st)
        if src.startswith("commercial") and "Meter" in src:
            idx = 9
            for i, w in enumerate(("Single", "Dual", "Triple", "Quad")):
                if w in src:
                    idx = i
                    break
            disc = 1 if "Disconnect" in src else 0
            return (idx, disc)
        if src in ("autoTurret", "shotgunTurret", "m60Turret", "flamethrowerTrap", "dartTrap"):
            order = ["dartTrap", "shotgunTurret", "autoTurret", "m60Turret", "flamethrowerTrap"]
            return (order.index(src) if src in order else 9,)
        if src.startswith("concretePlateRound13m"):
            side = 0 if "Center" in src else (1 if "Left" in src else 2)
            num = 0
            m = re.search(r"(\d+)$", src)
            if m:
                num = int(m.group(1))
            return (0, side, num)
        return (stem_anchor[stem], st, col, orig[r["BlockName"]])

    def key(r: dict) -> tuple:
        if r.get("Kind") == "campfire":
            return (-1, 0, (orig[r["BlockName"]],), orig[r["BlockName"]])
        try:
            pi = parents.index(r["Parent"])
        except ValueError:
            pi = 99
        kids = extra.get(r["Parent"], [])
        try:
            ci = kids.index(r["Child"])
        except ValueError:
            ci = 99
        return (pi, ci, extra_key(r), orig[r["BlockName"]])

    rows.sort(key=key)
    for i, r in enumerate(rows, 1):
        r["SortOrder1"] = p.pad_sort(i)


def _patch_draft_plate_icons() -> None:
    path = Path(r"c:\GitHub\7D2D-Mods\01_Draft\AGF-VP-DecorationBlock-v3.0.3\Config\blocks.xml")
    if not path.is_file():
        return
    text = path.read_text(encoding="utf-8")
    n = text.count('CustomIcon" value="concreteNoUpgradeMaster"')
    if not n:
        return
    path.write_text(
        text.replace(
            'CustomIcon" value="concreteNoUpgradeMaster"',
            'CustomIcon" value="glassIndustrialPlate"',
        ),
        encoding="utf-8",
    )
    print(f"draft blocks.xml CustomIcon plate icons {n}")


def _copy_plate_icons(rows: list[dict], p) -> None:
    dests = {r["BlockName"] for r in rows if "concretePlateRound13m" in r["BlockName"]}
    if not dests:
        return
    text = p.CANVAS.read_text(encoding="utf-8")
    raw, rlo, rhi = p.extract_js_value(text, "RAW", "[")
    n = 0
    for row in raw:
        if row and row[0] in dests and len(row) > 5:
            row[5] = "glassindustrialplate"
            n += 1
    text = p.splice(text, rlo, rhi, raw)
    p.CANVAS.write_text(text, encoding="utf-8")
    print(f"canvas plate icon key set on {n} round plates")


COLOR_ALPHA = [
    "ArmyGreen",
    "Black",
    "Blue",
    "Brown",
    "DarkGreen",
    "Green",
    "Grey",
    "Orange",
    "Pink",
    "Purple",
    "Red",
    "Tan",
    "White",
    "Yellow",
    "Aqua",
    "Silver",
    "Brass",
]

DRINK_SOURCES = {
    "decoSodaCan6Pack",
    "decoSodaCan6PackCorner",
    "decoSodaCanSingle",
    "decoWineBottle",
    "decoWineBottleCorner",
    "decoWineBottlePile01",
    "decoWineBottlePile02",
    "decoWineBottleShelfRow",
    "drinkingfountainDouble",
    "drinkingfountainSingle",
    "wineBarrel",
    "wineBarrelPlain",
    "wineBarrelSet",
}

PAINTING_BEFORE_CANVAS = {
    "posterCat",
    "posterCats",
    "posterSparky",
    "paintingAbstract01_2x2",
    "paintingAbstract02_2x2",
    "paintingAbstract03_2x2",
    "paintingAbstract04_2x2",
    "paintingBen",
    "paintingDerek",
    "paintingDuke",
    "paintingKen",
    "paintingLorien",
    "paintingNoah",
}

TREE_SEED_SOURCES = {
    "treePlantedMountainPine1m",
    "treePlantedOak1m",
    "treePlantedWinterPine1m",
}


def _take(rows: list[dict], pred) -> list[dict]:
    taken = [r for r in rows if pred(r)]
    rows[:] = [r for r in rows if not pred(r)]
    return taken


def _insert_after(rows: list[dict], after_id: str, chunk: list[dict]) -> None:
    if not chunk:
        return
    out: list[dict] = []
    placed = False
    for r in rows:
        out.append(r)
        if r["BlockName"] == after_id:
            out.extend(chunk)
            placed = True
    if not placed:
        out.extend(chunk)
    rows[:] = out


def _insert_before(rows: list[dict], before_id: str, chunk: list[dict]) -> None:
    if not chunk:
        return
    out: list[dict] = []
    placed = False
    for r in rows:
        if r["BlockName"] == before_id and not placed:
            out.extend(chunk)
            placed = True
        out.append(r)
    if not placed:
        out.extend(chunk)
    rows[:] = out


def _color_idx(src: str, p) -> int:
    if "SuperCorn" in src:
        col = "DarkGreen"
    else:
        col = p.color_of(src)
        if col == "_":
            col = "Grey"
        col = COLOR_LABEL.get(col, col)
        col = col.replace(" ", "")
        if col == "Gray":
            col = "Grey"
    try:
        return COLOR_ALPHA.index(col if col in COLOR_ALPHA else col)
    except ValueError:
        # ArmyGreen etc. already match; "Army Green" collapsed above
        for i, tok in enumerate(COLOR_ALPHA):
            if tok.lower() == col.lower():
                return i
        return 99


def apply_list_punch(rows: list[dict], p, dry: bool) -> dict:
    loc: dict[str, str] = {}

    def src_of(r: dict) -> str:
        return p.source_of(r["BlockName"])

    # --- names ---
    for r in rows:
        src = src_of(r)
        if src.startswith("cntFootlocker"):
            st = "Closed" if "Closed" in src else "Open"
            if "SuperCorn" in src:
                label = "Dark Green"
            else:
                col = p.color_of(src)
                label = COLOR_LABEL.get(col, col) if col != "_" else ""
            name = f"Footlocker, {st}, {label}" if label else f"Footlocker, {st}"
            _set_en(r, name, p, loc)
        elif src.startswith("oldChair1"):
            broken = "Broken" in src
            col = p.color_of(src)
            if col == "_":
                col = "Grey"
            label = COLOR_LABEL.get(col, col)
            name = f"Old Chair, Broken, {label}" if broken else f"Old Chair, {label}"
            _set_en(r, name, p, loc)
        elif src == "modularRopeTiled":
            _set_en(r, "Rope", p, loc)
        elif src == "modularRopeTopTied":
            _set_en(r, "Rope, Top Tied", p, loc)
        elif src == "modularRopeTiledSideCentered":
            _set_en(r, "Rope, Side, Climbable", p, loc)
        elif src == "modularRopeTopTiedSideCentered":
            _set_en(r, "Rope, Side, Top Tied, Climbable", p, loc)

    # --- cats ---
    for r in rows:
        src = src_of(r)
        if src.startswith("cinderBlocks"):
            r["Parent"], r["Child"] = "Decoration", "Trash"
        elif (
            src.startswith("noCorpseHangingLog")
            or src.startswith("corpseHangingRope")
            or src.startswith("corpseHangingLog")
        ):
            r["Parent"], r["Child"] = "Death", "Hanging"
        elif src in ("cntTrashPile10", "cntTrashPile11"):
            r["Parent"], r["Child"] = "Decoration", "Trash"
        elif src.startswith("cntWoodenChestRotten"):
            r["Parent"], r["Child"] = "Decoration", "Loot"
        elif src == "torchWallHolder":
            r["Parent"], r["Child"] = "Light", "Placeable"
        elif src == "planter":
            r["Parent"], r["Child"] = "Flora", "General"
        elif src.startswith("plantHydroponic"):
            r["Parent"], r["Child"] = "Flora", "Potted"

    # --- remove untargetable tree seeds + cardboard-model Hidden Stash ---
    removed = _take(
        rows,
        lambda r: src_of(r) in TREE_SEED_SOURCES or src_of(r) == "cntLootCrateHero",
    )
    print(f"removed {len(removed)} helper rows (tree seeds / hidden stash)")

    # --- kitchen: dishwashers -> fridges -> mini cooler -> picnic coolers -> water -> drinks ---
    fridges = _take(
        rows,
        lambda r: r["BlockName"].startswith("agfDecocntFridgeStainlessSteel")
        or r["BlockName"].startswith("agfDecocntRetroFridge"),
    )
    fridges.sort(
        key=lambda r: (
            0 if "Stainless" in r["BlockName"] else 1,
            1 if "Open" in r["BlockName"] else 0,
            r["BlockName"],
        )
    )
    mini = _take(rows, lambda r: "MiniBeverageCooler" in src_of(r))
    coolers = _take(
        rows,
        lambda r: src_of(r).startswith("cntCooler") and "MiniBeverage" not in src_of(r),
    )
    water = _take(rows, lambda r: src_of(r).startswith("cntWaterCooler"))
    water.sort(
        key=lambda r: (
            0 if src_of(r) == "cntWaterCoolerFull" else 1,
            1 if "Side" in src_of(r) else 0,
            src_of(r),
        )
    )
    drinks = _take(rows, lambda r: src_of(r) in DRINK_SOURCES)
    _insert_after(
        rows,
        "agfDecocntDishwasherEmptyYellow",
        fridges + mini + coolers + water + drinks,
    )

    # --- footlockers: Closed then Open, color alpha ---
    lockers = _take(rows, lambda r: src_of(r).startswith("cntFootlocker"))
    lockers.sort(
        key=lambda r: (
            0 if "Closed" in src_of(r) else 1,
            _color_idx(src_of(r), p),
            src_of(r),
        )
    )
    _insert_after(rows, "agfDecocntFileCabinetTallOpenGrey", lockers)

    # --- wall mirrors after open medicine cabinet ---
    mirrors = _take(rows, lambda r: src_of(r).startswith("wallMirror"))
    _insert_after(rows, "agfDecocntMedicineCabinetOpen", mirrors)

    # --- old chairs: intact then broken ---
    chairs = _take(rows, lambda r: src_of(r).startswith("oldChair1"))
    chairs.sort(
        key=lambda r: (
            1 if "Broken" in src_of(r) else 0,
            _color_idx(src_of(r), p),
            src_of(r),
        )
    )
    _insert_after(rows, "agfDecochairWood01", chairs)

    # --- plain reinforced chest before color variants ---
    plain_chest = _take(rows, lambda r: src_of(r) == "cntLootChestHero")
    _insert_before(rows, "agfDecocntBuriedFoodStashChest", plain_chest)

    # --- cinder blocks with trash, after broken glass ---
    cinder = _take(rows, lambda r: src_of(r).startswith("cinderBlocks"))
    _insert_after(rows, "agfDecoglassDebris01", cinder)

    # --- hanging logs / corpses together in death ---
    empty_logs = _take(rows, lambda r: src_of(r).startswith("noCorpseHangingLog"))
    hang_ropes = _take(rows, lambda r: src_of(r).startswith("corpseHangingRope"))
    hang_logs = _take(rows, lambda r: src_of(r).startswith("corpseHangingLog"))
    hang_ropes.sort(key=lambda r: (_color_idx(src_of(r), p), src_of(r)))
    hang_logs.sort(
        key=lambda r: (
            int(re.search(r"Log(\d+)", src_of(r)).group(1)) if re.search(r"Log(\d+)", src_of(r)) else 9,
            _color_idx(src_of(r), p),
            src_of(r),
        )
    )
    _insert_after(
        rows,
        "agfDecocntCoffinWildWestStack",
        empty_logs + hang_ropes + hang_logs,
    )

    # --- flora ends with planter + hydroponic ---
    flora_end = _take(
        rows,
        lambda r: src_of(r) == "planter" or src_of(r).startswith("plantHydroponic"),
    )
    flora_end.sort(
        key=lambda r: (0 if src_of(r) == "planter" else 1, src_of(r))
    )
    _insert_after(rows, "agfDecotreeWinterEverGreenLarge", flora_end)

    # --- paintings/posters immediately before canvas ---
    art = _take(rows, lambda r: src_of(r) in PAINTING_BEFORE_CANVAS)
    _insert_before(rows, "agfDecopictureCanvas_01a", art)

    # --- newspaper dispensers after mail drop, before ATM ---
    papers = _take(
        rows,
        lambda r: src_of(r).startswith("cntNewsDispense")
        or src_of(r).startswith("cntNewspaperDispenser"),
    )
    _insert_after(rows, "agfDecocntPostMailbox", papers)

    # --- trailing like-items ---
    trash_tail = _take(rows, lambda r: src_of(r) in ("cntTrashPile10", "cntTrashPile11"))
    _insert_after(rows, "agfDecocntTrashPile09", trash_tail)
    rotten = _take(rows, lambda r: src_of(r).startswith("cntWoodenChestRotten"))
    _insert_after(rows, "agfDecocntWoodenChestMilitaryOpen", rotten)
    torch = _take(rows, lambda r: src_of(r) == "torchWallHolder")
    _insert_after(rows, "agfDecowallTorchLight", torch)

    # vehicle parts after last whole vehicle (backhoe), not between crushed cars and minivans
    parts = _take(rows, lambda r: r.get("Parent") == "Vehicle" and r.get("Child") == "Parts")
    _insert_after(rows, "agfDecobackhoeYellow", parts)

    for i, r in enumerate(rows, 1):
        r["SortOrder1"] = p.pad_sort(i)

    print(f"list punch restamped {len(rows)} rows" + (" (dry)" if dry else ""))
    return loc

