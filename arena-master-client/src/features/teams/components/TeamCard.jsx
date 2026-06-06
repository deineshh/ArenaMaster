import { Card, Text, Group, Avatar } from '@mantine/core';
import { Link } from 'react-router';
import { uploadUrl } from '../../../utils/bracketHelpers';

export function TeamCard({ team }) {
  return (
    <Card
      component={Link}
      to={`/teams/${team.slug}`}
      withBorder
      padding="lg"
      className="hover-glow-neon"
      style={{
        textDecoration: 'none',
      }}
    >
      <Group>
        <Avatar src={uploadUrl(team.logoUrl)} radius="xl" size="lg" />
        <div>
          <Text fw={600} style={{ color: 'var(--color-text)' }}>{team.name}</Text>
          <Text size="sm" style={{ color: 'var(--color-text-muted)' }}>
            Капітан: {team.captainUsername} · {team.memberCount} учасників
          </Text>
        </div>
      </Group>
    </Card>
  );
}
