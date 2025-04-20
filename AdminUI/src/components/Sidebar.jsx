import { Drawer, List, ListItem, ListItemText } from '@mui/material';
import { Link, useLocation } from 'react-router-dom';
import { useEffect, useState } from 'react';

const drawerWidth = 240;

function Sidebar() {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [role, setRole] = useState(localStorage.getItem('role'));
  const location = useLocation();

  useEffect(() => {
    // Every time the URL changes, check LocalStorage
    setToken(localStorage.getItem('token'));
    setRole(localStorage.getItem('role'));
  }, [location]);

  if (!token) {
    return null; // Don't show Sidebar if not logged in
  }

  return (
    <Drawer
      variant="permanent"
      anchor="left"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
        },
      }}
    >
      <List>
        <ListItem button component={Link} to="/">
          <ListItemText primary="Home" />
        </ListItem>

        {role === 'admin' && (
          <ListItem button component={Link} to="/admin/dashboard">
            <ListItemText primary="Admin Dashboard" />
          </ListItem>
        )}

        <ListItem button component={Link} to="/tickets/open">
          <ListItemText primary="Tickets" />
        </ListItem>
        <ListItem button component={Link} to="/crm/accounts">
          <ListItemText primary="CRM Accounts" />
        </ListItem>
      </List>
    </Drawer>
  );
}

export default Sidebar;
