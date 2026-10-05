# LogReader Knowledge

Living catalog. `SCRIPT-LogReader.py` reads the `---` blocks. When a report lists **New patterns**, add a block here.

Severity:

- `error` — red / game-breaking (exceptions)
- `conflict` — XML patch miss
- `load` — folder did not become a loaded mod
- `noise` — known chatter; not printed in the report

---
id: mods-same-name
match: Mod with same name
kind: WRN
severity: load
meaning: Two folders share the same ModInfo Name. First folder wins; the later one is ignored.
---
id: mods-no-modinfo
match: does not contain a ModInfo.xml
kind: WRN
severity: load
meaning: Folder is not a mod. Nested packs, leftover files, or a zip extracted one level too deep.
---
id: mods-legacy-modinfo
match: ModInfo.xml in legacy format
kind: ERR
severity: load
meaning: Old A17-style ModInfo. Game requires V2. Folder is ignored.
---
id: mods-eac-skip
match: AntiCheat enabled, mod skipped
kind: INF
severity: load
meaning: SkipWithAntiCheat is set and EAC is on. Folder is skipped on purpose.
---
id: mods-eac-dll
match: AntiCheat needs to be disabled to load it
kind: WRN
severity: load
meaning: DLL mod is not EAC-safe. It did not load while AntiCheat is on.
---
id: mods-failed-dll
match: Failed loading DLL
kind: ERR
severity: load
meaning: Assembly load threw. Missing dependency, bad DLL, or wrong game version.
---
id: mods-failed-folder
match: Failed loading mod from folder
kind: ERR
severity: load
meaning: Exception while reading that folder. See the stack under it.
---
id: mods-failed-init
match: Failed initializing ModAPI
kind: ERR
severity: error
meaning: Mod folder loaded, then InitMod crashed. XML may still apply; Harmony usually did not.
cause: That mod's DLL InitMod threw. The folder is listed as loaded, but its Harmony/code did not finish.
suspects:
confidence: High — the log names the mod. The stack under it is the source.
---
id: mods-failed-modinfo-parse
match: Could not parse
kind: ERR
severity: load
meaning: ModInfo.xml is broken. Folder is ignored.
---
id: xml-patch-miss
match: XML patch for
kind: WRN
severity: conflict
meaning: That xpath missed. Another mod moved/removed the node, or the vanilla name changed.
---
id: gears-settings-missing
match: not found installed, unable to load the mod's current settings
kind: WRN
severity: noise
meaning: Gears still has settings XML for a mod that is no longer in the Mods folder. Harmless leftover.
---
id: discord-rpc-connect
match: RPC Connect error
kind: WRN
severity: noise
meaning: Discord overlay/RPC is not running. Game keeps retrying. Ignore unless they use Discord join.
---
id: discord-rpc-ready
match: RPC failed to receive READY payload
kind: WRN
severity: noise
meaning: Same Discord RPC miss as the connect error. Paired noise.
---
id: discord-webrtc
match: Will not log any `WebRTC
kind: WRN
severity: noise
meaning: Discord voice library turning off a metric. Ignore.
---
id: uiutils-no-binding
match: No device binding source could be found
kind: WRN
severity: noise
meaning: A menu action has no key/controller bind. Common with extra Quartz/UI actions.
---
id: steam-tick-long
match: Tick took exceptionally long
kind: WRN
severity: noise
meaning: One slow Steam callback. A hitch, not a mod failure.
---
id: eos-invalid-cache
match: Deleting local invalid cache file
kind: WRN
severity: noise
meaning: EOS news/title-storage cache was stale. Game deletes and re-downloads.
---
id: missing-preserve
match: missing the UnityEngine.Scripting.Preserve attribute
kind: WRN
severity: noise
meaning: Vanilla IL2CPP/console warning. PC ignores it.
---
id: next-airdrop
match: Next Airdrop:
kind: WRN
severity: noise
meaning: TFP logs the next airdrop time as a WRN. Not a problem.
---
id: twitch-entitlements
match: Failed to fetch Twitch entitlements
kind: WRN
severity: noise
meaning: Twitch Drops lookup failed (often 404). Ignore unless they use Twitch integration.
---
id: chunk-vml-blocked
match: ChunkManager mesh regeneration thread blocked
kind: WRN
severity: noise
meaning: Mesh queue backed up. FPS dip / hitch during world load or heavy building. Not a missing mod.
---
id: props-actiontarget
match: ActionTarget implicitly converted to Vector3
kind: WRN
severity: noise
meaning: A property/event ActionTarget could not become a position. Check nearby death or event lines.
---
id: nre-challenge-place
match: RequirementObjectiveGroupPlace
kind: EXC
severity: error
meaning: Place-door challenge tracker crashed after spawn.
cause: AGF-VP-DoorsPlus 4.0.0 broke tracking the place-door challenge. Vanilla code then NRE'd in RequirementObjectiveGroupPlace.AddIngredientGatheringReqs.
suspects: AGF-VP-DoorsPlus
confidence: High when DoorsPlus 4.0.0 is the loaded copy. 4.0.1 changelog: Fixed an error when you tried to track the place door challenge.
fixed_mod: AGF-VP-DoorsPlus
fixed_in: 4.0.1
fix_note: Fixed an error when you tried to track the place door challenge.
area: Challenges
---
id: gst-localization-get-bool
match: Localization.Get(string,bool)
kind: EXC
severity: error
meaning: Old GlobalStormTracker alert path calls a Localization.Get overload that 3.2 no longer has.
cause: AGF-NoEAC-GlobalStormTracker (especially 1.0.1) Harmony-patches WeatherManager.FrameUpdate and calls Localization.Get(string,bool). That overload was removed in 3.2, so the first storm-alert tick throws MissingMethodException.
suspects: AGF-NoEAC-GlobalStormTracker
confidence: High when GlobalStormTracker is loaded and the stack is WeatherManager.FrameUpdate. Removing the mod stops the error. Current fix is AGF-VPS-GlobalStormTracker 2.1.2 (renamed).
fixed_mod: AGF-NoEAC-GlobalStormTracker
fixed_in: 2.1.2
fix_note: Replace with AGF-VPS-GlobalStormTracker 2.1.2 (same mod, renamed NoEAC → VPS).
area: Weather / storms
---
id: loot-sort-echo-sortingcart
match: XUiC_ContainerStandardControls:Sort()
kind: EXC
severity: error
needs_mods: Echo_AdaptiveBackpack, AGF-VPS-SortingCart
meaning: Clicking Sort on the Sorting Cart crashed the loot window.
cause: Conflict between Echo Adaptive Backpack and Sorting Cart. Echo keeps the loot UI at 45 visible slots. Sorting Cart (with LargerStorageOption) has 168 backend slots. Sort writes those slots, then XUiC_LootContainer.OnTileEntityChanged indexes past the visible grid.
suspects: Echo_AdaptiveBackpack, AGF-VPS-SortingCart
confidence: High when both mods are loaded and the stack is loot Sort → OnTileEntityChanged. Nearby Echo loot-open lines name the Sorting Cart.
area: Loot / storage UI
---
id: xui-set-isvisible-parse
match: XUiView.set_IsVisible
kind: EXC
severity: error
meaning: Parsing a visible value threw inside XUiView.set_IsVisible.
cause: The crash is set_IsVisible while reading a visible attribute on a rect. Loaded mods that set visible="false" on a rect are the first place to look. GyroFlightModes does this on flightModeRow. Vanilla menu windows.xml also has one unnamed rect like that, so the mod hit is a suspect, not proof it is the only source.
area: UI
confidence: Medium. The stack names the visible parser. The mod list is a file search of loaded XUi, not a name found in the stack.
---
id: referenced-script-missing
match: The referenced script on this Behaviour
kind: UNITY
severity: noise
meaning: Unity missing MonoBehaviour on a prefab. Common vanilla leftover. Ignore unless a specific named object repeats with a mod.
---
id: texture-not-readable
match: is not readable, so Texture2D.ignoreMipmapLimit
kind: UNITY
severity: noise
meaning: Texture import flag. Atlas/texture mods (Adamant, Mittan) often log this. Cosmetic.
---
id: unloadasset-only-assets
match: UnloadAsset can only be used on assets
kind: UNITY
severity: noise
meaning: Unity cleanup after a runtime-created texture array. Usually paired with Addressables.Release noise.
---
id: addressables-release-unknown
match: Addressables.Release was called on an object
kind: UNITY
severity: noise
meaning: Same atlas unload path. Ignore unless it repeats every few seconds in play.
---
id: videoplayer-disabled
match: Cannot Prepare a disabled VideoPlayer
kind: UNITY
severity: noise
meaning: Menu/news video was disabled. Ignore.
---
id: fallback-native-dll
match: Fallback handler could not load library
kind: UNITY
severity: noise
meaning: Mono looking for a native helper DLL that is not there. Common at Harmony init. Ignore unless a specific mod DLL fails after it.
---
