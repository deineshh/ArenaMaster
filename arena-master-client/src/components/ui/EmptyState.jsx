import { Stack, Text, ThemeIcon } from '@mantine/core';
import { IconMoodEmpty } from '@tabler/icons-react';

export function EmptyState({ message = 'Нічого не знайдено' }) {
  return (
    <Stack align="center" py="xl" gap="sm">
      <ThemeIcon
        size={48}
        radius="xl"
        variant="light"
        style={{ background: 'var(--color-primary-light)', color: 'var(--color-primary)' }}
      >
        <IconMoodEmpty size={28} />
      </ThemeIcon>
      <Text style={{ color: 'var(--color-text-muted)' }}>{message}</Text>
    </Stack>
  );
}
