$managed = "C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed"
$asm = [System.Reflection.Assembly]::LoadFrom("$managed\Assembly-CSharp.dll")
$type = $asm.GetType("ChunkGameObject")
if ($type) {
    Write-Host "ChunkGameObject found"
    $type.GetMethods([System.Reflection.BindingFlags]"Public,NonPublic,Instance") | 
        Where-Object { $_.Name -like "*Create*" -or $_.Name -like "*Position*" } |
        ForEach-Object { Write-Host "  Method: $($_.Name)" }
}
