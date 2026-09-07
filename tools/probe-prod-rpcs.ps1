# Probe Unity SDK RPC index against production Nakama (via nakama-mcp)
# Usage (from repo root):
#   powershell -File tools/probe-prod-rpcs.ps1
#   powershell -File tools/probe-prod-rpcs.ps1 -OutFile reports/rpc-prod-probe.json

param(
    [string]$McpUrl = "https://nakama-mcp.intelli-verse-x.ai/",
    [string]$IndexPath = "",
    [string]$OutFile = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if (-not $IndexPath) {
    $IndexPath = Join-Path $root "Packages\com.intelliversex.sdk\Editor\RpcIndex\IVXRpcIndex.generated.json"
}
if (-not $OutFile) {
    $OutFile = Join-Path $env:TEMP "unity-rpc-prod-probe.json"
}

function Invoke-NakamaMcp([string]$tool, [hashtable]$arguments) {
    $bodyObj = @{
        jsonrpc = "2.0"
        id      = [guid]::NewGuid().ToString()
        method  = "tools/call"
        params  = @{ name = $tool; arguments = $arguments }
    }
    $body = $bodyObj | ConvertTo-Json -Depth 10 -Compress
    Invoke-RestMethod -Uri $McpUrl -Method POST -ContentType "application/json" -Body $body -TimeoutSec 60
}

Write-Host "Index: $IndexPath"
$unity = (Get-Content $IndexPath -Raw | ConvertFrom-Json).rpcs
$results = New-Object System.Collections.Generic.List[object]
$i = 0
foreach ($rpc in $unity) {
    $i++
    $id = $rpc.id
    try {
        $r = Invoke-NakamaMcp "nakama_rpc" @{ rpc_id = $id; payload = @{} }
        $text = if ($r.result.content) { $r.result.content[0].text } else { ($r | ConvertTo-Json -Compress) }
        $status = "ok"
        if ($text -match 'RPC function not found|code.:5') { $status = "not_found" }
        elseif ($text -match 'alias target unavailable|alias_target_unavailable') { $status = "broken_alias" }
        elseif ($text -match 'Error calling|401|Unauthorized|PermissionDenied|code.:16|code.:7|AUTH_REQUIRED|Not authenticated|unauthenticated|no_session') { $status = "auth_or_perm" }
        elseif ($text -match '"success"\s*:\s*false') { $status = "soft_fail" }
        elseif ($text -match '^Error calling') { $status = "error" }
        $results.Add([pscustomobject]@{
            id = $id; module = $rpc.module; status = $status
            snippet = $text.Substring(0, [Math]::Min(200, $text.Length)).Replace("`n", " ")
        })
    }
    catch {
        $msg = $_.Exception.Message
        $status = if ($msg -match '404|not found') { "not_found" } else { "exception" }
        $results.Add([pscustomobject]@{ id = $id; module = $rpc.module; status = $status; snippet = $msg })
    }
    if (($i % 25) -eq 0) { Write-Host "probed $i / $($unity.Count)" }
}

$results | Group-Object status | Sort-Object Count -Descending | Format-Table Name, Count -AutoSize
$results | ConvertTo-Json -Depth 4 | Set-Content $OutFile -Encoding utf8
Write-Host "Wrote $($results.Count) results -> $OutFile"
Write-Host "`nnot_found / broken_alias:"
$results | Where-Object { $_.status -in @('not_found', 'broken_alias') } | ForEach-Object { "$($_.status) $($_.id) :: $($_.snippet)" }
