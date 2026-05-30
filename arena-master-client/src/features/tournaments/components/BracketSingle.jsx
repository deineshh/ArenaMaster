import { Box, Card, Text, Group, ScrollArea } from '@mantine/core';
import styles from './Bracket.module.css';

export function BracketSingle({ matches }) {
  const rounds = {};
  for (const m of matches) {
    if (!rounds[m.round]) rounds[m.round] = [];
    rounds[m.round].push(m);
  }

  return (
    <ScrollArea className={styles.scroll}>
      <Group align="flex-start" wrap="nowrap" gap="lg">
        {Object.entries(rounds).map(([round, ms]) => (
          <Box key={round} className={styles.round}>
            <Text size="sm" className={styles.bracketLabel}>
              Раунд {round}
            </Text>
            {ms.map((m) => (
              <Card key={m.id} padding="sm" mb="sm" withBorder className={styles.match}>
                <Text size="sm" style={{ color: m.score1 != null ? 'var(--color-text)' : 'var(--color-text-muted)' }}>
                  {m.participant1Name || 'TBD'}
                </Text>
                <Text size="xs" ta="center" style={{ color: 'var(--color-text-muted)' }}>
                  vs
                </Text>
                <Text size="sm" style={{ color: m.score2 != null ? 'var(--color-text)' : 'var(--color-text-muted)' }}>
                  {m.participant2Name || 'TBD'}
                </Text>
                {m.score1 != null && (
                  <Text size="xs" ta="center" mt={4} className="gradient-text" fw={600}>
                    {m.score1}:{m.score2}
                  </Text>
                )}
              </Card>
            ))}
          </Box>
        ))}
      </Group>
    </ScrollArea>
  );
}
