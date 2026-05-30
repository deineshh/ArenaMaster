import { Box, Text, ScrollArea, Tooltip } from '@mantine/core';
import { IconInfoCircle } from '@tabler/icons-react';
import styles from './Bracket.module.css';

// Esport country flag list for high-fidelity placeholders
const COUNTRIES = ['ua', 'us', 'pl', 'de', 'gb', 'se', 'dk', 'fr', 'kr', 'ca', 'br', 'es', 'fi', 'no'];

const getParticipantCountry = (name) => {
  if (!name || name === 'TBD' || name === 'Очікується') return 'us';
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  const index = Math.abs(hash) % COUNTRIES.length;
  return COUNTRIES[index];
};

export function BracketSingle({ matches }) {
  if (!matches || matches.length === 0) {
    return (
      <Box p="xl" ta="center">
        <Text style={{ color: 'var(--color-text-muted)' }}>Брекет ще не згенеровано</Text>
      </Box>
    );
  }

  // 1. Group matches by round
  const roundsMap = {};
  for (const m of matches) {
    if (!roundsMap[m.round]) roundsMap[m.round] = [];
    roundsMap[m.round].push(m);
  }

  // 2. Sort rounds and sort matches in each round by matchNumber
  const sortedRounds = Object.entries(roundsMap)
    .sort(([rA], [rB]) => Number(rA) - Number(rB))
    .map(([roundNum, ms]) => {
      const sortedMs = [...ms].sort((mA, mB) => mA.matchNumber - mB.matchNumber);
      return {
        round: Number(roundNum),
        matches: sortedMs,
      };
    });

  const totalRounds = sortedRounds.length;

  // Layout parameters for mathematical connector line calculations
  const cardHeight = 60; // Height of card (30px * 2 rows)
  const gap = 34;       // Spacing between cards in Round 1

  // Dynamic round labels matching screenshot
  const getRoundLabel = (rIndex, total) => {
    const diff = total - rIndex - 1;
    if (diff === 0) return 'FINAL';
    if (diff === 1) return '1/2';
    if (diff === 2) return '1/4';
    if (diff === 3) return '1/8';
    return `ROUND ${rIndex + 1}`;
  };

  const renderFlag = (name) => {
    if (!name || name === 'TBD' || name === 'Очікується') {
      return (
        <div className={styles.flagPlaceholder}>
          <svg className={styles.flagPlaceholderSvg} viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 2L3 6v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V6l-9-4z" />
          </svg>
        </div>
      );
    }
    return (
      <img
        className={styles.flagBadge}
        src={`https://flagcdn.com/w40/${getParticipantCountry(name)}.png`}
        alt="Flag"
        onError={(e) => {
          e.target.style.display = 'none';
        }}
      />
    );
  };

  return (
    <ScrollArea className={styles.scroll}>
      <div className={styles.bracketContainer}>
        {sortedRounds.map(({ round, matches: roundMatches }, rIndex) => {
          const roundOffset = rIndex; // 0-indexed
          const blockSize = (cardHeight + gap) * Math.pow(2, roundOffset);
          const verticalMargin = (blockSize - cardHeight) / 2;

          return (
            <div key={round} className={styles.roundColumn}>
              <div className={styles.roundHeader}>
                {getRoundLabel(rIndex, totalRounds)}
              </div>
              <div className={styles.matchList}>
                {roundMatches.map((m) => {
                  const isTopMatch = m.matchNumber % 2 !== 0;
                  const hasWinner = m.winnerId !== null;
                  
                  const isP1Winner = m.score1 !== null && m.score2 !== null && m.score1 > m.score2;
                  const isP2Winner = m.score1 !== null && m.score2 !== null && m.score2 > m.score1;

                  // Vertical connector line height
                  const verticalLineHeight = blockSize / 2;

                  return (
                    <div
                      key={m.id}
                      className={styles.matchWrapper}
                      style={{ margin: `${verticalMargin}px 0` }}
                    >
                      {/* Left horizontal connector */}
                      {rIndex > 0 && (
                        <div
                          className={`${styles.connectorLine} ${styles.connectorLineHorizontal}`}
                          style={{
                            width: '25px',
                            left: '-25px',
                            top: '30px', // cardHeight / 2
                          }}
                        />
                      )}

                      {/* Right horizontal connector */}
                      {rIndex < totalRounds - 1 && (
                        <>
                          <div
                            className={`${styles.connectorLine} ${styles.connectorLineHorizontal} ${
                              hasWinner ? styles.connectorLineActive : ''
                            }`}
                            style={{
                              width: '25px',
                              right: '-25px',
                              top: '30px',
                            }}
                          />
                          <div
                            className={`${styles.connectorLine} ${styles.connectorLineVertical} ${
                              hasWinner ? styles.connectorLineActive : ''
                            }`}
                            style={{
                              height: `${verticalLineHeight}px`,
                              right: '-25px',
                              top: isTopMatch ? '30px' : 'auto',
                              bottom: !isTopMatch ? '30px' : 'auto',
                            }}
                          />
                        </>
                      )}

                      {/* Esports Match Card */}
                      <div
                        className={`${styles.matchCard} ${
                          m.status === 'ongoing' ? styles.matchCardActive : ''
                        }`}
                      >
                        {/* Centered Single Info Circle Icon */}
                        {m.participant1Name && m.participant2Name && (
                          <Tooltip label="Деталі матчу" position="top">
                            <span className={styles.infoButton}>
                              <IconInfoCircle size={10} />
                            </span>
                          </Tooltip>
                        )}

                        {/* Team 1 Row */}
                        <div className={styles.teamRow}>
                          <div className={styles.teamInfo}>
                            {renderFlag(m.participant1Name)}
                            <Text
                              className={`${styles.teamName} ${
                                !m.participant1Name ? styles.teamMuted : ''
                              } ${isP2Winner ? styles.teamLost : ''}`}
                            >
                              {m.participant1Name || 'TBD'}
                            </Text>
                          </div>
                          <div
                            className={`${styles.scoreBox} ${
                              isP1Winner ? styles.scoreWinner : ''
                            }`}
                          >
                            {m.score1 !== null ? m.score1 : 0}
                          </div>
                        </div>

                        {/* Team 2 Row */}
                        <div className={styles.teamRow}>
                          <div className={styles.teamInfo}>
                            {renderFlag(m.participant2Name)}
                            <Text
                              className={`${styles.teamName} ${
                                !m.participant2Name ? styles.teamMuted : ''
                              } ${isP1Winner ? styles.teamLost : ''}`}
                            >
                              {m.participant2Name || 'TBD'}
                            </Text>
                          </div>
                          <div
                            className={`${styles.scoreBox} ${
                              isP2Winner ? styles.scoreWinner : ''
                            }`}
                          >
                            {m.score2 !== null ? m.score2 : 0}
                          </div>
                        </div>
                      </div>

                      {/* Match Scheduled Date */}
                      {m.scheduledAt && (
                        <div className={styles.matchDate}>
                          {new Date(m.scheduledAt).toLocaleString('uk-UA', {
                            day: '2-digit',
                            month: 'short',
                            hour: '2-digit',
                            minute: '2-digit',
                          }).replace('р.', '')}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </ScrollArea>
  );
}
