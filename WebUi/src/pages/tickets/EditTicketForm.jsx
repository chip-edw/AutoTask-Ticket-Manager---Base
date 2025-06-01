import { useState, useEffect } from 'react';
import { TextField, Button, MenuItem, Box, Typography } from '@mui/material';
import api from '@/utils/api';
import { useParams, useNavigate } from 'react-router-dom';

const EditTicketForm = () => {
  const { ticketNumber } = useParams();
  const navigate = useNavigate();

  const [form, setForm] = useState({
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
        setForm({
          title: ticket.title || '',
          description: ticket.description || '',
          statusId: ticket.statusId || '',
          queueId: ticket.queueId || '',
          priorityId: ticket.priorityId || '',
          assignedResourceId: ticket.assignedResourceId || ''
        });

        setMetadata(metaRes.data);
        setLoading(false);
      } catch (err) {
        console.error(err);
        setError('Failed to load ticket or metadata.');
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
      await api.patch(`/tickets/${form.id}`, form);
      setSuccess('Ticket updated successfully!');
      navigate(`/tickets/${ticketNumber}`);
    } catch (err) {
      setError('Failed to update ticket.');
    }
  };

  if (loading) return <Typography>Loading...</Typography>;

  return (
    <Box sx={{ maxWidth: 600, mx: 'auto', mt: 4 }}>
      <Typography variant="h5" gutterBottom>
        Edit Ticket #{ticketNumber}
      </Typography>

      <TextField label="Title" name="title" value={form.title} onChange={handleChange} fullWidth margin="normal" />
      <TextField label="Description" name="description" value={form.description} onChange={handleChange} fullWidth margin="normal" multiline rows={4} />

      <TextField select label="Status" name="statusId" value={form.statusId} onChange={handleChange} fullWidth margin="normal">
        {metadata.statuses.map((s) => <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>)}
      </TextField>

      <TextField select label="Queue" name="queueId" value={form.queueId} onChange={handleChange} fullWidth margin="normal">
        {metadata.queues.map((q) => <MenuItem key={q.value} value={q.value}>{q.label}</MenuItem>)}
      </TextField>

      <TextField select label="Priority" name="priorityId" value={form.priorityId} onChange={handleChange} fullWidth margin="normal">
        {metadata.priorities.map((p) => <MenuItem key={p.value} value={p.value}>{p.label}</MenuItem>)}
      </TextField>

      <TextField select label="Assigned Resource" name="assignedResourceId" value={form.assignedResourceId} onChange={handleChange} fullWidth margin="normal">
        {metadata.resources.map((r) => <MenuItem key={r.value} value={r.value}>{r.label}</MenuItem>)}
      </TextField>

      <Button variant="contained" onClick={handleSubmit} sx={{ mt: 2 }}>
        Save Changes
      </Button>

      {error && <Typography color="error" sx={{ mt: 2 }}>{error}</Typography>}
      {success && <Typography color="success.main" sx={{ mt: 2 }}>{success}</Typography>}
    </Box>
  );
};

export default EditTicketForm;
