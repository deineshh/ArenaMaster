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
        borderLeft: '3px solid var(--mantine-color-purple-6)',
        textDecoration: 'none',
      }}
    >
      <Group>
        <Avatar src={uploadUrl(team.logoUrl)} radius="xl" size="lg" />
        <div>
          <Text fw={600} c="dark.0">{team.name}</Text>
          <Text size="sm" c="dark.2">
            Капітан: {team.captainUsername} · {team.memberCount} учасників
          </Text>
        </div>
      </Group>
    </Card>
  );
}
