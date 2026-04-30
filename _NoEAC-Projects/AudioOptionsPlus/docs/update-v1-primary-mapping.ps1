$ErrorActionPreference = 'Stop'

$tsvPath = "AudioOptionsPlus/docs/sounds-categorized.tsv"
$lines = Get-Content -Path $tsvPath
if ($lines.Count -lt 2) { throw "sounds-categorized.tsv is empty." }

$header = $lines[0]
$data = $lines[1..($lines.Count - 1)]
$out = New-Object System.Collections.Generic.List[string]
$out.Add($header)

foreach ($line in $data) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $p = $line -split "`t"
    if ($p.Length -lt 5) {
        while ($p.Length -lt 5) { $p += "" }
    }

    $name = $p[0]
    $scope = $p[3]
    $tagsRaw = $p[4]
    $tags = New-Object System.Collections.Generic.List[string]
    if (-not [string]::IsNullOrWhiteSpace($tagsRaw)) {
        foreach ($t in ($tagsRaw -split ';')) {
            if (-not [string]::IsNullOrWhiteSpace($t) -and -not $tags.Contains($t)) { $tags.Add($t) }
        }
    }

    # Normalize legacy prompt tags into one combined Interaction Prompts category.
    if ($tags.Contains('11_GrabPlaceSounds')) {
        [void]$tags.Remove('11_GrabPlaceSounds')
        if (-not $tags.Contains('11_InteractionPrompts')) { $tags.Add('11_InteractionPrompts') }
    }
    if ($tags.Contains('12_OpenCloseSounds')) {
        [void]$tags.Remove('12_OpenCloseSounds')
        if (-not $tags.Contains('11_InteractionPrompts')) { $tags.Add('11_InteractionPrompts') }
    }

    if ($name -match '^stunbaton_hit[0-9]+$') {
        if (-not $tags.Contains('2_ImpactHarvestSurface')) {
            $tags.Add('2_ImpactHarvestSurface')
        }
    }

    # Per v1 scope clarification: trap-related impact sounds are excluded from Impact adjustments.
    if ($name -match '(?i)(trap|bladetrap|darttrap|tripwire|electric_fence)') {
        if ($tags.Contains('2_ImpactHarvestSurface')) {
            [void]$tags.Remove('2_ImpactHarvestSurface')
        }
    }

    # Per v1 scope clarification: plant-destroy sounds are excluded from Impact adjustments.
    if ($name -match '(?i)^plantdestroy[0-9]*$') {
        if ($tags.Contains('2_ImpactHarvestSurface')) {
            [void]$tags.Remove('2_ImpactHarvestSurface')
        }
    }

    # Per v1 scope clarification: chuck boulder impact is excluded from Impact adjustments.
    if ($name -match '(?i)^chuckboulderimpact$') {
        if ($tags.Contains('2_ImpactHarvestSurface')) {
            [void]$tags.Remove('2_ImpactHarvestSurface')
        }
    }

    # Per v1 scope clarification: nest destroy is excluded from Impact adjustments.
    if ($name -match '(?i)^nest_destroy$') {
        if ($tags.Contains('2_ImpactHarvestSurface')) {
            [void]$tags.Remove('2_ImpactHarvestSurface')
        }
    }

    # Per v1 scope clarification: bow, crossbow, and spear sounds are excluded from v1 adjustments.
    if ($name -match '(?i)(^bow|^crossbow|^spear|_bow_|_crossbow_|_spear_|\bbow\b|\bcrossbow\b|\bspear\b)') {
        $tags.Clear()
    }

    # Per v1 scope clarification: hulk/demo explode warning sounds are excluded from explosion adjustments.
    if ($name -match '(?i)^demolitionzexplodewarning$|^hulkexplodewarning$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: mine placement/grab sounds are excluded from v1 adjustments.
    if ($name -match '(?i)^mine_.*_(grab|place)$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: specific car alarm sounds are excluded.
    if ($name -match '(?i)^caralarm1_dying$|^caralarm1_lp$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: selected weapon/explosion utility sounds are excluded.
    if ($name -match '(?i)^desertvulture_fire$|^desertvulture_reload_part_01$|^desertvulture_reload_part_02$|^desertvulture_reload_part_03$|^desertvulture_s_fire$|^grenade_pullpin$|^grenade_throw$|^grenade_unholster$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: selected wire connection sounds are excluded.
    if ($name -match '(?i)^wire_dead_break$|^wire_dead_connect$|^wire_live_break$|^wire_live_connect$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: selected player/world utility loops are excluded from v1 adjustments.
    if ($name -match '(?i)^lockpick_lp$|^Parachute_Flutter_Lp$|^player_death_stinger_lp$|^player1sprint1_lp$|^player2sprint1_lp$|^rallymarker_lp$|^stunbaton_charged_lp$|^Supply_Crate_Plane_lp$|^torch_lp$|^vomit_projectile_lp$|^mission_bounds_warning_lp$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: selected impact/place sounds are excluded.
    if ($name -match '(?i)^impact_driver_swinglight$|^keystone_destroyed$|^keystone_impact_overlay$|^waterblockimpact$|^bucketplace_water$|^trapdoor_trigger$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: player/biome effect a_* sounds belong to Player Made Sounds.
    if ($name -match '(?i)^a_') {
        $tags.Clear()
        $tags.Add('21_PlayerMadeSounds')
    }

    # Per v1 scope clarification: crafting completion sounds belong only to Crafting Complete UI.
    if ($name -match '(?i)_complete_item$|^craft_complete_item$|^cement_mixer_complete$|^forge_item_complete$') {
        $tags.Clear()
        $tags.Add('7_CraftingCompleteUI')
    }

    # Per v1 scope clarification: bicycle coast loop belongs to Vehicles.
    if ($name -match '(?i)^bicycle_coast_lp$') {
        $tags.Clear()
        $tags.Add('5_VehiclesNoHorn')
    }

    # Per v1 scope clarification: engine fire loops belong to Vehicles only.
    if ($name -match '(?i)^engine_fire_.*_lp$') {
        $tags.Clear()
        $tags.Add('5_VehiclesNoHorn')
    }

    # Per v1 scope clarification: vehicle loop sounds should not remain in Electrical.
    if ($name -match '(?i)^gyrocopter_.*_lp$|^motorbike_(idle_lp|mid_speed_lp|run_lp)$|^suv_(idle_lp|max_speed_lp)$') {
        $tags.Clear()
        $tags.Add('5_VehiclesNoHorn')
    }

    # Per v1 scope clarification: electrical *_destroy sounds belong in Impact/Harvest/Surface (except twitch_, handled separately).
    if ($name -match '(?i)_destroy' -and $tags.Contains('6_ElectricalAndBlockLoops') -and -not ($name -match '(?i)^twitch_')) {
        [void]$tags.Remove('6_ElectricalAndBlockLoops')
        if (-not $tags.Contains('2_ImpactHarvestSurface')) {
            $tags.Add('2_ImpactHarvestSurface')
        }
    }

    # Per v1 scope clarification: twitch sounds are isolated into their own category.
    if ($name -match '(?i)^twitch_') {
        $tags.Clear()
        $tags.Add('13_TwitchSounds')
    }

    # Per v1 scope clarification: all *_grab and *_place sounds are interaction prompts.
    if ($name -match '(?i)_(grab|place)$') {
        $tags.Clear()
        $tags.Add('11_InteractionPrompts')
    }

    # Per v1 scope clarification: *_craft and *_click sounds belong only to Interaction Prompts.
    if ($name -match '(?i)_craft|_click') {
        $tags.Clear()
        $tags.Add('11_InteractionPrompts')
    }

    # Per v1 scope clarification: all open_/close_ and *_open/*_close sounds are interaction prompts.
    if ($name -match '(?i)^(open_|close_)|_(open|close)$') {
        $tags.Clear()
        $tags.Add('11_InteractionPrompts')
    }

    # Per v1 scope clarification: specific craft/repair UI prompts belong to Interaction Prompts.
    if ($name -match '(?i)^craft_place_item$|^craft_repair_item$|^ItemNeedsRepair$|^missingitemtorepair$') {
        $tags.Clear()
        $tags.Add('11_InteractionPrompts')
    }

    # Per v1 scope clarification: door/hatch/vault/cellar/bridge/garage/manhole/gate sounds have their own category.
    if ($name -match '(?i)(door|hatch|vault|cellar|bridge|garage|manhole|rollup_gate|gate_chainlink|gate_wood_large)') {
        $tags.Clear()
        $tags.Add('15_DoorsHatchesVaultsCellarsBridge')
    }

    # Per v1 scope clarification: trader-related sounds are split by trader for separate volume controls.
    if ($name -match '(?i)^(trader|ui_trader_)') {
        $tags.Clear()
        if ($name -match '(?i)^trader_bob_') {
            $tags.Add('16_TraderBob')
        } elseif ($name -match '(?i)^trader_hugh_') {
            $tags.Add('17_TraderHugh')
        } elseif ($name -match '(?i)^trader_jen_|^trader_jenlike') {
            $tags.Add('18_TraderJen')
        } elseif ($name -match '(?i)^trader_joel_') {
            $tags.Add('19_TraderJoel')
        } elseif ($name -match '(?i)^trader_rekt_') {
            $tags.Add('20_TraderRekt')
        } else {
            $tags.Clear()
        }
    }

    # Per v1 scope clarification: twitch_no_trader is a Twitch event sound, not a trader voice line.
    if ($name -match '(?i)^twitch_no_trader$') {
        $tags.Clear()
        $tags.Add('13_TwitchSounds')
    }

    # Per v1 scope clarification: cough sounds are player-made sounds.
    if ($name -match '(?i)cough') {
        $tags.Clear()
        $tags.Add('21_PlayerMadeSounds')
    }

    # Per v1 scope clarification: traderneutralcough is excluded from v1 adjustments.
    if ($name -match '(?i)^traderneutralcough$') {
        $tags.Clear()
    }

    # Final precedence: trapdoor_trigger is excluded from v1 adjustments.
    if ($name -match '(?i)^trapdoor_trigger$') {
        $tags.Clear()
    }

    # Per v1 scope clarification: specific trader UI interaction prompts belong in Interaction Prompts.
    if ($name -match '(?i)^ui_trader_inv_reset$|^ui_trader_purchase$') {
        $tags.Clear()
        $tags.Add('11_InteractionPrompts')
    }

    # Per v1 scope clarification: breech sounds are excluded from v1 adjustments.
    if ($name -match '(?i)breech') {
        $tags.Clear()
    }

    # Per v1 scope clarification: only drone flying/idle are included, and they belong to Electrical/Block Loops.
    if ($name -match '(?i)^drone_') {
        if ($name -match '(?i)^drone_(fly|idle_hover)$') {
            $tags.Clear()
            $tags.Add('6_ElectricalAndBlockLoops')
        } else {
            $tags.Clear()
        }
    }

    # Per v1 scope clarification: spider category includes all spider sounds.
    if ($name -match '(?i)^spider') {
        $tags.Clear()
        $tags.Add('8_SpiderZombieSpecific')
    }

    # Per v1 scope clarification: player-made sounds get their own category.
    if ($name -match '(?i)^player' -and $name -notmatch '(?i)^player_death_stinger_lp$|^player1sprint1_lp$|^player2sprint1_lp$') {
        $tags.Clear()
        $tags.Add('21_PlayerMadeSounds')
    }

    $scope = if ($tags.Count -gt 0) { 'Primary' } else { 'OutOfScope' }

    $p[3] = $scope
    $p[4] = ($tags -join ';')
    $out.Add(($p -join "`t"))
}

$out | Set-Content -Path $tsvPath

# Regenerate primary-by-category markdown from current tagged TSV
$outPath = "AudioOptionsPlus/docs/v1-primary-sounds-by-category.md"
$rows = Get-Content $tsvPath | Select-Object -Skip 1 | ForEach-Object {
    $p = $_ -split "`t"
    if ($p.Length -lt 5) { return }
    [pscustomobject]@{ Name = $p[0]; Scope = $p[3]; Tags = $p[4] }
} | Where-Object { $_ -and $_.Scope -eq 'Primary' }

$order = @(
    '1_AugerChainsaw',
    '2_ImpactHarvestSurface',
    '3_GunFire',
    '4_Explosions',
    '5_VehiclesNoHorn',
    '6_ElectricalAndBlockLoops',
    '7_CraftingCompleteUI',
    '8_SpiderZombieSpecific',
    '9_AnimalPainDeath',
    '10_PlaceUpgradeRepairBlocks',
    '11_InteractionPrompts',
    '13_TwitchSounds',
    '15_DoorsHatchesVaultsCellarsBridge',
    '16_TraderBob',
    '17_TraderHugh',
    '18_TraderJen',
    '19_TraderJoel',
    '20_TraderRekt',
    '21_PlayerMadeSounds'
)

$title = @{}
$title['1_AugerChainsaw'] = 'Auger / Chainsaw'
$title['2_ImpactHarvestSurface'] = 'Impact / Harvest / Surface'
$title['3_GunFire'] = 'Gun Fire'
$title['4_Explosions'] = 'Explosions'
$title['5_VehiclesNoHorn'] = 'Vehicles (No Horn)'
$title['6_ElectricalAndBlockLoops'] = 'Electrical / Block Loops'
$title['7_CraftingCompleteUI'] = 'Crafting Complete UI'
$title['8_SpiderZombieSpecific'] = 'Spider Zombie Specific'
$title['9_AnimalPainDeath'] = 'Animal Pain / Death'
$title['10_PlaceUpgradeRepairBlocks'] = 'Place / Upgrade / Repair Blocks'
$title['11_InteractionPrompts'] = 'Interaction Prompts'
$title['13_TwitchSounds'] = 'Twitch Sounds'
$title['15_DoorsHatchesVaultsCellarsBridge'] = 'Doors / Hatches / Vaults / Cellars / Bridge'
$title['16_TraderBob'] = 'Trader Bob'
$title['17_TraderHugh'] = 'Trader Hugh'
$title['18_TraderJen'] = 'Trader Jen'
$title['19_TraderJoel'] = 'Trader Joel'
$title['20_TraderRekt'] = 'Trader Rekt'
$title['21_PlayerMadeSounds'] = 'Player Made Sounds'

$md = New-Object System.Collections.Generic.List[string]
$md.Add('# V1 Primary Sounds by Category')
$md.Add('')
$md.Add('Source: AudioOptionsPlus/docs/sounds-categorized.tsv')
$md.Add('')

foreach ($tag in $order) {
    $names = $rows | Where-Object {
        $t = @()
        if ($_.Tags) { $t = $_.Tags -split ';' }
        $t -contains $tag
    } | Select-Object -ExpandProperty Name -Unique | Sort-Object

    $md.Add("## $($title[$tag]) ($($names.Count))")
    $md.Add('')
    foreach ($n in $names) { $md.Add("- $n") }
    $md.Add('')
}

$md | Set-Content -Path $outPath

Write-Output "UPDATED=$tsvPath"
Write-Output "UPDATED=$outPath"
