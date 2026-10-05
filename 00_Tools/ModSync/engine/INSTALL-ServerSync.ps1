# ModSync player setup. Embedded in the generated .bat by generate.py.
#
# Player picks an existing 7 Days to Die install or makes a copy just for this
# server. A new copy takes game files only (plus 0_TFP_Harmony) so the server
# mods can be installed later by the sync itself.
#
# Settings arrive as environment variables from the .bat header, and the mod
# itself is decoded out of the .bat, so this file needs nothing beside it.
$ErrorActionPreference = 'Stop'

$ServerName = $env:MODSYNC_SERVER_NAME
$Ip = $env:MODSYNC_IP
$Port = 0
[void][int]::TryParse($env:MODSYNC_PORT, [ref]$Port)
$CopyFolderName = $env:MODSYNC_COPY_FOLDER
$ModFolderName = $env:MODSYNC_MOD_FOLDER
$EacOn = ($env:MODSYNC_EAC -match '^(on|yes|true|1)$')
$PackUrls = @(($env:MODSYNC_PACK_URL + '') -split '[|;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$BatPath = $env:MODSYNC_BAT_PATH

if (-not $ServerName) { throw 'This file is not meant to be run directly. Use the ModSync .bat.' }
if (-not $Ip -or $Port -le 0) { throw "The server address is missing. Ask $ServerName's admin for a new ModSync file." }
if (-not $CopyFolderName) { $CopyFolderName = '7 Days to Die - ' + $ServerName }
if (-not $ModFolderName) { $ModFolderName = 'AGF-NoEAC-ModSync' }

$LogFile = Join-Path $env:TEMP 'ModSync-Setup.log'
$RememberFile = Join-Path $env:LOCALAPPDATA ('AGF-ModSync\' + ($ServerName -replace '[^A-Za-z0-9]', '') + '.txt')

# The mod rides along inside the .bat as base64 lines. Pull them back out into
# a temp folder so the copy below has a real mod folder to work from.
function Expand-EmbeddedMod {
    if (-not $BatPath -or -not (Test-Path -LiteralPath $BatPath)) {
        throw 'Could not find the ModSync file to read the mod out of.'
    }

    $work = Join-Path $env:TEMP ('ModSync-Payload-' + [IO.Path]::GetRandomFileName())
    New-Item -ItemType Directory -Path $work -Force | Out-Null

    $parts = New-Object System.Text.StringBuilder
    foreach ($line in [IO.File]::ReadLines($BatPath)) {
        if ($line.StartsWith('::B64::')) { [void]$parts.Append($line.Substring(7)) }
    }
    if ($parts.Length -eq 0) { throw 'The ModSync file is incomplete. Ask the admin to send it again.' }

    $zip = Join-Path $work 'mod.zip'
    [IO.File]::WriteAllBytes($zip, [Convert]::FromBase64String($parts.ToString()))

    $out = Join-Path $work 'mod'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($zip, $out)
    Remove-Item -LiteralPath $zip -Force

    if (-not (Test-Path -LiteralPath (Join-Path $out 'ModSync.dll'))) {
        throw 'The ModSync file is damaged. Ask the admin to send it again.'
    }
    return $out
}

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

# A real 7D2D mod folder has ModInfo.xml in it. That file is the level we want
# in Mods\MODNAME\, not a wrapper folder and not Mods\MODNAME\MODNAME\.
function Test-ModInfoHere {
    param([string]$Dir)
    if (-not $Dir -or -not (Test-Path -LiteralPath $Dir)) { return $false }
    foreach ($f in @(Get-ChildItem -LiteralPath $Dir -File -ErrorAction SilentlyContinue)) {
        if ($f.Name -ieq 'ModInfo.xml') { return $true }
    }
    return $false
}

function Get-ModRootFolders {
    param([string]$ExtractRoot)
    $hits = New-Object System.Collections.Generic.List[string]
    if (Test-ModInfoHere $ExtractRoot) { [void]$hits.Add((Resolve-Path -LiteralPath $ExtractRoot).Path) }
    foreach ($dir in @(Get-ChildItem -LiteralPath $ExtractRoot -Directory -Recurse -ErrorAction SilentlyContinue)) {
        if (Test-ModInfoHere $dir.FullName) { [void]$hits.Add($dir.FullName) }
    }

    # When a zip wraps the same mod twice (MODNAME\ModInfo.xml and
    # MODNAME\MODNAME\ModInfo.xml), keep only the deepest folder.
    $deepestFirst = @($hits | Sort-Object { $_.Length } -Descending)
    $kept = New-Object System.Collections.Generic.List[string]
    foreach ($path in $deepestFirst) {
        $prefix = $path.TrimEnd('\') + '\'
        $underKept = $false
        foreach ($have in $kept) {
            if ($have.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { $underKept = $true; break }
            if ($have.Equals($path, [StringComparison]::OrdinalIgnoreCase)) { $underKept = $true; break }
        }
        if (-not $underKept) { [void]$kept.Add($path) }
    }
    return @($kept)
}

function Get-ArchiveKind {
    param([string]$Path, [bool]$Required = $true)
    $ext = [IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($ext -eq '.zip' -or $ext -eq '.rar' -or $ext -eq '.7z') { return $ext.TrimStart('.') }

    $fs = [IO.File]::OpenRead($Path)
    try {
        $b = New-Object byte[] 8
        [void]$fs.Read($b, 0, 8)
    } finally { $fs.Close() }
    if ($b[0] -eq 0x50 -and $b[1] -eq 0x4B) { return 'zip' }
    if ($b[0] -eq 0x52 -and $b[1] -eq 0x61 -and $b[2] -eq 0x72) { return 'rar' }
    if ($b[0] -eq 0x37 -and $b[1] -eq 0x7A) { return '7z' }
    if ($Required) { throw 'That download is not a zip or rar file.' }
    return $null
}

function Test-InsideModFolder {
    param([string]$Path, [string]$ExtractRoot)
    $dir = $Path
    if (Test-Path -LiteralPath $Path -PathType Leaf) { $dir = Split-Path -Parent $Path }
    $root = (Resolve-Path -LiteralPath $ExtractRoot).Path.TrimEnd('\')
    while ($dir -and $dir.Length -ge $root.Length) {
        if (Test-ModInfoHere $dir) { return $true }
        if ($dir.Equals($root, [StringComparison]::OrdinalIgnoreCase)) { break }
        $next = Split-Path -Parent $dir
        if (-not $next -or $next -eq $dir) { break }
        $dir = $next
    }
    return $false
}

# Archives sitting next to readmes or other junk, not inside a real mod.
function Get-LooseArchives {
    param([string]$ExtractRoot)
    $found = New-Object System.Collections.Generic.List[string]
    foreach ($f in @(Get-ChildItem -LiteralPath $ExtractRoot -File -Recurse -ErrorAction SilentlyContinue)) {
        $ext = $f.Extension.ToLowerInvariant()
        if ($ext -ne '.zip' -and $ext -ne '.rar' -and $ext -ne '.7z') { continue }
        if (Test-InsideModFolder $f.FullName $ExtractRoot) { continue }
        [void]$found.Add($f.FullName)
    }
    return @($found)
}

function Expand-NestedArchives {
    param([string]$ExtractRoot)
    $round = 0
    while ($round -lt 2) {
        $round++
        $loose = @(Get-LooseArchives $ExtractRoot)
        if ($loose.Count -eq 0) { break }
        $i = 0
        foreach ($arc in $loose) {
            $i++
            $stem = [IO.Path]::GetFileNameWithoutExtension($arc)
            if (-not $stem) { $stem = 'pack' + $i }
            $out = Join-Path $ExtractRoot ('_unpacked\' + $round + '\' + $stem)
            if (Test-Path -LiteralPath $out) { $out = $out + '-' + $i }
            try {
                Expand-PackArchive $arc $out
            } catch {
                Write-Host ("  Could not open " + $arc + ". Skipping it.") -ForegroundColor Yellow
            }
            Remove-Item -LiteralPath $arc -Force -ErrorAction SilentlyContinue
        }
    }
}

function Get-Unpacker {
    param([string]$Kind)
    foreach ($p in @(
            (Join-Path $env:ProgramFiles '7-Zip\7z.exe'),
            (Join-Path ${env:ProgramFiles(x86)} '7-Zip\7z.exe')
        )) {
        if ($p -and (Test-Path -LiteralPath $p)) { return @{ Exe = $p; Style = '7z' } }
    }
    if ($Kind -eq 'zip') { return @{ Exe = ''; Style = 'zip' } }
    foreach ($p in @(
            (Join-Path $env:ProgramFiles 'WinRAR\UnRAR.exe'),
            (Join-Path $env:ProgramFiles 'WinRAR\WinRAR.exe'),
            (Join-Path ${env:ProgramFiles(x86)} 'WinRAR\UnRAR.exe'),
            (Join-Path ${env:ProgramFiles(x86)} 'WinRAR\WinRAR.exe')
        )) {
        if ($p -and (Test-Path -LiteralPath $p)) {
            $style = 'unrar'
            if ([IO.Path]::GetFileNameWithoutExtension($p) -ieq 'WinRAR') { $style = 'winrar' }
            return @{ Exe = $p; Style = $style }
        }
    }
    throw 'A .rar pack needs 7-Zip or WinRAR on this PC. Install 7-Zip, or ask the admin for a .zip.'
}

function Expand-PackArchive {
    param([string]$Archive, [string]$OutDir)
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
    $kind = Get-ArchiveKind $Archive
    $tool = Get-Unpacker $kind
    if ($tool.Style -eq 'zip') {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [IO.Compression.ZipFile]::ExtractToDirectory($Archive, $OutDir)
        return
    }
    if ($tool.Style -eq '7z') {
        & $tool.Exe @('x', '-y', ('-o' + $OutDir), $Archive) *>> $LogFile
        if ($LASTEXITCODE -ne 0) { throw "Could not unzip the pack. Details: $LogFile" }
        return
    }
    if ($tool.Style -eq 'unrar') {
        & $tool.Exe @('x', '-y', '--', $Archive, ($OutDir.TrimEnd('\') + '\')) *>> $LogFile
        if ($LASTEXITCODE -ne 0) { throw "Could not unzip the pack. Details: $LogFile" }
        return
    }
    & $tool.Exe @('x', '-ibck', '-y', $Archive, ($OutDir.TrimEnd('\') + '\')) *>> $LogFile
    if ($LASTEXITCODE -ne 0) { throw "Could not unzip the pack. Details: $LogFile" }
}

function Install-ModRoots {
    param([string[]]$Roots, [string]$ModsDir)
    New-Item -ItemType Directory -Path $ModsDir -Force | Out-Null
    $count = 0
    $used = @{}
    foreach ($root in $Roots) {
        $name = Split-Path -Leaf $root
        if (-not $name -or $name -ieq 'Mods' -or $name -ieq '_unpacked') { continue }
        if (Test-ProtectedModName $name) {
            $destProtected = Join-Path $ModsDir $name
            if ($name -ieq '0_TFP_Harmony' -and -not (Test-Path -LiteralPath $destProtected)) {
                Copy-Quiet $root $destProtected
            }
            continue
        }
        $dest = Join-Path $ModsDir $name
        if ($used.ContainsKey($name.ToLowerInvariant())) {
            Write-Host ("  Pack has two folders named " + $name + ". Using the last one.") -ForegroundColor Yellow
        }
        $used[$name.ToLowerInvariant()] = $true
        Copy-Quiet $root $dest
        $count++
    }
    if ($count -eq 0) { throw 'The pack had no mod folders with ModInfo.xml in them.' }
    return $count
}

function Get-PackMarkerPath {
    param([string]$GameDir)
    return (Join-Path $GameDir 'ModSync-Pack.txt')
}

function Test-PackAlreadyInstalled {
    param([string]$GameDir, [string[]]$Urls)
    $file = Get-PackMarkerPath $GameDir
    if (-not (Test-Path -LiteralPath $file)) { return $false }
    $have = New-Object System.Collections.Generic.List[string]
    foreach ($line in @(Get-Content -LiteralPath $file -ErrorAction SilentlyContinue)) {
        if ($line.StartsWith('PACK_URL=', [StringComparison]::OrdinalIgnoreCase)) {
            [void]$have.Add($line.Substring(9).Trim())
        }
    }
    foreach ($url in $Urls) {
        if (-not ($have -contains $url)) { return $false }
    }
    return $true
}

function Save-PackMarker {
    param([string]$GameDir, [string[]]$Urls)
    $lines = New-Object System.Collections.Generic.List[string]
    [void]$lines.Add("# Pack downloaded for $ServerName.")
    foreach ($url in $Urls) { [void]$lines.Add("PACK_URL=$url") }
    $lines | Set-Content -LiteralPath (Get-PackMarkerPath $GameDir) -Encoding ASCII
}

function Get-PackFileName {
    param([string]$Url)
    $name = [IO.Path]::GetFileName(([Uri]$Url).AbsolutePath)
    if (-not $name) { $name = 'server-pack.zip' }
    return $name
}

function Test-OneDriveShareUrl {
    param([string]$Url)
    if (-not $Url) { return $false }
    return [bool]($Url -match '(?i)(1drv\.ms|onedrive\.live\.com|my\.microsoftpersonalcontent\.com)')
}

function ConvertTo-OneDriveShareId {
    param([string]$Url)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Url.Trim())
    $b64 = [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
    return "u!$b64"
}

function Expand-OneDriveShareUrl {
    param([string]$Url)
    try {
        $req = [System.Net.HttpWebRequest]::Create($Url)
        $req.AllowAutoRedirect = $false
        $req.Method = 'GET'
        $req.Timeout = 20000
        $req.UserAgent = 'AGF-ModSync'
        $resp = $req.GetResponse()
        $loc = $resp.Headers['Location']
        $resp.Close()
        if ($loc) { return $loc }
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        if ($resp) {
            $loc = $resp.Headers['Location']
            $resp.Close()
            if ($loc) { return $loc }
        }
    }
    return $Url
}

function Test-ODHasFacet {
    param($Item, [string]$Name)
    return ($Item.PSObject.Properties.Name -contains $Name) -and ($null -ne $Item.$Name)
}

function Get-OneDriveBadgerToken {
    $resp = Invoke-RestMethod -Method Post -Uri 'https://api-badgerp.svc.ms/v1.0/token' `
        -ContentType 'application/json' `
        -Body '{"appId":"5cbed6ac-a083-4e14-b191-b4ba07653de2"}' `
        -TimeoutSec 30
    if (-not $resp.token) { throw 'OneDrive did not return an access token.' }
    return [string]$resp.token
}

function Invoke-OneDriveApi {
    param(
        [string]$Uri,
        [string]$Token
    )
    $headers = @{
        'Accept'     = 'application/json'
        'Prefer'     = 'autoredeem'
        'User-Agent' = 'AGF-ModSync'
    }
    if ($Token) { $headers['Authorization'] = "Badger $Token" }
    return Invoke-RestMethod -Method Get -Uri $Uri -Headers $headers -TimeoutSec 60
}

function Test-DownloadedLooksLikeHtml {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return $true }
    $len = (Get-Item -LiteralPath $Path).Length
    if ($len -lt 32) { return $true }
    if ($len -gt 512000) { return $false }
    $head = [IO.File]::ReadAllText($Path)
    if ($head.Length -gt 400) { $head = $head.Substring(0, 400) }
    return [bool]($head -match '(?i)^\s*<(!doctype|html|head)|"error"\s*:')
}

function Invoke-PackDownload {
    param([string]$Url, [string]$Dest)
    New-Item -ItemType Directory -Path (Split-Path -Parent $Dest) -Force | Out-Null
    $curl = Join-Path $env:SystemRoot 'System32\curl.exe'
    if (Test-Path -LiteralPath $curl) {
        & $curl @('-L', '--fail', '--retry', '3', '-A', 'AGF-ModSync', '-o', $Dest, $Url)
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $Dest)) {
            throw 'Could not download the server pack. Check the link or try again.'
        }
        return
    }
    Invoke-WebRequest -Uri $Url -OutFile $Dest -UseBasicParsing
}

function Connect-OneDriveShareLite {
    param([string]$ShareUrl)
    if ($ShareUrl -match 'sharepoint\.com') {
        throw 'That looks like work/school OneDrive. Use a personal 1drv.ms link set to Anyone with the link can view.'
    }
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $token = ''
    try { $token = Get-OneDriveBadgerToken } catch { $token = '' }
    $candidates = @($ShareUrl.Trim())
    $expanded = Expand-OneDriveShareUrl $ShareUrl
    if ($expanded -and ($expanded -ne $ShareUrl)) { $candidates += $expanded }

    $lastError = ''
    foreach ($candidate in $candidates) {
        $shareId = ConvertTo-OneDriveShareId $candidate
        try {
            $root = Invoke-OneDriveApi -Uri "https://api.onedrive.com/v1.0/shares/$shareId/driveItem" -Token $token
            $driveId = [string]$root.parentReference.driveId
            $itemId = [string]$root.id
            if (-not $driveId -or -not $itemId) { throw 'OneDrive did not return file ids.' }
            return @{
                Token    = $token
                ShareId  = $shareId
                Root     = $root
                DriveId  = $driveId
                ItemId   = $itemId
                Name     = [string]$root.name
                IsFile   = (Test-ODHasFacet $root 'file')
                IsFolder = (Test-ODHasFacet $root 'folder')
            }
        } catch {
            $lastError = $_.Exception.Message
        }
    }
    throw ("Could not open the OneDrive link. Share it as Anyone with the link can view. Details: {0}" -f $lastError)
}

function Save-OneDriveDriveItem {
    param($Session, [string]$ItemId, [string]$OutFile)
    New-Item -ItemType Directory -Path (Split-Path -Parent $OutFile) -Force | Out-Null
    $url = 'https://api.onedrive.com/v1.0/drives/{0}/items/{1}/content' -f $Session.DriveId, $ItemId
    $wc = New-Object System.Net.WebClient
    try {
        $wc.Headers.Add('User-Agent', 'AGF-ModSync')
        if ($Session.Token) { $wc.Headers.Add('Authorization', "Badger $($Session.Token)") }
        $wc.DownloadFile($url, $OutFile)
    } finally {
        $wc.Dispose()
    }
}

function Copy-OneDriveFolderTree {
    param($Session, [string]$ItemId, [string]$DestDir)
    New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
    $uri = "https://api.onedrive.com/v1.0/drives/$($Session.DriveId)/items/${ItemId}/children?`$top=200"
    while ($uri) {
        $page = Invoke-OneDriveApi -Uri $uri -Token $Session.Token
        $vals = @()
        if (Test-ODHasFacet $page 'value') { $vals = @($page.value) }
        foreach ($item in $vals) {
            $name = [string]$item.name
            if (-not $name) { continue }
            $childDest = Join-Path $DestDir $name
            if (Test-ODHasFacet $item 'folder') {
                Copy-OneDriveFolderTree $Session ([string]$item.id) $childDest
            } else {
                Save-OneDriveDriveItem $Session ([string]$item.id) $childDest
            }
        }
        $uri = $null
        if (Test-ODHasFacet $page '@odata.nextLink') {
            $uri = [string]$page.'@odata.nextLink'
        }
    }
}

function Get-OneDrivePackInto {
    param([string]$Url, [string]$ArchiveDest, [string]$ExtractDest)
    Write-Host '  OneDrive share - opening the real download...' -ForegroundColor Yellow
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $session = Connect-OneDriveShareLite $Url
    if ($session.IsFile) {
        Save-OneDriveDriveItem $session $session.ItemId $ArchiveDest
        return 'archive'
    }

    Write-Host '  OneDrive folder - downloading files...' -ForegroundColor Yellow
    Copy-OneDriveFolderTree $session $session.ItemId $ExtractDest
    return 'folder'
}

function Download-PackArchive {
    param([string]$Url, [string]$Dest)
    Invoke-PackDownload $Url $Dest
}

function Install-OptionalPack {
    param([string]$GameDir)
    if ($PackUrls.Count -eq 0) { return $false }
    if (Test-PackAlreadyInstalled $GameDir $PackUrls) {
        Write-Host '  Server pack is already in this folder.' -ForegroundColor Green
        return $true
    }

    Write-Host '  Downloading the server pack. This can take a while...' -ForegroundColor Yellow
    $work = Join-Path $env:TEMP ('ModSync-Pack-' + [IO.Path]::GetRandomFileName())
    New-Item -ItemType Directory -Path $work -Force | Out-Null
    try {
        $extract = Join-Path $work 'extract'
        $nUrl = 0
        foreach ($url in $PackUrls) {
            $nUrl++
            $one = Join-Path $extract ('download-' + $nUrl)
            if (Test-OneDriveShareUrl $url) {
                $archive = Join-Path $work ($nUrl.ToString() + '-onedrive.bin')
                $kind = Get-OneDrivePackInto $url $archive $one
                if ($kind -eq 'archive') {
                    Expand-PackArchive $archive $one
                }
            } else {
                $archive = Join-Path $work ($nUrl.ToString() + '-' + (Get-PackFileName $url))
                Download-PackArchive $url $archive
                Expand-PackArchive $archive $one
            }
            Expand-NestedArchives $one
        }
        $roots = @(Get-ModRootFolders $extract)
        $n = Install-ModRoots $roots (Join-Path $GameDir 'Mods')
        Save-PackMarker $GameDir $PackUrls
        Write-Host ("  Installed " + $n + " mod folder(s). Extra files in the pack were left out.") -ForegroundColor Green
    } finally {
        if (Test-Path -LiteralPath $work) {
            Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    return $true
}

function Add-Unique {
    param($List, [string]$Path)
    if (-not $Path) { return }
    $clean = $Path.TrimEnd('\', '/')
    if (-not $clean) { return }
    if (-not (Test-Path -LiteralPath $clean)) { return }
    foreach ($have in $List) {
        if ($have -eq $clean) { return }
    }
    [void]$List.Add($clean)
}

function Get-SteamRoots {
    $roots = New-Object System.Collections.Generic.List[string]

    # HKCU stores SteamPath, HKLM stores InstallPath. Reading the wrong name on the
    # wrong key silently returns nothing.
    $probes = @(
        @{ Key = 'HKCU:\Software\Valve\Steam'; Name = 'SteamPath' }
        @{ Key = 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam'; Name = 'InstallPath' }
        @{ Key = 'HKLM:\SOFTWARE\Valve\Steam'; Name = 'InstallPath' }
    )
    foreach ($probe in $probes) {
        try {
            $p = (Get-ItemProperty -Path $probe.Key -ErrorAction Stop).($probe.Name)
            if ($p) { Add-Unique $roots ($p.Replace('/', '\')) }
        } catch { }
    }

    foreach ($guess in @((Join-Path ${env:ProgramFiles(x86)} 'Steam'), (Join-Path $env:ProgramFiles 'Steam'), 'C:\Steam')) {
        Add-Unique $roots $guess
    }
    return $roots
}

# Steam keeps games in library folders that are often on another drive, and the
# app manifest lives in the library, not in the Steam root. Miss this and any
# player who installed to a second disk looks like they have no game at all.
$script:LibraryCache = $null
function Get-SteamLibraries {
    if ($script:LibraryCache) { return $script:LibraryCache }

    $libs = New-Object System.Collections.Generic.List[string]
    foreach ($root in (Get-SteamRoots)) {
        Add-Unique $libs $root

        # libraryfolders.vdf is the modern list. config.vdf still carries
        # BaseInstallFolder_N on installs that have been upgraded for years.
        $sources = @(
            @{ File = (Join-Path $root 'steamapps\libraryfolders.vdf'); Patterns = @('"path"\s*"([^"]+)"', '"\d+"\s*"([A-Za-z]:[^"]+)"') }
            @{ File = (Join-Path $root 'config\config.vdf'); Patterns = @('"BaseInstallFolder_\d+"\s*"([^"]+)"') }
        )

        foreach ($source in $sources) {
            if (-not (Test-Path -LiteralPath $source.File)) { continue }
            try {
                $text = Get-Content -Raw -LiteralPath $source.File
                foreach ($pattern in $source.Patterns) {
                    foreach ($match in [regex]::Matches($text, $pattern)) {
                        # Steam writes these paths with doubled backslashes.
                        Add-Unique $libs ($match.Groups[1].Value.Replace('\\', '\'))
                    }
                }
            } catch { }
        }
    }

    $script:LibraryCache = $libs
    return $libs
}

function Get-SteamGameFolder {
    foreach ($lib in (Get-SteamLibraries)) {
        $manifest = Join-Path $lib 'steamapps\appmanifest_251570.acf'
        if (-not (Test-Path -LiteralPath $manifest)) { continue }
        try {
            $text = Get-Content -Raw -LiteralPath $manifest
            $install = [regex]::Match($text, '"installdir"\s*"([^"]+)"').Groups[1].Value
            if (-not $install) { continue }
            $game = Join-Path $lib ("steamapps\common\" + $install)
            if (Test-Path -LiteralPath (Join-Path $game '7DaysToDie.exe')) { return $game }
        } catch { }
    }

    # No usable manifest, so try the folder name Steam normally uses.
    foreach ($lib in (Get-SteamLibraries)) {
        foreach ($name in @('7 Days To Die', '7 Days to Die')) {
            $game = Join-Path $lib ("steamapps\common\" + $name)
            if (Test-Path -LiteralPath (Join-Path $game '7DaysToDie.exe')) { return $game }
        }
    }
    return $null
}

# Last resort so an unusual setup is never a dead end.
function Read-GameFolder {
    Write-Host ''
    Write-Host '  ModSync could not find 7 Days to Die by itself.' -ForegroundColor Yellow
    Write-Host ''

    # Printed so a report back to the admin says which places were searched,
    # instead of just "it did not work".
    $roots = @(Get-SteamRoots)
    $libs = @(Get-SteamLibraries)
    if ($roots.Count -eq 0) {
        Write-Host '  It could not find Steam itself on this PC.'
    } else {
        Write-Host '  Steam folders it checked:'
        foreach ($lib in $libs) { Write-Host ("    " + $lib) }
    }
    Write-Host ''
    Write-Host '  In Steam, right-click 7 Days to Die, choose Manage, then'
    Write-Host '  Browse local files. Copy the folder path from the address bar'
    Write-Host '  at the top of the window and paste it below.'
    Write-Host ''
    while ($true) {
        $typed = (Read-Host '  Paste the folder path (or press Enter to quit)').Trim().Trim('"')
        if (-not $typed) { return $null }
        if (Test-Path -LiteralPath (Join-Path $typed '7DaysToDie.exe')) { return $typed.TrimEnd('\', '/') }
        Write-Host '  That folder has no 7DaysToDie.exe in it. Try again.' -ForegroundColor Yellow
    }
}

function Get-OtherInstalls {
    param([string]$SteamGame)

    $parents = New-Object System.Collections.Generic.List[string]
    Add-Unique $parents (Split-Path -Parent $SteamGame)
    foreach ($lib in (Get-SteamLibraries)) {
        Add-Unique $parents (Join-Path $lib 'steamapps\common')
    }

    $found = New-Object System.Collections.Generic.List[string]
    foreach ($parent in $parents) {
        foreach ($dir in @(Get-ChildItem -LiteralPath $parent -Directory -ErrorAction SilentlyContinue)) {
            if ($dir.FullName -eq $SteamGame) { continue }
            if (Test-Path -LiteralPath (Join-Path $dir.FullName '7DaysToDie.exe')) {
                Add-Unique $found $dir.FullName
            }
        }
    }
    return @($found)
}

function Install-Helper {
    param([string]$GameDir, [string]$Source)
    $dest = Join-Path $GameDir ('Mods\' + $ModFolderName)
    if (Test-Path -LiteralPath $dest) { Remove-Item -LiteralPath $dest -Recurse -Force }
    New-Item -ItemType Directory -Path (Split-Path -Parent $dest) -Force | Out-Null
    Copy-Quiet $Source $dest
    # The shortcut carries the address. A join file in Mods is loaded by every
    # 7 Days to Die install that has this mod.
    $joinFile = Join-Path $dest 'modsync-join.txt'
    if (Test-Path -LiteralPath $joinFile) { Remove-Item -LiteralPath $joinFile -Force }
}

# After the player picks a folder for this server, extras may be moved aside.
# Without this marker ModSync will add and update mods but never delete anything,
# and the next setup run would treat the pack as "mods of your own."
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

# Harmony and ModSync itself are not the player's work. Everything else in a Mods
# folder is, and the server is about to decide what lives there.
function Test-ProtectedModName {
    param([string]$Name)
    if ($Name -ieq '0_TFP_Harmony') { return $true }
    if ($Name -ieq $ModFolderName) { return $true }
    if ($Name -like 'AGF-NoEAC-ModSync*') { return $true }
    return $false
}

function Get-ForeignMods {
    param([string]$GameDir)
    $items = New-Object System.Collections.Generic.List[string]
    $modsDir = Join-Path $GameDir 'Mods'
    if (-not (Test-Path -LiteralPath $modsDir)) { return @($items) }
    foreach ($entry in @(Get-ChildItem -LiteralPath $modsDir -ErrorAction SilentlyContinue)) {
        if (Test-ProtectedModName $entry.Name) { continue }
        [void]$items.Add($entry.Name)
    }
    return @($items)
}

# Moved, never deleted. A player who picks the wrong folder must always be able to
# put their own mods back. Each setup run gets its own dated folder, so running the
# tool again never overwrites an earlier rescue.
function Backup-ExistingMods {
    param([string]$GameDir, [string[]]$Names)
    $modsDir = Join-Path $GameDir 'Mods'
    $backupDir = Join-Path $GameDir ('Mods - Backup\' + (Get-Date -Format 'yyyy-MM-dd HHmmss'))
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

    foreach ($name in $Names) {
        Move-Item -LiteralPath (Join-Path $modsDir $name) -Destination (Join-Path $backupDir $name) -Force
    }
    return $backupDir
}

# A running log, appended to on every setup run, so there is always a record of
# what was moved and where it went.
function Write-BackupRecord {
    param([string]$GameDir, [string[]]$Names, [string]$BackupDir)
    # The folder may not exist yet when the choice is a brand new copy.
    New-Item -ItemType Directory -Path $GameDir -Force | Out-Null

    $lines = New-Object System.Collections.Generic.List[string]
    [void]$lines.Add('')
    [void]$lines.Add("# $ServerName setup ran " + (Get-Date -Format 'yyyy-MM-dd HH:mm') + '.')
    if ($Names.Count -gt 0) {
        [void]$lines.Add("# These were moved out of Mods to: $BackupDir")
        [void]$lines.Add('# Nothing was deleted. Move them back any time.')
        foreach ($name in $Names) { [void]$lines.Add("MOVED=$name") }
    } else {
        [void]$lines.Add('# There were no mods of your own here, so nothing was moved.')
    }

    $file = Join-Path $GameDir 'ModSync-Backup.txt'
    if (Test-Path -LiteralPath $file) {
        $lines | Add-Content -LiteralPath $file -Encoding UTF8
    } else {
        @("# What ModSync moved aside before letting $ServerName manage this game.") + $lines |
            Set-Content -LiteralPath $file -Encoding UTF8
    }
}

function Confirm-Target {
    param([string]$GameDir, [bool]$IsServerCopy)

    # A folder this server already manages holds the server's own mods, not the
    # player's. Sweeping those aside would just force the whole pack to download again.
    $serverManaged = Test-Path -LiteralPath (Join-Path $GameDir 'ModSync-Managed.txt')

    $foreign = @()
    if (-not $serverManaged) { $foreign = @(Get-ForeignMods $GameDir) }

    # Nothing at risk, so do not nag.
    if ($foreign.Count -eq 0 -and ($IsServerCopy -or $serverManaged)) {
        Write-BackupRecord $GameDir @() ''
        return $true
    }

    Write-Host ''
    Write-Host '  Please read this before continuing.' -ForegroundColor Yellow
    Write-Host ''
    Write-Host "  You picked:  $GameDir"
    Write-Host ''
    Write-Host "  $ServerName will decide which mods this game uses."
    Write-Host '  Mods it does not use can be replaced or set aside.'

    if ($foreign.Count -gt 0) {
        Write-Host ''
        Write-Host ("  This game already has " + $foreign.Count + " mod(s) of your own:")
        $shown = 0
        foreach ($name in $foreign) {
            if ($shown -ge 10) { break }
            Write-Host ("    - " + $name)
            $shown++
        }
        if ($foreign.Count -gt $shown) {
            Write-Host ("    ...and " + ($foreign.Count - $shown) + " more")
        }
        Write-Host ''
        Write-Host ('  They will be MOVED to:  ' + (Join-Path $GameDir 'Mods - Backup')) -ForegroundColor Cyan
        Write-Host '  Nothing is deleted. You can move them back whenever you like.'
        Write-Host '  Each run keeps its own dated folder in there.'
    }

    Write-Host ''
    Write-Host '  Type YES to use this game, or press Enter to pick a different one.'
    $answer = (Read-Host '  ').Trim()
    if ($answer -notmatch '^yes$') {
        Write-Host ''
        Write-Host '  Nothing was changed.' -ForegroundColor Yellow
        return $false
    }

    $backupDir = ''
    if ($foreign.Count -gt 0) {
        $backupDir = Backup-ExistingMods $GameDir $foreign
        Write-Host ''
        Write-Host ("  Moved " + $foreign.Count + " mod(s) into " + $backupDir) -ForegroundColor Green
    }
    Write-BackupRecord $GameDir $foreign $backupDir
    return $true
}

function New-PlayShortcut {
    param([string]$GameDir)
    $lnk = Join-Path ([Environment]::GetFolderPath('Desktop')) ($ServerName + '.lnk')
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($lnk)
    $shortcut.TargetPath = Join-Path $GameDir '7DaysToDie.exe'
    $args = '-skipintro -skipnewsscreen=true'
    if (-not $EacOn) { $args += ' -noeac' }
    # Not a game preference name. Those are saved into the shared user folder and
    # would change every 7 Days to Die install on this PC.
    $safeName = ($ServerName -replace '"', '')
    $args += " -modsyncjoin ${Ip}:${Port} -modsyncname `"$safeName`""
    $shortcut.Arguments = $args
    $shortcut.WorkingDirectory = $GameDir
    $shortcut.Description = $ServerName
    $shortcut.Save()
    return $lnk
}

function Write-AllSet {
    param([bool]$HadPack)
    Write-Host ''
    Write-Host '  All set.' -ForegroundColor Green
    Write-Host ''
    Write-Host "  A shortcut named `"$ServerName`" is on your desktop."
    if ($HadPack) {
        Write-Host '  Use it to play. The game will check the server for anything still missing.'
    } else {
        Write-Host '  Use it to play. The first time, it downloads the server mods and'
        Write-Host '  restarts itself once.'
    }
    Write-Host '  If the server asks for a password, type it in the game.'
    Write-Host ''
}

function Finish-Setup {
    param([string]$GameDir)
    $hadPack = [bool](Install-OptionalPack $GameDir)
    Install-Helper $GameDir $helperSource
    Set-ManagedMarker $GameDir $true
    [void](New-PlayShortcut $GameDir)
    Save-RememberedInstall $GameDir
    Write-AllSet $hadPack
}

'' | Set-Content -LiteralPath $LogFile

$steamGame = Get-SteamGameFolder
if (-not $steamGame) { $steamGame = Read-GameFolder }
if (-not $steamGame) { throw 'No 7 Days to Die folder was given, so nothing was changed.' }

$helperSource = Expand-EmbeddedMod
try {
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
            if (Confirm-Target $remembered ($remembered -eq $newCopyPath)) {
                Finish-Setup $remembered
                return
            }
        }
        Write-Host ''
    }

    $target = $null
    while (-not $target) {
        Write-Host '  Which game should this server use?'
        Write-Host ''

        $newCopyExists = Test-Path -LiteralPath (Join-Path $newCopyPath '7DaysToDie.exe')
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
        Write-Host '  A copy just for this server is safest. Your main game keeps its own mods.'
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

        $picked = $choice.Path
        Write-Host ''

        # Asked before the copy, so a declined choice costs nothing.
        if (-not (Confirm-Target $picked ($picked -eq $newCopyPath))) {
            Write-Host ''
            continue
        }

        if ($choice.MakeCopy) {
            Write-Host '  Copying the game. This is big, so it can take a while...' -ForegroundColor Yellow
            if (-not (Test-Path -LiteralPath $picked)) { New-Item -ItemType Directory -Path $picked | Out-Null }
            Copy-Quiet $steamGame $picked @('/XD', 'logs', (Join-Path $steamGame 'Mods'))

            $harmony = Join-Path $steamGame 'Mods\0_TFP_Harmony'
            if (Test-Path -LiteralPath $harmony) {
                Copy-Quiet $harmony (Join-Path $picked 'Mods\0_TFP_Harmony')
            } else {
                Write-Host '  Note: 0_TFP_Harmony was not found in your Steam game.' -ForegroundColor Yellow
            }
            Write-Host '  Copy finished.' -ForegroundColor Green
        } else {
            Write-Host ("  Using: " + (Split-Path -Leaf $picked)) -ForegroundColor Green
        }

        $target = $picked
    }

    Finish-Setup $target
} finally {
    $payloadRoot = Split-Path -Parent $helperSource
    if ($payloadRoot -and (Test-Path -LiteralPath $payloadRoot)) {
        Remove-Item -LiteralPath $payloadRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
