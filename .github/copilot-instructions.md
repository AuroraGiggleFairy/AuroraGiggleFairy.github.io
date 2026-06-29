# Copilot Instructions for 7D2D-Mods

## Communication
- Keep responses concise.
- Confirm changes as implemented best effort.
- Do not claim ready to test in game unless live game mod files were actually updated in that run.

## Lane Safety
- Do not modify 02_ActiveBuild or 03_ReleaseSource unless explicitly requested.
- Default deployment target is the live game Mods path only.
- For DLL deployment testing, include this dedicated server Mods path:
  - C:/Program Files (x86)/Steam/steamapps/common/7 Days to Die Dedicated Server/Mods

## Purple Book Generator Source of Truth
- Edit only this file for Purple Book generator logic:
  - _DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py

## Purple Book Run Workflow
- Use validation runs without lane sync by default:
  - c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild
- Use live game sync only when explicitly asked or clearly intended:
  - c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-PurpleBookGenerator.py --sync-game-mod --no-sync-activebuild

## Purple Book UI Guardrails
- Keep Schematics opener as iconbutton.
- For iconbutton tinting, use iconbutton template keys:
  - color_default, color_hovered, color_selected, color_disabled
- color(...) expressions require RGBA values (4 channels), not RGB.

## Unlock Review Guardrails
- Treat unlock review candidates as non-magazine recipes.
- Include schematic and book sourced unlocks only.
- Ammo filtering rules:
  - Exclude gas, thrown, and dart entries.
  - Keep rocket ammo included.
