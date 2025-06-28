import { useState, useEffect } from 'react';
import { TextField, Button, MenuItem, Box, Typography } from '@mui/material';
import api from '@/utils/api';
import { useParams, useNavigate } from 'react-router-dom';

const EditTicketForm = () => {
  const { ticketNumber } = useParams();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    id: '',
    title: '',
    description: '',
    statusId: '',
    queueId: '',
    priorityId: '',
    assignedResourceId: ''
  });

  const [metadata, setMetadata] = useState({
    statuses: [],
    queues: [],
    priorities: [],
    resources: []
  });

  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
  const loadTicketAndMetadata = async () => {
    try {
      const [ticketRes, metaRes] = await Promise.all([
        api.get(`/tickets/${ticketNumber}`),
        api.get('/tickets/metadata')
      ]);

      const ticket = ticketRes.data;
      console.log('🔍 Loaded ticket:', ticket);
      console.log('🔍 Loaded metadata:', metaRes.data);

      // Helper function to find ID by label
      const findIdByLabel = (options, label) => {
        const option = options.find(opt => opt.label === label);
        console.log(`🔍 Looking for "${label}" in options, found:`, option);
        return option ? option.value : '';
      };

      const metadata = {
        statuses: metaRes.data.statuses || [],
        queues: metaRes.data.queues || [],
        priorities: metaRes.data.priorities || [],
        resources: metaRes.data.resources || []
      };

      // ✅ Map current ticket values to dropdown IDs
      setForm({
        id: ticket.id,
        title: ticket.title || '',
        description: ticket.description || '',
        statusId: findIdByLabel(metadata.statuses, ticket.status),
        queueId: findIdByLabel(metadata.queues, ticket.queueName),
        priorityId: findIdByLabel(metadata.priorities, ticket.priority),
        assignedResourceId: ticket.assignedResourceId?.toString() || '',
        companyId: ticket.companyId?.toString() || ''
      });

      setMetadata(metadata);
      setLoading(false);
    } catch (err) {
      console.error(err);
      setError('Failed to load ticket data.');
      setLoading(false);
    }
  };

  loadTicketAndMetadata();
}, [ticketNumber]);

  const handleChange = (e) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async () => {
    setError('');
    setSuccess('');
    try {
      console.log('🔍 Submitting form data:', form);

    // Only send fields that have values (changed fields)
    const updateData = {};
    
    if (form.title) updateData.title = form.title;
    if (form.description) updateData.description = form.description;
    if (form.statusId) updateData.statusId = parseInt(form.statusId);
    if (form.queueId) updateData.queueId = parseInt(form.queueId);
    if (form.priorityId) updateData.priorityId = parseInt(form.priorityId);
    if (form.assignedResourceId) updateData.assignedResourceId = parseInt(form.assignedResourceId);
    if (form.companyId) updateData.companyId = parseInt(form.companyId);

    console.log('🔍 Sending only changed fields:', updateData);
    await api.patch(`/tickets/${form.id}`, updateData);
    setSuccess('Ticket updated successfully!');
    navigate(`/tickets/${ticketNumber}`);
  } catch (err) {
    console.error('Update error:', err);
    const message = err?.response?.data?.message || 'Failed to update ticket.';
    setError(message);
  }
};
  if (loading) return <Typography>Loading...</Typography>;

  return (
    <Box sx={{ maxWidth: 600, mx: 'auto', mt: 4 }}>
      <Typography variant="h5" gutterBottom>
        Edit Ticket #{ticketNumber}
      </Typography>

      <TextField 
        label="Title" 
        name="title" 
        value={form.title} 
        onChange={handleChange} 
        fullWidth 
        margin="normal" 
      />
      
      <TextField 
        label="Description" 
        name="description" 
        value={form.description} 
        onChange={handleChange} 
        fullWidth 
        margin="normal" 
        multiline 
        rows={4} 
      />

      <TextField 
        select 
        label="Status" 
        name="statusId" 
        value={form.statusId} 
        onChange={handleChange} 
        fullWidth 
        margin="normal"
      >
        {metadata.statuses.map((s) => (
          <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>
        ))}
      </TextField>

      <TextField 
        select 
        label="Queue" 
        name="queueId" 
        value={form.queueId} 
        onChange={handleChange} 
        fullWidth 
        margin="normal"
      >
        {metadata.queues.map((q) => (
          <MenuItem key={q.value} value={q.value}>{q.label}</MenuItem>
        ))}
      </TextField>

      <TextField 
        select 
        label="Priority" 
        name="priorityId" 
        value={form.priorityId} 
        onChange={handleChange} 
        fullWidth 
        margin="normal"
      >
        {metadata.priorities.map((p) => (
          <MenuItem key={p.value} value={p.value}>{p.label}</MenuItem>
        ))}
      </TextField>

      <TextField 
        select 
        label="Assigned Resource" 
        name="assignedResourceId" 
        value={form.assignedResourceId} 
        onChange={handleChange} 
        fullWidth 
        margin="normal"
      >
        {metadata.resources.map((r) => (
          <MenuItem key={r.value} value={r.value}>{r.label}</MenuItem>
        ))}
      </TextField>

      <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
        <Button variant="contained" onClick={handleSubmit}>
          Save Changes
        </Button>
        <Button variant="outlined" color="secondary" onClick={() => navigate(`/tickets/${ticketNumber}`)}>
          Cancel
        </Button>
      </Box>

      {error && <Typography color="error" sx={{ mt: 2 }}>{error}</Typography>}
      {success && <Typography color="success.main" sx={{ mt: 2 }}>{success}</Typography>}
    </Box>
  );
};

export default EditTicketForm;