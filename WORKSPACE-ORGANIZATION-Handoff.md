# Workspace Organization - Handoff (2026-07-22)

Start a new chat with: *"Continue the workspace organization work - see `WORKSPACE-ORGANIZATION-Handoff.md`."*

## Where the real detail lives

`WORKSPACE-ORGANIZATION-PLAN.md` Progress Log is the long-form record. This handoff is orientation only.

## Locked layout (Idea A)

```text
00_Images/
00_DLL-Projects/
00_Support/
  Automation/       # was 80_Automation
  WorkspaceData/    # was 82_WorkspaceData
  Archive/          # was 90_Archive
  Quarantine/       # was 91_Quarantine
  Temp/             # was 99_Temp
01_Draft/ … 06_PublishingSupport/
DiscordManagement/  # was 20_DiscordManagement (unnumbered; same workspace, different animal)
```

Root keepers: `README.md`, `RUN-*.bat`, org-plan docs.

## Git state

- Large **uncommitted** stack covering ReleaseData rename, Support nest, Discord rename, root loose-file triage, and path rewires.
- Confirm before commit/push.

## What just happened

- Nested machinery under `00_Support/` (Automation, WorkspaceData, Archive, Quarantine, Temp).
- Renamed `20_DiscordManagement` → `DiscordManagement` at root.
- Rewired runtime paths: root bats, dispatch/engine/package/nexus/image tools, MakeNewMod, GenPurpleBookCompat, Discord configs, `.gitignore`.

## Still pending

- Light doc pass on older plan/progress wording (historical `80_` names in Progress Log are OK as history).
- MCP home decision; quarantine/`obj`/`bin` untrack confirmation.
- Commit the stacked uncommitted work when you're ready.

```powershell
cd c:\GitHub\7D2D-Mods
git status --porcelain
Get-ChildItem -Directory | Select-Object Name
Get-ChildItem -File | Select-Object Name
```
