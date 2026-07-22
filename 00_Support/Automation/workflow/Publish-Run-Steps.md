# Publish Run — Concrete Step Sequence

Last updated: 2026-04-30

---

## What a Publish Run Is

An intentional release. Always a manual decision — the script never publishes on its own. Run this when the work in `02_ActiveBuild` is ready to go out publicly.

**Scope:** Everything. Update run steps first, then promote → zip → README → GigglePack release artifacts → Discord.
Only `AGF-` / `zzzAGF-` prefixed mods are managed in workspace and game-folder sync phases.

**Trigger:**
```
python 00_Support/Automation/workflow/00_dispatch.py --mode full
# or the chain runner:
python 00_Support/Automation/workflow/04_run_chain.py
# optional preflight confidence check before publish:
RUN-PrePublish-Check.bat
```

---

## Step Sequence

### Step 1 — Run the full update run

All update-run steps from [Update-Run-Steps.md](Update-Run-Steps.md) run first, in order:
- Drift scan → Notepad++ check → resolve drift → re-scan → game sync → orphan cleanup → CSV reconcile → GigglePack pending changes update

If the update run produces any `ACTION NEEDED` warnings (e.g. `mod_loaded` refs updated in another mod), the publish run **pauses and surfaces them** before continuing. The user should decide whether to bump those mod versions before proceeding, since they will be packaged as-is.

> Currently this is informational only — the run continues. A future flag could make it blocking.

---

### Step 2 — Promote `02_ActiveBuild` → `03_ReleaseSource` (version-gated)

For each mod in `02_ActiveBuild`:
- If version `0.x.x`: skip — draft mods do not promote
- If version `1.x.x+` and not in `03_ReleaseSource`: copy into `03_ReleaseSource`
- If version `1.x.x+` and already in `03_ReleaseSource`:
  - Workspace version > release version → overwrite
  - Workspace version ≤ release version → skip (release is already at least as new)

`03_ReleaseSource` is a **full mirror** of the publishable subset of `02_ActiveBuild`. It does not contain anything that isn't in `02_ActiveBuild`.

After promotion, clean up `03_ReleaseSource`:
- Remove any folders whose base name is not present in `02_ActiveBuild` (they were deleted or renamed)
- Remove older versioned copies of the same mod (keep only latest)

---

### Step 3 — Normalize READMEs in `03_ReleaseSource`

Regenerate each mod's `README.md` from the template and CSV data. This is identical to what `prep-work` does but targeted at `03_ReleaseSource` specifically. Existing `FEATURES` and `CHANGELOG` blocks in each mod's README are preserved — only the generated sections are overwritten.

---

### Step 4 — Build all zips

Source: `03_ReleaseSource` (not `02_ActiveBuild`).

Zips produced in `04_DownloadZips/`:

| Type | Naming | Contents |
|---|---|---|
| Individual mod zips | `AGF-{Category}-{ModName}.zip` | One mod folder each |
| Category zips | `00_{Category}_All.zip` | All mods in that category |
| GigglePack canonical | `00_GigglePack_All.zip` | Root mods + all `.Optionals-{Category}` subfolders |
| GigglePack versioned | `AGF-GigglePack-v{X}.{Y}.{Z}.zip` | Same as canonical, named for this release |

Old versioned GigglePack zips from previous releases are **not** deleted automatically — they accumulate. Manual cleanup only.

---

### Step 5 — Compute GigglePack release version and generate artifacts

Compare current `03_ReleaseSource` GigglePack mod versions against `gigglepack-release-state.json` (last published state).

**Version bump rules:**

| Change type | Version bump |
|---|---|
| `gigglepack-major-bump.txt` marker present | Major: `X+1.0.0` — marker is consumed after write |
| New mods added to GigglePack | Minor: `X.Y+1.0` |
| Existing mods updated or renamed or removed | Patch: `X.Y.Z+1` |
| No changes | Keep same version — no artifacts written |

**Outputs written:**

| File | Contents |
|---|---|
| `05_GigglePackReleaseData/gigglepack-release-state.json` | New published state snapshot (mod versions, change counts, release version, timestamp) |
| `05_GigglePackReleaseData/gigglepack-release-history.md` | New release entry prepended at top; all previous entries preserved |
| `05_GigglePackReleaseData/Discord/discord-post.txt` | Rendered Discord post for this release |

**After a successful write:** delete `05_GigglePackReleaseData/gigglepack-pending-changes.json` — it is now stale. The next update run will recompute it fresh against the new published baseline.

---

### Step 6 — Rebuild main `README.md`

Regenerate `README.md` from `05_GigglePackReleaseData/ReadmeSystem/Templates/TEMPLATE-MainReadMe.md` and current `03_ReleaseSource` state.

Each category section follows: section header → Download All link → individually listed mods.

The GigglePack section pulls the **latest 3 entries** from `gigglepack-release-history.md` and renders them in a collapsible changelog block.

New categories detected in `03_ReleaseSource` (by `AGF-{Category}-{ModName}` naming) get a new section appended automatically.

---

### Step 7 — Discord post (conditional)

Only runs if Step 5 produced a new GigglePack release (i.e. `has_update = True`).

Reads `05_GigglePackReleaseData/Discord/discord-post.txt` and posts to the webhook configured in `AGF_DISCORD_WEBHOOK_URL` (or `--discord-webhook-url` CLI override).

If the webhook env var is not set: skip silently and log a notice.

Long messages are split into chunks to stay within Discord's 2000-character limit.

---

### Step 8 — End-of-run summary

```
=== Publish Run Complete ===

Promoted to ReleaseSource:  17 mods  (2 updated, 0 added, 0 removed)

Zips built:
  - Individual: 17
  - Category:    5
  - GigglePack:  2  (canonical + versioned v1.5.0)

GigglePack v1.4.4 → v1.5.0
  - New:     2 mods
  - Updated: 4 mods
  - Renamed: 0 mods
  - Removed: 0 mods

Main README.md: rebuilt
Discord post: sent  (or: skipped — AGF_DISCORD_WEBHOOK_URL not set)

Log file: 00_Support/Automation/Logs/main-script-*.log
```

---

## What Publish Does NOT Do

| Thing | Reason |
|---|---|
| Auto-decide readiness | Publish is always a manual trigger |
| Push to GitHub | Git operations stay manual |
| Upload to NexusMods | `06_PublishingSupport/NexusMods/Workflow/SCRIPT-NexusMods.py` is a separate manual step |
| Delete old versioned GigglePack zips | Manual cleanup only |
| Post Discord if no GigglePack change | No change = no post |

---

## Relationship to Individual Modes

The full publish run (`--mode full`) is the intended way to publish. The individual modes exist for targeted use:

| Mode | What it runs |
|---|---|
| `--mode sync-work` | Update run only (Steps 1–9 of update spec) |
| `--mode promote` | Drift check + promote `02_ActiveBuild` → `03_ReleaseSource` only |
| `--mode package` | READMEs + zips + GigglePack artifacts + main README only (assumes `03_ReleaseSource` is already current) |
| `--mode full` | Everything — the complete publish run |
