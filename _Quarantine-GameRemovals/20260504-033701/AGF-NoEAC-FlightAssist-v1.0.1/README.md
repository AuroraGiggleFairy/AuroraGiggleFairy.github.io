# AGF-NoEAC-FlightAssist
7d2d Version 2  
**Version:** 1.0.1-BETA  
[Download](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips/AGF-NoEAC-FlightAssist.zip)

 

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
- Client-side (Only): EAC off required; server install has no effect; only the installing player gets the feature.

---
---

## 5. Compatibility
- Last 7d2d Version tested on: 2.6
- "AGF-NoEAC-FlightAssist" is SAFE to install on an existing game.
- "AGF-NoEAC-FlightAssist" is TBD to remove from an existing game.
- Unique Details: Updates Incoming.

---
---

## 6. Features Summary
<!-- FEATURES-SUMMARY START -->
- Two flight assist patterns:
  - Gyrocopter: Press Z to move forward while staying at the same elevation.
  - Helicopter: Press Z for Hover, then press Z again for forward same-elevation flight.
- HUD shows the current Flight Assist mode.
- See this mod's README for full details.
<!-- FEATURES-SUMMARY END -->

---
---

## 7. Features Details
<!-- FEATURES-DETAILED START -->
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
<!-- FEATURES-DETAILED END -->

---
---

## 8. Changelog
<!-- CHANGELOG START -->
v1.0.1
- ReadMe Format Update.

v0.0.3
- Forgot what was in version 0.0.2
- Allows two flight pattern assists:
  - Gyrocopter: pressing "x" activates flying forward locked on elevation
  - Hellicopter: pressing "x" first activates hover, press again activates forwward locked on elevation.
- Did a lot to ensure works with any flying vehicle and can determine which flight pattern to use.

v0.0.1
- Mod first created.
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