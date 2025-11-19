# QualityWaterAlert Development Tasks

## Phase 1: Core Functionality & Backend Setup

### Epic 1.1: Project Infrastructure

- [x] **1.1.1** Verify all project templates are properly scaffolded
  - Description: Ensure all 7 projects (WebApp, App, Core, Infrastructure, and 2 test projects) are created and configured
  - Status: Completed
  - Priority: Critical

- [ ] **1.1.2** Configure project dependencies
  - Description: Set up project references between WebApp → Core + Infrastructure, App → Core + Infrastructure, Infrastructure → Core, and test projects
  - Status: Not Started
  - Priority: Critical

- [ ] **1.1.3** Add NuGet packages for Blazor WebApp
  - Description: Add packages like MudBlazor (or Tailwind CSS), HttpClient, and any other required dependencies
  - Status: Not Started
  - Priority: High

- [ ] **1.1.4** Add NuGet packages for .NET MAUI
  - Description: Add necessary MAUI packages and UI framework packages
  - Status: Not Started
  - Priority: High

### Epic 1.2: Core Business Logic

- [ ] **1.2.1** Create WaterQualityParameter model
  - Description: Define the data model for water quality parameters (name, measured value, limit, unit, date)
  - Location: `src/QualityWaterAlert.Core/Models/WaterQualityParameter.cs`
  - Status: Not Started
  - Priority: Critical

- [ ] **1.2.2** Create WaterQualityAnalysis model
  - Description: Define the model for a complete water quality analysis result (commune, parameters, date)
  - Location: `src/QualityWaterAlert.Core/Models/WaterQualityAnalysis.cs`
  - Status: Not Started
  - Priority: Critical

- [ ] **1.2.3** Create ComplianceChecker service
  - Description: Implement logic to compare measured values against regulatory limits
  - Location: `src/QualityWaterAlert.Core/Services/ComplianceChecker.cs`
  - Status: Not Started
  - Priority: Critical

- [ ] **1.2.4** Create CommuneSearcher service
  - Description: Implement logic to search for communes by name or postal code
  - Location: `src/QualityWaterAlert.Core/Services/CommuneSearcher.cs`
  - Status: Not Started
  - Priority: High

### Epic 1.3: Data Access & API Integration

- [ ] **1.3.1** Create IDataProvider interface
  - Description: Define the contract for fetching water quality data
  - Location: `src/QualityWaterAlert.Infrastructure/Interfaces/IDataProvider.cs`
  - Status: Not Started
  - Priority: Critical

- [ ] **1.3.2** Implement DataGouvFrProvider
  - Description: Create API client to fetch data from data.gouv.fr API
  - Location: `src/QualityWaterAlert.Infrastructure/Providers/DataGouvFrProvider.cs`
  - Status: Not Started
  - Priority: Critical

- [ ] **1.3.3** Create IEmailService interface
  - Description: Define the contract for sending emails
  - Location: `src/QualityWaterAlert.Infrastructure/Interfaces/IEmailService.cs`
  - Status: Not Started
  - Priority: High

- [ ] **1.3.4** Implement EmailService
  - Description: Create email service for sending alert notifications
  - Location: `src/QualityWaterAlert.Infrastructure/Services/EmailService.cs`
  - Status: Not Started
  - Priority: High

### Epic 1.4: Unit Tests - Core

- [ ] **1.4.1** Write tests for ComplianceChecker
  - Description: Test the logic for comparing values against limits
  - Location: `tests/QualityWaterAlert.Core.Tests/Services/ComplianceCheckerTests.cs`
  - Status: Not Started
  - Priority: Critical

- [ ] **1.4.2** Write tests for CommuneSearcher
  - Description: Test the search functionality for communes
  - Location: `tests/QualityWaterAlert.Core.Tests/Services/CommuneSearcherTests.cs`
  - Status: Not Started
  - Priority: High

### Epic 1.5: Unit Tests - Infrastructure

- [ ] **1.5.1** Write tests for DataGouvFrProvider
  - Description: Mock API calls and test data parsing
  - Location: `tests/QualityWaterAlert.Infrastructure.Tests/Providers/DataGouvFrProviderTests.cs`
  - Status: Not Started
  - Priority: Critical

- [ ] **1.5.2** Write tests for EmailService
  - Description: Mock email sending and test notification content
  - Location: `tests/QualityWaterAlert.Infrastructure.Tests/Services/EmailServiceTests.cs`
  - Status: Not Started
  - Priority: High

## Phase 2: Blazor Web Application

### Epic 2.1: UI/UX Design & Components

- [ ] **2.1.1** Design main layout and navigation
  - Description: Create the overall page layout with navigation menu, header, and footer
  - Location: `src/QualityWaterAlert.WebApp/Shared/`
  - Status: Not Started
  - Priority: High

- [ ] **2.1.2** Create CommuneSearch component
  - Description: Blazor component for searching communes by name or postal code
  - Location: `src/QualityWaterAlert.WebApp/Components/CommuneSearch.razor`
  - Status: Not Started
  - Priority: Critical

- [ ] **2.1.3** Create WaterQualityDisplay component
  - Description: Blazor component to display water quality data with tables and charts
  - Location: `src/QualityWaterAlert.WebApp/Components/WaterQualityDisplay.razor`
  - Status: Not Started
  - Priority: Critical

- [ ] **2.1.4** Create ComplianceIndicator component
  - Description: Blazor component for visual compliance status (green/red, compliant/non-compliant)
  - Location: `src/QualityWaterAlert.WebApp/Components/ComplianceIndicator.razor`
  - Status: Not Started
  - Priority: High

- [ ] **2.1.5** Create AlertSubscription component
  - Description: Blazor component with bell icon and email subscription modal
  - Location: `src/QualityWaterAlert.WebApp/Components/AlertSubscription.razor`
  - Status: Not Started
  - Priority: Critical

- [ ] **2.1.6** Create AlertConfirmation component
  - Description: Blazor component to display confirmation after email subscription
  - Location: `src/QualityWaterAlert.WebApp/Components/AlertConfirmation.razor`
  - Status: Not Started
  - Priority: High

### Epic 2.2: Pages & Routing

- [ ] **2.2.1** Create Home page
  - Description: Main landing page with search functionality
  - Location: `src/QualityWaterAlert.WebApp/Pages/Home.razor`
  - Status: Not Started
  - Priority: High

- [ ] **2.2.2** Create Results page
  - Description: Display water quality analysis results for selected commune
  - Location: `src/QualityWaterAlert.WebApp/Pages/Results.razor`
  - Status: Not Started
  - Priority: Critical

- [ ] **2.2.3** Create About page
  - Description: Information about the application, data sources, and how to use it
  - Location: `src/QualityWaterAlert.WebApp/Pages/About.razor`
  - Status: Not Started
  - Priority: Medium

### Epic 2.3: Styling & Modern UI

- [ ] **2.3.1** Integrate MudBlazor or Tailwind CSS
  - Description: Set up modern UI framework for responsive design
  - Status: Not Started
  - Priority: High

- [ ] **2.3.2** Create responsive CSS/styling
  - Description: Ensure the application looks good on mobile, tablet, and desktop
  - Location: `src/QualityWaterAlert.WebApp/wwwroot/css/`
  - Status: Not Started
  - Priority: High

- [ ] **2.3.3** Add chart library (e.g., Chart.js)
  - Description: Integrate a charting library for data visualization
  - Status: Not Started
  - Priority: High

## Phase 3: .NET MAUI Application

### Epic 3.1: MAUI UI/UX Adaptation

- [ ] **3.1.1** Create MainPage (Shell)
  - Description: Main navigation shell for the MAUI app
  - Location: `src/QualityWaterAlert.App/AppShell.xaml`
  - Status: Not Started
  - Priority: High

- [ ] **3.1.2** Create CommuneSearchPage
  - Description: MAUI page for searching communes
  - Location: `src/QualityWaterAlert.App/Pages/CommuneSearchPage.xaml`
  - Status: Not Started
  - Priority: Critical

- [ ] **3.1.3** Create WaterQualityDetailPage
  - Description: MAUI page to display detailed water quality results
  - Location: `src/QualityWaterAlert.App/Pages/WaterQualityDetailPage.xaml`
  - Status: Not Started
  - Priority: Critical

- [ ] **3.1.4** Create AlertSubscriptionPage
  - Description: MAUI page for subscribing to email alerts
  - Location: `src/QualityWaterAlert.App/Pages/AlertSubscriptionPage.xaml`
  - Status: Not Started
  - Priority: High

### Epic 3.2: MAUI ViewModels & Logic

- [ ] **3.2.1** Create CommuneSearchViewModel
  - Description: MVVM ViewModel for search functionality
  - Location: `src/QualityWaterAlert.App/ViewModels/CommuneSearchViewModel.cs`
  - Status: Not Started
  - Priority: High

- [ ] **3.2.2** Create WaterQualityDetailViewModel
  - Description: MVVM ViewModel for displaying water quality details
  - Location: `src/QualityWaterAlert.App/ViewModels/WaterQualityDetailViewModel.cs`
  - Status: Not Started
  - Priority: High

- [ ] **3.2.3** Create AlertSubscriptionViewModel
  - Description: MVVM ViewModel for alert subscription logic
  - Location: `src/QualityWaterAlert.App/ViewModels/AlertSubscriptionViewModel.cs`
  - Status: Not Started
  - Priority: High

### Epic 3.3: Platform Testing

- [ ] **3.3.1** Test on Android emulator
  - Description: Run and test the MAUI app on Android
  - Status: Not Started
  - Priority: High

- [ ] **3.3.2** Test on iOS simulator
  - Description: Run and test the MAUI app on iOS (if available)
  - Status: Not Started
  - Priority: Medium

## Phase 4: Containerization & Deployment

### Epic 4.1: Docker Configuration

- [ ] **4.1.1** Create Dockerfile for Blazor WebApp
  - Description: Multi-stage Dockerfile for building and running the Blazor app
  - Location: `src/QualityWaterAlert.WebApp/Dockerfile`
  - Status: Not Started
  - Priority: Critical

- [ ] **4.1.2** Create docker-compose.yml
  - Description: Define services and volumes for the application
  - Location: `docker-compose.yml`
  - Status: Not Started
  - Priority: Critical

- [ ] **4.1.3** Create .dockerignore
  - Description: Exclude unnecessary files from Docker build context
  - Location: `.dockerignore`
  - Status: Not Started
  - Priority: Medium

### Epic 4.2: NAS Deployment Guide

- [ ] **4.2.1** Write NAS deployment documentation
  - Description: Step-by-step guide for deploying to NAS using Docker
  - Location: `docs/DEPLOYMENT.md`
  - Status: Not Started
  - Priority: High

- [ ] **4.2.2** Create environment configuration template
  - Description: Create .env.example with all necessary environment variables
  - Location: `.env.example`
  - Status: Not Started
  - Priority: Medium

## Phase 5: GitFlow & CI/CD Setup

### Epic 5.1: Repository Setup

- [ ] **5.1.1** Initialize Git branches
  - Description: Create main, develop, and initial feature branches
  - Status: Not Started
  - Priority: Medium

- [ ] **5.1.2** Create GitHub Actions workflows
  - Description: Set up CI/CD pipeline for automated testing and deployment
  - Location: `.github/workflows/`
  - Status: Not Started
  - Priority: High

- [ ] **5.1.3** Create CONTRIBUTING.md
  - Description: Guidelines for contributing to the project
  - Location: `CONTRIBUTING.md`
  - Status: Not Started
  - Priority: Medium

### Epic 5.2: Documentation

- [ ] **5.2.1** Create comprehensive README.md
  - Description: Project overview, setup instructions, and usage guide
  - Location: `README.md`
  - Status: Not Started
  - Priority: High

- [ ] **5.2.2** Create API documentation
  - Description: Document the data.gouv.fr API integration
  - Location: `docs/API.md`
  - Status: Not Started
  - Priority: Medium

- [ ] **5.2.3** Create architecture documentation
  - Description: Explain the clean architecture and project structure
  - Location: `docs/ARCHITECTURE.md`
  - Status: Not Started
  - Priority: Medium

## Summary

**Total Tasks**: 65
**Completed**: 1
**In Progress**: 0
**Not Started**: 64

**Critical Priority Tasks**: 16
**High Priority Tasks**: 22
**Medium Priority Tasks**: 16
**Low Priority Tasks**: 11
