# Build ZIP for Godot Asset Library (Custom download).
# Run from repo root: .\SDKs\godot\build_asset_library_zip.ps1
# Or from SDKs/godot: ..\..\SDKs\godot\build_asset_library_zip.ps1 (adjust paths below).
#
# Output: IntelliVerseX-Godot-5.1.0.zip with one root folder containing addons/, project.godot, etc.
# Upload this ZIP as a Release asset, then use the release download URL in Asset Library (Custom).

$ErrorActionPreference = "Stop"
$Version = "5.1.0"
$RootFolderName = "IntelliVerseX-Godot-$Version"
$ZipName = "$RootFolderName.zip"

# Script must be in SDKs/godot; we use its directory as the source.
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$GodotDir = $ScriptDir
if (-not (Test-Path (Join-Path $GodotDir "project.godot"))) {
    Write-Error "project.godot not found in $GodotDir. Run this script from the repo (e.g. .\SDKs\godot\build_asset_library_zip.ps1) or from SDKs\godot."
}
$OutDir = Join-Path ([System.IO.Path]::GetTempPath()) "IntelliVerseX-Godot-build"
$ZipRoot = Join-Path $OutDir $RootFolderName

if (Test-Path $OutDir) {
    Remove-Item -Recurse -Force $OutDir
}
New-Item -ItemType Directory -Path $ZipRoot | Out-Null

# Copy contents of SDKs/godot, excluding .godot and this script
$Exclude = @(".godot", "build_asset_library_zip.ps1", "*.zip")
Get-ChildItem -Path $GodotDir -Force | Where-Object {
    $name = $_.Name
    $skip = $false
    foreach ($e in $Exclude) {
        if ($e -like "*.*") { if ($name -like $e) { $skip = $true; break } }
        elseif ($name -eq $e) { $skip = $true; break }
    }
    -not $skip
} | ForEach-Object {
    $dest = Join-Path $ZipRoot $_.Name
    if ($_.PSIsContainer) {
        Copy-Item -Path $_.FullName -Destination $dest -Recurse -Force
    } else {
        Copy-Item -Path $_.FullName -Destination $dest -Force
    }
}

$ZipPath = Join-Path (Split-Path $ZipRoot -Parent) $ZipName
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
Compress-Archive -Path $ZipRoot -DestinationPath $ZipPath -CompressionLevel Optimal

# Copy ZIP to Godot folder so it's easy to find and upload
$ZipInGodot = Join-Path $GodotDir $ZipName
Copy-Item -Path $ZipPath -Destination $ZipInGodot -Force
Remove-Item -Recurse -Force $OutDir -ErrorAction SilentlyContinue

Write-Host "Created: $ZipInGodot"
Write-Host "(Also in temp: $ZipPath)"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Create a GitHub Release (e.g. tag godot-v$Version) at https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/releases/new"
Write-Host "2. Attach this ZIP as a release asset: $ZipName"
Write-Host "3. Copy the asset download URL (right-click the asset -> Copy link)."
Write-Host "   It will look like: https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/releases/download/godot-v$Version/$ZipName"
Write-Host "4. In Asset Library submit form, choose Repository host: Custom and paste that URL in the Download field."
