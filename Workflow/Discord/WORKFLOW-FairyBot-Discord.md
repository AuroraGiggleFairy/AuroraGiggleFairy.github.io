# Fairy Bot Discord Workflow

Last updated: 2026-07-11

## Scope

This workflow tracks the Fairy Discord bot setup and related Discord server plan work so it can be resumed quickly.

## Current Snapshot

- Local planning/config updates were completed in:
  - _DLL-Projects/Support_DiscordAutomation/discord_server_plan.json
  - _DLL-Projects/Support_DiscordAutomation/AuroraTwilight-Discord-Management.md
  - Workflow/Discord/mod-image-intake-config.json
- Runtime bot script for roles + live announcements:
  - SCRIPT-DiscordReactionRolesMulti.py
- Runtime bot config/state files:
  - Workflow/Discord/reaction-role-panels.json
  - Workflow/Discord/streamer-watchlist.json
- Auto live announcements were not firing during troubleshooting because no Python bot process was running.
- Temporary local plan hold: removed server-bot and streamer-auto-live-setup, while keeping bot-updates and test-for-agf.

## Working Methods

### 1) Plan and docs source of truth

- Discord structure source of truth:
  - _DLL-Projects/Support_DiscordAutomation/discord_server_plan.json
- Human-facing matrix mirror:
  - _DLL-Projects/Support_DiscordAutomation/AuroraTwilight-Discord-Management.md
- Apply script (when intentionally applying to Discord):
  - _DLL-Projects/Support_DiscordAutomation/apply_discord_server_plan.py

### 2) Bot run method (local)

Install dependencies:

```powershell
pip install -r Workflow/Discord/requirements-discord-tools.txt
```

Set token in environment:

```powershell
$env:AGF_DISCORD_BOT_TOKEN = "YOUR_BOT_TOKEN"
```

Run bot:

```powershell
python SCRIPT-DiscordReactionRolesMulti.py --config Workflow/Discord/reaction-role-panels.json --watchlist Workflow/Discord/streamer-watchlist.json
```

Optional publish refresh on startup:

```powershell
python SCRIPT-DiscordReactionRolesMulti.py --config Workflow/Discord/reaction-role-panels.json --watchlist Workflow/Discord/streamer-watchlist.json --publish-on-start
```

### 3) Bot health checks

Check if Python bot process is alive:

```powershell
Get-CimInstance Win32_Process | Where-Object { $_.Name -match 'python|py' } | Select-Object ProcessId, Name, CommandLine | Format-List
```

### 4) Stream announcement behavior

- Required for auto announcements:
  - Bot process is running continuously.
  - Stream links are registered (for example with /setstream).
  - Live announcement destination resolves from either:
    - live_announcement_channel_id (preferred), or
    - live_announcement_channel_name fallback.
- Related commands in script:
  - /setstream
  - /golive
  - /mystreams
- Official server announcement relays (Discord channel follows/webhooks) are not handled by this script.

### 5) Oracle always-on host method (current learning)

- Existing network resources created:
  - VCN: fairy-vcn
  - Subnet: fairy-public-subnet (public)
- A1 launch attempts failed due to host capacity in all ADs during this session.
- Key compatibility rule:
  - A1 requires Arm/aarch64 image compatibility.
  - E2 micro requires x86_64 image compatibility.
- Capacity warning can indicate AD-specific availability for a shape.

## Do-Not-Do Notes

- Do not assume config/doc changes become active in Discord until apply/run steps are executed.
- Do not treat official Discord announcement relays as part of SCRIPT-DiscordReactionRolesMulti.py.
- Do not paste malformed or duplicated SSH key strings in compute launch forms.
- Do not rebuild VCN/subnet repeatedly once fairy-vcn + fairy-public-subnet already exist.
- Do not expect shape availability messages to override image architecture compatibility filters.

## Change History

### 2026-07-11

- Added dedicated Fairy Bot workflow document for resume-later continuity.
- Recorded validated local bot run method, required files, and health checks.
- Recorded Oracle compute blockers and shape/image compatibility notes from setup attempts.
- Recorded temporary bot-channel adjustment in local planning files only (no live apply): remove server-bot and streamer-auto-live-setup; keep bot-updates and test-for-agf.
- Executed Discord plan in APPLY mode against live guild with zero member role updates; structural and permission/onboarding sync completed.
- Repaired live duplicate/move issue: deleted blank duplicate FOR AGF channels, moved existing my-notes and bot-updates into FOR AGF, and removed server-bot plus streamer-auto-live-setup.
- Post-repair dry-run now reports Created categories/channels = 0 and Renamed categories/channels = 0.
- Additional live repair: renamed existing help-is-here to ask-for-help-here (kept history), removed blank duplicate ask-for-help-here, moved original agf-server-info/server-chat/server-updates/playing-now from 7D2D HANGOUT into 7D2D COMMUNITY SERVER, and removed blank duplicates created there.
- Verified post-repair dry-run remains structure-clean (Created/Renamed categories/channels all zero, Member role updates zero).

## Resume Checklist

1. Decide immediate path:
   - Local machine always-on for now, or
   - Oracle VM retry when target shape capacity is available.
2. Confirm runtime config values:
   - Workflow/Discord/reaction-role-panels.json
   - Workflow/Discord/streamer-watchlist.json
3. Start bot and verify process is running.
4. Test command flow in Discord:
   - /mystreams
   - /setstream (if needed)
   - /golive (manual test)
5. If cloud hosting is used, validate SSH, then run the same bot command under a persistent service process.
