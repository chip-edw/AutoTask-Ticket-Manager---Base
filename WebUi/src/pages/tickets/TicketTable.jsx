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
    // TODO: Add actual sorting logic (local sort or API call)
  };

  if (loading) return <div>Loading tickets...</div>;
  if (error) return <div>Error loading tickets: {error}</div>;
  if (!tickets.length) return <div>No tickets found.</div>;

  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Tickets</h2>
      <table className="min-w-full border border-gray-300">
        <thead className="bg-gray-100">
          <tr>
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('ticketNumber')}>Ticket #</th>
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('title')}>Title</th>
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('companyName')}>Company</th>
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('queueName')}>Queue</th> {/* 👈 NEW HEADER */}
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('status')}>Status</th>
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('priority')}>Priority</th>
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('createdDate')}>Created</th>
            <th className="px-3 py-2 text-left cursor-pointer" onClick={() => handleSort('lastUpdated')}>Last Updated</th>
          </tr>
        </thead>
        <tbody>
          {tickets.map((ticket) => (
            <tr key={ticket.ticketNumber} className="border-t border-gray-200">
              <td className="px-3 py-2">{ticket.ticketNumber}</td>
              <td className="px-3 py-2">{ticket.title}</td>
              <td className="px-3 py-2">{ticket.companyName}</td>
              <td className="px-3 py-2">{ticket.queueName}</td> {/* 👈 NEW DATA CELL */}
              <td className="px-3 py-2">{ticket.status}</td>
              <td className="px-3 py-2">{ticket.priority}</td>
              <td className="px-3 py-2">{new Date(ticket.createdDate).toLocaleString()}</td>
              <td className="px-3 py-2">{new Date(ticket.lastUpdated).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default TicketTable;
