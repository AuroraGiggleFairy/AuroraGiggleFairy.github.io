# Nexus Mods Automation Capabilities

Last updated: 2026-06-29
Primary evidence:
- Local OpenAPI snapshot: 06_PublishingSupport/NexusMods/nexusAPI.txt (openapi 3.0.3, info.version 3.0.0)
- Current script: 06_PublishingSupport/NexusMods/Workflow/SCRIPT-NexusMods.py

## Quick Yes/No Matrix

1. Create a brand new mod page?
- No. Manual required.

2. Upload mod file binary?
- Yes.
- API supports upload sessions and file creation/versioning.
- Current repo automation: planning/check flow exists, full execute-upload flow is not wired yet.

3. Upload or modify images?
- No confirmed endpoint in current local Nexus snapshot.
- Treat as manual for now.

4. Edit page-level text areas (short description/main body/changelog sections on page)?
- No confirmed endpoint in current local Nexus snapshot.
- Treat as manual for now.

5. Read live mod/file/version state for checks and planning?
- Yes.
- Current repo automation: Yes.

## New-First Endpoint Policy

1. The script should use latest routes first:
- GET /mods/{id}/files
- GET /mod-files/{id}/versions

2. Legacy fallback is only for compatibility:
- GET /mods/{id}/file-update-groups
- GET /file-update-groups/{id}/versions
- v1 chain fallback: /games/{game_domain}/mods/{id}/files.json

## Available Data Fields (What You Can Actually Send)

1. Upload session fields
- filename: string
- size_bytes: integer

2. Create mod file fields (POST /mod-files)
- upload_id: uuid string
- mod_id: string
- name: string, max 50, pattern ^[a-zA-Z0-9 _'().-]+$
- version: string, max 50, pattern ^[a-zA-Z0-9.-]+$
- description: string (nullable)
- file_category: main | optional | miscellaneous
- primary_mod_manager_download: boolean
- allow_mod_manager_download: boolean
- show_requirements_pop_up: boolean

3. Create mod file version fields (POST /mod-files/{id}/versions)
- upload_id: uuid string
- name: string, max 50, pattern ^[a-zA-Z0-9 _'().-]+$
- version: string, max 50, pattern ^[a-zA-Z0-9.-]+$
- description: string (nullable)
- file_category: main | optional | miscellaneous
- primary_mod_manager_download: boolean
- allow_mod_manager_download: boolean
- show_requirements_pop_up: boolean
- archive_existing_file: boolean
- previous_version_id: string (nullable)

## Box-By-Box Practical Mapping

1. Short description
- Page-level short description: manual (no confirmed write endpoint).
- Closest API field: file/version description (text string).

2. Main section (long body)
- Manual (no confirmed page-body write endpoint).

3. Changelog section
- Manual as a page section.
- Closest API field: file/version description (text string), if you choose to place release notes there.

4. Mod file notes
- Supported via description on file/version create endpoints.
- Format contract in schema: string. Markdown/HTML rendering behavior is not defined in schema.

5. Images (thumbnail/gallery/banner)
- Manual (no confirmed image upload/update endpoint in current local snapshot).

## Stability Note

Many v3 routes are tagged Experimental. Keep live writes behind explicit run modes and dry-run checks.
