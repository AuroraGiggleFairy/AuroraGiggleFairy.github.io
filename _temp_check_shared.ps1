$basePath = "C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\B19Jay Mod Package"

$targetMods = @(
    "AGF-NoEAC-ScreamerAlert",
    "AGF-NoEAC-Toolbelt12Slots",
    "AGF-VP-AmmoDisassembly",
    "AGF-VP-ApiaryPlus",
    "AGF-VP-ArmorHarvestMods",
    "AGF-VP-AutomobilesRespawn",
    "AGF-VP-BedrollPlus",
    "AGF-VP-BreakItGetIt",
    "AGF-VP-BuyTraderVendingMachines",
    "AGF-VP-CraftSewingKits",
    "AGF-VP-CraftStackEngBattCells",
    "AGF-VP-CraftVitamins",
    "AGF-VP-DecorationBlock",
    "AGF-VP-DewsPlus",
    "AGF-VP-DoorsPlus",
    "AGF-VP-DrinkableAcid",
    "AGF-VP-DyesPlus",
    "AGF-VP-FloraHarvester",
    "AGF-VP-FuelBurnPlus",
    "AGF-VP-MasterTool",
    "AGF-VP-MedicationNoInsectSlow",
    "AGF-VP-MiningPlus",
    "AGF-VP-PumpkinsPlus",
    "AGF-VP-RenamesAlphabeticalSort",
    "AGF-VP-RestorePowerAnyTime",
    "AGF-VP-ScrapBatts4Acid",
    "AGF-VP-SimplifiedStacks",
    "AGF-VP-SmeltingPlus",
    "AGF-VP-WriteStoryOnCrate",
    "GW71_FoundationBlock",
    "Gears",
    "LittleRedSonja_ZombiePack",
    "Quartz"
)

$files = Get-ChildItem $basePath -Recurse -Filter "Localization.txt" | Where-Object {
    $_.FullName -notlike "*BACKUP*" -and
    ($targetMods | Where-Object { $_.FullName -like "*$_*" }).Count -gt 0
}

$found = $false
foreach ($file in $files) {
    $modName = $file.Directory.Parent.Name
    $lines = Get-Content $file.FullName -Encoding UTF8 -ErrorAction SilentlyContinue
    if ($null -eq $lines -or $lines.Count -eq 0) { continue }
    $headerStr = $lines[0]
    if ($headerStr.Length -gt 0 -and [int][char]$headerStr[0] -eq 0xFEFF) { $headerStr = $headerStr.Substring(1) }
    $headerCols = ($headerStr -split ',').Count
    for ($i = 1; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $cols = ($line -split ',').Count
        if ($cols -lt $headerCols) {
            Write-Host "SHORT: $modName | Line $($i+1) expected=$headerCols got=$cols | $($line.Substring(0,[Math]::Min(100,$line.Length)))"
            $found = $true
        }
    }
}
if (-not $found) { Write-Host "No short-column rows found in any of the target mods." }
