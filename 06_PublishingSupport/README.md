# 06_PublishingSupport

Publishing tooling and per-site data, grouped by destination site. Each site folder holds its own script(s), config, plans, and capability notes side by side, so a site's automation is self-contained in one place.

```text
06_PublishingSupport/
|-- NexusMods/                       Nexus Mods scripts, config, plans, templates, PublishHelp output
|-- ModNetwork/                      The Mod Network scripts, config, plans, field mapping notes
|-- 7DaysToDieMods/                  placeholder - no automation exists for this site yet
`-- Site-Automation-Capabilities.md  cross-site overview of what's automated vs. manual
```

## NexusMods

- `SCRIPT-NexusMods.py` - planning/audit/bbcode-generation modes (see its `--help`).
- `SCRIPT-NexusUpload.py` - standalone upload wrapper, run independently and manually.
- `SCRIPT-AuditNexusMods.py` - read-only version check against live Nexus data (also runnable via `RUN-Nexus-Version-Check.bat`).
- `SCRIPT-NexusPublishHelp.py` - generates local copy/paste publish packets (used by `Workflow/06_nexus.py`).
- `nexus-api-key.private.txt` and `PublishHelp/` are gitignored (secrets / generated output) - see root `.gitignore`.
- `PublishHelp/<mod>/` only ever contains `Details.md`, `FullDesc.md`, and the release zip - images are intentionally **not** copied in here. Upload images to Nexus directly from `00_Images/02_ImagesFinal/`.

## ModNetwork

- `SCRIPT-ModNetwork.py` - init-config / build-plan / check-live / prepare-upload / update-page modes (see its `--help`).
- `FieldMapping.txt` documents where each published field is sourced from.

## 7DaysToDieMods

Empty placeholder. No config, script, or docs exist for this site yet - create them here when that automation is built.

---

Related root-level launchers that call into this folder: `RUN-Publish.bat` (via `Workflow/06_nexus.py`). See `WORKSPACE-ORGANIZATION-PLAN.md` Progress Log for how this folder was created.
