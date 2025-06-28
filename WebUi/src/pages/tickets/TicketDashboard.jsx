import React from 'react';
import { Box, Typography, Button, Stack } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const TicketDashboard = () => {
  const navigate = useNavigate();

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Ticket Dashboard
      </Typography>

      <Typography variant="body1" gutterBottom>
        Welcome to the central ticket management dashboard. Use the options below to get started.
      </Typography>

      <Stack spacing={2} direction="row" sx={{ mt: 2 }}>
        <Button variant="contained" onClick={() => navigate('/tickets/open')}>
          View Open Tickets
        </Button>
        <Button variant="contained" color="success" onClick={() => navigate('/tickets/new')}>
          Create New Ticket
        </Button>
      </Stack>
    </Box>
  );
};

export default TicketDashboard;
