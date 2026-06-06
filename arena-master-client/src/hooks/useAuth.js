import { useSelector } from 'react-redux';
import { useGetMeQuery } from '../api/authApi';

const hasSession = () => { try { return !!localStorage.getItem('auth'); } catch { return false; } };

export function useAuth() {
  const { user } = useSelector((s) => s.auth);
  const skip = !!user || !hasSession();
  useGetMeQuery(undefined, { skip });
  return {
    user,
    isAuthenticated: !!user,
    isAdmin: user?.role === 'admin',
    isOrganizer: user?.role === 'organizer' || user?.role === 'admin',
  };
}
