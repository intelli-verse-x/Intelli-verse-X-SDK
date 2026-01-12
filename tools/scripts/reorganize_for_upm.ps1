<#
.SYNOPSIS
    Reorganizes IntelliVerseX SDK to clean UPM package structure.

.DESCRIPTION
    This script copies the SDK from the current Unity project structure
    to a clean UPM package structure suitable for distribution.

.PARAMETER OutputDir
    Directory to create the clean package (default: ..\intelliversex-sdk-package)

.PARAMETER DryRun
    Show what would be done without making changes

.EXAMPLE
    .\reorganize_for_upm.ps1
    
.EXAMPLE
    .\reorganize_for_upm.ps1 -OutputDir "C:\Packages\intelliversex-sdk" -DryRun
#>

param(
    [string]$OutputDir = "..\intelliversex-sdk-package",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

# Configuration
$SourceRoot = $PSScriptRoot | Split-Path -Parent | Split-Path -Parent
$SDKSource = Join-Path $SourceRoot "Assets\_IntelliVerseXSDK"

# Resolve output path
if (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $SourceRoot $OutputDir
}
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)

# Runtime modules to copy
$RuntimeFolders = @(
    "Core",
    "Identity",
    "Backend",
    "Monetization",
    "Analytics",
    "Localization",
    "Storage",
    "Networking",
    "Leaderboard",
    "Social",
    "Quiz",
    "QuizUI",
    "UI",
    "V2",
    "IAP",
    "IntroScene"
)

# Root files to copy
$RootFiles = @(
    "package.json",
    "README.md",
    "CHANGELOG.md",
    "LICENSE",
    "INSTALLATION.md"
)

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "HH:mm:ss"
    $prefix = if ($DryRun) { "[DRY-RUN] " } else { "" }
    Write-Host "[$timestamp] $prefix$Level: $Message"
}

function Copy-ItemSafe {
    param(
        [string]$Source,
        [string]$Destination,
        [switch]$Recurse,
        [string[]]$Exclude = @()
    )
    
    if (-not (Test-Path $Source)) {
        Write-Log "Source not found: $Source" "WARNING"
        return
    }
    
    # Create destination directory
    $destDir = if ($Recurse) { $Destination } else { Split-Path $Destination -Parent }
    if (-not (Test-Path $destDir)) {
        if (-not $DryRun) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }
        Write-Log "Created directory: $destDir"
    }
    
    if ($Recurse) {
        $srcName = Split-Path $Source -Leaf
        Write-Log "Copying directory: $srcName -> $Destination"
        
        if (-not $DryRun) {
            # Remove existing destination if exists
            if (Test-Path $Destination) {
                Remove-Item $Destination -Recurse -Force
            }
            
            # Copy with exclusions
            $items = Get-ChildItem $Source -Recurse | Where-Object {
                $relativePath = $_.FullName.Substring($Source.Length + 1)
                $excluded = $false
                foreach ($pattern in $Exclude) {
                    if ($_.Name -like $pattern) {
                        $excluded = $true
                        break
                    }
                }
                -not $excluded
            }
            
            # Create directory structure and copy files
            Copy-Item $Source $Destination -Recurse -Force
            
            # Remove excluded files
            foreach ($pattern in $Exclude) {
                Get-ChildItem $Destination -Recurse -Filter $pattern | Remove-Item -Force
            }
        }
    }
    else {
        $fileName = Split-Path $Source -Leaf
        Write-Log "Copying file: $fileName -> $Destination"
        
        if (-not $DryRun) {
            Copy-Item $Source $Destination -Force
        }
    }
}

# Main execution
Write-Host ""
Write-Host "=" * 60
Write-Host "IntelliVerseX SDK - UPM Package Reorganization"
Write-Host "=" * 60
Write-Host ""

# Validate source
if (-not (Test-Path $SDKSource)) {
    Write-Log "SDK source not found: $SDKSource" "ERROR"
    exit 1
}

$packageJson = Join-Path $SDKSource "package.json"
if (-not (Test-Path $packageJson)) {
    Write-Log "package.json not found: $packageJson" "ERROR"
    exit 1
}

Write-Log "Source: $SDKSource"
Write-Log "Output: $OutputDir"

# Create output directory
if (-not (Test-Path $OutputDir)) {
    if (-not $DryRun) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }
    Write-Log "Created output directory"
}

# Copy root files
Write-Host ""
Write-Log "--- Copying root files ---"
foreach ($file in $RootFiles) {
    $src = Join-Path $SDKSource $file
    $dst = Join-Path $OutputDir $file
    Copy-ItemSafe -Source $src -Destination $dst
}

# Copy Runtime folders
Write-Host ""
Write-Log "--- Copying Runtime modules ---"
$runtimeDir = Join-Path $OutputDir "Runtime"

foreach ($folder in $RuntimeFolders) {
    $src = Join-Path $SDKSource $folder
    $dst = Join-Path $runtimeDir $folder
    if (Test-Path $src) {
        Copy-ItemSafe -Source $src -Destination $dst -Recurse -Exclude @("*.meta")
    }
}

# Copy Editor folder
Write-Host ""
Write-Log "--- Copying Editor module ---"
$editorSrc = Join-Path $SDKSource "Editor"
$editorDst = Join-Path $OutputDir "Editor"
if (Test-Path $editorSrc) {
    Copy-ItemSafe -Source $editorSrc -Destination $editorDst -Recurse -Exclude @("*.meta")
}

# Copy Samples~
Write-Host ""
Write-Log "--- Copying Samples ---"
$samplesSrc = Join-Path $SDKSource "Samples~"
$samplesDst = Join-Path $OutputDir "Samples~"
if (Test-Path $samplesSrc) {
    Copy-ItemSafe -Source $samplesSrc -Destination $samplesDst -Recurse
}

# Copy Tests~
Write-Host ""
Write-Log "--- Copying Tests ---"
$testsSrc = Join-Path $SDKSource "Tests~"
$testsDst = Join-Path $OutputDir "Tests~"
if (Test-Path $testsSrc) {
    Copy-ItemSafe -Source $testsSrc -Destination $testsDst -Recurse
}

# Copy Documentation
Write-Host ""
Write-Log "--- Copying Documentation ---"
$docsSrc = Join-Path $SDKSource "Documentation"
$docsDst = Join-Path $OutputDir "Documentation~"
if (Test-Path $docsSrc) {
    Copy-ItemSafe -Source $docsSrc -Destination $docsDst -Recurse -Exclude @("*.meta")
}

# Copy Icons
Write-Host ""
Write-Log "--- Copying Icons ---"
$iconsSrc = Join-Path $SDKSource "Icons"
$iconsDst = Join-Path $OutputDir "Icons"
if (Test-Path $iconsSrc) {
    Copy-ItemSafe -Source $iconsSrc -Destination $iconsDst -Recurse -Exclude @("*.meta")
}

# Summary
Write-Host ""
Write-Host "=" * 60
Write-Log "Reorganization complete!"
Write-Host "=" * 60
Write-Host ""

if ($DryRun) {
    Write-Host "This was a dry run. No files were modified."
    Write-Host "Run without -DryRun to perform the actual reorganization."
}
else {
    Write-Host "✅ Success! Clean UPM package created at:"
    Write-Host "   $OutputDir"
    Write-Host ""
    Write-Host "Next steps:"
    Write-Host "1. Review the generated package structure"
    Write-Host "2. Test installation in a fresh Unity project"
    Write-Host "3. Push to a new Git repository for distribution"
}
