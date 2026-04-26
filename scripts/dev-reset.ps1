# CUIDADO: derruba containers E APAGA OS VOLUMES (dados de Postgres/Redis/Mongo).
# Útil para recomeçar do zero em desenvolvimento.
# Uso: .\scripts\dev-reset.ps1 -Confirm
param(
    [switch]$Confirm
)

if (-not $Confirm) {
    Write-Host "Esse comando APAGA todos os dados locais (Postgres/Redis/Mongo)." -ForegroundColor Yellow
    Write-Host "Se tem certeza, rode novamente com -Confirm" -ForegroundColor Yellow
    exit 1
}

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    docker compose -f docker-compose.dev.yml down -v
    Write-Host "Volumes apagados. Rode .\scripts\dev-up.ps1 para subir do zero." -ForegroundColor Green
} finally {
    Pop-Location
}
