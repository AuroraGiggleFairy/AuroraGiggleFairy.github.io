========================================================================
                       AGF-VP-AUTOMOBILESRESPAWN                        
========================================================================

Salvaged vehicles downgrade to a hubcap seed that regrows a vehicle.


NOTE: AGF Mod Guide and Changelog are further below.


------------------------------------------------------------------------
MOD SCOPE
------------------------------------------------------------------------

  - Mod Version: 4.1.1
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

  - Salvaging a vehicle leaves a hubcap seed at that location.
  - The seed regrows into a vehicle from the same family.
  - Growth time and hubcap durability are configurable (defaults: 600
    minutes and 2500 HP).


------------------------------------------------------------------------
OTHER DETAILS
------------------------------------------------------------------------

  - To change growth time, open Config/blocks.xml and search for
    GrowthRate.
    - 600 is the default and represents minutes, which is 10 hours.

  - To change hubcap durability, open the same file and search for
    MaxDamage.
    - 2500 is the default hit point value.



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

v4.1.1
    - Fixes to automation and readme generations.

------------------------------------------------------------------------

v4.1.0
    - Overhauled all README files and their management workflow.
    - Updated mod descriptions and details.
    - Images no longer within mod folders due to constraints.
    - Hubcap respawners will now show their name when looked at.

------------------------------------------------------------------------

v4.0.0
    - Updated for 7d2d version 3.
    - Used new code format for "plant growth"
    - Removed old code that isn't necessary anymore.

------------------------------------------------------------------------

v3.1.1
    - ReadMe Format Update.

------------------------------------------------------------------------

v3.1.0
    - Fixed issues related to challenges not working.
    - Corrected what parts of the semi truck should respawn.
    - Attempted fix at "terrain alignment" issues.

------------------------------------------------------------------------

v3.0.1
    - Implemented necessary code to ensure safe update on existing games
    using previous version of mod.

------------------------------------------------------------------------

v3.0.0
    - A rework for better coding and additional features.
    - Randomness is applied where appropriate.
    - The "plant" is a hubcap, drive over able AND will take up space to
    prevent building in its respawn.
    - Removed the tip for now. I like having it to inform new players on
    my server, but it isn't working as intended.
    - Updated localization.

------------------------------------------------------------------------

v2.0.3
    - Removed airConditioner, decoCarRadiatorFlat and radiatorHouse01
      from
    activating the message alert.
    - Significantly reduce the frequency of the message alert.

------------------------------------------------------------------------

v2.0.2
    - Updated the tags situation as the game updated.

------------------------------------------------------------------------

v2.0.1
    - had to fix tags for existing vehicles so challenges would work

------------------------------------------------------------------------

v2.0.0
    - Updated and fixed for V1.0
    - Had to make several fixes!