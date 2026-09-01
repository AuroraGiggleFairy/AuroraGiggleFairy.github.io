# Shared-pack helper for INSTALL-Mods.ps1
# Backends: personal OneDrive folder, Google Drive folder, or a downloadable .zip.

function Test-ODHasFacet {
    param($Item, [string]$Name)
    return ($Item.PSObject.Properties.Name -contains $Name) -and ($null -ne $Item.$Name)
}

function ConvertTo-OneDriveShareId {
    param([string]$Url)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Url.Trim())
    $b64 = [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
    return "u!$b64"
}

function Get-OneDriveShareUrlFromConfig {
    param([hashtable]$ConfigMap)
    if ($env:SHARE_URL) {
        $u = $env:SHARE_URL.Trim().Trim('"')
        if ($u) { return $u }
    }
    if ($env:ONEDRIVE_SHARE_URL) {
        $u = $env:ONEDRIVE_SHARE_URL.Trim().Trim('"')
        if ($u) { return $u }
    }
    $file = Join-Path $SourceDir 'INSTALL-ONEDRIVE.txt'
    if (Test-Path -LiteralPath $file) {
        $line = @(Get-Content -LiteralPath $file -Encoding UTF8 |
            ForEach-Object { $_.Trim().Trim('"') } |
            Where-Object { $_ -ne '' -and -not $_.StartsWith('#') } |
            Select-Object -First 1)
        if ($line.Count -gt 0 -and $line[0]) { return $line[0] }
    }
    if ($ConfigMap.ContainsKey('SHARE_URL') -and $ConfigMap['SHARE_URL']) {
        return $ConfigMap['SHARE_URL'].Trim().Trim('"')
    }
    if ($ConfigMap.ContainsKey('ONEDRIVE_SHARE_URL') -and $ConfigMap['ONEDRIVE_SHARE_URL']) {
        return $ConfigMap['ONEDRIVE_SHARE_URL'].Trim().Trim('"')
    }
    return ''
}

function Get-ShareKind {
    param([string]$Url)
    if (-not $Url) { return 'none' }
    $u = $Url.ToLowerInvariant()
    if ($u -match 'drive\.google\.com/file/d/') {
        return 'zip'
    }
    if ($u -match 'drive\.google\.com/.*/folders/' -or $u -match 'drive\.google\.com/drive/folders' -or
        $u -match 'drive\.google\.com/open\?id=' -or $u -match 'drive\.google\.com/embeddedfolderview') {
        return 'gdrive'
    }
    if ($u -match 'mega\.(nz|co\.nz)/(folder|fm/)') {
        return 'unsupported-folder'
    }
    if ($u -match 'sharepoint\.com' -and $u -notmatch '\.zip(\?|$)') {
        return 'unsupported-folder'
    }
    if ($u -match '1drv\.ms|onedrive\.live\.com|my\.microsoftpersonalcontent\.com') {
        if ($u -match '\.zip(\?|$)') { return 'zip' }
        return 'onedrive'
    }
    return 'zip'
}

function ConvertTo-DirectDownloadUrl {
    param([string]$Url)
    $u = $Url.Trim()
    if ($u -match 'dropbox\.com') {
        if ($u -match '[\?&]dl=0') {
            $u = $u -replace 'dl=0', 'dl=1'
        } elseif ($u -notmatch '[\?&]dl=') {
            if ($u.Contains('?')) { $u = $u + '&dl=1' } else { $u = $u + '?dl=1' }
        }
        return $u
    }
    if ($u -match 'drive\.google\.com/file/d/([^/]+)') {
        return ('https://drive.google.com/uc?export=download&id={0}' -f $Matches[1])
    }
    if ($u -match 'drive\.google\.com/open\?id=([^&]+)') {
        return ('https://drive.google.com/uc?export=download&id={0}' -f $Matches[1])
    }
    if ($u -match 'drive\.google\.com/uc\?' -and $u -notmatch 'export=download') {
        if ($u.Contains('?')) { return ($u + '&export=download') }
        return ($u + '?export=download')
    }
    return $u
}

function Get-ShareUrlCacheKey {
    param([string]$Url)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Url.Trim().ToLowerInvariant())
        $hash = [BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '')
        return $hash.Substring(0, 16)
    } finally {
        $sha.Dispose()
    }
}

function Get-HttpHeadInfo {
    param(
        [string]$Url,
        [System.Net.CookieContainer]$Cookies
    )
    $req = [System.Net.HttpWebRequest]::Create($Url)
    $req.Method = 'HEAD'
    $req.AllowAutoRedirect = $true
    $req.Timeout = 20000
    $req.UserAgent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) ClientModSync'
    if ($Cookies) { $req.CookieContainer = $Cookies }
    try {
        $resp = $req.GetResponse()
        try {
            return @{
                Length       = [string]$resp.Headers['Content-Length']
                LastModified = [string]$resp.Headers['Last-Modified']
                ETag         = [string]$resp.Headers['ETag']
                Type         = [string]$resp.ContentType
            }
        } finally {
            $resp.Close()
        }
    } catch {
        return $null
    }
}

function Save-HttpDownloadToFile {
    param(
        [string]$Url,
        [string]$OutFile,
        [System.Net.CookieContainer]$Cookies
    )
    if (-not $Cookies) { $Cookies = New-Object System.Net.CookieContainer }
    $req = [System.Net.HttpWebRequest]::Create($Url)
    $req.Method = 'GET'
    $req.AllowAutoRedirect = $true
    $req.Timeout = 120000
    $req.ReadWriteTimeout = 300000
    $req.UserAgent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) ClientModSync'
    $req.CookieContainer = $Cookies
    $resp = $req.GetResponse()
    try {
        $stream = $resp.GetResponseStream()
        $fs = [System.IO.File]::Create($OutFile)
        try {
            $buffer = New-Object byte[] 262144
            while (($n = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                $fs.Write($buffer, 0, $n)
            }
        } finally {
            $fs.Close()
            $stream.Close()
        }
        return @{
            Type         = [string]$resp.ContentType
            Length       = [string]$resp.Headers['Content-Length']
            LastModified = [string]$resp.Headers['Last-Modified']
            ETag         = [string]$resp.Headers['ETag']
        }
    } finally {
        $resp.Close()
    }
}

function Test-FileIsZip {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return $false }
    $fs = [System.IO.File]::OpenRead($Path)
    try {
        if ($fs.Length -lt 4) { return $false }
        $b0 = $fs.ReadByte()
        $b1 = $fs.ReadByte()
        return ($b0 -eq 0x50 -and $b1 -eq 0x4B)
    } finally {
        $fs.Close()
    }
}

function Get-GoogleDriveFileId {
    param([string]$Url)
    if ($Url -match 'id=([A-Za-z0-9_-]+)') { return $Matches[1] }
    if ($Url -match 'file/d/([^/]+)') { return $Matches[1] }
    return ''
}

function Save-ShareUrlToFile {
    param(
        [string]$Url,
        [string]$OutFile
    )
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $cookies = New-Object System.Net.CookieContainer
    $direct = ConvertTo-DirectDownloadUrl $Url
    $info = Save-HttpDownloadToFile -Url $direct -OutFile $OutFile -Cookies $cookies
    if (Test-FileIsZip $OutFile) { return $info }

    $head = ''
    try {
        $text = [System.IO.File]::ReadAllText($OutFile)
        if ($text.Length -gt 8000) { $head = $text.Substring(0, 8000) } else { $head = $text }
    } catch {
        $head = ''
    }
    $looksHtml = ($head -match '(?i)<html|<!doctype') -or ([string]$info.Type -match 'text/html')
    if ($looksHtml -and ($direct -match 'google\.com|drive\.google')) {
        $id = Get-GoogleDriveFileId $direct
        $confirm = ''
        if ($head -match 'confirm=([0-9A-Za-z_-]+)') { $confirm = $Matches[1] }
        if (-not $confirm) { $confirm = 't' }
        if ($id) {
            $retry = 'https://drive.google.com/uc?export=download&confirm={0}&id={1}' -f $confirm, $id
            $info = Save-HttpDownloadToFile -Url $retry -OutFile $OutFile -Cookies $cookies
            if (Test-FileIsZip $OutFile) { return $info }
        }
    }
    throw 'The link did not download a .zip of the mod folders. Use a direct zip link, or a view-only OneDrive / Google Drive folder.'
}

function Get-RemoteModsFromExtractedPack {
    param([string]$ExtractDir)
    $root = $ExtractDir
    if (-not (Test-Path -LiteralPath $root)) { return @() }
    $dirs = @(Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue)
    $withInfo = @(
        $dirs | Where-Object {
            (Test-Path -LiteralPath (Join-Path $_.FullName 'ModInfo.xml')) -or
            (Test-IsHarmonyFolderName $_.Name)
        }
    )
    if ($withInfo.Count -eq 0 -and $dirs.Count -eq 1) {
        $root = $dirs[0].FullName
        $dirs = @(Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue)
    }
    $mods = New-Object System.Collections.ArrayList
    foreach ($d in $dirs) {
        $name = $d.Name
        if ($name -like 'INSTALL*') { continue }
        if ($name.StartsWith('.')) { continue }
        $info = Join-Path $d.FullName 'ModInfo.xml'
        $hasInfo = Test-Path -LiteralPath $info
        if (-not $hasInfo -and -not (Test-IsHarmonyFolderName $name)) { continue }
        $ver = ''
        if ($hasInfo) { $ver = Get-ModInfoVersionFromFile $info }
        [void]$mods.Add([pscustomobject]@{
            Name      = $name
            Version   = $ver
            LocalPath = $d.FullName
            Kind      = 'zip'
        })
    }
    return @($mods.ToArray() | Sort-Object Name)
}

function Get-ExtractedSharePack {
    param([string]$ShareUrl)
    $key = Get-ShareUrlCacheKey $ShareUrl
    $cacheRoot = Join-Path $env:LOCALAPPDATA ('AGF-ClientModSync\cache\{0}' -f $key)
    $zipPath = Join-Path $cacheRoot 'pack.zip'
    $extract = Join-Path $cacheRoot 'extract'
    $metaPath = Join-Path $cacheRoot 'cache.txt'
    if (-not (Test-Path -LiteralPath $cacheRoot)) {
        New-Item -ItemType Directory -Path $cacheRoot -Force | Out-Null
    }
    $direct = ConvertTo-DirectDownloadUrl $ShareUrl
    $head = Get-HttpHeadInfo -Url $direct -Cookies $null
    $cached = $false
    if ((Test-Path -LiteralPath $metaPath) -and (Test-Path -LiteralPath $extract) -and $head) {
        $old = @{}
        Get-Content -LiteralPath $metaPath -Encoding UTF8 | ForEach-Object {
            $line = $_.Trim()
            $eq = $line.IndexOf('=')
            if ($eq -gt 0) { $old[$line.Substring(0, $eq)] = $line.Substring($eq + 1) }
        }
        $sameEtag = ($head.ETag -and $old['ETag'] -and ($head.ETag -eq $old['ETag']))
        $sameLen = ($head.Length -and $old['Length'] -and ($head.Length -eq $old['Length']))
        $sameMod = ($head.LastModified -and $old['LastModified'] -and ($head.LastModified -eq $old['LastModified']))
        if ($sameEtag -or ($sameLen -and $sameMod)) { $cached = $true }
    }
    if (-not $cached) {
        Write-Host '    downloading shared pack zip ...'
        Save-ShareUrlToFile -Url $ShareUrl -OutFile $zipPath | Out-Null
        if (Test-Path -LiteralPath $extract) {
            Remove-Item -LiteralPath $extract -Recurse -Force
        }
        New-Item -ItemType Directory -Path $extract | Out-Null
        Write-Host '    unpacking shared pack ...'
        Expand-Archive -LiteralPath $zipPath -DestinationPath $extract -Force
        $etag = ''
        $len = ''
        $lm = ''
        if ($head) {
            $etag = [string]$head.ETag
            $len = [string]$head.Length
            $lm = [string]$head.LastModified
        }
        @(
            "ETag=$etag"
            "Length=$len"
            "LastModified=$lm"
        ) | Set-Content -LiteralPath $metaPath -Encoding UTF8
    } else {
        Write-Host '    using cached shared pack (unchanged).'
    }
    return $extract
}

function Save-OneDriveItemContentToFile {
    param(
        $Session,
        [string]$OutFile
    )
    $url = 'https://api.onedrive.com/v1.0/drives/{0}/items/{1}/content' -f $Session.DriveId, $Session.ItemId
    $wc = New-Object System.Net.WebClient
    try {
        $wc.Headers.Add('User-Agent', 'AGF-FriendModInstaller')
        $wc.Headers.Add('Authorization', "Badger $($Session.Token)")
        $wc.DownloadFile($url, $OutFile)
    } finally {
        $wc.Dispose()
    }
}

function Get-GDriveFolderId {
    param([string]$Url)
    if ($Url -match 'drive\.google\.com/drive/folders/([A-Za-z0-9_-]+)') { return $Matches[1] }
    if ($Url -match 'drive\.google\.com/open\?id=([A-Za-z0-9_-]+)') { return $Matches[1] }
    if ($Url -match 'embeddedfolderview\?id=([A-Za-z0-9_-]+)') { return $Matches[1] }
    if ($Url -match '[?&]id=([A-Za-z0-9_-]{25,})') { return $Matches[1] }
    return ''
}

function Get-GDriveFolderUserAgent {
    return 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36'
}

function Get-HttpText {
    param(
        [string]$Url,
        [System.Net.CookieContainer]$Cookies,
        [string]$UserAgent
    )
    if (-not $Cookies) { $Cookies = New-Object System.Net.CookieContainer }
    if (-not $UserAgent) { $UserAgent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) ClientModSync' }
    $req = [System.Net.HttpWebRequest]::Create($Url)
    $req.Method = 'GET'
    $req.AllowAutoRedirect = $true
    $req.Timeout = 30000
    $req.UserAgent = $UserAgent
    $req.CookieContainer = $Cookies
    $resp = $req.GetResponse()
    try {
        $stream = $resp.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
        try {
            return $reader.ReadToEnd()
        } finally {
            $reader.Close()
        }
    } finally {
        $resp.Close()
    }
}

function Get-GDriveFolderListing {
    param(
        [string]$FolderId,
        [System.Net.CookieContainer]$Cookies
    )
    if (-not $Cookies) {
        if (-not $script:GDriveCookies) { $script:GDriveCookies = New-Object System.Net.CookieContainer }
        $Cookies = $script:GDriveCookies
    }
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $url = 'https://drive.google.com/embeddedfolderview?id={0}' -f $FolderId
    $html = Get-HttpText -Url $url -Cookies $Cookies -UserAgent (Get-GDriveFolderUserAgent)
    if ($html -match '(?i)you need permission|request access|sign in') {
        throw 'Google Drive says this folder is not viewable by anyone with the link. Set sharing to Anyone with the link can view.'
    }
    $name = ''
    if ($html -match '(?i)<title>([^<]+)</title>') {
        $name = [System.Net.WebUtility]::HtmlDecode($Matches[1]).Trim()
        $name = $name -replace '\s*[–-]\s*Google Drive\s*$', ''
    }
    $files = New-Object System.Collections.ArrayList
    $folders = New-Object System.Collections.ArrayList
    $seen = @{}
    $entryRe = [regex]'<div class="flip-entry" id="entry-([^"]+)"[\s\S]*?<div class="flip-entry-title">([^<]+)</div>'
    foreach ($m in $entryRe.Matches($html)) {
        $id = $m.Groups[1].Value
        $childName = [System.Net.WebUtility]::HtmlDecode($m.Groups[2].Value).Trim()
        if (-not $id -or $seen.ContainsKey($id)) { continue }
        if ($id -eq $FolderId) { continue }
        $seen[$id] = $true
        $chunk = $m.Value
        $isFolder = ($chunk -match 'drive-sprite-folder') -or ($chunk -match '/drive/folders/')
        if ($isFolder) {
            [void]$folders.Add([pscustomobject]@{ Id = $id; Name = $childName })
        } else {
            [void]$files.Add([pscustomobject]@{ Id = $id; Name = $childName })
        }
    }
    return @{
        Name    = $name
        Files   = @($files.ToArray())
        Folders = @($folders.ToArray())
    }
}

function Get-GDriveFileText {
    param(
        [string]$FileId,
        [System.Net.CookieContainer]$Cookies
    )
    $tmp = Join-Path $env:TEMP ('AGF-GDrive-{0}.txt' -f [guid]::NewGuid().ToString('N'))
    try {
        Save-GDriveFileIdToFile -FileId $FileId -OutFile $tmp -Cookies $Cookies
        return [System.IO.File]::ReadAllText($tmp)
    } finally {
        if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue }
    }
}

function Save-GDriveFileIdToFile {
    param(
        [string]$FileId,
        [string]$OutFile,
        [System.Net.CookieContainer]$Cookies
    )
    if (-not $Cookies) {
        if (-not $script:GDriveCookies) { $script:GDriveCookies = New-Object System.Net.CookieContainer }
        $Cookies = $script:GDriveCookies
    }
    $url = 'https://drive.google.com/uc?export=download&id={0}' -f $FileId
    $info = Save-HttpDownloadToFile -Url $url -OutFile $OutFile -Cookies $Cookies
    $head = ''
    try {
        $len = (Get-Item -LiteralPath $OutFile).Length
        if ($len -gt 0 -and $len -lt 262144) {
            $text = [System.IO.File]::ReadAllText($OutFile)
            if ($text.Length -gt 8000) { $head = $text.Substring(0, 8000) } else { $head = $text }
        }
    } catch {
        $head = ''
    }
    $looksHtml = ($head -match '(?i)<html|<!doctype') -or ([string]$info.Type -match 'text/html')
    if ($looksHtml) {
        $confirm = 't'
        if ($head -match 'confirm=([0-9A-Za-z_-]+)') { $confirm = $Matches[1] }
        $retry = 'https://drive.google.com/uc?export=download&confirm={0}&id={1}' -f $confirm, $FileId
        $null = Save-HttpDownloadToFile -Url $retry -OutFile $OutFile -Cookies $Cookies
    }
}

function Get-GDriveRemoteMods {
    param(
        [string]$FolderId,
        [System.Net.CookieContainer]$Cookies
    )
    if (-not $Cookies) {
        if (-not $script:GDriveCookies) { $script:GDriveCookies = New-Object System.Net.CookieContainer }
        $Cookies = $script:GDriveCookies
    }
    $root = Get-GDriveFolderListing -FolderId $FolderId -Cookies $Cookies
    $modFolders = @($root.Folders)
    $probe = @()
    foreach ($d in $modFolders) {
        $kids = Get-GDriveFolderListing -FolderId $d.Id -Cookies $Cookies
        $hasInfo = @($kids.Files | Where-Object { $_.Name -eq 'ModInfo.xml' }).Count -gt 0
        if ($hasInfo -or (Test-IsHarmonyFolderName $d.Name)) { $probe += $d }
    }
    if ($probe.Count -eq 0 -and $modFolders.Count -eq 1) {
        $FolderId = $modFolders[0].Id
        $root = Get-GDriveFolderListing -FolderId $FolderId -Cookies $Cookies
        $modFolders = @($root.Folders)
    }
    $mods = New-Object System.Collections.ArrayList
    foreach ($d in $modFolders) {
        $name = $d.Name
        if ($name -like 'INSTALL*') { continue }
        if ($name.StartsWith('.')) { continue }
        $kids = Get-GDriveFolderListing -FolderId $d.Id -Cookies $Cookies
        $infoItem = @($kids.Files | Where-Object { $_.Name -eq 'ModInfo.xml' } | Select-Object -First 1)
        $hasInfo = [bool]$infoItem
        if (-not $hasInfo -and -not (Test-IsHarmonyFolderName $name)) { continue }
        $ver = ''
        if ($hasInfo) {
            try {
                $ver = Get-ModInfoVersionFromXml (Get-GDriveFileText -FileId $infoItem.Id -Cookies $Cookies)
            } catch {
                $ver = ''
            }
        }
        [void]$mods.Add([pscustomobject]@{
            Name     = $name
            Version  = $ver
            Kind     = 'gdrive'
            FolderId = $d.Id
        })
    }
    return @($mods.ToArray() | Sort-Object Name)
}

function Get-GDriveDownloadJobs {
    param(
        [string]$FolderId,
        [string]$LocalDir,
        [System.Collections.ArrayList]$Jobs,
        [System.Net.CookieContainer]$Cookies
    )
    if (-not (Test-Path -LiteralPath $LocalDir)) {
        New-Item -ItemType Directory -Path $LocalDir | Out-Null
    }
    $listing = Get-GDriveFolderListing -FolderId $FolderId -Cookies $Cookies
    foreach ($f in @($listing.Files)) {
        $dest = Join-Path $LocalDir $f.Name
        [void]$Jobs.Add([pscustomobject]@{ Id = $f.Id; Dest = $dest })
        if (($Jobs.Count % 20) -eq 0) {
            Write-Host ("`r    listing files ... {0}" -f $Jobs.Count) -NoNewline
        }
    }
    foreach ($d in @($listing.Folders)) {
        Get-GDriveDownloadJobs -FolderId $d.Id -LocalDir (Join-Path $LocalDir $d.Name) -Jobs $Jobs -Cookies $Cookies
    }
}

function Invoke-GDriveJobDownloads {
    param(
        [object[]]$Jobs,
        [string]$Label
    )
    if ($null -eq $Jobs -or $Jobs.Count -eq 0) { return }
    $parallel = 8
    [System.Net.ServicePointManager]::DefaultConnectionLimit = 48
    $n = $Jobs.Count
    $worker = {
        param([string]$FileId, [string]$Dest)
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $tmp = $Dest + '.part'
        $cookies = New-Object System.Net.CookieContainer
        $dir = Split-Path -Parent $Dest
        if (-not (Test-Path -LiteralPath $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
        }
        $download = {
            param([string]$Url, [string]$OutFile, [System.Net.CookieContainer]$Cookies)
            $req = [System.Net.HttpWebRequest]::Create($Url)
            $req.Method = 'GET'
            $req.AllowAutoRedirect = $true
            $req.Timeout = 120000
            $req.ReadWriteTimeout = 300000
            $req.UserAgent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) ClientModSync'
            $req.CookieContainer = $Cookies
            $resp = $req.GetResponse()
            try {
                $stream = $resp.GetResponseStream()
                $fs = [System.IO.File]::Create($OutFile)
                try {
                    $buffer = New-Object byte[] 262144
                    while (($read = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                        $fs.Write($buffer, 0, $read)
                    }
                } finally {
                    $fs.Close()
                    $stream.Close()
                }
                return [string]$resp.ContentType
            } finally {
                $resp.Close()
            }
        }
        try {
            $url = 'https://drive.google.com/uc?export=download&id={0}' -f $FileId
            $ctype = & $download $url $tmp $cookies
            $head = ''
            $len = 0
            if (Test-Path -LiteralPath $tmp) { $len = (Get-Item -LiteralPath $tmp).Length }
            if ($len -gt 0 -and $len -lt 262144) {
                try { $head = [System.IO.File]::ReadAllText($tmp) } catch { $head = '' }
                if ($head.Length -gt 8000) { $head = $head.Substring(0, 8000) }
            }
            if (($head -match '(?i)<html|<!doctype') -or ($ctype -match 'text/html')) {
                $confirm = 't'
                if ($head -match 'confirm=([0-9A-Za-z_-]+)') { $confirm = $Matches[1] }
                $retry = 'https://drive.google.com/uc?export=download&confirm={0}&id={1}' -f $confirm, $FileId
                $null = & $download $retry $tmp $cookies
            }
            if (Test-Path -LiteralPath $Dest) { Remove-Item -LiteralPath $Dest -Force }
            Move-Item -LiteralPath $tmp -Destination $Dest -Force
        } finally {
            if (Test-Path -LiteralPath $tmp) {
                Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
            }
        }
    }

    $pool = [runspacefactory]::CreateRunspacePool(1, $parallel)
    $pool.Open()
    $queue = New-Object System.Collections.Queue
    foreach ($j in $Jobs) { [void]$queue.Enqueue($j) }
    $active = New-Object System.Collections.ArrayList
    $finished = 0
    $fail = 0
    $failMsg = New-Object System.Collections.ArrayList
    Write-Host ("    {0}: 0 / {1} files" -f $Label, $n) -NoNewline
    while ($queue.Count -gt 0 -or $active.Count -gt 0) {
        while ($active.Count -lt $parallel -and $queue.Count -gt 0) {
            $j = $queue.Dequeue()
            $ps = [powershell]::Create()
            $ps.RunspacePool = $pool
            [void]$ps.AddScript($worker).AddArgument([string]$j.Id).AddArgument([string]$j.Dest)
            $handle = $ps.BeginInvoke()
            [void]$active.Add([pscustomobject]@{ PS = $ps; Handle = $handle; Dest = $j.Dest })
        }
        Start-Sleep -Milliseconds 200
        $still = New-Object System.Collections.ArrayList
        foreach ($w in @($active.ToArray())) {
            if (-not $w.Handle.IsCompleted) {
                [void]$still.Add($w)
                continue
            }
            $finished++
            try {
                $null = $w.PS.EndInvoke($w.Handle)
            } catch {
                $fail++
                [void]$failMsg.Add(("{0}: {1}" -f (Split-Path -Leaf $w.Dest), $_.Exception.Message))
            } finally {
                $w.PS.Dispose()
            }
        }
        $active = $still
        Write-Host ("`r    {0}: {1} / {2} files" -f $Label, $finished, $n) -NoNewline
    }
    Write-Host ''
    $pool.Close()
    $pool.Dispose()
    if ($fail -gt 0) {
        $sample = ($failMsg | Select-Object -First 3) -join '; '
        throw ("{0} file(s) failed to download ({1})" -f $fail, $sample)
    }
}

function Save-GDriveModFolder {
    param(
        $RemoteMod,
        [string]$DestParent
    )
    $dest = Join-Path $DestParent $RemoteMod.Name
    if (Test-Path -LiteralPath $dest) {
        Remove-Item -LiteralPath $dest -Recurse -Force
    }
    Write-Host ("    listing files in {0} ..." -f $RemoteMod.Name)
    $jobs = New-Object System.Collections.ArrayList
    Get-GDriveDownloadJobs -FolderId $RemoteMod.FolderId -LocalDir $dest -Jobs $jobs -Cookies $script:GDriveCookies
    if ($jobs.Count -gt 0) {
        Write-Host ("`r    listing files in {0} ... {1}" -f $RemoteMod.Name, $jobs.Count)
    }
    Invoke-GDriveJobDownloads -Jobs @($jobs.ToArray()) -Label $RemoteMod.Name
}

function Get-RemoteModsFromShareUrl {
    param([string]$ShareUrl)
    $kind = Get-ShareKind $ShareUrl
    if ($kind -eq 'unsupported-folder') {
        throw 'This link is a cloud folder this updater cannot list. Zip the mod folders and share that zip (anyone with the link, no login), or use a view-only OneDrive or Google Drive folder.'
    }
    if ($kind -eq 'gdrive') {
        $id = Get-GDriveFolderId $ShareUrl
        if (-not $id) { throw 'Could not read a Google Drive folder id from that link.' }
        if (-not $script:GDriveCookies) { $script:GDriveCookies = New-Object System.Net.CookieContainer }
        $listing = Get-GDriveFolderListing -FolderId $id -Cookies $script:GDriveCookies
        $hasStuff = (@($listing.Files).Count -gt 0) -or (@($listing.Folders).Count -gt 0)
        if (-not $hasStuff -and $ShareUrl -notmatch '/folders/') {
            # open?id= is used for both files and folders; empty listing means try as a zip file.
        } else {
            if ($listing.Name) { Write-Host ("    Google Drive folder: {0}" -f $listing.Name) }
            return @{
                Kind    = 'gdrive'
                Session = $null
                Mods    = @(Get-GDriveRemoteMods -FolderId $id -Cookies $script:GDriveCookies)
                Name    = $listing.Name
            }
        }
    }
    if ($kind -eq 'onedrive') {
        $session = Connect-OneDriveShare -ShareUrl $ShareUrl
        if ($session.IsFile) {
            $fileName = [string]$session.Name
            if ($fileName -notlike '*.zip') {
                throw 'The link points to a single file that is not a zip. Share a folder of unzipped mods, or a .zip of those folders.'
            }
            Write-Host ("    OneDrive file: {0}" -f $fileName)
            $key = Get-ShareUrlCacheKey $ShareUrl
            $cacheRoot = Join-Path $env:LOCALAPPDATA ('AGF-ClientModSync\cache\{0}' -f $key)
            $zipPath = Join-Path $cacheRoot 'pack.zip'
            $extract = Join-Path $cacheRoot 'extract'
            if (-not (Test-Path -LiteralPath $cacheRoot)) {
                New-Item -ItemType Directory -Path $cacheRoot -Force | Out-Null
            }
            Write-Host '    downloading shared pack zip ...'
            Save-OneDriveItemContentToFile -Session $session -OutFile $zipPath
            if (-not (Test-FileIsZip $zipPath)) {
                throw 'The shared OneDrive file is not a zip of the mod folders.'
            }
            if (Test-Path -LiteralPath $extract) {
                Remove-Item -LiteralPath $extract -Recurse -Force
            }
            New-Item -ItemType Directory -Path $extract | Out-Null
            Write-Host '    unpacking shared pack ...'
            Expand-Archive -LiteralPath $zipPath -DestinationPath $extract -Force
            return @{
                Kind    = 'zip'
                Session = $null
                Mods    = @(Get-RemoteModsFromExtractedPack $extract)
            }
        }
        return @{
            Kind    = 'onedrive'
            Session = $session
            Mods    = @(Get-OneDriveRemoteMods -Session $session)
            Name    = $session.Name
        }
    }
    $extract = Get-ExtractedSharePack -ShareUrl $ShareUrl
    return @{
        Kind    = 'zip'
        Session = $null
        Mods    = @(Get-RemoteModsFromExtractedPack $extract)
    }
}

function Test-IsGameExeFolder {
    param([string]$Dir)
    foreach ($exe in @('7DaysToDie.exe', '7DaysToDieServer.exe', '7DaysToDie_EAC.exe')) {
        if (Test-Path -LiteralPath (Join-Path $Dir $exe)) { return $true }
    }
    return $false
}

function Resolve-GameModsFolder {
    param([string]$StartDir)
    $dir = $StartDir.TrimEnd('\', '/')
    for ($i = 0; $i -lt 5; $i++) {
        if (Test-IsGameExeFolder $dir) {
            $mods = Join-Path $dir 'Mods'
            if (-not (Test-Path -LiteralPath $mods)) {
                New-Item -ItemType Directory -Path $mods | Out-Null
            }
            return (Resolve-Path -LiteralPath $mods).Path
        }
        $parent = Split-Path -Parent $dir
        if (-not $parent -or $parent -eq $dir) { break }
        $dir = $parent
    }
    $leaf = Split-Path -Leaf $StartDir
    if ($leaf.Equals('Mods', [StringComparison]::OrdinalIgnoreCase)) {
        return (Resolve-Path -LiteralPath $StartDir).Path
    }
    $dirs = @(Get-ChildItem -LiteralPath $StartDir -Directory -ErrorAction SilentlyContinue)
    foreach ($d in $dirs) {
        if (Test-Path -LiteralPath (Join-Path $d.FullName 'ModInfo.xml')) {
            return (Resolve-Path -LiteralPath $StartDir).Path
        }
    }
    return $null
}

function Test-LooksLikeModsFolder {
    param([string]$Dir)
    return [bool](Resolve-GameModsFolder $Dir)
}

function Invoke-OneDriveApi {
    param(
        [string]$Uri,
        [string]$Token,
        [switch]$AllowEmpty
    )
    $headers = @{
        'Accept'          = 'application/json'
        'Prefer'          = 'autoredeem'
        'User-Agent'      = 'AGF-FriendModInstaller'
    }
    if ($Token) { $headers['Authorization'] = "Badger $Token" }
    return Invoke-RestMethod -Method Get -Uri $Uri -Headers $headers -TimeoutSec 60
}

function Get-OneDriveBadgerToken {
    $resp = Invoke-RestMethod -Method Post -Uri 'https://api-badgerp.svc.ms/v1.0/token' `
        -ContentType 'application/json' `
        -Body '{"appId":"5cbed6ac-a083-4e14-b191-b4ba07653de2"}' `
        -TimeoutSec 30
    if (-not $resp.token) { throw 'OneDrive did not return an access token.' }
    return [string]$resp.token
}

function Expand-OneDriveShareUrl {
    param([string]$Url)
    try {
        $req = [System.Net.HttpWebRequest]::Create($Url)
        $req.AllowAutoRedirect = $false
        $req.Method = 'GET'
        $req.Timeout = 20000
        $req.UserAgent = 'AGF-FriendModInstaller'
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

function Connect-OneDriveShare {
    param([string]$ShareUrl)
    if ($ShareUrl -match 'sharepoint\.com') {
        throw 'That looks like work/school OneDrive. Use a personal OneDrive folder link (1drv.ms or onedrive.live.com) set to Anyone with the link can view.'
    }
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $token = Get-OneDriveBadgerToken
    $candidates = @($ShareUrl.Trim())
    $expanded = Expand-OneDriveShareUrl $ShareUrl
    if ($expanded -and ($expanded -ne $ShareUrl)) { $candidates += $expanded }

    $lastError = ''
    foreach ($candidate in $candidates) {
        $shareId = ConvertTo-OneDriveShareId $candidate
        try {
            $redeemUri = "https://my.microsoftpersonalcontent.com/_api/v2.0/shares/$shareId/driveitem?`$select=id,parentReference"
            $null = Invoke-OneDriveApi -Uri $redeemUri -Token $token
        } catch {
            $lastError = $_.Exception.Message
        }
        try {
            $root = Invoke-OneDriveApi -Uri "https://api.onedrive.com/v1.0/shares/$shareId/driveItem" -Token $token
            $driveId = [string]$root.parentReference.driveId
            $itemId = [string]$root.id
            if (-not $driveId -or -not $itemId) { throw 'OneDrive did not return folder ids.' }
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
    throw ("Could not open the OneDrive folder link. Share it as Anyone with the link can view. Details: {0}" -f $lastError)
}

function Get-OneDriveChildren {
    param(
        [string]$DriveId,
        [string]$ItemId,
        [string]$Token
    )
    $out = New-Object System.Collections.ArrayList
    $uri = "https://api.onedrive.com/v1.0/drives/$DriveId/items/${ItemId}/children?`$top=200"
    while ($uri) {
        $page = Invoke-OneDriveApi -Uri $uri -Token $Token
        $vals = @()
        if (Test-ODHasFacet $page 'value') { $vals = @($page.value) }
        foreach ($item in $vals) {
            [void]$out.Add($item)
        }
        $uri = $null
        if (Test-ODHasFacet $page '@odata.nextLink') {
            $uri = [string]$page.'@odata.nextLink'
        }
    }
    return @($out.ToArray())
}

function Test-IsHarmonyFolderName {
    param([string]$Name)
    return $Name.Equals('0_TFP_Harmony', [StringComparison]::OrdinalIgnoreCase)
}

function Get-ModInfoVersionFromXml {
    param([string]$XmlText)
    if (-not $XmlText) { return '' }
    try {
        $xml = New-Object System.Xml.XmlDocument
        $xml.XmlResolver = $null
        $xml.LoadXml($XmlText)
    } catch {
        return ''
    }
    $nodes = $xml.SelectNodes('//*[local-name()="Version"]')
    foreach ($n in @($nodes)) {
        if ($n.Attributes -and $n.Attributes['value'] -and $n.Attributes['value'].Value) {
            return ([string]$n.Attributes['value'].Value).Trim()
        }
        $inner = ([string]$n.InnerText).Trim()
        if ($inner) { return $inner }
    }
    return ''
}

function Get-ModInfoVersionFromFile {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return '' }
    try {
        $text = [System.IO.File]::ReadAllText($Path)
        return (Get-ModInfoVersionFromXml $text)
    } catch {
        return ''
    }
}

function Get-OneDriveItemText {
    param(
        $Item,
        [string]$DriveId,
        [string]$Token
    )
    $url = $null
    if (Test-ODHasFacet $Item '@content.downloadUrl') {
        $url = [string]$Item.'@content.downloadUrl'
    }
    if (-not $url) {
        $url = "https://api.onedrive.com/v1.0/drives/$DriveId/items/$([string]$Item.id)/content"
    }
    $wc = New-Object System.Net.WebClient
    $wc.Encoding = [System.Text.Encoding]::UTF8
    $wc.Headers.Add('User-Agent', 'AGF-FriendModInstaller')
    if ($Token -and ($url -like 'https://api.onedrive.com/*')) {
        $wc.Headers.Add('Authorization', "Badger $Token")
    }
    try {
        return [string]$wc.DownloadString($url)
    } finally {
        $wc.Dispose()
    }
}

function Get-OneDriveRemoteMods {
    param($Session)
    $mods = New-Object System.Collections.ArrayList
    if ($Session.IsFile) {
        return @()
    }
    $children = @(Get-OneDriveChildren -DriveId $Session.DriveId -ItemId $Session.ItemId -Token $Session.Token)
    foreach ($child in $children) {
        if (-not (Test-ODHasFacet $child 'folder')) { continue }
        $name = [string]$child.name
        if ($name -like 'INSTALL*') { continue }
        if ($name.StartsWith('.')) { continue }
        $grand = @(Get-OneDriveChildren -DriveId $Session.DriveId -ItemId ([string]$child.id) -Token $Session.Token)
        $infoItem = $null
        foreach ($g in $grand) {
            if ([string]$g.name -eq 'ModInfo.xml') { $infoItem = $g; break }
        }
        $hasInfo = [bool]$infoItem
        if (-not $hasInfo -and -not (Test-IsHarmonyFolderName $name)) { continue }
        $when = [datetimeoffset]::MinValue
        if (Test-ODHasFacet $child 'lastModifiedDateTime') {
            try {
                $rawWhen = $child.lastModifiedDateTime
                if ($rawWhen -is [datetimeoffset]) {
                    $when = $rawWhen
                } elseif ($rawWhen -is [datetime]) {
                    $when = [datetimeoffset]::new([datetime]$rawWhen)
                } else {
                    $when = [datetimeoffset]::Parse([string]$rawWhen, [System.Globalization.CultureInfo]::InvariantCulture)
                }
            } catch {
                $when = [datetimeoffset]::MinValue
            }
        }
        $ver = ''
        if ($infoItem) {
            try {
                $ver = Get-ModInfoVersionFromXml (Get-OneDriveItemText -Item $infoItem -DriveId $Session.DriveId -Token $Session.Token)
            } catch {
                $ver = ''
            }
        }
        [void]$mods.Add([pscustomobject]@{
            Name         = $name
            Id           = [string]$child.id
            DriveId      = $Session.DriveId
            LastModified = $when
            Version      = $ver
        })
    }
    return @($mods.ToArray() | Sort-Object Name)
}

function Get-OneDriveDownloadJobs {
    param(
        [string]$DriveId,
        $Item,
        [string]$LocalDir,
        [string]$Token,
        [System.Collections.ArrayList]$Jobs
    )
    $name = [string]$Item.name
    $dest = Join-Path $LocalDir $name
    if (Test-ODHasFacet $Item 'folder') {
        if (-not (Test-Path -LiteralPath $dest)) {
            New-Item -ItemType Directory -Path $dest | Out-Null
        }
        $kids = @(Get-OneDriveChildren -DriveId $DriveId -ItemId ([string]$Item.id) -Token $Token)
        foreach ($kid in $kids) {
            Get-OneDriveDownloadJobs -DriveId $DriveId -Item $kid -LocalDir $dest -Token $Token -Jobs $Jobs
        }
        return
    }
    $url = $null
    if (Test-ODHasFacet $Item '@content.downloadUrl') {
        $url = [string]$Item.'@content.downloadUrl'
    }
    if (-not $url) {
        $url = "https://api.onedrive.com/v1.0/drives/$DriveId/items/$([string]$Item.id)/content"
    }
    [void]$Jobs.Add([pscustomobject]@{ Url = $url; Dest = $dest })
    if (($Jobs.Count % 20) -eq 0) {
        Write-Host ("`r    listing files ... {0}" -f $Jobs.Count) -NoNewline
    }
}

function Invoke-OneDriveJobDownloads {
    param(
        [object[]]$Jobs,
        [string]$Token,
        [string]$Label
    )
    if ($null -eq $Jobs -or $Jobs.Count -eq 0) { return }
    $parallel = 16
    [System.Net.ServicePointManager]::DefaultConnectionLimit = 48
    $n = $Jobs.Count
    $worker = {
        param([string]$Url, [string]$Dest, [string]$Token)
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $tmp = $Dest + '.part'
        $wc = New-Object System.Net.WebClient
        try {
            $dir = Split-Path -Parent $Dest
            if (-not (Test-Path -LiteralPath $dir)) {
                New-Item -ItemType Directory -Path $dir -Force | Out-Null
            }
            $wc.Headers.Add('User-Agent', 'AGF-FriendModInstaller')
            if ($Token -and ($Url -like 'https://api.onedrive.com/*')) {
                $wc.Headers.Add('Authorization', "Badger $Token")
            }
            $ok = $false
            $last = $null
            for ($a = 1; $a -le 3; $a++) {
                try {
                    if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Force }
                    $wc.DownloadFile($Url, $tmp)
                    $ok = $true
                    break
                } catch {
                    $last = $_
                    Start-Sleep -Seconds $a
                }
            }
            if (-not $ok) { throw $last.Exception }
            if (Test-Path -LiteralPath $Dest) { Remove-Item -LiteralPath $Dest -Force }
            Move-Item -LiteralPath $tmp -Destination $Dest -Force
        } finally {
            $wc.Dispose()
            if (Test-Path -LiteralPath $tmp) {
                Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
            }
        }
    }

    $pool = [runspacefactory]::CreateRunspacePool(1, $parallel)
    $pool.Open()
    $queue = New-Object System.Collections.Queue
    foreach ($j in $Jobs) { [void]$queue.Enqueue($j) }
    $active = New-Object System.Collections.ArrayList
    $finished = 0
    $fail = 0
    $failMsg = New-Object System.Collections.ArrayList

    Write-Host ("    {0}: 0 / {1} files" -f $Label, $n) -NoNewline
    while ($queue.Count -gt 0 -or $active.Count -gt 0) {
        while ($active.Count -lt $parallel -and $queue.Count -gt 0) {
            $j = $queue.Dequeue()
            $ps = [powershell]::Create()
            $ps.RunspacePool = $pool
            [void]$ps.AddScript($worker).AddArgument([string]$j.Url).AddArgument([string]$j.Dest).AddArgument([string]$Token)
            $handle = $ps.BeginInvoke()
            [void]$active.Add([pscustomobject]@{ PS = $ps; Handle = $handle; Dest = $j.Dest })
        }
        Start-Sleep -Milliseconds 200
        $still = New-Object System.Collections.ArrayList
        foreach ($w in @($active.ToArray())) {
            if (-not $w.Handle.IsCompleted) {
                [void]$still.Add($w)
                continue
            }
            $finished++
            try {
                $null = $w.PS.EndInvoke($w.Handle)
            } catch {
                $fail++
                [void]$failMsg.Add(("{0}: {1}" -f (Split-Path -Leaf $w.Dest), $_.Exception.Message))
            } finally {
                $w.PS.Dispose()
            }
        }
        $active = $still
        Write-Host ("`r    {0}: {1} / {2} files" -f $Label, $finished, $n) -NoNewline
    }
    Write-Host ''
    $pool.Close()
    $pool.Dispose()
    if ($fail -gt 0) {
        $sample = ($failMsg | Select-Object -First 3) -join '; '
        throw ("{0} file(s) failed to download ({1})" -f $fail, $sample)
    }
}

function Save-OneDriveItemTree {
    param(
        [string]$DriveId,
        $Item,
        [string]$LocalDir,
        [string]$Token
    )
    $jobs = New-Object System.Collections.ArrayList
    Get-OneDriveDownloadJobs -DriveId $DriveId -Item $Item -LocalDir $LocalDir -Token $Token -Jobs $jobs
    Invoke-OneDriveJobDownloads -Jobs @($jobs.ToArray()) -Token $Token -Label ([string]$Item.name)
}

function Save-OneDriveModFolder {
    param(
        $RemoteMod,
        [string]$DestParent,
        [string]$Token
    )
    $meta = Invoke-OneDriveApi -Uri ("https://api.onedrive.com/v1.0/drives/{0}/items/{1}" -f $RemoteMod.DriveId, $RemoteMod.Id) -Token $Token
    $dest = Join-Path $DestParent $RemoteMod.Name
    if (Test-Path -LiteralPath $dest) {
        Remove-Item -LiteralPath $dest -Recurse -Force
    }
    Write-Host ("    listing files in {0} ..." -f $RemoteMod.Name)
    $jobs = New-Object System.Collections.ArrayList
    Get-OneDriveDownloadJobs -DriveId $RemoteMod.DriveId -Item $meta -LocalDir $DestParent -Token $Token -Jobs $jobs
    if ($jobs.Count -gt 0) {
        Write-Host ("`r    listing files in {0} ... {1}" -f $RemoteMod.Name, $jobs.Count)
    }
    Invoke-OneDriveJobDownloads -Jobs @($jobs.ToArray()) -Token $Token -Label $RemoteMod.Name
}

function Test-LocalModIsCurrent {
    param(
        [string]$ModsDir,
        $RemoteMod
    )
    $local = Join-Path $ModsDir $RemoteMod.Name
    if (Test-IsHarmonyFolderName $RemoteMod.Name) {
        $hasInfo = Test-Path -LiteralPath (Join-Path $local 'ModInfo.xml')
        $hasDll = Test-Path -LiteralPath (Join-Path $local '0Harmony.dll')
        return ($hasInfo -or $hasDll)
    }
    $localInfo = Join-Path $local 'ModInfo.xml'
    if (-not (Test-Path -LiteralPath $localInfo)) { return $false }
    $localVer = Get-ModInfoVersionFromFile $localInfo
    $remoteVer = ''
    if ($RemoteMod.PSObject.Properties.Name -contains 'Version') {
        $remoteVer = ([string]$RemoteMod.Version).Trim()
    }
    if ($localVer -and $remoteVer) {
        return $localVer.Equals($remoteVer, [StringComparison]::OrdinalIgnoreCase)
    }
    return $false
}
