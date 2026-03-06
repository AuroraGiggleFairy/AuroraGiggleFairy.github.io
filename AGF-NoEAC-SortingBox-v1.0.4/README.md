AGF Modified - SortingBox
7d2d Version 2 - v1.0.4

AGF Renamed to SortingBox
Permission is Granted, credits below.

AGF Changelog:
v1.0.4
- The sort of storages respects locked slots.

v1.0.3
- Added to ignore it when EAC is loaded.

v1.0.2
- Thanks to Asylum for the .dll update.
- Increased size of the sorting box's storage depending on using one of my backpacks or not.

v1.0.1
-Fix for sorting boxes ignoring the locked slots of storages.

===============================================================================
                           ASYLUM ROBOTIC INBOX MOD
===============================================================================

WHAT IT DOES:
The Asylum Robotic Inbox is a special container that automatically sorts and 
distributes items to other nearby storage containers. Simply place items in 
the Robotic Inbox and it will automatically move them to containers that 
already contain similar items.

MAIN FEATURES:
- Automatic item sorting to nearby containers (25-meter radius by default)
- Respects lock states and passwords
- Follows land claim boundaries
- Multiple color variants available
- Repairable locks if broken
- Works with all player-placed storage containers

HOW TO GET A ROBOTIC INBOX:
1. Craft one with Forged Iron

HOW TO USE:
1. Place a Robotic Inbox near your storage containers
2. Put items in the Robotic Inbox
3. Items automatically move to containers that already contain the same type
4. Items that can't be sorted stay in the inbox for manual placement
5. Hold Action Key to lock/unlock or set password

SECURITY RULES:
- Locked inboxes only distribute to locked containers with same password
- Unlocked inboxes only distribute to unlocked containers
- Won't move items outside your land claim area
- Ignores backpacks, vehicles, and non-player storage

ADMIN CONFIGURATION COMMANDS:
- ri horizontal-range <number> - Set scanning width (default: 5, max: 128)
- ri vertical-range <number> - Set scanning height (default: 5, max: 253)
- ri success-notice-time <seconds> - Success message duration (default: 2.0)
- ri blocked-notice-time <seconds> - Blocked message duration (default: 3.0)
- ri base-siphoning-protection true/false - Land claim protection (default: true)
- ri debug true/false - Toggle debug logging
- help roboticinbox - Show all available commands

COMPATIBILITY:
- Works on Dedicated Servers (EAC can stay enabled)
- Works on Single Player and P2P (host must disable EAC)
- Can be added to existing maps without issues

CREDITS:
Original mod by Jonathan Robertson (Kanaverum)
Updated for Asylum Mods v2.0 with permission from original author
Maintained by Asylum Mods
Modified by AuroraGiggleFairy

===============================================================================
