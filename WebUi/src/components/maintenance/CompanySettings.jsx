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
import api from '@/utils/api';

const CompanySettings = () => {
  console.log("✅ CompanySettings component is mounted");

  const [companies, setCompanies] = useState([]);
  const [dirtyRows, setDirtyRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '' });

  useEffect(() => {
    fetchCompanySettings();
  }, []);

  const fetchCompanySettings = async () => {
    setLoading(true);
    setLoadError(false);
    try {
      const res = await api.get('/maintenance/companysettings');
      console.log("📦 Company data loaded:", res.data);
      setCompanies(res.data);
    } catch (error) {
      console.error('❌ Error fetching company settings:', error);
      setSnackbar({ open: true, message: 'Failed to load company settings' });
      setLoadError(true);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    setSaving(true);
    const updates = companies.filter((c) => dirtyRows.includes(c.autotaskId));
    console.log("💾 Saving rows:", updates); // 🔍 Add this
    try {
      await api.put('/maintenance/companysettings/update', updates);
      setSnackbar({ open: true, message: '✅ Company settings saved' });
      setDirtyRows([]);
      await refreshMemory();
    } catch (error) {
      console.error('❌ Error saving settings:', error); // 🔍 Show response error
      setSnackbar({ open: true, message: 'Save failed' });
    } finally {
      setSaving(false);
    }
  };

  const handleRowUpdate = (newRow) => {
    const updatedRows = companies.map((row) =>
      row.autotaskId === newRow.autotaskId ? newRow : row
    );
    setCompanies(updatedRows);
    if (!dirtyRows.includes(newRow.autotaskId)) {
      setDirtyRows([...dirtyRows, newRow.autotaskId]);
    }
    return newRow; // required
  };
  

  const refreshMemory = async () => {
    try {
      await api.post('/maintenance/companysettings/refreshcompanymemory');
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
    { field: 'enabled', headerName: 'Enabled', type: 'boolean', editable: true, width: 120 },
    { field: 'enableEmail', headerName: 'Enable Email', type: 'boolean', editable: true, width: 140 },
    { field: 'autoAssign', headerName: 'Auto Assign', type: 'boolean', editable: true, width: 140 },
  ];

  return (
    <Box sx={{ height: 600, width: '100%' }}>
      <Typography variant="h5" gutterBottom>
        Company Settings Management
      </Typography>

      {loading ? (
        <CircularProgress />
      ) : loadError ? (
        <Typography color="error" sx={{ mt: 2 }}>
          ⚠️ Failed to load company settings. Please try again later.
        </Typography>
      ) : (
        <DataGrid
          rows={companies}
          columns={columns}
          getRowId={(row) => row.autotaskId}
          processRowUpdate={handleRowUpdate}
          experimentalFeatures={{ newEditingApi: true }}
          pageSize={25}
          rowsPerPageOptions={[25, 50, 100]}
          checkboxSelection={false}
          disableRowSelectionOnClick
        />
      )}

      {!loadError && (
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
      )}

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
