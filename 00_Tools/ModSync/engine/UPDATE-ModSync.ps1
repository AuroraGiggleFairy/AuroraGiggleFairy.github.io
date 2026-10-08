# UpdateModSync. Embedded in Output\UpdateModSync.bat by generate.py --update.
# Finds every 7 Days to Die install that already has ModSync. If that copy is
# older than the one packed in the bat, it replaces it and removes the old folder.
# No questions. The window closes when this script ends.
$ErrorActionPreference = 'Stop'

$BatPath = $env:MODSYNC_BAT_PATH
$TargetName = $env:MODSYNC_MOD_FOLDER
if (-not $TargetName) { $TargetName = 'AGF-NoEAC-ModSync-v0.1.2' }

function Expand-EmbeddedMod {
    if (-not $BatPath -or -not (Test-Path -LiteralPath $BatPath)) {
        throw 'Could not read UpdateModSync.'
    }

    $work = Join-Path $env:TEMP ('UpdateModSync-' + [IO.Path]::GetRandomFileName())
    New-Item -ItemType Directory -Path $work -Force | Out-Null

    $parts = New-Object System.Text.StringBuilder
    foreach ($line in [IO.File]::ReadLines($BatPath)) {
        if ($line.StartsWith('::B64::')) { [void]$parts.Append($line.Substring(7)) }
    }
    if ($parts.Length -eq 0) { throw 'UpdateModSync is incomplete.' }

    $zip = Join-Path $work 'mod.zip'
    [IO.File]::WriteAllBytes($zip, [Convert]::FromBase64String($parts.ToString()))

    $out = Join-Path $work 'mod'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($zip, $out)
    Remove-Item -LiteralPath $zip -Force

    if (-not (Test-Path -LiteralPath (Join-Path $out 'ModSync.dll'))) {
        throw 'UpdateModSync is damaged.'
    }
    return $out
}

function ConvertTo-ModVersion {
    param([string]$Text)
    if (-not $Text) { return [version]'0.0.0' }
    $match = [regex]::Match($Text, '(\d+)\.(\d+)\.(\d+)')
    if ($match.Success) { return [version]$match.Value }
    $match = [regex]::Match($Text, '(\d+)\.(\d+)')
    if ($match.Success) { return [version]($match.Value + '.0') }
    return [version]'0.0.0'
}

function Get-ModVersion {
    param([string]$Folder)
    $info = Join-Path $Folder 'ModInfo.xml'
    if (Test-Path -LiteralPath $info) {
        $text = [IO.File]::ReadAllText($info)
        $match = [regex]::Match($text, 'Version\s+value\s*=\s*"([^"]+)"')
        if ($match.Success) { return ConvertTo-ModVersion $match.Groups[1].Value }
    }
    return ConvertTo-ModVersion (Split-Path -Leaf $Folder)
}

function Test-FileLocked {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return $false }
    try {
        $stream = [IO.File]::Open($Path, 'Open', 'ReadWrite', 'None')
        $stream.Close()
        return $false
    } catch {
        return $true
    }
}

function Test-GameDir {
    param([string]$Dir)
    if (-not $Dir) { return $false }
    foreach ($exe in @('7DaysToDie.exe', '7DaysToDieServer.exe')) {
        if (Test-Path -LiteralPath (Join-Path $Dir $exe)) { return $true }
    }
    return $false
}

function Add-GameDir {
    param($List, [string]$Dir)
    if (-not (Test-GameDir $Dir)) { return }
    try { $Dir = (Resolve-Path -LiteralPath $Dir).Path } catch { return }
    foreach ($have in $List) {
        if ([string]$have -eq $Dir) { return }
    }
    [void]$List.Add($Dir)
}

function Add-UniquePath {
    param($List, [string]$Path)
    if (-not $Path) { return }
    $Path = $Path.Trim().Trim('"').Replace('/', '\').TrimEnd('\')
    if (-not $Path) { return }
    foreach ($have in $List) {
        if ([string]$have -eq $Path) { return }
    }
    [void]$List.Add($Path)
}

function Get-SteamLibraries {
    $roots = New-Object System.Collections.Generic.List[string]
    foreach ($probe in @(
            @{ Key = 'HKCU:\Software\Valve\Steam'; Name = 'SteamPath' }
            @{ Key = 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam'; Name = 'InstallPath' }
            @{ Key = 'HKLM:\SOFTWARE\Valve\Steam'; Name = 'InstallPath' }
        )) {
        try {
            $value = (Get-ItemProperty -Path $probe.Key -ErrorAction Stop).($probe.Name)
            if ($value) { Add-UniquePath $roots ($value.Replace('/', '\')) }
        } catch { }
    }
    foreach ($guess in @(
            (Join-Path ${env:ProgramFiles(x86)} 'Steam'),
            (Join-Path $env:ProgramFiles 'Steam')
        )) {
        Add-UniquePath $roots $guess
    }

    $libs = New-Object System.Collections.Generic.List[string]
    foreach ($root in $roots) {
        Add-UniquePath $libs $root
        $sources = @(
            @{ File = (Join-Path $root 'steamapps\libraryfolders.vdf'); Patterns = @('"path"\s*"([^"]+)"', '"\d+"\s*"([A-Za-z]:[^"]+)"') }
            @{ File = (Join-Path $root 'config\config.vdf'); Patterns = @('"BaseInstallFolder_\d+"\s*"([^"]+)"') }
        )
        foreach ($source in $sources) {
            if (-not (Test-Path -LiteralPath $source.File)) { continue }
            try {
                $text = [IO.File]::ReadAllText($source.File)
                foreach ($pattern in $source.Patterns) {
                    foreach ($match in [regex]::Matches($text, $pattern)) {
                        Add-UniquePath $libs ($match.Groups[1].Value.Replace('\\', '\'))
                    }
                }
            } catch { }
        }
    }
    return $libs
}

function Get-GameDirs {
    $games = New-Object System.Collections.Generic.List[string]
    foreach ($lib in (Get-SteamLibraries)) {
        $common = Join-Path $lib 'steamapps\common'
        if (-not (Test-Path -LiteralPath $common)) { continue }
        foreach ($dir in @(Get-ChildItem -LiteralPath $common -Directory -ErrorAction SilentlyContinue)) {
            Add-GameDir $games $dir.FullName
        }
    }

    $remember = Join-Path $env:LOCALAPPDATA 'AGF-ModSync'
    if (Test-Path -LiteralPath $remember) {
        foreach ($file in @(Get-ChildItem -LiteralPath $remember -Filter *.txt -File -ErrorAction SilentlyContinue)) {
            foreach ($line in @(Get-Content -LiteralPath $file.FullName -ErrorAction SilentlyContinue)) {
                $text = $line.Trim().Trim('"')
                if ($text -and -not $text.StartsWith('#')) { Add-GameDir $games $text }
            }
        }
    }

    $desktops = @(
        [Environment]::GetFolderPath('Desktop'),
        (Join-Path $env:USERPROFILE 'OneDrive\Desktop'),
        (Join-Path $env:PUBLIC 'Desktop')
    )
    $shell = $null
    foreach ($desk in $desktops) {
        if (-not $desk -or -not (Test-Path -LiteralPath $desk)) { continue }
        foreach ($lnk in @(Get-ChildItem -LiteralPath $desk -Filter *.lnk -File -ErrorAction SilentlyContinue)) {
            try {
                if (-not $shell) { $shell = New-Object -ComObject WScript.Shell }
                $target = $shell.CreateShortcut($lnk.FullName).TargetPath
                if ($target -match '7DaysToDie(Server)?\.exe$') {
                    Add-GameDir $games (Split-Path -Parent $target)
                }
            } catch { }
        }
    }
    return @($games)
}

function Update-ModsFolder {
    param(
        [string]$ModsDir,
        [string]$Source,
        [string]$TargetName,
        $TargetVersion,
        [string]$SourceHash
    )

    $prefix = 'AGF-NoEAC-ModSync'
    if (-not (Test-Path -LiteralPath $ModsDir)) { return 'none' }
    $folders = @(Get-ChildItem -LiteralPath $ModsDir -Directory -ErrorAction SilentlyContinue | Where-Object {
            $_.Name.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)
        })
    if ($folders.Count -eq 0) { return 'none' }

    $older = New-Object System.Collections.Generic.List[object]
    $newer = 0
    foreach ($folder in $folders) {
        $cmp = (Get-ModVersion $folder.FullName).CompareTo($TargetVersion)
        if ($cmp -lt 0) { [void]$older.Add($folder) }
        elseif ($cmp -gt 0) { $newer++ }
    }
    if ($newer -gt 0) {
        foreach ($folder in $older) {
            if (Test-FileLocked (Join-Path $folder.FullName 'ModSync.dll')) { return 'locked' }
            try {
                Remove-Item -LiteralPath $folder.FullName -Recurse -Force -ErrorAction Stop
            } catch {
                return 'locked'
            }
        }
        return 'newer'
    }
    if ($older.Count -eq 0) { return 'current' }

    $dest = Join-Path $ModsDir $TargetName
    foreach ($folder in $older) {
        if (Test-FileLocked (Join-Path $folder.FullName 'ModSync.dll')) { return 'locked' }
    }
    $destDll = Join-Path $dest 'ModSync.dll'
    if ((Test-Path -LiteralPath $destDll) -and (Test-FileLocked $destDll)) { return 'locked' }

    $destExisted = Test-Path -LiteralPath $dest
    New-Item -ItemType Directory -Path $dest -Force | Out-Null
    & robocopy $Source $dest /E /R:1 /W:1 /NFL /NDL /NP /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) {
        if (-not $destExisted) { Remove-Item -LiteralPath $dest -Recurse -Force -ErrorAction SilentlyContinue }
        return 'copyfail'
    }
    if (-not (Test-Path -LiteralPath $destDll)) { return 'copyfail' }
    $destHash = (Get-FileHash -LiteralPath $destDll -Algorithm SHA256).Hash
    if ($destHash -ne $SourceHash) {
        if (-not $destExisted) { Remove-Item -LiteralPath $dest -Recurse -Force -ErrorAction SilentlyContinue }
        return 'copyfail'
    }

    $destFull = (Resolve-Path -LiteralPath $dest).Path
    foreach ($folder in $older) {
        $full = (Resolve-Path -LiteralPath $folder.FullName).Path
        if ($full -eq $destFull) { continue }
        try {
            Remove-Item -LiteralPath $full -Recurse -Force -ErrorAction Stop
        } catch {
            return 'locked'
        }
    }
    return 'updated'
}

if ($BatPath) {

$source = Expand-EmbeddedMod
$work = Split-Path -Parent $source
try {
    $targetVersion = Get-ModVersion $source
    $sourceHash = (Get-FileHash -LiteralPath (Join-Path $source 'ModSync.dll') -Algorithm SHA256).Hash
    $updated = 0
    $current = 0
    $failed = 0
    foreach ($game in @(Get-GameDirs)) {
        $result = Update-ModsFolder (Join-Path $game 'Mods') $source $TargetName $targetVersion $sourceHash
        switch ($result) {
            'updated' { $updated++ }
            'current' { $current++ }
            'newer' { $current++ }
            'locked' { $failed++ }
            'copyfail' { $failed++ }
        }
    }
    Write-Host ''
    if (($updated + $current + $failed) -eq 0) {
        Write-Host 'No ModSync was found.'
    } else {
        Write-Host "Updated $updated."
        if ($current -gt 0) { Write-Host "Already up to date: $current." }
        if ($failed -gt 0) { Write-Host "Could not update $failed. Close 7 Days to Die and run this again." }
    }
} finally {
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
}

}
