# AGENTS Guide for 7D2D-Mods

This file defines default expectations for AI coding agents in this repository.

## Scope and Safety
- Do not modify 02_ActiveBuild or 03_ReleaseSource unless explicitly requested.
- Prefer draft lane work and controlled sync actions.
- For 7D2D deploy work, default to live game Mods paths.

## Preferred Communication
- Keep updates concise.
- Use implemented best effort confirmations.
- Do not claim ready to test in game unless a live game mod write occurred in the same run.

## Purple Book Generator Rules
- Source of truth is:
  - _DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py
- Validate with no-sync runs first.
- Sync live game mod only when intended.

## Purple Icon Technical Rule
- Schematics opener remains iconbutton.
- Use iconbutton color keys: color_default, color_hovered, color_selected, color_disabled.
- Use RGBA values for color keys.

## Unlock Review Technical Rules
- Non-magazine recipe intent.
- Schematic/book source gating for non-ammo unlocks.
- Exclude gas, thrown, and dart ammo candidates.
- Keep rocket ammo included.
