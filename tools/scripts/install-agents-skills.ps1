# Copies .agents/skills from this repository into the user-level agents skills folder for
# clients that only scan ~/.agents/skills/ (not the project tree).
# Run from repository root: ./tools/scripts/install-agents-skills.ps1

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$Source = Join-Path $RepoRoot ".agents\skills"
$DestRoot = Join-Path $env:USERPROFILE ".agents\skills"

if (-not (Test-Path $Source)) {
    Write-Error "Missing folder: $Source"
    exit 1
}

New-Item -ItemType Directory -Force -Path $DestRoot | Out-Null
Copy-Item -Path (Join-Path $Source "*") -Destination $DestRoot -Recurse -Force
Write-Host "Installed repo skills to: $DestRoot"
Write-Host "Source: $Source"
