# Discord Management

Administrative source and tooling for the Discord server itself. This domain is
a separate animal from the mod lifecycle, but it stays in this same workspace.
It is intentionally unnumbered at the repo root (not under `00_Support`).

Separate from DLL source development and from the mod pipeline under
`00_Support/Automation/workflow`.

```text
DiscordManagement/
|-- ServerPlan/     # human plan, machine-readable server plan, dry-run/apply tooling
`-- Automation/     # reaction-role bot, configs, FairyBot docs
```

Active launcher: `Automation/SCRIPT-DiscordReactionRolesMulti.py`.

Role-selector and mod-image-intake scripts were removed (no longer used); leftover
config/docs under `Automation/` are reference-only.

Release announcement posting stays under `05_ReleaseData/GigglePack/Discord/`.

> Note: `ServerPlan/apply_discord_server_plan.py` is currently stored in a legacy
> encoding that standard Python rejects without an encoding declaration. That
> pre-existing issue was not changed during the folder migration.
