$ErrorActionPreference = 'Stop'
$root = 'C:\GitHub\7D2D-Mods'
$draftRoot = Join-Path $root '01_Draft'
$sourceRoots = @((Join-Path $root '_x2.6'), (Join-Path $root '_xObsolete'))

$draftFiles = Get-ChildItem -LiteralPath $draftRoot -Recurse -File -Filter 'Localization.csv' |
    Where-Object { $_.FullName -match '\\Config\\Localization\.csv$' }

$sourceFiles = @()
foreach ($sr in $sourceRoots) {
    if (Test-Path $sr) {
        $sourceFiles += Get-ChildItem -LiteralPath $sr -Recurse -File -Filter 'Localization.txt'
    }
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

function LooksMojibake([string]$s) {
    if ([string]::IsNullOrEmpty($s)) { return $false }
    if ($s.Contains([char]0xFFFD)) { return $true }
    foreach ($ch in $s.ToCharArray()) {
        $code = [int][char]$ch
        if (($code -ge 0 -and $code -le 8) -or ($code -ge 11 -and $code -le 12) -or ($code -ge 14 -and $code -le 31) -or ($code -ge 127 -and $code -le 159)) {
            return $true
        }
    }
    return $false
}

# Global source key index
$sourceByKey = @{}
foreach ($sf in $sourceFiles) {
    $rows = Import-Csv -LiteralPath $sf.FullName
    foreach ($r in $rows) {
        $k = $r.Key
        if ([string]::IsNullOrEmpty($k)) { continue }
        if (-not $sourceByKey.ContainsKey($k)) {
            $sourceByKey[$k] = @()
        }
        $sourceByKey[$k] += [pscustomobject]@{ Row = $r; Path = $sf.FullName }
    }
}

$flags = @()
$noSourceRows = 0

foreach ($df in $draftFiles) {
    $draftRows = Import-Csv -LiteralPath $df.FullName
    if ($draftRows.Count -eq 0) { continue }
    $draftCols = $draftRows[0].PSObject.Properties.Name

    foreach ($dr in $draftRows) {
        $key = [string]$dr.Key
        if ([string]::IsNullOrEmpty($key)) { continue }
        if (-not $sourceByKey.ContainsKey($key)) {
            $noSourceRows++
            continue
        }

        $srcOptions = $sourceByKey[$key]
        $chosen = $srcOptions | Select-Object -First 1

        # Prefer source row where english matches exactly
        foreach ($opt in $srcOptions) {
            $svEn = [string]$opt.Row.english
            $dvEn = [string]$dr.english
            if (-not [string]::IsNullOrEmpty($svEn) -and $svEn -eq $dvEn) {
                $chosen = $opt
                break
            }
        }

        $sr = $chosen.Row

        foreach ($col in $draftCols) {
            if ($col -eq 'KeepLoaded') { continue }
            if (-not ($sr.PSObject.Properties.Name -contains $col)) { continue }

            $dv = [string]$dr.$col
            $sv = [string]$sr.$col
            if ($dv -eq $sv) { continue }
            if ([string]::IsNullOrEmpty($dv) -or [string]::IsNullOrEmpty($sv)) { continue }

            $latinFixed = Fix-Latin1ToUtf8 $dv
            $cpFixed = Fix-Cp1252ToUtf8 $dv
            $encEq = ($latinFixed -eq $sv) -or ($cpFixed -eq $sv)
            $looksBad = LooksMojibake $dv

            if ($encEq -or ($looksBad -and ($latinFixed -eq $sv -or $cpFixed -eq $sv))) {
                $flags += [pscustomobject]@{
                    File = $df.FullName
                    Key = $key
                    Column = $col
                    Draft = $dv
                    Source = $sv
                    SourcePath = $chosen.Path
                    EncodingEquivalent = $encEq
                }
            }
        }
    }
}

Write-Output ("DRAFT_FILES=" + $draftFiles.Count)
Write-Output ("SOURCE_FILES=" + $sourceFiles.Count)
Write-Output ("SOURCE_KEYS=" + $sourceByKey.Keys.Count)
Write-Output ("ROWS_WITHOUT_SOURCE_KEY=" + $noSourceRows)
Write-Output ("FLAGGED_CELLS=" + $flags.Count)
$flags | Group-Object File | Sort-Object Name | ForEach-Object {
    Write-Output ("FILE_FLAGGED=" + $_.Name + "|COUNT=" + $_.Count)
}
$flags | Sort-Object File, Key, Column | Select-Object -First 300 | ForEach-Object {
    $d = $_.Draft
    $s = $_.Source
    if ($d.Length -gt 80) { $d = $d.Substring(0,80) }
    if ($s.Length -gt 80) { $s = $s.Substring(0,80) }
    Write-Output ("FLAG|" + $_.File + "|" + $_.Key + "|" + $_.Column + "|" + $_.EncodingEquivalent + "|" + $d + "|" + $s)
}
