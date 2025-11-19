# QualityWaterAlert Development Plan

## 1. Project Structure & Technology Stack

This project will be developed using a combination of Blazor for the web application and .NET MAUI for the cross-platform mobile application, ensuring a modern and versatile user experience. The entire solution will be containerized with Docker for easy deployment on a NAS or any other container-hosting environment.

### Technology Choices:
- **Web Framework**: Blazor Server
- **Mobile Framework**: .NET MAUI
- **Backend Language**: C# with .NET
- **Unit Testing**: NUnit
- **Containerization**: Docker
- **Version Control**: Git, following the GitFlow pattern

### Solution Structure:
The solution will be organized into the following projects to maintain a clean and scalable architecture:

```
/QualityWaterAlert
|-- /src
|   |-- QualityWaterAlert.WebApp/           # Blazor Server project
|   |-- QualityWaterAlert.App/              # .NET MAUI project
|   |-- QualityWaterAlert.Core/             # Shared business logic, models, and services
|   |-- QualityWaterAlert.Infrastructure/   # Data access, API clients, etc.
|-- /tests
|   |-- QualityWaterAlert.Core.Tests/       # NUnit tests for the Core project
|   |-- QualityWaterAlert.Infrastructure.Tests/ # NUnit tests for Infrastructure
|-- docker-compose.yml
|-- .gitignore
|-- README.md
|-- QualityWaterAlert.sln
```

## 2. Development Phases & Milestones

### Phase 1: Core Functionality & Backend Setup
- **Milestone 1.1: Project Scaffolding**: Set up the solution structure with all the defined projects.
- **Milestone 1.2: Core Logic**: Implement the core business logic in the `QualityWaterAlert.Core` project. This includes models for water quality data and the logic for comparing measured values against regulatory limits.
- **Milestone 1.3: Data Access**: Create the API client in the `QualityWaterAlert.Infrastructure` project to fetch data from the `data.gouv.fr` API.
- **Milestone 1.4: Unit Testing**: Implement NUnit tests for all core logic and data access components to ensure they are working correctly.

### Phase 2: Blazor Web Application
- **Milestone 2.1: UI/UX Design**: Design a clean and intuitive user interface for the Blazor application, focusing on data visualization.
- **Milestone 2.2: Component Development**: Develop Blazor components for searching by commune, displaying data in tables and charts, and managing alert subscriptions.
- **Milestone 2.3: Email Service Integration**: Integrate an email service to handle the sending of alerts.
- **Milestone 2.4: End-to-End Testing**: Manually test the complete user flow from searching to subscribing for alerts.

### Phase 3: .NET MAUI Application
- **Milestone 3.1: UI/UX Adaptation**: Adapt the UI/UX design for mobile devices, ensuring a native look and feel.
- **Milestone 3.2: View Development**: Develop the necessary views in .NET MAUI to replicate the functionality of the Blazor app.
- **Milestone 3.3: Platform Testing**: Test the application on both Android and iOS (simulators or physical devices).

### Phase 4: Containerization & Deployment
- **Milestone 4.1: Dockerfile Creation**: Write a `Dockerfile` for the Blazor application.
- **Milestone 4.2: Docker Compose**: Create a `docker-compose.yml` file to manage the application container.
- **Milestone 4.3: NAS Deployment**: Write clear instructions on how to deploy the application to a NAS using Docker.

## 3. GitFlow Workflow

The project will adhere to the GitFlow branching model to ensure a structured and predictable release cycle.

- **`main` branch**: Represents the production-ready code.
- **`develop` branch**: The primary development branch where all feature branches are merged.
- **Feature branches (`feature/`)**: Created from `develop` for new features (e.g., `feature/data-display`, `feature/alert-system`).
- **Release branches (`release/`)**: Created from `develop` when preparing for a new release.
- **Hotfix branches (`hotfix/`)**: Created from `main` to address critical production issues.

All merges into `develop` and `main` will be done through Pull Requests, which must be reviewed and pass all automated tests before being merged.
