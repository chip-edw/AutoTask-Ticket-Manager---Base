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
- [ ] Secure API Key Authentication Middleware
- [ ] Basic Ticket Listing and Dashboard UI
- [ ] Implement Management API endpoints:
  - [ ] `/api/scheduler/reload`
  - [x] `/api/email/check` (initial integration in Worker service)
  - [x] `/api/config/update` (via StartupConfiguration updates)
  - [ ] `/api/status/health`
- [ ] Lightweight Windows Desktop Client (initial interface)

---

### Phase 2: Beta Enhancements (v0.5)

Focus: Expanding functionality, improving collaboration, optional cloud backend.

- Full Ticket Management interface (Create, Update, Assign)
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
- Optional Salesforce CRM Basic Integration
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
