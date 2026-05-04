# AGF Mod Workflow — Goals and Requirements

*Working document for reviewing what this workflow needs to do, what's broken, and what's missing.*

---

## Overall Goal

Reduce manual effort to near-zero for keeping mod work synchronized, correctly organized, and published to public sites. The workflow should handle the messy parts automatically — version tracking, folder naming, file placement, pack updates, and publishing — so the only manual step required is the work itself (writing the mod).

---

## Workspace Layout

Three locations are in play at all times:

| Location | Role |
|----------|------|
| `7D2D-Mods/` workspace | Primary workspace: all lanes, helper files, scripts, DLL projects |
| Game Mods folder | Live testing: what the game actually loads |
| `_DLL-Projects/` subfolder | DLL mod source code, lives inside the workspace |

There is also a separate **new mod creation script** that bootstraps a new mod and wires it into some of the same helper files used by the main workflow. That means the main workflow should be compatible with mods created by that setup script and avoid duplicating or breaking its setup work.
The current expectation is that the new-mod creation script mainly **creates the initial entry/setup**, and the update workflow should then apply the normal ongoing synchronization, README generation, helper-file reconciliation, and other maintenance behavior.

---

## Core Requirements

### 1. Version-Driven Sync

- The **version number in `ModInfo.xml`** is the trigger for everything.
- When a version number changes anywhere (workspace lane or game folder), the script must detect it and act on it automatically the next time it runs.
- No manual intervention should be needed to trigger a sync — just run the script.

### 2. Lane Placement by Version Rules

| Version | Where it belongs |
|---------|-----------------|
| `0.x.x` (major = 0) | `01_Draft` — work in progress |
| `1.x.x+` (major ≥ 1) | `02_ActiveBuild` or `03_ReleaseSource` — tested/published |

- A `0.x.x` mod is **only pushed to the game folder if it was already there** — it does not get added to the game automatically.
- A `1.x.x+` mod gets synced to the game folder via `sync-work`.
- Old versioned folders (previous version) must be **removed** after the new version is placed.
- Both `01_Draft` and `02_ActiveBuild` should hold your latest work where applicable.
- Anything that remains in `01_Draft` is not intended to flow into `03_ReleaseSource`.

### 2a. ActiveBuild vs. ReleaseSource

- `01_Draft` is the latest in-progress lane for unfinished or pre-release work.
- `02_ActiveBuild` is the **current tested working set**.
- `03_ReleaseSource` is the **published/release mirror** of the versions you are actually putting out publicly.
- In practice, `03_ReleaseSource` should reflect the publishable state of mods that came from `02_ActiveBuild`, but it should be treated as the release lane, not just a casual duplicate.
- `03_ReleaseSource` and everything after it in the pipeline should only be touched by the **publish** workflow.
- Both `01_Draft` and `02_ActiveBuild` may contain your latest changes, but only `02_ActiveBuild` is a source for publish.
- `02_ActiveBuild` is the lane that should keep reflecting the latest state of your working, testable mods.
- Publish flow should move or refresh content from `02_ActiveBuild` into `03_ReleaseSource` as part of a deliberate release step.

### 3. Mod Name Changes

This is the most fragile part. When `Name` in `ModInfo.xml` changes:

- The **folder name** must be updated to match.
- The entry in **`HELPER_ModCompatibility.csv`** must be updated.
- The **quote file** in `_Quotes/` must be renamed to match.
- Any **README references** must reflect the new name.
- This must work whether the name change happens in the workspace lane OR in the game folder.
- Old folder names must be cleaned up after rename.
- During the stabilization period, renamed/replaced folders should preferably go to a temporary `BACKUP-Renames` area instead of being hard-deleted immediately. Once the rename flow is proven reliable, this can later become immediate deletion.

### 4. Run Modes — Update vs. Publish

There are two distinct intents when running the script:

| Intent | What should happen |
|--------|-------------------|
| **Update only** — I'm still working, not releasing | Sync Draft, ActiveBuild, and game folder state as needed; refresh README outputs and helper-file-driven content already in place; reconcile helper files; and keep GigglePack draft notes/state current. No promotion, no ReleaseSource work, no zipping, no public publish. |
| **Publish** — I'm ready to release | Run the update flow first, then promote to release lane, build zips, update GigglePack release, update main README, and optionally auto-post to Discord. |

Right now these are not cleanly separated enough.

Publish readiness is a **manual decision** you make. The workflow should support two explicit runs:

- **Update run**: maintain and synchronize current work without releasing it.
- **Publish run**: perform the update run first, then complete the release/publishing actions.

### 5. GigglePack Continuous Updates

**Current problem:** The GigglePack release artifacts have to be manually edited after running the script to consolidate everything, and the Discord post is a separate manual step after a git push.

**What it should do:**
- Keep a running "in-progress" GigglePack state that accumulates changes across multiple update runs.
- Track the latest changes made before publishing, including new mods, updated mods, renamed mods, and deleted mods.
- If you run the update flow multiple times before publishing, it should keep refining the same in-progress GigglePack change record rather than creating a new published release each time.
- Only finalize and version-stamp the GigglePack when explicitly told to publish.
- After a publish is completed, the next update cycle should begin tracking against that newly published baseline.
- After publish, the Discord post should be ready to send and eventually auto-post, but only after the GigglePack update logic is proven reliable.
- Git publishing should remain manual for now so the workflow can be verified safely before any automatic git actions are introduced.

### 5a. GigglePack State Model

- The workflow should maintain two distinct GigglePack concepts:
- **Draft/in-progress update state**: the running summary of everything changed since the last publish.
- **Published release state**: the last finalized GigglePack version that was actually released.
- Repeated update runs should modify the draft/in-progress state.
- Publish should consume that draft/in-progress state, produce the finalized release artifacts, and then establish the new published baseline.

### 6. Category, Zip, and README Rules

- Existing category naming rules and zip composition rules must be preserved.
- Categories are currently derived from the mod folder/name pattern: `AGF-{Category}-{ModName}`.
- That naming pattern is the current source of truth for determining a mod's category.
- Each published category gets its own `README.md` section header and its own category-level download zip.
- The current category zip naming scheme is `00_{Category}_All.zip`.
- Individual published mod zip files keep the mod-based naming pattern such as `AGF-{Category}-{ModName}.zip`.
- Publish must continue creating the expected per-mod zip files and category-level "download all" zip files.
- The rules for what goes into each zip, including root contents and optional subfolders, remain part of the workflow requirements and cannot be dropped during refactoring.
- The main `README.md` must continue reflecting categories and download links generated by the publish workflow.
- In the main `README.md`, every category section should follow the existing formula: category header, category-level "Download All" link, then the individually listed mods in that category.
- If a new category is introduced, publish should append a matching new section to the end of the existing category sections in the main `README.md`.
- If a new category is introduced, publish should also create that category's `00_{Category}_All.zip` automatically.
- GigglePack_All is the single combined AGF download with a ready-to-go server-side setup in root and other category content organized into optional folders.
- Existing GigglePack placement rules for current categories must remain unchanged.
- New categories should also be represented consistently inside the GigglePack structure so category-level content is not left out of the combined pack.
- The current optional-folder naming pattern inside combined packs is `.Optionals-{Category}` where applicable.
- Category handling should be driven by clear rules or metadata, not by one-off manual edits after each publish.

---

## What Is Working Well

- Zipping files and placing them in the correct output locations (`04_DownloadZips`).
- The general lane structure and folder organization.
- Per-mod README generation from template + CSV data.

---

## What Still Needs Work

| Area | Problem |
|------|---------|
| Version-triggered rename flow | Name changes in `ModInfo.xml` break multiple helper files |
| Old folder cleanup | Superseded versioned folders are not always removed |
| Rename safety | Need temporary backup handling while rename logic is still being proven |
| GigglePack accumulation | Changes must be manually consolidated before publishing |
| Category automation | New categories should create README sections, `00_{Category}_All.zip`, and GigglePack placement automatically |
| Outline drift | The old written outline can fall behind the real code and output rules |
| Discord post | Should eventually auto-post on publish, but only after GigglePack update flow is trustworthy |
| Git publish | Intentionally remains manual for now |
| Main README | Still has gaps; not fully auto-generated from current state |
| Image support | Not implemented; needed for future publishing |
| Multi-site publishing | Only GitHub now; Nexus Mods and others planned |

---

## Future / Planned Scope

- **Images:** Mod screenshots and banners as part of the release package.
- **Nexus Mods:** Automated or semi-automated publishing via `SCRIPT-NexusMods.py`.
- **Other sites:** Additional publishing targets beyond GitHub and Nexus.
- **Git integration:** Possible later, but intentionally deferred until the workflow is proven stable.

---

## Decisions Captured So Far

1. `0.x.x` mods are only pushed to the game folder if they were already present there.
2. Renamed/replaced folders should go to a temporary backup area first while the rename flow is being validated.
3. Discord posting should eventually be auto-post on publish, but only after GigglePack update behavior is dependable.
4. Git publish stays manual for now.
5. Publish readiness is a manual choice, so the workflow should support separate `update` and `publish` runs.
6. `03_ReleaseSource` and later packaging steps belong to publish, not update.
7. Repeated update runs should keep revising the same in-progress GigglePack change set until publish happens.
8. Existing category naming and zip composition rules must stay intact.
9. New categories should automatically produce a README section, a category-level download zip, and consistent GigglePack placement.
10. Category detection currently comes from the mod naming convention `AGF-{Category}-{ModName}`.
11. The category-level zip naming scheme is `00_{Category}_All.zip`.
12. Category sections in the main `README.md` follow the existing pattern: section header, category-level download-all link, then individually listed mods.
13. GigglePack uses a root-plus-optionals layout, including `.Optionals-{Category}` folders where applicable.
14. New categories going forward are expected to be neither EAC-friendly nor server-side, so they default to `.Optionals-{Category}` placement inside GigglePack rather than root.
15. This default placement rule should be overridable per category in the future if a new category turns out to be server-ready.

## Remaining Questions

1. What exact files should store the GigglePack draft/in-progress state versus the finalized published state?
2. Should `03_ReleaseSource` always be force-mirrored from `02_ActiveBuild` during publish, or should some files stay release-only?
3. Which helper-file fields does the new-mod creation script prefill today, so the main workflow can avoid overwriting intentional starter values?
---
