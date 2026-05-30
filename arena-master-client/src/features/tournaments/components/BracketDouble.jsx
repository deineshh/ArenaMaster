import { Box, Card, Text, Group, ScrollArea, Stack } from '@mantine/core';
import styles from './Bracket.module.css';

export function BracketDouble({ matches }) {
  const winners = matches.filter((m) => m.bracketSide === 'winners');
  const losers = matches.filter((m) => m.bracketSide === 'losers');
  const grand = matches.filter((m) => m.bracketSide === 'grand_final');

  const renderSide = (list, label) => {
    const rounds = {};
    for (const m of list) {
      if (!rounds[m.round]) rounds[m.round] = [];
      rounds[m.round].push(m);
    }
    return (
      <Box>
        <Text size="sm" className={styles.bracketLabel}>
          {label}
        </Text>
        <Group align="flex-start" wrap="nowrap" gap="md">
          {Object.entries(rounds).map(([round, ms]) => (
            <Box key={round}>
              {ms.map((m) => (
                <Card key={m.id} padding="xs" mb="xs" withBorder className={styles.match}>
                  <Text size="xs" style={{ color: m.score1 != null ? 'var(--color-text)' : 'var(--color-text-muted)' }}>
                    {m.participant1Name || 'TBD'}
                  </Text>
                  <Text size="xs" style={{ color: m.score2 != null ? 'var(--color-text)' : 'var(--color-text-muted)' }}>
                    {m.participant2Name || 'TBD'}
                  </Text>
                  {m.score1 != null && (
                    <Text size="xs" className="gradient-text" fw={600}>
                      {m.score1}:{m.score2}
                    </Text>
                  )}
                </Card>
              ))}
            </Box>
          ))}
        </Group>
      </Box>
    );
  };

  return (
    <ScrollArea className={styles.scroll}>
      <Stack gap="xl">
        {renderSide(winners, 'Winners Bracket')}
        {losers.length > 0 && renderSide(losers, 'Losers Bracket')}
        {grand.length > 0 && renderSide(grand, 'Grand Final')}
      </Stack>
    </ScrollArea>
  );
}
