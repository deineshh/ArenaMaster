import { Group, Title, Button } from '@mantine/core';
import { Link } from 'react-router';

export function PageHeader({ title, actionLabel, actionTo, actionOnClick }) {
  return (
    <Group justify="space-between" mb="lg">
      <Title order={1} className="gradient-text" style={{ fontSize: 28 }}>{title}</Title>
      {actionLabel && (actionOnClick ? (
        <Button
          onClick={actionOnClick}
          style={{
            background: 'var(--brand-gradient)',
            transition: 'all var(--transition-normal)',
          }}
        >
          {actionLabel}
        </Button>
      ) : (
        <Button
          component={Link}
          to={actionTo}
          style={{
            background: 'var(--brand-gradient)',
            transition: 'all var(--transition-normal)',
          }}
        >
          {actionLabel}
        </Button>
      ))}
    </Group>
  );
}
