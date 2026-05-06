# QualityWaterAlert

Monitor drinking water quality in France — a Blazor Server web application that fetches official government data, checks regulatory compliance, and sends email alerts when quality degrades.

[![CI](https://github.com/ClementG/QualityWaterAlert/actions/workflows/ci.yml/badge.svg)](https://github.com/ClementG/QualityWaterAlert/actions/workflows/ci.yml)
[![Docker Build](https://github.com/ClementG/QualityWaterAlert/actions/workflows/docker.yml/badge.svg)](https://github.com/ClementG/QualityWaterAlert/actions/workflows/docker.yml)

## Features

- **Commune search** — by name or postal code; covers all ~35 000 French communes
- **Compliance dashboard** — measured values compared to regulatory limits, conformity donut chart, stat cards, sampling history
- **Efficient data fetching** — HTTP Range requests on the remote government ZIP; downloads only the relevant department files (~2–10 MB vs 274 MB full archive), cached 24 h
- **Email alerts** — subscribe per commune; notified when a non-conformity is detected

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Web UI | Blazor Server (.NET 10) + MudBlazor 8 |
| Charts | Chart.js 4 (CDN) |
| Business logic | C# 13, Clean Architecture |
| Tests | NUnit 4 — 130 tests, 100 % passing |
| Container | Docker (multi-stage) + docker-compose |
| Data source | data.gouv.fr open data API |

## Architecture

Clean Architecture with three active layers; dependencies flow inward only.

```
Core  ←  Infrastructure  ←  WebApp
```

- **`Core`** — models (`Commune`, `WaterNetwork`, `SamplingEvent`, `WaterQualityParameter`, `WaterQualityAnalysis`) and services (`ComplianceChecker`, `CommuneSearcher`). Zero external dependencies.
- **`Infrastructure`** — `DataGouvFrProvider` (HTTP Range requests on the government ZIP), `EmailService` (SMTP). Implements interfaces defined in Core.
- **`WebApp`** — Blazor Server pages and components. Consumes Core and Infrastructure via dependency injection.
- **`App`** — .NET MAUI shell, scaffolded for post-MVP mobile support.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker (optional, for containerized deployment)

### Run locally

```bash
git clone https://github.com/ClementG/QualityWaterAlert.git
cd QualityWaterAlert
dotnet restore
dotnet run --project src/QualityWaterAlert.WebApp
```

Open `http://localhost:5295` in your browser (HTTPS: `https://localhost:7099`).

### Run with Docker

```bash
cp .env.example .env   # optional — configure SMTP and Cloudflare Tunnel
docker-compose up --build
```

App available at `http://localhost:8080`.

### Run tests

```bash
dotnet test                                               # all 130 tests
dotnet test tests/QualityWaterAlert.Core.Tests/           # Core (108 tests)
dotnet test tests/QualityWaterAlert.Infrastructure.Tests/ # Infrastructure (22 tests)

# Filter by name
dotnet test --filter "FullyQualifiedName~ComplianceCheckerTests"
```

## Configuration

Copy `.env.example` to `.env` and fill in the values you need.

| Variable | Description |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` or `Development` |
| `SMTP_HOST` | SMTP server (e.g. `smtp.gmail.com`) |
| `SMTP_PORT` | 587 (STARTTLS, recommended), 465 (SSL), or 25 |
| `SMTP_FROM_EMAIL` | Sender address |
| `SMTP_FROM_NAME` | Display name in the "From" field |
| `SMTP_USERNAME` / `SMTP_PASSWORD` | SMTP credentials |
| `CLOUDFLARE_TUNNEL_TOKEN` | For Cloudflare Tunnel external access (Option A) |

SMTP variables are optional — the app runs without email alerts if they are not set.

## Deployment

The application is designed to run on a NAS (TerraMaster, Synology, QNAP) behind a reverse proxy.  
See **[docs/DEPLOYMENT.md](docs/DEPLOYMENT.md)** for the full step-by-step guide covering:

- Docker Compose setup on TOS 5.1+
- **Option A (recommended)**: Cloudflare Tunnel — HTTPS with no open ports
- **Option B**: Port forwarding + DuckDNS + nginx Proxy Manager + Let's Encrypt
- Update procedure and useful commands

## Project Structure

```
QualityWaterAlert/
├── src/
│   ├── QualityWaterAlert.Core/            # Models and business services
│   ├── QualityWaterAlert.Infrastructure/  # Data provider (data.gouv.fr) + SMTP
│   ├── QualityWaterAlert.WebApp/          # Blazor Server application
│   └── QualityWaterAlert.App/             # .NET MAUI shell (post-MVP)
├── tests/
│   ├── QualityWaterAlert.Core.Tests/
│   └── QualityWaterAlert.Infrastructure.Tests/
├── docs/
│   └── DEPLOYMENT.md
├── .github/
│   └── workflows/
│       ├── ci.yml                         # Build + test on every push/PR
│       └── docker.yml                     # Docker image validation on master
├── docker-compose.yml
├── .env.example
├── CONTRIBUTING.md
└── README.md
```

## Data Source

Water quality data is published by the French Ministry of Health on the government open data platform:

> [Résultats du contrôle sanitaire de l'eau distribuée commune par commune](https://www.data.gouv.fr/datasets/resultats-du-controle-sanitaire-de-leau-distribuee-commune-par-commune/)

Files used at runtime per department: `DIS_COM_UDI_YYYY.txt`, `DIS_PLV_YYYY_{dept}.txt`, `DIS_RESULT_YYYY_{dept}.txt`. Data is fetched on demand and cached in memory for 24 hours.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the GitFlow branching model, commit message conventions, and PR process.

## License

MIT License — see [LICENSE](LICENSE) for details.

## Author

Clement G — [github.com/ClementG](https://github.com/ClementG)
