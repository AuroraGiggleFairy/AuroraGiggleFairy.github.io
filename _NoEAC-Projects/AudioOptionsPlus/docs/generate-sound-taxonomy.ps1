$ErrorActionPreference = 'Stop'

$path = "AudioOptionsPlus/sounds.xml"
$text = Get-Content -Path $path -Raw
$matches = [regex]::Matches($text, '<SoundDataNode\s+name="([^"]+)"')
$names = foreach ($m in $matches) { $m.Groups[1].Value }

function Get-SoundCategory([string]$name) {
    $n = $name.ToLowerInvariant()

    if ($n -match '^twitch') { return @('Integration', 'Twitch / Crowd Events') }
    if ($n -match '(trader|bunkerwoman|bunkerman|sysadmin|dialog|voiceover|intercom|elevator|password)') { return @('NPC & Dialogue', 'Trader / NPC / Spoken') }
    if ($n -match '(^ui_|_ui_|^quest|challenge|journal|objective|reward|tooltip|menu|notification|complete_item|item_complete|craft_complete_item)') { return @('UI & Meta', 'UI / Quest / Notifications') }

    if ($n -match '(step|slither|footstep|runloop|landthump|wingflap)') {
        if ($n -match '(snake|slither)') { return @('Movement', 'Snake / Reptile Locomotion') }
        if ($n -match '(spider|insect|bug)') { return @('Movement', 'Arachnid / Insect Locomotion') }
        if ($n -match '(crawler|_crawl)') { return @('Movement', 'Crawler / Prone Locomotion') }
        if ($n -match '(hoof|stag|deer)') { return @('Movement', 'Hoofed Animal Locomotion') }
        if ($n -match '(dog|wolf|boar|bear|coyote|rabbit|chicken|animalpaw|animalhvy|animallight)') { return @('Movement', 'Quadruped Animal Locomotion') }
        if ($n -match '(vulture|bird|wing)') { return @('Movement', 'Avian Locomotion') }
        return @('Movement', 'Humanoid / Player Locomotion')
    }

    if ($n -match '(zombie|boar|bear|wolf|dog|vulture|snake|spider|crawler|stag|rabbit|chicken|mutated|hazmat|feral|burnt|screamer|sense|roam|alert|pain|death|attack|giveup|vomit)') {
        return @('Creatures', 'Vocals / Behaviors / Combat Cues')
    }

    if ($n -match '(^44magnum|^ak47|^m60|^smg|^pistol|^rifle|^sniper|^shotgun|^autoshotgun|^blunderbuss|^desertvulture|^crossbow|^bow|weaponfire|weapon_fire|_fire$|_s_fire$|reload|_sight_|turret_fire|darttrap_fire)') {
        return @('Combat', 'Firearms / Ranged Weapons')
    }
    if ($n -match '(club|sledge|axe|knife|knuckles|spear|stunbaton|baton|shovel|wrench|ratchet|nailgun|auger|chainsaw|machete|fists|swing|repair|upgrade)') {
        return @('Combat', 'Melee / Tools / Repair-Upgrade Actions')
    }
    if ($n -match '(explosion|explode|rocket|grenade|landmine|mine_|blast|hulkexplode)') {
        return @('Combat', 'Explosions / Heavy Impacts')
    }
    if ($n -match '(impact|hit|destroy|demolish|break|thump)') {
        return @('Combat', 'Impacts / Material Hits / Breaks')
    }

    if ($n -match '(door|open_|close_|open$|close$|chest|crate|furniture|pickup|place|switch|lever|gate|lock|unlock|cardboard|cupboard|keystone|loot)') {
        return @('World Interaction', 'Containers / Doors / Use Actions')
    }

    if ($n -match '(forge|campfire|chem|cement|workbench|collector|generator|dewcollector|bankgenerator|crafting)') {
        return @('Crafting & Base', 'Workstations / Base Devices')
    }

    if ($n -match '(vehicle|suv|minibike|motorbike|motorcycle|gyrocopter|bicycle|drone|_engine_|engine_|vwheel|truck|car|horn)') {
        return @('Vehicles', 'Vehicle / Drone Audio')
    }

    if ($n -match '(electric|wire|power|bladetrap|darttrap|tripwire|junkturret|turret|trap_)') {
        return @('Electrical & Defenses', 'Power / Traps / Turrets')
    }

    if ($n -match '(biome|wind|rain|thunder|ambient|amb_|music|firemediumloop|firesmallloop|cooler|fridge|waterfall|underwater|torch_lp|light_|_light_|headlight|flashlight|weather)') {
        return @('Ambient', 'World / Ambience / Loops')
    }

    if ($n -match '(food|seeds|meat|corn|potato|jars|drug|medical|ammo|parts|mod_|outfit|footwear|headwear|gloves|item_)') {
        return @('Items', 'Items / Inventory / Consumables')
    }

    return @('Misc', 'Uncategorized / Review Needed')
}

$rows = foreach ($name in $names) {
    $cat = Get-SoundCategory $name
    [pscustomobject]@{
        SoundDataNode = $name
        Category = $cat[0]
        Subcategory = $cat[1]
    }
}

$docsDir = "AudioOptionsPlus/docs"
if (-not (Test-Path $docsDir)) { New-Item -ItemType Directory -Path $docsDir | Out-Null }

$tsvPath = Join-Path $docsDir "sounds-categorized.tsv"
"SoundDataNode`tCategory`tSubcategory" | Set-Content $tsvPath
$rows | Sort-Object Category, Subcategory, SoundDataNode | ForEach-Object {
    "{0}`t{1}`t{2}" -f $_.SoundDataNode, $_.Category, $_.Subcategory
} | Add-Content $tsvPath

$outlinePath = Join-Path $docsDir "sounds-outline.md"
$total = $rows.Count
$categoryGroups = $rows | Group-Object Category | Sort-Object Count -Descending

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# AudioOptionsPlus Sound Taxonomy Outline")
$lines.Add("")
$lines.Add("Generated from AudioOptionsPlus/sounds.xml.")
$lines.Add("")
$lines.Add("- Total SoundDataNode entries: $total")
$lines.Add("- Full editable mapping: docs/sounds-categorized.tsv")
$lines.Add("")
$lines.Add("## High-Level Categories")
$lines.Add("")
foreach ($cg in $categoryGroups) {
    $lines.Add("- $($cg.Name): $($cg.Count)")
}

$lines.Add("")
$lines.Add("## Detailed Outline")
$lines.Add("")
foreach ($cg in $categoryGroups) {
    $lines.Add("### $($cg.Name) ($($cg.Count))")
    $subGroups = $cg.Group | Group-Object Subcategory | Sort-Object Count -Descending
    foreach ($sg in $subGroups) {
        $lines.Add("")
        $lines.Add("- $($sg.Name) ($($sg.Count))")
        $sample = $sg.Group | Select-Object -First 12
        foreach ($s in $sample) {
            $lines.Add("  - $($s.SoundDataNode)")
        }
        if ($sg.Count -gt $sample.Count) {
            $lines.Add("  - ... plus $($sg.Count - $sample.Count) more")
        }
    }
    $lines.Add("")
}

$lines.Add("## Movement Concept Proposal (for future volume sliders)")
$lines.Add("")
$lines.Add("- Humanoid / Player Locomotion")
$lines.Add("- Quadruped Animal Locomotion")
$lines.Add("- Hoofed Animal Locomotion")
$lines.Add("- Snake / Reptile Locomotion")
$lines.Add("- Arachnid / Insect Locomotion")
$lines.Add("- Crawler / Prone Locomotion")
$lines.Add("- Avian Locomotion")
$lines.Add("")
$lines.Add("## Notes")
$lines.Add("")
$lines.Add("- This is a naming-pattern taxonomy. Some entries may need manual reassignment.")
$lines.Add("- Use docs/sounds-categorized.tsv as the authoritative full list to edit category decisions.")

$lines | Set-Content -Path $outlinePath

Write-Output "UPDATED=$outlinePath"
Write-Output "UPDATED=$tsvPath"
Write-Output "CATEGORY_COUNTS:"
$categoryGroups | ForEach-Object { "{0}`t{1}" -f $_.Name, $_.Count }
