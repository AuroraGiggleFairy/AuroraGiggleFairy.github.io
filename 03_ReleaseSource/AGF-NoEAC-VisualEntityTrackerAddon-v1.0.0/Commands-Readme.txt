========================================================================
              AGF-NOEAC-VISUALENTITYTRACKERADDON COMMANDS
========================================================================


------------------------------------------------------------------------
PLAYER CHAT COMMANDS
------------------------------------------------------------------------

Quick Use:
  1. /agfvet
  2. /agfvet on
  3. /agfvet off

1. /agfvet
  - Shows current Visual Entity Tracker mode and available options.
  - Aliases: /agf-vet, /agfvet help, /agf-vet help, /agfvet status,
    /agf-vet status.

2. /agfvet on
  - Enables Visual Entity Tracker.
  - Alias: /agf-vet on.

3. /agfvet off
  - Disables Visual Entity Tracker.
  - Alias: /agf-vet off.


------------------------------------------------------------------------
ADMIN CONSOLE COMMANDS (F1)
------------------------------------------------------------------------

Quick Use:
  1. agf-vet
  2. agf-vet default <off|on>
  3. agf-vet set <entityId|all> <off|on|default>
  4. agf-vet list

1. agf-vet
  - Shows admin usage/help and the current default mode.
  - Alias: agf-vet help.

2. agf-vet default <off|on>
  - Sets the default mode for new joining players.

3. agf-vet set <entityId|all> <off|on|default>
  - Sets mode for one online player by entityId, or for all online
    players.
  - Using default applies the current default mode immediately.
  - all applies to currently online players.

4. agf-vet list
  - Lists online players and their current Visual Entity Tracker state.
  - Output row format: <index>. id=<entityId>, <playerName>,
    vet=<OFF|ON>.
  - Ends with total online count.


------------------------------------------------------------------------
ADMIN ERROR HANDLING
------------------------------------------------------------------------

  - Invalid admin command usage returns help text with expected syntax.
  - Example:
    agf-vet default banana
    -> invalid option. Use: agf-vet default <off|on>.


------------------------------------------------------------------------
BEHAVIOR NOTES
------------------------------------------------------------------------

  - /agfvet and /agf-vet are both valid command roots.
  - /agfvet with no argument is treated as status.
  - /agfvet help and /agfvet status use the same status response path.
  - set all applies to online players only.
  - default controls baseline behavior for new joiners.
