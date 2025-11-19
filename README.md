# QualityWaterAlert

A modern web and mobile application to monitor drinking water quality in France using official government data.

## Overview

QualityWaterAlert provides French citizens with a clear, accessible, and proactive way to monitor the quality of their drinking water. The application presents official government data in a user-friendly format, compares it against regulatory standards, and alerts users to potential quality issues in their commune.

## Features

- 🔍 **Search by Commune**: Find water quality data by commune name or postal code
- 📊 **Data Visualization**: View water quality analysis results in clean tables and charts
- ✅ **Compliance Checking**: See a clear comparison between measured values and regulatory limits
- 🔔 **Email Alerts**: Subscribe to notifications for water quality changes in your commune
- 📱 **Cross-Platform**: Access via web (Blazor) or mobile app (.NET MAUI)

## Technology Stack

- **Web Framework**: Blazor (ASP.NET Core)
- **Mobile Framework**: .NET MAUI
- **Language**: C#
- **Testing**: NUnit
- **Containerization**: Docker
- **Data Source**: data.gouv.fr API

## Project Structure

```
/QualityWaterAlert
├── /src
│   ├── QualityWaterAlert.WebApp/              # Blazor web application
│   ├── QualityWaterAlert.App/                 # .NET MAUI mobile app
│   ├── QualityWaterAlert.Core/                # Business logic and models
│   └── QualityWaterAlert.Infrastructure/      # Data access and API integration
├── /tests
│   ├── QualityWaterAlert.Core.Tests/          # Core logic tests
│   └── QualityWaterAlert.Infrastructure.Tests/# Infrastructure tests
├── /.github/
│   └── /prompts/                              # Project documentation
├── /.specify/
│   ├── /memory/                               # Constitution and guidelines
│   ├── /spec/                                 # Specification documents
│   ├── /plan/                                 # Development plan
│   └── /tasks/                                # Task list
├── docker-compose.yml
├── .gitignore
└── README.md
```

## Getting Started

### Prerequisites

- .NET 10.0 or higher
- Docker (for containerized deployment)
- Git

### Development Setup

1. Clone the repository:
```bash
git clone https://github.com/ClementG/QualityWaterAlert.git
cd QualityWaterAlert
```

2. Switch to the develop branch:
```bash
git checkout develop
```

3. Build the solution:
```bash
dotnet build
```

4. Run the Blazor web app:
```bash
cd src/QualityWaterAlert.WebApp
dotnet run
```

5. Run tests:
```bash
dotnet test
```

### Docker Deployment

Build and run the Docker container:

```bash
docker-compose up --build
```

The application will be available at `http://localhost:8080`

## Development Workflow

This project follows the **GitFlow** branching model:

- **main**: Production-ready code
- **develop**: Primary development branch
- **feature/**: Feature branches for new functionality
- **release/**: Release preparation branches
- **hotfix/**: Critical production fixes

All changes must:
1. Be implemented with test-first development (TDD)
2. Pass all unit tests
3. Be reviewed in a Pull Request
4. Adhere to the project constitution

## Project Constitution

See [Constitution](./CONSTITUTION.md) for core principles, development standards, and governance rules.

## Documentation

- [Specification](./SPECIFICATION.md) - Feature specifications and requirements
- [Development Plan](./DEVELOPMENT_PLAN.md) - Detailed development roadmap
- [Architecture](./ARCHITECTURE.md) - System architecture and design patterns
- [Deployment Guide](./DEPLOYMENT.md) - Instructions for deploying to NAS

## Contributing

Please read [CONTRIBUTING.md](./CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Data Source

Water quality data is sourced from the French government's open data platform:
- Dataset: [Résultats du contrôle sanitaire de l'eau distribuée commune par commune](https://www.data.gouv.fr/datasets/resultats-du-controle-sanitaire-de-leau-distribuee-commune-par-commune/)

## Support

For issues or feature requests, please open an issue on [GitHub](https://github.com/ClementG/QualityWaterAlert/issues).

## Authors

- Clement G ([GitHub](https://github.com/ClementG))

---

**Last Updated**: November 19, 2025
