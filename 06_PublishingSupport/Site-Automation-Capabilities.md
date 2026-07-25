# Site Automation Capabilities (Human-Readable)

Last updated: 2026-07-25
Scope: Publish automation capabilities by site for AGF workflow decisions.

This file is the quick control panel:
- What can be automated now
- What is API-supported but not wired yet
- What is still manual

## Current Site Coverage

1. Nexus Mods
- Detailed capability sheet: 06_PublishingSupport/NexusMods/Workflow/Nexus-Automation-Capabilities.md
- Status: Status report bat + gated upload pipeline wired for existing mods (file version + changelog append).
- Day-to-day (from `NexusMods/` root): `RUN-Nexus-Status.bat` (also saves discovered IDs) → review `Nexus-Status.md` → optional `RUN-Nexus-Update*.bat` for existing pages. Scripts/config stay in `Workflow/`.
- Endpoint policy: use latest mod-files/mod-file-versions/changelogs routes first, then legacy fallback only when needed.
- Gap: New mod-page creation, page body text, and images are still manual.

2. The Mod Network
- Detailed capability sheet: 06_PublishingSupport/ModNetwork/Workflow/ModNetwork-Automation-Capabilities.md
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
