========================================================================
                           AGF-VP-FARMINGPLUS                           
========================================================================

Adds a Seed Station, plantable birdnests, and x5/x25/self-replanting


NOTE: AGF Mod Guide and Changelog are further below.


------------------------------------------------------------------------
MOD SCOPE
------------------------------------------------------------------------

  - Mod Version: 5.6.3
  - 7d2d Version: 3
  - Website: https://auroragigglefairy.github.io/
  - Languages Supported: All 13 game-supported languages.
  - Mod Type: Server-Side (EAC-Friendly)
    - Server install works for all joining players.
    - EAC can be on or off.
    - Also works in singleplayer.
  - Safe to install on existing game: Yes (Safe)
  - Safe to remove from existing game: No (Dangerous)
  - Dependencies: None, works standalone.


------------------------------------------------------------------------
FEATURES
------------------------------------------------------------------------

  - Adds a Seed Station for faster seed crafting and management (Unlocks
    at Seeds 20).
  - Adds a plantable Birdnest for eggs and feathers (Unlocks at Seeds
    20).
  - Adds x5 and x25 variants to seeds/farm plots, plus a "replants
    itself" variant for each seed.
  - x5 and x25 seeds require matching x5/x25 farm plots (except
    mushrooms and bird nests).


------------------------------------------------------------------------
OTHER DETAILS
------------------------------------------------------------------------

  - Implementation edits used to make this work:
      - Adds a new seedStation workstation block and crafting area
        (seedStation) for organized seed workflows.
      - Adds progression wiring so advanced farming unlocks at seed
        crafting level 20 (including station access and x5/x25 lines).
      - Adds trader + loot integration so farming additions can be
        naturally discovered.
      - Adds large recipe sets for normal, x5, x25, and replant variants
        (including station-specific and inventory paths).
      - Adds UI category display groups for seed station tabs: normal,
        x5, x25, and replants.
      - Adds compatibility conditionals (notably bee flower behavior by
        game version) to keep updates safer across versions.
      - Feature set:
      - Birdnest and Beehives now plantable for eggs, feathers, and
        honey.
      - Naturally discovered through traders and loot.
      - Unlocks at seed crafting level 20.
      - A new "Seed Station" that offers faster seed crafting and
        organization.
      - x5 farming at seed crafting level 20:
      - Combine 5 normal seeds into 1 x5 seed recipe.
      - One x5 seed is designed to cover the same planting output as 5
        normal seeds.
      - Most x5 seeds (except mushrooms and bird nests) are paired with
        x5 farm plot usage.
      - Level 3 Living Off the Land reduces seed-station crafting time
        by 70%.
      - x25 variants are also available.
      - Replant variants are available for each seed:
      - Replants itself.
      - Harvests no extra seed copies.
      - Can be scrapped to convert back close to 1:1.



========================================================================
                             AGF MOD GUIDE                              
========================================================================

------------------------------------------------------------------------
A. Install Mods
------------------------------------------------------------------------

  1. Close the game.
  2. In Steam, right-click 7 Days to Die -> Manage -> Browse local
     files, then open Mods.
  3. Extract the zip into the Mods folder. Make sure it ends up as
     Mods/<ModName>/ModInfo.xml.
  4. Restart the game.


------------------------------------------------------------------------
B. Ask AuroraGiggleFairy for Help
------------------------------------------------------------------------

  1. Join AGF's Discord: https://discord.gg/Vm5eyW6N4r.
    - AGF checks website messages often, but Discord is the fastest and
      best way to get help.
  2. Find #ask-for-help-here under the NEED HELP? section.
    - All questions are welcome, whether you are new or experienced.
    - This includes mod conflicts, features not working as expected,
      server or admin issues, translation errors, and other mod-related
      problems.
  3. Post your help request in #ask-for-help-here:
    - Share a brief message about what is happening.
    - Attach your latest log file.
      - Enter the game, then press F1 to open the console.
      - Click Open logs folder in the top-right.
      - The correct log file should already be selected. Drag and drop
        it into #ask-for-help-here.
    - A screenshot can also help.
      - Use PrtSc (Print Screen) or your system screenshot tool, then
        paste the image into Discord chat.
    - If preferred, DMs are open and you are welcome to message AGF
      directly.


------------------------------------------------------------------------
C. Backups
------------------------------------------------------------------------

  - To Create:
    - Open %appdata% -> Roaming -> 7DaysToDie -> Saves, then open your
      World Name folder (for example, Navezgane).
    - Copy your Game Name folder (for example, MyGame) to a safe place.
  - To Restore:
    - Copy that saved Game Name folder back into the same World Name
      folder in Saves.
    - Replace the current folder if asked.


------------------------------------------------------------------------
D. Update Mods
------------------------------------------------------------------------

  1. Close the game.
  2. Make a backup first (see section C).
  3. Install the new version in Mods.
    - If asked, allow overwrite or replace.
  4. If both old and new folders are there, keep the newer one and
     delete the older one.
  5. Start the game and confirm your save loads.


------------------------------------------------------------------------
E. Remove Mods
------------------------------------------------------------------------

  - Warning: Removing a mod from an active save can destroy your saved
    game. Back up first.
  - Never delete 0_TFP_Harmony; it comes with the game.
  1. Close the game.
  2. In Mods, delete each mod folder you are removing, except
     0_TFP_Harmony.


------------------------------------------------------------------------
F. The 0_TFP_Harmony Mod (Do Not Remove)
------------------------------------------------------------------------

  - Never delete 0_TFP_Harmony; it comes with the game.
  - If it is missing, restore it by verifying game files in Steam:
    1. In Steam, right-click 7 Days to Die.
    2. Select Properties.
    3. Select Installed Files.
    4. Click Verify integrity of game files and wait for completion.


------------------------------------------------------------------------
G. EAC
------------------------------------------------------------------------

  - EAC stands for Easy Anti-Cheat and helps protect multiplayer
    sessions from cheating.
  - Some mods require EAC to be turned off so they can work.

  - How to launch 7 Days to Die with EAC off:
    1. In Steam Library, select 7 Days to Die.
    2. Click Play.
    3. In the launch popup, select Launch game without EAC.
    4. Click Play.

  - If the launch popup does not appear:
    1. In Steam Library, select 7 Days to Die.
    2. Click the gear icon on the right, then click Properties.
    3. Under Launch Options, open the Selected Launch Option dropdown.
    4. Choose Ask when starting game or Launch game without EAC.
    
  - If you run multiplayer with EAC off, use these safety practices:
    - Simplest method: keep your server password private and have people
      ask for it.
    - If you want tighter security on who joins, use the whitelist
      system.
    - Admin tools such as Server Tools have security options.
    - Talk to other server hosts, for example AGF in Discord:
      https://discord.gg/Vm5eyW6N4r


------------------------------------------------------------------------
H. Support AuroraGiggleFairy
------------------------------------------------------------------------

  - I have been actively creating and supporting 7 Days to Die mods
    since Alpha 18 (2019), and I genuinely love doing this work.
  - I spend a lot of time fixing complex issues, keeping everything up
    to date, and helping players, modders, and server communities.
  - If my work helps you, here are ways to support me:
    - Help spread my mods by sharing them with others, creating content,
      or sharing my GitHub link: https://auroragigglefairy.github.io/
    - Join my Discord to share feedback, keep up with updates, or
      volunteer as a tester: https://discord.gg/Vm5eyW6N4r
    - Support me on Twitch: https://www.twitch.tv/auroragigglefairy
    - Need hosting? Use my PingPerfect Referral Link:
      https://pingperfect.com/aff.php?aff=1834
    - Support me directly by donating to my PayPal:
      https://www.paypal.com/donate/?hosted_button_id=3B7BCQAZ6KHXC
  - From the bottom of my heart, thank you. <3


------------------------------------------------------------------------
I. AGF Modding Focus
------------------------------------------------------------------------

  - I have been modding 7 Days to Die for 7 years.
  - I do my best to prioritize accessibility, user-friendliness, and
    localization where possible.
  - I provide kind, comprehensive support to players, modders, and
    server communities, and I rely on community feedback to keep
    improving my mods.



========================================================================
                               CHANGELOG                                
========================================================================

v5.6.3
    - Reduced the xp gain per crop harvested from 10 down to 1. Removes
    massive xp gain loophole.

------------------------------------------------------------------------

v5.6.2
    - ReadMe Format Update.

------------------------------------------------------------------------

v5.6.1
    - Several Replants were scrapping into fibers, fixed it.

------------------------------------------------------------------------

v5.6.0
    - Now you can craft seeds that replant themselves.
    - Updated README format.

------------------------------------------------------------------------

v5.5.0
    - Removed windows.xml as the purple book conditional is now within
      the
    purple book mod.

------------------------------------------------------------------------

v5.4.0
    - If playing 7d2d version 2.5, will NO LONGER be able to craft bee
    flower seeds.
    - Still safe to update AND does work with previous versions.
    - Existing Bee Flower Seeds can be planted and grown, but will no
    longer drop seeds.
    - Bee flower no longer findable at traders and loot.

------------------------------------------------------------------------

v5.3.1
    - Max Damage of seed station is now 500 (increased).
    - Breaking the seed station returns it to your inventory.
    - Icon fixed to show which recipes have to be done at the seed
    station.

------------------------------------------------------------------------

v5.3.0
    - Now can craft x5 and x25 variants directly from inventory.

------------------------------------------------------------------------

v5.2.0
    - Will now only apply purple book edits if you have my purple book
    installed.

------------------------------------------------------------------------

v5.1.1
    - Changed how the seeds alphabetically sort themselves.

------------------------------------------------------------------------

v5.1.0
    - changed beeflower ingredients to require crops, not the seeds.
      Also
    changed the amount.
    - Reduced crafting time of beeflower and birdnest to match other
    crops.
    - Added Seed Workbench (aka hydroponic farm block), required to
      craft
    x5 farming.
    - Station has categories for regular seeds/farm plots and another
      for
    x5 variants.
    - Added x25 varients along with multiple recipes to obtain them.
    - Made categories at seed station for single, x5, and x25 variants.
    - Updated sorting orders for both in menus and alphabetical ordering
    in inventories/storages.

------------------------------------------------------------------------

v5.0.1
    - Fixed an error that allowed crazy farming of Hops.

------------------------------------------------------------------------

v5.0.0
    - Updated for 7d2d Version 2.
    - Removed my previous seed collecting system.
    - Updated purple book adjustments.
    - Reduced probability of finding nest or beeflower in world and
    traders.
    - reduced how much eggs, feathers, and honey you get per "crop" for
    better gameplay.
    - Updated the code to better access vanilla code to account for
    possible game updates.

------------------------------------------------------------------------

v4.0.0
    - Updated to fit zooming features on HUD.

------------------------------------------------------------------------

v3.1.0
    - Updated the windows section to work with HUD update.
    - Updated added seeds and x5 seeds to appropriately work with Farmer
    - Armor.
    - You now only find 1 honey with bee's, unless you get the extra
      from
    farmer armor.
    - Changed crafting time of birdnest and beehive from 20 to 3
      seconds.
    - Changed birdnest's cornmeal ingredient count from 5 to 25.
    - World crops have an additional seed drop chance with the farmer
    armor seed bonus, at half the chance.

------------------------------------------------------------------------

v3.0.0
    - Updated to V1

------------------------------------------------------------------------

v2.0.0
    - Updated the Bee Flower progression blocks to use a better model
    since
    - A20! came out.
    - REVAMPED the seed drop chance on player planted crops.
    - Removed the recipe of 3 seeds per flower at max Living Off the
    - Land.
    - Returned the x5 versions of crops and farmplots from my A19 mods.

------------------------------------------------------------------------

v1.2.0
    - World crops 10% seed drop.
    - Increased seed drop chance.

------------------------------------------------------------------------

v1.1.0
    - Tier 3 of living off the land actually reduces seed requirements
      for
    planting.
    - Updated ReadMe to new format.
