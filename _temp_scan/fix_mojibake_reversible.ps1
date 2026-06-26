$ErrorActionPreference = 'Stop'

$root = 'C:\GitHub\7D2D-Mods\01_Draft'
$files = Get-ChildItem -LiteralPath $root -Recurse -File -Filter 'Localization.csv' |
    Where-Object { $_.FullName -match '\\Config\\Localization\.csv$' }

function Decode-LegacyToUtf8 {
    param(
        [string]$Text,
        [int]$CodePage
    )
    if ([string]::IsNullOrEmpty($Text)) { return $null }
    try {
        $bytes = [System.Text.Encoding]::GetEncoding($CodePage).GetBytes($Text)
        return [System.Text.Encoding]::UTF8.GetString($bytes)
    } catch {
        return $null
    }
}

function Encode-Utf8AsLegacy {
    param(
        [string]$Text,
        [int]$CodePage
    )
    if ([string]::IsNullOrEmpty($Text)) { return $null }
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return [System.Text.Encoding]::GetEncoding($CodePage).GetString($bytes)
    } catch {
        return $null
    }
}

function Try-ReversibleFix {
    param([string]$Original)

    if ([string]::IsNullOrEmpty($Original)) { return $null }

    $best = $null
    foreach ($cp in @(1252, 28591)) {
        $candidate = Decode-LegacyToUtf8 -Text $Original -CodePage $cp
        if ([string]::IsNullOrEmpty($candidate)) { continue }
        if ($candidate -eq $Original) { continue }
        if ($candidate.Contains([char]0xFFFD)) { continue }

        $roundtrip = Encode-Utf8AsLegacy -Text $candidate -CodePage $cp
        if ($roundtrip -eq $Original) {
            $best = $candidate
            break
        }
    }

    return $best
}

$totalCellFixes = 0
$changedFiles = @()

foreach ($file in $files) {
    $rows = Import-Csv -LiteralPath $file.FullName
    if ($rows.Count -eq 0) { continue }

    $headers = $rows[0].PSObject.Properties.Name
    $newRows = @()
    $fileFixes = 0

    foreach ($r in $rows) {
        $o = [ordered]@{}
        foreach ($h in $headers) {
            $val = [string]$r.$h

            # Skip key/id-ish columns to avoid unintended changes in identifiers.
            if ($h -eq 'Key' -or $h -eq 'File' -or $h -eq 'Type' -or $h -eq 'UsedInMainMenu' -or $h -eq 'NoTranslate' -or $h -eq 'KeepLoaded') {
                $o[$h] = $val
                continue
            }

            $fixed = Try-ReversibleFix -Original $val
            if ($null -ne $fixed -and $fixed -ne $val) {
                $o[$h] = $fixed
                $fileFixes++
                $totalCellFixes++
            } else {
                $o[$h] = $val
            }
        }
        $newRows += [pscustomobject]$o
    }

    if ($fileFixes -gt 0) {
        $newRows | Export-Csv -LiteralPath $file.FullName -NoTypeInformation -Encoding UTF8
        $changedFiles += [pscustomobject]@{ File = $file.FullName; Fixes = $fileFixes }
    }
}

Write-Output ('FILES_SCANNED=' + $files.Count)
Write-Output ('FILES_CHANGED=' + $changedFiles.Count)
Write-Output ('TOTAL_CELL_FIXES=' + $totalCellFixes)
$changedFiles | Sort-Object File | ForEach-Object {
    Write-Output ('CHANGED|' + $_.File + '|Fixes=' + $_.Fixes)
}
