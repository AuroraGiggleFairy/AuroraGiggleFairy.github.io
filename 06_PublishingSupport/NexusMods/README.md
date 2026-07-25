# Nexus Mods publishing

This folder is your Nexus command center.

## Look here

| File | What it is |
|---|---|
| `Nexus-Status.md` | ReleaseSource vs Nexus (Needs Update / Matches / First Upload Needed) |
| `PublishHelp/` | Per-mod copy/paste packets (Details / FullDesc) |
| `RUN-Nexus-Status.bat` | Refresh status **and** save discovered Nexus IDs into config |
| `RUN-Nexus-Update.bat` | Push newer zip versions to **existing** Nexus pages |

## Typical flow

1. `RUN-Nexus-Status.bat` — see what’s out of date; config IDs update automatically  
2. For **First Upload Needed**: create the page once on Nexus, then re-run Status  
3. `RUN-Nexus-Update.bat` — update file versions on pages that already exist  

The API cannot create brand-new mod pages. It can only update files/versions (and append changelogs) on pages that already exist.

## Behind the scenes

Everything under `Workflow/` — scripts, `nexusmods-config.json`, templates, plans, capability notes.

API key: `nexus-api-key.private.txt` (gitignored).
