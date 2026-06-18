import { Outlet, NavLink } from 'react-router';
import { AppShell, Stack, Title, Text } from '@mantine/core';
import { RoleGuard } from '../../components/ui/RoleGuard';
import { useMediaQuery } from '@mantine/hooks';
import styles from './AdminLayout.module.css';

const links = [
  { to: '/admin/users', label: 'Користувачі' },
  { to: '/admin/tournaments', label: 'Турніри' },
  { to: '/admin/teams', label: 'Команди' },
  { to: '/admin/disciplines', label: 'Дисципліни' },
  { to: '/admin/matches', label: 'Матчі' },
  { to: '/admin/participants', label: 'Учасники' },
  { to: '/admin/members', label: 'Склад команд' },
];

export default function AdminLayout() {
  const isMobile = useMediaQuery('(max-width: 768px)');

  if (isMobile) {
    return (
      <RoleGuard role="admin">
        <Stack p="md" align="center" className="fade-in">
          <Title order={3} className="gradient-text">Адмін-панель</Title>
          <Text ta="center" style={{ color: 'var(--color-text-muted)' }}>
            Адмін-панель доступна з планшета та десктопу (768px+).
          </Text>
        </Stack>
      </RoleGuard>
    );
  }

  return (
    <RoleGuard role="admin">
      <AppShell navbar={{ width: 220, breakpoint: 'sm' }} padding="md">
        <AppShell.Navbar p="md" className={styles.nav}>
          <Title order={4} mb="lg" className="gradient-text">
            Адмін
          </Title>
          <Stack gap="xs">
            {links.map((l) => (
              <NavLink key={l.to} to={l.to} className={styles.link}>
                {l.label}
              </NavLink>
            ))}
          </Stack>
        </AppShell.Navbar>
        <AppShell.Main className="fade-in">
          <Outlet />
        </AppShell.Main>
      </AppShell>
    </RoleGuard>
  );
}
