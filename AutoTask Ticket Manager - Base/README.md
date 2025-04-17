# AutoTask Ticket Manager Service (ATTMS)

AutoTask Ticket Manager Service (ATTMS) is a modular .NET 8 worker service designed to streamline support operations by integrating email, scheduling, and ticket lifecycle management for AutoTask PSA users.

---

## 🚀 Overview

This application is a complete refactor of a system I wrote currently used in production at an ISV. The refactored version embraces modern .NET best practices with:

- **Scoped dependency injection**
- **Multi-threaded background scheduling**
- **Plugin architecture for extensibility**
- **Serilog-based structured logging**
- **SQLite-backed persistence**

It is being actively developed with the goal of becoming an open-source tool, enabling others to extend functionality by writing plugins without altering base code.

---

## 🔧 Core Features

### ✅ Email-to-Ticket Automation

- Integrates with Microsoft 365 using the **Microsoft Graph API**
- Parses inbound emails to create/update tickets in AutoTask
- Handles automatic assignment rules and sender exclusions
- Built on a registered Azure App with secure token acquisition
- Configurable using the `StartupConfiguration` dictionary

### ⏲️ Background Scheduling

- Runs all scheduled jobs from a dedicated **Scheduler plugin**
- Each job inherits from `ISchedulerJob` and defines its own execution logic
- Supports run frequency and activation status per job
- Jobs are defined in the database and dynamically executed using reflection
- Plugin runs on an independent thread for separation of concerns

### 🔌 Plugin Architecture

- One core **scheduler plugin** currently enabled
- Easily extended to support other plugin types in the future
- Plugins are dropped into a designated folder and loaded dynamically at runtime
- Uses `ServiceActivator.ServiceProvider` to resolve scoped dependencies

### 📝 Ticket Management

- Integrates with AutoTask APIs for querying, creating, and updating tickets
- Scheduled jobs can load ticket data, assign technicians, and apply business logic
- Maintains in-memory dictionaries for open tickets, companies, and exclusions

### 📬 Email Integration (Microsoft Graph API)

- Uses **Microsoft Graph API** to communicate securely with Exchange Online
- Registered application in Azure where OAuth scopes and security are managed
- Supports sending alert emails, parsing support inboxes, and scheduling enhancements
- Paves the way for future Microsoft 365 integrations like:
  - Microsoft Teams ticket notifications
  - Tasks and To-Dos linked to AutoTask tickets
  - Scheduled reminders and calendar syncing

---

## 🗂️ Configuration

Settings are stored in a SQLite database (`ATTMS.db`) and loaded into memory using `StartupConfiguration`. Sensitive settings like Graph API credentials are stored in a protected key-value store and accessed via:

```csharp
StartupConfiguration.GetProtectedSetting("ClientSecret");
```
---

## 📅 Roadmap & To-Do

Planned enhancements and upcoming features for ATTMS:

- [ ] 🔐 Implement API Key or JWT-based authentication for internal maintenance API
- [ ] 🌐 Add IP allowlisting or internal-only hosting for extra API security
- [ ] 📁 Create structured endpoints for:
  - [ ] `/api/scheduler/reload`
  - [ ] `/api/email/check`
  - [ ] `/api/config/update`
  - [ ] `/api/status/health`
- [ ] 💬 Integrate Microsoft Graph for Teams chat updates around ticket workflows
- [ ] ✅ Add Graph API functionality for Tasks and To-Dos related to AutoTask tickets
- [ ] 🖥️ Develop lightweight Windows Desktop Client to interact with internal API
- [ ] 🔄 Use HttpClientFactory (and optionally Refit) for robust API client design in desktop app
- [ ] 📦 Add versioning support to the API: `/api/v1/...`
