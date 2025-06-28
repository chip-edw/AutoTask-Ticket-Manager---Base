# ATTMS WebUI - Roadmap

This document outlines the planned development milestones and enhancements for the ATTMS WebUI project.

## 🎯 MVP (Minimum Viable Product) Goals

- [x] Set up project using Vite, React, and Material-UI
- [x] Implement simulated login with basic authentication flow
- [x] Add protected routes (PrivateRoute)
- [x] Create Sidebar and TopBar components
- [x] Scaffold core pages: Home, Dashboard, Ticket List, Account List
- [x] **✅ COMPLETED:** Implement API services for Tickets with full CRUD operations
- [x] **✅ COMPLETED:** Display Tickets in a Material-UI DataGrid on TicketList page
- [x] **✅ COMPLETED:** Implement Create Ticket functionality with form validation
- [x] **✅ COMPLETED:** Implement Edit Ticket functionality with AutoTask picklist integration
- [x] **✅ COMPLETED:** Backend integration with AutoTask API for real metadata (statuses, queues, priorities)
- [x] **✅ COMPLETED:** PATCH-based ticket updates for efficient modifications
- [x] **✅ COMPLETED:** Basic error handling for failed API calls
- [ ] Display Accounts in a Material-UI DataGrid on AccountList page
- [ ] Static WebUI build for deployment via ATTMS Maintenance API

## 🔄 Current Sprint: BASIC CRUD Completion

**Sprint Goal:** Complete full ticket CRUD operations with AutoTask integration

- [x] **✅ COMPLETED:** Edit Ticket form with real AutoTask picklists
- [ ] **🔄 IN PROGRESS:** Fix Create New Ticket form to use real metadata (remove hardcoded values)
- [ ] **🔄 IN PROGRESS:** Add AutoTask Resources to backend metadata endpoint
- [ ] **🔄 IN PROGRESS:** Add Resource assignment dropdown to Create and Edit forms
- [ ] **📋 PLANNED:** Testing and validation of complete CRUD workflow

## 🔥 Post-MVP Enhancements

- [ ] Implement real authentication with ATTMS backend (token-based)
- [ ] Session persistence via localStorage
- [ ] Add loading indicators and error boundary components
- [ ] Implement pagination and search for Ticket lists
- [ ] Advanced filtering and sorting for ticket management
- [ ] Create basic Settings page for Admin preferences
- [ ] Add Delete ticket functionality with confirmation dialogs
- [ ] Bulk operations for ticket management

## 🚀 Future Phase (Version 2+)

- [ ] Expand user management (create/edit WebUI users)
- [ ] Push notifications for ticket updates (if feasible)
- [ ] Add support for custom CRM entity linking
- [ ] Complete CRM Account management (Create, Read, Update, Delete)
- [ ] Theme customization (light/dark modes)
- [ ] Internationalization (i18n) support
- [ ] Advanced reporting and analytics dashboard
- [ ] Integration with additional AutoTask entities (Projects, Time Entries, etc.)

## 🛠 Technical Improvements (Optional)

- [ ] Migrate to TypeScript for improved type safety
- [ ] Implement unit and integration tests (Jest + React Testing Library)
- [ ] Set up CI/CD pipeline for build/test automation
- [ ] Performance optimization and code splitting
- [ ] Enhanced accessibility (WCAG compliance)
- [ ] Progressive Web App (PWA) capabilities

## 🏗️ Architecture Notes

**Current Tech Stack:**

- **Frontend:** React 18 + Vite + Material-UI + Tailwind CSS
- **Backend Integration:** RESTful APIs at `/api/v1/tickets/*`
- **Data Flow:** React forms → API calls → AutoTask integration
- **State Management:** React hooks (useState, useEffect)

**API Endpoints Implemented:**

- `GET /api/v1/tickets` - Retrieve ticket list
- `POST /api/v1/tickets` - Create new tickets
- `PATCH /api/v1/tickets/{id}` - Update existing tickets
- `GET /api/v1/tickets/metadata` - Fetch AutoTask picklists

---

✅ **Current Status:** Core ticket CRUD operations are functional with AutoTask integration. Focus is on completing the remaining BASIC CRUD sprint items before moving to post-MVP enhancements.

🎯 **Next Milestone:** Complete AutoTask Resources integration and finalize Create form improvements to achieve full CRUD functionality.
