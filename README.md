# ATTMS: AutoTask Ticket Manager Service & Lightweight ITSM Platform
[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)

---

## 📚 Overview

**ATTMS** (AutoTask Ticket Manager Service) is an open-source platform designed to enhance the support and CRM workflows for AutoTask PSA users.

**Originally developed as a self-taught C# project**, ATTMS was a completely new approach inspired by earlier work using a Microsoft Outlook VSTO plugin. It provided an opportunity to learn the Microsoft Graph API and develop a more scalable, service-based system for AutoTask integration.  
Now, ATTMS has been fully refactored to embrace **modern .NET 8 best practices**, building a solid, extensible foundation for ITSM and CRM workflows.

**ATTMS is cross-platform**, running on both Windows and Linux environments.

---

## 🚀 Core Objectives

- Provide a **modern lightweight desktop and web client** for managing AutoTask tickets, customers, and opportunities.
- Solve **AutoTask CRM limitations** through a clean, customizable presentation layer.
- Leverage existing **Microsoft 365 investments** (Teams, Planner, Outlook) for internal collaboration and notifications.
- **Minimize storage** overhead by using AutoTask and Microsoft 365 as primary systems of record.
- Build a **plugin-extensible**, **cross-platform** ITSM enhancement solution.

---

## 🔧 Core Components

### ✅ Worker Service Foundation

- Built on **.NET 8** with **Scoped Dependency Injection** throughout.
- **Modular Plugin Architecture** allowing runtime extensibility:
  - Plugins can be dropped into a designated folder and dynamically loaded.
  - Scheduler plugin manages background jobs independently from the core service.
- **Multi-threaded Background Scheduling**:
  - Jobs inherit from `ISchedulerJob`.
  - Jobs execute based on individual schedules stored in SQLite.
- **Structured Logging** using Serilog:
  - Detailed operational logging for visibility and diagnostics.

### ✅ Email-to-Ticket Automation

- Integrates with Microsoft 365 using the **Microsoft Graph API**.
- Parses inbound emails to create/update AutoTask tickets.
- Applies sender exclusions and automatic assignment rules.
- Future planned integration with Teams, Planner, and Microsoft To-Do for ticket-based collaboration.

### ✅ Ticket and Opportunity Management

- Full AutoTask API integration for Tickets, Companies, Contacts, and Opportunities.
- Future enhancements will allow lightweight opportunity and CRM workflows on top of AutoTask data.

### ✅ Internal API (Management API)

- Lightweight internal maintenance API secured by:
  - API Key Authentication (with future Key Vault optionality)
  - IP restrictions (localhost-only Setup APIs)
- Exposes endpoints for Scheduler control, health checks, and ticket management.
- Designed for safe automation and future desktop app integrations.

---

## 🗂️ Setup and Configuration

- **Persistence**: SQLite database (`ATTMS.db`) used for lightweight storage of schedules, config, and plugin metadata.
- **Secrets Management**:
  - Local `keys.json` for encrypted API keys during development.
  - Future Azure Key Vault support planned for production environments.
- **StartupConfiguration** Service:
  - Centralized access for protected app settings.
  - Example usage:
    ```csharp
    StartupConfiguration.GetProtectedSetting("ClientSecret");
    ```

### 🎨 Frontend Styling with Tailwind CSS

ATTMS uses **Tailwind CSS v3.4.3** for its Admin UI components.

> ⚠️ Note: Tailwind v4 introduced a new PostCSS plugin format that caused color and utility classes to be omitted during Vite builds in our JSX-based setup.  
> We are intentionally using Tailwind v3.4.3 for stability during MVP development.

See [`docs/dev/tailwind.md`](./docs/dev/tailwind.md) for configuration details.


 ### 🧠 Company Cache (`Companies.companies`)

ATTMS maintains an in-memory observable dictionary of AutoTask companies using the `Companies.companies` structure.  
This cache is loaded at application startup or refreshed manually via the Maintenance API.

It is used to enrich ticket data with company names in the Ticket Management screen.

> ⚠️ If a new company is created in AutoTask and is assigned to a ticket,
> it will **not appear correctly in the UI** until the Maintenance task is rerun to refresh the cache.

This design avoids unnecessary database lookups and keeps runtime processing fast and lightweight.

To view the current dictionary, use the internal Management API or UI tools under the **Company Settings** screen.


---

## 📦 Storage Strategy

| Data | Storage Location |
|:-----|:-----------------|
| Tickets, Contacts, Accounts | AutoTask PSA (via API) |
| Emails, Tasks, Teams Chats | Microsoft 365 (via MS Graph) |
| Configuration, Plugins, Schedules | Local SQLite Database |
| Attachments (future optional) | Microsoft 365 OneDrive/SharePoint APIs |

✅ ATTMS reduces infrastructure costs by **leveraging paid-for systems** instead of duplicating storage.

---

## 🌐 Cross-Platform Support

- Windows and Linux deployment support
- No Windows-specific services or hardcoded paths (uses `Path.Combine`, platform-agnostic APIs)
- Designed to run on Docker containers, Azure App Services, or local VM installs
- Certificates, file handling, and process management written to handle both OS families gracefully

---

## 🗺️ Roadmap

## 🗺️ Roadmap

ATTMS is developed in phased iterations:

### 🚧 MVP Release (v0.1)
- Secure setup and configuration APIs
- Microsoft Graph email ingestion + AutoTask ticket creation
- Modular plugin scheduler
- SQLite-based runtime config and job definitions
- Admin Maintenance UI for Company Settings, Exclusions

### 🔄 What’s Next?
- Full ticket listing and assignment UI
- Auto-assign plugin framework (deferred from MVP)
- Internal health monitoring and service reload endpoints

👉 View the full development plan in [`ROADMAP.md`](./ROADMAP.md)


---

## 📈 Future Vision

**ATTMS** aims to become the go-to open-source bridge between  
AutoTask PSA, Microsoft 365, and lightweight CRM/ITSM needs,  
offering companies a flexible, low-cost enhancement to their existing systems.

**Focus Areas:**

- CRM Light Enhancements
- Support Desk Streamlining
- Cross-Platform Worker Service
- Plugin Market Ecosystem

---

## 📜 License

ATTMS is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

You are free to:

- Use, modify, and deploy this software
- Share changes, provided you also share your source code under the same license
- Run ATTMS internally or on a public server

You may **not**:

- Re-license or sell the software without also open-sourcing your modifications
- Use this code in proprietary software without obtaining a separate commercial license

For full license terms, see the [LICENSE](./LICENSE) file.

---

💼 **Need help customizing or deploying ATTMS?**  
I offer consulting services and implementation support.  
Reach out at [chip@chip-edwards.com](mailto:chip@chip-edwards.com).


# 📚 Documentation

These documents provide deeper insights into ATTMS internals:

- [Email Processing Flow](docs/Email-Processing.md)
- [Admin Maintenance Checklist](docs/Admin-Maintenance-Checklist.md)

---

# 🚀 ATTMS — Modernize AutoTask Support and CRM Workflows

> *"Because your ITSM platform should work for you, not the other way around."*