import { Link, Outlet } from 'react-router-dom';
import {
  Box,
  Typography,
  List,
  ListItem,
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
          <ListItem button component={Link} to="company-settings">
            <ListItemText primary="Company Settings" />
          </ListItem>
          <ListItem button component={Link} to="system-stats">
            <ListItemText primary="System Stats" />
          </ListItem>
          <ListItem button component={Link} to="subject-exclusions">
            <ListItemText primary="Subject Exclusions" />
          </ListItem>
          <ListItem button component={Link} to="sender-exclusions">
            <ListItemText primary="Sender Exclusions" />
          </ListItem>
          <ListItem button component={Link} to="reload">
            <ListItemText primary="Reload Memory" />
          </ListItem>
        </List>
      </Box>

      <Divider sx={{ mb: 2 }} />

      {/* Nested component rendering based on selected route */}
      <Outlet />
    </Box>
  );
};

export default MaintenancePanel;
