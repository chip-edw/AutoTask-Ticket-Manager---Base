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
- [ ] Secure API Key Authentication Middleware
- [ ] Basic Ticket Listing and Dashboard UI
- [ ] Implement Management API endpoints:
  - [ ] `/api/scheduler/reload`
  - [ ] `/api/email/check`
  - [ ] `/api/config/update`
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

## 🏆 Future Vision

- CRM Light Enhancements for Opportunities/Contacts management
- Full Multi-Tenant SaaS Deployment Mode (optional for hosted ATTMS)
- Dedicated Plugin Marketplace for third-party developers
- Full Mobile Support (ATTMS mobile client or PWA)

---

# 🚀 ATTMS — Build the Future of Lightweight Support and CRM

---
