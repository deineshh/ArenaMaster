import { ActionIcon, Indicator, Menu, Text, Stack, Button, Group } from '@mantine/core';
import { IconBell } from '@tabler/icons-react';
import { useNavigate } from 'react-router';
import { useNotifications } from '../../hooks/useNotifications';
import {
  useMarkAllReadMutation,
  useMarkReadMutation,
} from '../../api/notificationsApi';
import { formatDate } from '../../utils/formatDate';

export function NotificationBell() {
  const { items, unreadCount } = useNotifications();
  const [markAll] = useMarkAllReadMutation();
  const [markRead] = useMarkReadMutation();
  const navigate = useNavigate();

  const handleClick = (n) => {
    if (!n.isRead) markRead(n.id);
    if (n.type === 'team_invitation') navigate('/teams/invitations');
  };

  return (
    <Menu width={320} position="bottom-end">
      <Menu.Target>
        <Indicator
          disabled={unreadCount === 0}
          label={unreadCount}
          size={16}
          styles={{ indicator: { background: 'var(--color-accent)' } }}
        >
          <ActionIcon
            variant="subtle"
            size="lg"
            style={{ color: 'var(--color-text)', transition: 'all var(--transition-fast)' }}
          >
            <IconBell size={20} />
          </ActionIcon>
        </Indicator>
      </Menu.Target>
      <Menu.Dropdown
        style={{
          background: 'var(--color-bg-card)',
          borderColor: 'var(--color-border)',
        }}
      >
        <Menu.Label style={{ borderBottom: '1px solid var(--color-border)' }}>
          <Group justify="space-between">
            <Text fw={600} style={{ color: 'var(--color-text)' }}>Сповіщення</Text>
            {unreadCount > 0 && (
              <Button
                size="xs"
                variant="subtle"
                style={{ color: 'var(--color-primary)' }}
                onClick={() => markAll()}
              >
                Прочитати всі
              </Button>
            )}
          </Group>
        </Menu.Label>
        {items.length === 0 ? (
          <Menu.Item disabled style={{ color: 'var(--color-text-muted)' }}>
            Немає сповіщень
          </Menu.Item>
        ) : (
          items.slice(0, 8).map((n) => (
            <Menu.Item
              key={n.id}
              onClick={() => handleClick(n)}
              style={{
                opacity: n.isRead ? 0.6 : 1,
                borderBottom: '1px solid var(--color-border)',
                transition: 'all var(--transition-fast)',
              }}
            >
              <Stack gap={2}>
                <Text size="sm" fw={500} style={{ color: n.isRead ? 'var(--color-text-muted)' : 'var(--color-text)' }}>
                  {n.title}
                </Text>
                <Text size="xs" style={{ color: 'var(--color-text-muted)' }}>
                  {n.body}
                </Text>
                <Text size="xs" style={{ color: 'var(--color-text-muted)' }}>
                  {formatDate(n.createdAt)}
                </Text>
              </Stack>
            </Menu.Item>
          ))
        )}
      </Menu.Dropdown>
    </Menu>
  );
}
