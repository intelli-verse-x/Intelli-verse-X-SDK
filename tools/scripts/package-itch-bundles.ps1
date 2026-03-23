# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.
#
# Build per-platform zip bundles for itch.io (and mirrors GitHub release layout).
# Run from repository root:  .\tools\scripts\package-itch-bundles.ps1

[CmdletBinding()]
param(
    [string]$Version = "5.2.0",
    [string]$OutputRoot = "dist/itch"
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    $here = $PSScriptRoot
    return (Resolve-Path (Join-Path $here "..\..")).Path
}

function Copy-TreeWithExcludes {
    param(
        [string]$Source,
        [string]$Destination,
        [string[]]$ExcludeDirNames
    )
    if (-not (Test-Path $Source)) {
        throw "Source path not found: $Source"
    }
    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    $xdArgs = @()
    foreach ($d in $ExcludeDirNames) {
        $xdArgs += @("/XD", $d)
    }
    $null = & robocopy $Source $Destination /E /NFL /NDL /NJH /NJS /nc /ns /np @xdArgs
    if ($LASTEXITCODE -ge 8) {
        throw "robocopy failed with exit code $LASTEXITCODE for $Source"
    }
}

function New-PlatformZip {
    param(
        [string]$ZipName,
        [string]$SourceRelative,
        [string[]]$ExcludeDirs = @()
    )

    $repo = Get-RepoRoot
    $src = Join-Path $repo $SourceRelative
    $outDir = Join-Path $repo (Join-Path $OutputRoot $Version)
    $temp = Join-Path $env:TEMP ("itch-pack-" + [Guid]::NewGuid().ToString("N"))
    $zipPath = Join-Path $outDir ("{0}-{1}.zip" -f $ZipName, $Version)

    try {
        Copy-TreeWithExcludes -Source $src -Destination $temp -ExcludeDirNames $ExcludeDirs
        New-Item -ItemType Directory -Force -Path $outDir | Out-Null
        if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
        Compress-Archive -Path (Join-Path $temp "*") -DestinationPath $zipPath -CompressionLevel Optimal
        Write-Host "OK  $zipPath"
    }
    finally {
        if (Test-Path $temp) { Remove-Item -Recurse -Force $temp }
    }
}

$repoRoot = Get-RepoRoot
Set-Location $repoRoot
Write-Host "Repo: $repoRoot"
Write-Host "Version: $Version"
Write-Host "Output: $(Join-Path $repoRoot (Join-Path $OutputRoot $Version))"
Write-Host ""

# Excludes: keep bundles small and avoid dev/cache artifacts.
$jsExcludes = @("node_modules", "dist", "coverage", ".git")
$cppExcludes = @("test_package/build", ".git")
$javaExcludes = @("build", ".gradle", ".idea", "out", ".git")

New-PlatformZip -ZipName "intelliversex-unity-sdk" -SourceRelative "Assets\Intelli-verse-X-SDK" -ExcludeDirs @("Tests~", "Samples~", ".git")
New-PlatformZip -ZipName "intelliversex-unreal-sdk" -SourceRelative "SDKs\unreal" -ExcludeDirs @(".git")
New-PlatformZip -ZipName "intelliversex-godot-sdk" -SourceRelative "SDKs\godot" -ExcludeDirs @(".git")
New-PlatformZip -ZipName "intelliversex-defold-sdk" -SourceRelative "SDKs\defold" -ExcludeDirs @(".git")
New-PlatformZip -ZipName "intelliversex-cocos2dx-sdk" -SourceRelative "SDKs\cocos2dx" -ExcludeDirs @(".git")
New-PlatformZip -ZipName "intelliversex-javascript-sdk" -SourceRelative "SDKs\javascript" -ExcludeDirs $jsExcludes
New-PlatformZip -ZipName "intelliversex-cpp-sdk" -SourceRelative "SDKs\cpp" -ExcludeDirs $cppExcludes
New-PlatformZip -ZipName "intelliversex-java-sdk" -SourceRelative "SDKs\java" -ExcludeDirs $javaExcludes

Write-Host ""
Write-Host "Done. Upload zips from: $(Join-Path $repoRoot (Join-Path $OutputRoot $Version))"
