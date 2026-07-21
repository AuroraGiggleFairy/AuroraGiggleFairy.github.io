# BMCounter Blood Moon Source Workflow

## Working Methods
- Use decompiled v3 assembly under `00_DLL-Projects/References/Decompiled-DLLs/Decompiled_AssemblyCSharp_7d2dv3` to verify value sources.
- For XML patch `<conditional>` blocks, only these relevant condition functions are available: `gamepref(...)` and `serverinfo(...)` (plus non-data helpers).
- For runtime XUi binding expressions (`{...}` or `{# ...}`), use NCalc functions from `BindingNcalcFunctions`, including `gamestat(...)`, `gamepref(...)`, `serverinfoint(...)`, `serverinfobool(...)`, `serverinfostring(...)`, and `cvar(...)`.
- Prefer `gamestat('BloodMoonDay')` for next blood moon tracking; it reflects runtime scheduling logic.

## Change History
- 2026-07-08: Investigated v3 behavior where gameplay updates after sandbox change but `gamepref` does not refresh without full app restart.
- 2026-07-08: Verified `XmlPatchConditionEvaluator` does not support `gamestat(...)`; only `gamepref(...)`/`serverinfo(...)` are usable in patch conditions.
- 2026-07-08: Verified `AIDirectorBloodMoonComponent` writes next blood moon schedule through `GameStateManager.SetBloodMoonDay(...)` and this persists to `EnumGameStats.BloodMoonDay`.
- 2026-07-08: Verified `XUiC_CompassWindow` uses `GameStats.GetInt(EnumGameStats.BloodMoonDay)` for live blood-moon warning behavior.
- 2026-07-08: Implemented live game mod XML update in `Mods/AGF-HUDPlus-BMCounter-v3.0.0/Config/XUi_InGame/windows.xml` to use runtime `max(0, gamestat('BloodMoonDay') - day)` instead of frequency/range branch blocks.
- 2026-07-08: Updated live game mod XML to show localized `Blood Moon Tonight` (`localization('07daycountdown_0')`) when day difference resolves to zero using a ternary expression.
- 2026-07-08: Updated live game mod XML again to use the reduced key set: `agfNextBloodMoon` and `agfBloodMoonTonight` for zero-day text.

## Do-Not-Do Notes
- Do not rely on `gamepref('BloodMoonFrequency')` as the sole runtime source in v3 for countdown logic.
- Do not expect `serverinfo('BloodMoonFrequency')` to represent effective sandbox runtime value if local server info still mirrors `GamePrefs`.
- Do not try `gamestat(...)` in XML patch `<if cond="...">`; it is not supported there and will fail.
