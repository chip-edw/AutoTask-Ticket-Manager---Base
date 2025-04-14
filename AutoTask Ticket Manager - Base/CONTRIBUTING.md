# Contributing to AutoTask Ticket Manager Service (ATTMS)

Thank you for your interest in contributing! ATTMS is a modular, plugin-based background service built in .NET for managing support tickets from AutoTask and integrating with Microsoft 365 via MS Graph.

## Ways to Contribute

- **Reporting Bugs:** Found a problem? [Open an issue](https://github.com/YOUR_REPO_HERE/issues).
- **Submitting Features or Plugins:** You can submit enhancements or plugin jobs by following the coding and submission guidelines below.
- **Improving Documentation:** Spotted a typo or want to expand a section? PRs to `README.md` and related docs are welcome.

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022+
- SQLite
- An Azure App Registration with MS Graph permissions (for email features)
- An AutoTask API account with REST access

## Development Guidelines

- Fork the repository and clone your fork.
- Create a new branch from `main` for your changes.
- Use consistent naming for plugins and jobs (e.g., `MyCustomPlugin`, `GetOpenTicketTitlesJob`)
- Ensure all scheduler jobs implement the `ISchedulerJob` interface.
- Respect scoped service lifetimes. Always use `IServiceScopeFactory` or `CreateScope()` in plugins that access scoped dependencies.
- Follow Serilog conventions for logging: `Log.Information(...)`, `Log.Warning(...)`, etc.
- Add or update unit tests if applicable.
- Run tests and confirm clean build before PR.

## Commit & PR Etiquette

- Provide clear, descriptive commit messages.
- Reference issue numbers if applicable.
- Prefer small, focused commits.
- PR titles should be concise but descriptive.

## Licensing

By contributing, you agree that your contributions will be licensed under the same license as this project.
