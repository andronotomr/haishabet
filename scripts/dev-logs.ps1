# Mostra logs dos containers de dev em tempo real.
# Uso: .\scripts\dev-logs.ps1            (todos os serviços)
#      .\scripts\dev-logs.ps1 postgres   (apenas postgres)
param(
    [string]$Service = ''
)

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    if ([string]::IsNullOrWhiteSpace($Service)) {
        docker compose -f docker-compose.dev.yml logs -f --tail=100
    } else {
        docker compose -f docker-compose.dev.yml logs -f --tail=100 $Service
    }
} finally {
    Pop-Location
}
