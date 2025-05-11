import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { useTickets } from './useTickets';

const TicketTable = () => {
  const location = useLocation();
  const { tickets, loading, error } = useTickets(location.pathname);
  const [sortField, setSortField] = useState('createdDate');
  const [sortDirection, setSortDirection] = useState('desc');

  useEffect(() => {
    console.log('📍 Route changed:', location.pathname);
  }, [location]);

  const handleSort = (field) => {
    const direction = sortField === field && sortDirection === 'asc' ? 'desc' : 'asc';
    setSortField(field);
    setSortDirection(direction);
    // TODO: Add sorting logic here (backend or local)
  };

  if (loading) return <div>Loading tickets...</div>;
  if (error) return <div>Error loading tickets: {error}</div>;
  if (!tickets.length) return <div>No tickets found.</div>;

  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Tickets</h2>
      <table className="min-w-full border">
        <thead>
          <tr>
            <th onClick={() => handleSort('ticketNumber')}>Ticket #</th>
            <th onClick={() => handleSort('title')}>Title</th>
            <th onClick={() => handleSort('status')}>Status</th>
            <th onClick={() => handleSort('priority')}>Priority</th>
            <th onClick={() => handleSort('createdDate')}>Created</th>
            <th onClick={() => handleSort('lastUpdated')}>Last Updated</th>
          </tr>
        </thead>
        <tbody>
          {tickets.map((ticket) => (
            <tr key={ticket.ticketNumber}>
              <td>{ticket.ticketNumber}</td>
              <td>{ticket.title}</td>
              <td>{ticket.status}</td>
              <td>{ticket.priority}</td>
              <td>{new Date(ticket.createdDate).toLocaleString()}</td>
              <td>{new Date(ticket.lastUpdated).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default TicketTable;
