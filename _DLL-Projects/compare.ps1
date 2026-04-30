$tempPath = "AudioOptionsPlus/tempLocal.txt"
$currPath = "AudioOptionsPlus/Config/Localization.txt"
Add-Type -AssemblyName Microsoft.VisualBasic
function Parse-LocalizationCSV($path) {
    $rows = @(); $parser = New-Object Microsoft.VisualBasic.FileIO.TextFieldParser($path)
    $parser.TextFieldType = [Microsoft.VisualBasic.FileIO.FieldType]::Delimited
    $parser.SetDelimiters(","); $parser.HasFieldsEnclosedInQuotes = $true
    while (!$parser.EndOfData) { try { $rows += ,@($parser.ReadFields()) } catch { $parser.ReadLine() | Out-Null } }
    $parser.Close(); return $rows
}
$tempRows = Parse-LocalizationCSV $tempPath; $currRows = Parse-LocalizationCSV $currPath
$tempKeys = @{}; for($i=1; $i -lt $tempRows.Count; $i++) { $tempKeys[$tempRows[$i][0]] = $i }
$currKeys = @{}; for($i=1; $i -lt $currRows.Count; $i++) { $currKeys[$currRows[$i][0]] = $i }
"=== Row Counts ==="; "Temp file: $($tempRows.Count) rows"; "Current file: $($currRows.Count) rows"; ""
"=== Key Counts ==="; "Temp keys: $($tempKeys.Count)"; "Current keys: $($currKeys.Count)"; ""
$onlyInTemp = $tempKeys.Keys | ? { -not $currKeys.ContainsKey($_) }
$onlyInCurrent = $currKeys.Keys | ? { -not $tempKeys.ContainsKey($_) }
"Keys only in Temp: $($onlyInTemp.Count)"; "Keys only in Current: $($onlyInCurrent.Count)"; ""
$matched = 0; $unmatched = @()
foreach ($key in $currKeys.Keys) {
    $c1 = $key -replace 'xuiOptionsAudioAOPVolumeProfiles', 'xuiOptionsAudioAOP'
    $c2 = $key -replace 'xuiOptionsAudioAOPSoundSwap', 'xuiOptionsAudioAOP'
    $c3 = $key -replace 'xuiOptionsAudioAOPSoundSwap', 'xuiOptionsAudio'
    if ($tempKeys.ContainsKey($c1) -or $tempKeys.ContainsKey($c2) -or $tempKeys.ContainsKey($c3) -or $tempKeys.ContainsKey($key)) { $matched++ } else { $unmatched += $key }
}
"=== Mapping Coverage ==="; "Matched: $matched / $($currKeys.Count)"; "Unmatched: $($unmatched.Count)"; ""
$mojibakeRows = 0
for($i=1; $i -lt $currRows.Count; $i++) { $row = $currRows[$i]; for($j=1; $j -lt $row.Count; $j++) { if ($row[$j] -match '�|\?\?\?\?') { $mojibakeRows++; break } } }
"=== Mojibake ==="; "Rows with mojibake: $mojibakeRows"; ""
"=== Sample Unmatched (up to 15) ==="; $unmatched | Select -First 15
