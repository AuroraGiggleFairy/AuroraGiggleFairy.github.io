import argparse
import csv
import datetime as dt
import difflib
import hashlib
import io
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import textwrap
import threading
import urllib.error
import urllib.request
import zipfile
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Set, Tuple
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape

# =============================================================
# CONFIG
# =============================================================
WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
VS_CODE_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
LANE_DRAFT_PREFERRED = os.path.join(VS_CODE_ROOT, "01_Draft")
LANE_ACTIVE_BUILD_PREFERRED = os.path.join(VS_CODE_ROOT, "02_ActiveBuild")
LANE_RELEASE_SOURCE_PREFERRED = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
LANE_DOWNLOAD_ZIPS_PREFERRED = os.path.join(VS_CODE_ROOT, "04_DownloadZips")

LANE_DRAFT_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods2.In-Progress")
LANE_ACTIVE_BUILD_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods0.Staging")
LANE_RELEASE_SOURCE_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods1.PublishReady")
LANE_DOWNLOAD_ZIPS_LEGACY = os.path.join(VS_CODE_ROOT, "_Mods3.zip")


def resolve_lane_path(preferred: str, legacy: str) -> str:
    """Prefer the new lane folder name; fall back to legacy names for compatibility."""
    if os.path.isdir(preferred):
        return preferred
    if os.path.isdir(legacy):
        return legacy
    return preferred


IN_PROGRESS = resolve_lane_path(LANE_DRAFT_PREFERRED, LANE_DRAFT_LEGACY)
STAGING = resolve_lane_path(LANE_ACTIVE_BUILD_PREFERRED, LANE_ACTIVE_BUILD_LEGACY)
PUBLISH_READY = resolve_lane_path(LANE_RELEASE_SOURCE_PREFERRED, LANE_RELEASE_SOURCE_LEGACY)
GAME_MODS = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods"
ZIP_OUTPUT = resolve_lane_path(LANE_DOWNLOAD_ZIPS_PREFERRED, LANE_DOWNLOAD_ZIPS_LEGACY)
QUOTES_DIR = os.path.join(VS_CODE_ROOT, "_Quotes")
LOGS_DIR = os.path.join(VS_CODE_ROOT, "Logs")
MAIN_LOG_MAX_FILES = 10

README_SYSTEM_ROOT = os.path.join(VS_CODE_ROOT, "Workflow", "ReadmeSystem")
README_TEMPLATE_ROOT = os.path.join(README_SYSTEM_ROOT, "Templates")
README_SNIPPETS_ROOT = os.path.join(README_SYSTEM_ROOT, "Snippets")
README_LONG_GUIDES_ROOT = os.path.join(README_SNIPPETS_ROOT, "LongGuides")
COMPAT_CSV = os.path.join(README_SYSTEM_ROOT, "Data", "HELPER_ModCompatibility.csv")
ABOUTME_GUIDE_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "ModReadme-ABOUTME-md-Snippet.md")
ABOUTME_MAIN_GUIDE_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "MainReadme-1-ABOUTME-md-Snippet.md")
MODSCOPE_GUIDE_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "ModReadme-MODSCOPE-md-Snippet.md")
HARMONY_REQUIREMENT_WARNING_TXT_SNIPPET_PATH = os.path.join(
    README_SNIPPETS_ROOT, "ModReadme-HARMONYWARNING-txt-Snippet.txt"
)
MODGUIDE_TEXT_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "ModReadme-MODGUIDE-txt-Snippet.txt")
MAINREADME_MODTYPE_GUIDE_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "MainReadme-MODTYPE-md-Snippet.md")
MODTYPE_GUIDE_TXT_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "ModReadme-MODTYPE-txt-Snippet.txt")
LANGUAGE_MAIN_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "MainReadme-LANGUAGE-md-Snippet.md")
MODGUIDE_MAIN_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "MainReadme-2-MODGUIDE-md-Snippet.md")
ASKFORHELP_MAIN_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "MainReadme-3-ASKFORHELP-md-Snippet.md")
SUPPORT_MAIN_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "MainReadme-4-SUPPORT-md-Snippet.md")
INSTALL_GUIDE_SNIPPET_PATH = os.path.join(README_LONG_GUIDES_ROOT, "ModReadme-INSTALL-md-Snippet.md")
REMOVAL_GUIDE_SNIPPET_PATH = os.path.join(README_LONG_GUIDES_ROOT, "ModReadme-REMOVAL-md-Snippet.md")
UPDATE_GUIDE_SNIPPET_PATH = os.path.join(README_LONG_GUIDES_ROOT, "ModReadme-UPDATE-md-Snippet.md")
BACKUP_GUIDE_SNIPPET_PATH = os.path.join(README_LONG_GUIDES_ROOT, "ModReadme-BACKUP-md-Snippet.md")
HELP_GUIDE_SNIPPET_PATH = os.path.join(README_SNIPPETS_ROOT, "ModReadme-HELP-md-Snippet.md")
ABOUTME_GUIDE_PLACEHOLDER = "{{ABOUTME_GUIDE_BODY}}"
ABOUTME_MAIN_GUIDE_PLACEHOLDER = "{{ABOUTME_MAIN_GUIDE_BODY}}"
MODSCOPE_GUIDE_PLACEHOLDER = "{{MODSCOPE_GUIDE_BODY}}"
HARMONY_REQUIREMENT_WARNING_PLACEHOLDER = "{{HARMONY_REQUIREMENT_WARNING}}"
SHORT_GUIDE_PLACEHOLDER = "{{MODGUIDE_TEXT_BODY}}"
LEGACY_SHORT_GUIDE_PLACEHOLDER = "{{SHORT_GUIDE_BODY}}"
MODTYPE_GUIDE_PLACEHOLDER = "{{MODTYPE_GUIDE_BODY}}"
LANGUAGE_MAIN_PLACEHOLDER = "{{LANGUAGE_MAIN_BODY}}"
MODGUIDE_MAIN_PLACEHOLDER = "{{MODGUIDE_MAIN_BODY}}"
ASKFORHELP_MAIN_PLACEHOLDER = "{{ASKFORHELP_MAIN_BODY}}"
SUPPORT_MAIN_PLACEHOLDER = "{{SUPPORT_MAIN_BODY}}"
INSTALL_GUIDE_PLACEHOLDER = "{{INSTALL_GUIDE_BODY}}"
REMOVAL_GUIDE_PLACEHOLDER = "{{REMOVAL_GUIDE_BODY}}"
UPDATE_GUIDE_PLACEHOLDER = "{{UPDATE_GUIDE_BODY}}"
BACKUP_GUIDE_PLACEHOLDER = "{{BACKUP_GUIDE_BODY}}"
HELP_GUIDE_PLACEHOLDER = "{{HELP_GUIDE_BODY}}"
OTHER_DETAILS_SECTION_PLACEHOLDER = "{{OTHER_DETAILS_SECTION}}"
CHANGELOG_BODY_PLACEHOLDER = "{{CHANGELOG_BODY}}"
TITLE_CARD_CALLOUT_PLACEHOLDER = "{{TITLE_CARD_CALLOUT_BLOCK}}"
SHORT_GUIDE_RAW_TOKEN = "__AGF_SHORT_GUIDE_RAW_BLOCK__"
MOD_README_TEMPLATE = os.path.join(README_TEMPLATE_ROOT, "TEMPLATE-ModReadMes.md")
MAIN_TEMPLATE_PATH = os.path.join(README_TEMPLATE_ROOT, "TEMPLATE-MainReadMe.md")
MAIN_MOD_CATEGORY_TEMPLATE_PATH = os.path.join(README_TEMPLATE_ROOT, "TEMPLATE-MainReadMe-1ModCategory")
MAIN_MOD_ENTRY_TEMPLATE_PATH = os.path.join(README_TEMPLATE_ROOT, "TEMPLATE-MainReadMe-2ModEntry")
GIGGLE_PACK_TEMPLATE_PATH = os.path.join(README_TEMPLATE_ROOT, "TEMPLATE-MainReadMe-0GigglePack")
CATEGORY_DESCRIPTIONS_PATH = os.path.join(README_TEMPLATE_ROOT, "TEMPLATE-CategoryDescriptions.md")
IMAGES_ROOT = os.path.join(VS_CODE_ROOT, "00_Images")
IMAGES_GENERATED_ROOT = os.path.join(IMAGES_ROOT, "_generated")
IMAGES_THUMBNAIL_ROOT = os.path.join(IMAGES_GENERATED_ROOT, "thumbnails")
DISCORD_TEMPLATE_PATH = os.path.join(VS_CODE_ROOT, "05_GigglePackReleaseData", "Discord", "TEMPLATE-DiscordUpdate.md")
MAIN_README_PATH = os.path.join(VS_CODE_ROOT, "README.md")

AGF_PREFIXES = ("AGF-", "zzzAGF-")
BASE_DOWNLOAD_URL = "https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips"
BACKPACK_DEFAULT_ACTIVE_TOKEN = "084Slots"
GAME_OPTIONALS_BACKPACK_DIR = ".Optionals-Backpack"
GAME_OPTIONALS_HUDPLUS_DIR = ".Optionals-HUDPlus"
GAME_OPTIONALS_4MODDERS_DIR = ".Optionals-4Modders"
GAME_OPTIONALS_REQUESTED_DIR = ".Optionals-Requested"
RELEASE_META_DIR_NAME = ".release"
GIGGLEPACK_RELEASE_DATA_DIR = "GigglePack"
GIGGLEPACK_RELEASE_ROOT_DIR = os.path.join(VS_CODE_ROOT, "05_GigglePackReleaseData")
GIGGLEPACK_PENDING_CHANGES_PATH = os.path.join(
    GIGGLEPACK_RELEASE_ROOT_DIR,
    "gigglepack-pending-changes.json",
)
RUN_LOCK_PATH = os.path.join(VS_CODE_ROOT, ".script-main.lock")
RUN_MANIFEST_MAX_FILES = 20
GAME_REMOVALS_QUARANTINE_DIR = os.path.join(VS_CODE_ROOT, "_Quarantine-GameRemovals")
TRANSACTION_ROLLBACK_DIR = os.path.join(VS_CODE_ROOT, "_TransactionRollback")
DRAFT_PROMOTION_BASELINE_PATH = os.path.join(VS_CODE_ROOT, "Workflow", "draft-promotion-baseline.json")
GIGGLEPACK_CANONICAL_ZIP = "00_GigglePack_All.zip"
GIGGLEPACK_VERSIONED_ZIP_PREFIX = "AGF-GigglePack-v"
LEGACY_FINAL_GIGGLEPACK_ZIP = "AGF-7d2d-v2.6-GigglePack-Final.zip"
LEGACY_FINAL_CATEGORY_KEY = "AGF 7d2d v2.6 GigglePack FINAL"
GIGGLEPACK_BASELINE_VERSION = "0.1.0"
GIGGLEPACK_MAJOR_BUMP_MARKER = "gigglepack-major-bump.txt"
DISCORD_WEBHOOK_ENV_VAR = "AGF_DISCORD_WEBHOOK_URL"
GIGGLEPACK_V100_FOCUS_MODS = (
    "AGF-NoEAC-ExpandedInteractionPrompts",
    "AGF-NoEAC-ScreamerAlert",
)
# Defender has repeatedly false-flagged DEFLATED output for this specific mod zip.
ZIP_STORED_MOD_BASES = {
    "AGF-NoEAC-AutoRun",
}
README_COMPAT_FIELDS = (
    "TESTED_GAME_VERSION",
    "EAC_FRIENDLY",
    "SERVER_SIDE_PLAYER",
    "SERVER_SIDE_DEDICATED",
    "CLIENT_SIDE",
    "MOD_TYPE_ID",
    "SAFE_TO_INSTALL",
    "SAFE_TO_REMOVE",
    "UNIQUE",
    "QUOTE_FILE",
)

COMPAT_CSV_FIELD_ORDER = [
    "MOD_NAME",
    "MOD_TYPE_ID",
    "TESTED_GAME_VERSION",
    "SAFE_TO_INSTALL",
    "SAFE_TO_REMOVE",
    "UNIQUE",
    "QUOTE_FILE",
    "EAC_FRIENDLY",
    "SERVER_SIDE_PLAYER",
    "SERVER_SIDE_DEDICATED",
    "CLIENT_SIDE",
]

DEFAULT_MOD_TYPE_LINE_BY_ID = {
    "1": "Server-side (EAC-friendly): Server install works for all joining players; EAC on or off. (Also works in singleplayer.)",
    "2": "Server-side (EAC Off): EAC off required; server install works for all joining players. (Also works in singleplayer.)",
    "3": "Server/Client-side (Required): EAC off required; host and joining players must install it. (Also works in singleplayer.)",
    "4": "Client-side (Only): EAC off required; server install has no effect; install on each player PC. (Also works in singleplayer.)",
}

MOD_TYPE_COMPAT_BY_ID = {
    "1": {
        "EAC_FRIENDLY": "Yes",
        "SERVER_SIDE_PLAYER": "Yes",
        "SERVER_SIDE_DEDICATED": "Yes",
        "CLIENT_SIDE": "None",
    },
    "2": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "Yes",
        "SERVER_SIDE_DEDICATED": "Yes",
        "CLIENT_SIDE": "None",
    },
    "3": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "No",
        "SERVER_SIDE_DEDICATED": "No",
        "CLIENT_SIDE": "Required",
    },
    "4": {
        "EAC_FRIENDLY": "No",
        "SERVER_SIDE_PLAYER": "N/A",
        "SERVER_SIDE_DEDICATED": "N/A",
        "CLIENT_SIDE": "Only",
    },
}

FAIL_FAST_ENABLED = True

DEFAULT_ABOUTME_GUIDE_BODY = """## 1. About AGF
- AuroraGiggleFairy (AGF) creates accessibility-focused, vanilla-enhancing mods for 7 Days to Die.
- Goal is to deliver practical, easy-to-use features shaped by community feedback.
- Main site and first release source: [auroragigglefairy.github.io](https://auroragigglefairy.github.io/)
- Discord is best for latest updates, fastest contact, and becoming a tester: [discord.gg/Vm5eyW6N4r](https://discord.gg/Vm5eyW6N4r)

---
---"""

DEFAULT_ABOUTME_MAIN_GUIDE_BODY = """- My name is AuroraGiggleFairy (AGF).
- Making accessibility-focused and vanilla enhancing mods for 7 Days to Die since 2019.

- I have been modding 7 Days to Die for 7 years.
- I do my best to prioritize accessibility, user-friendliness, and localization where possible.
- I provide kind, comprehensive support to players, modders, and server communities, and I rely on community feedback to keep improving my mods."""

DEFAULT_MODTYPE_GUIDE_BODY = """---

> *This guide explains where to install a mod and whether it is EAC friendly.*

---

### A. Install on Server, Client, or Both?
>   - **Server** - where the game is hosted. This could be your own PC if you are hosting the game yourself, or a game hosting service like Pingperfect.
>   - **Client** - your own PC.
> - Some mods only need to be in one place. Others need to be in both. Please see the Mod Types section below for exact requirements.

---

### B. EAC Friendly?

> - EAC stands for **Easy Anti-Cheat**. It's a program built into 7 Days to Die that helps protect multiplayer sessions from cheating.
> - Mod Type 1 is EAC friendly. Mod Types 2, 3, and 4 require EAC to be turned off.
> - Running without EAC opens up a wider range of mods and experiences. If you're running a multiplayer server without EAC, here are some good practices to keep things running smoothly:
>   - **Recommended practices when running multiplayer with EAC off:**
>     - Require a password and be conservative in distributing it.
>     - There are tools available such as ALOC and Server Tools to add extra protections.
>     - Optionally, require the whitelist system for the strictest limitation on who can join.
>     - Seek out other server hosts and discuss what they do.
>     - You can find help on AGF's Discord: [DISCORD](https://discord.gg/Vm5eyW6N4r)

---

### C. Mod Types

> - Mod Type 1 is **Server-Side (EAC-Friendly)**: server install works for all joining players, EAC can be on or off, and it also works in singleplayer.
> - Mod Type 2 is **Server-Side (EAC Off)**: EAC off is required, server install works for all joining players, and it also works in singleplayer.
> - Mod Type 3 is **Server/Client-Side (Required)**: EAC off is required, the host and all joining players must install it, and it also works in singleplayer.
> - Mod Type 4 is **Client-Side (Only)**: EAC off is required, server install has no effect, each player installs it on their own PC, and it also works in singleplayer.
"""

DEFAULT_MODGUIDE_MAIN_BODY = """### A. Install Mods
1. Close the game.
2. In Steam, right-click `7 Days to Die` -> `Manage` -> `Browse local files`, then open `Mods`.
3. Extract the zip into the `Mods` folder. Make sure it ends up as `Mods/<ModName>/ModInfo.xml`.
4. Restart the game.

---

### B. Ask AuroraGiggleFairy for Help
1. Join AGF's Discord: [DISCORD](https://discord.gg/Vm5eyW6N4r).
     - AGF checks website messages often, but Discord is the fastest and best way to get help.
2. Find `#help-is-here` under the `NEED HELP?` section.
     - All questions are welcome, whether you are new or experienced.
     - This includes mod conflicts, features not working as expected, server or admin issues, translation errors, and other mod-related problems.
3. Post your help request in `#help-is-here`.
     - Share a brief message about what is happening.
     - Attach your latest log file.
         - Enter the game, then press `F1` to open the console.
         - Click `Open logs folder` in the top-right.
         - The correct log file should already be selected. Drag and drop it into `#help-is-here`.
     - A screenshot can also help.
         - Use `PrtSc` (Print Screen) or your system screenshot tool, then paste the image into Discord chat.
     - If preferred, DMs are open and you are welcome to message AGF directly.

---

### C. Backups
- To create:
    - Open `%appdata%` -> `Roaming` -> `7DaysToDie` -> `Saves`, then open your World Name folder (for example, `Navezgane`).
    - Copy your Game Name folder (for example, `MyGame`) to a safe place.
- To restore:
    - Copy that saved Game Name folder back into the same World Name folder in `Saves`.
    - Replace the current folder if asked.

---

### D. Update Mods
1. Close the game.
2. Make a backup first (see section C).
3. Install the new version in `Mods`.
     - If asked, allow overwrite or replace.
4. If both old and new folders are there, keep the newer one and delete the older one.
5. Start the game and confirm your save loads.

---

### E. Remove Mods
- Warning: Removing a mod from an active save can destroy your saved game. Back up first.
- Never delete `0_TFP_Harmony`; it comes with the game.
1. Close the game.
2. In `Mods`, delete each mod folder you are removing, except `0_TFP_Harmony`.

---

### F. The `0_TFP_Harmony` Mod (Do Not Remove)
- Never delete `0_TFP_Harmony`; it comes with the game.
- If it is missing, restore it by verifying game files in Steam:
    1. In Steam, right-click 7 Days to Die.
    2. Select Properties.
    3. Select Installed Files.
    4. Click Verify integrity of game files and wait for completion.

---

### G. EAC
- EAC stands for Easy Anti-Cheat and helps protect multiplayer sessions from cheating.
- Some mods require EAC to be turned off so they can work.
- How to launch 7 Days to Die with EAC off:
    1. In Steam Library, select 7 Days to Die.
    2. Click Play.
    3. In the launch popup, select Launch game without EAC.
    4. Click Play.
- If the launch popup does not appear:
    1. In Steam Library, select 7 Days to Die.
    2. Click the gear icon on the right, then click Properties.
    3. Under Launch Options, open the Selected Launch Option dropdown.
    4. Choose Ask when starting game or Launch game without EAC."""

DEFAULT_ASKFORHELP_MAIN_BODY = """1. Join AGF's Discord: [DISCORD](https://discord.gg/Vm5eyW6N4r).
    - AGF checks website messages often, but Discord is the fastest and best way to get help.
2. Find `#help-is-here` under the `NEED HELP?` section.
    - All questions are welcome, whether you are new or experienced.
    - This includes mod conflicts, features not working as expected, server or admin issues, translation errors, and other mod-related problems.
3. Post your help request in `#help-is-here`.
    - Share a brief message about what is happening.
    - Attach your latest log file.
      - Enter the game, then press `F1` to open the console.
      - Click `Open logs folder` in the top-right.
      - The correct log file should already be selected. Drag and drop it into `#help-is-here`.
    - A screenshot can also help.
      - Use `PrtSc` (Print Screen) or your system screenshot tool, then paste the image into Discord chat.
    - If preferred, DMs are open and you are welcome to message AGF directly."""

DEFAULT_LANGUAGE_MAIN_BODY = """### A. What languages do AGF mods support?
- 7 Days to Die currently supports 13 languages: English, German, Spanish, French, Italian, Japanese, Korean, Polish, Portuguese, Russian, Turkish, Simplified Chinese, and Traditional Chinese.
- AGF mods add support for all 13 languages.
- If you find a translation error, please let AGF know on [DISCORD](https://discord.gg/Vm5eyW6N4r)."""

DEFAULT_SUPPORT_MAIN_BODY = """- I have been actively creating and supporting 7 Days to Die mods since Alpha 18 (2019), and I genuinely love doing this work.
- I spend a lot of time fixing complex issues, keeping everything up to date, and helping players, modders, and server communities.
- If my work helps you, here are ways to support me:
    - Help spread my mods by sharing them with others, creating content, or sharing my GitHub link: https://auroragigglefairy.github.io/
    - Join my Discord to share feedback, keep up with updates, or volunteer as a tester: https://discord.gg/Vm5eyW6N4r
    - Support me on Twitch: https://www.twitch.tv/auroragigglefairy
    - Need hosting? Use my PingPerfect Referral Link: https://pingperfect.com/aff.php?aff=1834
    - Support me directly by donating to my PayPal: https://www.paypal.com/donate/?hosted_button_id=3B7BCQAZ6KHXC
- I genuinely appreciate your support.
- Support is always optional and never expected.
- It encourages me to keep investing time and energy into this work.
- From the bottom of my heart, thank you."""

DEFAULT_INSTALL_GUIDE_BODY = """---

> **IMPORTANT** Make a BACKUP!
> - If you are making changes to an existing game, ALWAYS make a backup first.
> - *(See the backup instructions further below.)*

---

### A. Special Note
> - The mod named **\"0_TFP_Harmony\"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - *In Steam, right click on `7 Days to Die`*
>    - *Select \"Properties\"*
>    - *Select \"Installed Files\"*
>    - *Click on \"Verify integrity of game files\" and wait for completion.*

---

### B. Install to Your PC (Singleplayer, Hosting Friends, or Client-Required Mods)
> 1. **In Steam, right click `7 Days to Die`**
> 2. **Select `Manage`, then `Browse local files`**
> 3. **Open the `Mods` folder**
> 4. **Extract the mod into this `Mods` folder**
>    - *Final folder should look like: `Mods/<ModFolder>/ModInfo.xml`*
>    - *If the zip creates an extra parent folder, move the mod folder up one level*
> 5. **Keep `0_TFP_Harmony` in this folder**
>    - *It comes with the game and should remain in `Mods`*

---

### C. Install to a Dedicated Server *(hosted sites or player-run)*
> 1. **Find the main server folder**
>    - *If using a hosted site, use their file manager or a program like FileZilla*
> 2. **Open or create the `Mods` folder**
> 3. **Extract the mod into this `Mods` folder**
>    - *Final folder should look like: `Mods/<ModFolder>/ModInfo.xml`*
>    - *If the zip creates an extra parent folder, move the mod folder up one level*
> 4. **Fully restart the server after install**
"""

DEFAULT_REMOVAL_GUIDE_BODY = """---

> **IMPORTANT** Make a BACKUP First!

---

### A. Special Note
> - The mod named **\"0_TFP_Harmony\"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - *In Steam, right click on `7 Days to Die`*
>    - *Select \"Properties\"*
>    - *Select \"Installed Files\"*
>    - *Click on \"Verify integrity of game files\" and wait for completion.*

---

### B. How do I remove mods?
> - The safest approach is to only remove mods when starting a new game.
> - Never delete `0_TFP_Harmony`; it comes with the game.
> - ALSO smart to make a backup of the game (see below for instructions if needed).
>     - If you remove a mod that added new items or features, characters and/or the map may reset, or become permanently unplayable.
>     - If you are unsure, check the mod's readme for specific removal notes or ask in AGF's [DISCORD](https://discord.gg/Vm5eyW6N4r).
> - To remove, simply locate the mod folder and delete it. All done!
"""

DEFAULT_UPDATE_GUIDE_BODY = """---

> **IMPORTANT** Make a BACKUP First!

---

### A. Special Note
> - The mod named **\"0_TFP_Harmony\"** is ***REQUIRED*** and should never be removed.
> - If it is missing, you can restore it by verifying your installation:
>    - *In Steam, right click on 7 Days to Die*
>    - *Select \"Properties\"*
>    - *Select \"Installed Files\"*
>    - *Click on \"Verify integrity of game files\" and wait for completion.*

---

### B. Updating individual AGF Mods
> 1. **Shutdown the game.**
> 2. **Make a backup.** *(instructions below)*
> 3. **Install the new version as usual.** *(see above)*
> 4. **Delete the older version.**
>    - *In your Mods folder, you will see two folders for the mod, each with its version number.*
> 5. **THEN** you may turn the game back on.

---

### C. Updating entire AGF Pack
> 1. **Shutdown the game.**
> 2. **Make a backup.** *(instructions below)*
> 3. **Carefully delete all AGF mods.** *(don't forget the zzzAGF mod)*
> 4. **Install the new package as usual.** *(see above)*
> 5. **THEN** you may turn the game back on.
"""

DEFAULT_BACKUP_GUIDE_BODY = """---

> *Having multiple backups are best when experimenting or hosting.*

---

### A. Backing Up Your Singleplayer or Local Game
> 1. **Open \"Run\"** *(Windows key + R)*
> 2. **Type `%appdata%` and click OK**
> 3. **Open the \"Roaming\" folder**
> 4. **Open \"7DaysToDie\"**
> 5. **Open \"Saves\"**
> 6. **Find your World Name folder** *(e.g., \"Navezgane\")*
>    - *You can check your world name in the game's \"Continue Game\" or \"Join Server\" menu.*
> 7. **Select your Game Name folder** *(inside the World Name folder)*
>    - *The Game Name is also shown in the game menus.*
> 8. **Copy the entire Game Name folder and paste it somewhere safe**
>    - *(like your Desktop or another safe location)*.

---

### B. Dedicated Server Game
> 1. **Open the server's \"Saves\" folder**
>    - *Its location varies by host, but it's typically somewhere in the main dedicated server folder.*
>    - *If you cannot find it, check your host's documentation or ask support.*
> 2. **Find your World Name folder** *(e.g., \"Navezgane\")*
>    - *You can check your world name in the game's \"Join Server\" menu or \"Server Config\".*
> 3. **Select your Game Name folder** *(inside the World Name folder)*
>    - *The Game Name is also shown in the game's \"Join Server\" menu or \"Server Config\".*
> 4. **Copy the entire Game Name folder and paste it somewhere safe**
>    - *(like your Desktop or another safe location)*.

---

### C. To Restore a Backup
> - *Keep your backup until you're sure everything works!*
> 1. **Undo any mod changes**
> 2. **Delete the current Game Name folder** *(in \"Saves\")*
> 3. **Move your backup folder back into \"Saves\"**
"""

DEFAULT_HELP_GUIDE_BODY = """> 1. **Join AGF's Discord:** [DISCORD](https://discord.gg/Vm5eyW6N4r).
>    - AGF checks website messages often, but Discord is the fastest and best way to get help.
> 2. **Find `#help-is-here`** under the `NEED HELP?` section.
>    - All questions are welcome, whether you are new or experienced.
>    - This includes mod conflicts, features not working as expected, server or admin issues, translation errors, and other mod-related problems.
> 3. **Post your help request in `#help-is-here`:**
>    - Share a brief message about what is happening.
>    - Attach your latest log file.
>      - *Enter the game, then press `F1` to open the console.*
>      - *Click `Open logs folder` in the top-right.*
>      - *The correct log file should already be selected. Drag and drop it into `#help-is-here`.*
>    - A screenshot can also help.
>      - *Use `PrtSc` (Print Screen) or your system screenshot tool, then paste the image into Discord chat.*
> - *If preferred, DMs are open and you are welcome to message AGF directly.*
"""

DEFAULT_MODSCOPE_GUIDE_BODY = """- Mod Version: {{MOD_VERSION}}
- 7d2d Version: {{TESTED_GAME_VERSION}}
- Website: https://auroragigglefairy.github.io/
{{MOD_TYPE_BLOCK}}
- Safe to install on existing game: {{SAFE_TO_INSTALL}}
- Safe to remove from existing game: {{SAFE_TO_REMOVE}}
{{DEPENDENCIES_BLOCK}}"""

DEFAULT_HARMONY_REQUIREMENT_WARNING_BODY = """- Harmony requirement details:
    - Requires 0_TFP_Harmony (built-in game mod).
    - To check, confirm Mods/0_TFP_Harmony exists in the game folder.
    - To restore, run Steam Verify integrity of game files."""

DEFAULT_SHORT_GUIDE_BODY = """## A. Install Mods
> 1. **Close the game.**
> 2. **In Steam, right-click `7 Days to Die` -> `Manage` -> `Browse local files`, then open `Mods`.**
> 3. **Extract the zip into the `Mods` folder. Make sure it ends up as `Mods/<ModName>/ModInfo.xml`.**
> 4. **Restart the game.**"""


@dataclass
class RunTransaction:
    enabled: bool
    rollback_root: str
    actions: List[Dict[str, str]] = field(default_factory=list)
    rolling_back: bool = False


CURRENT_TRANSACTION: Optional[RunTransaction] = None


def _build_mod_type_line(mod_type_name: str, detail_lines: List[str]) -> str:
    name = (mod_type_name or "").strip()
    details = [d.strip() for d in detail_lines if (d or "").strip()]
    if not details:
        return name
    return f"{name}: {'; '.join(details)}"


def _parse_mod_type_lines_from_txt_snippet(text: str) -> Dict[str, str]:
    parsed: Dict[str, str] = {}
    current_id = ""
    current_name = ""
    current_details: List[str] = []

    def commit_current() -> None:
        nonlocal current_id, current_name, current_details
        if current_id and current_name:
            parsed[current_id] = _build_mod_type_line(current_name, current_details)
        current_id = ""
        current_name = ""
        current_details = []

    for raw_line in text.splitlines():
        line = raw_line.strip()
        if not line:
            continue

        header = re.match(r"^MOD\s*TYPE\s+(\d+)\s*$", line, flags=re.IGNORECASE)
        if header:
            commit_current()
            current_id = header.group(1)
            continue

        if not current_id:
            continue

        name_match = re.match(r"^-\s*Mod\s*Type\s*:\s*(.+?)\s*$", line, flags=re.IGNORECASE)
        if name_match:
            current_name = name_match.group(1).strip()
            continue

        detail_match = re.match(r"^-\s*(.+?)\s*$", line)
        if detail_match:
            detail = detail_match.group(1).strip()
            if not detail:
                continue
            if re.match(r"^Mod\s*Type\s*:", detail, flags=re.IGNORECASE):
                continue
            current_details.append(detail)

    commit_current()
    return parsed


def _parse_mod_type_lines_from_md_table(text: str) -> Dict[str, str]:
    parsed: Dict[str, str] = {}
    for raw_line in text.splitlines():
        line = raw_line.strip()
        if not line.startswith("|"):
            continue

        parts = [part.strip() for part in line.strip("|").split("|")]
        if len(parts) < 3:
            continue
        mod_type_id = parts[0]
        mod_type_name = parts[1]
        wording = parts[2]

        if not mod_type_id.isdigit():
            continue
        if mod_type_name in {"Mod Type", "----------"}:
            continue

        parsed[mod_type_id] = f"{mod_type_name}: {wording}"

    return parsed


def load_mod_type_lines_from_modtype_guide(log: "Logger") -> Dict[str, str]:
    """Load MOD_TYPE_ID wording map.

    Preferred source:
    - ModReadme-MODTYPE-txt-Snippet.txt in MOD TYPE N bullet-block format.

    Backward-compatible fallback:
    - legacy ModReadme-MODTYPE-md-Snippet.md in markdown table format.
    """
    if os.path.isfile(MODTYPE_GUIDE_TXT_SNIPPET_PATH):
        try:
            with open(MODTYPE_GUIDE_TXT_SNIPPET_PATH, "r", encoding="utf-8") as f:
                txt = f.read()
            parsed_txt = _parse_mod_type_lines_from_txt_snippet(txt)
            if parsed_txt:
                return parsed_txt
            log.warn(
                "No MOD_TYPE entries parsed from ModReadme-MODTYPE-txt-Snippet.txt; "
                "falling back to md snippet/default wording"
            )
        except Exception as ex:
            log.warn(
                f"Failed to read {MODTYPE_GUIDE_TXT_SNIPPET_PATH}: {ex}; "
                "falling back to md snippet/default wording"
            )

    log.warn(
        "Missing/invalid mod type snippets; using built-in MOD_TYPE_ID wording defaults"
    )
    return dict(DEFAULT_MOD_TYPE_LINE_BY_ID)


def _parse_mainreadme_mod_type_lines_from_md_snippet(text: str) -> Dict[str, str]:
    parsed: Dict[str, str] = {}
    pending_mod_type_id = ""

    for raw_line in text.splitlines():
        line = raw_line.strip()
        if not line:
            continue

        # Allow quoted markdown list lines such as: > - Mod Type 1 is **...**: ...
        line = re.sub(r"^>\s*", "", line)

        # New format support:
        # MOD TYPE 1
        # - **Server-Side (EAC-Friendly)**: ...
        header_match = re.match(r"^MOD\s*TYPE\s*(\d+)\s*$", line, flags=re.IGNORECASE)
        if header_match:
            pending_mod_type_id = header_match.group(1)
            continue

        bullet_sentence_match = re.match(r"^-\s*\*\*(.+?)\*\*\s*:\s*(.+)$", line)
        if bullet_sentence_match and pending_mod_type_id:
            mod_type_name = bullet_sentence_match.group(1).strip()
            wording = bullet_sentence_match.group(2).strip()
            if mod_type_name and wording:
                parsed[pending_mod_type_id] = f"{mod_type_name}: {wording}"
            pending_mod_type_id = ""
            continue

        sentence_match = re.match(
            r"^-?\s*Mod\s*Type\s*(\d+)\s*is\s*(.+)$",
            line,
            flags=re.IGNORECASE,
        )
        if not sentence_match:
            continue

        mod_type_id = sentence_match.group(1)
        remainder = sentence_match.group(2).strip()

        bold_match = re.match(r"^\*\*(.+?)\*\*\s*:\s*(.+)$", remainder)
        if bold_match:
            mod_type_name = bold_match.group(1).strip()
            wording = bold_match.group(2).strip()
        else:
            parts = [p.strip() for p in remainder.split(":", 1)]
            if len(parts) == 2:
                mod_type_name, wording = parts
            else:
                # If no colon is present, keep the whole sentence as wording.
                mod_type_name = "Mod Type"
                wording = remainder

        if not mod_type_name or not wording:
            continue

        parsed[mod_type_id] = f"{mod_type_name}: {wording}"

    return parsed


def load_mainreadme_mod_type_lines(log: "Logger") -> Dict[str, str]:
    """Load MOD_TYPE_ID wording for main README mod cards.

    Preferred source:
    - MainReadme-MODTYPE-md-Snippet.md sentence-form lines.

    Fallback:
    - Per-mod MODTYPE map loader (txt snippet + defaults).
    """
    if os.path.isfile(MAINREADME_MODTYPE_GUIDE_SNIPPET_PATH):
        try:
            with open(MAINREADME_MODTYPE_GUIDE_SNIPPET_PATH, "r", encoding="utf-8") as f:
                md = f.read()
            parsed = _parse_mainreadme_mod_type_lines_from_md_snippet(md)
            if parsed:
                return parsed
            log.warn(
                "No sentence-form MOD TYPE lines parsed from MainReadme-MODTYPE-md-Snippet.md; "
                "falling back to per-mod MODTYPE source"
            )
        except Exception as ex:
            log.warn(
                f"Failed to read {MAINREADME_MODTYPE_GUIDE_SNIPPET_PATH}: {ex}; "
                "falling back to per-mod MODTYPE source"
            )

    return load_mod_type_lines_from_modtype_guide(log)


def load_readme_snippet(
    snippet_path: str,
    snippet_label: str,
    default_text: str,
    log: Optional["Logger"] = None,
) -> str:
    if not os.path.isfile(snippet_path):
        if log:
            log.warn(
                f"{snippet_label} snippet not found: {snippet_path}; "
                f"using built-in {snippet_label.lower()} text"
            )
        return default_text

    try:
        with open(snippet_path, "r", encoding="utf-8") as f:
            text = f.read().strip()
    except Exception as ex:
        if log:
            log.warn(
                f"Failed reading {snippet_label.lower()} snippet {snippet_path}: {ex}; "
                f"using built-in {snippet_label.lower()} text"
            )
        return default_text

    if not text:
        if log:
            log.warn(
                f"{snippet_label} snippet is empty: {snippet_path}; "
                f"using built-in {snippet_label.lower()} text"
            )
        return default_text

    return text


def load_install_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        INSTALL_GUIDE_SNIPPET_PATH,
        "Install guide",
        DEFAULT_INSTALL_GUIDE_BODY,
        log,
    )


def load_aboutme_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        ABOUTME_GUIDE_SNIPPET_PATH,
        "About Me guide",
        DEFAULT_ABOUTME_GUIDE_BODY,
        log,
    )

def load_aboutme_main_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        ABOUTME_MAIN_GUIDE_SNIPPET_PATH,
        "About Me main guide",
        DEFAULT_ABOUTME_MAIN_GUIDE_BODY,
        log,
    )


def load_modtype_guide_body(log: Optional["Logger"] = None) -> str:
    if os.path.isfile(MAINREADME_MODTYPE_GUIDE_SNIPPET_PATH):
        return load_readme_snippet(
            MAINREADME_MODTYPE_GUIDE_SNIPPET_PATH,
            "Mod Type guide",
            DEFAULT_MODTYPE_GUIDE_BODY,
            log,
        )

    return load_readme_snippet(
        MAINREADME_MODTYPE_GUIDE_SNIPPET_PATH,
        "Mod Type guide",
        DEFAULT_MODTYPE_GUIDE_BODY,
        log,
    )


def load_modguide_main_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        MODGUIDE_MAIN_SNIPPET_PATH,
        "Main AGF guide",
        DEFAULT_MODGUIDE_MAIN_BODY,
        log,
    )


def load_askforhelp_main_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        ASKFORHELP_MAIN_SNIPPET_PATH,
        "Main ask-for-help guide",
        DEFAULT_ASKFORHELP_MAIN_BODY,
        log,
    )


def load_language_main_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        LANGUAGE_MAIN_SNIPPET_PATH,
        "Main language support guide",
        DEFAULT_LANGUAGE_MAIN_BODY,
        log,
    )


def load_support_main_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        SUPPORT_MAIN_SNIPPET_PATH,
        "Main support guide",
        DEFAULT_SUPPORT_MAIN_BODY,
        log,
    )


def load_removal_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        REMOVAL_GUIDE_SNIPPET_PATH,
        "Removal guide",
        DEFAULT_REMOVAL_GUIDE_BODY,
        log,
    )


def load_update_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        UPDATE_GUIDE_SNIPPET_PATH,
        "Update guide",
        DEFAULT_UPDATE_GUIDE_BODY,
        log,
    )


def load_backup_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        BACKUP_GUIDE_SNIPPET_PATH,
        "Backup guide",
        DEFAULT_BACKUP_GUIDE_BODY,
        log,
    )


def load_help_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        HELP_GUIDE_SNIPPET_PATH,
        "Help guide",
        DEFAULT_HELP_GUIDE_BODY,
        log,
    )


def load_modscope_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        MODSCOPE_GUIDE_SNIPPET_PATH,
        "Mod Scope guide",
        DEFAULT_MODSCOPE_GUIDE_BODY,
        log,
    )


def load_harmony_requirement_warning_body(log: Optional["Logger"] = None) -> str:
    if os.path.isfile(HARMONY_REQUIREMENT_WARNING_TXT_SNIPPET_PATH):
        return load_readme_snippet(
            HARMONY_REQUIREMENT_WARNING_TXT_SNIPPET_PATH,
            "Harmony requirement warning",
            DEFAULT_HARMONY_REQUIREMENT_WARNING_BODY,
            log,
        )

    return DEFAULT_HARMONY_REQUIREMENT_WARNING_BODY


def load_short_guide_body(log: Optional["Logger"] = None) -> str:
    return load_readme_snippet(
        MODGUIDE_TEXT_SNIPPET_PATH,
        "Mod guide text",
        DEFAULT_SHORT_GUIDE_BODY,
        log,
    )


# =============================================================
# LOGGING
# =============================================================
@dataclass
class RunStats:
    scanned_workspace_mods: int = 0
    scanned_game_mods: int = 0
    synced_pull_from_game: int = 0
    synced_push_to_game: int = 0
    sync_conflicts: int = 0
    moved_to_publish_ready: int = 0
    moved_to_in_progress: int = 0
    renamed_folders: int = 0
    csv_added_rows: int = 0
    csv_removed_rows: int = 0
    quote_files_created: int = 0
    quote_files_renamed: int = 0
    quote_files_blanked_none: int = 0
    readmes_written: int = 0
    readable_txt_written: int = 0
    pushed_back_to_game: int = 0
    mod_zips_created: int = 0
    pack_zips_created: int = 0
    promoted_to_publish_ready: int = 0
    warnings: int = 0
    errors: int = 0


class Logger:
    def __init__(self, verbose: bool = False, dry_run: bool = False) -> None:
        self.verbose = verbose
        self.dry_run = dry_run
        self.stats = RunStats()
        self._messages: List[str] = []
        self._action_needed: List[str] = []
        self._lock = threading.Lock()

    def _emit(self, level: str, message: str) -> None:
        ts = dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        line = f"[{ts}] [{level}] {message}"
        with self._lock:
            self._messages.append(line)
        if level in ("WARN", "ERROR") or self.verbose:
            print(line)

    def info(self, message: str) -> None:
        self._emit("INFO", message)

    def warn(self, message: str) -> None:
        self.stats.warnings += 1
        self._emit("WARN", message)

    def error(self, message: str) -> None:
        self.stats.errors += 1
        self._emit("ERROR", message)

    def action_needed(self, message: str) -> None:
        with self._lock:
            if message not in self._action_needed:
                self._action_needed.append(message)
        self.warn(f"ACTION NEEDED: {message}")

    def get_action_needed_lines(self) -> List[str]:
        with self._lock:
            return list(self._action_needed)

    def _extract_mod_changes(self) -> List[Tuple[str, str]]:
        """Build a deduplicated list of mod changes detected in this run's log messages."""
        action_by_mod: Dict[str, List[str]] = {}

        action_map: List[Tuple[re.Pattern[str], str]] = [
            (re.compile(r"sync-work repair:"), "repaired in game"),
            (re.compile(r"sync-work push:"), "pushed to game"),
            (re.compile(r"sync-work pull:"), "pulled from game"),
            (re.compile(r"sync-work tie resolved"), "tie resolved (game updated)"),
            (re.compile(r"Promoted new mod:"), "promoted (new)"),
            (re.compile(r"Promoted update:"), "promoted (updated)"),
            (re.compile(r"Promoted refresh:"), "promoted (refresh)"),
            (re.compile(r"Moved to PublishReady:"), "moved to publish-ready"),
            (re.compile(r"Moved to In-Progress:"), "moved to in-progress"),
            (re.compile(r"Workspace is newer for"), "workspace newer (pushed to game)"),
            (re.compile(r"Game is newer for"), "game newer (pulled to workspace)"),
            (re.compile(r"Version tie resolved for"), "version tie resolved"),
        ]

        mod_pattern = re.compile(r"\b(AGF-[A-Za-z0-9-]+-v[0-9][0-9A-Za-z\.-]*)\b")

        for line in self._messages:
            message_match = re.search(r"\[[^\]]+\]\s\[[^\]]+\]\s(.+)$", line)
            message = message_match.group(1) if message_match else line

            action = None
            for pattern, label in action_map:
                if pattern.search(message):
                    action = label
                    break
            if not action:
                continue

            mod_match = mod_pattern.search(message)
            if not mod_match:
                continue

            mod_name = mod_match.group(1)
            if mod_name not in action_by_mod:
                action_by_mod[mod_name] = []
            if action not in action_by_mod[mod_name]:
                action_by_mod[mod_name].append(action)

        changes: List[Tuple[str, str]] = []
        for mod_name in sorted(action_by_mod.keys(), key=lambda name: name.lower()):
            actions = ", ".join(action_by_mod[mod_name])
            changes.append((mod_name, actions))
        return changes

    def get_mod_change_summary_lines(self) -> List[str]:
        changes = self._extract_mod_changes()
        if not changes:
            return ["none"]
        return [f"{mod_name} | {actions}" for mod_name, actions in changes]

    def write_log_file(self) -> Optional[str]:
        try:
            os.makedirs(LOGS_DIR, exist_ok=True)
            stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
            mode = "dryrun" if self.dry_run else "live"
            path = os.path.join(LOGS_DIR, f"main-script-{mode}-{stamp}.log")
            lines: List[str] = []
            lines.extend(self._messages)
            lines.append("")
            lines.append("=== SUMMARY ===")
            for key, value in self.stats.__dict__.items():
                lines.append(f"{key}: {value}")
            lines.append("")
            lines.append("=== MOD CHANGES ===")
            lines.extend(self.get_mod_change_summary_lines())
            atomic_write_text(path, "\n".join(lines) + "\n", encoding="utf-8")

            # Keep only the newest main-script logs to avoid unbounded growth.
            log_candidates: List[Tuple[float, str]] = []
            for name in os.listdir(LOGS_DIR):
                if not (name.startswith("main-script-") and name.endswith(".log")):
                    continue
                full_path = os.path.join(LOGS_DIR, name)
                if os.path.isfile(full_path):
                    log_candidates.append((os.path.getmtime(full_path), full_path))

            log_candidates.sort(key=lambda item: item[0], reverse=True)
            for _, old_path in log_candidates[MAIN_LOG_MAX_FILES:]:
                try:
                    os.remove(old_path)
                except Exception:
                    pass

            return path
        except Exception:
            return None


def atomic_write_text(path: str, content: str, encoding: str = "utf-8", newline: Optional[str] = None) -> None:
    """Write file content atomically to reduce partial-write corruption risk."""
    target_dir = os.path.dirname(path)
    if target_dir:
        os.makedirs(target_dir, exist_ok=True)

    fd, tmp_path = tempfile.mkstemp(prefix=".tmp-", suffix=".tmp", dir=target_dir or None)
    try:
        with os.fdopen(fd, "w", encoding=encoding, newline=newline) as f:
            f.write(content)
        os.replace(tmp_path, path)
    except Exception:
        try:
            os.remove(tmp_path)
        except Exception:
            pass
        raise


def atomic_write_json(path: str, payload: Dict[str, object], ensure_ascii: bool = False, indent: int = 2) -> None:
    text = json.dumps(payload, ensure_ascii=ensure_ascii, indent=indent)
    atomic_write_text(path, text + "\n", encoding="utf-8")


def is_path_within(path: str, parent: str) -> bool:
    try:
        return os.path.commonpath([os.path.abspath(path), os.path.abspath(parent)]) == os.path.abspath(parent)
    except Exception:
        return False


def remove_dir_force(path: str) -> None:
    if not os.path.exists(path):
        return
    if os.path.isdir(path):
        shutil.rmtree(path)
    else:
        os.remove(path)


def create_transaction(enabled: bool) -> Optional[RunTransaction]:
    if not enabled:
        return None
    stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
    tx_root = os.path.join(TRANSACTION_ROLLBACK_DIR, f"tx-{stamp}-{os.getpid()}")
    os.makedirs(tx_root, exist_ok=True)
    return RunTransaction(enabled=True, rollback_root=tx_root)


def transaction_snapshot_dir(path: str) -> str:
    if not CURRENT_TRANSACTION or not CURRENT_TRANSACTION.enabled:
        raise RuntimeError("Transaction snapshot requested without active transaction")
    snap_id = len(CURRENT_TRANSACTION.actions) + 1
    backup_path = os.path.join(CURRENT_TRANSACTION.rollback_root, f"snapshot-{snap_id}")
    shutil.copytree(path, backup_path)
    return backup_path


def rollback_transaction(log: Logger) -> None:
    if not CURRENT_TRANSACTION or not CURRENT_TRANSACTION.enabled:
        return

    txn = CURRENT_TRANSACTION
    txn.rolling_back = True
    rollback_failures = 0
    log.warn("Attempting transactional rollback of filesystem changes")

    for action in reversed(txn.actions):
        kind = action.get("kind", "")
        try:
            if kind == "undo_copy":
                dst = action.get("dst", "")
                dst_backup = action.get("dst_backup", "")
                if dst and os.path.exists(dst):
                    remove_dir_force(dst)
                if dst and dst_backup and os.path.exists(dst_backup):
                    os.makedirs(os.path.dirname(dst), exist_ok=True)
                    shutil.copytree(dst_backup, dst)
            elif kind == "undo_move":
                src = action.get("src", "")
                dst = action.get("dst", "")
                dst_backup = action.get("dst_backup", "")
                if dst and os.path.exists(dst):
                    os.makedirs(os.path.dirname(src), exist_ok=True)
                    if os.path.exists(src):
                        remove_dir_force(src)
                    shutil.move(dst, src)
                if dst and dst_backup and os.path.exists(dst_backup):
                    os.makedirs(os.path.dirname(dst), exist_ok=True)
                    shutil.copytree(dst_backup, dst)
            elif kind == "restore_removed":
                path = action.get("path", "")
                backup = action.get("backup", "")
                if path and os.path.exists(path):
                    remove_dir_force(path)
                if path and backup and os.path.exists(backup):
                    os.makedirs(os.path.dirname(path), exist_ok=True)
                    shutil.copytree(backup, path)
            elif kind == "restore_quarantined":
                path = action.get("path", "")
                quarantine = action.get("quarantine", "")
                if path and os.path.exists(path):
                    remove_dir_force(path)
                if path and quarantine and os.path.exists(quarantine):
                    os.makedirs(os.path.dirname(path), exist_ok=True)
                    shutil.move(quarantine, path)
        except Exception as ex:
            rollback_failures += 1
            log.error(f"Rollback action failed ({kind}): {ex}")

    txn.rolling_back = False
    if rollback_failures == 0:
        log.info("Transactional rollback completed successfully")
    else:
        log.error(f"Transactional rollback finished with {rollback_failures} failure(s)")


def finalize_transaction(success: bool, log: Logger) -> None:
    global CURRENT_TRANSACTION
    if not CURRENT_TRANSACTION:
        return
    txn = CURRENT_TRANSACTION
    if not success:
        rollback_transaction(log)
    try:
        if os.path.isdir(txn.rollback_root):
            shutil.rmtree(txn.rollback_root)
    except Exception as ex:
        log.warn(f"Could not clean transaction temp dir {txn.rollback_root}: {ex}")
    CURRENT_TRANSACTION = None


def cleanup_game_quarantine(retention_days: int, dry_run: bool, log: Logger) -> None:
    if retention_days < 0:
        return
    if not os.path.isdir(GAME_REMOVALS_QUARANTINE_DIR):
        return

    cutoff = dt.datetime.now().timestamp() - (retention_days * 86400)
    for entry in os.listdir(GAME_REMOVALS_QUARANTINE_DIR):
        entry_path = os.path.join(GAME_REMOVALS_QUARANTINE_DIR, entry)
        if not os.path.isdir(entry_path):
            continue
        try:
            mtime = os.path.getmtime(entry_path)
        except Exception:
            continue
        if mtime >= cutoff:
            continue
        if dry_run:
            log.info(f"[DRYRUN] Would remove old game quarantine entry: {entry_path}")
        else:
            try:
                shutil.rmtree(entry_path)
                log.info(f"Removed old game quarantine entry: {entry_path}")
            except Exception as ex:
                log.warn(f"Failed to remove old game quarantine entry {entry_path}: {ex}")


def validate_agf_rows_in_csv(log: Logger) -> bool:
    if not os.path.isfile(COMPAT_CSV):
        return True

    _, rows = load_compat_csv()
    invalid_rows: List[str] = []
    for row in rows:
        mod_name = row.get("MOD_NAME", "").strip()
        if mod_name and not is_agf_mod(mod_name):
            invalid_rows.append(mod_name)

    if not invalid_rows:
        return True

    preview = ", ".join(sorted(set(invalid_rows))[:8])
    extra = "" if len(set(invalid_rows)) <= 8 else f" (+{len(set(invalid_rows)) - 8} more)"
    log.error(
        "Non-AGF rows found in HELPER_ModCompatibility.csv. "
        f"Remove these rows before running: {preview}{extra}"
    )
    return False


def check_directory_write_access(path: str) -> Tuple[bool, str]:
    if not os.path.isdir(path):
        return False, "directory missing"
    try:
        fd, probe_path = tempfile.mkstemp(prefix=".write-test-", dir=path)
        os.close(fd)
        os.remove(probe_path)
        return True, ""
    except Exception as ex:
        return False, str(ex)


def run_writeability_preflight(mode: str, dry_run: bool, log: Logger) -> bool:
    probe_dirs: List[str] = [LOGS_DIR, os.path.dirname(COMPAT_CSV), QUOTES_DIR]

    if mode in ("update", "full", "sync-work"):
        probe_dirs.extend([STAGING, GAME_MODS])
    if mode in ("update", "full"):
        probe_dirs.append(IN_PROGRESS)
    if mode in ("promote", "full", "package"):
        probe_dirs.append(PUBLISH_READY)
    if mode in ("package", "full"):
        probe_dirs.extend([ZIP_OUTPUT, GIGGLEPACK_RELEASE_ROOT_DIR])
    if mode == "migrate-readmes-once":
        probe_dirs.extend([STAGING, IN_PROGRESS])

    seen: set[str] = set()
    failures: List[str] = []
    for directory in probe_dirs:
        normalized = os.path.normpath(directory)
        if normalized in seen:
            continue
        seen.add(normalized)

        ok, reason = check_directory_write_access(normalized)
        if not ok:
            failures.append(f"{normalized}: {reason}")

    if not failures:
        log.info("Preflight writeability check passed")
        return True

    for item in failures:
        if dry_run:
            log.warn(f"Preflight writeability issue (dry-run only): {item}")
        else:
            log.error(f"Preflight writeability issue: {item}")

    return dry_run


# =============================================================
# HELPERS
# =============================================================
def is_agf_mod(folder: str) -> bool:
    return folder.startswith(AGF_PREFIXES)


def is_backpack_mod(folder: str) -> bool:
    return folder.startswith("AGF-BackpackPlus-")


def is_hudplus_mod(folder: str) -> bool:
    return folder.startswith("AGF-HUDPlus-")


def is_hudpluszother_mod(folder: str) -> bool:
    return folder.startswith("AGF-HUDPluszOther-")


def is_4modders_mod(folder: str) -> bool:
    return folder.startswith("AGF-4Modders-")


def is_requested_mod(folder: str) -> bool:
    return folder.startswith("AGF-Requested-") or folder.startswith("zzzAGF-Requested-")


def get_base_mod_name(name: str) -> str:
    return re.sub(r"-v\d+\.\d+(\.\d+)*$", "", name)


def parse_modinfo(modinfo_path: str, fallback_name: str) -> Tuple[str, str]:
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        name_tag = root.find("Name")
        version_tag = root.find("Version")
        mod_name = name_tag.attrib.get("value", fallback_name) if name_tag is not None else fallback_name
        mod_version = version_tag.attrib.get("value", "0.0.0") if version_tag is not None else "0.0.0"
        return mod_name, mod_version
    except Exception:
        return fallback_name, "0.0.0"


def load_draft_promotion_baseline(log: "Logger") -> Dict[str, Dict[str, object]]:
    if not os.path.exists(DRAFT_PROMOTION_BASELINE_PATH):
        return {}

    try:
        with open(DRAFT_PROMOTION_BASELINE_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)
        if isinstance(data, dict):
            return data
        log.warn("Draft promotion baseline file is invalid; starting with empty baseline")
        return {}
    except Exception as ex:
        log.warn(f"Could not read draft promotion baseline file: {ex}")
        return {}


def save_draft_promotion_baseline(
    baseline: Dict[str, Dict[str, object]],
    dry_run: bool,
    log: "Logger",
) -> None:
    if dry_run:
        log.info(f"[DRYRUN] Would update draft promotion baseline: {DRAFT_PROMOTION_BASELINE_PATH}")
        return

    try:
        os.makedirs(os.path.dirname(DRAFT_PROMOTION_BASELINE_PATH), exist_ok=True)
        with open(DRAFT_PROMOTION_BASELINE_PATH, "w", encoding="utf-8") as f:
            json.dump(baseline, f, indent=2, sort_keys=True)
    except Exception as ex:
        log.warn(f"Could not write draft promotion baseline file: {ex}")


def refresh_draft_promotion_baseline_from_lane(dry_run: bool, log: "Logger") -> None:
    """Capture current Draft lane versions as the promotion baseline."""
    draft_folders = scan_mod_folders(IN_PROGRESS)
    now_iso = dt.datetime.now().isoformat(timespec="seconds")
    baseline: Dict[str, Dict[str, object]] = {}

    for draft_folder, draft_path in sorted(draft_folders.items()):
        base_name = get_base_mod_name(draft_folder)
        draft_ver = get_modinfo_version(draft_path)
        if not draft_ver:
            continue
        baseline[base_name] = {
            "folder": draft_folder,
            "version": draft_ver,
            "recorded_at": now_iso,
        }

    save_draft_promotion_baseline(baseline, dry_run, log)
    log.info(
        f"Draft promotion baseline refreshed from Draft lane: {len(baseline)} mod(s) recorded"
    )


def get_modinfo_display_name(modinfo_path: str, fallback_name: str) -> str:
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        display_tag = root.find("DisplayName")
        if display_tag is not None:
            display_name = display_tag.attrib.get("value", "").strip()
            if display_name:
                return display_name
    except Exception:
        pass
    return fallback_name


def format_version_for_display(version: str, display_name: str) -> str:
    version_text = (version or "0.0.0").strip()
    if "BETA" in (display_name or "") and not version_text.endswith("-BETA"):
        return f"{version_text}-BETA"
    return version_text


def get_modinfo_version(folder_path: str) -> Optional[str]:
    modinfo_path = os.path.join(folder_path, "ModInfo.xml")
    if not os.path.exists(modinfo_path):
        return None
    _, version = parse_modinfo(modinfo_path, "")
    return version or None


def compare_versions(v1: Optional[str], v2: Optional[str]) -> int:
    def to_tuple(v: Optional[str]) -> Tuple[int, ...]:
        parts = re.findall(r"\d+", v or "0.0.0")
        return tuple(int(p) for p in parts)

    t1 = to_tuple(v1)
    t2 = to_tuple(v2)
    maxlen = max(len(t1), len(t2))
    t1 += (0,) * (maxlen - len(t1))
    t2 += (0,) * (maxlen - len(t2))
    return (t1 > t2) - (t1 < t2)


def has_version_drift(folder_name: str, mod_version: str) -> bool:
    version_match = re.search(r"-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
    folder_version = version_match.group(1) if version_match else ""
    return folder_version != (mod_version or "")


def get_notepadpp_pids() -> List[int]:
    """Return live Notepad++ process IDs on Windows.

    Uses CSV tasklist output to avoid fragile substring checks.
    """
    try:
        result = subprocess.run(
            ["tasklist", "/FO", "CSV", "/NH", "/FI", "IMAGENAME eq notepad++.exe"],
            capture_output=True,
            text=True,
            check=False,
        )
        output = (result.stdout or "").strip()
        pids: List[int] = []
        if not output:
            return pids

        for row in csv.reader(io.StringIO(output)):
            if len(row) < 2:
                continue
            image = (row[0] or "").strip().lower()
            if image != "notepad++.exe":
                continue
            try:
                pids.append(int((row[1] or "").strip()))
            except ValueError:
                continue
        return pids
    except Exception:
        return []


def is_notepadpp_running() -> bool:
    return bool(get_notepadpp_pids())


def extract_folder_version(folder_name: str) -> str:
    version_match = re.search(r"-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
    return version_match.group(1) if version_match else ""


def ensure_notepadpp_closed_for_version_bumps(
    folder_renames: List[Tuple[str, str, str]],
    dry_run: bool,
    log: Logger,
) -> bool:
    """When version bumps are about to be applied, prompt user to close Notepad++ if running."""
    has_version_bump = any(
        extract_folder_version(old) != extract_folder_version(new) for old, new, _ in folder_renames
    )
    if not has_version_bump:
        return True

    if not is_notepadpp_running():
        return True

    message = (
        "Notepad++ is currently open. It may be locking files that need to be updated. "
        "Please close Notepad++ and press Enter to continue, or type 'skip' to abort."
    )

    if dry_run or not sys.stdin or not sys.stdin.isatty():
        log.warn(message + " Proceeding because this run is non-interactive.")
        return True

    while True:
        print(message)
        try:
            response = input().strip().lower()
        except EOFError:
            response = "skip"

        if response == "skip":
            log.error("Run aborted by user due to Notepad++ pre-flight check for version bumps")
            return False

        pids = get_notepadpp_pids()
        if not pids:
            return True

        log.warn(
            "Notepad++ is still running (PID(s): "
            + ", ".join(str(pid) for pid in pids)
            + "). Close it, then press Enter, or type 'skip' to abort."
        )


def ensure_notepadpp_closed_for_game_sync(dry_run: bool, log: Logger) -> bool:
    """Before touching game Mods data, require Notepad++ closure in interactive runs."""
    if not is_notepadpp_running():
        return True

    message = (
        "Notepad++ is currently open. Game-folder sync/push/pull is about to run and may fail due to file locks. "
        "Please close Notepad++ and press Enter to continue, or type 'skip' to abort."
    )

    if dry_run or not sys.stdin or not sys.stdin.isatty():
        log.warn(message + " Proceeding because this run is non-interactive.")
        return True

    while True:
        print(message)
        try:
            response = input().strip().lower()
        except EOFError:
            response = "skip"

        if response == "skip":
            log.error("Run aborted by user due to Notepad++ pre-flight check for game-folder sync")
            return False

        pids = get_notepadpp_pids()
        if not pids:
            return True

        log.warn(
            "Notepad++ is still running (PID(s): "
            + ", ".join(str(pid) for pid in pids)
            + "). Close it, then press Enter, or type 'skip' to abort."
        )


def resolve_publish_gigglepack_action(requested_action: str, dry_run: bool, log: Logger) -> str:
    """Resolve publish-time GigglePack behavior for full-mode runs."""
    action = (requested_action or "finalize").strip().lower()
    if action in {"finalize", "append-latest", "queue"}:
        return action

    if action != "ask":
        log.warn(
            f"Unknown --publish-gigglepack-action '{requested_action}'. Falling back to 'finalize'."
        )
        return "finalize"

    if dry_run or not sys.stdin or not sys.stdin.isatty():
        log.info(
            "Publish GigglePack action is 'ask' but run is non-interactive/dry-run; defaulting to 'queue'."
        )
        return "queue"

    prompt = (
        "Publish GigglePack decision: [F] finalize release now, "
        "[A] append to latest finalized release notes, "
        "or [Q] queue pending changes only (default: Q)"
    )

    while True:
        print(prompt)
        try:
            response = input().strip().lower()
        except EOFError:
            response = ""

        if response in {"", "q", "queue"}:
            return "queue"
        if response in {"a", "append", "append-latest"}:
            return "append-latest"
        if response in {"f", "finalize"}:
            return "finalize"

        log.warn("Invalid publish selection. Type 'F' (finalize), 'A' (append-latest), or 'Q' (queue).")


def acquire_run_lock() -> bool:
    def read_lock_pid() -> Optional[int]:
        try:
            with open(RUN_LOCK_PATH, "r", encoding="utf-8") as f:
                for line in f:
                    if line.startswith("pid="):
                        value = line.split("=", 1)[1].strip()
                        return int(value)
        except Exception:
            return None
        return None

    def pid_is_running(pid: int) -> bool:
        if pid <= 0:
            return False
        try:
            os.kill(pid, 0)
            return True
        except OSError:
            return False
        except Exception:
            return False

    for _ in range(2):
        try:
            fd = os.open(RUN_LOCK_PATH, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
            with os.fdopen(fd, "w", encoding="utf-8") as f:
                f.write(f"pid={os.getpid()}\n")
                f.write(f"started={dt.datetime.now().isoformat()}\n")
            return True
        except FileExistsError:
            lock_pid = read_lock_pid()
            if lock_pid is not None and pid_is_running(lock_pid):
                return False

            # Lock file exists but owner process is gone (or unreadable); clear stale lock and retry once.
            try:
                os.remove(RUN_LOCK_PATH)
            except FileNotFoundError:
                pass
            except Exception:
                return False
        except Exception:
            return False

    return False


def release_run_lock() -> None:
    try:
        if os.path.exists(RUN_LOCK_PATH):
            os.remove(RUN_LOCK_PATH)
    except Exception:
        pass


def write_run_manifest(log: Logger, mode: str, dry_run: bool, exit_code: int, log_path: Optional[str]) -> Optional[str]:
    """Write a machine-readable run manifest for audits and troubleshooting."""
    try:
        os.makedirs(LOGS_DIR, exist_ok=True)
        stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
        manifest_path = os.path.join(LOGS_DIR, f"run-manifest-{stamp}.json")
        payload = {
            "timestamp": dt.datetime.now().isoformat(),
            "mode": mode,
            "dry_run": dry_run,
            "exit_code": exit_code,
            "log_path": log_path,
            "stats": log.stats.__dict__,
            "action_needed": log.get_action_needed_lines(),
            "mod_changes": log.get_mod_change_summary_lines(),
        }
        atomic_write_json(manifest_path, payload, ensure_ascii=False, indent=2)

        manifest_candidates: List[Tuple[float, str]] = []
        for name in os.listdir(LOGS_DIR):
            if not (name.startswith("run-manifest-") and name.endswith(".json")):
                continue
            full_path = os.path.join(LOGS_DIR, name)
            if os.path.isfile(full_path):
                manifest_candidates.append((os.path.getmtime(full_path), full_path))

        manifest_candidates.sort(key=lambda item: item[0], reverse=True)
        for _, old_path in manifest_candidates[RUN_MANIFEST_MAX_FILES:]:
            try:
                os.remove(old_path)
            except Exception:
                pass

        return manifest_path
    except Exception:
        return None


def hash_file(path: str) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(65536), b""):
            h.update(chunk)
    return h.hexdigest()


def hash_directory(path: str) -> str:
    h = hashlib.sha256()
    for root, _, files in os.walk(path):
        rel_root = os.path.relpath(root, path).replace("\\", "/")
        for file in sorted(files):
            full_path = os.path.join(root, file)
            rel_path = os.path.join(rel_root, file).replace("\\", "/")
            h.update(rel_path.encode("utf-8", errors="replace"))
            h.update(hash_file(full_path).encode("ascii"))
    return h.hexdigest()


def scan_mod_folders(base_dir: str) -> Dict[str, str]:
    if not os.path.isdir(base_dir):
        return {}
    return {
        name: os.path.join(base_dir, name)
        for name in os.listdir(base_dir)
        if os.path.isdir(os.path.join(base_dir, name)) and is_agf_mod(name)
    }


def maybe_remove_dir(path: str, dry_run: bool, log: Logger) -> bool:
    if not os.path.exists(path):
        return True
    if dry_run:
        log.info(f"[DRYRUN] Would remove directory: {path}")
        return True
    try:
        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back:
            if is_path_within(path, GAME_MODS):
                rel = os.path.relpath(path, GAME_MODS)
                stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
                quarantine_target = os.path.join(GAME_REMOVALS_QUARANTINE_DIR, stamp, rel)
                os.makedirs(os.path.dirname(quarantine_target), exist_ok=True)
                if os.path.exists(quarantine_target):
                    remove_dir_force(quarantine_target)
                shutil.move(path, quarantine_target)
                CURRENT_TRANSACTION.actions.append(
                    {
                        "kind": "restore_quarantined",
                        "path": path,
                        "quarantine": quarantine_target,
                    }
                )
                log.info(f"Quarantined game folder instead of delete: {path} -> {quarantine_target}")
                return True

            backup_path = transaction_snapshot_dir(path)
            remove_dir_force(path)
            CURRENT_TRANSACTION.actions.append(
                {
                    "kind": "restore_removed",
                    "path": path,
                    "backup": backup_path,
                }
            )
            return True

        if is_path_within(path, GAME_MODS):
            rel = os.path.relpath(path, GAME_MODS)
            stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
            quarantine_target = os.path.join(GAME_REMOVALS_QUARANTINE_DIR, stamp, rel)
            os.makedirs(os.path.dirname(quarantine_target), exist_ok=True)
            if os.path.exists(quarantine_target):
                remove_dir_force(quarantine_target)
            shutil.move(path, quarantine_target)
            log.info(f"Quarantined game folder instead of delete: {path} -> {quarantine_target}")
            return True
        remove_dir_force(path)
        return True
    except Exception as ex:
        log.error(f"Failed to remove directory {path}: {ex}")
        if FAIL_FAST_ENABLED:
            raise RuntimeError(f"Failed to remove directory: {path}") from ex
        return False


def maybe_copytree(src: str, dst: str, dry_run: bool, log: Logger) -> bool:
    if dry_run:
        log.info(f"[DRYRUN] Would copy directory: {src} -> {dst}")
        return True
    try:
        dst_backup = ""
        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back and os.path.exists(dst):
            dst_backup = transaction_snapshot_dir(dst)

        if os.path.exists(dst):
            remove_dir_force(dst)
        shutil.copytree(src, dst)

        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back:
            CURRENT_TRANSACTION.actions.append(
                {
                    "kind": "undo_copy",
                    "dst": dst,
                    "dst_backup": dst_backup,
                }
            )
        return True
    except Exception as ex:
        log.error(f"Failed to copy directory {src} -> {dst}: {ex}")
        if FAIL_FAST_ENABLED:
            raise RuntimeError(f"Failed to copy directory: {src} -> {dst}") from ex
        return False


def maybe_move(src: str, dst: str, dry_run: bool, log: Logger) -> bool:
    if dry_run:
        log.info(f"[DRYRUN] Would move directory: {src} -> {dst}")
        return True
    try:
        dst_backup = ""
        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back and os.path.exists(dst):
            dst_backup = transaction_snapshot_dir(dst)

        if os.path.exists(dst):
            remove_dir_force(dst)
        shutil.move(src, dst)

        if CURRENT_TRANSACTION and CURRENT_TRANSACTION.enabled and not CURRENT_TRANSACTION.rolling_back:
            CURRENT_TRANSACTION.actions.append(
                {
                    "kind": "undo_move",
                    "src": src,
                    "dst": dst,
                    "dst_backup": dst_backup,
                }
            )
        return True
    except Exception as ex:
        log.error(f"Failed to move directory {src} -> {dst}: {ex}")
        if FAIL_FAST_ENABLED:
            raise RuntimeError(f"Failed to move directory: {src} -> {dst}") from ex
        return False


def _ensure_trailing_blank_count(lines: List[str], count: int) -> None:
    trailing = 0
    idx = len(lines) - 1
    while idx >= 0 and lines[idx] == "":
        trailing += 1
        idx -= 1

    if trailing < count:
        lines.extend([""] * (count - trailing))
    elif trailing > count:
        del lines[idx + 1 : idx + 1 + (trailing - count)]


def _normalize_list_indentation(text: str) -> str:
    output: List[str] = []
    for raw_line in text.splitlines():
        line = raw_line.rstrip()
        match = re.match(r"^(\s*)(\d+\.\s+|-\s+)(.*)$", line)
        if not match:
            output.append(line)
            continue

        leading = match.group(1).replace("\t", "    ")
        marker = match.group(2)
        body = match.group(3)
        level = max(0, len(leading) // 2)
        indent = 2 + (level * 2)
        output.append((" " * indent) + marker + body)
    return "\n".join(output)


def _apply_divider_spacing(text: str, major_divider: str, minor_divider: str) -> str:
    lines = text.splitlines()
    output: List[str] = []
    idx = 0

    while idx < len(lines):
        line = lines[idx].rstrip()

        if (
            line == major_divider
            and idx + 2 < len(lines)
            and lines[idx + 1].strip()
            and lines[idx + 2].rstrip() == major_divider
        ):
            if output:
                _ensure_trailing_blank_count(output, 3)
            output.append(major_divider)
            output.append(lines[idx + 1].strip())
            output.append(major_divider)
            _ensure_trailing_blank_count(output, 1)
            idx += 3
            while idx < len(lines) and not lines[idx].strip():
                idx += 1
            continue

        if (
            line == minor_divider
            and idx + 2 < len(lines)
            and lines[idx + 1].strip()
            and lines[idx + 2].rstrip() == minor_divider
        ):
            if output:
                _ensure_trailing_blank_count(output, 2)
            output.append(minor_divider)
            output.append(lines[idx + 1].strip())
            output.append(minor_divider)
            _ensure_trailing_blank_count(output, 1)
            idx += 3
            while idx < len(lines) and not lines[idx].strip():
                idx += 1
            continue

        if line in {major_divider, minor_divider}:
            if output:
                _ensure_trailing_blank_count(output, 1)
            output.append(line)
            _ensure_trailing_blank_count(output, 1)
            idx += 1
            while idx < len(lines) and not lines[idx].strip():
                idx += 1
            continue

        output.append(line)
        idx += 1

    while output and output[0] == "":
        output.pop(0)
    while output and output[-1] == "":
        output.pop()
    return "\n".join(output)


def _should_preserve_unwrapped_line(line: str, major_divider: str, minor_divider: str) -> bool:
    stripped = line.strip()
    if not stripped:
        return True
    if stripped in {major_divider, minor_divider}:
        return True
    # Keep URL-only lines intact, but allow prose lines that include links to wrap.
    if re.fullmatch(r"(?:https?://\S+|www\.\S+)", stripped, re.IGNORECASE):
        return True
    # Keep path-only lines intact; mixed prose/path lines should still wrap.
    if re.fullmatch(r"(?:[A-Za-z]:[\\/].*|%[A-Za-z0-9_]+%)", stripped):
        return True
    return False


def _wrap_text_to_width(text: str, width: int, major_divider: str, minor_divider: str) -> str:
    wrapped: List[str] = []
    lines = text.splitlines()
    for raw_line in lines:
        line = raw_line.rstrip()
        if _should_preserve_unwrapped_line(line, major_divider, minor_divider) or len(line) <= width:
            wrapped.append(line)
            continue

        allow_url_breaks = False

        list_match = re.match(r"^(\s*)(\d+\.\s+|-\s+)(.*)$", line)
        wrapped_lines: List[str] = []
        if list_match:
            leading = list_match.group(1)
            marker = list_match.group(2)
            body = list_match.group(3).strip()
            initial_indent = f"{leading}{marker}"
            subsequent_indent = " " * len(initial_indent)
            if not body:
                wrapped.append(line)
                continue
            wrapped_lines = textwrap.wrap(
                body,
                width=width,
                initial_indent=initial_indent,
                subsequent_indent=subsequent_indent,
                break_long_words=allow_url_breaks,
                break_on_hyphens=allow_url_breaks,
            )
            wrapped.extend(wrapped_lines)
            continue

        leading_match = re.match(r"^(\s*)", line)
        leading = leading_match.group(1) if leading_match else ""
        body = line[len(leading):].strip()
        if not body:
            wrapped.append(line)
            continue
        wrapped_lines = textwrap.wrap(
            body,
            width=width,
            initial_indent=leading,
            subsequent_indent=leading,
            break_long_words=allow_url_breaks,
            break_on_hyphens=allow_url_breaks,
        )
        wrapped.extend(wrapped_lines)

    return "\n".join(wrapped)


def markdown_to_text(md: str) -> str:
    wrap_width = 72
    major_divider = "=" * wrap_width
    minor_divider = "-" * wrap_width

    md = re.sub(r"```[\s\S]*?```", "", md)
    md = re.sub(r"!\[[^\]]*\]\([^\)]*\)", "", md)
    md = re.sub(r"<!--[\s\S]*?-->", "", md)
    md = re.sub(r"\[([^\]]+)\]\(([^\)]+)\)", r"\1: \2", md)
    md = re.sub(r"[`*~]", "", md)
    md = re.sub(r"^[ \t]*---+[ \t]*\r?$", minor_divider, md, flags=re.MULTILINE)
    md = re.sub(r"^#+\s*", "", md, flags=re.MULTILINE)
    md = re.sub(r"^>\s?", "", md, flags=re.MULTILINE)
    md = re.sub(r"^[ \t]*={3,}[ \t]*\r?$", major_divider, md, flags=re.MULTILINE)
    md = re.sub(r"^[ \t]*-{3,}[ \t]*\r?$", minor_divider, md, flags=re.MULTILINE)
    md = _normalize_list_indentation(md)
    md = _apply_divider_spacing(md, major_divider, minor_divider)
    md = _wrap_text_to_width(md, wrap_width, major_divider, minor_divider)
    md = _center_h1_titles(md, wrap_width)
    md = re.sub(r"\n{5,}", "\n\n\n\n", md)
    return md.strip()


def _center_h1_titles(text: str, width: int = 72) -> str:
    major_divider = "=" * width
    lines = text.splitlines()
    output: List[str] = []
    idx = 0

    while idx < len(lines):
        current = lines[idx].rstrip()
        if (
            current == major_divider
            and idx + 2 < len(lines)
            and lines[idx + 2].rstrip() == major_divider
        ):
            title = lines[idx + 1].strip()
            left_pad = max(0, (width - len(title)) // 2)
            right_pad = max(0, width - len(title) - left_pad)
            output.append(major_divider)
            output.append((" " * left_pad) + title + (" " * right_pad))
            output.append(major_divider)
            idx += 3
            continue

        output.append(current)
        idx += 1

    return "\n".join(output)


def format_changelog_text(changelog_block: str, include_h1: bool = True) -> str:
    wrap_width = 72
    major_divider = "=" * wrap_width
    minor_divider = "-" * wrap_width

    raw_lines = [(line or "").rstrip() for line in (changelog_block or "").splitlines()]
    version_pattern = re.compile(r"^v\d+(?:\.\d+){0,3}(?:\b.*)?$", re.IGNORECASE)

    sections: List[Tuple[str, List[str]]] = []
    current_title: Optional[str] = None
    current_body: List[str] = []

    def _push_current() -> None:
        nonlocal current_title, current_body
        if current_title is not None:
            sections.append((current_title, current_body[:]))
            current_title = None
            current_body = []

    for raw in raw_lines:
        stripped = raw.strip()
        if not stripped:
            if current_title is not None and current_body and current_body[-1] != "":
                current_body.append("")
            continue

        cleaned = re.sub(r"^#+\s*", "", stripped)
        cleaned = re.sub(r"\[([^\]]+)\]\(([^\)]+)\)", r"\1: \2", cleaned)
        cleaned = re.sub(r"[`*~]", "", cleaned).strip()
        if not cleaned:
            continue
        if re.fullmatch(r"[=-]{10,}", cleaned):
            continue
        if cleaned.lower() == "changelog":
            continue

        if version_pattern.match(cleaned):
            _push_current()
            current_title = cleaned
            continue

        if current_title is None:
            current_title = "Notes"
        current_body.append(cleaned)

    _push_current()

    if not sections:
        sections = [("Notes", ["- Add changelog entries here."])]

    output: List[str] = []
    if include_h1:
        centered_title = _center_h1_titles("\n".join([major_divider, "CHANGELOG", major_divider]), wrap_width).splitlines()[1]
        output = [major_divider, centered_title, major_divider, ""]

    for idx, (title, body_lines) in enumerate(sections):
        if idx > 0:
            # H3 rule: single 72-char divider with one blank line above and below.
            output.extend(["", minor_divider, ""])

        output.append(title)
        body_text = "\n".join(body_lines).strip()
        if not body_text:
            body_text = "- Add changelog entries here."

        normalized_body_lines: List[str] = []
        for raw_body_line in body_text.splitlines():
            stripped_body = raw_body_line.strip()
            if not stripped_body:
                normalized_body_lines.append("")
                continue
            if re.match(r"^(?:[-*]\s+|\d+\.\s+)", stripped_body):
                if stripped_body.startswith("* "):
                    stripped_body = "- " + stripped_body[2:]
                normalized_body_lines.append("  " + stripped_body)
            else:
                normalized_body_lines.append("  - " + stripped_body)

        normalized_body = "\n".join(normalized_body_lines).strip("\n")
        normalized_body = _wrap_text_to_width(normalized_body, wrap_width, major_divider, minor_divider)
        output.extend(normalized_body.splitlines())

    return "\n".join(output).strip()


def format_blockquote(text: str) -> str:
    if not text.strip():
        return ""
    lines = text.splitlines()
    return "\n".join(f"> {line}" if line.strip() else ">" for line in lines)


def format_quote_for_readme(text: str) -> str:
    if not text.strip():
        return ""
    normalized = " ".join(line.strip() for line in text.splitlines() if line.strip())
    if not normalized:
        return ""
    if normalized.startswith('"') and normalized.endswith('"') and len(normalized) >= 2:
        return normalized
    stripped = normalized.strip('"')
    return f'"{stripped}"'


def build_title_card_callout_block_for_readme(quote_text: str) -> str:
    """Render the intro callout block with deterministic spacing.

    Rules:
    - Quote present: one blank line above quote, one blank line between quote and NOTE.
    - Quote missing: keep a two-blank-line pause before NOTE.
    - Template owns the single blank line after NOTE before the next section divider.
    """
    note_line = "NOTE: AGF Mod Guide and Changelog are further below."
    if quote_text.strip():
        return "\n".join([quote_text.strip(), "", note_line])
    return "\n".join(["", note_line])


def _ensure_sentence(text: str) -> str:
    cleaned = re.sub(r"\s+", " ", text).strip()
    if not cleaned:
        return ""
    return cleaned if re.search(r"[.!?]$", cleaned) else f"{cleaned}."


def normalize_safety_value_for_readme(value: str) -> str:
    normalized = (value or "").strip().lower()
    if normalized in {"safe", "yes", "y", "true", "1"}:
        return "Yes (Safe)"
    if normalized in {"dangerous", "no", "n", "false", "0"}:
        return "No (Dangerous)"
    if not normalized or normalized in {"missingdata", "tbd", "none"}:
        return "Unknown"
    return str(value).strip()


def format_dependencies_block_for_readme(value: str) -> str:
    raw = (value or "").strip()
    if has_harmony_dependency(raw):
        return load_harmony_requirement_warning_body()

    normalized_raw = raw.lower()

    placeholder_tokens = {"", "0", "none", "missingdata", "tbd", "n/a", "na"}
    if normalized_raw in placeholder_tokens:
        return "- Dependencies: None, works standalone."

    parts = [part.strip() for part in re.split(r"[;\n|,]+", raw) if part.strip()]
    cleaned: List[str] = []
    for part in parts:
        token = part.strip()
        token_lower = token.lower()
        if token_lower in placeholder_tokens:
            continue
        if token_lower == "x":
            cleaned.append("0_TFP_Harmony (built-in game mod)")
            continue
        cleaned.append(token)

    if not cleaned:
        return "- Dependencies: None, works standalone."

    if len(cleaned) == 1:
        return f"- Dependencies: {cleaned[0]}"

    lines = ["- Dependencies:"]
    lines.extend(f"  - {part}" for part in cleaned)
    return "\n".join(lines)


def has_harmony_dependency(value: str) -> bool:
    raw = (value or "").strip()
    if not raw:
        return False

    if raw.lower() == "x":
        return True

    for part in re.split(r"[;\n|,]+", raw):
        token = part.strip().lower()
        if not token:
            continue
        if token == "x":
            return True
        if re.search(r"\b0[_\-\s]*tfp[_\-\s]*harmony\b", token):
            return True
        if token == "harmony" or token.startswith("harmony ") or token.endswith(" harmony"):
            return True

    return False


def format_mod_type_block_for_readme(mod_type_line: str) -> str:
    raw = (mod_type_line or "").strip()
    if raw in {"0", "TBD"}:
        return "- Mod Type: TBD"
    if not raw or raw == "MISSINGDATA":
        return "- Mod Type: MISSINGDATA"

    type_name = raw
    details = ""
    if ":" in raw:
        type_name, details = raw.split(":", 1)
        type_name = type_name.strip()
        details = details.strip()

    lines = [f"- Mod Type: {type_name or raw}"]
    if not details:
        return "\n".join(lines)

    detail_lines: List[str] = []
    details_no_parenthetical = re.sub(r"\([^()]*\)", "", details)
    for segment in details_no_parenthetical.split(";"):
        sentence = _ensure_sentence(segment.strip(" ."))
        if sentence:
            detail_lines.append(sentence)

    for match in re.finditer(r"\(([^()]*)\)", details):
        sentence = _ensure_sentence(match.group(1).strip(" ."))
        if sentence:
            detail_lines.append(sentence)

    if not detail_lines:
        sentence = _ensure_sentence(details)
        if sentence:
            detail_lines.append(sentence)

    for sentence in detail_lines:
        lines.append(f"  - {sentence}")

    return "\n".join(lines)


def markdown_features_to_html(features_text: str) -> str:
    lines = [line.rstrip() for line in features_text.strip().splitlines() if line.strip()]
    if not lines:
        return ""
    html = ""
    stack: List[int] = []
    for line in lines:
        indent = len(line) - len(line.lstrip(" "))
        indent += 4 * (len(line) - len(line.lstrip("\t")))
        content = line.lstrip("-* \t").strip()
        while stack and indent < stack[-1]:
            html += "</ul>"
            stack.pop()
        if not stack or indent > stack[-1]:
            html += "<ul>"
            stack.append(indent)
        html += f"<li>{content}</li>"
    while stack:
        html += "</ul>"
        stack.pop()
    return html


def extract_readme_block(readme_path: str, start_marker: str, end_marker: str) -> str:
    if not os.path.exists(readme_path):
        return ""
    try:
        with open(readme_path, "r", encoding="utf-8") as f:
            content = f.read()
        start = content.find(start_marker)
        end = content.find(end_marker)
        if start != -1 and end != -1:
            return content[start + len(start_marker):end]
    except Exception:
        return ""
    return ""


def _normalize_heading_label(text: str) -> str:
    label = re.sub(r"^#+\s*", "", text).strip()
    label = re.sub(r"^\d+\.\s*", "", label).strip()
    return label.lower()


def extract_markdown_section_by_headings(readme_path: str, headings: Tuple[str, ...]) -> str:
    if not os.path.exists(readme_path):
        return ""
    try:
        with open(readme_path, "r", encoding="utf-8") as f:
            lines = f.read().splitlines()
    except Exception:
        return ""

    heading_set = {_normalize_heading_label(h) for h in headings if h.strip()}
    start_idx: Optional[int] = None
    for idx, line in enumerate(lines):
        stripped = line.strip()
        if stripped.startswith("##") and _normalize_heading_label(stripped) in heading_set:
            start_idx = idx + 1
            break

    if start_idx is None:
        return ""

    collected: List[str] = []
    for line in lines[start_idx:]:
        stripped = line.strip()
        if stripped.startswith("##"):
            break
        if stripped in {"---", "=" * 40}:
            continue
        collected.append(line.rstrip())

    return "\n".join(collected).strip("\n")


def extract_txt_section(readme_path: str, headings: Tuple[str, ...], stop_headings: Tuple[str, ...]) -> str:
    if not os.path.exists(readme_path):
        return ""
    try:
        with open(readme_path, "r", encoding="utf-8") as f:
            lines = f.read().splitlines()
    except Exception:
        return ""

    heading_set = {h.strip().lower() for h in headings if h.strip()}
    stop_set = {h.strip().lower() for h in stop_headings if h.strip()}

    start_idx: Optional[int] = None
    for idx, line in enumerate(lines):
        if line.strip().lower() in heading_set:
            start_idx = idx + 1
            break

    if start_idx is None:
        return ""

    collected: List[str] = []
    for line in lines[start_idx:]:
        stripped = line.strip()
        lowered = stripped.lower()
        if lowered in stop_set:
            break
        if re.fullmatch(r"[-=]{10,}", stripped):
            continue
        collected.append(line.rstrip())

    return "\n".join(collected).strip("\n")


def _normalize_preserved_list_indentation(block: str) -> str:
    """Normalize preserved list indentation without flattening nested bullets."""
    if not block:
        return block

    lines = block.splitlines()
    bullet_match_re = re.compile(r"^(\s*)([-*]|\d+\.)\s+(.*)$")

    normalized_lines: List[str] = []
    previous_bullet_depth = 0
    previous_bullet_indent = 0
    previous_was_label = False
    for raw_line in lines:
        line = raw_line.rstrip()
        stripped = line.strip()

        if not stripped:
            if normalized_lines and normalized_lines[-1] != "":
                normalized_lines.append("")
            previous_was_label = False
            continue

        if re.fullmatch(r"[-=]{10,}", stripped):
            if normalized_lines and normalized_lines[-1] != "":
                normalized_lines.append("")
            previous_was_label = False
            continue

        bullet_match = bullet_match_re.match(line)
        if bullet_match:
            leading = bullet_match.group(1)
            marker = bullet_match.group(2)
            body = bullet_match.group(3).strip()
            leading_spaces = len(leading)
            if previous_was_label:
                depth = 1
            elif previous_bullet_depth == 0 and leading_spaces > 0:
                depth = 1 if leading_spaces > previous_bullet_indent else 0
            elif leading_spaces > previous_bullet_indent:
                depth = previous_bullet_depth + 1
            elif leading_spaces < previous_bullet_indent:
                depth = max(0, previous_bullet_depth - 1)
            else:
                depth = previous_bullet_depth

            indent = " " * (2 + depth * 2)
            normalized_lines.append(f"{indent}{marker} {body}" if body else f"{indent}{marker}")
            previous_bullet_depth = depth
            previous_bullet_indent = leading_spaces
            previous_was_label = False
            continue

        if normalized_lines and re.match(r"^(\s*)([-*]|\d+\.)\s+", normalized_lines[-1]) and not re.fullmatch(r".*:\s*$", stripped):
            normalized_lines[-1] = normalized_lines[-1].rstrip() + " " + stripped
            previous_was_label = False
            continue

        normalized_lines.append(stripped)
        previous_was_label = bool(re.fullmatch(r"[^:]+:", stripped))
        previous_bullet_depth = 0
        previous_bullet_indent = 0

    return "\n".join(normalized_lines).strip()


def sanitize_preserved_readme_block(block: str, flatten_list_markers: bool = False) -> str:
    """Clean malformed legacy markers from preserved README sections."""
    if not block:
        return block

    # Some legacy files contain leftover chevrons right after section markers.
    cleaned = re.sub(r"^[ \t]*>+[ \t]*", "", block)
    cleaned = re.sub(r"^\s*>+\s*$", "", cleaned, flags=re.MULTILINE)
    # Keep repeated regen idempotent by resetting inherited section indentation.
    cleaned = textwrap.dedent(cleaned)
    if flatten_list_markers:
        cleaned = re.sub(r"(?m)^[ \t]+(?=(?:[-*]\s+|\d+\.\s+))", "", cleaned)
        bullet_line_re = re.compile(r"^(?:[-*]\s+|\d+\.\s+)")
        bullet_body_re = re.compile(r"^(?:[-*]\s+|\d+\.\s+)(.*)$")
        merged_lines: List[str] = []
        for raw_line in cleaned.splitlines():
            stripped = raw_line.strip()
            if not stripped:
                if merged_lines and merged_lines[-1] != "":
                    merged_lines.append("")
                continue

            # Divider artifacts should not be merged into nearby bullets.
            if re.fullmatch(r"[-=]{10,}", stripped):
                continue

            if bullet_line_re.match(stripped):
                # Repair malformed wrapped bullets that were accidentally promoted
                # to nested list items in previous generations (for example:
                # '- items' / '- inside) ...').
                curr_body_match = bullet_body_re.match(stripped)
                curr_body = (curr_body_match.group(1).strip() if curr_body_match else "")
                if merged_lines and bullet_line_re.match(merged_lines[-1]):
                    prev_body_match = bullet_body_re.match(merged_lines[-1].strip())
                    prev_body = (prev_body_match.group(1).strip() if prev_body_match else "")
                    prev_tail = prev_body.rstrip()
                    continuation_cue = bool(
                        re.search(r"(?:\b(?:and|or|both|with|without|to|for|from|in|on|of|by|than|not)\b|\()$", prev_tail, re.IGNORECASE)
                    ) or not re.search(r"[.!?]$", prev_tail)
                    looks_like_fragment = bool(re.match(r"^[a-z0-9]", curr_body))
                    if looks_like_fragment and len(curr_body) <= 60 and continuation_cue:
                        merged_lines[-1] = merged_lines[-1].rstrip() + " " + curr_body
                        continue

                merged_lines.append(stripped)
                continue

            if merged_lines and bullet_line_re.match(merged_lines[-1]):
                merged_lines[-1] = merged_lines[-1].rstrip() + " " + stripped
                continue

            merged_lines.append(stripped)

        cleaned = "\n".join(merged_lines)
    else:
        cleaned = _normalize_preserved_list_indentation(cleaned)
    cleaned = re.sub(r"\n{3,}", "\n\n", cleaned)
    return cleaned.strip()


def _flatten_bullet_continuation_lines(block: str) -> str:
    """Flatten continuation lines that do NOT start with '-' into their bullet line.

    After flattening, every line in the returned block starts with a '-'
    (or is blank). The indentation of each '-' line is preserved exactly
    as the user placed it.  No indentation recalculation is performed.
    """
    if not block:
        return block

    result: List[str] = []
    for raw_line in block.splitlines():
        stripped = raw_line.strip()
        if not stripped:
            if result and result[-1] != "":
                result.append("")
            continue
        # Detect if this line starts with a bullet marker.
        is_bullet = bool(re.match(r"^\s*-", stripped))
        if not is_bullet and result:
            # Continuation of the previous bullet line — merge.
            result[-1] = result[-1].rstrip() + " " + stripped
        else:
            # Preserve original leading whitespace for bullet lines.
            leading = raw_line[:len(raw_line) - len(raw_line.lstrip())]
            result.append(leading + stripped)

    return "\n".join(result)


def _wrap_preserved_bullet_block(block: str, width: int = 72) -> str:
    """Wrap bullet lines that exceed *width*, preserving dash indentation.

    Continuation lines are indented to dash-position + 2 (two spaces past
    the '-' character itself, i.e. one space past the space that follows
    the '-' character).  Lines that do NOT start with '-' are passed
    through unchanged.
    """
    if not block:
        return block

    wrapped: List[str] = []
    for raw_line in block.splitlines():
        line = raw_line.rstrip()
        if not line:
            wrapped.append("")
            continue
        if len(line) <= width:
            wrapped.append(line)
            continue

        # Identify bullet prefix: leading whitespace + '-' + whitespace.
        bullet_match = re.match(r"^(\s*)(-\s+)(.*)", line)
        if bullet_match:
            leading = bullet_match.group(1)
            marker = bullet_match.group(2)
            body = bullet_match.group(3)
            dash_prefix = leading + marker
            # dash_position is the column where '-' sits (len of leading).
            # Continuation indent = dash_position + 2 (one past the space after '-').
            cont_indent = " " * (len(leading) + 2)
            wrapped_lines = textwrap.wrap(
                body,
                width=width,
                initial_indent=dash_prefix,
                subsequent_indent=cont_indent,
                break_long_words=False,
                break_on_hyphens=False,
            )
            wrapped.extend(wrapped_lines)
        else:
            wrapped.append(line)

    return "\n".join(wrapped)


def _format_other_details_block(block: str, width: int = 72) -> str:
    """Flatten continuation lines then wrap long bullet lines.

    Step 1 — Flatten: merge non-bullet continuation lines into their
    preceding bullet line with a single space separator.
    Step 2 — Wrap: for bullet lines exceeding *width*, wrap with
    continuation indent = dash-position + 2.

    If the first bullet line has less than 2 spaces of indent while other
    bullets have >= 2, the first bullet is padded to 2 spaces to prevent
    it from being flush-left.
    """
    if not block:
        return block
    flattened = _flatten_bullet_continuation_lines(block)
    wrapped = _wrap_preserved_bullet_block(flattened, width)
    # Ensure the first bullet line has at least 2-space indent if others do.
    lines = wrapped.splitlines()
    if lines:
        first_match = re.match(r"^(-)(\s+.*)", lines[0].strip())
        if first_match and not lines[0].startswith("  -"):
            # First line has a bare '-' without indent, but other lines have indent.
            for line in lines[1:]:
                if re.match(r"^\s+-", line):
                    lines[0] = "  " + lines[0].strip()
                    break
    return "\n".join(lines)


def _reformat_other_details_section(txt_content: str, formatted_body: str) -> str:
    """Replace the OTHER DETAILS section body in *txt_content* with *formatted_body*.

    Finds "OTHER DETAILS" between two dividers, emits the pre-formatted body in its
    place including the top divider, then copies all remaining lines as-is.
    """
    if not formatted_body:
        return txt_content

    lines = txt_content.splitlines()
    output: List[str] = []
    i = 0
    found_other_details = False

    while i < len(lines):
        line = lines[i]
        stripped = line.strip()

        if (not found_other_details
                and stripped == "OTHER DETAILS"
                and i >= 1
                and i + 1 < len(lines)
                and lines[i - 1].strip().startswith("---")
                and lines[i + 1].strip().startswith("---")):
            found_other_details = True
            # The top divider (lines[i - 1]) was already output on the previous
            # loop iteration, so we do NOT re-emit it here.
            output.append("OTHER DETAILS")
            output.append(lines[i + 1])  # bottom divider
            output.append("")            # blank line
            for fline in formatted_body.splitlines():
                output.append(fline)
            # Ensure 3 blank lines before the next major section divider.
            _ensure_trailing_blank_count(output, 3)
            # Skip ALL original body lines until the next divider section.
            # The original body starts at i+2 (the line after the bottom divider).
            i += 2
            while i < len(lines):
                s = lines[i].strip()
                if s.startswith("---") or s.startswith("==="):
                    break
                i += 1
            # Continue main loop for remaining content.
            continue

        output.append(line)
        i += 1

    return "\n".join(output)


def remove_standalone_bullet_line(block: str) -> str:
    """Remove legacy '- Works standalone.' bullets from preserved README blocks.

    If extra text was accidentally appended on the same line, keep that tail text.
    """
    if not block:
        return block

    cleaned_lines: List[str] = []
    for raw_line in block.splitlines():
        line = raw_line.rstrip()
        match = re.match(r"^(\s*(?:[-*]|\d+\.)\s+)Works standalone\.\s*(.*)$", line)
        if not match:
            cleaned_lines.append(line)
            continue

        trailing = (match.group(2) or "").strip()
        if trailing:
            cleaned_lines.append(match.group(1) + trailing)

    return "\n".join(cleaned_lines).strip()


def remove_feature_pointer_lines(block: str) -> str:
    """Remove pointer-only bullets from FEATURES blocks.

    Keep these lines available for other contexts (for example publishing/image
    surfaces) by only stripping them from per-mod README FEATURES content.
    """
    if not block:
        return block

    pointer_patterns = (
        re.compile(r"^(\s*(?:[-*]|\d+\.)\s+)See this mod's README for full details\.\s*$", re.IGNORECASE),
        re.compile(r"^(\s*(?:[-*]|\d+\.)\s+)See Other Details below for full feature details\.\s*$", re.IGNORECASE),
    )

    cleaned_lines: List[str] = []
    for raw_line in block.splitlines():
        line = raw_line.rstrip()
        if any(pattern.match(line) for pattern in pointer_patterns):
            continue
        cleaned_lines.append(line)

    return "\n".join(cleaned_lines).strip()


def extract_mod_description_from_modinfo(modinfo_path: str) -> str:
    if not os.path.exists(modinfo_path):
        return ""
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        desc_tag = root.find("Description")
        if desc_tag is not None and "value" in desc_tag.attrib:
            return desc_tag.attrib["value"]
    except Exception:
        return ""
    return ""


def extract_one_line_summary_from_readme_txt(readme_txt_path: str) -> str:
    """Extract the one-line summary from README.txt (line below top H1 wrapper)."""
    if not os.path.exists(readme_txt_path):
        return ""
    try:
        with open(readme_txt_path, "r", encoding="utf-8") as f:
            lines = f.read().splitlines()
    except Exception:
        return ""

    if not lines:
        return ""

    start_idx = 0
    h1_dividers = [
        i for i, raw in enumerate(lines)
        if len(raw.strip()) >= 10 and re.fullmatch(r"=+", raw.strip())
    ]
    if len(h1_dividers) >= 2 and h1_dividers[1] > h1_dividers[0]:
        start_idx = h1_dividers[1] + 1

    end_idx = len(lines)
    for i in range(start_idx, len(lines)):
        stripped = lines[i].strip()
        if len(stripped) >= 10 and re.fullmatch(r"-+", stripped):
            end_idx = i
            break

    for i in range(start_idx, end_idx):
        stripped = lines[i].strip()
        if not stripped:
            continue
        if len(stripped) >= 10 and re.fullmatch(r"[=-]+", stripped):
            continue
        return re.sub(r"\s+", " ", stripped).strip()

    return ""


def resolve_mod_description(mod_path: str, modinfo_path: str) -> str:
    """Prefer README.txt one-line summary; fall back to ModInfo Description."""
    readme_summary = extract_one_line_summary_from_readme_txt(os.path.join(mod_path, "README.txt")).strip()
    if readme_summary:
        return readme_summary
    return extract_mod_description_from_modinfo(modinfo_path).strip()


def sync_modinfo_description_from_summary(
    modinfo_path: str,
    summary: str,
    dry_run: bool,
    log: "Logger",
) -> None:
    """Sync ModInfo.xml <Description value> to match the README summary when it differs."""
    summary = (summary or "").strip()
    if not summary or not os.path.exists(modinfo_path):
        return

    current = extract_mod_description_from_modinfo(modinfo_path).strip()
    if current == summary:
        return

    try:
        with open(modinfo_path, "r", encoding="utf-8") as f:
            xml_text = f.read()
    except Exception as ex:
        log.warn(f"Could not read ModInfo.xml for description sync ({modinfo_path}): {ex}")
        return

    escaped_summary = escape(summary, {'"': "&quot;"})
    updated_text, substitutions = re.subn(
        r'(<Description\s+value=")[^"]*("\s*/>)',
        rf'\1{escaped_summary}\2',
        xml_text,
        count=1,
    )
    if substitutions == 0:
        lines = xml_text.splitlines()
        if not lines:
            log.warn(f"Could not locate Description tag for sync in {modinfo_path}")
            return

        version_idx = next((i for i, line in enumerate(lines) if "<Version" in line), None)
        insert_idx: Optional[int] = None
        removed_orphan_text = False

        if version_idx is not None:
            insert_idx = version_idx + 1
            while insert_idx < len(lines):
                stripped = lines[insert_idx].strip()
                if not stripped:
                    insert_idx += 1
                    continue
                if stripped.startswith("<"):
                    break
                lines.pop(insert_idx)
                removed_orphan_text = True

        if insert_idx is None:
            for i, line in enumerate(lines):
                stripped = line.strip()
                if stripped.startswith("<Author") or stripped.startswith("<Website") or stripped.startswith("</xml"):
                    insert_idx = i
                    break

        if insert_idx is None:
            insert_idx = len(lines)

        desc_indent = "  "
        if insert_idx < len(lines):
            indent_match = re.match(r"^(\s*)<", lines[insert_idx])
            if indent_match:
                desc_indent = indent_match.group(1)
        elif version_idx is not None:
            indent_match = re.match(r"^(\s*)<", lines[version_idx])
            if indent_match:
                desc_indent = indent_match.group(1)

        lines.insert(insert_idx, f'{desc_indent}<Description value="{escaped_summary}" />')
        updated_text = "\n".join(lines)
        if xml_text.endswith("\n"):
            updated_text += "\n"

        if dry_run:
            msg = (
                "[DRYRUN] Would insert missing ModInfo Description from README summary for "
                f"{os.path.basename(os.path.dirname(modinfo_path))}"
            )
            if removed_orphan_text:
                msg += " (and remove malformed orphan text)"
            log.info(msg)
            return

        atomic_write_text(modinfo_path, updated_text, encoding="utf-8")
        msg = (
            "Inserted missing ModInfo Description from README summary for "
            f"{os.path.basename(os.path.dirname(modinfo_path))}"
        )
        if removed_orphan_text:
            msg += " and removed malformed orphan text"
        log.info(msg)
        return

    if dry_run:
        log.info(
            "[DRYRUN] Would sync ModInfo Description from README summary for "
            f"{os.path.basename(os.path.dirname(modinfo_path))}"
        )
        return

    atomic_write_text(modinfo_path, updated_text, encoding="utf-8")
    log.info(
        "Synced ModInfo Description from README summary for "
        f"{os.path.basename(os.path.dirname(modinfo_path))}"
    )


def zip_download_link(zip_name: str) -> str:
    return f"{BASE_DOWNLOAD_URL}/{zip_name}"


def mod_download_markdown_link(mod_name: str) -> str:
    base_name = get_base_mod_name(mod_name)
    return f"[{mod_name}]({zip_download_link(base_name + '.zip')})"


def extract_markdown_section(markdown_text: str, heading: str, next_heading: Optional[str] = None) -> str:
    start_match = re.search(rf"^##\s*{re.escape(heading)}\s*$", markdown_text, re.MULTILINE)
    if not start_match:
        return markdown_text.strip()

    body_start = start_match.end()
    if not next_heading:
        return markdown_text[body_start:].strip()

    end_match = re.search(
        rf"^##\s*{re.escape(next_heading)}\s*$",
        markdown_text[body_start:],
        re.MULTILINE,
    )
    if not end_match:
        return markdown_text[body_start:].strip()

    body_end = body_start + end_match.start()
    return markdown_text[body_start:body_end].strip()


def render_discord_post_from_template(
    release_version: str,
    now_iso: str,
    versioned_zip_name: str,
    previous_release_version: Optional[str],
    new_mod_entries: List[Dict[str, str]],
    updated_mod_entries: List[Dict[str, str]],
    renamed_mod_entries: List[Dict[str, str]],
    removed_mod_entries: List[Dict[str, str]],
    log: Logger,
) -> str:
    if not os.path.exists(DISCORD_TEMPLATE_PATH):
        log.warn(f"Discord template missing: {DISCORD_TEMPLATE_PATH}. Using fallback format.")
        return ""

    try:
        with open(DISCORD_TEMPLATE_PATH, "r", encoding="utf-8") as f:
            template_text = f.read()
    except Exception as ex:
        log.warn(f"Could not read Discord template {DISCORD_TEMPLATE_PATH}: {ex}. Using fallback format.")
        return ""

    template_start_marker = "<!-- DISCORD_TEMPLATE_START -->"
    template_end_marker = "<!-- DISCORD_TEMPLATE_END -->"
    if template_start_marker in template_text and template_end_marker in template_text:
        body = template_text.split(template_start_marker, 1)[1].split(template_end_marker, 1)[0].strip()
    else:
        body = extract_markdown_section(template_text, "Copy/Paste Format", "Notes")
    canonical_download_url = zip_download_link(GIGGLEPACK_CANONICAL_ZIP)
    release_file_label = f"GigglePack-v{release_version}.zip"
    static_map = {
        "GIGGLEPACK_VERSION": release_version,
        "RELEASE_STAMP": now_iso,
        "VERSIONED_ZIP_URL": canonical_download_url,
        # Compatibility aliases for older/custom templates.
        # latest zip is deprecated, so map these to the canonical download URL.
        "LATEST_ZIP_URL": canonical_download_url,
        "DOWNLOAD_URL": canonical_download_url,
        "DOWNLOAD_FILE_LABEL": release_file_label,
        "NEW_COUNT": str(len(new_mod_entries)),
        "UPDATED_COUNT": str(len(updated_mod_entries)),
        "RENAMED_COUNT": str(len(renamed_mod_entries)),
        "REMOVED_COUNT": str(len(removed_mod_entries)),
        "PREVIOUS_GIGGLEPACK_VERSION": previous_release_version or "None",
        "NEW_MOD_LINES": "\n".join(
            f"  - {mod_download_markdown_link(e['mod'])} (new: v{e.get('to_display', e['to'])})"
            for e in new_mod_entries
        )
        or "  - None",
        "UPDATED_MOD_LINES": "\n".join(
            f"  - {mod_download_markdown_link(e['mod'])} "
            f"(v{e.get('from_display', e['from'])} -> v{e.get('to_display', e['to'])})"
            for e in updated_mod_entries
        )
        or "  - None",
        "RENAMED_MOD_LINES": "\n".join(
            f"  - {mod_download_markdown_link(e['to_mod'])} "
            f"(renamed from {e['from_mod']}, v{e.get('version_display', e['version'])})"
            for e in renamed_mod_entries
        )
        or "  - None",
        "REMOVED_MOD_LINES": "\n".join(
            f"  - {e['mod']} (was v{e.get('from_display', e['from'])})" for e in removed_mod_entries
        )
        or "  - None",
    }

    def token_value(token: str) -> str:
        if token in static_map:
            return static_map[token]

        new_mod_match = re.fullmatch(r"NEW_MOD_(\d+)", token)
        if new_mod_match:
            idx = int(new_mod_match.group(1)) - 1
            return new_mod_entries[idx]["mod"] if 0 <= idx < len(new_mod_entries) else "None"

        new_mod_ver_match = re.fullmatch(r"NEW_MOD_(\d+)_VERSION", token)
        if new_mod_ver_match:
            idx = int(new_mod_ver_match.group(1)) - 1
            return (
                new_mod_entries[idx].get("to_display", new_mod_entries[idx]["to"])
                if 0 <= idx < len(new_mod_entries)
                else "None"
            )

        updated_mod_match = re.fullmatch(r"UPDATED_MOD_(\d+)", token)
        if updated_mod_match:
            idx = int(updated_mod_match.group(1)) - 1
            return updated_mod_entries[idx]["mod"] if 0 <= idx < len(updated_mod_entries) else "None"

        updated_old_match = re.fullmatch(r"UPDATED_MOD_(\d+)_OLD", token)
        if updated_old_match:
            idx = int(updated_old_match.group(1)) - 1
            return (
                updated_mod_entries[idx].get("from_display", updated_mod_entries[idx]["from"])
                if 0 <= idx < len(updated_mod_entries)
                else "None"
            )

        updated_new_match = re.fullmatch(r"UPDATED_MOD_(\d+)_NEW", token)
        if updated_new_match:
            idx = int(updated_new_match.group(1)) - 1
            return (
                updated_mod_entries[idx].get("to_display", updated_mod_entries[idx]["to"])
                if 0 <= idx < len(updated_mod_entries)
                else "None"
            )

        removed_mod_match = re.fullmatch(r"REMOVED_MOD_(\d+)", token)
        if removed_mod_match:
            idx = int(removed_mod_match.group(1)) - 1
            return removed_mod_entries[idx]["mod"] if 0 <= idx < len(removed_mod_entries) else "None"

        removed_ver_match = re.fullmatch(r"REMOVED_MOD_(\d+)_VERSION", token)
        if removed_ver_match:
            idx = int(removed_ver_match.group(1)) - 1
            return (
                removed_mod_entries[idx].get("from_display", removed_mod_entries[idx]["from"])
                if 0 <= idx < len(removed_mod_entries)
                else "None"
            )

        return "None"

    rendered = re.sub(r"\{\{([A-Z0-9_]+)\}\}", lambda m: token_value(m.group(1)), body)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(new:\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(vNone\s*->\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(renamed from None,\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"(?m)^(\s*-\s*)None\s*\(was\s*vNone\)\s*$", r"\1None", rendered)
    rendered = re.sub(r"\n{3,}", "\n\n", rendered).strip()
    return rendered + "\n"


def get_mod_bases_for_dirs(mod_dirs: Tuple[str, ...]) -> set[str]:
    mod_bases: set[str] = set()
    for mod_dir in mod_dirs:
        for folder_name in scan_mod_folders(mod_dir):
            mod_bases.add(get_base_mod_name(folder_name))
    return mod_bases


def build_readme_metadata_index(
    csv_rows: List[Dict[str, str]],
    target_mod_bases: set[str],
    log: Logger,
) -> Dict[str, Dict[str, str]]:
    csv_map: Dict[str, Dict[str, str]] = {}
    for row in csv_rows:
        mod_name = row.get("MOD_NAME", "").strip()
        if mod_name:
            csv_map[mod_name] = dict(row)

    compat_data: Dict[str, Dict[str, str]] = {}
    missing_in_csv: List[str] = []
    for base_name in sorted(target_mod_bases):
        row = dict(csv_map.get(base_name, {}))
        if not row:
            missing_in_csv.append(base_name)

        row["MOD_NAME"] = base_name
        for field_name in README_COMPAT_FIELDS:
            if not row.get(field_name):
                if field_name == "QUOTE_FILE":
                    row[field_name] = f"{base_name}.txt"
                else:
                    row[field_name] = "MISSINGDATA"
        compat_data[base_name] = row

    if missing_in_csv:
        preview = ", ".join(missing_in_csv[:5])
        extra = "" if len(missing_in_csv) <= 5 else f" (+{len(missing_in_csv) - 5} more)"
        log.warn(
            "README metadata is missing in HELPER_ModCompatibility.csv for: "
            f"{preview}{extra}. Using MISSINGDATA defaults."
        )

    return compat_data


# =============================================================
# STEP 1 + 2: SCAN + SYNC
# =============================================================
def sync_workspace_and_game(dry_run: bool, log: Logger) -> List[Tuple[str, str]]:
    log.info("Step 1/2: Scan mod folders and sync by version")

    pub_folders = scan_mod_folders(PUBLISH_READY)
    inprog_folders = scan_mod_folders(IN_PROGRESS)
    game_folders = scan_mod_folders(GAME_MODS)

    log.stats.scanned_workspace_mods = len(pub_folders) + len(inprog_folders)
    log.stats.scanned_game_mods = len(game_folders)

    ws_by_base: Dict[str, Tuple[str, str]] = {}
    for folder, path in {**pub_folders, **inprog_folders}.items():
        ws_by_base[get_base_mod_name(folder)] = (folder, path)

    game_by_base: Dict[str, Tuple[str, str]] = {}
    for folder, path in game_folders.items():
        game_by_base[get_base_mod_name(folder)] = (folder, path)

    mods_pulled_from_game: List[Tuple[str, str]] = []

    for base_name, (ws_folder, ws_path) in ws_by_base.items():
        if base_name not in game_by_base:
            continue

        game_folder, game_path = game_by_base[base_name]
        if is_4modders_mod(ws_folder) or is_4modders_mod(game_folder):
            log.info(
                f"Skipping game-root auto-sync for optional 4Modders mod: {ws_folder} / {game_folder}"
            )
            continue
        if is_requested_mod(ws_folder) or is_requested_mod(game_folder):
            log.info(
                f"Skipping game-root auto-sync for optional Requested mod: {ws_folder} / {game_folder}"
            )
            continue

        ws_ver = get_modinfo_version(ws_path)
        game_ver = get_modinfo_version(game_path)

        if ws_ver is None or game_ver is None:
            log.warn(
                f"Skipping sync for {base_name}: missing/unreadable ModInfo.xml "
                f"(workspace={ws_ver}, game={game_ver})"
            )
            continue

        cmp_value = compare_versions(ws_ver, game_ver)

        if cmp_value < 0:
            log.info(
                f"Game is newer for {base_name}: game={game_folder} v{game_ver}, "
                f"workspace={ws_folder} v{ws_ver}. Pulling from game."
            )
            if maybe_copytree(game_path, ws_path, dry_run, log):
                mods_pulled_from_game.append((ws_folder, ws_path))
                log.stats.synced_pull_from_game += 1
                if maybe_remove_dir(game_path, dry_run, log):
                    log.info(f"Removed game mod after pull: {game_folder}")

        elif cmp_value > 0:
            new_game_dest = os.path.join(GAME_MODS, ws_folder)
            log.info(
                f"Workspace is newer for {base_name}: workspace={ws_folder} v{ws_ver}, "
                f"game={game_folder} v{game_ver}. Pushing to game."
            )
            if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(ws_path, new_game_dest, dry_run, log):
                log.stats.synced_push_to_game += 1

        else:
            try:
                ws_hash = hash_directory(ws_path)
                game_hash = hash_directory(game_path)
                if ws_hash != game_hash:
                    # Same version but different bytes: keep game folder aligned to workspace source-of-truth.
                    new_game_dest = os.path.join(GAME_MODS, ws_folder)
                    if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(ws_path, new_game_dest, dry_run, log):
                        log.stats.synced_push_to_game += 1
                        log.info(
                            f"Version tie resolved for {base_name}: both v{ws_ver} but content differed. "
                            "Pushed workspace copy to game."
                        )
                    else:
                        log.stats.sync_conflicts += 1
                        log.warn(
                            f"Version tie conflict for {base_name}: both v{ws_ver} but content differs. "
                            "Auto-push to game failed."
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare tied versions for {base_name}: {ex}")

    # Mirror 4Modders mods into game optionals folder in full mode as non-root installs.
    optionals_4modders_path = os.path.join(GAME_MODS, GAME_OPTIONALS_4MODDERS_DIR)
    if dry_run:
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_4modders_path}")
    else:
        os.makedirs(optionals_4modders_path, exist_ok=True)

    workspace_sources = {**pub_folders, **inprog_folders}
    for folder, ws_path in workspace_sources.items():
        if not is_4modders_mod(folder):
            continue
        if maybe_copytree(ws_path, os.path.join(optionals_4modders_path, folder), dry_run, log):
            log.info(f"sync mirror: 4Modders optional updated: {folder}")

    # Mirror Requested mods into game optionals folder in full mode as non-root installs.
    optionals_requested_path = os.path.join(GAME_MODS, GAME_OPTIONALS_REQUESTED_DIR)
    if dry_run:
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_requested_path}")
    else:
        os.makedirs(optionals_requested_path, exist_ok=True)

    for folder, ws_path in workspace_sources.items():
        if not is_requested_mod(folder):
            continue
        if maybe_copytree(ws_path, os.path.join(optionals_requested_path, folder), dry_run, log):
            log.info(f"sync mirror: Requested optional updated: {folder}")

    return mods_pulled_from_game


# =============================================================
# STEP 3: MOVE BY MAJOR VERSION
# =============================================================
def move_mods_by_major_version(dry_run: bool, log: Logger) -> None:
    log.info("Step 3: Move mods between In-Progress and PublishReady by major version")

    pub_folders = scan_mod_folders(PUBLISH_READY)
    inprog_folders = scan_mod_folders(IN_PROGRESS)
    all_mods = set(pub_folders) | set(inprog_folders)

    for folder_name in all_mods:
        pub_path = pub_folders.get(folder_name)
        inprog_path = inprog_folders.get(folder_name)
        mod_path = pub_path or inprog_path
        if not mod_path:
            continue

        version = get_modinfo_version(mod_path)
        if not version or "." not in version:
            log.warn(f"Skipping major-version move for {folder_name}: invalid version '{version}'")
            continue

        try:
            major = int(version.split(".", 1)[0])
        except ValueError:
            log.warn(f"Skipping major-version move for {folder_name}: non-numeric major '{version}'")
            continue

        if major == 0 and pub_path:
            dest = os.path.join(IN_PROGRESS, folder_name)
            if maybe_move(pub_path, dest, dry_run, log):
                log.stats.moved_to_in_progress += 1
                log.info(f"Moved to In-Progress: {folder_name} (v{version})")

        elif major >= 1 and inprog_path:
            dest = os.path.join(PUBLISH_READY, folder_name)
            if maybe_move(inprog_path, dest, dry_run, log):
                log.stats.moved_to_publish_ready += 1
                log.info(f"Moved to PublishReady: {folder_name} (v{version})")


def sync_publishready_to_staging_latest(dry_run: bool, log: Logger) -> None:
    """Ensure ActiveBuild has the latest released versions by base mod name.

    Rules:
    - If a release mod is missing in ActiveBuild, copy it in.
    - If release version is higher than ActiveBuild version, replace ActiveBuild copy.
    - If ActiveBuild is higher, keep ActiveBuild and warn.
    - If versions tie and content differs, keep ActiveBuild and warn.
    """
    log.info("Step 3.5: Ensure latest ReleaseSource mods are in ActiveBuild")

    publish_folders = scan_mod_folders(PUBLISH_READY)
    staging_folders = scan_mod_folders(STAGING)

    publish_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in publish_folders.items()
    }
    staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in staging_folders.items()
    }

    for base_name in sorted(publish_by_base.keys()):
        pub_folder, pub_path = publish_by_base[base_name]
        pub_ver = get_modinfo_version(pub_path)
        if pub_ver is None:
            log.warn(f"ActiveBuild sync skipped for {pub_folder}: unreadable release ModInfo.xml")
            continue

        if base_name not in staging_by_base:
            dest = os.path.join(STAGING, pub_folder)
            if maybe_copytree(pub_path, dest, dry_run, log):
                log.info(f"ActiveBuild sync add: {pub_folder} v{pub_ver}")
            continue

        st_folder, st_path = staging_by_base[base_name]
        st_ver = get_modinfo_version(st_path)
        if st_ver is None:
            log.warn(f"ActiveBuild sync skipped for {st_folder}: unreadable active ModInfo.xml")
            continue

        cmp_value = compare_versions(pub_ver, st_ver)
        if cmp_value > 0:
            dest = os.path.join(STAGING, pub_folder)
            if maybe_remove_dir(st_path, dry_run, log) and maybe_copytree(pub_path, dest, dry_run, log):
                log.info(f"ActiveBuild sync update: {st_folder} v{st_ver} -> {pub_folder} v{pub_ver}")
        elif cmp_value < 0:
            log.warn(
                f"ActiveBuild sync kept newer active mod for {base_name}: "
                f"active v{st_ver} > release v{pub_ver}"
            )
        else:
            try:
                pub_hash = hash_directory(pub_path)
                st_hash = hash_directory(st_path)
                if pub_hash != st_hash:
                    log.warn(
                        f"ActiveBuild sync conflict for {base_name}: both v{pub_ver} but content differs. "
                        "Keeping ActiveBuild copy."
                    )
            except Exception as ex:
                log.warn(f"Could not hash compare release/active tie for {base_name}: {ex}")


def sync_game_and_draft(dry_run: bool, log: Logger) -> List[Tuple[str, str]]:
    """Sync updates from Game into Draft for AGF mods present in both lanes.

    This keeps Draft current when a mod was originally tracked in Draft but got
    updated while testing in the game Mods folder.
    """
    log.info("Step 0.25: Compare Game <-> Draft and pull newer game updates into Draft")

    draft_folders = scan_mod_folders(IN_PROGRESS)
    game_folders = scan_mod_folders(GAME_MODS)

    draft_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in draft_folders.items()
    }
    game_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in game_folders.items()
    }

    mods_pulled_from_game: List[Tuple[str, str]] = []

    overlap = sorted(set(draft_by_base.keys()) & set(game_by_base.keys()))
    for base_name in overlap:
        draft_folder, draft_path = draft_by_base[base_name]
        game_folder, game_path = game_by_base[base_name]

        draft_ver = get_modinfo_version(draft_path)
        game_ver = get_modinfo_version(game_path)

        if draft_ver is None or game_ver is None:
            log.warn(
                f"Draft/game sync skipped for {base_name}: missing/unreadable ModInfo.xml "
                f"(draft={draft_ver}, game={game_ver})"
            )
            continue

        cmp_value = compare_versions(game_ver, draft_ver)
        if cmp_value > 0:
            if maybe_copytree(game_path, draft_path, dry_run, log):
                log.stats.synced_pull_from_game += 1
                mods_pulled_from_game.append((draft_folder, draft_path))
                log.info(
                    f"Draft/game pull: {game_folder} v{game_ver} -> {draft_folder} v{draft_ver}"
                )
        elif cmp_value == 0:
            try:
                draft_hash = hash_directory(draft_path)
                game_hash = hash_directory(game_path)
                if draft_hash != game_hash:
                    if maybe_copytree(game_path, draft_path, dry_run, log):
                        log.stats.synced_pull_from_game += 1
                        mods_pulled_from_game.append((draft_folder, draft_path))
                        log.info(
                            f"Draft/game tie refresh for {base_name}: both v{game_ver} but content differed. "
                            "Pulled game copy into Draft."
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare game/draft tie for {base_name}: {ex}")

    return mods_pulled_from_game


def reset_activebuild_to_draft(dry_run: bool, log: Logger) -> None:
    """Move all ActiveBuild AGF mods into Draft and reset Draft promotion baseline."""
    log.info("Lane reset: move all ActiveBuild AGF mods to Draft")

    staging_folders = scan_mod_folders(STAGING)
    if not staging_folders:
        log.info("Lane reset: no ActiveBuild AGF mods found to move")
        return

    draft_folders = scan_mod_folders(IN_PROGRESS)
    draft_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in draft_folders.items()
    }

    moved_count = 0
    for st_folder in sorted(staging_folders.keys()):
        st_path = staging_folders[st_folder]
        base_name = get_base_mod_name(st_folder)

        existing_draft = draft_by_base.get(base_name)
        if existing_draft:
            draft_folder, draft_path = existing_draft
            if maybe_remove_dir(draft_path, dry_run, log):
                log.info(
                    f"Lane reset: removed existing Draft copy {draft_folder} before moving {st_folder}"
                )
            else:
                continue

        dest = os.path.join(IN_PROGRESS, st_folder)
        if maybe_move(st_path, dest, dry_run, log):
            moved_count += 1
            log.stats.moved_to_in_progress += 1
            log.info(f"Lane reset move: {st_folder} ActiveBuild -> Draft")

    refresh_draft_promotion_baseline_from_lane(dry_run, log)
    log.info(f"Lane reset complete: moved {moved_count} mod(s) from ActiveBuild to Draft")


def enforce_staging_major_policy(dry_run: bool, log: Logger) -> None:
    """Ensure ActiveBuild only contains major-version >= 1 mods.

    Any ActiveBuild mod on major 0 is moved back to Draft. If Draft already has
    that base mod, keep whichever version is newer there and remove the other copy.
    """
    log.info("Lane policy: keep major v0.x mods in Draft, not ActiveBuild")

    staging_folders = scan_mod_folders(STAGING)
    draft_folders = scan_mod_folders(IN_PROGRESS)
    draft_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in draft_folders.items()
    }

    for st_folder, st_path in staging_folders.items():
        st_ver = get_modinfo_version(st_path)
        if st_ver is None:
            log.warn(f"Lane policy skipped for {st_folder}: unreadable active ModInfo.xml")
            continue

        try:
            st_major = int((st_ver or "0.0.0").split(".", 1)[0])
        except Exception:
            st_major = 0

        if st_major >= 1:
            continue

        base_name = get_base_mod_name(st_folder)
        draft_match = draft_by_base.get(base_name)

        if not draft_match:
            dest = os.path.join(IN_PROGRESS, st_folder)
            if maybe_move(st_path, dest, dry_run, log):
                log.info(f"Lane policy move: {st_folder} v{st_ver} moved ActiveBuild -> Draft")
            continue

        draft_folder, draft_path = draft_match
        draft_ver = get_modinfo_version(draft_path)
        if draft_ver is None:
            log.warn(f"Lane policy skipped merge for {st_folder}: unreadable draft ModInfo.xml")
            continue

        cmp_value = compare_versions(st_ver, draft_ver)
        if cmp_value > 0:
            replacement_dest = os.path.join(IN_PROGRESS, st_folder)
            if maybe_remove_dir(draft_path, dry_run, log) and maybe_move(st_path, replacement_dest, dry_run, log):
                log.info(
                    f"Lane policy replace: kept newer v0 draft copy from ActiveBuild "
                    f"({st_folder} v{st_ver} > {draft_folder} v{draft_ver})"
                )
        elif cmp_value < 0:
            if maybe_remove_dir(st_path, dry_run, log):
                log.info(
                    f"Lane policy cleanup: removed ActiveBuild v0 copy {st_folder} v{st_ver}; "
                    f"Draft already newer ({draft_folder} v{draft_ver})"
                )
        else:
            try:
                st_hash = hash_directory(st_path)
                draft_hash = hash_directory(draft_path)
                if st_hash != draft_hash:
                    replacement_dest = os.path.join(IN_PROGRESS, st_folder)
                    if maybe_remove_dir(draft_path, dry_run, log) and maybe_move(st_path, replacement_dest, dry_run, log):
                        log.info(
                            f"Lane policy tie refresh: replaced Draft copy with ActiveBuild copy for {base_name} "
                            f"at v{st_ver}, then removed ActiveBuild copy"
                        )
                else:
                    if maybe_remove_dir(st_path, dry_run, log):
                        log.info(
                            f"Lane policy cleanup: removed duplicate ActiveBuild v0 copy {st_folder} "
                            f"(matching Draft v{draft_ver})"
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare lane-policy tie for {base_name}: {ex}")


def sync_draft_to_staging_latest(dry_run: bool, log: Logger) -> None:
    """Promote Draft mods into ActiveBuild only on major-version upgrade.

    Rules:
    - Draft major < 1 never promotes to ActiveBuild.
    - If no ActiveBuild copy exists and draft major >= 1, add it.
    - If ActiveBuild exists, promote only when draft major > active major.
    - Minor/patch-only updates in Draft are intentionally held in Draft.
    """
    log.info("Step 0.5: Promote Draft mods to ActiveBuild only on major-version upgrade")

    draft_folders = scan_mod_folders(IN_PROGRESS)
    staging_folders = scan_mod_folders(STAGING)

    draft_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in draft_folders.items()
    }
    staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in staging_folders.items()
    }

    baseline = load_draft_promotion_baseline(log)
    baseline_changed = False

    for base_name in sorted(draft_by_base.keys()):
        draft_folder, draft_path = draft_by_base[base_name]
        draft_ver = get_modinfo_version(draft_path)
        if draft_ver is None:
            log.warn(f"Draft sync skipped for {draft_folder}: unreadable draft ModInfo.xml")
            continue

        baseline_entry = baseline.get(base_name)
        baseline_ver = ""
        baseline_major: Optional[int] = None
        if isinstance(baseline_entry, dict):
            baseline_ver = str(baseline_entry.get("version", ""))
            try:
                baseline_major = int(baseline_ver.split(".", 1)[0])
            except Exception:
                baseline_major = None

        try:
            draft_major = int((draft_ver or "0.0.0").split(".", 1)[0])
        except Exception:
            draft_major = 0

        # Keep baseline in sync while the mod is still draft-only (major < 1).
        # This ensures the first v1 major bump is recognized as promotable.
        if draft_major < 1:
            if baseline_major is None or baseline_ver != draft_ver:
                baseline[base_name] = {
                    "folder": draft_folder,
                    "version": draft_ver,
                    "recorded_at": dt.datetime.now().isoformat(timespec="seconds"),
                }
                baseline_changed = True
                if baseline_major is None:
                    log.info(
                        f"Draft baseline recorded for {draft_folder} v{draft_ver} "
                        "while draft-only (major < 1)"
                    )
                else:
                    log.info(
                        f"Draft baseline updated for {draft_folder} v{draft_ver} "
                        "while draft-only (major < 1)"
                    )
            log.info(
                f"Draft sync skipped for {draft_folder}: version {draft_ver} is draft-only (major < 1)"
            )
            continue

        # First sighting in Draft records current version and waits for a future major bump.
        if baseline_major is None:
            baseline[base_name] = {
                "folder": draft_folder,
                "version": draft_ver,
                "recorded_at": dt.datetime.now().isoformat(timespec="seconds"),
            }
            baseline_changed = True
            log.info(
                f"Draft baseline recorded for {draft_folder} v{draft_ver}; "
                "will promote after a future major bump"
            )
            continue

        if draft_major <= baseline_major:
            log.info(
                f"Draft hold: {draft_folder} v{draft_ver} not promoted "
                f"(baseline major {baseline_major}; waiting for major increase)"
            )
            continue

        remove_draft_after_sync = False

        if base_name not in staging_by_base:
            dest = os.path.join(STAGING, draft_folder)
            if maybe_copytree(draft_path, dest, dry_run, log):
                log.info(f"Draft promotion add: {draft_folder} v{draft_ver} (no existing ActiveBuild copy)")
                remove_draft_after_sync = True
        else:
            st_folder, st_path = staging_by_base[base_name]
            st_ver = get_modinfo_version(st_path)
            if st_ver is None:
                log.warn(f"Draft sync skipped for {st_folder}: unreadable active ModInfo.xml")
                continue

            try:
                st_major = int((st_ver or "0.0.0").split(".", 1)[0])
            except Exception:
                st_major = 0

            if draft_major > st_major:
                dest = os.path.join(STAGING, draft_folder)
                if maybe_remove_dir(st_path, dry_run, log) and maybe_copytree(draft_path, dest, dry_run, log):
                    log.info(
                        f"Draft promotion update: {st_folder} v{st_ver} -> {draft_folder} v{draft_ver} "
                        f"(major {st_major} -> {draft_major})"
                    )
                    remove_draft_after_sync = True
            else:
                log.info(
                    f"Draft hold: {draft_folder} v{draft_ver} not promoted because major did not increase "
                    f"(ActiveBuild {st_folder} v{st_ver})"
                )

        if remove_draft_after_sync and maybe_remove_dir(draft_path, dry_run, log):
            baseline[base_name] = {
                "folder": draft_folder,
                "version": draft_ver,
                "recorded_at": dt.datetime.now().isoformat(timespec="seconds"),
            }
            baseline_changed = True
            log.info(
                f"Draft promotion cleanup: removed {draft_folder} v{draft_ver} from Draft "
                "after syncing to ActiveBuild"
            )

    if baseline_changed:
        save_draft_promotion_baseline(baseline, dry_run, log)


# =============================================================
# STEP 4: RENAME + CSV + QUOTES + MOD README
# =============================================================
def rename_mod_folders_to_modinfo(
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
) -> List[Tuple[str, str, str]]:
    log.info("Rename folders to match ModInfo Name+Version when safe")

    folder_renames: List[Tuple[str, str, str]] = []
    target_dirs = mod_dirs if mod_dirs is not None else (PUBLISH_READY, IN_PROGRESS)
    for mod_dir in target_dirs:
        folders = scan_mod_folders(mod_dir)
        for folder_name, folder_path in folders.items():
            modinfo_path = os.path.join(folder_path, "ModInfo.xml")
            if not os.path.exists(modinfo_path):
                continue

            mod_name, mod_version = parse_modinfo(modinfo_path, folder_name)
            if not is_agf_mod(mod_name):
                log.warn(f"Rename skipped for {folder_name}: ModInfo Name does not start with AGF/zzzAGF")
                continue

            folder_base_name = get_base_mod_name(folder_name)
            name_changed = folder_base_name != mod_name
            version_changed = has_version_drift(folder_name, mod_version)

            # Policy: name changes are only applied when accompanied by a version bump/change.
            if name_changed and not version_changed:
                log.warn(
                    f"Rename skipped for {folder_name}: ModInfo Name changed but version did not. "
                    "Bump version to apply name change."
                )
                continue

            target_name = f"{mod_name}-v{mod_version}"
            if target_name == folder_name:
                continue

            src = folder_path
            dst = os.path.join(mod_dir, target_name)
            if os.path.exists(dst):
                log.warn(f"Rename skipped due to collision: {folder_name} -> {target_name}")
                continue

            if maybe_move(src, dst, dry_run, log):
                folder_renames.append((folder_name, target_name, mod_dir))
                log.stats.renamed_folders += 1
                log.info(f"Renamed folder: {folder_name} -> {target_name}")

    return folder_renames


def plan_mod_folder_renames(mod_dirs: Tuple[str, ...], log: Logger) -> List[Tuple[str, str, str]]:
    """Plan folder renames without modifying files. Mirrors rename policy and collision guard."""
    planned: List[Tuple[str, str, str]] = []
    for mod_dir in mod_dirs:
        folders = scan_mod_folders(mod_dir)
        for folder_name, folder_path in folders.items():
            modinfo_path = os.path.join(folder_path, "ModInfo.xml")
            if not os.path.exists(modinfo_path):
                continue

            mod_name, mod_version = parse_modinfo(modinfo_path, folder_name)
            if not is_agf_mod(mod_name):
                continue

            folder_base_name = get_base_mod_name(folder_name)
            name_changed = folder_base_name != mod_name
            version_changed = has_version_drift(folder_name, mod_version)

            if name_changed and not version_changed:
                continue

            target_name = f"{mod_name}-v{mod_version}"
            if target_name == folder_name:
                continue

            dst = os.path.join(mod_dir, target_name)
            if os.path.exists(dst):
                continue

            planned.append((folder_name, target_name, mod_dir))

    return planned


def load_compat_csv() -> Tuple[List[str], List[Dict[str, str]]]:
    default_fields = list(COMPAT_CSV_FIELD_ORDER)
    if not os.path.exists(COMPAT_CSV):
        return default_fields, []

    def normalize_header_name(header: str) -> str:
        return header.lstrip("\ufeff").strip().strip('"')

    with open(COMPAT_CSV, "r", encoding="utf-8", newline="") as f:
        reader = csv.DictReader(f)
        raw_fieldnames = list(reader.fieldnames) if reader.fieldnames else default_fields
        fieldnames = [normalize_header_name(fn) for fn in raw_fieldnames]

        rows: List[Dict[str, str]] = []
        for raw_row in reader:
            clean_row: Dict[str, str] = {}
            for key, value in raw_row.items():
                if key is None:
                    continue
                clean_key = normalize_header_name(key)
                clean_row[clean_key] = value
            rows.append(clean_row)

    return fieldnames, rows


def save_compat_csv(fieldnames: List[str], rows: List[Dict[str, str]], dry_run: bool, log: Logger) -> None:
    def row_has_missingdata(row: Dict[str, str]) -> bool:
        for fn in fieldnames:
            value = str(row.get(fn, ""))
            normalized = value.strip().lower()
            if "missingdata" in normalized or normalized == "tbd":
                return True
            if fn == "MOD_TYPE_ID" and normalized == "0":
                return True
        return False

    rows.sort(key=lambda r: (0 if row_has_missingdata(r) else 1, r.get("MOD_NAME", "").lower()))
    if dry_run:
        log.info(f"[DRYRUN] Would write compatibility CSV: {COMPAT_CSV} ({len(rows)} rows)")
        return

    csv_buffer = io.StringIO(newline="")
    writer = csv.DictWriter(csv_buffer, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(rows)
    atomic_write_text(COMPAT_CSV, csv_buffer.getvalue(), encoding="utf-8", newline="")


def normalize_compat_csv(
    folder_renames: List[Tuple[str, str, str]],
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
    prune_to_mods_now: bool = True,
) -> List[Dict[str, str]]:
    log.info("Step 4.2: Normalize HELPER_ModCompatibility.csv")

    fieldnames, rows = load_compat_csv()
    if "MOD_NAME" not in fieldnames:
        fieldnames.insert(0, "MOD_NAME")
    if "TESTED_GAME_VERSION" not in fieldnames:
        insert_at = 1 if "MOD_NAME" in fieldnames else 0
        fieldnames.insert(insert_at, "TESTED_GAME_VERSION")
    if "QUOTE_FILE" not in fieldnames:
        fieldnames.append("QUOTE_FILE")

    compatibility_fields = [
        "EAC_FRIENDLY",
        "SERVER_SIDE_PLAYER",
        "SERVER_SIDE_DEDICATED",
        "CLIENT_SIDE",
        "MOD_TYPE_ID",
        "SAFE_TO_INSTALL",
        "SAFE_TO_REMOVE",
        "UNIQUE",
    ]
    for compat_field in compatibility_fields:
        if compat_field not in fieldnames:
            fieldnames.append(compat_field)

    # Keep a stable canonical CSV layout.
    ordered_known_fields = [fn for fn in COMPAT_CSV_FIELD_ORDER if fn in fieldnames]
    ordered_extra_fields = [fn for fn in fieldnames if fn not in COMPAT_CSV_FIELD_ORDER]
    fieldnames = ordered_known_fields + ordered_extra_fields

    rename_base_map = {
        get_base_mod_name(old): get_base_mod_name(new)
        for old, new, _ in folder_renames
    }

    scan_dirs = mod_dirs if mod_dirs is not None else (PUBLISH_READY, IN_PROGRESS)
    mods_now: set[str] = set()
    for mod_dir in scan_dirs:
        for folder_name in scan_mod_folders(mod_dir):
            mods_now.add(get_base_mod_name(folder_name))

    for row in rows:
        old = row.get("MOD_NAME", "")
        if old in rename_base_map:
            row["MOD_NAME"] = rename_base_map[old]

    before_non_agf_filter = len(rows)
    rows = [row for row in rows if is_agf_mod(row.get("MOD_NAME", "").strip())]
    dropped_non_agf = before_non_agf_filter - len(rows)
    if dropped_non_agf > 0:
        log.warn(f"Removed {dropped_non_agf} non-AGF rows from HELPER_ModCompatibility.csv")

    removed = 0
    if prune_to_mods_now:
        before = len(rows)
        rows = [row for row in rows if row.get("MOD_NAME") in mods_now]
        removed = before - len(rows)
        log.stats.csv_removed_rows += removed

    existing = {row.get("MOD_NAME") for row in rows}
    for mod in sorted(mods_now):
        if mod not in existing:
            new_row = {fn: "TBD" for fn in fieldnames}
            new_row["MOD_NAME"] = mod
            if "MOD_TYPE_ID" in fieldnames:
                new_row["MOD_TYPE_ID"] = "TBD"
            new_row["QUOTE_FILE"] = f"{mod}.txt"
            rows.append(new_row)
            log.stats.csv_added_rows += 1

    for row in rows:
        row["QUOTE_FILE"] = f"{row.get('MOD_NAME', 'TBD')}.txt"
        for fn in fieldnames:
            if not row.get(fn):
                row[fn] = "TBD"

    def _is_missing(value: str) -> bool:
        text = str(value or "").strip()
        return not text or text.upper() in {"MISSINGDATA", "TBD", "0"}

    for row in rows:
        legacy_server_side = str(row.get("SERVER_SIDE", "")).strip()
        server_side_player = str(row.get("SERVER_SIDE_PLAYER", "")).strip()
        server_side_dedicated = str(row.get("SERVER_SIDE_DEDICATED", "")).strip()

        # Legacy mapping: one SERVER_SIDE value feeds both new host columns.
        if _is_missing(server_side_player) and not _is_missing(legacy_server_side):
            row["SERVER_SIDE_PLAYER"] = legacy_server_side
            server_side_player = legacy_server_side
        if _is_missing(server_side_dedicated) and not _is_missing(legacy_server_side):
            row["SERVER_SIDE_DEDICATED"] = legacy_server_side

        client_required = str(row.get("CLIENT_REQUIRED", "")).strip()
        client_side = str(row.get("CLIENT_SIDE", "")).strip()

        if _is_missing(client_side) and not _is_missing(client_required):
            row["CLIENT_SIDE"] = client_required
            client_side = client_required

        mod_type_id = str(row.get("MOD_TYPE_ID", "") or "").strip()
        if not mod_type_id:
            row["MOD_TYPE_ID"] = "TBD"
            mod_type_id = "TBD"
        elif mod_type_id == "0":
            row["MOD_TYPE_ID"] = "TBD"
            mod_type_id = "TBD"
        if _is_missing(mod_type_id):
            continue

        derived_compat = MOD_TYPE_COMPAT_BY_ID.get(mod_type_id)
        if not derived_compat:
            log.warn(
                f"Invalid MOD_TYPE_ID '{mod_type_id}' for {row.get('MOD_NAME', 'UNKNOWN')}; keeping existing compatibility values"
            )
            continue

        for compat_field, compat_value in derived_compat.items():
            row[compat_field] = compat_value

    save_compat_csv(fieldnames, rows, dry_run, log)
    return rows


def normalize_quote_files(csv_rows: List[Dict[str, str]], folder_renames: List[Tuple[str, str, str]], dry_run: bool, log: Logger) -> None:
    log.info("Step 4.3: Normalize quote files")

    if dry_run:
        log.info(f"[DRYRUN] Would ensure quote directory exists: {QUOTES_DIR}")
    else:
        os.makedirs(QUOTES_DIR, exist_ok=True)

    for old_name, new_name, _ in folder_renames:
        old_base = get_base_mod_name(old_name)
        new_base = get_base_mod_name(new_name)
        old_quote = os.path.join(QUOTES_DIR, f"{old_base}.txt")
        new_quote = os.path.join(QUOTES_DIR, f"{new_base}.txt")
        if os.path.exists(old_quote) and not os.path.exists(new_quote):
            if dry_run:
                log.info(f"[DRYRUN] Would rename quote file: {old_quote} -> {new_quote}")
            else:
                os.rename(old_quote, new_quote)
            log.stats.quote_files_renamed += 1

    for row in csv_rows:
        quote_file = row.get("QUOTE_FILE", "")
        if not quote_file:
            continue
        quote_path = os.path.join(QUOTES_DIR, quote_file)

        if not os.path.exists(quote_path):
            if dry_run:
                log.info(f"[DRYRUN] Would create quote file: {quote_path}")
            else:
                atomic_write_text(quote_path, "", encoding="utf-8")
            log.stats.quote_files_created += 1
            continue

        try:
            with open(quote_path, "r", encoding="utf-8") as f:
                content = f.read()
            normalized = content.strip().lower()
            if normalized in {"none", "missingdata"}:
                if dry_run:
                    log.info(f"[DRYRUN] Would blank quote file containing placeholder text: {quote_path}")
                else:
                    atomic_write_text(quote_path, "", encoding="utf-8")
                log.stats.quote_files_blanked_none += 1
        except Exception as ex:
            log.warn(f"Failed reading/normalizing quote file {quote_path}: {ex}")


def update_mod_loaded_references_for_renames(
    folder_renames: List[Tuple[str, str, str]],
    dry_run: bool,
    log: Logger,
    scan_dirs: Tuple[str, ...] = (IN_PROGRESS, STAGING),
) -> None:
    """Update mod_loaded('old') -> mod_loaded('new') across workspace XML files for name renames."""
    rename_pairs: List[Tuple[str, str]] = []
    seen_pairs: set[Tuple[str, str]] = set()
    for old_name, new_name, _ in folder_renames:
        old_base = get_base_mod_name(old_name)
        new_base = get_base_mod_name(new_name)
        if old_base == new_base:
            continue
        pair = (old_base, new_base)
        if pair in seen_pairs:
            continue
        seen_pairs.add(pair)
        rename_pairs.append(pair)

    if not rename_pairs:
        return

    log.info("Updating mod_loaded references for renamed mods")
    renamed_bases: set[str] = set()
    for old_base, new_base in rename_pairs:
        renamed_bases.add(old_base)
        renamed_bases.add(new_base)
    touched_other_mod_bases: set[str] = set()

    for scan_dir in scan_dirs:
        if not os.path.isdir(scan_dir):
            continue

        for root, _, files in os.walk(scan_dir):
            for filename in files:
                if not filename.lower().endswith(".xml"):
                    continue

                file_path = os.path.join(root, filename)
                try:
                    with open(file_path, "r", encoding="utf-8") as f:
                        content = f.read()
                except Exception as ex:
                    log.warn(f"Failed reading XML for mod_loaded update: {file_path}: {ex}")
                    continue

                updated_content = content
                replacement_count = 0
                for old_base, new_base in rename_pairs:
                    old_token = f"mod_loaded('{old_base}')"
                    new_token = f"mod_loaded('{new_base}')"
                    count = updated_content.count(old_token)
                    if count:
                        updated_content = updated_content.replace(old_token, new_token)
                        replacement_count += count

                if replacement_count == 0:
                    continue

                rel_path = os.path.relpath(file_path, VS_CODE_ROOT).replace("\\", "/")

                owner_base = ""
                rel_parts = rel_path.split("/")
                if rel_parts and rel_parts[0] in (
                    os.path.basename(IN_PROGRESS),
                    os.path.basename(STAGING),
                ) and len(rel_parts) >= 2:
                    owner_base = get_base_mod_name(rel_parts[1])
                if owner_base and owner_base not in renamed_bases:
                    touched_other_mod_bases.add(owner_base)

                if dry_run:
                    log.info(f"[DRYRUN] Would update mod_loaded ref in: {rel_path} ({replacement_count} occurrences)")
                    continue

                try:
                    atomic_write_text(file_path, updated_content, encoding="utf-8")
                    log.info(f"Updated mod_loaded ref in: {rel_path} ({replacement_count} occurrences)")
                except Exception as ex:
                    log.warn(f"Failed writing XML for mod_loaded update: {file_path}: {ex}")

    for mod_base in sorted(touched_other_mod_bases):
        log.action_needed(
            f"'{mod_base}' had mod_loaded references updated. Bump its ModInfo version and review its README."
        )


def gather_mod_versions_by_base_in_dirs(mod_dirs: Tuple[str, ...]) -> Dict[str, str]:
    versions: Dict[str, str] = {}
    for mod_dir in mod_dirs:
        for folder_name, folder_path in scan_mod_folders(mod_dir).items():
            base_name = get_base_mod_name(folder_name)
            version = get_modinfo_version(folder_path) or "0.0.0"
            versions[base_name] = version
    return versions


def update_gigglepack_pending_changes(
    dry_run: bool,
    log: Logger,
    consolidate_updates_label: str = "",
) -> Dict[str, object]:
    """Compute/update pending changes since last published GigglePack state from ActiveBuild."""
    release_state = load_gigglepack_release_state()
    prev_version = str(release_state.get("gigglepack_version", "0.0.0"))
    prev_mods_raw = release_state.get("mods", {})
    prev_mods = prev_mods_raw if isinstance(prev_mods_raw, dict) else {}

    current_mods = gather_mod_versions_by_base_in_dirs((STAGING,))

    added_mods: List[Tuple[str, str]] = []
    updated_existing_mods: List[Tuple[str, str, str]] = []
    removed_mods: List[Tuple[str, str]] = []

    for mod_name in sorted(current_mods):
        current_ver = current_mods[mod_name]
        previous_ver = str(prev_mods.get(mod_name, ""))
        if not previous_ver:
            added_mods.append((mod_name, current_ver))
        elif compare_versions(current_ver, previous_ver) > 0:
            updated_existing_mods.append((mod_name, previous_ver, current_ver))

    for mod_name in sorted(prev_mods):
        if mod_name not in current_mods:
            removed_mods.append((mod_name, str(prev_mods.get(mod_name, ""))))

    renamed_mods, added_mods, removed_mods = detect_renamed_mods(added_mods, removed_mods)

    updated_mods_payload = [
        [mod_name, old_ver, new_ver]
        for mod_name, old_ver, new_ver in updated_existing_mods
    ]

    payload: Dict[str, object] = {
        "computed_at": dt.datetime.now().isoformat(timespec="seconds"),
        "since_gigglepack_version": prev_version,
        "new_mods": [[mod_name, ver] for mod_name, ver in added_mods],
        "updated_mods": updated_mods_payload,
        "renamed_mods": [[old_name, new_name, ver] for old_name, new_name, ver in renamed_mods],
        "removed_mods": [[mod_name, old_ver] for mod_name, old_ver in removed_mods],
    }

    collapse_label = (consolidate_updates_label or "").strip()
    if collapse_label and updated_mods_payload:
        payload["updated_mods_expanded_count"] = len(updated_mods_payload)
        payload["updated_mods_expanded"] = updated_mods_payload
        payload["updated_mods"] = [[collapse_label, "multiple", "multiple"]]
        payload["one_time_consolidation_note"] = (
            "Pending updates were intentionally consolidated into one bulk entry for this run."
        )
        log.info(
            "Pending changes consolidation active: "
            f"collapsed {len(updated_mods_payload)} updated mods into '{collapse_label}'"
        )

    if dry_run:
        log.info(f"[DRYRUN] Would write pending changes file: {GIGGLEPACK_PENDING_CHANGES_PATH}")
    else:
        os.makedirs(os.path.dirname(GIGGLEPACK_PENDING_CHANGES_PATH), exist_ok=True)
        atomic_write_json(GIGGLEPACK_PENDING_CHANGES_PATH, payload, ensure_ascii=False, indent=2)

    log.info(
        "Pending GigglePack changes updated: "
        f"new={len(added_mods)}, updated={len(updated_existing_mods)}, "
        f"renamed={len(renamed_mods)}, removed={len(removed_mods)}"
    )
    return payload


def generate_mod_readmes(
    csv_rows: List[Dict[str, str]],
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
    legacy_cleanup_allowlist: Optional[Set[str]] = None,
    delete_legacy_readmes: bool = True,
) -> None:
    log.info("Generate per-mod README.txt")
    modscope_guide_body = load_modscope_guide_body(log)
    short_guide_body = load_short_guide_body(log)
    mod_type_line_by_id = load_mod_type_lines_from_modtype_guide(log)

    if not os.path.exists(MOD_README_TEMPLATE):
        log.error(f"Missing required template: {MOD_README_TEMPLATE}")
        raise FileNotFoundError(MOD_README_TEMPLATE)

    with open(MOD_README_TEMPLATE, "r", encoding="utf-8") as f:
        template = f.read()

    target_dirs = mod_dirs if mod_dirs is not None else (PUBLISH_READY, IN_PROGRESS)
    target_mod_bases = get_mod_bases_for_dirs(target_dirs)
    compat_data = build_readme_metadata_index(csv_rows, target_mod_bases, log)

    for mod_dir in target_dirs:
        folders = scan_mod_folders(mod_dir)
        for folder_name, mod_path in folders.items():
            modinfo_path = os.path.join(mod_path, "ModInfo.xml")
            if not os.path.exists(modinfo_path):
                log.warn(f"Skipping README for {folder_name}: missing ModInfo.xml")
                continue

            mod_name, mod_version = parse_modinfo(modinfo_path, folder_name)
            mod_display_name = get_modinfo_display_name(modinfo_path, mod_name)
            mod_version_display = format_version_for_display(mod_version, mod_display_name)
            mod_description = resolve_mod_description(mod_path, modinfo_path)
            sync_modinfo_description_from_summary(modinfo_path, mod_description, dry_run, log)
            base_name = get_base_mod_name(folder_name)
            zip_name = f"{base_name}.zip"
            download_link = zip_download_link(zip_name)

            compat = compat_data.get(base_name, {})
            tested_game_version = compat.get("TESTED_GAME_VERSION", "MISSINGDATA")
            eac_friendly = compat.get("EAC_FRIENDLY", "MISSINGDATA")
            server_side_player = compat.get("SERVER_SIDE_PLAYER", "MISSINGDATA")
            if not server_side_player or server_side_player == "MISSINGDATA":
                server_side_player = compat.get("SERVER_SIDE", "MISSINGDATA")
            server_side_dedicated = compat.get("SERVER_SIDE_DEDICATED", "MISSINGDATA")
            if not server_side_dedicated or server_side_dedicated == "MISSINGDATA":
                server_side_dedicated = compat.get("SERVER_SIDE", "MISSINGDATA")
            client_side = compat.get("CLIENT_SIDE", "MISSINGDATA")
            if not client_side or client_side == "MISSINGDATA":
                client_side = compat.get("CLIENT_REQUIRED", "MISSINGDATA")
            mod_type_id = str(compat.get("MOD_TYPE_ID", "MISSINGDATA") or "").strip()
            if mod_type_id == "0":
                mod_type_id = "TBD"
            mod_type_line = mod_type_line_by_id.get(mod_type_id)
            if not mod_type_line:
                if mod_type_id and mod_type_id not in {"MISSINGDATA", "TBD"}:
                    log.warn(f"Invalid MOD_TYPE_ID '{mod_type_id}' for {base_name}; using MISSINGDATA")
                mod_type_line = "MISSINGDATA" if mod_type_id == "MISSINGDATA" else "TBD"
            mod_type_block = format_mod_type_block_for_readme(mod_type_line)
            safe_to_install = normalize_safety_value_for_readme(compat.get("SAFE_TO_INSTALL", "MISSINGDATA"))
            safe_to_remove = normalize_safety_value_for_readme(compat.get("SAFE_TO_REMOVE", "MISSINGDATA"))
            dependencies_value = compat.get("DEPENDENCIES", "MISSINGDATA")
            dependencies_block = format_dependencies_block_for_readme(dependencies_value)
            harmony_requirement_warning_block = ""
            unique = compat.get("UNIQUE", "MISSINGDATA")

            quote_file_name = compat.get("QUOTE_FILE", f"{base_name}.txt")
            quote_file_path = os.path.join(QUOTES_DIR, quote_file_name)
            fallback_quote_path = os.path.join(QUOTES_DIR, f"{base_name}.txt")
            if not os.path.exists(quote_file_path) and quote_file_name != f"{base_name}.txt" and os.path.exists(fallback_quote_path):
                quote_file_path = fallback_quote_path

            quote_text_rendered = ""
            if os.path.exists(quote_file_path):
                try:
                    with open(quote_file_path, "r", encoding="utf-8") as f:
                        quote_text = f.read().strip()
                    if quote_text:
                        quote_text_rendered = format_quote_for_readme(quote_text)
                except Exception as ex:
                    log.warn(f"Failed reading quote for {folder_name}: {ex}")

            readme_content = template
            readme_content = readme_content.replace(MODSCOPE_GUIDE_PLACEHOLDER, modscope_guide_body)
            readme_content = readme_content.replace(SHORT_GUIDE_PLACEHOLDER, SHORT_GUIDE_RAW_TOKEN)
            # Backward compatibility for older templates.
            readme_content = readme_content.replace(LEGACY_SHORT_GUIDE_PLACEHOLDER, SHORT_GUIDE_RAW_TOKEN)
            readme_content = readme_content.replace("{{MOD_NAME}}", mod_name)
            readme_content = readme_content.replace("{{MOD_NAME_UPPER}}", mod_name.upper())
            readme_content = readme_content.replace("{{MOD_VERSION}}", mod_version_display)
            readme_content = readme_content.replace("{{MOD_DESCRIPTION}}", mod_description)
            readme_content = readme_content.replace("{{DOWNLOAD_LINK}}", download_link)

            title_card_callout_block = build_title_card_callout_block_for_readme(quote_text_rendered)
            if TITLE_CARD_CALLOUT_PLACEHOLDER in readme_content:
                readme_content = readme_content.replace(TITLE_CARD_CALLOUT_PLACEHOLDER, title_card_callout_block)
            else:
                # Backward compatibility for older templates that still carry {{QUOTE}} + static NOTE text.
                if quote_text_rendered:
                    readme_content = readme_content.replace("{{QUOTE}}", quote_text_rendered)
                else:
                    readme_content = readme_content.replace("\n\n{{QUOTE}}\n\n", "\n\n")
                    readme_content = readme_content.replace("{{QUOTE}}", "")
            readme_content = readme_content.replace("{{TESTED_GAME_VERSION}}", tested_game_version)
            readme_content = readme_content.replace("{{EAC_FRIENDLY}}", eac_friendly)
            readme_content = readme_content.replace("{{SERVER_SIDE_PLAYER}}", server_side_player)
            readme_content = readme_content.replace("{{SERVER_SIDE_DEDICATED}}", server_side_dedicated)
            readme_content = readme_content.replace("{{CLIENT_SIDE}}", client_side)
            readme_content = readme_content.replace("{{MOD_TYPE_ID}}", mod_type_id)
            readme_content = readme_content.replace("{{MOD_TYPE_LINE}}", mod_type_line)
            readme_content = readme_content.replace("{{MOD_TYPE_BLOCK}}", mod_type_block)
            # Backward compatibility for older templates.
            readme_content = readme_content.replace("{{SERVER_SIDE}}", server_side_player)
            readme_content = readme_content.replace("{{CLIENT_REQUIRED}}", client_side)
            readme_content = readme_content.replace("{{MOD_TYPE}}", mod_type_line)
            readme_content = readme_content.replace("{{SAFE_TO_INSTALL}}", safe_to_install)
            readme_content = readme_content.replace("{{SAFE_TO_REMOVE}}", safe_to_remove)
            readme_content = readme_content.replace("{{DEPENDENCIES_BLOCK}}", dependencies_block)
            readme_content = readme_content.replace(
                HARMONY_REQUIREMENT_WARNING_PLACEHOLDER,
                harmony_requirement_warning_block,
            )
            readme_content = readme_content.replace("{{UNIQUE}}", unique)

            readme_md_path = os.path.join(mod_path, "README.md")
            readme_txt_path = os.path.join(mod_path, "README.txt")
            features_summary_block = extract_readme_block(readme_md_path, "<!-- FEATURES-SUMMARY START -->", "<!-- FEATURES-SUMMARY END -->")
            features_detailed_block = extract_readme_block(readme_md_path, "<!-- FEATURES-DETAILED START -->", "<!-- FEATURES-DETAILED END -->")
            changelog_block = extract_readme_block(readme_md_path, "<!-- CHANGELOG START -->", "<!-- CHANGELOG END -->")

            if not changelog_block:
                changelog_block = extract_markdown_section_by_headings(
                    readme_md_path,
                    ("Changelog (latest)", "Changelog"),
                )
            if not changelog_block:
                changelog_block = extract_txt_section(
                    readme_txt_path,
                    ("Changelog (latest)", "Changelog"),
                    (
                        "FEATURES",
                        "Features",
                        "OTHER DETAILS",
                        "Other Details",
                        "QUICK GUIDE",
                        "AGF MOD GUIDE",
                        "A. Install Mods",
                    ),
                )

            if not features_summary_block:
                features_summary_block = extract_markdown_section_by_headings(
                    readme_md_path,
                    ("Features Summary", "Features"),
                )
            if not features_summary_block:
                features_summary_block = extract_txt_section(
                    readme_txt_path,
                    ("Features", "Features Summary"),
                    (
                        "Deeper Details",
                        "Other Details",
                        "Features Details",
                        "QUICK GUIDE",
                        "AGF MOD GUIDE",
                        "Full Guide",
                        "Quick Guide (Install / Help / Backups / Update / Remove / EAC / Support)",
                        "A. Install Mods",
                    ),
                )
            if not features_detailed_block:
                features_detailed_block = extract_markdown_section_by_headings(
                    readme_md_path,
                    ("Features Details", "Deeper Details", "Other Details"),
                )
            if not features_detailed_block:
                features_detailed_block = extract_txt_section(
                    readme_txt_path,
                    ("Deeper Details", "Other Details", "Features Details"),
                    (
                        "QUICK GUIDE",
                        "AGF MOD GUIDE",
                        "Full Guide",
                        "Quick Guide (Install / Help / Backups / Update / Remove / EAC / Support)",
                        "A. Install Mods",
                    ),
                )

            features_summary_block = sanitize_preserved_readme_block(features_summary_block, flatten_list_markers=True)
            features_summary_block = remove_standalone_bullet_line(features_summary_block)
            features_summary_block = remove_feature_pointer_lines(features_summary_block)
            # OTHER DETAILS: clean without re-indenting, then flatten+wrap preserving user's dash positions.
            # Do NOT use textwrap.dedent() — it strips the user's manual indentation.
            features_detailed_block_clean = re.sub(r"^[ \t]*>+[ \t]*", "", features_detailed_block)
            features_detailed_block_clean = re.sub(r"^\s*>+\s*$", "", features_detailed_block_clean, flags=re.MULTILINE)
            features_detailed_block_clean = re.sub(r"\n{3,}", "\n\n", features_detailed_block_clean)
            features_detailed_block_clean = remove_standalone_bullet_line(features_detailed_block_clean)
            # Strip trailing whitespace per line and leading/trailing blank lines,
            # but DO NOT strip leading whitespace from the first content line.
            features_detailed_block_raw = "\n".join(
                line.rstrip() for line in features_detailed_block_clean.splitlines()
            ).strip("\n")
            other_details_formatted = _format_other_details_block(features_detailed_block_raw, width=72)
            # Flatten legacy nested list indentation so changelog bullets stay uniform on regen.
            changelog_block = sanitize_preserved_readme_block(changelog_block, flatten_list_markers=True)

            if features_summary_block.strip():
                features_body = features_summary_block.strip()
            else:
                fallback_feature = _ensure_sentence(mod_description) or "Feature summary coming soon."
                features_body = f"- {fallback_feature}"
            deeper_details_body = features_detailed_block_raw
            other_details_section = ""
            if deeper_details_body:
                other_details_section = (
                    "------------------------------------------------------------\n"
                    "OTHER DETAILS\n"
                    "------------------------------------------------------------\n\n"
                    f"{deeper_details_body}\n"
                )

            readme_content = readme_content.replace("{{FEATURES_BODY}}", features_body)
            readme_content = readme_content.replace(OTHER_DETAILS_SECTION_PLACEHOLDER, other_details_section)
            # Backward compatibility for older templates.
            readme_content = readme_content.replace("{{DEEPER_DETAILS_BODY}}", deeper_details_body)
            template_has_changelog_placeholder = CHANGELOG_BODY_PLACEHOLDER in readme_content

            changelog_body_content = format_changelog_text(changelog_block, include_h1=False)
            if template_has_changelog_placeholder:
                readme_content = readme_content.replace(CHANGELOG_BODY_PLACEHOLDER, changelog_body_content)

            txt_content = markdown_to_text(readme_content)
            if SHORT_GUIDE_RAW_TOKEN in txt_content:
                short_guide_text = short_guide_body.strip("\r\n")
                txt_content = re.sub(
                    rf"(?:\n)*{re.escape(SHORT_GUIDE_RAW_TOKEN)}(?:\n)*",
                    "\n\n\n\n" + short_guide_text + "\n\n\n\n",
                    txt_content,
                    count=1,
                )
            if not template_has_changelog_placeholder:
                changelog_content = format_changelog_text(changelog_block, include_h1=True)
                if changelog_content:
                    txt_content = txt_content.rstrip() + "\n\n\n\n" + changelog_content
            txt_content = txt_content.rstrip() + "\n"

            # Post-process OTHER DETAILS section with the pre-formatted body.
            if other_details_formatted:
                txt_content = _reformat_other_details_section(txt_content, other_details_formatted)

            if dry_run:
                log.info(f"[DRYRUN] Would write README.txt for {folder_name}")
            else:
                try:
                    atomic_write_text(readme_txt_path, txt_content, encoding="utf-8")
                    legacy_readable_path = os.path.join(mod_path, "ReadableReadMe.txt")
                    if delete_legacy_readmes:
                        cleanup_allowed = True
                        if legacy_cleanup_allowlist is not None:
                            mod_path_key = os.path.normcase(os.path.abspath(mod_path))
                            cleanup_allowed = mod_path_key in legacy_cleanup_allowlist

                        if cleanup_allowed:
                            if os.path.exists(readme_md_path):
                                os.remove(readme_md_path)
                            if os.path.exists(legacy_readable_path):
                                os.remove(legacy_readable_path)
                        elif os.path.exists(readme_md_path) or os.path.exists(legacy_readable_path):
                            log.info(
                                f"Kept legacy README files for {folder_name}: migration checks not met"
                            )
                except Exception as ex:
                    log.warn(f"Failed writing README files for {folder_name}: {ex}")
                    continue

            log.stats.readmes_written += 1


def prep_names_and_readmes_for_dirs(mod_dirs: Tuple[str, ...], dry_run: bool, log: Logger) -> List[Tuple[str, str, str]]:
    """Refresh folder naming and README files for specific mod directories."""
    log.info("Pre-release prep: refresh folder names + README files")

    folder_renames = rename_mod_folders_to_modinfo(dry_run, log, mod_dirs=mod_dirs)
    csv_rows = normalize_compat_csv(
        folder_renames,
        dry_run,
        log,
        mod_dirs=mod_dirs,
        prune_to_mods_now=False,
    )
    if not csv_rows:
        log.warn("Compatibility CSV has no rows; README generation will use fallback values.")

    for row in csv_rows:
        mod_name = row.get("MOD_NAME", "")
        if not row.get("QUOTE_FILE") and mod_name:
            row["QUOTE_FILE"] = f"{mod_name}.txt"

    normalize_quote_files(csv_rows, folder_renames, dry_run, log)
    generate_mod_readmes(csv_rows, dry_run, log, mod_dirs=mod_dirs)
    return folder_renames


def collect_feature_migration_candidates(
    mod_dirs: Tuple[str, ...],
    log: Logger,
) -> Tuple[Set[str], List[Dict[str, object]]]:
    """Build allowlist for safe legacy README cleanup based on feature migration readiness.

    A mod is eligible for legacy README deletion only when both feature summary and
    feature details content can be extracted from existing README sources.
    """
    allowlist: Set[str] = set()
    rows: List[Dict[str, object]] = []

    for mod_dir in mod_dirs:
        folders = scan_mod_folders(mod_dir)
        for folder_name, mod_path in folders.items():
            modinfo_path = os.path.join(mod_path, "ModInfo.xml")
            if not os.path.exists(modinfo_path):
                continue

            readme_md_path = os.path.join(mod_path, "README.md")
            readme_txt_path = os.path.join(mod_path, "README.txt")
            readable_txt_path = os.path.join(mod_path, "ReadableReadMe.txt")
            readme_txt_exists = os.path.exists(readme_txt_path)
            readme_md_exists = os.path.exists(readme_md_path)
            readable_exists = os.path.exists(readable_txt_path)

            features_summary_block = extract_readme_block(
                readme_md_path,
                "<!-- FEATURES-SUMMARY START -->",
                "<!-- FEATURES-SUMMARY END -->",
            )
            features_detailed_block = extract_readme_block(
                readme_md_path,
                "<!-- FEATURES-DETAILED START -->",
                "<!-- FEATURES-DETAILED END -->",
            )

            if not features_summary_block:
                features_summary_block = extract_markdown_section_by_headings(
                    readme_md_path,
                    ("Features Summary", "Features"),
                )
            if not features_summary_block:
                features_summary_block = extract_txt_section(
                    readme_txt_path,
                    ("Features", "Features Summary"),
                    (
                        "Deeper Details",
                        "Other Details",
                        "Features Details",
                        "QUICK GUIDE",
                        "AGF MOD GUIDE",
                        "Full Guide",
                        "Quick Guide (Install / Help / Backups / Update / Remove / EAC / Support)",
                        "A. Install Mods",
                    ),
                )

            if not features_detailed_block:
                features_detailed_block = extract_markdown_section_by_headings(
                    readme_md_path,
                    ("Features Details", "Deeper Details", "Other Details"),
                )
            if not features_detailed_block:
                features_detailed_block = extract_txt_section(
                    readme_txt_path,
                    ("Deeper Details", "Other Details", "Features Details"),
                    (
                        "QUICK GUIDE",
                        "AGF MOD GUIDE",
                        "Full Guide",
                        "Quick Guide (Install / Help / Backups / Update / Remove / EAC / Support)",
                        "A. Install Mods",
                    ),
                )

            features_summary_block = sanitize_preserved_readme_block(features_summary_block, flatten_list_markers=True)
            features_summary_block = remove_standalone_bullet_line(features_summary_block)
            features_summary_block = remove_feature_pointer_lines(features_summary_block)
            # Keep relative list indentation so nested detail bullets do not collapse.
            features_detailed_block = sanitize_preserved_readme_block(features_detailed_block)
            features_detailed_block = remove_standalone_bullet_line(features_detailed_block)
            has_summary = bool(features_summary_block.strip())
            has_details = bool(features_detailed_block.strip())
            can_cleanup = has_summary and has_details

            if can_cleanup:
                allowlist.add(os.path.normcase(os.path.abspath(mod_path)))

            rows.append(
                {
                    "lane": mod_dir,
                    "folder": folder_name,
                    "path": mod_path,
                    "readme_md_exists": readme_md_exists,
                    "readable_txt_exists": readable_exists,
                    "readme_txt_exists": readme_txt_exists,
                    "has_features_summary": has_summary,
                    "has_features_details": has_details,
                    "eligible_for_legacy_cleanup": can_cleanup,
                }
            )

    eligible_count = sum(1 for row in rows if row.get("eligible_for_legacy_cleanup"))
    log.info(
        "Feature-migration readiness scan complete: "
        f"eligible={eligible_count}, ineligible={len(rows) - eligible_count}, total={len(rows)}"
    )
    return allowlist, rows


def write_feature_migration_report(
    rows: List[Dict[str, object]],
    dry_run: bool,
    log: Logger,
    report_tag: str = "scan",
) -> str:
    report_dir = os.path.join(README_SYSTEM_ROOT, "temp")
    stamp = dt.datetime.now().strftime("%Y%m%d-%H%M%S")
    safe_tag = re.sub(r"[^A-Za-z0-9_-]+", "-", (report_tag or "scan")).strip("-") or "scan"
    report_path = os.path.join(report_dir, f"feature-migration-{safe_tag}-{stamp}.json")
    payload = {
        "generated_at": dt.datetime.now().isoformat(timespec="seconds"),
        "dry_run": dry_run,
        "total_mods": len(rows),
        "eligible_for_legacy_cleanup": sum(1 for row in rows if row.get("eligible_for_legacy_cleanup")),
        "rows": rows,
    }
    if dry_run:
        log.info(f"[DRYRUN] Would write migration report: {report_path}")
    else:
        os.makedirs(report_dir, exist_ok=True)
        atomic_write_json(report_path, payload, ensure_ascii=False, indent=2)
        log.info(f"Wrote migration report: {report_path}")
    return report_path


def migrate_readmes_one_time(
    dry_run: bool,
    log: Logger,
    target_dirs: Tuple[str, ...],
) -> None:
    """One-time migration for Draft/Active README modernization.

    Generates README.txt using the current template flow, but only deletes legacy
    README.md / ReadableReadMe.txt for mods that pass feature migration checks.
    """
    log.info(
        "One-time README migration: preserving features summary/details and "
        "conditionally removing legacy README files"
    )

    folder_renames = rename_mod_folders_to_modinfo(dry_run, log, mod_dirs=target_dirs)
    csv_rows = normalize_compat_csv(
        folder_renames,
        dry_run,
        log,
        mod_dirs=target_dirs,
        prune_to_mods_now=False,
    )
    normalize_quote_files(csv_rows, folder_renames, dry_run, log)

    cleanup_allowlist, pre_rows = collect_feature_migration_candidates(target_dirs, log)
    write_feature_migration_report(pre_rows, dry_run, log, report_tag="pre")

    generate_mod_readmes(
        csv_rows,
        dry_run,
        log,
        mod_dirs=target_dirs,
        legacy_cleanup_allowlist=cleanup_allowlist,
        delete_legacy_readmes=True,
    )

    post_rows: List[Dict[str, object]] = []
    for row in pre_rows:
        mod_path = str(row.get("path", "") or "")
        post_rows.append(
            {
                "lane": row.get("lane", ""),
                "folder": row.get("folder", ""),
                "path": mod_path,
                "legacy_cleanup_was_eligible": bool(row.get("eligible_for_legacy_cleanup", False)),
                "readme_md_exists_after": os.path.exists(os.path.join(mod_path, "README.md")),
                "readable_txt_exists_after": os.path.exists(os.path.join(mod_path, "ReadableReadMe.txt")),
                "readme_txt_exists_after": os.path.exists(os.path.join(mod_path, "README.txt")),
            }
        )
    write_feature_migration_report(post_rows, dry_run, log, report_tag="post")


# =============================================================
# STEP 5: PUSH BACK PULLED MODS
# =============================================================
def push_back_pulled_mods(mods_pulled_from_game: List[Tuple[str, str]], dry_run: bool, log: Logger) -> None:
    log.info("Step 5: Push updated pulled mods back to game folder")

    workspace_by_base: Dict[str, str] = {}
    for folder, path in scan_mod_folders(PUBLISH_READY).items():
        workspace_by_base[get_base_mod_name(folder)] = path
    for folder, path in scan_mod_folders(IN_PROGRESS).items():
        workspace_by_base[get_base_mod_name(folder)] = path
    for folder, path in scan_mod_folders(STAGING).items():
        workspace_by_base[get_base_mod_name(folder)] = path

    for mod_name, ws_path in mods_pulled_from_game:
        resolved_ws_path = ws_path
        if not os.path.exists(resolved_ws_path):
            base_name = get_base_mod_name(mod_name)
            resolved_ws_path = workspace_by_base.get(base_name, "")

        if (not resolved_ws_path or not os.path.exists(resolved_ws_path)) and not dry_run:
            log.warn(f"Pushback skipped for {mod_name}: workspace path missing {ws_path}")
            continue

        pushed_folder_name = os.path.basename(resolved_ws_path)
        pushed_version = get_modinfo_version(resolved_ws_path)
        if pushed_version is None:
            log.warn(f"Pushback skipped for {mod_name}: unreadable ModInfo.xml at {resolved_ws_path}")
            continue

        try:
            pushed_major = int((pushed_version or "0.0.0").split(".", 1)[0])
        except Exception:
            pushed_major = 0

        if pushed_major < 1:
            log.info(
                f"Pushback skipped for {mod_name}: version {pushed_version} is draft-only (major < 1)"
            )
            continue

        if is_4modders_mod(pushed_folder_name):
            existing_game_root_path = os.path.join(GAME_MODS, pushed_folder_name)
            if not os.path.isdir(existing_game_root_path):
                log.info(
                    f"Pushback skipped for {mod_name}: 4Modders mods only push when already present in game root"
                )
                continue

        if is_requested_mod(pushed_folder_name):
            existing_game_root_path = os.path.join(GAME_MODS, pushed_folder_name)
            if not os.path.isdir(existing_game_root_path):
                log.info(
                    f"Pushback skipped for {mod_name}: Requested mods only push when already present in game root"
                )
                continue

        if mod_name != pushed_folder_name:
            old_game_path = os.path.join(GAME_MODS, mod_name)
            if maybe_remove_dir(old_game_path, dry_run, log):
                log.info(
                    f"Pushback cleanup: removed old game folder name {mod_name} before pushing {pushed_folder_name}"
                )

        dest_path = os.path.join(GAME_MODS, pushed_folder_name)
        if maybe_remove_dir(dest_path, dry_run, log) and maybe_copytree(resolved_ws_path, dest_path, dry_run, log):
            log.stats.pushed_back_to_game += 1
            log.info(f"Pushback complete: {mod_name}")


def push_staging_mods_to_game(mod_bases: Set[str], dry_run: bool, log: Logger, reason: str) -> None:
    """Push a targeted set of ActiveBuild mods into the live game folder."""
    if not mod_bases:
        log.info(f"Targeted game sync skipped: no ActiveBuild mods changed for {reason}")
        return

    staging_folders = scan_mod_folders(STAGING)
    if not staging_folders:
        log.warn("Targeted game sync skipped: ActiveBuild has no managed mods")
        return

    targeted_backpack_updates = any(
        is_backpack_mod(folder_name) and get_base_mod_name(folder_name) in mod_bases
        for folder_name in staging_folders
    )

    backpack_mods = sorted([f for f in staging_folders if is_backpack_mod(f)])
    active_backpack = next((f for f in backpack_mods if BACKPACK_DEFAULT_ACTIVE_TOKEN in f), None)
    if active_backpack is None and backpack_mods:
        active_backpack = backpack_mods[0]
        log.warn(
            f"Default backpack token '{BACKPACK_DEFAULT_ACTIVE_TOKEN}' not found. "
            f"Using '{active_backpack}' as active backpack."
        )

    optionals_backpack_path = os.path.join(GAME_MODS, GAME_OPTIONALS_BACKPACK_DIR)
    if targeted_backpack_updates:
        if dry_run:
            log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_backpack_path}")
        else:
            os.makedirs(optionals_backpack_path, exist_ok=True)

        game_folders = scan_mod_folders(GAME_MODS)
        for game_folder, game_path in game_folders.items():
            if is_backpack_mod(game_folder) and active_backpack and game_folder != active_backpack:
                if maybe_remove_dir(game_path, dry_run, log):
                    log.info(
                        "Targeted game sync cleanup: removed non-active backpack from game root: "
                        f"{game_folder}"
                    )

    synced_root = 0
    mirrored_backpack_optionals = 0
    for folder_name, staging_path in sorted(staging_folders.items()):
        base_name = get_base_mod_name(folder_name)
        if base_name not in mod_bases:
            continue

        version = get_modinfo_version(staging_path)
        if version is None:
            log.warn(f"Targeted game sync skipped for {folder_name}: unreadable ModInfo.xml")
            continue

        try:
            major_version = int((version or "0.0.0").split(".", 1)[0])
        except Exception:
            major_version = 0

        if major_version < 1:
            log.info(
                f"Targeted game sync skipped for {folder_name}: version {version} is draft-only (major < 1)"
            )
            continue

        if is_4modders_mod(folder_name):
            game_path = os.path.join(GAME_MODS, folder_name)
            if not os.path.isdir(game_path):
                log.info(
                    f"Targeted game sync skipped for {folder_name}: 4Modders mods only push when already present in game root"
                )
                continue

        if is_requested_mod(folder_name):
            game_path = os.path.join(GAME_MODS, folder_name)
            if not os.path.isdir(game_path):
                log.info(
                    f"Targeted game sync skipped for {folder_name}: Requested mods only push when already present in game root"
                )
                continue

        if is_backpack_mod(folder_name):
            if maybe_copytree(staging_path, os.path.join(optionals_backpack_path, folder_name), dry_run, log):
                mirrored_backpack_optionals += 1
                log.info(f"Targeted game sync mirror: backpack optional updated: {folder_name}")

            if active_backpack and folder_name != active_backpack:
                log.info(
                    "Targeted game sync skipped for non-active backpack in game root: "
                    f"{folder_name}"
                )
                continue

        destination_path = os.path.join(GAME_MODS, folder_name)
        if maybe_remove_dir(destination_path, dry_run, log) and maybe_copytree(staging_path, destination_path, dry_run, log):
            synced_root += 1
            log.stats.pushed_back_to_game += 1
            log.info(f"Targeted game sync complete: {folder_name} ({reason})")

    log.info(
        f"Targeted game sync summary: {synced_root} mod(s) {'would be ' if dry_run else ''}pushed to game root "
        f"for {reason}; backpack optionals mirrored={mirrored_backpack_optionals}"
    )


def remap_pulled_mods_after_renames(
    mods_pulled_from_game: List[Tuple[str, str]],
    folder_renames: List[Tuple[str, str, str]],
    log: Logger,
) -> List[Tuple[str, str]]:
    """Remap pulled-mod tracking entries when folder names changed during the run."""
    if not mods_pulled_from_game or not folder_renames:
        return mods_pulled_from_game

    rename_by_name: Dict[str, Tuple[str, str]] = {}
    rename_by_name_and_dir: Dict[Tuple[str, str], Tuple[str, str]] = {}
    for old_name, new_name, mod_dir in folder_renames:
        new_path = os.path.join(mod_dir, new_name)
        rename_by_name[old_name] = (new_name, new_path)
        rename_by_name_and_dir[(old_name, mod_dir)] = (new_name, new_path)

    remapped: List[Tuple[str, str]] = []
    seen: set[Tuple[str, str]] = set()
    for mod_name, ws_path in mods_pulled_from_game:
        current_name = mod_name
        current_path = ws_path

        mod_dir = os.path.dirname(ws_path)
        mapped = rename_by_name_and_dir.get((mod_name, mod_dir))
        if mapped is None:
            mapped = rename_by_name.get(mod_name)

        if mapped is not None:
            current_name, current_path = mapped
            log.info(
                f"Pushback remap: {mod_name} -> {current_name} (using post-rename workspace path)"
            )

        key = (current_name, current_path)
        if key in seen:
            continue
        seen.add(key)
        remapped.append(key)

    return remapped


def get_managed_mod_names_from_csv(log: Logger) -> set[str]:
    """Return managed mod base names from HELPER_ModCompatibility.csv."""
    managed: set[str] = set()
    try:
        _, rows = load_compat_csv()
        for row in rows:
            mod_name = row.get("MOD_NAME", "").strip()
            if mod_name and is_agf_mod(mod_name):
                managed.add(mod_name)
            elif mod_name:
                log.warn(f"Ignoring non-AGF CSV row in managed roster: {mod_name}")
    except Exception as ex:
        log.warn(f"Could not load managed mods from CSV; unknown orphans will be preserved: {ex}")
    return managed


def sync_staging_and_game(dry_run: bool, log: Logger) -> List[Tuple[str, str]]:
    """Sync only between staging and game lanes.

    This mode is for day-to-day test syncs and does not touch PublishReady.
    """
    log.info("Mode sync-work: Sync Staging <-> Game by version")
    log.info(
        "Policy: keep one active BackpackPlus in game root, keep all BackpackPlus in .Optionals-Backpack, "
        "mirror all HUDPlus/HUDPluszOther in .Optionals-HUDPlus and keep HUDPluszOther out of game root, "
        "and mirror AGF-4Modders into .Optionals-4Modders without auto-pushing them into game root."
    )

    staging_folders = scan_mod_folders(STAGING)
    game_folders = scan_mod_folders(GAME_MODS)
    draft_folders = scan_mod_folders(IN_PROGRESS)
    draft_bases: set[str] = {get_base_mod_name(folder) for folder in draft_folders}
    mods_pulled_from_game: List[Tuple[str, str]] = []
    renamed_suffixes: set[str] = set()
    for folder_name in staging_folders:
        match = re.match(r"^AGF-4Modders-(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if match:
            renamed_suffixes.add(match.group(1))

    log.stats.scanned_workspace_mods = len(staging_folders)
    log.stats.scanned_game_mods = len(game_folders)

    backpack_mods = sorted([f for f in staging_folders if is_backpack_mod(f)])
    active_backpack = next((f for f in backpack_mods if BACKPACK_DEFAULT_ACTIVE_TOKEN in f), None)
    if active_backpack is None and backpack_mods:
        active_backpack = backpack_mods[0]
        log.warn(
            f"Default backpack token '{BACKPACK_DEFAULT_ACTIVE_TOKEN}' not found. "
            f"Using '{active_backpack}' as active backpack."
        )

    # Keep game root clean for root-policy-only mods.
    for game_folder, game_path in game_folders.items():
        game_base = get_base_mod_name(game_folder)
        if game_base in draft_bases:
            log.info(f"sync-work preserve: kept Draft-tracked game mod in root: {game_folder}")
            continue

        if is_backpack_mod(game_folder) and active_backpack and game_folder != active_backpack:
            if maybe_remove_dir(game_path, dry_run, log):
                log.info(f"sync-work cleanup: removed non-active backpack from game root: {game_folder}")
            continue
        if is_hudpluszother_mod(game_folder):
            if maybe_remove_dir(game_path, dry_run, log):
                log.info(f"sync-work cleanup: removed HUDPluszOther optional from game root: {game_folder}")

    cleanup_legacy_4modders_replacements_with_suffixes(
        GAME_MODS,
        renamed_suffixes,
        dry_run,
        log,
        "game root",
    )

    # Re-scan after cleanup decisions for consistent sync maps.
    game_folders = scan_mod_folders(GAME_MODS)

    # Sync allowed game-root mods by version.
    allowed_staging_root: Dict[str, str] = {}
    for folder, path in staging_folders.items():
        if is_backpack_mod(folder) and active_backpack and folder != active_backpack:
            continue
        if is_hudpluszother_mod(folder):
            continue
        if is_4modders_mod(folder):
            continue
        if is_requested_mod(folder):
            continue
        allowed_staging_root[folder] = path

    staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in allowed_staging_root.items()
    }
    game_by_base_all: Dict[str, List[Tuple[str, str]]] = {}
    for folder, path in game_folders.items():
        base_name = get_base_mod_name(folder)
        game_by_base_all.setdefault(base_name, []).append((folder, path))

    # If multiple game folders exist for the same base, keep only the ActiveBuild target name.
    for base_name, entries in game_by_base_all.items():
        if len(entries) <= 1:
            continue
        keep_folder = staging_by_base.get(base_name, ("", ""))[0]
        for game_folder, game_path in entries:
            if keep_folder and game_folder == keep_folder:
                continue
            if maybe_remove_dir(game_path, dry_run, log):
                log.info(
                    "sync-work cleanup: removed duplicate/old game version for base "
                    f"{base_name}: {game_folder}"
                )

    game_folders = scan_mod_folders(GAME_MODS)
    game_by_base_all = {}
    for folder, path in game_folders.items():
        base_name = get_base_mod_name(folder)
        game_by_base_all.setdefault(base_name, []).append((folder, path))
    managed_mods = get_managed_mod_names_from_csv(log)

    # Remove AGF mods from game root that are no longer represented in ActiveBuild.
    # This handles renamed mods and removed mods so stale folders do not linger in game.
    stale_game_bases = sorted(set(game_by_base_all.keys()) - set(staging_by_base.keys()))
    for base_name in stale_game_bases:
        if base_name in draft_bases:
            for game_folder, _game_path in game_by_base_all.get(base_name, []):
                log.info(
                    f"sync-work preserve: kept game mod not in ActiveBuild because Draft tracks it: {game_folder}"
                )
            continue

        for game_folder, game_path in game_by_base_all.get(base_name, []):
            if base_name in managed_mods:
                if maybe_remove_dir(game_path, dry_run, log):
                    log.info(
                        f"sync-work cleanup: removed managed stale game mod not present in ActiveBuild: {game_folder}"
                    )
            else:
                log.warn(
                    f"sync-work orphan preserved: unknown game mod not in ActiveBuild and not in CSV: {game_folder}"
                )

    # Re-scan game root after stale cleanup so overlap/missing calculations are accurate.
    game_folders = scan_mod_folders(GAME_MODS)
    game_by_base_all = {}
    for folder, path in game_folders.items():
        base_name = get_base_mod_name(folder)
        game_by_base_all.setdefault(base_name, []).append((folder, path))

    overlap = sorted(set(staging_by_base.keys()) & set(game_by_base_all.keys()))
    for base_name in overlap:
        st_folder, st_path = staging_by_base[base_name]
        game_entries = list(game_by_base_all.get(base_name, []))
        preferred = next(((name, path) for name, path in game_entries if name == st_folder), None)
        game_folder, game_path = preferred if preferred else game_entries[0]

        # Remove any extra versions for this base in game root.
        for extra_folder, extra_path in game_entries:
            if extra_folder == game_folder:
                continue
            if maybe_remove_dir(extra_path, dry_run, log):
                log.info(
                    f"sync-work cleanup: removed extra game version for {base_name}: {extra_folder}"
                )
        st_ver = get_modinfo_version(st_path)
        game_ver = get_modinfo_version(game_path)

        if st_ver is None:
            log.warn(
                f"Skipping sync-work for {base_name}: missing/unreadable ModInfo.xml "
                f"(staging={st_ver}, game={game_ver})"
            )
            continue

        if game_ver is None:
            target = os.path.join(GAME_MODS, st_folder)
            if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(st_path, target, dry_run, log):
                log.stats.synced_push_to_game += 1
                log.info(
                    f"sync-work repair: replaced malformed game copy for {base_name} "
                    f"with staging v{st_ver}"
                )
            continue

        cmp_value = compare_versions(st_ver, game_ver)
        if cmp_value > 0:
            target = os.path.join(GAME_MODS, st_folder)
            if maybe_copytree(st_path, target, dry_run, log):
                log.stats.synced_push_to_game += 1
                log.info(f"sync-work push: {st_folder} v{st_ver} -> game")
        elif cmp_value < 0:
            target = os.path.join(STAGING, game_folder)
            if maybe_copytree(game_path, target, dry_run, log):
                log.stats.synced_pull_from_game += 1
                mods_pulled_from_game.append((game_folder, target))
                log.info(f"sync-work pull: {game_folder} v{game_ver} -> staging")
        else:
            try:
                st_hash = hash_directory(st_path)
                game_hash = hash_directory(game_path)
                if st_hash != game_hash:
                    # In sync-work mode, staging is authoritative for game copies on tied versions.
                    target = os.path.join(GAME_MODS, st_folder)
                    if maybe_remove_dir(game_path, dry_run, log) and maybe_copytree(st_path, target, dry_run, log):
                        log.stats.synced_push_to_game += 1
                        log.info(
                            f"sync-work tie resolved for {base_name}: both v{st_ver} but content differed. "
                            "Pushed staging copy to game."
                        )
                    else:
                        log.stats.sync_conflicts += 1
                        log.warn(
                            f"sync-work conflict for {base_name}: both v{st_ver} but content differs. "
                            "Auto-push to game failed."
                        )
            except Exception as ex:
                log.warn(f"Could not hash compare tied versions for {base_name}: {ex}")

    # Ensure active backpack exists in game root even if it did not overlap.
    if active_backpack:
        active_staging_path = staging_folders.get(active_backpack)
        active_game_path = os.path.join(GAME_MODS, active_backpack)
        if active_staging_path and active_backpack not in game_folders:
            if maybe_copytree(active_staging_path, active_game_path, dry_run, log):
                log.stats.synced_push_to_game += 1
                log.info(f"sync-work push: ensured active backpack in game root: {active_backpack}")

    # Push all other allowed staging mods that are missing from game root by base name.
    missing_in_game = sorted(set(staging_by_base.keys()) - set(game_by_base_all.keys()))
    for base_name in missing_in_game:
        st_folder, st_path = staging_by_base[base_name]
        st_ver = get_modinfo_version(st_path)
        if st_ver is None:
            log.warn(f"sync-work missing push skipped for {base_name}: staging ModInfo.xml unreadable")
            continue
        target = os.path.join(GAME_MODS, st_folder)
        if maybe_copytree(st_path, target, dry_run, log):
            log.stats.synced_push_to_game += 1
            log.info(f"sync-work push: added missing game mod {st_folder} v{st_ver}")

    # Mirror optionals folders in game space.
    optionals_backpack_path = os.path.join(GAME_MODS, GAME_OPTIONALS_BACKPACK_DIR)
    optionals_hudplus_path = os.path.join(GAME_MODS, GAME_OPTIONALS_HUDPLUS_DIR)
    optionals_4modders_path = os.path.join(GAME_MODS, GAME_OPTIONALS_4MODDERS_DIR)
    optionals_requested_path = os.path.join(GAME_MODS, GAME_OPTIONALS_REQUESTED_DIR)
    if dry_run:
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_backpack_path}")
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_hudplus_path}")
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_4modders_path}")
        log.info(f"[DRYRUN] Would ensure game optionals folder exists: {optionals_requested_path}")
    else:
        os.makedirs(optionals_backpack_path, exist_ok=True)
        os.makedirs(optionals_hudplus_path, exist_ok=True)
        os.makedirs(optionals_4modders_path, exist_ok=True)
        os.makedirs(optionals_requested_path, exist_ok=True)

    cleanup_legacy_4modders_replacements_with_suffixes(
        optionals_hudplus_path,
        renamed_suffixes,
        dry_run,
        log,
        GAME_OPTIONALS_HUDPLUS_DIR,
    )

    for folder, st_path in staging_folders.items():
        if is_backpack_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_backpack_path, folder), dry_run, log):
                log.info(f"sync-work mirror: backpack optional updated: {folder}")
            continue
        if is_hudplus_mod(folder) or is_hudpluszother_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_hudplus_path, folder), dry_run, log):
                log.info(f"sync-work mirror: HUD optional updated: {folder}")
            continue
        if is_4modders_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_4modders_path, folder), dry_run, log):
                log.info(f"sync-work mirror: 4Modders optional updated: {folder}")
            continue
        if is_requested_mod(folder):
            if maybe_copytree(st_path, os.path.join(optionals_requested_path, folder), dry_run, log):
                log.info(f"sync-work mirror: Requested optional updated: {folder}")

    # Remove stale optionals entries that no longer match ActiveBuild folder names.
    expected_backpack = {folder for folder in staging_folders if is_backpack_mod(folder)}
    expected_hudplus = {
        folder
        for folder in staging_folders
        if is_hudplus_mod(folder) or is_hudpluszother_mod(folder)
    }
    expected_4modders = {folder for folder in staging_folders if is_4modders_mod(folder)}
    expected_requested = {folder for folder in staging_folders if is_requested_mod(folder)}

    optionals_cleanup_targets = (
        (optionals_backpack_path, expected_backpack, GAME_OPTIONALS_BACKPACK_DIR),
        (optionals_hudplus_path, expected_hudplus, GAME_OPTIONALS_HUDPLUS_DIR),
        (optionals_4modders_path, expected_4modders, GAME_OPTIONALS_4MODDERS_DIR),
        (optionals_requested_path, expected_requested, GAME_OPTIONALS_REQUESTED_DIR),
    )
    for optionals_dir, expected_names, label in optionals_cleanup_targets:
        if not os.path.isdir(optionals_dir):
            continue
        existing = scan_mod_folders(optionals_dir)
        for folder_name, folder_path in existing.items():
            if folder_name in expected_names:
                continue
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    f"sync-work cleanup: removed stale optional from {label}: {folder_name}"
                )

    # Normalize active-build folder names after pulls so names track ModInfo versions.
    rename_mod_folders_to_modinfo(dry_run, log, mod_dirs=(STAGING,))

    # Extra cleanup pass after renames: ensure game root and optionals reflect final ActiveBuild names.
    log.info("sync-work cleanup: post-rename cleanup pass")

    final_staging_folders = scan_mod_folders(STAGING)
    final_allowed_staging_root: Dict[str, str] = {}
    final_backpack_mods = sorted([f for f in final_staging_folders if is_backpack_mod(f)])
    final_active_backpack = next((f for f in final_backpack_mods if BACKPACK_DEFAULT_ACTIVE_TOKEN in f), None)
    if final_active_backpack is None and final_backpack_mods:
        final_active_backpack = final_backpack_mods[0]

    for folder, path in final_staging_folders.items():
        if is_backpack_mod(folder) and final_active_backpack and folder != final_active_backpack:
            continue
        if is_hudpluszother_mod(folder):
            continue
        if is_4modders_mod(folder):
            continue
        final_allowed_staging_root[folder] = path

    final_staging_by_base: Dict[str, Tuple[str, str]] = {
        get_base_mod_name(folder): (folder, path)
        for folder, path in final_allowed_staging_root.items()
    }

    final_game_folders = scan_mod_folders(GAME_MODS)
    final_game_by_base_all: Dict[str, List[Tuple[str, str]]] = {}
    for folder, path in final_game_folders.items():
        base_name = get_base_mod_name(folder)
        final_game_by_base_all.setdefault(base_name, []).append((folder, path))

    # Final pass: enforce one game-root folder per ActiveBuild base name.
    for base_name, entries in final_game_by_base_all.items():
        expected_folder = final_staging_by_base.get(base_name, ("", ""))[0]
        if not expected_folder:
            continue
        for folder_name, folder_path in entries:
            if folder_name == expected_folder:
                continue
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    "sync-work cleanup: post-rename removed extra game version for base "
                    f"{base_name}: {folder_name}"
                )

    final_game_folders = scan_mod_folders(GAME_MODS)
    final_game_by_base_all = {}
    for folder, path in final_game_folders.items():
        base_name = get_base_mod_name(folder)
        final_game_by_base_all.setdefault(base_name, []).append((folder, path))

    final_stale_game_bases = sorted(set(final_game_by_base_all.keys()) - set(final_staging_by_base.keys()))
    for base_name in final_stale_game_bases:
        if base_name in draft_bases:
            for game_folder, _game_path in final_game_by_base_all.get(base_name, []):
                log.info(
                    "sync-work preserve: post-rename kept game mod not in ActiveBuild "
                    f"because Draft tracks it: {game_folder}"
                )
            continue

        for game_folder, game_path in final_game_by_base_all.get(base_name, []):
            if base_name in managed_mods:
                if maybe_remove_dir(game_path, dry_run, log):
                    log.info(
                        "sync-work cleanup: post-rename removed managed stale game mod "
                        f"not present in ActiveBuild: {game_folder}"
                    )
            else:
                log.warn(
                    "sync-work orphan preserved: post-rename unknown game mod not in "
                    f"ActiveBuild and not in CSV: {game_folder}"
                )

    final_expected_backpack = {folder for folder in final_staging_folders if is_backpack_mod(folder)}
    final_expected_hudplus = {
        folder
        for folder in final_staging_folders
        if is_hudplus_mod(folder) or is_hudpluszother_mod(folder)
    }
    final_expected_4modders = {folder for folder in final_staging_folders if is_4modders_mod(folder)}
    final_expected_requested = {folder for folder in final_staging_folders if is_requested_mod(folder)}

    final_optionals_cleanup_targets = (
        (optionals_backpack_path, final_expected_backpack, GAME_OPTIONALS_BACKPACK_DIR),
        (optionals_hudplus_path, final_expected_hudplus, GAME_OPTIONALS_HUDPLUS_DIR),
        (optionals_4modders_path, final_expected_4modders, GAME_OPTIONALS_4MODDERS_DIR),
        (optionals_requested_path, final_expected_requested, GAME_OPTIONALS_REQUESTED_DIR),
    )
    for optionals_dir, expected_names, label in final_optionals_cleanup_targets:
        if not os.path.isdir(optionals_dir):
            continue
        existing = scan_mod_folders(optionals_dir)
        for folder_name, folder_path in existing.items():
            if folder_name in expected_names:
                continue
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    f"sync-work cleanup: post-rename removed stale optional from {label}: {folder_name}"
                )

    return mods_pulled_from_game


def promote_staging_to_publish_ready(dry_run: bool, log: Logger) -> None:
    """Promote staging mods to publish-ready only when version is higher or missing in publish-ready."""
    log.info("Mode promote: Promote Staging -> PublishReady (version-gated)")

    staging_folders = scan_mod_folders(STAGING)
    publish_folders = scan_mod_folders(PUBLISH_READY)

    publish_by_base: Dict[str, Tuple[str, str]] = {}
    for folder, path in publish_folders.items():
        publish_by_base[get_base_mod_name(folder)] = (folder, path)

    for st_folder, st_path in staging_folders.items():
        base_name = get_base_mod_name(st_folder)
        st_ver = get_modinfo_version(st_path)

        if st_ver is None:
            log.warn(f"Promote skipped for {st_folder}: missing/unreadable ModInfo.xml")
            continue

        try:
            st_major = int((st_ver or "0.0.0").split(".", 1)[0])
        except Exception:
            st_major = 0
        if st_major < 1:
            log.info(
                f"Promote skipped for {st_folder}: staging version {st_ver} is draft-only (major < 1)"
            )
            continue

        if base_name not in publish_by_base:
            legacy_replacement = re.match(
                r"^(AGF-NoEAC-|AGF-HUDPluszOther-)(.+)-v([0-9][0-9a-zA-Z\.-]*)$",
                st_folder,
            )
            if legacy_replacement:
                suffix = legacy_replacement.group(2)
                version = legacy_replacement.group(3)
                old_4modders_name = f"AGF-4Modders-{suffix}-v{version}"
                old_4modders_path = publish_folders.get(old_4modders_name)
                if old_4modders_path and maybe_remove_dir(old_4modders_path, dry_run, log):
                    log.info(
                        "Promote rename replacement: removed stale 4Modders release folder "
                        f"{old_4modders_name} before promoting {st_folder}"
                    )

            dest = os.path.join(PUBLISH_READY, st_folder)
            if maybe_copytree(st_path, dest, dry_run, log):
                log.stats.promoted_to_publish_ready += 1
                log.info(f"Promoted new mod: {st_folder} v{st_ver}")
            continue

        pub_folder, pub_path = publish_by_base[base_name]
        pub_ver = get_modinfo_version(pub_path)
        if pub_ver is None:
            log.warn(f"Promote skipped for {st_folder}: publish copy has unreadable ModInfo.xml")
            continue

        cmp_value = compare_versions(st_ver, pub_ver)
        if cmp_value > 0:
            dest = os.path.join(PUBLISH_READY, st_folder)
            if pub_folder != st_folder:
                if not maybe_remove_dir(pub_path, dry_run, log):
                    log.warn(f"Promote skipped for {st_folder}: could not remove old publish folder {pub_folder}")
                    continue
            if maybe_copytree(st_path, dest, dry_run, log):
                log.stats.promoted_to_publish_ready += 1
                log.info(f"Promoted update: {st_folder} v{st_ver} (was {pub_folder} v{pub_ver})")
        elif cmp_value == 0:
            try:
                st_hash = hash_directory(st_path)
                pub_hash = hash_directory(pub_path)
                if st_hash == pub_hash:
                    log.info(f"Promote skipped for {st_folder}: same version/content as publish-ready ({st_ver})")
                else:
                    dest = os.path.join(PUBLISH_READY, st_folder)
                    if pub_folder != st_folder:
                        if not maybe_remove_dir(pub_path, dry_run, log):
                            log.warn(
                                f"Promote skipped for {st_folder}: could not remove old publish folder {pub_folder}"
                            )
                            continue
                    if maybe_copytree(st_path, dest, dry_run, log):
                        log.stats.promoted_to_publish_ready += 1
                        log.info(
                            f"Promoted refresh: {st_folder} v{st_ver} (same version, content/name changed from {pub_folder})"
                        )
            except Exception as ex:
                log.warn(f"Promote hash compare failed for {st_folder}: {ex}")
        else:
            log.warn(
                f"Promote skipped for {st_folder}: staging version {st_ver} is lower than publish-ready {pub_ver}"
            )


def cleanup_release_legacy_4modders_renames(dry_run: bool, log: Logger) -> None:
    """Remove legacy ReleaseSource folders that were renamed into AGF-4Modders equivalents."""
    log.info("Cleanup: Remove legacy ReleaseSource folders replaced by AGF-4Modders renames")

    publish_folders = scan_mod_folders(PUBLISH_READY)
    staging_folders = scan_mod_folders(STAGING)
    for folder_name in sorted(publish_folders.keys()):
        if not folder_name.startswith("AGF-4Modders-"):
            continue

        match = re.match(r"^AGF-4Modders-(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if not match:
            continue

        suffix = match.group(1)
        version = match.group(2)
        for legacy_prefix in ("AGF-NoEAC-", "AGF-HUDPluszOther-"):
            legacy_folder = f"{legacy_prefix}{suffix}-v{version}"
            legacy_path = os.path.join(PUBLISH_READY, legacy_folder)
            if os.path.isdir(legacy_path):
                if legacy_folder in staging_folders:
                    # ActiveBuild is the source-of-truth. Keep the legacy-named release folder
                    # when that naming still exists in ActiveBuild.
                    log.info(
                        "Skipped legacy release cleanup because ActiveBuild still has: "
                        f"{legacy_folder}"
                    )
                    continue
                if maybe_remove_dir(legacy_path, dry_run, log):
                    log.info(
                        f"Removed legacy release folder after 4Modders rename: {legacy_folder} -> {folder_name}"
                    )


def cleanup_legacy_4modders_renames_in_dir(base_dir: str, dry_run: bool, log: Logger, staging_dir: Optional[str] = None) -> None:
    """Remove legacy NoEAC/HUDPluszOther folders that were replaced by AGF-4Modders in a lane.

    When ``staging_dir`` is provided (recommended for PUBLISH_READY), staging is used as the
    source-of-truth to decide which side of a 4Modders/NoEAC pair is stale:
    - If staging still has AGF-4Modders-X  -> NoEAC-X in base_dir is legacy, delete it.
    - If staging has AGF-NoEAC-X (not 4Modders-X) -> 4Modders-X in base_dir is stale, delete it.
    Without a staging_dir the function keeps its original behaviour (always deletes the NoEAC side).
    """
    lane_name = os.path.basename(base_dir.rstrip("\\/"))
    log.info(f"Cleanup: Remove legacy 4Modders-replaced folders in {lane_name}")

    folders = scan_mod_folders(base_dir)
    staging_folders: Dict[str, str] = scan_mod_folders(staging_dir) if staging_dir else {}

    for folder_name in sorted(folders.keys()):
        if not folder_name.startswith("AGF-4Modders-"):
            continue

        match = re.match(r"^AGF-4Modders-(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if not match:
            continue

        suffix = match.group(1)
        folder_path = folders[folder_name]

        if staging_dir:
            staging_has_4modders = any(
                n.startswith(f"AGF-4Modders-{suffix}-v") for n in staging_folders
            )
            if not staging_has_4modders:
                # Staging no longer uses the 4Modders name for this suffix.
                # The 4Modders folder here is stale; delete it and keep the NoEAC counterpart.
                for legacy_prefix in ("AGF-NoEAC-", "AGF-HUDPluszOther-"):
                    legacy_pattern = re.compile(rf"^{re.escape(legacy_prefix + suffix)}-v.+$")
                    has_counterpart = any(legacy_pattern.match(n) for n in folders if n != folder_name)
                    if has_counterpart:
                        if maybe_remove_dir(folder_path, dry_run, log):
                            log.info(
                                f"Removed stale 4Modders folder in {lane_name}: {folder_name} "
                                f"(staging now uses a non-4Modders name for suffix '{suffix}')"
                            )
                        break
                continue

        for legacy_prefix in ("AGF-NoEAC-", "AGF-HUDPluszOther-"):
            legacy_pattern = re.compile(rf"^{re.escape(legacy_prefix + suffix)}-v.+$")
            for candidate_name, candidate_path in list(folders.items()):
                if candidate_name == folder_name:
                    continue
                if not legacy_pattern.match(candidate_name):
                    continue
                if maybe_remove_dir(candidate_path, dry_run, log):
                    log.info(f"Removed legacy folder in {lane_name}: {candidate_name} -> replaced by {folder_name}")


def cleanup_legacy_4modders_replacements_with_suffixes(
    base_dir: str,
    renamed_suffixes: set[str],
    dry_run: bool,
    log: Logger,
    lane_label: str,
) -> None:
    """Remove legacy NoEAC/HUDPluszOther folders whose suffix now exists as AGF-4Modders."""
    if not os.path.isdir(base_dir) or not renamed_suffixes:
        return

    folders = scan_mod_folders(base_dir)
    for folder_name, folder_path in list(folders.items()):
        match = re.match(r"^(AGF-NoEAC-|AGF-HUDPluszOther-)(.+)-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
        if not match:
            continue

        suffix = match.group(2)
        if suffix not in renamed_suffixes:
            continue

        replacement_name = f"AGF-4Modders-{suffix}"
        if maybe_remove_dir(folder_path, dry_run, log):
            log.info(f"Removed legacy folder in {lane_label}: {folder_name} -> replaced by {replacement_name}")


def cleanup_older_versions_in_dir(base_dir: str, dry_run: bool, log: Logger) -> None:
    """Keep only the newest folder per base mod name in a lane."""
    lane_name = os.path.basename(base_dir.rstrip("\\/"))
    log.info(f"Cleanup: Remove older duplicate versions in {lane_name}")

    folders = scan_mod_folders(base_dir)
    by_base: Dict[str, List[Tuple[str, str, str]]] = {}
    for folder_name, folder_path in folders.items():
        base_name = get_base_mod_name(folder_name)
        version = get_modinfo_version(folder_path)
        if not version:
            version_match = re.search(r"-v([0-9][0-9a-zA-Z\.-]*)$", folder_name)
            version = version_match.group(1) if version_match else "0.0.0"
        by_base.setdefault(base_name, []).append((folder_name, folder_path, version))

    for base_name, entries in by_base.items():
        if len(entries) <= 1:
            continue

        sorted_entries = sorted(entries, key=lambda x: (tuple(int(p) for p in re.findall(r"\d+", x[2]) or [0]), x[0]))
        keep_name, keep_path, keep_ver = sorted_entries[-1]
        for folder_name, folder_path, version in sorted_entries[:-1]:
            if maybe_remove_dir(folder_path, dry_run, log):
                log.info(
                    f"Removed older duplicate in {lane_name}: {folder_name} v{version} "
                    f"(kept {keep_name} v{keep_ver})"
                )


# =============================================================
# STEP 6: ZIP MODS + PACKS
# =============================================================
def collect_publishready_folders() -> List[str]:
    return sorted(scan_mod_folders(PUBLISH_READY).keys())


def build_zip_arcname(*parts: str) -> str:
    """Build a ZIP entry path using POSIX separators for cross-platform extraction."""
    normalized_parts: List[str] = []
    for part in parts:
        if part is None:
            continue
        text = str(part).replace("\\", "/").strip("/")
        if text:
            normalized_parts.append(text)
    return "/".join(normalized_parts)


def zip_mod_folder(mod_folder: str, dry_run: bool, log: Logger) -> Tuple[str, bool]:
    mod_path = os.path.join(PUBLISH_READY, mod_folder)
    base_name = get_base_mod_name(mod_folder)
    zip_name = f"{base_name}.zip"
    zip_path = os.path.join(ZIP_OUTPUT, zip_name)
    compression = zipfile.ZIP_STORED if base_name in ZIP_STORED_MOD_BASES else zipfile.ZIP_DEFLATED

    if dry_run:
        log.info(f"[DRYRUN] Would create mod zip: {zip_name}")
        return zip_name, True

    try:
        with zipfile.ZipFile(zip_path, "w", compression) as zipf:
            for root, dirs, files in os.walk(mod_path):
                dirs.sort()
                for file in sorted(files):
                    file_path = os.path.join(root, file)
                    arcname = build_zip_arcname(mod_folder, os.path.relpath(file_path, mod_path))
                    zipf.write(file_path, arcname, compress_type=compression)
        return zip_name, True
    except Exception as ex:
        log.error(f"Failed creating mod zip {zip_name}: {ex}")
        return zip_name, False


def zip_category(pack_name: str, root_mods: List[str], optionals_map: Optional[Dict[str, List[str]]], dry_run: bool, log: Logger) -> bool:
    zip_path = os.path.join(ZIP_OUTPUT, f"{pack_name}.zip")
    if dry_run:
        log.info(f"[DRYRUN] Would create pack zip: {pack_name}.zip")
        return True

    try:
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zipf:
            for mod_folder in root_mods:
                mod_path = os.path.join(PUBLISH_READY, mod_folder)
                if not os.path.isdir(mod_path):
                    continue
                for root, dirs, files in os.walk(mod_path):
                    dirs.sort()
                    for file in sorted(files):
                        file_path = os.path.join(root, file)
                        arcname = build_zip_arcname(mod_folder, os.path.relpath(file_path, mod_path))
                        zipf.write(file_path, arcname)

            if optionals_map:
                for opt_folder, opt_mods in optionals_map.items():
                    for mod_folder in opt_mods:
                        mod_path = os.path.join(PUBLISH_READY, mod_folder)
                        if not os.path.isdir(mod_path):
                            continue
                        for root, dirs, files in os.walk(mod_path):
                            dirs.sort()
                            for file in sorted(files):
                                file_path = os.path.join(root, file)
                                arcname = build_zip_arcname(opt_folder, mod_folder, os.path.relpath(file_path, mod_path))
                                zipf.write(file_path, arcname)
        return True
    except Exception as ex:
        log.error(f"Failed creating pack zip {pack_name}.zip: {ex}")
        return False


def build_pack_definitions(all_folders: List[str]) -> List[Tuple[str, List[str], Optional[Dict[str, List[str]]]]]:
    backpackplus_mods = [f for f in all_folders if f.startswith("AGF-BackpackPlus-")]
    hudplus_mods = [f for f in all_folders if f.startswith("AGF-HUDPlus-")]
    hudpluszother_mods = [f for f in all_folders if f.startswith("AGF-HUDPluszOther-")]
    noeac_mods = [f for f in all_folders if f.startswith("AGF-NoEAC-")]
    modders_mods = [f for f in all_folders if f.startswith("AGF-4Modders-")]
    vp_mods = [f for f in all_folders if f.startswith("AGF-VP-")]
    special_mods = [f for f in all_folders if f.startswith("zzzAGF-Special")]
    requested_mods = [f for f in all_folders if f.startswith("AGF-Requested-") or f.startswith("zzzAGF-Requested-")]

    backpackplus_84 = next((f for f in backpackplus_mods if "84Slots" in f), None)

    packs: List[Tuple[str, List[str], Optional[Dict[str, List[str]]]]] = []
    packs.append(("00_BackpackPlus_All", backpackplus_mods, None))

    giggle_root = hudplus_mods + vp_mods + special_mods + ([backpackplus_84] if backpackplus_84 else [])
    giggle_optionals = {
        ".Optionals-BackpackPlus": backpackplus_mods,
        ".Optionals-HUDPlus": hudplus_mods + hudpluszother_mods,
        ".Optionals-NoEAC": noeac_mods,
        ".Optionals-4Modders": modders_mods,
        ".Optionals-Requested": requested_mods,
    }
    giggle_optionals = {k: v for k, v in giggle_optionals.items() if v}
    packs.append(("00_GigglePack_All", giggle_root, giggle_optionals or None))

    hudplus_all_root = hudplus_mods + special_mods
    hudplus_all_optionals = {
        ".Optionals-NoEAC": noeac_mods,
        ".Optionals-HUDPluszOther": hudpluszother_mods,
    }
    hudplus_all_optionals = {k: v for k, v in hudplus_all_optionals.items() if v}
    packs.append(("00_HUDPlus_All", hudplus_all_root, hudplus_all_optionals or None))
    packs.append(("00_HUDPluszOther_All", hudpluszother_mods, None))
    packs.append(("00_NoEAC_All", noeac_mods, None))
    packs.append(("00_4Modders_All", modders_mods, None))
    packs.append(("00_Requested_All", requested_mods, None))

    vp_all_root = vp_mods + special_mods
    vp_all_optionals = {".Optionals-NoEAC": noeac_mods}
    vp_all_optionals = {k: v for k, v in vp_all_optionals.items() if v}
    packs.append(("00_VP_All", vp_all_root, vp_all_optionals or None))

    return packs


def gather_mod_versions_by_base() -> Dict[str, str]:
    versions: Dict[str, str] = {}
    for folder_name, folder_path in scan_mod_folders(PUBLISH_READY).items():
        base_name = get_base_mod_name(folder_name)
        version = get_modinfo_version(folder_path) or "0.0.0"
        versions[base_name] = version
    return versions


def bump_patch_version(version: str) -> str:
    numbers = [int(p) for p in re.findall(r"\d+", version)]
    while len(numbers) < 3:
        numbers.append(0)
    numbers[2] += 1
    return f"{numbers[0]}.{numbers[1]}.{numbers[2]}"


def parse_three_part_version(version: str) -> Tuple[int, int, int]:
    numbers = [int(p) for p in re.findall(r"\d+", version)]
    while len(numbers) < 3:
        numbers.append(0)
    return numbers[0], numbers[1], numbers[2]


def format_three_part_version(major: int, minor: int, patch: int) -> str:
    return f"{major}.{minor}.{patch}"


def format_release_stamp(dt_value: dt.datetime) -> str:
    stamp = dt_value.strftime("%B %d, %Y %I:%M%p")
    stamp = stamp.lstrip("0").replace(" 0", " ")
    return stamp.replace("AM", "am").replace("PM", "pm")


def extract_mod_suffix_for_rename(mod_name: str) -> str:
    """Return the mod-name tail after AGF category for rename matching heuristics."""
    if mod_name.startswith("AGF-"):
        rest = mod_name[len("AGF-"):]
        if "-" in rest:
            return rest.split("-", 1)[1]
    return mod_name


def detect_renamed_mods(
    added_mods: List[Tuple[str, str]],
    removed_mods: List[Tuple[str, str]],
) -> Tuple[List[Tuple[str, str, str]], List[Tuple[str, str]], List[Tuple[str, str]]]:
    """Detect renamed mods and remove them from added/removed buckets.

    Primary match: same version and same mod suffix (category changed).
    Secondary match: same version and best similarity score.
    """
    remaining_added = list(added_mods)
    remaining_removed = list(removed_mods)
    renamed_mods: List[Tuple[str, str, str]] = []

    removed_by_key: Dict[Tuple[str, str], List[Tuple[str, str]]] = {}
    for old_name, old_ver in remaining_removed:
        key = (old_ver, extract_mod_suffix_for_rename(old_name).lower())
        removed_by_key.setdefault(key, []).append((old_name, old_ver))

    unmatched_added: List[Tuple[str, str]] = []
    for new_name, new_ver in remaining_added:
        key = (new_ver, extract_mod_suffix_for_rename(new_name).lower())
        matches = removed_by_key.get(key, [])
        if matches:
            old_name, old_ver = matches.pop(0)
            renamed_mods.append((old_name, new_name, old_ver))
        else:
            unmatched_added.append((new_name, new_ver))

    remaining_added = unmatched_added
    remaining_removed = []
    for entries in removed_by_key.values():
        remaining_removed.extend(entries)

    if remaining_added and remaining_removed:
        unmatched_removed = list(remaining_removed)
        final_added: List[Tuple[str, str]] = []
        for new_name, new_ver in remaining_added:
            candidates = [(old_name, old_ver) for old_name, old_ver in unmatched_removed if old_ver == new_ver]
            if not candidates:
                final_added.append((new_name, new_ver))
                continue

            best = max(
                candidates,
                key=lambda item: difflib.SequenceMatcher(
                    None,
                    extract_mod_suffix_for_rename(item[0]).lower(),
                    extract_mod_suffix_for_rename(new_name).lower(),
                ).ratio(),
            )
            best_ratio = difflib.SequenceMatcher(
                None,
                extract_mod_suffix_for_rename(best[0]).lower(),
                extract_mod_suffix_for_rename(new_name).lower(),
            ).ratio()

            if best_ratio >= 0.8:
                renamed_mods.append((best[0], new_name, best[1]))
                unmatched_removed.remove(best)
            else:
                final_added.append((new_name, new_ver))

        remaining_added = final_added
        remaining_removed = unmatched_removed

    renamed_mods.sort(key=lambda item: (item[0].lower(), item[1].lower()))
    remaining_added.sort(key=lambda item: item[0].lower())
    remaining_removed.sort(key=lambda item: item[0].lower())
    return renamed_mods, remaining_added, remaining_removed


def load_json_file(path: str) -> Dict[str, object]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
        return data if isinstance(data, dict) else {}
    except Exception:
        return {}


def get_gigglepack_release_dir_for_write() -> str:
    return GIGGLEPACK_RELEASE_ROOT_DIR


def get_gigglepack_release_dir_for_read() -> str:
    preferred = GIGGLEPACK_RELEASE_ROOT_DIR
    legacy = os.path.join(ZIP_OUTPUT, RELEASE_META_DIR_NAME, GIGGLEPACK_RELEASE_DATA_DIR)
    if os.path.isdir(preferred):
        return preferred
    if os.path.isdir(legacy):
        return legacy
    return preferred


def write_text_file(path: str, content: str, dry_run: bool, log: Logger) -> None:
    if dry_run:
        log.info(f"[DRYRUN] Would write file: {path}")
        return
    atomic_write_text(path, content, encoding="utf-8")


def split_discord_message(content: str, max_len: int = 2000) -> List[str]:
    if len(content) <= max_len:
        return [content]

    lines = content.splitlines(keepends=True)
    chunks: List[str] = []
    current = ""
    for line in lines:
        if len(current) + len(line) <= max_len:
            current += line
            continue

        if current:
            chunks.append(current.rstrip("\n"))
            current = ""

        while len(line) > max_len:
            chunks.append(line[:max_len])
            line = line[max_len:]
        current = line

    if current:
        chunks.append(current.rstrip("\n"))
    return chunks


def post_discord_webhook_message(webhook_url: str, content: str, dry_run: bool, log: Logger) -> bool:
    parts = split_discord_message(content)
    if dry_run:
        log.info(f"[DRYRUN] Would post Discord message in {len(parts)} part(s)")
        return True

    for idx, part in enumerate(parts, start=1):
        payload = json.dumps({"content": part}).encode("utf-8")
        request = urllib.request.Request(
            webhook_url,
            data=payload,
            headers={"Content-Type": "application/json", "User-Agent": "AGF-GigglePack-Automation"},
            method="POST",
        )
        try:
            with urllib.request.urlopen(request, timeout=20) as response:
                status = response.getcode()
                if status not in (200, 204):
                    log.warn(f"Discord webhook returned HTTP {status} on part {idx}/{len(parts)}")
                    return False
        except urllib.error.URLError as ex:
            log.warn(f"Failed to post Discord webhook part {idx}/{len(parts)}: {ex}")
            return False
        except Exception as ex:
            log.warn(f"Unexpected Discord webhook error on part {idx}/{len(parts)}: {ex}")
            return False

    return True


def maybe_post_discord_release_update(
    release_result: Dict[str, object],
    webhook_url: str,
    dry_run: bool,
    log: Logger,
) -> None:
    if not release_result:
        return

    has_update = bool(release_result.get("has_update", False))
    if not has_update:
        log.info("GigglePack update not detected; skipping Discord auto-post")
        return

    webhook = webhook_url.strip()
    if not webhook:
        log.info(
            f"Discord auto-post skipped: set --discord-webhook-url or env {DISCORD_WEBHOOK_ENV_VAR} to enable"
        )
        return

    discord_text = str(release_result.get("discord_text", "")).strip()
    if not discord_text:
        log.warn("Discord auto-post skipped: generated Discord text is empty")
        return

    if post_discord_webhook_message(webhook, discord_text, dry_run, log):
        log.info("Discord webhook post completed")


def generate_mod_images(dry_run: bool, log: Logger) -> None:
    """Generate _01.png composite mod images into 00_Images/_generated/."""
    image_script = os.path.join(WORKFLOW_DIR, "SCRIPT-GenerateModImages.py")
    if not os.path.isfile(image_script):
        log.warn(f"Image generation script not found: {image_script}")
        return

    cmd = [sys.executable, image_script, "--changed-only"]
    if dry_run:
        cmd.append("--dry-run")

    log.info(f"Generating mod images: {' '.join(cmd)}")
    try:
        result = subprocess.run(cmd, check=False, capture_output=True, text=True)
        for line in (result.stdout or "").splitlines():
            log.info(f"[ImageGen] {line}")
        for line in (result.stderr or "").splitlines():
            if line.strip():
                log.warn(f"[ImageGen] {line}")
        if result.returncode != 0:
            log.warn(f"Image generation script exited with code {result.returncode}")
    except Exception as ex:
        log.warn(f"Failed to run image generation script: {ex}")


def copy_mod_images_to_mod_folders(
    dry_run: bool,
    log: Logger,
    mod_dirs: Optional[Tuple[str, ...]] = None,
) -> Set[str]:
    """No-op: mod image copying has been disabled."""
    log.info("Mod image copy disabled (no longer deployed to mod folders)")
    return set()


def generate_gigglepack_release_artifacts(
    dry_run: bool,
    log: Logger,
    append_to_latest_release: bool = False,
) -> Dict[str, object]:
    """Create versioned GigglePack zips and release notes for Discord/GitHub usage."""
    log.info("Step 6.5: Generate GigglePack release metadata + changelog outputs")

    canonical_zip_path = os.path.join(ZIP_OUTPUT, GIGGLEPACK_CANONICAL_ZIP)
    if not os.path.isfile(canonical_zip_path) and not dry_run:
        log.warn(f"GigglePack release metadata skipped: missing {canonical_zip_path}")
        return {"has_update": False, "discord_text": ""}

    release_meta_dir = get_gigglepack_release_dir_for_write()
    state_path = os.path.join(release_meta_dir, "gigglepack-release-state.json")
    discord_path = os.path.join(release_meta_dir, "Discord", "discord-post.txt")
    markdown_path = os.path.join(release_meta_dir, "gigglepack-release-history.md")

    prev_state = load_json_file(state_path)
    prev_version = str(prev_state.get("gigglepack_version", "0.0.0"))
    prev_mods = prev_state.get("mods", {})
    if not isinstance(prev_mods, dict):
        prev_mods = {}

    marker_path = os.path.join(release_meta_dir, GIGGLEPACK_MAJOR_BUMP_MARKER)
    major_bump_requested = os.path.isfile(marker_path)

    all_folders = collect_publishready_folders()
    packs = build_pack_definitions(all_folders)
    giggle_pack = next((pack for pack in packs if pack[0] == "00_GigglePack_All"), None)
    if not giggle_pack:
        log.warn("GigglePack release metadata skipped: could not resolve pack definition")
        return {"has_update": False, "discord_text": ""}

    _, giggle_root, giggle_optionals = giggle_pack
    giggle_mod_folder_names = set(giggle_root)
    if giggle_optionals:
        for opt_mods in giggle_optionals.values():
            giggle_mod_folder_names.update(opt_mods)

    current_versions = gather_mod_versions_by_base()
    giggle_mod_versions: Dict[str, str] = {}
    for folder_name in sorted(giggle_mod_folder_names):
        base_name = get_base_mod_name(folder_name)
        if base_name in current_versions:
            giggle_mod_versions[base_name] = current_versions[base_name]

    updated_existing_mods: List[Tuple[str, str, str]] = []
    added_mods: List[Tuple[str, str]] = []
    removed_mods: List[Tuple[str, str]] = []

    for mod_name in sorted(giggle_mod_versions):
        current_ver = giggle_mod_versions[mod_name]
        previous_ver = str(prev_mods.get(mod_name, ""))
        if not previous_ver:
            added_mods.append((mod_name, current_ver))
        elif compare_versions(current_ver, previous_ver) > 0:
            updated_existing_mods.append((mod_name, previous_ver, current_ver))

    for mod_name in sorted(prev_mods):
        if mod_name not in giggle_mod_versions:
            removed_mods.append((mod_name, str(prev_mods.get(mod_name, ""))))

    renamed_mods, added_mods, removed_mods = detect_renamed_mods(added_mods, removed_mods)

    is_baseline_release = not prev_state
    append_latest_mode = bool(append_to_latest_release and not is_baseline_release)
    if append_to_latest_release and is_baseline_release:
        log.warn(
            "Append-latest requested but no prior GigglePack release state exists; "
            "falling back to normal finalize."
        )

    if append_latest_mode:
        release_version = prev_version
    elif is_baseline_release:
        release_version = GIGGLEPACK_BASELINE_VERSION
    elif major_bump_requested:
        prev_major, _, _ = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major + 1, 0, 0)
    elif added_mods:
        prev_major, prev_minor, _ = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major, prev_minor + 1, 0)
    elif updated_existing_mods or renamed_mods or removed_mods:
        prev_major, prev_minor, prev_patch = parse_three_part_version(prev_version)
        release_version = format_three_part_version(prev_major, prev_minor, prev_patch + 1)
    else:
        release_version = prev_version

    has_update = bool(
        is_baseline_release
        or (major_bump_requested and not append_latest_mode)
        or added_mods
        or updated_existing_mods
        or renamed_mods
        or removed_mods
    )

    if not has_update:
        existing_discord_text = ""
        if os.path.isfile(discord_path):
            try:
                with open(discord_path, "r", encoding="utf-8") as f:
                    existing_discord_text = f.read().strip()
            except Exception:
                existing_discord_text = ""

        # Do not rewrite latest files/state on no-change runs.
        return {
            "has_update": False,
            "release_version": prev_version,
            "discord_text": existing_discord_text,
            "discord_path": discord_path,
        }

    if append_latest_mode:
        previous_release_version = str(prev_state.get("previous_gigglepack_version", prev_version)).strip() or prev_version
    else:
        previous_release_version = prev_version

    versioned_zip_name = f"{GIGGLEPACK_VERSIONED_ZIP_PREFIX}{release_version}.zip"
    versioned_zip_path = os.path.join(ZIP_OUTPUT, versioned_zip_name)

    if dry_run:
        log.info(f"[DRYRUN] Would copy {GIGGLEPACK_CANONICAL_ZIP} -> {versioned_zip_name}")
    else:
        shutil.copy2(canonical_zip_path, versioned_zip_path)

    new_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "to": new_ver}
        for mod_name, new_ver in added_mods
    ]
    updated_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "from": old_ver, "to": new_ver}
        for mod_name, old_ver, new_ver in updated_existing_mods
    ]
    renamed_mod_entries: List[Dict[str, str]] = [
        {"from_mod": old_mod_name, "to_mod": new_mod_name, "version": ver}
        for old_mod_name, new_mod_name, ver in renamed_mods
    ]
    removed_mod_entries: List[Dict[str, str]] = [
        {"mod": mod_name, "from": old_ver}
        for mod_name, old_ver in removed_mods
    ]

    beta_display_by_base: Dict[str, bool] = {}
    for folder_name, folder_path in scan_mod_folders(PUBLISH_READY).items():
        modinfo_path = os.path.join(folder_path, "ModInfo.xml")
        mod_name, _ = parse_modinfo(modinfo_path, folder_name)
        display_name = get_modinfo_display_name(modinfo_path, mod_name)
        beta_display_by_base[get_base_mod_name(folder_name)] = "BETA" in display_name

    def maybe_beta_display(mod_name: str, version_value: str) -> str:
        if beta_display_by_base.get(mod_name, False) and not str(version_value).endswith("-BETA"):
            return f"{version_value}-BETA"
        return version_value

    new_mod_entries_display: List[Dict[str, str]] = [
        {
            "mod": entry["mod"],
            "to": entry["to"],
            "to_display": maybe_beta_display(entry["mod"], entry["to"]),
        }
        for entry in new_mod_entries
    ]
    updated_mod_entries_display: List[Dict[str, str]] = [
        {
            "mod": entry["mod"],
            "from": entry["from"],
            "to": entry["to"],
            "from_display": maybe_beta_display(entry["mod"], entry["from"]),
            "to_display": maybe_beta_display(entry["mod"], entry["to"]),
        }
        for entry in updated_mod_entries
    ]
    renamed_mod_entries_display: List[Dict[str, str]] = [
        {
            "from_mod": entry["from_mod"],
            "to_mod": entry["to_mod"],
            "version": entry["version"],
            "version_display": maybe_beta_display(entry["to_mod"], entry["version"]),
        }
        for entry in renamed_mod_entries
    ]
    removed_mod_entries_display: List[Dict[str, str]] = [
        {
            "mod": entry["mod"],
            "from": entry["from"],
            "from_display": maybe_beta_display(entry["mod"], entry["from"]),
        }
        for entry in removed_mod_entries
    ]

    # Baseline special handling: keep changelog intentionally focused and readable.
    if release_version == GIGGLEPACK_BASELINE_VERSION:
        focused_entries: List[Dict[str, str]] = []
        for mod_name in GIGGLEPACK_V100_FOCUS_MODS:
            if mod_name in giggle_mod_versions:
                focused_entries.append({"mod": mod_name, "to": giggle_mod_versions[mod_name]})
        if focused_entries:
            new_mod_entries = focused_entries
            new_mod_entries_display = [
                {
                    "mod": entry["mod"],
                    "to": entry["to"],
                    "to_display": maybe_beta_display(entry["mod"], entry["to"]),
                }
                for entry in focused_entries
            ]
            updated_mod_entries = []
            updated_mod_entries_display = []
            renamed_mod_entries = []
            renamed_mod_entries_display = []
            removed_mod_entries = []
            removed_mod_entries_display = []

    def merge_state_entries(
        existing_entries: object,
        incoming_entries: List[Dict[str, str]],
        identity_keys: Tuple[str, ...],
    ) -> List[Dict[str, str]]:
        merged: List[Dict[str, str]] = []
        seen: Set[Tuple[str, ...]] = set()

        if isinstance(existing_entries, list):
            for entry in existing_entries:
                if not isinstance(entry, dict):
                    continue
                normalized = {str(k): str(v) for k, v in entry.items()}
                identity = tuple(normalized.get(key, "").strip() for key in identity_keys)
                if not any(identity):
                    continue
                if identity in seen:
                    continue
                seen.add(identity)
                merged.append(normalized)

        for entry in incoming_entries:
            normalized = {str(k): str(v) for k, v in entry.items()}
            identity = tuple(normalized.get(key, "").strip() for key in identity_keys)
            if not any(identity):
                continue
            if identity in seen:
                continue
            seen.add(identity)
            merged.append(normalized)

        return merged

    state_new_mod_entries = new_mod_entries_display
    state_updated_mod_entries = updated_mod_entries_display
    state_renamed_mod_entries = renamed_mod_entries_display
    state_removed_mod_entries = removed_mod_entries_display

    if append_latest_mode:
        state_new_mod_entries = merge_state_entries(prev_state.get("new_mods", []), new_mod_entries_display, ("mod", "to"))
        state_updated_mod_entries = merge_state_entries(prev_state.get("updated_mods", []), updated_mod_entries_display, ("mod", "from", "to"))
        state_renamed_mod_entries = merge_state_entries(prev_state.get("renamed_mods", []), renamed_mod_entries_display, ("from_mod", "to_mod", "version"))
        state_removed_mod_entries = merge_state_entries(prev_state.get("removed_mods", []), removed_mod_entries_display, ("mod", "from"))
        log.info("Append-latest mode: merged current changes into the latest finalized changelog entry")

    new_mod_lines = [
        f"- {mod_download_markdown_link(entry['mod'])} (new: v{entry.get('to_display', entry['to'])})"
        for entry in state_new_mod_entries
    ]
    updated_existing_lines = [
        f"- {mod_download_markdown_link(entry['mod'])} "
        f"(v{entry.get('from_display', entry['from'])} -> v{entry.get('to_display', entry['to'])})"
        for entry in state_updated_mod_entries
    ]
    renamed_lines = [
        f"- {mod_download_markdown_link(entry['to_mod'])} "
        f"(renamed from {entry['from_mod']}, v{entry.get('version_display', entry['version'])})"
        for entry in state_renamed_mod_entries
    ]
    removed_lines = [
        f"- {entry['mod']} (was v{entry.get('from_display', entry['from'])})"
        for entry in state_removed_mod_entries
    ]

    if has_update:
        now_dt = dt.datetime.now()
    else:
        previous_release_stamp = str(prev_state.get("released_at", "")).strip()
        try:
            now_dt = dt.datetime.strptime(previous_release_stamp, "%Y-%m-%d %H:%M:%S")
        except Exception:
            now_dt = dt.datetime.now()
    now_iso = now_dt.strftime("%Y-%m-%d %H:%M:%S")
    now_display = format_release_stamp(now_dt)

    discord_text = render_discord_post_from_template(
        release_version=release_version,
        now_iso=now_display,
        versioned_zip_name=versioned_zip_name,
        previous_release_version=previous_release_version,
        new_mod_entries=state_new_mod_entries,
        updated_mod_entries=state_updated_mod_entries,
        renamed_mod_entries=state_renamed_mod_entries,
        removed_mod_entries=state_removed_mod_entries,
        log=log,
    )

    if not discord_text:
        fallback_chunks: List[str] = [
            f"GigglePack v{release_version} - {now_display}",
            f"Download: {zip_download_link(GIGGLEPACK_CANONICAL_ZIP)}",
            (
                f"Change summary: +{len(new_mod_lines)} new, ~{len(updated_existing_lines)} updated, "
                f"={len(renamed_lines)} renamed, -{len(removed_lines)} removed"
            ),
            "",
            "**New mods:**",
        ]
        fallback_chunks.extend(new_mod_lines or ["- None"])
        fallback_chunks.extend(["", "**Updated existing mods:**"])
        fallback_chunks.extend(updated_existing_lines or ["- None"])
        fallback_chunks.extend(["", "**Renamed mods:**"])
        fallback_chunks.extend(renamed_lines or ["- None"])
        fallback_chunks.extend(["", "**Removed mods:**"])
        fallback_chunks.extend(removed_lines or ["- None"])
        discord_text = "\n".join(fallback_chunks).strip() + "\n"

    markdown_lines: List[str] = [
        f"## GigglePack v{release_version} ({now_display})",
        (
            f"### Summary: +{len(new_mod_lines)} new, ~{len(updated_existing_lines)} updated, "
            f"={len(renamed_lines)} renamed, -{len(removed_lines)} removed"
        ),
        "- **New Mods**",
    ]
    if new_mod_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in new_mod_lines])
    else:
        markdown_lines.append("  - None")

    markdown_lines.append("- **Updated Existing Mods**")
    if updated_existing_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in updated_existing_lines])
    else:
        markdown_lines.append("  - None")

    markdown_lines.append("- **Renamed Mods**")
    if renamed_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in renamed_lines])
    else:
        markdown_lines.append("  - None")

    markdown_lines.append("- **Removed Mods**")
    if removed_lines:
        markdown_lines.extend([re.sub(r"^-\s", "  - ", line) for line in removed_lines])
    else:
        markdown_lines.append("  - None")

    new_markdown_entry = "\n".join(markdown_lines).strip()

    existing_entries: List[str] = []
    if os.path.isfile(markdown_path):
        try:
            with open(markdown_path, "r", encoding="utf-8") as f:
                existing_content = f.read().strip()
            if existing_content:
                if existing_content.startswith("# GigglePack Release Changelog"):
                    existing_body = existing_content.split("\n", 1)[1].strip() if "\n" in existing_content else ""
                else:
                    existing_body = existing_content
                if existing_body:
                    existing_entries = []
                    for chunk in re.split(r"\n\n---\n\n", existing_body):
                        chunk = re.sub(r'^Newest entries appear at the top\.\s*', '', chunk.strip())
                        if chunk:
                            existing_entries.append(chunk)
        except Exception:
            existing_entries = []

    def entry_version(entry_text: str) -> str:
        match = re.search(r"^#{1,2}\s*GigglePack\s+v([0-9]+\.[0-9]+\.[0-9]+)", entry_text, re.MULTILINE)
        return match.group(1) if match else ""

    filtered_entries = [entry for entry in existing_entries if entry_version(entry) != release_version]
    all_entries = [new_markdown_entry] + filtered_entries
    markdown_text = (
        "# GigglePack Release Changelog\n\n"
        + "\n\n---\n\n".join(all_entries).strip()
        + "\n"
    )

    write_text_file(discord_path, discord_text, dry_run, log)
    write_text_file(markdown_path, markdown_text, dry_run, log)

    state_payload = {
        "gigglepack_version": release_version,
        "previous_gigglepack_version": previous_release_version,
        "is_baseline_release": is_baseline_release,
        "released_at": now_iso,
        "mods": giggle_mod_versions,
        "updated_mods": state_updated_mod_entries,
        "new_mods": state_new_mod_entries,
        "renamed_mods": state_renamed_mod_entries,
        "removed_mods": state_removed_mod_entries,
        "versioned_zip": versioned_zip_name,
        "change_counts": {
            "new_mods": len(state_new_mod_entries),
            "updated_existing_mods": len(state_updated_mod_entries),
            "renamed_mods": len(state_renamed_mod_entries),
            "removed_mods": len(state_removed_mod_entries),
        },
    }

    if dry_run:
        log.info(f"[DRYRUN] Would write state file: {state_path}")
    else:
        os.makedirs(release_meta_dir, exist_ok=True)
        atomic_write_json(state_path, state_payload, ensure_ascii=True, indent=2)
        if major_bump_requested and not append_latest_mode:
            try:
                os.remove(marker_path)
                log.info(f"Consumed major bump marker: {marker_path}")
            except Exception as ex:
                log.warn(f"Could not remove major bump marker {marker_path}: {ex}")

    return {
        "has_update": has_update,
        "release_version": release_version,
        "discord_text": discord_text,
        "discord_path": discord_path,
    }

    log.info(
        f"GigglePack release v{release_version}: {len(updated_existing_lines)} updated existing mods, "
        f"{len(new_mod_entries)} new mods, {len(removed_lines)} removed mods"
    )


def create_all_zips(dry_run: bool, workers: int, log: Logger) -> List[str]:
    log.info("Step 6: Create all mod and category zip files")

    if dry_run:
        log.info(f"[DRYRUN] Would ensure zip output directory exists: {ZIP_OUTPUT}")
    else:
        os.makedirs(ZIP_OUTPUT, exist_ok=True)

    existing_zips = []
    if os.path.isdir(ZIP_OUTPUT):
        existing_zips = [f for f in os.listdir(ZIP_OUTPUT) if f.lower().endswith(".zip")]

    for file in existing_zips:
        if file == LEGACY_FINAL_GIGGLEPACK_ZIP:
            continue
        path = os.path.join(ZIP_OUTPUT, file)
        if dry_run:
            log.info(f"[DRYRUN] Would delete old zip: {file}")
        else:
            try:
                os.remove(path)
            except Exception as ex:
                log.warn(f"Failed to delete old zip {file}: {ex}")

    all_folders = collect_publishready_folders()

    created_mod_zips: List[str] = []
    if all_folders:
        with ThreadPoolExecutor(max_workers=max(1, workers)) as executor:
            futures = [executor.submit(zip_mod_folder, folder, dry_run, log) for folder in all_folders]
            for future in as_completed(futures):
                zip_name, ok = future.result()
                if ok:
                    created_mod_zips.append(zip_name)
                    log.stats.mod_zips_created += 1

    packs = build_pack_definitions(all_folders)

    for pack_name, root_mods, optionals_map in packs:
        has_root_content = bool(root_mods)
        has_optionals_content = bool(optionals_map and any(optionals_map.values()))
        if not has_root_content and not has_optionals_content:
            log.info(f"Pack zip skipped (empty): {pack_name}.zip")
            continue

        ok = zip_category(pack_name, root_mods, optionals_map, dry_run, log)
        if ok:
            log.stats.pack_zips_created += 1
            created_mod_zips.append(f"{pack_name}.zip")

    legacy_final_zip_path = os.path.join(ZIP_OUTPUT, LEGACY_FINAL_GIGGLEPACK_ZIP)
    canonical_zip_path = os.path.join(ZIP_OUTPUT, GIGGLEPACK_CANONICAL_ZIP)
    if not os.path.isfile(legacy_final_zip_path):
        if os.path.isfile(canonical_zip_path):
            if dry_run:
                log.info(
                    f"[DRYRUN] Would preserve legacy final GigglePack zip: {LEGACY_FINAL_GIGGLEPACK_ZIP}"
                )
            else:
                try:
                    shutil.copy2(canonical_zip_path, legacy_final_zip_path)
                    created_mod_zips.append(LEGACY_FINAL_GIGGLEPACK_ZIP)
                    log.info(
                        "Created preserved legacy final GigglePack zip from current canonical pack: "
                        f"{LEGACY_FINAL_GIGGLEPACK_ZIP}"
                    )
                except Exception as ex:
                    log.warn(f"Failed to create {LEGACY_FINAL_GIGGLEPACK_ZIP}: {ex}")
        else:
            log.warn(
                "Could not create preserved legacy final GigglePack zip because canonical zip is missing"
            )

    return sorted(set(created_mod_zips))


# =============================================================
# STEP 7: MAIN README
# =============================================================
DEFAULT_MAIN_README_CATEGORY_TEMPLATE = (
    "---\n\n"
    "<br>\n\n"
    "## **{{CATEGORY_TITLE}}**\n\n"
    "{{CATEGORY_DOWNLOAD_LINE}} - {{CATEGORY_DESCRIPTION}}\n\n"
    "*[(Back to Top)](#agf-7-days-to-die-mods)*\n\n"
    "---\n\n"
    "---\n"
)

DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE = (
    '<table><tr>\n'
    '<td width="160">{{MOD_BANNER}}</td>\n'
    '<td valign="top">\n'
    '<b>{{MOD_NAME}}</b> &nbsp;·&nbsp; v{{MOD_VERSION}} &nbsp;·&nbsp; <a href="{{DOWNLOAD_LINK}}">Download</a><br>\n'
    '{{MOD_DESCRIPTION}}\n'
    '{{MOD_FEATURES_BLOCK}}'
    '</td>\n'
    '</tr></table>\n'
    '\n---\n'
)


def render_main_readme_category_block(
    category_template: str,
    category_title: str,
    category_download_line: str,
    category_description: str,
) -> List[str]:
    block = re.sub(r"<!--.*?-->", "", category_template, flags=re.DOTALL)
    block = block.replace("{{CATEGORY_TITLE}}", category_title)
    block = block.replace("{{CATEGORY_DOWNLOAD_LINE}}", category_download_line)
    block = block.replace("{{CATEGORY_DESCRIPTION}}", category_description)
    # If no download line, collapse the resulting blank line it leaves behind
    block = re.sub(r"\n{3,}", "\n\n", block).strip("\n")
    return block.splitlines()


def generate_thumbnails(dry_run: bool, log: Logger) -> None:
    """Generate small Thumbnail_*.png files from the 1920x1080 * _01.png originals.

    Thumbnails are stored in IMAGES_THUMBNAIL_ROOT (00_Images/_generated/thumbnails/)
    at 640px wide (maintaining aspect ratio) for fast page loading.
    """
    log.info("Generating thumbnail images from full-size originals")

    if not os.path.isdir(IMAGES_GENERATED_ROOT):
        log.warn(f"Images directory not found: {IMAGES_GENERATED_ROOT}")
        return

    if dry_run:
        log.info(f"[DRYRUN] Would ensure thumbnails directory exists: {IMAGES_THUMBNAIL_ROOT}")
    else:
        os.makedirs(IMAGES_THUMBNAIL_ROOT, exist_ok=True)

    thumbnail_width = 640
    generated = 0
    skipped = 0

    for filename in os.listdir(IMAGES_GENERATED_ROOT):
        if not filename.endswith("_01.png"):
            continue
        if not filename.startswith("AGF-") and not filename.startswith("zzzAGF-"):
            continue

        base_name = get_base_mod_name(filename.replace("_01.png", ""))
        thumb_filename = f"Thumbnail_{base_name}.png"
        thumb_path = os.path.join(IMAGES_THUMBNAIL_ROOT, thumb_filename)
        full_path = os.path.join(IMAGES_GENERATED_ROOT, filename)

        if os.path.isfile(thumb_path):
            full_mtime = os.path.getmtime(full_path)
            thumb_mtime = os.path.getmtime(thumb_path)
            if thumb_mtime >= full_mtime:
                skipped += 1
                continue

        if dry_run:
            log.info(f"[DRYRUN] Would generate thumbnail: {thumb_filename} <- {filename}")
            generated += 1
            continue

        try:
            from PIL import Image
            img = Image.open(full_path)
            w, h = img.size
            if w > thumbnail_width:
                ratio = thumbnail_width / w
                new_h = int(h * ratio)
                img.thumbnail((thumbnail_width, new_h), Image.Resampling.LANCZOS)
            img.save(thumb_path, "PNG", optimize=True)
            generated += 1
        except Exception as ex:
            log.warn(f"Failed generating thumbnail for {filename}: {ex}")

    log.info(f"Thumbnail generation complete: {generated} generated, {skipped} up-to-date")


def build_mod_entry(
    folder_name: str,
    mod_entry_template: Optional[str] = None,
    compat_map: Optional[Dict[str, Dict[str, str]]] = None,
    mod_type_lines: Optional[Dict[str, str]] = None,
) -> str:
    mod_path = os.path.join(PUBLISH_READY, folder_name)
    modinfo_path = os.path.join(mod_path, "ModInfo.xml")

    name, version = parse_modinfo(modinfo_path, folder_name)
    display_name = get_modinfo_display_name(modinfo_path, name)
    version_display = format_version_for_display(version, display_name)
    desc = resolve_mod_description(mod_path, modinfo_path)
    link = zip_download_link(f"{get_base_mod_name(folder_name)}.zip")

    features_block = ""
    base_mod = get_base_mod_name(folder_name)
    if compat_map is not None:
        mod_type_id = (compat_map.get(base_mod) or {}).get("MOD_TYPE_ID", "").strip()
        type_line_map = mod_type_lines if mod_type_lines is not None else DEFAULT_MOD_TYPE_LINE_BY_ID
        mod_type_text = type_line_map.get(mod_type_id, "")
        if mod_type_text:
            features_block = f"<ul><li><em>{mod_type_text}</em></li></ul>\n"

    base_mod_name = get_base_mod_name(folder_name)
    repo_base = "https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/blob/main/00_Images/_generated"

    # Thumbnail image (small) for display on the page
    thumb_file = f"Thumbnail_{base_mod_name}.png"
    thumb_abs = os.path.join(IMAGES_THUMBNAIL_ROOT, thumb_file)
    # Full-size image (1920x1080) for click-to-view
    full_file = f"{base_mod_name}_01.png"
    full_abs = os.path.join(IMAGES_GENERATED_ROOT, full_file)

    banner_html = ""
    if os.path.isfile(thumb_abs) and os.path.isfile(full_abs):
        full_url = f"{repo_base}/{full_file}?raw=true"
        thumb_url = f"{repo_base}/thumbnails/{thumb_file}?raw=true"
        banner_html = f'<a href="{full_url}"><img src="{thumb_url}" width="150"></a>'
    elif os.path.isfile(full_abs):
        # Fallback: use full-size for both src and href if thumbnail is missing
        full_url = f"{repo_base}/{full_file}?raw=true"
        banner_html = f'<a href="{full_url}"><img src="{full_url}" width="150"></a>'

    template = re.sub(r"<!--.*?-->", "", (mod_entry_template or DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE), flags=re.DOTALL).strip("\n")
    entry = template
    entry = entry.replace("{{MOD_NAME}}", display_name)
    entry = entry.replace("{{MOD_VERSION}}", version_display)
    entry = entry.replace("{{DOWNLOAD_LINK}}", link)
    entry = entry.replace("{{MOD_DESCRIPTION}}", desc)
    entry = entry.replace("{{MOD_BANNER}}", banner_html)
    entry = entry.replace("{{MOD_FEATURES_BLOCK}}", features_block)
    entry = re.sub(r"\n{3,}", "\n\n", entry).strip()
    return entry


def load_gigglepack_release_state() -> Dict[str, object]:
    state_path = os.path.join(get_gigglepack_release_dir_for_read(), "gigglepack-release-state.json")
    return load_json_file(state_path)


def load_recent_gigglepack_release_entries(limit: int = 3) -> List[Dict[str, object]]:
    if limit <= 0:
        return []

    markdown_path = os.path.join(get_gigglepack_release_dir_for_read(), "gigglepack-release-history.md")
    if not os.path.isfile(markdown_path):
        return []

    try:
        with open(markdown_path, "r", encoding="utf-8") as f:
            lines = f.read().splitlines()
    except Exception:
        return []

    entries: List[Dict[str, object]] = []
    current: Optional[Dict[str, object]] = None
    section: Optional[str] = None

    for raw_line in lines:
        line = raw_line.strip()
        if not line:
            continue

        if line in {"# GigglePack Release Changelog", "Newest entries appear at the top.", "---", ""}:
            continue

        header_match = re.match(r"^##\s+GigglePack\s+v([0-9]+\.[0-9]+\.[0-9]+)\s+\((.+)\)$", line)
        if header_match:
            if current:
                entries.append(current)
            current = {
                "version": header_match.group(1).strip(),
                "stamp": header_match.group(2).strip(),
                "new": [],
                "updated": [],
                "renamed": [],
                "removed": [],
                "new_count": 0,
                "updated_count": 0,
                "renamed_count": 0,
                "removed_count": 0,
            }
            section = None
            continue

        if not current:
            continue

        summary_match = re.match(
            r"^###\s+Summary:\s*\+(\d+)\s+new,\s*~(\d+)\s+updated,\s*=(\d+)\s+renamed,\s*-(\d+)\s+removed$",
            line,
        )
        if summary_match:
            current["new_count"] = int(summary_match.group(1))
            current["updated_count"] = int(summary_match.group(2))
            current["renamed_count"] = int(summary_match.group(3))
            current["removed_count"] = int(summary_match.group(4))
            continue

        old_summary_match = re.match(
            r"^###\s+Summary:\s*\+(\d+)\s+new,\s*~(\d+)\s+updated,\s*-(\d+)\s+removed$",
            line,
        )
        if old_summary_match:
            current["new_count"] = int(old_summary_match.group(1))
            current["updated_count"] = int(old_summary_match.group(2))
            current["renamed_count"] = 0
            current["removed_count"] = int(old_summary_match.group(3))
            continue

        if line == "- **New Mods**":
            section = "new"
            continue
        if line == "- **Updated Existing Mods**":
            section = "updated"
            continue
        if line == "- **Renamed Mods**":
            section = "renamed"
            continue
        if line == "- **Removed Mods**":
            section = "removed"
            continue

        if section and line.startswith("- "):
            item_text = line[2:].strip()
            current_list = current.get(section)
            if isinstance(current_list, list):
                current_list.append(item_text)

    if current:
        entries.append(current)

    return entries[:limit]


def build_gigglepack_readme_release_lines(state: Dict[str, object]) -> List[str]:
    history_entries = load_recent_gigglepack_release_entries(limit=3)
    if not history_entries and not state:
        return []

    def escape_html(text: str) -> str:
        return text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

    def mod_download_html_link(mod_name: str) -> str:
        href = escape_html(zip_download_link(f"{get_base_mod_name(mod_name)}.zip"))
        return f"<a href=\"{href}\">{escape_html(mod_name)}</a>"

    def markdown_links_to_html(text: str) -> str:
        parts: List[str] = []
        last_idx = 0
        for match in re.finditer(r"\[([^\]]+)\]\(([^\)]+)\)", text):
            parts.append(escape_html(text[last_idx:match.start()]))
            label = escape_html(match.group(1).strip())
            href = escape_html(match.group(2).strip())
            parts.append(f"<a href=\"{href}\">{label}</a>")
            last_idx = match.end()
        parts.append(escape_html(text[last_idx:]))
        return "".join(parts)

    def render_release_item_html(item_text: str, section_name: str) -> str:
        if section_name == "removed":
            plain_text = re.sub(r"\[([^\]]+)\]\(([^\)]+)\)", r"\1", item_text)
            return escape_html(plain_text)

        if "[" in item_text and "](" in item_text:
            return markdown_links_to_html(item_text)

        if section_name in ("new", "updated"):
            match = re.match(r"^([A-Za-z0-9\-]+)(\s*\(.+\))$", item_text)
            if match:
                mod_name = match.group(1).strip()
                suffix = match.group(2)
                if is_agf_mod(mod_name):
                    return f"{mod_download_html_link(mod_name)}{escape_html(suffix)}"

        if section_name == "renamed":
            match = re.match(r"^([A-Za-z0-9\-]+)(\s*\(renamed from .+\))$", item_text)
            if match:
                mod_name = match.group(1).strip()
                suffix = match.group(2)
                if is_agf_mod(mod_name):
                    return f"{mod_download_html_link(mod_name)}{escape_html(suffix)}"

        return escape_html(item_text)

    parsed_entries: List[Dict[str, object]] = []
    if history_entries:
        for entry in history_entries:
            version = str(entry.get("version", "")).strip() or "unknown"
            stamp = str(entry.get("stamp", "")).strip()
            new_items = [render_release_item_html(str(item).strip(), "new") for item in entry.get("new", []) if str(item).strip()]
            updated_items = [render_release_item_html(str(item).strip(), "updated") for item in entry.get("updated", []) if str(item).strip()]
            renamed_items = [render_release_item_html(str(item).strip(), "renamed") for item in entry.get("renamed", []) if str(item).strip()]
            removed_items = [render_release_item_html(str(item).strip(), "removed") for item in entry.get("removed", []) if str(item).strip()]
            parsed_entries.append({
                "header": f"GigglePack v{escape_html(version)}" + (f" - {escape_html(stamp)}" if stamp else ""),
                "new_count": int(entry.get("new_count", len(new_items))),
                "updated_count": int(entry.get("updated_count", len(updated_items))),
                "renamed_count": int(entry.get("renamed_count", len(renamed_items))),
                "removed_count": int(entry.get("removed_count", len(removed_items))),
                "new_items": new_items,
                "updated_items": updated_items,
                "renamed_items": renamed_items,
                "removed_items": removed_items,
            })
    else:
        release_version = str(state.get("gigglepack_version", "")).strip() or "unknown"
        previous_release_version = str(state.get("previous_gigglepack_version", "")).strip()
        released_at = str(state.get("released_at", "")).strip()
        new_mods = state.get("new_mods", [])
        updated_mods = state.get("updated_mods", [])
        renamed_mods = state.get("renamed_mods", [])
        removed_mods = state.get("removed_mods", [])
        change_counts = state.get("change_counts", {}) if isinstance(state.get("change_counts", {}), dict) else {}

        def format_release_stamp(raw: str) -> str:
            if not raw:
                return ""
            try:
                parsed = dt.datetime.strptime(raw, "%Y-%m-%d %H:%M:%S")
                stamp = parsed.strftime("%B %d, %Y %I:%M%p")
                stamp = stamp.lstrip("0").replace(" 0", " ")
                return stamp.replace("AM", "am").replace("PM", "pm")
            except Exception:
                return raw

        new_items: List[str] = []
        if isinstance(new_mods, list):
            for entry in new_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    to_ver = str(entry.get("to_display", entry.get("to", ""))).strip()
                    if mod_name:
                        new_items.append(f"{mod_download_html_link(mod_name)} (new: v{escape_html(to_ver)})")

        updated_items: List[str] = []
        if isinstance(updated_mods, list):
            for entry in updated_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    from_ver = str(entry.get("from_display", entry.get("from", ""))).strip()
                    to_ver = str(entry.get("to_display", entry.get("to", ""))).strip()
                    if mod_name:
                        updated_items.append(
                            f"{mod_download_html_link(mod_name)} (v{escape_html(from_ver)} -&gt; v{escape_html(to_ver)})"
                        )

        renamed_items: List[str] = []
        if isinstance(renamed_mods, list):
            for entry in renamed_mods:
                if isinstance(entry, dict):
                    from_mod = str(entry.get("from_mod", "")).strip()
                    to_mod = str(entry.get("to_mod", "")).strip()
                    version = str(entry.get("version_display", entry.get("version", ""))).strip()
                    if from_mod and to_mod:
                        renamed_items.append(
                            f"{mod_download_html_link(to_mod)} (renamed from {escape_html(from_mod)}, v{escape_html(version)})"
                        )

        removed_items: List[str] = []
        if isinstance(removed_mods, list):
            for entry in removed_mods:
                if isinstance(entry, dict):
                    mod_name = str(entry.get("mod", "")).strip()
                    from_ver = str(entry.get("from_display", entry.get("from", ""))).strip()
                    if mod_name:
                        removed_items.append(f"{escape_html(mod_name)} (was v{escape_html(from_ver)})")

        header = f"GigglePack v{escape_html(release_version)}"
        release_stamp = format_release_stamp(released_at)
        if release_stamp:
            header += f" - {escape_html(release_stamp)}"

        parsed_entries.append({
            "header": header,
            "new_count": int(change_counts.get("new_mods", len(new_items))),
            "updated_count": int(change_counts.get("updated_existing_mods", len(updated_items))),
            "renamed_count": int(change_counts.get("renamed_mods", len(renamed_items))),
            "removed_count": int(change_counts.get("removed_mods", len(removed_items))),
            "new_items": new_items,
            "updated_items": updated_items,
            "renamed_items": renamed_items,
            "removed_items": removed_items,
            "previous_release_version": previous_release_version,
        })

    lines: List[str] = []

    def build_inner_list(items: List[str]) -> str:
        if not items:
            return "<ul><li>None</li></ul>"
        return "<ul>" + "".join(f"<li>{item}</li>" for item in items) + "</ul>"

    detail_bits: List[str] = ["<ul>"]
    for idx, entry in enumerate(parsed_entries):
        header_line = str(entry.get("header", "")).strip() or "GigglePack"
        new_count = int(entry.get("new_count", 0))
        updated_count = int(entry.get("updated_count", 0))
        renamed_count = int(entry.get("renamed_count", 0))
        removed_count = int(entry.get("removed_count", 0))
        new_items = entry.get("new_items", []) if isinstance(entry.get("new_items", []), list) else []
        updated_items = entry.get("updated_items", []) if isinstance(entry.get("updated_items", []), list) else []
        renamed_items = entry.get("renamed_items", []) if isinstance(entry.get("renamed_items", []), list) else []
        removed_items = entry.get("removed_items", []) if isinstance(entry.get("removed_items", []), list) else []

        detail_bits.extend([
            f"<li>{header_line}",
            "<ul>",
            (
                f"<li>Change summary: +{new_count} new, ~{updated_count} updated, "
                f"={renamed_count} renamed, -{removed_count} removed</li>"
            ),
        ])
        previous_release_version = str(entry.get("previous_release_version", "")).strip()
        entry_version_match = re.search(r"v([0-9]+\.[0-9]+\.[0-9]+)", header_line)
        entry_version = entry_version_match.group(1) if entry_version_match else ""
        if previous_release_version and previous_release_version != entry_version:
            detail_bits.append(f"<li>Previous GigglePack version: v{escape_html(previous_release_version)}</li>")

        detail_bits.append(f"<li>New mods:{build_inner_list(new_items)}</li>")
        detail_bits.append(f"<li>Updated existing mods:{build_inner_list(updated_items)}</li>")
        detail_bits.append(f"<li>Renamed mods:{build_inner_list(renamed_items)}</li>")
        detail_bits.append(f"<li>Removed mods:{build_inner_list(removed_items)}</li>")
        detail_bits.extend(["</ul>", "</li>"])

    detail_bits.append("</ul>")
    changelog_html = "".join(detail_bits)

    lines.extend([
        "<details markdown=\"1\"><summary><i>Changelog (latest 3 releases)</i></summary>",
        changelog_html,
        "</details>",
    ])

    return lines


def load_category_descriptions(log: Optional[Logger] = None) -> Dict[str, str]:
    """Parse TEMPLATE-CategoryDescriptions.md into a dict keyed by section name."""
    result: Dict[str, str] = {}
    if not os.path.isfile(CATEGORY_DESCRIPTIONS_PATH):
        if log:
            log.warn(f"Category descriptions file not found: {CATEGORY_DESCRIPTIONS_PATH}")
        return result
    try:
        with open(CATEGORY_DESCRIPTIONS_PATH, "r", encoding="utf-8") as f:
            content = f.read()
        # Split on [SECTION] markers
        parts = re.split(r"^\[([^\]]+)\]", content, flags=re.MULTILINE)
        # parts = [preamble, key1, value1, key2, value2, ...]
        it = iter(parts[1:])  # skip preamble
        for key in it:
            value = next(it, "").strip()
            if key.strip() and key.strip() != "category_descriptions":
                result[key.strip()] = value
    except Exception as ex:
        if log:
            log.warn(f"Could not parse category descriptions: {ex}")
    return result


def generate_main_readme(dry_run: bool, log: Logger) -> None:
    log.info("Step 7: Generate main README.md")

    if not os.path.exists(MAIN_TEMPLATE_PATH):
        log.error(f"Missing required template: {MAIN_TEMPLATE_PATH}")
        raise FileNotFoundError(MAIN_TEMPLATE_PATH)

    with open(MAIN_TEMPLATE_PATH, "r", encoding="utf-8") as f:
        main_template = f.read()

    category_template = DEFAULT_MAIN_README_CATEGORY_TEMPLATE
    if os.path.isfile(MAIN_MOD_CATEGORY_TEMPLATE_PATH):
        try:
            with open(MAIN_MOD_CATEGORY_TEMPLATE_PATH, "r", encoding="utf-8") as f:
                category_template = f.read().strip("\n") or DEFAULT_MAIN_README_CATEGORY_TEMPLATE
        except Exception as ex:
            log.warn(f"Could not read main mod-category template {MAIN_MOD_CATEGORY_TEMPLATE_PATH}: {ex}")

    mod_entry_template = DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE
    if os.path.isfile(MAIN_MOD_ENTRY_TEMPLATE_PATH):
        try:
            with open(MAIN_MOD_ENTRY_TEMPLATE_PATH, "r", encoding="utf-8") as f:
                mod_entry_template = f.read().strip("\n") or DEFAULT_MAIN_README_MOD_ENTRY_TEMPLATE
        except Exception as ex:
            log.warn(f"Could not read main mod-entry template {MAIN_MOD_ENTRY_TEMPLATE_PATH}: {ex}")

    cat_desc = load_category_descriptions(log)

    mod_type_lines = load_mainreadme_mod_type_lines(log)
    _, compat_rows = load_compat_csv()
    compat_map: Dict[str, Dict[str, str]] = {row.get("MOD_NAME", "").strip(): dict(row) for row in compat_rows if row.get("MOD_NAME", "").strip()}

    now_str = dt.datetime.now().strftime("%B %d, %Y, %I:%M %p EST").lstrip("0").replace(" 0", " ")
    main_content = main_template.replace("{{LAST_UPDATED}}", now_str)
    main_content = main_content.replace(ABOUTME_MAIN_GUIDE_PLACEHOLDER, load_aboutme_main_guide_body(log))
    main_content = main_content.replace(MODTYPE_GUIDE_PLACEHOLDER, load_modtype_guide_body(log))
    main_content = main_content.replace(LANGUAGE_MAIN_PLACEHOLDER, load_language_main_body(log))
    main_content = main_content.replace(MODGUIDE_MAIN_PLACEHOLDER, load_modguide_main_body(log))
    main_content = main_content.replace(ASKFORHELP_MAIN_PLACEHOLDER, load_askforhelp_main_body(log))
    main_content = main_content.replace(SUPPORT_MAIN_PLACEHOLDER, load_support_main_body(log))
    main_content = main_content.replace(INSTALL_GUIDE_PLACEHOLDER, load_install_guide_body(log))
    main_content = main_content.replace(REMOVAL_GUIDE_PLACEHOLDER, load_removal_guide_body(log))
    main_content = main_content.replace(UPDATE_GUIDE_PLACEHOLDER, load_update_guide_body(log))
    main_content = main_content.replace(BACKUP_GUIDE_PLACEHOLDER, load_backup_guide_body(log))

    all_mods = collect_publishready_folders()
    backpackplus_mods = [f for f in all_mods if f.startswith("AGF-BackpackPlus-")]
    hudplus_mods = [f for f in all_mods if f.startswith("AGF-HUDPlus-")]
    noeac_mods = [f for f in all_mods if f.startswith("AGF-NoEAC-")]
    modders_mods = [f for f in all_mods if f.startswith("AGF-4Modders-")]
    vp_mods = [f for f in all_mods if f.startswith("AGF-VP-")]
    special_mods = [f for f in all_mods if f.startswith("zzzAGF-Special")]
    requested_mods = [f for f in all_mods if f.startswith("AGF-Requested-") or f.startswith("zzzAGF-Requested-")]

    updates_in_progress = "Updates are in progress."

    def category_download_line(mods: List[str], label: str, zip_name: str) -> str:
        if mods:
            return f"[**⬇️ {label}**]({zip_download_link(zip_name)})"
        return updates_in_progress

    md: List[str] = []
    giggle_release_state = load_gigglepack_release_state()
    giggle_release_version = str(giggle_release_state.get("gigglepack_version", "")).strip()
    giggle_download_label = category_download_line(all_mods, "DOWNLOAD ALL AGF MODS", "00_GigglePack_All.zip")
    if giggle_release_version and giggle_download_label != updates_in_progress:
        giggle_download_label += f" **(GigglePack v{giggle_release_version})**"

    giggle_release_lines = build_gigglepack_readme_release_lines(giggle_release_state)

    giggle_template = DEFAULT_MAIN_README_CATEGORY_TEMPLATE
    if os.path.isfile(GIGGLE_PACK_TEMPLATE_PATH):
        try:
            with open(GIGGLE_PACK_TEMPLATE_PATH, "r", encoding="utf-8") as f:
                giggle_template = f.read().strip("\n") or DEFAULT_MAIN_README_CATEGORY_TEMPLATE
        except Exception as ex:
            log.warn(f"Could not read GigglePack template {GIGGLE_PACK_TEMPLATE_PATH}: {ex}")

    giggle_description = cat_desc.get("GIGGLE PACK", "All AGF mods in one convenient download.")
    giggle_changelog = "\n".join(giggle_release_lines) if giggle_release_lines else ""

    giggle_block = re.sub(r"<!--.*?-->", "", giggle_template, flags=re.DOTALL)
    giggle_block = giggle_block.replace("{{CATEGORY_TITLE}}", "A. GIGGLE PACK")
    giggle_block = giggle_block.replace("{{CATEGORY_DOWNLOAD_LINE}}", giggle_download_label)
    giggle_block = giggle_block.replace("{{CATEGORY_DESCRIPTION}}", giggle_description)
    giggle_block = giggle_block.replace("{{GIGGLE_CHANGELOG}}", giggle_changelog)
    giggle_block = re.sub(r"\n{3,}", "\n\n", giggle_block).strip("\n")
    md.extend(giggle_block.splitlines())
    md.append("")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "B. HUD PLUS MODS",
            category_download_line(hudplus_mods, "Download All HUD Plus Mods", "00_HUDPlus_All.zip"),
            cat_desc.get("HUDPLUS", "Quality-of-life HUD enhancements and visual tweaks."),
        )
    )
    md.append("")
    if hudplus_mods:
        for mod in hudplus_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
    else:
        md.append("*Updates are in progress.*")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "C. BACKPACK PLUS MODS",
            category_download_line(
                backpackplus_mods,
                "Download All Backpack Plus Mods",
                "00_BackpackPlus_All.zip",
            ),
            cat_desc.get("BACKPACKPLUS", "Increases backpack size. Choose the slot count that fits your needs."),
        )
    )
    md.append("")

    preferred_last = "AGF-BackpackPlus-119Slots"
    backpack_sorted = sorted(backpackplus_mods, key=lambda x: (get_base_mod_name(x) == preferred_last, x))
    if backpack_sorted:
        for mod in backpack_sorted:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
    else:
        md.append("*Updates are in progress.*")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "D. SPECIAL MOD PATCHES",
            updates_in_progress if not special_mods else "",
            cat_desc.get("SPECIAL", "Patches to support other mods and modlets alongside AGF mods."),
        )
    )
    md.append("")
    if special_mods:
        for mod in special_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
    else:
        md.append("*Updates are in progress.*")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "E. VANILLA PLUS MODS",
            category_download_line(vp_mods, "Download All VP Mods", "00_VP_All.zip"),
            cat_desc.get("VP", "Gameplay tweaks and new features that expand on the base game."),
        )
    )
    md.append("")
    if vp_mods:
        for mod in vp_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
    else:
        md.append("*Updates are in progress.*")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "F. NO EAC MODS",
            category_download_line(noeac_mods, "Download All NoEAC Mods", "00_NoEAC_All.zip"),
            cat_desc.get("NOEAC", "Game enhancements that require a DLL. EAC must be off."),
        )
    )
    md.append("")
    if noeac_mods:
        for mod in noeac_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
    else:
        md.append("*Updates are in progress.*")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "G. 4MODDERS MODS",
            category_download_line(modders_mods, "Download All 4Modders Mods", "00_4Modders_All.zip"),
            cat_desc.get("4MODDERS", "Modder resources and niche mods. Read each description before installing."),
        )
    )
    md.append("")
    if modders_mods:
        for mod in modders_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
    else:
        md.append("*Updates are in progress.*")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "H. REQUESTED MODS",
            category_download_line(requested_mods, "Download All Requested Mods", "00_Requested_All.zip"),
            cat_desc.get("REQUESTED", "Community-requested modifications and standalone features."),
        )
    )
    md.append("")
    if requested_mods:
        for mod in requested_mods:
            md.append(build_mod_entry(mod, mod_entry_template, compat_map, mod_type_lines))
    else:
        md.append("*Updates are in progress.*")

    md.extend(
        render_main_readme_category_block(
            category_template,
            "I. AGF-7d2d-v2.6-GigglePack-Final",
            f"[**⬇️ Download AGF 7D2D v2.6 Final**]({zip_download_link(LEGACY_FINAL_GIGGLEPACK_ZIP)})",
            cat_desc.get(
                LEGACY_FINAL_CATEGORY_KEY,
                "Everything AGF made for 7d2d 2.6 in a frozen final snapshot.",
            ),
        )
    )

    modlist_str = "\n".join(md)
    main_content = re.sub(
        r"<!-- MOD_LIST_START -->(.*?)<!-- MOD_LIST_END -->",
        f"<!-- MOD_LIST_START -->\n{modlist_str}\n<!-- MOD_LIST_END -->",
        main_content,
        flags=re.DOTALL,
    )

    main_content = main_content.replace("<li>></li>", "")

    lines = main_content.splitlines()
    cleaned: List[str] = []
    for i, line in enumerate(lines):
        if line.strip() == "---" and i > 0 and lines[i - 1].strip() != "":
            cleaned.append("")
        cleaned.append(line)

    final_content = "\n".join(cleaned)

    if dry_run:
        log.info(f"[DRYRUN] Would write main README: {MAIN_README_PATH}")
        return

    atomic_write_text(MAIN_README_PATH, final_content, encoding="utf-8")


# =============================================================
# VALIDATION
# =============================================================
def run_self_tests(log: Logger) -> bool:
    """Run lightweight in-process reliability tests against temp fixtures."""
    log.info("Running self-test harness")
    passed = 0
    failed = 0

    def record(test_name: str, condition: bool) -> None:
        nonlocal passed, failed
        if condition:
            passed += 1
            log.info(f"self-test pass: {test_name}")
        else:
            failed += 1
            log.error(f"self-test fail: {test_name}")

    def write_modinfo(mod_dir: str, name: str, version: str) -> None:
        xml = (
            "<xml>\n"
            f"  <Name value=\"{name}\"/>\n"
            f"  <Version value=\"{version}\"/>\n"
            "</xml>\n"
        )
        atomic_write_text(os.path.join(mod_dir, "ModInfo.xml"), xml, encoding="utf-8")

    with tempfile.TemporaryDirectory() as temp_root:
        mods_dir = os.path.join(temp_root, "mods")
        os.makedirs(mods_dir, exist_ok=True)

        agf_dir = os.path.join(mods_dir, "AGF-VP-FilterTest-v1.0.0")
        ext_dir = os.path.join(mods_dir, "SomeExternalMod-v9.9.9")
        os.makedirs(agf_dir, exist_ok=True)
        os.makedirs(ext_dir, exist_ok=True)
        write_modinfo(agf_dir, "AGF-VP-FilterTest", "1.0.0")

        filtered = scan_mod_folders(mods_dir)
        record("scan_mod_folders filters non-AGF mods", "AGF-VP-FilterTest-v1.0.0" in filtered and "SomeExternalMod-v9.9.9" not in filtered)

        name_only_dir = os.path.join(mods_dir, "AGF-VP-NameOnly-v1.0.0")
        ver_only_dir = os.path.join(mods_dir, "AGF-VP-VersionOnly-v1.0.0")
        combined_dir = os.path.join(mods_dir, "AGF-VP-CombinedOld-v1.0.0")
        os.makedirs(name_only_dir, exist_ok=True)
        os.makedirs(ver_only_dir, exist_ok=True)
        os.makedirs(combined_dir, exist_ok=True)
        write_modinfo(name_only_dir, "AGF-VP-NameOnlyRenamed", "1.0.0")
        write_modinfo(ver_only_dir, "AGF-VP-VersionOnly", "1.0.1")
        write_modinfo(combined_dir, "AGF-VP-CombinedNew", "1.0.1")

        planned = plan_mod_folder_renames((mods_dir,), log)
        planned_names = {(old, new) for old, new, _ in planned}
        record(
            "name-only drift is skipped without version bump",
            ("AGF-VP-NameOnly-v1.0.0", "AGF-VP-NameOnlyRenamed-v1.0.0") not in planned_names,
        )
        record(
            "version-only drift is planned",
            ("AGF-VP-VersionOnly-v1.0.0", "AGF-VP-VersionOnly-v1.0.1") in planned_names,
        )
        record(
            "combined drift is planned",
            ("AGF-VP-CombinedOld-v1.0.0", "AGF-VP-CombinedNew-v1.0.1") in planned_names,
        )

    log.info(f"self-test summary: passed={passed}, failed={failed}")
    return failed == 0


def validate_required_paths(strict: bool, log: Logger, mode: str) -> bool:
    ok = True
    required_dirs = []
    required_files = []

    if mode == "sync-work":
        required_dirs = [STAGING, GAME_MODS]
    elif mode == "self-test":
        required_dirs = []
        required_files = []
    elif mode == "update":
        required_dirs = [STAGING, IN_PROGRESS, GAME_MODS]
        required_files = [MOD_README_TEMPLATE]
    elif mode == "prep-work":
        required_dirs = [STAGING]
        required_files = [MOD_README_TEMPLATE]
    elif mode == "promote":
        required_dirs = [STAGING, PUBLISH_READY]
        required_files = [MOD_README_TEMPLATE]
    elif mode == "package":
        required_dirs = [PUBLISH_READY]
        required_files = [MOD_README_TEMPLATE, MAIN_TEMPLATE_PATH]
    elif mode == "migrate-readmes-once":
        required_dirs = [STAGING, IN_PROGRESS]
        required_files = [MOD_README_TEMPLATE]
    else:
        required_dirs = [STAGING, PUBLISH_READY, IN_PROGRESS, GAME_MODS]
        required_files = [MOD_README_TEMPLATE, MAIN_TEMPLATE_PATH]

    for path in required_dirs:
        if not os.path.isdir(path):
            message = f"Required directory not found: {path}"
            if strict:
                log.error(message)
                ok = False
            else:
                log.warn(message)

    for path in required_files:
        if not os.path.isfile(path):
            message = f"Required template not found: {path}"
            if strict:
                log.error(message)
                ok = False
            else:
                log.warn(message)

    return ok


# =============================================================
# MAIN
# =============================================================
def run_pipeline(args: argparse.Namespace) -> int:
    global FAIL_FAST_ENABLED, CURRENT_TRANSACTION
    FAIL_FAST_ENABLED = args.fail_fast
    CURRENT_TRANSACTION = create_transaction(args.transaction_rollback and not args.dry_run)
    run_success = False

    log = Logger(verbose=args.verbose, dry_run=args.dry_run)
    log.info("Starting SCRIPT-Main automation pipeline")
    log.info(f"Selected mode: {args.mode}")
    log.info("Scope policy: only AGF-/zzzAGF-prefixed mods are managed in workspace and game folders")
    if args.dry_run:
        log.info("Dry-run mode enabled: no file system changes will be written")

    try:
        if args.mode == "self-test":
            ok = run_self_tests(log)
            log_path = log.write_log_file()
            exit_code = 0 if ok and log.stats.errors == 0 else 1
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, exit_code, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            run_success = exit_code == 0
            return exit_code

        if not validate_required_paths(strict=args.strict, log=log, mode=args.mode):
            log.error("Path validation failed in strict mode")
            log_path = log.write_log_file()
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            return 1

        cleanup_game_quarantine(args.quarantine_retention_days, args.dry_run, log)

        if args.enforce_agf_csv and not validate_agf_rows_in_csv(log):
            log_path = log.write_log_file()
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            return 1

        if args.preflight_write_check and not run_writeability_preflight(args.mode, args.dry_run, log):
            log.error("Preflight writeability check failed")
            log_path = log.write_log_file()
            manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
            if log_path:
                print(f"Log file: {log_path}")
            if manifest_path:
                print(f"Run manifest: {manifest_path}")
            return 1

        if args.mode == "sync-work":
            enforce_staging_major_policy(args.dry_run, log)
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            sync_staging_and_game(args.dry_run, log)
        elif args.mode == "reset-active-to-draft":
            reset_activebuild_to_draft(args.dry_run, log)
        elif args.mode == "update":
            log.info(
                "Mode update: sync Draft<->Game, ingest Draft->ActiveBuild, "
                "sync ActiveBuild<->Game, normalize names/readmes — no promote or package."
            )

            pulled_mods_for_pushback: List[Tuple[str, str]] = []

            # 0.25) Pull newer game updates into Draft for overlapping AGF mods.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_game_and_draft(args.dry_run, log))

            # 0.4) Normalize Draft names first so version-bumped game pulls are promoted using updated folder names.
            pre_promote_draft_rename_plan = plan_mod_folder_renames((IN_PROGRESS,), log)
            if not ensure_notepadpp_closed_for_version_bumps(pre_promote_draft_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pre_promote_draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            update_mod_loaded_references_for_renames(pre_promote_draft_renames, args.dry_run, log)

            # 0.5) Ensure ActiveBuild includes latest Draft copies before game sync.
            sync_draft_to_staging_latest(args.dry_run, log)

            # 0.75) Enforce lane policy after ingest so v0.x stays in Draft.
            enforce_staging_major_policy(args.dry_run, log)

            # 0.9) Resolve name/version folder drift first, then update mod_loaded refs before any game push.
            pre_sync_rename_plan = plan_mod_folder_renames((STAGING, IN_PROGRESS), log)
            if not ensure_notepadpp_closed_for_version_bumps(pre_sync_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pre_sync_staging_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(STAGING,))
            pre_sync_draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            pre_sync_renames = pre_sync_staging_renames + pre_sync_draft_renames
            update_mod_loaded_references_for_renames(pre_sync_renames, args.dry_run, log)

            # 1) Keep ActiveBuild and Game synchronized.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_staging_and_game(args.dry_run, log))

            # 2) Apply naming/readme updates in ActiveBuild.
            staging_renames = prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(STAGING, args.dry_run, log)
            cleanup_older_versions_in_dir(STAGING, args.dry_run, log)

            # 3) Normalize names/metadata in Draft as well.
            post_sync_draft_rename_plan = plan_mod_folder_renames((IN_PROGRESS,), log)
            if not ensure_notepadpp_closed_for_version_bumps(post_sync_draft_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            cleanup_older_versions_in_dir(IN_PROGRESS, args.dry_run, log)
            all_renames = pre_promote_draft_renames + pre_sync_renames + staging_renames + draft_renames
            post_sync_renames = staging_renames + draft_renames
            csv_rows = normalize_compat_csv(
                all_renames,
                args.dry_run,
                log,
                mod_dirs=(STAGING, IN_PROGRESS),
                prune_to_mods_now=True,
            )
            normalize_quote_files(csv_rows, all_renames, args.dry_run, log)
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(STAGING,))
            generate_mod_images(args.dry_run, log)
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            push_staging_mods_to_game(
                set(),
                args.dry_run,
                log,
                reason="mod image refresh",
            )
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            update_mod_loaded_references_for_renames(post_sync_renames, args.dry_run, log)
            update_mod_loaded_references_for_renames(all_renames, args.dry_run, log)

            pulled_mods_for_pushback = remap_pulled_mods_after_renames(
                pulled_mods_for_pushback,
                all_renames,
                log,
            )

            # 4) Push back only pulled mods that qualify.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            push_back_pulled_mods(pulled_mods_for_pushback, args.dry_run, log)

            # 5) Recompute pending changes against last published GigglePack state.
            update_gigglepack_pending_changes(
                args.dry_run,
                log,
                args.pending_updates_consolidation_label,
            )

        elif args.mode == "prep-work":
            prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
        elif args.mode == "migrate-readmes-once":
            migrate_readmes_one_time(args.dry_run, log, target_dirs=(STAGING, IN_PROGRESS))
        elif args.mode == "promote":
            enforce_staging_major_policy(args.dry_run, log)
            prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            generate_mod_images(args.dry_run, log)
            promote_staging_to_publish_ready(args.dry_run, log)
        elif args.mode == "package":
            prep_names_and_readmes_for_dirs((PUBLISH_READY,), args.dry_run, log)
            create_all_zips(args.dry_run, args.workers, log)
            generate_gigglepack_release_artifacts(args.dry_run, log)
            generate_thumbnails(args.dry_run, log)
            generate_main_readme(args.dry_run, log)
        else:
            log.info(
                "Mode full: sync Game<->Draft for tracked draft mods, ingest Draft->ActiveBuild, "
                "sync ActiveBuild<->Game, then apply targeted pushback for pulled mods, then finalize/promote/package."
            )

            pulled_mods_for_pushback: List[Tuple[str, str]] = []

            # 0.25) Pull newer game updates into Draft for overlapping AGF mods.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_game_and_draft(args.dry_run, log))

            # 0.4) Normalize Draft names first so version-bumped game pulls are promoted using updated folder names.
            pre_promote_draft_rename_plan = plan_mod_folder_renames((IN_PROGRESS,), log)
            if not ensure_notepadpp_closed_for_version_bumps(pre_promote_draft_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pre_promote_draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            update_mod_loaded_references_for_renames(pre_promote_draft_renames, args.dry_run, log)

            # 0.5) Ensure ActiveBuild includes latest Draft copies before game sync.
            sync_draft_to_staging_latest(args.dry_run, log)

            # 0.75) Enforce lane policy after ingest so v0.x stays in Draft.
            enforce_staging_major_policy(args.dry_run, log)

            # 0.9) Resolve name/version folder drift first, then update mod_loaded refs before any game push.
            pre_sync_rename_plan = plan_mod_folder_renames((STAGING, IN_PROGRESS), log)
            if not ensure_notepadpp_closed_for_version_bumps(pre_sync_rename_plan, args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pre_sync_staging_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(STAGING,))
            pre_sync_draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            pre_sync_renames = pre_sync_staging_renames + pre_sync_draft_renames
            update_mod_loaded_references_for_renames(pre_sync_renames, args.dry_run, log)

            # 1) Keep ActiveBuild and Game synchronized first.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            pulled_mods_for_pushback.extend(sync_staging_and_game(args.dry_run, log))

            # 2) Apply naming/readme updates in ActiveBuild.
            staging_renames = prep_names_and_readmes_for_dirs((STAGING,), args.dry_run, log)
            update_mod_loaded_references_for_renames(staging_renames, args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(STAGING, args.dry_run, log)
            cleanup_older_versions_in_dir(STAGING, args.dry_run, log)
            generate_mod_images(args.dry_run, log)
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            push_staging_mods_to_game(
                set(),
                args.dry_run,
                log,
                reason="mod image refresh",
            )

            # 3) Promote finalized ActiveBuild content to ReleaseSource.
            promote_staging_to_publish_ready(args.dry_run, log)

            # 3.5) Remove stale legacy folder names replaced by AGF-4Modders naming.
            cleanup_release_legacy_4modders_renames(args.dry_run, log)
            cleanup_legacy_4modders_renames_in_dir(PUBLISH_READY, args.dry_run, log, staging_dir=STAGING)
            cleanup_older_versions_in_dir(PUBLISH_READY, args.dry_run, log)

            # 4) Ensure ReleaseSource + Draft metadata/quotes/readmes are normalized before packaging.
            release_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(PUBLISH_READY,))
            draft_renames = rename_mod_folders_to_modinfo(args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            cleanup_older_versions_in_dir(IN_PROGRESS, args.dry_run, log)
            all_renames = pre_promote_draft_renames + release_renames + draft_renames
            csv_rows = normalize_compat_csv(
                all_renames,
                args.dry_run,
                log,
                mod_dirs=(PUBLISH_READY, IN_PROGRESS),
                prune_to_mods_now=True,
            )
            normalize_quote_files(csv_rows, all_renames, args.dry_run, log)
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(PUBLISH_READY,))
            generate_mod_readmes(csv_rows, args.dry_run, log, mod_dirs=(IN_PROGRESS,))
            update_mod_loaded_references_for_renames(all_renames, args.dry_run, log)

            pulled_mods_for_pushback = remap_pulled_mods_after_renames(
                pulled_mods_for_pushback,
                all_renames,
                log,
            )

            # 5) After names/metadata are finalized, push back only pulled mods that qualify.
            if not ensure_notepadpp_closed_for_game_sync(args.dry_run, log):
                log_path = log.write_log_file()
                manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
                if log_path:
                    print(f"Log file: {log_path}")
                if manifest_path:
                    print(f"Run manifest: {manifest_path}")
                return 1
            push_back_pulled_mods(pulled_mods_for_pushback, args.dry_run, log)

            # 6) Package and regenerate main README.
            create_all_zips(args.dry_run, args.workers, log)
            publish_gigglepack_action = resolve_publish_gigglepack_action(
                args.publish_gigglepack_action,
                args.dry_run,
                log,
            )
            if publish_gigglepack_action == "finalize":
                generate_gigglepack_release_artifacts(args.dry_run, log)
            elif publish_gigglepack_action == "append-latest":
                generate_gigglepack_release_artifacts(
                    args.dry_run,
                    log,
                    append_to_latest_release=True,
                )
            else:
                log.info(
                    "GigglePack finalization skipped for this publish run; "
                    "updating pending changes queue only."
                )
                update_gigglepack_pending_changes(
                    args.dry_run,
                    log,
                    args.pending_updates_consolidation_label,
                )
                log.action_needed(
                    "GigglePack release remains queued. Re-run publish with --publish-gigglepack-action finalize when ready."
                )
            generate_thumbnails(args.dry_run, log)
            generate_main_readme(args.dry_run, log)

        # Mark successful completion before the finally block so transactional
        # rollback is not triggered on successful runs.
        run_success = log.stats.errors == 0
    except Exception as ex:
        log.error(f"Pipeline aborted due to unhandled exception: {ex}")
        log_path = log.write_log_file()
        manifest_path = write_run_manifest(log, args.mode, args.dry_run, 1, log_path)
        if log_path:
            print(f"Log file: {log_path}")
        if manifest_path:
            print(f"Run manifest: {manifest_path}")
        return 1
    finally:
        finalize_transaction(success=run_success, log=log)

    print("\n=== RUN SUMMARY ===")
    for key, value in log.stats.__dict__.items():
        print(f"{key}: {value}")

    action_needed = log.get_action_needed_lines()
    if action_needed:
        print("\n=== ACTION NEEDED ===")
        for line in action_needed:
            print(f"- {line}")

    print("\n=== MOD CHANGES ===")
    for line in log.get_mod_change_summary_lines():
        print(line)

    log_path = log.write_log_file()
    exit_code = 0 if log.stats.errors == 0 else 1
    manifest_path = write_run_manifest(log, args.mode, args.dry_run, exit_code, log_path)
    if log_path:
        print(f"Log file: {log_path}")
    if manifest_path:
        print(f"Run manifest: {manifest_path}")

    run_success = exit_code == 0
    return exit_code


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Main AGF mod automation pipeline")
    parser.add_argument(
        "--mode",
        choices=[
            "full",
            "update",
            "sync-work",
            "reset-active-to-draft",
            "prep-work",
            "promote",
            "package",
            "migrate-readmes-once",
            "self-test",
        ],
        default="full",
        help="Workflow mode: full pipeline, update/sync, one-time readme migration, promote, or package output",
    )
    parser.add_argument("--dry-run", action="store_true", help="Preview actions without writing changes")
    parser.add_argument("--verbose", action="store_true", help="Show INFO logs in console")
    parser.add_argument("--strict", action="store_true", help="Fail when required directories/templates are missing")
    parser.add_argument(
        "--fail-fast",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Abort immediately on critical filesystem operation failures",
    )
    parser.add_argument(
        "--transaction-rollback",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Attempt to rollback filesystem operations after an error",
    )
    parser.add_argument(
        "--quarantine-retention-days",
        type=int,
        default=7,
        help="Delete game-removal quarantine entries older than this many days",
    )
    parser.add_argument(
        "--enforce-agf-csv",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Fail if Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv contains non-AGF rows",
    )
    parser.add_argument(
        "--preflight-write-check",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Verify write access to key folders before running",
    )
    parser.add_argument(
        "--workers",
        type=int,
        default=max(2, min(8, os.cpu_count() or 2)),
        help="Worker count for parallel mod zip creation",
    )
    parser.add_argument(
        "--pending-updates-consolidation-label",
        default="",
        help=(
            "Optional one-time label to collapse pending updated_mods into a single bulk entry "
            "(example: AGF-Bulk-ReadMe-Format-Update)"
        ),
    )
    parser.add_argument(
        "--publish-gigglepack-action",
        choices=["ask", "finalize", "append-latest", "queue"],
        default="finalize",
        help=(
            "Only used by mode=full publish flow: ask before GigglePack finalize, "
            "always finalize, append to latest finalized release notes, or queue pending changes only"
        ),
    )
    return parser


if __name__ == "__main__":
    arg_parser = build_arg_parser()
    cli_args = arg_parser.parse_args()
    if not acquire_run_lock():
        print(f"Another pipeline run is already active (lock file exists: {RUN_LOCK_PATH}).")
        sys.exit(1)
    try:
        sys.exit(run_pipeline(cli_args))
    finally:
        release_run_lock()
