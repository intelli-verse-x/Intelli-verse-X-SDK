param(
    [switch]$CI
)

$ErrorActionPreference = "Stop"

Write-Host "IntelliVerseX - Running context validation..."

if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
    Write-Error "Python is required. Install Python 3.11+ and ensure 'python' is on PATH."
    exit 1
}

$argsList = @("tools/context/validate_context.py")
if ($CI) { $argsList += "--ci" }

python @argsList
exit $LASTEXITCODE

