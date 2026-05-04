# Rename & Version-Change Scenario Matrix

Last updated: 2026-04-30

---

## Authoring Contract

The user **only ever changes `ModInfo.xml`** — never renames folders manually.
The script treats `ModInfo.xml` `<Name>` and `<Version>` as the sole source of truth and derives the correct folder name from them.

This means:
- `folder base name ≠ ModInfo.Name` → the script renames the folder to match ModInfo
- `folder version suffix ≠ ModInfo.Version` → the script renames the folder to match ModInfo
- Name changes are only applied when version also changes
- "Reversed drift" (folder renamed manually, ModInfo not updated) **cannot occur** in this workflow

---

## Identity Model

| Concept | Value | Source |
|---|---|---|
| Identity key (stable) | `AGF-VP-FooBar` | `ModInfo.xml` `<Name value="..."/>` |
| Version | `1.2.0` | `ModInfo.xml` `<Version value="..."/>` |
| Canonical folder name | `AGF-VP-FooBar-v1.2.0` | `{Name}-v{Version}` |
| Old identity (pre-rename) | `AGF-VP-FooBar` | Derived from current folder name before resolution |

**The identity key is ModInfo.Name — not the folder name.** The folder name is just a cached reflection of it.

---

## Pre-Flight Check: Notepad++ Open

Before any version-bump rename or name-change file-update operation, the script checks whether Notepad++ has any of the affected files open. The same check also runs before any game-folder sync/push/pull operation. Notepad++ holds write locks that silently prevent saves on Windows.

**Detection:** Check if `notepad++.exe` is running. If it is, prompt:
```
Notepad++ is currently open. It may be locking files that need to be updated.
Please close Notepad++ and press Enter to continue, or type 'skip' to abort.
```

- If the user closes it and presses Enter → proceed
- If the user types `skip` → abort with a clear message

For drift handling, this check only runs when relevant drift is detected.
For game-folder sync/push/pull phases, it runs before each game-touching phase.

---

## Drift Types

**Name drift** — folder base name ≠ `ModInfo.Name`

```
Folder: AGF-VP-FooBar-v1.2.0
ModInfo: <Name value="AGF-VP-FooBaz"/>    ← name changed in ModInfo.xml
```

**Version drift** — folder version suffix ≠ `ModInfo.Version`

```
Folder: AGF-VP-FooBar-v1.2.0
ModInfo: <Version value="1.3.0"/>         ← version bumped in ModInfo.xml
```

**Combined drift** — both at once (rename + version bump in same ModInfo edit)

---

## Drift Resolution Rules

Run at the **start** of every update run, before game sync.

| Drift type | Folder rename | CSV | Quote file | mod_loaded refs |
|---|---|---|---|---|
| Version only | `{base}-v{old}` → `{base}-v{new}` | No | No | No |
| Name only | Skip + warn (requires version bump) | No | No | No |
| Combined | `{old_name}-v{old_ver}` → `{new_name}-v{new_ver}` | Rename key | Rename file | Yes |
| Clean | No action | — | — | — |

---

## mod_loaded Reference Updates

When a mod's name changes (`old_name` → `new_name`), every XML file in the workspace that contains `mod_loaded('old_name')` must be updated.

**Scan scope:** All `*.xml` files in:
- `01_Draft/**`
- `02_ActiveBuild/**`

`03_ReleaseSource` is excluded — it is overwritten in full when publish mirrors from `02_ActiveBuild`, so updating it here would be wasted work.
The game folder is also excluded — it is regenerated from workspace sources.

**Pattern to replace (exact string):** `mod_loaded('AGF-VP-FooBar')` → `mod_loaded('AGF-VP-FooBaz')`

**Reporting:** Log each file updated:
```
Updated mod_loaded ref in: 02_ActiveBuild/zzzAGF-Special-Compatibilities-v4.1.0/Config/XUi/windows.xml (3 occurrences)
```

**Side-effect notification:** If a mod *other than the renamed one* has files updated by this scan, it means that mod's content has changed without a version bump. After completing all renames, print a warning for each such mod:
```
ACTION NEEDED: 'AGF-Special-Compatibilities' had mod_loaded references updated.
  → Its ModInfo.xml version should be bumped and its README reviewed.
```
These are not blocking — the run continues — but they must be visible and not buried in a log.

---

## Scenario Matrix

### S1 — Clean state, no drift, mod in game

```
Workspace folder:  AGF-VP-FooBar-v1.2.0  (Name=FooBar, Version=1.2.0)  ← clean
Game:              AGF-VP-FooBar-v1.1.0
```

| Step | Action |
|---|---|
| Drift check | Clean — no rename needed |
| Game sync | Workspace newer → push `v1.2.0` to game, remove `v1.1.0` from game |

---

### S2 — Version drift only

```
Workspace folder:  AGF-VP-FooBar-v1.2.0  (Name=FooBar, Version=1.3.0)
Game:              AGF-VP-FooBar-v1.2.0
```

| Step | Action |
|---|---|
| Drift check | Version drift. Rename folder: `FooBar-v1.2.0` → `FooBar-v1.3.0` |
| Notepad++ check | Prompt if running (version bump is being applied) |
| Game sync | Workspace `v1.3.0` > game `v1.2.0` → push new, remove old from game |

---

### S3 — Name drift only (mod renamed in ModInfo, same version)

```
Workspace folder:  AGF-VP-FooBar-v1.2.0  (Name=FooBaz, Version=1.2.0)
Game:              AGF-VP-FooBar-v1.2.0
```

| Step | Action |
|---|---|
| Drift check | Name drift detected |
| Rename | Skipped. Warning emitted: version bump required for name change |
| Game sync | No rename-based sync action until version is bumped |

---

### S4 — Combined drift (rename + version bump)

```
Workspace folder:  AGF-VP-FooBar-v1.2.0  (Name=FooBaz, Version=1.3.0)
Game:              AGF-VP-FooBar-v1.2.0
```

| Step | Action |
|---|---|
| Drift check | Combined drift detected |
| Notepad++ check | Prompt if running |
| Folder | Rename folder to `FooBaz-v1.3.0` |
| CSV / Quote / mod_loaded | Same as S3 |
| Game sync | Push `FooBaz-v1.3.0`. Orphan `FooBar-v1.2.0` in game → remove (it's in CSV) |

---

### S5 — 0.x.x mod, not in game (new draft)

```
Workspace folder:  AGF-VP-FooBar-v0.1.0  (Name=FooBar, Version=0.1.0)  ← clean
Game:              (not present)
```

| Step | Action |
|---|---|
| Drift check | Clean |
| Game sync | 0.x.x, not in game → **skip push** |

---

### S6 — 0.x.x mod, already in game (manually placed for testing)

```
Workspace folder:  AGF-VP-FooBar-v0.2.0  (Name=FooBar, Version=0.2.0)  ← clean
Game:              AGF-VP-FooBar-v0.1.0
```

| Step | Action |
|---|---|
| Drift check | Clean |
| Game sync | 0.x.x, **already present in game** → sync normally (push newer, remove older) |

The "don't push 0.x.x" rule only blocks initial placement. Once a mod is in game, it stays synced.

---

### S7 — New mod, 1.x.x, not yet in game

```
Workspace folder:  AGF-VP-FooBar-v1.0.0  (Name=FooBar, Version=1.0.0)  ← clean
Game:              (not present)
```

| Step | Action |
|---|---|
| Drift check | Clean |
| Game sync | 1.x.x, not in game → push to game |
| CSV | Not in CSV → add new row |

---

### S8 — Version drift + game has old version

```
Workspace folder:  AGF-VP-FooBar-v1.2.0  (Name=FooBar, Version=1.4.0)
Game:              AGF-VP-FooBar-v1.2.0
```

| Step | Action |
|---|---|
| Drift check | Version drift. Rename folder to `FooBar-v1.4.0` |
| Game sync | Workspace `v1.4.0` > game `v1.2.0` → push new, remove old from game |

---

### S9 — Draft graduation: 0.x.x → 1.x.x

```
Workspace folder:  AGF-VP-FooBar-v0.9.0  (Name=FooBar, Version=1.0.0)
Game:              (not present)
```

| Step | Action |
|---|---|
| Drift check | Version drift. Rename folder to `FooBar-v1.0.0` |
| Game sync | Now 1.x.x, not in game → push to game |

This is the natural graduation path from 01_Draft to active.

---

### S10 — Mod deleted from workspace, still in game

```
Workspace:  (not present)
Game:       AGF-VP-FooBar-v1.2.0
```

| Step | Action |
|---|---|
| Orphan check | `AGF-VP-FooBar` in CSV → remove from game |
| Orphan check | `AGF-VP-FooBar` NOT in CSV → warn only, do not touch |

---

## Orphan Rules (Game Folder Cleanup)

An "orphan" is a game folder whose base name is not present in the current workspace (01_Draft + 02_ActiveBuild combined, after all drift resolution).

| Condition | Action |
|---|---|
| Orphan base name IS in workspace CSV (`HELPER_ModCompatibility.csv`) | **Remove from game.** It was a known workspace mod; it was either renamed or deleted. |
| Orphan base name is NOT in workspace CSV | **Warn only. Do not touch.** It's an externally placed mod not managed by this workflow. |

The CSV is the roster of "mods this workflow manages." Anything in game whose base name is in the CSV but missing from workspace lanes is our responsibility to clean up.

---

## Full Artifact Update Checklist (on name change)

| Artifact | Change |
|---|---|
| Workspace folder | `{old_name}-v{ver}` → `{new_name}-v{ver}` |
| `HELPER_ModCompatibility.csv` | `MOD_NAME` key: old → new |
| `_Quotes/{old_name}.txt` | Renamed to `{new_name}.txt` |
| All `*.xml` in 01_Draft, 02_ActiveBuild | `mod_loaded('old_name')` → `mod_loaded('new_name')` (03_ReleaseSource excluded — overwritten on publish) |
| `05_GigglePackReleaseData/` change log | Rename event: `old_name → new_name` |
| `README.md` + category zips | Regenerated from workspace state — no manual keying |

---

## Version Folder Collision Guard

Before renaming a folder to `{name}-v{new_version}`, check that the target folder name doesn't already exist in the same lane. If it does:
- Log a conflict: "Cannot rename `FooBar-v1.2.0` → `FooBar-v1.3.0`: target already exists."
- Skip the rename. Don't overwrite.

---

## Order of Operations Within an Update Run

```
1.  Quick-scan all mod folders in 01_Draft + 02_ActiveBuild to detect any drift
2.  If any drift found that includes a version bump: check if Notepad++ is running → prompt
3.  For each drifted folder (in any order):
    a. Read ModInfo.xml → get canonical Name and Version
    b. If version-only drift: rename folder only
    c. If name (or combined) drift:
       - Check collision guard (target folder name must not already exist)
       - Rename folder to {new_name}-v{new_ver}
       - Update CSV key old→new
       - Rename quote file old→new
       - Scan and replace mod_loaded refs in all workspace XMLs
       - Record rename event in GigglePack change log
4.  Re-scan workspace lanes (all names now stable)
5.  Before each game sync/push/pull phase: check if Notepad++ is running → prompt
6.  Run game sync against resolved names
7.  Run orphan cleanup using CSV roster
8.  CSV reconcile (add new mod rows, update version fields for all known mods)
```

Steps 1–3 complete in full before step 4. Multiple renames in one run are processed independently before the re-scan.
