import { Container, Paper, Title, TextInput, Button, Stack } from '@mantine/core';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';
import { useCreateTeamMutation } from '../../api/teamsApi';
import { ProtectedRoute } from '../../components/ui/ProtectedRoute';
import { notifications } from '@mantine/notifications';

export default function CreateTeamPage() {
  const [create] = useCreateTeamMutation();
  const navigate = useNavigate();
  const { register, handleSubmit } = useForm();

  const onSubmit = async (values) => {
    try {
      const result = await create(values).unwrap();
      notifications.show({ message: 'Команду створено', color: 'green' });
      navigate(`/teams/${result.slug}`);
    } catch (e) {
      notifications.show({ message: e.data?.message || 'Помилка', color: 'red' });
    }
  };

  return (
    <ProtectedRoute>
      <Container size="xs" py="xl" className="fade-in">
        <Paper
          p="xl"
          withBorder
          style={{
            background: 'var(--color-bg-card)',
            borderColor: 'var(--color-border)',
            borderLeft: '3px solid var(--color-primary)',
          }}
        >
          <Title order={2} mb="lg" className="gradient-text">
            Створити команду
          </Title>
          <form onSubmit={handleSubmit(onSubmit)}>
            <Stack>
              <TextInput
                label="Назва команди"
                {...register('name', { required: true })}
                styles={{
                  input: {
                    background: 'var(--color-bg)',
                    borderColor: 'var(--color-border)',
                    color: 'var(--color-text)',
                  },
                  inputFocus: { borderColor: 'var(--color-primary)', boxShadow: 'var(--glow-primary)' },
                  label: { color: 'var(--color-text-muted)', marginBottom: 4 },
                }}
              />
              <Button
                type="submit"
                style={{
                  background: 'var(--brand-gradient)',
                  transition: 'all var(--transition-normal)',
                }}
              >
                Створити
              </Button>
            </Stack>
          </form>
        </Paper>
      </Container>
    </ProtectedRoute>
  );
}
