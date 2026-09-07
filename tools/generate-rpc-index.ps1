# Generate IVXRpcIndex.generated.json
# Run from repo root: powershell -File tools/generate-rpc-index.ps1

$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\Packages\com.intelliversex.sdk"
$ids = New-Object "System.Collections.Generic.HashSet[string]"

Get-ChildItem $root -Recurse -Filter *.cs |
  Where-Object { $_.FullName -notmatch '\\Tests' } |
  ForEach-Object {
    foreach ($line in Get-Content $_.FullName) {
      if ($line -match 'const string RPC_\w+\s*=\s*"([a-z0-9_]+)"') { [void]$ids.Add($Matches[1]) }
      if ($line -match 'RpcAsync\([^,]+,\s*"([a-z0-9_]+)"') { [void]$ids.Add($Matches[1]) }
    }
  }

[void]$ids.Add("nakama_js_health")
$sorted = $ids | Sort-Object
$entries = foreach ($id in $sorted) {
  $module = "platform"
  if ($id -like "wallet*" -or $id -like "get_wallet*") { $module = "wallet" }
  elseif ($id -like "*leaderboard*" -or $id -like "submit_score*") { $module = "leaderboard" }
  elseif ($id -like "hiro_*") { $module = "hiro" }
  elseif ($id -like "satori_*") { $module = "satori" }
  elseif ($id -like "mp_*") { $module = "multiplayer" }
  elseif ($id -like "create_or_sync*" -or $id -like "*identity*") { $module = "identity" }
  elseif ($id -like "ivx_quest*" -or $id -like "daily_*" -or $id -like "fortune_*" -or $id -like "league_*" -or $id -like "push_*") { $module = "liveops" }
  elseif ($id -like "*group*") { $module = "social" }
  [pscustomobject]@{ id = $id; module = $module; authRequired = $true }
}

$doc = [pscustomobject]@{
  generatedAt = (Get-Date).ToUniversalTime().ToString("o")
  generator = "tools/generate-rpc-index.ps1"
  count = @($entries).Count
  rpcs = @($entries)
}

$outDir = Join-Path $root "Editor\RpcIndex"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$out = Join-Path $outDir "IVXRpcIndex.generated.json"
$doc | ConvertTo-Json -Depth 6 | Set-Content $out -Encoding utf8
Write-Host "Wrote $($doc.count) RPCs -> $out"
