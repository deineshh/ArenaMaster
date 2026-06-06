import { Container, Paper, Title, TextInput, PasswordInput, Button, Stack, Anchor, Text, Group } from '@mantine/core';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router';
import { useLoginMutation } from '../../api/authApi';
import { notifications } from '@mantine/notifications';

export default function LoginPage() {
  const [login, { isLoading }] = useLoginMutation();
  const navigate = useNavigate();
  const { register, handleSubmit } = useForm();

  const onSubmit = async (values) => {
    try {
      await login(values).unwrap();
      notifications.show({ title: 'Успіх', message: 'Ви увійшли', color: 'green' });
      navigate('/');
    } catch (err) {
      const message = err?.data?.message || 'Невірний email або пароль';
      notifications.show({ title: 'Помилка', message, color: 'red' });
    }
  };

  return (
    <Container size="xs" py="xl">
      <Paper
        p="xl"
        withBorder
        style={{
          background: 'var(--color-bg-card)',
          borderColor: 'var(--color-border)',
        }}
      >
        <Title order={2} mb="lg" className="gradient-text">
          Вхід
        </Title>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Stack>
            <TextInput
              label="Email"
              {...register('email', { required: true })}
              styles={{
                input: {
                  background: 'var(--color-bg)',
                  borderColor: 'var(--color-border)',
                  color: 'var(--color-text)',
                  transition: 'all var(--transition-fast)',
                },
                inputFocus: { borderColor: 'var(--color-primary)', boxShadow: 'var(--glow-primary)' },
                label: { color: 'var(--color-text-muted)', marginBottom: 4 },
              }}
            />
            <PasswordInput
              label="Пароль"
              {...register('password', { required: true })}
              styles={{
                input: {
                  background: 'var(--color-bg)',
                  borderColor: 'var(--color-border)',
                  color: 'var(--color-text)',
                  transition: 'all var(--transition-fast)',
                },
                inputFocus: { borderColor: 'var(--color-primary)', boxShadow: 'var(--glow-primary)' },
                label: { color: 'var(--color-text-muted)', marginBottom: 4 },
              }}
            />
            <Button
              type="submit"
              loading={isLoading}
              fullWidth
              style={{
                background: 'var(--brand-gradient)',
                transition: 'all var(--transition-normal)',
              }}
            >
              Увійти
            </Button>
            <Stack gap="xs">
              <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
                Або через OAuth:
              </Text>
              <Group grow>
                <Button
                  variant="outline"
                  component="a"
                  style={{
                    borderColor: 'var(--color-border)',
                    color: 'var(--color-text)',
                    transition: 'all var(--transition-normal)',
                  }}
                  href={`${import.meta.env.VITE_API_URL || ''}/api/auth/oauth/google`}
                >
                  Google
                </Button>
                <Button
                  variant="outline"
                  component="a"
                  style={{
                    borderColor: 'var(--color-border)',
                    color: 'var(--color-text)',
                    transition: 'all var(--transition-normal)',
                  }}
                  href={`${import.meta.env.VITE_API_URL || ''}/api/auth/oauth/discord`}
                >
                  Discord
                </Button>
              </Group>
            </Stack>
            <Text size="sm" ta="center" style={{ color: 'var(--color-text-muted)' }}>
              Немає акаунту?{' '}
              <Anchor component={Link} to="/register" style={{ color: 'var(--color-primary)' }}>
                Зареєструватися
              </Anchor>
            </Text>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
}
