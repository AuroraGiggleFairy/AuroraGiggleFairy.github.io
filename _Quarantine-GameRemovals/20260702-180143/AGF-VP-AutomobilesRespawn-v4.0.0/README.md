# AGF-VP-AutomobilesRespawn
7d2d Version 3
**Version:** 4.0.0  
[Download](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips/AGF-VP-AutomobilesRespawn.zip)

 

---
---

## README TABLE OF CONTENTS
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

---
---

## 1. About AGF
- AuroraGiggleFairy (AGF) creates accessibility-focused, vanilla-enhancing mods for 7 Days to Die.
- Goal is to deliver practical, easy-to-use features shaped by community feedback.
- Main site and first release source: [auroragigglefairy.github.io](https://auroragigglefairy.github.io/)
- Discord is best for latest updates, fastest contact, and becoming a tester: [discord.gg/Vm5eyW6N4r](https://discord.gg/Vm5eyW6N4r)

---
---

## 3. Need Help?
- AuroraGiggleFairy is available via Discord: https://discord.gg/Vm5eyW6N4r
- All questions welcome from newcomers and seasoned players alike.

---
---

## 4. Install Scope & EAC Requirement
- Server-Side (EAC-Friendly): Server install works for all joining players; EAC on or off.

---
---

## 5. Compatibility
- Last 7d2d Version tested on: 3
- "AGF-VP-AutomobilesRespawn" is SAFE to install on an existing game.
- "AGF-VP-AutomobilesRespawn" is DANGEROUS to remove from an existing game.
- Unique Details: None

---
---

## 6. Features Summary
<!-- FEATURES-SUMMARY START -->
- Salvaged vehicles downgrade to a hubcap seed at that location.
- The hubcap regrows into a similar vehicle type.
- Growth time and hubcap durability are configurable (default: `600` minutes, `2500` HP).
- See this mod's README for full details.
<!-- FEATURES-SUMMARY END -->

---
---

## 7. Features Details
<!-- FEATURES-DETAILED START -->
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
<!-- FEATURES-DETAILED END -->

---
---

## 8. Changelog
<!-- CHANGELOG START -->
v4.0.0
- Updated for 7d2d version 3.
- Used new code format for "plant growth"
- Removed old code that isn't necessary anymore.

v3.1.1
- ReadMe Format Update.

v3.1.0
- Fixed issues related to challenges not working.
- Corrected what parts of the semi truck should respawn.
- Attempted fix at "terrain alignment" issues.

v3.0.1
- Implemented necessary code to ensure safe update on existing games using previous version of mod.

v3.0.0
- A rework for better coding and additional features.
- Randomness is applied where appropriate.
- The "plant" is a hubcap, drive over able AND will take up space to prevent building in its respawn.
- Removed the tip for now. I like having it to inform new players on my server, but it isn't working as intended.
- Updated localization.

v2.0.3
- Removed airConditioner, decoCarRadiatorFlat and radiatorHouse01 from activating the message alert.
- Significantly reduce the frequency of the message alert.

v2.0.2
- Updated the tags situation as the game updated.

v2.0.1
- had to fix tags for existing vehicles so challenges would work

v2.0.0
- Updated and fixed for V1.0
- Had to make several fixes!
<!-- CHANGELOG END -->

---
---

## 9. Install Scope & EAC Requirement Guide

---

> *This guide explains where to install a mod and whether it is EAC friendly.*

---

### A. Install on Server, Client, or Both?
>   - **Server** - where the game is hosted. This could be your own PC if you are hosting the game yourself, or a game hosting service like Pingperfect.
>   - **Client** - your own PC.
> - Some mods only need to be in one place. Others need to be in both. Please see the Mod Types section below for exact requirements.

---

### B. EAC Friendly?

> - EAC stands for **Easy Anti-Cheat**. It's a program built into 7 Days to Die that helps protect multiplayer sessions from cheating.
> - Mod Type 1 is EAC friendly. Mod Types 2, 3, and 4 require EAC to be turned off.
> - Running without EAC opens up a wider range of mods and experiences. If you're running a multiplayer server without EAC, here are some good practices to keep things running smoothly:
>   - **Recommended practices when running multiplayer with EAC off:**
>     - Require a password and be conservative in distributing it.
>     - There are tools available such as ALOC and Server Tools to add extra protections.
>     - Optionally, require the whitelist system for the strictest limitation on who can join.
>     - Seek out other server hosts and discuss what they do.
>     - You can find help on AGF's Discord: [DISCORD](https://discord.gg/Vm5eyW6N4r)

---

### C. Mod Types

| # | Mod Type | What It Means |
|---|----------|---------------|
| 1 | Server-Side (EAC-Friendly) | Server install works for all joining players; EAC on or off. |
| 2 | Server-Side (EAC Off) | EAC off required; server install works for all joining players. |
| 3 | Server/Client-Side (Required) | EAC off required; host and joining players must install it. |
| 4 | Client-Side (Only) | EAC off required; server install has no effect; only the installing player gets the feature. |

---
---

## 10. Installation Guide

---

> ⚠️ **IMPORTANT** ⚠️ Make a BACKUP!
> - If you are making changes to an existing game, ALWAYS make a backup first.
> - *(See the backup instructions further below.)*

---

### A. Special Note
> - The mod named **"0_TFP_Harmony"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - *In Steam, right click on `7 Days to Die`*
>    - *Select "Properties"*
>    - *Select "Installed Files"*
>    - *Click on "Verify integrity of game files" and wait for completion.*

---

### B. Install to Your PC (Singleplayer, Hosting Friends, or Client-Required Mods)
> 1. **In Steam, right click `7 Days to Die`**
> 2. **Select `Manage`, then `Browse local files`**
> 3. **Open the `Mods` folder**
> 4. **Extract the mod into this `Mods` folder**
>    - *Final folder should look like: `Mods/<ModFolder>/ModInfo.xml`*
>    - *If the zip creates an extra parent folder, move the mod folder up one level*
> 5. **Keep `0_TFP_Harmony` in this folder**
>    - *It comes with the game and should remain in `Mods`*

---

### C. Install to a Dedicated Server *(hosted sites or player-run)*
> 1. **Find the main server folder**
>    - *If using a hosted site, use their file manager or a program like FileZilla*
> 2. **Open or create the `Mods` folder**
> 3. **Extract the mod into this `Mods` folder**
>    - *Final folder should look like: `Mods/<ModFolder>/ModInfo.xml`*
>    - *If the zip creates an extra parent folder, move the mod folder up one level*
> 4. **Fully restart the server after install**

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
>    - *In Steam, right click on `7 Days to Die`*
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
>    - *You can check your world name in the game's "Continue Game" or "Join Server" menu.*
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
>    - *You can check your world name in the game's "Join Server" menu or "Server Config".*
> 3. **Select your Game Name folder** *(inside the World Name folder)*
>    - *The Game Name is also shown in the game's "Join Server" menu or "Server Config".*
> 4. **Copy the entire Game Name folder and paste it somewhere safe**
>    - *(like your Desktop or another safe location)*.

---

### C. To Restore a Backup
> - *Keep your backup until you're sure everything works!*
> 1. **Undo any mod changes**
> 2. **Delete the current Game Name folder** *(in "Saves")*
> 3. **Move your backup folder back into "Saves"**

---
---