# Lockable Stations Commands README

This file is a general command guide for players and admins.

For exact chat/console response wording, use TEMP-LockableStations-CommandResponses.txt in this same folder (same layout as Screamer Alert).

## Player Chat Commands

### Quick Use

1. /agfls
2. /agfls status
3. /agfls lock
4. /agfls unlock
5. /agfls pin set \<pin\>
6. /agfls pin clear
7. /agfls pin use \<pin\>

Look at a supported station (about 15m). Chat roots: `/agfls` (player-facing) and `/agf-ls`.

### 1. /agfls

- Shows the command name and available options.
- Aliases: /agf-ls, /agfls help, /agf-ls help

### 2. /agfls status

- Reports lock, owner, and keypad state for the focused station.

### 3. /agfls lock

- Locks the focused station. Owner or admin.

### 4. /agfls unlock

- Unlocks the focused station. Owner or admin.

### 5. /agfls pin set \<pin\>

- Sets a keypad pin and locks the station. Owner or admin.

### 6. /agfls pin clear

- Clears the keypad pin. Owner or admin.

### 7. /agfls pin use \<pin\>

- Grants access with a keypad pin.

## Admin Console Commands

F1 is admin-only, same pattern as Screamer Alert. Prefix `[LockableStations]`. No chat colors. No slash.

### Quick Use

1. agf-ls
2. agf-ls defaultlock \<true|false\>
3. agf-ls nearby lock
4. agf-ls nearby unlock

### 1. agf-ls

- Shows admin usage plus the current defaultlock value.
- Aliases: agf-ls, agf-ls help, agf-ls defaultlock

### 2. agf-ls defaultlock \<true|false\>

- Sets whether newly placed stations and first-seen stations on this save start locked. This session only (does not write xml).

### 3. agf-ls nearby lock / unlock

- Locks or unlocks this mod's stations in range. Range matches Sorting Cart land-claim size for this game.
- Inside a land claim: everything in that claim (loaded chunks).
- Outside a claim: land-claim size centered on the player (loaded chunks).

## Admin Error Handling

- Non-admin: `[LockableStations] admin permission required.`
- Invalid: `[LockableStations] invalid option. Use: agf-ls help.`

## Behavior Notes

- Commands use the same look target as the interaction prompt, out to 15m.
- Multi-block stations resolve to the parent tile.
- Server whispers are not treated as new commands.
- Chat response colors match Screamer Alert / Visual Entity Tracker (purple status, green options, pink error).
- lock, unlock, status, and pin are chat only.
- defaultlock and nearby are F1 only. Chat `/agfls defaultlock` is not an admin command.
- Installing on an existing save: stations with no saved record use current defaultlock the first time they are seen.
