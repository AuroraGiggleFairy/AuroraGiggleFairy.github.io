# ESCWindowPlus Generator Workflow

## Working Methods
- Primary generator script: _Generator/Code/SCRIPT-GenerateESCMenu.py
- Non-technical run path: _Generator/3-GenerateESCWindow.bat (uses --easy)
- v3.0 defaults expected by script:
	- Localization target: Config/Localization.csv
	- XUi windows target: Config/XUi_InGame/windows.xml
	- XUi template target: Config/XUi_InGame/templates.xml
- Legacy compatibility: if templates.xml is missing and default path is used, generator falls back to Config/XUi_InGame/xui.xml.
- Localization generation/merge writes UTF-8 CSV and keeps language columns quoted.

## Change History
- 2026-07-01:
	- Updated generator defaults from old paths to v3.0 paths (XUi_InGame + Localization.csv).
	- Added KeepLoaded column support to generated full-header CSV output.
	- Added KeepLoaded to merge bootstrap header when creating a new localization file.
	- Added templates.xml default with legacy xui.xml fallback behavior.
	- Updated _Generator/Code/README-ESCMenu-Generator.md path examples to match v3.0 defaults.

## Do-Not-Do Notes
- Do not reintroduce Config/XUi defaults for windows/xui targets in this generator.
- Do not emit Localization.txt as default output for v3.0 workflows.
- Do not remove KeepLoaded from generated localization headers.
- Do not hard-fail on templates path when legacy xui.xml exists and default xui path was used.
