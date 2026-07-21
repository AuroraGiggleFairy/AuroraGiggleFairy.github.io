# Workspace Organization Plan

> **Status:** In progress - see Progress Log below for what's actually been done  
> **Created:** July 19, 2026  
> **Scope:** Organize the `7D2D-Mods` workspace without moving, deleting, or breaking anything yet.

---

## Progress Log

Read this section first in any new session to know what's real vs. still just proposed below.

### Completed

- **`00_Images`**: reorganized into its numbered-stage pattern (source vs. generated vs. assets split). Not fully finished - still missing a root `README.md`, has a stale `LEGACY-IMAGES-README.txt`, and `media-status.csv` has drifted from disk state.
- **`00_DLL-Projects` cleanup (2026-07-20/21)**:
  - Deleted the stale `Support/ESCWindowPlus/...-backup-20260701-111344/` snapshot entirely (confirmed duplicate of the live generator, plus ~110 leftover `__pycache__` files).
  - Moved `Support/DecorationBlockPlusFiles/*` -> `Generators/DecorationBlockPlus/` (31 files, real generator tooling for the DecorationBlockPlus mod).
  - Moved the live ESCWindowPlus generator out of `01_Draft/AGF-4Modders-ESCWindowPlus-v0.3.2/_Generator/` into `00_DLL-Projects/Generators/ESCWindowPlus/_Generator/`. Its script previously wrote directly into the mod's `Config/`/`Links/` folders (path assumptions broke on move); it now writes self-contained outputs to `_Generator/GeneratedFiles/`, and "push to game" is an explicit copy step. `WORKFLOW-ESCWindowPlus-Generator.md` moved alongside it and was updated.
  - Removed the now-empty `Support/`, `Tools/`, `Tools/Templates/`, and `.vscode/` folders.
  - Updated `AGF-NoEAC-Projects.code-workspace` to drop the dead `Support` reference.
  - Deleted the stray transcript log at `References/Research/search_results_extended.txt`.
  - Moved `References/Decompiled-DLLs/` (14,442 files, ~107MB, gitignored/regenerable) -> `82_WorkspaceData/References/Decompiled-DLLs/`. No new `83_DevelopmentResources` domain was created.
  - Purged every `__pycache__` folder repo-wide (all gitignored junk; some had been accidentally committed before the ignore rule existed).
- **Root gitignore fixes**: added `.history/` (was leaking editor local-history into `git status`), `99_Temp/`, `82_WorkspaceData/References/Decompiled-DLLs/` (replacing the old DLL-Projects path), and `00_DLL-Projects/Generators/**/GeneratedFiles/`.
- **`99_Temp/` created** (flat, gitignored, no subfolder taxonomy needed) and swept the four confirmed-safe root temp files into it: `TEMP-aop-weather-localization.csv`, `TEMP-aop-weather-windows.xml`, `TEMP-direct-live-edit.py`, `_test_upload.ps1`.

### Deliberately not touched yet (needs a decision or a foundation first)

- Discord domain split (`21_DiscordBot` creation, resolving `SCRIPT-DiscordRoleSelectorBot.py` vs. `SCRIPT-DiscordReactionRolesMulti.py`, classifying image intake) - decision-heavy, see Section 11 below.
- `06_PublishingSupport`, `80_Automation`, script registry, centralized path module - per Section 12, these need a registry/path-module foundation before any script actually moves.
- `_x2.6/` and `_Quarantine-GameRemovals/` archival - both are still live-referenced by scripts (`SCRIPT-TransferChangelogs.py`, `SCRIPT-FixDraftChangelogs.py`, `SCRIPT-RestoreAndFix.py`, and a `--quarantine-retention-days` flag in `Workflow/05_pipeline_engine.py`). Moving them now would break those scripts.
- `81_Documentation` / further `82_WorkspaceData` consolidation beyond the Decompiled-DLLs move.
- Known latent bug (unrelated to reorg, not yet fixed): `SCRIPT-Main.py` and `Workflow/03_package.py` reference a root `SCRIPT-GenerateModBanners.py` that doesn't exist; the real script is `Workflow/SCRIPT-GenerateModImages.py`.
- Nothing from this work has been committed to git yet - the repo has a large pool of uncommitted changes (some predating this reorg work).

---

## 1. Goal

Make the repository root simple enough that the important workflow is obvious at a glance, while preserving every currently working script until it has been inventoried, tested, and migrated safely.

The desired result is:

- the existing mod lifecycle remains easy to read from `00` through `06`;
- DLL source projects remain prominent and receive a first-class development area;
- publication support is grouped by destination;
- Discord server management and Discord bot development are clearly separate;
- normal actions are started through a small set of obvious launchers;
- old, experimental, one-time, and unknown scripts are cataloged before archival;
- no path is changed until all callers and hard-coded references are known;
- secrets, generated files, logs, and temporary work have predictable homes.

## 2. Important Findings from the Current Workspace

This proposal is based on the current repository, not only on folder names.

### 2.1 The numbered folders are an operational contract

The current folders have defined workflow roles:

| Folder | Current role |
|---|---|
| `00_Images` | Source media, generated banners/thumbnails, and image intake |
| `01_Draft` | Work-in-progress mod lane |
| `02_ActiveBuild` | Testing lane synchronized with the game's Mods folder |
| `03_ReleaseSource` | Release-ready source lane |
| `04_DownloadZips` | Generated public artifacts |
| `05_GigglePackReleaseData` | GigglePack state plus current publication support data |

These names are referenced throughout the Python scripts, batch files, MCP servers, documentation, generated URLs, and configuration. They should not be casually renamed.

### 2.2 Publishing has already become a sixth workflow phase

`RUN-Publish.bat` runs the main workflow and then invokes `Workflow/06_nexus.py`. Nexus and ModNetwork scripts also consume `03_ReleaseSource`, `04_DownloadZips`, and release metadata. This supports adding a visible publication phase after packaging.

### 2.3 The root is overloaded

At the time of review, the root contains **45 files**, including **31 Python, batch, or PowerShell scripts**. They include a mixture of:

- primary workflow entry points;
- publishing tools;
- Discord tools;
- DLL synchronization;
- mod generators;
- repairs and one-time migrations;
- diagnostics;
- temporary experiments;
- loose notes and logs.

The naming style (`RUN-*`, `SCRIPT-*`, `TEMP-*`) helps somewhat, but it does not indicate whether a file is active, occasional, obsolete, dangerous, or safe to archive.

### 2.4 Discord currently crosses several unrelated locations

Discord-related material currently appears in at least four conceptual areas:

1. `Workflow/Discord` — bot configurations, role panels, image intake, documentation, and state.
2. `00_DLL-Projects/Support_DiscordAutomation` — Discord server plan and apply tooling.
3. `05_GigglePackReleaseData/Discord` — generated release announcement and webhook posting.
4. Root `SCRIPT-Discord*.py` files and loose Discord notes.

These are not all the same concern. Server administration, a continuously running bot, release announcements, and mod-image intake should not be treated as one folder merely because they all use Discord.

### 2.5 DLL work is first-class but its current name hides that importance

`00_DLL-Projects` contains many active C# projects, support utilities, decompiled references, build work, a dedicated VS Code workspace, and project-specific generators. It is also referenced directly by synchronization scripts and MCP tools. It deserves a prominent, documented development area, but moving it must be a deliberate migration.

### 2.6 Path migration is the main risk

Examples found during this planning review include:

- root-relative paths derived from `__file__`;
- batch launchers that assume `SCRIPT-Main.py` and `.venv` are in the root;
- Python scripts that directly name `Workflow`, `00_Images`, `03_ReleaseSource`, or `00_DLL-Projects`;
- older utilities with absolute `c:\GitHub\7D2D-Mods\...` paths;
- MCP servers with a fixed workspace path;
- docs and configs containing executable command examples;
- public GitHub download and image URLs that include repository paths.

Therefore, **organization must be handled as a compatibility migration, not as a drag-and-drop cleanup**.

---

## 3. Recommended Numbering Scheme

### Recommendation: use number ranges by domain

Keep `00–09` for the mod release pipeline. Use later ranges for important parallel domains instead of pretending everything happens sequentially after publishing.

| Range | Meaning | Why |
|---|---|---|
| `00` | Development inputs | Images and DLL source projects feed mods before or during Draft/Active Build work |
| `01–09` | Mod production and release pipeline | This is the real sequential workflow |
| `20–29` | Community and Discord operations | Operational work, not a release lane |
| `80–89` | Shared automation and internal tooling | Cross-cutting implementation support |
| `90–99` | Archive, quarantine, and migration history | Keeps inactive material out of daily view |

This scheme leaves room for growth without renumbering the established workflow later.

### Why DLL projects belong at `00`, not after publishing

DLL projects are not a later phase after Draft or Active Build. They are source projects for DLL-based mods that may currently exist in `01_Draft`, `02_ActiveBuild`, or `03_ReleaseSource`. Compiled DLL output flows from the source project into the corresponding mod folder.

That makes DLL projects an **upstream development input**, similar to images. The `00` prefix should therefore represent supporting source material that feeds the lifecycle, while `01–06` represents the mod's progression through the lifecycle.

Discord management is still separate because it is not mod source and is not a mandatory workflow phase.

---

## 4. Proposed Top-Level Structure

```text
7D2D-Mods/
|
|-- README.md
|-- WORKSPACE-ORGANIZATION-PLAN.md
|-- 7D2D-Mods.code-workspace
|-- .gitignore
|
|-- RUN-Update.bat
|-- RUN-Publish.bat
|-- RUN-PrePublish-Check.bat
|-- RUN-Tools.bat                    # proposed menu/dispatcher, not created yet
| |
|-- 00_DLL-Projects/
|-- 00_Images/
|-- 01_Draft/
|-- 02_ActiveBuild/
|-- 03_ReleaseSource/
|-- 04_DownloadZips/
|-- 05_ReleaseData/
|-- 06_PublishingSupport/
|
|-- 20_DiscordManagement/
|-- 21_DiscordBot/
|
|-- 80_Automation/
|-- 81_Documentation/
|-- 82_WorkspaceData/
|
|-- 90_Archive/
|-- 91_Quarantine/
`-- 99_Temp/
```

This is the **target model**, not an instruction to create or rename all of these immediately.

### Root-file policy

The finished root should contain only:

1. repository essentials (`README.md`, `.gitignore`, workspace file);
2. the numbered domain folders;
3. a maximum of approximately three or four safe human entry-point launchers;
4. optionally a short `START-HERE.md` if the public `README.md` should remain website-focused.

Implementation scripts, helper configs, logs, old notes, and temporary files should not remain loose in the root.

---

## 5. Folder Responsibilities

## `00_Images`

**Recommendation:** keep the current name and role.

It is both an input/support phase and a public URL location. Moving it would require updates to generators, manifests, README image URLs, and possibly existing external links. Internal cleanup can happen later without changing its public path.

Suggested internal shape:

```text
00_Images/
|-- README.md
|-- source/
|-- mod-media/
|-- generated/          # consider only if public URL compatibility is handled
|-- templates/
|-- fonts/
`-- tools/
```

Do not rename `_generated` until public URL and script compatibility is explicitly addressed.

## `01_Draft` through `04_DownloadZips`

**Recommendation:** keep all four names unchanged.

These clearly describe the lane model and are deeply integrated with active automation. Their contents can be governed more clearly, but their top-level paths should be treated as stable API names.

## `05_ReleaseData`

**Proposed eventual rename of:** `05_GigglePackReleaseData`

Purpose:

- release state;
- release history;
- pending change data;
- generated announcement payloads;
- pack-specific metadata.

Suggested structure:

```text
05_ReleaseData/
|-- README.md
|-- GigglePack/
|   |-- release-state.json
|   |-- pending-changes.json
|   `-- release-history.md
`-- GeneratedAnnouncements/
    `-- Discord/
```

### Compatibility warning

The current `05_GigglePackReleaseData` path is referenced by the main pipeline and publishing scripts. Rename only after introducing centralized path configuration or temporary compatibility handling.

### Lower-risk alternative

Keep the current folder name permanently and only move publication-site folders out of it. This produces most of the organizational benefit with much less migration risk.

## `06_PublishingSupport`

Purpose: everything used to prepare, validate, or execute publication **outside the GitHub Pages repository output itself**.

```text
06_PublishingSupport/
|-- README.md
|-- NexusMods/
|   |-- README.md
|   |-- config/
|   |-- templates/
|   |-- generated/
|   `-- tools/
|-- ModNetwork/
|   |-- README.md
|   |-- config/
|   |-- templates/
|   |-- generated/
|   `-- tools/
|-- FutureSiteName/
`-- Shared/
    |-- field-mappings/
    `-- templates/
```

This folder should contain site-specific publishing support currently split between root scripts and `05_GigglePackReleaseData`.

### What belongs here

- Nexus audit, plan, description, upload, and version-check tools;
- ModNetwork planning/upload/update tools;
- site API capability notes;
- per-site templates and non-secret configuration;
- generated upload plans and publication assistance files.

### What does not belong here

- canonical zip artifacts (`04_DownloadZips`);
- canonical mod source (`03_ReleaseSource`);
- pack release state (`05_ReleaseData` or current equivalent);
- general Discord server administration;
- the continuously running Discord bot.

### Secrets policy

Files such as API keys must be ignored by Git and named consistently, for example:

```text
*.private.json
*.private.txt
.env
.env.*
```

Prefer environment variables where scripts support them. Each publication site should include an example configuration with placeholder values, never a real secret.

## `00_DLL-Projects`

**Renamed from:** `_DLL-Projects`

Purpose: all first-class C# mod source, build support, decompiled references, capability contracts, and DLL-specific tools.

Suggested internal structure:

```text
00_DLL-Projects/
|-- README.md
|-- AGF-NoEAC-Projects.code-workspace
|-- Projects/
|   |-- DLL_NoEAC-.../
|   |-- DLL_4Modders-.../
|   `-- DLL_Fixes-.../
|-- Generators/
|-- Support/
|-- References/
|   `-- Decompiled-DLLs/
|-- Tools/
`-- BuildTemp/
```

### Practical recommendation

Do this in two stages:

1. Rename `_DLL-Projects` to `00_DLL-Projects` only after references are centralized and tested.
2. Reorganize its internal projects later. Do not combine the top-level rename and a deep internal restructure in one change.

### Why this folder receives number `00`

It contains source code used to build the DLL portions of mods in the lifecycle lanes. It therefore sits before or alongside Draft work rather than after release work. Sharing the `00` prefix with `00_Images` identifies both as important development inputs without implying that one must happen before the other.

The DLL source project remains separate from its assembled mod folder:

```text
00_DLL-Projects/<project source>
    -> build/sync compiled DLL
01_Draft/<assembled mod under development>
    -> promote when ready
02_ActiveBuild/<assembled mod under test>
    -> promote when release-ready
03_ReleaseSource/<release source>
```

## `20_DiscordManagement`

Purpose: administering the Discord server itself.

```text
20_DiscordManagement/
|-- README.md
|-- ServerPlan/
|   |-- discord-server-plan.json
|   `-- Discord-Management.md
|-- ApplyTools/
|-- Audits/
|-- RoleAndChannelDesign/
`-- Notes/
```

Examples that belong here:

- server/category/channel plan;
- permissions and onboarding plans;
- apply/dry-run scripts that change Discord structure;
- role design and server administration audits;
- human management documentation.

These currently overlap with `00_DLL-Projects/Support_DiscordAutomation`. Discord server management is not DLL work, so it should eventually leave that folder.

## `21_DiscordBot`

Purpose: runnable bot application code, bot-specific configuration, dependencies, deployment instructions, and runtime state.

```text
21_DiscordBot/
|-- README.md
|-- src/
|-- config/
|   |-- examples/
|   `-- local/                 # ignored if it contains private IDs/settings
|-- data/
|-- requirements.txt
|-- deployment/
|   |-- local/
|   |-- gravel-host/
|   `-- systemd/
|-- tests/
`-- RUN-Bot.bat
```

Likely candidates after validation:

- `SCRIPT-DiscordReactionRolesMulti.py`;
- `SCRIPT-DiscordRoleSelectorBot.py` if still independently used;
- reaction-role panels;
- streamer watchlist;
- bot requirements;
- hosting/deployment notes.

### Separate adjacent Discord concerns

- **Release webhook announcement generation/posting** remains part of release/publication automation, not the bot.
- **Mod-image intake** may be placed under `21_DiscordBot` if it runs as a bot feature, or `20_DiscordManagement/IntakeAutomation` if it is an administrative utility. Decide based on how it is actually launched and maintained.
- **Server plan application** belongs in `20_DiscordManagement`, not `21_DiscordBot`.

## `80_Automation`

Purpose: implementation scripts shared by the workflow and other domains.

```text
80_Automation/
|-- README.md
|-- workflow/
|-- mod-tools/
|-- media-tools/
|-- dll-tools/
|-- diagnostics/
|-- repair-tools/
|-- migrations/
|-- mcp-servers/
|-- config/
`-- registry/
    `-- scripts.json
```

The current `Workflow` folder would eventually become `80_Automation/workflow`, but only after stable root wrappers and a centralized path module exist.

## `81_Documentation`

Purpose: internal operating documentation and project notes that do not belong to one domain folder.

```text
81_Documentation/
|-- Workspace/
|-- Workflow/
|-- Handoffs/
|-- Research/
`-- Historical/
```

Domain-specific documentation should stay with its domain. For example, Nexus documentation belongs under `06_PublishingSupport/NexusMods`, and bot deployment documentation belongs under `21_DiscordBot`.

## `82_WorkspaceData`

Purpose: shared, durable non-source data used by automation.

Potential contents:

- `_Quotes`;
- compatibility tables, if they are not kept with README automation;
- stable helper mappings such as DLL synchronization maps;
- manually maintained source data that is not generated output.

This folder should not become a miscellaneous dumping ground. Every subfolder needs an owner and README.

## `90_Archive`

Purpose: known inactive material retained for reference.

```text
90_Archive/
|-- README.md
|-- scripts/
|   |-- one-time/
|   |-- superseded/
|   `-- unknown-retained/
|-- old-game-versions/
|-- backups/
`-- notes/
```

An archive item should include either a manifest entry or a small note stating:

- original path;
- archived date;
- last known purpose;
- replacement, if any;
- whether it is safe to delete later.

## `91_Quarantine`

Purpose: automated safety holding areas such as game removals and transaction rollback data.

This is distinct from an archive:

- **archive** = intentionally retained historical material;
- **quarantine** = temporary recovery material created by a workflow.

Existing `_Quarantine-GameRemovals` and `_TransactionRollback` could eventually become subfolders here, but only after the pipeline paths and retention logic are updated.

## `99_Temp`

Purpose: disposable experiments and short-lived files.

Rules:

- ignored by Git by default;
- no script may depend on it for a normal workflow;
- contents can be deleted after review;
- temporary root files such as `TEMP-*` should eventually move here.

---

## 6. Script Organization Strategy

Moving scripts safely requires knowing what each one is and how it is called.

### 6.1 Keep stable launchers in the root

Recommended root entry points:

| Launcher | Human purpose |
|---|---|
| `RUN-Update.bat` | Synchronize and update day-to-day work |
| `RUN-PrePublish-Check.bat` | Validate before publishing |
| `RUN-Publish.bat` | Perform the normal full publish flow |
| `RUN-Tools.bat` | Open a menu for occasional tools |

Optional specialized launchers such as “publish without GigglePack” can be exposed through `RUN-Tools.bat` or a `RUN-Advanced/` area rather than occupying the root.

### 6.2 Use wrappers before moving implementations

The safest pattern is:

```text
Root launcher (stable human interface)
    -> dispatcher/CLI (stable automation interface)
        -> implementation module (free to move internally)
```

For example, `RUN-Publish.bat` can remain in the root even if the Python implementation later moves to `80_Automation/workflow`. The launcher should resolve the repository root and invoke the implementation through a stable path or module.

### 6.3 Create a script registry before archiving anything

Recommended fields:

| Field | Meaning |
|---|---|
| `id` | Stable script identifier |
| `current_path` | Existing location |
| `proposed_path` | Eventual destination |
| `category` | entry-point, workflow, publish, bot, repair, migration, diagnostic, temp |
| `status` | active, occasional, unknown, superseded, archived |
| `owner_domain` | workflow, DLL, Nexus, ModNetwork, Discord management, Discord bot |
| `called_by` | batch file, Python script, task, doc, manual only |
| `inputs` / `outputs` | Important files and folders |
| `destructive` | Whether it deletes, overwrites, renames, posts, or uploads |
| `dry_run` | Whether safe preview exists |
| `last_verified` | Date and method |
| `replacement` | New script if superseded |
| `notes` | Known constraints |

Suggested statuses:

- **ACTIVE-CORE** — normal workflow dependency;
- **ACTIVE-OPTIONAL** — intentionally used but not every run;
- **REVIEW** — purpose appears plausible but usage is unconfirmed;
- **ONE-TIME** — migration or repair that should not remain prominent;
- **SUPERSEDED** — replacement is known;
- **ARCHIVED** — retained but removed from active execution paths;
- **TEMP** — disposable after explicit confirmation.

### 6.4 Never infer “unused” from age alone

File modification time is useful evidence, not proof. A stable script may be old because it works. Before moving a candidate:

1. search code, configs, docs, VS Code tasks, and batch files for its name;
2. check imports and subprocess calls;
3. inspect its inputs and outputs;
4. check Git history if needed;
5. ask whether it is launched manually outside the repository;
6. run a non-destructive help, self-test, or dry-run if available;
7. record the result in the registry.

### 6.5 Proposed category mapping for current root scripts

This is a review starting point, not a final disposition.

| Category | Examples | Proposed destination |
|---|---|---|
| Core workflow | `SCRIPT-Main.py`, `SCRIPT-GenerateModBanners.py` | `80_Automation/workflow` or `media-tools` |
| Publishing | `SCRIPT-NexusMods.py`, `SCRIPT-NexusUpload.py`, `SCRIPT-AuditNexusMods.py`, `SCRIPT-ModNetwork.py` | `06_PublishingSupport/<site>/tools` |
| Discord management/bot | `SCRIPT-Discord*.py` | `20_DiscordManagement` or `21_DiscordBot` after classification |
| DLL support | `SCRIPT-SyncNoEACDlls.py`, `HELPER_NoEACDllSync.json` | `00_DLL-Projects/Tools` or `80_Automation/dll-tools` |
| Mod creation/generation | `SCRIPT-MakeNewMod.py`, PurpleBook tools | `80_Automation/mod-tools` or owning project |
| Diagnostics | `SCRIPT-FinalCheck.py`, `SCRIPT-CheckRemainingIssues.py` | `80_Automation/diagnostics` |
| Repairs/migrations | `SCRIPT-RestoreAndFix.py`, `SCRIPT-MigrateFeatureSections.py`, changelog transfer/fix tools | `80_Automation/repair-tools` or `migrations`, then archive when retired |
| Temporary | `_test_upload.ps1`, `TEMP-*` | `99_Temp` after confirming no dependencies |

---

## 7. Path-Safety Design Before Reorganization

### 7.1 Establish one repository-root resolver

Create a small shared path/config module used by active Python tools. Conceptually:

```python
REPO_ROOT = find_repo_root(__file__)
PATHS = {
    "draft": REPO_ROOT / "01_Draft",
    "active_build": REPO_ROOT / "02_ActiveBuild",
    "release_source": REPO_ROOT / "03_ReleaseSource",
    "download_zips": REPO_ROOT / "04_DownloadZips",
}
```

Do not scatter replacement strings across dozens of files. A central path contract makes future changes testable and reversible.

### 7.2 Resolve paths from the repository, not the current shell directory

Batch launchers should continue to use `%~dp0` or a derived repository root. Python scripts should resolve from `__file__` or the shared root module. Avoid relying on the user's current working directory.

### 7.3 Remove absolute local workspace paths from active tools

Absolute paths such as `c:\GitHub\7D2D-Mods\...` should be replaced in active scripts with:

1. repository-relative paths;
2. configuration values;
3. environment variables for machine-specific locations, especially the game Mods folder.

Archived scripts can retain original paths if they are clearly marked non-runnable.

### 7.4 Treat public URLs as compatibility contracts

Paths embedded in GitHub download and image URLs are externally visible. Renaming `00_Images` or `04_DownloadZips` may break existing links outside this repository. Prefer keeping those names.

### 7.5 Use temporary compatibility shims only when useful

For an important renamed script, a small wrapper at the old path can print a deprecation notice and call the new location. For directory migrations, scripts may temporarily accept both old and new paths, as the lane code has done previously.

Compatibility shims should have an explicit removal milestone; otherwise they become permanent clutter.

### 7.6 Validate callers, not only the moved script

A successful `python moved_script.py --help` does not prove the migration works. Validation must include:

- batch launchers;
- subprocess callers;
- imports;
- VS Code workspace/tasks;
- MCP configuration;
- documentation commands;
- config file paths;
- output locations;
- public URLs where applicable.

---

## 8. Recommended Migration Phases

Only one conceptual area should move per commit/checkpoint.

## Phase 0 — Freeze and baseline

**No reorganization yet.**

1. Ensure Git status and intentionally private/untracked files are understood.
2. Record the current successful commands for update, dry-run, self-test, publish preparation, Nexus planning, Discord bot startup, and DLL synchronization.
3. Capture expected outputs and exit codes.
4. Back up private local configuration and secrets outside the repository if necessary.

**Exit criterion:** the current workflow has a reproducible baseline.

## Phase 1 — Inventory and classification

1. Create the script registry.
2. Classify all root scripts and batch files.
3. Mark destructive/networked tools.
4. Identify every hard-coded repository path.
5. Identify duplicate documentation and backup folders.
6. Ask for confirmation on all `REVIEW`, `ONE-TIME`, and `TEMP` candidates.

**Exit criterion:** every root script has an owner, status, and intended destination.

## Phase 2 — Stabilize entry points and paths

1. Define the supported root launchers.
2. Add a shared repository-root/path module.
3. Update active scripts to use centralized paths.
4. Add or strengthen `--help`, `--dry-run`, and self-test behavior.
5. Update MCP servers and workspace tasks to avoid fixed local paths where possible.

**Exit criterion:** implementation scripts can move without changing the human commands.

## Phase 3 — Clean obvious non-operational clutter

1. Create `99_Temp` and move confirmed disposable experiments there.
2. Move loose logs into a defined log location.
3. Move loose notes into their owning domain or `81_Documentation`.
4. Consolidate duplicate workflow documents only after selecting the source of truth.
5. Add archive manifests before moving backups or one-time scripts.

**Exit criterion:** the root is visibly cleaner without changing core implementation paths.

## Phase 4 — Separate publishing support

1. Create `06_PublishingSupport`.
2. Move one publication site at a time, beginning with Nexus Mods.
3. Keep `03_ReleaseSource`, `04_DownloadZips`, and pack release state unchanged.
4. Update wrappers, configs, docs, and generated-output paths.
5. Run read-only/version-check and dry-run tests before any upload test.
6. Repeat for ModNetwork.

**Exit criterion:** publication tools are grouped by site and normal publication checks still work.

## Phase 5 — Separate Discord domains

1. Create `20_DiscordManagement` and move server-plan/apply tooling.
2. Test its dry-run against Discord before any apply operation.
3. Create `21_DiscordBot` and move bot runtime code/config.
4. Add a stable `RUN-Bot.bat` and deployment package definition.
5. Decide the owner of image-intake automation.
6. Keep release webhook announcements with release/publication automation.

**Exit criterion:** server management and bot runtime can be understood, tested, and deployed independently.

## Phase 6 — Align DLL source projects with the mod lifecycle

1. Update DLL sync scripts, MCP tools, workspace files, and project documentation to use centralized paths.
2. Rename `_DLL-Projects` to `00_DLL-Projects` in its own checkpoint.
3. Test solution discovery, C# builds, DLL sync dry-run, and selected real sync.
4. Only later reorganize internal `Projects`, `Support`, `References`, and `Tools` folders.

**Exit criterion:** DLL source development and synchronization work from the upstream `00` location, while assembled mods continue through Draft, Active Build, and Release Source.

## Phase 7 — Move shared automation

1. Move `Workflow` implementation into `80_Automation/workflow` only after wrappers are stable.
2. Move diagnostics, repairs, generators, and MCP servers by category.
3. Retain root wrappers for supported human commands.
4. Archive superseded wrappers after a defined compatibility period.

**Exit criterion:** the root exposes actions while implementation details live under automation.

## Phase 8 — Archive and delete conservatively

1. Archive scripts whose replacements and non-use have been proven.
2. Keep unknown scripts in `90_Archive/scripts/unknown-retained`, not the recycle bin.
3. Run at least one normal work cycle and one publication cycle.
4. Delete only after an explicit later review.

**Exit criterion:** nothing remains in the root merely because its purpose is unknown, and nothing was deleted merely because it looked old.

---

## 9. Validation Checklist for Every Migration Checkpoint

### Static checks

- [ ] Search for every old path and old script name.
- [ ] Review `.bat`, `.ps1`, `.py`, JSON, Markdown, workspace, and MCP references.
- [ ] Confirm private files remain ignored.
- [ ] Confirm public download/image paths were not unintentionally changed.
- [ ] Confirm generated outputs still go to canonical folders.

### Safe execution checks

- [ ] Python files compile or show help successfully.
- [ ] Main workflow self-test passes.
- [ ] Update dry-run completes.
- [ ] Publish dry-run completes without publishing.
- [ ] Nexus/ModNetwork planning or audit mode completes without upload.
- [ ] DLL sync dry-run resolves projects and targets.
- [ ] Discord management dry-run does not propose unexpected changes.
- [ ] Discord bot starts with test configuration and resolves its files.

### Behavioral checks

- [ ] `RUN-Update.bat` still works from a double-click and from a terminal.
- [ ] `RUN-PrePublish-Check.bat` still works.
- [ ] `RUN-Publish.bat` still reaches the intended publication-support phase.
- [ ] Release sources, zips, README generation, images, and logs are unchanged unless intentionally migrated.
- [ ] Rollback/quarantine paths still function.

### Recovery checks

- [ ] The change is isolated in Git.
- [ ] Old-to-new mapping is documented.
- [ ] Reverting one commit restores the prior working layout.
- [ ] No secret was added to Git history.

---

## 10. Proposed Root After the First Low-Risk Cleanup

Before deep script moves, a realistic intermediate root could be:

```text
.gitignore
7D2D-Mods.code-workspace
README.md
WORKSPACE-ORGANIZATION-PLAN.md

RUN-Update.bat
RUN-PrePublish-Check.bat
RUN-Publish.bat
RUN-Tools.bat
SCRIPT-Main.py                 # temporarily remains for compatibility

00_DLL-Projects/               # only after its dedicated migration
00_Images/
01_Draft/
02_ActiveBuild/
03_ReleaseSource/
04_DownloadZips/
05_GigglePackReleaseData/      # initially retained to reduce risk
06_PublishingSupport/
20_DiscordManagement/
21_DiscordBot/
80_Automation/
81_Documentation/
82_WorkspaceData/
90_Archive/
91_Quarantine/
99_Temp/
```

This intermediate state intentionally preserves `SCRIPT-Main.py` and `05_GigglePackReleaseData` until compatibility work is complete.

---

## 11. Decisions to Confirm Before Implementation

1. **Should `05_GigglePackReleaseData` be renamed?**  
   Recommendation: initially keep it, extract Nexus/ModNetwork into `06_PublishingSupport`, and reconsider the rename after paths are centralized.

2. **Should `_DLL-Projects` become `00_DLL-Projects`?**  
   Recommendation: yes. DLL projects are upstream source for mods in Draft/Active Build, so `00` describes their role better than `10`. Do it as a standalone migration after dependency updates.

3. **Which root launchers are truly part of the daily workflow?**  
   Proposed core set: Update, Pre-Publish Check, Publish, and Tools.

4. **Is `SCRIPT-DiscordRoleSelectorBot.py` still needed separately from `SCRIPT-DiscordReactionRolesMulti.py`?**  
   This must be verified before either is moved or archived.

5. **Does Discord mod-image intake belong to bot runtime or server management?**  
   Decide from actual operational use.

6. **Should MCP servers remain inside this public website repository?**  
   They can live under `80_Automation/mcp-servers`, but their fixed workspace assumptions need review.

7. **Which loose notes contain durable information?**  
   Files such as Discord notes, PurpleBook handoffs, support notes, and logs need owner-based classification before relocation.

8. **How long should compatibility wrappers remain?**  
   Suggested: through at least one complete normal update and publication cycle, followed by explicit review.

---

## 12. Recommended First Implementation Task

When implementation is approved, do **not** begin by moving folders. Begin with a “workspace inventory and compatibility foundation” change that delivers:

1. a complete script registry;
2. a machine-readable old-path/new-path map;
3. a centralized repository path module;
4. a documented set of supported root launchers;
5. baseline dry-run/self-test results;
6. `.gitignore` verification for secrets, temporary files, logs, and generated data.

Only after that foundation should the first real move occur. The best first domain move is likely **Nexus Mods support into `06_PublishingSupport/NexusMods`**, because it has a clear boundary and can be validated with audit/planning operations before any network upload.

---

## 13. Summary Recommendation

- Keep `00_Images` through `04_DownloadZips` as stable workflow paths.
- Treat publication as phase `06`, but separate publication tooling from pack release state.
- Initially retain `05_GigglePackReleaseData` to avoid unnecessary risk.
- Rename `_DLL-Projects` to `00_DLL-Projects` in a dedicated migration, identifying it as upstream source for DLL-based mods in the lifecycle lanes.
- Create separate `20_DiscordManagement` and `21_DiscordBot` domains.
- Keep only a few stable `RUN-*` launchers in the root.
- Move implementation details under `80_Automation` only after wrappers and centralized paths exist.
- Catalog every script before deciding whether it is active, one-time, superseded, archived, or temporary.
- Archive first; delete much later.
- Use small Git checkpoints and dry-run validation for every domain move.

The key principle is: **make the workspace easier to understand without making existing automation harder to trust.**