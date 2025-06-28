# ATTMS Internal Roadmap

---

## Near-Term Improvements

- Finalize MaintenanceController for AdminUI-driven dictionary and list reloads
- Implement AdminUI Maintenance Form with Master Reload and Selective Reload capabilities
- Complete AdminUI Role-Based Access Control (RBAC) foundations

### 🧩 Branch: feature/rename-adminui-add-tickets-ui

#### Goal:
Rename `AdminUI/` to `WebUI/` to support a unified, role-driven interface for both users and administrators. Scaffold and begin development of the user-facing Ticket module.

#### TODOs:
- [ ] Rename `AdminUI/` folder to `WebUI/`
- [ ] Update all import paths and references (e.g., Vite config, TypeScript aliases, etc.)
- [ ] Create `WebUI/pages/tickets/` folder and add `TicketTable.jsx`
- [ ] Create API hook or service module under `WebUI/services/tickets/`
- [ ] Connect to backend endpoint `/api/v1/tickets`
- [ ] Add route for `/tickets` and ensure role-gated access
- [ ] Confirm successful render with ticket list MVP (ticket number, title, status, etc.)
- [ ] Commit and push with clear messages (e.g., `rename: AdminUI to WebUI`, `feat: scaffold ticket module`)

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
