# Nexus Mods Automation Capabilities

Last updated: 2026-07-25
Primary evidence:
- Local OpenAPI snapshot: `06_PublishingSupport/NexusMods/nexusAPI.txt` (openapi 3.0.3, info.version 3.0.0)
- Scripts under `06_PublishingSupport/NexusMods/Workflow/`

## Quick Yes/No Matrix

1. Create a brand new mod page?
- No. Manual required once, then add the Nexus ID to `nexusmods-config.json`.

2. Update an existing mod with a new file version?
- Yes (existing pages only — cannot create new mod pages).
- API: upload session → PUT → finalise → `POST /mod-files/{id}/versions`
- Wired: `SCRIPT-NexusUpdate.py` / `RUN-Nexus-Update.bat`
- Gate: requires existing `nexus_mod_id`, resolvable file group, zip in `04_DownloadZips`, and local version newer than Nexus

3. Append changelog entries for a version?
- Yes (Experimental).
- API: `POST /mods/{id}/changelogs` (additive only)
- Wired: after a successful file-version create in the upload pipeline

4. Upload or modify images?
- No confirmed endpoint in current local Nexus snapshot.
- Treat as manual.

5. Edit page-level text areas (short description / main body)?
- No confirmed write endpoint for mod page body/summary in current local snapshot.
- Treat as manual. File-version `description` is only the file note.

6. Read live mod/file/version state for checks and planning?
- Yes.
- Wired: `SCRIPT-AuditNexusMods.py` / `RUN-Nexus-Status.bat` writes `Nexus-Status.md` + `.json`
- Discovery: GraphQL author/name list first, then v1 `search=` fallback (old `name=` returns HTTP 422)
- Matching uses PublishHelp `Details.md` titles (`AGF - V3 - Category - Name`)
- Also: `--mode check-live` / `discover-groups`

## Day-to-day workflow (minimal Nexus site work)

1. Finish normal publish so `03_ReleaseSource` + `04_DownloadZips` are current.
2. Run `RUN-Nexus-Status.bat` → reviews Nexus and saves high-confidence IDs into config; open `Nexus-Status.md`.
3. For mods still listed as First Upload Needed: create the page once on the site, then re-run Status.
4. Update: `RUN-Nexus-Update.bat` (optional single-mod pilot arg). Prefer one mod first. Validates before live write.

Policy: automate file version + changelog. Leave page body/images alone unless you care enough to fix them manually.

## New-First Endpoint Policy

1. Prefer latest routes:
- `GET /mods/{id}/files`
- `GET /mod-files/{id}/versions`
- `POST /mod-files/{id}/versions`
- `POST /mods/{id}/changelogs`
- `POST /uploads` (+ finalise / poll)

2. Legacy fallback only for compatibility:
- `GET /mods/{id}/file-update-groups`
- `GET /file-update-groups/{id}/versions`
- v1: `/games/{game_domain}/mods/{id}/files.json` and search

## Available Write Fields (file/version)

Create mod file version (`POST /mod-files/{id}/versions`):
- `upload_id`, `name` (max 50, charset-limited), `version` (max 50), `description` (nullable)
- `file_category`: main | optional | miscellaneous
- `primary_mod_manager_download`, `allow_mod_manager_download`, `show_requirements_pop_up`
- `archive_existing_file`, `previous_version_id`, `update_mod_version`

Changelog append (`POST /mods/{id}/changelogs`):
- `version` + `changelog` (single string, 1–65535 chars). Additive; repeats append more text.

Upload session:
- `filename`, `size_bytes`

## Box-By-Box Practical Mapping

1. Short description / main page body / images
- Manual (no confirmed write endpoint).

2. Mod file notes
- Supported via file/version `description`.

3. Changelog section on the mod page
- Supported via `POST /mods/{id}/changelogs` (wired after upload).

4. Version number users see on the page
- Prefer `update_mod_version: true` when creating a new file version.

## Stability Note

Many v3 routes are tagged Experimental. Keep live writes behind dry-run validation and explicit confirm bats. Prefer single-mod pilot uploads before batch.
