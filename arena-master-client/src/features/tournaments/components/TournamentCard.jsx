import { Card, Text, Badge, Image, Group } from '@mantine/core';
import { Link } from 'react-router';
import { uploadUrl } from '../../../utils/bracketHelpers';
import { formatDate } from '../../../utils/formatDate';
import { statusColors, statusLabels } from '../../../styles/component-tokens';

export function TournamentCard({ tournament: t }) {
  return (
    <Card
      component={Link}
      to={`/tournaments/${t.slug}`}
      withBorder
      padding="lg"
      className="hover-glow-neon"
      style={{
        borderLeft: '3px solid var(--mantine-color-purple-6)',
        textDecoration: 'none',
      }}
    >
      {t.coverUrl && (
        <Card.Section>
          <Image src={uploadUrl(t.coverUrl)} h={140} alt={t.title} className="transition-all" />
        </Card.Section>
      )}
      <Group justify="space-between" mt="md">
        <Text fw={600} lineClamp={1} c="dark.0">
          {t.title}
        </Text>
        <Badge color={statusColors[t.status] || 'gray'} variant="light">
          {statusLabels[t.status] || t.status}
        </Badge>
      </Group>
      <Text size="sm" c="dark.2">
        {t.disciplineName}
      </Text>
      <Text size="sm" c="dark.2">
        {formatDate(t.startsAt)} · {t.participantsCount}/{t.maxParticipants}
      </Text>
    </Card>
  );
}
