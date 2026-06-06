import { Container, Paper, Title, TextInput, Select, Button, Stack, Textarea, Switch } from '@mantine/core';
import { useForm, Controller } from 'react-hook-form';
import { useNavigate } from 'react-router';
import { useCreateTournamentMutation } from '../../api/tournamentsApi';
import { useGetDisciplinesQuery } from '../../api/disciplinesApi';
import { useAuth } from '../../hooks/useAuth';
import { RoleGuard } from '../../components/ui/RoleGuard';
import { notifications } from '@mantine/notifications';

const inputStyles = {
  input: {
    background: 'var(--color-bg)',
    borderColor: 'var(--color-border)',
    color: 'var(--color-text)',
    transition: 'all var(--transition-fast)',
  },
  inputFocus: { borderColor: 'var(--color-primary)', boxShadow: 'var(--glow-primary)' },
  label: { color: 'var(--color-text-muted)', marginBottom: 4 },
};

export default function CreateTournamentPage() {
  const { isOrganizer } = useAuth();
  const { data: disciplines } = useGetDisciplinesQuery();
  const [create] = useCreateTournamentMutation();
  const navigate = useNavigate();
  const { register, handleSubmit, control } = useForm({
    defaultValues: { format: 'single_elimination', participantType: 'solo', maxParticipants: 8, autoAccept: false },
  });

  if (!isOrganizer) {
    return (
      <Container py="xl" className="fade-in">
        <Title order={3} style={{ color: 'var(--color-text)' }}>Доступ лише для організаторів</Title>
      </Container>
    );
  }

  const onSubmit = async (values) => {
    try {
      const result = await create({
        ...values,
        disciplineId: values.disciplineId,
        registrationEndsAt: new Date(values.registrationEndsAt).toISOString(),
        startsAt: new Date(values.startsAt).toISOString(),
      }).unwrap();
      notifications.show({ message: 'Турнір створено', color: 'green' });
      navigate(`/tournaments/${result.slug}`);
    } catch {
      notifications.show({ message: 'Помилка створення', color: 'red' });
    }
  };

  return (
    <RoleGuard role="organizer">
      <Container size="sm" py="xl" className="fade-in">
        <Paper
          p="xl"
          withBorder
          style={{
            background: 'var(--color-bg-card)',
            borderColor: 'var(--color-border)',
          }}
        >
          <Title order={2} mb="lg" className="gradient-text">
            Створити турнір
          </Title>
          <form onSubmit={handleSubmit(onSubmit)}>
            <Stack>
              <TextInput label="Назва" styles={inputStyles} {...register('title', { required: true })} />
              <Controller
                name="disciplineId"
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <Select
                    label="Дисципліна"
                    data={(disciplines ?? []).map((d) => ({ value: d.id, label: d.name }))}
                    styles={inputStyles}
                    {...field}
                  />
                )}
              />
              <Controller
                name="format"
                control={control}
                render={({ field }) => (
                  <Select
                    label="Формат"
                    data={[
                      { value: 'single_elimination', label: 'Single Elimination' },
                      { value: 'double_elimination', label: 'Double Elimination' },
                    ]}
                    styles={inputStyles}
                    {...field}
                  />
                )}
              />
              <Controller
                name="participantType"
                control={control}
                render={({ field }) => (
                  <Select
                    label="Тип учасників"
                    data={[
                      { value: 'solo', label: 'Solo (1v1)' },
                      { value: 'team', label: 'Team' },
                    ]}
                    styles={inputStyles}
                    {...field}
                  />
                )}
              />
              <Controller
                name="maxParticipants"
                control={control}
                render={({ field }) => (
                  <Select
                    label="Макс. учасників"
                    data={['4', '8', '16', '32']}
                    value={String(field.value)}
                    onChange={(v) => field.onChange(Number(v))}
                    styles={inputStyles}
                  />
                )}
              />
              <TextInput label="Дедлайн реєстрації" type="datetime-local" styles={inputStyles} {...register('registrationEndsAt')} />
              <TextInput label="Дата початку" type="datetime-local" styles={inputStyles} {...register('startsAt')} />
              <Textarea label="Опис" styles={inputStyles} {...register('description')} />
              <Textarea label="Призовий фонд" placeholder="1 місце — 500 грн, 2 місце — 200 грн" styles={inputStyles} {...register('prizeDescription')} />
              <TextInput label="Стрім-лінк" styles={inputStyles} {...register('streamUrl')} />
              <Switch
                label="Автоприйняття заявок"
                {...register('autoAccept')}
                styles={{ label: { color: 'var(--color-text-muted)' } }}
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
    </RoleGuard>
  );
}
