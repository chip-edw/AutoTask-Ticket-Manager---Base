import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '@/utils/api';
import LoadingSpinner from '@/components/LoadingSpinner';

const TicketDetail = () => {
  const { ticketNumber } = useParams();
  const navigate = useNavigate();

  const [ticket, setTicket] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);


  useEffect(() => {
    const fetchTicket = async () => {
      try {
        const res = await api.get(`/tickets/${ticketNumber}`);
        setTicket(res.data);
      } catch (err) {
          const message = err?.response?.data?.message || err.message || `Ticket ${ticketNumber} not found.`;
          setError(message);
      } finally {
        setLoading(false);
      }
    };

    fetchTicket();
  }, [ticketNumber]);


  if (loading) {
    return (
      <div className="p-8 flex justify-center items-center">
        <div className="w-8 h-8 border-4 border-blue-500 border-dashed rounded-full animate-spin"></div>
      </div>
    );
  }

  if (error) return <div className="p-4 text-red-600">Error: {error}</div>;

return (
  <div className="p-4 space-y-4">
    <h1 className="text-xl font-bold">Ticket #{ticket.ticketNumber}</h1>
    <div><strong>Title:</strong> {ticket.title}</div>
    <div><strong>Description:</strong> {ticket.description}</div>
    <div><strong>Status:</strong> {ticket.status}</div>
    <div><strong>Priority:</strong> {ticket.priority}</div>
    <div><strong>Queue:</strong> {ticket.queueName}</div>
    <div><strong>Company:</strong> {ticket.companyName}</div>
    <div><strong>Created:</strong> {new Date(ticket.createdDate).toLocaleString()}</div>

    <div className="pt-4 flex gap-2">
      <button
        className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
        onClick={() => navigate(`/tickets/${ticket.ticketNumber}/edit`)}
      >
        Edit
      </button>
      <button
        className="border border-gray-400 text-gray-700 px-4 py-2 rounded hover:bg-gray-100"
        onClick={() => navigate(-1)}
      >
        Back
      </button>
    </div>
  </div>
);

};

export default TicketDetail;
