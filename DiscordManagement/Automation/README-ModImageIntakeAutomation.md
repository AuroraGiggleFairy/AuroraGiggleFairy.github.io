# Mod Image Intake Automation

Script: `SCRIPT-DiscordModImageIntake.py`
Config: `Workflow/Discord/mod-image-intake-config.json`

## What It Does

- `inspect`: Shows what already exists in the `mod-image-intake` forum channel.
- `seed`: Creates missing mod threads from `mod-image-intake-thread-seed.csv` and attaches each mod's generated reference image.
- `sync`: Reads forum threads and updates `mod-image-intake-tracker.csv` with:
  - `discord_thread_url`
  - `submitted_count` (image attachments from non-bot users)
  - `intake_status` / `main_submission_status` (`Needed` or `Submitted`)
  - `last_update_utc`
- `seed-sync`: Runs seed then sync.

## Run

Set token in your shell first:

```powershell
$env:AGF_DISCORD_BOT_TOKEN = "<token>"
```

Inspect current forum state:

```powershell
c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-DiscordModImageIntake.py --config Workflow/Discord/mod-image-intake-config.json --action inspect
```

Seed missing threads (all rows):

```powershell
c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-DiscordModImageIntake.py --config Workflow/Discord/mod-image-intake-config.json --action seed
```

Seed a small test batch first:

```powershell
c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-DiscordModImageIntake.py --config Workflow/Discord/mod-image-intake-config.json --action seed --limit 5
```

Sync submissions into tracker:

```powershell
c:/GitHub/7D2D-Mods/.venv/Scripts/python.exe SCRIPT-DiscordModImageIntake.py --config Workflow/Discord/mod-image-intake-config.json --action sync
```

## Notes

- The script auto-resolves `forum_channel_id` by channel name if ID is 0, then saves it into config.
- Existing threads are not recreated if title already exists in active forum threads.
- Generated reference image is attached on thread creation when the file exists.
