// src/components/PrivateRoute.jsx
import { Navigate, Outlet } from 'react-router-dom';

function PrivateRoute({ requiredRole }) {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && role !== requiredRole) {
    return <Navigate to="/" replace />;
  }

  // Render the child route(s)
  return <Outlet />;
}

export default PrivateRoute;
