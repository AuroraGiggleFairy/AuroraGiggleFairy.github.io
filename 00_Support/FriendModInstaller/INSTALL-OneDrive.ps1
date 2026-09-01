# OneDrive personal "anyone with the link" helper for INSTALL-Mods.ps1
# Work/school SharePoint links are not supported.

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
    $file = Join-Path $SourceDir 'INSTALL-ONEDRIVE.txt'
    if (Test-Path -LiteralPath $file) {
        $line = @(Get-Content -LiteralPath $file -Encoding UTF8 |
            ForEach-Object { $_.Trim().Trim('"') } |
            Where-Object { $_ -ne '' -and -not $_.StartsWith('#') } |
            Select-Object -First 1)
        if ($line.Count -gt 0 -and $line[0]) { return $line[0] }
    }
    if ($ConfigMap.ContainsKey('ONEDRIVE_SHARE_URL') -and $ConfigMap['ONEDRIVE_SHARE_URL']) {
        return $ConfigMap['ONEDRIVE_SHARE_URL'].Trim().Trim('"')
    }
    return ''
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
