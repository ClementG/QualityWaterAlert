# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run a specific test project
dotnet test tests/QualityWaterAlert.Core.Tests/
dotnet test tests/QualityWaterAlert.Infrastructure.Tests/

# Run a single test by name (NUnit filter syntax)
dotnet test --filter "FullyQualifiedName~ComplianceCheckerTests"
dotnet test --filter "TestName=SomeSpecificTestMethodName"

# Run the Blazor web application
cd src/QualityWaterAlert.WebApp && dotnet run

# Run with Docker
docker-compose up --build
# App available at http://localhost:8080
```

## Architecture

This is a **Clean Architecture** .NET 10 solution with four projects in `src/` and two test projects in `tests/`.

### Layer dependencies (inner → outer)
```
Core ← Infrastructure ← WebApp / App
```

- **`QualityWaterAlert.Core`** — Pure business logic, no external dependencies. Contains:
  - `Models/`: `Commune`, `WaterNetwork`, `SamplingEvent`, `WaterQualityAnalysis`, `WaterQualityParameter`
  - `Services/`: `IComplianceChecker` / `ComplianceChecker`, `ICommuneSearcher` / `CommuneSearcher`
  - Interfaces and result types (`ComplianceResult`, `ComplianceCheckSummary`, `AnalysisComplianceSummary`) are colocated in the service files rather than in separate files.

- **`QualityWaterAlert.Infrastructure`** — External integrations:
  - `Interfaces/IDataProvider.cs` — contract for fetching water quality data
  - `Providers/DataGouvFrProvider.cs` — fetches from `data.gouv.fr` API with an in-memory 60-minute commune cache
  - `Interfaces/IEmailService.cs` / `Services/EmailService.cs` — SMTP-based alert emails (configured via constructor parameters, not DI config yet)

- **`QualityWaterAlert.WebApp`** — Blazor Server app (Interactive Server render mode). Uses **MudBlazor** for UI and **Syncfusion.Blazor.Charts** for charts. Refit + `Microsoft.Extensions.Http` for typed API clients.

- **`QualityWaterAlert.App`** — .NET MAUI cross-platform mobile app (Windows/iOS/macCatalyst). Currently scaffolded for post-MVP.

### Key domain concepts

- **Commune** — French municipality identified by a 5-digit INSEE code (e.g. `75056` for Paris). Has one or more `WaterNetwork`s.
- **WaterNetwork** (UDI — Unité de Distribution) — a water distribution unit serving part of a commune.
- **SamplingEvent** (prélèvement) — a water sampling at a specific date/location. Contains multiple `WaterQualityParameter` measurements.
- **WaterQualityParameter** — a single measured parameter (e.g. Nitrates at 12 mg/L). Has `MeasuredValue` (string), optional `NumericValue` (decimal), `QualityLimit` (string like `"<=50"`, `"200-1100"`, or `"X"`), and a `ConformityStatus` char (`'C'`/`'N'`).
- **ComplianceChecker** — parses the `QualityLimit` string (supports `<=`, `<`, `>=`, `>`, plain number, and `min-max` ranges) and compares against `NumericValue`. Always uses `CultureInfo.InvariantCulture` for decimal parsing.

### Data source

Data is the French government's open water quality dataset: `DIS_COM_UDI_2025.txt` (communes/networks), `DIS_PLV_2025.txt` (sampling events), `DIS_RESULT_2025.txt` (measurement results). Local copies of these files exist under `datas/`. `DataGouvFrProvider` is intended to fetch them from `data.gouv.fr` at runtime, but the parsing stubs are incomplete — the data loading pipeline is still work-in-progress.

### Testing

- Framework: **NUnit 4** with `NUnit3TestAdapter`
- Test projects reference only the layer they test; infrastructure tests are in `tests/QualityWaterAlert.Infrastructure.Tests/`
- TDD (Red-Green-Refactor) is mandatory per the project constitution

### Branching model

GitFlow: `main` (production) ← `develop` ← `feature/*`. All merges via Pull Request. Current active branch is `develop`.
