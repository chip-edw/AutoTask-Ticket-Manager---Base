import { Link, useNavigate } from 'react-router-dom';
import {
  Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Collapse
} from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AssignmentIcon from '@mui/icons-material/Assignment';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import { useState } from 'react';

const Sidebar = () => {
  const role = localStorage.getItem('role');
  const navigate = useNavigate();
  const [ticketOpen, setTicketOpen] = useState(true);

  const handleTicketClick = () => {
    setTicketOpen(!ticketOpen);
    navigate('/tickets'); // Navigate to TicketDashboard
  };

  return (
    <Drawer variant="permanent" anchor="left">
      <List>

        <ListItem disablePadding>
          <ListItemButton component={Link} to="/">
            <ListItemIcon><HomeIcon /></ListItemIcon>
            <ListItemText primary="Home" />
          </ListItemButton>
        </ListItem>

        {/* Ticket Section */}
        <ListItem disablePadding>
          <ListItemButton onClick={handleTicketClick}>
            <ListItemIcon><AssignmentIcon /></ListItemIcon>
            <ListItemText primary="Tickets" />
            {ticketOpen ? <ExpandLess /> : <ExpandMore />}
          </ListItemButton>
        </ListItem>
        <Collapse in={ticketOpen} timeout="auto" unmountOnExit>
          <List component="div" disablePadding>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/tickets/open" sx={{ pl: 4 }}>
                <ListItemText primary="Open Tickets" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/tickets/new" sx={{ pl: 4 }}>
                <ListItemText primary="Create Ticket" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/tickets/closed" sx={{ pl: 4 }}>
                <ListItemText primary="Closed Tickets" />
              </ListItemButton>
            </ListItem>
          </List>
        </Collapse>

        <ListItem disablePadding>
          <ListItemButton component={Link} to="/crm/accounts">
            <ListItemIcon><AccountCircleIcon /></ListItemIcon>
            <ListItemText primary="CRM Accounts" />
          </ListItemButton>
        </ListItem>

        {role === 'admin' && (
          <>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/admin/dashboard">
                <ListItemIcon><DashboardIcon /></ListItemIcon>
                <ListItemText primary="Admin Dashboard" />
              </ListItemButton>
            </ListItem>

            <ListItem disablePadding>
              <ListItemButton component={Link} to="/admin/maintenance">
                <ListItemIcon><AssignmentIcon /></ListItemIcon>
                <ListItemText primary="Maintenance Panel" />
              </ListItemButton>
            </ListItem>
          </>
        )}
      </List>
    </Drawer>
  );
};

export default Sidebar;
