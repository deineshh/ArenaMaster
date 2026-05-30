import { Group, Button, Text, Anchor, Burger, Drawer, Stack } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { Link, useNavigate } from 'react-router';
import { useAuth } from '../../hooks/useAuth';
import { useLogoutMutation } from '../../api/authApi';
import { NotificationBell } from '../../features/notifications/NotificationBell';
import { ThemeToggle } from '../ui/ThemeToggle';
import styles from './Header.module.css';

export function Header() {
  const { user, isAuthenticated, isAdmin, isOrganizer } = useAuth();
  const [logout] = useLogoutMutation();
  const navigate = useNavigate();
  const [opened, { toggle, close }] = useDisclosure();

  const links = (
    <>
      <Anchor component={Link} to="/tournaments" className={styles.navLink}>
        Турніри
      </Anchor>
      <Anchor component={Link} to="/teams" className={styles.navLink}>
        Команди
      </Anchor>
      {isOrganizer && (
        <Anchor component={Link} to="/tournaments/create" className={styles.navLinkSpecial}>
          + Турнір
        </Anchor>
      )}
      {isAdmin && (
        <Anchor component={Link} to="/admin/users" className={styles.navLinkSpecial}>
          Адмін
        </Anchor>
      )}
    </>
  );

  return (
    <Group h="100%" px="md" justify="space-between" className={styles.header}>
      <Group>
        <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" color="gray" />
        <Text component={Link} to="/" className={styles.logo}>
          ArenaMaster
        </Text>
        <Group gap="lg" visibleFrom="sm">
          {links}
        </Group>
      </Group>
      <Group>
        <ThemeToggle />
        {isAuthenticated && <NotificationBell />}
        {isAuthenticated ? (
          <>
            <Button
              variant="subtle"
              component={Link}
              to={`/profile/${user.username}`}
              style={{
                color: 'var(--color-text)',
                transition: 'all var(--transition-normal)',
              }}
            >
              {user.username}
            </Button>
            <Button
              variant="outline"
              color="purple"
              style={{
                borderColor: 'var(--color-primary)',
                transition: 'all var(--transition-normal)',
              }}
              onClick={async () => {
                await logout();
                navigate('/');
              }}
            >
              Вийти
            </Button>
          </>
        ) : (
          <>
            <Button
              variant="subtle"
              component={Link}
              to="/login"
              style={{
                color: 'var(--color-text)',
                transition: 'all var(--transition-normal)',
              }}
            >
              Увійти
            </Button>
            <Button
              component={Link}
              to="/register"
              style={{
                background: 'var(--brand-gradient)',
                transition: 'all var(--transition-normal)',
              }}
            >
              Реєстрація
            </Button>
          </>
        )}
      </Group>
      <Drawer
        opened={opened}
        onClose={close}
        hiddenFrom="sm"
        title={
          <Text className={styles.logo} style={{ fontSize: '1.1rem' }}>
            ArenaMaster
          </Text>
        }
        padding="md"
        styles={{
          header: { borderBottom: '1px solid var(--color-border)' },
          body: { paddingTop: 16 },
        }}
      >
        <Stack gap="md" onClick={close}>
          {links}
        </Stack>
      </Drawer>
    </Group>
  );
}
