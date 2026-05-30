import { Container, Paper, Title, Stack, Group, Button, Text } from '@mantine/core';
import { ProtectedRoute } from '../../components/ui/ProtectedRoute';
import { useMyInvitationsQuery, useAcceptInvitationMutation, useDeclineInvitationMutation } from '../../api/teamsApi';
import { notifications } from '@mantine/notifications';

export default function InvitationsPage() {
  const { data: invitations, isLoading } = useMyInvitationsQuery();
  const [accept] = useAcceptInvitationMutation();
  const [decline] = useDeclineInvitationMutation();

  const handleAccept = async (id) => {
    try {
      await accept(id).unwrap();
      notifications.show({ message: 'Запрошення прийнято', color: 'green' });
    } catch {
      notifications.show({ message: 'Помилка', color: 'red' });
    }
  };

  const handleDecline = async (id) => {
    try {
      await decline(id).unwrap();
      notifications.show({ message: 'Запрошення відхилено', color: 'yellow' });
    } catch {
      notifications.show({ message: 'Помилка', color: 'red' });
    }
  };

  return (
    <ProtectedRoute>
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
            Запрошення до команд
          </Title>

          {isLoading ? (
            <Text style={{ color: 'var(--color-text-muted)' }}>Завантаження...</Text>
          ) : !invitations?.length ? (
            <Text style={{ color: 'var(--color-text-muted)' }}>Немає активних запрошень</Text>
          ) : (
            <Stack>
              {invitations.map((inv) => (
                <Paper
                  key={inv.id}
                  p="md"
                  withBorder
                  style={{
                    background: 'var(--color-bg)',
                    borderColor: 'var(--color-border)',
                  }}
                >
                  <Group justify="space-between">
                    <div>
                      <Text fw={500} style={{ color: 'var(--color-text)' }}>{inv.teamName}</Text>
                      <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
                        {new Date(inv.createdAt).toLocaleDateString('uk-UA')}
                      </Text>
                    </div>
                    <Group>
                      <Button
                        size="sm"
                        style={{
                          background: 'var(--color-success)',
                          transition: 'all var(--transition-normal)',
                        }}
                        onClick={() => handleAccept(inv.id)}
                      >
                        Прийняти
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        style={{
                          borderColor: 'var(--color-error)',
                          color: 'var(--color-error)',
                          transition: 'all var(--transition-normal)',
                        }}
                        onClick={() => handleDecline(inv.id)}
                      >
                        Відхилити
                      </Button>
                    </Group>
                  </Group>
                </Paper>
              ))}
            </Stack>
          )}
        </Paper>
      </Container>
    </ProtectedRoute>
  );
}
