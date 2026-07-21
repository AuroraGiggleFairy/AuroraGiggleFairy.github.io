# Discord Management

Administrative source and tooling for the Discord server itself. This domain is
separate from DLL source development and from continuously running bot code.

`ServerPlan/` contains the human plan, machine-readable server plan, and the
dry-run/apply tooling used to manage Discord structure.

> Note: `apply_discord_server_plan.py` is currently stored in a legacy encoding
> that standard Python rejects without an encoding declaration. That pre-existing
> issue was not changed during the folder migration.
