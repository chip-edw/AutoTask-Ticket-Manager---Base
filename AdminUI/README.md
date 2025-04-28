# ATTMS AdminUI

AdminUI is the lightweight, extensible front-end management portal for the AutoTask Ticket Manager Service (ATTMS) platform.

Built using [React](https://react.dev/), [Vite](https://vitejs.dev/), and [Material-UI (MUI)](https://mui.com/), AdminUI provides a fast, simple, cross-platform interface to interact with ATTMS backend services.

## ✨ Features

- ⚡ Powered by React 18 and Vite for lightning-fast development and builds
- 🎨 Styled with Material-UI (MUI) components for a clean, professional look
- 🔒 Basic simulated login system with protected routing
- 📋 Pages for Dashboard, Ticket List, and CRM Account List
- 🛠️ Planned integration with ATTMS Maintenance APIs and Worker Service APIs
- 📦 Ready for static deployment and local download via ATTMS Maintenance API

## 📂 Project Structure

```plaintext
AdminUI/
├── public/
├── src/
│   ├── api/              # API services (auth, tickets, accounts)
│   ├── assets/           # Static assets (e.g., logos)
│   ├── components/       # Shared UI components (Sidebar, TopBar, PrivateRoute)
│   ├── pages/            # Application pages (Dashboard, Tickets, Accounts, Login)
│   ├── App.jsx           # Main app component with routes
│   ├── main.jsx          # App entry point
│   ├── App.css           # App-level styling
│   └── index.css         # Global styles
├── .vscode/              # VSCode workspace settings (optional but recommended)
├── package.json
├── vite.config.js
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- [Node.js](https://nodejs.org/) (v18+ recommended)
- [VS Code](https://code.visualstudio.com/) with ESLint and Prettier extensions (recommended)

### Install Dependencies

```
npm install
```

### Start Development Server

```
npm run dev
```

Visit: [http://localhost:5173](http://localhost:5173)

### Build for Production

```
npm run build
```

The production-ready static files will be output to the `/dist` folder.  
These files can later be served locally via the ATTMS Maintenance API.

## ⚙️ Development Notes

- **Authentication:**  
  Login is currently simulated. No real backend authentication is yet implemented.
- **API Integration:**  
  Only authentication API (`authApi.js`) is scaffolded.  
  Ticket and Account API services are planned but not yet wired.

- **Planned Enhancements:**
  - Implement real login with ATTMS Maintenance API
  - Wire Ticket List and Account List pages to real APIs
  - Add pagination, filtering, and search functionality
  - Enhance error handling and session management
  - Implement localStorage-based session persistence

## 📄 License

AdminUI is part of the ATTMS platform and is intended for open-source release under the MIT License (pending).

## 🙌 Acknowledgements

- [React](https://react.dev/)
- [Vite](https://vitejs.dev/)
- [Material-UI](https://mui.com/)
