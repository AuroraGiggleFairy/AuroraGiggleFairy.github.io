========================================================================
                        AGF-NOEAC-SCREAMERALERT                         
========================================================================

Sends private chat alerts for Screamers and Screamer hordes within 120m.


NOTE: AGF Mod Guide and Changelog are further below.


------------------------------------------------------------------------
MOD SCOPE
------------------------------------------------------------------------

  - Mod Version: 2.3.0
  - 7d2d Version: 3
  - Website: https://auroragigglefairy.github.io/
  - Languages Supported: All 13 game-supported languages.
  - Mod Type: Server-Side (EAC Off)
    - EAC off required.
    - Server install works for all joining players.
    - Also works in singleplayer.
  - Safe to install on existing game: Yes (Safe)
  - Safe to remove from existing game: Unknown
  - Dependencies: Requires 0_TFP_Harmony (built-in game mod).
    - To check, explore Installed Files and confirm Mods/0_TFP_Harmony
      exists.
    - To restore, use Steam Verify: Right click 7 Days to Die ->
      Properties -> Installed Files -> Verify integrity of game files.


------------------------------------------------------------------------
FEATURES
------------------------------------------------------------------------

  - Sends private chat alerts when Screamers or Screamer-spawned horde
    zombies spawn within 120m.
  - Players can toggle alerts with chat commands: /agfsa on and /agfsa
    off.
  - Commands README is available for players and admins.
  - If a player installs EnhancedAGF locally, live on-screen alerts are
    shown instead of private chat messages.


------------------------------------------------------------------------
OTHER DETAILS
------------------------------------------------------------------------

- Detects Screamers and Screamer hordes within 120 meters.
            - Sends private chat alerts for qualifying enemies within
              120 meters.
            - If a player installs EnhancedAGF locally, live on-screen
              alerts are shown instead of private chat messages.
            - COUNT mode requires EnhancedAGF locally; without it, COUNT
              resolves to ON.

            - Player Commands:
              - /agfsa: Show current alert setting.
            - /agfsa on: Turn alerts on.
            - /agfsa off: Turn alerts off.
            - /agfsa count: Set COUNT mode. Requires local EnhancedAGF;
              otherwise resolves to ON.

            - Admin Console Commands (F1): agf-sa help: Show admin
              command usage.
            - agf-sa default <off|on|count>: Set the default mode for
              new joining players.
            - agf-sa set <entityId|all> <off|on|count|default>: Set mode
              for one online player or all online players.
            - agf-sa list: List online players with entityId, mode, and
              EnhancedAGF capability state.
            - See the Commands README for full command details and
              examples.
