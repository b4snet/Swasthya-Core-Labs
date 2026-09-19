param(
    [ValidateSet('start', 'stop')]
    [string]$Action = 'start'
)

$ErrorActionPreference = 'Stop'

$PgBin = 'C:\Program Files\PostgreSQL\17\bin'
$PgCtl = Join-Path $PgBin 'pg_ctl.exe'
$InitDb = Join-Path $PgBin 'initdb.exe'

$StateDir = Join-Path $env:TEMP 'scl-pg-test'
$DataDir = Join-Path $StateDir 'data'
$Port = 55432
$LogFile = Join-Path $StateDir 'postgres.log'
$PasswordFile = Join-Path $StateDir 'password.txt'
$ConnectionFile = Join-Path $StateDir 'connection.txt'

if ($Action -eq 'stop') {
    if (Test-Path -LiteralPath $DataDir) {
        & $PgCtl -D $DataDir stop -m fast -w 2>&1 | Out-Null
    }
    if (Test-Path -LiteralPath $StateDir) {
        Remove-Item -LiteralPath $StateDir -Recurse -Force
    }
    Write-Output 'Scratch PostgreSQL cluster stopped and removed.'
    exit 0
}

if (-not (Test-Path -LiteralPath $InitDb)) {
    throw "initdb.exe not found at $InitDb"
}

New-Item -ItemType Directory -Path $StateDir -Force | Out-Null

if (-not (Test-Path -LiteralPath $DataDir)) {
    $password = [Guid]::NewGuid().ToString('N')
    Set-Content -LiteralPath $PasswordFile -Value $password -NoNewline

    & $InitDb -D $DataDir -U postgres --auth=scram-sha-256 --pwfile=$PasswordFile --encoding=UTF8 --locale=C 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'initdb failed'
    }
}

# Start pg_ctl through a detached hidden PowerShell so the postmaster does not
# hold this shell's stdio handles (which would block callers waiting on them).
$inner = "& '$PgCtl' -D '$DataDir' -o '-p $Port -c listen_addresses=127.0.0.1 -c logging_collector=off' -l '$LogFile' start -w"
$stdout = Join-Path $StateDir 'pg_ctl.stdout.txt'
$stderr = Join-Path $StateDir 'pg_ctl.stderr.txt'
Start-Process -FilePath 'powershell.exe' `
    -ArgumentList @('-NoProfile', '-WindowStyle', 'Hidden', '-Command', $inner) `
    -WindowStyle Hidden `
    -RedirectStandardOutput $stdout `
    -RedirectStandardError $stderr `
    | Out-Null

$ready = $false
for ($i = 0; $i -lt 120; $i++) {
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $async = $client.BeginConnect('127.0.0.1', $Port, $null, $null)
        if ($async.AsyncWaitHandle.WaitOne(400) -and $client.Connected) {
            $ready = $true
            break
        }
    }
    catch {
    }
    finally {
        $client.Close()
    }
    Start-Sleep -Milliseconds 500
}

if (-not $ready) {
    if (Test-Path -LiteralPath $LogFile) {
        Get-Content -LiteralPath $LogFile -Tail 30
    }
    throw 'PostgreSQL server did not become ready on port 55432'
}

$password = Get-Content -LiteralPath $PasswordFile -Raw
$connection = "Host=127.0.0.1;Port=$Port;Database=postgres;Username=postgres;Password=$password;SSL Mode=Disable"

Set-Content -LiteralPath $ConnectionFile -Value $connection -NoNewline
Write-Output "Scratch PostgreSQL running on 127.0.0.1:$Port"
Write-Output "Connection file: $ConnectionFile"