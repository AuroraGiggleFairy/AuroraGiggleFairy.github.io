# Purple Book Notes For AI Direction

Use this page to keep AI changes focused.
Answer in plain words. Short answers are fine.
Last updated: 2026-06-27

## 1) What are we fixing right now?
- Current task: Keep Purple Book generator output stable while preserving current Schematics icon behavior and unlock review filtering rules.
- Why this matters in-game: The book UI must stay readable and predictable, and unlock review output must not include bad candidates.
- How it should look when correct: Schematics opener style stays correct, tabs remain stable, and unlock review matches current intent.

## 2) What can change?
- Allowed to change: Purple Book generator logic in source-of-truth script and related generated outputs.
- Not allowed to change: 02_ActiveBuild or 03_ReleaseSource unless explicitly requested.

## 3) What must stay the same?
- Things that must keep working: Magazines, Books, Unlocks, and Armors generation behavior.
- Tabs/features that cannot be touched without explicit approval: Unrelated UI systems outside Purple Book scope.

## 4) Exact rule for this pass
- One-line goal: Make one scoped change at a time while preserving all currently working Purple Book behavior.
- Before edits, AI must state:
	- What it will change
	- What it will not change
- After edits, AI must show:
	- What changed
	- What stayed unchanged

## 5) UI rules (plain language)
- Text must fully fit in buttons: Yes
- No overlap allowed: Yes
- First button may sit outside main panel: Only when explicitly intended by tab design
- Any size/spacing limits: Keep existing stable layout unless the active task is specific UI sizing/spacing work
- Schematics opener must remain: iconbutton
- Iconbutton color keys to use: color_default, color_hovered, color_selected, color_disabled
- Color channel rule: Use RGBA values (4 channels)

## 6) Compatibility safety
- What to re-check every run:
	- No regression in Magazines, Books, Unlocks, Armors
	- Unlock review filtering still matches rules
	- Generated XML structure remains valid
- Known risky files or areas:
	- Generator: 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py
	- Generated UI XML: 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/XUi/windows.xml
	- Unlock review candidate logic and ledger interactions

## 7) Test checklist
- Visual checks:
	- Schematics opener style and state colors are correct
	- No overlap/regression in key tabs
- Functional checks:
	- Generator run succeeds
	- Output files are regenerated
- Regression checks:
	- Core tabs still generate as expected
	- Unlock review excludes gas/thrown/dart ammo and keeps rocket ammo
	- If localization changed, mojibake = 0

## 8) Generate and sync rule
- Must regenerate these files:
	- 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/XUi/windows.xml
	- 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/XUi/xui.xml
	- 01_Draft/AGF-PurpleBookGenerator-v0.0.1/Config/Localization.txt
- Must sync to these paths:
	- Default: validation no-sync run only
	- Live sync path (when explicitly intended):
		- C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-PurpleBookGenerator-v0.0.1/Config
- Standard validation command:
	- c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-PurpleBookGenerator.py --no-sync-game-mod --no-sync-activebuild
- Live sync command (only when requested/intended):
	- c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-PurpleBookGenerator.py --sync-game-mod --no-sync-activebuild
- AI can say ready to test in game only when: live game files were actually updated in that run.

## 9) Decisions log
- Date: 2026-06-27
- Decision: Keep Schematics opener as iconbutton and use iconbutton template color keys.
- Why: This preserves expected button behavior and avoids styling regressions.
- Date: 2026-06-27
- Decision: Enforce RGBA channel usage for color expressions and color values.
- Why: Prevents runtime binding errors caused by 3-channel usage.
- Date: 2026-06-27
- Decision: Keep no-sync validation as default; live sync only when intended.
- Why: Reduces accidental live writes while keeping test sync available when needed.

## 10) What failed before
- Change tried: Non-template color styling for Schematics iconbutton.
- Why it failed: Iconbutton styling is template-driven and requires specific color_* keys.
- How to catch it fast next time: Verify emitted XML uses color_default/color_hovered/color_selected/color_disabled.
- Change tried: 3-channel color expression use.
- Why it failed: Runtime expected RGBA and rejected RGB arg count.
- How to catch it fast next time: Validate all color entries are 4-channel values.

## 11) Repeated break list
- Break item: Schematics color appears wrong despite color edits.
- Trigger: Using generic color fields instead of iconbutton template keys.
- Quick check: Inspect generated windows.xml for iconbutton color_* attributes.
- Break item: Unlock review includes wrong ammo candidates.
- Trigger: Filter drift in candidate selection logic.
- Quick check: Confirm gas/thrown/dart excluded and rocket retained.

## 12) Open questions for user
- Question: What is the next active Purple Book target area (UI, unlock review, localization, or compatibility pass)?
- Assumption: Current stable behavior should be preserved unless explicitly overridden.
- Next check: Confirm target area before next generator pass.
