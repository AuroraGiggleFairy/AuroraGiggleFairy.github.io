$dumpPath = 'c:/GitHub/7D2D-Mods/_DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/temp/fullgen-compare/dump/Config/XUi_InGame/windows.xml'
$outPath = 'c:/GitHub/7D2D-Mods/temp/PurpleBookCompat-StrictParity-Dishong.txt'

$doc = New-Object System.Xml.XmlDocument
$doc.PreserveWhitespace = $true
$doc.Load($dumpPath)

function Get-Node([string]$xpath) {
    $n = $doc.SelectSingleNode($xpath)
    if (-not $n) {
        throw "Missing node: $xpath"
    }
    return $n
}

function Add-SetAttr(
    [System.Collections.Generic.List[string]]$lines,
    [string]$xpath,
    [System.Xml.XmlNode]$node,
    [string[]]$attrs
) {
    foreach ($a in $attrs) {
        if ($node.Attributes[$a]) {
            $v = $node.Attributes[$a].Value
            $lines.Add(('{0}<set xpath="{1}/@{2}">{3}</set>' -f "`t`t", $xpath, $a, $v))
        }
    }
}

$skills = @(
    @{
        Name = 'craftingWorkstations'
        Checklist = 'checklistWorkstations'
        Zoomed = 'zoomedWorkstations'
        Title = 'checklistWorkstationsTitle'
        Label = 'checklistWorkstationsMagName'
    },
    @{
        Name = 'craftingVehicles'
        Checklist = 'checklistVehicles'
        Zoomed = 'zoomedVehicles'
        Title = 'checklistVehiclesTitle'
        Label = 'checklistVehiclesMagName'
    },
    @{
        Name = 'craftingSeeds'
        Checklist = 'checklistSeeds'
        Zoomed = 'zoomedSeeds'
        Title = 'checklistSeedsTitle'
        Label = 'checklistSeedsMagName'
    }
)

$lines = New-Object 'System.Collections.Generic.List[string]'
$lines.Add('<conditional>')
$lines.Add('<if cond="mod_loaded(''AGF-HUDPlus-PurpleBook'')">')
$lines.Add('    <!-- Auto-generated strict parity block from fullgen dump windows; replaces checklist sections and zoomed entries to mirror known-good overwrite. -->')

foreach ($s in $skills) {
    $checkPath = "/windows/window[@name='Schematics']//rect[@name='$($s.Checklist)']"
    $zoomPath = "/windows/window[@name='Schematics']//rect[@name='$($s.Zoomed)']"
    $lookupCheckPath = "//window[@name='Schematics']//rect[@name='$($s.Checklist)']"
    $lookupZoomPath = "//window[@name='Schematics']//rect[@name='$($s.Zoomed)']"

    $checkNode = Get-Node $lookupCheckPath
    $zoomNode = Get-Node $lookupZoomPath
    $labelNode = $zoomNode.SelectSingleNode("./label[@name='$($s.Label)']")
    $titleGrid = $zoomNode.SelectSingleNode("./grid[@name='$($s.Title)']")
    $listGrid = $zoomNode.SelectSingleNode("./grid[@name='$($s.Checklist)']")

    if (-not $labelNode -or -not $titleGrid -or -not $listGrid) {
        throw "Missing zoomed nodes for $($s.Name)"
    }

    $lines.Add('')
    $lines.Add(('{0}<!-- {1}: strict parity from dump windows -->' -f "`t`t", $s.Name))

    Add-SetAttr $lines $checkPath $checkNode @('pos', 'width', 'height')
    Add-SetAttr $lines ("$zoomPath/label[@name='$($s.Label)']") $labelNode @('pos', 'width', 'height', 'justify', 'font_size', 'text_key')
    Add-SetAttr $lines ("$zoomPath/grid[@name='$($s.Title)']") $titleGrid @('rows', 'cols', 'pos', 'cell_width', 'cell_height', 'repeat_content', 'arrangement', 'controller')
    Add-SetAttr $lines ("$zoomPath/grid[@name='$($s.Checklist)']") $listGrid @('rows', 'cols', 'pos', 'cell_width', 'cell_height', 'repeat_content', 'arrangement', 'controller')

    $lines.Add(('{0}<remove xpath="{1}/rect[starts-with(@name,''Section'')]"/>' -f "`t`t", $checkPath))
    $lines.Add(('{0}<remove xpath="{1}/grid[@name=''{2}'']/entry"/>' -f "`t`t", $zoomPath, $s.Checklist))

    $sectionNodes = $checkNode.SelectNodes("./rect[starts-with(@name,'Section')]")
    $lines.Add(('{0}<append xpath="{1}">' -f "`t`t", $checkPath))
    foreach ($sec in $sectionNodes) {
        $lines.Add(('{0}{1}' -f "`t`t`t", $sec.OuterXml.Trim()))
    }
    $lines.Add('`t`t</append>')

    $entryNodes = $listGrid.SelectNodes('./entry')
    $lines.Add(('{0}<append xpath="{1}/grid[@name=''{2}'']">' -f "`t`t", $zoomPath, $s.Checklist))
    foreach ($en in $entryNodes) {
        $lines.Add(('{0}{1}' -f "`t`t`t", $en.OuterXml.Trim()))
    }
    $lines.Add('`t`t</append>')

    $magFillNode = $titleGrid.SelectSingleNode("./entry[@name='Magazine']/filledsprite[@name='yesUnlocked']")
    $lvlLabel = $titleGrid.SelectSingleNode("./entry[@name='level']/label[2]")
    if ($magFillNode -and $magFillNode.Attributes['fill']) {
        $lines.Add(('{0}<set xpath="{1}/grid[@name=''{2}'']/entry[@name=''Magazine'']/filledsprite[@name=''yesUnlocked'']/@fill">{3}</set>' -f "`t`t", $zoomPath, $s.Title, $magFillNode.Attributes['fill'].Value))
    }
    if ($lvlLabel -and $lvlLabel.Attributes['text']) {
        $lines.Add(('{0}<set xpath="{1}/grid[@name=''{2}'']/entry[@name=''level'']/label[2]/@text">{3}</set>' -f "`t`t", $zoomPath, $s.Title, $lvlLabel.Attributes['text'].Value))
    }
}

$lines.Add('</if>')
$lines.Add('</conditional>')
Set-Content -Path $outPath -Value ($lines -join "`r`n") -Encoding UTF8
Write-Output "GENERATED:$outPath"
