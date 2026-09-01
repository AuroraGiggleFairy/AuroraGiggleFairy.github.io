# Admin generator. Reads ADMIN-CONFIG.txt and writes one player .bat into Output\.
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parent
ENGINE = ROOT / "engine"
OUT_DIR = ROOT / "Output"
CONFIG_PATH = ROOT / "ADMIN-CONFIG.txt"


def read_config(path: Path) -> dict:
    data = {}
    if not path.is_file():
        raise SystemExit("Missing ADMIN-CONFIG.txt")
    for raw in path.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        if "=" not in line:
            continue
        key, val = line.split("=", 1)
        data[key.strip().upper()] = val.strip().strip('"')
    return data


def safe_bat_stem(name: str) -> str:
    name = name.strip()
    if name.lower().endswith(".bat"):
        name = name[:-4]
    invalid = '<>:"/\\|?*'
    cleaned = "".join("_" if ch in invalid else ch for ch in name).strip()
    cleaned = cleaned.rstrip(".")
    if not cleaned:
        raise SystemExit("CLIENT_BAT_NAME is empty.")
    return cleaned


def assemble(share_url: str, bat_stem: str, remove_not_in_cloud: str, discord_url: str, dry_run: str) -> str:
    od = (ENGINE / "INSTALL-OneDrive.ps1").read_text(encoding="utf-8")
    text = (ENGINE / "INSTALL-Mods.ps1").read_text(encoding="utf-8")
    start = text.find("$ErrorActionPreference = 'Stop'")
    if start < 0:
        raise SystemExit("could not find script start")
    main_body = text[start:]
    main_body = main_body.replace(
        """if (-not $SourceDir) { $SourceDir = $PSScriptRoot }
$oneDriveHelper = Join-Path $PSScriptRoot 'INSTALL-OneDrive.ps1'
if (Test-Path -LiteralPath $oneDriveHelper) { . $oneDriveHelper }
""",
        """$SkipConfirm = ($env:AGF_SKIP_CONFIRM -eq '1')
$PauseWhenDone = ($env:AGF_NO_PAUSE -ne '1')
$SourceDir = $env:AGF_SOURCE_DIR
if (-not $SourceDir) { $SourceDir = $PSScriptRoot }
$SourceDir = $SourceDir.TrimEnd('\\', '/')
$MarkerMod = ''
$DeleteMods = ''
$DryRun = -1
$ForceModsPath = ''
""",
    )

    old_elev = """function Restart-Elevated {
    $argList = @(
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-File', $PSCommandPath,
        '-MarkerMod', $script:MarkerMod,
        '-DeleteMods', $script:DeleteMods,
        '-DryRun', "$script:DryRun",
        '-SourceDir', $SourceDir,
        '-SkipConfirm',
        '-PauseWhenDone'
    )
    if ($script:ForceModsPath) {
        $argList += @('-ForceModsPath', $script:ForceModsPath)
    }
    $proc = Start-Process -FilePath 'powershell.exe' -Verb RunAs -Wait -PassThru -ArgumentList $argList
    if ($null -eq $proc) { exit 1 }
    exit $proc.ExitCode
}"""
    new_elev = f"""function Restart-Elevated {{
    $bat = $env:AGF_BAT_PATH
    if (-not $bat) {{ $bat = Join-Path $SourceDir '{bat_stem}.bat' }}
    $proc = Start-Process -FilePath $bat -ArgumentList 'skipconfirm' -Verb RunAs -Wait -PassThru
    if ($null -eq $proc) {{ exit 1 }}
    exit $proc.ExitCode
}}"""
    if old_elev not in main_body:
        raise SystemExit("Restart-Elevated block not found")
    main_body = main_body.replace(old_elev, new_elev)

    idx = main_body.find("$script:MarkerMod = $MarkerMod")
    if idx < 0:
        raise SystemExit("script MarkerMod assign not found")
    main_body = (
        main_body[:idx]
        + """if ($env:MARKER_MOD) { $MarkerMod = $env:MARKER_MOD.Trim() }
if ($null -ne $env:DELETE_MODS) { $DeleteMods = $env:DELETE_MODS }
if ($env:DRY_RUN -match '^[01]$') { $DryRun = [int]$env:DRY_RUN }
if ($env:FORCE_MODS_PATH) { $ForceModsPath = $env:FORCE_MODS_PATH.Trim().Trim('"') }

"""
        + main_body[idx:]
    )

    needle = "$script:DiscordWebhook = Get-DiscordWebhookUrl -ConfigMap $config\n"
    if needle not in main_body:
        raise SystemExit("discord assign not found")
    main_body = main_body.replace(
        needle,
        needle
        + """if ($env:DISCORD_WEBHOOK_URL) {
    $w = $env:DISCORD_WEBHOOK_URL.Trim().Trim('"')
    if ($w -match '^https://(?:discord|discordapp)\\.com/api/webhooks/\\d+/') { $script:DiscordWebhook = $w }
}
""",
    )

    main_body = main_body.replace("__AGF_CLIENT_BAT_STEM__", bat_stem)
    main_body = main_body.replace("__AGF_CLIENT_BAT_FILE__", bat_stem + ".bat")
    od = od.replace("__AGF_CLIENT_BAT_STEM__", bat_stem)
    od = od.replace("__AGF_CLIENT_BAT_FILE__", bat_stem + ".bat")

    header = f"""@echo off
setlocal
cd /d "%~dp0"
title {bat_stem}

REM ============================================================
REM  Player sync file. Put this anywhere and double-click it.
REM  It finds 7 Days to Die and keeps Mods matched to the
REM  shared pack. No login required.
REM ============================================================

set "SHARE_URL={share_url}"
set "ONEDRIVE_SHARE_URL=%SHARE_URL%"
set "REMOVE_NOT_IN_CLOUD={remove_not_in_cloud}"
set "DELETE_MODS="
set "DISCORD_WEBHOOK_URL={discord_url}"
set "DRY_RUN={dry_run}"
set "MARKER_MOD="

if /i "%~1"=="skipconfirm" set "AGF_SKIP_CONFIRM=1"

set "AGF_SOURCE_DIR=%~dp0"
set "AGF_BAT_PATH=%~f0"

echo.
echo  Close 7 Days to Die before continuing.
echo.

set "AGF_PS=%TEMP%\\AGF-SyncMods.ps1"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$p='%~f0'; $o=$env:AGF_PS; $t=[IO.File]::ReadAllText($p); $m='### AGF-POWERSHELL ###'; $i=$t.LastIndexOf($m); if($i -lt 0){{ Write-Error 'Missing script marker'; exit 1 }}; $utf8=New-Object System.Text.UTF8Encoding $false; [IO.File]::WriteAllText($o, $t.Substring($i+$m.Length), $utf8)"
if errorlevel 1 (
    echo Failed to start the updater.
    pause
    exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%AGF_PS%"
set "EXIT_CODE=%ERRORLEVEL%"
del "%AGF_PS%" >nul 2>&1
exit /b %EXIT_CODE%

### AGF-POWERSHELL ###
"""
    return header + "\n" + od + "\n" + main_body


def main() -> None:
    cfg = read_config(CONFIG_PATH)
    share = cfg.get("SHARE_URL", "").strip()
    if not share:
        raise SystemExit("Set SHARE_URL in ADMIN-CONFIG.txt")
    if "sharepoint.com" in share.lower() and ".zip" not in share.lower():
        raise SystemExit(
            "Work/school SharePoint folder links cannot be listed. "
            "Zip the mod folders and set SHARE_URL to that zip, or use a view-only OneDrive / Google Drive folder."
        )

    stem = safe_bat_stem(cfg.get("CLIENT_BAT_NAME", ""))
    if re.search(r"SERVERNAME", stem, re.I):
        raise SystemExit("Change CLIENT_BAT_NAME in ADMIN-CONFIG.txt (replace SERVERNAME with your server name).")

    remove = cfg.get("REMOVE_NOT_IN_CLOUD", "1")
    if remove not in ("0", "1"):
        remove = "1"
    dry = cfg.get("DRY_RUN", "0")
    if dry not in ("0", "1"):
        dry = "0"
    discord = cfg.get("DISCORD_WEBHOOK_URL", "")

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    out_path = OUT_DIR / f"{stem}.bat"
    text = assemble(share, stem, remove, discord, dry)
    out_path.write_text(text, encoding="utf-8", newline="\r\n")
    raw = out_path.read_bytes()
    if raw.startswith(b"\xef\xbb\xbf"):
        out_path.write_bytes(raw[3:])
    print("Wrote", out_path)


if __name__ == "__main__":
    main()
