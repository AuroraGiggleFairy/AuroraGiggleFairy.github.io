# Decoration Block Generator Instructions

Use this file as the only plan for the Decoration Block generator.

Technical names stay exact.

## Terms

| Term | Meaning |
| --- | --- |
| Flatten | Copy parent block properties into each child. The child then does not need Extends. |
| Extends | A game property that copies properties from a parent block. |
| Exclude list | The Extends `param1` list. Do not copy these parent properties. |
| Deco rewrite | Turn flattened vanilla blocks into `agfDeco*` blocks. |
| Clone | A new block that uses a vanilla model and Deco rules. |
| Variant helper | One block that lets the player pick a model from a long list. |
| Twin | An extra clone of the same model with a different function. |

## Status

| Item | Status |
| --- | --- |
| Work folder | Exists |
| Flatten | Works |
| Deco rewrite script | Full pass is on. Use `--all`. |
| Doors | Not in this mod. DoorsPlus owns doors. |

## Folders

| Use | Path |
| --- | --- |
| Generator scripts | `00_DLL-Projects/Generators/DecorationBlock` |
| Draft Deco mod work | `01_Draft/AGF-VP-DecorationBlock-v3.0.3` |
| Live game test copy | `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-VP-DecorationBlock-v3.0.3` |
| Excel baseline (do not edit) | `00_Support/Archive/AGF-VP-DecorationBlock-v3.0.3-ExcelBaseline-20260822` |
| Older game-version copy | `00_Support/Archive/old-game-versions/_x2.6/AGF-VP-DecorationBlock-v3.0.3` |
| DoorsPlus only. Do not put Deco files there. | `00_DLL-Projects/Generators/DoorsPlus` |
| Game blocks file | `C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config\blocks.xml` |

Edit Draft and the live game copy together. Do not edit the Excel baseline.

## Files in this folder

| File | Role |
| --- | --- |
| `flatten_blocks.py` | Flatten script. This script works. |
| `generate_decoblocks.py` | Deco rewrite. Writes blocks, cooking UI, and cooking XUi to Draft and the live game copy. |
| `blocks.xml` | Local copy of vanilla blocks. |
| `blocks_flattened.xml` | Flatten output. |
| `INSTRUCTIONS.md` | This file. |

`blocks.xml` file order: masters, then clones, then the variant helper.

## What the player gets

1. The player crafts one block: `agfDecorationsVariantHelper`.
2. That helper is the variant helper.
3. Each picked model is an `agfDeco*` clone.
4. Break a clone. The helper comes back.

## Flatten first

1. Copy the game `blocks.xml` into this folder.
2. Run `flatten_blocks.py`.
3. Use `blocks_flattened.xml` as the input for the Deco rewrite.

Flatten already does this:

- Resolve Extends.
- Honor Extends `param1` as an exclude list.
- Do not copy `CreativeMode` from the parent.
- Remove Extends, drops, `RepairItems`, and `UpgradeBlock` from the written file.

## Skip these blocks

Skip a block if any item below is true.

### Flatten already skips

- The name has `master`, `LootHelper`, or `VariantHelper`. Match is case-insensitive.
- The name ends with `Shapes`.
- The block has a property class `TrapDoor`.
- The name starts with `trader` and the name without `trader` also exists.
- The name ends with `TraderOnly` and the name without `TraderOnly` also exists.
- The name ends with `POI` and the name without `POI` also exists.
- The name has `player` and the name without that token also exists. Match is case-insensitive.

### Deco rewrite must also skip

This list is in `generate_decoblocks.py`.

- Terrain, air, and water.
- Test, debug, hidden, and imposter blocks.
- Sleepers, hazards, and game-event blocks.
- Infested and cursed blocks.
- Filler and texture blocks. Names with `filler` or `texture`. Keep `escalatorFiller`.
- Master and helper blocks. Match is case-insensitive.
- Twitch copies. Names with `twitch`.
- `noLoot` display-case extras.
- Trap windows and trap spikes. Names that start with `trap`.
- `spawnTrader` and other `SpawnEntity` blocks.
- `flagWallHungSign`. The painted flags stay.
- The noose (`modularRopeNoose`).
- Blank canvas frames (`signCanvas*`, `canvasWood*`) and invisible canvas signs (`canvasInvisible*`). They have no Sign Editor on the Deco clone, so they only block movement.
- Sign decal size plates (`signDecalWood*`, `signDecalCorrugatedMetal*`). Same canvas class, no Sign Editor on the Deco clone. Keep posters, gas signs, bulletin boards, and other painted signs.
- Vanilla writable storage crates (`cntWoodWritableCrate`, `cntIronWritableCrate`, `cntSteelWritableCrate`). Deco loot already uses those crate lists.
- Fire-hazard pipes (`pipeFireHazard*`). Same look as the broken pipes.
- Trader Rekt feed pipes (`pipesFeedRekt`).
- `cntShippingCrateConstructionSupplies`. Same mesh as the plain shipping crate, with a helper icon tint. Keep `cntShippingCrateHero` as `Shipping Crate`.
- Climbable modular ropes keep `Class` = `Ladder`.
- Jail bars are Upgradeable.
- `mushroomBiome*` hidden sprouts. Keep planted crop stages (`plantedCorn1` / `2` / `3Harvest` and the other crops) as deco. Crops may use `Place` = `Door` for rotation; that is not a real door.
- Test-only blocks (`CreativeMode` = `Test`), including `foodClutterPile*`.
- Distant gimbal trees still need `Shape` = `ModelEntity` on the clone. Do not copy `BigDecorationRadius` or `IsDecoration`. Keep `treePlanted*` growth stages and world trees as decorative trees.
- Sign, pipe, and wreckage filler when the player does not need them.
- Vanilla lock twins and player dups. Skip if the name has `Insecure` or a `Player` token. Do not require the token at the end of the name.
- Doors, hatches, and gates. DoorsPlus owns those. Skip `BlockTag` = `Door`, `Class` = `DoorSecure`, `Class` = `PoweredDoor`, and `CompositeTileEntity` door features. Match `door`, `hatch`, and `gate` as words. Do not match `gate` inside `corrugated`. Do not skip crop blocks only because `Place` = `Door`.
- Ladders, catwalks, and stair shapes. Keep escalators and other models that are not in the shape menu.
- World trees with `Class` = `ModelTree` or `Shape` = `DistantDecoTree` stay, as decorative clones: force `ModelEntity` and drop `BigDecorationRadius`. Keep potted plants and other `ModelEntity` plants.
- Shape-menu cubes and plates, except glass, I-beams, and `concretePlateRound*`.
- Twitch blocks when they use the same model, icon, and color as a normal block.
- Same look twice. One clone per mesh + tint. Treat `*LightPrefab` as the same mesh as `*Prefab`. Keep Open and Closed when the model differs. Keep color tints when the tint differs. Prefer the non-POI, non-player name. Police car: keep the normal car and the alarm car. Drop lock-pick and unlocked twins of the same mesh.

Keep `Empty`, `Full`, `Open`, and `Closed` when they use a different model. Treat each as Player Loot, including an empty look that has no vanilla loot Class.

If you are not sure, skip the block.

## Make each kept block into Deco form

- New name: `agfDeco` + the vanilla name.
- Each clone extends `agfDecoMaster`.

Set these on every clone:

| Property | Value |
| --- | --- |
| `CreativeMode` | `None` |
| `CustomIcon` | The vanilla block name |

Keep only look and place properties: `Model`, `ModelOffset`, `Texture`, `Shape`, `Place`, `Path`, tint, size, collide, sounds.

Do not copy `CustomIconTint` or `TintColor` onto a Powered clone, except flashlight and lantern housing colors.

Then apply one kind rule.

### Powered lights

Keep one clone per fixture. A fixture is the same `Model`, size, `Place`, `HandleFace`, and `ModelOffset`.

Vanilla housing colors only change `CustomIconTint` and `TintColor`. The light value stays `0.5`. Skip those twins. Prefer the `White` name.

Flashlights (`flashlight*`) and lanterns (`lanternDecor*`) keep every housing color. Copy `TintColor` and `CustomIconTint` onto those clones. `IgnoreLightsOff` means the tint is the object color, not a bulb color.

Treat a `*RedPrefab` as the same fixture as the matching `*Prefab`. Example: keep `lightIndustrial`, skip `lightIndustrialRed`.

Skip `_player` and helper twins when the same fixture already has a keep name.

All Powered clones use `Class` = `PoweredLight`. The emitted light color is the same. Flashlight and lantern housing tints stay.

## Kinds of blocks

| Kind | What it is | Old pack did this | Open choice |
| --- | --- | --- | --- |
| Deco Copy | Visual clone only. Same model. No extra function. | Yes. Most blocks. | Keep. |
| Campfire Like | Same model. Works as a campfire. | Yes. A `Cooking` twin with `Class` = `Campfire`. | Keep. Each Cooking twin needs a `workstation_*` window group and a matching `ui_display` list. Helper icon tint is always `FF0000`; do not copy vanilla `CustomIconTint`. |
| Powered | Same model. Needs power. Lights and other electric blocks. | Yes for lights (`Class` = `PoweredLight`). Some non-lights were tagged by mistake. Old pack cloned every housing color. | Keep. Only real lights (`Class` = `Light` or `PoweredLight`). One clone per fixture, except flashlight and lantern colors. |
| Player Loot | Same deco model. Player storage with the quality of `cntWoodWritableCrate`. | Old pack used one `agfStorage` box. | Locked. See loot tiers below. `Empty` / `Full` / `Open` / `Closed` are each their own loot family when the model differs. Empty looks are still loot. |
| Insecure Loot | Hidden unlocked twin of that same deco model and tier. | Yes, one insecure twin only. | Locked. Each Wood, Iron, and Steel loot block has its own insecure twin. Not on the helper list. |
| Upgradeable | Same model. Structure piece the player can upgrade. | Old pack only upgraded doors and shutters to Strong. | Locked. See structure pieces below. |

### Player Loot tiers

Match `cntWoodWritableCrate` quality. Keep the deco model. Do not swap in crate prefabs.

Use `Class` = `CompositeTileEntity` with storage, lock, and writable sign features. CompositeTileEntity means: the v3.1 class that holds storage, lock, and sign features.

| Tier | Secure name shape | Insecure twin | Storage list | Size | Upgrade to |
| --- | --- | --- | --- | --- | --- |
| Wood | `agfDeco` + vanilla name | `...Insecure` | `playerWoodWritableStorage` | 8x6 | Iron, with `resourceForgedIron` x10, 4 hits |
| Iron | `...Iron` | `...IronInsecure` | `playerIronWritableStorage` | 8x8 | Steel, with `resourceForgedSteel` x10, 4 hits |
| Steel | `...Steel` | `...SteelInsecure` | `playerSteelWritableStorage` | 8x10 | None |

If `AGF-VP-LargerStorageOption` is loaded, a `blocks.xml` conditional changes those lists to `playerWoodWritableStorageLarge`, `playerIronWritableStorageLarge`, and `playerSteelWritableStorageLarge`. Do not put those sizes in Deco `loot.xml`. Larger Storage Option already sets the Large list sizes, including its BackpackPlus check.

Secure blocks lock. Insecure blocks do not lock.

Damage and repair follow the vanilla crates: Wood 500 and wood repair, Iron 2500 and forged iron, Steel 5000 and forged steel.

The helper list shows the Wood secure block only. The player upgrades Wood → Iron → Steel in place.

### Upgradeable structure pieces

These keep the deco model. They are not doors.

Examples:

- Chain link fence pieces. Skip chain link doors and gates. DoorsPlus owns those.
- Shipping container pieces: sides, caps, frames, and like parts.
- Bandit walls.
- Tarp fences. Not plain hanging tarps (`tarpHanging*`).
- Sandbags, guardrails, fences, bollards, parking blocks.
- Plastic barriers, concrete barriers, and I-beams.
- Glass blocks (`glassBusiness*`, `glassBulletproof*`, `glassIndustrial*`). Not glass debris. Bulletproof glass sits with other structure pieces in the helper, not with broken glass deco.
- Helipad. Upgradeable. Force `StabilitySupport` true, `Path` solid, and collide so it can hold other blocks.
- Round concrete plates (`concretePlateRound*`). Use `CustomIcon` = `concreteNoUpgradeMaster`.
- Other fence, wall, and structure pieces of the same kind.

Do not treat furniture, loot, lights, or campfires as Upgradeable.

The placed block starts as Deco quality (`DecoMaterial`). The next upgrade is Iron. Then Steel. There is no Wood tier. Keep the same model.

The helper list shows the Deco starting block only.

| Tier | Name shape | Material | HP |
| --- | --- | --- | --- |
| Deco | `agfDeco` + vanilla name | `DecoMaterial` | 500 |
| Iron | `...Iron` | `Mmetal` | 2500 |
| Steel | `...Steel` | `Msteel` | 5000 |

Iron and Steel match `cntIronWritableCrate` and `cntSteelWritableCrate`.

## Other files the mod needs

The rewrite must also make or update these files.

| File | Content |
| --- | --- |
| `Config/blocks.xml` | Master, clones, twins, helper. |
| `Config/materials.xml` | `DecoMaterial` and `DecoMaterialSteel`. |
| `Config/recipes.xml` | One recipe for the helper. |
| `Config/loot.xml` | No Deco loot containers. List names and sizes come from vanilla or Larger Storage Option. |
| `Config/Localization.csv` | One name for each block. |
| `Config/ui_display.xml` | One campfire category list per Cooking twin. |
| `Config/XUi_InGame/xui.xml` | One `workstation_*` window group per Cooking twin. Append to `/xui`. v3.1 has no `/xui/ruleset`. |
| `Config/XUi_InGame/windows.xml` | Larger loot grid. |

Do not edit `ModInfo.xml` or README files unless the user asks.

## Localization

`generate_decoblocks.py` writes `Config/Localization.csv` to Draft and the live game copy.

CSV format: header unquoted. Key and metadata unquoted. Every language cell always quoted. `Context / Alternate Text` stays empty.

English name shape:

name + `, pose/part` if it has one + `, position` if it has one + `, status` if it has one + `, color` if it has one + `, 1m` / `, 2x2` if it has a measure + `, A` if it has a letter variant + `, Iron` or `, Steel` if it is a later tier + category tag.

Status words: `Open`, `Closed`, `Empty`, `Full`, `Working`, `Stopped`, `Broken`. Use `Open, Broken` when both apply. On `cntDeskMetal*`, `cntDeskWood*`, `cntArmoireDrawer*`, `cntMorticianDrawer*`, `cntwallOven*`, and `cntDishwasher*`, vanilla `Empty` is Open and vanilla `Full` is Closed. Coolers without `Open` in the name are Closed. `cntFridgeStainlessSteel` is Closed; `cntFridgeStainlessSteelVer2Open` is Open. `wallClock` is `Wall Clock, Stopped`. `wallClockBroken` is `Wall Clock, Broken`. `wallClockWorking` is `Wall Clock, Working`. `cntSuitcase` is `Suitcase, Closed`. `cntLuggageMediumOpen` and `cntLuggageMediumClosed` are the same suitcase, so `Suitcase, Open` and `Suitcase, Closed`. Big luggage and piles stay Luggage. `cntLootCrateShamway` is `Cardboard Box, 1`. `cntCardboardBox` is `Cardboard Box, 2`. `cntGarageStorage` is `Cardboard Box, 3`. `cntShippingCrateHero` is `Shipping Crate`. Tinted buried hero chests are `Buried Supplies, Blue` / `Red` / `Green` / `Brown`. Untinted `cntLootChestHero` stays `Reinforced Chest`. Closed `cntGunSafe` is `Gun Safe, Closed, Black`. Open is `Gun Safe, Open, Black`. Uncolored lockers are Green. Locker parts: `Short`; `Tall` (three tall doors); `Tall, 6 Small`; `Tall, Mixed 1`; `Tall, Mixed 2`.

| Tag | Color | Use |
| --- | --- | --- |
| `[c8e6be](Loot)[-]` | Soft green | Player Loot |
| `[f0beb9](Campfire)[-]` | Soft red | Cooking twins |
| `[f0cdaa](Structure)[-]` | Soft orange | Upgradeable structure |
| `[e6dca5](Electric)[-]` | Soft gold | Powered lights |
| `[ddcdfa](Deco)[-]` | Purple | Plain deco only |

If vanilla omits a color, position, or status that belongs on the block, add the simple word. Uncolored gun safes are Black. Uncolored lockers are Green. Position words: `Corner`, `Side`, `Offset`, `Left`, `Right`. Under-counter mini beverage coolers and wall ovens are Offset. Flashlight and lantern clones keep color: `Flashlight, White`, `Flashlight, Corner, Red`, `Lantern, Red`. Other languages keep vanilla word order and still get the comma material and the tag.

Fill all 13 game languages.

Keep an old Excel name when that key still exists and the text is valid. Rebuild Japanese, Korean, Russian, and Chinese from vanilla. Drop keys for blocks the generator no longer emits.

Add the keys `agfDecoBlockDesc` and `agfDecorationsDesc`.

## Rules

- Keep Deco work in `Generators/DecorationBlock`.
- Keep door work in `Generators/DoorsPlus`. Do not make Deco door, hatch, or gate clones.
- DoorsPlus may read `flatten_blocks.py` from this folder. That is the only shared file.
- Do not use Excel as the live source.
- Do not write a Deco block that still extends a vanilla parent for stats.
- Always set `CustomIcon` and `CreativeMode` on new blocks.
- All loot is Player Loot with `cntWoodWritableCrate` quality. Wood upgrades to Iron, then Steel. Each tier has an Insecure twin.
- Upgradeable is for structure pieces only: fences, walls, shipping container parts, bandit walls, tarp fences, sandbags, guardrails, bollards, parking blocks, barriers, I-beams, glass, helipad, and like pieces. Start as Deco quality (`DecoMaterial`, 500). Next is Iron (`Mmetal`, 2500), then Steel (`Msteel`, 5000). Skip doors and gates.
- Intact workstations are Player Loot: `campfire`, `forge`, `workbench`, `cementMixer`, `chemistryStation`, `tableSaw`. Broken looks stay loot through the `cnt` rule.
- Do not make a Cooking twin for `campfire` or `cntCollapsedCampfire`. Those already are campfires. Cooking twins stay on stoves, ovens, grills, coffee makers, and cooking pots.
- `pictureCanvas_01a` through `j` are unique paintings. Keep them.

## Later: category picker in EnhancedAGF.dll

Do not build this now.

Later, add a category picker for Deco models in `EnhancedAGF.dll`. The player will pick a category, then pick a model. This is the same idea as the shape menu categories.

The DLL project is:

`00_DLL-Projects/Projects/DLL_NoEAC-EnhancedAGF`

Keep helper order in named groups so that picker can use them. Examples: kitchen, furniture, lights, plants, vehicles.

Until the DLL exists, the helper still uses one `PlaceAltBlockValue` list.

Helper and `SortOrder1` follow `legacy_helper_order.txt`, then group like with like.

- Cooking twins sit first in `PlaceAltBlockValue`, with their own early `SortOrder1`. Their loot counterparts stay later in the main list, not next to the campfire twin. Every Cooking twin uses `CustomIconTint` `FF0000`, including color-housing ovens and ranges.
- Skip vanilla writable storage crates (`cntWoodWritableCrate`, `cntIronWritableCrate`, `cntSteelWritableCrate`). Deco loot already uses those lists.
- Object families stay together. Color variants sit with the uncolored parent even if the Excel list dumped them at the end (`bed02ArmyGreen` with `bed02`, `churchPewCenterRed` with the other pews).
- Shared families: pipes (not fire-hazard or Rekt feed pipes); movie/wanted/snack posters with cat/dog posters; remaining `sign*` signs; couches including leather and old couch chairs; chairs, stools, and wheelchairs; bedding including frameless beds; hanging and potted ferns; coolers; lockers; workstations including apiary, chicken coop, and combo bench; wine bottles with soda cans; TVs split stand vs wall-mounted; laptops, speakers, and headphones; cardboard boxes, then the plain `Shipping Crate`, then branded shipping crates; garage tools, buckets, and spray cans; electrical boxes with turrets and traps; hanging logs including empty logs; escalators; elevator panels; flashlights by color; clean drapes then dirty drapes; planted crops; decorative trees; structure pieces including I-beams and helipad; structure glass together (business, industrial, then bulletproof); broken and debris glass; paintings and canvas pictures; mushrooms.
- After `--all`, write `helper_categories.csv` in this generator folder. Columns: block name, SortOrder1, SortOrder2, whether it is on the helper, kind, helper family, suggested category, English name. Use that file to recategorize and tweak order. Do not ship it in the game Mods copy.
- Size words sort Extra Small → Small → Medium → Large → Extra Large. Leave those words in the display name; do not move them to the end.

## File order in `Config/blocks.xml`

1. Masters.
2. Clones by kind.
3. `agfDecorationsVariantHelper` last.

## Next action

1. Review names in-game after the generated `Localization.csv` pass.
2. Trim skip or kind rules if the helper list is too large or missing a family.
