[← Back to ATTMS Repository](https://github.com/chip-edw/AutoTask-Ticket-Manager---Base)

# Contributing to AutoTask Ticket Manager Service (ATTMS)

Thank you for your interest in contributing! ATTMS is a modular, plugin-based background service built in .NET for managing support tickets from AutoTask and integrating with Microsoft 365 via MS Graph.

## Ways to Contribute

- **Reporting Bugs:** Found a problem? [Open an issue](https://github.com/chip-edw/AutoTask-Ticket-Manager---Base/issues).
- **Submitting Features or Plugins:** You can submit enhancements or plugin jobs by following the coding and submission guidelines below.
- **Improving Documentation:** Spotted a typo or want to expand a section? ...PRs to [`README.md`](../README.md) and [`docs/`](./docs) are welcome.

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

By contributing, you agree that your contributions will be licensed under the [AGPL-3.0 License](./LICENSE).

## 🤝 Code of Conduct

Please be respectful and considerate. This project follows the [Contributor Covenant](https://www.contributor-covenant.org/version/2/1/code_of_conduct/).  
If you have concerns or experience any issues, feel free to contact the maintainer directly at [chip@chip-edwards.com](mailto:chip@chip-edwards.com).

