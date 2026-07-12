$ErrorActionPreference = 'Stop'

$modPath = "c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-HUDPlus-PurpleBook-v3.0.0"
$templatePath = "c:/GitHub/7D2D-Mods/Workflow/ReadmeSystem/Templates/TEMPLATE-ModReadMes.md"
$modscopePath = "c:/GitHub/7D2D-Mods/Workflow/ReadmeSystem/Snippets/ModReadme-MODSCOPE-md-Snippet.md"
$harmonyWarningTxtPath = "c:/GitHub/7D2D-Mods/Workflow/ReadmeSystem/Snippets/ModReadme-HARMONYWARNING-txt-Snippet.txt"
$harmonyWarningMdPath = "c:/GitHub/7D2D-Mods/Workflow/ReadmeSystem/Snippets/ModReadme-HARMONYWARNING-md-Snippet.md"
$modTypeSnippetTxtPath = "c:/GitHub/7D2D-Mods/Workflow/ReadmeSystem/Snippets/ModReadme-MODTYPE-txt-Snippet.txt"
$shortPath = "c:/GitHub/7D2D-Mods/Workflow/ReadmeSystem/Snippets/ModReadme-MODGUIDE-txt-Snippet.txt"
$csvPath = "c:/GitHub/7D2D-Mods/Workflow/ReadmeSystem/Data/HELPER_ModCompatibility.csv"

if (-not (Test-Path $modPath)) { throw "Mod path not found: $modPath" }

$modInfoPath = Join-Path $modPath "ModInfo.xml"
[xml]$modInfo = Get-Content -Raw -Encoding UTF8 $modInfoPath
$modName = $modInfo.xml.Name.value
if (-not $modName) { $modName = [System.IO.Path]::GetFileName($modPath) }
$modVersion = $modInfo.xml.Version.value
if (-not $modVersion) { $modVersion = "0.0.0" }
$modDescription = $modInfo.xml.Description.value
if (-not $modDescription) { $modDescription = "(The description line from modinfo.xml)" }

$baseName = [regex]::Replace([System.IO.Path]::GetFileName($modPath), '-v\d+\.\d+(\.\d+)*$', '')
$row = Import-Csv -Path $csvPath | Where-Object { $_.MOD_NAME -eq $baseName } | Select-Object -First 1
if (-not $row) { throw "No compatibility CSV row found for $baseName" }

$modTypeMapDefaults = @{
    '1' = 'Server-side (EAC-friendly): Server install works for all joining players; EAC on or off. (Also works in singleplayer.)'
    '2' = 'Server-side (EAC Off): EAC off required; server install works for all joining players. (Also works in singleplayer.)'
    '3' = 'Server/Client-side (Required): EAC off required; host and joining players must install it. (Also works in singleplayer.)'
    '4' = 'Client-side (Only): EAC off required; server install has no effect; install on each player PC. (Also works in singleplayer.)'
}
$modTypeMap = @{}

function Build-ModTypeMapFromTxtSnippet {
    param(
        [string]$Path,
        [hashtable]$FallbackMap
    )

    if (-not (Test-Path $Path)) { return $FallbackMap }

    try {
        $text = Get-Content -Raw -Encoding UTF8 $Path
    }
    catch {
        return $FallbackMap
    }

    $parsed = @{}
    $currentId = $null
    $currentName = $null
    $currentDetails = [System.Collections.Generic.List[string]]::new()

    $commit = {
        if ($currentId -and $currentName) {
            if ($currentDetails.Count -gt 0) {
                $parsed[$currentId] = "${currentName}: $($currentDetails -join '; ')"
            }
            else {
                $parsed[$currentId] = "$currentName"
            }
        }

        $currentId = $null
        $currentName = $null
        $currentDetails = [System.Collections.Generic.List[string]]::new()
    }

    foreach ($rawLine in ($text -split "`r?`n")) {
        $line = ($rawLine + '').Trim()
        if ($line.Length -eq 0) { continue }

        $headerMatch = [regex]::Match($line, '^MOD\s*TYPE\s+(\d+)$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($headerMatch.Success) {
            & $commit
            $currentId = $headerMatch.Groups[1].Value
            continue
        }

        if (-not $currentId) { continue }

        $nameMatch = [regex]::Match($line, '^-\s*Mod\s*Type\s*:\s*(.+)$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($nameMatch.Success) {
            $currentName = $nameMatch.Groups[1].Value.Trim()
            continue
        }

        $detailMatch = [regex]::Match($line, '^-\s*(.+)$')
        if ($detailMatch.Success) {
            $detail = $detailMatch.Groups[1].Value.Trim()
            if ($detail.Length -eq 0) { continue }
            if ([regex]::IsMatch($detail, '^Mod\s*Type\s*:', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) { continue }
            $currentDetails.Add($detail)
        }
    }

    & $commit

    if ($parsed.Count -eq 0) { return $FallbackMap }
    return $parsed
}

$modTypeMap = Build-ModTypeMapFromTxtSnippet -Path $modTypeSnippetTxtPath -FallbackMap $modTypeMapDefaults
$modTypeLine = $modTypeMap[[string]$row.MOD_TYPE_ID]
if (-not $modTypeLine) { $modTypeLine = "MISSINGDATA" }

function Normalize-SafetyValue {
    param([string]$Value)

    if ($null -eq $Value) { $Value = '' }
    $normalized = ($Value + '').Trim().ToLowerInvariant()
    if ($normalized -in @('safe', 'yes', 'y', 'true', '1')) { return 'Yes (Safe)' }
    if ($normalized -in @('dangerous', 'no', 'n', 'false', '0')) { return 'No (Dangerous)' }
    if ($normalized.Length -eq 0 -or $normalized -in @('missingdata', 'tbd', 'none')) { return 'Unknown' }
    return ($Value + '').Trim()
}

$safeToInstall = Normalize-SafetyValue $row.SAFE_TO_INSTALL
$safeToRemove = Normalize-SafetyValue $row.SAFE_TO_REMOVE

function Build-DependenciesBlock {
    param([string]$Value)

    if ($null -eq $Value) { $Value = '' }
    $raw = ($Value + '').Trim()
    if (Test-HasHarmonyDependency $raw) {
        if (Test-Path $harmonyWarningTxtPath) {
            return (Get-Content -Raw -Encoding UTF8 $harmonyWarningTxtPath).Trim()
        }

        if (Test-Path $harmonyWarningMdPath) {
            return (Get-Content -Raw -Encoding UTF8 $harmonyWarningMdPath).Trim()
        }

        return @"
- Dependencies: Requires 0_TFP_Harmony (built-in game mod).
  - To check, confirm Mods/0_TFP_Harmony exists in the game folder.
  - To restore, run Steam Verify integrity of game files.
"@.Trim()
    }

    return '- Dependencies: None, works standalone.'
}

$dependenciesBlock = Build-DependenciesBlock $row.DEPENDENCIES

function Test-HasHarmonyDependency {
    param([string]$Value)

    if ($null -eq $Value) { return $false }
    $raw = ($Value + '').Trim()
    if ($raw.Length -eq 0) { return $false }

    foreach ($part in ([regex]::Split($raw, '[;\n|,]+'))) {
        $token = $part.Trim().ToLowerInvariant()
        if ($token.Length -eq 0) { continue }
        if ($token -eq 'x') { return $true }
        if ([regex]::IsMatch($token, '\b0[_\-\s]*tfp[_\-\s]*harmony\b')) { return $true }
        if ($token -eq 'harmony' -or $token.StartsWith('harmony ') -or $token.EndsWith(' harmony')) { return $true }
    }

    return $false
}

function Ensure-Sentence {
    param([string]$Text)
    if ($null -eq $Text) { $Text = '' }
    $cleaned = [regex]::Replace($Text, '\s+', ' ').Trim()
    if ($cleaned.Length -eq 0) { return '' }
    if ($cleaned -match '[.!?]$') { return $cleaned }
    return "$cleaned."
}

function Remove-StandaloneBulletLine {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return $Text }

    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($rawLine in ($Text -split "`r?`n")) {
        $line = ($rawLine + '').TrimEnd()
        $match = [regex]::Match($line, '^(\s*(?:[-*]|\d+\.)\s+)Works standalone\.\s*(.*)$')
        if (-not $match.Success) {
            $lines.Add($line)
            continue
        }

        $tail = ($match.Groups[2].Value + '').Trim()
        if ($tail.Length -gt 0) {
            $lines.Add($match.Groups[1].Value + $tail)
        }
    }

    return (($lines -join "`n").Trim())
}

function Remove-FeaturePointerLines {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return $Text }

    $patterns = @(
        '^(\s*(?:[-*]|\d+\.)\s+)See this mod''s README for full details\.\s*$',
        '^(\s*(?:[-*]|\d+\.)\s+)See Other Details below for full feature details\.\s*$'
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($rawLine in ($Text -split "`r?`n")) {
        $line = ($rawLine + '').TrimEnd()
        $drop = $false
        foreach ($pattern in $patterns) {
            if ([regex]::IsMatch($line, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
                $drop = $true
                break
            }
        }
        if (-not $drop) {
            $lines.Add($line)
        }
    }

    return (($lines -join "`n").Trim())
}

function Build-ModTypeBlock {
    param([string]$ModTypeLine)

    if ($null -eq $ModTypeLine) { $ModTypeLine = '' }
    $raw = $ModTypeLine.Trim()
    if ($raw.Length -eq 0 -or $raw -in @('MISSINGDATA', 'TBD')) {
        return '- Mod Type: MISSINGDATA'
    }

    $typeName = $raw
    $details = ''
    $split = [regex]::Match($raw, '^(.*?):\s*(.+)$')
    if ($split.Success) {
        $typeName = $split.Groups[1].Value.Trim()
        $details = $split.Groups[2].Value.Trim()
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    if ($typeName.Length -gt 0) {
        $lines.Add("- Mod Type: $typeName")
    }
    else {
        $lines.Add("- Mod Type: $raw")
    }

    if ($details.Length -eq 0) {
        return ($lines -join "`n")
    }

    $detailLines = [System.Collections.Generic.List[string]]::new()
    $detailsNoParen = [regex]::Replace($details, '\([^()]*\)', '')
    foreach ($segment in ($detailsNoParen -split ';')) {
        $sentence = Ensure-Sentence ($segment.Trim(' ', '.'))
        if ($sentence.Length -gt 0) {
            $detailLines.Add($sentence)
        }
    }

    foreach ($m in [regex]::Matches($details, '\(([^()]*)\)')) {
        $sentence = Ensure-Sentence ($m.Groups[1].Value.Trim(' ', '.'))
        if ($sentence.Length -gt 0) {
            $detailLines.Add($sentence)
        }
    }

    if ($detailLines.Count -eq 0) {
        $sentence = Ensure-Sentence $details
        if ($sentence.Length -gt 0) {
            $detailLines.Add($sentence)
        }
    }

    foreach ($sentence in $detailLines) {
        $lines.Add("  - $sentence")
    }

    return ($lines -join "`n")
}

$modTypeBlock = Build-ModTypeBlock $modTypeLine
$uniqueLine = ""
$featuresBody = ""
$deeperDetailsBody = ""
$changelogBody = ""

$existingReadmeMdPath = Join-Path $modPath "README.md"
if (Test-Path $existingReadmeMdPath) {
    try {
        $existingReadmeMd = Get-Content -Raw -Encoding UTF8 $existingReadmeMdPath
        $summaryMatch = [regex]::Match($existingReadmeMd, '<!-- FEATURES-SUMMARY START -->([\s\S]*?)<!-- FEATURES-SUMMARY END -->')
        if ($summaryMatch.Success) {
            $featuresBody = $summaryMatch.Groups[1].Value.Trim()
        }
        $detailsMatch = [regex]::Match($existingReadmeMd, '<!-- FEATURES-DETAILED START -->([\s\S]*?)<!-- FEATURES-DETAILED END -->')
        if ($detailsMatch.Success) {
            $deeperDetailsBody = $detailsMatch.Groups[1].Value.Trim()
        }
        $changelogMatch = [regex]::Match($existingReadmeMd, '<!-- CHANGELOG START -->([\s\S]*?)<!-- CHANGELOG END -->')
        if ($changelogMatch.Success) {
            $changelogBody = $changelogMatch.Groups[1].Value.Trim()
        }
        if ([string]::IsNullOrWhiteSpace($changelogBody)) {
            $changelogSectionMatch = [regex]::Match($existingReadmeMd, '(?ms)^##\s*\d+\.\s*Changelog\s*$([\s\S]*?)(?=^##\s+\d+\.|\z)')
            if ($changelogSectionMatch.Success) {
                $changelogBody = $changelogSectionMatch.Groups[1].Value.Trim()
            }
        }
    }
    catch {
        # Keep defaults when section extraction fails.
    }
}

if ([string]::IsNullOrWhiteSpace($featuresBody)) {
    $fallbackFeature = Ensure-Sentence $modDescription
    if ([string]::IsNullOrWhiteSpace($fallbackFeature)) {
        $fallbackFeature = 'Feature summary coming soon.'
    }
    $featuresBody = "- $fallbackFeature"
}

$featuresBody = Remove-StandaloneBulletLine $featuresBody
$featuresBody = Remove-FeaturePointerLines $featuresBody
$deeperDetailsBody = Remove-StandaloneBulletLine $deeperDetailsBody
# Flatten legacy nested list indentation in changelog to avoid extra-indented bullets on regen.
$changelogBody = [regex]::Replace($changelogBody, '(?m)^[ \t]+(?=(?:[-*]\s+|\d+\.\s+))', '')

$otherDetailsSection = ""
if (-not [string]::IsNullOrWhiteSpace($deeperDetailsBody)) {
    $otherDetailsSection = @"
------------------------------------------------------------
OTHER DETAILS
------------------------------------------------------------

$deeperDetailsBody
"@.TrimEnd()
}
$quoteMd = ""
$quotePath = "c:/GitHub/7D2D-Mods/_Quotes/$($row.QUOTE_FILE)"
if ((-not [string]::IsNullOrWhiteSpace($row.QUOTE_FILE)) -and (Test-Path $quotePath)) {
    $qtRaw = Get-Content -Raw -Encoding UTF8 $quotePath
    if ($qtRaw) {
        $qt = $qtRaw.Trim()
        if ($qt.Length -gt 0) {
            $flatQuote = (($qt -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 }) -join " ").Trim()
            if ($flatQuote.Length -gt 0) {
                if ($flatQuote.StartsWith('"') -and $flatQuote.EndsWith('"')) {
                    $quoteMd = $flatQuote
                }
                else {
                    $quoteMd = '"' + $flatQuote.Trim('"') + '"'
                }
            }
        }
    }
}

function Build-TitleCardCalloutBlock {
    param([string]$QuoteText)

    $noteLine = 'NOTE: AGF Mod Guide and Changelog are further below.'
    if (-not [string]::IsNullOrWhiteSpace($QuoteText)) {
        return (@($QuoteText.Trim(), '', $noteLine) -join "`n")
    }

    # Keep the intentional callout pause when no quote is present.
    return (@('', $noteLine) -join "`n")
}

$template = Get-Content -Raw -Encoding UTF8 $templatePath
$modscope = Get-Content -Raw -Encoding UTF8 $modscopePath
$short = Get-Content -Raw -Encoding UTF8 $shortPath

function Ensure-TrailingBlankCount {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [int]$Count
    )
    $trailing = 0
    for ($i = $Lines.Count - 1; $i -ge 0; $i--) {
        if ($Lines[$i] -eq '') {
            $trailing++
        }
        else {
            break
        }
    }

    if ($trailing -lt $Count) {
        for ($j = 0; $j -lt ($Count - $trailing); $j++) {
            $Lines.Add('')
        }
    }
    elseif ($trailing -gt $Count) {
        for ($j = 0; $j -lt ($trailing - $Count); $j++) {
            $Lines.RemoveAt($Lines.Count - 1)
        }
    }
}

function Apply-DividerSpacing {
    param(
        [string]$Text,
        [string]$MajorDivider,
        [string]$MinorDivider
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($line in ($Text -split "`n", 0, 'SimpleMatch')) {
        $lines.Add($line)
    }

    $output = [System.Collections.Generic.List[string]]::new()
    $idx = 0
    while ($idx -lt $lines.Count) {
        $line = $lines[$idx].TrimEnd()

        if (
            $line -eq $MajorDivider -and
            ($idx + 2) -lt $lines.Count -and
            -not [string]::IsNullOrWhiteSpace($lines[$idx + 1]) -and
            $lines[$idx + 2].TrimEnd() -eq $MajorDivider
        ) {
            if ($output.Count -gt 0) {
                Ensure-TrailingBlankCount -Lines $output -Count 3
            }
            $output.Add($MajorDivider)
            $output.Add($lines[$idx + 1].Trim())
            $output.Add($MajorDivider)

            $nextIdx = $idx + 3
            while ($nextIdx -lt $lines.Count -and [string]::IsNullOrWhiteSpace($lines[$nextIdx])) {
                $nextIdx += 1
            }
            Ensure-TrailingBlankCount -Lines $output -Count 1
            $idx = $nextIdx
            continue
        }

        if (
            $line -eq $MinorDivider -and
            ($idx + 2) -lt $lines.Count -and
            -not [string]::IsNullOrWhiteSpace($lines[$idx + 1]) -and
            $lines[$idx + 2].TrimEnd() -eq $MinorDivider
        ) {
            if ($output.Count -gt 0) {
                Ensure-TrailingBlankCount -Lines $output -Count 2
            }
            $output.Add($MinorDivider)
            $output.Add($lines[$idx + 1].Trim())
            $output.Add($MinorDivider)

            $nextIdx = $idx + 3
            while ($nextIdx -lt $lines.Count -and [string]::IsNullOrWhiteSpace($lines[$nextIdx])) {
                $nextIdx += 1
            }
            Ensure-TrailingBlankCount -Lines $output -Count 1
            $idx = $nextIdx
            continue
        }

        if ($line -eq $MajorDivider -or $line -eq $MinorDivider) {
            if ($output.Count -gt 0) {
                Ensure-TrailingBlankCount -Lines $output -Count 1
            }
            $output.Add($line)

            $nextIdx = $idx + 1
            while ($nextIdx -lt $lines.Count -and [string]::IsNullOrWhiteSpace($lines[$nextIdx])) {
                $nextIdx += 1
            }
            Ensure-TrailingBlankCount -Lines $output -Count 1
            $idx = $nextIdx
            continue
        }

        $output.Add($line)
        $idx += 1
    }

    while ($output.Count -gt 0 -and $output[0] -eq '') { $output.RemoveAt(0) }
    while ($output.Count -gt 0 -and $output[$output.Count - 1] -eq '') { $output.RemoveAt($output.Count - 1) }

    return ($output -join "`n")
}

function Normalize-ListIndentation {
    param([string]$Text)

    $lines = $Text -split "`n", 0, 'SimpleMatch'
    $out = [System.Collections.Generic.List[string]]::new()

    foreach ($rawLine in $lines) {
        $line = $rawLine.TrimEnd()
        $match = [regex]::Match($line, '^(\s*)(\d+\.\s+|-\s+)(.*)$')
        if (-not $match.Success) {
            $out.Add($line)
            continue
        }

        $leading = $match.Groups[1].Value.Replace("`t", "    ")
        $marker = $match.Groups[2].Value
        $body = $match.Groups[3].Value
        $level = [Math]::Max(0, [int]([Math]::Floor($leading.Length / 2.0)))
        $indent = 2 + ($level * 2)
        $out.Add((' ' * $indent) + $marker + $body)
    }

    return ($out -join "`n")
}

function Should-PreserveUnwrappedLine {
    param(
        [string]$Line,
        [string]$MajorDivider,
        [string]$MinorDivider
    )

    $trimmed = $Line.Trim()
    if ($trimmed.Length -eq 0) { return $true }
    if ($trimmed -eq $MajorDivider -or $trimmed -eq $MinorDivider) { return $true }
    if ($trimmed -match '^(https?://\S+|www\.\S+)$') { return $true }
    if ($trimmed -match '^([A-Za-z]:[\\/].*|%[A-Za-z0-9_]+%)$') { return $true }
    return $false
}

function Wrap-WithIndent {
    param(
        [string]$Text,
        [int]$Width,
        [string]$InitialIndent,
        [string]$SubsequentIndent,
        [bool]$AllowWordBreak = $false
    )

    $result = [System.Collections.Generic.List[string]]::new()
    $words = $Text -split '\s+'
    $currentIndent = $InitialIndent
    $currentText = ""

    foreach ($word in $words) {
        if ([string]::IsNullOrWhiteSpace($word)) { continue }
        $pending = $word

        while ($pending.Length -gt 0) {
            $candidate = if ($currentText.Length -eq 0) { $pending } else { "$currentText $pending" }
            if (($currentIndent.Length + $candidate.Length) -le $Width) {
                $currentText = $candidate
                break
            }

            if ($currentText.Length -gt 0) {
                $result.Add($currentIndent + $currentText)
                $currentIndent = $SubsequentIndent
                $currentText = ""
                continue
            }

            if (-not $AllowWordBreak) {
                $result.Add($currentIndent + $pending)
                $currentIndent = $SubsequentIndent
                break
            }

            $chunkWidth = [Math]::Max(1, $Width - $currentIndent.Length)
            if ($pending.Length -le $chunkWidth) {
                $currentText = $pending
                break
            }

            $result.Add($currentIndent + $pending.Substring(0, $chunkWidth))
            $pending = $pending.Substring($chunkWidth)
            $currentIndent = $SubsequentIndent
        }
    }

    if ($currentText.Length -gt 0) {
        $result.Add($currentIndent + $currentText)
    }

    return $result
}

function Wrap-TextToWidth {
    param(
        [string]$Text,
        [int]$Width,
        [string]$MajorDivider,
        [string]$MinorDivider
    )

    $lines = $Text -split "`n", 0, 'SimpleMatch'
    $wrapped = [System.Collections.Generic.List[string]]::new()

    for ($idx = 0; $idx -lt $lines.Count; $idx++) {
        $rawLine = $lines[$idx]
        $line = $rawLine.TrimEnd()

        if ((Should-PreserveUnwrappedLine -Line $line -MajorDivider $MajorDivider -MinorDivider $MinorDivider) -or $line.Length -le $Width) {
            $wrapped.Add($line)
            continue
        }

        $allowWordBreak = $false

        $listMatch = [regex]::Match($line, '^(\s*)(\d+\.\s+|-\s+)(.*)$')
        if ($listMatch.Success) {
            $leading = $listMatch.Groups[1].Value
            $marker = $listMatch.Groups[2].Value
            $body = $listMatch.Groups[3].Value.Trim()
            if ($body.Length -eq 0) {
                $wrapped.Add($line)
                continue
            }

            $initialIndent = $leading + $marker
            $subsequentIndent = ' ' * $initialIndent.Length
            $segments = @(Wrap-WithIndent -Text $body -Width $Width -InitialIndent $initialIndent -SubsequentIndent $subsequentIndent -AllowWordBreak $allowWordBreak)
            foreach ($segment in $segments) {
                $wrapped.Add($segment)
            }
            continue
        }

        $leading = ([regex]::Match($line, '^\s*')).Value
        $body = $line.Substring($leading.Length).Trim()
        if ($body.Length -eq 0) {
            $wrapped.Add($line)
            continue
        }

        $segments = @(Wrap-WithIndent -Text $body -Width $Width -InitialIndent $leading -SubsequentIndent $leading -AllowWordBreak $allowWordBreak)
        foreach ($segment in $segments) {
            $wrapped.Add($segment)
        }
    }

    return ($wrapped -join "`n")
}

function Center-H1Titles {
    param(
        [string]$Text,
        [int]$Width = 72
    )

    $major = "=" * $Width
    $lines = $Text -split "`n", 0, 'SimpleMatch'
    $out = [System.Collections.Generic.List[string]]::new()

    $idx = 0
    while ($idx -lt $lines.Count) {
        $line = $lines[$idx].TrimEnd()
        if (
            $line -eq $major -and
            ($idx + 2) -lt $lines.Count -and
            $lines[$idx + 2].TrimEnd() -eq $major
        ) {
            $title = $lines[$idx + 1].Trim()
            $left = [Math]::Max(0, [int]([Math]::Floor(($Width - $title.Length) / 2.0)))
            $right = [Math]::Max(0, $Width - $title.Length - $left)
            $out.Add($major)
            $out.Add((' ' * $left) + $title + (' ' * $right))
            $out.Add($major)
            $idx += 3
            continue
        }

        $out.Add($line)
        $idx += 1
    }

    return ($out -join "`n")
}

function Format-ChangelogText {
    param(
        [string]$Body,
        [int]$Width = 72,
        [bool]$IncludeH1 = $true
    )

    $major = "=" * $Width
    $minor = "-" * $Width
    $verPattern = '^(?i)v\d+(?:\.\d+){0,3}(?:\b.*)?$'

    $sections = [System.Collections.Generic.List[object]]::new()
    $currentTitle = ''
    $currentBody = [System.Collections.Generic.List[string]]::new()

    function Add-ChangelogSection {
        param(
            [string]$Title,
            [System.Collections.Generic.List[string]]$Lines,
            [System.Collections.Generic.List[object]]$Target
        )

        if ([string]::IsNullOrWhiteSpace($Title)) { return }
        $Target.Add([PSCustomObject]@{ Title = $Title.Trim(); Body = @($Lines) })
    }

    foreach ($raw in (($Body + '') -split "`r?`n")) {
        $s = $raw.Trim()
        if ($s.Length -eq 0) { continue }
        $s = $s -replace '^#+\s*',''
        $s = $s -replace '\[([^\]]+)\]\(([^\)]+)\)','$1: $2'
        $s = $s -replace '`|\*|~',''
        if ($s -match '^[=-]{10,}$') { continue }
        if ($s.ToLowerInvariant() -eq 'changelog') { continue }

        if ($s -match $verPattern) {
            Add-ChangelogSection -Title $currentTitle -Lines $currentBody -Target $sections
            $currentTitle = $s
            $currentBody = [System.Collections.Generic.List[string]]::new()
            continue
        }

        if ([string]::IsNullOrWhiteSpace($currentTitle)) {
            $currentTitle = 'Notes'
        }

        if ($s -match '^(?:[-*]\s+|\d+\.\s+)') {
            if ($s.StartsWith('* ')) { $s = '- ' + $s.Substring(2) }
            $currentBody.Add('  ' + $s)
        }
        else {
            $currentBody.Add('  - ' + $s)
        }
    }

    Add-ChangelogSection -Title $currentTitle -Lines $currentBody -Target $sections
    if ($sections.Count -eq 0) {
        $sections.Add([PSCustomObject]@{ Title = 'Notes'; Body = @('  - Add changelog entries here.') })
    }

    $out = [System.Collections.Generic.List[string]]::new()
    if ($IncludeH1) {
        $title = 'CHANGELOG'
        $left = [Math]::Max(0, [int]([Math]::Floor(($Width - $title.Length) / 2.0)))
        $right = [Math]::Max(0, $Width - $title.Length - $left)
        $titleCentered = (' ' * $left) + $title + (' ' * $right)
        $out.Add($major)
        $out.Add($titleCentered)
        $out.Add($major)
        $out.Add('')
    }

    for ($i = 0; $i -lt $sections.Count; $i++) {
        $sec = $sections[$i]
        if ($i -gt 0) {
            $out.Add($minor)
            $out.Add('')
        }

        $out.Add(($sec.Title + '').Trim())
        if ($sec.Body.Count -eq 0) {
            $out.Add('  - Add changelog entries here.')
        }
        else {
            foreach ($b in $sec.Body) { $out.Add($b) }
        }
        $out.Add('')
    }

    while ($out.Count -gt 0 -and $out[$out.Count - 1] -eq '') { $out.RemoveAt($out.Count - 1) }
    return ($out -join "`n")
}

$content = $template
$content = $content.Replace("{{MODSCOPE_GUIDE_BODY}}", $modscope)
$shortGuideRawToken = "__AGF_SHORT_GUIDE_RAW_BLOCK__"
$content = $content.Replace("{{MODGUIDE_TEXT_BODY}}", $shortGuideRawToken)
$content = $content.Replace("{{SHORT_GUIDE_BODY}}", $shortGuideRawToken)
$content = $content.Replace("{{MOD_NAME}}", $modName)
$content = $content.Replace("{{MOD_NAME_UPPER}}", $modName.ToUpperInvariant())
$content = $content.Replace("{{MOD_VERSION}}", $modVersion)
$content = $content.Replace("{{MOD_DESCRIPTION}}", $modDescription)
$titleCardCalloutBlock = Build-TitleCardCalloutBlock -QuoteText $quoteMd
if ($content.Contains("{{TITLE_CARD_CALLOUT_BLOCK}}")) {
    $content = $content.Replace("{{TITLE_CARD_CALLOUT_BLOCK}}", $titleCardCalloutBlock)
}
else {
    # Backward compatibility for older templates that still use {{QUOTE}}.
    $content = $content.Replace("{{QUOTE}}", $quoteMd)
}
$content = $content.Replace("{{MOD_TYPE_LINE}}", $modTypeLine)
$content = $content.Replace("{{MOD_TYPE_BLOCK}}", $modTypeBlock)
$content = $content.Replace("{{TESTED_GAME_VERSION}}", $row.TESTED_GAME_VERSION)
$content = $content.Replace("{{SAFE_TO_INSTALL}}", $safeToInstall)
$content = $content.Replace("{{SAFE_TO_REMOVE}}", $safeToRemove)
$content = $content.Replace("{{DEPENDENCIES_BLOCK}}", $dependenciesBlock)
$content = $content.Replace("{{UNIQUE_LINE}}", $uniqueLine)
$content = $content.Replace("{{FEATURES_BODY}}", $featuresBody)
$content = $content.Replace("{{OTHER_DETAILS_SECTION}}", $otherDetailsSection)
$content = $content.Replace("{{DEEPER_DETAILS_BODY}}", $deeperDetailsBody)
$templateHasChangelogPlaceholder = $content.Contains("{{CHANGELOG_BODY}}")
$changelogBodyText = Format-ChangelogText -Body $changelogBody -Width 72 -IncludeH1:$false
if ($templateHasChangelogPlaceholder) {
    $content = $content.Replace("{{CHANGELOG_BODY}}", $changelogBodyText)
}

$txt = $content
$wrapWidth = 72
$majorDivider = "=" * $wrapWidth
$minorDivider = "-" * $wrapWidth
$txt = $txt -replace "`r`n", "`n"
$txt = $txt -replace "`r", "`n"
$txt = [regex]::Replace($txt, '```[\s\S]*?```', '')
$txt = [regex]::Replace($txt, '!\[[^\]]*\]\([^\)]*\)', '')
$txt = [regex]::Replace($txt, '<!--[\s\S]*?-->', '')
$txt = [regex]::Replace($txt, '\[([^\]]+)\]\(([^\)]+)\)', '$1: $2')
$txt = [regex]::Replace($txt, '[`*~]', '')
$txt = [regex]::Replace($txt, '^[ \t]*---+[ \t]*\r?$', $minorDivider, [System.Text.RegularExpressions.RegexOptions]::Multiline)
$txt = [regex]::Replace($txt, '^#+\s*', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
$txt = [regex]::Replace($txt, '^>\s?', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
$txt = [regex]::Replace($txt, '^[ \t]*={3,}[ \t]*\r?$', $majorDivider, [System.Text.RegularExpressions.RegexOptions]::Multiline)
$txt = [regex]::Replace($txt, '^[ \t]*-{3,}[ \t]*\r?$', $minorDivider, [System.Text.RegularExpressions.RegexOptions]::Multiline)
$txt = Normalize-ListIndentation -Text $txt
$txt = [regex]::Replace($txt, "`n$([regex]::Escape($majorDivider))`n(?:`n$([regex]::Escape($majorDivider))`n)+", "`n$majorDivider`n")
$txt = [regex]::Replace($txt, "`n$([regex]::Escape($minorDivider))`n(?:`n$([regex]::Escape($minorDivider))`n)+", "`n$minorDivider`n")
$txt = [regex]::Replace($txt, '^[ \t]+$', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
$txt = Apply-DividerSpacing -Text $txt -MajorDivider $majorDivider -MinorDivider $minorDivider
$txt = Wrap-TextToWidth -Text $txt -Width $wrapWidth -MajorDivider $majorDivider -MinorDivider $minorDivider
$txt = Center-H1Titles -Text $txt -Width $wrapWidth
$txt = [regex]::Replace($txt, "`n{5,}", "`n`n`n`n")
$txt = $txt.Trim()
if ($txt.Contains($shortGuideRawToken)) {
    $shortTrim = $short.Trim("`r", "`n")
    $shortGuidePattern = "(?:`n)*" + [regex]::Escape($shortGuideRawToken) + "(?:`n)*"
    $txt = [regex]::Replace($txt, $shortGuidePattern, "`n`n`n`n$shortTrim`n`n`n`n", 1)
}

$changelogText = Format-ChangelogText -Body $changelogBody -Width $wrapWidth -IncludeH1:$true
if ((-not $templateHasChangelogPlaceholder) -and (-not [string]::IsNullOrWhiteSpace($changelogText))) {
    $txt = $txt.TrimEnd() + "`n`n`n`n" + $changelogText
}

$txt = $txt.TrimEnd()

$outPath = Join-Path $modPath "README.txt"
Set-Content -Path $outPath -Encoding UTF8 -Value $txt
Write-Output "WROTE: $outPath"
