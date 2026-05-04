# AGF-VP-SmeltingPlus
7d2d Version 2  
**Version:** 2.4.1  
[Download](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips/AGF-VP-SmeltingPlus.zip)

 

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
- "AGF-VP-SmeltingPlus" is SAFE to install on an existing game.
- "AGF-VP-SmeltingPlus" is DANGEROUS to remove from an existing game.
- Unique Details: None

---
---

## 6. Features Summary
<!-- FEATURES-SUMMARY START -->
- Forge input has 3 smelting rows.
- Sand smelting is 1:5 instead of 1:4.
- You can now craft single units of clay, sand, and stone. (Helps with OCD!)
- Workstation level 75 unlocks advanced smelting for metals.
- Take 30,000 metal (five stacks of 6000) and craft into a single advanced stack of 6000.
- This advanced stack smelts at 1:5 so one stack fills the forge.
- See this mod's README for full details.
<!-- FEATURES-SUMMARY END -->

---
---

## 7. Features Details
<!-- FEATURES-DETAILED START -->
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
<!-- FEATURES-DETAILED END -->

---
---

## 8. Changelog
<!-- CHANGELOG START -->
v2.4.1
- ReadMe Format Update.

v2.4.0
- Removed as the purple book conditional within windows.xml as it is now within the purple book mod.

v2.3.0
- Now works appropriately with version 2.5 (and previous game versions).
- Updated progression.xml and windows.xml.

v2.2.2
- Added hopefully a clearer description of what the 1:5 metals are.

v2.2.1
- Added conditional for if someone has Mining Plus installed or not.

v2.2.0
- Will now only apply purple book edits if you have my purple book installed.

v2.1.2
- expanded possible ways of crafting the adv smelt stacks, like from radiators and bundled stacks.

v2.1.1
- Had the ratio numbers written in reverse.... *facepalm*

v2.1.0
- added advanced smelting metals

v2.0.1
- Removed the tint color on the "units"

v2.0.0
- Updated to V1.0
<!-- CHANGELOG END -->

---
---

## 9. Important Mod Details

---

### A. What is a Mod Type?
> - There are 6 types and they tell you where a mod has to be installed to work, and whether or not you need EAC on or off.
>   - **Server** — where the game is hosted. This could be your own PC if you are hosting the game yourself, or a game hosting service like Pingperfect.
>   - **Client** — your own PC.
>   - Some mods only need to be in one place. Others need to be in both. Some have other caveats. Please see the table in the next section.

---

### B. The 6 Mod Types

| # | Mod Type | What It Means |
|---|----------|---------------|
| 1 | Server-Side (EAC-Friendly) | Server install works for all joining players; EAC on or off. |
| 2 | Server-Side (EAC Off) | EAC off required; server install works for all joining players. |
| 3 | Server-Side (Dedicated Only, EAC Off) | EAC off required; dedicated uses server install only, but player-hosted requires host and joining players to install it. |
| 4 | Hybrid (EAC Off) | EAC off required; server install works for all joining players; client install is optional for extra features. |
| 5 | Server/Client-Side (Required) | EAC off required; host and joining players must install it. |
| 6 | Client-Side (Only) | EAC off required; server install has no effect; only the installing player gets the feature. |

---

### C. Should I play with EAC on or off?
> - EAC stands for **Easy Anti-Cheat**. It's a program built into 7 Days to Die that helps protect multiplayer sessions from cheating.
> - Mod Types 2–6 require EAC to be turned off. These mods use custom code to deliver more advanced features.
> - Running without EAC opens up a wider range of mods and experiences. If you're running a multiplayer server without EAC, here are some good practices to keep things running smoothly:
>   - **Recommended practices when running multiplayer with EAC off:**
>     - Require a password and be conservative in distributing it.
>     - There are tools available such as ALOC and Server Tools to add extra protections.
>     - Optionally, require the whitelist system for the strictest limitation on who can join.
>     - Seek out other server hosts and discuss what they do.
>     - You can find help on AGF's Discord: [DISCORD](https://discord.gg/Vm5eyW6N4r)

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