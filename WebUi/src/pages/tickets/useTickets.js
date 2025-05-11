import { useEffect, useState } from 'react';

export const useTickets = (trigger) => {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTickets = async () => {
      try {
        const response = await fetch('/api/v1/tickets');
        if (!response.ok) throw new Error(`Status: ${response.status}`);
        const data = await response.json();
        setTickets(data);
      } catch (err) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    };

    fetchTickets();
  }, [trigger]); // ✅ This re-runs when location.pathname changes

  return { tickets, loading, error };
};
