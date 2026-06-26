# AGF Capability Contract Template v1

Purpose
- Define one reusable handshake contract between AGF server mods and AGF client enhancement mods.
- Allow mixed player populations on one server: baseline server-only and enhanced client-capable.

Scope
- Works for EAC-off AGF DLL mods.
- Contract defines capability detection and routing, not gameplay logic.

## 1) Contract Identity
- Contract name: agf.capability
- Contract version: 1
- Backward compatibility rule: unknown features are ignored.

## 2) Handshake Payload (Template)
Required fields
- contractName (string)
- contractVersion (int)
- playerEntityId (int)
- userCombined (string)
- features (string list)

Optional fields
- clientModName (string)
- clientModVersion (string)
- sessionNonce (string)

Feature naming pattern
- <mod>.<feature>.<level>
Example
- screameralert.enhanced.v1

## 3) Session Lifecycle Rules
Join/Spawn
- Client sends handshake after local player spawns in world.
- Server marks capability by entity and user identity.

Disconnect
- Server removes entity mapping immediately.
- Server decrements/removes user capability references to avoid sticky state.

Reconnect
- Capability is re-established only by a new handshake in the new session.

## 4) Server Classification States
- server-baseline: no valid enhanced capability for that player in this session.
- enhanced-client: player reported supported feature in this session.

## 5) Routing Policy Template
- If player is enhanced-client for a feature:
  - Do not send fallback baseline message for that same feature channel.
- If player is server-baseline:
  - Send fallback baseline message.

## 6) Timeout/Fallback Rules
- If no handshake is received within startup window, treat player as server-baseline.
- On malformed handshake, ignore packet and keep server-baseline behavior.

## 7) Command Policy Template
Player commands
- server-baseline: keep safe minimal commands.
- enhanced-client: allow extended quality-of-life commands.

Admin commands
- Always available to admin.
- Admin can query capability state per entity.

## 8) Logging Template
Recommended events
- handshake accepted/rejected (rate-limited)
- capability set/remove
- fallback route chosen due to missing capability

## 9) Screamer Alert Example Mapping
Feature
- screameralert.enhanced.v1

Policy
- enhanced-client: local enhanced alert UX path.
- server-baseline: server fallback whisper alerts.

## 10) Change Management
- Increment contractVersion for breaking payload changes.
- Keep prior parser for at least one transition release when possible.
