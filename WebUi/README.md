# ATTMS WebUI

WebUI is the lightweight, extensible front-end management portal for the AutoTask Ticket Manager Service (ATTMS) platform.

Built using [React](https://react.dev/), [Vite](https://vitejs.dev/), and [Material-UI (MUI)](https://mui.com/), WebUI provides a fast, simple, cross-platform interface to interact with ATTMS backend services.

## ✨ Features

- ⚡ Powered by React 18 and Vite for lightning-fast development and builds
- 🎨 Styled with Material-UI (MUI) components and Tailwind CSS for a clean, professional look
- 🔒 Basic simulated login system with protected routing
- 📋 Full CRUD operations for AutoTask ticket management
- 🔗 Real-time integration with AutoTask API for metadata (statuses, queues, priorities)
- ✏️ Create and Edit ticket forms with validation and AutoTask picklist dropdowns
- 📊 Ticket list view with Material-UI DataGrid
- 🛠️ Integration with ATTMS backend APIs
- 📦 Ready for static deployment and local download via ATTMS Maintenance API

## 📂 Project Structure

```plaintext
WebUI/
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
- ATTMS backend service running (typically on localhost:60050)

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
  Login is currently simulated. Real backend authentication integration is planned.
- **Ticket Management:**  
  Full CRUD operations implemented with AutoTask API integration:

  - ✅ Create new tickets with AutoTask metadata validation
  - ✅ View tickets in paginated DataGrid
  - ✅ Edit tickets with real-time AutoTask picklist data
  - ✅ PATCH-based updates for efficient ticket modifications

- **API Integration:**  
  Connected to ATTMS backend APIs at `/api/v1/tickets/*` endpoints:

  - GET `/tickets` - Retrieve ticket list
  - POST `/tickets` - Create new tickets
  - PATCH `/tickets/{id}` - Update existing tickets
  - GET `/tickets/metadata` - Fetch AutoTask picklists (statuses, queues, priorities)

- **Planned Enhancements:**
  - Add AutoTask Resources for ticket assignment
  - Implement real authentication with ATTMS backend
  - Add CRM Account management functionality
  - Enhance error handling and session management
  - Implement localStorage-based session persistence

## 🛠️ Environment Variables

To enable dynamic configuration across environments (development, production, etc.), the ATTMS WebUI uses the following environment variable:

```
VITE_API_BASE_URL=http://localhost:60050/api/v1
```

### Where to Define It

Create a `.env.local` file in the root of your WebUI project and add the variable there:

```
# .env.local
VITE_API_BASE_URL=http://localhost:60050/api/v1
```

⚠️ **Important:** All environment variables intended for use in the frontend **must start with** `VITE_` to be exposed to your React app when using Vite.

🔒 **Best Practice:** Add `.env.local` to your `.gitignore` to prevent accidentally committing environment-specific values to version control.

---

### Example `.gitignore` entry

```
# Local environment config
.env.local
```

## 📄 License

WebUI is part of the ATTMS platform and is licensed under the same terms. See the [LICENSE](../LICENSE) file in the root directory for details.

## 🙌 Acknowledgements

- [React](https://react.dev/)
- [Vite](https://vitejs.dev/)
- [Material-UI](https://mui.com/)
- [Tailwind CSS](https://tailwindcss.com/)
