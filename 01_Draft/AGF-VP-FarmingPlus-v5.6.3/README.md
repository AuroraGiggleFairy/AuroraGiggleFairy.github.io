# AGF-VP-FarmingPlus
7d2d Version 3
**Version:** 5.6.3  
[Download](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips/AGF-VP-FarmingPlus.zip)

 

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
- "AGF-VP-FarmingPlus" is SAFE to install on an existing game.
- "AGF-VP-FarmingPlus" is DANGEROUS to remove from an existing game.
- Unique Details: None

---
---

## 6. Features Summary
<!-- FEATURES-SUMMARY START -->
- Adds a Seed Station for faster seed crafting and management (Unlocks at Seeds 20).
- Adds a plantable Birdnest for eggs and feathers (Unlocks at Seeds 20).
- Adds x5 and x25 variants to seeds/farm plots, plus a "replants itself" variant for each seed.
- x5 and x25 seeds require matching x5/x25 farm plots (except mushrooms and bird nests).
- See this mod's README for full details.
<!-- FEATURES-SUMMARY END -->

---
---

## 7. Features Details
<!-- FEATURES-DETAILED START -->
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
<!-- FEATURES-DETAILED END -->

---
---

## 8. Changelog
<!-- CHANGELOG START -->
v5.6.3
- Reduced the xp gain per crop harvested from 10 down to 1. Removes massive xp gain loophole.

v5.6.2
- ReadMe Format Update.

v5.6.1
- Several Replants were scrapping into fibers, fixed it.

v5.6.0
- Now you can craft seeds that replant themselves.
- Updated README format.

v5.5.0
- Removed windows.xml as the purple book conditional is now within the purple book mod.

v5.4.0
- If playing 7d2d version 2.5, will NO LONGER be able to craft bee flower seeds.
- Still safe to update AND does work with previous versions.
- Existing Bee Flower Seeds can be planted and grown, but will no longer drop seeds.
- Bee flower no longer findable at traders and loot.

v5.3.1
- Max Damage of seed station is now 500 (increased).
- Breaking the seed station returns it to your inventory.
- Icon fixed to show which recipes have to be done at the seed station.

v5.3.0
- Now can craft x5 and x25 variants directly from inventory.

v5.2.0
- Will now only apply purple book edits if you have my purple book installed.

v5.1.1
- Changed how the seeds alphabetically sort themselves.

v5.1.0
- changed beeflower ingredients to require crops, not the seeds. Also changed the amount.
- Reduced crafting time of beeflower and birdnest to match other crops.
- Added Seed Workbench (aka hydroponic farm block), required to craft x5 farming.
- Station has categories for regular seeds/farm plots and another for x5 variants.
- Added x25 varients along with multiple recipes to obtain them.
- Made categories at seed station for single, x5, and x25 variants.
- Updated sorting orders for both in menus and alphabetical ordering in inventories/storages.

v5.0.1
- Fixed an error that allowed crazy farming of Hops.

v5.0.0
- Updated for 7d2d Version 2.
- Removed my previous seed collecting system.
- Updated purple book adjustments.
- Reduced probability of finding nest or beeflower in world and traders.
- reduced how much eggs, feathers, and honey you get per "crop" for better gameplay.
- Updated the code to better access vanilla code to account for possible game updates.

v4.0.0
- Updated to fit zooming features on HUD.

v3.1.0
- Updated the windows section to work with HUD update.
- Updated added seeds and x5 seeds to appropriately work with Farmer Armor.
- You now only find 1 honey with bee's, unless you get the extra from farmer armor.
- Changed crafting time of birdnest and beehive from 20 to 3 seconds.
- Changed birdnest's cornmeal ingredient count from 5 to 25.
- World crops have an additional seed drop chance with the farmer armor seed bonus, at half the chance.

v3.0.0
- Updated to V1

v2.0.0
- Updated the Bee Flower progression blocks to use a better model since A20! came out.
- REVAMPED the seed drop chance on player planted crops.
- Removed the recipe of 3 seeds per flower at max Living Off the Land.
- Returned the x5 versions of crops and farmplots from my A19 mods.

v1.2.0
- World crops 10% seed drop.
- Increased seed drop chance.

v1.1.0
- Tier 3 of living off the land actually reduces seed requirements for planting.
- Updated ReadMe to new format.
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