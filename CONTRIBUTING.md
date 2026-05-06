# Contributing to QualityWaterAlert

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (pour les tests d'intégration Docker)
- Git

## Mise en place locale

```bash
git clone https://github.com/<org>/QualityWaterAlert.git
cd QualityWaterAlert
dotnet restore
dotnet build
dotnet test
```

## Modèle de branches (GitFlow)

| Branche | Rôle |
|---------|------|
| `master` | Production — uniquement des merges depuis `release/*` ou `hotfix/*` |
| `develop` | Intégration — toutes les features convergent ici |
| `feature/<slug>` | Nouvelle fonctionnalité (part de `develop`) |
| `hotfix/<slug>` | Correction urgente en prod (part de `master`) |
| `release/<version>` | Stabilisation avant mise en prod |

```
master ←── release/x.y.z ←── develop ←── feature/my-feature
  ↑                                            (PR → develop)
hotfix/xyz
```

**Règle** : on ne pousse jamais directement sur `master` ni sur `develop` — toujours via une Pull Request.

## Workflow feature

```bash
git checkout develop
git pull
git checkout -b feature/ma-feature

# … développement …

git push -u origin feature/ma-feature
# Ouvrir une PR → develop
```

## Commits

Format : `type(scope): message court en impératif`

| Type | Quand |
|------|-------|
| `feat` | Nouvelle fonctionnalité |
| `fix` | Correction de bug |
| `test` | Ajout/modification de tests |
| `refactor` | Refactoring sans changement fonctionnel |
| `docs` | Documentation uniquement |
| `chore` | Tâche de maintenance (CI, dépendances…) |

Exemples :
```
feat(commune-search): ajouter la recherche par code postal
fix(data-provider): corriger le parsing des champs vides RFC-4180
test(compliance-checker): ajouter les cas limites pour les plages min-max
```

## Tests

Le projet suit la méthode **TDD (Red → Green → Refactor)**. Tout nouveau code doit être couvert.

```bash
# Tous les tests
dotnet test

# Un projet spécifique
dotnet test tests/QualityWaterAlert.Core.Tests/
dotnet test tests/QualityWaterAlert.Infrastructure.Tests/

# Un test précis
dotnet test --filter "FullyQualifiedName~ComplianceCheckerTests"
```

Les tests doivent passer à 100 % avant d'ouvrir une PR. Le workflow CI vérifie cela automatiquement.

## Pull Requests

1. La branche doit être à jour avec `develop` (rebase ou merge)
2. Tous les tests CI doivent être verts
3. Le titre suit la même convention que les commits (`type(scope): ...`)
4. Décrire le *pourquoi* dans la description, pas le *quoi*
5. Au moins une review approbatrice avant le merge

## Architecture

Consulter [`CLAUDE.md`](CLAUDE.md) pour les détails sur la Clean Architecture, les couches et les conventions de code.
