import { Navigate } from 'react-router';
import { useAuth } from '../../hooks/useAuth';
import { PageLoader } from './PageLoader';

export function ProtectedRoute({ children }) {
  const { user, isAuthenticated } = useAuth();
  if (!user && !isAuthenticated) return <PageLoader />;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return children;
}
