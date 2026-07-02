# Screamer Alert Commands README

This file is a general command guide for players and admins.

For full command variants and exact response scripting, use TEMP-ScreamerAlert-CommandResponses.txt in this same folder.

## Player Chat Commands

### Quick Use

1. /agf-sa
2. /agf-sa on
3. /agf-sa off
4. /agf-sa count (EnhancedAGF install only)

### 1. /agf-sa

- Shows current Screamer Alert mode and available options.
- Aliases: /agfsa, /agf-sa help, /agfsa help, /agf-sa status, /agfsa status


### 2. /agf-sa on

- Enables Screamer Alert.
- Aliases: /agfsa on

### 3. /agf-sa off

- Disables Screamer Alert.
- Aliases: /agfsa off

### 4. /agf-sa count (EnhancedAGF install only)

- Enables count-capable alert behavior when EnhancedAGF is available.
- Aliases: /agfsa count, /agf-sa counts, /agfsa counts, /agf-sa numbers, /agfsa numbers
- Note: If EnhancedAGF is not installed, this command switches to ON and tells you in the response.




## Admin Console Commands

### Quick Use

1. agf-sa
2. agf-sa default <off|on|count>
3. agf-sa set <entityId|all> <off|on|count|default>
4. agf-sa list

### 1. agf-sa

- Shows admin usage/help plus the current default value.
- Aliases: agf-sa, agf-sa help, agf-sa default



### 2. agf-sa default <off|on|count>

- Sets the default used for first-time joining players.
- If default is COUNT, non-EnhancedAGF players fall back to ON.


### 3. agf-sa set <entityId|all> <off|on|count|default>

- Sets Screamer Alert mode for one online player by entityId, or for all online players.
- Using default applies the current default setting immediately.
- COUNT (or default=COUNT) falls back to ON for non-EnhancedAGF players.
- all applies to currently online players.


### 4. agf-sa list

- Lists online players and their Screamer Alert state.
- Includes: player name, entityId, mode, and EnhancedAGF capability state.

## Admin Error Handling

- Invalid admin command usage returns support/help messages with expected syntax.
- Example: agf-sa default banana -> invalid option. Use: agf-sa default <off|on|count>.

## Behavior Notes

- COUNT resolves to ON when EnhancedAGF=NO or EnhancedAGF=UNKNOWN for that target.
- set all applies to online players only.
- default controls baseline behavior for new joiners.

