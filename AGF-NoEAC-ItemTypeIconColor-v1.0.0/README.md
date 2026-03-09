# Add Color to Item Type Icons
7d2d Version 2  
**Version:** 1.0.0  
[Download](https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/_zip/AGF-NoEAC-ItemTypeIconColor.zip)

> 
---

## README TABLE OF CONTENTS
1. About Author
2. Mod Philosophy
3. Need Help?
4. Compatibility
5. Installation
6. Removal
7. Features
8. Changelog

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

## 2. Mod Philosophy
- Preferably easy installation and use!
- Goal: Enhance Vanilla Gameplay!
- Feedback and Testing is Beneficial!
- Detailed Notes for Individual Preference and Mod Learning!
- Accessibility is Required
- All 13 Languages Supported (best I can.)

> "The best mods rely on community involvement."

---

## 3. Need Help?
- AuroraGiggleFairy is available via Discord: https://discord.gg/Vm5eyW6N4r
- All questions welcome from newcomers and seasoned players alike.

---

## 4. Compatibility
- EAC Friendly: 
- Server Side: 
- Client Required for Multiplayer: 

---

## 5. Installation

### Safe to install on existing games: 

### Singleplayer, Player-to-Player Multiplayer, or Client-Side Requirement
> - Open "Run" (press Windows key + R)
> - Type %appdata% and click OK
> - Open the "Roaming" folder
> - Open "7DaysToDie"
> - Open or create the "Mods" folder
> - Extract the mod here
> - Make sure the mod folder is not inside another folder with the same name (move it up if needed)

### Multiplayer Dedicated Server *(hosted sites or player-run)*
> - Find the main server folder and open the "Mods" folder
>    - *If using a hosted site, use their file manager or a program like FileZilla*
> - Extract the mod here
> - Make sure the mod folder is not inside another folder with the same name (move it up if needed)

### Special Note
> - The mod named **"0_TFP_Harmony"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - In Steam, right click on 7 Days to Die
>    - Select "Properties"
>    - Select "Installed Files"
>    - Click on "Verify integrity of game files" and wait for completion.

---

## 6. Removal

### Safe to remove from an existing game: 
- Simply locate the mod folder and delete it.

---

## 7. Features
<!-- FEATURES START -->
- For modders, in xml, at least for items.xml and blocks.xml, you can use the property "ItemTypeIconColor".
- Vanilla, you could only adjust the color of the ALTERNATE icon color (like after reading a book), with "AltItemTypeIconColor".



5.  EXAMPLE

*I pulled this from my HUD Mod*
<!--Adding color and opacity settings to the un-read closed book icon and read open book icon.-->
<conditional>
<if cond="mod_loaded(ItemTypeIconColor')">    
<!--WITH ItemTypeIconColor Mod-->
<insertafter xpath="/items/item/property[@name='AltItemTypeIcon' and @value='book_read']">
<property name="AltItemTypeIconColor" value="255,0,0,180" />
<property name="ItemTypeIconColor" value="0,255,0"/>
</insertafter>
</if>
<else>
<!--Without ItemTypeIconColor Mod"-->
<insertafter xpath="/items/item/property[@name='AltItemTypeIcon' and @value='book_read']">
<property name="AltItemTypeIconColor" value="0,255,0,180" />
</insertafter>
</else>
</conditional>



6.  CHANGELOG
v1.0.0
- Made the mod.
<!-- FEATURES END -->

---

## 8. Changelog
<!-- CHANGELOG START -->

<!-- CHANGELOG END -->