import { Container, Stack, ThemeIcon, Text, Button } from '@mantine/core';
import { IconAlertCircle } from '@tabler/icons-react';

export function ErrorDisplay({ message = 'Щось пішло не так', onRetry }) {
  return (
    <Container py="xl">
      <Stack align="center" gap="sm">
        <ThemeIcon
          size={48}
          radius="xl"
          variant="light"
          style={{ background: 'rgba(239, 68, 68, 0.12)', color: 'var(--color-error)' }}
        >
          <IconAlertCircle size={28} />
        </ThemeIcon>
        <Text ta="center" style={{ color: 'var(--color-text-muted)' }}>{message}</Text>
        {onRetry && (
          <Button
            variant="light"
            size="sm"
            onClick={onRetry}
            style={{
              color: 'var(--color-primary)',
              background: 'var(--color-primary-light)',
              transition: 'all var(--transition-normal)',
            }}
          >
            Спробувати знову
          </Button>
        )}
      </Stack>
    </Container>
  );
}
