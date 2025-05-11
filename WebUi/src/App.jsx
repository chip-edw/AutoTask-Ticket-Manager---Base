import TicketPanel from './pages/tickets/TicketPanel';
import TicketTable from './pages/tickets/TicketTable';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Box, Toolbar } from '@mui/material';

import Sidebar from './components/Sidebar';
import TopBar from './components/TopBar';
import PrivateRoute from './components/PrivateRoute';

import Home from './pages/Home';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import TicketList from './pages/TicketList';
import AccountList from './pages/AccountList';

// Maintenance Panel components
import MaintenancePanel from './components/maintenance/MaintenancePanel';
import CompanySettings from './components/maintenance/CompanySettings';
import SystemStats from './components/maintenance/SystemStats';
import SubjectExclusions from './components/maintenance/SubjectExclusions';
import SenderExclusions from './components/maintenance/SenderExclusions';
import ReloadPanel from './components/maintenance/ReloadPanel';

function App() {
  return (
    <Router>
      <Box sx={{ display: 'flex' }}>
        <TopBar />
        <Sidebar />
        <Box component="main" sx={{ flexGrow: 1, p: 3, ml: '240px' }}>
          <Toolbar />
          <Routes>
            {/* Public route */}
            <Route path="/login" element={<Login />} />

            {/* Protected routes */}
            <Route path="/" element={
              <PrivateRoute>
                <Home />
              </PrivateRoute>
            } />

            <Route path="/admin/dashboard" element={
              <PrivateRoute requiredRole="admin">
                <Dashboard />
              </PrivateRoute>
            } />

            <Route path="/tickets/open" element={
              <PrivateRoute>
                <TicketList />
              </PrivateRoute>
            } />

            <Route path="/tickets" element={<TicketPanel />}>
              <Route index element={<TicketTable />} />
              {/* Future options: */}
              {/* <Route path="closed" element={<ClosedTicketTable />} /> */}
              {/* <Route path=":ticketNumber" element={<TicketDetails />} /> */}
            </Route>

            <Route path="/crm/accounts" element={
              <PrivateRoute>
                <AccountList />
              </PrivateRoute>
            } />

            {/* Maintenance Panel (Admin-only nested routes) */}
            <Route element={<PrivateRoute requiredRole="admin" />}>
              <Route path="/admin/maintenance" element={<MaintenancePanel />}>
                <Route path="company-settings" element={<CompanySettings />} />
                <Route path="system-stats" element={<SystemStats />} />
                <Route path="subject-exclusions" element={<SubjectExclusions />} />
                <Route path="sender-exclusions" element={<SenderExclusions />} />
                <Route path="reload" element={<ReloadPanel />} />
              </Route>
            </Route>


            {/* Fallback route */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Box>
      </Box>
    </Router>
  );
}

export default App;
