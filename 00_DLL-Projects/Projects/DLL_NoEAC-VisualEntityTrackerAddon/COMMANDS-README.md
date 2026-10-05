# Visual Entity Tracker Addon Commands README

This file is a general command guide for players and admins.

For full command variants and exact response scripting, use TEMP-VisualEntityTrackerAddon-CommandResponses.txt in this same folder.

## Player Chat Commands

### Quick Use

1. /agfet
2. /agfet on
3. /agfet off

### 1. /agfet

- Shows current Visual Entity Tracker mode and available options.
- Aliases: /agf-et, /agfet help, /agf-et help, /agfet status, /agf-et status

### 2. /agfet on

- Enables Visual Entity Tracker.
- Alias: /agf-et on

### 3. /agfet off

- Disables Visual Entity Tracker.
- Alias: /agf-et off

## Admin Console Commands

### Quick Use

1. agf-et
2. agf-et default <off|on>
3. agf-et set <entityId|all> <off|on|default>
4. agf-et list

### 1. agf-et

- Shows admin usage/help plus the current default value.
- Aliases: agf-et, agf-et help, agf-et default

### 2. agf-et default <off|on>

- Sets the default used for first-time joining players.

### 3. agf-et set <entityId|all> <off|on|default>

- Sets Visual Entity Tracker mode for one online player by entityId, or for all online players.
- Using default applies the current default setting immediately.
- all applies to currently online players.

### 4. agf-et list

- Lists online players and their Visual Entity Tracker state.
- Includes: player name, entityId, and mode.

## Admin Error Handling

- Invalid admin command usage returns support/help messages with expected syntax.
- Example: agf-et default banana -> invalid option. Use: agf-et default <off|on>.

## Behavior Notes

- /agfet and /agf-et are both valid chat command roots.
- /agfet with no argument is treated as status.
- /agfet help and /agfet status use the same status response path.
- set all applies to online players only.
- default controls baseline behavior for new joiners.
