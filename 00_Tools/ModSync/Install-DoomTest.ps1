# Doom Test Server setup.
# Player picks an existing 7 Days to Die install or makes a copy just for this
# server. A new copy takes game files only (plus 0_TFP_Harmony) so the server
# mods can be installed later.
$ErrorActionPreference = 'Stop'

$ServerName = 'Doom Test Server'
$CopyFolderName = '7 Days to Die - Doom Test'
$Ip = '63.143.56.130'
$Port = 41341

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$HelperSource = Join-Path $RepoRoot '01_Draft\AGF-NoEAC-ModSync-v0.1.3'
$LogFile = Join-Path $env:TEMP 'ModSync-Setup.log'
$RememberFile = Join-Path $env:LOCALAPPDATA ('AGF-ModSync\' + ($ServerName -replace '[^A-Za-z0-9]', '') + '.txt')

function Get-RememberedInstall {
    if (-not (Test-Path -LiteralPath $RememberFile)) { return $null }
    $line = (Get-Content -LiteralPath $RememberFile -ErrorAction SilentlyContinue |
        Where-Object { $_.Trim() -ne '' -and -not $_.Trim().StartsWith('#') } |
        Select-Object -First 1)
    if (-not $line) { return $null }
    $path = $line.Trim().Trim('"')
    if (Test-Path -LiteralPath (Join-Path $path '7DaysToDie.exe')) { return $path }
    return $null
}

function Save-RememberedInstall {
    param([string]$Path)
    New-Item -ItemType Directory -Path (Split-Path -Parent $RememberFile) -Force | Out-Null
    @("# ModSync remembers which game $ServerName uses.", $Path) |
        Set-Content -LiteralPath $RememberFile -Encoding UTF8
}

function Copy-Quiet {
    param([string]$From, [string]$To, [string[]]$ExtraArgs = @())
    $roboArgs = @($From, $To, '/E', '/R:1', '/W:1', '/NFL', '/NDL', '/NP', '/NJH', '/NJS') + $ExtraArgs
    & robocopy @roboArgs *>> $LogFile
    if ($LASTEXITCODE -ge 8) { throw "Copy failed. Details: $LogFile" }
}

function Get-SteamGameFolder {
    $roots = New-Object System.Collections.Generic.List[string]
    foreach ($reg in @('HKCU:\Software\Valve\Steam', 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam')) {
        try {
            $p = (Get-ItemProperty -Path $reg -ErrorAction Stop).SteamPath
            if ($p) { [void]$roots.Add($p) }
        } catch { }
    }
    $fallback = Join-Path ${env:ProgramFiles(x86)} 'Steam'
    if (Test-Path -LiteralPath $fallback) { [void]$roots.Add($fallback) }

    foreach ($root in $roots) {
        $manifest = Join-Path $root 'steamapps\appmanifest_251570.acf'
        if (-not (Test-Path -LiteralPath $manifest)) { continue }
        $text = Get-Content -Raw -LiteralPath $manifest
        $install = [regex]::Match($text, '"installdir"\s+"([^"]+)"').Groups[1].Value
        if (-not $install) { continue }
        $game = Join-Path $root ("steamapps\common\" + $install)
        if (Test-Path -LiteralPath (Join-Path $game '7DaysToDie.exe')) { return $game }
    }
    return $null
}

function Get-OtherInstalls {
    param([string]$SteamGame)
    $parent = Split-Path -Parent $SteamGame
    $found = New-Object System.Collections.Generic.List[string]
    foreach ($dir in @(Get-ChildItem -LiteralPath $parent -Directory -ErrorAction SilentlyContinue)) {
        if ($dir.FullName -eq $SteamGame) { continue }
        if (Test-Path -LiteralPath (Join-Path $dir.FullName '7DaysToDie.exe')) {
            [void]$found.Add($dir.FullName)
        }
    }
    return @($found)
}

function Install-Helper {
    param([string]$GameDir)
    $dest = Join-Path $GameDir 'Mods\AGF-NoEAC-ModSync-v0.1.2'
    if (Test-Path -LiteralPath $dest) { Remove-Item -LiteralPath $dest -Recurse -Force }
    New-Item -ItemType Directory -Path (Split-Path -Parent $dest) -Force | Out-Null
    Copy-Quiet $HelperSource $dest
    # The shortcut carries the address. A join file in Mods is loaded by every
    # 7 Days to Die install that has this mod.
    $joinFile = Join-Path $dest 'modsync-join.txt'
    if (Test-Path -LiteralPath $joinFile) { Remove-Item -LiteralPath $joinFile -Force }
}

# The game does not read this file.
# Setup writes it so a later setup run does not ask again before using this folder.
function Set-ManagedMarker {
    param([string]$GameDir, [bool]$Managed)
    $marker = Join-Path $GameDir 'ModSync-Managed.txt'
    if ($Managed) {
        @(
            "# This game folder is kept matching $ServerName by ModSync."
            '# Mods the server does not use will be removed from it.'
            "SERVER=$ServerName"
        ) | Set-Content -LiteralPath $marker -Encoding ASCII
    } elseif (Test-Path -LiteralPath $marker) {
        Remove-Item -LiteralPath $marker -Force
    }
}

function New-PlayShortcut {
    param([string]$GameDir)
    $lnk = Join-Path ([Environment]::GetFolderPath('Desktop')) ($ServerName + '.lnk')
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($lnk)
    $shortcut.TargetPath = Join-Path $GameDir '7DaysToDie.exe'
    $safeName = ($ServerName -replace '"', '')
    $shortcut.Arguments = "-skipintro -skipnewsscreen=true -noeac -modsyncjoin ${Ip}:${Port} -modsyncname `"$safeName`""
    $shortcut.WorkingDirectory = $GameDir
    $shortcut.Description = $ServerName
    $shortcut.Save()
    return $lnk
}

'' | Set-Content -LiteralPath $LogFile

if (-not (Test-Path -LiteralPath (Join-Path $HelperSource 'ModSync.dll'))) {
    throw "Helper is missing. Build DLL_NoEAC-ModSync first."
}

$steamGame = Get-SteamGameFolder
if (-not $steamGame) { throw 'Could not find 7 Days to Die in Steam.' }

$existing = @(Get-OtherInstalls $steamGame)
$newCopyPath = Join-Path (Split-Path -Parent $steamGame) $CopyFolderName
$newCopyExists = Test-Path -LiteralPath (Join-Path $newCopyPath '7DaysToDie.exe')

Write-Host ''
Write-Host "  $ServerName" -ForegroundColor Cyan
Write-Host ''

$remembered = Get-RememberedInstall
if ($remembered) {
    Write-Host ("  Last time you used: " + (Split-Path -Leaf $remembered)) -ForegroundColor Green
    Write-Host '  Press Enter to use it again, or type C to choose a different one.'
    $answer = (Read-Host '  ').Trim()
    if ($answer -notmatch '^[Cc]') {
        Install-Helper $remembered
        Set-ManagedMarker $remembered ($remembered -eq $newCopyPath)
        $shortcut = New-PlayShortcut $remembered
        Save-RememberedInstall $remembered
        Write-Host ''
        Write-Host '  All set.' -ForegroundColor Green
        Write-Host ''
        Write-Host "  A shortcut named `"$ServerName`" is on your desktop."
        Write-Host '  Use it to play. If the server asks for a password, type it in the game.'
        Write-Host ''
        return
    }
    Write-Host ''
}

Write-Host '  Which game should this server use?'
Write-Host ''

$options = New-Object System.Collections.Generic.List[object]
if ($newCopyExists) {
    [void]$options.Add([pscustomobject]@{ Label = "$CopyFolderName  (already set up for this server)"; Path = $newCopyPath; MakeCopy = $false })
} else {
    [void]$options.Add([pscustomobject]@{ Label = 'Make a copy just for this server  (recommended)'; Path = $newCopyPath; MakeCopy = $true })
}
[void]$options.Add([pscustomobject]@{ Label = "$(Split-Path -Leaf $steamGame)  (your main Steam game)"; Path = $steamGame; MakeCopy = $false })
foreach ($e in $existing) {
    if ($e -eq $newCopyPath) { continue }
    [void]$options.Add([pscustomobject]@{ Label = (Split-Path -Leaf $e); Path = $e; MakeCopy = $false })
}

for ($i = 0; $i -lt $options.Count; $i++) {
    Write-Host ("    {0}) {1}" -f ($i + 1), $options[$i].Label)
}
Write-Host ''

$choice = $null
while (-not $choice) {
    $typed = (Read-Host '  Type a number').Trim()
    if ($typed -match '^\d+$') {
        $n = [int]$typed
        if ($n -ge 1 -and $n -le $options.Count) { $choice = $options[$n - 1] }
    }
    if (-not $choice) { Write-Host '  Please type one of the numbers above.' -ForegroundColor Yellow }
}

$target = $choice.Path
Write-Host ''

if ($choice.MakeCopy) {
    Write-Host '  Copying the game. This is big, so it can take a while...' -ForegroundColor Yellow
    if (-not (Test-Path -LiteralPath $target)) { New-Item -ItemType Directory -Path $target | Out-Null }
    Copy-Quiet $steamGame $target @('/XD', 'logs', (Join-Path $steamGame 'Mods'))

    $harmony = Join-Path $steamGame 'Mods\0_TFP_Harmony'
    if (Test-Path -LiteralPath $harmony) {
        Copy-Quiet $harmony (Join-Path $target 'Mods\0_TFP_Harmony')
    } else {
        Write-Host '  Note: 0_TFP_Harmony was not found in your Steam game.' -ForegroundColor Yellow
    }
    Write-Host '  Copy finished.' -ForegroundColor Green
} else {
    Write-Host ("  Using: " + (Split-Path -Leaf $target)) -ForegroundColor Green
}

Install-Helper $target
Set-ManagedMarker $target ($target -eq $newCopyPath)
$shortcut = New-PlayShortcut $target
Save-RememberedInstall $target

Write-Host ''
Write-Host '  All set.' -ForegroundColor Green
Write-Host ''
Write-Host "  A shortcut named `"$ServerName`" is on your desktop."
Write-Host '  Use it to play. If the server asks for a password, type it in the game.'
Write-Host ''
