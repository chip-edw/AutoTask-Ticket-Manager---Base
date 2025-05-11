# ATTMS AdminUI - Roadmap

This document outlines the planned development milestones and enhancements for the ATTMS AdminUI project.

## 🎯 MVP (Minimum Viable Product) Goals

- [x] Set up project using Vite, React, and Material-UI
- [x] Implement simulated login with basic authentication flow
- [x] Add protected routes (PrivateRoute)
- [x] Create Sidebar and TopBar components
- [x] Scaffold core pages: Home, Dashboard, Ticket List, Account List
- [ ] Implement API services for Tickets and Accounts
- [ ] Display Tickets in a Material-UI DataGrid on TicketList page
- [ ] Display Accounts in a Material-UI DataGrid on AccountList page
- [ ] Basic error handling for failed API calls
- [ ] Static AdminUI build for deployment via ATTMS Maintenance API

## 🔥 Post-MVP Enhancements

- [ ] Implement real authentication with ATTMS Maintenance API (token-based)
- [ ] Session persistence via localStorage
- [ ] Add loading indicators and error boundary components
- [ ] Implement pagination and search for Ticket and Account lists
- [ ] Create basic Settings page for Admin preferences

## 🚀 Future Phase (Version 2+)

- [ ] Expand user management (create/edit AdminUI users)
- [ ] Push notifications for ticket updates (if feasible)
- [ ] Add support for custom CRM entity linking
- [ ] Theme customization (light/dark modes)
- [ ] Internationalization (i18n) support

## 🛠 Technical Improvements (Optional)

- [ ] Migrate to TypeScript for improved type safety
- [ ] Implement unit and integration tests (Jest + React Testing Library)
- [ ] Set up CI/CD pipeline for build/test automation

---

✅ MVP is targeted to provide a lightweight, fast Admin UI for ATTMS core support operations.
Further phases will focus on real-time collaboration, customization, and production hardening.
