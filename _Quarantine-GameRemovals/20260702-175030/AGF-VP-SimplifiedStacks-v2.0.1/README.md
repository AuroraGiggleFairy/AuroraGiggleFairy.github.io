# AGF-VP-SimplifiedStacks
7d2d Version 3
**Version:** 2.0.1  
[Download](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips/AGF-VP-SimplifiedStacks.zip)

 

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
- "AGF-VP-SimplifiedStacks" is SAFE to install on an existing game.
- "AGF-VP-SimplifiedStacks" is DANGEROUS to remove from an existing game.
- Unique Details: None

---
---

## 6. Features Summary
<!-- FEATURES-SUMMARY START -->
- Simplified stack sizes to reduce inventory clutter while keeping a vanilla-like feel.
- Simple category tiers: consumables (50), ammo (500), resources (mostly 6000, with some 1000), placeable blocks (500), farming/building blocks (5000), and a few exceptions (30000).
- Existing bundle values are adjusted to match the new stack sizes.
- See this mod's README for full details.
<!-- FEATURES-SUMMARY END -->

---
---

## 7. Features Details
<!-- FEATURES-DETAILED START -->
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
<!-- FEATURES-DETAILED END -->

---
---

## 8. Changelog
<!-- CHANGELOG START -->
v2.0.1
- Removed recipe for an item that doesn't exist.

v2.0.0
- Updated for 7d2d version 3.
- Removed old version conditionals.
- Removed stack size change for the new quality items.

v1.2.4
- ReadMe Format Update.

v1.2.3
- Included Honey Tea, stacks to 50.

v1.2.2
- Fun Pimps added full localization for new items, added it to affected items.

v1.2.1
- The larger raw meat bundle was missing its description. Corrected.

v1.2.0
- updated for 7d2d version 2.5.
- added stack sizes to new apiary tools.
- updated recipes for bundled exploding arrows/bolts.
- End table lamp now stacks to 500 correctly.
- New items have updated stack sizes.
- beeswax is updated to 50 instead of 10, however needs to be monitored for breaking duke economy.
- Raw meat bundle stacks at 50.
- Added a LARGER raw meat bundle that accounts for all 500 of raw meat stack size.

v1.1.1
- Resources to make gun powder bundles now correctly modified.

v1.1.0
- Re-evaluated 7d2d version 2's use of resources and modified stack sizes for a variety of items.
- Increased gun powder stacks to 6000 and updated bundle functioning/localization.
- Majority of ammo ingredients now stack to 6,000.

v1.0.0
- Separated from StacksBundlesAmmoPlus now that I understand conditionals.
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