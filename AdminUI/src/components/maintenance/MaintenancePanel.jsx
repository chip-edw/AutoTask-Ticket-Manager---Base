import { Link, Outlet } from 'react-router-dom';
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Divider,
} from '@mui/material';

const MaintenancePanel = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Admin Maintenance Panel
      </Typography>

      <Box sx={{ mb: 2 }}>
        <List dense>
          <ListItemButton component={Link} to="company-settings">
            <ListItemText primary="Company Settings" />
          </ListItemButton>
          <ListItemButton component={Link} to="system-stats">
            <ListItemText primary="System Stats" />
          </ListItemButton>
          <ListItemButton component={Link} to="subject-exclusions">
            <ListItemText primary="Subject Exclusions" />
          </ListItemButton>
          <ListItemButton component={Link} to="sender-exclusions">
            <ListItemText primary="Sender Exclusions" />
          </ListItemButton>
          <ListItemButton component={Link} to="reload">
            <ListItemText primary="Reload Memory" />
          </ListItemButton>
        </List>
      </Box>

      <Divider sx={{ mb: 2 }} />

      {/* Nested component rendering based on selected route */}
      <Outlet />
    </Box>
  );
};

export default MaintenancePanel;
