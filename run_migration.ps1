param([string]$ConnectionString = "Host=127.0.0.1;Port=55432;Database=scl;Username=scl_user;Password=scl_pass")

Write-Host "Setting connection string: $ConnectionString"
$env:SCL_DB_CONNECTION = $ConnectionString

Write-Host "Running migration..."
Set-Location "src\Swasthya.CoreLabs.Infrastructure"
$result = dotnet ef migrations add AddResultEntity --context CoreLabDbContext 2>&1
Write-Host $result