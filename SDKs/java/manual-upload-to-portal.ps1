# Run this from SDKs/java/ after ./gradlew publish (same machine/IP).
# Reads ossrhStagingApiUsername and ossrhStagingApiPassword from gradle.properties
# and calls the OSSRH Staging API to send the deployment to the Central Portal.

$ErrorActionPreference = 'Stop'
$propsFile = Join-Path $PSScriptRoot 'gradle.properties'
if (-not (Test-Path $propsFile)) {
    Write-Error "gradle.properties not found in $PSScriptRoot"
    exit 1
}

$content = Get-Content $propsFile -Raw
$user = $null
$pass = $null
foreach ($line in (Get-Content $propsFile)) {
    $line = $line.Trim()
    if ($line.StartsWith('ossrhStagingApiUsername=')) {
        $user = ($line -split '=', 2)[1].Trim()
    }
    if ($line.StartsWith('ossrhStagingApiPassword=')) {
        $pass = ($line -split '=', 2)[1].Trim()
    }
}

if (-not $user -or -not $pass) {
    Write-Error "ossrhStagingApiUsername and/or ossrhStagingApiPassword not set in gradle.properties"
    exit 1
}

$pair = "${user}:${pass}"
$b64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($pair))
$uri = 'https://ossrh-staging-api.central.sonatype.com/manual/upload/defaultRepository/ai.intelli-verse-x'

Write-Host "Calling manual upload for namespace ai.intelli-verse-x..."
try {
    Invoke-RestMethod -Uri $uri -Method Post -Headers @{ Authorization = "Bearer $b64" }
    Write-Host "Done. Check https://central.sonatype.com/publishing/deployments and click Publish."
} catch {
    Write-Error $_.Exception.Message
    exit 1
}
