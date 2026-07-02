AGF-VP-DewsPlus
7d2d Version 3
Version: 2.4.3  
Download: https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04DownloadZips/AGF-VP-DewsPlus.zip

 

========================================

========================================

README TABLE OF CONTENTS
1. About Author
2. Mod Philosophy
3. Need Help?
4. Install Scope & EAC Requirement
5. Compatibility
6. Features Summary
7. Features Details
8. Changelog
9. Install Scope & EAC Requirement Guide
10. Installation Guide
11. Removal Guide
12. Update Guide
13. Backup Guide

========================================

========================================

1. About AGF
- AuroraGiggleFairy (AGF) creates accessibility-focused, vanilla-enhancing mods for 7 Days to Die.
- Goal is to deliver practical, easy-to-use features shaped by community feedback.
- Main site and first release source: auroragigglefairy.github.io: https://auroragigglefairy.github.io/
- Discord is best for latest updates, fastest contact, and becoming a tester: discord.gg/Vm5eyW6N4r: https://discord.gg/Vm5eyW6N4r

========================================

========================================

3. Need Help?
- AuroraGiggleFairy is available via Discord: https://discord.gg/Vm5eyW6N4r
- All questions welcome from newcomers and seasoned players alike.

========================================

========================================

4. Install Scope & EAC Requirement
- Server-Side (EAC-Friendly): Server install works for all joining players; EAC on or off.

========================================

========================================

5. Compatibility
- Last 7d2d Version tested on: 3
- "AGF-VP-DewsPlus" is SAFE to install on an existing game.
- "AGF-VP-DewsPlus" is DANGEROUS to remove from an existing game.
- Unique Details: None

========================================

========================================

6. Features Summary

- Dew collector size is 2x2 wide instead of vanilla 3x3.
- Dew collector heatmap is removed.
- Break old 3x3 collectors to get the 2x2 version.
- Can be combined into x5 and x25 variants.
- See this mod's README for full details.

========================================

========================================

7. Features Details

- Works standalone.
Specific XML Edits:
- blocks.xml removes HeatMapStrength, HeatMapTime, and HeatMapFrequency from cntDewCollector.
- blocks.xml adds cntDewCollectorAGF (extends cntDewCollector) with MultiBlockDim set to 2,3,2.
- blocks.xml adds cntDewCollectorAGFx5 and cntDewCollectorAGFx25, including adjusted durability/repair values, self-return destroy/fall drops, and bundled output (drinkJarRiverWaterx5/x25, drinkJarBoiledWaterx5/x25).
- For 2.6+, blocks.xml requires x5/x25 dew tools on the x5/x25 collectors and sets matching FuelTypes values.
- blocks.xml replaces vanilla cntDewCollector drop behavior so old placed collectors convert to cntDewCollectorAGF when broken.
- recipes.xml renames the vanilla dew collector recipe output to cntDewCollectorAGF and adds workbench combine recipes for x5/x25 collectors.
- recipes.xml adds x5/x25 dew tool combine recipes under a pre-2.6 conditional for compatibility.
- recipes.xml adds bulk opening recipes so 500 bundled jars can be opened at once (x5 and x25 bundles).
- progression.xml updates crafting workstation unlock tags/display entries for cntDewCollectorAGF, cntDewCollectorAGFx5, and cntDewCollectorAGFx25.
- progression.xml conditionally adds pre-2.6 unlock tags/display entries for x5/x25 dew tools.
- challenges.xml updates the dew collector placement challenge objective to cntDewCollectorAGF.
- items.xml adds x5/x25 bundled water items and pre-2.6 x5/x25 dew tool items.

Usage Notes:
- Existing worlds can convert old placed dew collectors by breaking them, which returns the AGF collector block.
- x5/x25 collectors are designed for scaled output and pair with the matching tool tiers where required.

========================================

========================================

8. Changelog

v2.4.3
- ReadMe Format Update.

v2.5.1
- Corrected a conditional that allowed v2.6 stuff in earlier versions!

v2.4.1
- Corrected a conditional error that prevented games from loading.

v2.4.0
- Updated for 7d2d version 2.6.
- Now has x5 and x25 versions of the tools that go into the dew collector.
- Naming scheme update.
- Updated README format

v2.3.2
- A bulk crafting and open was missing a digit. Only gaining 250 water instead of 2,500.

v2.3.1
- updated for 2.5
- dew collectors craft at workbench.

v2.3.0
- Removed HEAT production on all dew collectors.
- Added a x25 version!
- Increased the bundle stack sizes of the larger water bundles from 50 to 500.

v2.2.1
- Breaking a x5 dew collector returns it to your hands, just like the regular one.

v2.2.0
- Will now only apply purple book edits if you have my purple book installed.

v2.1.0
- If you have 50 jugs, you can open all of them in the crafting menu.

v2.0.3
- updated for 7d2d Version 2

v2.0.2
- changed the bulk murky water icon to be consistent
- gave this icon a murky water color-ish

v2.0.1
- corrected the Challenge of placing a dew collector.

v2.0.0
- Updated for V1.0
- New method of x5 due to tools for dews

v1.1.4
- Added in windows.xml to make sure the dew collector is the correct name on the schematic list.
- Fixed the sorting of the dew collector types.
- Made the x5 dew collector unlock with the regular one.

v1.1.3
- Corrected the name of what gets unlocked for Dew Collector's under progression.xml.
- Removed the "third" recipe for a dew collector that was unintentionally there.

V1.1.2
- Added the number "5" to the icon type.

v1.1.1
- Fixed the recipe to the correct x5 dew collector.

v1.1.0
- Dew collectors now 2x2, except the old ones, which if you destroy, you get the 2x2 version.
- changed mod name to DewsPlus

v1.0.0
- Created the mod.

========================================

========================================

9. Install Scope & EAC Requirement Guide

========================================

This guide explains where to install a mod and whether it is EAC friendly.

========================================

A. Install on Server, Client, or Both?
  - Server - where the game is hosted. This could be your own PC if you are hosting the game yourself, or a game hosting service like Pingperfect.
  - Client - your own PC.
- Some mods only need to be in one place. Others need to be in both. Please see the Mod Types section below for exact requirements.

========================================

B. EAC Friendly?

- EAC stands for Easy Anti-Cheat. It's a program built into 7 Days to Die that helps protect multiplayer sessions from cheating.
- Mod Type 1 is EAC friendly. Mod Types 2, 3, and 4 require EAC to be turned off.
- Running without EAC opens up a wider range of mods and experiences. If you're running a multiplayer server without EAC, here are some good practices to keep things running smoothly:
  - Recommended practices when running multiplayer with EAC off:
    - Require a password and be conservative in distributing it.
    - There are tools available such as ALOC and Server Tools to add extra protections.
    - Optionally, require the whitelist system for the strictest limitation on who can join.
    - Seek out other server hosts and discuss what they do.
    - You can find help on AGF's Discord: DISCORD: https://discord.gg/Vm5eyW6N4r

========================================

C. Mod Types

| # | Mod Type | What It Means |
|---|----------|---------------|
| 1 | Server-Side (EAC-Friendly) | Server install works for all joining players; EAC on or off. |
| 2 | Server-Side (EAC Off) | EAC off required; server install works for all joining players. |
| 3 | Server/Client-Side (Required) | EAC off required; host and joining players must install it. |
| 4 | Client-Side (Only) | EAC off required; server install has no effect; only the installing player gets the feature. |

========================================

========================================

10. Installation Guide

========================================

⚠️ IMPORTANT ⚠️ Make a BACKUP!
- If you are making changes to an existing game, ALWAYS make a backup first.
- (See the backup instructions further below.)

========================================

A. Special Note
- The mod named "0TFPHarmony" is REQUIRED and should never be removed.
- If it is missing, you can restore it by verifying your installation:
   - In Steam, right click on 7 Days to Die
   - Select "Properties"
   - Select "Installed Files"
   - Click on "Verify integrity of game files" and wait for completion.

========================================

B. Install to Your PC (Singleplayer, Hosting Friends, or Client-Required Mods)
1. In Steam, right click 7 Days to Die
2. Select Manage, then Browse local files
3. Open the Mods folder
4. Extract the mod into this Mods folder
   - Final folder should look like: Mods//ModInfo.xml
   - If the zip creates an extra parent folder, move the mod folder up one level
5. Keep 0TFPHarmony in this folder
   - It comes with the game and should remain in Mods

========================================

C. Install to a Dedicated Server (hosted sites or player-run)
1. Find the main server folder
   - If using a hosted site, use their file manager or a program like FileZilla
2. Open or create the Mods folder
3. Extract the mod into this Mods folder
   - Final folder should look like: Mods//ModInfo.xml
   - If the zip creates an extra parent folder, move the mod folder up one level
4. Fully restart the server after install

========================================

========================================

11. Removal Guide

========================================

⚠️ IMPORTANT ⚠️ Make a BACKUP First!

========================================

A. Special Note
- The mod named "0TFPHarmony" is REQUIRED and should never be removed.
- If it is missing, you can restore it by verifying your installation:
   - In Steam, right click on 7 Days to Die
   - Select "Properties"
   - Select "Installed Files"
   - Click on "Verify integrity of game files" and wait for completion.

========================================

B. How do I remove mods?
- The safest approach is to only remove mods when starting a new game.
- ALSO smart to make a backup of the game (see below for instructions if needed).
    - If you remove a mod that added new items or features, characters and/or the map may reset, or become permanently unplayable.
    - If you are unsure, check the mod's readme for specific removal notes or ask in AGF's DISCORD: https://discord.gg/Vm5eyW6N4r.
- To remove, simply locate the mod folder and delete it. All done!

========================================

========================================

12. Update Guide

========================================

⚠️ IMPORTANT ⚠️ Make a BACKUP First!

========================================

A. Special Note
- The mod named "0TFPHarmony" is REQUIRED and should never be removed.
- If it is missing, you can restore it by verifying your installation:
   - In Steam, right click on 7 Days to Die
   - Select "Properties"
   - Select "Installed Files"
   - Click on "Verify integrity of game files" and wait for completion.

========================================

B. Updating individual AGF Mods
1. Shutdown the game.
2. Make a backup. (instructions below)
3. Install the new version as usual. (see above)
4. Delete the older version.
   - In your Mods folder, you will see two folders for the mod, each with its version number.
5. THEN you may turn the game back on.

========================================

C. Updating entire AGF Pack
1. Shutdown the game.
2. Make a backup. (instructions below)
3. Carefully delete all AGF mods. (don't forget the zzzAGF mod)
4. Install the new package as usual. (see above)
5. THEN you may turn the game back on.

========================================

========================================

13. Backup Guide

========================================

Having multiple backups are best when experimenting or hosting.

========================================

A. Backing Up Your Singleplayer or Local Game
1. Open "Run" (Windows key + R)
2. Type %appdata% and click OK
3. Open the "Roaming" folder
4. Open "7DaysToDie"
5. Open "Saves"
6. Find your World Name folder (e.g., "Navezgane")
   - You can check your world name in the game's "Continue Game" or "Join Server" menu.
7. Select your Game Name folder (inside the World Name folder)
   - The Game Name is also shown in the game menus.
8. Copy the entire Game Name folder and paste it somewhere safe
   - (like your Desktop or another safe location).

========================================

B. Dedicated Server Game
1. Open the server's "Saves" folder
   - Its location varies by host, but it's typically somewhere in the main dedicated server folder.
   - If you cannot find it, check your host's documentation or ask support.
2. Find your World Name folder (e.g., "Navezgane")
   - You can check your world name in the game's "Join Server" menu or "Server Config".
3. Select your Game Name folder (inside the World Name folder)
   - The Game Name is also shown in the game's "Join Server" menu or "Server Config".
4. Copy the entire Game Name folder and paste it somewhere safe
   - (like your Desktop or another safe location).

========================================

C. To Restore a Backup
- Keep your backup until you're sure everything works!
1. Undo any mod changes
2. Delete the current Game Name folder (in "Saves")
3. Move your backup folder back into "Saves"

========================================

========================================