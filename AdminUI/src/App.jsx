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

function App() {
  return (
    <Router>
      <Box sx={{ display: 'flex' }}>
        <TopBar />
        <Sidebar />
        <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
          <Toolbar />
          <Routes>
            {/* Public route */}
            <Route path="/login" element={<Login />} />

            {/* Private routes (must be logged in) */}
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
            <Route path="/crm/accounts" element={
              <PrivateRoute>
                <AccountList />
              </PrivateRoute>
            } />

            {/* Fallback route */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Box>
      </Box>
    </Router>
  );
}

export default App;
