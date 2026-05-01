# QualityWaterAlert Development Tasks

## Phase 1: Core Functionality & Backend Setup

### Epic 1.1: Project Infrastructure

- [x] **1.1.1** Verify all project templates are properly scaffolded
  - Description: Ensure all 7 projects (WebApp, App, Core, Infrastructure, and 2 test projects) are created and configured
  - Status: Completed
  - Priority: Critical

- [x] **1.1.2** Configure project dependencies
  - Description: Set up project references between WebApp → Core + Infrastructure, App → Core + Infrastructure, Infrastructure → Core, and test projects
  - Status: Completed
  - Priority: Critical

- [x] **1.1.3** Add NuGet packages for Blazor WebApp
  - Description: Add packages like MudBlazor (or Tailwind CSS), HttpClient, and any other required dependencies
  - Status: Completed
  - Priority: High

### Epic 1.2: Core Business Logic

- [x] **1.2.1** Create WaterQualityParameter model
  - Description: Define the data model for water quality parameters (name, measured value, limit, unit, date)
  - Location: `src/QualityWaterAlert.Core/Models/WaterQualityParameter.cs`
  - Status: Completed
  - Priority: Critical

- [x] **1.2.2** Create WaterQualityAnalysis model
  - Description: Define the model for a complete water quality analysis result (commune, parameters, date)
  - Location: `src/QualityWaterAlert.Core/Models/WaterQualityAnalysis.cs`
  - Status: Completed
  - Priority: Critical

- [x] **1.2.3** Create ComplianceChecker service
  - Description: Implement logic to compare measured values against regulatory limits
  - Location: `src/QualityWaterAlert.Core/Services/ComplianceChecker.cs`
  - Status: Completed
  - Priority: Critical

- [x] **1.2.4** Create CommuneSearcher service
  - Description: Implement logic to search for communes by name or postal code
  - Location: `src/QualityWaterAlert.Core/Services/CommuneSearcher.cs`
  - Status: Completed
  - Priority: High

### Epic 1.3: Data Access & API Integration

- [x] **1.3.1** Create IDataProvider interface
  - Description: Define the contract for fetching water quality data
  - Location: `src/QualityWaterAlert.Infrastructure/Interfaces/IDataProvider.cs`
  - Status: Completed
  - Priority: Critical

- [x] **1.3.2** Implement DataGouvFrProvider
  - Description: Create API client to fetch data from data.gouv.fr API
  - Location: `src/QualityWaterAlert.Infrastructure/Providers/DataGouvFrProvider.cs`
  - Status: Completed (rewritten)
  - Priority: Critical
  - Implementation: Full production pipeline using HTTP Range requests (~500 lines)
    - Correct dataset ID: `5cf8d9ed8b4c4110294c841d`
    - **HTTP Range requests** on the remote ZIP — no full 274 MB download:
      - 1 API call → discovers latest `dis-YYYY-dept.zip` URL (cached 24 h)
      - 2 range requests → reads ZIP Central Directory (~20 KB)
      - 4 range requests → extracts only the needed dept files (~2–10 MB per dept)
    - `GetAllCommunesAsync()` — parses `DIS_COM_UDI_YYYY.txt`, all communes cached 24 h
    - `GetWaterQualityAnalysisAsync()` — derives dept from INSEE code, fetches and parses
      `DIS_PLV_YYYY_{dept}.txt` + `DIS_RESULT_YYYY_{dept}.txt`, joins them, computes
      conformity via `ComplianceChecker`, returns complete `WaterQualityAnalysis`
    - Per-department in-memory cache (24 h TTL, `ConcurrentDictionary`)
    - URL and Central Directory cached independently (URL cached even if CD load fails)
    - RFC-4180-compliant CSV parser (handles empty unquoted fields, escaped quotes)
    - ZIP format: EOCD → CD parse → local file header → DEFLATE decompress
    - `Program.cs`: registered as singleton with named `HttpClient` (2-min timeout)
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **1.3.3** Create IEmailService interface
  - Description: Define the contract for sending emails
  - Location: `src/QualityWaterAlert.Infrastructure/Interfaces/IEmailService.cs`
  - Status: Completed
  - Priority: High

- [x] **1.3.4** Implement EmailService
  - Description: Create email service for sending alert notifications
  - Location: `src/QualityWaterAlert.Infrastructure/Services/EmailService.cs`
  - Status: Completed
  - Priority: High

### Epic 1.4: Unit Tests - Core

- [x] **1.4.1** Write tests for ComplianceChecker
  - Description: Test the logic for comparing values against limits
  - Location: `tests/QualityWaterAlert.Core.Tests/Services/ComplianceCheckerTests.cs`
  - Status: Completed
  - Priority: Critical
  - Test Results: **17/17 PASSING** (100%)
    - Numeric comparisons (<=, <, >=, >, ranges) - all passing
    - Qualitative measurements - all passing
    - Edge cases (zero, decimals, missing values) - all passing
    - Multiple parameters - passing
    - Sampling event aggregation - passing
    - Analysis aggregation - passing
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **1.4.2** Write tests for CommuneSearcher
  - Description: Test the search functionality for communes
  - Location: `tests/QualityWaterAlert.Core.Tests/Services/CommuneSearcherTests.cs`
  - Status: Completed
  - Priority: High
  - Test Results: **42/42 PASSING** (100%)
    - Search by name (partial matching) - all passing
    - Search by INSEE code (exact match) - all passing
    - Search by postal code - all passing
    - Search by department - all passing
    - Pagination (GetAllCommunes) - all passing
    - Advanced search with criteria - all passing
    - Sorting (NameAscending, ConformityFirst, NetworkCount, etc.) - all passing
    - Related communes lookup - all passing
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **1.4.3** Write tests for data models
  - Description: Test all 5 data models (WaterQualityParameter, Commune, WaterNetwork, SamplingEvent, WaterQualityAnalysis)
  - Location: `tests/QualityWaterAlert.Core.Tests/Models/DataModelTests.cs`
  - Status: Completed
  - Priority: High
  - Test Results: **48/48 PASSING** (100%)
    - WaterQualityParameter properties and methods - all passing
    - Commune conformity calculations - all passing
    - WaterNetwork conformity tracking - all passing
    - SamplingEvent aggregation - all passing
    - WaterQualityAnalysis aggregation and trending - all passing
  - Build Status: ✅ Clean (0 errors, 0 warnings)

### Epic 1.5: Unit Tests - Infrastructure

- [x] **1.5.1** Write tests for DataGouvFrProvider
  - Description: Mock API calls and test data parsing
  - Location: `tests/QualityWaterAlert.Infrastructure.Tests/Providers/DataGouvFrProviderTests.cs`
  - Status: Completed (updated to match rewritten provider)
  - Priority: Critical
  - Test Results: **10/10 PASSING** (100%)
    - Constructor validation (null HttpClient) — passing
    - Constructor valid — passing
    - ArgumentException for invalid INSEE codes (null, empty, whitespace) — passing
    - InvalidOperationException when commune not found — passing
    - InvalidOperationException on API failure (replaces former "empty list" fallback) — passing
    - InvalidOperationException when no matching ZIP resource — passing
    - Cache: dataset API queried only once; URL cached even if ZIP parsing fails — passing
  - Test doubles: `StubHttpMessageHandler`, `CountingHttpMessageHandler`, `UrlAwareCountingHandler`
    (URL-aware handler counts calls to `/api/1/datasets/` separately from ZIP range requests)
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **1.5.2** Write tests for EmailService
  - Description: Mock email sending and test notification content
  - Location: `tests/QualityWaterAlert.Infrastructure.Tests/Services/EmailServiceTests.cs`
  - Status: Completed
  - Priority: High
  - Test Results: **12/12 PASSING** (100%)
    - Constructor null-argument validation (smtpServer, fromEmail, senderName) — passing
    - SendEmailAsync validation: invalid To/Subject/Body (null, empty, whitespace) — passing
  - Build Status: ✅ Clean (0 errors, 0 warnings)

## Phase 2: Blazor Web Application

### Epic 2.1: UI/UX Design & Components

- [x] **2.1.1** Design main layout and navigation
  - Description: Create the overall page layout with navigation menu, header, and footer
  - Location: `src/QualityWaterAlert.WebApp/Components/Layout/` (MainLayout.razor, NavMenu.razor)
  - Status: Completed
  - Priority: High

- [x] **2.1.2** Create CommuneSearch component
  - Description: Blazor component for searching communes by name or postal code
  - Location: `src/QualityWaterAlert.WebApp/Components/CommuneSearch.razor`
  - Status: Completed
  - Priority: Critical
  - Implementation:
    - Text input with search icon; placeholder switches to "Chargement…" during init
    - Auto-detects postal code (digits) vs commune name and routes to `SearchByPostalCode` / `SearchByName`
    - Dropdown with up to 15 results (configurable via `MaxResults` parameter)
    - `OnCommuneSelected` EventCallback emits the chosen `Commune` to parent
    - Escape key closes dropdown; min query length configurable via `MinQueryLength` (default 2)
    - `IDataProvider` registered in DI (`Program.cs`) with typed `HttpClient`
    - Core usings added to `_Imports.razor` (Core.Models, Core.Services, Infrastructure.Interfaces)
    - Component styles added to `wwwroot/app.css`
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.1.3** Create WaterQualityDisplay component
  - Description: Blazor component to display water quality data with tables and charts
  - Location: `src/QualityWaterAlert.WebApp/Components/WaterQualityDisplay.razor`
  - Status: Completed
  - Priority: Critical
  - Implementation:
    - Conformity banner (green/red alert) with commune name, status text, and date range
    - 4 summary stat cards: Prélèvements, Conformes, Non-conformes, Taux de conformité (color-coded)
    - Problematic parameters table (only rendered when non-conform results exist)
    - Recent samplings table with bacterio/chemical/overall conformity badges (configurable via `MaxSamplings`, default 10)
    - Latest sampling measurements table with measured value, regulatory limit, and status
    - `IsLoading` parameter shows a spinner while data is being fetched
    - All computed values (`Latest`, `Oldest`, `Problematic`, `RecentSamplings`, `PctColor`) exposed as properties in `@code` section — no inline `@{ }` blocks in template
    - Component styles added to `wwwroot/app.css` (`.wqd-stat-value`, `.wqd-danger-header`)
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.1.4** Create ComplianceIndicator component
  - Description: Blazor component for visual compliance status (green/red, compliant/non-compliant)
  - Location: `src/QualityWaterAlert.WebApp/Components/ComplianceIndicator.razor`
  - Status: Completed
  - Priority: High
  - Implementation:
    - Parameters: `ConformityStatus` (char?), `ShowLabel` (bool, default true), `Size` ("sm"/"md"/"lg", default "md")
    - 'C' → green pill "Conforme", 'N' → red pill "Non-conforme", null → yellow "Inconnu"
    - `role="img"` + `aria-label` pour l'accessibilité
    - WaterQualityDisplay refactorisé pour utiliser `<ComplianceIndicator>` (méthode ConformityBadge supprimée)
    - Component styles added to `wwwroot/app.css`
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.1.5** Create AlertSubscription component
  - Description: Blazor component with bell icon and email subscription modal
  - Location: `src/QualityWaterAlert.WebApp/Components/AlertSubscription.razor`
  - Status: Completed
  - Priority: Critical
  - Implementation:
    - Bouton cloche SVG qui ouvre une modal overlay CSS pure (sans JS interop)
    - Modal avec `EditForm` + `DataAnnotationsValidator` — validation [Required] + [EmailAddress]
    - Spinner sur le bouton submit pendant le traitement
    - Paramètres: `Commune?`, `EventCallback<string> OnSubscribed`
    - Fermeture par clic sur l'overlay, bouton ×, ou bouton Annuler
    - Component styles added to `wwwroot/app.css`
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.1.6** Create AlertConfirmation component
  - Description: Blazor component to display confirmation after email subscription
  - Location: `src/QualityWaterAlert.WebApp/Components/AlertConfirmation.razor`
  - Status: Completed
  - Priority: High
  - Implementation:
    - Bandeau vert "Inscription confirmée" avec email et nom de commune
    - Paramètres: `Email?`, `Commune?`, `EventCallback OnDismiss`, `Size` ("sm"/"md"/"lg")
    - Bouton × conditionnel (affiché uniquement si `OnDismiss` est branché)
    - `aria-live="polite"` pour l'accessibilité
    - Component styles added to `wwwroot/app.css`
  - Build Status: ✅ Clean (0 errors, 0 warnings)

### Epic 2.2: Pages & Routing ✅ COMPLETE

- [x] **2.2.1** Create Home page
  - Description: Main landing page with search functionality
  - Location: `src/QualityWaterAlert.WebApp/Components/Pages/Home.razor`
  - Status: Completed
  - Priority: High
  - Implementation:
    - Hero avec gradient bleu, titre, sous-titre et CommuneSearch
    - Sélection d'une commune → NavigationManager vers `/results/{inseeCode}`
    - 3 blocs features (Données officielles, Conformité instantanée, Alertes email)
    - Styles home-hero / home-feature ajoutés à app.css
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.2.2** Create Results page
  - Description: Display water quality analysis results for selected commune
  - Location: `src/QualityWaterAlert.WebApp/Components/Pages/Results.razor`
  - Status: Completed
  - Priority: Critical
  - Implementation:
    - Routes: `@page "/results/{InseeCode}"` + `@page "/results"` (redirige vers `/`)
    - Header: nom commune, dept/INSEE, ComplianceIndicator (lg), AlertSubscription
    - Chargement async via `IDataProvider.GetWaterQualityAnalysisAsync` avec état IsLoading
    - Gestion d'erreur avec alerte warning
    - AlertConfirmation affiché après abonnement, dismissable
    - Skeleton placeholder pour le titre pendant le chargement
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.2.3** Create About page
  - Description: Information about the application, data sources, and how to use it
  - Location: `src/QualityWaterAlert.WebApp/Components/Pages/About.razor`
  - Status: Completed
  - Priority: Medium
  - Implementation:
    - 5 sections: présentation, source données (fichiers .txt), comment utiliser, légende des indicateurs (ComplianceIndicator live), RGPD
    - Layout max-width 720px
  - Build Status: ✅ Clean (0 errors, 0 warnings)

### Epic 2.3: Styling & Modern UI ✅ COMPLETE

- [x] **2.3.1** Integrate MudBlazor or Tailwind CSS
  - Description: Set up modern UI framework for responsive design
  - Status: Completed
  - Priority: High
  - Implementation:
    - MudBlazor 8 configured: `AddMudServices()` + CSS/JS in App.razor
    - Custom blue water theme (Primary #1565C0, dark drawer #1A237E)
    - `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, `MudSnackbarProvider` in MainLayout
    - Bootstrap CSS removed; all components migrated to MudBlazor
    - MainLayout: `MudLayout` + `MudAppBar` + `MudDrawer` with responsive hamburger (mobile/desktop)
    - NavMenu: `MudNavLink` items
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.3.2** Create responsive CSS/styling
  - Description: Ensure the application looks good on mobile, tablet, and desktop
  - Location: `src/QualityWaterAlert.WebApp/wwwroot/app.css`
  - Status: Completed
  - Priority: High
  - Implementation:
    - All pages migrated to `MudGrid`/`MudItem` (responsive xs/sm/md breakpoints)
    - `CommuneSearch` rewritten with `MudAutocomplete` (mobile-friendly, accessible)
    - `AlertSubscription` → `IDialogService` + `MudDialog` (new `AlertSubscriptionDialog.razor`)
    - `AlertConfirmation` → `MudAlert`
    - `WaterQualityDisplay` → `MudCard` + custom `.wqa-table` CSS (overflow-x-auto)
    - Home hero full-width gradient; features in `MudGrid` 3-column responsive
    - `app.css` rewritten: MudBlazor CSS variable utilities, custom table/badge styles
  - Build Status: ✅ Clean (0 errors, 0 warnings)

- [x] **2.3.3** Add chart library (e.g., Chart.js)
  - Description: Integrate a charting library for data visualization
  - Status: Completed
  - Priority: High
  - Implementation:
    - Chart.js 4 via CDN (`chart.umd.min.js`)
    - `wwwroot/js/charts.js`: `window.qwa.createDonutChart()` + `destroyChart()` interop helpers
    - `Components/Charts/ConformityDonutChart.razor`: canvas-based donut chart, `IAsyncDisposable`
    - Chart shows conforming vs non-conforming samplings (green/red) with legend + tooltips
    - Placed alongside stat cards in `WaterQualityDisplay` (md=4 column, stacks on mobile)
    - Graceful degradation: `JSException` caught if CDN fails to load
  - Build Status: ✅ Clean (0 errors, 0 warnings)

## Phase 3: Containerization & Deployment

### Epic 3.1: Docker Configuration

- [x] **3.1.1** Create Dockerfile for Blazor WebApp
  - Description: Multi-stage Dockerfile for building and running the Blazor app
  - Location: `src/QualityWaterAlert.WebApp/Dockerfile`
  - Status: Completed
  - Priority: Critical
  - Implementation:
    - Build stage: `mcr.microsoft.com/dotnet/sdk:10.0` — restores only csproj files first (layer cache), then copies src/ and publishes Release
    - Runtime stage: `mcr.microsoft.com/dotnet/aspnet:10.0` — minimal ASP.NET runtime, non-root user (appuser:1001), exposes port 8080
    - Build context: repository root (docker-compose sets context, Dockerfile path points here)
    - `ASPNETCORE_HTTP_PORTS=8080` set by base image (default since .NET 8)

- [x] **3.1.2** Create docker-compose.yml
  - Description: Define services and volumes for the application
  - Location: `docker-compose.yml`
  - Status: Completed
  - Priority: Critical
  - Implementation:
    - Service `webapp`: build context `.` (repo root), dockerfile `src/QualityWaterAlert.WebApp/Dockerfile`
    - Port `8080:8080`, restart `unless-stopped`
    - `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` pour reverse proxy NAS (Synology/QNAP/nginx)
    - `env_file: .env` optionnel (`required: false`) pour les variables SMTP futures
    - Healthcheck `curl -f http://localhost:8080/` (30s interval, 30s start_period)

- [x] **3.1.3** Create .dockerignore
  - Description: Exclude unnecessary files from Docker build context
  - Location: `.dockerignore`
  - Status: Completed (was already present)
  - Priority: Medium
  - Implementation: Excludes .git, .vs, bin, obj, .env*, tests, docs, .specify

### Epic 3.2: NAS Deployment Guide

- [ ] **3.2.1** Write NAS deployment documentation
  - Description: Step-by-step guide for deploying to NAS using Docker
  - Location: `docs/DEPLOYMENT.md`
  - Status: Not Started
  - Priority: High

- [ ] **3.2.2** Create environment configuration template
  - Description: Create .env.example with all necessary environment variables
  - Location: `.env.example`
  - Status: Not Started
  - Priority: Medium

## Phase 4: GitFlow & CI/CD Setup

### Epic 4.1: Repository Setup

- [ ] **4.1.1** Initialize Git branches
  - Description: Create main, develop, and initial feature branches
  - Status: Not Started
  - Priority: Medium

- [ ] **4.1.2** Create GitHub Actions workflows
  - Description: Set up CI/CD pipeline for automated testing and deployment
  - Location: `.github/workflows/`
  - Status: Not Started
  - Priority: High

- [ ] **4.1.3** Create CONTRIBUTING.md
  - Description: Guidelines for contributing to the project
  - Location: `CONTRIBUTING.md`
  - Status: Not Started
  - Priority: Medium

### Epic 4.2: Documentation

- [ ] **4.2.1** Create comprehensive README.md
  - Description: Project overview, setup instructions, and usage guide
  - Location: `README.md`
  - Status: Not Started
  - Priority: High

- [ ] **4.2.2** Create API documentation
  - Description: Document the data.gouv.fr API integration
  - Location: `docs/API.md`
  - Status: Not Started
  - Priority: Medium

- [ ] **4.2.3** Create architecture documentation
  - Description: Explain the clean architecture and project structure
  - Location: `docs/ARCHITECTURE.md`
  - Status: Not Started
  - Priority: Medium

## Phase 5: Future Enhancements (Post-MVP)

### Epic 5.1: Mobile Application (Optional)

- [ ] **5.1.1** Design and implement .NET MAUI mobile application
  - Description: Cross-platform mobile app for iOS and Android
  - Status: Not Started (Future)
  - Priority: Low

## Summary

**Total Tasks**: 41 (removed 24 MAUI-related tasks for future phase)
**Completed**: 17
**In Progress**: 0
**Not Started**: 24

**Critical Priority Tasks**: 10
**High Priority Tasks**: 18
**Medium Priority Tasks**: 12
**Low Priority Tasks**: 1

### Phase 1.2 Progress: ✅ COMPLETE (All 4 tasks done)
- ✅ 1.2.1 WaterQualityParameter model (287 lines)
- ✅ 1.2.2 WaterQualityAnalysis model (220 lines)
- ✅ 1.2.3 ComplianceChecker service (380+ lines)
- ✅ 1.2.4 CommuneSearcher service (340+ lines)
- ✅ Plus supporting models: Commune, WaterNetwork, SamplingEvent (~400 lines)
- ✅ Build verified: All projects compile, 0 errors, 0 warnings

### Phase 1.3 Progress: ✅ COMPLETE (All 4 tasks done)
- ✅ 1.3.1 IDataProvider interface (Completed)
- ✅ 1.3.2 DataGouvFrProvider implementation (rewritten, ~500 lines)
  - HTTP Range requests — only fetches dept-specific files (~2–10 MB vs 274 MB full ZIP)
  - Full PLV + RESULT pipeline: `GetWaterQualityAnalysisAsync` fully implemented
  - Per-dept in-memory cache with 24 h TTL, URL/CD caches are independent layers
  - RFC-4180 CSV parser, ComplianceChecker integration, correct dataset ID
  - Build Status: ✅ Clean (0 errors, 0 warnings)
- ✅ 1.3.3 IEmailService interface (Completed)
- ✅ 1.3.4 EmailService implementation (Completed)

### Phase 1.4 Progress: ✅ COMPLETE (All 3 tasks done)
- ✅ 1.4.1 ComplianceChecker tests (17/17 PASSING)
- ✅ 1.4.2 CommuneSearcher tests (42/42 PASSING)  
- ✅ 1.4.3 Data model tests (48/48 PASSING)
- ✅ **Total Phase 1.4: 108/108 tests PASSING (100% pass rate)**
- ✅ Build verified: All projects compile, 0 errors, 0 warnings

### Phase 2.3 Progress: ✅ COMPLETE (3/3 tasks done)
- ✅ 2.3.1 MudBlazor 8 fully configured — custom theme, layout, all components migrated
- ✅ 2.3.2 Responsive CSS — MudGrid breakpoints, MudAutocomplete, MudDialog, app.css refactored
- ✅ 2.3.3 Chart.js 4 — ConformityDonutChart.razor + charts.js, displayed in WaterQualityDisplay
- ✅ Build verified: 0 errors, 0 warnings; 108/108 Core tests PASSING

### Phase 2.1 Progress: ✅ COMPLETE (6/6 tasks done)
- ✅ 2.1.1 Main layout and navigation (MainLayout.razor, NavMenu.razor)
- ✅ 2.1.2 CommuneSearch component — search by name or postal code, EventCallback, Bootstrap dropdown
- ✅ 2.1.3 WaterQualityDisplay component — conformity banner, stat cards, problematic params, samplings table, measurements table
- ✅ 2.1.4 ComplianceIndicator component — pill coloré 3 tailles, 'C'/'N'/null, utilisé dans WaterQualityDisplay
- ✅ 2.1.5 AlertSubscription component — modal overlay CSS pure, EditForm validé, EventCallback<string>
- ✅ 2.1.6 AlertConfirmation component — bandeau vert, email + commune, OnDismiss optionnel

### Phase 1.5 Progress: ✅ COMPLETE (All 2 tasks done)
- ✅ 1.5.1 DataGouvFrProvider tests (10/10 PASSING)
  - Constructor validation, INSEE code validation, not-found error
  - API failure now throws (no silent empty-list fallback)
  - URL cache: API only queried once even after ZIP-parsing failure (UrlAwareCountingHandler)
- ✅ 1.5.2 EmailService tests (12/12 PASSING)
  - Constructor validation, SendEmailAsync argument validation
- ✅ **Total Phase 1.5: 22/22 tests PASSING (100% pass rate)**
- ✅ Build verified: All projects compile, 0 errors, 0 warnings
