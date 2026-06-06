import { Container, Paper, Title, TextInput, PasswordInput, Button, Stack, Anchor, Text } from '@mantine/core';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router';
import { useRegisterMutation } from '../../api/authApi';
import { notifications } from '@mantine/notifications';

export default function RegisterPage() {
  const [registerUser, { isLoading }] = useRegisterMutation();
  const navigate = useNavigate();
  const { register, handleSubmit } = useForm();

  const onSubmit = async (values) => {
    try {
      await registerUser(values).unwrap();
      notifications.show({
        title: 'Реєстрація',
        message: 'Перевірте email для підтвердження',
        color: 'green',
      });
      navigate('/login');
    } catch (err) {
      notifications.show({
        title: 'Помилка',
        message: err.data?.message || 'Не вдалося зареєструватися',
        color: 'red',
      });
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
          Реєстрація
        </Title>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Stack>
            <TextInput
              label="Нікнейм"
              {...register('username', { required: true })}
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
            <TextInput
              label="Email"
              type="email"
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
              {...register('password', { required: true, minLength: 8 })}
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
              Зареєструватися
            </Button>
            <Text size="sm" ta="center" style={{ color: 'var(--color-text-muted)' }}>
              Вже є акаунт?{' '}
              <Anchor component={Link} to="/login" style={{ color: 'var(--color-primary)' }}>
                Увійти
              </Anchor>
            </Text>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
}
