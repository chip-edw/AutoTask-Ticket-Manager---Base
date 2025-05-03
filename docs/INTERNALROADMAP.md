# ATTMS Internal Roadmap

---

## Near-Term Improvements

- Finalize MaintenanceController for AdminUI-driven dictionary and list reloads
- Implement AdminUI Maintenance Form with Master Reload and Selective Reload capabilities
- Complete AdminUI Role-Based Access Control (RBAC) foundations

---

## Technical Debt and Future Refactoring

- **Refactor StartupLoaderService**:
  - Make all memory loading methods fully `async/await`.
  - Refactor `AutoTaskAPIGet.GetAutoTaskCompanies()`, `GetNotCompletedTickets()`, and `GetAutoTaskActiveResources()` to `async Task` versions.
  - Eliminate synchronous `.Result` or `.Wait()` blocking calls.
  - Implement `Task.WhenAll()` pattern for parallel asynchronous memory loading to improve startup performance.
- **General Async Improvements**:
  - Audit any other API or DB loading methods for unnecessary thread blocking.
  - Ensure `StartupConfiguration` loading methods remain efficient for both Startup and dynamic AdminUI-triggered reloads.

---

## Longer-Term Goals

- Plugin Framework for ATTMS: Hot-reloadable plugins for specialized scheduled jobs, maintenance tasks, and external integrations.
- Scheduled Dynamic Reloads: Implement scheduled background refresh of critical dictionaries and lists (e.g., nightly AutoTask sync).
- Database Migrations: Add automatic database migration logic for SQLite schema evolution at startup.
- Prepare for GitHub Open Source Launch: Final documentation polish, licensing decisions, and community guidelines.
- Refactor ExclusionService to use DI and implement IExclusionService

---
