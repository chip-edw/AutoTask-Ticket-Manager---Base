// src/tickets/TicketDetail.jsx
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

const TicketDetail = () => {
  const { ticketNumber } = useParams();
  const [ticket, setTicket] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTicket = async () => {
      try {
        const res = await fetch(`/api/v1/tickets/${ticketNumber}`);
        if (!res.ok) {
          throw new Error(`Ticket ${ticketNumber} not found.`);
        }
        const data = await res.json();
        setTicket(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchTicket();
  }, [ticketNumber]);

  if (loading) return <div className="p-4">Loading...</div>;
  if (error) return <div className="p-4 text-red-600">Error: {error}</div>;

  return (
    <div className="p-4 space-y-2">
      <h1 className="text-xl font-bold">Ticket #{ticket.ticketNumber}</h1>
      <div><strong>Title:</strong> {ticket.title}</div>
      <div><strong>Description:</strong> {ticket.description}</div>
      <div><strong>Status:</strong> {ticket.status}</div>
      <div><strong>Priority:</strong> {ticket.priority}</div>
      <div><strong>Queue:</strong> {ticket.queue}</div>
      <div><strong>Company:</strong> {ticket.companyName}</div>
      <div><strong>Created:</strong> {new Date(ticket.createdDate).toLocaleString()}</div>
    </div>
  );
};

export default TicketDetail;
