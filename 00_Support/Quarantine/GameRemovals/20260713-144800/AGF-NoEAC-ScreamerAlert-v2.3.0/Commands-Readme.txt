========================================================================
                    AGF-NOEAC-SCREAMERALERT COMMANDS
========================================================================


------------------------------------------------------------------------
PLAYER CHAT COMMANDS
------------------------------------------------------------------------

Quick Use:
  1. /agf-sa
  2. /agf-sa on
  3. /agf-sa off
  4. /agf-sa count (EnhancedAGF install only)

1. /agf-sa
  - Shows current Screamer Alert mode and available options.
  - Aliases: /agfsa, /agf-sa help, /agfsa help, /agf-sa status,
    /agfsa status.

2. /agf-sa on
  - Enables Screamer Alert.
  - Alias: /agfsa on.

3. /agf-sa off
  - Disables Screamer Alert.
  - Alias: /agfsa off.

4. /agf-sa count (EnhancedAGF install only)
  - Enables count-capable alert behavior when EnhancedAGF is available.
  - Aliases: /agfsa count, /agf-sa counts, /agfsa counts,
    /agf-sa numbers, /agfsa numbers.
  - Without EnhancedAGF, this command resolves to ON.


------------------------------------------------------------------------
ADMIN CONSOLE COMMANDS (F1)
------------------------------------------------------------------------

Quick Use:
  1. agf-sa
  2. agf-sa default <off|on|count>
  3. agf-sa set <entityId|all> <off|on|count|default>
  4. agf-sa list

1. agf-sa
  - Shows admin usage/help and the current default mode.
  - Alias: agf-sa help.

2. agf-sa default <off|on|count>
  - Sets the default mode for new joining players.
  - If default is COUNT, non-EnhancedAGF players use ON.

3. agf-sa set <entityId|all> <off|on|count|default>
  - Sets mode for one online player by entityId, or for all online
    players.
  - Using default applies the current default mode immediately.
  - If target mode resolves to COUNT without EnhancedAGF, it uses ON.
  - all applies to currently online players.

4. agf-sa list
  - Lists online players and their current Screamer Alert state.
  - Output row format: <index>. id=<entityId>, <playerName>,
    sa=<OFF|ON|COUNT>, enhanced=<YES|NO>.
  - list sends a capability probe and waits briefly for replies.
  - if capability does not answer within that timed wait, it reports NO.
  - Ends with total online count.


------------------------------------------------------------------------
ADMIN ERROR HANDLING
------------------------------------------------------------------------

  - Invalid admin command usage returns help text with expected syntax.
  - Example:
    agf-sa default banana
    -> invalid option. Use: agf-sa default <off|on|count>.


------------------------------------------------------------------------
BEHAVIOR NOTES
------------------------------------------------------------------------

  - /agf-sa and /agfsa are both valid command roots.
  - /agfsa with no argument is treated as status.
  - /agfsa help and /agfsa status use the same status response path.
  - COUNT resolves to ON when EnhancedAGF is NO.
  - set all applies to online players only.
  - default controls baseline behavior for new joiners.