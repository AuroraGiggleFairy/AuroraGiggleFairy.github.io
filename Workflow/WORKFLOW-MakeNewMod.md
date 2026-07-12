# WORKFLOW - MakeNewMod

## Working methods
- Keep new-mod scaffolding in `SCRIPT-MakeNewMod.py` aligned with the current README migration pipeline.
- Seed `README.md` with `FEATURES-SUMMARY`, `FEATURES-DETAILED`, and `CHANGELOG` marker blocks so publish extraction can preserve content.
- Default `MOD_TYPE_ID` to `TBD` in compatibility CSV rows for newly created mods.

## Change history
- 2026-07-05: Replaced stale README template/snippet wiring with a direct modern README scaffold in `SCRIPT-MakeNewMod.py`.
- 2026-07-05: Added `Other Details` section seed line `- Add or remove as needed.` inside `FEATURES-DETAILED` markers.
- 2026-07-05: Updated new row default for `MOD_TYPE_ID` from `0` to `TBD`.
- 2026-07-05: Executed non-dry `migrate-readmes-once` run in `Workflow/05_pipeline_engine.py`; summary stayed clean (`readmes_written=91`, `warnings=0`, `errors=0`) and migration pre/post reports were written under `Workflow/ReadmeSystem/temp`.

## Do-not-do notes
- Do not use obsolete snippet file names (for example `ABOUTME-Guide.md`) in this script.
- Do not remove marker blocks from scaffolded `README.md`; extraction logic depends on them.
- Do not default `MOD_TYPE_ID` back to `0`; use `TBD`.
