# ATTMS: AutoTask Ticket Manager Service & Lightweight ITSM Platform

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

### Phase 1: MVP Release (v0.1)

- [x] Secure Setup API (localhost-only protection)
- [x] Encrypted Key Generation and Secrets Management
- [ ] Secure API Key Authentication Middleware
- [ ] Ticket Listing and Basic Dashboard UI
- [ ] Management API endpoints:
  - [ ] `/api/scheduler/reload`
  - [ ] `/api/email/check`
  - [ ] `/api/config/update`
  - [ ] `/api/status/health`
- [ ] Lightweight Windows Desktop Client (Phase 1 UI)

### Phase 2: Beta Enhancements (v0.5)

- Full Ticket Management interface (Create/Update/Assign)
- Opportunity Dashboard (simplified CRM overlay)
- Microsoft Teams Chat Updates for ticket changes
- Microsoft Planner Task creation from AutoTask tickets
- Optional PostgreSQL or Azure SQL backend support

### Phase 3: Open Source Launch (v1.0+)

- Publish Docker Images and install scripts
- Optional Salesforce CRM basic integration
- Public Contribution Guidelines and Plugin SDK
- Cloud/SaaS self-hosted deployment instructions
- Professional-grade API versioning (`/api/v1/...`)

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

Planned for **MIT License** upon open-source release.  
Commercial consulting, deployment, and plugin customization services may be available separately.

---

# 📚 Documentation

These documents provide deeper insights into ATTMS internals:

- [Email Processing Flow](docs/Email-Processing.md)
- [Admin Maintenance Checklist](docs/Admin-Maintenance-Checklist.md)

---

# 🚀 ATTMS — Modernize AutoTask Support and CRM Workflows

> *"Because your ITSM platform should work for you, not the other way around."*