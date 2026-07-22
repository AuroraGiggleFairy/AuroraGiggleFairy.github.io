# Fairy Bot Discord Workflow

Last updated: 2026-07-15

## Scope

This workflow tracks the Fairy Discord bot setup and related Discord server plan work so it can be resumed quickly.

## Current Snapshot

- Local planning/config updates were completed in:
  - DiscordManagement/ServerPlan/discord_server_plan.json
  - DiscordManagement/ServerPlan/AuroraTwilight-Discord-Management.md
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
  - DiscordManagement/ServerPlan/discord_server_plan.json
- Human-facing matrix mirror:
  - DiscordManagement/ServerPlan/AuroraTwilight-Discord-Management.md
- Apply script (when intentionally applying to Discord):
  - DiscordManagement/ServerPlan/apply_discord_server_plan.py

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

## Hosting Options (researched 2026-07-15)

### Oracle OCI (current attempt)
- VCN `fairy-vcn` and subnet `fairy-public-subnet` already created.
- **Always Free shapes exhausted** across all 3 ADs in home region — A1.Flex (4 OCPU / 24GB) and E2.1.Micro (1 OCPU / 1GB) both out of capacity. Cannot switch regions.
- Upgraded to **PAYG account** — still hitting Out of Capacity on A1.Flex. PAYG gives higher priority but region is saturated.
- **Cheapest paid Oracle shape:** VM.Standard.E2.1 at ~$18/month (1 OCPU, 8GB RAM) — provisions reliably since it's not free-tier.
- **Oracle Cloud Shell workaround:** Write a retry script using `oci compute instance launch` in a loop across AD-1/2/3 every 60 seconds until a slot opens.
- Bot needs very little: Python Discord bot with yt-dlp, polling every 120s. 512MB RAM is overkill.

### Gravel Host (recommended alternative)
- **gravelhost.com** — dedicated Discord bot hosting.
- **$2.50/month** — 512MB RAM, 1 CPU core, 24/7 uptime, auto-restarts, web panel.
- No infrastructure management — upload bot files, set env vars, click start.
- Setup steps:
  1. Create account at gravelhost.com
  2. Create a Python bot node ($2.50/mo)
  3. Upload: SCRIPT-DiscordReactionRolesMulti.py, reaction-role-panels.json, streamer-watchlist.json, requirements-discord-tools.txt
  4. Set env var: AGF_DISCORD_BOT_TOKEN
  5. Startup command: `python SCRIPT-DiscordReactionRolesMulti.py --watchlist streamer-watchlist.json`
  6. Click start
- No SSH, no systemd, no capacity fighting.

### Other Cheap Alternatives (not pursued)
- **Hetzner CX11** — ~€3.29/month (1 vCPU, 2GB RAM, 20GB SSD)
- **Vultr** — $2.50/month (1 vCPU, 512MB, 10GB)
- **DigitalOcean** — $4/month (1 vCPU, 512MB, 10GB)
- **AWS EC2 t4g.nano** — ~$3.20/month (ARM, 2 vCPU, 512MB)

## Change History

### 2026-07-15

- Added hosting options section (Oracle paid shapes, Gravel Host, other cheap VPS alternatives).
- Updated notes after Oracle PAYG upgrade still yielded Out of Capacity.

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

1. Decide hosting path:
   - **Gravel Host ($2.50/mo)** — upload bot files, click start, done.
   - **Oracle VM.Standard.E2.1 ($18/mo)** — provisions reliably, needs SSH + systemd setup.
   - **Oracle A1.Flex retry** — run Cloud Shell retry script across AD-1/2/3 until capacity opens.
   - **Local machine** — bot runs only while your PC is on.
2. Confirm runtime config values:
   - Workflow/Discord/reaction-role-panels.json
   - Workflow/Discord/streamer-watchlist.json
3. Start bot and verify process is running.
4. Test command flow in Discord:
   - /mystreams
   - /setstream (if needed)
   - /golive (manual test)
5. If cloud hosting is used, validate SSH, then run the same bot command under a persistent service process.
