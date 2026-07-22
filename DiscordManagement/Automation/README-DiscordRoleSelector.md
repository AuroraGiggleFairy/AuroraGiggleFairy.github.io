# Discord Role Selector Bot

This tool lets people self-assign roles (including a Viewer role) in one Discord channel.

## What It Does

- Publishes a role selection panel in your chosen channel.
- Lets users add/remove roles themselves.
- Supports button roles and reaction roles at the same time.
- Optionally supports a select menu.
- Updates existing panel message if you keep `message_id` in config.

## Files

- `SCRIPT-DiscordRoleSelectorBot.py`
- `Workflow/Discord/role-selector-config.example.json`
- `Workflow/Discord/requirements-discord-tools.txt`

## 1) Prepare Discord

1. Create or reuse a bot in Discord Developer Portal.
2. Enable these bot intents:
   - `SERVER MEMBERS INTENT`
   - `GUILD MESSAGE REACTIONS`
3. Give bot these permissions in your server/channel:
   - `View Channels`
   - `Send Messages`
   - `Manage Roles`
   - `Read Message History`
   - `Add Reactions`
4. Put the bot role above all roles it should grant/remove.

## 2) Prepare Config

1. Copy `Workflow/Discord/role-selector-config.example.json` to `Workflow/Discord/role-selector-config.json`.
2. Fill in your IDs and roles:
   - `guild_id`
   - `channel_id`
   - each `role_id`
   - each `emoji` if using reaction roles
3. Pick interaction modes:
   - `enable_buttons`: clickable role buttons
   - `enable_reactions`: reaction-based role toggles
   - `enable_select_menu`: dropdown style
4. Keep `message_id` as `0` for first run.

## 3) Install Dependency

```powershell
pip install -r Workflow/Discord/requirements-discord-tools.txt
```

## 4) Run Bot

Set token in environment:

```powershell
$env:AGF_DISCORD_BOT_TOKEN = "YOUR_BOT_TOKEN"
```

Run and publish panel right away:

```powershell
python SCRIPT-DiscordRoleSelectorBot.py --config Workflow/Discord/role-selector-config.json --publish-on-start
```

After first publish, the script writes `message_id` back into your config so future runs update the same message.

## 5) Ongoing Use

- Keep bot running for interaction handling.
- Admins with `Manage Roles` can use slash command `/publishroles` to refresh panel.

## Troubleshooting

- If users cannot receive roles, check role hierarchy first.
- If slash command not visible, confirm `guild_id` and restart bot.
- If config parse fails, validate JSON and role IDs.
- If reaction roles do not apply, verify each role option has a valid emoji and bot has Add Reactions permission.
