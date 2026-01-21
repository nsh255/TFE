$ErrorActionPreference = 'Stop'

$root = "c:\Users\usuario\Desktop\TFE\Ascension\Ascension"
$prefabDir = Join-Path $root "Assets\Resources\Enemies"

$metaFiles = Get-ChildItem -Path $root -Recurse -Filter *.meta -File -ErrorAction SilentlyContinue
$guidToMeta = @{}
foreach ($m in $metaFiles) {
  $first = Select-String -Path $m.FullName -Pattern '^guid: ' -List -ErrorAction SilentlyContinue
  if ($first) {
    $guid = $first.Line.Substring(6).Trim()
    if (-not $guidToMeta.ContainsKey($guid)) { $guidToMeta[$guid] = $m.FullName }
  }
}

$prefabs = Get-ChildItem -Path $prefabDir -Filter *.prefab -File -ErrorAction Stop

$missingScriptRefs = @()
$nullEnemyData = @()

foreach ($p in $prefabs) {
  $text = Get-Content -Path $p.FullName -Raw

  if ($text -match 'enemyData:\s*\{fileID:\s*0\s*\}') {
    $nullEnemyData += $p.Name
  }

  $scriptGuidMatches = [regex]::Matches($text, 'm_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([0-9a-f]{32}),\s*type:\s*3\}')
  foreach ($m in $scriptGuidMatches) {
    $guid = $m.Groups[1].Value
    if (-not $guidToMeta.ContainsKey($guid)) {
      $missingScriptRefs += [pscustomobject]@{ Prefab = $p.Name; MissingScriptGuid = $guid }
    }
  }
}

"--- Prefabs with enemyData NULL ---"
$nullEnemyData | Sort-Object
""
"--- Missing script GUID references in enemy prefabs ---"
if ($missingScriptRefs.Count -eq 0) {
  "(none found)"
} else {
  $missingScriptRefs | Sort-Object Prefab | Format-Table -AutoSize | Out-String
}
