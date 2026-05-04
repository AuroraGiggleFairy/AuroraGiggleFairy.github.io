# Update Run — Concrete Step Sequence

Last updated: 2026-04-30

---

## What an Update Run Is

The day-to-day operation. Run this any time work has been done on a mod — after bumping a version, renaming, editing XML, or just to pull changes back from the game folder. It keeps everything in sync and tracks what has changed since the last publish.

**Scope:** `01_Draft`, `02_ActiveBuild`, and the game folder only.
Only `AGF-` / `zzzAGF-` prefixed mods are managed by this workflow.

**Does NOT touch:** `03_ReleaseSource`, `04_DownloadZips`, zips, main README, Discord.

**Idempotent:** Running it multiple times is safe and expected. Each run refines the same in-progress state rather than creating new versioned artifacts.

---

## Trigger

```
python SCRIPT-Main.py --mode update
# or double-click:
RUN-Update.bat

# safety check harness:
python SCRIPT-Main.py --mode self-test
# one-click pre-publish confidence check:
RUN-PrePublish-Check.bat
```

Default safety behavior:
- `--transaction-rollback` enabled
- `--quarantine-retention-days 7`
- `--enforce-agf-csv` enabled (non-AGF CSV rows fail preflight)
- `--preflight-write-check` enabled

---

## Step Sequence

### Step 1 — Quick drift scan (read-only)

Scan every mod folder in `01_Draft` and `02_ActiveBuild`. For each folder:
- Read `ModInfo.xml` → get `Name` and `Version`
- Compare folder base name against `ModInfo.Name`
- Compare folder version suffix against `ModInfo.Version`
- Classify as: clean, version-drift, name-drift, or combined-drift

Collect the full drift list before touching anything.

If **no drift found anywhere**: skip Steps 2–4 entirely for drift handling. Game-sync pre-flight still applies before any game-folder write/pull/push.

---

### Step 2 — Notepad++ pre-flight (drift operations)

Check if `notepad++.exe` is currently running.

If it is:
```
Notepad++ is currently open. It may be locking files that need to be updated.
Please close Notepad++ and press Enter to continue, or type 'skip' to abort.
```
- Enter → proceed
- `skip` → abort run cleanly

Version-only drift does require this check.

Separate from drift handling: before any game-folder sync/push/pull step, the same pre-flight check runs again.

On live runs, game-folder removals are quarantined to `_Quarantine-GameRemovals/` instead of being hard-deleted.

---

### Step 3 — Resolve drift

Process each drifted mod in order. For each:

**Version-only drift:**
1. Check collision guard (target folder name must not already exist in the same lane)
2. Rename folder: `{base}-v{old}` → `{base}-v{new}`
3. Log: `Renamed folder: AGF-VP-FooBar-v1.2.0 → AGF-VP-FooBar-v1.3.0`

**Name + version drift (combined):**
1. Check collision guard
2. Rename folder to `{new_name}-v{new_ver}`
3. Update `HELPER_ModCompatibility.csv`: rename `MOD_NAME` key old → new
4. Rename `_Quotes/{old_name}.txt` → `_Quotes/{new_name}.txt` (if exists)
5. Scan all `*.xml` in `01_Draft/**` and `02_ActiveBuild/**` for `mod_loaded('old_name')` and replace with `mod_loaded('new_name')`. Record which files were updated and how many occurrences.
6. Record rename event in the pending changes file (see Step 6)

**Name-only drift (no version bump):**
1. Skip rename
2. Warn: name changes are only applied when version also changes

After all drift is resolved, collect **side-effect notifications**: any mod other than the renamed one whose XML files were updated. These will be surfaced in the final summary.

---

### Step 4 — Re-scan workspace lanes

Scan `01_Draft` and `02_ActiveBuild` again with all names now stable. Build the definitive map of:
- `base_name → (lane, folder_path, version)`

This is the working roster for all remaining steps.

---

### Step 5 — Game sync

Using the stable roster from Step 4, sync against the game mods folder. This follows the existing sync logic:

| Condition | Action |
|---|---|
| Workspace version > game version | Push workspace copy to game, remove old game version |
| Game version > workspace version | Pull game copy to workspace lane, remove from game |
| Same version, content differs | Push workspace to game (workspace is source of truth) |
| Same version, same content | No action |
| Workspace is `0.x.x`, mod not in game | Skip — do not push |
| Workspace is `0.x.x`, mod already in game | Sync normally |
| Workspace is `1.x.x+`, mod not in game | Push to game |
| 4Modders mod | Handled separately via `.Optionals-4Modders/` folder, not game root |

Log every action taken.

---

### Step 6 — Orphan cleanup

After sync, check the game folder for orphans: folders whose base name is absent from the stable workspace roster.

| Condition | Action |
|---|---|
| Orphan base name is in `HELPER_ModCompatibility.csv` | Remove from game. Log: `Removed orphan from game: AGF-VP-FooBar-v1.2.0 (known managed mod)` |
| Orphan base name not in CSV | Warn only. Log: `Unknown mod in game folder (not managed): SomeExternalMod-v1.0.0` |

---

### Step 7 — CSV reconcile

Update `HELPER_ModCompatibility.csv` to reflect the current workspace state:
- Add a new row for any mod in `01_Draft` or `02_ActiveBuild` that has no CSV entry
- Update the `VERSION` field for all known mods to match current `ModInfo.xml`
- Do not remove rows for mods that were deleted (removals stay for publish-time changelog tracking)

Preserve all other fields in existing rows (EAC_FRIENDLY, SERVER_SIDE, etc.).

---

### Step 8 — GigglePack pending changes update

**File:** `05_GigglePackReleaseData/gigglepack-pending-changes.json`

**Behavior — each update run:**
1. Load `gigglepack-release-state.json` (last published baseline)
2. Compare current `02_ActiveBuild` mod versions against that baseline
3. Recompute the full pending change set: new mods, updated mods, renamed mods, removed mods
4. Overwrite `gigglepack-pending-changes.json` with the current computed state

**Key behavior:** This file is always a **current full diff** between last published state and now. Running multiple times doesn't accumulate duplicates — it recomputes from the stable published baseline each time. A rename detected in Step 3 is reflected here automatically because the mod's old name is gone from the workspace and the new name is present.

**Format:**
```json
{
  "computed_at": "2026-04-30T21:00:00",
  "since_gigglepack_version": "1.5.2",
  "new_mods": [["AGF-NoEAC-NewThing", "1.0.0"]],
  "updated_mods": [["AGF-HUDPlus-1Main", "5.4.3", "5.4.4"]],
  "renamed_mods": [["AGF-VP-FooBar", "AGF-VP-FooBaz", "1.2.0"]],
  "removed_mods": []
}
```

---

### Step 9 — End-of-run summary

Print a human-readable summary. Any `ACTION NEEDED` items appear first.

```
=== Update Run Complete ===

ACTION NEEDED:
  - 'AGF-Special-Compatibilities' had mod_loaded refs updated due to rename of 'AGF-VP-FooBar'.
    → Bump its version in ModInfo.xml and review its README.
  - 'AGF-HUDPlus-1Main' had mod_loaded refs updated.
    → Bump its version in ModInfo.xml and review its README.

Renames resolved:
  - AGF-VP-FooBar-v1.2.0 → AGF-VP-FooBaz-v1.2.0
    CSV key updated, quote file renamed, mod_loaded refs updated in 2 files

Game sync:
  - Pushed to game:    3 mods
  - Pulled from game:  0 mods
  - Removed orphans:   1 mod  (AGF-VP-FooBar-v1.2.0 — was renamed)
  - No change:         14 mods

CSV:
  - 1 new row added
  - 4 version fields updated

Pending GigglePack changes since v1.5.2:
  - New:     1 mod
  - Updated: 3 mods
  - Renamed: 1 mod
  - Removed: 0 mods

No publish actions taken. Run with --mode package to build a release.
```

If nothing changed at all:
```

Each run also writes a machine-readable manifest at `Logs/run-manifest-*.json`.
=== Update Run Complete — Nothing to do ===
All 17 mods are already in sync. No drift detected.
```

---

## What This Run Does NOT Do

| Thing | Why not here |
|---|---|
| Promote to `03_ReleaseSource` | Publish-only step |
| Rebuild main `README.md` | Publish-only step |
| Build category zips or GigglePack zip | Publish-only step |
| Post to Discord | Publish-only step |
| Finalize GigglePack version number | Publish-only step — version is computed at publish time from the pending changes |
