import { Container, Title, Tabs, Text, Badge, Button, Stack, Anchor, Group } from '@mantine/core';
import { useParams } from 'react-router';
import {
  useGetTournamentQuery,
  useGetParticipantsQuery,
  useGetBracketQuery,
  useRegisterMutation,
} from '../../api/tournamentsApi';
import { useAuth } from '../../hooks/useAuth';
import { formatDate } from '../../utils/formatDate';
import { CoverImage } from '../../components/ui/CoverImage';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { ParticipantsList } from './components/ParticipantsList';
import { BracketSingle } from './components/BracketSingle';
import { BracketDouble } from './components/BracketDouble';
import { notifications } from '@mantine/notifications';
import { statusColors, statusLabels } from '../../styles/component-tokens';

export default function TournamentDetailPage() {
  const { slug } = useParams();
  const { data: t, isLoading } = useGetTournamentQuery(slug);
  const { isAuthenticated } = useAuth();
  const { data: participants } = useGetParticipantsQuery(t?.id, { skip: !t?.id });
  const { data: bracket } = useGetBracketQuery(t?.id, { skip: !t?.id });
  const [register] = useRegisterMutation();

  if (isLoading) return <Container py="xl"><SkeletonCard count={1} type="profile" /></Container>;
  if (!t) return <Container py="xl"><Text ta="center" style={{ color: 'var(--color-text-muted)' }}>Турнір не знайдено</Text></Container>;

  const handleRegister = async () => {
    try {
      await register({ id: t.id }).unwrap();
      notifications.show({ message: 'Заявку подано', color: 'green' });
    } catch (e) {
      notifications.show({ message: e.data?.message || 'Помилка', color: 'red' });
    }
  };

  return (
    <Container size="lg" py="xl" className="fade-in">
      {t.coverUrl && <CoverImage src={t.coverUrl} height={200} alt={t.title} />}
      <Group justify="space-between" mb="md">
        <div>
          <Title order={1} style={{ color: 'var(--color-text)' }}>{t.title}</Title>
          <Text style={{ color: 'var(--color-text-muted)' }}>
            {t.disciplineName} · {t.format} · {t.organizerUsername}
          </Text>
        </div>
        <Badge
          size="lg"
          variant="light"
          color={statusColors[t.status] || 'gray'}
          styles={{ light: { border: '1px solid currentColor' } }}
        >
          {statusLabels[t.status] || t.status}
        </Badge>
      </Group>

      {t.status === 'registration' && isAuthenticated && (
        <Button
          mb="lg"
          onClick={handleRegister}
          style={{
            background: 'var(--brand-gradient)',
            transition: 'all var(--transition-normal)',
          }}
        >
          Зареєструватися
        </Button>
      )}

      {t.status === 'ongoing' && t.streamUrl && (
        <Anchor
          href={t.streamUrl}
          target="_blank"
          mb="lg"
          display="block"
          style={{ color: 'var(--color-accent)', transition: 'all var(--transition-normal)' }}
        >
          Дивитися трансляцію
        </Anchor>
      )}

      <Tabs defaultValue="overview" styles={{
        tab: {
          color: 'var(--color-text-muted)',
          transition: 'all var(--transition-fast)',
          '&[dataActive]': { color: 'var(--color-primary)', borderColor: 'var(--color-primary)' },
          '&:hover': { color: 'var(--color-text)' },
        },
      }}>
        <Tabs.List>
          <Tabs.Tab value="overview">Огляд</Tabs.Tab>
          <Tabs.Tab value="participants">Учасники</Tabs.Tab>
          <Tabs.Tab value="bracket">Брекет</Tabs.Tab>
          <Tabs.Tab value="schedule">Розклад</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="overview" pt="md">
          <Stack gap="md">
            <Text style={{ color: 'var(--color-text)' }}>{t.description}</Text>
            {t.prizes?.length > 0 && (
              <Stack gap="xs">
                <Title order={4} style={{ color: 'var(--color-text)' }}>Призи</Title>
                {t.prizes.map((p) => (
                  <Text key={p.place} style={{ color: 'var(--color-text)' }}>
                    {p.place} місце — {p.description}
                  </Text>
                ))}
              </Stack>
            )}
            <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
              Реєстрація до: {formatDate(t.registrationEndsAt)} · Старт: {formatDate(t.startsAt)}
            </Text>
          </Stack>
        </Tabs.Panel>

        <Tabs.Panel value="participants" pt="md">
          <ParticipantsList participants={participants ?? []} />
        </Tabs.Panel>

        <Tabs.Panel value="bracket" pt="md">
          {t.format === 'double_elimination' ? (
            <BracketDouble matches={bracket ?? []} />
          ) : (
            <BracketSingle matches={bracket ?? []} />
          )}
        </Tabs.Panel>

        <Tabs.Panel value="schedule" pt="md">
          <Stack gap="sm">
            {(bracket ?? [])
              .filter((m) => m.scheduledAt || m.status === 'finished')
              .map((m) => (
                <Group key={m.id} justify="space-between" style={{
                  background: 'var(--color-bg-card)',
                  border: '1px solid var(--color-border)',
                  borderRadius: 8,
                  padding: '12px 16px',
                  borderLeft: '3px solid var(--color-primary)',
                }}>
                  <Text style={{ color: 'var(--color-text)' }}>
                    {m.participant1Name || 'TBD'} vs {m.participant2Name || 'TBD'}
                  </Text>
                  <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
                    {m.score1 != null ? `${m.score1}:${m.score2}` : formatDate(m.scheduledAt)}
                  </Text>
                </Group>
              ))}
          </Stack>
        </Tabs.Panel>
      </Tabs>
    </Container>
  );
}
