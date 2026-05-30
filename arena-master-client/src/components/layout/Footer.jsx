import { Container, Group, Text, Anchor } from '@mantine/core';
import { Link } from 'react-router';

export function Footer() {
  return (
    <Container
      size="lg"
      py="xl"
      mt="xl"
      style={{
        borderTop: '1px solid var(--color-border)',
        position: 'relative',
      }}
    >
      <Group justify="space-between">
        <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
          © {new Date().getFullYear()} ArenaMaster
        </Text>
        <Group gap="md">
          <Anchor
            component={Link}
            to="/tournaments"
            size="sm"
            style={{ color: 'var(--color-text-muted)', textDecoration: 'none', transition: 'color 150ms ease' }}
          >
            Турніри
          </Anchor>
          <Anchor
            component={Link}
            to="/teams"
            size="sm"
            style={{ color: 'var(--color-text-muted)', textDecoration: 'none', transition: 'color 150ms ease' }}
          >
            Команди
          </Anchor>
        </Group>
      </Group>
    </Container>
  );
}
