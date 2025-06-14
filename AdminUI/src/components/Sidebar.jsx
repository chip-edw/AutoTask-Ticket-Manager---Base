import { Link } from 'react-router-dom';
import { Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText } from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AssignmentIcon from '@mui/icons-material/Assignment';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';

const Sidebar = () => {
  const role = localStorage.getItem('role');

  return (
    <Drawer variant="permanent" anchor="left">
      <List>
        <ListItemButton component={Link} to="/">
          <ListItemIcon><HomeIcon /></ListItemIcon>
          <ListItemText primary="Home" />
        </ListItemButton>

        <ListItemButton component={Link} to="/tickets/open">
          <ListItemIcon><AssignmentIcon /></ListItemIcon>
          <ListItemText primary="Tickets" />
        </ListItemButton>

        <ListItemButton component={Link} to="/crm/accounts">
          <ListItemIcon><AccountCircleIcon /></ListItemIcon>
          <ListItemText primary="CRM Accounts" />
        </ListItemButton>

        {/* Admin-only links */}
        {role === 'admin' && (
          <>
            <ListItemButton component={Link} to="/admin/dashboard">
              <ListItemIcon><DashboardIcon /></ListItemIcon>
              <ListItemText primary="Admin Dashboard" />
            </ListItemButton>

            <ListItemButton component={Link} to="/admin/maintenance">
              <ListItemIcon><AssignmentIcon /></ListItemIcon>
              <ListItemText primary="Maintenance Panel" />
            </ListItemButton>
          </>
        )}
      </List>
    </Drawer>
  );
};

export default Sidebar;
