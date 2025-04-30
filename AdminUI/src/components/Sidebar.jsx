import { Link } from 'react-router-dom';
import { Drawer, List, ListItem, ListItemIcon, ListItemText } from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AssignmentIcon from '@mui/icons-material/Assignment';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';

const Sidebar = () => {
  const role = localStorage.getItem('role');

  return (
    <Drawer variant="permanent" anchor="left">
      <List>
        <ListItem button component={Link} to="/">
          <ListItemIcon><HomeIcon /></ListItemIcon>
          <ListItemText primary="Home" />
        </ListItem>

        <ListItem button component={Link} to="/tickets/open">
          <ListItemIcon><AssignmentIcon /></ListItemIcon>
          <ListItemText primary="Tickets" />
        </ListItem>

        <ListItem button component={Link} to="/crm/accounts">
          <ListItemIcon><AccountCircleIcon /></ListItemIcon>
          <ListItemText primary="CRM Accounts" />
        </ListItem>

        {/* Admin-only links */}
        {role === 'admin' && (
          <>
            <ListItem button component={Link} to="/admin/dashboard">
              <ListItemIcon><DashboardIcon /></ListItemIcon>
              <ListItemText primary="Admin Dashboard" />
            </ListItem>

            <ListItem button component={Link} to="/admin/maintenance">
              <ListItemIcon><AssignmentIcon /></ListItemIcon>
              <ListItemText primary="Maintenance Panel" />
            </ListItem>
          </>
        )}
      </List>
    </Drawer>
  );
};

export default Sidebar;
