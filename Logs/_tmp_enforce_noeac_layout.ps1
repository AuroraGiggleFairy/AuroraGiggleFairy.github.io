$root = "C:\GitHub\7D2D-Mods\_NoEAC-Projects"
$excludeTop = @('.vscode','Decompiled DLLs','_Decompiled DLLs')
$artifactDirNames = @('bin','obj','Debug','Release','TestResults','.vs','.vscode')
$projects = Get-ChildItem $root -Directory | Where-Object { $excludeTop -notcontains $_.Name }
$copied = @(); $removed = @(); $noDll = @()

function Get-AssemblyNameFromCsproj($csprojPath) {
  $fallback = [System.IO.Path]::GetFileNameWithoutExtension($csprojPath)
  try {
    $xml = [xml](Get-Content -Raw $csprojPath)
    $asm = $xml.Project.PropertyGroup.AssemblyName | Where-Object { $_ -and $_.Trim() -ne '' } | Select-Object -First 1
    if ($asm) { return [string]$asm }
  } catch {}
  return $fallback
}

foreach ($folder in $projects) {
  $csprojs = Get-ChildItem $folder.FullName -Recurse -File -Filter *.csproj -ErrorAction SilentlyContinue
  if ($csprojs.Count -eq 0) { continue }

  $primary = $csprojs | Sort-Object { $_.FullName.Length } | Select-Object -First 1
  $asm = Get-AssemblyNameFromCsproj $primary.FullName
  $dllName = "$asm.dll"

  $dllCandidates = Get-ChildItem $folder.FullName -Recurse -File -Filter $dllName -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
  if ($dllCandidates.Count -eq 0) {
    $noDll += "$($folder.Name) (expected $dllName)"
  } else {
    $latest = $dllCandidates | Select-Object -First 1
    $target = Join-Path $folder.FullName $dllName
    $needCopy = $true
    if (Test-Path $target) {
      $srcTime = (Get-Item $latest.FullName).LastWriteTimeUtc
      $dstTime = (Get-Item $target).LastWriteTimeUtc
      if ($srcTime -le $dstTime -and $latest.FullName -ieq $target) { $needCopy = $false }
    }
    if ($needCopy) {
      Copy-Item -Path $latest.FullName -Destination $target -Force
      $copied += "$($folder.Name): $($latest.FullName.Replace($root+'\\','')) -> $dllName"
    }
  }

  foreach ($name in $artifactDirNames) {
    Get-ChildItem $folder.FullName -Directory -Recurse -ErrorAction SilentlyContinue |
      Where-Object { $_.Name -ieq $name } |
      Sort-Object FullName -Descending |
      ForEach-Object {
        try {
          Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction Stop
          $removed += "$($folder.Name): removed artifact dir $($_.FullName.Replace($root+'\\',''))"
        } catch {}
      }
  }

  Get-ChildItem $folder.FullName -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^AGF-.*-v\d+(\.\d+)*$' } |
    ForEach-Object {
      try {
        Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction Stop
        $removed += "$($folder.Name): removed package dir $($_.Name)"
      } catch {}
    }
}

Write-Output "Copied-to-root DLL count: $($copied.Count)"
$copied | ForEach-Object { "COPIED: $_" }
Write-Output "Removed directory count: $($removed.Count)"
$removed | ForEach-Object { "REMOVED: $_" }
Write-Output "Projects missing expected DLL: $($noDll.Count)"
$noDll | ForEach-Object { "NO_DLL: $_" }
