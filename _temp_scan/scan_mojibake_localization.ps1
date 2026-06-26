$ErrorActionPreference = 'Stop'
$root = 'C:\GitHub\7D2D-Mods'
$draftRoot = Join-Path $root '01_Draft'
$sourceRootA = Join-Path $root '_x2.6'
$sourceRootB = Join-Path $root '_xObsolete'

$draftFiles = Get-ChildItem -LiteralPath $draftRoot -Recurse -File -Filter 'Localization.csv' |
    Where-Object { $_.FullName -match '\\Config\\Localization\.csv$' }

$sourceFiles = @()
if (Test-Path $sourceRootA) {
    $sourceFiles += Get-ChildItem -LiteralPath $sourceRootA -Recurse -File -Filter 'Localization.txt'
}
if (Test-Path $sourceRootB) {
    $sourceFiles += Get-ChildItem -LiteralPath $sourceRootB -Recurse -File -Filter 'Localization.txt'
}

$sourceByMod = @{}
foreach ($sf in $sourceFiles) {
    $mod = $sf.Directory.Parent.Name
    if (-not $sourceByMod.ContainsKey($mod)) {
        $sourceByMod[$mod] = @()
    }
    $sourceByMod[$mod] += $sf.FullName
}

function Fix-Latin1ToUtf8([string]$s) {
    if ([string]::IsNullOrEmpty($s)) { return $null }
    try {
        $bytes = [System.Text.Encoding]::GetEncoding('iso-8859-1').GetBytes($s)
        return [System.Text.Encoding]::UTF8.GetString($bytes)
    } catch {
        return $null
    }
}

function Fix-Cp1252ToUtf8([string]$s) {
    if ([string]::IsNullOrEmpty($s)) { return $null }
    try {
        $bytes = [System.Text.Encoding]::GetEncoding(1252).GetBytes($s)
        return [System.Text.Encoding]::UTF8.GetString($bytes)
    } catch {
        return $null
    }
}

$flags = @()
$missingSource = @()

foreach ($df in $draftFiles) {
    $mod = $df.Directory.Parent.Name
    if (-not $sourceByMod.ContainsKey($mod)) {
        $missingSource += $df.FullName
        continue
    }

    $sourcePath = $sourceByMod[$mod] | Select-Object -First 1
    $draftRows = Import-Csv -LiteralPath $df.FullName
    $sourceRows = Import-Csv -LiteralPath $sourcePath

    $srcByKey = @{}
    foreach ($r in $sourceRows) {
        $k = $r.Key
        if (-not [string]::IsNullOrEmpty($k) -and -not $srcByKey.ContainsKey($k)) {
            $srcByKey[$k] = $r
        }
    }

    $draftCols = @()
    if ($draftRows.Count -gt 0) {
        $draftCols = $draftRows[0].PSObject.Properties.Name
    }

    foreach ($dr in $draftRows) {
        $key = $dr.Key
        if ([string]::IsNullOrEmpty($key)) { continue }
        if (-not $srcByKey.ContainsKey($key)) { continue }

        $sr = $srcByKey[$key]

        foreach ($col in $draftCols) {
            if ($col -eq 'KeepLoaded') { continue }
            if (-not ($sr.PSObject.Properties.Name -contains $col)) { continue }

            $dv = [string]$dr.$col
            $sv = [string]$sr.$col
            if ($dv -eq $sv) { continue }
            if ([string]::IsNullOrEmpty($dv) -or [string]::IsNullOrEmpty($sv)) { continue }

            $latinFixed = Fix-Latin1ToUtf8 $dv
            $cpFixed = Fix-Cp1252ToUtf8 $dv
            $isEncodingEquivalent = ($latinFixed -eq $sv) -or ($cpFixed -eq $sv)

            if ($isEncodingEquivalent) {
                $flags += [pscustomobject]@{
                    Mod = $mod
                    File = $df.FullName
                    Key = $key
                    Column = $col
                    DraftSample = $dv.Substring(0, [Math]::Min(120, $dv.Length))
                    SourceSample = $sv.Substring(0, [Math]::Min(120, $sv.Length))
                    EncodingEquivalent = $isEncodingEquivalent
                }
            }
        }
    }
}

Write-Output ("DRAFT_FILES=" + $draftFiles.Count)
Write-Output ("SOURCE_FILES=" + $sourceFiles.Count)
Write-Output ("MISSING_SOURCE_COUNT=" + $missingSource.Count)
$missingSource | Sort-Object | ForEach-Object { Write-Output ("MISSING_SOURCE=" + $_) }
Write-Output ("FLAGGED_CELL_COUNT=" + $flags.Count)
$flags | Sort-Object Mod, Key, Column | ForEach-Object {
    Write-Output ("FLAG|" + $_.Mod + "|" + $_.Key + "|" + $_.Column + "|" + $_.EncodingEquivalent + "|" + $_.DraftSample + "|" + $_.SourceSample)
}
