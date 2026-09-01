# Visual Entity Tracker Addon Commands README

This file is a general command guide for players and admins.

For full command variants and exact response scripting, use TEMP-VisualEntityTrackerAddon-CommandResponses.txt in this same folder.

## Player Chat Commands

### Quick Use

1. /agfvet
2. /agfvet on
3. /agfvet off

### 1. /agfvet

- Shows current Visual Entity Tracker mode and available options.
- Aliases: /agf-vet, /agfvet help, /agf-vet help, /agfvet status, /agf-vet status

### 2. /agfvet on

- Enables Visual Entity Tracker.
- Alias: /agf-vet on

### 3. /agfvet off

- Disables Visual Entity Tracker.
- Alias: /agf-vet off

## Admin Console Commands

### Quick Use

1. agf-vet
2. agf-vet default <off|on>
3. agf-vet set <entityId|all> <off|on|default>
4. agf-vet list

### 1. agf-vet

- Shows admin usage/help plus the current default value.
- Aliases: agf-vet, agf-vet help, agf-vet default

### 2. agf-vet default <off|on>

- Sets the default used for first-time joining players.

### 3. agf-vet set <entityId|all> <off|on|default>

- Sets Visual Entity Tracker mode for one online player by entityId, or for all online players.
- Using default applies the current default setting immediately.
- all applies to currently online players.

### 4. agf-vet list

- Lists online players and their Visual Entity Tracker state.
- Includes: player name, entityId, and mode.

## Admin Error Handling

- Invalid admin command usage returns support/help messages with expected syntax.
- Example: agf-vet default banana -> invalid option. Use: agf-vet default <off|on>.

## Behavior Notes

- /agfvet and /agf-vet are both valid chat command roots.
- /agfvet with no argument is treated as status.
- /agfvet help and /agfvet status use the same status response path.
- set all applies to online players only.
- default controls baseline behavior for new joiners.
