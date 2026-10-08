========================================================================
              AGF-NOEAC-VISUALENTITYTRACKERADDON COMMANDS
========================================================================


------------------------------------------------------------------------
PLAYER CHAT COMMANDS
------------------------------------------------------------------------

Quick Use:
  1. /agfet
  2. /agfet on
  3. /agfet off

1. /agfet
  - Shows current Visual Entity Tracker mode and available options.
  - Aliases: /agf-et, /agfet help, /agf-et help, /agfet status,
    /agf-et status.

2. /agfet on
  - Enables Visual Entity Tracker.
  - Alias: /agf-et on.

3. /agfet off
  - Disables Visual Entity Tracker.
  - Alias: /agf-et off.


------------------------------------------------------------------------
ADMIN CONSOLE COMMANDS (F1)
------------------------------------------------------------------------

Quick Use:
  1. agf-et
  2. agf-et default <off|on>
  3. agf-et set <entityId|all> <off|on|default>
  4. agf-et list

1. agf-et
  - Shows admin usage/help and the current default mode.
  - Alias: agf-et help.

2. agf-et default <off|on>
  - Sets the default mode for new joining players.

3. agf-et set <entityId|all> <off|on|default>
  - Sets mode for one online player by entityId, or for all online
    players.
  - Using default applies the current default mode immediately.
  - all applies to currently online players.

4. agf-et list
  - Lists online players and their current Visual Entity Tracker state.
  - Output row format: <index>. id=<entityId>, <playerName>,
    et=<OFF|ON>.
  - Ends with total online count.


------------------------------------------------------------------------
ADMIN ERROR HANDLING
------------------------------------------------------------------------

  - Invalid admin command usage returns help text with expected syntax.
  - Example:
    agf-et default banana
    -> invalid option. Use: agf-et default <off|on>.


------------------------------------------------------------------------
BEHAVIOR NOTES
------------------------------------------------------------------------

  - /agfet and /agf-et are both valid command roots.
  - /agfet with no argument is treated as status.
  - /agfet help and /agfet status use the same status response path.
  - set all applies to online players only.
  - default controls baseline behavior for new joiners.
