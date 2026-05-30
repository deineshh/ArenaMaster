import { Group, Avatar, Text, Badge, Stack } from '@mantine/core';
import { uploadUrl } from '../../../utils/bracketHelpers';

export function ParticipantsList({ participants }) {
  if (!participants.length) return <Text style={{ color: 'var(--color-text-muted)' }}>Ще немає учасників</Text>;

  return (
    <Stack gap="sm">
      {participants.map((p) => (
        <Group key={p.id} style={{
          background: 'var(--color-bg-card)',
          border: '1px solid var(--color-border)',
          borderRadius: 8,
          padding: '10px 16px',
          borderLeft: '3px solid var(--color-primary)',
        }}>
          <Avatar src={uploadUrl(p.avatarUrl)} radius="xl" />
          <div>
            <Text fw={500} style={{ color: 'var(--color-text)' }}>{p.username || p.teamName}</Text>
            <Badge
              size="xs"
              variant="light"
              styles={{ light: { border: '1px solid currentColor' } }}
            >
              {p.status}
            </Badge>
          </div>
        </Group>
      ))}
    </Stack>
  );
}
