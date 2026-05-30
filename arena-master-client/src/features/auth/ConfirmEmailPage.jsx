import { Container, Paper, Title, Text, Loader, Center } from '@mantine/core';
import { useSearchParams } from 'react-router';
import { useConfirmEmailQuery } from '../../api/authApi';

export default function ConfirmEmailPage() {
  const [params] = useSearchParams();
  const token = params.get('token') ?? '';
  const { isLoading, isSuccess, isError } = useConfirmEmailQuery(token, { skip: !token });

  return (
    <Container size="xs" py="xl">
      <Paper
        p="xl"
        withBorder
        ta="center"
        style={{
          background: 'var(--color-bg-card)',
          borderColor: 'var(--color-border)',
          borderLeft: '3px solid var(--color-primary)',
        }}
      >
        {isLoading && (
          <Center>
            <Loader color="purple" />
          </Center>
        )}
        {isSuccess && (
          <div className="fade-in">
            <Title order={2} className="gradient-text">Email підтверджено</Title>
            <Text style={{ color: 'var(--color-text-muted)' }} mt="md">
              Тепер ви можете увійти.
            </Text>
          </div>
        )}
        {isError && (
          <div className="fade-in">
            <Title order={2} style={{ color: 'var(--color-error)' }}>
              Помилка
            </Title>
            <Text style={{ color: 'var(--color-text-muted)' }} mt="md">
              Невірний або прострочений токен.
            </Text>
          </div>
        )}
      </Paper>
    </Container>
  );
}
