# 90_Archive

Known inactive material retained for reference. Distinct from `91_Quarantine` (temporary automated safety-holding data) - everything here was moved in deliberately and is meant to stay.

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

---

See `WORKSPACE-ORGANIZATION-PLAN.md` Section 9 (`90_Archive`) for the full domain definition, and its Progress Log for how this folder was created.
