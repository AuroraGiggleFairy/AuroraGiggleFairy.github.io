#Requires -Version 5.1
param(
    [string]$MarkerMod = '',
    [string]$DeleteMods = '',
    [int]$DryRun = -1,
    [string]$ForceModsPath = '',
    [string]$SourceDir = '',
    [switch]$SkipConfirm,
    [switch]$PauseWhenDone
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $SourceDir) { $SourceDir = $PSScriptRoot }
$oneDriveHelper = Join-Path $PSScriptRoot 'INSTALL-OneDrive.ps1'
if (Test-Path -LiteralPath $oneDriveHelper) { . $oneDriveHelper }
$LogFile = Join-Path $SourceDir 'INSTALL-LOG.txt'
$ConfigFile = Join-Path $SourceDir 'INSTALL-CONFIG.txt'
$ManualPathFile = Join-Path $SourceDir 'MY-MODS-FOLDER.txt'
$WebhookFile = Join-Path $SourceDir 'INSTALL-DISCORD-WEBHOOK.txt'
$ProtectedMods = @('0_TFP_Harmony')
$copied = 0
$deleted = 0
$failed = 0
$sourceMods = @()
$targets = @()
$script:DiscordWebhook = ''
$script:CachedCandidateMods = $null

function Write-Log {
    param(
        [string]$Message,
        [string]$Color = 'Gray'
    )
    $line = '{0}  {1}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $Message
    Add-Content -LiteralPath $LogFile -Value $line -Encoding UTF8
    if ($Color -eq 'Gray') {
        Write-Host $Message
    } else {
        Write-Host $Message -ForegroundColor $Color
    }
}

function Read-ConfigFile {
    $map = @{}
    if (-not (Test-Path -LiteralPath $ConfigFile)) { return $map }
    Get-Content -LiteralPath $ConfigFile -Encoding UTF8 | ForEach-Object {
        $line = $_.Trim()
        if ($line -eq '' -or $line.StartsWith('#')) { return }
        $eq = $line.IndexOf('=')
        if ($eq -lt 1) { return }
        $key = $line.Substring(0, $eq).Trim().ToUpperInvariant()
        $val = $line.Substring($eq + 1).Trim()
        $map[$key] = $val
    }
    return $map
}

function Test-IsProtectedLocalName {
    param([string]$Name)
    foreach ($p in $ProtectedMods) {
        if ($Name.Equals($p, [StringComparison]::OrdinalIgnoreCase)) { return $true }
    }
    if ($Name -like 'INSTALL*') { return $true }
    if ($Name -like 'AGF-ModUpdater*') { return $true }
    if ($Name -like 'ModUpdater*') { return $true }
    if ($Name.Equals('desktop.ini', [StringComparison]::OrdinalIgnoreCase)) { return $true }
    if ($Name.Equals('thumbs.db', [StringComparison]::OrdinalIgnoreCase)) { return $true }
    return $false
}

function Get-ModFamily {
    param([string]$FolderName)
    if ($FolderName -match '^(?i)(.*)-v\d+(?:\.\d+)*$') {
        return $Matches[1]
    }
    return $FolderName
}

function Test-HasMarker {
    param(
        [string]$ModsDir,
        [string]$Marker
    )
    if (-not $Marker) { return $false }
    if (-not (Test-Path -LiteralPath $ModsDir)) { return $false }
    $dirs = @(Get-ChildItem -LiteralPath $ModsDir -Directory -ErrorAction SilentlyContinue)
    foreach ($d in $dirs) {
        if ($d.Name.StartsWith($Marker, [StringComparison]::OrdinalIgnoreCase) -and
            (Test-Path -LiteralPath (Join-Path $d.FullName 'ModInfo.xml'))) {
            return $true
        }
    }
    return $false
}

function Get-UniquePaths {
    param([string[]]$Paths)
    $seen = @{}
    $out = New-Object System.Collections.Generic.List[string]
    foreach ($p in $Paths) {
        if (-not $p) { continue }
        $full = $p.TrimEnd('\', '/')
        if (Test-Path -LiteralPath $full) {
            try { $full = (Resolve-Path -LiteralPath $full).Path } catch { }
        }
        $key = $full.ToLowerInvariant()
        if (-not $seen.ContainsKey($key)) {
            $seen[$key] = $true
            [void]$out.Add($full)
        }
    }
    return @($out)
}

function Get-SteamLibraryPaths {
    $roots = New-Object System.Collections.Generic.List[string]
    foreach ($reg in @(
            'HKCU:\Software\Valve\Steam',
            'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam',
            'HKLM:\SOFTWARE\Valve\Steam'
        )) {
        foreach ($prop in @('SteamPath', 'InstallPath')) {
            try {
                $p = (Get-ItemProperty -Path $reg -ErrorAction Stop).$prop
                if ($p) { [void]$roots.Add($p.TrimEnd('\', '/')) }
            } catch { }
        }
    }
    foreach ($d in @(
            (Join-Path ${env:ProgramFiles(x86)} 'Steam'),
            (Join-Path $env:ProgramFiles 'Steam')
        )) {
        if ($d -and (Test-Path -LiteralPath $d)) { [void]$roots.Add($d.TrimEnd('\', '/')) }
    }

    $libs = New-Object System.Collections.Generic.List[string]
    foreach ($root in (Get-UniquePaths @($roots))) {
        [void]$libs.Add($root)
        $vdf = Join-Path $root 'steamapps\libraryfolders.vdf'
        if (Test-Path -LiteralPath $vdf) {
            $text = Get-Content -Raw -LiteralPath $vdf -Encoding UTF8
            foreach ($m in [regex]::Matches($text, '"path"\s+"([^"]+)"')) {
                $p = ($m.Groups[1].Value -replace '\\\\', '\').TrimEnd('\', '/')
                if ($p) { [void]$libs.Add($p) }
            }
        }
    }
    return @(Get-UniquePaths ($libs | Where-Object { $_ -and (Test-Path -LiteralPath $_) }))
}

function Test-ShouldSkipGameSearchDir {
    param([string]$Name)
    if (-not $Name) { return $true }
    $n = $Name.ToLowerInvariant()
    $skip = @(
        '$recycle.bin', 'system volume information', 'recovery', 'perflogs',
        'windows', 'windows.old', 'winsxs', 'windowsapps',
        'appdata', 'application data', 'local settings',
        'temp', 'tmp', 'node_modules', '.git',
        'inetcache', 'package cache', 'windows.old',
        'csc', 'config.msi', 'msocache', 'system volume information'
    )
    return ($skip -contains $n)
}

function Add-GameModsUnderRoot {
    param(
        [string]$Root,
        [int]$MaxDepth,
        [System.Collections.Generic.List[string]]$Found,
        [int]$MaxVisit = 500
    )
    if (-not $Root -or -not (Test-Path -LiteralPath $Root)) { return }
    $start = $Root
    try { $start = (Resolve-Path -LiteralPath $Root).Path } catch { return }
    $queue = New-Object System.Collections.Queue
    $queue.Enqueue((New-Object psobject -Property @{ Dir = $start; Depth = 0 }))
    $visited = 0
    $reparse = [System.IO.FileAttributes]::ReparsePoint
    while ($queue.Count -gt 0 -and $visited -lt $MaxVisit) {
        $cur = $queue.Dequeue()
        $visited++
        $dir = [string]$cur.Dir
        $depth = [int]$cur.Depth
        if (Test-IsGameExeFolder $dir) {
            $mods = Join-Path $dir 'Mods'
            if (-not (Test-Path -LiteralPath $mods)) {
                try { New-Item -ItemType Directory -Path $mods | Out-Null } catch { }
            }
            [void]$Found.Add($mods)
            continue
        }
        if ($depth -ge $MaxDepth) { continue }
        $kids = @(Get-ChildItem -LiteralPath $dir -Directory -ErrorAction SilentlyContinue)
        foreach ($k in $kids) {
            if (Test-ShouldSkipGameSearchDir $k.Name) { continue }
            try {
                if ($k.Attributes -band $reparse) { continue }
            } catch { }
            $queue.Enqueue((New-Object psobject -Property @{ Dir = $k.FullName; Depth = ($depth + 1) }))
        }
    }
}

function Get-LocalFixedDriveRoots {
    $roots = New-Object System.Collections.Generic.List[string]
    $disks = @()
    try {
        $disks = @(Get-CimInstance -ClassName Win32_LogicalDisk -Filter 'DriveType=3' -ErrorAction Stop)
    } catch {
        try {
            $disks = @(Get-WmiObject Win32_LogicalDisk -Filter 'DriveType=3' -ErrorAction Stop)
        } catch { $disks = @() }
    }
    foreach ($d in $disks) {
        $id = $null
        if ($d.PSObject.Properties.Name -contains 'DeviceID') { $id = [string]$d.DeviceID }
        if (-not $id) { continue }
        $root = $id.TrimEnd('\') + '\'
        if (Test-Path -LiteralPath $root) { [void]$roots.Add($root) }
    }
    if ($roots.Count -eq 0) {
        foreach ($drv in @(Get-PSDrive -PSProvider FileSystem -ErrorAction SilentlyContinue)) {
            if (-not $drv.Root) { continue }
            if ($drv.Root -like '\\*') { continue }
            [void]$roots.Add($drv.Root)
        }
    }
    return @(Get-UniquePaths @($roots))
}

function Get-CandidateModsFolders {
    if ($null -ne $script:CachedCandidateMods) {
        return @($script:CachedCandidateMods)
    }
    $found = New-Object System.Collections.Generic.List[string]
    foreach ($lib in (Get-SteamLibraryPaths)) {
        $common = Join-Path $lib 'steamapps\common'
        if (-not (Test-Path -LiteralPath $common)) { continue }
        $dirs = @(Get-ChildItem -LiteralPath $common -Directory -ErrorAction SilentlyContinue)
        foreach ($gameDir in $dirs) {
            if (-not (Test-IsGameExeFolder $gameDir.FullName)) { continue }
            $mods = Join-Path $gameDir.FullName 'Mods'
            [void]$found.Add($mods)
        }
    }
    $appDataMods = Join-Path $env:APPDATA '7DaysToDie\Mods'
    if (Test-Path -LiteralPath $appDataMods) { [void]$found.Add($appDataMods) }
    $near = Resolve-GameModsFolder $SourceDir
    if ($near) { [void]$found.Add($near) }

    Write-Host '  Searching this PC for 7 Days to Die...' -ForegroundColor DarkGray
    $hotNames = @(
        '7D2D', '7 Days To Die', 'Games', 'Game', 'SteamLibrary', 'Steam',
        'GOG Games', 'GOG Galaxy\Games', 'Epic Games', 'zz_COMMUNITYSERVER'
    )
    foreach ($root in (Get-LocalFixedDriveRoots)) {
        Write-Host ("    {0}" -f $root.TrimEnd('\')) -ForegroundColor DarkGray
        foreach ($hot in $hotNames) {
            Add-GameModsUnderRoot -Root (Join-Path $root $hot) -MaxDepth 6 -Found $found -MaxVisit 2000
        }
        Add-GameModsUnderRoot -Root $root -MaxDepth 10 -Found $found -MaxVisit 12000
    }
    $script:CachedCandidateMods = @(Get-UniquePaths @($found))
    return @($script:CachedCandidateMods)
}

function ConvertTo-ModsFolderPath {
    param([string]$PathText)
    if (-not $PathText) { return $null }
    $p = $PathText.Trim().Trim('"')
    if (-not (Test-Path -LiteralPath $p)) { return $null }
    $p = (Resolve-Path -LiteralPath $p).Path
    if (Test-Path -LiteralPath $p -PathType Leaf) { $p = Split-Path -Parent $p }
    if (Test-IsGameExeFolder $p) {
        $mods = Join-Path $p 'Mods'
        if (-not (Test-Path -LiteralPath $mods)) {
            New-Item -ItemType Directory -Path $mods | Out-Null
        }
        return (Resolve-Path -LiteralPath $mods).Path
    }
    $leaf = Split-Path -Leaf $p
    if ($leaf.Equals('Mods', [StringComparison]::OrdinalIgnoreCase)) {
        return $p
    }
    if (Test-IsGameExeFolder (Split-Path -Parent $p)) {
        return $p
    }
    $dirs = @(Get-ChildItem -LiteralPath $p -Directory -ErrorAction SilentlyContinue)
    foreach ($d in $dirs) {
        if (Test-Path -LiteralPath (Join-Path $d.FullName 'ModInfo.xml')) { return $p }
    }
    $near = New-Object System.Collections.Generic.List[string]
    Add-GameModsUnderRoot -Root $p -MaxDepth 3 -Found $near
    $uniq = @(Get-UniquePaths @($near))
    if ($uniq.Count -eq 1) {
        $mods = $uniq[0]
        if (-not (Test-Path -LiteralPath $mods)) {
            New-Item -ItemType Directory -Path $mods | Out-Null
        }
        return (Resolve-Path -LiteralPath $mods).Path
    }
    return $null
}

function Get-UpdaterIdentity {
    $bat = $env:AGF_BAT_PATH
    $name = '__AGF_CLIENT_BAT_STEM__'
    if ($bat) {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($bat)
    }
    $dot = $name.LastIndexOf('.')
    if ($dot -ge 0 -and $dot -lt ($name.Length - 1)) {
        $id = Get-SafeIdentity ($name.Substring($dot + 1).Trim())
        if ($id) { return $id }
    }
    return 'default'
}

function Get-SafeIdentity {
    param([string]$Raw)
    if (-not $Raw) { return 'default' }
    $invalid = [System.IO.Path]::GetInvalidFileNameChars()
    $chars = New-Object System.Text.StringBuilder
    foreach ($ch in $Raw.ToCharArray()) {
        if ($invalid -contains $ch) {
            [void]$chars.Append('_')
        } else {
            [void]$chars.Append($ch)
        }
    }
    $s = $chars.ToString().Trim()
    if (-not $s) { return 'default' }
    return $s
}

function Get-RememberFileName {
    return ('AGF-ModUpdater.{0}.txt' -f (Get-UpdaterIdentity))
}

function Get-RememberFilePath {
    param([string]$ModsDir)
    return (Join-Path $ModsDir (Get-RememberFileName))
}

function Find-RememberedModsFolders {
    $name = Get-RememberFileName
    $hits = New-Object System.Collections.Generic.List[string]
    foreach ($mods in @(Get-CandidateModsFolders)) {
        if (Test-Path -LiteralPath (Join-Path $mods $name)) {
            [void]$hits.Add($mods)
        }
    }
    return @($hits)
}

function Clear-OtherRememberFiles {
    param([string]$KeepModsDir)
    $name = Get-RememberFileName
    $keep = $KeepModsDir.TrimEnd('\', '/').ToLowerInvariant()
    foreach ($mods in @(Get-CandidateModsFolders)) {
        if ($mods.TrimEnd('\', '/').ToLowerInvariant() -eq $keep) { continue }
        $p = Join-Path $mods $name
        if (Test-Path -LiteralPath $p) {
            try { Remove-Item -LiteralPath $p -Force -ErrorAction Stop } catch { }
        }
    }
}

function Write-RememberFile {
    param([string]$ModsDir)
    $path = Get-RememberFilePath $ModsDir
    $utf8 = New-Object System.Text.UTF8Encoding $false
    $nl = [Environment]::NewLine
    $body = 'This file remembers which 7 Days to Die folder to update.{0}Left here by Mod Updater. You can leave it.{0}{1}{0}' -f $nl, (Get-UpdaterIdentity)
    try {
        [System.IO.File]::WriteAllText($path, $body, $utf8)
    } catch {
    }
}

function Save-ChosenModsFolder {
    param([string]$ModsDir)
    $file = Join-Path $env:TEMP 'AGF-SyncMods-target.txt'
    $utf8 = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($file, $ModsDir, $utf8)
    Write-RememberFile $ModsDir
    Clear-OtherRememberFiles $ModsDir
}

function Read-ChosenModsFolder {
    $file = Join-Path $env:TEMP 'AGF-SyncMods-target.txt'
    if (Test-Path -LiteralPath $file) {
        $line = [System.IO.File]::ReadAllText($file).Trim().Trim([char]0xFEFF)
        if ($line) {
            $resolved = ConvertTo-ModsFolderPath $line
            if ($resolved) { return $resolved }
        }
    }
    $remembered = @(Find-RememberedModsFolders)
    if ($remembered.Count -eq 1) { return $remembered[0] }
    return $null
}

function Confirm-ModsFolderChoice {
    param([string]$ModsDir)
    Write-Host ''
    Write-Host '  Update this folder?' -ForegroundColor Green
    Write-Host ("    {0}" -f $ModsDir)
    Write-Host '  Y = use this one.  N = pick again.' -ForegroundColor Green
    return (Read-YesNo '  Y or N')
}

function Read-YesNo {
    param([string]$Prompt)
    while ($true) {
        $typed = Read-Host $Prompt
        $a = $typed.Trim().ToLowerInvariant()
        if ($a -eq 'y' -or $a -eq 'yes') { return $true }
        if ($a -eq 'n' -or $a -eq 'no') { return $false }
        Write-Host '  Type Y or N.' -ForegroundColor Yellow
    }
}

function Select-TargetModsFolder {
    if ($SkipConfirm) {
        $saved = Read-ChosenModsFolder
        if ($saved) { return $saved }
        $auto = @(Get-CandidateModsFolders)
        if ($auto.Count -eq 1) {
            Save-ChosenModsFolder $auto[0]
            return $auto[0]
        }
        Write-Log 'ERROR: Need a chosen Mods folder for unattended install.' 'Red'
        return $null
    }

    $remembered = @(Find-RememberedModsFolders)
    if ($remembered.Count -eq 1) {
        Write-Log ("Remembered Mods folder: {0}" -f $remembered[0])
        if (Confirm-ModsFolderChoice $remembered[0]) {
            Save-ChosenModsFolder $remembered[0]
            return $remembered[0]
        }
        Write-Host ''
        Write-Host '  Okay, pick again.' -ForegroundColor Yellow
    }

    while ($true) {
        $candidates = @(Get-CandidateModsFolders)
        Write-Log ("Found {0} 7 Days to Die Mods folder(s)." -f $candidates.Count)

        $chosen = $null
        if ($candidates.Count -eq 0) {
            Write-Host ''
            Write-Host '  Could not find 7 Days to Die automatically.' -ForegroundColor Yellow
            Write-Host '  In Explorer, open the folder that has 7DaysToDie.exe, copy the path,' -ForegroundColor Yellow
            Write-Host '  paste it here, then press Enter.' -ForegroundColor Yellow
            $typed = Read-Host '  Path'
            $chosen = ConvertTo-ModsFolderPath $typed
        } elseif ($candidates.Count -eq 1) {
            Write-Host ''
            Write-Host '  Found this 7 Days to Die:' -ForegroundColor Green
            Write-Host ("    {0}" -f (Get-ModsFolderShortPath $candidates[0]))
            Write-Host ''
            Write-Host '  Press Enter to use it. Or paste a different path.' -ForegroundColor Green
            $typed = Read-Host ' '
            if ($typed.Trim()) {
                $chosen = ConvertTo-ModsFolderPath $typed
            } else {
                $chosen = $candidates[0]
            }
        } else {
            Write-Host ''
            Write-Host '  Found more than one 7 Days to Die. Type the number to use:' -ForegroundColor Yellow
            for ($i = 0; $i -lt $candidates.Count; $i++) {
                Write-Host ("    {0}) {1}" -f ($i + 1), (Get-ModsFolderShortPath $candidates[$i]))
            }
            Write-Host '  Or paste a different game folder path (the one with 7DaysToDie.exe).' -ForegroundColor Yellow
            $typed = Read-Host '  Choice'
            $t = $typed.Trim()
            if ($t -match '^\d+$') {
                $n = [int]$t
                if ($n -ge 1 -and $n -le $candidates.Count) {
                    $chosen = $candidates[$n - 1]
                }
            } else {
                $chosen = ConvertTo-ModsFolderPath $t
            }
        }

        if (-not $chosen) {
            Write-Host ''
            Write-Host '  That path is not a 7 Days to Die game folder or Mods folder. Try again.' -ForegroundColor Red
            continue
        }

        if (Confirm-ModsFolderChoice $chosen) {
            Save-ChosenModsFolder $chosen
            return $chosen
        }
        Write-Host ''
        Write-Host '  Okay, pick again.' -ForegroundColor Yellow
    }
}

function Get-ModsFolderShortPath {
    param([string]$ModsDir)
    $cur = $ModsDir.TrimEnd('\', '/')
    $parts = New-Object System.Collections.Generic.List[string]
    for ($n = 0; $n -lt 4; $n++) {
        if (-not $cur) { break }
        $parts.Insert(0, (Split-Path -Leaf $cur))
        $parent = Split-Path -Parent $cur
        if (-not $parent -or $parent -eq $cur) { break }
        $cur = $parent
    }
    return ($parts -join '\')
}

function Get-TargetKind {
    param([string]$ModsDir)
    $parent = Split-Path -Parent $ModsDir
    if (Test-Path -LiteralPath (Join-Path $parent '7DaysToDie.exe')) { return 'game' }
    if (Test-Path -LiteralPath (Join-Path $parent '7DaysToDieServer.exe')) { return 'dedicated server' }
    if ($ModsDir -like '*\AppData\Roaming\7DaysToDie\Mods') { return 'AppData mods' }
    return 'mods folder'
}

function Get-SourceMods {
    $list = New-Object System.Collections.Generic.List[object]
    $dirs = @(Get-ChildItem -LiteralPath $SourceDir -Directory -ErrorAction SilentlyContinue)
    foreach ($d in $dirs) {
        if ($d.Name -like 'INSTALL*') { continue }
        if ($d.Name.StartsWith('.')) { continue }
        if ($ProtectedMods -contains $d.Name) { continue }
        if (-not (Test-Path -LiteralPath (Join-Path $d.FullName 'ModInfo.xml'))) { continue }
        [void]$list.Add($d)
    }
    return @($list | Sort-Object Name)
}

function Test-CanWrite {
    param([string]$Dir)
    $probe = Join-Path $Dir ('.agf_write_test_{0}' -f [Guid]::NewGuid().ToString('N'))
    try {
        [System.IO.File]::WriteAllText($probe, 'ok')
        Remove-Item -LiteralPath $probe -Force -ErrorAction SilentlyContinue
        return $true
    } catch {
        Remove-Item -LiteralPath $probe -Force -ErrorAction SilentlyContinue
        return $false
    }
}

function Restart-Elevated {
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
}

function Get-ProcessesUsingGameFolder {
    param([string[]]$ModsDirs)
    $names = @('7DaysToDie', '7DaysToDie_EAC', '7DaysToDieDedicated', '7DaysToDieServer')
    $roots = New-Object System.Collections.Generic.List[string]
    foreach ($mods in @($ModsDirs)) {
        if (-not $mods) { continue }
        $game = Split-Path -Parent $mods
        if ($game) { [void]$roots.Add($game.TrimEnd('\', '/').ToLowerInvariant()) }
    }
    if ($roots.Count -eq 0) { return @() }
    $hits = New-Object System.Collections.Generic.List[object]
    $procs = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $names -contains $_.ProcessName })
    foreach ($p in $procs) {
        $exe = $null
        try { $exe = [string]$p.Path } catch { $exe = $null }
        if (-not $exe) { continue }
        $exeDir = Split-Path -Parent $exe
        if (-not $exeDir) { continue }
        $exeDirN = $exeDir.TrimEnd('\', '/').ToLowerInvariant()
        foreach ($root in $roots) {
            if ($exeDirN -eq $root) {
                [void]$hits.Add($p)
                break
            }
        }
    }
    return @($hits.ToArray())
}

function Wait-GameClosed {
    param([string[]]$ModsDirs)
    $running = @(Get-ProcessesUsingGameFolder $ModsDirs)
    if ($running.Count -eq 0) { return }

    if ($SkipConfirm) {
        Write-Log 'WARNING: That 7 Days to Die is still running. Continuing because this is an unattended install.' 'Yellow'
        return
    }

    while ($true) {
        $running = @(Get-ProcessesUsingGameFolder $ModsDirs)
        if ($running.Count -eq 0) { return }
        Write-Host ''
        Write-Host '  That 7 Days to Die is still running. Close it, then press Enter.' -ForegroundColor Yellow
        Write-Host '  (A dedicated server or a different copy can stay open.)' -ForegroundColor DarkGray
        [void](Read-Host)
    }
}

function Get-DeleteTokens {
    param([string]$Raw)
    if (-not $Raw) { return @() }
    return @(
        $Raw.Split(@(';', ','), [StringSplitOptions]::RemoveEmptyEntries) |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ }
    )
}

function Get-DiscordWebhookUrl {
    param([hashtable]$ConfigMap)
    $candidates = New-Object System.Collections.Generic.List[string]
    if (Test-Path -LiteralPath $WebhookFile) {
        Get-Content -LiteralPath $WebhookFile -Encoding UTF8 | ForEach-Object {
            $line = $_.Trim().Trim('"')
            if ($line -ne '' -and -not $line.StartsWith('#')) { [void]$candidates.Add($line) }
        }
    }
    if ($ConfigMap.ContainsKey('DISCORD_WEBHOOK_URL') -and $ConfigMap['DISCORD_WEBHOOK_URL']) {
        [void]$candidates.Add($ConfigMap['DISCORD_WEBHOOK_URL'].Trim().Trim('"'))
    }
    foreach ($url in $candidates) {
        if ($url -match '^https://(?:discord|discordapp)\.com/api/webhooks/\d+/') {
            return $url
        }
    }
    return ''
}

function Send-DiscordInstallReport {
    param([string]$Status)
    if ($script:DryRun -eq 1) { return }
    $url = $script:DiscordWebhook
    if (-not $url) { return }

    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $sent = $false
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

        $modNames = @()
        foreach ($m in @($sourceMods)) { $modNames += $m.Name }
        $targetLines = if (@($targets).Count -gt 0) { $targets -join "`n" } else { '(none found)' }
        $text = @(
            "**Friend mod install: $Status**"
            ('PC: {0}  /  {1}' -f $env:COMPUTERNAME, $env:USERNAME)
            ('Copied: {0}   Removed: {1}   Failed: {2}' -f $copied, $deleted, $failed)
            ('Mods: {0}' -f $(if ($modNames.Count) { $modNames -join ', ' } else { '(none)' }))
            'Target:'
            $targetLines
        ) -join "`n"
        if ($text.Length -gt 1800) { $text = $text.Substring(0, 1800) }

        $curlExe = Join-Path $env:SystemRoot 'System32\curl.exe'
        if ((Test-Path -LiteralPath $curlExe) -and (Test-Path -LiteralPath $LogFile)) {
            $payload = @{ username = 'AGF Friend Installer'; content = $text } | ConvertTo-Json -Compress
            $payloadPath = Join-Path $env:TEMP ('agf-discord-' + [guid]::NewGuid().ToString('N') + '.json')
            $utf8 = New-Object System.Text.UTF8Encoding $false
            [System.IO.File]::WriteAllText($payloadPath, $payload, $utf8)
            $curlArgs = @(
                '-sS', '-f', '--max-time', '20',
                '-H', 'User-Agent: AGF-FriendModInstaller',
                '-F', ('payload_json=<{0}' -f $payloadPath),
                '-F', ('file1=@"{0}";filename=INSTALL-LOG.txt;type=text/plain' -f $LogFile),
                $url
            )
            & $curlExe @curlArgs 2>$null | Out-Null
            if ($LASTEXITCODE -eq 0) { $sent = $true }
            Remove-Item -LiteralPath $payloadPath -Force -ErrorAction SilentlyContinue
        }

        if (-not $sent) {
            $logTail = ''
            if (Test-Path -LiteralPath $LogFile) {
                $raw = [System.IO.File]::ReadAllText($LogFile)
                if ($raw.Length -gt 1100) { $raw = $raw.Substring($raw.Length - 1100) }
                $logTail = "``````{0}``````" -f $raw
            }
            $content = $text + "`n" + $logTail
            if ($content.Length -gt 1900) { $content = $content.Substring(0, 1900) }
            $body = @{ username = 'AGF Friend Installer'; content = $content } | ConvertTo-Json
            Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType 'application/json; charset=utf-8' -Headers @{ 'User-Agent' = 'AGF-FriendModInstaller' } -TimeoutSec 15 | Out-Null
            $sent = $true
        }
    } catch {
        $sent = $false
    } finally {
        $ErrorActionPreference = $prevEap
    }

    if ($sent) {
        Write-Host '  A short install report was sent to AGF.' -ForegroundColor DarkGray
    } else {
        Write-Host '  Could not send the install report to AGF. That is OK - the install itself is unchanged.' -ForegroundColor DarkGray
    }
}

function Complete-Run {
    param(
        [int]$Code,
        [string]$DiscordStatus = '',
        [switch]$SendDiscord
    )
    if ($SendDiscord -and $DiscordStatus) {
        Send-DiscordInstallReport -Status $DiscordStatus
    }
    if ($PauseWhenDone) {
        Write-Host ''
        Write-Host '  Press Enter to close.'
        [void](Read-Host)
    }
    exit $Code
}

# --- start ---
'' | Set-Content -LiteralPath $LogFile -Encoding UTF8

$config = Read-ConfigFile
if (-not $MarkerMod -and $config.ContainsKey('MARKER_MOD')) { $MarkerMod = $config['MARKER_MOD'] }
if (-not $DeleteMods -and $config.ContainsKey('DELETE_MODS')) { $DeleteMods = $config['DELETE_MODS'] }
if ($DryRun -lt 0) {
    if ($config.ContainsKey('DRY_RUN') -and $config['DRY_RUN'] -eq '1') { $DryRun = 1 } else { $DryRun = 0 }
}
if (-not $ForceModsPath -and $config.ContainsKey('FORCE_MODS_PATH')) { $ForceModsPath = $config['FORCE_MODS_PATH'] }
if (-not $ForceModsPath -and (Test-Path -LiteralPath $ManualPathFile)) {
    $line = (Get-Content -LiteralPath $ManualPathFile -Encoding UTF8 |
            Where-Object { $_.Trim() -ne '' -and -not $_.Trim().StartsWith('#') } |
            Select-Object -First 1)
    if ($line) { $ForceModsPath = $line.Trim().Trim('"') }
}

$script:MarkerMod = $MarkerMod
$script:DeleteMods = $DeleteMods
$script:DryRun = $DryRun
$script:ForceModsPath = $ForceModsPath
$script:DiscordWebhook = Get-DiscordWebhookUrl -ConfigMap $config

$oneDriveUrl = ''
if (Get-Command 'Get-OneDriveShareUrlFromConfig' -ErrorAction SilentlyContinue) {
    $oneDriveUrl = Get-OneDriveShareUrlFromConfig -ConfigMap $config
}
$removeNotInCloud = [bool]$oneDriveUrl
if ($config.ContainsKey('REMOVE_NOT_IN_CLOUD')) {
    $removeNotInCloud = ($config['REMOVE_NOT_IN_CLOUD'] -eq '1')
}
if ($null -ne $env:REMOVE_NOT_IN_CLOUD -and $env:REMOVE_NOT_IN_CLOUD -ne '') {
    if ($env:REMOVE_NOT_IN_CLOUD -eq '1') { $removeNotInCloud = $true }
    if ($env:REMOVE_NOT_IN_CLOUD -eq '0') { $removeNotInCloud = $false }
}
$odSession = $null
$remoteMods = @()
$needDownload = New-Object System.Collections.Generic.List[object]
$alreadyOk = New-Object System.Collections.Generic.List[object]
$odTemp = Join-Path $env:TEMP 'AGF-Share-Pack'

Write-Host ''
Write-Host '  7 Days to Die  -  Install / Update Mods' -ForegroundColor Cyan
Write-Host '  ========================================' -ForegroundColor Cyan
Write-Host ''
Write-Log ("Source folder: {0}" -f $SourceDir)

if ($oneDriveUrl) {
    $modsTarget = Select-TargetModsFolder
    if (-not $modsTarget) {
        Complete-Run -Code 1
    }
    Write-Log ("Using this Mods folder: {0}" -f $modsTarget)
    Write-Log 'Reading the shared pack (no login required).'
    try {
        $pack = Get-RemoteModsFromShareUrl -ShareUrl $oneDriveUrl
        $odSession = $pack.Session
        $remoteMods = @($pack.Mods)
        if ($pack.Kind -eq 'onedrive' -and $pack.Name) {
            Write-Log ("Shared folder name: {0}" -f $pack.Name)
        }
        if ($pack.Kind -eq 'gdrive' -and $pack.Name) {
            Write-Log ("Shared folder name: {0}" -f $pack.Name)
        }
    } catch {
        Write-Log ("ERROR: Could not read the shared pack. {0}" -f $_.Exception.Message) 'Red'
        Write-Log 'Use a view-only folder share, or a direct .zip of the mod folders. Players do not log in.' 'Red'
        Complete-Run -Code 1 -DiscordStatus 'PACK FAILED' -SendDiscord
    }
    if ($remoteMods.Count -eq 0) {
        Write-Log 'ERROR: The shared pack has no mod folders with ModInfo.xml.' 'Red'
        Complete-Run -Code 1 -DiscordStatus 'PACK EMPTY' -SendDiscord
    }
    Write-Log ("Pack mods: {0}" -f ($remoteMods.Name -join ', '))
    $localHarmony = Join-Path $modsTarget '0_TFP_Harmony'
    $remoteHasHarmony = $false
    foreach ($r in $remoteMods) {
        if (Test-IsHarmonyFolderName $r.Name) { $remoteHasHarmony = $true; break }
    }
    $localHasHarmony = (Test-Path -LiteralPath (Join-Path $localHarmony 'ModInfo.xml')) -or
        (Test-Path -LiteralPath (Join-Path $localHarmony '0Harmony.dll'))
    if (-not $localHasHarmony -and -not $remoteHasHarmony) {
        Write-Log 'WARNING: 0_TFP_Harmony is missing here and is not in the shared folder. Add it to the shared pack, or use Steam Verify.' 'Yellow'
    }
    $targets = @($modsTarget)
    foreach ($r in $remoteMods) {
        if (Test-LocalModIsCurrent -ModsDir $modsTarget -RemoteMod $r) {
            [void]$alreadyOk.Add($r)
        } else {
            if (Test-IsHarmonyFolderName $r.Name) {
                Write-Log '0_TFP_Harmony is missing locally. Will copy it from the shared folder.' 'Yellow'
            }
            [void]$needDownload.Add($r)
        }
    }
    $sourceMods = $remoteMods
} else {
    if (-not $MarkerMod) {
        Write-Log 'ERROR: MARKER_MOD is empty. Set it in INSTALL-CONFIG.txt' 'Red'
        Complete-Run -Code 1
    }

    $sourceMods = @(Get-SourceMods)
    if ($sourceMods.Count -eq 0) {
        Write-Log 'ERROR: No mod folders found next to this installer, and no shared pack link is set.' 'Red'
        Write-Log 'Each mod must be a folder that contains ModInfo.xml, or set a shared pack link.' 'Red'
        Complete-Run -Code 1
    }
    Write-Log ("Mods to install: {0}" -f ($sourceMods.Name -join ', '))

    $targets = @()
    if ($ForceModsPath) {
        if (-not (Test-Path -LiteralPath $ForceModsPath)) {
            Write-Log ("ERROR: FORCE / MY-MODS-FOLDER path does not exist: {0}" -f $ForceModsPath) 'Red'
            Complete-Run -Code 1 -DiscordStatus 'PATH MISSING' -SendDiscord
        }
        $targets = @((Resolve-Path -LiteralPath $ForceModsPath).Path)
        Write-Log ("Using forced Mods path: {0}" -f $targets[0])
        if (-not (Test-HasMarker $targets[0] $MarkerMod)) {
            Write-Log ("WARNING: That folder does not contain marker '{0}'. Installing anyway." -f $MarkerMod) 'Yellow'
        }
    } else {
        $candidates = @(Get-CandidateModsFolders)
        Write-Log ("Checked {0} possible Mods folder(s)." -f $candidates.Count)
        $targets = @(Get-UniquePaths @($candidates | Where-Object { Test-HasMarker $_ $MarkerMod }))
    }

    if ($targets.Count -eq 0) {
        Write-Host ''
        Write-Log ("ERROR: Could not find a 7 Days to Die Mods folder that already has '{0}'." -f $MarkerMod) 'Red'
        Write-Host ''
        Write-Host '  That marker is how this installer knows it is YOUR game, not a' -ForegroundColor Yellow
        Write-Host '  different copy or overhaul.' -ForegroundColor Yellow
        Write-Host ''
        Write-Host '  Or put __AGF_CLIENT_BAT_FILE__ in the Mods folder and use a shared pack link.' -ForegroundColor Yellow
        Write-Host ''
        Complete-Run -Code 1 -DiscordStatus 'GAME NOT FOUND' -SendDiscord
    }
}

Write-Host ''
Write-Host '  Found your 7 Days to Die here:' -ForegroundColor Green
foreach ($t in $targets) {
    Write-Host ("    {0}" -f (Get-ModsFolderShortPath $t))
    Write-Log ("Target: {0}" -f $t)
}

$sameDir = @($targets | Where-Object { $_.TrimEnd('\', '/') -eq $SourceDir.TrimEnd('\', '/') })
$copyMods = $sourceMods
if ($oneDriveUrl) {
    $copyMods = @()
} elseif ($sameDir.Count -eq $targets.Count -and $targets.Count -gt 0) {
    Write-Log 'Installer is already inside the Mods folder. Will replace old versions only (no copy).' 'Yellow'
    $copyMods = @()
}

$planDeletes = New-Object System.Collections.Generic.List[string]
$planCopies = New-Object System.Collections.Generic.List[string]
$deleteTokens = @(Get-DeleteTokens $DeleteMods)

foreach ($destMods in $targets) {
    $existing = @(Get-ChildItem -LiteralPath $destMods -Directory -ErrorAction SilentlyContinue)

    foreach ($src in $sourceMods) {
        $family = Get-ModFamily $src.Name
        foreach ($old in $existing) {
            if ($old.Name -eq $src.Name) { continue }
            if ((Get-ModFamily $old.Name) -eq $family) {
                $path = Join-Path $destMods $old.Name
                if (-not $planDeletes.Contains($path)) { [void]$planDeletes.Add($path) }
            }
        }
        if ($copyMods.Count -gt 0) {
            [void]$planCopies.Add(('{0}  ->  {1}' -f $src.Name, (Join-Path $destMods $src.Name)))
        }
    }

    foreach ($token in $deleteTokens) {
        if ($ProtectedMods | Where-Object { $token.StartsWith($_, [StringComparison]::OrdinalIgnoreCase) }) {
            Write-Log ("Skipping protected mod delete: {0}" -f $token) 'Yellow'
            continue
        }
        foreach ($old in $existing) {
            $isIncoming = @($sourceMods | Where-Object { $_.Name -eq $old.Name }).Count -gt 0
            if ($isIncoming) { continue }
            if ($old.Name.StartsWith($token, [StringComparison]::OrdinalIgnoreCase)) {
                $path = Join-Path $destMods $old.Name
                if (-not $planDeletes.Contains($path)) { [void]$planDeletes.Add($path) }
            }
        }
    }

    if ($oneDriveUrl -and $removeNotInCloud) {
        $remoteNames = @{}
        foreach ($r in $remoteMods) { $remoteNames[$r.Name.ToLowerInvariant()] = $true }
        foreach ($old in $existing) {
            if (Test-IsProtectedLocalName $old.Name) { continue }
            if ($remoteNames.ContainsKey($old.Name.ToLowerInvariant())) { continue }
            $path = Join-Path $destMods $old.Name
            if (-not $planDeletes.Contains($path)) { [void]$planDeletes.Add($path) }
        }
        $looseFiles = @(Get-ChildItem -LiteralPath $destMods -File -ErrorAction SilentlyContinue)
        foreach ($f in $looseFiles) {
            if (Test-IsProtectedLocalName $f.Name) { continue }
            if (-not $planDeletes.Contains($f.FullName)) { [void]$planDeletes.Add($f.FullName) }
        }
    }
}

Write-Host ''
if ($oneDriveUrl) {
    if ($alreadyOk.Count -gt 0) {
        Write-Host ("  Already up to date ({0}):" -f $alreadyOk.Count) -ForegroundColor Gray
        foreach ($s in $alreadyOk) { Write-Host ("    = {0}" -f $s.Name) }
    }
    if ($needDownload.Count -gt 0) {
        Write-Host ''
        Write-Host ("  Will download / update {0} mod(s):" -f $needDownload.Count) -ForegroundColor Cyan
        foreach ($s in $needDownload) { Write-Host ("    + {0}" -f $s.Name) }
    } elseif ($alreadyOk.Count -gt 0) {
        Write-Host ''
        Write-Host '  Shared pack already matches this Mods folder.' -ForegroundColor Green
    }
} else {
    Write-Host ("  Will install / update {0} mod(s):" -f $sourceMods.Count) -ForegroundColor Cyan
    foreach ($s in $sourceMods) {
        Write-Host ("    + {0}" -f $s.Name)
    }
}

if ($planDeletes.Count -gt 0) {
    Write-Host ''
    Write-Host '  Will remove extra folders / files not in the shared pack:' -ForegroundColor Yellow
    foreach ($p in $planDeletes) {
        Write-Host ("    - {0}" -f (Split-Path -Leaf $p))
    }
} else {
    Write-Host ''
    Write-Host '  No old mod folders to remove.' -ForegroundColor Gray
}

if ($DryRun -eq 1) {
    Write-Host ''
    Write-Log 'DRY_RUN=1  Nothing was changed.' 'Yellow'
    Complete-Run -Code 0
}

if (-not $SkipConfirm) {
    Write-Host ''
    if ($script:DiscordWebhook) {
        Write-Host '  A short report (PC name + install log) will be sent to AGF.' -ForegroundColor DarkGray
    }
    Write-Host '  Press Enter to sync. Close this window to cancel.' -ForegroundColor Green
    [void](Read-Host)
}

Wait-GameClosed -ModsDirs $targets | Out-Null

$needAdmin = $false
foreach ($destMods in $targets) {
    if (-not (Test-CanWrite $destMods)) { $needAdmin = $true }
}
if ($needAdmin) {
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $isAdmin) {
        Write-Log 'Need administrator permission to write into the Mods folder. A prompt will appear.' 'Yellow'
        Restart-Elevated
        exit 0
    }
}

foreach ($destMods in $targets) {
    Save-ChosenModsFolder $destMods
}

$copied = 0
$deleted = 0
$failed = 0

if ($oneDriveUrl -and $needDownload.Count -gt 0) {
    if (Test-Path -LiteralPath $odTemp) {
        Remove-Item -LiteralPath $odTemp -Recurse -Force
    }
    New-Item -ItemType Directory -Path $odTemp | Out-Null
    $oldProgress = $ProgressPreference
    $ProgressPreference = 'SilentlyContinue'
    $dlIndex = 0
    foreach ($r in $needDownload) {
        $dlIndex++
        Write-Log ("Downloading {0}  ({1} of {2}) ..." -f $r.Name, $dlIndex, $needDownload.Count)
        try {
            if (($r.PSObject.Properties.Name -contains 'LocalPath') -and $r.LocalPath) {
                $dest = Join-Path $odTemp $r.Name
                if (Test-Path -LiteralPath $dest) {
                    Remove-Item -LiteralPath $dest -Recurse -Force
                }
                Copy-Item -LiteralPath $r.LocalPath -Destination $dest -Recurse -Force
            } elseif (($r.PSObject.Properties.Name -contains 'Kind') -and ($r.Kind -eq 'gdrive')) {
                Save-GDriveModFolder -RemoteMod $r -DestParent $odTemp
            } else {
                Save-OneDriveModFolder -RemoteMod $r -DestParent $odTemp -Token $odSession.Token
            }
        } catch {
            Write-Log ("FAILED to download {0}  ({1})" -f $r.Name, $_.Exception.Message) 'Red'
            $failed++
        }
    }
    $ProgressPreference = $oldProgress
    $copyMods = @(Get-ChildItem -LiteralPath $odTemp -Directory -ErrorAction SilentlyContinue)
}

foreach ($path in $planDeletes) {
    try {
        if (Test-Path -LiteralPath $path) {
            Remove-Item -LiteralPath $path -Recurse -Force
            Write-Log ("Removed  {0}" -f $path) 'Yellow'
            $deleted++
        }
    } catch {
        Write-Log ("FAILED to remove {0}  ({1})" -f $path, $_.Exception.Message) 'Red'
        $failed++
    }
}

foreach ($destMods in $targets) {
    if ($copyMods.Count -eq 0) { continue }
    $destNorm = $destMods.TrimEnd('\', '/')
    $srcNorm = $SourceDir.TrimEnd('\', '/')
    if (-not $oneDriveUrl -and $destNorm -eq $srcNorm) { continue }

    foreach ($src in $copyMods) {
        $destFolder = Join-Path $destMods $src.Name
        try {
            if (Test-Path -LiteralPath $destFolder) {
                Remove-Item -LiteralPath $destFolder -Recurse -Force
            }
            Copy-Item -LiteralPath $src.FullName -Destination $destFolder -Recurse -Force
            Write-Log ("Copied   {0}" -f $src.Name) 'Green'
            $copied++
        } catch {
            Write-Log ("FAILED to copy {0}  ({1})" -f $src.Name, $_.Exception.Message) 'Red'
            $failed++
        }
    }
}

Write-Host ''
if ($failed -gt 0) {
    Write-Host '  Finished with errors. See INSTALL-LOG.txt in this folder.' -ForegroundColor Red
    Write-Log ("Done with errors. Copied={0} Removed={1} Failed={2}" -f $copied, $deleted, $failed) 'Red'
    Complete-Run -Code 1 -DiscordStatus 'FAILED' -SendDiscord
}

Write-Host '  Done. You can start 7 Days to Die.' -ForegroundColor Green
Write-Log ("Done. Copied={0} Removed={1} Failed=0" -f $copied, $deleted) 'Green'
Complete-Run -Code 0 -DiscordStatus 'OK' -SendDiscord
