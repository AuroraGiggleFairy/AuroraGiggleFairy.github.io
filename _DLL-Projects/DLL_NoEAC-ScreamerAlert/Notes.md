# Screamer Alert - Architecture and Workflow (Current)

This is the canonical reference for the Screamer Alert DLL project.

Scope:
- Source folder: _DLL-Projects/DLL_NoEAC-ScreamerAlert
- Current shipped mod version: AGF-NoEAC-ScreamerAlert-v2.1.2
- Last updated: 2026-07-01

Recent update (2026-07-01):
- ESC Screamer buttons now use server-authoritative mode request/ack networking instead of local-only mode writes.
- Added net packages:
   - NetPackageScreamerAlertModeRequest.cs
   - NetPackageScreamerAlertModeAck.cs
- ESC option query now refreshes current mode from server on open.
- COUNT button visibility is now capability-gated via `opt_row_1_num_visible` binding and only appears when EnhancedAGF capability is available.
- ESC option button presses now apply immediate local selected-state feedback before authoritative reconciliation, so selection changes instantly without close/reopen.

## 1. What Is Current Right Now

1. The server is authoritative for tracked screamer/horde state.
2. Baseline players receive server whispers when incidents are in range.
3. Enhanced-capable players render alerts in UI (not via fallback whisper stream).
4. Modes are tri-state per player:
   - Off
   - On
   - On + # (count mode)
5. Count mode falls back to On for non-enhanced clients.

## 2. Runtime Architecture

1. Startup and wiring
   - ModAPI.cs applies Harmony patches and registers chat hook.
   - ModAPI.cs ensures ScreamerAlertManager and ScreamerAlertsController exist as persistent objects.

2. Server tracking loop
   - ScreamerAlertManager.cs server scan interval: 0.5s.
   - Scout screamers are rebuilt from world entities each scan.
   - Horde zombie IDs are patch-managed and dead/missing entries are cleaned.
   - Net sync send interval: 0.5s to enhanced-capable clients.

3. Client capability routing
   - NetPackageScreamerAlertClientHello.cs marks capability on server.
   - ScreamerAlertHybridRouting.cs tracks capability by entity/user references.
   - Capability contract constants are defined in AgfCapabilityContract.cs.

4. Baseline whisper behavior
   - Trigger range: 120m.
   - Incident merge window: 3s.
   - Incident cooldown: 7s.
   - Chat message text is stamped using localization templates:
     - ScreamerAlert_Scout_ChatStamped
     - ScreamerAlert_Horde_ChatStamped

5. UI behavior
   - XUiC_ScreamerAlerts.cs publishes bindings:
     - {screameralert}
     - {screamerhordealert}
     - {screameralertsvisible}
   - Refresh cadence is throttled to 0.2s.
   - XUiC_ScreamerAlertOptions.cs now requests authoritative mode from server and applies ack-synced selected state.

## 3. Mode Storage and Resolution

1. Storage file
   - Saved in current save folder as:
     - ScreamerAlert.playerModes.tsv

2. Defaults and per-player values
   - __default__ row stores server default.
   - Per-player keys are persisted from player identity (with entity fallback).

3. Resolution rule
   - If a target is non-enhanced and requested mode is On + #, effective mode becomes On.

## 4. Command Surface (Current)

1. Player chat commands
   - /agf-sa
   - /agf-sa on
   - /agf-sa off
   - /agf-sa count (aliases: count/counts/numbers)

2. Admin console commands
   - agf-sa
   - agf-sa default <off|on|count>
   - agf-sa set <entityId|all> <off|on|count|default>
   - agf-sa list

3. Command docs split
   - General command guide: COMMANDS-README.md
   - Full scripted response matrix: TEMP-ScreamerAlert-CommandResponses.txt

## 5. UI Integration Files

1. Screamer alert rect patch in shipped mod
   - 02_ActiveBuild/AGF-NoEAC-ScreamerAlert-v2.1.2/Config/XUi_InGame/windows.xml

2. ESC options controller integration source
   - XUiC_ScreamerAlertOptions.cs in this DLL project.
   - Controller name expected by UI:
     - ScreamerAlertOptions, ScreamerAlert
   - Expected button IDs:
     - btnScreamerOff
     - btnScreamerOn
     - btnScreamerNum

3. ESC generator linkage
   - ESC windows generation source that should preserve this wiring:
     - 01_Draft/AGF-4Modders-ESCWindowPlus-v0.3.2/_Generator/Code/SCRIPT-GenerateESCMenu.py

Note:
- There is no dedicated ScreamerAlert content generator in this DLL folder.
- The generator coupling is for ESC window wiring in the ESCWindowPlus project.

## 6. Clean Workflow (Build, Deploy, Verify)

1. Edit source
   - Update C# files in _DLL-Projects/DLL_NoEAC-ScreamerAlert.
   - Update command docs when behavior text changes.

2. Build DLL from source folder
```powershell
& mcs -target:library -out:ScreamerAlert.dll -recurse:*.cs `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\UnityEngine.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\UnityEngine.CoreModule.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\0_TFP_Harmony\0Harmony.dll" `
  -reference:"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\netstandard.dll"
```

3. Deploy DLL for testing
   - Live game target:
     - C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-ScreamerAlert-v2.1.2/ScreamerAlert.dll
   - Dedicated server target:
     - C:/Program Files (x86)/Steam/steamapps/common/7 Days to Die Dedicated Server/Mods/AGF-NoEAC-ScreamerAlert-v2.1.2/ScreamerAlert.dll

4. Lane policy
   - Do not manually modify 02_ActiveBuild or 03_ReleaseSource unless explicitly requested for that operation.

5. Verify behavior
   - Player: /agf-sa, /agf-sa on, /agf-sa off, /agf-sa count.
   - Admin: agf-sa default/set/list.
   - Confirm fallback: non-enhanced COUNT requests resolve to ON.
   - Confirm no XUi or net package errors in log.

## 7. Known Gotchas

1. ScreamerAlertUI.xml in this folder is a placeholder/stub and not the runtime source of truth.
2. When localization templates include commas, keep fields properly quoted in CSV.
3. Keep horde/scout token parsing order correct in any downstream chat classifier logic.
4. Do not rely on authoritative ack timing alone for in-menu visual selection updates; keep immediate UI selected-state application on button press.

## 8. Source-of-Truth File Map

1. Core runtime
   - ModAPI.cs
   - ScreamerAlertManager.cs
   - ScreamerAlertsController.cs
   - ScreamerAlertHybridRouting.cs

2. Commands and mode state
   - ChatCmdScreamerAlert.cs
   - ConsoleCmdScreamerAlert.cs
   - ScreamerAlertModeSettings.cs

3. Network
   - NetPackageScreamerAlertClientHello.cs
   - NetPackageScreamerAlertSync.cs

4. UI controllers
   - XUiC_ScreamerAlerts.cs
   - XUiC_ScreamerAlertOptions.cs
