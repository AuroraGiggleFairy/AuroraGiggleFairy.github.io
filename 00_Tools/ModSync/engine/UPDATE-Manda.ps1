# Update Manda. Embedded in Output\UpdateManda.bat by generate.py --manda.
# Uses the folder the player already picked for Manda.
# A separate copy that is still below 3.3 is brought up from their Steam install
# (game files and Harmony only). Then the packed ModSync is installed.
$ErrorActionPreference = 'Stop'

$BatPath = $env:MODSYNC_BAT_PATH
$TargetName = $env:MODSYNC_MOD_FOLDER
if (-not $TargetName) { $TargetName = 'AGF-NoEAC-ModSync-v0.1.2' }
$ServerName = $env:MODSYNC_SERVER_NAME
if (-not $ServerName) { $ServerName = 'MandaServer' }
$CopyFolderName = $env:MODSYNC_COPY_FOLDER
if (-not $CopyFolderName) { $CopyFolderName = '7 Days to Die - MandaServer' }
$Game33 = [version]'1.330.0.0'

function Expand-EmbeddedMod {
    if (-not $BatPath -or -not (Test-Path -LiteralPath $BatPath)) {
        throw 'Could not read Update Manda.'
    }

    $work = Join-Path $env:TEMP ('UpdateManda-' + [IO.Path]::GetRandomFileName())
    New-Item -ItemType Directory -Path $work -Force | Out-Null

    $parts = New-Object System.Text.StringBuilder
    foreach ($line in [IO.File]::ReadLines($BatPath)) {
        if ($line.StartsWith('::B64::')) { [void]$parts.Append($line.Substring(7)) }
    }
    if ($parts.Length -eq 0) { throw 'Update Manda is incomplete.' }

    $zip = Join-Path $work 'mod.zip'
    [IO.File]::WriteAllBytes($zip, [Convert]::FromBase64String($parts.ToString()))

    $out = Join-Path $work 'mod'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($zip, $out)
    Remove-Item -LiteralPath $zip -Force

    if (-not (Test-Path -LiteralPath (Join-Path $out 'ModSync.dll'))) {
        throw 'Update Manda is damaged.'
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

function Get-GameRelease {
    param([string]$GameDir)
    $config = Join-Path $GameDir 'MicrosoftGame.Config'
    if (-not (Test-Path -LiteralPath $config)) { return $null }
    $text = [IO.File]::ReadAllText($config)
    $match = [regex]::Match($text, '<Identity[^>]*Version="([^"]+)"')
    if (-not $match.Success) { return $null }
    try { return [version]$match.Groups[1].Value } catch { return $null }
}

function Test-IsGame33 {
    param($Release)
    if (-not $Release) { return $false }
    return ($Release -ge $Game33)
}

function Test-GameDir {
    param([string]$Dir)
    if (-not $Dir) { return $false }
    return (Test-Path -LiteralPath (Join-Path $Dir '7DaysToDie.exe'))
}

function Test-SameFolder {
    param([string]$Left, [string]$Right)
    if (-not $Left -or -not $Right) { return $false }
    $a = [IO.Path]::GetFullPath($Left).TrimEnd('\')
    $b = [IO.Path]::GetFullPath($Right).TrimEnd('\')
    return ($a -eq $b)
}

function Test-GameRunning {
    param([string]$GameDir)
    if (-not $GameDir) { return $false }
    $root = [IO.Path]::GetFullPath($GameDir).TrimEnd('\') + '\'
    foreach ($name in @('7DaysToDie', '7DaysToDie_EAC', '7dLauncher')) {
        foreach ($proc in @(Get-Process -Name $name -ErrorAction SilentlyContinue)) {
            try {
                $path = $proc.Path
                if ($path -and $path.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) { return $true }
            } catch { }
        }
    }
    return $false
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

function Get-SteamGameFolder {
    foreach ($lib in (Get-SteamLibraries)) {
        $manifest = Join-Path $lib 'steamapps\appmanifest_251570.acf'
        if (-not (Test-Path -LiteralPath $manifest)) { continue }
        try {
            $text = [IO.File]::ReadAllText($manifest)
            $install = [regex]::Match($text, '"installdir"\s*"([^"]+)"').Groups[1].Value
            if (-not $install) { continue }
            $game = Join-Path $lib ("steamapps\common\" + $install)
            if (Test-GameDir $game) { return (Resolve-Path -LiteralPath $game).Path }
        } catch { }
    }
    foreach ($lib in (Get-SteamLibraries)) {
        foreach ($name in @('7 Days To Die', '7 Days to Die')) {
            $game = Join-Path $lib ("steamapps\common\" + $name)
            if (Test-GameDir $game) { return (Resolve-Path -LiteralPath $game).Path }
        }
    }
    return $null
}

function Get-RememberedManda {
    $safe = ($ServerName -replace '[^A-Za-z0-9]', '')
    $file = Join-Path $env:LOCALAPPDATA ('AGF-ModSync\' + $safe + '.txt')
    if (-not (Test-Path -LiteralPath $file)) { return $null }
    foreach ($line in @(Get-Content -LiteralPath $file -ErrorAction SilentlyContinue)) {
        $text = $line.Trim().Trim('"')
        if ($text -and -not $text.StartsWith('#') -and (Test-GameDir $text)) {
            return (Resolve-Path -LiteralPath $text).Path
        }
    }
    return $null
}

function Get-MandaFolder {
    param([string]$SteamGame)
    $remembered = Get-RememberedManda
    if ($remembered) { return $remembered }

    if ($SteamGame) {
        $sibling = Join-Path (Split-Path -Parent $SteamGame) $CopyFolderName
        if (Test-GameDir $sibling) { return (Resolve-Path -LiteralPath $sibling).Path }
    }

    $desktops = @(
        [Environment]::GetFolderPath('Desktop'),
        (Join-Path $env:USERPROFILE 'OneDrive\Desktop'),
        (Join-Path $env:PUBLIC 'Desktop')
    )
    $shell = $null
    foreach ($desk in $desktops) {
        if (-not $desk -or -not (Test-Path -LiteralPath $desk)) { continue }
        $lnk = Join-Path $desk ($ServerName + '.lnk')
        if (-not (Test-Path -LiteralPath $lnk)) { continue }
        try {
            if (-not $shell) { $shell = New-Object -ComObject WScript.Shell }
            $target = $shell.CreateShortcut($lnk).TargetPath
            if ($target -match '7DaysToDie\.exe$') {
                $dir = Split-Path -Parent $target
                if (Test-GameDir $dir) { return (Resolve-Path -LiteralPath $dir).Path }
            }
        } catch { }
    }
    return $null
}

function Copy-GameFromSteam {
    param([string]$SteamGame, [string]$MandaGame)
    Write-Host ''
    Write-Host '  Updating the Manda game from Steam. This can take a while.'
    & robocopy $SteamGame $MandaGame /E /R:1 /W:1 /XD Mods logs /NFL /NDL /NP /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) { return $false }

    $harmony = Join-Path $SteamGame 'Mods\0_TFP_Harmony'
    if (Test-Path -LiteralPath $harmony) {
        $dest = Join-Path $MandaGame 'Mods\0_TFP_Harmony'
        New-Item -ItemType Directory -Path $dest -Force | Out-Null
        & robocopy $harmony $dest /E /R:1 /W:1 /NFL /NDL /NP /NJH /NJS | Out-Null
        if ($LASTEXITCODE -ge 8) { return $false }
    }
    return $true
}

function Remove-OlderModSync {
    param([string]$ModsDir, [string]$KeepFull, $TargetVersion)
    $prefix = 'AGF-NoEAC-ModSync'
    foreach ($folder in @(Get-ChildItem -LiteralPath $ModsDir -Directory -ErrorAction SilentlyContinue)) {
        if (-not $folder.Name.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { continue }
        $full = (Resolve-Path -LiteralPath $folder.FullName).Path
        if ($KeepFull -and ($full -eq $KeepFull)) { continue }
        $cmp = (Get-ModVersion $full).CompareTo($TargetVersion)
        if ($cmp -gt 0) { continue }
        Remove-Item -LiteralPath $full -Recurse -Force
    }
}

function Install-LatestModSync {
    param(
        [string]$ModsDir,
        [string]$Source,
        [string]$FolderName,
        $TargetVersion,
        [string]$SourceHash
    )

    $prefix = 'AGF-NoEAC-ModSync'
    if (-not (Test-Path -LiteralPath $ModsDir)) {
        New-Item -ItemType Directory -Path $ModsDir -Force | Out-Null
    }
    $folders = @(Get-ChildItem -LiteralPath $ModsDir -Directory -ErrorAction SilentlyContinue | Where-Object {
            $_.Name.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)
        })

    $newer = 0
    foreach ($folder in $folders) {
        if ((Get-ModVersion $folder.FullName).CompareTo($TargetVersion) -gt 0) { $newer++ }
    }
    if ($newer -gt 0) {
        Remove-OlderModSync $ModsDir $null $TargetVersion
        return 'current'
    }

    $dest = Join-Path $ModsDir $FolderName
    $destDll = Join-Path $dest 'ModSync.dll'
    if ((Test-Path -LiteralPath $destDll) -and ((Get-FileHash -LiteralPath $destDll -Algorithm SHA256).Hash -eq $SourceHash)) {
        Remove-OlderModSync $ModsDir (Resolve-Path -LiteralPath $dest).Path $TargetVersion
        return 'current'
    }

    foreach ($folder in $folders) {
        if (Test-FileLocked (Join-Path $folder.FullName 'ModSync.dll')) { return 'locked' }
    }
    if ((Test-Path -LiteralPath $destDll) -and (Test-FileLocked $destDll)) { return 'locked' }

    $destExisted = Test-Path -LiteralPath $dest
    New-Item -ItemType Directory -Path $dest -Force | Out-Null
    & robocopy $Source $dest /E /R:1 /W:1 /NFL /NDL /NP /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8 -or -not (Test-Path -LiteralPath $destDll)) {
        if (-not $destExisted) { Remove-Item -LiteralPath $dest -Recurse -Force -ErrorAction SilentlyContinue }
        return 'copyfail'
    }
    $destHash = (Get-FileHash -LiteralPath $destDll -Algorithm SHA256).Hash
    if ($destHash -ne $SourceHash) {
        if (-not $destExisted) { Remove-Item -LiteralPath $dest -Recurse -Force -ErrorAction SilentlyContinue }
        return 'copyfail'
    }

    Remove-OlderModSync $ModsDir (Resolve-Path -LiteralPath $dest).Path $TargetVersion
    return 'updated'
}

if ($BatPath) {

Write-Host ''
Write-Host '  Update Manda'

$source = Expand-EmbeddedMod
$work = Split-Path -Parent $source
try {
    $steam = Get-SteamGameFolder
    $steamRelease = $null
    if ($steam) { $steamRelease = Get-GameRelease $steam }

    if (-not $steam -or -not (Test-IsGame33 $steamRelease)) {
        Write-Host ''
        Write-Host '  Steam is not 3.3 yet. Update 7 Days to Die in Steam, then run this again.'
    } else {
        $manda = Get-MandaFolder $steam
        if (-not $manda) {
            Write-Host ''
            Write-Host '  No Manda install was found.'
        } elseif ((Test-GameRunning $manda) -or (Test-GameRunning $steam)) {
            Write-Host ''
            Write-Host '  Close 7 Days to Die, then run this again.'
        } else {
            $same = Test-SameFolder $manda $steam
            $ready = $false
            if ($same -or (Test-IsGame33 (Get-GameRelease $manda))) {
                Write-Host ''
                if ($same) { Write-Host '  Using the Steam install for Manda.' }
                else { Write-Host '  Manda is already 3.3.' }
                $ready = $true
            } else {
                if (Copy-GameFromSteam $steam $manda) {
                    if (Test-IsGame33 (Get-GameRelease $manda)) {
                        Write-Host '  Updated the Manda game to 3.3.'
                        $ready = $true
                    } else {
                        Write-Host '  The Manda game is still not 3.3.'
                    }
                } else {
                    Write-Host '  Could not update the Manda game.'
                }
            }

            if ($ready) {
                $result = Install-LatestModSync (Join-Path $manda 'Mods') $source $TargetName (Get-ModVersion $source) (
                    (Get-FileHash -LiteralPath (Join-Path $source 'ModSync.dll') -Algorithm SHA256).Hash)
                Write-Host ''
                if ($result -eq 'updated') { Write-Host '  Updated ModSync.' }
                elseif ($result -eq 'current') { Write-Host '  ModSync is already up to date.' }
                else { Write-Host '  Could not update ModSync. Close 7 Days to Die and run this again.' }
            }
        }
    }
} finally {
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
}

}
