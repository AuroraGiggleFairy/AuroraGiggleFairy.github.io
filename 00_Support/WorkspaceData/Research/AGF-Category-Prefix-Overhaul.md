# AGF naming — suggestion

Applied 2026-10-08 to ActiveBuild and the scoped Draft mods. TerrainLeveler is still TBD and its folder was not renamed. The other Draft folders were not renamed. Giggle Pack = footer only.

## Locked (2026-10-07)

Public title:

```
AGF - V# - SCOPE - ModName
```

`V#` is the game line (`V3` now, `V4` when that game ships). `SCOPE` is **SERVER**, **CLIENT**, **BOTH**, **ADMIN**, or **COMPAT**. Example: `AGF - V3 - SERVER - FuelBurnPlus`.

The title does not carry every detail. The readme **Mod Scope** section does.

- **AGF** stays first.
- **V3** stays in the title. People lose track of which game the mod is for when it is missing.
- The next 7 Days to Die major is **V4**. V3 listings stay V3. New work for that game uses V4.
- **V3** means the whole 3.x line, not one patch. Each later 3.x game update needs version gates so the latest mod still loads, and an existing save can take that update safely.
- EAC rules, and any extra subcategory, stay in Mod Scope. They are not title words.

| SCOPE | Who it is for | Current mod types |
|---|---|---|
| SERVER | On the server. Joining players get it without installing it. | 1 EAC-Friendly, 2 EAC Off, 5 EAC Varies |
| CLIENT | At least on each player's own game. | 4 Client-Side Only |
| BOTH | On the host and on every joining player. | 3 Server/Client Required |
| ADMIN | Fixes, tools, and modder stuff. For server admins and for people creating overhauls. The title does not also say SERVER, CLIENT, or BOTH. That person is expected to know the install side. A casual player should read ADMIN as not for them to mess with. | 4Modders, ESC Window, ModSync, and the same kind of mod |
| COMPAT | One patch for one other mod. There is no single compatibility pack. Five `z`s put it after the other late folders. | The other mod's name |

The folder keeps the game `V#` and ends with the mod version `-vX.Y.Z`. Example: `AGF-V3-SERVER-FuelBurnPlus-v3.1.1`.

Some folders need a load-order prefix in front of AGF. `0` loads earlier. Some number of `z` loads later. The display name starts with `AGF` and does not use that prefix.

A fix for anyone uses SERVER, CLIENT, or BOTH. A fix that overhauls may want, and that a casual player would not be looking for, uses ADMIN.

COMPAT folders use five `z`s: `zzzzzAGF-V3-COMPAT-WMM12SlotToolbelt-v1.0.0`. Every other late mod uses three `z`s.

## Before the full rename

The folder preview below stays a suggestion for every mod that is not a COMPAT. The COMPAT split is in ActiveBuild. ReleaseSource and the game Mods folder were not changed. The full scope rename of every other mod waits.

Compatibility is its own pass first.

1. One COMPAT mod per non-AGF mod. `DoomMod_Standalone` and `DoomClassicMaps` share one COMPAT, `DoomSurvival`.
2. That mod holds the conditionals for that other mod.
3. That mod also holds the conditionals that involve one of the AGF mods together with that other mod.
4. If a DLL is required, it is rebuilt and renamed for that other mod only.
5. DisplayName is `AGF COMPAT` plus the name, with a space. No parentheses.

0SCore:

| Field | Value |
|---|---|
| File name | `zzzzzAGF-V3-COMPAT-0SCore-v1.0.0` |
| Mod name | `AGF-COMPAT-0SCore` |
| Display name | `AGF COMPAT 0SCore` |

Every COMPAT folder is `v1.0.0`. DoomSurvival is `v2.1.0`.

The folder token can differ from the Name the game still checks. `0SCore` still checks `0-SCore_sphereii`. `GBZ15SlotToolbelt` still checks `15SlotToolbelt`. `OakravenAmmoPress` still checks `V2_OakravenAmmoPress`. `OutbackRoadies` still checks `V3_OutbackRoadies`. `POIScourgeLite` still checks `POI_Scourge_Lite`. `DoomSurvival` checks both Doom names.

| COMPAT name | DisplayName | Still checks |
|---|---|---|
| `0SCore` | `AGF COMPAT 0SCore` | `0-SCore_sphereii` |
| `GBZ15SlotToolbelt` | `AGF COMPAT GBZ15SlotToolbelt` | `15SlotToolbelt` |
| `Companions` | `AGF COMPAT Companions` | `Companions` |
| `Dewtas18SlotToolbelt` | `AGF COMPAT Dewtas` | `Dewtas18SlotToolbelt` |
| `DishongTowerChallenge` | `AGF COMPAT Dishong Tower Challenge` | `DishongTowerChallenge` |
| `QuickStack` | `AGF COMPAT Quick Stack` | `QuickStack` |
| `OakravenAmmoPress` | `AGF COMPAT OakravenAmmoPress` | `V2_OakravenAmmoPress` |
| `OutbackRoadies` | `AGF COMPAT OutbackRoadies` | `V3_OutbackRoadies` |
| `DoomSurvival` | `AGF COMPAT DoomSurvival` | `DoomMod_Standalone`, `DoomClassicMaps` |
| `POIScourgeLite` | `AGF COMPAT POIScourgeLite` | `POI_Scourge_Lite` |
| `WMM12SlotToolbelt` | `AGF COMPAT WMM12SlotToolbelt` | `WMM12SlotToolbelt` |
| `BDubVehicles` | `AGF COMPAT BDubVehicles` | none |
| `IZYWeapons` | `AGF COMPAT IZYWeapons` | none |
| `GSVanillaCookBook` | `AGF COMPAT GSVanillaCookBook` | `VanillaExtended` |

`OakravenAmmoPress` has patch files. No parent file includes them yet. The HelpfulRenames localization for Outback Roadies belongs in `OutbackRoadies`, including the check that both mods are loaded.

## Listings

No shelf letter. No pile numbers.

```
AGF - V# - SCOPE - ModName
```

`SCOPE` is SERVER, CLIENT, BOTH, or ADMIN. Details beyond that live in the readme Mod Scope section.

Older idea, not in the title: a family between SCOPE and the name (HUDPlus, BackpackPlus, Optional, EACDedi, Compat).

**Optional** = not in the main Giggle Pack set. That covers by-request mods and other extras. They are still yours. The footer can still say Giggle Pack.

**EACDedi** page line: dedicated server, EAC can stay on. Solo or listen-host, EAC off. Still a SERVER install.

## Folders

The folder includes the game `V#` and ends with `-vX.Y.Z`. `0` in front of AGF is the early-load prefix. Some number of `z` in front of AGF is the late-load prefix. The display name does not use that prefix. `1` `2` `3` are not folder prefixes.

| Folder | Who it is for |
|---|---|
| `AGF-BOTH-…` | Admin: every machine. Sorts first. |
| `AGF-CLIENT-…` | At least each player's own game. |
| `AGF-SERVER-Name` | Normal gameplay. EAC on or off anywhere. A–Z. |
| `AGF-SERVER-x…` | Your shelves, after that A–Z list. `x` is only a sort marker. |
| `AGF-SERVER-zHelpfulRenames` | Still loads last among SERVER. |
| `zzzAGF-…` | Still loads last overall. |

Shelves, in folder order: `xBackpackPlus`, `xEACDedi`, `xHUDPlus`, `xOptional`.

Listing says `HUDPlus`. Folder says `xHUDPlus`. Same for Backpack, EACDedi, Optional.

4Modders-style mods use ADMIN when they are for overhaul creators. A casual fix uses SERVER, CLIENT, or BOTH instead.

Your `0 / 1 / 2 / 3` order stays a pack-list order, not a folder prefix.

---

## Current names and the new names

Preview only. Current folder, then the folder it would become. Sorted by the new folder. The new folder keeps the `0` or `z` load prefix, includes `V3`, and ends with `-vX.Y.Z`.

SCOPE in the folder comes from the mod type table: 1, 2, and 5 are SERVER, 3 is BOTH, 4 is CLIENT. `4Modders` and ModSync are ADMIN. `TBD` means the type table has no type yet. `0AGF-LawnTractorPatchGuard` is CLIENT to match LawnTractorV3Fix. HelpfulRenames is SERVER, with three `z`s.

The HUDPlus shelf is dropped. `HUDPlus-1Main` becomes `1HUDPlus`. `BMCounter`, `PurpleBook`, `RemoveEnteringPopUp`, `VisualEntityTracker`, and `Weekday` get a `2` so they load next to it. `VisualEntityTrackerAddon` gets a `3` so it loads after the tracker.

Draft is unchanged except this list: CombatGlitchMitigations, MultiLookStorage, QuartermasterCrafting, and StorageLaptop are BOTH. ESCWindowPlus and ModSync are ADMIN. DecorationBlock is SERVER. ConsoleOpacityMod is CLIENT. TerrainLeveler stays `TBD`.

### ActiveBuild

| Current folder | New folder |
|---|---|
| `0AGF-LawnTractorPatchGuard-v1.0.0` | `0AGF-V3-CLIENT-LawnTractorPatchGuard-v1.0.0` |
| `AGF-4Modders-Fix4DestroyBiomeBadge-v2.1.0` | `AGF-V3-ADMIN-Fix4DestroyBiomeBadge-v2.1.0` |
| `AGF-4Modders-Fix4PerkPageReset-v1.0.0` | `AGF-V3-ADMIN-Fix4PerkPageReset-v1.0.0` |
| `AGF-NoEAC-CosmeticLockIcon-v3.1.2` | `AGF-V3-BOTH-CosmeticLockIcon-v3.1.2` |
| `AGF-NoEAC-GyroFlightModes-v1.0.1` | `AGF-V3-BOTH-GyroFlightModes-v1.0.1` |
| `AGF-NoEAC-PartyGroupPlus-v1.2.0` | `AGF-V3-BOTH-PartyGroupPlus-v1.2.0` |
| `AGF-NoEAC-Toolbelt12Slots-v2.3.0` | `AGF-V3-BOTH-Toolbelt12Slots-v2.3.0` |
| `AGF-NoEAC-AudioOptionsPlus-v2.1.0` | `AGF-V3-CLIENT-AudioOptionsPlus-v2.1.0` |
| `AGF-NoEAC-AutoRun-v2.2.0` | `AGF-V3-CLIENT-AutoRun-v2.2.0` |
| `AGF-NoEAC-EnhancedAGF-v5.0.0` | `AGF-V3-CLIENT-EnhancedAGF-v5.0.0` |
| `AGF-NoEAC-HideDLCCosmetics-v1.1.0` | `AGF-V3-CLIENT-HideDLCCosmetics-v1.1.0` |
| `AGF-NoEAC-MapPlus-v1.1.2` | `AGF-V3-CLIENT-MapPlus-v1.1.2` |
| `AGF-NoEAC-OpenAllButton-v2.0.1` | `AGF-V3-CLIENT-OpenAllButton-v2.0.1` |
| `AGF-NoEAC-SmeltTimerOption-v1.1.0` | `AGF-V3-CLIENT-SmeltTimerOption-v1.1.0` |
| `AGF-HUDPlus-1Main-v6.6.1` | `AGF-V3-SERVER-1HUDPlus-v6.6.1` |
| `AGF-HUDPlus-BMCounter-v4.0.2` | `AGF-V3-SERVER-2BMCounter-v4.0.2` |
| `AGF-HUDPlus-PurpleBook-v3.2.0` | `AGF-V3-SERVER-2PurpleBook-v3.2.0` |
| `AGF-HUDPlus-RemoveEnteringPopUp-v2.1.1` | `AGF-V3-SERVER-2RemoveEnteringPopUp-v2.1.1` |
| `AGF-HUDPlus-VisualEntityTracker-v1.1.1` | `AGF-V3-SERVER-2VisualEntityTracker-v1.1.1` |
| `AGF-HUDPlus-Weekday-v3.1.3` | `AGF-V3-SERVER-2Weekday-v3.1.3` |
| `AGF-VPS-VisualEntityTrackerAddon-v1.0.1` | `AGF-V3-SERVER-3VisualEntityTrackerAddon-v1.0.1` |
| `AGF-VP-AdminModdingSupport-v2.0.0` | `AGF-V3-SERVER-AdminModdingSupport-v2.0.0` |
| `AGF-VP-AlternativeRecipes-v2.0.0` | `AGF-V3-SERVER-AlternativeRecipes-v2.0.0` |
| `AGF-VP-AmmoDisassembly-v2.0.1` | `AGF-V3-SERVER-AmmoDisassembly-v2.0.1` |
| `AGF-Requested-AnimalTrackerAlwaysOn-v1.0.0` | `AGF-V3-SERVER-AnimalTrackerAlwaysOn-v1.0.0` |
| `AGF-VP-ArcheryFeathersChange-v2.0.0` | `AGF-V3-SERVER-ArcheryFeathersChange-v2.0.0` |
| `AGF-VP-AutomobilesRespawn-v4.1.1` | `AGF-V3-SERVER-AutomobilesRespawn-v4.1.1` |
| `AGF-VP-BedrollPlus-v2.1.1` | `AGF-V3-SERVER-BedrollPlus-v2.1.1` |
| `AGF-VP-BetterEggChance-v3.1.1` | `AGF-V3-SERVER-BetterEggChance-v3.1.1` |
| `AGF-VP-BreakItGetIt-v2.2.0` | `AGF-V3-SERVER-BreakItGetIt-v2.2.0` |
| `AGF-VP-CraftSewingKits-v2.1.1` | `AGF-V3-SERVER-CraftSewingKits-v2.1.1` |
| `AGF-VP-CraftStackEngBattCells-v4.0.0` | `AGF-V3-SERVER-CraftStackEngBattCells-v4.0.0` |
| `AGF-VP-CraftVitamins-v2.1.2` | `AGF-V3-SERVER-CraftVitamins-v2.1.2` |
| `AGF-VP-DoorsPlus-v4.0.1` | `AGF-V3-SERVER-DoorsPlus-v4.0.1` |
| `AGF-VP-DrinkableAcid-v3.0.1` | `AGF-V3-SERVER-DrinkableAcid-v3.0.1` |
| `AGF-VP-DyesPlus-v4.1.0` | `AGF-V3-SERVER-DyesPlus-v4.1.0` |
| `AGF-VP-FloraHarvester-v3.0.2` | `AGF-V3-SERVER-FloraHarvester-v3.0.2` |
| `AGF-VPS-FuelAutoShutOff-v1.1.0` | `AGF-V3-SERVER-FuelAutoShutOff-v1.1.0` |
| `AGF-VP-FuelBurnPlus-v3.1.2` | `AGF-V3-SERVER-FuelBurnPlus-v3.1.2` |
| `AGF-VPS-GlobalStormTracker-v2.1.2` | `AGF-V3-SERVER-GlobalStormTracker-v2.1.2` |
| `AGF-VPS-HonkOpensYourDoors-v1.0.1` | `AGF-V3-SERVER-HonkOpensYourDoors-v1.0.1` |
| `AGF-VP-LargerStorageOption-v1.2.0` | `AGF-V3-SERVER-LargerStorageOption-v1.2.0` |
| `AGF-VPS-LootTimerHolds-v1.0.0` | `AGF-V3-SERVER-LootTimerHolds-v1.0.0` |
| `AGF-VP-MasterTool-v7.1.2` | `AGF-V3-SERVER-MasterTool-v7.1.2` |
| `AGF-VP-MaxLevel500-v3.1.1` | `AGF-V3-SERVER-MaxLevel500-v3.1.1` |
| `AGF-VP-MiningPlus-v2.0.1` | `AGF-V3-SERVER-MiningPlus-v2.0.1` |
| `AGF-VP-Mod988-v3.0.0` | `AGF-V3-SERVER-Mod988-v3.0.0` |
| `AGF-VP-ModSlotsPlus-v4.1.2` | `AGF-V3-SERVER-ModSlotsPlus-v4.1.2` |
| `AGF-VP-PaintbrushPlus-v3.0.0` | `AGF-V3-SERVER-PaintbrushPlus-v3.0.0` |
| `AGF-VP-PickupLanternsPlus-v3.1.1` | `AGF-V3-SERVER-PickupLanternsPlus-v3.1.1` |
| `AGF-VP-PlayerResetQuests-v3.1.1` | `AGF-V3-SERVER-PlayerResetQuests-v3.1.1` |
| `AGF-VP-RebundleBundles-v2.1.2` | `AGF-V3-SERVER-RebundleBundles-v2.1.2` |
| `AGF-VP-RecipeRottingFlesh-v2.0.0` | `AGF-V3-SERVER-RecipeRottingFlesh-v2.0.0` |
| `AGF-VP-RestorePowerAnyTime-v2.1.1` | `AGF-V3-SERVER-RestorePowerAnyTime-v2.1.1` |
| `AGF-VP-ScrapBatts4Acid-v2.1.1` | `AGF-V3-SERVER-ScrapBatts4Acid-v2.1.1` |
| `AGF-VP-ScrapEquipmentFaster-v2.1.1` | `AGF-V3-SERVER-ScrapEquipmentFaster-v2.1.1` |
| `AGF-VPS-ScreamerAlert-v2.3.4` | `AGF-V3-SERVER-ScreamerAlert-v2.3.4` |
| `AGF-VP-SimplifiedStacks-v2.1.2` | `AGF-V3-SERVER-SimplifiedStacks-v2.1.2` |
| `AGF-Requested-SmallerInteractionPrompt-v2.0.0` | `AGF-V3-SERVER-SmallerInteractionPrompt-v2.0.0` |
| `AGF-VP-SmeltingPlus-v3.0.1` | `AGF-V3-SERVER-SmeltingPlus-v3.0.1` |
| `AGF-VPS-SortingCart-v2.1.0` | `AGF-V3-SERVER-SortingCart-v2.1.0` |
| `AGF-VP-StayLongerAnimalCorpse-v3.1.1` | `AGF-V3-SERVER-StayLongerAnimalCorpse-v3.1.1` |
| `AGF-VP-StayLongerPlayerBackpack-v3.1.1` | `AGF-V3-SERVER-StayLongerPlayerBackpack-v3.1.1` |
| `AGF-VP-TacticalRiflePlus-v3.0.0` | `AGF-V3-SERVER-TacticalRiflePlus-v3.0.0` |
| `AGF-Requested-TinyBuffsPopUp-v2.0.0` | `AGF-V3-SERVER-TinyBuffsPopUp-v2.0.0` |
| `AGF-VP-VehiclePerformance-v2.1.1` | `AGF-V3-SERVER-VehiclePerformance-v2.1.1` |
| `AGF-VP-VehicleStoragePlus-v3.1.1` | `AGF-V3-SERVER-VehicleStoragePlus-v3.1.1` |
| `AGF-VP-VehiclesExtraSeating-v2.1.1` | `AGF-V3-SERVER-VehiclesExtraSeating-v2.1.1` |
| `AGF-VP-ZombieCorpseLeaveQuicker-v3.1.1` | `AGF-V3-SERVER-ZombieCorpseLeaveQuicker-v3.1.1` |
| `AGF-VP-zHelpfulRenames-v3.0.1` | `zzzAGF-V3-SERVER-HelpfulRenames-v3.0.1` |
| `zzzAGF-Requested-LootStaysOnEmpty-v1.0.4` | `zzzAGF-V3-SERVER-LootStaysOnEmpty-v1.0.4` |
| `zzzzAGF-LawnTractorV3Fix-v1.0.0` | `zzzAGF-V3-CLIENT-LawnTractorV3Fix-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-0SCore-v1.0.0` |
| `zzzAGF-Special-LocalizationPatches-v1.0.2` | `zzzzzAGF-V3-COMPAT-BDubVehicles-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-Companions-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-Dewtas18SlotToolbelt-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-DishongTowerChallenge-v1.0.0` |
| `zzzAGF-Special-NoEACCompatibilities-v1.0.0` | `zzzzzAGF-V3-COMPAT-DoomSurvival-v2.1.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-GBZ15SlotToolbelt-v1.0.0` |
| `zzzAGF-Special-LocalizationPatches-v1.0.2` | `zzzzzAGF-V3-COMPAT-GSVanillaCookBook-v1.0.0` |
| `zzzAGF-Special-LocalizationPatches-v1.0.2` | `zzzzzAGF-V3-COMPAT-IZYWeapons-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-OakravenAmmoPress-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-OutbackRoadies-v1.0.0` |
| `zzzAGF-Special-NoEACCompatibilities-v1.0.0` | `zzzzzAGF-V3-COMPAT-POIScourgeLite-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-QuickStack-v1.0.0` |
| `zzzAGF-Special-Compatibilities-v5.3.0` | `zzzzzAGF-V3-COMPAT-WMM12SlotToolbelt-v1.0.0` |

### Draft

| Current folder | New folder |
|---|---|
| `AGF-NoEAC-TerrainLeveler-v0.0.2` | `AGF-V3-TBD-TerrainLeveler-v0.0.2` |
| `AGF-4Modders-10IngredientSlots-v1.0.2` | `AGF-V3-ADMIN-10IngredientSlots-v1.0.2` |
| `AGF-4Modders-AntiCM-v0.0.2` | `AGF-V3-ADMIN-AntiCM-v0.0.2` |
| `AGF-4Modders-CustomWindowEnteringDuration-v1.0.1` | `AGF-V3-ADMIN-CustomWindowEnteringDuration-v1.0.1` |
| `AGF-4Modders-ESCWindowPlus-v0.3.2` | `AGF-V3-ADMIN-ESCWindowPlus-v0.3.2` |
| `AGF-4Modders-Fix4RemoveItems-v1.0.1` | `AGF-V3-ADMIN-Fix4RemoveItems-v1.0.1` |
| `AGF-4Modders-ItemTypeIconColor-v2.0.0` | `AGF-V3-ADMIN-ItemTypeIconColor-v2.0.0` |
| `AGF-NoEAC-ModSync-v0.1.3` | `AGF-V3-ADMIN-ModSync-v0.1.3` |
| `AGF-4Modders-SkillPointCap-v1.0.1` | `AGF-V3-ADMIN-SkillPointCap-v1.0.1` |
| `AGF-NoEAC-CombatGlitchMitigations-v0.0.1` | `AGF-V3-BOTH-CombatGlitchMitigations-v0.0.1` |
| `AGF-NoEAC-MultiLookStorage-v0.0.1` | `AGF-V3-BOTH-MultiLookStorage-v0.0.1` |
| `AGF-NoEAC-QuartermasterCrafting-v0.0.2` | `AGF-V3-BOTH-QuartermasterCrafting-v0.0.2` |
| `AGF-NoEAC-StorageLaptop-v0.0.1` | `AGF-V3-BOTH-StorageLaptop-v0.0.1` |
| `AGF-NoEAC-ConsoleOpacityMod-v1.0.1` | `AGF-V3-CLIENT-ConsoleOpacityMod-v1.0.1` |
| `AGF-VP-ApiaryPlus-v1.0.4` | `AGF-V3-SERVER-ApiaryPlus-v1.0.4` |
| `AGF-VP-ArmorHarvestMods-v2.1.1` | `AGF-V3-SERVER-ArmorHarvestMods-v2.1.1` |
| `AGF-VP-BuyTraderVendingMachines-v3.0.3` | `AGF-V3-SERVER-BuyTraderVendingMachines-v3.0.3` |
| `AGF-VP-DecorationBlock-v3.0.3` | `AGF-V3-SERVER-DecorationBlock-v3.0.3` |
| `AGF-VP-DewsPlus-v2.4.3` | `AGF-V3-SERVER-DewsPlus-v2.4.3` |
| `AGF-VP-FarmingPlus-v5.6.3` | `AGF-V3-SERVER-FarmingPlus-v5.6.3` |
| `AGF-VPS-LockableStations-v1.2.2` | `AGF-V3-SERVER-LockableStations-v1.2.2` |
| `AGF-VP-MedicalTreatOthersTreatedInjuries-v0.0.2` | `AGF-V3-SERVER-MedicalTreatOthersTreatedInjuries-v0.0.2` |
| `AGF-VP-MedicallyTreatOthers-v0.0.2` | `AGF-V3-SERVER-MedicallyTreatOthers-v0.0.2` |
| `AGF-VP-MedicationNoInsectSlow-v1.1.1` | `AGF-V3-SERVER-MedicationNoInsectSlow-v1.1.1` |
| `AGF-VP-ModBundling-v1.0.2` | `AGF-V3-SERVER-ModBundling-v1.0.2` |
| `AGF-VP-PlayerVendingMachinesPlus-v1.0.1` | `AGF-V3-SERVER-PlayerVendingMachinesPlus-v1.0.1` |
| `AGF-VP-PumpkinsPlus-v2.0.5` | `AGF-V3-SERVER-PumpkinsPlus-v2.0.5` |
| `AGF-VP-TreesPlus-v3.0.2` | `AGF-V3-SERVER-TreesPlus-v3.0.2` |
| `AGF-VP-WriteStoryOnCrate-v1.0.1` | `AGF-V3-SERVER-WriteStoryOnCrate-v1.0.1` |
| `AGF-VP-XPDeathPenaltyReduction-v2.0.1` | `AGF-V3-SERVER-XPDeathPenaltyReduction-v2.0.1` |

## Suggested ModInfo Name and DisplayName

Suggestion only. These two fields are for `ModInfo.xml`. They are not the folder and they are not the public title.

Name is `AGF-` plus the mod name. DisplayName is `AGF` plus that name, with a space between words.

A `0` or `z` and `V3` stay on the folder. They are not in Name or DisplayName. SERVER, CLIENT, BOTH, and ADMIN stay on the folder too. COMPAT is the exception: it is in the Name and the DisplayName. The HUD `1`, `2`, and `3` stay on the Name. They stay off the DisplayName.

`AGF-EnhancedAGF` displays as `AGF Enhanced`. COMPAT Name is `AGF-COMPAT-` plus the other mod's token. COMPAT DisplayName is `AGF COMPAT` plus the name. A DisplayName stays `not yet` until that name is provided.

### ActiveBuild

| New folder | Suggested Name | Suggested DisplayName |
|---|---|---|
| `0AGF-V3-CLIENT-LawnTractorPatchGuard-v1.0.0` | `AGF-LawnTractorPatchGuard` | `AGF Lawn Tractor Patch Guard` |
| `AGF-V3-ADMIN-Fix4DestroyBiomeBadge-v2.1.0` | `AGF-Fix4DestroyBiomeBadge` | `AGF Fix 4 Destroy Biome Badge` |
| `AGF-V3-ADMIN-Fix4PerkPageReset-v1.0.0` | `AGF-Fix4PerkPageReset` | `AGF Fix 4 Perk Page Reset` |
| `AGF-V3-BOTH-CosmeticLockIcon-v3.1.2` | `AGF-CosmeticLockIcon` | `AGF Cosmetic Lock Icon` |
| `AGF-V3-BOTH-GyroFlightModes-v1.0.1` | `AGF-GyroFlightModes` | `AGF Gyro Flight Modes` |
| `AGF-V3-BOTH-PartyGroupPlus-v1.2.0` | `AGF-PartyGroupPlus` | `AGF Party Group Plus` |
| `AGF-V3-BOTH-Toolbelt12Slots-v2.3.0` | `AGF-Toolbelt12Slots` | `AGF Toolbelt 12 Slots` |
| `AGF-V3-CLIENT-AudioOptionsPlus-v2.1.0` | `AGF-AudioOptionsPlus` | `AGF Audio Options Plus` |
| `AGF-V3-CLIENT-AutoRun-v2.2.0` | `AGF-AutoRun` | `AGF Auto Run` |
| `AGF-V3-CLIENT-EnhancedAGF-v5.0.0` | `AGF-EnhancedAGF` | `AGF Enhanced` |
| `AGF-V3-CLIENT-HideDLCCosmetics-v1.1.0` | `AGF-HideDLCCosmetics` | `AGF Hide DLC Cosmetics` |
| `AGF-V3-CLIENT-MapPlus-v1.1.2` | `AGF-MapPlus` | `AGF Map Plus` |
| `AGF-V3-CLIENT-OpenAllButton-v2.0.1` | `AGF-OpenAllButton` | `AGF Open All Button` |
| `AGF-V3-CLIENT-SmeltTimerOption-v1.1.0` | `AGF-SmeltTimerOption` | `AGF Smelt Timer Option` |
| `AGF-V3-SERVER-1HUDPlus-v6.6.1` | `AGF-1HUDPlus` | `AGF HUD Plus` |
| `AGF-V3-SERVER-2BMCounter-v4.0.2` | `AGF-2BMCounter` | `AGF BM Counter` |
| `AGF-V3-SERVER-2PurpleBook-v3.2.0` | `AGF-2PurpleBook` | `AGF Purple Book` |
| `AGF-V3-SERVER-2RemoveEnteringPopUp-v2.1.1` | `AGF-2RemoveEnteringPopUp` | `AGF Remove Entering Pop Up` |
| `AGF-V3-SERVER-2VisualEntityTracker-v1.1.1` | `AGF-2VisualEntityTracker` | `AGF Visual Entity Tracker` |
| `AGF-V3-SERVER-2Weekday-v3.1.3` | `AGF-2Weekday` | `AGF Weekday` |
| `AGF-V3-SERVER-3VisualEntityTrackerAddon-v1.0.1` | `AGF-3VisualEntityTrackerAddon` | `AGF Visual Entity Tracker Addon` |
| `AGF-V3-SERVER-AdminModdingSupport-v2.0.0` | `AGF-AdminModdingSupport` | `AGF Admin Modding Support` |
| `AGF-V3-SERVER-AlternativeRecipes-v2.0.0` | `AGF-AlternativeRecipes` | `AGF Alternative Recipes` |
| `AGF-V3-SERVER-AmmoDisassembly-v2.0.1` | `AGF-AmmoDisassembly` | `AGF Ammo Disassembly` |
| `AGF-V3-SERVER-AnimalTrackerAlwaysOn-v1.0.0` | `AGF-AnimalTrackerAlwaysOn` | `AGF Animal Tracker Always On` |
| `AGF-V3-SERVER-ArcheryFeathersChange-v2.0.0` | `AGF-ArcheryFeathersChange` | `AGF Archery Feathers Change` |
| `AGF-V3-SERVER-AutomobilesRespawn-v4.1.1` | `AGF-AutomobilesRespawn` | `AGF Automobiles Respawn` |
| `AGF-V3-SERVER-BedrollPlus-v2.1.1` | `AGF-BedrollPlus` | `AGF Bedroll Plus` |
| `AGF-V3-SERVER-BetterEggChance-v3.1.1` | `AGF-BetterEggChance` | `AGF Better Egg Chance` |
| `AGF-V3-SERVER-BreakItGetIt-v2.2.0` | `AGF-BreakItGetIt` | `AGF Break It Get It` |
| `AGF-V3-SERVER-CraftSewingKits-v2.1.1` | `AGF-CraftSewingKits` | `AGF Craft Sewing Kits` |
| `AGF-V3-SERVER-CraftStackEngBattCells-v4.0.0` | `AGF-CraftStackEngBattCells` | `AGF Craft Stack Eng Batt Cells` |
| `AGF-V3-SERVER-CraftVitamins-v2.1.2` | `AGF-CraftVitamins` | `AGF Craft Vitamins` |
| `AGF-V3-SERVER-DoorsPlus-v4.0.1` | `AGF-DoorsPlus` | `AGF Doors Plus` |
| `AGF-V3-SERVER-DrinkableAcid-v3.0.1` | `AGF-DrinkableAcid` | `AGF Drinkable Acid` |
| `AGF-V3-SERVER-DyesPlus-v4.1.0` | `AGF-DyesPlus` | `AGF Dyes Plus` |
| `AGF-V3-SERVER-FloraHarvester-v3.0.2` | `AGF-FloraHarvester` | `AGF Flora Harvester` |
| `AGF-V3-SERVER-FuelAutoShutOff-v1.1.0` | `AGF-FuelAutoShutOff` | `AGF Fuel Auto Shut Off` |
| `AGF-V3-SERVER-FuelBurnPlus-v3.1.2` | `AGF-FuelBurnPlus` | `AGF Fuel Burn Plus` |
| `AGF-V3-SERVER-GlobalStormTracker-v2.1.2` | `AGF-GlobalStormTracker` | `AGF Global Storm Tracker` |
| `AGF-V3-SERVER-HonkOpensYourDoors-v1.0.1` | `AGF-HonkOpensYourDoors` | `AGF Honk Opens Your Doors` |
| `AGF-V3-SERVER-LargerStorageOption-v1.2.0` | `AGF-LargerStorageOption` | `AGF Larger Storage Option` |
| `AGF-V3-SERVER-LootTimerHolds-v1.0.0` | `AGF-LootTimerHolds` | `AGF Loot Timer Holds` |
| `AGF-V3-SERVER-MasterTool-v7.1.2` | `AGF-MasterTool` | `AGF Master Tool` |
| `AGF-V3-SERVER-MaxLevel500-v3.1.1` | `AGF-MaxLevel500` | `AGF Max Level 500` |
| `AGF-V3-SERVER-MiningPlus-v2.0.1` | `AGF-MiningPlus` | `AGF Mining Plus` |
| `AGF-V3-SERVER-Mod988-v3.0.0` | `AGF-Mod988` | `AGF Mod 988` |
| `AGF-V3-SERVER-ModSlotsPlus-v4.1.2` | `AGF-ModSlotsPlus` | `AGF Mod Slots Plus` |
| `AGF-V3-SERVER-PaintbrushPlus-v3.0.0` | `AGF-PaintbrushPlus` | `AGF Paintbrush Plus` |
| `AGF-V3-SERVER-PickupLanternsPlus-v3.1.1` | `AGF-PickupLanternsPlus` | `AGF Pickup Lanterns Plus` |
| `AGF-V3-SERVER-PlayerResetQuests-v3.1.1` | `AGF-PlayerResetQuests` | `AGF Player Reset Quests` |
| `AGF-V3-SERVER-RebundleBundles-v2.1.2` | `AGF-RebundleBundles` | `AGF Rebundle Bundles` |
| `AGF-V3-SERVER-RecipeRottingFlesh-v2.0.0` | `AGF-RecipeRottingFlesh` | `AGF Recipe Rotting Flesh` |
| `AGF-V3-SERVER-RestorePowerAnyTime-v2.1.1` | `AGF-RestorePowerAnyTime` | `AGF Restore Power Any Time` |
| `AGF-V3-SERVER-ScrapBatts4Acid-v2.1.1` | `AGF-ScrapBatts4Acid` | `AGF Scrap Batts 4 Acid` |
| `AGF-V3-SERVER-ScrapEquipmentFaster-v2.1.1` | `AGF-ScrapEquipmentFaster` | `AGF Scrap Equipment Faster` |
| `AGF-V3-SERVER-ScreamerAlert-v2.3.4` | `AGF-ScreamerAlert` | `AGF Screamer Alert` |
| `AGF-V3-SERVER-SimplifiedStacks-v2.1.2` | `AGF-SimplifiedStacks` | `AGF Simplified Stacks` |
| `AGF-V3-SERVER-SmallerInteractionPrompt-v2.0.0` | `AGF-SmallerInteractionPrompt` | `AGF Smaller Interaction Prompt` |
| `AGF-V3-SERVER-SmeltingPlus-v3.0.1` | `AGF-SmeltingPlus` | `AGF Smelting Plus` |
| `AGF-V3-SERVER-SortingCart-v2.1.0` | `AGF-SortingCart` | `AGF Sorting Cart` |
| `AGF-V3-SERVER-StayLongerAnimalCorpse-v3.1.1` | `AGF-StayLongerAnimalCorpse` | `AGF Stay Longer Animal Corpse` |
| `AGF-V3-SERVER-StayLongerPlayerBackpack-v3.1.1` | `AGF-StayLongerPlayerBackpack` | `AGF Stay Longer Player Backpack` |
| `AGF-V3-SERVER-TacticalRiflePlus-v3.0.0` | `AGF-TacticalRiflePlus` | `AGF Tactical Rifle Plus` |
| `AGF-V3-SERVER-TinyBuffsPopUp-v2.0.0` | `AGF-TinyBuffsPopUp` | `AGF Tiny Buffs Pop Up` |
| `AGF-V3-SERVER-VehiclePerformance-v2.1.1` | `AGF-VehiclePerformance` | `AGF Vehicle Performance` |
| `AGF-V3-SERVER-VehicleStoragePlus-v3.1.1` | `AGF-VehicleStoragePlus` | `AGF Vehicle Storage Plus` |
| `AGF-V3-SERVER-VehiclesExtraSeating-v2.1.1` | `AGF-VehiclesExtraSeating` | `AGF Vehicles Extra Seating` |
| `AGF-V3-SERVER-ZombieCorpseLeaveQuicker-v3.1.1` | `AGF-ZombieCorpseLeaveQuicker` | `AGF Zombie Corpse Leave Quicker` |
| `zzzAGF-V3-SERVER-HelpfulRenames-v3.0.1` | `AGF-HelpfulRenames` | `AGF Helpful Renames` |
| `zzzAGF-V3-SERVER-LootStaysOnEmpty-v1.0.4` | `AGF-LootStaysOnEmpty` | `AGF Loot Stays On Empty` |
| `zzzAGF-V3-CLIENT-LawnTractorV3Fix-v1.0.0` | `AGF-LawnTractorV3Fix` | `AGF Lawn Tractor V3 Fix` |
| `zzzzzAGF-V3-COMPAT-0SCore-v1.0.0` | `AGF-COMPAT-0SCore` | `AGF COMPAT 0SCore` |
| `zzzzzAGF-V3-COMPAT-BDubVehicles-v1.0.0` | `AGF-COMPAT-BDubVehicles` | `AGF COMPAT BDubVehicles` |
| `zzzzzAGF-V3-COMPAT-Companions-v1.0.0` | `AGF-COMPAT-Companions` | `AGF COMPAT Companions` |
| `zzzzzAGF-V3-COMPAT-Dewtas18SlotToolbelt-v1.0.0` | `AGF-COMPAT-Dewtas18SlotToolbelt` | `AGF COMPAT Dewtas` |
| `zzzzzAGF-V3-COMPAT-DishongTowerChallenge-v1.0.0` | `AGF-COMPAT-DishongTowerChallenge` | `AGF COMPAT Dishong Tower Challenge` |
| `zzzzzAGF-V3-COMPAT-DoomSurvival-v2.1.0` | `AGF-COMPAT-DoomSurvival` | `AGF COMPAT DoomSurvival` |
| `zzzzzAGF-V3-COMPAT-GBZ15SlotToolbelt-v1.0.0` | `AGF-COMPAT-GBZ15SlotToolbelt` | `AGF COMPAT GBZ15SlotToolbelt` |
| `zzzzzAGF-V3-COMPAT-GSVanillaCookBook-v1.0.0` | `AGF-COMPAT-GSVanillaCookBook` | `AGF COMPAT GSVanillaCookBook` |
| `zzzzzAGF-V3-COMPAT-IZYWeapons-v1.0.0` | `AGF-COMPAT-IZYWeapons` | `AGF COMPAT IZYWeapons` |
| `zzzzzAGF-V3-COMPAT-OakravenAmmoPress-v1.0.0` | `AGF-COMPAT-OakravenAmmoPress` | `AGF COMPAT OakravenAmmoPress` |
| `zzzzzAGF-V3-COMPAT-OutbackRoadies-v1.0.0` | `AGF-COMPAT-OutbackRoadies` | `AGF COMPAT OutbackRoadies` |
| `zzzzzAGF-V3-COMPAT-POIScourgeLite-v1.0.0` | `AGF-COMPAT-POIScourgeLite` | `AGF COMPAT POIScourgeLite` |
| `zzzzzAGF-V3-COMPAT-QuickStack-v1.0.0` | `AGF-COMPAT-QuickStack` | `AGF COMPAT Quick Stack` |
| `zzzzzAGF-V3-COMPAT-WMM12SlotToolbelt-v1.0.0` | `AGF-COMPAT-WMM12SlotToolbelt` | `AGF COMPAT WMM12SlotToolbelt` |

### Draft

| New folder | Suggested Name | Suggested DisplayName |
|---|---|---|
| `AGF-V3-TBD-TerrainLeveler-v0.0.2` | `AGF-TerrainLeveler` | `AGF Terrain Leveler` |
| `AGF-V3-ADMIN-10IngredientSlots-v1.0.2` | `AGF-10IngredientSlots` | `AGF 10 Ingredient Slots` |
| `AGF-V3-ADMIN-AntiCM-v0.0.2` | `AGF-AntiCM` | `AGF Anti CM` |
| `AGF-V3-ADMIN-CustomWindowEnteringDuration-v1.0.1` | `AGF-CustomWindowEnteringDuration` | `AGF Custom Window Entering Duration` |
| `AGF-V3-ADMIN-ESCWindowPlus-v0.3.2` | `AGF-ESCWindowPlus` | `AGF ESC Window Plus` |
| `AGF-V3-ADMIN-Fix4RemoveItems-v1.0.1` | `AGF-Fix4RemoveItems` | `AGF Fix 4 Remove Items` |
| `AGF-V3-ADMIN-ItemTypeIconColor-v2.0.0` | `AGF-ItemTypeIconColor` | `AGF Item Type Icon Color` |
| `AGF-V3-ADMIN-ModSync-v0.1.3` | `AGF-ModSync` | `AGF Mod Sync` |
| `AGF-V3-ADMIN-SkillPointCap-v1.0.1` | `AGF-SkillPointCap` | `AGF Skill Point Cap` |
| `AGF-V3-BOTH-CombatGlitchMitigations-v0.0.1` | `AGF-CombatGlitchMitigations` | `AGF Combat Glitch Mitigations` |
| `AGF-V3-BOTH-MultiLookStorage-v0.0.1` | `AGF-MultiLookStorage` | `AGF Multi Look Storage` |
| `AGF-V3-BOTH-QuartermasterCrafting-v0.0.2` | `AGF-QuartermasterCrafting` | `AGF Quartermaster Crafting` |
| `AGF-V3-BOTH-StorageLaptop-v0.0.1` | `AGF-StorageLaptop` | `AGF Storage Laptop` |
| `AGF-V3-CLIENT-ConsoleOpacityMod-v1.0.1` | `AGF-ConsoleOpacityMod` | `AGF Console Opacity Mod` |
| `AGF-V3-SERVER-ApiaryPlus-v1.0.4` | `AGF-ApiaryPlus` | `AGF Apiary Plus` |
| `AGF-V3-SERVER-ArmorHarvestMods-v2.1.1` | `AGF-ArmorHarvestMods` | `AGF Armor Harvest Mods` |
| `AGF-V3-SERVER-BuyTraderVendingMachines-v3.0.3` | `AGF-BuyTraderVendingMachines` | `AGF Buy Trader Vending Machines` |
| `AGF-V3-SERVER-DecorationBlock-v3.0.3` | `AGF-DecorationBlock` | `AGF Decoration Block` |
| `AGF-V3-SERVER-DewsPlus-v2.4.3` | `AGF-DewsPlus` | `AGF Dews Plus` |
| `AGF-V3-SERVER-FarmingPlus-v5.6.3` | `AGF-FarmingPlus` | `AGF Farming Plus` |
| `AGF-V3-SERVER-LockableStations-v1.2.2` | `AGF-LockableStations` | `AGF Lockable Stations` |
| `AGF-V3-SERVER-MedicalTreatOthersTreatedInjuries-v0.0.2` | `AGF-MedicalTreatOthersTreatedInjuries` | `AGF Medical Treat Others Treated Injuries` |
| `AGF-V3-SERVER-MedicallyTreatOthers-v0.0.2` | `AGF-MedicallyTreatOthers` | `AGF Medically Treat Others` |
| `AGF-V3-SERVER-MedicationNoInsectSlow-v1.1.1` | `AGF-MedicationNoInsectSlow` | `AGF Medication No Insect Slow` |
| `AGF-V3-SERVER-ModBundling-v1.0.2` | `AGF-ModBundling` | `AGF Mod Bundling` |
| `AGF-V3-SERVER-PlayerVendingMachinesPlus-v1.0.1` | `AGF-PlayerVendingMachinesPlus` | `AGF Player Vending Machines Plus` |
| `AGF-V3-SERVER-PumpkinsPlus-v2.0.5` | `AGF-PumpkinsPlus` | `AGF Pumpkins Plus` |
| `AGF-V3-SERVER-TreesPlus-v3.0.2` | `AGF-TreesPlus` | `AGF Trees Plus` |
| `AGF-V3-SERVER-WriteStoryOnCrate-v1.0.1` | `AGF-WriteStoryOnCrate` | `AGF Write Story On Crate` |
| `AGF-V3-SERVER-XPDeathPenaltyReduction-v2.0.1` | `AGF-XPDeathPenaltyReduction` | `AGF XP Death Penalty Reduction` |
