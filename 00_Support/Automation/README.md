# Automation (under 00_Support)

Cross-cutting automation for the mod lifecycle and occasional tools.
Lives under `00_Support/Automation/` so root stays human-facing.

```text
00_Support/Automation/
|-- workflow/     # Update/Publish pipeline (00_dispatch, 01..06, engine, docs)
|-- mod-tools/    # MakeNewMod and similar scaffold helpers
|-- launchers/    # Occasional bats (dry-runs) — not daily root entry points
|-- Logs/         # Pipeline run logs + manifests (written by 05_pipeline_engine)
`-- mcp-servers/  # Parked experimental Cursor MCP stubs (unused)
```

Daily root launchers stay at the repo root (`RUN-Update.bat`, `RUN-Publish.bat`,
`RUN-Publish-NoGigglePack.bat`, `RUN-MakeNewMod.bat`, `RUN-ImageIntake.bat`,
`RUN-SendDiscordUpdate.bat`) and call into these folders.
Pipeline entry is `workflow/00_dispatch.py` (formerly root `SCRIPT-Main.py`).
