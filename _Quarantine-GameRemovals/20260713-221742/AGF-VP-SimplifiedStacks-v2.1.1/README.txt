========================================================================
                        AGF-VP-SIMPLIFIEDSTACKS                         
========================================================================

Simplified stack sizes to reduce inventory clutter with vanilla feel.


NOTE: AGF Mod Guide and Changelog are further below.


------------------------------------------------------------------------
MOD SCOPE
------------------------------------------------------------------------

  - Mod Version: 2.1.0
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

  - Simplified stack sizes to reduce inventory clutter while keeping a
    vanilla-like feel.
  - Simple category tiers: consumables (50), ammo (500), resources (500,
    1000, or 6000 based on function), placeable blocks (500),
    farming/building blocks (5000), and a few exceptions like duke coins
    and feathers (30000).
  - Existing bundle values are adjusted to match the new stack sizes.



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

v2.1.1
    - Fixes to automation and readme generations.

------------------------------------------------------------------------

v2.1.0
    - Overhauled all README files and their management workflow.
    - Updated mod descriptions and details.
    - Images no longer within mod folders due to constraints.
    - Removed incorrect and unused localization for raw meat bundle.
    - Added stack sizes to missing items.
    - Quest Reward bundles now stack to 50.

------------------------------------------------------------------------

v2.0.1
    - Removed recipe for an item that doesn't exist.

------------------------------------------------------------------------

v2.0.0
    - Updated for 7d2d version 3.
    - Removed old version conditionals.
    - Removed stack size change for the new quality items.

------------------------------------------------------------------------

v1.2.4
    - ReadMe Format Update.

------------------------------------------------------------------------

v1.2.3
    - Included Honey Tea, stacks to 50.

------------------------------------------------------------------------

v1.2.2
    - Fun Pimps added full localization for new items, added it to
    affected items.

------------------------------------------------------------------------

v1.2.1
    - The larger raw meat bundle was missing its description. Corrected.

------------------------------------------------------------------------

v1.2.0
    - updated for 7d2d version 2.5.
    - added stack sizes to new apiary tools.
    - updated recipes for bundled exploding arrows/bolts.
    - End table lamp now stacks to 500 correctly.
    - New items have updated stack sizes.
    - beeswax is updated to 50 instead of 10, however needs to be
    monitored for breaking duke economy.
    - Raw meat bundle stacks at 50.
    - Added a LARGER raw meat bundle that accounts for all 500 of raw
      meat
    stack size.

------------------------------------------------------------------------

v1.1.1
    - Resources to make gun powder bundles now correctly modified.

------------------------------------------------------------------------

v1.1.0
    - Re-evaluated 7d2d version 2's use of resources and modified stack
    sizes for a variety of items.
    - Increased gun powder stacks to 6000 and updated bundle
    functioning/localization.
    - Majority of ammo ingredients now stack to 6,000.

------------------------------------------------------------------------

v1.0.0
    - Separated from StacksBundlesAmmoPlus now that I understand
    conditionals.
