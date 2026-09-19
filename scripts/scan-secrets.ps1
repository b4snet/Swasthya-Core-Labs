# Swasthya Core Labs — secret/internal-sensitive-data scanner.
# Scans repository files for common secret patterns and flags findings.
# Best-effort: not a substitute for gitleaks or Git secret scanning.
# Usage: powershell -ExecutionPolicy Bypass -File .\scripts\scan-secrets.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

$patterns = @(
    'AKIA[0-9A-Z]{16}',
    '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----',
    '(?i)aws_secret_access_key\s*[=:]\s*["'']?[A-Za-z0-9/+]{40}',
    '(?i)(api[_-]?key|client[_-]?secret)\s*[=:]\s*["''][^"'']{16,}["'']',
    '(?i)password\s*[=:]\s*["''][^"'']{8,}["'']',
    '(?i)Server\s*=\s*[^;]+;\s*(Database|Initial Catalog)\s*=\s*[^;]+;\s*.*Password\s*=',
    '[A-Za-z0-9]+://[^:/\s]+:[^@\s]+@',
    'ghp_[A-Za-z0-9]{36,}',
    'github_pat_[A-Za-z0-9_]{22,}',
    'xox[baprs]-[A-Za-z0-9-]{10,}'
)

$excludedDirs = @('\.git$', '\\bin\\', '\\obj\\', '\\TestResults\\', '\\.vs\\', '\\.idea\\')
$findings = [System.Collections.Generic.List[object]]::new()

Get-ChildItem -Path $root -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object {
    $file = $_
    $rel = $file.FullName.Substring($root.Length).TrimStart('\')
    if ($rel -match '\\(bin|obj|TestResults|\.vs|\.idea)\\' -Or $rel.StartsWith('.git\')) { return }

    try {
        $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction Stop
    } catch { return }
    if (-not $content) { return }

    foreach ($pattern in $patterns) {
        $matches = [regex]::Matches($content, $pattern)
        if ($matches.Count -gt 0) {
            $findings.Add([pscustomobject]@{ File = $rel; Pattern = $pattern; Count = $matches.Count })
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Host 'SECRET SCAN: potential sensitive data found' -ForegroundColor Red
    $findings | Format-Table -AutoSize
    exit 1
}

Write-Host 'SECRET SCAN: clean' -ForegroundColor Green
exit 0