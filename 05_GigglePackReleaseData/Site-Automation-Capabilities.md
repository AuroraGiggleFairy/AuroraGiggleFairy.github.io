# Site Automation Capabilities (Human-Readable)

Last updated: 2026-06-29
Scope: Publish automation capabilities by site for AGF workflow decisions.

This file is the quick control panel:
- What can be automated now
- What is API-supported but not wired yet
- What is still manual

## Current Site Coverage

1. Nexus Mods
- Detailed capability sheet: 05_GigglePackReleaseData/NexusMods/Nexus-Automation-Capabilities.md
- Status: API supports upload sessions and file creation/versioning for existing mods.
- Endpoint policy: use latest mod-files/mod-file-versions routes first, then legacy fallback only when needed.
- Gap: New mod-page creation endpoint is not present in the local v3 OpenAPI snapshot.

2. The Mod Network
- Detailed capability sheet: 05_GigglePackReleaseData/ModNetwork/ModNetwork-Automation-Capabilities.md
- Status: Existing script supports publish/update and page metadata patching.
- Gap: Image upload automation is not currently wired in script.

3. 7daystodiemods
- Detailed capability sheet: not created yet
- Status: discovery not started in this file set yet.

## Decision Rules

1. If capability is marked Supported + Wired, use automation path.
2. If capability is marked Supported + Not Wired, implement in script before relying on it.
3. If capability is marked Not Exposed, keep it manual.
4. If capability is marked Unknown, run one small probe test and record result in the site sheet.
