# QualityWaterAlert Constitution

## Core Principles

### I. Modern Web Application
The project is a web application built with Blazor, focusing on delivering a modern, responsive, and user-friendly interface.

### II. Data-Driven Insights
The application will utilize public data from the French government's open data platform to provide users with up-to-date information about drinking water quality. Data integrity, accuracy, and clear visualization are paramount. The primary dataset is: `https://www.data.gouv.fr/datasets/resultats-du-controle-sanitaire-de-leau-distribuee-commune-par-commune/`.

### III. Test-First Development (NON-NEGOTIABLE)
TDD is mandatory. All new features must be accompanied by a comprehensive suite of unit tests. Tests must be written before implementation, confirmed to fail, and then the implementation will be written to make the tests pass. The Red-Green-Refactor cycle is strictly enforced.

### IV. Clean Architecture
The application will adhere to Clean Architecture principles. This ensures a clear separation of concerns between the UI (Blazor components), application logic, and data access layers. This approach enhances maintainability, scalability, and testability.

### V. Continuous Integration & Delivery (CI/CD)
A CI/CD pipeline will be established to automate the build, testing, and deployment processes. This ensures that every change is automatically validated and can be released to users quickly and reliably.

## Technology Stack

- **Framework**: Blazor (ASP.NET Core)
- **Language**: C#
- **Frontend**: Blazor components, HTML5, CSS3. A modern UI framework (e.g., MudBlazor, Tailwind CSS) will be used.
- **Testing**: xUnit for unit and integration testing. Playwright for end-to-end testing.
- **Data Source**: Public APIs from data.gouv.fr.

## Development Workflow

- **Version Control**: Git, using a feature-branch workflow. All work is done on feature branches and merged into the main branch via Pull Requests.
- **Repository**: The project is hosted on GitHub at `https://github.com/ClementG/QualityWaterAlert`.
- **Code Reviews**: All Pull Requests must be reviewed and approved by at least one other team member before being merged. Reviews should focus on correctness, clarity, and adherence to the constitution.
- **Issue Tracking**: GitHub Issues will be used to track all tasks, features, and bugs.

## Governance

This Constitution supersedes all other practices. All pull requests and code reviews must verify compliance with these principles. Any deviation or complexity must be explicitly justified and documented. Amendments to this constitution require team-wide discussion, documentation of the change, an approved vote, and a clear migration plan if necessary.

**Version**: 1.0.0 | **Ratified**: 2025-11-19 | **Last Amended**: 2025-11-19
