# Purple Book Generator Handoff (2026-05-28)

## Status Update (2026-05-29)
- Magazines, Books, Unlocks, and Armors are essentially generating as intended in current generator output.
- These core areas are now considered stable enough to move forward.
- Next focus: other Purple Book aspects and remaining polish/feature passes.

## Overall Goal
- Generator can recreate Purple Book from vanilla files.
- Generator will be able to recreate from mod files to produce compatibility patches.

## Three Goals (Current Contract)
1. Complete and stabilize vanilla-file generation first.
2. Preserve existing working behavior while iterating (no regressions/lost work).
3. Push every successful generation to the game mod path for immediate in-game testing.

## Current Focus
- Phase: Vanilla-first generator completion.
- Priority: Correct vanilla data, layout, order, grouping, naming, and visual behavior.
- Compatibility patch generation is Phase 2 after vanilla generation is stable.

## Latest State Snapshot (2026-05-28)
- Shared Overview button behavior exists across Magazines, Books, Unlocks, and Armors.
- Current Overview button settings in generator normalizer:
  - `pos="168,33"`
  - `width="120"`
  - `height="29"`
  - `font_size="17"`
  - `defaultcolor="130, 88, 48"`
  - `bordercolor="[black]"`
  - `selectedcolor="74, 33, 150"`
  - `tooltip_key="agfAllOverviewTooltip"`
- Localization keys for Overview semantics are in active use:
  - `agfOverview` = `Overview`
  - `agfAllOverviewTooltip` = `Return to full page overview`
- Current behavior note: this button type uses one `bordercolor` for both selected and unselected states.

## Next Chat Priorities
1. Localization fixes.
   - Identify any missing/incorrect strings in generated `Localization.txt`.
   - Preserve existing user/localized rows while applying generator-side fixes.
   - Re-verify `agfOverview` and `agfAllOverviewTooltip` output after each run.
2. Zoomed-in Armors tab polish.
   - Tune zoomed-in armor sub-tab row fit/spacing/alignment.
   - Keep tab readability while preserving current cross-page Overview behavior.
   - Prevent regressions to Magazines, Books, and Unlocks while adjusting Armors.

## Active Workstream (Right Now)
- While working on Armors, double-check preservation of Magazines, Books, and Unlocks on every generation cycle.
- Armor card work uses two references together:
  - Handmade Purple Book output as the visual/structure baseline.
  - Example biker card as the armor-card pattern reference.
- Scope discipline: armor-card changes must not break existing Magazines/Books/Unlocks behavior.

## Source of Truth Paths
- Workspace root:
  - C:/GitHub/7D2D-Mods
- Real generator source (edit this file):
  - 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py
- Wrapper launcher (do not use for layout logic edits):
  - SCRIPT-PurpleBookGenerator.py
- Generated draft outputs:
  - 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/XUi/windows.xml
  - 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/XUi/xui.xml
  - 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/Localization.txt
- Live game mod outputs:
  - C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config/XUi/windows.xml
  - C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config/XUi/xui.xml
  - C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config/Localization.txt

## Mandatory Iteration Rules (Every Change)
1. Make the change only in the real generator source.
2. Run generator.
3. Confirm generation succeeded with no errors.
4. Confirm outputs were copied/pushed to live game mod path.
5. Validate resulting windows.xml structure for the targeted change.
6. Validate preservation of Magazines, Books, and Unlocks while Armors work is in progress.
7. Launch/test in game immediately.
8. If wrong, adjust generator and repeat this full loop.

## Standard Run Command
- c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-PurpleBookGenerator.py

## Guardrails
- Do not skip game push/test after generation.
- Do not do broad refactors and layout rewrites in the same pass.
- Keep each pass focused to one concern (data, structure, spacing, text alignment, or ordering).
- Preserve user localization edits (especially existing keys/rows) unless explicitly told otherwise.
- Keep backup safeguards on unless explicitly asked to disable for a run.
- During Armors work, preservation checks for Magazines/Books/Unlocks are mandatory, not optional.
- Communication rule: only say "ready to test in game" when the live game mod folder files were actually changed in that run.

## Definition of Done (Per Session)
- Generator run succeeds.
- Draft outputs are updated.
- Live mod outputs are updated.
- In-game test completed for the targeted area.
- No regression found in previously working Purple Book behavior.

## Response Rule
- Do not claim "ready to test in game" unless there was an actual write/update to live mod output files in:
  - C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config

## Quick Resume Checklist For New Chat
1. Read this file first.
2. Confirm current target tab/feature.
3. Edit only real generator source.
4. Run/generate/push/test loop until correct.
5. Report outcome briefly and keep momentum.
