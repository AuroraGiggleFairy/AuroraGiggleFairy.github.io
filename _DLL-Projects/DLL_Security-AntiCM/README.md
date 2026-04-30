# AntiCM

## Changes Made To Original ForceDisable Source

This project includes a hardened rewrite of the originally provided ForceDisable script. The intent stayed the same (detect CM/DM use and enforce server rules), but implementation details were improved for reliability, safety, and maintainability.

- Execution model changed from a scene MonoBehaviour loop to a Harmony hook on `GameManager.Update` so monitoring reliably runs server-side.
- Logic was refactored into a static monitor with clear helper methods (`Initialize`, `Tick`, `LoadConfig`, `TryKick`, etc.).
- Runtime configuration was added via `Mods/AntiCM/AntiCMConfig.txt` with auto-create defaults and periodic reload.
- Hardcoded settings moved into config values (kick toggle, Discord toggle/webhook, scan interval, whitelist IDs).
- Runtime folder/log paths were standardized to `Mods/AntiCM` and `CMDebugLog.txt`.
- Reflection handling was hardened with one-time cached lookups and safer `KickPlayer` overload resolution.
- Performance guardrails were added: scan throttling and per-player action cooldown.
- Command detection matching was tightened (`cm`, `/cm`, `dm`, `/dm`, and `debug` variants).
- Command buffer clearing was removed to avoid mutating shared player command history.
- Startup/config failure logging was improved to simplify server troubleshooting.
- Discord payload text was normalized to ASCII-safe content.

AntiCM is a server-side 7 Days to Die DLL mod focused on enforcement against unauthorized Creative Mode (CM) and Debug Mode (DM) use. It is designed to be lightweight, configurable at runtime, and practical for live servers where administrators want automatic detection, logging, and optional removal of offenders.

## Purpose And Design Goals

This mod exists to enforce server rules around CM/DM access in a way that is:

- Reliable: catches both mode flags and command-based attempts.
- Fast: reflection lookups are cached and scans are throttled.
- Operationally friendly: behavior can be changed via config without recompiling.
- Auditable: writes detections to a persistent log file.

## What It Detects

AntiCM checks each active player for three signal types:

1. Creative flag enabled (`bCreativeMode`).
2. Debug flag enabled (`DebugMode`).
3. Recent command buffer entries indicating CM/DM usage.
   - Exact or prefix checks for `cm`, `/cm`, `dm`, `/dm`.
   - Generic check for command text containing `debug`.

Any non-whitelisted player matching one or more signals is treated as a detection event.

## Enforcement Behavior

On detection, AntiCM performs the following actions in order:

1. Forces relevant flags back to `false` when found.
2. Writes a timestamped record to `Mods/AntiCM/CMDebugLog.txt`.
3. Optionally sends a Discord webhook alert.
4. Optionally kicks the player via `GameManager.KickPlayer`.

Whitelisted Steam IDs are always skipped.

## Runtime Architecture

### Harmony Hook

The mod attaches to `GameManager.Update` using Harmony:

- Patch target: `GameManager.Update`
- Patch type: `Postfix`
- Entry path: `ForceDisableMonitor.Tick()`

Why this hook:

- `GameManager.Update` runs continuously on the server, giving predictable recurring execution.
- A postfix hook avoids interfering with base game logic and reduces compatibility risk.
- It keeps the monitoring system decoupled from game object injection patterns.

### Reflection Targets

To remain resilient when direct APIs are unavailable or private/internal, the mod uses reflection for:

- Player fields:
  - `bCreativeMode`
  - `DebugMode`
  - `m_clientInfo`
  - `m_lastCommands`
- ClientInfo field:
  - `playerId` (Steam ID extraction)
- GameManager method:
  - `KickPlayer` overload with `(clientInfo, string reason)`

Why reflection is used:

- Several target members are not publicly exposed in a stable API surface.
- It allows server-side enforcement without requiring custom game-side interface changes.

### Performance Guardrails

The monitor includes several controls to keep overhead low:

- One-time reflection caching during initialization.
- Configurable scan interval (`scanIntervalSeconds`) to avoid per-frame full checks.
- Per-player action cooldown to prevent repeated spam actions in rapid succession.
- Config reload timer (every 10 seconds) so config changes apply without restart/rebuild.

## Configuration

Primary config file:

- `Mods/AntiCM/AntiCMConfig.txt`

If missing, AntiCM creates it automatically with defaults.

Supported keys:

- `kickOnDetection=true|false`
- `enableDiscordAlerts=true|false`
- `discordWebhookUrl=`
- `scanIntervalSeconds=0.25`
- `allowedSteamIds=id1,id2,id3`

Notes:

- `allowedSteamIds` is comma-separated.
- Discord alerts require both `enableDiscordAlerts=true` and a valid `discordWebhookUrl`.
- Lower scan intervals increase responsiveness but cost more CPU.

## Files Created At Runtime

Under the server/game root `Mods/AntiCM` folder, the mod may create:

- `AntiCMConfig.txt` (default config, if absent)
- `CMDebugLog.txt` (detection log)

## Build Inputs And References

Source files needed to build:

- `AntiCM.csproj`
- `ModAPI.cs`
- `ForceDisable- Source code.cs`
- `HarmonyPatches.cs`
- `ModInfo.xml`

External dependency DLLs used by the project:

- `0Harmony.dll`
- `Assembly-CSharp.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.IMGUIModule.dll`
- `UnityEngine.UI.dll`

These are required because the mod compiles against game and Harmony types such as:

- `GameManager`, `EntityPlayer`, `IModApi`
- `MonoBehaviour`, `Time`, `Debug`
- Harmony patch attributes and patching runtime

## Build And Packaging

Build command:

```powershell
dotnet build .\AntiCM\AntiCM.csproj -c Release
```

Project packaging target behavior:

- After a successful build, the project creates a deployable folder:
  - `AntiCM/Release/AntiCM`
- It copies:
  - `AntiCM.dll`
  - `ModInfo.xml`
  - `AntiCMConfig.txt` (if present)

## Server Deployment

Copy the packaged folder into server `Mods` so layout is:

- `ServerRoot/Mods/AntiCM/AntiCM.dll`
- `ServerRoot/Mods/AntiCM/ModInfo.xml`
- `ServerRoot/Mods/AntiCM/AntiCMConfig.txt`

Then edit config values as needed and restart/reload server.

## Why It Is Set Up This Way

Key implementation choices and rationale:

- Harmony `GameManager.Update` hook:
  - Simple recurring execution point with broad compatibility.
- Reflection over direct member access:
  - Needed for private/internal fields in 7DTD internals.
- Config in `Mods/AntiCM`:
  - Keeps runtime state co-located with deployed mod and server-friendly.
- Auto-created config:
  - Reduces first-run friction and missing-file failure cases.
- Detection + reaction model:
  - CM/DM flags are authoritative indicators even if no command text is present.

## Limitations And Operational Notes

- Detection is reactive, not preventative.
  - The player may briefly have CM/DM enabled before the next scan catches it.
- Reflection target names can change between game versions.
  - If internals are renamed by an update, fields/method resolution may need adjustment.
- Very broad command matching (`contains("debug")`) can intentionally favor strict enforcement over minimal false positives.

## Troubleshooting

If build fails:

1. Verify all referenced external DLL paths in `AntiCM.csproj` exist on the build machine.
2. Confirm .NET SDK availability for `net48` build.

If mod loads but does not enforce:

1. Check server log for `[AntiCM] Initialized...` line.
2. Confirm `ModInfo.xml` and `AntiCM.dll` are in the same deployed mod folder.
3. Verify whitelist and toggles in `AntiCMConfig.txt`.
4. Confirm target game version did not rename reflected members.
