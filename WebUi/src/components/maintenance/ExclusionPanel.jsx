import React, { useEffect, useState, useCallback } from 'react';
import api from '@/utils/api';
import {
  Box,
  Button,
  TextField,
  Typography,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Snackbar,
  Alert
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';

const apiBase = '/maintenance';

export default function ExclusionPanel({ title, type }) {
  const [exclusions, setExclusions] = useState([]);
  const [newEntry, setNewEntry] = useState('');
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const endpoint = `${apiBase}/${type}-exclusions`;

  const fetchExclusions = useCallback(async () => {
    try {
      const response = await api.get(endpoint);
      setExclusions(response.data);
    } catch (error) {
      console.error('Failed to fetch exclusions:', error);
      showSnackbar('Failed to fetch exclusions.', 'error');
    }
  }, [endpoint]);

  const reloadExclusions = async () => {
    try {
      await api.post(`${apiBase}/reload`, { reloadExclusions: true });
    } catch (error) {
      console.error('Failed to reload memory exclusions:', error);
      showSnackbar('Changes saved, but reload failed.', 'warning');
    }
  };

  const showSnackbar = (message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  };

  const handleSnackbarClose = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  const addExclusion = async () => {
    if (!newEntry.trim()) return;
    try {
      const payload = type === 'sender'
        ? { email: newEntry }
        : { keyword: newEntry };

      await api.post(endpoint, payload);
      setNewEntry('');
      await fetchExclusions();
      await reloadExclusions();
      showSnackbar('Exclusion added and memory reloaded.');
    } catch (error) {
      console.error('Failed to add exclusion:', error);
      showSnackbar('Failed to add exclusion.', 'error');
    }
  };

  const deleteExclusion = async (id) => {
    try {
      await api.delete(`${endpoint}/${id}`);
      await fetchExclusions();
      await reloadExclusions();
      showSnackbar('Exclusion removed and memory reloaded.');
    } catch (error) {
      console.error('Failed to delete exclusion:', error);
      showSnackbar('Failed to delete exclusion.', 'error');
    }
  };

  useEffect(() => {
    fetchExclusions();
  }, [type, fetchExclusions]); // ✅ Fix: included fetchExclusions in dependencies

  return (
    <Box sx={{ my: 4 }}>
      <Typography variant="h6" gutterBottom>{title}</Typography>

      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        <TextField
          label={type === 'sender' ? 'Email Address' : 'Keyword'}
          value={newEntry}
          onChange={(e) => setNewEntry(e.target.value)}
          fullWidth
        />
        <Button variant="contained" onClick={addExclusion}>Add</Button>
      </Box>

      <List dense>
  {exclusions.map((item) => {
    console.log("Rendering item:", item);

    return (
      <ListItem
        key={item.id}
        secondaryAction={
          <IconButton edge="end" onClick={() => deleteExclusion(item.id)}>
            <DeleteIcon />
          </IconButton>
        }
      >
      <ListItemText
        primary={
          type === 'sender'
            ? (item.email || '[NO ADDRESS]')
            : (item.keyword || '[NO KEYWORD]')
        }
        secondary={
          item.createdOn
            ? new Date(item.createdOn).toLocaleString()
            : '[No timestamp]'
        }
      />
          </ListItem>
        );
      })}
</List>


      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={handleSnackbarClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={handleSnackbarClose} severity={snackbar.severity} sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
