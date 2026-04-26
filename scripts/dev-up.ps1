# Sobe os containers de desenvolvimento (Postgres + Redis + Mongo).
# Uso: .\scripts\dev-up.ps1
param(
    [switch]$Detach = $true
)

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    if ($Detach) {
        docker compose -f docker-compose.dev.yml up -d
    } else {
        docker compose -f docker-compose.dev.yml up
    }
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host ""
    Write-Host "Containers de dev rodando:" -ForegroundColor Green
    docker compose -f docker-compose.dev.yml ps
} finally {
    Pop-Location
}
