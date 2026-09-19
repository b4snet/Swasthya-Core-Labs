# Swasthya Core Labs — deterministic local quality gate.
# Runs: restore (with NuGet audit) -> format verify -> build (warn-as-error)
#       -> test -> vulnerability listing -> secret scan -> git diff --check
#
# Usage: powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) { $dotnet = 'dotnet' }

Write-Host '==> Restore (NuGet audit enabled)' -ForegroundColor Cyan
& $dotnet restore Swasthya.CoreLabs.slnx --nologo
if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }

Write-Host '==> Format verification' -ForegroundColor Cyan
& $dotnet format Swasthya.CoreLabs.slnx --verify-no-changes --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Format verification failed' }

Write-Host '==> Build (warnings as errors)' -ForegroundColor Cyan
& $dotnet build Swasthya.CoreLabs.slnx -c Release --no-restore -warnaserror --nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

# Tests: full suite when a PostgreSQL target is available (env var or scratch
# cluster), otherwise run only the non-DB suite. A scratch cluster started here
# is stopped again at the end of this script.
$managedScratch = $false
$testFilterArgs = @()
if (-not $env:SCL_PG_TEST_CONNECTION) {
    $scratchConnection = Join-Path $env:TEMP 'scl-pg-test\connection.txt'
    if (Test-Path -LiteralPath $scratchConnection) {
        $env:SCL_PG_TEST_CONNECTION = Get-Content -LiteralPath $scratchConnection -Raw
    }
    elseif (Test-Path -LiteralPath 'C:\Program Files\PostgreSQL\17\bin\initdb.exe') {
        & "$PSScriptRoot\pg-test.ps1" -Action start
        if ($LASTEXITCODE -ne 0) { throw 'Scratch PostgreSQL start failed' }
        $managedScratch = $true
        $env:SCL_PG_TEST_CONNECTION = Get-Content -LiteralPath $scratchConnection -Raw
    }
}
if (-not $env:SCL_PG_TEST_CONNECTION) {
    Write-Host '==> PostgreSQL unavailable; running non-DB suite only' -ForegroundColor Yellow
    $testFilterArgs = @('--filter', 'Category!=Db')
}

Write-Host '==> Tests' -ForegroundColor Cyan
& $dotnet test Swasthya.CoreLabs.slnx -c Release --no-build --nologo @testFilterArgs
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }

if ($managedScratch) {
    Write-Host '==> Stopping scratch PostgreSQL' -ForegroundColor Cyan
    & "$PSScriptRoot\pg-test.ps1" -Action stop
    if ($LASTEXITCODE -ne 0) { throw 'Scratch PostgreSQL stop failed' }
}

Write-Host '==> Vulnerability scan (top-level and transitive)' -ForegroundColor Cyan
& $dotnet list Swasthya.CoreLabs.slnx package --vulnerable --include-transitive
if ($LASTEXITCODE -ne 0) { throw 'Vulnerability scan failed' }

Write-Host '==> Secret scan' -ForegroundColor Cyan
& "$PSScriptRoot\scan-secrets.ps1"
if ($LASTEXITCODE -ne 0) { throw 'Secret scan failed' }

Write-Host '==> git diff --check' -ForegroundColor Cyan
& git diff --check
if ($LASTEXITCODE -ne 0) { throw 'git diff --check failed' }

Write-Host 'ALL QUALITY GATES PASSED' -ForegroundColor Green