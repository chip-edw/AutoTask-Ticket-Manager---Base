# ATTMS Project Roadmap

---

## 🛣️ Overview

This document outlines the planned development phases for the ATTMS (AutoTask Ticket Manager Service) project.

ATTMS is evolving from a self-taught C# project into a modern, extensible open-source ITSM platform — built around AutoTask PSA, Microsoft Graph API, and future integrations with Salesforce.

---

## 📅 Development Phases

---

### Phase 1: MVP Release (v0.1)

Focus: Core functionality to manage setup, encryption, and ticket visibility.

- [x] Secure Setup API with localhost-only access
- [x] Encrypted Key Generation and Secrets Management
- [x] ServiceCollectionExtensions.cs created for clean DI registration
- [x] Plugin architecture implemented (Scheduler Plugin)
- [x] Initial scheduler job (`CheckEmail`) wired and operational
- [x] Serilog logging configured and integrated
- [x] SQLite local storage + EF Core with migrations
- [x] Scoped DI wired into EmailManager, Scheduler, and Graph services
- [x] StartupConfiguration memory model for exclusions (runtime performance design)
- [x] AutoTask API integration scaffolding (Ticket creation, Role ID filtering)
- [x] `MaintenanceController` created using best practices
- [x] Company Settings Admin UI completed
- [x] Subject Exclusion Keywords model, DTO, service implemented

**🔐 Management API**
- [ ] Secure API Key Authentication Middleware
- [ ] Implement Management API endpoints:
  - [ ] `/api/scheduler/reload`
  - [x] `/api/email/check` (initial integration in Worker service)
  - [x] `/api/config/update` (via StartupConfiguration updates)
  - [ ] `/api/status/health`

**🧾 Ticket UI & Dashboard**
- [x]  Full Ticket CRUD API (`/api/v1/tickets/*`)
- [x]  Ticket Listing UI (sortable table, live API integration)
- [x]  Ticket Detail View with Edit functionality
- [x]  Real-time AutoTask metadata integration (statuses, queues, priorities)
- [x]  Manual Ticket Creation via WebUI forms
- [x]  Ticket Update with PATCH-based efficient updates
- [x]  AutoTask picklist integration for consistent dropdowns
- [ ] 📋 **PLANNED:** Ticket Filtering (by queueName, status, companyName)
- [ ] 📋 **PLANNED:** Basic Dashboard Metrics (ticket count summaries, trend indicators)


**🖥️ Client Integration**
- [ ] Lightweight Windows Desktop Client (initial interface)

---

### 🔄 Current Sprint: BASIC CRUD Completion

**Sprint Goal:** Complete full ticket CRUD operations with AutoTask integration

- [x] ✅ **COMPLETED:** Edit Ticket form with real AutoTask picklists
- [ ] 🔄 **IN PROGRESS:** Fix Create New Ticket form to use real metadata (remove hardcoded values)
- [ ] 🔄 **IN PROGRESS:** Add AutoTask Resources to backend metadata endpoint
- [ ] 🔄 **IN PROGRESS:** Add Resource assignment dropdown to Create and Edit forms
- [ ] 📋 **PLANNED:** Testing and validation of complete CRUD workflow


---

### Phase 2: Beta Enhancements (v0.5)

Focus: Expanding functionality, improving collaboration, optional cloud backend.

- Full Ticket Management interface (Create, Update, Assign, bulk operations, advanced filtering)
- Opportunity Dashboard (CRM light overlay)
- Microsoft Teams Chat Updates for Ticket/Opportunity Changes
- Microsoft Planner Task Auto-Creation from Tickets
- Optional PostgreSQL or Azure SQL backend support
- API Hardening (better error handling, retries, resiliency)
- Sender Exclusions Admin UI (in progress)
- Auto-Assign Maintenance Screen (planned)

---

### Phase 3: Open Source Launch (v1.0+)

Focus: Public visibility, contribution readiness, cross-platform polish.

- Publish Official Docker Image and Deployment Scripts
- Public Contribution Guidelines (CONTRIBUTING.md)
- Plugin SDK for extending ATTMS capabilities
- Self-hosted Deployment Documentation (Azure, AWS, Linux/Windows)
- Professional-grade API Versioning (`/api/v1/...`, `/api/v2/...`)

---

#### 🔧 Planned Refactors (Post v1.0 → Target: v1.1)

**ExclusionService Refactor — Static to DI**

- Replace static `ExclusionService` with DI-compliant runtime exclusion filtering

**Context:**  
Currently, `ExclusionService` provides static access to `StartupConfiguration.senderExclusionsList` and `subjectExclusionKeyWordList` for use in the runtime email pipeline (e.g., `EmailManager`, `TicketHandler`).  
This design was adopted to avoid a large-scale refactor due to static method usage in core message processing components.

**Why It's Needed:**  
- Improve testability and mockability of runtime filtering logic  
- Eliminate shared global state where possible (`StartupConfiguration`)  
- Align with modern DI-based architecture already used in Admin UI and Maintenance APIs  
- Allow dynamic hot-reloading of exclusions without service restart  

**Tasks:**  
- [ ] Refactor `EmailManager.CheckEmail()` and related consumers to be instance-based  
- [ ] Inject `IExclusionService` via DI in all relevant message-handling services  
- [ ] Migrate `StartupConfiguration.subjectExclusionKeyWordList` and `senderExclusionsList` to an in-memory cache or runtime reload pattern  
- [ ] Retire static `ExclusionService.cs` (or mark legacy-only)  

**Risks:**  
- Static-to-instance refactor may cascade into multiple components (EmailManager, ContentProcessor, TicketHandler)  
- Needs careful coordination with message queue / plugin scheduler if tightly coupled

**Target Release:** `v1.1`  
**Status:** ⏳ Deferred — design accepted, blocked by static dependencies in runtime services  
**Tags:** `[Refactor]`, `[TechDebt]`, `[StartupConfiguration]`, `[EmailPipeline]`

---

#### 🔐 Planned Infrastructure Enhancements (v1.1+)

**Rate Limiting Middleware**

Introduce request rate limiting to prevent abuse and allow for eventual SaaS-ready controls.

**Why It's Needed:**  
- Protect the API from being overwhelmed by bad actors or runaway clients  
- Enable future tenant-specific quotas or developer plans  
- Improve resilience and uptime under load  

**Implementation Options:**  
- Use `AspNetCoreRateLimit` NuGet package for IP-based throttling  
- Consider policy-based limits (per route, per key, etc.)  
- Future enhancement: tie into API key system when multi-tenant

**Tasks:**  
- [ ] Add NuGet package and base configuration for IP/route limiting  
- [ ] Integrate into ASP.NET middleware pipeline  
- [ ] Expose `/api/status/limits` (optional, debug-use)  
- [ ] Document usage and customization in AdminDocs  

**Target Release:** `v1.1`  
**Status:** 🧭 Planned  
**Tags:** `[Security]`, `[Middleware]`, `[Scalability]`, `[RateLimiting]`



---

#### 🔌 Planned Plugin Modules (v1.1+ and Beyond)

**Pluggable Auto-Assign Rules Engine**

- Introduce a flexible plugin interface for defining auto-assignment rules
- Allows organizations to customize how tickets are assigned based on sender, company, priority, subject, etc.

**Interface Design (planned):**
```csharp
public interface IAutoAssignPlugin
{
    int? ResolveAssigneeId(TicketContext context);
}

```

---

**🧩 Confluence Sync Plugin**

- Automatically update Confluence customer dashboards or wiki pages when tickets are created or modified
- Target use case: Account Managers reviewing customer activity in real time
- Mapping logic can link AutoTask Companies to Confluence Spaces and Pages
- Triggered by ticket status changes, scheduler jobs, or webhook-style events
- **Plugin Scope:** Runs independently, integrates via ATTMS plugin contract

---

**📣 Microsoft Teams Notification Plugin**

- Sends updates to designated Teams channels when tickets are created, assigned, escalated, or resolved
- Integrates with Microsoft Graph API to support @mentions and rich card formatting
- Maps AutoTask Queues, Priorities, or Companies to Teams groups or channels
- Use cases include:
  - Real-time support team notifications
  - Escalation alerts to leadership
  - Assignment alerts to individual technicians
- **Plugin Scope:** Fully isolated module, can be enabled/disabled per environment

---

## 🏗️ Architecture Achievements

**Current Tech Stack:**
- **Backend:** .NET 8 + Entity Framework Core + SQLite
- **Frontend:** React 18 + Vite + Material-UI + Tailwind CSS
- **API Integration:** AutoTask REST API + Microsoft Graph API
- **Logging:** Serilog with structured logging

**API Architecture:**
- RESTful design with proper HTTP status codes
- PATCH-based updates for efficient data transfer
- Real-time metadata loading from AutoTask
- Proper error handling and logging throughout
- Dependency injection and service layer pattern

**Data Flow:**
- React forms → API calls → AutoTask integration → Real-time UI updates
- In-memory caching for performance (Companies, Picklists)
- Efficient partial updates using PATCH methodology

---

## 🎯 Current Status Summary

**✅ Major Achievements:**
- Complete ticket CRUD operations with AutoTask integration
- Modern React-based WebUI with Material-UI components
- Real-time AutoTask metadata integration
- Efficient PATCH-based update system
- Comprehensive API documentation and error handling

**🔄 Current Focus:**
- Completing AutoTask Resources integration
- Finalizing Create form improvements
- Polishing CRUD workflow for production readiness

**📋 Next Priority:**
- Advanced filtering and search capabilities
- Dashboard metrics and reporting
- Plugin framework for extensibility

---

✅ **MVP Status:** Core ticket management functionality is complete and production-ready. 
Focus has shifted to enhancing user experience and preparing for broader deployment.