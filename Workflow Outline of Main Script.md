# AGF 7D2D Mod Management Script Workflow (AI & Human Reference, v2)

## How to Use This Outline

This outline is the **master reference** for both humans and AI. The main script (SCRIPT-Main.py) must follow this outline exactly. If the script and this outline ever diverge, **this outline is the source of truth**. Every step is written for beginners, with clear instructions, conditions, and references to files and templates. If you are an AI, use this outline to repair or regenerate the script as needed.

---

## Section A: Lane Model and Script Modes

**Why:** To keep drafting, testing, and publishing separate so nothing is accidentally released.

### Lane Model (Folder Roles)

1. `01_Draft` = **Draft Lane**
   - Work-in-progress ideas and unfinished edits.
   - Not automatically synced to the game.
2. `02_ActiveBuild` = **Test Lane**
   - Mods currently being tested.
   - This lane syncs with your game mods folder.
3. `Game Mods Folder` = **Runtime Lane**
   - The copy currently used by the game.
   - Sync target for active testing.
4. `03_ReleaseSource` = **Release Lane**
   - Release-ready versions only.
   - Source for final zips and website downloads.
5. `04_DownloadZips` = **Final Artifact Lane**
   - Generated zip files for public downloads.

### Legacy Name Compatibility

- The script still supports older lane folder names while transitioning:
  - `_Mods2.In-Progress` -> `01_Draft`
  - `_Mods0.Staging` -> `02_ActiveBuild`
  - `_Mods1.PublishReady` -> `03_ReleaseSource`
  - `_Mods3.zip` -> `04_DownloadZips`

### What End Users Download

- End users download **zip files only**.
- Zips are generated in `04_DownloadZips`.
- Public links should point to those generated zip files.

### Script Modes (Single Script Entry Point)

Use only `SCRIPT-Main.py` with one of these modes:

1. `--mode sync-work`
   - Syncs `02_ActiveBuild` and game Mods by version.
   - Keeps only one active BackpackPlus in game root (defaults to `084Slots` when present).
   - Mirrors all BackpackPlus variants to `.Optionals-Backpack`.
   - Mirrors all HUDPlus/HUDPluszOther to `.Optionals-HUDPlus` (HUDPluszOther root placement is user-managed/manual).
   - Does not touch `03_ReleaseSource`.
2. `--mode promote`
   - First refreshes folder names and per-mod README files in `02_ActiveBuild`.
   - Promotes from `02_ActiveBuild` to `03_ReleaseSource`.
   - Promotion is version-gated (deliberate bump behavior).
3. `--mode package`
   - First refreshes folder names and per-mod README files in `03_ReleaseSource`.
   - Builds zip outputs and updates the main README download list.
   - Uses release lane data.
4. `--mode full`
   - Runs the legacy full pipeline end-to-end.

### Recommended Day-to-Day Workflow

1. Build in `01_Draft`.
2. Move/copy to `02_ActiveBuild` when ready to test.
3. Run `SCRIPT-Main.py --mode sync-work`.
4. Test in game.
5. When ready to release, bump version in `ModInfo.xml` and run `SCRIPT-Main.py --mode promote`.
6. Run `SCRIPT-Main.py --mode package`.

---

## Step 1: Find and Match Your Mods

**Why:** To identify all mod folders to manage, regardless of their current state.

1. Look for mod folders in these locations:
   - `_Mods1.PublishReady` (finished mods, ready to share)
   - `_Mods2.In-Progress` (mods you are still working on)
   - The game’s mods folder (where you test mods in the game)
2. Only include folders that start with `AGF` or `zzzAGF`.
3. Match mods by **folder name** (not by the Name in ModInfo.xml).
4. For each matched mod, read the version number from its `ModInfo.xml` file in both places.
   - If `ModInfo.xml` is missing or unreadable, skip that mod and log a warning.

---

## Step 2: Sync Mods by Version Number

**Why:** To ensure you always work with the latest version of each mod, and avoid losing changes.

1. For each mod with the same folder name:
    - If the version in both places is the same: **Do nothing.**
    - If the version in the game mods folder is **higher**:
       - Copy the whole mod folder from the game mods folder to `_Mods1.PublishReady` or `_Mods2.In-Progress` (whichever it belongs in).
       - Delete the copied mod from the game mods folder (just that one, not others).
       - Don’t try to merge files—just copy everything over.
    - If the version in `_Mods1.PublishReady` or `_Mods2.In-Progress` is **higher**:
       - Delete the mod from the game mods folder.
       - Copy the whole mod folder from `_Mods1.PublishReady` or `_Mods2.In-Progress` to the game mods folder.
       - Don’t try to merge files—just copy everything over.
2. Note:
   - If you change the Name in ModInfo.xml but not the folder name, the script still matches by folder name.
   - Folder renaming is handled later (see Step 4).

---

## Step 3: Move Mods Based on Major Version

**Why:** To keep dev/in-progress mods and publish-ready mods in the correct folders.

1. For each mod (after syncing):
   - Open its `ModInfo.xml` and find the version (like 1.2.3).
   - The **major version** is the number before the first dot (e.g., 1 in 1.2.3).
2. If the major version is **0**:
   - Make sure the mod is in `_Mods2.In-Progress`. If it’s in `_Mods1.PublishReady`, move it to `_Mods2.In-Progress`.
3. If the major version is **1 or higher**:
   - Make sure the mod is in `_Mods1.PublishReady`. If it’s in `_Mods2.In-Progress`, move it to `_Mods1.PublishReady`.
4. Always move the whole folder—don’t merge files.

---

## Step 4: Special Handling (Renaming, Compatibility, Quotes, README)

### Individual README Data Contract

- Identity and version come from each mod's `ModInfo.xml` (`Name` and `Version`).
- Compatibility fields come from `HELPER_ModCompatibility.csv` by `MOD_NAME` (base mod name, no version suffix).
- Quotes come from `_Quotes/<MOD_NAME>.txt` (or `QUOTE_FILE` in CSV when provided).
- If a mod exists but has no CSV row yet, README generation still runs with `MISSINGDATA` defaults and logs a warning.
- Existing `FEATURES` and `CHANGELOG` blocks are preserved when regenerating README files.

**Why:** To ensure all metadata, compatibility, and documentation is correct, version-independent, and easy to maintain.

### Folder Renaming
- If the folder name does not match the `Name` in `ModInfo.xml`, rename the folder to match (if the name is valid).
- If you rename a folder, update all references in the CSV and quote files (see below).

### Update `HELPER_ModCompatibility.csv`
- The CSV is the **source of truth** for all mod compatibility and quote info.
- All lookups, updates, and file names use the **base mod name** (folder name with version removed, e.g., `AGF-BackpackPlus-60Slots`), not the versioned folder name. This ensures version changes don’t break anything.
- If a folder was renamed, update the `MOD_NAME` in the CSV to the new base mod name.
- Add new mods to the CSV using the base mod name. For any missing info, put `MISSINGDATA` so you know to fill it in later.
- If any fields are empty, fill them with `MISSINGDATA`.
- Remove CSV entries for mods that no longer exist in your workspace.
- **Before saving, alphabetize all rows by `MOD_NAME`** so the CSV is easy to read and maintain.
- If you hit an error reading or writing the CSV, log a warning and keep going.

### Quote Files
- For every mod in the CSV, make sure there is a quote file in `_Quotes/` (named with the **base mod name**: `MOD_NAME.txt`).
- If a mod is renamed, rename its quote file to match the new base mod name.
- Never delete quote files—only create or rename them.
- The `QUOTE_FILE` column in the CSV must always match the actual quote file name (using the base mod name).
- If a quote file contains only `None`, blank it out (do not delete).

### README.md and ReadableReadMe.txt
- For each mod, create a `README.md` from the template `TEMPLATE-ModReadMes.md`:
  - Fill in **all fields** (`MOD_NAME`, `MOD_VERSION`, `DOWNLOAD_LINK`, and all compatibility/metadata fields such as EAC_FRIENDLY, SERVER_SIDE, CLIENT_REQUIRED, SAFE_TO_INSTALL, SAFE_TO_REMOVE, UNIQUE, etc.) **from `HELPER_ModCompatibility.csv` using the base mod name**.
  - Insert the contents of the quote file (looked up by base mod name) as a Markdown blockquote for `{{QUOTE}}`.
- After updating `README.md`, create `ReadableReadMe.txt` in the same folder by converting the Markdown to plain text (remove formatting, links, blockquotes, and convert dividers).
- Do this for both `_Mods1.PublishReady` and `_Mods2.In-Progress`.
- If you hit an error reading or writing a README, log a warning and keep going.

### Preserve Important Info
- Keep the `Version` in `ModInfo.xml` from the game mods folder (if it’s higher).
- Keep the changelog/features section in `README.md` from the game mods folder (if it’s newer or changed).
- All other info (compatibility, metadata, quote references) is managed in VS Code and only overwritten when a new version is published, using the base mod name for all lookups.

---

## Step 5: Push Updated Mods Back to the Game Mods Folder (If Pulled Earlier)

**Why:** To ensure your game is always ready to test the latest version of any mod you worked on, and you never lose changes made in VS Code.

1. For any mod that was pulled from the game mods folder in Step 2 (because it was newer there), the script will now push the updated version (after all renaming, README, and CSV updates) back into the game mods folder.
2. Only mods that were originally pulled from the game mods folder during this script run are pushed back—nothing else is overwritten.
3. If copying fails (e.g., file in use), log a warning and continue.

---

## Step 6: Create Zip Files for Mods and Categories (After All Other Steps)

**Why:** To package your mods for easy sharing and backup, always including the latest updates and documentation.

- Only start this step after you have finished Steps 1–5 for all mods. Zipping is always the last stage, so your zips include all updates, renames, and new README files.
- **Before creating new zips, delete all existing zip files in `_Mods3.zip`.** This ensures only the latest, correct zips are present and prevents outdated or duplicate files.

1. **Create a zip for each mod:**
   - For every mod folder in `_Mods1.PublishReady` (starting with `AGF-` or `zzzAGF-`):
     - Make a zip file in `_Mods3.zip`.
     - The zip file name should match the mod name (without the version number).
     - Inside the zip, the folder should keep its original name (with the version number).
2. **Create category (pack) zips:**
   - For each special group, make a zip in `_Mods3.zip` with the following rules:

   **BackpackPlus_All**
   - Root: All `AGF-BackpackPlus-*` mods.
   - No optionals.

   **GigglePack_All**
   - Root: All `AGF-HUDPlus*`, all `AGF-VP-*`, all `zzzAGF-Special*`, and the one `AGF-BackpackPlus-84Slots-*` mod.
   - `.Optionals-BackpackPlus`: All `AGF-BackpackPlus-*` mods.
   - `.Optionals-HUDPlus`: All `AGF-HUDPlus*` and all `AGF-HUDPluszOther-*` mods.
   - `.Optionals-NoEAC`: All `AGF-NoEAC-*` mods.

   **HUDPlus_All**
   - Root: All `AGF-HUDPlus*` and all `zzzAGF-Special*` mods. (Do not include any `AGF-HUDPluszOther-*` mods in the root; those are only in `.Optionals-HUDPluszOther`.)
   - `.Optionals-NoEAC`: All `AGF-NoEAC-*` mods.
   - `.Optionals-HUDPluszOther`: All `AGF-HUDPluszOther-*` mods.

   **HUDPluszOther_All**
   - Root: All `AGF-HUDPluszOther-*` mods.
   - No optionals.

   **AGF-NoEAC_All**
   - Root: All `AGF-NoEAC-*` mods.
   - No optionals.

   **VP_All**
   - Root: All `AGF-VP-*` and all `zzzAGF-Special*` mods.
   - `.Optionals-NoEAC`: All `AGF-NoEAC-*` mods.

3. **How optionals work:**
   - Optionals are folders inside the zip (like `.Optionals-BackpackPlus`) that contain extra mods for that pack.
   - The root of the zip always contains the main mods for that pack.
4. All zips are created in `_Mods3.zip` and are ready to upload or share.
- If zipping fails for any mod or pack, log a warning and continue.

### Step 6.5: Auto-Versioned GigglePack Release Notes

**Why:** To create a pack-level release version and a ready-to-post update summary for Discord (and later website use).

1. After pack zips are created, treat `00_GigglePack_All.zip` as the canonical build source.
2. Track pack release state in `05_GigglePackReleaseData/gigglepack-release-state.json`.
3. Compare current GigglePack mod versions against the previous state to determine what changed since the last GigglePack release.
4. Assign a GigglePack release version automatically:
   - First release state: `1.0.0`
   - `X` (major) increases only when you intentionally trigger a major cycle bump:
   - Create `05_GigglePackReleaseData/gigglepack-major-bump.txt` before packaging.
     - The script consumes/removes that marker after a successful release-state write.
   - `Y` (minor) increases when **new mods are added** to GigglePack.
   - `Z` (patch) increases when **existing GigglePack mods are updated** (or removed).
   - If nothing changed, keep the same version.
5. Produce these outputs automatically:
   - `04_DownloadZips/AGF-GigglePack-vX.Y.Z.zip` (versioned zip)
   - `05_GigglePackReleaseData/latest-gigglepack-discord.txt` (Discord-ready update text)
   - `05_GigglePackReleaseData/latest-gigglepack-release.md` (Markdown release note)
6. The release notes must explicitly separate:
   - New mods (with their first released version)
   - Updated existing mods (showing previous version -> latest version)
   - Removed mods (if any)
   and include a link to the versioned GigglePack zip.

### Step 6.6: Discord Auto-Post (Update-Only)

**Why:** To automatically publish a Discord announcement only when GigglePack actually changes.

1. The Discord message is rendered from template:
   - `05_GigglePackReleaseData/TEMPLATE-DiscordUpdate.md`
   - Parsed block is only the text between:
     - `<!-- DISCORD_TEMPLATE_START -->`
     - `<!-- DISCORD_TEMPLATE_END -->`
2. The generated post output is written to:
   - `05_GigglePackReleaseData/latest-gigglepack-discord.txt`
3. Webhook configuration:
   - Preferred: user environment variable `AGF_DISCORD_WEBHOOK_URL`
   - Optional override: CLI argument `--discord-webhook-url`
4. Posting behavior:
   - In `package` and `full` modes, webhook posting is attempted only when GigglePack has an update.
   - No update means no post (`skip auto-post`).
   - Update is true when any of these occur:
     - baseline release
     - major bump marker used
     - new mods added
     - existing mods updated
     - mods removed
5. Message size safety:
   - If Discord content exceeds message limits, the script posts in chunks.
6. First-time seeding note:
   - If you need an initial post while there are no detected changes, do a one-time manual webhook send using `latest-gigglepack-discord.txt`.

---

## Step 7: Update Main README.md

**Why:** To keep your main documentation up to date and ready to publish or distribute with your mod pack.

1. After all zipping is complete, update the main `README.md` in the workspace root using the template `TEMPLATE-1Main.md`.
2. Fill in the mod list and all placeholders dynamically, so the README always reflects the current state of all mods and categories.
3. The script enforces consistent formatting for all category headers and special sections in README.md, including a single divider (`---`), blank line, `<br>`, blank line, and the section header. This applies to all main categories and the HUDPluszOther table section, ensuring a clean and professional appearance.
4. If updating the README fails, log a warning and continue.

5. The script always lists the AGF-BackpackPlus-119Slots mod last in the Backpack Plus Mods section, regardless of folder order, to ensure consistent presentation.
6. If GigglePack release metadata exists in `05_GigglePackReleaseData/gigglepack-release-state.json`, the script also injects into the Giggle Pack section:
   - GigglePack version inline next to the main Giggle download link
   - A collapsible changelog summary that shows previous GigglePack version and latest GigglePack version
   - Separate New Mods and Updated Existing Mods lists with explicit version transitions

---

## Step 8: What If Something Goes Wrong?

**Why:** To ensure the workflow is robust and you never lose progress due to errors.

1. If `ModInfo.xml` is missing or can’t be read, skip that mod and write a warning in the log.
2. If copying files or folders fails (for example, if a file is in use), write the error in the log and keep going with the other mods.
3. If any other step fails, log a warning and continue with the rest of the workflow.

---

**You’re done!**

If you follow these steps, your mods will always be organized, up to date, and ready to share or test. If you get stuck, check the log for details or ask for help.
