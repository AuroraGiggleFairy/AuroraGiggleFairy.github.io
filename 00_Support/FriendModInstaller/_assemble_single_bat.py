# One-shot assembler. Not shipped to friends.
from pathlib import Path

root = Path(__file__).resolve().parent
od = (root / "INSTALL-OneDrive.ps1").read_text(encoding="utf-8")
main = (root / "INSTALL-Mods.ps1").read_text(encoding="utf-8")

# Drop requires/param from main; helper is inlined.
main = main.replace("#Requires -Version 5.1\n", "")
main = main.split("\n", 10)
# Find the line after param block
text = "\n".join(main) if False else (root / "INSTALL-Mods.ps1").read_text(encoding="utf-8")
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

new_elev = """function Restart-Elevated {
    $bat = $env:AGF_BAT_PATH
    if (-not $bat) { $bat = Join-Path $SourceDir 'ModUpdater.MandaOutback.bat' }
    $proc = Start-Process -FilePath $bat -ArgumentList 'skipconfirm' -Verb RunAs -Wait -PassThru
    if ($null -eq $proc) { exit 1 }
    exit $proc.ExitCode
}"""
if old_elev not in main_body:
    raise SystemExit("Restart-Elevated block not found")
main_body = main_body.replace(old_elev, new_elev)

after_config = """if (-not $ForceModsPath -and (Test-Path -LiteralPath $ManualPathFile)) {
    $line = (Get-Content -LiteralPath $ManualPathFile -Encoding UTF8 |
            Where-Object { $_.Trim() -ne '' -and -not $_.Trim().StartsWith('#') } |
            Select-Object -First 1)
    if ($line) { $ForceModsPath = $line.Trim().Trim('\"') }
}

if ($env:MARKER_MOD) { $MarkerMod = $env:MARKER_MOD.Trim() }
if ($null -ne $env:DELETE_MODS -and $env:DELETE_MODS -ne '') { $DeleteMods = $env:DELETE_MODS }
if ($env:DRY_RUN -match '^[01]$') { $DryRun = [int]$env:DRY_RUN }
if ($env:FORCE_MODS_PATH) { $ForceModsPath = $env:FORCE_MODS_PATH.Trim().Trim('\"') }
"""
if after_config.split("if ($env:MARKER_MOD)")[0] not in main_body:
    # insert env overrides after manual path file read
    marker = """    if ($line) { $ForceModsPath = $line.Trim().Trim('\"') }
}

$script:MarkerMod = $MarkerMod"""
    # the file uses Trim('"') not escaped
    pass

needle = """    if ($line) { $ForceModsPath = $line.Trim().Trim('\"') }
}

$script:MarkerMod = $MarkerMod"""
# actual source uses: Trim('"')
needle2 = """    if ($line) { $ForceModsPath = $line.Trim().Trim('\"') }
}"""
# read exact from file
idx = main_body.find("$script:MarkerMod = $MarkerMod")
if idx < 0:
    raise SystemExit("script MarkerMod assign not found")
main_body = (
    main_body[:idx]
    + """if ($env:MARKER_MOD) { $MarkerMod = $env:MARKER_MOD.Trim() }
if ($null -ne $env:DELETE_MODS) { $DeleteMods = $env:DELETE_MODS }
if ($env:DRY_RUN -match '^[01]$') { $DryRun = [int]$env:DRY_RUN }
if ($env:FORCE_MODS_PATH) { $ForceModsPath = $env:FORCE_MODS_PATH.Trim().Trim('\"') }

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
    $w = $env:DISCORD_WEBHOOK_URL.Trim().Trim('\"')
    if ($w -match '^https://(?:discord|discordapp)\\.com/api/webhooks/\\d+/') { $script:DiscordWebhook = $w }
}
""",
)

od = od.replace(
    """function Get-OneDriveShareUrlFromConfig {
    param([hashtable]$ConfigMap)
    $file = Join-Path $SourceDir 'INSTALL-ONEDRIVE.txt'
""",
    """function Get-OneDriveShareUrlFromConfig {
    param([hashtable]$ConfigMap)
    if ($env:ONEDRIVE_SHARE_URL) {
        $u = $env:ONEDRIVE_SHARE_URL.Trim().Trim('\"')
        if ($u) { return $u }
    }
    $file = Join-Path $SourceDir 'INSTALL-ONEDRIVE.txt'
""",
)

header = r'''@echo off
setlocal
cd /d "%~dp0"
title ModUpdater.MandaOutback

REM ============================================================
REM  EDIT THESE, then send THIS ONE FILE.
REM  Friends can put this file anywhere and double-click it.
REM  It finds 7 Days to Die and asks which one to sync.
REM  After the first pick, later runs just confirm that folder.
REM ============================================================

REM Personal OneDrive folder link: Anyone with the link can view
set "ONEDRIVE_SHARE_URL="

REM 1 = make Mods match the OneDrive folder exactly (delete extras)
REM 0 = only add/update cloud mods; leave extra local mods alone
set "REMOVE_NOT_IN_CLOUD=1"

REM Optional. Remove extra leftover mods. Separate with ;
set "DELETE_MODS="

REM Optional. Private Discord webhook for install reports
set "DISCORD_WEBHOOK_URL="

REM 1 = show the plan only, change nothing
set "DRY_RUN=0"

REM Only used if ONEDRIVE_SHARE_URL is empty (old zip-next-to-bat mode)
set "MARKER_MOD=AGF-HUDPlus-1Main"

REM ============================================================

if /i "%~1"=="skipconfirm" set "AGF_SKIP_CONFIRM=1"

set "AGF_SOURCE_DIR=%~dp0"
set "AGF_BAT_PATH=%~f0"

echo.
echo  Close 7 Days to Die before continuing.
echo.

set "AGF_PS=%TEMP%\AGF-SyncMods.ps1"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$p='%~f0'; $o=$env:AGF_PS; $t=[IO.File]::ReadAllText($p); $m='### AGF-POWERSHELL ###'; $i=$t.LastIndexOf($m); if($i -lt 0){ Write-Error 'Missing script marker'; exit 1 }; $utf8=New-Object System.Text.UTF8Encoding $false; [IO.File]::WriteAllText($o, $t.Substring($i+$m.Length), $utf8)"
if errorlevel 1 (
    echo Failed to start the installer.
    pause
    exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%AGF_PS%"
set "EXIT_CODE=%ERRORLEVEL%"
del "%AGF_PS%" >nul 2>&1
exit /b %EXIT_CODE%

### AGF-POWERSHELL ###
'''

out = header + "\n" + od + "\n" + main_body
out_path = root / "ModUpdater.MandaOutback.bat"
out_path.write_text(out, encoding="utf-8", newline="\r\n")
old = root / "INSTALL-Mods.bat"
if old.exists():
    old.unlink()
print("wrote", out_path, "chars", len(out))
