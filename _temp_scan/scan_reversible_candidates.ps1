$ErrorActionPreference = 'Stop'
$root = 'C:\GitHub\7D2D-Mods\01_Draft'
$files = Get-ChildItem -LiteralPath $root -Recurse -File -Filter 'Localization.csv' | Where-Object { $_.FullName -match '\\Config\\Localization\.csv$' }

function Decode-LegacyToUtf8([string]$Text, [int]$CodePage) {
    if ([string]::IsNullOrEmpty($Text)) { return $null }
    try {
        $bytes = [System.Text.Encoding]::GetEncoding($CodePage).GetBytes($Text)
        return [System.Text.Encoding]::UTF8.GetString($bytes)
    } catch { return $null }
}

function Encode-Utf8AsLegacy([string]$Text, [int]$CodePage) {
    if ([string]::IsNullOrEmpty($Text)) { return $null }
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return [System.Text.Encoding]::GetEncoding($CodePage).GetString($bytes)
    } catch { return $null }
}

function HasReversibleFix([string]$Original) {
    if ([string]::IsNullOrEmpty($Original)) { return $false }
    foreach ($cp in @(1252,28591)) {
        $cand = Decode-LegacyToUtf8 -Text $Original -CodePage $cp
        if ([string]::IsNullOrEmpty($cand)) { continue }
        if ($cand -eq $Original) { continue }
        if ($cand.Contains([char]0xFFFD)) { continue }
        $rt = Encode-Utf8AsLegacy -Text $cand -CodePage $cp
        if ($rt -eq $Original) { return $true }
    }
    return $false
}

$total=0
$byFile=@{}
foreach($f in $files){
    $rows=Import-Csv -LiteralPath $f.FullName
    if($rows.Count -eq 0){ continue }
    $headers=$rows[0].PSObject.Properties.Name
    foreach($r in $rows){
        foreach($h in $headers){
            if($h -eq 'Key' -or $h -eq 'File' -or $h -eq 'Type' -or $h -eq 'UsedInMainMenu' -or $h -eq 'NoTranslate' -or $h -eq 'KeepLoaded'){ continue }
            $v=[string]$r.$h
            if(HasReversibleFix $v){
                $total++
                if(-not $byFile.ContainsKey($f.FullName)){ $byFile[$f.FullName]=0 }
                $byFile[$f.FullName]++
            }
        }
    }
}
"FILES_SCANNED=$($files.Count)"
"REVERSIBLE_CANDIDATES=$total"
$byFile.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 30 | ForEach-Object { "FILE=" + $_.Key + "|COUNT=" + $_.Value }
