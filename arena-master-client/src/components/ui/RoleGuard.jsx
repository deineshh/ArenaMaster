import { Navigate } from 'react-router';
import { useAuth } from '../../hooks/useAuth';

export function RoleGuard({ role, children }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  if (user.role !== role && !(role === 'organizer' && user.role === 'admin'))
    return <Navigate to="/" replace />;
  return children;
}
