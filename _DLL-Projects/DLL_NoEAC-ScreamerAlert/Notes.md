# Screamer Alert Mod - Documentation

Audience:
This document is for human readers and for AI systems that may need to review or recreate the mod. Each section includes a plain-language explanation and technical details.

## I. Overview

### A. Basic Description
This mod shows on-screen alerts when screamers or horde zombies are nearby. It works in singleplayer and multiplayer.

### B. Tech Details
- Code is in the ScreamerAlertVariant folder.
- Uses Harmony patches plus UI controller classes.
- Multiplayer flow is server authoritative and synced to clients.

### C. Main Files
- ScreamerAlertManager.cs: tracks screamers/horde entities, server sync flow, and list state.
- ScreamerAlertsController.cs: per-player alert evaluation and UI-facing state.
- NetPackageScreamerAlertSync.cs: net package that syncs alert tracking data to clients.
- XUiC_ScreamerAlerts.cs: UI controller for text bindings.

## II. UI Implementation

### A. Basic Description
When a screamer or horde threat is near the player, alert text appears on screen. It clears when no tracked threat is within range.

### B. Tech Details
- UI binds to controller values `{screameralert}` and `{screamerhordealert}`.
- UI logic is handled by XUiC_ScreamerAlerts.cs and ScreamerAlertsController.cs.
- Typical XML block:

```xml
<rect name="ScreamerAlertRect" controller="ScreamerAlerts, ScreamerAlert">
    <sprite depth="2" name="Background" pos="125,-131" width="250" height="98" sprite="ui_game_header_fill" color="0,0,0,180" type="sliced" pivot="center"/>
    <label depth="3" name="ScreamerAlert" pos="125,-113" font_size="28" color="[white]" style="outline" text="{screameralert}" pivot="center" justify="center" width="250" height="38"/>
    <label depth="3" name="ScreamerHordeAlert" pos="125,-149" font_size="28" color="[white]" style="outline" text="{screamerhordealert}" pivot="center" justify="center" width="250" height="38"/>
</rect>
```

## III. Localization

### A. Basic Description
Alert text can be translated by editing localization rows.

### B. Localization Example
```csv
Key,File,Type,UsedInMainMenu,NoTranslate,english,Context / Alternate Text,german,spanish,french,italian,japanese,koreana,polish,brazilian,russian,turkish,schinese,tchinese
ScreamerAlert_Scout,ui,label,,,"[FF5555]Screamer Alert[-]",,"[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]","[FF5555]Screamer Alert[-]"
ScreamerAlert_Horde,ui,label,,,"[FFA94D]Horde Incoming[-]",,"[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]","[FFA94D]Horde Incoming[-]"
```

### C. Tech Details
- These keys are consumed by the screamer alert UI controller and rendered through the UI binding fields.

## IV. MCS Build Command

### A. Basic Description
Run this exact command in PowerShell from the ScreamerAlertVariant directory to compile ScreamerAlert.dll.

### B. Tech Details (Exact Paste Command)
```powershell
& mcs -target:library -out:ScreamerAlert.dll -recurse:*.cs `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\UnityEngine.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\UnityEngine.CoreModule.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\0_TFP_Harmony\0Harmony.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\netstandard.dll"
```

## V. DLL References

### A. Basic Description
The build depends on game and Harmony DLL files. If a reference path is wrong, compile will fail.

### B. Tech Details
- 0Harmony.dll: Mods/0_TFP_Harmony/0Harmony.dll
- UnityEngine.dll: 7DaysToDie_Data/Managed/UnityEngine.dll
- UnityEngine.CoreModule.dll: 7DaysToDie_Data/Managed/UnityEngine.CoreModule.dll
- Assembly-CSharp.dll: 7DaysToDie_Data/Managed/Assembly-CSharp.dll
- netstandard.dll: 7DaysToDie_Data/Managed/netstandard.dll

## VI. Screamer Tracking and Alert Logic

### A. Basic Description
The mod keeps a tracked list of screamers. If any tracked screamer is close enough, the screamer alert is active.

### B. Tech Details
- Server and singleplayer maintain authoritative screamer IDs in a persistent set.
- Clients store synced IDs from NetPackageScreamerAlertSync.
- Proximity check uses player position versus tracked entity position (distance threshold, typically 120m).
- Alert text state is derived from current tracked-in-range results.
- Server-side entity scan and authoritative screamer rebuild now run on a 0.2 second interval.

### C. Patch/Hook Coverage
- Spawn/update related patch files in this mod include:
  - ScreamerScoutSpawnPatch.cs
  - EntityFactoryCreateEntityPatch.cs
  - AIScoutHordeSpawnerSpawnPatch.cs
  - AIScoutHordeSpawnerSpawnUpdatePatch.cs

## VII. Horde Tracking and Alert Logic

### A. Basic Description
Horde zombies are tracked separately and use a different alert text.

### B. Tech Details
- Horde IDs are maintained in a dedicated persistent set.
- Similar proximity logic to screamers, but writes to the horde alert channel.
- Relevant patch coverage includes horde spawner and additional horde spawn hooks, such as:
  - AIHordeSpawnerTickPatch.cs
  - HordeSpawnMorePatch.cs

## VIII. Multiplayer Handling

### A. Basic Description
Server tracks global state; clients display alerts using synced data.

### B. Tech Details
- Server sends screamer/horde tracking updates through NetPackageScreamerAlertSync on a regular interval.
- Current sync send interval is 0.5 seconds.
- Clients deserialize and refresh local synced sets.
- This avoids relying on non-authoritative or partially synced vanilla fields on clients.

---
Last updated: March 25, 2026
