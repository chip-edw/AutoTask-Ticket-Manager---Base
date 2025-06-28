import { useEffect, useState } from 'react';
import api from '@/utils/api';

export const useTickets = (trigger) => {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTickets = async () => {
      try {
        const response = await api.get('/tickets');
        setTickets(response.data); // ✅ Use .data directly with axios
      } catch (err) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    };

    fetchTickets();
  }, [trigger]);

  return { tickets, loading, error };
};
