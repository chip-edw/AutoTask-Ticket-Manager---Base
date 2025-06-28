import { Outlet } from 'react-router-dom';

const TicketPanel = () => {
  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Ticket Management</h1>
      <Outlet />
    </div>
  );
};

export default TicketPanel;
