# AudioOptionsPlus UI Workflow

## Working Methods
- Keep x2.6 files as read-only reference only:
  - `_x2.6/AGF-NoEAC-AudioOptionsPlus-v1.0.1/...`
- Apply active XML UI edits in live mod folder only:
  - `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-AudioOptionsPlus-v1.0.1/Config/XUi_Menu/windows.xml`
- Use the AutoRun mod patch pattern as the vanilla-flow reference for v3 menu patch paths:
  - `Mods/AGF-NoEAC-AutoRun-v2.1.1/Config/XUi_Menu/windows.xml`
- For optionsAudio v3 compatibility, patch under:
  - `/windows/window[@name='optionsAudio']/rect[@name='tabs']/rect[@name='content']/rect[@name='tabsContents']`
- Keep Custom Volumes as one scrollable single-column list and include both volume profile controls and sound swap controls in the same grid.
- Build and deploy DLL from project root output:
  - Build: `dotnet build -c Release` in `_DLL-Projects/DLL_NoEAC-AudioOptionsPlus`
  - Deploy: copy `AudioOptionsPlus.dll` to live mod root and verify SHA256 hash match.
- Validation after each structural XML change:
  - Parse check with PowerShell XML cast wrapped in a root node.
  - Quick rect-tag sanity check (open/close totals and no malformed closing tags).

## Change History
- 2026-07-15:
  - Merged old two-page AudioOptionsPlus content into one single-column list in live mod `windows.xml`.
  - Updated live localization tab label to `Custom Volumes` in live `Localization.csv`.
  - Corrected targeting mistake by restoring x2.6 reference files after accidental edits.
  - Built `AudioOptionsPlus.dll` (Release) and deployed to live mod folder with hash verification.
  - Aligned `Custom Volumes` row sizing pattern to vanilla-style width behavior (removed per-row custom widths and preset gap).
  - Removed `Silly Sounds` enable UI entry from the menu.
  - Moved animal swap controls to the existing `Audio -> Game` list insertion point after `ProfanityFilter` using vanilla-flow insert pattern.
  - Updated animal swap combo values to `Default/Swapped` display values to avoid raw localization-key text in the UI.
  - Extended `XUiC_AudioOptions` to own moved animal swap controls and preset propagation so preset changes update individual animal rows again.
  - Added options-dialog UI session snapshot/restore with save hook so unsaved AudioOptionsPlus changes are reverted on close unless the options dialog save path is used.
  - Added explicit invokeValueChanged signaling from custom option handlers so vanilla dirty-state UI is triggered (asterisk, Apply enablement, and unsaved-changes prompt).
  - Verified exact vanilla hook points in `XUiC_OptionsDialogBase` and switched dirty signaling to call dialog `SetChanged()` directly from custom handlers for reliable Apply/unsaved/asterisk behavior.
  - Determined exact vanilla flow from decompiled classes (`XUiC_OptionsDialogBase` + `XUiC_OptionEntryAbs`): unsaved/apply state is driven by option-entry `IsChanged` checks in `updateUnsavedState()`, not by custom controller values.
  - Added explicit pending-change detection for AudioOptionsPlus UI session and hooked `XUiC_OptionsDialogBase.Update` to call `SetChanged()` when pending changes exist, so Apply/unsaved indicator follows the same dialog-level lifecycle even for non-OptionEntry custom controls.
  - Fixed UI session snapshot typing (float/int/string) so restore/save baseline uses correct PlayerPrefs value types instead of string-only snapshots.
  - Moved AudioOptionsPlus UI session start/end from custom tab controller open/close to `XUiC_OptionsDialogBase` open/close for `XUiC_OptionsAudio` so tab switches cannot reset pending-change baseline mid-session.
  - Added `resetToDefaults` integration for `XUiC_OptionsAudio` to reset AudioOptionsPlus values and reload open custom controllers, then mark dialog changed to keep default-reset behavior aligned with vanilla options flow.
  - Migrated all AOP UI rows from `options_combo` to `options_combo_custom` so controls use `XUiC_OptionEntryCustom` lifecycle instead of `XUiC_OptionEntryLegacy` (which never reports `IsChanged`).
  - Added `XUiC_OptionsAudio.Init` delegate wiring for all AOP custom rows (`GetSettingValue`, `DiscardChanges`, `ApplyChanges`, `ResetDefaults`, `IsChangedDelegate`, `IsDefaultDelegate`) to follow vanilla custom-option behavior.
  - Refactored `XUiC_AudioOptions` to stop immediate persistence on value changes and instead provide apply/reset/pending helpers used by OptionEntryCustom delegates.
  - Added fallback delegate routing through `XUiC_OptionsAudio` so AOP apply/reset/changed/default checks can resolve the AOP controller even if `LiveControllers` is empty.
  - Hardened OptionEntryCustom binding by rebinding AOP delegates on both `XUiC_OptionsAudio.Init` and `XUiC_OptionsAudio.OnOpen` and expanding lookup across window hierarchy (`Option<name>` and `<name>` IDs).
  - Added one-shot runtime diagnostics (`Console.WriteLine`) for each bind success/failure so game log can confirm exactly which option entries are bound at open time.
  - Added deterministic fallback lifecycle hooks that do not depend on delegate bind timing:
    - `XUiC_OptionsDialogBase.saveChanges` now force-applies AOP controller values once per save.
    - `XUiC_OptionsDialogBase.discardChanges` now force-reloads AOP controller UI from config.
    - `XUiC_OptionEntryCustom.SelectionChanged` now forces `SetChanged()` for `OptionAOP*` rows in `XUiC_OptionsAudio`.
  - Added evidence-first runtime tracing across vanilla lifecycle methods (`SelectionChanged`, `Event_SettingChanged`, `updateUnsavedState`, `saveChanges`, `discardChanges`, `resetToDefaults`, dialog open/close) for AOP rows to identify exact misswire point from log output.
  - Identified root blocker from live log: Harmony init abort was caused by stale patch target signature for `Audio.Manager.Play(Vector3, string, int, bool)`; current v3 overload is `Play(Vector3, string, int, bool, float)`.
  - Fixed `Patch_Audio_Manager_Play_Position` Harmony target signature to include trailing `float` parameter so AudioOptionsPlus patch registration can complete.
  - Reduced runtime overhead after successful diagnostics by gating all AOPTRACE and bind diagnostic logging behind a disabled-by-default flag in `AudioOptionsPlusUiTrace`.

## Do-Not-Do Notes
- Do not edit `_x2.6` reference files when implementing live gameplay/menu changes.
- Do not assume old v2.6 `tabsHeader/tabsContents` path works for v3 without checking vanilla v3 menu structure.
- Do not split Custom Volumes back into separate pages unless explicitly requested.
- Do not make OptionEntryCustom delegates depend only on `LiveControllers`; that creates silent no-op behavior when controller registration timing does not match option-entry lifecycle.
- Do not assume `Init`-only delegate binding is sufficient for injected options; rebind at `OnOpen` to survive lifecycle/order differences.
- Repeated failure pattern noted: delegate-wiring hardening alone did not restore runtime behavior; maintain deterministic dialog-lifecycle fallbacks until runtime evidence proves all OptionEntryCustom bindings fire reliably.
- Do not keep static Harmony signatures for `Audio.Manager.Play` without verifying decompiled overloads for the target game version first; a stale overload signature aborts mod init and masks all downstream UI diagnostics.
