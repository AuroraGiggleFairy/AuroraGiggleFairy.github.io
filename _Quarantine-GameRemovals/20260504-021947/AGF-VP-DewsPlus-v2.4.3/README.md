# AGF-VP-DewsPlus
7d2d Version 2  
**Version:** 2.4.3  
[Download](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips/AGF-VP-DewsPlus.zip)

 

---
---

## README TABLE OF CONTENTS
1. About Author
2. Mod Philosophy
3. Need Help?
4. Mod Type
5. Compatibility
6. Features Summary
7. Features Details
8. Changelog
9. Important Mod Details
10. Installation Guide
11. Removal Guide
12. Update Guide
13. Backup Guide

---
---

## 1. About Author
- My name is AuroraGiggleFairy (AGF), previously known as RilesPlus
- Started playing 7d2d during Alpha 8
- Started attempting to mod in Alpha 17
- First published a mod during Alpha 18
- Where to find:
  - https://discord.gg/Vm5eyW6N4r
  - https://auroragigglefairy.github.io/
  - https://www.twitch.tv/AuroraGiggleFairy
  - https://7daystodiemods.com/
  - https://www.nexusmods.com/7daystodie

---
---

## 2. Mod Philosophy
- Preferably easy installation and use!
- Goal: Enhance Vanilla Gameplay!
- Feedback and Testing is Beneficial!
- Detailed Notes for Individual Preference and Mod Learning!
- Accessibility is Required
- All 13 Languages Supported (best I can.)

> "The best mods rely on community involvement."

---

### **Language Support**
> - 7 Days to Die currently supports 13 languages: English, German, Spanish, French, Italian, Japanese, Koreana, Polish, Portuguese, Russian, Turkish, Simplified Chinese, and Traditional Chinese.  
> - *AGF mods add support for all 13 languages.*
> - *If you find a translation error, please let AGF know on [DISCORD](https://discord.gg/Vm5eyW6N4r).*

---
---

## 3. Need Help?
- AuroraGiggleFairy is available via Discord: https://discord.gg/Vm5eyW6N4r
- All questions welcome from newcomers and seasoned players alike.

---
---

## 4. Mod Type
- Server-side (EAC-friendly): Server install works for all joining players; EAC on or off.

---
---

## 5. Compatibility
- Last 7d2d Version tested on: 2.6
- "AGF-VP-DewsPlus" is SAFE to install on an existing game.
- "AGF-VP-DewsPlus" is DANGEROUS to remove from an existing game.
- Unique Details: None

---
---

## 6. Features Summary
<!-- FEATURES-SUMMARY START -->
- Dew collector size is `2x2` wide instead of vanilla `3x3`.
- Dew collector heatmap is removed.
- Break old `3x3` collectors to get the `2x2` version.
- Can be combined into x5 and x25 variants.
- See this mod's README for full details.
<!-- FEATURES-SUMMARY END -->

---
---

## 7. Features Details
<!-- FEATURES-DETAILED START -->
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
<!-- FEATURES-DETAILED END -->

---
---

## 8. Changelog
<!-- CHANGELOG START -->
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
<!-- CHANGELOG END -->

---
---

## 9. Important Mod Details

---

### A. What are 🖧 Server-Side 🖧 Mods? 
> - *Mods that can be installed on **JUST** the server and will automatically work for joining players.*
> - *Don’t let the word “server” confuse you—these mods also work in singleplayer if you install them on your own device.*

---

### B. What are 🖥️ Client-Side 🖥️ Mods? 
> - *For multiplayer, both the **HOST** and **PLAYER** must have these mods installed.*
> - *For singleplayer, just install them on your own device.*

---

### C. What are 🛡️ EAC-Friendly 🛡️ Mods? 
> - *EAC stands for Easy-Anti-Cheat.*
> - *EAC-Friendly means the mod will work with it enabled or disabled.* 
> - *If a mod is not EAC-Friendly, EAC must be turned off for it to work.*
>    - *Non-EAC-Friendly mods often have custom code.*

#### 1. What are the risks of disabling EAC?
> - *Players may use hacks or cheats to gain unfair advantages.*
> - *Servers may be at higher risk for exploits, griefing, or disruptive behavior.*

#### 2. Why disable EAC?
> - *Non-EAC-Friendly mods add amazing features and enhancements to the game.*

#### 3. What are the best practices when running multiplayer with EAC disabled?
> - *Require a password and be conservative in distributing it.*
> - *There are tools available such as ALOC and Server Tools to add extra protections.*
> - *Optionally, require the whitelist system for the strictest limitation on who can join.*
> - *Seek out other server hosts and discuss what they do.*
>    - *You can do this on AGF's Discord as well: [DISCORD](https://discord.gg/Vm5eyW6N4r)*

---
---

## 10. Installation Guide

---

> ⚠️ **IMPORTANT** ⚠️ Make a BACKUP!
> - If you are making changes to an existing game, ALWAYS make a backup first!  
> - *(See the backup instructions further below.)*

---

### A. Special Note
> - The mod named **"0_TFP_Harmony"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - *In Steam, right click on 7 Days to Die*
>    - *Select "Properties"*
>    - *Select "Installed Files"*
>    - *Click on "Verify integrity of game files" and wait for completion.*

---

### B. Singleplayer, Player-to-Player Multiplayer, or Client-Side Requirement
> 1. **Open "Run"** *(Windows key + R)*
> 2. **Type `%appdata%` and click OK**
> 3. **Open the "Roaming" folder**
> 4. **Open "7DaysToDie"**
> 5. **Open or create the "Mods" folder**
> 6. **Extract the mod here**
>    - *Make sure the mod folder is not inside another folder with the same name (move it up if needed)*

---

### C. Multiplayer Dedicated Server *(hosted sites or player-run)*
> 1. **Find the main server folder**
>    - *If using a hosted site, use their file manager or a program like FileZilla.*
> 2. **Open the "Mods" folder**   
> 3. **Extract the mod here**
>    - *Make sure the mod folder is not inside another folder with the same name (move it up if needed)*

---
---

## 11. Removal Guide

---

> ⚠️ **IMPORTANT** ⚠️ Make a BACKUP First!

---

### A. Special Note
> - The mod named **"0_TFP_Harmony"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - *In Steam, right click on 7 Days to Die*
>    - *Select "Properties"*
>    - *Select "Installed Files"*
>    - *Click on "Verify integrity of game files" and wait for completion.*

---

### B. How do I remove mods?
> - The safest approach is to only remove mods when starting a new game.
> - ALSO smart to make a backup of the game (see below for instructions if needed).
>     - If you remove a mod that added new items or features, characters and/or the map may reset, or become permanently unplayable.
>     - If you are unsure, check the mod's readme for specific removal notes or ask in AGF's [DISCORD](https://discord.gg/Vm5eyW6N4r).
> - To remove, simply locate the mod folder and delete it. All done!

---
---

## 12. Update Guide

---

> ⚠️ **IMPORTANT** ⚠️ Make a BACKUP First!

---

### A. Special Note
> - The mod named **"0_TFP_Harmony"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - *In Steam, right click on 7 Days to Die*
>    - *Select "Properties"*
>    - *Select "Installed Files"*
>    - *Click on "Verify integrity of game files" and wait for completion.*

---

### B. Updating individual AGF Mods
> 1. **Shutdown the game.** 
> 2. **Make a backup.** *(instructions below)*
> 3. **Install the new version as usual.** *(see above)* 
> 4. **Delete the older version.**
>    - *In your Mods folder, you will see two folders for the mod, each with its version number.*
> 5. **THEN** you may turn the game back on.

---

### C. Updating entire AGF Pack
> 1. **Shutdown the game.**
> 2. **Make a backup.** *(instructions below)*
> 3. **Carefully delete all AGF mods.** *(don't forget the zzzAGF mod)*
> 4. **Install the new package as usual.** *(see above)*
> 5. **THEN** you may turn the game back on.

---
---

## 13. Backup Guide

---

> *Having multiple backups are best when experimenting or hosting.*

---

### A. Backing Up Your Singleplayer or Local Game
> 1. **Open "Run"** *(Windows key + R)*
> 2. **Type `%appdata%` and click OK**
> 3. **Open the "Roaming" folder**
> 4. **Open "7DaysToDie"**
> 5. **Open "Saves"**
> 6. **Find your World Name folder** *(e.g., "Navezgane")*
>    - *You can check your world name in the game’s "Continue Game" or "Join Server" menu.*
> 7. **Select your Game Name folder** *(inside the World Name folder)*
>    - *The Game Name is also shown in the game menus.*
> 8. **Copy the entire Game Name folder and paste it somewhere safe**
>    - *(like your Desktop or another safe location)*.

---

### B. Dedicated Server Game
> 1. **Open the server's "Saves" folder**
>    - *Its location varies by host, but it's typically somewhere in the main dedicated server folder.*
>    - *If you cannot find it, check your host's documentation or ask support.*
> 2. **Find your World Name folder** *(e.g., "Navezgane")*
>    - *You can check your world name in the game’s "Join Server" menu or "Server Config".*
> 3. **Select your Game Name folder** *(inside the World Name folder)*
>    - *The Game Name is also shown in the game's "Join Server" menu or "Server Config".*
> 4. **Copy the entire Game Name folder and paste it somewhere safe**
>    - *(like your Desktop or another safe location)*.

---

### C. To Restore a Backup
> - *Keep your backup until you’re sure everything works!*
> 1. **Undo any mod changes**
> 2. **Delete the current Game Name folder** *(in "Saves")*
> 3. **Move your backup folder back into "Saves"**

---
---