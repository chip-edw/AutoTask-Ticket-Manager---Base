/**
 * NOTE: This component uses @mui/x-data-grid (Community version).
 *
 * The free version supports:
 * - Column sorting, filtering, pagination
 * - Checkbox and inline editing
 * - Virtualization for large datasets
 *
 * Upgrade to @mui/x-data-grid-pro or -premium later if needed for:
 * - Server-side data loading
 * - Grouping, aggregation, or advanced filtering
 * - Excel export or pinned columns
 *
 * Current strategy: Use Community version during MVP phase to avoid licensing costs.
 */


import React, { useEffect, useState } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  Snackbar,
  Typography,
} from '@mui/material';
import { DataGrid } from '@mui/x-data-grid';
import axios from 'axios';

const CompanySettings = () => {
  console.log("✅ CompanySettings component is mounted");

  const [companies, setCompanies] = useState([]);
  const [dirtyRows, setDirtyRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '' });

  useEffect(() => {
    fetchCompanySettings();
  }, []);

  const fetchCompanySettings = async () => {
    setLoading(true);
    try {
      const res = await axios.get('/api/v1/maintenance/companysettings');
      console.log("📦 Company data loaded:", res.data);
      setCompanies(res.data);
    } catch (error) {
      console.error('❌ Error fetching company settings:', error);
      setSnackbar({ open: true, message: 'Failed to load company settings' });
    } finally {
      setLoading(false);
    }
  };

  const handleCellEditCommit = (params) => {
    const updatedRows = companies.map((row) =>
      row.autotaskId === params.id ? { ...row, [params.field]: params.value } : row
    );
    setCompanies(updatedRows);
    if (!dirtyRows.includes(params.id)) {
      setDirtyRows([...dirtyRows, params.id]);
    }
  };

  const handleSave = async () => {
    setSaving(true);
    const updates = companies.filter((c) => dirtyRows.includes(c.autotaskId));
    try {
      await axios.put('/api/v1/maintenance/companysettings/update', updates);
      setSnackbar({ open: true, message: '✅ Company settings saved' });
      setDirtyRows([]);
      await refreshMemory();
    } catch (error) {
      console.error('❌ Error saving settings:', error);
      setSnackbar({ open: true, message: 'Save failed' });
    } finally {
      setSaving(false);
    }
  };

  const refreshMemory = async () => {
    try {
      await axios.post('/api/v1/maintenance/companysettings/refreshcompanymemory');
      setSnackbar({ open: true, message: '🔁 Memory refreshed' });
    } catch (error) {
      console.error('❌ Memory refresh failed:', error);
      setSnackbar({ open: true, message: 'Failed to refresh memory' });
    }
  };

  const columns = [
    { field: 'accountName', headerName: 'Company', width: 200 },
    {
      field: 'supportEmail',
      headerName: 'Support Email',
      width: 250,
      editable: true,
    },
    { field: 'enabled', headerName: 'Enabled', type: 'boolean', editable: true },
    { field: 'enableEmail', headerName: 'Enable Email', type: 'boolean', editable: true },
    { field: 'autoAssign', headerName: 'Auto-Assign', type: 'boolean', editable: true },
  ];

  return (
    <Box sx={{ height: 600, width: '100%' }}>
<Typography>This is CompanySettings.jsx</Typography>

      {loading ? (
        <CircularProgress />
      ) : (
        <DataGrid
          rows={companies}
          columns={columns}
          getRowId={(row) => row.autotaskId}
          onCellEditCommit={handleCellEditCommit}
          pageSize={25}
          rowsPerPageOptions={[25, 50, 100]}
          checkboxSelection={false}
          disableRowSelectionOnClick
        />
      )}
      <Box sx={{ mt: 2 }}>
        <Button
          variant="contained"
          onClick={handleSave}
          disabled={saving || dirtyRows.length === 0}
        >
          {saving ? 'Saving...' : 'Save Changes'}
        </Button>
        <Button
          variant="outlined"
          onClick={refreshMemory}
          sx={{ ml: 2 }}
        >
          Refresh Memory
        </Button>
      </Box>
      <Snackbar
        open={snackbar.open}
        autoHideDuration={3000}
        onClose={() => setSnackbar({ open: false, message: '' })}
        message={snackbar.message}
      />
    </Box>
  );
};

export default CompanySettings;
