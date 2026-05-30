import { Container, Avatar, Title, Text, Group, SimpleGrid, Tabs, Button, Stack, Anchor } from '@mantine/core';
import { useParams, Link } from 'react-router';
import { useGetProfileQuery, useGetTournamentHistoryQuery } from '../../api/usersApi';
import { useAuth } from '../../hooks/useAuth';
import { uploadUrl } from '../../utils/bracketHelpers';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { StatCard } from '../../components/ui/StatCard';

export default function ProfilePage() {
  const { username } = useParams();
  const { data, isLoading } = useGetProfileQuery(username);
  const { data: history } = useGetTournamentHistoryQuery(username);
  const { user: currentUser } = useAuth();

  if (isLoading) return <Container py="xl"><SkeletonCard count={1} type="profile" /></Container>;
  if (!data) return <Container py="xl"><Text ta="center" style={{ color: 'var(--color-text-muted)' }}>Профіль не знайдено</Text></Container>;

  const profile = data.profile;
  const isOwner = currentUser?.username === username;

  return (
    <Container size="md" py="xl" className="fade-in">
      <Group mb="xl" style={{
        background: 'var(--color-bg-card)',
        border: '1px solid var(--color-border)',
        borderRadius: 'var(--radius-card)',
        padding: 24,
      }}>
        <Avatar
          src={uploadUrl(profile.avatarUrl)}
          size={100}
          radius="md"
          style={{ border: '2px solid var(--color-primary)' }}
        />
        <div>
          <Title order={1} className="gradient-text">{profile.username}</Title>
          <Text style={{ color: 'var(--color-text-muted)' }}>{profile.bio}</Text>
          {profile.discordUrl && (
            <Anchor
              href={profile.discordUrl}
              size="sm"
              style={{ color: 'var(--color-primary)', transition: 'color var(--transition-fast)' }}
            >
              Discord
            </Anchor>
          )}
          {isOwner && (
            <Button
              component={Link}
              to="/profile/edit"
              variant="light"
              mt="sm"
              size="xs"
              style={{
                background: 'var(--color-primary-light)',
                color: 'var(--color-primary)',
                border: '1px solid var(--color-primary)',
                transition: 'all var(--transition-normal)',
              }}
            >
              Редагувати
            </Button>
          )}
        </div>
      </Group>

      <SimpleGrid cols={3} mb="xl">
        <StatCard value={profile.tournamentsCount} label="Турнірів" />
        <StatCard value={profile.winsCount} label="Перемог" />
        <StatCard value={profile.matchesPlayed} label="Матчів" />
      </SimpleGrid>

      <Tabs defaultValue="tournaments" styles={{
        tab: {
          color: 'var(--color-text-muted)',
          transition: 'all var(--transition-fast)',
          '&[dataActive]': { color: 'var(--color-primary)', borderColor: 'var(--color-primary)' },
          '&:hover': { color: 'var(--color-text)' },
        },
      }}>
        <Tabs.List>
          <Tabs.Tab value="tournaments">Турніри</Tabs.Tab>
          <Tabs.Tab value="teams">Команди</Tabs.Tab>
        </Tabs.List>
        <Tabs.Panel value="tournaments" pt="md">
          <Stack gap="sm">
              {(history ?? []).map((t) => (
              <div
                key={t.tournamentId}
                style={{
                  borderRadius: 8,
                  padding: 12,
                  transition: 'all var(--transition-normal)',
                }}
              >
                <Text
                  component={Link}
                  to={`/tournaments/${t.slug}`}
                  fw={500}
                  style={{ color: 'var(--color-text)', textDecoration: 'none' }}
                >
                  {t.title}
                </Text>
                <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
                  {t.status}
                </Text>
              </div>
            ))}
          </Stack>
        </Tabs.Panel>
        <Tabs.Panel value="teams" pt="md">
          <Stack gap="sm">
              {(data.teams ?? []).map((t) => (
              <div
                key={t.id}
                style={{
                  borderRadius: 8,
                  padding: 12,
                  transition: 'all var(--transition-normal)',
                }}
              >
                <Text
                  component={Link}
                  to={`/teams/${t.slug}`}
                  fw={500}
                  style={{ color: 'var(--color-text)', textDecoration: 'none' }}
                >
                  {t.name}
                </Text>
                <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
                  {t.role}
                </Text>
              </div>
            ))}
          </Stack>
        </Tabs.Panel>
      </Tabs>
    </Container>
  );
}
