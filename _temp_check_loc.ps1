$basePath = "C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\B19Jay Mod Package"
$files = Get-ChildItem $basePath -Recurse -Filter "Localization.txt"
$found = $false
foreach ($file in $files) {
    $lines = Get-Content $file.FullName -Encoding UTF8 -ErrorAction SilentlyContinue
    if ($null -eq $lines -or $lines.Count -eq 0) {
        Write-Host "EMPTY: $($file.FullName)"
        $found = $true
        continue
    }
    $header = $lines[0].TrimStart([char]0xFEFF)
    $headerCols = ($header -split ',').Count
    for ($i = 1; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $cols = ($line -split ',').Count
        if ($cols -ne $headerCols) {
            Write-Host "BAD: $($file.FullName)"
            Write-Host "  Header cols=$headerCols, Row cols=$cols, Line $($i+1): $($line.Substring(0,[Math]::Min(120,$line.Length)))"
            $found = $true
        }
    }
}
if (-not $found) { Write-Host "No column-count issues found in any Localization.txt" }
