$ErrorActionPreference = 'Stop'

$root = 'C:\GitHub\7D2D-Mods'
$draftRoot = Join-Path $root '01_Draft'
$sourceRoots = @(
    (Join-Path $root '_x2.6'),
    (Join-Path $root '_xObsolete')
)

$draftFiles = Get-ChildItem -LiteralPath $draftRoot -Recurse -File -Filter 'Localization.csv' |
    Where-Object { $_.FullName -match '\\Config\\Localization\.csv$' }

$sourceFiles = @()
foreach ($sr in $sourceRoots) {
    if (Test-Path $sr) {
        $sourceFiles += Get-ChildItem -LiteralPath $sr -Recurse -File -Filter 'Localization.txt'
    }
}

$sourceByModKey = @{}
$sourceByKey = @{}

foreach ($sf in $sourceFiles) {
    $mod = $sf.Directory.Parent.Name
    if (-not $sourceByModKey.ContainsKey($mod)) {
        $sourceByModKey[$mod] = @{}
    }

    $rows = Import-Csv -LiteralPath $sf.FullName
    foreach ($r in $rows) {
        $key = [string]$r.Key
        if ([string]::IsNullOrEmpty($key)) { continue }

        if (-not $sourceByModKey[$mod].ContainsKey($key)) {
            $sourceByModKey[$mod][$key] = [pscustomobject]@{
                Row = $r
                Path = $sf.FullName
                Mod = $mod
            }
        }

        if (-not $sourceByKey.ContainsKey($key)) {
            $sourceByKey[$key] = @()
        }
        $sourceByKey[$key] += [pscustomobject]@{
            Row = $r
            Path = $sf.FullName
            Mod = $mod
        }
    }
}

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

function Encode-Utf8AsLegacyString {
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

function Is-MojibakeEquivalent {
    param(
        [string]$DraftValue,
        [string]$SourceValue
    )
    if ([string]::IsNullOrEmpty($DraftValue) -or [string]::IsNullOrEmpty($SourceValue)) {
        return $false
    }

    foreach ($cp in @(1252, 28591)) {
        $decoded = Decode-LegacyToUtf8 -Text $DraftValue -CodePage $cp
        if ($decoded -eq $SourceValue) { return $true }

        $encoded = Encode-Utf8AsLegacyString -Text $SourceValue -CodePage $cp
        if ($encoded -eq $DraftValue) { return $true }
    }

    return $false
}

function Get-SuspiciousScore {
    param([string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return 0 }

    $score = 0
    $markers = @(0x00C3, 0x00C2, 0x00D0, 0x00D1, 0x00E3, 0x00E5, 0x00E6, 0x00E7, 0x00EC, 0x00ED, 0x00EF, 0x00F0, 0x00E2)

    foreach ($ch in $Text.ToCharArray()) {
        $code = [int][char]$ch

        if ($code -eq 0xFFFD) {
            $score += 4
            continue
        }

        if (($code -ge 0 -and $code -le 8) -or ($code -ge 11 -and $code -le 12) -or ($code -ge 14 -and $code -le 31) -or ($code -ge 127 -and $code -le 159)) {
            $score += 2
        }

        if ($markers -contains $code) {
            $score += 1
        }
    }

    return $score
}

function Pick-BetterDecodedCandidate {
    param([string]$Original)

    if ([string]::IsNullOrEmpty($Original)) { return $null }

    $origScore = Get-SuspiciousScore -Text $Original
    if ($origScore -eq 0) { return $null }

    $candidates = @(
        (Decode-LegacyToUtf8 -Text $Original -CodePage 1252),
        (Decode-LegacyToUtf8 -Text $Original -CodePage 28591)
    )

    $best = $null
    $bestScore = $origScore

    foreach ($cand in $candidates) {
        if ([string]::IsNullOrEmpty($cand)) { continue }
        if ($cand -eq $Original) { continue }

        $candScore = Get-SuspiciousScore -Text $cand
        if ($candScore -lt $bestScore) {
            $best = $cand
            $bestScore = $candScore
        }
    }

    if ($null -ne $best -and $bestScore -le ($origScore - 1)) {
        return $best
    }

    return $null
}

function Choose-SourceRow {
    param(
        [string]$ModName,
        [string]$Key,
        [pscustomobject]$DraftRow
    )

    if ($sourceByModKey.ContainsKey($ModName) -and $sourceByModKey[$ModName].ContainsKey($Key)) {
        return $sourceByModKey[$ModName][$Key]
    }

    if (-not $sourceByKey.ContainsKey($Key)) {
        return $null
    }

    $options = $sourceByKey[$Key]
    if ($options.Count -eq 1) {
        return $options[0]
    }

    $draftEnglishLike = @(
        [string]$DraftRow.english,
        [string]$DraftRow.KeepLoaded,
        [string]$DraftRow.'Context / Alternate Text'
    ) | Where-Object { -not [string]::IsNullOrEmpty($_) } | Select-Object -Unique

    foreach ($candidate in $draftEnglishLike) {
        foreach ($opt in $options) {
            if ([string]$opt.Row.english -eq $candidate) {
                return $opt
            }
        }
    }

    return $options[0]
}

$changedFiles = @()
$totalCellFixes = 0
$totalShiftRealignRows = 0
$totalSourceBasedFixes = 0
$totalFallbackFixes = 0

foreach ($df in $draftFiles) {
    $rows = Import-Csv -LiteralPath $df.FullName
    if ($rows.Count -eq 0) { continue }

    $mod = $df.Directory.Parent.Name
    $headers = $rows[0].PSObject.Properties.Name

    $newRows = @()
    $fileChanged = $false
    $fileCellFixes = 0
    $fileShiftRows = 0

    foreach ($dr in $rows) {
        $out = [ordered]@{}
        foreach ($h in $headers) {
            $out[$h] = [string]$dr.$h
        }

        $key = [string]$out['Key']
        $srcInfo = $null
        $srcRow = $null

        if (-not [string]::IsNullOrEmpty($key)) {
            $srcInfo = Choose-SourceRow -ModName $mod -Key $key -DraftRow $dr
            if ($null -ne $srcInfo) {
                $srcRow = $srcInfo.Row
            }
        }

        $didShiftRealign = $false
        if ($null -ne $srcRow -and ($headers -contains 'KeepLoaded') -and ($headers -contains 'english')) {
            $srcEnglish = [string]$srcRow.english
            $curKeepLoaded = [string]$out['KeepLoaded']
            $curEnglish = [string]$out['english']

            if (-not [string]::IsNullOrEmpty($curKeepLoaded) -and [string]::IsNullOrEmpty($curEnglish) -and $curKeepLoaded -eq $srcEnglish) {
                if ($out['KeepLoaded'] -ne '') {
                    $out['KeepLoaded'] = ''
                    $fileChanged = $true
                    $fileCellFixes++
                    $totalCellFixes++
                }

                foreach ($col in $headers) {
                    if ($col -eq 'KeepLoaded') { continue }
                    if (-not ($srcRow.PSObject.Properties.Name -contains $col)) { continue }

                    $sv = [string]$srcRow.$col
                    if ([string]$out[$col] -ne $sv) {
                        $out[$col] = $sv
                        $fileChanged = $true
                        $fileCellFixes++
                        $totalCellFixes++
                        $totalSourceBasedFixes++
                    }
                }

                $fileShiftRows++
                $totalShiftRealignRows++
                $didShiftRealign = $true
            }
        }

        if (-not $didShiftRealign) {
            foreach ($col in $headers) {
                if ($col -eq 'Key' -or $col -eq 'KeepLoaded') { continue }

                $dv = [string]$out[$col]
                if ([string]::IsNullOrEmpty($dv)) { continue }

                $fixedBySource = $false
                if ($null -ne $srcRow -and ($srcRow.PSObject.Properties.Name -contains $col)) {
                    $sv = [string]$srcRow.$col
                    if (-not [string]::IsNullOrEmpty($sv) -and $dv -ne $sv) {
                        if (Is-MojibakeEquivalent -DraftValue $dv -SourceValue $sv) {
                            $out[$col] = $sv
                            $fileChanged = $true
                            $fileCellFixes++
                            $totalCellFixes++
                            $totalSourceBasedFixes++
                            $fixedBySource = $true
                        }
                    }
                }

                if ($fixedBySource) { continue }

                # Fallback: decode obvious mojibake even without source coverage.
                $fallback = Pick-BetterDecodedCandidate -Original $dv
                if ($null -ne $fallback -and $fallback -ne $dv) {
                    $out[$col] = $fallback
                    $fileChanged = $true
                    $fileCellFixes++
                    $totalCellFixes++
                    $totalFallbackFixes++
                }
            }
        }

        $newRows += [pscustomobject]$out
    }

    if ($fileChanged) {
        $newRows | Export-Csv -LiteralPath $df.FullName -NoTypeInformation -Encoding UTF8
        $changedFiles += [pscustomobject]@{
            File = $df.FullName
            CellFixes = $fileCellFixes
            ShiftRows = $fileShiftRows
        }
    }
}

Write-Output ('DRAFT_FILES=' + $draftFiles.Count)
Write-Output ('CHANGED_FILES=' + $changedFiles.Count)
Write-Output ('TOTAL_CELL_FIXES=' + $totalCellFixes)
Write-Output ('TOTAL_SHIFT_REALIGN_ROWS=' + $totalShiftRealignRows)
Write-Output ('TOTAL_SOURCE_BASED_FIXES=' + $totalSourceBasedFixes)
Write-Output ('TOTAL_FALLBACK_FIXES=' + $totalFallbackFixes)

$changedFiles | Sort-Object File | ForEach-Object {
    Write-Output ('CHANGED|' + $_.File + '|CellFixes=' + $_.CellFixes + '|ShiftRows=' + $_.ShiftRows)
}
