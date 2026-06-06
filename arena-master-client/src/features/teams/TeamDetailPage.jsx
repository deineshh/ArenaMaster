import { Container, Title, Text, Group, Avatar, Badge, Button, Stack, TextInput } from '@mantine/core';
import { useParams } from 'react-router';
import { useState } from 'react';
import { useGetTeamQuery, useInvitePlayerMutation } from '../../api/teamsApi';
import { useAuth } from '../../hooks/useAuth';
import { uploadUrl } from '../../utils/bracketHelpers';
import { formatDate } from '../../utils/formatDate';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { Link } from 'react-router';
import { notifications } from '@mantine/notifications';

export default function TeamDetailPage() {
  const { slug } = useParams();
  const { data, isLoading } = useGetTeamQuery(slug);
  const { user } = useAuth();
  const [invite] = useInvitePlayerMutation();
  const [inviteName, setInviteName] = useState('');

  if (isLoading) return <Container py="xl"><SkeletonCard count={1} type="profile" /></Container>;
  if (!data) return <Container py="xl"><Text ta="center" style={{ color: 'var(--color-text-muted)' }}>Команду не знайдено</Text></Container>;

  const captainMember = data.members?.find((m) => m.role === 'captain');
  const isCaptain = user?.username === captainMember?.username;

  const handleInvite = async () => {
    try {
      await invite({ id: data.id, username: inviteName }).unwrap();
      notifications.show({ message: 'Запрошення надіслано', color: 'green' });
      setInviteName('');
    } catch {
      notifications.show({ message: 'Помилка', color: 'red' });
    }
  };

  return (
    <Container size="md" py="xl" className="fade-in">
      <Group mb="xl" style={{
        background: 'var(--color-bg-card)',
        border: '1px solid var(--color-border)',
        borderRadius: 'var(--radius-card)',
        padding: 24,
      }}>
        <Avatar src={uploadUrl(data.logoUrl)} size={80} radius="md" />
        <div>
          <Title order={1} style={{ color: 'var(--color-text)' }}>{data.name}</Title>
          <Text style={{ color: 'var(--color-text-muted)' }}>Створено: {formatDate(data.createdAt)}</Text>
          <Text size="sm" mt={2} style={{ color: 'var(--color-text-muted)' }}>
            Турнірів: {data.tournamentsCount} · Перемог: {data.winsCount}
          </Text>
        </div>
      </Group>

      <Title order={3} mb="md" style={{ color: 'var(--color-text)' }}>
        Склад
      </Title>
      <Stack gap="sm" mb="xl">
        {data.members?.map((m) => (
          <Group key={m.userId} style={{
            background: 'var(--color-bg-card)',
            border: '1px solid var(--color-border)',
            borderRadius: 8,
            padding: '10px 16px',
          }}>
            <Avatar src={uploadUrl(m.avatarUrl)} radius="xl" />
            <Text
              component={Link}
              to={`/profile/${m.username}`}
              fw={500}
              style={{ color: 'var(--color-text)', transition: 'color var(--transition-fast)' }}
            >
              {m.username}
            </Text>
            <Badge
              variant="light"
              styles={{ light: { border: '1px solid currentColor' } }}
            >
              {m.role}
            </Badge>
          </Group>
        ))}
      </Stack>

      {isCaptain && (
        <Group style={{
          background: 'var(--color-bg-card)',
          border: '1px solid var(--color-border)',
          borderRadius: 8,
          padding: 16,
        }}>
          <TextInput
            placeholder="Нікнейм гравця"
            value={inviteName}
            onChange={(e) => setInviteName(e.target.value)}
            styles={{
              input: {
                background: 'var(--color-bg)',
                borderColor: 'var(--color-border)',
                color: 'var(--color-text)',
              },
            }}
          />
          <Button
            onClick={handleInvite}
            style={{
              background: 'var(--brand-gradient)',
              transition: 'all var(--transition-normal)',
            }}
          >
            Запросити
          </Button>
        </Group>
      )}
    </Container>
  );
}
