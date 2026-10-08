"""
Create a new Draft mod folder.

You type the token, the scope, and whether it loads early, normal, or late.
The script fills AGF, V3, the folder, the ModInfo name, and the display name.
"""
import csv
import argparse
import os
import re


WORKSPACE_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))


def resolve_lane_path(preferred: str, legacy: str) -> str:
    if os.path.isdir(preferred):
        return preferred
    if os.path.isdir(legacy):
        return legacy
    return preferred


INPROGRESS_DIR = resolve_lane_path(
    os.path.join(WORKSPACE_ROOT, "01_Draft"),
    os.path.join(WORKSPACE_ROOT, "_Mods2.In-Progress"),
)
README_SYSTEM_DIR = os.path.join(WORKSPACE_ROOT, "05_ReleaseData", "ReadmeSystem")
COMPAT_CSV_PATH = os.path.join(README_SYSTEM_DIR, "HELPER_ModCompatibility.csv")
QUOTES_DIR = os.path.join(README_SYSTEM_DIR, "Quotes")
TEMPLATE_MODINFO = '''<?xml version="1.0" encoding="UTF-8" ?>\n<xml>\n    <Name value=\"{mod_name}\"/>\n    <DisplayName value=\"{display_name}\"/>\n    <Version value=\"{version}\"/>\n    <Description value=\"\"/>\n    <Author value=\"AuroraGiggleFairy (AGF)\"/>\n    <Website value=\"https://auroragigglefairy.github.io/\"/>\n</xml>\n'''


VERSION_PATTERN = re.compile(r"^\d+\.\d+\.\d+$")
TOKEN_PATTERN = re.compile(r"^[A-Za-z0-9]+$")
GAME_LINE = "V3"
SCOPES = ("SERVER", "CLIENT", "BOTH", "ADMIN", "COMPAT")
LOAD_PREFIX = {"early": "0", "normal": "", "late": "zzz"}


def fail(message: str) -> None:
    print(message)
    raise SystemExit(1)


def spaced_token(token: str) -> str:
    return re.sub(r"(?<=[a-z])(?=[A-Z])", " ", token).strip()


def normalize_scope(value: str) -> str:
    scope = (value or "").strip().upper()
    if scope in SCOPES:
        return scope
    return ""


def normalize_load(value: str) -> str:
    load = (value or "").strip().lower()
    if load in LOAD_PREFIX:
        return load
    return ""


def load_prefix(scope: str, load: str) -> str:
    if scope == "COMPAT":
        return "zzzzz"
    return LOAD_PREFIX[load]


def build_mod_identity(token: str, scope: str, load: str, version: str) -> tuple:
    prefix = load_prefix(scope, load)
    folder_base = f"{prefix}AGF-{GAME_LINE}-{scope}-{token}"
    folder_name = f"{folder_base}-v{version}"
    pretty = spaced_token(token)
    if scope == "COMPAT":
        modinfo_name = f"AGF-COMPAT-{token}"
        display_name = f"AGF COMPAT {pretty}"
    else:
        modinfo_name = f"AGF-{token}"
        display_name = f"AGF {pretty}"
    return folder_base, folder_name, modinfo_name, display_name


def build_initial_readme_txt(mod_name: str, version: str) -> str:
    display_name = mod_name.upper()
    sep = "=" * 72
    sub_sep = "-" * 72
    return f"""{sep}
{display_name:^72}
{sep}

Add mod description here.

NOTE: AGF Mod Guide and Changelog are further below.


{sub_sep}
MOD SCOPE
{sub_sep}

  - Mod Version: {version}
  - 7d2d Version: MISSINGDATA
  - Website: https://auroragigglefairy.github.io/
  - Mod Type: MISSINGDATA (Server-Side/EAC-Friendly, VPS, or Client-Side/NoEAC)
  - Safe to install on existing game: MISSINGDATA
  - Safe to remove from existing game: MISSINGDATA
  - Dependencies: None, works standalone.


{sub_sep}
FEATURES
{sub_sep}

  - Add feature descriptions here.


{sub_sep}
OTHER DETAILS
{sub_sep}

  - Add other details here.



{sep}
                         AGF MOD GUIDE                         
{sep}

{sub_sep}
A. Install Mods
{sub_sep}

  1. Close the game.
  2. In Steam, right-click 7 Days to Die -> Manage -> Browse local
     files, then open Mods.
  3. Extract the zip into the Mods folder. Make sure it ends up as
     Mods/<ModName>/ModInfo.xml.
  4. Restart the game.


{sub_sep}
B. Ask AuroraGiggleFairy for Help
{sub_sep}

  1. Join AGF's Discord: https://discord.gg/Vm5eyW6N4r.
    - AGF checks website messages often, but Discord is the fastest and
      best way to get help.
  2. Find #ask-for-help-here under the NEED HELP? section.
    - All questions are welcome, whether you are new or experienced.
    - This includes mod conflicts, features not working as expected,
      server or admin issues, translation errors, and other mod-related
      problems.
  3. Post your help request in #ask-for-help-here:
    - Share a brief message about what is happening.
    - Attach your latest log file.
      - Enter the game, then press F1 to open the console.
      - Click Open logs folder in the top-right.
      - The correct log file should already be selected. Drag and drop
        it into #ask-for-help-here.
    - A screenshot can also help.
      - Use PrtSc (Print Screen) or your system screenshot tool, then
        paste the image into Discord chat.
    - If preferred, DMs are open and you are welcome to message AGF
      directly.


{sub_sep}
C. Backups
{sub_sep}

  - To Create:
    - Open %appdata% -> Roaming -> 7DaysToDie -> Saves, then open your
      World Name folder (for example, Navezgane).
    - Copy your Game Name folder (for example, MyGame) to a safe place.
  - To Restore:
    - Copy that saved Game Name folder back into the same World Name
      folder in Saves.
    - Replace the current folder if asked.


{sub_sep}
D. Update Mods
{sub_sep}

  1. Close the game.
  2. Make a backup first (see section C).
  3. Install the new version in Mods.
    - If asked, allow overwrite or replace.
  4. If both old and new folders are there, keep the newer one and
     delete the older one.
  5. Start the game and confirm your save loads.


{sub_sep}
E. Remove Mods
{sub_sep}

  - Warning: Removing a mod from an active save can destroy your saved
    game. Back up first.
  - Never delete 0_TFP_Harmony; it comes with the game.
  1. Close the game.
  2. In Mods, delete each mod folder you are removing, except
     0_TFP_Harmony.


{sub_sep}
F. The 0_TFP_Harmony Mod (Do Not Remove)
{sub_sep}

  - Never delete 0_TFP_Harmony; it comes with the game.
  - If it is missing, restore it by verifying game files in Steam:
    1. In Steam, right-click 7 Days to Die.
    2. Select Properties.
    3. Select Installed Files.
    4. Click Verify integrity of game files and wait for completion.


{sub_sep}
G. EAC
{sub_sep}

  - EAC stands for Easy Anti-Cheat and helps protect multiplayer
    sessions from cheating.
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
    4. Choose Ask when starting game or Launch game without EAC.
    
  - If you run multiplayer with EAC off, use these safety practices:
    - Simplest method: keep your server password private and have people
      ask for it.
    - If you want tighter security on who joins, use the whitelist
      system.
    - Admin tools such as Server Tools have security options.
    - Talk to other server hosts, for example AGF in Discord:
      https://discord.gg/Vm5eyW6N4r


{sub_sep}
H. Support AuroraGiggleFairy
{sub_sep}

  - I have been actively creating and supporting 7 Days to Die mods
    since Alpha 18 (2019), and I genuinely love doing this work.
  - I spend a lot of time fixing complex issues, keeping everything up
    to date, and helping players, modders, and server communities.
  - If my work helps you, here are ways to support me:
    - Help spread my mods by sharing them with others, creating content,
      or sharing my GitHub link: https://auroragigglefairy.github.io/
    - Join my Discord to share feedback, keep up with updates, or
      volunteer as a tester: https://discord.gg/Vm5eyW6N4r
    - Support me on Twitch: https://www.twitch.tv/auroragigglefairy
    - Need hosting? Use my PingPerfect Referral Link:
      https://pingperfect.com/aff.php?aff=1834
    - Support me directly by donating to my PayPal:
      https://www.paypal.com/donate/?hosted_button_id=3B7BCQAZ6KHXC
  - From the bottom of my heart, thank you. <3


{sub_sep}
I. AGF Modding Focus
{sub_sep}

  - I have been modding 7 Days to Die for 7 years.
  - I do my best to prioritize accessibility, user-friendliness, and
    localization where possible.
  - I provide kind, comprehensive support to players, modders, and
    server communities, and I rely on community feedback to keep
    improving my mods.



{sep}
                            CHANGELOG                            
{sep}

v0.0.1
      - Initial creation of this mod.
"""


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Create a new mod folder scaffold in the in-progress lane."
    )
    parser.add_argument(
        "--name",
        help="Mod token only, e.g. FuelBurnPlus or 0SCore",
    )
    parser.add_argument(
        "--scope",
        help="SERVER, CLIENT, BOTH, ADMIN, or COMPAT",
    )
    parser.add_argument(
        "--load",
        default="",
        help="early, normal, or late. Enter for normal. COMPAT always uses five z's.",
    )
    parser.add_argument(
        "--version",
        default="0.0.1",
        help="Initial mod version in SemVer format (default: 0.0.1)",
    )
    parser.add_argument(
        "--non-interactive",
        action="store_true",
        help="Fail instead of prompting when required inputs are missing",
    )
    return parser.parse_args()


def append_compatibility_row(mod_name: str, folder_base: str) -> None:
    if not os.path.exists(COMPAT_CSV_PATH):
        return

    with open(COMPAT_CSV_PATH, "r", encoding="utf-8-sig", newline="") as f:
        reader = csv.reader(f)
        header = next(reader, None)
        rows = list(reader)

    if not header:
        return

    fieldnames = [str(name).lstrip("\ufeff").strip() for name in header]
    if not fieldnames or "MOD_NAME" not in fieldnames:
        return

    mod_name_idx = fieldnames.index("MOD_NAME")
    existing_mods = {
        str(row[mod_name_idx]).strip()
        for row in rows
        if len(row) > mod_name_idx
    }
    if mod_name in existing_mods:
        return

    new_row = ["TBD" for _ in fieldnames]
    new_row[mod_name_idx] = mod_name
    if "MOD_TYPE_ID" in fieldnames:
        mod_type_idx = fieldnames.index("MOD_TYPE_ID")
        new_row[mod_type_idx] = "TBD"
    if "QUOTE_FILE" in fieldnames:
        quote_idx = fieldnames.index("QUOTE_FILE")
        new_row[quote_idx] = f"{mod_name}.txt"
    if "PREVIOUS_MOD_NAME" in fieldnames:
        new_row[fieldnames.index("PREVIOUS_MOD_NAME")] = ""
    if "FOLDER_BASE" in fieldnames:
        new_row[fieldnames.index("FOLDER_BASE")] = folder_base

    with open(COMPAT_CSV_PATH, "a", encoding="utf-8", newline="") as f:
        writer = csv.writer(f, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(new_row)


def ensure_quote_file(mod_name: str) -> None:
    os.makedirs(QUOTES_DIR, exist_ok=True)
    quote_file_path = os.path.join(QUOTES_DIR, f"{mod_name}.txt")
    if os.path.exists(quote_file_path):
        return

    with open(quote_file_path, "w", encoding="utf-8", newline="") as f:
        f.write("")


def prompt_value(label: str, non_interactive: bool) -> str:
    if non_interactive:
        return ""
    return input(label).strip()


def main():
    args = parse_args()

    token = (args.name or "").strip()
    if not token:
        token = prompt_value("Enter the mod token (e.g. FuelBurnPlus): ", args.non_interactive)
    if not token:
        fail("Mod token cannot be empty.")
    if not TOKEN_PATTERN.match(token):
        fail("Mod token uses letters and numbers only, such as FuelBurnPlus or 0SCore.")

    scope = normalize_scope(args.scope or "")
    if not scope:
        scope = normalize_scope(prompt_value("Enter scope (SERVER, CLIENT, BOTH, ADMIN, COMPAT): ", args.non_interactive))
    if not scope:
        fail("Scope must be SERVER, CLIENT, BOTH, ADMIN, or COMPAT.")

    if args.load and not normalize_load(args.load):
        fail("Load must be early, normal, or late.")

    load = normalize_load(args.load or "")
    if scope != "COMPAT" and not load:
        load = normalize_load(prompt_value("Enter load (early, normal, late). Enter for normal: ", args.non_interactive))
        if not load:
            load = "normal"
    if scope != "COMPAT" and not load:
        fail("Load must be early, normal, or late.")
    if not load:
        load = "normal"

    version = args.version.strip()
    if not VERSION_PATTERN.match(version):
        fail("Version must be in SemVer format: X.Y.Z")

    folder_base, folder_name, modinfo_name, display_name = build_mod_identity(token, scope, load, version)
    mod_path = os.path.join(INPROGRESS_DIR, folder_name)

    if os.path.exists(mod_path):
        append_compatibility_row(modinfo_name, folder_base)
        ensure_quote_file(modinfo_name)
        print(f"Folder {folder_name} already exists in {INPROGRESS_DIR}.")
        print("Compatibility CSV row and quote file were verified/backfilled.")
        return

    os.makedirs(INPROGRESS_DIR, exist_ok=True)
    os.makedirs(mod_path)

    # Add entry to 05_ReleaseData/ReadmeSystem/HELPER_ModCompatibility.csv
    # (without duplicating existing MOD_NAME rows)
    append_compatibility_row(modinfo_name, folder_base)
    # Create default quote file in ReadmeSystem/Quotes if it does not exist yet
    ensure_quote_file(modinfo_name)

    # Create ModInfo.xml
    with open(os.path.join(mod_path, "ModInfo.xml"), "w", encoding="utf-8") as f:
        f.write(TEMPLATE_MODINFO.format(mod_name=modinfo_name, display_name=display_name, version=version))

    # Create README.txt scaffold used by readme migration/publish pipelines.
    with open(os.path.join(mod_path, "README.txt"), "w", encoding="utf-8") as f:
        f.write(build_initial_readme_txt(display_name, version))

    # Create Config and XUi_InGame folders (case sensitive)
    config_path = os.path.join(mod_path, "Config")
    xui_path = os.path.join(config_path, "XUi_InGame")
    os.makedirs(xui_path)
    # Create Localization.csv with the current draft schema
    localization_path = os.path.join(config_path, "Localization.csv")
    localization_header = "Key,File,Type,UsedInMainMenu,NoTranslate,KeepLoaded,english,Context / Alternate Text,german,spanish,french,italian,japanese,koreana,polish,brazilian,russian,turkish,schinese,tchinese\n"
    with open(localization_path, "w", encoding="utf-8") as f:
        f.write(localization_header)

    print(f"Created new mod folder: {folder_name}")
    print(f"ModInfo name: {modinfo_name}")
    print(f"Display name: {display_name}")
    print(f"Location: {mod_path}")
    print("You can now edit ModInfo.xml, README.txt, and Config files as needed.")


if __name__ == "__main__":
    main()