# Visual Entity Tracker Addon (NoEAC toggle) — working notes

Server-side companion for `AGF-HUDPlus-VisualEntityTracker`.
Not the retired DLL compass scanner (`DLL_NoEAC-VisualEntityTracker`).

## Locked names

| Role | Value |
|---|---|
| Mod folder | `AGF-NoEAC-VisualEntityTrackerAddon` |
| Alternate considered | `AGF-NoEAC-Addon4VisualEntityTracker` |
| Display / chat label | Visual Entity Tracker |
| Chat | `/agfvet` (player-facing, like `/agfsa`) and `/agf-vet` |
| F1 console | `agf-vet` |
| Console prefix | `[VisualEntityTracker]` |
| DLL assembly | `VisualEntityTrackerAddon.dll` |

XML tracker stays `AGF-HUDPlus-VisualEntityTracker`. This addon only turns that tracker on or off per player.

## Current status

- Draft pack: `01_Draft/AGF-NoEAC-VisualEntityTrackerAddon-v0.0.1`
- Command/response spec is in `TEMP-VisualEntityTrackerAddon-CommandResponses.txt`
- Player/admin guide is in `COMMANDS-README.md` and `Commands-Readme.txt`
- Chat/console C# is implemented in this folder; DLL copies into the draft pack on build

## Chat whispers (Screamer Alert parity)

Status / help sends **two whispers**:
1. `[Visual Entity Tracker = ON]` (or OFF)
2. `[Options: /agfvet off, on]`

Screamer Alert also sends a third `[COUNT requires EnhancedAGF]` line. This addon has no COUNT, so it stops at two.

On / off send only the mode whisper, same as shipped Screamer Alert localization.

## Scope for first implementation

- Server-authoritative chat + F1 admin commands
- Persist per-player ON/OFF
- Default ON
- Apply by adding/removing `buffZAlert` (XML VET’s scanner / `agfVet` cvar)
- No DLL entity scan, no compass Harmony, no ESC options UI yet

