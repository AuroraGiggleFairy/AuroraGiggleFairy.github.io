AGF-ItemTypeIconColor
7d2d Version 2.5 - v1.0.0

_____________________________________________________________________________________________________________________
README TABLE OF CONTENTS
1. About Author
2. Mod Philosophy
3. Installation and Removal Notes
4. Features
5. Example
6. Change Log

_____________________________________________________________________________________________________________________
1.  ABOUT AUTHOR
	-My name is AuroraGiggleFairy (AGF), previously known as RilesPlus
	-Started playing 7d2d during Alpha 12
	-Started attempting to mod in Alpha 17
	-First published a mod during Alpha 18
	-Where to find:
		https://discord.gg/Vm5eyW6N4r
		https://auroragigglefairy.github.io/
		https://www.twitch.tv/AuroraGiggleFairy
		https://7daystodiemods.com/
		https://www.nexusmods.com/7daystodie

______________________________________________________________________________________________________________________
2.  MOD PHILOSOPHY
	-Preferably easy installation and use!
	-Goal: Enhance Vanilla Gameplay!
	-Feedback and Testing is Beneficial!
	-Detailed Notes for Individual Preference and Mod Learning!
	-Full Language Support (best I can.)
		
	"The best mods rely on community involvement."

______________________________________________________________________________________________________________________
3.  INSTALLATION and REMOVAL NOTES
	*First, if you run into any conflicts, you may contact AuroraGiggleFairy via discord: https://discord.gg/Vm5eyW6N4r
		-All questions welcome from newcombers to seasoned 7d2d people.

	-EAC MUST be turned off.
	-Singplayer, install on client.
	-Multiplayer, install on both host and client.	
	-All 13 languages supported.
	
	-AGF-ItemTypeIconColor is SAFE to install on new game or existing game.
	-AGF-ItemTypeIconColor is SAFE to remove from an existing game.

______________________________________________________________________________________________________________________
4.  FEATURES


- For modders, in xml, at least for items.xml and blocks.xml, you can use the property "ItemTypeIconColor".
  - Vanilla, you could only adjust the color of the ALTERNATE icon color (like after reading a book), with "AltItemTypeIconColor".


______________________________________________________________________________________________________________________
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


______________________________________________________________________________________________________________________
6.  CHANGELOG
v1.0.0
-Made the mod.