---
name: renames-alphabetical-sort
description: >-
  Work on AGF-VP-RenamesAlphabeticalSort localization table and its canvas
  spreadsheet. Use when the user mentions Renames Alphabetical Sort, the
  renames canvas, item/block naming prefixes, Auto Sort names, or asks to
  change categories, vanilla vs mod English, or loc colors for that mod.
---

# Renames Alphabetical Sort

Do **not** read `renames-alphabetical-sort-patterns.canvas.tsx` (one giant RAW line). Do **not** rewrite the canvas file.

## Files

Mod loc (Type column = canvas Category):

`01_Draft/AGF-VP-RenamesAlphabeticalSort-v2.0.5/Config/Localization.csv`

Thin table copy (bucket, category, key, vanilla, mod) — grep this first:

`00_DLL-Projects/Generators/RenamesAlphabeticalSort/catalog.csv`

Which keys belong on which canvas table:

`00_DLL-Projects/Generators/RenamesAlphabeticalSort/buckets.csv`

Vanilla (read/grep, never copy into the repo):

- `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/Localization.csv`
- `.../Data/Config/items.xml`
- `.../Data/Config/blocks.xml`
- `.../Data/Config/item_modifiers.xml`

Canvas (UI only; do not Read):

`C:/Users/rft30/.cursor/projects/c-GitHub-7D2D-Mods/canvases/renames-alphabetical-sort-patterns.canvas.tsx`

## Questions

Grep `catalog.csv` or `Localization.csv`. For vanilla properties:

```
python 00_DLL-Projects/Generators/RenamesAlphabeticalSort/lookup.py KEY
```

Do not dump whole XML files.

## Canvas is the blueprint

The canvas is the plan. The user gives instructions; the **agent** applies them. Do **not** wait for the user to click cells, and do **not** write `Localization.csv` until they explicitly say to implement the plan.

In-game shows `liveName`, not raw loc:

- Category prefix/color: edit `GROUP_STYLE` after `const ROW_H` (group key = Type before `-`). **Admin** is our red `FF5555`. Every other visible `[Word]` prefix is our orange `FFA94D`. Example: `Ammo: { hex: "FFA94D", word: "Ammo" }` → `[FFA94D][Ammo][-] rest`.
- Suffixes are vanilla tan `DECEA3` (`(T1)`, `(Forge)`, `(100)`, `(I)`), except consumable stats: food `00de50`, water `00c8ff`, health `FF5555`, infection `FFA94D`. Seed / Growing / Harvest colors stay as they are until asked.
- Every visible category is `[Word]` then **exactly one space** then the remainder (`[Mod] Serrated Blade`, `[Ammo] .44 Magnum`, `[Clothing] Name (T1)`, `[Drink] Water (20)`). Hidden sort tags sit between `[-]` and that space and must not glue the name to the prefix. Use `wrapCat` after `ROW_H`.
- One item remainder: add to `PLAN` (`key: "remainder"`). If the value starts with `[`, it is the full loc string and skips the group wrap.

## Ammo plan

Visible: `[FFA94D][Ammo][-]` remainder. Hidden family/variant tags `dddd`/`eeee` sit **in front** of the orange wrap so Auto Sort places ammo immediately after weapons (`cccc` → `dddd`). Same family/variant order as before.

- Strip `(Ammo)` from vanilla. Do not leave it on rockets/thrown.
- Like items: noun first, comma, qualifier (`Bolt, Iron` not `Iron Bolt`) unless already in that form. Rockets stay `Rocket Frag` / `Rocket HE` (no comma).
- Caliber bullets: Ball (plain), then HiPower, then AP. Archery (arrow/bolt): Stone, Iron, Steel, Flaming, Exploding. Turret: plain, Shells, AP.
- Thrown (Ammo-Thrown) share family `10` so they stay a block. Arrow/bolt families sit next to each other from the `Arrow,` / `Bolt,` names; hidden `eeee` is still required for Stone→Exploding order.
- Edit the `AMMO` map after `ROW_H` (`rest`, `fam`, `var`). In-game column sorts by the raw planned loc string so hidden tags match Auto Sort.
- `ammoBundle*` boxes are planned as **Ammo** (same family/variant as the matching round, plus the `(100)` / `(75)` / `(1000)` tag). **Interleaved (A):** loose round, then its box, then the next variant. Do not keep Blunderbuss ammo loc (removed in v3). **ammoGasCanBundle** stays on Bundle (`Gas Can (5000)`). Loose **Gas Can** (`ammoGasCan`) and **Paint** (`resourcePaint`) are Tools after Lockpick, not Ammo. Resource stacks and **Raw Meat Bundle** stay on Bundle (`foodRawMeatBundle` is loc Type Item; `planCat` maps it to Bundle).

Armor: `[FFA94D][Armor][-]` then the existing loc remainder (`[bbb…][ffffff] Primitive Hood`, Athletic Hat, Enforcer Sunglasses, etc.). Do not rename pieces. Primitive first, other sets A–Z, slots Hood → Outfit → Gloves → Shoes via those `bbb` tags.

Clothes (Type `Clothes`): `[FFA94D][Clothing][-]` then hidden slot `aaaaaa` Head → Outfit → Hands → Feet and tier `aa0001`–`aa0003`, then vanilla name and tan `(T1)` / `(T2)` / `(T3)`. Visible form is `[Clothing] Name (T#)` (one space after `[Clothing]`). Drop the old `-Head-T1` style text. Order stays Head → Outfit → Hands → Feet, T1 → T2 → T3. `clothesLive` after `ROW_H`.

Item modifiers (loc Type `mod`): keep the existing four hosts. Visible `[FFA94D][Mod][-]`, `[Mod-Armor]`, `[Mod-Drone]`, `[Mod-Vehicle]` (`Mod-Drone` shortens live `Mod-Robotic Drone`). Remainder stays current loc after `stripLead`, minus a trailing ` Mod`. `planCat` maps `modArmor*` → Mod-Armor, `modRoboticDrone*` → Mod-Drone, `modVehicle*` → Mod-Vehicle, else Mod (guns/melee/tools/mixed). Helmet-only armor uses hidden `dddd01` so they list first, then other Mod-Armor (`dddd02`): Agility, Fortitude, Intellect, Perception, Strength, then Cigar, Treasure Hunter's, Water Purifier, ending with Helmet Light beside Night Vision. Mixed `[Mod]` uses Purple Book Tools & Weapons section order via `MIXED_MODS` (`dddd` family): Melee General → Block Damage → Clubs → Special → Ranged General → Scopes → Barrels → Shotguns → Motor Tools last; `eeee` is item order inside each section. Structural Brace sits next to Diamond Blade Tip; Ergonomic Grip next to Fortifying Grip. Keep visible slot labels on Trigger Group, Barrel, Scope, Shotgun Barrel, and Club. Do not use `Blade,` / `Grip,` / `BlockDmg,` / `Motor Tool` remainders. Magazine Extender and Drum Magazine use those vanilla names (not `Magazine, Extender` / `Magazine, Drum`) and stay adjacent in Ranged General. Tube extender remainder is `Shotgun Tube Extender` (still last in Shotguns). Remainders stay current loc after `stripLead`. Do not keep a visible `Special,` remainder prefix (hidden family 04 covers that Purple Book section). Admin mods stay Admin. Do not add dyes. Drop unused `modArmorCoolingMesh` and old `modArmorInsulatedLiner` (and their schematics). Keep T1–T3 insulated liners. `modsLive` / `HELMET_MODS` / `MIXED_MODS` after `ROW_H`.

Book: `[FFA94D][Book][-]` then the existing remainder. Titles unchanged for now.

Reading (one canvas table): **Book**, **Magazine**, and **Schematic**. Visible prefixes stay `[Book]`, `[Magazine]`, `[Schematic]` in `FFA94D`. Hidden leading `[ffcc01]` is the same on all three so Auto Sort keeps them as one block among other orange prefixes; inside the block they go Book → Magazine → Schematic, then title A–Z. Category column still shows Book / Magazine / Schematic. `foodCanShamSchematic` remainder is `Can of Sham` (no Food-Canned-15, no Ingredient).

Schematic names for mods use the **vanilla mod title** (strip trailing `Mod` / `Schematic`), not the special mod remainders (`Barrel Extender` not `Barrel, Extender`; `Motor Tool Large Tank` not `Large Tank`; `Barbed Wire` not `Club, Barbed Wire`). All schematics sort A–Z by that remainder. `schematicsLive` after `ROW_H`.

Parts: `[FFA94D][Parts][-]` then remainder after `stripLead`, minus the word `Parts` if present (`Armor Parts` → `[Parts] Armor`; Armor Crafting Kit unchanged). Same wrap as other categories (brackets, one space). No extra hidden family — remainder A–Z. `stripPartsWord` after `ROW_H`. Do not strip `Parts` from Reward names (4x4 Parts Bundle, etc.).

Reward: `[FFA94D][Reward][-]` then original remainder (do not strip Bundle/Parts). Match vanilla **v3 `items.xml`**: delete leftover loc-only Cloth/Leather/Military/Scrap/Iron/Steel armor bundles and Mining Helmet from this mod loc; skip `questRewardBundleMaster` (`CreativeMode=None`). Include `questRewardVehiclePartsBundle`. Pipe `T0*` duplicates still exist in v3 XML (same names as Pipe*). Hidden `dddd` families: Books/magazines → Food/Farm → Base (Battery/Generator/Solar, Traps, Blade/Dart, turrets, Security Camera) → Vehicles (Vehicle Parts, then 4x4 / Gyro / Mini / Motorcycle) → Ammo Crafting → weapon/tool Crafting A–Z → Pipe → named guns (T1→T2→T3, no Legendary) → Melee/Ranged Mods → **all Legendary last** (Legendary Crafting, then T3 Legendary guns). `REWARD` / `rewardsLive` after `ROW_H`.

Tools and weapons: **no visible `[Tool]` / `[Weapon]` prefix** and **no `(T#)`**. Hidden `bbbb` (tools) / `cccc` (weapons) keep the two blocks apart. Inside each block, hidden family is equipment type, then `eeee` is slot order (tier, then the line). Visible remainder is the vanilla name. Tool families: **Repair Kit, Lockpick, Gas Can, Paint** first (`bbbb00`), then Repair (Taza, Stone Axe, Claw Hammer, Nailgun) → Axe → Shovel → Pick → Salvage → utility (Torch, Flashlight, Wire Tool, Paint Brush). Weapon families: Knuckles → Blade → Club → Baton → Spear → Sledge → Bow → Crossbow → Handgun (Pipe, Pistol, SMG-5, Magnum, Desert Vulture) → Shotgun → Rifle → MG → Rocket → Robotics (melee, then ranged, then explosive, then robotic). Candy cane knife/club sit in Blade/Club at T1. `GEAR` / `gearLive` after `ROW_H`.

Consumable (one canvas table): Candy, Food, Food-Canned, Drink, Drink-Special, Medical, plus planned **Special**. Crop harvested items stay on the **Crop** table. Orange `FFA94D` prefixes: `[Drink]`, `[Food]`, `[Crops]`, `[Canned]`, `[Special]`, `[Candy]`, `[Medical]`. Leading hidden family (hex only) forces that in-game order: `f00001` Drink → `f00002` Food → `f00003` Crops → `f00004` Canned → `f00005` Special → `f00006` Candy → `f00007` Medical. Special = timed combat/skill/XP buff: Grandpa’s first A–Z (Awesome, Fergit’n, Learnin’, Moonshine), then the rest A–Z (Beer, Fort Bites, Recog, Steroids). Candy stays Candy. Mega Crush stays Drink. Painkillers and Vitamins stay Medical. Stats: food-primary `(food)(water)(health)(infection%)(I)`; drinks and Special `(water)(food)` then the same tail. Crops use the food-primary stats **without** `(I)`. Visible stat colors: food `00de50`, water `00c8ff`, health `FF5555`, infection `FFA94D`; `(I)` is tan `DECEA3`. Hidden tags must be 6 hex digits (`0-9a-f`) or they print. Food/canned/candy/crops: `[fd|fc|fa|fb][food][fe][water+1000]`. Drinks: `[ed][water][ee][food+1000]`. Special `a00001`–`a00008`. Medical `b00001`–`b00013`: Honey, Herbal, Antibiotics, Vitamins, Painkillers, Splint, Cast, Aloe, Bandage, First Aid Bandage, First Aid Kit, Blood Bag, **Testosterone Extract** `(I)`. Non-edible crops (aloe leaf, flowers, coffee beans, cotton, hops) have the `[Crops]` prefix and no stats. Empty jar `ed0001` after no-water drinks, before Murky Water. Edit `CONSUME` after `ROW_H`.

Groups not in `GROUP_STYLE` still preview current loc. `mod` in RAW stays loc English so sync does not wipe the plan.

Do **not** keep loc-only unused keys (no matching v3 `items.xml` / `item_modifiers.xml` / `blocks.xml` name). Drop them from `Localization.csv` instead of hiding them. That includes Blunderbuss ammo/boxes, leftover Cloth/Leather/Military/Scrap/Iron/Steel armor reward bundles, Mining Helmet bundle, old Needle & Thread books, `meleeToolSalvageParts`, and `resourceCandyTin` / `resourceAirFilter` (those are mines now). Also drop duplicate loc rows (keep the first). Type `Admin` keys may exist only as Dev items — keep those. `hidden()` is only a canvas filter for anything that still has to stay in loc.

**Resource** table (always last): leftover `resource*` not already on Crop / Consumable / Parts / Bundle. Vanilla extras via `VANILLA_RESOURCES`. Visible orange prefixes: `[Build]` Wood → Cobblestone Rocks → Concrete Mix → Forged Iron → Forged Steel; `[Ore]` Clay Soil → Small Stone → Crushed Sand → Cement → Iron → Lead → Brass → Nitrate Powder → Coal → Oil Shale; `[Gem]` Silver Nugget → Gold Nugget → Raw Diamond; `[Ammo-I]` ammo ingredients (Gunpowder → casings/tips/buckshot/paper/feather/arrowheads/rocket) with hidden `dddd00` so they sit immediately before finished `[Ammo]`; `[Resource]` Plant Fibers → Cloth Fragment → Leather → Bone → Glue → Duct Tape → Sewing Kit → Scrap Polymers → Short Iron Pipe → Nails → Spring → Mechanical Parts → Electrical Parts → Lens → Snowball; draft `[Vehicle]` Wheel → Acid → Oil → Headlight → Engine → Car Battery, then chassis/handlebars/accessories bicycle → minibike → motorcycle → 4x4 → gyro, then the five placeables last (Bicycle → Minibike → Motorcycle → 4x4 Truck → Gyrocopter); skip commented-out Helicopter; draft `[Electric]` Solar Cell → Generator Bank → Battery Bank → Solar Bank → Basic Light Bulb → Fluorescent Light → Spotlight → Switch → Electric Wire Relay → Electric Timer Relay → Motion Sensor → Trigger Plate 1x1 / 1x5 → Tripwire Post → Speaker → Electric Fence Post → Dart Trap → Blade Trap → SMG Auto Turret → Shotgun Auto Turret → M60 Auto Turret (skip Dev collapsed banks, POI sensors, decorative lanterns, wood/iron/glass spikes); `[Smelt]` forge scrap (candlestick, door knob, radiator, trophies, broken glass, fishing weight). Other leftovers keep vanilla names (`dddd09`). **Repair Kit**, **Lockpick**, **Gas Can**, and **Paint** are Tools first. **Beeswax** is Station Apiary first; **Chicken Feed** is Station Chicken Coop first. **Testosterone Extract** is Medical after Blood Bag `(I)`. Brass *stack* stays Bundle; loose brass is Ore. Hide `resourceWaterFilter` and unused `resourceQueenBee` if they are still in loc; do not re-add them. `RESOURCE_PLAN` / `resourceLive` after `ROW_H`.

Never bump `sort_v1`, `sort_finished_v1`, or `sort_by_group_v1`. Tell the user to **reopen the canvas**.

## Canvas tables

One table per **category group**. Hyphenated Types share the prefix table (`Ammo` + `Ammo-Thrown` → Ammo; `Station-C` / `Station-D` / `Station-F` / `Station-A` → Station; `Mod` + `Mod-Armor` + `Mod-Drone` + `Mod-Vehicle` → Mod). **Book** + **Magazine** + **Schematic** → Reading. **Candy** + **Food** / **Food-Canned** + **Drink** / **Drink-Special** + **Medical** + planned **Special** → Consumable. `resource*` that already have a table stay there; leftover Ammo-Ingredient `resource*` → **Resource** (last table). Category column still shows the full Type. Each table shows **20 rows** (or all rows if the group is smaller); scroll for the rest.

Workstation insert tools live on **Station**: Campfire (`Station-C`), Forge (`Station-F`), Dew Collector (`Station-D`), Apiary (`Station-A`), Chicken Coop (`Station-H`, planned; not in this mod loc yet). Visible `[FFA94D][Station][-]` then original tool name, then tan `(Host)` at the end (`[Station] Advanced Bellows (Forge)`). Hidden `dddd` families follow slot order from XUi `required_tools` / block `RequiredMods`: Campfire (Pot, Grill, Beaker) → Forge (Bellows, Anvil, Crucible) → Dew (Gatherer, Tarp, Water Filter) → Apiary (**Beeswax** first, then Honey Extractor, Brood Box, Smoker) → Chicken Coop (**Chicken Feed** first, then Brooding Lamp, Chicken Run, Nesting Box). Empty loc Type on apiary tools is planned as `Station-A` (`planCat`); chicken keys `toolChickenCoop*` → `Station-H`. Do not keep an Apiary table, and do not write the word Apiary into loc Type unless asked. `STATION` / `stationLive` after `ROW_H`.

Loc Type `Block` is planned as **Flora** (planted crops, trees, seed/growing/harvest). Do not keep a Block table for those rows. Harvested crop *items* (`Crop`) stay on Crop unless asked to merge. Growing and harvest blocks do not appear in inventory; leave those names as they are (tan Growing, green Harvest, tree fractions). **Seeds** (inventory) use orange `[Seed]`: tree seeds first with no `(1/5)` (Blue Spruce, Oak, Pine), then crop seeds A–Z. Hidden `dddd01` trees / `dddd02` crops after the prefix. Crops use **3 stages without fractions** on the Flora table for growing/harvest. Trees keep **5 stages** like Trees Plus for growing/harvest only. Hidden `a00002`–`a00005` remain on tree growing/harvest. Renames `Config/blocks.xml` sets `DisplayInfo=Name` on `treeMasterGrowing` and the three mature planted trees so the loc name shows when looking at a growing/mature tree (not the seed block). English loc is implemented for tree stages and for **harvest** keys: farm `*HarvestDesc`, wild `planted*3Harvest`, POI `mushroom01`/`02`/`mushroom01Desc`, biome harvest `mushroomBiome2`/`4`, radiated harvest `mushroomRadiated02`/`04`. Leave snowberry and cacti alone. Crop seed/growing loc is still plan-only. Skip Trees Plus x5/x25 blocks. `floraLive` after `ROW_H`.

RAW is **every** loc key (first row wins if duplicated). Shape: `[bucket, category, key, vanilla, mod]`. Visible columns: Category, In-game, Code, Vanilla English — no Mod English column (`mod` still feeds loc remainder).

## Admin names

Visible label is `[Admin]` (language-appropriate word) in `FF5555`, **including the brackets**, then a space, then the rest of the name. Same pattern as screamer alerts:

`[FF5555][Admin][-] Dev: Assassin Quality Armor Bundle`

Not `[FF5555]Admin[-]` (no brackets) and not `[[FF5555]Admin[-]]` (brackets outside the color).

If the leftover name is a placeholder (`n/a` or `0`), use vanilla for that language, then vanilla English, then the loc **Key** (code). Folding stock has no vanilla row, so it becomes `[Admin] modGunFoldingStock`.

Admin word by language: english Admin · german Administrator · spanish Admin · french Admin · italian Amministratore · japanese 管理 · koreana 관리 · polish Administrator · brazilian Administrador · russian Администратор · turkish Yönetici · schinese 管理员 · tchinese 管理員

Do not include Dev POI blocks.

## Other languages

Copy **hidden tags** from English (identical in every language). Translate only the visible `[Word]`. Remainder comes from that language’s vanilla / previous loc (`stripLead`), **not** a machine translation of the English canvas remainder, and **not** English noun-first (`Bolt, Iron`). Keep `(I)` as `(I)`. Station `(Host)` uses vanilla workstation names (Campfire, Forge, Dew Collector, Apiary, Chicken Coop).

Visible `[Word]` glossary (same order as Admin): german · spanish · french · italian · japanese · koreana · polish · brazilian · russian · turkish · schinese · tchinese. Source of truth: `PREFIX` in `apply_other_langs.py`.

- Ammo: Munition · Municiones · Munitions · Munizioni · 弾薬 · 탄약 · Amunicja · Munição · Боеприпасы · Mühimmat · 弹药 · 彈藥
- Ammo-I: Munition-Zutat · Municiones-Ingrediente · Munitions-Composant · Munizioni-Ingrediente · 弾薬-材料 · 탄약-요소 · Amunicja-Składnik · Munição-Ingrediente · Боеприпасы-Ингредиент · Mühimmat-Bileşen · 弹药-原料 · 彈藥-配料
- Armor: Rüstung · Armadura · Armure · Armatura · アーマー · 방어구 · Pancerz · Armadura · Броня · Zırh · 护甲 · 護甲
- Clothing: Kleidung · Ropa · Vêtements · Vestiti · 衣服 · 의류 · Ubranie · Roupas · Одежда · Giyim · 衣物 · 服裝
- Book: Buch · Libro · Livre · Libro · 本 · 책 · Książka · Livro · Книга · Kitap · 书籍 · 書籍
- Magazine: Magazin · Revista · Magazine · Rivista · マガジン · 잡지 · Magazyn · Revista · Журнал · Dergi · 杂志 · 雜誌
- Schematic: Bauplan · Esquema · Schéma · Schema · 設計図 · 도면 · Schemat · Diagrama · Схема · Şema · 设计图 · 原理圖
- Candy: Süßigkeit · Dulce · Bonbon · Caramella · キャンディ · 사탕 · Słodycze · Doce · Сладость · Şeker · 糖果 · 糖果
- Food: Nahrung · Comida · Nourriture · Cibo · 食料 · 음식 · Żywność · Comida · Еда · Gıda · 食物 · 食物
- Crops: Ernte · Cultivos · Cultures · Colture · 作物 · 작물 · Plony · Cultivos · Урожай · Mahsul · 作物 · 作物
- Drink: Getränk · Bebida · Boisson · Bevanda · 飲み物 · 음료 · Napój · Bebida · Напиток · İçecek · 饮料 · 飲料
- Canned: Konserve · Enlatado · Conserve · Scatoletta · 缶詰 · 통조림 · Konserwa · Enlatado · Консервы · Konserve · 罐头 · 罐頭
- Special: Spezial · Especial · Spécial · Speciale · スペシャル · 스페셜 · Specjalne · Especial · Особое · Özel · 特殊 · 特殊
- Medical: Medizin · Médico · Médical · Medicina · 医療 · 의료 · Medyczne · Médico · Медицина · Medikal · 医疗 · 醫療
- Parts: Teile · Partes · Pièces · Parti · パーツ · 부품 · Części · Peças · Части · Parçaları · 部件 · 零件
- Reward: Belohnung · Recompensa · Récompense · Ricompensa · 報酬 · 보상 · Nagroda · Recompensa · Награда · Ödül · 报酬 · 獎勵
- Station: Station · Estación · Station · Stazione · 装置 · 작업대 · Stacja · Estação · Станция · İstasyon · 设施 · 設施
- Mod: Mod · Mod · Modification · Mod · MOD · 모드 · Modyfikacja · Mod · Модификация · Mod · 模组 · 模組
- Mod-Armor: Mod-Rüstung · Mod-Armadura · Mod-Armure · Mod-Armatura · MOD-アーマー · 모드-방어구 · Modyfikacja-Pancerz · Mod-Armadura · Мод-Броня · Mod-Zırh · 模组-护甲 · 模組-護甲
- Mod-Drone: Mod-Drohne · Mod-Dron · Mod-Drone · Mod-Drone · MOD-ドローン · 모드-드론 · Modyfikacja-Dron · Mod-Drone · Мод-Дрон · Mod-Drone · 模组-无人机 · 模組-無人機
- Mod-Vehicle: Mod-Fahrzeug · Mod-Vehículo · Mod-Véhicule · Mod-Veicolo · MOD-車両 · 모드-차량 · Modyfikacja-Pojazd · Mod-Veículo · Мод-Транспорт · Mod-Araç · 模组-载具 · 模組-車輛
- Seed: Samen · Semilla · Graine · Seme · 種子 · 씨앗 · Nasiono · Semente · Семя · Tohum · 种子 · 種子
- Build: Bau · Construcción · Construction · Costruzione · 建築 · 건축 · Budowa · Construção · Строй · Yapı · 建造 · 建造
- Ore: Erz · Mineral · Minerai · Minerale · 鉱石 · 광석 · Ruda · Minério · Руда · Cevher · 矿石 · 礦石
- Gem: Edelstein · Gema · Gemme · Gemma · 宝石 · 보석 · Klejnot · Gema · Самоцвет · Mücevher · 宝石 · 寶石
- Resource: Ressource · Recurso · Ressource · Risorsa · 資源 · 자원 · Zasób · Recurso · Ресурс · Kaynak · 资源 · 資源
- Vehicle: Fahrzeug · Vehículo · Véhicule · Veicolo · 車両 · 차량 · Pojazd · Veículo · Транспорт · Araç · 载具 · 載具
- Electric: Elektro · Eléctrico · Électrique · Elettrico · 電気 · 전기 · Elektryka · Elétrico · Электрика · Elektrik · 电力 · 電力
- Smelt: Schmelz · Fundición · Fonte · Fusione · 精錬 · 제련 · Wytop · Fundição · Плавка · Eritme · 熔炼 · 熔煉

Growing / Harvest labels: Anbau/Sammeln · Creciendo/Cosechar · Pousse/Récolter · In crescita/Raccogli · 生育中/採取 · 성장 중/수확 · Rosnący/Pozyskaj · Cultivo/Colher · Растет/Добыть · Yetişen/Topla · 生长中/收集 · 生長中/收穫

Skip Type `Admin` (already multilingual). Clear leftover Context `n/a` / `[ffb400][-] n/a` stubs when rewriting langs. Do not keep or re-wrap loc-only unused keys.

When asked to patch other languages after an English loc change:

```
python 00_DLL-Projects/Generators/RenamesAlphabeticalSort/apply_other_langs.py
```

## Table data changes (implementation only)

Do **not** use this during planning. When the user asks to implement the plan, edit `Localization.csv` (`Type` and/or `english`; other languages only if asked) and `buckets.csv` when moving keys between tables. English names come from canvas `liveName` via `python apply_plan.py`, then:

```
python 00_DLL-Projects/Generators/RenamesAlphabeticalSort/sync_canvas.py
```

If they also asked for other languages, run `apply_other_langs.py` after `apply_plan.py`. That script copies English hidden tags, swaps `[Word]` from `PREFIX`, and keeps each language’s own remainder.

That refreshes `catalog.csv` and **only** the `const RAW` line in the canvas.

## Canvas UI changes (columns, sort, color legend, in-game preview)

StrReplace **after** `const ROW_H`. Never touch the `const RAW` line. Never replace the whole file.

Top of canvas is compact `HueCompare` `Table` (`width: max-content`) after `ROW_H`: Name, pure square, pure hex, our square, our hex. Rows: Red `FF0000`/`FF5555`, Blue `0000FF` / water `00c8ff`, Green `00FF00` / food `00de50`, Orange `FF8000`/`FFA94D`, then `DECEA3` with no pure pair. `B8A3D8` is not a blue. Do not restore the old chat/loc chip legends unless asked.

`locParts`: `[[` prints a `[` and leaves the second `[` for a following tag. Unknown `[...]` (including `[Admin]`) is literal text, so `[FF5555][Admin][-]` previews as red `[Admin]`.

## Do not

- Edit ModInfo / README
- Translate non-English loc unless asked (other langs are implemented; still wait for the ask before re-running)
- Read or regenerate the deco-helper canvas
- One-off Python that rewrites the whole canvas
- Write loc while the canvas is still the plan unless the user asks to implement
