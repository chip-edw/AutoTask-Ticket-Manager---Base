import React, { useEffect, useState } from 'react';
import { useLocation, Link } from 'react-router-dom';
import { useTickets } from './useTickets';
import FiltersBar from '../../components/FiltersBar'; // Adjust path if needed based on your structure

const TicketTable = () => {
  const location = useLocation();
  const { tickets, loading, error } = useTickets(location.pathname);

  const [sortField, setSortField] = useState('createdDate');
  const [sortDirection, setSortDirection] = useState('desc');

  const [queueFilter, setQueueFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [companyFilter, setCompanyFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');

  useEffect(() => {
    console.log('📍 Route changed:', location.pathname);
  }, [location]);

  const handleSort = (field) => {
    const direction = sortField === field && sortDirection === 'asc' ? 'desc' : 'asc';
    setSortField(field);
    setSortDirection(direction);
  };

  const sortedTickets = [...tickets].sort((a, b) => {
    const aValue = a[sortField];
    const bValue = b[sortField];

    if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
    if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
    return 0;
  });

  const filteredTickets = sortedTickets.filter((ticket) =>
    (queueFilter === '' || ticket.queueName === queueFilter) &&
    (statusFilter === '' || ticket.status === statusFilter) &&
    (companyFilter === '' || ticket.companyName === companyFilter) &&
    (priorityFilter === '' || ticket.priority === priorityFilter)
  );

  const queueOptions = [...new Set(tickets.map((t) => t.queueName))].sort();
  const statusOptions = [...new Set(tickets.map((t) => t.status))].sort();
  const companyOptions = [...new Set(tickets.map((t) => t.companyName))].sort();
  const priorityOptions = [...new Set(tickets.map((t) => t.priority))].sort();

  return (
<div className="p-4 w-full max-w-screen-xl mx-auto">

  <FiltersBar
    queueFilter={queueFilter}
    statusFilter={statusFilter}
    companyFilter={companyFilter}
    priorityFilter={priorityFilter}
    queueOptions={queueOptions}
    statusOptions={statusOptions}
    companyOptions={companyOptions}
    priorityOptions={priorityOptions}
    onQueueChange={setQueueFilter}
    onStatusChange={setStatusFilter}
    onCompanyChange={setCompanyFilter}
    onPriorityChange={setPriorityFilter}
  />

  <div className="mt-4" /> {/* or h-10 for more space */}
      {loading && <p>Loading tickets...</p>}
      {error && <p className="text-red-600">Error loading tickets: {error.message}</p>}

      {!loading && filteredTickets.length === 0 && <p>No tickets match the filter criteria.</p>}

      {!loading && filteredTickets.length > 0 && (
        <table className="min-w-full table-auto border border-gray-300 text-sm mt-10">
          <thead className="bg-gray-100">
            <tr>
              <th className="cursor-pointer px-4 py-2 text-left" onClick={() => handleSort('ticketNumber')}>Ticket #</th>
              <th className="cursor-pointer px-4 py-2 text-left" onClick={() => handleSort('queueName')}>Queue</th>
              <th className="cursor-pointer px-4 py-2 text-left" onClick={() => handleSort('status')}>Status</th>
              <th className="cursor-pointer px-4 py-2 text-left" onClick={() => handleSort('priority')}>Priority</th>
              <th className="cursor-pointer px-4 py-2 text-left" onClick={() => handleSort('companyName')}>Company</th>
              <th className="cursor-pointer px-4 py-2 text-left" onClick={() => handleSort('createdDate')}>Created Date</th>
            </tr>
          </thead>
          <tbody>
            {filteredTickets.map((ticket) => (
              <tr key={ticket.ticketNumber} className="border-t border-gray-200 hover:bg-gray-50">
                <td className="px-4 py-2 text-blue-600 underline">
                  <Link to={`/tickets/${ticket.ticketNumber}`}>{ticket.ticketNumber}</Link>
                </td>
                <td className="px-4 py-2">{ticket.queueName}</td>
                <td className="px-4 py-2">{ticket.status}</td>
                <td className="px-4 py-2">{ticket.priority}</td>
                <td className="px-4 py-2">{ticket.companyName}</td>
                <td className="px-4 py-2">{new Date(ticket.createdDate).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default TicketTable;
