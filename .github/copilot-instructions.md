# Copilot Instructions for 7D2D-Mods

Cursor-era source of truth (always-on): `.cursor/rules/repo-map-and-ownership.mdc`
7dtdmods first listings: `.cursor/rules/7dtdmods-listing-fill.mdc`.

This file used to be the Visual Studio / GitHub Copilot always-on brief. Cursor does not auto-load it. Keep both aligned when rules change.

## Communication
- Keep responses concise.
- Confirm changes as implemented best effort.
- Do not claim ready to test in game unless live game Mods files were actually updated in that run.

## Lanes and deploy
- New / `0.x.x` work stays in `01_Draft`. Wait for `RUN-MakeNewMod.bat`; do not invent mod folders.
- Public/release tools (not game mods) live in `00_Tools` (ModSync, LogReader). Do not put them in Draft/ActiveBuild or in `00_DLL-Projects/Generators`.
- Do not modify `02_ActiveBuild` or `03_ReleaseSource` unless explicitly requested.
- Default test deploy is the live game Mods path only:
  - `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods`
- For DLL deployment testing, also:
  - `C:/Program Files (x86)/Steam/steamapps/common/7 Days to Die Dedicated Server/Mods`
- Do not promote Draft → ActiveBuild yourself. User runs `RUN-Update.bat` for official lane sync.
- Do not edit ModInfo/README unless the user explicitly asks. Scope metadata goes in `05_ReleaseData/ReadmeSystem/HELPER_ModCompatibility.csv`.
- New Config xpath XML root: `AGF` plus the last hyphen segment of ModInfo Name (drop `zzz` and middle tokens). Reuse an existing mod root if that mod already has Config XML. See `.cursor/rules/mod-xml-root-name.mdc`.

## Evidence
- Do not guess. Read vanilla `Data/Config` (and decompile if DLL), then this repo.
- If a claim cannot be verified from source, label it Unverified.

## README wording (when asked)
- Follow `05_ReleaseData/ReadmeSystem/WORKFLOW-AI-README-Review.md`.
- Work one mod and one section at a time.
- Default output: Suggestion 1, Suggestion 2, Recommended (short reason).
- Discussion-only until the user asks to implement.

## Purple Book generator
- Logic only: `00_DLL-Projects/Generators/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py`
- Default: `--no-sync-game-mod --no-sync-activebuild`
- Live game sync only when explicitly asked.
- Schematics opener stays iconbutton; tint keys `color_default`, `color_hovered`, `color_selected`, `color_disabled`.
- `color(...)` requires RGBA (4 channels).
- Unlock review: non-magazine; schematic/book only; exclude gas, thrown, dart; keep rocket ammo.
