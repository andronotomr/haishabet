# Smoke test end-to-end da API Haishabet.
# Pré-requisito: backend rodando em $base. Sobe com:
#   cd backend ; dotnet run --project src/Haishabet.Api
# Uso: .\scripts\api-smoke.ps1                # default localhost:5221
#      .\scripts\api-smoke.ps1 -Base 'http://localhost:5000'
param(
    [string]$Base = 'http://localhost:5221'
)

$ErrorActionPreference = 'Stop'

# 1) /health
$h = Invoke-WebRequest -Uri "$Base/health" -UseBasicParsing -TimeoutSec 3
Write-Host "/health -> $($h.Content)" -ForegroundColor Green

# 2) Register
Write-Host ""
Write-Host "=== Register ===" -ForegroundColor Cyan
$emailRand = "demo+$(Get-Random -Maximum 99999)@haishabet.test"
$reg = @{
    email     = $emailRand
    password  = 'senha12345'
    cpf       = '52998224725'
    fullName  = 'Demo User'
    birthDate = '1990-05-10'
} | ConvertTo-Json
$rsp = Invoke-RestMethod -Uri "$Base/api/auth/register" -Method Post -ContentType 'application/json' -Body $reg
Write-Host "User: $($rsp.user.email) (id=$($rsp.user.id))"
$token = $rsp.accessToken
$auth = @{ Authorization = "Bearer $token" }

# 3) Deposit R$ 100
Write-Host ""
Write-Host "=== Deposit R$ 100 (manual) ===" -ForegroundColor Cyan
$bal = Invoke-RestMethod -Uri "$Base/api/wallet/deposit/manual" -Method Post -Headers $auth -ContentType 'application/json' -Body '{"amountCents":10000}'
Write-Host "Saldo apos deposito: R$ $([math]::Round($bal.balanceRealCents/100, 2))"

# 4) Catalog
Write-Host ""
Write-Host "=== Catalog ===" -ForegroundColor Cyan
$games = Invoke-RestMethod -Uri "$Base/api/games"
foreach ($g in $games) {
    Write-Host ("- {0} ({1}) RTP={2:P0} bet R${3}-{4}" -f $g.name, $g.slug, $g.rtp, ($g.minBetCents/100), ($g.maxBetCents/100))
}

# 5) Spin x N
$N = 20
$BetCents = 500
Write-Host ""
Write-Host "=== Spin tigre-da-fortuna x$N (R$$($BetCents/100) cada) ===" -ForegroundColor Cyan
$totalBet = 0; $totalPayout = 0
for ($i = 1; $i -le $N; $i++) {
    $body = @{ gameSlug = 'tigre-da-fortuna'; betCents = $BetCents } | ConvertTo-Json
    try {
        $spin = Invoke-RestMethod -Uri "$Base/api/games/spin" -Method Post -Headers $auth -ContentType 'application/json' -Body $body
        $totalBet += $BetCents
        $totalPayout += $spin.payoutCents
        $marker = if ($spin.payoutCents -gt 0) { '*' } else { ' ' }
        Write-Host ("{0} #{1,2} {2,-4} pay={3,5}c saldo={4,6}c [{5}|{6}|{7}]" -f $marker, $i, $spin.result, $spin.payoutCents, $spin.balanceAfterCents, ($spin.grid[0] -join ''), ($spin.grid[1] -join ''), ($spin.grid[2] -join ''))
    } catch {
        Write-Host "Spin $i falhou: $($_.Exception.Message)" -ForegroundColor Red
        break
    }
}

# 6) Saldo final
Write-Host ""
$final = Invoke-RestMethod -Uri "$Base/api/wallet/balance" -Headers $auth
$rtp = if ($totalBet -gt 0) { $totalPayout / $totalBet } else { 0 }
Write-Host "=== Resumo ===" -ForegroundColor Cyan
Write-Host "Saldo final:      R$ $([math]::Round($final.balanceRealCents/100, 2))"
Write-Host "Total apostado:   R$ $([math]::Round($totalBet/100, 2))"
Write-Host "Total ganho:      R$ $([math]::Round($totalPayout/100, 2))"
Write-Host ("RTP nessa sessao: {0:P2}  (alvo: 96%, com {1} giros a variancia eh enorme)" -f $rtp, $N)
