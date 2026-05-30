import { Card, Text } from '@mantine/core';

export function StatCard({ value, label, color = 'purple.4' }) {
  return (
    <Card withBorder padding="md" ta="center" style={{ borderLeft: '3px solid var(--mantine-color-purple-6)' }}>
      <Text size="xl" fw={700} c={color}>{value}</Text>
      <Text size="sm" c="dark.2">{label}</Text>
    </Card>
  );
}
