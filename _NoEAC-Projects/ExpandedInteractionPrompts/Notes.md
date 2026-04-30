# Expanded Interaction Prompts - Mod Outline

## I. Overall Summary

### A. Basic Description
This mod extends interaction text so players get useful status details without opening the UI. It adds readable, color-coded info for workstations, forges, storages, and vehicles.

### B. Tech Details
- Main logic lives in Patch_BlockWorkstation_GetActivationText.cs and Patch_PlayerMoveController_Update.cs.
- Uses Harmony postfix patches for block/tile activation text and a Harmony transpiler for vehicle prompt formatting inside PlayerMoveController.Update.
- Formatting is centralized in SafeFormatter to avoid format crashes and keep prompt building consistent.

## II. Localization Example

### A. Basic Description
All visible prompt fragments are localization keys so text can be translated and color-adjusted without code edits.

### B. Localization Data Example
```csv
Key,File,Type,UsedInMainMenu,NoTranslate,english,Context / Alternate Text,german,spanish,french,italian,japanese,koreana,polish,brazilian,russian,turkish,schinese,tchinese
xuiSlots,UI,HUD,,,[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-],[00ff00]({0})[-]
xuiLockedAGF,UI,Tooltip,,,"[ff0000](Locked)[-]",,"[ff0000](Verriegelt)[-]","[ff0000](Bloqueado)[-]","[ff0000](Verrouille(e))[-]","[ff0000](Bloccato)[-]","[ff0000](ロック状態)[-]","[ff0000](잠김)[-]","[ff0000](Zamkniete)[-]","[ff0000](Trancado)[-]","[ff0000](Заперто)[-]","[ff0000](Kilitli)[-]","[ff0000](已锁)[-]","[ff0000](已鎖定)[-]"
xuiUnlockedAGF,UI,Tooltip,,,"[00ff00](Unlocked)[-]",,"[00ff00](Entsperrt)[-]","[00ff00](Desbloqueado)[-]","[00ff00](Deverrouille(e))[-]","[00ff00](Sbloccato)[-]","[00ff00](ロック解除)[-]","[00ff00](열림)[-]","[00ff00](Odblokowane)[-]","[00ff00](Desbloqueado)[-]","[00ff00](Разблокировано)[-]","[00ff00](Kilidi Acik)[-]","[00ff00](已解锁)[-]","[00ff00](已解鎖)[-]"
xuiSeatsAGF,UI,Tooltip,,,"[ddcdfa](Seats {0})[-]",,"[ddcdfa](Sitze {0})[-]","[ddcdfa](Asientos {0})[-]","[ddcdfa](Sieges {0})[-]","[ddcdfa](Sedili {0})[-]","[ddcdfa](座席 {0})[-]","[ddcdfa](좌석 {0})[-]","[ddcdfa](Miejsca {0})[-]","[ddcdfa](Assentos {0})[-]","[ddcdfa](Мест {0})[-]","[ddcdfa](Koltuk {0})[-]","[ddcdfa](座位 {0})[-]","[ddcdfa](座位 {0})[-]"
```

### C. Tech Details
- Localization file is Localization.txt.
- Vehicle keys include xuiSeatsAGF, xuiLockedAGF, xuiUnlockedAGF, xuiSlots, xuiEmpty.
- Workstation and forge keys include xuiQueued, xuiNoFuel, xuiNeed2TurnOn, xuiSmeltingAGF.

## III. How This Is Patched

### A. Basic Description
The mod injects into existing game prompt functions rather than replacing whole systems.

### B. Tech Details
- Patch_PlayerMoveController_Update transpiles string.Format calls:
  - Redirects string.Format(string, object, object) to SafeFormatter.FocusAwareFormat.
  - Redirects string.Format(string, object) to SafeFormatter.BulletproofFormat.
- Postfix patches add storage and workstation details after vanilla prompt creation:
  - BlockSecureLoot.GetActivationText
  - BlockSecureLootSigned.GetActivationText
  - BlockLoot.GetActivationText
  - TEFeatureStorage.GetActivationText
  - BlockForge.GetActivationText
  - BlockCampfire.GetActivationText
  - BlockWorkstation.GetActivationText

## IV. useWorkstation

### A. Basic Description
For normal workstations (like workbench and mixer), prompt shows crafting status and output slots.

### B. Tech Details
- Reads workstation state from TileEntityWorkstation.
- Uses queue check (hasRecipeInQueue) for crafting state.
- No burn/fuel decision tree for plain workstation prompts.
- Output slot display is appended as xuiEmpty when no used slots, or xuiSlots with used/total when items exist.

## V. useCampfire

### A. Basic Description
Campfire-style stations show one active crafting condition and slot usage.

### B. Tech Details
- Checks queue plus fire/fuel state from TileEntityWorkstation fields (IsBurning, Fuel, BurnTimeLeft).
- Status priority when queue exists:
  - Burning with fuel or burn time -> xuiQueued
  - Not burning but fuel or burn time present -> xuiNeed2TurnOn
  - No usable fuel or burn time -> xuiNoFuel
- Output slots appended with xuiEmpty or xuiSlots.

## VI. useForge

### A. Basic Description
Forge prompts include smelting progress plus normal crafting and output details.

### B. Tech Details
- Patch resolves parent block when player targets a forge multiblock child.
- Smelting section counts primary forge input slots (first 3 Input slots) and formats through xuiSmeltingAGF.
- Crafting status follows the same queue/burn/fuel logic as campfire.
- Output section uses output array counting and appends xuiEmpty or xuiSlots.

## VII. Storages

### A. Basic Description
Storage prompts show how full a container is directly in the interaction text.

### B. Tech Details
- Secure storage patches (BlockSecureLoot and BlockSecureLootSigned) append slot usage when accessible or normally lockable.
- Jammed secure locks are intentionally skipped.
- Standard loot (BlockLoot) only appends data in touched/non-empty state to match expected loot behavior.
- Player/feature storage patch (TEFeatureStorage) appends slot data unless prompt is jammed.
- Used slot counting rule is item != null and not IsEmpty().

## VIII. Vehicles

### A. Basic Description
Vehicle prompts show seats and lock status for everyone, and storage slots only for owner/allies.

### B. Tech Details
- Vehicle prompt injection is done in SafeFormatter.FocusAwareFormat from the player controller transpiler path.
- Trigger condition is when format string equals localized tooltipInteract and current raycast hit resolves to a vehicle.
- Vehicle resolve path is Voxel.voxelRayHitInfo to EntityVehicle.FindCollisionEntity(transform).
- Access checks:
  - Owner via LocalPlayerIsOwner()
  - Ally/allowed user via isAllowedUser(PlatformManager.InternalLocalUserIdentifier)
- Output order:
  - Seat count from GetAttachMaxCount() using xuiSeatsAGF
  - Lock state from isLocked using xuiLockedAGF or xuiUnlockedAGF
  - Slots using xuiSlots with used/total, only if owner/allowed
- Vehicle storage source is entityVehicle.bag.GetSlots() (not lootContainer.items).

---
Last updated: March 25, 2026