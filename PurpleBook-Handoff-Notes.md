# Purple Book Generator Handoff (2026-05-28)

## Status Update (2026-06-28, generator now manages gameevents trigger)
- Change history:
  - Added generator-managed output for `Config/gameevents.xml` in `SCRIPT-PurpleBookGenerator.py`.
  - Added spawn hook patch generation that appends `AddBuff` for `agfRecalculate` on `game_on_spawn`.
  - Added sync handling so `gameevents.xml` is copied when `--sync-activebuild` and/or `--sync-game-mod` are enabled.
- Working method used:
  - Source edit only: `01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py`.
  - Validation run (no lane/game sync):
    - `c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild`
  - Validation result included:
    - `[ok] wrote .../Config/gameevents.xml`
    - Generated content includes `buff_name="agfRecalculate"` under `game_on_spawn`.

## Status Update (2026-06-28, draft AGF-HUDPlus-PurpleBook refresh)
- Change history:
  - Re-generated latest Purple Book outputs from generator source using no-sync run.
  - Copied updated files into draft target mod folder `01_Draft/AGF-HUDPlus-PurpleBook-v2.0.1`.
  - Included files:
    - `Config/XUi_InGame/windows.xml`
    - `Config/XUi_InGame/xui.xml`
    - `Config/Localization.csv`
  - Verified source/target file hash matches for all three files.
  - Localization mojibake validation on target draft file: `MOJIBAKE_MATCHES: 0`.
- Working method used:
  - Generate latest without lane/game sync:
    - `c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild`
  - Copy generated files from:
    - `01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/...`
  - Into draft target:
    - `01_Draft/AGF-HUDPlus-PurpleBook-v2.0.1/Config/...`

## Status Update (2026-06-28, live game sync for test)
- Change history:
  - Deployed latest Purple Book generator outputs to live game mod path for in-game validation.
  - Follow-up fix: remapped `armorScavengerOutfit` header icon to medium class (`ui_game_symbol_light_armor2`) and re-synced live files.
- Working method used:
  - Run with game sync only:
    - `c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py --sync-game-mod --no-sync-activebuild`
  - Synced files:
    - `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config/XUi_InGame/windows.xml`
    - `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config/XUi_InGame/xui.xml`
    - `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config/Localization.csv`

## Status Update (2026-06-27, armor type icon mismatch)
- Change history:
  - Fixed armor header icon mismatch where overview cards were restored from a known-good block that still had medium icon sprites for every set.
  - Added explicit post-merge icon normalization in generator so both overview (`allArmors`) and zoom tabs use per-outfit class icons consistently.
  - Updated primitive outfit to use the light armor icon so it aligns with current armor rating presentation.
- Working method used:
  - Edit only: `01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py`.
  - Validation run (no lane/game sync):
    - `c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild`
  - Verification check: inspect `windows.xml` iconArmor sprites by tab/grid and confirm overview + zoom parity per set.
  - Localization mojibake check after generation: `MOJIBAKE_MATCHES: 0`.
- Do-not-do note:
  - Do not rely on restored overview snapshots to preserve current icon semantics; always re-apply armor icon mapping after any overview replacement/merge stage.

## Status Update (2026-06-28, set-bonus description v3.0 accuracy)
- Change history:
  - Audited generated armor set-bonus descriptions against v3.0 vanilla set-bonus intent.
  - Found two accuracy gaps in generated English copy:
    - Lumberjack set-bonus description omitted the wood-harvest effect.
    - Preacher set-bonus description omitted the infection-resist effect.
  - Added targeted generator-side English overrides in set-bonus desc extraction so these two rows stay accurate on future runs.
- Working method used:
  - Edit only: `01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py`.
  - Validation run (no lane/game sync):
    - `c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild`
  - Verified generated rows:
    - `agf5ArmorLumberjackSetBonusDesc`
    - `agf5ArmorPreacherSetBonusDesc`
  - Mojibake signature check after generation: `0`.
- Do-not-do note:
  - Do not assume armorOutfitDesc set-bonus paragraphs fully cover dual-effect bonuses; validate against buff set-bonus intent before shipping wording.

## Status Update (2026-06-27)
- Change history: Fixed Primitive armor set-bonus fallback mojibake in generator localization output.
- Working method used:
  - Edit only: `01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py`.
  - Validation run (no lane/game sync):
    - `c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild`
  - Verify output row: `agf5ArmorPrimitiveSetBonusDesc` in generated `Config/Localization.csv`.
- Do-not-do note:
  - Do not paste raw multilingual fallback literals directly into Python source when encoding can drift.
  - Use ASCII-safe unicode escapes (or source-derived text) for hardcoded multilingual fallbacks.

## Status Update (2026-06-27, localization quality pass)
- Change history:
  - Replaced placeholder `[language] ...` output for generator-authored localization keys with curated translations for:
    - `agf4UnlocksCategoryArmorsHelmet`
    - `agf4UnlocksCategoryArmorsPlating`
    - `agf4UnlocksCategoryDrone`
    - `agf4UnlocksCategoryToolsWeaponsClub`
    - `agf4UnlocksCategoryToolsWeaponsMotorTool`
    - `agf4UnlocksCategoryToolsWeaponsOtherGun`
    - `agf4UnlocksCategoryToolsWeaponsOtherMelee`
    - `agf4UnlocksCategoryToolsWeaponsShotgun`
    - `agf5ArmorSetBonusTooltip`
  - Added CSV style normalization so generated `Localization.csv` writes:
    - plain header fields
    - forced quoting for language columns
    - non-language columns quoted only when CSV escaping is required
- Validation:
  - Draft no-sync generator run succeeded.
  - Placeholder rows check after generation: `0`.
  - Mojibake signature check (known corruption sequences) after generation: `0`.
- Do-not-do note:
  - Do not use synthetic placeholder auto-translation output in shipped localization rows.

## Status Update (2026-06-27, untranslated UI headers/categories)
- Change history:
  - Added curated translations in generator map for previously blank UI/header keys used by Purple Book tabs and unlock categories, including:
    - main tabs (`agf1MainTab*`), header hint, and overview tooltip
    - unlock category headers and ammo subgroup rows (`agf4UnlocksCategory*`)
    - armor rating group labels (`agf5ArmorsRatingHeavy/Light/Medium`)
    - button tooltip key (`agf0PurpleBookButtonTooltip`)
  - Updated merge behavior so these generator-managed UI keys overwrite stale blank rows during localization merge (`force_generated_keys`).
  - Updated localization preservation validator to allow those same managed keys (`allowed_changes`) so valid generator updates are not blocked.
- Working method used:
  - Edit only: `01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py`.
  - Validation run (no lane/game sync):
    - `c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe c:/GitHub/7D2D-Mods/01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild`
  - Validation checks after generation:
    - target translated UI keys found: `33`
    - target keys with missing language columns: `0`
    - mostly untranslated rows: `0`
    - mojibake signature matches: `0`
- Do-not-do note:
  - Do not add new generator-managed localization keys without updating both merge override allowlist and preservation allowlist, or writes will be blocked/ignored.

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
