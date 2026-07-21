# LockableWorkstations Chat Commands

These commands are available through chat/console using `lw`.
Aliases: `lockws`, `lockableworkstations`.

All commands operate on the block you are currently looking at (focused block), up to 5 meters away.

## Status

`lw status`
- Shows the focused block coordinates and lock details:
- lock state
- owner id
- whether a keypad code exists
- allowed-user count

## Lock State

`lw lock`
- Locks the focused block.
- If no owner exists and the caller has a player identity, caller becomes owner.

`lw unlock`
- Unlocks the focused block.

## Keypad

`lw code set <code>`
- Owner/admin only.
- Sets keypad code for focused block and clears previous allowed list.
- If unlocked, it also locks the block.

`lw code clear`
- Owner/admin only.
- Removes keypad code from focused block.

`lw code use <code>`
- Player identity required.
- If code matches, caller is added to that block's allowed-user list.

## Permissions

- Admins can manage any lockable workstation.
- Non-admins can manage workstations they own, or where owner ACL allows them.
- Commands require an in-world player focus target and do not support console/telnet execution.

## Supported Blocks

- Workstations
- Collectors
- Power sources
- Powered ranged traps
