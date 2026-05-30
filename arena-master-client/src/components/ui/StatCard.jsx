import { Card, Text } from '@mantine/core';

export function StatCard({ value, label, color = 'purple.4' }) {
  return (
    <Card withBorder padding="md" ta="center" style={{  }}>
      <Text size="xl" fw={700} c={color}>{value}</Text>
      <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>{label}</Text>
    </Card>
  );
}
