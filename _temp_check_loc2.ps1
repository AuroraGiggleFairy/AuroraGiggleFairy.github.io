$basePath = "C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\B19Jay Mod Package"

function Parse-CsvLine([string]$line) {
    $fields = [System.Collections.Generic.List[string]]::new()
    $current = [System.Text.StringBuilder]::new()
    $inQuotes = $false
    for ($i = 0; $i -lt $line.Length; $i++) {
        $ch = $line[$i]
        if ($inQuotes) {
            if ($ch -eq '"') {
                if ($i+1 -lt $line.Length -and $line[$i+1] -eq '"') { [void]$current.Append('"'); $i++ }
                else { $inQuotes = $false }
            } else { [void]$current.Append($ch) }
        } else {
            if ($ch -eq '"') { $inQuotes = $true }
            elseif ($ch -eq ',') { $fields.Add($current.ToString()); [void]$current.Clear() }
            else { [void]$current.Append($ch) }
        }
    }
    $fields.Add($current.ToString())
    return $fields
}

$files = Get-ChildItem $basePath -Recurse -Filter "Localization.txt" | Where-Object { $_.FullName -notlike "*BACKUP*" }
$found = $false
foreach ($file in $files) {
    $lines = Get-Content $file.FullName -Encoding UTF8 -ErrorAction SilentlyContinue
    if ($null -eq $lines -or $lines.Count -eq 0) { continue }
    $headerStr = $lines[0]
    if ($headerStr.Length -gt 0 -and [int][char]$headerStr[0] -eq 0xFEFF) { $headerStr = $headerStr.Substring(1) }
    $headerCols = (Parse-CsvLine $headerStr).Count
    for ($i = 1; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $cols = (Parse-CsvLine $line).Count
        if ($cols -gt $headerCols) {
            Write-Host "EXTRA: $($file.Directory.Parent.Name) | Line $($i+1) expected=$headerCols got=$cols | $($line.Substring(0,[Math]::Min(120,$line.Length)))"
            $found = $true
        }
    }
}
if (-not $found) { Write-Host "No extra-column rows found (all unquoted commas are inside quoted fields)." }
