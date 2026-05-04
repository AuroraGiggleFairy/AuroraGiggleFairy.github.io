# Mod Type Descriptions

### Mod Types with Simple Descriptions
0 To Be Determined.
1 Server-side (EAC-friendly): Server install works for all joining players; EAC on or off.
2 Server-side (EAC Off): EAC off required; server install works for all joining players.
3 Server/Client-side (Required): EAC off required; host and joining players must install it.
4 Client-side (Only): EAC off required; server install has no effect; only the installing player gets the feature.
5 Hybrid (EAC Off): EAC off required; server install works for all joining players; client install is optional for extra features.
6 Server-side (Dedicated Only, EAC Off): EAC off required; dedicated uses server install only, but player-hosted requires host and joining players to install it.


### Legend
- EAC Friendly
  - Yes: Works with EAC on
  - No: Requires EAC off

- Server Side (Player Host)
  - Yes: Host install alone works for all joining players
  - No: Joining players must install it locally
  - Hybrid: Players can join without it; install locally for full features.
  - N/A: Installing on the host has no effect.

- Server Side (Dedicated Host)
  - Yes: Server install alone works for all joining players
  - No: Joining players must install it locally
  - Hybrid: Players can join without it; install locally for full features.
  - N/A: Installing on the server has no effect.

- Client Side
  - Required: Clients must install to join or function correctly
  - Optional: Clients can join without it, but may miss features
  - None: Clients do not need this; local install has no effect
  - Only: Requires EAC off; ignores server mod requirements and applies only to the installing player.
