# Setup do ambiente de desenvolvimento

Ambiente alvo: **Windows 10/11** (host) + **Docker Desktop com WSL2** + ferramentas nativas para mobile.

> Você está no Windows. Comandos abaixo assumem PowerShell. Para Flutter Android, recomendado direto no Windows (não WSL).

---

## 1. Pré-requisitos

| Ferramenta | Versão | Como instalar |
|---|---|---|
| Git | 2.40+ | git-scm.com |
| Docker Desktop | última | docker.com — habilitar WSL2 backend |
| **.NET SDK** | **8.0 LTS** | dotnet.microsoft.com/download (ou `winget install Microsoft.DotNet.SDK.8`) |
| Node.js | 20 LTS | nodejs.org (apenas para a web admin) |
| pnpm | 9+ | `npm i -g pnpm` |
| Flutter SDK | 3.24+ | docs.flutter.dev/get-started/install/windows |
| Android Studio | última | developer.android.com (SDK Android + emulador) |
| JDK | 17 (Temurin) | adoptium.net |
| IDE backend | Rider, Visual Studio 2022, ou VS Code | + extensão **C# Dev Kit** (VS Code) |
| IDE web/mobile | VS Code/Cursor | + extensões Dart, Flutter, ESLint, Tailwind, Docker |

Validar:
```powershell
git --version
docker --version
dotnet --version    # deve mostrar 8.0.x
node -v ; pnpm -v
flutter doctor      # tudo verde p/ Android toolchain
```

---

## 2. Estrutura

```powershell
cd C:\El\haishabet
ls
# backend  doc  mobile  web
```

---

## 3. Subir bancos locais (Docker)

Criar `haishabet/docker-compose.dev.yml` na raiz (Fase 0):

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: haishabet
      POSTGRES_PASSWORD: dev_change_me
      POSTGRES_DB: haishabet
    ports: ["5432:5432"]
    volumes: ["pgdata:/var/lib/postgresql/data"]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]
    command: ["redis-server","--appendonly","yes"]
    volumes: ["redisdata:/data"]

  mongo:
    image: mongo:7
    environment:
      MONGO_INITDB_ROOT_USERNAME: haishabet
      MONGO_INITDB_ROOT_PASSWORD: dev_change_me
    ports: ["27017:27017"]
    volumes: ["mongodata:/data/db"]

volumes:
  pgdata: {}
  redisdata: {}
  mongodata: {}
```

Subir:
```powershell
cd C:\El\haishabet
docker compose -f docker-compose.dev.yml up -d
docker ps   # 3 containers up
```

---

## 4. Backend (.NET 8 / ASP.NET Core)

### 4.1 Scaffold inicial (rodar uma vez)

```powershell
cd C:\El\haishabet\backend
dotnet new sln -n Haishabet
mkdir src, tests

dotnet new webapi    -o src/Haishabet.Api          --use-controllers
dotnet new classlib  -o src/Haishabet.Application
dotnet new classlib  -o src/Haishabet.Domain
dotnet new classlib  -o src/Haishabet.Infrastructure
dotnet new classlib  -o src/Haishabet.GameEngine
dotnet new xunit     -o tests/Haishabet.UnitTests
dotnet new xunit     -o tests/Haishabet.IntegrationTests

# Adicionar tudo à solution
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }

# Referências entre projetos
dotnet add src/Haishabet.Api/Haishabet.Api.csproj reference `
  src/Haishabet.Application/Haishabet.Application.csproj `
  src/Haishabet.Infrastructure/Haishabet.Infrastructure.csproj
dotnet add src/Haishabet.Application/Haishabet.Application.csproj reference `
  src/Haishabet.Domain/Haishabet.Domain.csproj
dotnet add src/Haishabet.Infrastructure/Haishabet.Infrastructure.csproj reference `
  src/Haishabet.Domain/Haishabet.Domain.csproj `
  src/Haishabet.Application/Haishabet.Application.csproj
dotnet add src/Haishabet.GameEngine/Haishabet.GameEngine.csproj reference `
  src/Haishabet.Domain/Haishabet.Domain.csproj
```

### 4.2 Pacotes principais

```powershell
# API (camada de entrada)
cd C:\El\haishabet\backend\src\Haishabet.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.PostgreSql
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File

# Infraestrutura (DB + clients externos)
cd ..\Haishabet.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package MongoDB.Driver
dotnet add package StackExchange.Redis
dotnet add package Refit
dotnet add package Refit.HttpClientFactory
dotnet add package Polly

# Application (use cases)
cd ..\Haishabet.Application
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

# Testes de integração com containers reais
cd ..\..\tests\Haishabet.IntegrationTests
dotnet add package Testcontainers.PostgreSql
dotnet add package Testcontainers.Redis
dotnet add package Testcontainers.MongoDb
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

### 4.3 EF Core tools (uma vez no host)
```powershell
dotnet tool install --global dotnet-ef
```

### 4.4 Migration inicial (depois de criar o DbContext)
```powershell
cd C:\El\haishabet\backend
dotnet ef migrations add Init `
  --project src/Haishabet.Infrastructure `
  --startup-project src/Haishabet.Api
dotnet ef database update `
  --project src/Haishabet.Infrastructure `
  --startup-project src/Haishabet.Api
```

### 4.5 `appsettings.Development.json` mínimo
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=haishabet;Username=haishabet;Password=dev_change_me",
    "Redis": "localhost:6379",
    "Mongo": "mongodb://haishabet:dev_change_me@localhost:27017/haishabet?authSource=admin"
  },
  "Jwt": {
    "Issuer": "haishabet",
    "Audience": "haishabet-app",
    "AccessSecret": "dev_change_me_long_random",
    "RefreshSecret": "dev_change_me_other_long_random",
    "AccessMinutes": 15,
    "RefreshDays": 30
  },
  "Psp": {
    "Provider": "Efi",
    "Sandbox": true,
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

> Use `dotnet user-secrets` para credenciais locais — nunca comitar segredos.

### 4.6 Rodar
```powershell
cd C:\El\haishabet\backend
dotnet run --project src/Haishabet.Api
# sobe em http://localhost:5000 e https://localhost:5001
```

Hot reload:
```powershell
dotnet watch --project src/Haishabet.Api run
```

---

## 5. Web admin (Next.js)

```powershell
cd C:\El\haishabet\web
pnpm create next-app@latest . --ts --tailwind --eslint --app --src-dir --import-alias "@/*"
pnpm add zustand swr @tanstack/react-table recharts zod react-hook-form @hookform/resolvers
pnpm add @microsoft/signalr   # cliente SignalR para tempo real
pnpm dlx shadcn@latest init
pnpm dev
```

---

## 6. Mobile (Flutter)

```powershell
cd C:\El\haishabet\mobile
flutter create . --org com.haishabet --project-name haishabet_app --platforms=android,ios
flutter pub add flutter_riverpod go_router dio freezed_annotation json_annotation
flutter pub add --dev build_runner freezed json_serializable
flutter pub add flutter_secure_storage local_auth firebase_core firebase_messaging
flutter pub add signalr_netcore   # cliente SignalR para tempo real
flutter run
```

---

## 7. Workflow diário

| Terminal | Comando |
|---|---|
| 1 | `docker compose -f docker-compose.dev.yml up` |
| 2 (`backend/`) | `dotnet watch --project src/Haishabet.Api run` |
| 3 (`web/`) | `pnpm dev` |
| 4 (`mobile/`) | `flutter run` |

---

## 8. Quando subir para a VPS (Fase 5)

Detalhes completos em [hospedagem.md](hospedagem.md). Resumo:

- **Dev/staging:** Hetzner CX22/CX32 (€5–10/mês) ou Contabo VPS S ($7/mês). Latência BR ~150–200ms é aceitável para staging.
- **Produção (após licença SPA/MF):** **obrigatório no Brasil** — Magalu Cloud, AWS sa-east-1, GCP southamerica-east1, Azure brazilsouth, Locaweb, Hostinger Cloud BR.

Setup VPS resumido:
- Ubuntu 24.04 LTS, usuário não-root, ssh por chave, ufw fechado exceto 22/80/443.
- Docker + Compose plugin.
- Container do backend .NET (multi-stage build com `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`).
- Nginx reverse proxy + Certbot Let's Encrypt para HTTPS.
- Postgres/Redis/Mongo containerizados com volumes + backup diário em S3-compat (Backblaze B2/Wasabi).
- CI/CD GitHub Actions: build → docker push → ssh → `docker compose pull && up -d`.

---

## 9. Comandos úteis

```powershell
# Testes backend (todos)
cd C:\El\haishabet\backend ; dotnet test

# Migration nova
cd C:\El\haishabet\backend ; dotnet ef migrations add NomeDaAlteracao --project src/Haishabet.Infrastructure --startup-project src/Haishabet.Api

# Reverter última migration
cd C:\El\haishabet\backend ; dotnet ef migrations remove --project src/Haishabet.Infrastructure --startup-project src/Haishabet.Api

# Reset DB local (CUIDADO — apaga tudo)
cd C:\El\haishabet\backend ; dotnet ef database drop --project src/Haishabet.Infrastructure --startup-project src/Haishabet.Api

# Build Flutter Android (APK debug)
cd C:\El\haishabet\mobile ; flutter build apk --debug

# Build Flutter Android (release, requer keystore)
cd C:\El\haishabet\mobile ; flutter build apk --release

# Logs container
docker logs -f haishabet-postgres-1
```
