=== AGF-4Modders-10IngredientSlots-v1.0.2 ===
SUMMARY:
- Shows up to 10 recipe ingredients instead of 5.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Expands the ingredient panel in the crafting info window from 5 visible rows to 10 visible rows.
- Adds rows 6 through 10 so recipes with larger requirements can be fully displayed without truncation.
- Adjusts ingredient panel layout values (row count, position, and row height) so the expanded list fits cleanly.
- Compatible with Vanilla UI, AGF HUDPlus 1Main, and AGF BackpackPlus.

=== AGF-4Modders-CustomWindowEnteringDuration-v1.0.1 ===
SUMMARY:
- Lets you set how long the "Entering Area" pop-up stays on screen.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Adds a config option for the "Entering Area" pop-up display duration.
- Edit `Config/windowEnteringDuration.xml` to set your preferred time.
- Default value is 3.0 seconds.

=== AGF-4Modders-Fix4DestroyBiomeBadge-v1.0.1 ===
SUMMARY:
- Prevents DestroyBiomeBadge from removing clothing items.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Fixes DestroyBiomeBadge behavior so it targets biome equipment only.
- Clothing items are no longer removed by this event.

=== AGF-4Modders-Fix4RemoveItems-v1.0.1 ===
SUMMARY:
- Lets the RemoveItems event run repeatedly in multiplayer.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Fixes RemoveItems behavior so it can be triggered more than once in multiplayer sessions.
- Prevents one-time-only behavior that can block repeated event use.

=== AGF-4Modders-ItemTypeIconColor-v1.0.2 ===
SUMMARY:
- Adds a new XML property, `ItemTypeIconColor`, so you can color the main item type icon.
- Vanilla only supports alternate icon color (`AltItemTypeIconColor`) for unlock-state style behavior.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Adds support for `ItemTypeIconColor` on item classes during `ItemClass.Init`, with parsed colors cached by item name.
- Supports color input as `[RRGGBB]`, `[RRGGBBAA]`, `R,G,B`, or `R,G,B,A`.
- Applies icon tint logic across major UI item views (trader entries, recipe entries, item info, crafting info, and quest turn-in entries).
- Uses unlock-aware behavior: if an item is unlocked and `AltItemTypeIconColor` exists, alternate tint is used; otherwise `ItemTypeIconColor` is used.
- Includes block fallback support for item-type tint lookup where relevant UI entries resolve a block from the item.

=== AGF-4Modders-SkillPointCap-v1.0.1 ===
SUMMARY:
- Lets you set a level cap for when level-ups stop granting skill points.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Adds a dedicated `progression.xml` attribute for skill-point reward limits.
- Lets you set a separate level cap for when level-ups stop giving skill points.
- In multiplayer, joining players must have this installed, and the server XML configuration is authoritative over client settings.

=== AGF-BackpackPlus-060Slots-v3.2.3 ===
SUMMARY:
- Expands your backpack to 60 slots while keeping vanilla-size inventory cells.
- Also adds craftable large storage options with 60, 80, and 100 slots.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Inventory size matches vanilla (no shrinking).
- 60-slot backpack with 3 rows (30 encumbrance slots).
- PackMule skill unlocks all encumbrance slots.
- Compatible with PackMule, buffs, Twitch integration, and mobility perks.
- Craftable large storage (wood, iron, steel) with 60/80/100 slots:
    - Easy-to-see lockable slots
    - Multiple storage block shapes
    - Breaking storage returns it to your inventory

=== AGF-BackpackPlus-072Slots-v3.2.3 ===
SUMMARY:
- Expands your backpack to 72 slots with inventory cells 16% smaller than vanilla.
- Also adds craftable large storage options with 120, 144, and 168 slots.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Inventory cell size is 16% smaller than vanilla (56Ã—56 vs 67Ã—67 px)
- 72-slot backpack with 3 rows (36 encumbrance slots)
- PackMule skill unlocks all encumbrance slots
- Compatible with PackMule, buffs, Twitch integration, and mobility perks
- Craftable large storage (wood, iron, steel) with 120/144/168 slots:
    - Easy-to-see lockable slots
    - Multiple storage block shapes
    - Breaking storage returns it to your inventory

=== AGF-BackpackPlus-084Slots-v3.2.3 ===
SUMMARY:
- Expands your backpack to 84 slots with inventory cells 16% smaller than vanilla.
- Also adds craftable large storage options with 120, 144, and 168 slots.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Inventory cell size is 16% smaller than vanilla (56Ã—56 vs 67Ã—67 px)
- 84-slot backpack with 3 rows (36 encumbrance slots)
- PackMule skill unlocks all encumbrance slots
- Compatible with PackMule, buffs, Twitch integration, and mobility perks
- Craftable large storage (wood, iron, steel) with 120/144/168 slots:
    - Easy-to-see lockable slots
    - Multiple storage block shapes
    - Breaking storage returns it to your inventory

=== AGF-BackpackPlus-119Slots-v1.2.3 ===
SUMMARY:
- Expands your backpack to 119 slots with inventory cells 16% smaller than vanilla.
- Includes larger storage options and may need UI zoom adjustment due to backpack size.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Inventory cell size is 16% smaller than vanilla (56Ã—56 vs 67Ã—67 px)
- 119-slot backpack with 3 rows (68 encumbrance slots)
- Some UI overlap may occur due to the large backpack size; adjust UI zoom in the game menu if needed
- PackMule skill unlocks all encumbrance slots
- Compatible with PackMule, buffs, Twitch integration, and mobility perks
- Craftable large storage (wood, iron, steel) with 120/144/168 slots:
    - Easy-to-see lockable slots
    - Multiple storage block shapes
    - Breaking storage returns it to your inventory

=== AGF-HUDPlus-1Main-v5.4.6 ===
SUMMARY:
- Makes the HUD and menus easier to read.
- Improves layout for compass, toolbelt, map, trader windows, chat, and timers.
- Adds quick visual hints: clearer slot markers, color hints, 5 centered crafting slots, and always-visible date/time/location.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Displays health, stamina, food, water, level, XP, and elevation in a compact, easy-to-read format.
- Lockable inventory slots are clearly marked for quick identification.
- Compass is wider and features a visible center mark.
- Toolbelt is lowered for better backpack mod compatibility, with numbered slots for convenience.
- Chat messages are easier to read with updated backgrounds.
- Map details are displayed in a dedicated section beside the map, with clear labels.
- Crafting and burning timers are visually clearer and easier to read.
- Date, time, outdoor temperature, and the location display are visible on all menus.
- Storm visual alerts now appear below the location display, freeing space for other mods.
- Trader dialogue, including quest selection, is easier to read with updated backgrounds.
- Simple color-coding for interactive prompts (the text that appears when looking at loot, workstations, etc.) for quick recognition.
- You now have 5 centered crafting slots instead of 4.
- Schematics, magazines, and books are color-coded to show whether theyâ€™ve been read.
- The NoEAC-EnhancedAGF provides even more visual features if also installed.
- Vehicle stats display has been updated for clearer, easier at-a-glance reading.
- Compatible with all 7 Days to Die versions since 2.5. (For new game updates, check for the latest HUDPlus version if needed.)

=== AGF-HUDPlus-BMCounter-v2.1.3 ===
SUMMARY:
- Adds a Blood Moon countdown under the compass.
- Designed for fixed Blood Moon schedules (not varied ranges).
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Adds a Blood Moon countdown display under the compass.
- Supported fixed frequencies: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 14, 15, 20, 28, 30, and 60 days.
- If your Blood Moon frequency or Blood Moon range is different, this mod will not apply.

=== AGF-HUDPlus-HealthBars-v4.0.1 ===
SUMMARY:
- Adds enemy health bars in AGF HUDPlus style.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Adds AGF-style enemy health bars that are easier to read during combat.
- Built to work standalone and conditionally with other AGF HUDPlus features.

=== AGF-HUDPlus-PurpleBook-v2.0.1 ===
SUMMARY:
- Adds a Purple Book button with a one-page view with zoom for progression, unlock details, and armor descriptions.
- The Purple Book button appears next to the crafting icon when your inventory is open.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Works standalone and does not require HUDPlus-1Main.
- The Purple Book button appears next to the crafting icon when your inventory is open.
- Opens a dedicated Crafting List window with one-page browsing and zoom-in views.
- Main sections are organized into tabs for:
  - Magazine unlocks
  - Books
  - Unlocks from schematics/books
  - Full armor descriptions
- Checklist pages use visual progress indicators so you can quickly see what is completed vs still missing.
- Progress values are recalculated automatically in-game so the display stays in sync with your current progression.
- Compatibility requests may be made to AGF.

=== AGF-HUDPlus-RemoveEnteringPopUp-v1.0.1 ===
SUMMARY:
- Removes the "Entering" popup when you enter POIs/areas.
- You will no longer see location-entry popup text on screen.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Edits the `windowEntering` UI window used for location-entry notifications.
- Removes the `TextContent` label and popup `grid` elements from that window.
- This disables the visible "Entering" POI/area popup presentation.

=== AGF-HUDPluszOther-RemoveWrittentormAlerts-v2.0.1 ===
SUMMARY:
- Removes written storm warning messages from the HUD.
- You will no longer see incoming storm text alerts on screen.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Edits the `windowEntering` UI window used by HUD alert text.
- Removes the `MessageContent` label that displays written storm warning messages.
- This keeps written storm alerts from appearing while leaving other HUD elements intact.

=== AGF-HUDPluszOther-SmallerInteractionPrompt-v1.0.1 ===
SUMMARY:
- Makes the interaction prompt text smaller on screen.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Edits the `interactionPrompt` window label font size in `XUi/windows.xml`.
- Changes prompt font size from the default 34 down to 30.
- This affects the central interaction prompt text shown when targeting interactable objects.

=== AGF-HUDPluszOther-TinyBuffsPopUp-v1.0.1 ===
SUMMARY:
- Makes buff pop-up notifications about half the vanilla size.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Replaces the default `BuffPopoutList` UI block in `HUDLeftStatBars` with a smaller custom version.
- Uses these reduced values (default -> this mod): panel height `46 -> 23`, icon size `40x40 -> 20x20`, text font `28 -> 18`, and text area height `40 -> 20`.
- Keeps buff pop-up behavior while reducing on-screen footprint.

=== AGF-HUDPluszOther-Weekday-v2.1.1 ===
SUMMARY:
- Adds the weekday name next to the in-game day number (for example: Day 14, Monday).
- If Blood Moon is set to every 7 days, Blood Moon occurs on Sunday night.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Adds `daycount_{day%7}` localization to the compass day text so the HUD shows both day number and weekday name.
- Includes localized weekday names in all 13 supported languages through `Config/Localization.txt`.
- Uses conditional XUi paths for vanilla and AGF HUD/BMCounter variants so it works standalone or with those companion mods.
- Weekday enhancement originally credited to DragonTander and used with permission.

=== AGF-NoEAC-AudioOptionsPlus-v1.0.1 ===
SUMMARY:
- Adds two audio tabs to the vanilla game audio menu: Volume Profiles and Sound Swap.
- Volume Profiles gives separate volume control for many sound groups (for example: motorized tools, vehicles, and trader voices).
- Sound Swap lets you turn Silly Sounds on/off and swap animal pain/death sounds to non-pain sounds.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Expands the vanilla `optionsAudio` menu from 4 tabs to 6 tabs and inserts two new tab pages: `Volume Profiles` and `Sound Swap`.
- `Volume Profiles` includes an overall preset (`Off`, `Low`, `Medium`, `High`, `Default`, `Custom`) and detailed per-category sliders in 5% steps.
- Categories include door types, animal pain/death, block building, electrical loops, explosions, gunfire, item interaction, motor tools, player sounds, protected-block dings, spider zombie, surface impact, traders, Twitch sounds, and vehicles.
- `Sound Swap` includes a global animal pain/death preset (`All Default`, `All Swapped`, `Custom`) plus per-animal swap controls (bear, boar, canines, chicken, stag, mountain lion, rabbit, snake, and vulture).
- Includes a `Silly Sounds` toggle for Twitch Integration and localized UI labels/tooltips across all 13 supported languages.

=== AGF-NoEAC-AutoRun-v1.0.1 ===
SUMMARY:
- Adds auto-run on foot and in vehicles using a double tap of the Forward key.
- Hold Sprint when starting auto-run to begin sprinting; double tap Sprint later to toggle sprint on.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Works on foot and in vehicles.
- Double tap your Forward key to enable Auto-Run.
- Hold Sprint while double tapping Forward to start Auto-Run with Sprint enabled.
- While Auto-Run is active, double tap Sprint to enable Sprint or tap Sprint once to disable it.
- Press Forward once to turn Auto-Run off and return to manual movement control.

=== AGF-NoEAC-ConsoleOpacityMod-v1.0.1 ===
SUMMARY:
- Lets you customize F1 console background opacity.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Loads `Config/ConsoleOpacity.xml` on mod initialization and reads the `<Opacity>` value.
- Opacity values are clamped between `0.0` and `1.0` for safe limits.
- Applies configured opacity to the console `Scroll View` background each time the console opens.
- Keeps important console controls readable by using a slightly higher alpha on `CommandField` and full alpha on `CloseButton`.

=== AGF-NoEAC-CosmeticLockIcon-v1.0.2 ===
SUMMARY:
- Adds a scrap icon to locked cosmetic armor entries.
- Helps you see if you still need to scrap an armor to unlock its cosmetic.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Appends item properties for cosmetic armor master entries (`armorPrimitiveMaster`, `armorLightMaster`, `armorMediumMaster`, `armorHeavyMaster`).
- Sets `ItemTypeIcon` to `scrap` so locked cosmetic armor entries show a clear locked-style marker.
- Sets `AltItemTypeIconColor` alpha to `0` so the alternate icon is hidden cleanly.
- Clears item type icons from `biomeWeatherItem1` so biome gear does not show these cosmetic lock markers.

=== AGF-NoEAC-EnhancedAGF-v3.2.1 ===
SUMMARY:
- Adds extra AGF features, especially for HUDPlus.
- Expands the HUD and menu data available to compatible AGF mods.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- DLL-only patch: loads Harmony patches and custom UI bindings that other AGF mods can read.
- Most visible features appear when paired with AGF HUDPlus modules (especially `AGF-HUDPlus-1Main`).
- Adds expanded player bindings for HUD/menus, including level, loot stage, XP to next level, elevation, health/stamina/food/water value variants (current, max, with max, percent), and bag usage/capacity values.
- Adds status bindings for day/night state, active flashlight source (hand/gun/helmet), current ammo icon/count/visibility, and DoomGuy health icon states.
- Adds vehicle HUD bindings for durability, forward/reverse speed, fuel (current/max/percent), turbo/cruise state, pitch/roll, flight-assist mode state, ground clearance, and mounted visibility.
- Includes caching/polling optimizations in several bindings/controllers to keep frequent HUD refreshes responsive.

=== AGF-NoEAC-ExpandedInteractionPrompts-v1.1.1 ===
SUMMARY:
- When looking at any workstation, provides crafting and status information on the prompt.
- When looking at storages, provides slots used status.
- When looking at vehicles, shows seats available, locked status, and slots used.
- LockableWorkstations compatible: hides extra prompt details if access is denied.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Uses Harmony patches to expand activation prompts for `BlockWorkstation`, `BlockCampfire`, `BlockForge`, `BlockLoot`, `BlockSecureLoot`, `BlockSecureLootSigned`, `TEFeatureStorage`, and vehicle-focused interaction text.
- Workstation and campfire prompts show queue state (`Crafting`, `Needs Fuel`, `Start Fire`) plus output slot usage (`used/total` or `Empty`).
- Forge prompts also include smelting input usage (`Smelting x/y`), then crafting state, then output slot usage.
- Storage prompts append slot usage for regular loot, secure loot, signed secure loot, and composite storage-backed player containers.
- Vehicle prompts show seat count and lock state for everyone; storage slot usage is shown only to owners/allowed users.
- If `LockableWorkstations` is active and the prompt is denied/jammed, extra details are suppressed for compatibility.
- Includes a safer prompt-formatting patch to reduce format issues and preserve prompt output stability.

=== AGF-NoEAC-FlightAssist-v1.0.1 ===
SUMMARY:
- Two flight assist patterns:
  - Gyrocopter: Press Z to move forward while staying at the same elevation.
  - Helicopter: Press Z for Hover, then press Z again for forward same-elevation flight.
- HUD shows the current Flight Assist mode.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Harmony patches run on vehicle movement/physics to provide altitude assist for likely flying vehicles (detected from vehicle force properties).
- Two control patterns are handled automatically:
  - `BasicPlane`: uses Y-lock forward flight mode.
  - `HelicopterLike`: cycles between `Hover` and `Y-Lock` while assist is active.
- Activation uses hotkey `Z` by default (`Config/FlightLevelAssistConfig.txt`), and can also use an optional controller action binding.
- Assist behavior includes pitch leveling before hard lock, altitude hold, and mode-aware transitions to reduce abrupt movement.
- Plane-like Y-lock includes momentum gating and auto-disengage when forward momentum stays too low.
- Helicopter-like hover and forward modes include damping/transition logic for smoother stabilization.
- Assist is canceled by key safety conditions such as manual pitch input, no fuel, crash-level pitch disruption, driver detach, or conflicting external auto-run state.
- Integrates with AGF AutoRun when available for plane-like Y-lock forward hold; falls back to direct forward input when AutoRun is unavailable.
- Exposes HUD bindings for assist visibility, mode text (`Off`, `Hover`, `Y-Lock`), mode tooltips, and mode-specific icon visibility.

=== AGF-NoEAC-GlobalStormTracker-v1.0.1 ===
SUMMARY:
- Sends a global chat message showing where the storm is and when it ends.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Uses Harmony to patch WeatherManager.FrameUpdate and monitor storm state per biome.
- Runs only on server authority (dedicated server or hosting player), so clients receive alerts without local install.
- Sends one alert when a biome storm starts, then suppresses repeated spam by tracking last storm state.
- Alert message includes localized biome name and storm clear time (day and in-game clock).
- Biomes are localized using keys for Desert, Pine Forest, Snow, Wasteland, Burnt Forest, and Unknown fallback.
- Ignores early unsynced world time at startup to avoid false login alerts.
- Resets tracked storm timing data when the biome storm ends so the next storm can alert correctly.

=== AGF-NoEAC-LockableWorkstations-v1.2.1 ===
SUMMARY:
- Workstations, Collectors, Generators, and Ranged Defenses start locked when placed.
- These stations can be unlocked, locked again, or set with a key code.
- Without client install, look at block within 5 meters and use chat commands '/agf-lw help'.
- With client installed, use the UI like storages or chat commands.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Hybrid Mod Description:
- Works standalone as server-only, or as server+client for full UI features.
- Players can join without client install; client install is optional for enhanced lock UI and syncing.

All Versions:
- Placed workstations, collectors, generators, and ranged defenses are lockable.
- Locked interaction plays locked feedback and respects owner/allowed-user permissions.

Server-Side Use:
- Commands target the focused supported block within 5 meters.
- Server-only users can lock, unlock, check status, and manage keypad codes via chat commands.
- Server-only installs do not show the client lock-status UI overlays.
- Chat command for server-side-only users: `/agf-lw <lock|unlock|code set <code>|code clear|code use <code>>`.
- Chat command to check current target lock state: `/agf-lw status`.

Client-Side Use:
- Shows lock status in UI prompts (Secured, Accessible, Access Denied).
- Supports storage-style lock editing flow and synced lock-state updates.
- Client-side installs can use both chat commands and F1 console commands.
- Console command (F1): `agf-lw <lock|unlock|code set <code>|code clear|code use <code>>`.
- Status command: `/agf-lw status` or `agf-lw status`.

Chat Commands:
- Main alias: `/agf-lw`
- Status: `/agf-lw status`
- Lock: `/agf-lw lock`
- Unlock: `/agf-lw unlock`
- Set keypad code: `/agf-lw code set <code>`
- Clear keypad code: `/agf-lw code clear`
- Use keypad code for access: `/agf-lw code use <code>`
- Help: `/agf-lw help`

F1 Console Commands (client install required):
- Main alias: `agf-lw`
- Status: `agf-lw status`
- Lock: `agf-lw lock`
- Unlock: `agf-lw unlock`
- Set keypad code: `agf-lw code set <code>`
- Clear keypad code: `agf-lw code clear`
- Use keypad code for access: `agf-lw code use <code>`
- Help: `agf-lw help`

Command Notes:
- If you do not install the client side, use chat commands only.
- Focused-block commands require an in-world player and a valid lockable target in range.
- Console/telnet cannot use focused-block lock commands.
- Legacy aliases also work: `lw`, `lockws`, `lockableworkstations`.

Admin Behavior:
- Admins can run the same agf-lw commands to manage any focused supported block, including blocks they do not own.
- Admins can still open locked supported blocks (access-denied checks explicitly allow admins).
- Non-admin players can manage owned or ACL-allowed workstations.

=== AGF-NoEAC-OpenAllButton-v1.0.2 ===
SUMMARY:
- Players who install it get an "Open All" button on eligible bundled items.
- "Open All" rapidly opens one at a time until done, inventory fills, or you stop it.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- This is a client-side UI button addition only; it does not require other joining players to install it.
- Simply adds an "Open All" button for bundled items.
- It opens one at a time rapid fire until all are opened or if you run out of inventory space.
- It can be stopped by either closing the window or pushing the stop opening button.
- Again, if there isn't room for more, it will stop and not allow you to open more.

=== AGF-NoEAC-ScreamerAlert-v1.2.1 ===
SUMMARY:
- Warns you when Screamers or Screamer horde zombies are nearby (120m).
- Players without client installation get private chat alerts.
- Players with client installation get a live on-screen alert and optional Screamer and horde counts.
- You can turn alerts on or off with chat commands: /agf-sa on or /agf-sa off.
- See this mod's README for full details.
DETAIL:
- Works standalone.
All Versions:
- Detects live Screamers and Screamer-spawned zombies within 120 meters.
- Sends alerts only while qualifying enemies are alive and in range.

Server-Side Use:
- Players without client installation receive a private chat alert.
- Server-side only users can use Off or On modes.
- Server-side alerts are notification-only and are not live-tracked in UI.
- Chat command for server-side-only users: /agf-sa <off|on>.
- Chat command to check current setting: /agf-sa status.

Client-Side Use:
- Shows a real-time on-screen alert.
- Alert clears automatically when no qualifying enemies are alive or in range.
- Supports 3 modes: Off, On (alert only), Counts (Screamer + horde zombie counts).
- Chat command to set mode: /agf-sa <off|on|counts|cycle>.
- F1 console command to set mode: agf-sa <off|on|counts|cycle>.
- Status command: /agf-sa status or agf-sa status.
- Can also be toggled through ESC Window integration (if client installation is present).

Chat Commands:
- Main alias: /agf-sa
- Set mode: /agf-sa <off|on|counts|cycle>
- Status: /agf-sa status

F1 Console Commands:
- Main aliases: agf-sa, agf-screameralert
- Set mode: agf-sa <off|on|counts|cycle>
- Status: agf-sa status

Admin Behavior:
- Admin command (F1): agf-sa admin <entityId> <off|on|counts|cycle>
- Allows admins to set another player's alert mode directly.

=== AGF-NoEAC-SortingBox-v1.0.6 ===
SUMMARY:
- Craft a Sorting Box for 10 Forged Iron, place items in it, and matching items auto-sort into nearby storage (25m range).
- Sorting rules: it skips containers in use; 
  - Unlocked Sorting Boxes sort into unlocked storages only.
  - Locked Sorting Boxes sort into unlocked storages and locked storages that share the same keycode.
- See this mod's README for full details.
DETAIL:
- Works standalone.
All Versions:
- Uses Sorting Box tagged blocks as source boxes for item distribution.
- When a valid Sorting Box interaction finishes, it scans nearby storage and distributes matching items.
- Crafting recipe: 10 Forged Iron for the Sorting Box helper item.
- Default scan range is 25m horizontal and 25m vertical (25 / 25).
- Default range values are editable in `robotic-inbox.json`.
- Distribution skips targets that are currently in use.
- Backpacks, vehicles, and non-player storage are ignored.
- Sorting rules:
- If the Sorting Box is unlocked, it can sort into unlocked storage only.
- If the Sorting Box is locked, it can sort into unlocked storage and locked storage only when keycodes/passwords match.
- If the target is locked without a keycode/password set, transfer is blocked.
- Slot-lock behavior is respected when combining/sorting stacks in target containers.
- Temporary notice text can appear on affected containers when distribution is blocked.

Server-Side Use:
- Dedicated server install runs the sorting logic for all players.
- Main settings file: `robotic-inbox.json`.
- Includes configurable options for range, blocked/success notice timing, and base siphoning protection.
- Base siphoning protection helps prevent inbox scans from crossing outside the same LCB area.

Player-Hosted Use:
- For player-hosted sessions, host and joining players should install the mod so behavior stays consistent.

F1 Console Commands:
- Main aliases: `roboticinbox`, `ri`
- Show current settings: `roboticinbox settings`
- Set horizontal scan range: `roboticinbox horizontal-range <int>`
- Set vertical scan range: `roboticinbox vertical-range <int>`
- Set success notice time: `roboticinbox success-notice-time <float>`
- Set blocked notice time: `roboticinbox blocked-notice-time <float>`
- Toggle base siphoning protection: `roboticinbox base-siphoning-protection`
- Toggle debug mode: `roboticinbox dm`

Notes:
- This mod does not use chat commands for configuration.
- Settings are saved and loaded from `robotic-inbox.json`.

=== AGF-NoEAC-Toolbelt12Slots-v1.0.3 ===
SUMMARY:
- Expands the toolbelt from 10 slots to 12 slots.
- Adds hotkeys for the extra slots using - and =.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Expands the toolbelt from 10 slots to 12.
- Toolbelt hotkeys run from 1 through = on the keyboard.
- Fixes a mod compatibility issue where holding Shift could block toolbelt hotkey selection.
- In Edit Mode, when the toolbelt is shown in two rows, holding Shift correctly selects slots in the second row.
- Includes focus handling improvements around mounted and recently dismounted states to keep slot selection stable.
- This mod changes local toolbelt and input handling behavior for the installing player.

=== AGF-VP-AdminModdingSupport-v1.0.4 ===
SUMMARY:
- Faster Block Replace Tool: delay changed `0.83 -> 0.1`.
- Two dev-only XP books for admins to grant either `100k` or `1M` XP.
- Dev-only harvest testing blocks for all harvest types, with baseline harvest set to `10`.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `items.xml` changes Block Replace Tool (`meleeToolBlockReplaceTool`) Action1 delay from default `0.83` to `0.1`.
- `items.xml` adds two dev-only XP books:
  - `levelUp100k` gives `100k` XP on use.
  - `levelUp1m` gives `1M` XP on use.
- `blocks.xml` adds dev-only test blocks tuned for simple baseline harvest checks (10 items without perk/ability modifiers):
  - `agfTestButcherBlock` (butcher-style harvest set).
  - `agfTestSalvageBlock` (salvage parts/resources set).
  - `agfTestMiningBlock` (ore/mining test drops).
  - `agfTestWoodBlock` (wood/lumber test drops).

Usage Notes:
- Added books and test blocks are marked `CreativeMode=Dev` (admin/dev menu use).
- XP books are intended for admin support workflows where staff need to grant either `100k` or `1M` XP to players.
- XP gain still respects your server XP multiplier settings.
- Test blocks are intended for controlled admin/modding tests, not normal survival progression.
- Test block harvest values are set so baseline no-perk/no-ability checks stay simple and consistent.

=== AGF-VP-AlternativeRecipes-v1.0.2 ===
SUMMARY:
- New Cobblestone Block recipe directly from clay and small stone.
- New First Aid Bandage and Kit recipes directly from cloth.
- Original vanilla recipes are still available.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `recipes.xml` appends a new `cobblestoneShapes:VariantHelper` recipe (`count="1"`, `craft_time="3"`) using:
  - `resourceClayLump` x10
  - `resourceRockSmall` x10
- `recipes.xml` appends a new `medicalFirstAidBandage` recipe (`count="1"`, `tags="learnable"`) using:
  - `medicalAloeCream` x1
  - `resourceCloth` x10
- `recipes.xml` appends a new `medicalFirstAidKit` recipe (`count="1"`, `craft_area="chemistryStation"`, `tags="learnable,chemStationCrafting"`) using:
  - `medicalBloodBag` x1
  - `drinkJarBeer` x1
  - `resourceCloth` x50
  - `resourceSewingKit` x1

Usage Notes:
- These are alternative recipe paths; original vanilla crafting paths remain available.

=== AGF-VP-AmmoDisassembly-v1.0.2 ===
SUMMARY:
- Scrap ammo into bundles that return ingredients (vanilla scrapping math).
- Combine bundles into `x100` or `x1000` for large batch opening.
- Optional: works with client-side mod 'OpensAllButton'.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `materials.xml` adds custom ammo materials (`Mammo*`, `Mthrown*`) that route scrapping through `forge_category` entries like `disa*`/`dthrown*`.
- `items.xml` appends many ammo/thrown items to use those custom materials and sets `Weight` to `1` for the disassembly flow.
- `items.xml` adds disassembly bundle items (`disa*`, `dthrown*`) with `Action0` class `OpenBundle` so opening each bundle returns ingredient items.
- `recipes.xml` appends always-unlocked `salvageScrap` recipes for those bundle items, covering bullets, shells, arrows/bolts, rockets, throwables, darts, and junk turret ammo.
- `recipes.xml` also appends bundle-combine recipes so `100` or `1000` standard disassembly bundles can be crafted into one larger bundle for faster handling.

Usage Notes:
- This mod includes a server-side solution for large opening workflows via the `x100`/`x1000` bundle recipes.
- Optional OpensAll support is a separate client-side-only feature and is not required for the server-side batch workflow here.
- Disassembly bundle outputs follow vanilla scrapping math.

=== AGF-VP-ApiaryPlus-v1.0.4 ===
SUMMARY:
- Apiary size is `2x2` instead of vanilla `3x2`.
- Apiary heatmap is removed.
- Break old `3x2` apiaries to get the `2x2` version.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `blocks.xml` removes `HeatMapStrength`, `HeatMapTime`, and `HeatMapFrequency` from `cntApiary`.
- `blocks.xml` adds new block `cntApiaryAGF` that extends `cntApiary` and sets `MultiBlockDim` to `2,2,1` (vanilla is `3,2,1`).
- `blocks.xml` removes old `cntApiary` drop entries and replaces destroy/fall drops so old apiaries return `cntApiaryAGF`.
- `recipes.xml` renames the craftable apiary recipe from `cntApiary` to `cntApiaryAGF`.
- `progression.xml` unlocks `cntApiaryAGF` at `craftingWorkstations` level `30+` and updates the displayed unlock entry to the new block.
- `XUi/windows.xml` includes a conditional compatibility set for `AGF-HUDPlus-PurpleBook` so tooltip text points to `cntApiaryAGF` when that mod is loaded.

Usage Notes:
- Existing placed vanilla `3x2x1` apiaries should be broken to get the `2x2x1` apiary block.

=== AGF-VP-ArcheryFeathersChange-v1.0.3 ===
SUMMARY:
- All archery ammo requires feathers (no longer requires plastic).
- Feathers can be crafted from plastic.
- Compatibility is auto-determined for other AGF mods.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `recipes.xml` removes `resourceScrapPolymers` from recipes with names containing `Arrow` or `Bolt`.
- `recipes.xml` appends `resourceFeather` ingredients to advanced arrow/bolt single and bundle recipes:
  - Flaming, Exploding, and Steel AP arrows
  - Flaming, Exploding, and Steel AP crossbow bolts
- `recipes.xml` adds a workbench recipe to craft feathers from plastic:
  - `resourceFeather` x1 from `resourceScrapPolymers` x2 (`craft_time="1"`, `craft_area="workbench"`)
- `items.xml` includes a conditional section for `AGF-VP-AmmoDisassembly` that updates disassembly bundle outputs (single, `100`, and `1000` variants) so archery disassembly returns feathers instead of plastic.

Usage Notes:
- This mod changes advanced archery to feather-based crafting while keeping a conversion path from plastic to feathers.

=== AGF-VP-ArmorHarvestMods-v2.1.1 ===
SUMMARY:
- Harvest mods become craftable at Armor Magazine level 11: Farmer, Miner, Scavenger, Lumberjack.
- These mods move harvesting bonuses onto your current armor.
- Mod ingredient costs match a full set of the related armor and scale with quality.
- Harvest bonuses are non-stacking.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `item_modifiers.xml` adds four tiered harvest mods:
  - `modArmorFarmer`
  - `modArmorMiner`
  - `modArmorScavenger`
  - `modArmorLumberjack`
- `item_modifiers.xml` sets `installable_tags="armor"` and uses `blocked_tags` checks so same-type armor-set bonuses and similar harvest checks do not stack.
- `item_modifiers.xml` enables quality scaling (`ShowQuality="true"`) and tier-based effect values for harvest, stamina, block damage, loot/XP, and related harvesting bonuses.
- `recipes.xml` adds workbench recipes for all four mods (`craft_time="30"`) and scales ingredient counts by tier using `CraftingIngredientCount` passive effects.
- `progression.xml` wires the mods into `craftingArmor` unlock displays and unlock tags at level `11+` (with higher-tier scaling entries also updated).
- `items.xml` adds compatibility tags to lumberjack/miner armor pieces for stack-prevention checks and sets `resourceArmorCraftingKit` material/tags/weight support.
- `materials.xml` defines `MArmorCraftingKit` with forge-category scrapping support.
- `ui_display.xml` adds custom display groups (`modFarmer`, `modMiner`, `modScavenger`, `modLumberjack`) so stat lines show correctly in UI.

Usage Notes:
- These mods are designed to move harvesting-style bonuses onto armor mods while preserving non-stacking behavior with matching set mechanics.

=== AGF-VP-AutomobilesRespawn-v3.1.1 ===
SUMMARY:
- Salvaged vehicles downgrade to a hubcap seed at that location.
- The hubcap regrows into a similar vehicle type.
- Growth time and hubcap durability are configurable (default: `600` minutes, `2500` HP).
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `blocks.xml` adds `vehicleGrowingMaster` (hubcap model) using `PlantGrowing` behavior.
- `blocks.xml` sets editable seed defaults on the master block:
  - `PlantGrowing.GrowthRate="600"` (minutes)
  - `MaxDamage="2500"`
- `blocks.xml` adds many `carAGF*Planted` seed blocks with vehicle-sized `MultiBlockDim` values and `PlantGrowing.Next` targets.
- `blocks.xml` appends `carSeed` tags and `DowngradeBlock` mappings to vanilla vehicle blocks so salvaged vehicles downgrade into planted seed variants.
- `blockplaceholders.xml` defines randomized respawn pools for those helper placeholders (sedans, trucks/SUVs, buses, work vehicles, heavy equipment, etc.).
- `blocks.xml` keeps legacy `planted*` blocks for safe upgrades on existing saves.

Usage Notes:
- The hubcap seed occupies vehicle space while regrowing, which helps preserve the respawn footprint.
- Respawn pools are grouped by vehicle families, so regrowth stays similar to the original vehicle type.
- Seed growth time and durability can be adjusted in `vehicleGrowingMaster`.

=== AGF-VP-BedrollPlus-v1.0.1 ===
SUMMARY:
- Adds a new block called Bedroll+; essentially the bedroll but with all bed models and higher durability.
- Vanilla bedroll remains unchanged.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `blocks.xml` adds `bedrollAGFMaster` (`Class="SleepingBag"`) with shared Bedroll+ behavior, including:
  - `MaxDamage="500"`
  - pickup enabled (`CanPickup` returns `bedrollBlockVariantHelperAGF`)
  - repair with cloth (`resourceCloth` x20)
- `blocks.xml` adds many player-usable bedroll/bed variants extending that master (multiple colors and bed styles) with spawn-point functionality.
- `recipes.xml` adds a recipe for `bedrollBlockVariantHelperAGF`:
  - cloth x20
  - wood x10
  - scrap iron x10
- `materials.xml` adds support materials (`Mfurnituremetal`, `Mfurniturecloth`) used by the Bedroll+ block set.
- `Localization.txt` adds display names for the Bedroll+ variants across supported languages.

Usage Notes:
- Bedroll+ is intended as a tougher, recoverable spawn-point bed option with expanded visual/style choices.
- Vanilla bedroll behavior is not modified by this mod.

=== AGF-VP-BetterEggChance-v2.1.1 ===
SUMMARY:
- Chance to find eggs in birdnest increased from 35% to 63%.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `loot.xml` updates bird nest egg loot chance by changing:
  - `/lootcontainers/lootgroup[@name='groupBirdNest02']/item[@name='foodEgg']/@loot_prob_template`
  - from `medLow` (vanilla) to `medHigh`

Result:
- Bird nests roll eggs more often than vanilla (the summary value is 35% to 63%).

=== AGF-VP-BreakItGetIt-v1.0.3 ===
SUMMARY:
- Break supported stations, storage, signs, and electrical blocks to get the block back.
- When you break a forge, its smelted contents can be picked up from the dropped backpack.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `blocks.xml` removes existing `drop` entries for multiple vanilla blocks, then appends custom `Destroy` and `Fall` drops that return the placed block item.
- For most supported blocks, `Fall` recovery is set to `prob="0.9"` with `stick_chance="1"`.
- Supported groups include core stations (campfire, forge, workbench, cement mixer, chemistry station), writable crates (wood/iron/steel), wood signs, and power banks (generator, battery, solar).
- Dew collector behavior is conditional: `cntDewCollector` is only changed when `AGF-VP-DewsPlus` is not loaded.
- Table saw is explicitly handled with `dropextendsoff` so it does not inherit cement mixer return behavior; it uses its own salvage-style drops and no block return on destroy.

Result:
- Breaking supported blocks generally returns the block item, and falling versions usually have a high chance to be recoverable.
- Container contents are still handled by normal game behavior (for example, forge contents dropping into a backpack).

=== AGF-VP-BuyTraderVendingMachines-v3.0.3 ===
SUMMARY:
- At high trader stage, traders start selling drink and snack vending machines.
- These are vending machines that restock daily.
- Player-owned machines can be picked up by breaking them.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- `traders.xml` adds trader stage template `agfTier` with `min="100"`, then adds an `AGFVending` item group and injects it into `traderGeneral` so traders can sell these machines.
- `blocks.xml` adds player versions of trader-restocking vending machines:
  - drink variant helper (`cntVendingMachineDrinks_PlayerVariantHelper`)
  - snack machine (`cntVendingMachineSnacksFull_Player`)
- These player versions use `TraderStageTemplate="agfTier"` and `EconomicValue="15000"`.
- Break/recovery behavior is added for purchased machines:
  - `Destroy` returns the machine item
  - `Fall` has a recovery chance (`prob=".9"`)
- `Localization.txt` labels these as Trader Restock vending machines and includes description text that they restock daily and can be picked up by breaking.

Usage Notes:
- Trader-managed vending machines are the ones that stock food/drink and reset daily.
- This mod targets the purchased player versions so you can recover them; it is separate from map/world-placed machines.

=== AGF-VP-CraftSewingKits-v1.0.1 ===
SUMMARY:
- After reading Wasteland Treasures 6, you can craft sewing kits at a workbench.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- `recipes.xml` adds a learnable workbench recipe for `resourceSewingKit`:
  - mechanical parts x5
  - cotton x5
  - boiled water x1
- `items.xml` sets `resourceSewingKit` to unlock via `perkWasteTreasuresCloth`.
- `progression.xml` appends `resourceSewingKit` to that book perk's craftable tags.

Usage Notes:
- After unlocking Wasteland Treasures 6, the sewing kit recipe becomes craftable at the workbench.

=== AGF-VP-CraftStackEngBattCells-v3.3.1 ===
SUMMARY:
- Engines, batteries, and solar cells now stack.
- They are all craftable, unlocked across Electrician levels.
- To use a battery bank, craft a "Rechargeable Battery" from a regular battery.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- `items.xml` updates stack behavior:
  - `smallEngine` stack size set to 500
  - `carBattery` converted to non-quality stackable battery (stack 500)
  - `solarCell` converted to non-quality stackable cell (stack 500)
- `items.xml` also adds `carBatteryCharge` (Rechargeable Battery) for battery-bank use.
- `blocks.xml` changes battery bank slot item to `carBatteryCharge` and adjusts battery/solar bank output-per-stack values.
- `recipes.xml` adds/adjusts workbench crafting for key items:
  - engine (`smallEngine`)
  - solar bank (`solarbank`)
  - solar cell (`solarCell`)
  - rechargeable battery conversion (`carBattery` -> `carBatteryCharge`)
- `progression.xml` ties unlocks to `craftingElectrician` progression:
  - engine at Electrician level 45+
  - solar bank at level 80+
  - solar cell at level 100
  - battery recipe placement adjusted into Electrician progression tiers

Usage Notes:
- Existing battery banks should be refit with Rechargeable Battery items (`carBatteryCharge`) for this system.
- Stacking for car batteries and solar cells is partially enabled by removing their quality-tier behavior in this setup.

=== AGF-VP-CraftVitamins-v1.1.1 ===
SUMMARY:
- You can craft vitamins after unlocking Medical level 30, same as herbal antibiotics.
- Craftable at campfire with cooking pot, or cheaper at chemistry station.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- `recipes.xml` adds vitamin crafting (`drugVitamins`) in two stations:
  - campfire with cooking pot
  - chemistry station (lower ingredient cost)
- `items.xml` sets `drugVitamins` to unlock through `craftingMedical`.
- `progression.xml` adds vitamins to the same Medical unlock/tags used for herbal antibiotics.
  - Effective unlock point is Medical crafting level 30 (same as herbal antibiotics tier unlock).

Usage Notes:
- Vitamins unlock at Medical crafting level 30, same as herbal antibiotics.

=== AGF-VP-DecorationBlock-v3.0.3 ===
SUMMARY:
- DecorationBlock is an all-in-one mega deco block that uses all in-game block models, with 5,700+ options.
- Includes practical options like storage variants, campfire-usable cookware variants, electrical lights, structural blocks, and more.
- Placed the wrong model? Break it to get the block back.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Use `agfDecorationsVariantHelper` to place from a very large alternate list (5,700+ entries) via `SelectAlternates` and `PlaceAltBlockValue`.
- The helper is crafted at the workbench, and `perkAdvancedEngineering` can reduce ingredient count through `CraftingIngredientCount`.
- Core variants inherit from `agfDecoMaster`, using `DecoMaterial` by default (500 MaxDamage), with dedicated steel-material variants reaching 5000 MaxDamage.
- Secure storage variants use custom `agfStorage` loot sizing (10 x 10) and commonly pair with insecure downgrade/open states.
- Functional variants include campfire-class blocks, powered-light blocks, and many powered door/hatch/gate options.
- Recovery behavior is built in: destroy returns the helper block, and collapse/fall has a high recovery chance.

NOTE
- Localization now covers all supported languages and all blocks used by this mod.

=== AGF-VP-DewsPlus-v2.4.3 ===
SUMMARY:
- Dew collector size is `2x2` wide instead of vanilla `3x3`.
- Dew collector heatmap is removed.
- Break old `3x3` collectors to get the `2x2` version.
- Can be combined into x5 and x25 variants.
- See this mod's README for full details.
DETAIL:
- Works standalone.
Specific XML Edits:
- `blocks.xml` removes `HeatMapStrength`, `HeatMapTime`, and `HeatMapFrequency` from `cntDewCollector`.
- `blocks.xml` adds `cntDewCollectorAGF` (extends `cntDewCollector`) with `MultiBlockDim` set to `2,3,2`.
- `blocks.xml` adds `cntDewCollectorAGFx5` and `cntDewCollectorAGFx25`, including adjusted durability/repair values, self-return destroy/fall drops, and bundled output (`drinkJarRiverWaterx5/x25`, `drinkJarBoiledWaterx5/x25`).
- For `2.6+`, `blocks.xml` requires x5/x25 dew tools on the x5/x25 collectors and sets matching `FuelTypes` values.
- `blocks.xml` replaces vanilla `cntDewCollector` drop behavior so old placed collectors convert to `cntDewCollectorAGF` when broken.
- `recipes.xml` renames the vanilla dew collector recipe output to `cntDewCollectorAGF` and adds workbench combine recipes for x5/x25 collectors.
- `recipes.xml` adds x5/x25 dew tool combine recipes under a pre-`2.6` conditional for compatibility.
- `recipes.xml` adds bulk opening recipes so `500` bundled jars can be opened at once (`x5` and `x25` bundles).
- `progression.xml` updates crafting workstation unlock tags/display entries for `cntDewCollectorAGF`, `cntDewCollectorAGFx5`, and `cntDewCollectorAGFx25`.
- `progression.xml` conditionally adds pre-`2.6` unlock tags/display entries for x5/x25 dew tools.
- `challenges.xml` updates the dew collector placement challenge objective to `cntDewCollectorAGF`.
- `items.xml` adds x5/x25 bundled water items and pre-`2.6` x5/x25 dew tool items.

Usage Notes:
- Existing worlds can convert old placed dew collectors by breaking them, which returns the AGF collector block.
- x5/x25 collectors are designed for scaled output and pair with the matching tool tiers where required.

=== AGF-VP-DoorsPlus-v3.0.1 ===
SUMMARY:
- All door models are selectable in each "All Doors" variant: wood, iron, steel, and powered.
- Search All Doors in crafting menu.
- Doors with barricades also have a "plain" version available.
- Break the door to get it back.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Adds AGF helper recipes in `recipes.xml` for:
  - `miscwoodDoorVariantHelperAGF`
  - `miscironDoorVariantHelperAGF`
  - `miscsteelDoorVariantHelperAGF` (workbench)
  - `miscpoweredDoorVariantHelperAGF` (workbench, learnable, perk-scaled ingredient reduction)
- Wood-tier AGF variants are normalized to 1000 HP in the AGF upgrade chain.
- Wood door helper crafting cost is 10 wood.
- Powered door helper base crafting cost is 10 forged steel, 10 springs, 10 mechanical parts, and 10 electrical parts (with perk-based reductions).
- Upgrade requirements in the AGF chain are:
  - Wood -> Iron: 10 forged iron, 5 upgrade hits.
  - Iron -> Steel: 10 forged steel, 5 upgrade hits.
- Destroyed doors drop their matching helper item (count 1), and fall events also drop it at 75% chance for recovery.
- Wires powered helper unlocks through `progression.xml` under electrician progression, including recipe-tag unlock and display entry integration.
- Updates challenge tracking in `challenges.xml` so the door placement challenge targets AGF door helper placement.
- Extends allowed upgrade item behavior in `items.xml` so door-upgrade actions accept the helper-driven door variant flow.
- Keeps legacy/older door blocks present for save compatibility while providing current AGF helper-driven variant systems.
- Uses variant-helper block architecture (`SelectAlternates` + large `PlaceAltBlockValue` sets) for broad in-game door selection from crafted helpers.
- Includes plain/clean variants (in addition to barricaded/reinforced-style options) so visual style choices are available alongside functional tiers.
- Entirely standalone and server side.

=== AGF-VP-DrinkableAcid-v2.0.2 ===
SUMMARY:
- You can now drink acid for interesting, possibly life ending, effects!
- One drink does about 120 damage over time. Bring a bandage.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Reuses vanilla `resourceAcid` (no separate acid item added).
  - Sets acid `HoldType` for drinking use and swaps mesh to bottled water visuals.
  - Tags acid as `drinks` and adds an `Eat` action with drink timing/sound.
  - Applies `buffDrankAcid` on use and drives duration through CVar logic (`+23` per drink, clamped to `63`).
  - Adds custom `buffDrankAcid` in `buffs.xml` with timer/display CVar handling and auto-removal at 0.
  - Implements buff-side effects in `buffs.xml`: water loss, damage ticks, trippy/infected screen effects, movement/jump buffs, danceoff audio, celebration kill effect, and silly alt sounds.
  - Adds god-mode integration in `buffs.xml` so enabling god mode clears acid duration state.
- Drink ACID for a stacked "fun but dangerous" effect package:
  - Lose about 30% water.
  - Take heavy damage over time (about 120 HP total: large hit on start + ticking damage).
  - Trippy screen effect.
  - "Painting" style screen effect.
  - Fast buff: faster walk/run, plus vehicle torque/speed boost.
  - Jump buff: higher jumps, lower jump stamina cost, and temporary leg-protection buff.
  - Danceoff music loop while active, with record-scratch ending sound.
  - Celebration buff (kills can pop confetti).
  - Silly sounds while active.
- Re-drinking extends duration, capped at 63 second duration (effectively about 3 drinks).
- Entering god mode clears the acid duration and ends the effect.

=== AGF-VP-DyesPlus-v3.1.1 ===
SUMMARY:
- Adds 27 extra dye colors.
- Uses a simple naming theme for the added colors.
- Easy dye swapping: scrap any dye for 15 paint, then craft the color you want for 15 paint.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Core dye loop:
  - Scrap any dye to get 15 paint.
  - Craft dye colors with 15 paint.
- Implementation edits used to make this work:
  - Adds recipe entries in `recipes.xml` for vanilla dyes and 27 added dyes.
  - Keeps recipe cost consistent at 15 paint per dye for easy color swapping.
  - Adds 27 new dye modifiers (bright/dark/light variants) that tint items across clothing, armor, weapons, tools, vehicles, and drones.
  - Extends crafting discoverability by assigning dye modifiers to the Science group, including vanilla dye entries.
- Legacy invisible dye data is retained only for save corruption prevention / existing-save compatibility, and is not a current craftable feature.

=== AGF-VP-FarmingPlus-v5.6.2 ===
SUMMARY:
- Adds a Seed Station for faster seed crafting and management (Unlocks at Seeds 20).
- Adds a plantable Birdnest for eggs and feathers (Unlocks at Seeds 20).
- Adds x5 and x25 variants to seeds/farm plots, plus a "replants itself" variant for each seed.
- x5 and x25 seeds require matching x5/x25 farm plots (except mushrooms and bird nests).
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds a new `seedStation` workstation block and crafting area (`seedStation`) for organized seed workflows.
  - Adds progression wiring so advanced farming unlocks at seed crafting level 20 (including station access and x5/x25 lines).
  - Adds trader + loot integration so farming additions can be naturally discovered.
  - Adds large recipe sets for normal, x5, x25, and replant variants (including station-specific and inventory paths).
  - Adds UI category display groups for seed station tabs: normal, x5, x25, and replants.
  - Adds compatibility conditionals (notably bee flower behavior by game version) to keep updates safer across versions.
- Feature set:
  - Birdnest and Beehives now plantable for eggs, feathers, and honey.
  - Naturally discovered through traders and loot.
  - Unlocks at seed crafting level 20.
  - A new "Seed Station" that offers faster seed crafting and organization.
  - x5 farming at seed crafting level 20:
  - Combine 5 normal seeds into 1 x5 seed recipe.
  - One x5 seed is designed to cover the same planting output as 5 normal seeds.
  - Most x5 seeds (except mushrooms and bird nests) are paired with x5 farm plot usage.
  - Level 3 Living Off the Land reduces seed-station crafting time by 70%.
  - x25 variants are also available.
  - Replant variants are available for each seed:
  - Replants itself.
  - Harvests no extra seed copies.
  - Can be scrapped to convert back close to 1:1.

=== AGF-VP-FloraHarvester-v2.3.1 ===
SUMMARY:
- A tool to more quickly harvest flora, including plants and crops.
- Unlocks at Seeds level 4.
- Living Off the Land perks reduces stamina use.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds a new item: `meleeToolFloraHarvester`.
  - Adds a learnable recipe costing 250 `resourceYuccaFibers`.
  - Adds progression wiring so it unlocks in `craftingSeeds` at level 4.
  - Adds a `perkLivingOffTheLand` effect group for this tool tag to reduce stamina use by level.
  - Adds a custom material (`MresourceYuccaFibersAGF`) to prevent particle/material issues.
  - Sets repair behavior to use Yucca Fibers and configures the tool for effectively no normal durability wear.
- Feature set:
  - Tool is intended for fast harvesting of flora-style targets (plants/crops).
  - Living Off the Land reduces stamina use while using this tool.
  - Unlocks through seed progression and is inexpensive to craft/repair.

=== AGF-VP-FuelBurnPlus-v2.3.1 ===
SUMMARY:
- Combine wood or coal into single fuel items with vanilla burn times: 10m, 60m, 600m, or 6,000m.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds 4 condensed fuel items: `resourceWood10min`, `resourceWood60min`, `resourceWood600min`, and `resourceWood6000min`.
  - Sets fuel values to match the intended burn tiers: 600, 3600, 36000, and 360000.
  - Adds dual input paths for each tier so you can craft from either wood or coal.
  - Uses vanilla-equivalent resource math per tier:
    - 10m = 12 wood or 6 coal.
    - 60m = 72 wood or 36 coal.
    - 600m = 720 wood or 360 coal.
    - 6000m = 7200 wood or 3600 coal.
  - Sets craft XP gain to 0 for all condensed fuel recipes.
  - Uses short craft times (2s, 5s, and 8s tiers) to keep conversion practical.
- Feature set:
  - Lets you condense large amounts of fuel into single items for easier campfire burn planning.
  - Supports both wood-first and coal-first resource storage styles.
  - Keeps conversion values aligned to vanilla burn-time ratios.

=== AGF-VP-MasterTool-v6.1.2 ===
SUMMARY:
- The Master Tool can do the work of an auger, chainsaw, knife, wrench, and nailgun combined, with minimal entity damage output.
- Unlock and craft this tool through a late game path: max key crafting skills, finish Tier 6 trader progression, complete the Master Tool quest, then craft it.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds `meleeMasterTool` with combined tags for mining, salvaging, butchering, repairing/upgrading, and nailgun-style use.
  - Tool stats are tuned for utility: very low entity damage (`EntityDamage` 1) with strong block/salvage damage (`BlockDamage` 50).
  - Adds perk-linked scaling through tags (`perkMiner69r`, `perkMotherLode`, `perkSalvageOperations`, `perkTheHuntsman`) plus harvest-count effects.
  - Adds a workbench recipe for `meleeMasterTool` (120s) requiring:
  - `resourceLegendaryParts` x10
  - `resourceMasterToolElectricals` x1
  - `resourceMasterToolHarvesting` x1
  - `resourceMasterToolSalvaging` x1
  - `resourceMasterToolButchering` x1
  - Adds 4 component recipes at the workbench (250s each, high XP gain), each gated by specific crafting skills.
  - Progression gates component unlocks at:
  - `craftingHarvestingTools` level 100 (Harvesting component)
  - `craftingElectrician` level 100 (Electricals component)
  - `craftingSalvageTools` level 75 (Salvaging component)
  - `craftingBlades` level 75 (Butchering component)
  - Adds a trader mod-job pipeline:
  - Completing `quest_tier6complete` sets `MTJobUnlocked=1`.
  - Trader dialog then reveals the Master Tool mod-job entry.
  - Quest `treasure_MasterTool` is Tier 6, shareable, and rewards `meleeMasterToolSchematic`.
  - Adds custom enemy groups/gamestage spawns for that quest, making the buried-supplies run intentionally high difficulty.
- Feature set:
  - One multi-role endgame tool for mining, salvage, butchering, and repair/upgrade actions.
  - Perk synergy is preserved through tags so related harvesting/mining/salvage/butchering bonuses still apply.
  - Unlock path is intentionally long-form: finish Tier 6 progression, run the trader mission, earn schematic, then craft final tool.
  - Team play is strongly recommended for the quest portion due to the encounter setup.

Find a translation error? Please let me know!

=== AGF-VP-MaxLevel500-v2.0.3 ===
SUMMARY:
- Simply raises the max level cap from 300 to 500... For funsies.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Sets `/progression/level/@max_level` to `500`.
  - Sets player class level requirement value to `500` in `entityclasses.xml`.
- Feature set:
  - Raises the player max level cap from 300 to 500.
  - Keeps both progression and player-class level limits aligned to avoid mismatch issues.

=== AGF-VP-MedicationNoInsectSlow-v1.1.1 ===
SUMMARY:
- Bee Gone Cream unlocks at Medical Crafting level 10 and is crafted at a campfire with a cooking pot.
- Bee Gone Cream removes and prevents Bee/Insect swarm slows, can be used on yourself or other players, and is also findable in the world.
- Steroids also include this anti-slow benefit with matching stat/description display.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds a new item: `medicalInsectCream` (Bee Gone Cream) with primary self-use and secondary use-on-other actions.
  - Adds campfire recipe (`toolCookingPot` required): 4 Aloe, 4 Blueberries, 4 Potatoes, 1 Boiled Water.
  - Adds progression unlock in `craftingMedical` at level 10.
  - Adds custom buff `buffInsectCream` with timed duration handling (about 3 minutes per use, capped at 543 seconds total).
  - Buff logic removes current `buffInjurySlow` and keeps clearing it while active.
  - Adds `noSlowAGF` CVar protection while Bee Gone Cream or Steroids are active.
  - Adds insect attack requirements so Bee/Insect swarm slow effects only apply when `noSlowAGF` is not active.
  - Extends `buffDrugSteroids` to also remove/prevent slow and to share the same `noSlowAGF` protection logic.
  - Adds loot/trader integration (medical loot groups plus trader medicine group), modeled similar to Aloe Cream availability.
  - Adds UI display wiring for `dStopsSlows` and duration on Bee Gone Cream, plus `dStopsSlows` display on Steroids.
  - Adds God-mode cleanup for Bee Gone Cream timer CVar to prevent stale state.
- Feature set:
  - Craft or loot Bee Gone Cream as a dedicated anti-slow medication.
  - Use it on yourself or teammates.
  - Active effect clears current insect slow and prevents new Bee/Insect swarm slows while duration remains.
  - Steroids now provide the same anti-slow protection for consistency.

=== AGF-VP-MiningPlus-v1.2.1 ===
SUMMARY:
- Mining Perk now allows you to bundle clay, sand, and brass.
- The sand you get from mining is very slightly increased.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds 3 new bundle items: `resourceClayLumpBundle`, `resourceCrushedSandBundle`, and `resourceScrapBrassBundle`.
  - Each new bundle extends vanilla bundle behavior and unpacks into 6000 of its base resource.
  - Adds matching crafting recipes (6000 input -> 1 bundle, 2s craft time, 0 craft XP).
  - Extends `perkArtOfMiningPallets` recipe-tag unlock list so the mining book perk unlocks these 3 bundle recipes.
  - Increases sand gain from mining blocks:
  - `terrStone` now drops `resourceCrushedSand` x2 on harvest (default was 0).
  - `terrGravel` sand drop count is set to 11 (default was 8).
- Feature set:
  - Mining-book bundle functionality now includes Clay, Sand, and Brass for easier bulk storage/handling.
  - Sand generation from normal stone/gravel mining is slightly increased.
  - Changes are intentionally lightweight and vanilla-adjacent.

=== AGF-VP-Mod988-v2.0.2 ===
SUMMARY:
- Simply replaces the noose with plain rope, both the block AND the icon.
- This is done for mental health purposes.
- For some, this really matters, and they are worth it.
- ***Suicide Hotline is 988***
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Replaces the `modularRopeNoose` block model with a plain tied-rope model (`modularRopeTopTiedPrefab.prefab`).
  - Overrides the same block's icon to `modularRopeTiled` so inventory/UI visuals match the new in-world model.
  - Scope is intentionally minimal: one block target, model swap + icon swap only.
- Feature set:
  - In-world noose visuals are replaced by plain rope visuals.
  - Block icon presentation is aligned with the visual replacement.
  - Intended as a lightweight comfort/sensitivity adjustment with no gameplay-system overhaul.

=== AGF-VP-ModBundling-v1.0.2 ===
SUMMARY:
- Are you a mod hoarder? Bundle mods to save space.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds a large set of bundle items for mods (`*Bundle` item variants) across armor, gun, melee, vehicle, robotic drone, and utility/radiation mod categories.
  - Each bundle item uses `OpenBundle` behavior on use and returns exactly 1 matching original mod item.
  - Bundle items are configured with stack size 50, non-sellable trader behavior, and bundle-themed icon/model presentation.
  - Adds matching recipes for each bundle item, using simple 1-to-1 conversion (`1 mod -> 1 bundle`) with 0 craft time and 0 craft XP.
  - Current config includes 92 bundle item/recipe definitions.
  - Several legacy or removed-game-version entries are intentionally commented out rather than active (maintained for easier future toggling/reference).
- Feature set:
  - Lets players compress many different mod items into stackable bundle versions to reduce storage clutter.
  - Keeps conversion simple and reversible: craft a bundle, then open it back into the original mod.
  - Focuses on inventory quality-of-life and organization, not stat or combat balance changes.

=== AGF-VP-ModSlotsPlus-v3.0.2 ===
SUMMARY:
- Mod slot overhaul so upgrading to a new item tier does not reduce your available mod slots.
- Quality 6 equipment should always have the most mod slots.
- Highest mod slot count is 6.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Rebalances item ModSlots by replacing specific vanilla slot progression patterns with higher progression curves.
  - Uses targeted set operations against existing ModSlots value patterns (order-sensitive matching).
  - Updated slot pattern mappings:
  - 4,4,4,4,4,4 -> 5,5,5,5,5,6
  - 3,3,3,4,4,4 -> 4,4,4,4,5,6
  - 2,2,3,3,4,4 -> 3,3,3,4,4,5
  - 2,2,2,3,3,4 -> 3,3,3,4,4,5
  - 1,1,2,2,3,4 -> 2,2,3,4,5,6
  - Leaves primitive armor style progression (1,1,1,1,1,1) unchanged.
  - Tier-0 style progression (1,1,1,2,2,3) is present as a commented-out option and not active in this version.
- Feature set:
  - Reduces the chance of feeling like you lose mod capacity when moving to a new item tier.
  - Ensures top-end equipment progression reaches up to 6 slots.
  - Raises both minimum and maximum slot outcomes for many item groups.
  - Highest possible mod slot count remains 6.

*Note: These changes apply to items generated or crafted after installation.*

=== AGF-VP-PaintbrushPlus-v2.1.1 ===
SUMMARY:
- Painting costs ZERO paint.
- You can hold down the trigger to continue painting.
- Painting distance is reduced to prevent accidental long distance painting.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Targets `meleeToolPaintTool` directly in `items.xml`.
  - Enables infinite paint ammo on paint action (`Action1`) via `Infinite_ammo=true`.
  - Enables hold-to-continue painting behavior by setting `BurstRoundCount` to 1000.
  - Sets both paint tool action delays to `0.3` for consistent repeat timing.
  - Sets both paint action ranges (`Action0` and `Action1`) to `6` to reduce accidental long-distance painting.
- Feature set:
  - Painting no longer consumes paint resources.
  - Holding trigger can continue painting without repeated click spam.
  - Painting reach is shortened to keep painting focused on nearby surfaces.
  - Overall behavior is tuned for faster, more controlled paint workflows.

=== AGF-VP-PickupLanternsPlus-v2.0.2 ===
SUMMARY:
- Lanterns, Burning Barrels, Flashlights, and Jack-o-Lanterns can be picked up.
- Lanterns (old and new) and Flashlights are in a single block that you can select different variants from.
- Lanterns are removed from loot and trader lists as you can just pick them up.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds player-pickup flashlight block variants (`*_player`) for standard, corner, and side-centered flashlight models/colors.
  - These player flashlight variants are configured with `CanPickup=true`, 2-second `TakeDelay`, and player-facing creative/filter/group settings.
  - Merges old lanterns + flashlight variants into the main lantern variant helper flow by appending many entries to `lanternDecorLightBlockVariantHelper` `PlaceAltBlockValue`.
  - Updates old lantern pickup return behavior so old lantern pickup routes to `lanternDecorLightBlockVariantHelper`.
  - Enables pickup behavior on key decorative lights by appending pickup properties to:
  - `lanternDecorLightWhite`
  - `burningBarrel`
  - `decoPumpkinJackOLantern`
  - all `flashlightDecor*`, `flashlightSideCenteredDecor*`, and `flashlightCornerDecor*` blocks
  - Removes obsolete/separate recipe entry for `lanternOldBlockVariantHelper` to keep the new helper workflow clean.
  - Removes pickupable lantern helper items from loot and trader lists (`lanternDecorLightBlockVariantHelper` and `lanternOldBlockVariantHelper`).
- Feature set:
  - Lanterns, flashlights, burning barrels, and jack-o-lanterns can be picked up instead of being permanent placements.
  - Old/new lantern and flashlight variants are unified under one variant-helper selection flow.
  - Since these are now pickup-focused, helper items are removed from loot/trader pools to reduce unnecessary duplicate acquisition paths.

=== AGF-VP-PlayerResetQuests-v2.0.2 ===
SUMMARY:
- When speaking with a trader, players can choose to regenerate the quest list.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Uses a trader dialog insertion in `dialogs.xml`.
  - Inserts `response_entry id="resetquests"` into the main trader start response list, directly after the special-jobs entry.
  - This effectively exposes the existing quest-reset dialog option in normal player-facing trader conversation flow.
  - Scope is intentionally minimal: one dialog insertion, no quest-table/stat/perk rewrites.
- Feature set:
  - Players can trigger trader quest-list regeneration through trader dialogue.
  - Useful for refreshing quest offerings without waiting on normal cycle behavior.
  - Keeps the change focused to dialogue availability rather than broader progression systems.

=== AGF-VP-PlayerVendingMachinesPlus-v1.0.1 ===
SUMMARY:
- You can craft a player vending machine easily (10 forged iron).
- They cost the same as magazines (economic value="100")
- Traders sell between 1-4 instead of just 1.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds a direct crafting recipe for `cntVendingMachine`.
  - Recipe cost is 10 `resourceForgedIron`, 10-second craft time, 0 craft XP, with `packMuleCrafting` tag.
  - Sets vending machine block `EconomicValue` to 100 (aligned with magazine-level value target).
  - Sets vending machine block drop count to 1 so breaking the placed machine returns it.
  - Adds anti-exploit/cleanup properties on the vending machine block:
  - `SellableToTrader=false`
  - `NoScrapping=true`
  - Updates trader item counts for `cntVendingMachine` to `1,4` so traders can stock multiple at once.
- Feature set:
  - Players can craft vending machines cheaply and quickly for trade-base setup.
  - Vending machines are easier to source from traders (up to 4 at a time).
  - Value and sell/scrap restrictions reduce abuse paths while keeping placement flexible.
  - Placed machines are recoverable when broken, reducing accidental loss.

=== AGF-VP-PumpkinsPlus-v2.0.5 ===
SUMMARY:
- Throw pumpkins that explode like molotovs.
- Wear a jack-o-lantern as a cosmetic mod on your helmet!
- You can also bundle the jack-o-lantern head mod.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Adds `thrownAmmoMolotovJacks`, a pumpkin-themed throwable with molotov-style burning behavior.
  - Throw item setup includes custom pumpkin icon/mesh/hand mesh, throw animation tuning, and explosive burn properties.
  - On impact, it applies `buffBurningMolotov` area effects with duration handling tied to molotov-related progression checks.
  - Adds recipe: `1 thrownAmmoMolotovCocktail + 1 foodCropPumpkin -> 1 thrownAmmoMolotovJacks`.
  - Adds wearable head cosmetic mod `modJackOHead` in `item_modifiers.xml` (head installable visual attachment).
  - Cosmetic mod attaches a jack-o-lantern prefab to third-person head view and removes/cleans it on equip-stop or first-person handling.
  - Adds recipe: `1 decoPumpkinJackOLantern -> 1 modJackOHead`.
  - Adds optional collector bundle item `modJackOHeadBundle` plus 1-to-1 bundle recipe (`modJackOHead -> modJackOHeadBundle`).
  - Adds custom material entry `Mjackolantern` for pumpkin-themed material categorization.
- Feature set:
  - Adds a throwable pumpkin variant that behaves like a molotov-style fire bomb.
  - Adds a cosmetic jack-o-lantern head mod for helmet visuals.
  - Supports bundling/unbundling of the jack-o-lantern mod for storage/collection.
  - Keeps all additions focused on thematic utility/cosmetic gameplay rather than broad system overhaul.

=== AGF-VP-QuickStart-v2.0.1 ===
SUMMARY:
- You load into the game, automatically redeem initial challenges, and get starter equipment/rewards.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Appends new actions into `game_first_spawn` in `gameevents.xml`.
  - Adds an `AddItems` action during spawn flow to grant quick-start starter resources/equipment.
  - Added starter items include fibers, wood, small stone, stone axe, wooden club, primitive bow, stone arrows, and primitive outfit.
  - Uses configured item counts for immediate early-game startup pacing.
  - Adds a `CompleteChallenge` action in the same spawn sequence.
  - Auto-completes the early challenge chain (`redeemChallenge`, gather/craft/wear starter tasks, and group completion), with force-redeem enabled.
  - Scope is intentionally focused to first-spawn sequence behavior only.
- Feature set:
  - New players spawn in with early baseline tools/resources already granted.
  - Initial challenge redemption/completion is automated instead of manually stepped through.
  - Reduces startup friction so players can begin core gameplay faster.

=== AGF-VP-RebundleBundles-v1.0.1 ===
SUMMARY:
- Rebundle Bundles after opening.
- Bundle icons are now tinted for easier visual differentiation
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Retunes core vanilla bundle recipes to reduce exploit potential and speed up rebundling:
  - Sets `craft_exp_gain=0` on key mining/resource bundle recipes.
  - Sets `craft_time=2` (from longer defaults) on those same bundle recipes.
  - Adds rebundle recipes for commonly opened/consumed bundle categories so items can be bundled again after opening.
  - Added rebundle recipe groups include:
  - resource/ammo-production bundles (`ammoGasCanBundle`, `resourceGunPowderBundle`, `resourceLockPickBundle`)
  - ballistic ammo bundles (9mm, .44, 7.62, shotgun variants)
  - turret ammo bundles
  - arrow and crossbow bolt bundle variants (stone/iron/steel/flaming/exploding)
  - Total new rebundle recipes added: 28.
  - Adds visual differentiation by tinting key bundle icons (`resourceRockSmallBundle`, `resourceLockPickBundle`, `ammoBundleMaster`) with the VP purple tint.
- Feature set:
  - Players can rebundle many common resources and ammo types after opening bundles.
  - Rebundling is fast (2s) and grants no craft XP, reducing abuse while keeping convenience.
  - Bundle visuals are easier to identify at a glance due to consistent tinting.
  - Focus is inventory/stack-management quality-of-life, especially for high-volume ammo/resource handling.

=== AGF-VP-RecipeRottingFlesh-v1.0.1 ===
SUMMARY:
- Adds a recipe for rotting flesh from raw meat and murky water at a campfire with a cooking pot.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Appends a new recipe for `foodRottingFlesh`.
  - Recipe is crafted at the campfire and requires a cooking pot (`craft_area="campfire"`, `craft_tool="toolCookingPot"`).
  - Recipe is tied to the Master Chef progression tag (`tags="perkMasterChef"`).
  - Base ingredients are `foodRawMeat x3` and `drinkJarRiverWater x1` for `foodRottingFlesh x1`.
  - Adds a perk scaling effect that lowers raw meat cost on this recipe via `CraftingIngredientCount` for `foodRawMeat` across level range 1 to 3.
- Feature set:
  - Adds a direct way to craft rotting flesh from common supplies.
  - Keeps crafting in the normal cooking workflow (campfire plus cooking pot).
  - Master Chef investment improves recipe efficiency by reducing meat requirement as perk level increases.

=== AGF-VP-RenamesAlphabeticalSort-v2.0.5 ===
SUMMARY:
- Naming scheme for better sorting when pressing Auto Sort.
- Food and water have values in titles.
- Food items that are also ingredients are indicated in the title.
- Similar item types sort together in tier order (for example, bone knife to machetes).
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Primarily a localization pass in `Config/Localization.txt` that renames many existing item/block/mod entries with sorting-friendly prefixes.
  - Prefix/tag patterns are applied consistently across supported languages so naming intent stays aligned in all locales.
  - Common pattern examples include:
  - category prefixes such as `Ammo[-]`, `Ammo-Ingredient[-]`, `Food-Candy[-]`, `Mod-Armor[-]`, `Mod-Vehicle[-]`, and `Admin[-]`
  - grow-state prefixes such as `Seed`, `Growing`, and `Harvest` for planted blocks
  - armor piece ordering tags like `Armor[bbb0xx]` to keep sets grouped and ordered together
  - Small support changes in `Config/items.xml` adjust item group tags to improve Auto Sort behavior for edge cases (`foodCornMeal`, `resourceCropCottonPlant`, `resourceCropAloeLeaf`, `drinkJarEmpty`).
- Feature set:
  - Auto Sort groups like-items together more reliably in inventory and containers.
  - Item names are easier to scan because category/type is shown at the front of the name.
  - Food/drink and ingredient overlap is easier to identify from naming.
  - This is mostly naming/grouping quality-of-life; gameplay mechanics are not the core target of this mod.

=== AGF-VP-RestorePowerAnyTime-v1.0.3 ===
SUMMARY:
- Restore Power quests can be done at any time of day.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Removes quest XML properties that enforce start/end hour limits:
  - `allowed_start_hour`
  - `allowed_end_hour`
  - Uses broad removal XPaths (`//property[@name='allowed_start_hour']` and `//property[@name='allowed_end_hour']`) so those time-window restrictions are no longer applied.
- Feature set:
  - Restore Power quests are not locked to nighttime windows.
  - Players can start and complete the power restore objective at any time of day.
  - This is a timing-rule quality-of-life change; it does not add new quest content.

=== AGF-VP-ScrapBatts4Acid-v1.1.1 ===
SUMMARY:
- Scrap a car battery for 2 acid; no longer gives lead.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Changes `carBattery` material assignment from lead-scrap behavior to acid-scrap behavior by setting battery material to `MresourceAcid`.
  - Adds/uses a salvage-scrap recipe path for `resourceAcid` (`tags="salvageScrap"`, `always_unlocked="true"`) so scrap conversion can output acid.
  - Sets material forge-category routing for `MresourceAcid` to `resourceAcid`, linking the battery scrap material to acid output.
  - Adjusts item weights (`carBattery` and `resourceAcid`) to tune practical scrap yield behavior around the intended per-battery acid return.
- Feature set:
  - Scrapping car batteries gives acid instead of lead.
  - Target practical outcome is approximately 2 acid per battery under normal scrap-yield rules.
  - This is a resource-conversion quality-of-life/economy tweak focused on acid access.

=== AGF-VP-ScrapEquipmentFaster-v1.0.1 ===
SUMMARY:
- Equipment scrapping takes 4 seconds instead of 10.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - Applies a global XML set on `ScrapTimeOverride` values: `//property[@name='ScrapTimeOverride']/@value` is changed to `4.0`.
  - This replaces the default `10.0` scrap-time override wherever that property exists.
  - The broad `//` XPath means the change is applied across all matching equipment/item entries that use `ScrapTimeOverride`.
- Feature set:
  - Equipment/items using `ScrapTimeOverride` now scrap in 4 seconds instead of 10 seconds.
  - Scrapping flow is noticeably faster with no extra unlocks or new items required.
  - This is a pure time-to-scrap quality-of-life adjustment.

=== AGF-VP-SimplifiedStacks-v1.2.4 ===
SUMMARY:
- Simplified stack sizes to reduce inventory clutter while keeping a vanilla-like feel.
- Simple category tiers: consumables (50), ammo (500), resources (mostly 6000, with some 1000), placeable blocks (500), farming/building blocks (5000), and a few exceptions (30000).
- Existing bundle values are adjusted to match the new stack sizes.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Simplified stack sizes that keeps close to vanilla experience:
  - Bundles and consumables stack to 50.
  - Ammo stacks to 500.
  - Resources are generally 500, 1000, or 6000.
  - Gas, coins, and plant fibers stack to 30000.
  - Placeable blocks (like workstations) stack to 500.
  - Farming and general blocks stack to 5000.
  - Existing bundles are modified to match new stack sizes.
- Implementation edits used to make this work:
  - Applies broad stack-size rebalance across `items.xml`, `blocks.xml`, and `recipes.xml`.
  - Core stack targets used by category:
  - 50 for many bundles/consumables/medicals/food tiers.
  - 500 for most ammo outputs and many frequently used items.
  - 1000 and 6000 tiers for mid/high-volume components and resources.
  - 30000 for select high-volume inventory pressure items (for example gas, dukes/cash, fibers, feathers, paper, nails, paint).
  - Raises many placeable/workstation-type blocks to 500 and many farming/general/building blocks to 5000.
  - Updates bundle behavior to stay consistent with new stack sizes:
  - `Create_item_count` adjustments (for example gas bundle to 30000)
  - ingredient count rebalance for ammo/resource bundle recipes
  - Includes conditional recipe logic for specific game version ranges and optional mod interop (`AGF-VP-ArcheryFeathersChange`, `AGF-VP-RebundleBundles`).
  - Adds economy guardrail tweaks for selected high-stack items (for example lower economic value on some affected items).
- Feature set:
  - Inventory management is more consistent because stack sizes follow a simpler tier model.
  - High-usage materials and ammo require fewer inventory slots while preserving recognizable vanilla progression.
  - Bundles remain practical at new stack limits because recipe inputs/outputs are rebalanced to match.
  - Compatibility-aware conditionals help keep behavior aligned when paired with related AGF mods.

=== AGF-VP-SmeltingPlus-v2.4.1 ===
SUMMARY:
- Forge input has 3 smelting rows.
- Sand smelting is 1:5 instead of 1:4.
- You can now craft single units of clay, sand, and stone. (Helps with OCD!)
- Workstation level 75 unlocks advanced smelting for metals.
- Take 30,000 metal (five stacks of 6000) and craft into a single advanced stack of 6000.
- This advanced stack smelts at 1:5 so one stack fills the forge.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/XUi/windows.xml` sets forge input grid rows to `3`, updates window sizing/positions, and appends numeric labels `1`, `2`, `3`.
  - `Config/XUi/controls.xml` sets forge material table columns to `3`, changes material label justification to right, and inserts a spacer label.
  - `Config/XUi_Common/styles.xml` updates reserve name/weight widths and adds style `workstation.agf` used by the spacer label.
  - `Config/items.xml` sets `resourceCrushedSand` `Weight` to `5`.
  - `Config/recipes.xml` sets recipe `resourceCrushedSand` ingredient `unit_glass` count to `5`.
  - `Config/recipes.xml` adds always-unlocked forge recipes for 1:1 unit conversion of `unit_clay`, `unit_glass`, and `unit_stone`.
  - `Config/items.xml` adds advanced smelting items `resourceScrapIronAdvSmelt`, `resourceScrapBrassAdvSmelt`, and `resourceScrapLeadAdvSmelt`.
  - These advanced items use `Stacknumber 6000` and `UnlockedBy="craftingWorkstations"`.
  - `Config/recipes.xml` adds workbench recipes (`craft_time 4`, `count 6000`, `craft_exp_gain 0`) for advanced iron, brass, and lead smelting items.
  - Brass advanced smelt includes recipes using raw brass, radiators, and coins, plus a conditional brass bundle recipe when `AGF-VP-MiningPlus` is loaded.
  - `Config/progression.xml` appends advanced smelt items to workstation progression and uses game-version conditionals for `1.2.0-1.2.4` and `1.2.5+` unlock entry paths.
  - `Config/items.xml` appends forge icon/economy properties for unit materials (`unit_glass`, `unit_stone`, `unit_clay`, `unit_iron`, `unit_brass`, `unit_lead`).
- Feature set:
  - Forge input window displays three smelting rows.
  - Sand smelting output matches the configured 1:5 unit behavior.
  - 1:1 forge unit conversion recipes are available for clay, glass, and stone.
  - Normal metal smelting is 1:1, so filling 30000 forge capacity takes five full 6000 stacks.
  - Advanced smelting recipes convert those five full stacks (30000 total) into one 6000 advanced stack.
  - The advanced stack smelts at 1:5, so that one stack fills the same 30000 forge capacity.
  - Workstation progression and conditional recipe paths are defined for supported game versions and optional related AGF mods.

=== AGF-VP-StayLongerAnimalCorpse-v2.0.3 ===
SUMMARY:
- Animal Corpses disappear after 10 minutes instead of 5.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/entityclasses.xml` sets `TimeStayAfterDeath` to `600` seconds for entity classes whose names start with `animal`.
  - The XPath excludes names containing `Swarm`, so swarm/insect entries are not included in this change.
  - Swarm corpses are intentionally excluded because they are meant to disappear quickly and can otherwise become hard-to-break land-mine-like leftovers.
- Feature set:
  - Animal corpses stay after death for 10 minutes (`600` seconds) instead of the default 5 minutes (`300` seconds).

=== AGF-VP-StayLongerPlayerBackpack-v2.0.2 ===
SUMMARY:
- Player backpack disappears after 5 hours instead of 1.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/entityclasses.xml` sets `Backpack` `TimeStayAfterDeath` to `18000` seconds.
- Feature set:
  - Player backpack despawn time is 5 hours (`18000` seconds) instead of the default 1 hour (`3600` seconds).

=== AGF-VP-StayLongerZombieLoot-v2.0.2 ===
SUMMARY:
- Zombie loot disappears after 1 hour instead of 20 minutes.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/entityclasses.xml` sets `TimeStayAfterDeath` to `3600` seconds for entity classes whose names start with `EntityLootContainer`.
- Feature set:
  - Zombie loot container despawn time is 1 hour (`3600` seconds) instead of 20 minutes (`1200` seconds).

=== AGF-VP-TacticalRiflePlus-v2.1.1 ===
SUMMARY:
- Tactical Rifle no longer uses default 3-round burst.
- Tactical Rifle magazine increased from 30 to 36.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/items.xml` sets `gunMGT2TacticalAR` `BurstRoundCount` from default `3` to `1000`.
  - `Config/items.xml` sets `gunMGT2TacticalAR` `MagazineSize` from `30` to `36`.
- Feature set:
  - Tactical rifle burst behavior is changed from default 3-round burst settings.
  - Tactical rifle magazine size is 36 instead of 30.

=== AGF-VP-TraderAlwaysOpen-v2.0.2 ===
SUMMARY:
- Traders are always open, except during bloodmoons.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/traders.xml` removes trader `open_time`.
  - `Config/traders.xml` removes trader `close_time`.
- Feature set:
  - Traders are available without normal open/close hour restrictions.
  - Bloodmoon trader lock behavior is handled by vanilla game logic.

=== AGF-VP-TraderCostlierPrices-v1.0.1 ===
SUMMARY:
- Trader buy prices are increased, from default x3 to x10.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/traders.xml` sets `/traders/@buy_markup` to `10`.
  - Default `buy_markup` is `3`.
  - `sell_markdown` is present only as a commented line and is not actively changed.
- Feature set:
  - Trader purchase prices use `buy_markup=10`.

=== AGF-VP-TraderSellsMoreBooks-v3.0.1 ===
SUMMARY:
- A specific magazine can sell between 1-8 copies instead of 1-3.
- Traders can carry 7-13 different magazine types instead of a fixed 7.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/traders.xml` sets `skillMagazines` item count range from `1,3` to `1,8`.
  - `Config/traders.xml` sets trader `skillMagazines` listing count from default `7` to range `7,13`.
- Feature set:
  - A specific magazine can sell between 1-8 copies instead of 1-3.
  - Traders can carry 7-13 different magazine types instead of a fixed 7.

=== AGF-VP-TradersRestockEvery2Days-v1.0.1 ===
SUMMARY:
- Traders restock every 2 days instead of 3.
- Easier to remember and helpful for games with longer daily time.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/traders.xml` sets `trader_info` `reset_interval` from `3` to `2`.
  - This edit is inside a conditional check for `mod_loaded('TradersRestockEvery2Days')`.
- Feature set:
  - Trader restock interval is 2 days instead of the default 3 days.

=== AGF-VP-TreesPlus-v3.0.1 ===
SUMMARY:
- Planting less trees helps performance.
- Can plant x5 and x25 versions of trees.
- Tree names and growth stages are shown for planted trees.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Implementation edits used to make this work:
  - `Config/recipes.xml` adds x5 and x25 crafting recipes for oak, mountain pine, and winter pine tree seeds.
  - `Config/recipes.xml` includes both direct x25 crafting from base seeds and x25 crafting from five x5 seeds.
  - `Config/recipes.xml` adds conditional `seedStation` recipe variants when `AGF-VP-FarmingPlus` is loaded.
  - `Config/blocks.xml` adds x5 and x25 planted tree block chains for oak, mountain pine, and winter pine across growth stages.
  - `Config/blocks.xml` sets higher `MaxDamage` and wood harvest counts for x5 and x25 tree variants relative to base trees.
  - `Config/blocks.xml` adds `DisplayInfo="Name"` on configured tree growth stages so tree name/stage text is shown while planted.
  - `Config/blocks.xml` adds seed-group/icon presentation properties for tree seed items to align with AGF seed grouping patterns.
- Feature set:
  - x5 and x25 tree seed variants are craftable.
  - Planted x5 and x25 trees have scaled durability and wood yield.
  - Tree name/stage display is enabled on configured planted growth stages.
  - FarmingPlus seed-station crafting support is conditionally available when that mod is loaded.

=== AGF-VP-VehiclePerformance-v1.0.2 ===
SUMMARY:
- All vehicles have better acceleration, braking power, handling, hill climbing, and durability.
- Gyrocopter reverse is smoother.
- Speed increases are conservative to support server performance.
- Vehicle speed changes:
  - Bicycle max speed is 10 instead of 8.5.
  - Minibike max speed is 12 instead of 9.2.
  - Motorcycle max speed is 16 instead of 14.
  - 4x4 truck max speed is 16 instead of 14.
  - Gyrocopter max speed in air is 16 instead of 15.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- Config/vehicles.xml
  - velocityMax_turbo:
    - Bicycle: 8,4,10,4 (default 6,4,8.5,4)
    - Minibike: 10,4,12,4 (default 7,4,9.2,4)
    - Motorcycle: 12,8,16,10 (default 9.8,6,14,8)
    - 4x4 Truck: 12,8,16,10 (default 10,8,14,10)
    - Gyrocopter: 10,4,16,6 (default 9,9,15,9)
  - motorTorque_turbo:
    - Minibike: 900,300,1150,300 (default 400,200,560,200)
    - Motorcycle: 5000,500,6000,650 (default 1400,500,2100,650)
    - 4x4 Truck: 6000,1500,7500,2000 (default 3500,1500,4500,2000)
    - Gyrocopter: 10,100,10,100 (default 1,1,2,2)
  - brakeTorque:
    - Bicycle: 5000 (default 3000)
    - Minibike: 6000 (default 3000)
    - Motorcycle: 10000 (default 3000)
    - 4x4 Truck: 10000 (default 6000)
  - upAngleMax:
    - Bicycle: 80 (default 70)
    - Minibike: 80 (default 70)
    - Motorcycle: 90 (default 70)
    - 4x4 Truck: 90 (default 70)
  - Handling:
    - Bicycle, Minibike, Motorcycle
    - tiltDampening: 0.5 (default 0.22)
    - tiltThreshold: 5 (default 3)
    - tiltDampenThreshold: 80 (default 8)

- Config/items.xml DegradationMax:
  - Gyrocopter: 7000 (default 3500)
  - Bicycle: 4000 (default 1500)
  - Minibike: 4000 (default 2000)
  - Motorcycle: 8000 (default 4000)
  - 4x4 Truck: 16000 (default 8000)

=== AGF-VP-VehiclesExtraSeating-v1.0.1 ===
SUMMARY:
- 2 wheel vehicles can seat 3 people with or without expanded seating mod.
- Truck can seat up to 8 players and does require the expanded seating mod.
- See this mod's README for full details.
DETAIL:
- Works standalone.

- `Config/vehicles.xml` adds or remaps seat classes on four vehicles:
  - Bicycle:
    - Adds `seat1` and `seat2` blocks under `vehicleBicycle`.
    - Positioning was accounted for by setting seat pose, seat position, seat rotation, exit points, and IK hand/foot placement for the added seats.
    - Result: 3 total seats.
  - Minibike:
    - Removes seat-mod requirement from `seat1`.
    - Adds `seat2`.
    - Positioning was accounted for by setting seat pose, seat position, seat rotation, exit points, and IK hand/foot placement for the added seat.
    - Result: 3 total seats.
  - Motorcycle:
    - Removes seat-mod requirement from `seat1`.
    - Adds `seat2`.
    - Positioning was accounted for by setting seat pose, seat position, seat rotation, exit points, and IK hand/foot placement for the added seat.
    - Result: 3 total seats.
  - 4x4 Truck:
    - Reassigns `seat5` to `seat7` and `seat4` to `seat6`.
    - Appends new `seat4` and `seat5` definitions.
    - New `seat5` keeps `<property name="mod" value="seat"/>`.
    - Positioning was accounted for by setting seat pose, seat position, seat rotation, exit points, and IK hand/foot placement for both added seats.
    - Result: 8 total seats, with seat-mod requirement still present on at least one added seat.

=== AGF-VP-VehicleStoragePlus-v2.1.2 ===
SUMMARY:
- Vehicles are each given +2 rows of storage (except truck).
- Truck storage is already capped at 90 slots due to code limits.
- See this mod's README for full details.
DETAIL:
- Works standalone.

- `Config/loot.xml` changes vehicle storage by setting `/lootcontainers/lootcontainer/@size`.

- Conditional behavior by game version:
  - `game_version() >= 1.2.0 and < 1.2.5`
    - Bicycle: `9,4` (default `9,1`)
    - Minibike: `9,6` (default `9,3`)
    - Motorcycle: `9,8` (default `9,4`)
    - 4x4 Truck: `9,10` (default `9,9`)
    - Gyrocopter: `9,8` (default `9,5`)
  - `game_version() >= 1.2.5`
    - Bicycle: `10,3` (default `10,1`)
    - Minibike: `10,5` (default `10,3`)
    - Motorcycle: `10,7` (default `10,4`)
    - 4x4 Truck: `10,9` (default `10,9`)
    - Gyrocopter: `10,7` (default `10,5`)

- Notes:
  - The file enforces a max grid size note of `9,10` or `10,9` (90 slots).
  - On modern versions, truck storage remains at the vanilla cap of 90 slots while other vehicles are increased.

=== AGF-VP-WriteStoryOnCrate-v1.0.1 ===
SUMMARY:
- Iron Writeable Storage Crate with 8 lines of writable text.
- See this mod's README for full details.
DETAIL:
- Works standalone.

- `Config/blocks.xml`
  - Adds `agfMessageCrate` (player-placeable) and `agfMessageCrateInsecure`.
  - Both use `TEFeatureSignable` with:
    - `FontSize=45`
    - `LineCount=8`
    - `LineWidth=0.6`
  - `agfMessageCrate` includes `TEFeatureLockable` and downgrades to `agfMessageCrateInsecure`.
  - Both crate variants use `TEFeatureStorage` with loot list `agfMessageCrate`.

- `Config/loot.xml`
  - Defines loot container `agfMessageCrate` with size `6,6` (36 slots).

- `Config/recipes.xml`
  - Adds recipe: `agfMessageCrate` x1 from `resourceLeather` x7.

=== AGF-VP-XPDeathPenaltyReduction-v2.0.1 ===
SUMMARY:
- Each death penalty is 10% up to a max of 30%, instead of 25% up to 50%.
- See this mod's README for full details.
DETAIL:
- Works standalone.

- `Config/entityclasses.xml`
  - Sets `ExpDeficitPerDeathPercentage` for `playerMale` to `.1` (10% per death).
  - Sets `ExpDeficitMaxPercentage` for `playerMale` to `.3` (30% max).
  - Vanilla comparison noted in config comments: per-death value `.25` and max `.5`.

=== AGF-VP-ZombieBiomeSpawning-v3.0.1 ===
SUMMARY:
- Biome Spawning essentially increased by x5, while animal spawning is tuned differently.
- The point? Gives a server something for newcomers and pros alike.
- See this mod's README for full details.
DETAIL:
- Works standalone.

- `Config/spawning.xml`
  - Removes vanilla biome spawn blocks with `<remove xpath="/spawning/biome"/>` and replaces them with a custom biome spawn set.
  - Rebuilds spawn tables for all core biomes: `pine_forest`, `burnt_forest`, `desert`, `snow`, and `wasteland`.
  - Uses district-based spawn routing with `tags`/`notags` values (for example: `commercial`, `industrial`, `downtown`) so spawn pressure scales by location type.
  - Zombie population is increased progressively by biome/district tier (commonly around x5 cap values versus listed vanilla defaults in comments).
    - Example caps by biome tier:
      - `pine_forest` up to `15`
      - `burnt_forest` and `desert` up to `20` in higher-pressure districts/night entries
      - `snow` and `wasteland` up to `20` across high-pressure entries
  - Animal spawning is tuned separately from zombies using dedicated wild/enemy animal entity groups per biome and time window.
  - Night entries generally use shorter respawn delays than day entries, increasing sustained nighttime pressure.

=== AGF-VP-ZombieCorpseLeaveQuicker-v2.0.2 ===
SUMMARY:
- Zombie Corpses disappear after 10 seconds instead of 30, for performance purposes.
- See this mod's README for full details.
DETAIL:
- Works standalone.

- `Config/entityclasses.xml`
  - Sets `TimeStayAfterDeath` from `30` to `10` for entity classes, reducing corpse stay duration after death.
  - Purpose noted in config comment: improve performance by clearing zombie corpses faster.

=== AGF-VP-ZombieLargerHordes-v2.0.2 ===
SUMMARY:
- Wandering hordes for zombies increased roughly x3.
- Wandering hordes for animals increased by smaller amounts.
- See this mod's README for full details.
DETAIL:
- Works standalone.

- `Config/gamestages.xml`
  - Updates `WanderingHorde` spawn counts for gamestages `01` through `50`.
  - Most core zombie stages are raised to about 3x vanilla values (for example: `05 -> 15`, `10 -> 30`, `11 -> 33`, `02 -> 6`).
  - Animal/special-group stages are also increased, but with smaller multipliers than core zombie stages.
    - `ZombieDogGroup`: `6 -> 10` and `7 -> 12`
    - `VultureGroup`: `6 -> 9` and `8 -> 12`
    - `ZombieAnimalsGroup`: `5 -> 9`
    - `WolfPack`: `5 -> 9`
    - `ZombieBearsGroup`: `2 -> 6`

=== zzzAGF-Special-Compatibilities-v4.1.1 ===
SUMMARY:
- This mod is used to provide compatibility patches between AGF mods and other mods.
- Requests for more compatibility can be given.
- See this mod's README for full details.
DETAIL:
- Works standalone.
- This mod provides compatibility patches for AGF mods with other common modlets.
- Common AGF mods covered by this compatibility pack include:
    - HUDPlus
    - PurpleBook
    - RenamesforAlphabeticalSort
    - SimplifiedStackSizes
    - AmmoDisassembly
    - RebundleBundles
- Language support patches include:
    - BDub's Vehicles
    - IZY's Weapons
    - GS Vanilla Cook Book
- Current compatibility list includes:
    - Dewtas' 18 slot toolbelt
    - Khaine's 15 slot toolbelt
    - Wookie's 12 slot toolbelt
    - RecipeStatsTab
    - BDub's Vehicles
    - IZY's Weapons
    - MoreQuests
    - Oakraven Ammo Press
    - GSVanillaCookBook
- More compatibility patches can be added by request.

