========================================================================
                     AGF-VP-MEDICATIONNOINSECTSLOW                      
========================================================================

Adds Bee Gone Cream to clear and prevent insect slow effects; steroids

"Thanks to "Go-go, Godzilla" for the name idea. Thanks to "Go-go,
Godzilla", B19JAY, mandy_tj, 13ubblegum_ and Asher_Gamess for
brainstorming support. You can check out B19JAY and his crew here:
https://www.twitch.tv/b19jay"

NOTE: AGF Mod Guide and Changelog are further below.


------------------------------------------------------------------------
MOD SCOPE
------------------------------------------------------------------------

  - Mod Version: 1.1.1
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

  - Bee Gone Cream unlocks at Medical Crafting level 10 and is crafted
    at a campfire with a cooking pot.
  - Bee Gone Cream removes and prevents Bee/Insect swarm slows, can be
    used on yourself or other players, and is also findable in the
    world.
  - Steroids also include this anti-slow benefit with matching
    stat/description display.


------------------------------------------------------------------------
OTHER DETAILS
------------------------------------------------------------------------

  - Implementation edits used to make this work:
        - Adds a new item: medicalInsectCream (Bee Gone Cream) with
          primary self-use and secondary use-on-other actions.
      - Adds campfire recipe (toolCookingPot required): 4 Aloe, 4
        Blueberries, 4 Potatoes, 1 Boiled Water.
      - Adds progression unlock in craftingMedical at level 10.
      - Adds custom buff buffInsectCream with timed duration handling
        (about 3 minutes per use, capped at 543 seconds total).
      - Buff logic removes current buffInjurySlow and keeps clearing it
        while active.
      - Adds noSlowAGF CVar protection while Bee Gone Cream or Steroids
        are active.
      - Adds insect attack requirements so Bee/Insect swarm slow effects
        only apply when noSlowAGF is not active.
      - Extends buffDrugSteroids to also remove/prevent slow and to
        share the same noSlowAGF protection logic.
      - Adds loot/trader integration (medical loot groups plus trader
        medicine group), modeled similar to Aloe Cream availability.
      - Adds UI display wiring for dStopsSlows and duration on Bee Gone
        Cream, plus dStopsSlows display on Steroids.
      - Adds God-mode cleanup for Bee Gone Cream timer CVar to prevent
        stale state.
      - Feature set:
        - Craft or loot Bee Gone Cream as a dedicated anti-slow
          medication.
      - Use it on yourself or teammates.
      - Active effect clears current insect slow and prevents new
        Bee/Insect swarm slows while duration remains.
      - Steroids now provide the same anti-slow protection for
        consistency.

  - Implementation edits used to make this work:
          - Adds a new item: medicalInsectCream (Bee Gone Cream) with
            primary self-use and secondary use-on-other actions.
        - Adds campfire recipe (toolCookingPot required): 4 Aloe, 4
          Blueberries, 4 Potatoes, 1 Boiled Water.
        - Adds progression unlock in craftingMedical at level 10.
        - Adds custom buff buffInsectCream with timed duration handling
          (about 3 minutes per use, capped at 543 seconds total).
        - Buff logic removes current buffInjurySlow and keeps clearing
          it while active.
        - Adds noSlowAGF CVar protection while Bee Gone Cream or
          Steroids are active.
        - Adds insect attack requirements so Bee/Insect swarm slow
          effects only apply when noSlowAGF is not active.
        - Extends buffDrugSteroids to also remove/prevent slow and to
          share the same noSlowAGF protection logic.
        - Adds loot/trader integration (medical loot groups plus trader
          medicine group), modeled similar to Aloe Cream availability.
        - Adds UI display wiring for dStopsSlows and duration on Bee
          Gone Cream, plus dStopsSlows display on Steroids.
        - Adds God-mode cleanup for Bee Gone Cream timer CVar to prevent
          stale state.
        - Feature set:
          - Craft or loot Bee Gone Cream as a dedicated anti-slow
            medication.
        - Use it on yourself or teammates.
        - Active effect clears current insect slow and prevents new
          Bee/Insect swarm slows while duration remains.
        - Steroids now provide the same anti-slow protection for
          consistency.

  - Implementation edits used to make this work:
          - Adds a new item: medicalInsectCream (Bee Gone Cream) with
            primary self-use and secondary use-on-other actions.
        - Adds campfire recipe (toolCookingPot required): 4 Aloe, 4
          Blueberries, 4 Potatoes, 1 Boiled Water.
        - Adds progression unlock in craftingMedical at level 10.
        - Adds custom buff buffInsectCream with timed duration handling
          (about 3 minutes per use, capped at 543 seconds total).
        - Buff logic removes current buffInjurySlow and keeps clearing
          it while active.
        - Adds noSlowAGF CVar protection while Bee Gone Cream or
          Steroids are active.
        - Adds insect attack requirements so Bee/Insect swarm slow
          effects only apply when noSlowAGF is not active.
        - Extends buffDrugSteroids to also remove/prevent slow and to
          share the same noSlowAGF protection logic.
        - Adds loot/trader integration (medical loot groups plus trader
          medicine group), modeled similar to Aloe Cream availability.
        - Adds UI display wiring for dStopsSlows and duration on Bee
          Gone Cream, plus dStopsSlows display on Steroids.
        - Adds God-mode cleanup for Bee Gone Cream timer CVar to prevent
          stale state.
        - Feature set:
          - Craft or loot Bee Gone Cream as a dedicated anti-slow
            medication.
        - Use it on yourself or teammates.
        - Active effect clears current insect slow and prevents new
          Bee/Insect swarm slows while duration remains.
        - Steroids now provide the same anti-slow protection for
          consistency.

    - Implementation edits used to make this work:
            - Adds a new item: medicalInsectCream (Bee Gone Cream) with
              primary self-use and secondary use-on-other actions.
          - Adds campfire recipe (toolCookingPot required): 4 Aloe, 4
            Blueberries, 4 Potatoes, 1 Boiled Water.
          - Adds progression unlock in craftingMedical at level 10.
          - Adds custom buff buffInsectCream with timed duration
            handling (about 3 minutes per use, capped at 543 seconds
            total).
          - Buff logic removes current buffInjurySlow and keeps clearing
            it while active.
          - Adds noSlowAGF CVar protection while Bee Gone Cream or
            Steroids are active.
          - Adds insect attack requirements so Bee/Insect swarm slow
            effects only apply when noSlowAGF is not active.
          - Extends buffDrugSteroids to also remove/prevent slow and to
            share the same noSlowAGF protection logic.
          - Adds loot/trader integration (medical loot groups plus
            trader medicine group), modeled similar to Aloe Cream
            availability.
          - Adds UI display wiring for dStopsSlows and duration on Bee
            Gone Cream, plus dStopsSlows display on Steroids.
          - Adds God-mode cleanup for Bee Gone Cream timer CVar to
            prevent stale state.
          - Feature set:
            - Craft or loot Bee Gone Cream as a dedicated anti-slow
              medication.
          - Use it on yourself or teammates.
          - Active effect clears current insect slow and prevents new
            Bee/Insect swarm slows while duration remains.
          - Steroids now provide the same anti-slow protection for
            consistency.



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

Notes
    - Notes
    - Notes
    - Notes
    - Notes
    - Notes
    - Notes
    - Notes
    - Notes
    - Add changelog entries here.