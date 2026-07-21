# ModNetwork Automation Capabilities

Last updated: 2026-06-29
Primary evidence:
- Local notes: 06_PublishingSupport/ModNetwork/Api Informaiton.txt
- Local mapping guide: 06_PublishingSupport/ModNetwork/Workflow/FieldMapping.txt
- Current script: 06_PublishingSupport/ModNetwork/Workflow/SCRIPT-ModNetwork.py

## Capability Matrix

1. Create a new mod page + first release
- API support: Yes.
- Endpoint: POST /publish without slug.
- Current automation: Yes (SCRIPT-ModNetwork.py execute-upload).

2. Upload a new release for existing mod
- API support: Yes.
- Endpoint: POST /publish with slug.
- Current automation: Yes (SCRIPT-ModNetwork.py execute-upload).

3. Update page metadata/text boxes on existing page
- API support: Yes (creator endpoint).
- Endpoint used by script: PATCH /api/creator-mods/{mod_id}.
- Current automation: Yes (SCRIPT-ModNetwork.py update-page).

4. Upload/update thumbnail and screenshots
- API support: Unknown from current docs.
- Current automation: No (config keys exist but publish request currently sends only zip file + form fields).
- Practical result: Manual currently.

5. Read live mod and release state
- API support: Yes.
- Endpoints in notes: GET /mod/{slug}, GET /mod/{slug}/versions, GET /search.
- Current automation: Yes (check-live + planning comparisons).

## Text Boxes And Accepted Content (Current Workflow)

1. Publish endpoint fields used in automation
- title (new mods)
- slug (updates)
- version
- description (short text)
- long_description (script sends HTML derived from README)
- changelog (text extracted from README)
- game_id
- game_version
- mod_side
- requirements
- release_type
- file (zip upload)

2. Update-page endpoint fields used in automation
- long_description (HTML)
- install_guide (HTML)
- description
- version
- game_version
- mod_side
- requirements

3. Dedicated areas represented by automation
- Main page body: long_description
- How To Install tab: install_guide
- Release notes/history area: changelog through publish release

## Known Gaps / Cleanup

1. MOD_TYPE_ID mapping comment in SCRIPT-ModNetwork.py is from older taxonomy notes and should be refreshed to your final type semantics.
2. tmn_thumbnail_path and tmn_screenshot_paths are present in config/plan but not attached in tmn_publish_request yet.
3. page_url has at least one bad value in config (example uses /mods/undefined), so add slug/url validation before live runs.

## Suggested Next Implementation Order

1. Add optional image upload fields only after confirming exact accepted form names from TMN.
2. Add a preflight validator mode for:
- missing tmn_game_id
- invalid/missing tmn_slug for update intent
- invalid page_url format
3. Write execution receipts to 06_PublishingSupport/ModNetwork/Workflow/modnetwork-upload-results.json.
