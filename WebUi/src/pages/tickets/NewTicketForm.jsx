import { useState, useEffect } from 'react';
import { TextField, Button, MenuItem, Box, Typography } from '@mui/material';
import api from '@/utils/api';

const NewTicketForm = () => {
  const [form, setForm] = useState({
    title: '',
    description: '',
    statusId: '',
    queueId: '',
    priorityId: '',
    assignedResourceId: ''
  });

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Optional: Load picklists here from API if available
  const [statuses, setStatuses] = useState([
    { value: '1', label: 'New' },
    { value: '2', label: 'In Progress' },
    { value: '5', label: 'Complete' }
  ]);
  const [queues, setQueues] = useState([
    { value: '1', label: 'Support' },
    { value: '2', label: 'Development' }
  ]);
  const [priorities, setPriorities] = useState([
    { value: '1', label: 'Low' },
    { value: '2', label: 'Medium' },
    { value: '3', label: 'High' }
  ]);
  const [resources, setResources] = useState([
    { value: '123', label: 'John Doe' },
    { value: '456', label: 'Jane Smith' }
  ]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async () => {
    setSaving(true);
    setError('');
    setSuccess('');

    try {
      const response = await api.post('/tickets', form);
      setSuccess(`Ticket created: #${response.data.ticketNumber}`);
      setForm({
        title: '',
        description: '',
        statusId: '',
        queueId: '',
        priorityId: '',
        assignedResourceId: ''
      });
    } catch (err) {
      setError('Failed to create ticket.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box sx={{ maxWidth: 600, mx: 'auto', mt: 4 }}>
      <Typography variant="h5" gutterBottom>
        Create New Ticket
      </Typography>

      <TextField label="Title" name="title" value={form.title} onChange={handleChange} fullWidth margin="normal" />
      <TextField label="Description" name="description" value={form.description} onChange={handleChange} fullWidth margin="normal" multiline rows={4} />

      <TextField select label="Status" name="statusId" value={form.statusId} onChange={handleChange} fullWidth margin="normal">
        {statuses.map((s) => <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>)}
      </TextField>

      <TextField select label="Queue" name="queueId" value={form.queueId} onChange={handleChange} fullWidth margin="normal">
        {queues.map((q) => <MenuItem key={q.value} value={q.value}>{q.label}</MenuItem>)}
      </TextField>

      <TextField select label="Priority" name="priorityId" value={form.priorityId} onChange={handleChange} fullWidth margin="normal">
        {priorities.map((p) => <MenuItem key={p.value} value={p.value}>{p.label}</MenuItem>)}
      </TextField>

      <TextField select label="Assigned Resource" name="assignedResourceId" value={form.assignedResourceId} onChange={handleChange} fullWidth margin="normal">
        {resources.map((r) => <MenuItem key={r.value} value={r.value}>{r.label}</MenuItem>)}
      </TextField>

      <Button variant="contained" onClick={handleSubmit} disabled={saving} sx={{ mt: 2 }}>
        {saving ? 'Creating...' : 'Create Ticket'}
      </Button>

      {error && <Typography color="error" sx={{ mt: 2 }}>{error}</Typography>}
      {success && <Typography color="success.main" sx={{ mt: 2 }}>{success}</Typography>}
    </Box>
  );
};

export default NewTicketForm;
