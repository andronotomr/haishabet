# Para os containers de desenvolvimento (volumes preservados).
# Uso: .\scripts\dev-down.ps1
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    docker compose -f docker-compose.dev.yml down
} finally {
    Pop-Location
}
