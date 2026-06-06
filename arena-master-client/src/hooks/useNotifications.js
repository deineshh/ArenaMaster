import { useSelector } from 'react-redux';
import { useGetNotificationsQuery } from '../api/notificationsApi';
import { useAuth } from './useAuth';

export function useNotifications() {
  const { isAuthenticated } = useAuth();
  useGetNotificationsQuery(undefined, { skip: !isAuthenticated, pollingInterval: 60000 });
  const { items, unreadCount } = useSelector((s) => s.notifications);
  return { items, unreadCount };
}
