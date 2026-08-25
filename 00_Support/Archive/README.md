# Archive (under 00_Support)

Known inactive material retained for reference. Distinct from `00_Support/Quarantine` (temporary automated safety-holding data) - everything here was moved in deliberately and is meant to stay.

Every archived item should have an entry below stating: original path, archived date, last known purpose, its replacement (if any), and whether it's safe to delete later.

## `old-game-versions/_x2.6`

- **Original path:** `_x2.6/` (repo root)
- **Archived:** 2026-07-21
- **Last known purpose:** full snapshot of mod source from a previous 7 Days To Die game version (~80 mod folders plus a `.Optionals-*`/`_xObsolete` subset), kept as a read-only diff/reference baseline when porting old mod logic forward to the current game version.
- **Replacement:** none - superseded in practice by the live `01_Draft`/`02_ActiveBuild`/`03_ReleaseSource` pipeline, but still actively consulted as historical reference (see e.g. `00_DLL-Projects/Projects/DLL_NoEAC-AudioOptionsPlus/WORKFLOW-AudioOptionsPlus-UI.md`).
- **Safe to delete later:** no - still live-referenced by `SCRIPT-TransferChangelogs.py`, `SCRIPT-FixDraftChangelogs.py`, and `SCRIPT-RestoreAndFix.py` (all updated to the new path on the move), plus manual dev-workflow references. Treat as permanent reference material, not a deletion candidate.

## `backups/_BACKUP-PurpleBookGenerator-20260527-184538`

- **Original path:** `_BACKUP-PurpleBookGenerator-20260527-184538/` (repo root)
- **Archived:** 2026-07-21
- **Last known purpose:** dated snapshot backup of the PurpleBookGenerator mod (`Config/`, `Generator/`, `ModInfo.xml`, README files) taken 2026-05-27.
- **Replacement:** the live generator at `00_DLL-Projects/Generators/AGF-PurpleBookGenerator-v0.0.1/`.
- **Safe to delete later:** likely yes, once it's confirmed the live generator has fully superseded it - it was not referenced by any script at time of archival. Kept for now rather than deleted immediately (archive first, delete later).

## `notes/ModReadmeStuff`

- **Original path:** `Workflow/ModReadmeStuff/` (repo root)
- **Archived:** 2026-07-21
- **Last known purpose:** a June 17 discussion/notes file (`Notes.md`) proposing a reordered table of contents for the per-mod README template, plus two example output files (`Examples/AGF-HUDPlus-1Main_Example.md`/`.txt`).
- **Replacement:** superseded by the current, actively maintained `05_GigglePackReleaseData/ReadmeSystem/Templates/TEMPLATE-ModReadMes.md` and its change history in `05_GigglePackReleaseData/ReadmeSystem/WORKFLOW-ReadmeSystem.md` - the proposed reordering was never adopted into the live template.
- **Safe to delete later:** likely yes - it's a stale planning discussion, not referenced by any script. Kept for now rather than deleted immediately (archive first, delete later).

## `AGF-VP-DecorationBlock-v3.0.3-ExcelBaseline-20260822`

- **Original path:** `01_Draft/AGF-VP-DecorationBlock-v3.0.3/`
- **Archived:** 2026-08-22
- **Last known purpose:** snapshot of the last Excel-made Decoration Block pack (v3.0.3) before the v3.1 generator rewrite. Use this when you need the old helper order, clone XML, or localization.
- **Replacement:** live work stays in `01_Draft/AGF-VP-DecorationBlock-v3.0.3/`. Generator work stays in `00_DLL-Projects/Generators/DecorationBlock/`. An older game-version copy also exists at `old-game-versions/_x2.6/AGF-VP-DecorationBlock-v3.0.3/` (Localization.txt / Config/XUi, not this Draft snapshot).
- **Safe to delete later:** no — this is the Excel baseline for the rewrite.

## `Alter-Autominers`

- **Original path:** Nexus Autominers 4324 v1.3 by Alter (`Downloads\...\Alter_Autominers`), plus live test folder `Mods\Alter_Autominers-7d2dv3` (+ zip)
- **Archived:** 2026-07-23
- **Last known purpose:** consolidated third-party Autominers work (not an AGF mod). Contains:
  - `Alter_Autominers-7d2dv3/` — last live-tested package moved out of the game Mods folder
  - `Alter_Autominers-7d2dv3.zip` — zip from the live Mods folder
  - `v1.3-original/` — pristine Nexus v1.3 baseline
  - `v1.4-updated/` — port package + `Source/` used for the rebuild
  - `Decompiled-DLL/` — decompiled original `Autominers.dll` sources (moved from `WorkspaceData/References/Decompiled-DLLs/Autominers`)
- **Replacement:** none — external/community request only; do **not** promote into `01_Draft` / `02_ActiveBuild` / `00_DLL-Projects`.
- **Safe to delete later:** yes, once no longer needed locally.

---

See `WORKSPACE-ORGANIZATION-PLAN.md` / handoff for domain intent. Current path: `00_Support/Archive/` (formerly `90_Archive`).
