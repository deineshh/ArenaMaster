import { Container, Title, Tabs, Text, Badge, Button, Stack, Anchor, Group, Box, Grid, Paper } from '@mantine/core';
import { useParams, Link } from 'react-router';
import {
  IconTrophy,
  IconCalendar,
  IconUsers,
  IconInfoCircle,
  IconLivePhoto,
  IconFlame,
  IconArrowLeft,
  IconClock
} from '@tabler/icons-react';
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
import { statusLabels } from '../../styles/component-tokens';

const getFormatLabel = (fmt) => {
  if (fmt === 'single_elimination') return 'Single Elimination';
  if (fmt === 'double_elimination') return 'Double Elimination';
  return fmt || '';
};

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

  // Helper to generate next power of two
  const nextPowerOfTwo = (n) => {
    let p = 1;
    while (p < n) p *= 2;
    return Math.max(p, 2);
  };

  // Helper to dynamically build a Mock Bracket populated with accepted participants
  const generateMockBracket = (acceptedParts) => {
    const list = acceptedParts || [];
    const size = Math.max(nextPowerOfTwo(list.length), 4); // Minimal 4 slots (2 matches in round 1)
    const mockMatches = [];
    const round1Count = size / 2;

    // Round 1
    for (let i = 0; i < round1Count; i++) {
      const p1 = list[i * 2];
      const p2 = list[i * 2 + 1];
      mockMatches.push({
        id: `mock-r1-${i}`,
        round: 1,
        matchNumber: i + 1,
        bracketSide: 'winners',
        participant1Id: p1?.id || null,
        participant1Name: p1?.userUsername || p1?.teamName || null,
        participant2Id: p2?.id || null,
        participant2Name: p2?.userUsername || p2?.teamName || null,
        score1: null,
        score2: null,
        winnerId: null,
        status: 'pending'
      });
    }

    // Subsequent rounds
    let prevRoundCount = round1Count;
    let roundNum = 2;
    while (prevRoundCount > 1) {
      const currentRoundCount = prevRoundCount / 2;
      for (let i = 0; i < currentRoundCount; i++) {
        mockMatches.push({
          id: `mock-r${roundNum}-${i}`,
          round: roundNum,
          matchNumber: i + 1,
          bracketSide: 'winners',
          participant1Id: null,
          participant1Name: null,
          participant2Id: null,
          participant2Name: null,
          score1: null,
          score2: null,
          winnerId: null,
          status: 'pending'
        });
      }
      prevRoundCount = currentRoundCount;
      roundNum++;
    }

    return mockMatches;
  };

  const accepted = (participants ?? []).filter((p) => p.status === 'accepted');
  const isBracketEmpty = !bracket || bracket.length === 0;
  const activeBracket = !isBracketEmpty ? bracket : generateMockBracket(accepted);

  return (
    <Container size="lg" py="xl" className="fade-in" style={{ background: 'var(--color-bg)', minHeight: '100vh' }}>
      
      {/* Back Button */}
      <Anchor component={Link} to="/tournaments" style={{
        display: 'flex',
        alignItems: 'center',
        gap: '6px',
        color: 'var(--color-text-muted)',
        fontFamily: 'Chakra Petch, sans-serif',
        fontSize: '14px',
        fontWeight: 600,
        marginBottom: '20px',
        textDecoration: 'none',
        transition: 'color var(--transition-fast)'
      }} className="hover-glow-accent">
        <IconArrowLeft size={16} /> Повернутися до списку
      </Anchor>

      {/* Esports Cover Hero Banner Banner */}
      <Box style={{
        position: 'relative',
        borderRadius: '12px',
        overflow: 'hidden',
        border: '1px solid var(--color-border-subtle)',
        background: 'var(--surface-secondary)',
        marginBottom: '32px',
        boxShadow: 'var(--shadow-elevated)'
      }}>
        {t.coverUrl ? (
          <CoverImage src={t.coverUrl} height={200} alt={t.title} />
        ) : (
          <Box style={{
            height: '200px',
            background: 'linear-gradient(135deg, var(--surface-tertiary) 0%, var(--surface-secondary) 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <IconTrophy size={80} color="var(--color-primary)" opacity={0.3} />
          </Box>
        )}
        {/* Widescreen Grid Overlay */}
        <Box style={{
          position: 'absolute',
          top: 0, left: 0, right: 0, bottom: 0,
          background: 'linear-gradient(to top, rgba(21, 21, 48, 0.95) 0%, rgba(21, 21, 48, 0.3) 100%)',
          pointerEvents: 'none'
        }} />

        {/* Floating Headers */}
        <Box style={{
          position: 'absolute',
          bottom: '24px',
          left: '24px',
          right: '24px',
          zIndex: 10
        }}>
          <Group justify="space-between" align="flex-end" wrap="wrap">
            <div>
              <Group gap="xs" mb={6}>
                <Badge color={t.status === 'ongoing' ? 'red' : 'purple'} variant="filled" style={{
                  fontFamily: 'Chakra Petch, sans-serif',
                  fontWeight: 700
                }}>
                  {statusLabels[t.status] || t.status}
                </Badge>
                <Badge variant="outline" color="cyan" style={{ fontFamily: 'Chakra Petch, sans-serif' }}>
                  {t.disciplineName}
                </Badge>
              </Group>
              <Title order={1} style={{
                fontFamily: 'Russo One, sans-serif',
                fontSize: '28px',
                color: 'var(--color-text)',
                textShadow: '0 2px 10px rgba(0,0,0,0.8)'
              }}>
                {t.title}
              </Title>
              <Text size="sm" mt={4} style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)' }}>
                Формат: {getFormatLabel(t.format)} · Організатор: {t.organizerUsername}
              </Text>
            </div>

            {t.status === 'registration' && isAuthenticated && (
              <Button
                onClick={handleRegister}
                style={{
                  background: 'var(--brand-gradient)',
                  fontFamily: 'Chakra Petch, sans-serif',
                  fontWeight: 700,
                  boxShadow: 'var(--glow-primary)',
                  transition: 'transform var(--transition-normal)'
                }}
                className="hover-lift"
              >
                Зареєструватися
              </Button>
            )}

            {t.status === 'ongoing' && t.streamUrl && (
              <Button
                component="a"
                href={t.streamUrl}
                target="_blank"
                leftSection={<IconLivePhoto size={16} />}
                style={{
                  background: '#e11d48',
                  fontFamily: 'Chakra Petch, sans-serif',
                  fontWeight: 700,
                  boxShadow: '0 0 15px rgba(225, 29, 72, 0.4)',
                  transition: 'transform var(--transition-normal)'
                }}
                className="hover-lift"
              >
                Дивитися трансляцію
              </Button>
            )}
          </Group>
        </Box>
      </Box>

      {/* Tabs list with esports high-contrast style */}
      <Tabs defaultValue="overview" styles={{
        tab: {
          color: 'var(--color-text-muted)',
          fontFamily: 'Chakra Petch, sans-serif',
          fontWeight: 600,
          fontSize: '15px',
          borderBottom: '2px solid transparent',
          transition: 'all var(--transition-fast)',
          '&[dataActive]': { color: 'var(--color-accent)', borderColor: 'var(--color-accent)' },
          '&:hover': { color: 'var(--color-text)' },
        },
        list: {
          borderColor: 'var(--color-border-subtle)',
          marginBottom: '24px'
        }
      }}>
        <Tabs.List>
          <Tabs.Tab value="overview" leftSection={<IconInfoCircle size={16} />}>{'Огляд'}</Tabs.Tab>
          <Tabs.Tab value="participants" leftSection={<IconUsers size={16} />}>{'Учасники'} ({accepted.length})</Tabs.Tab>
          <Tabs.Tab value="bracket" leftSection={<IconTrophy size={16} />}>
            {'Брекет'} {isBracketEmpty && "(Прев'ю)"}
          </Tabs.Tab>
          <Tabs.Tab value="schedule" leftSection={<IconClock size={16} />}>{'Розклад'}</Tabs.Tab>
        </Tabs.List>

        {/* Tab 1: Overview with dynamic charts */}
        <Tabs.Panel value="overview">
          <Grid gutter="xl">
            {/* Description & Schedule Details */}
            <Grid.Col span={{ base: 12, md: 7 }}>
              <Paper p="xl" style={{
                background: 'var(--surface-secondary)',
                border: '1px solid var(--color-border-subtle)',
                borderRadius: '12px',
                boxShadow: 'var(--shadow-card)'
              }}>
                <Title order={3} mb="md" style={{ fontFamily: 'Russo One, sans-serif', fontSize: '18px', color: 'var(--color-text)' }}>
                  Про турнір
                </Title>
                <Text style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text)', lineHeight: '1.6', whiteSpace: 'pre-wrap' }}>
                  {t.description || 'Опис турніру відсутній.'}
                </Text>

                {/* Timeline Schedule Node Grid */}
                <Title order={3} mt="xl" mb="md" style={{ fontFamily: 'Russo One, sans-serif', fontSize: '18px', color: 'var(--color-text)' }}>
                  Графік
                </Title>
                <Stack gap="md">
                  <Group wrap="nowrap" gap="md">
                    <Box style={{
                      width: '32px', height: '32px', borderRadius: '50%',
                      background: 'rgba(124, 58, 237, 0.1)',
                      display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0
                    }}>
                      <IconCalendar size={16} color="var(--color-primary)" />
                    </Box>
                    <div>
                      <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)', fontWeight: 600 }}>
                        КІНЕЦЬ РЕЄСТРАЦІЇ
                      </Text>
                      <Text size="sm" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text)', fontWeight: 600 }}>
                        {formatDate(t.registrationEndsAt)}
                      </Text>
                    </div>
                  </Group>

                  <Group wrap="nowrap" gap="md">
                    <Box style={{
                      width: '32px', height: '32px', borderRadius: '50%',
                      background: 'rgba(244, 63, 94, 0.1)',
                      display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0
                    }}>
                      <IconFlame size={16} color="var(--color-accent)" />
                    </Box>
                    <div>
                      <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)', fontWeight: 600 }}>
                        СТАРТ ЗМАГАНЬ
                      </Text>
                      <Text size="sm" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text)', fontWeight: 600 }}>
                        {formatDate(t.startsAt)}
                      </Text>
                    </div>
                  </Group>
                </Stack>
              </Paper>
            </Grid.Col>

            {/* Prize Podium Column Chart */}
            <Grid.Col span={{ base: 12, md: 5 }}>
              <Paper p="xl" style={{
                background: 'var(--surface-secondary)',
                border: '1px solid var(--color-border-subtle)',
                borderRadius: '12px',
                boxShadow: 'var(--shadow-card)',
                display: 'flex',
                flexDirection: 'column',
                height: '100%'
              }}>
                <Title order={3} mb="xl" style={{ fontFamily: 'Russo One, sans-serif', fontSize: '18px', color: 'var(--color-text)' }}>
                  Призи
                </Title>

                {t.prizes && t.prizes.length > 0 ? (
                  <Box style={{
                    flexGrow: 1,
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'flex-end',
                    paddingBottom: '20px'
                  }}>
                    {/* Podium Columns */}
                    <div style={{
                      display: 'flex',
                      alignItems: 'flex-end',
                      justifyContent: 'center',
                      gap: '16px',
                      borderBottom: '2px solid var(--color-border-subtle)',
                      paddingBottom: '8px'
                    }}>
                      {/* Place 2 (Silver) */}
                      {t.prizes.find(p => p.place === 2) && (
                        <div style={{
                          display: 'flex',
                          flexDirection: 'column',
                          alignItems: 'center',
                          width: '80px'
                        }}>
                          <Text size="xs" ta="center" style={{ fontFamily: 'Russo One, sans-serif', color: 'var(--color-text)', fontWeight: 'bold', marginBottom: '6px' }}>
                            {t.prizes.find(p => p.place === 2).description}
                          </Text>
                          <div style={{
                            width: '100%',
                            height: '70px',
                            background: 'linear-gradient(135deg, #e2e8f0 0%, #94a3b8 100%)',
                            borderRadius: '6px 6px 0 0',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            boxShadow: '0 0 15px rgba(226, 232, 240, 0.2)'
                          }}>
                            <IconTrophy size={20} color="var(--color-text-muted)" />
                          </div>
                          <Text size="xs" mt={4} style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)', fontWeight: 700 }}>
                            2 МІСЦЕ
                          </Text>
                        </div>
                      )}

                      {/* Place 1 (Gold) */}
                      {t.prizes.find(p => p.place === 1) && (
                        <div style={{
                          display: 'flex',
                          flexDirection: 'column',
                          alignItems: 'center',
                          width: '90px'
                        }}>
                          <Text size="sm" ta="center" style={{ fontFamily: 'Russo One, sans-serif', color: '#f59e0b', fontWeight: 'bold', marginBottom: '6px' }}>
                            {t.prizes.find(p => p.place === 1).description}
                          </Text>
                          <div style={{
                            width: '100%',
                            height: '100px',
                            background: 'linear-gradient(135deg, #ffe066 0%, #f59e0b 100%)',
                            borderRadius: '6px 6px 0 0',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            boxShadow: '0 0 25px rgba(245, 158, 11, 0.3)'
                          }}>
                            <IconTrophy size={28} color="#b45309" />
                          </div>
                          <Text size="xs" mt={4} style={{ fontFamily: 'Chakra Petch, sans-serif', color: '#f59e0b', fontWeight: 700 }}>
                            1 МІСЦЕ
                          </Text>
                        </div>
                      )}

                      {/* Place 3 (Bronze) */}
                      {t.prizes.find(p => p.place === 3) && (
                        <div style={{
                          display: 'flex',
                          flexDirection: 'column',
                          alignItems: 'center',
                          width: '80px'
                        }}>
                          <Text size="xs" ta="center" style={{ fontFamily: 'Russo One, sans-serif', color: '#ffc090', fontWeight: 'bold', marginBottom: '6px' }}>
                            {t.prizes.find(p => p.place === 3).description}
                          </Text>
                          <div style={{
                            width: '100%',
                            height: '50px',
                            background: 'linear-gradient(135deg, #ffc090 0%, #d97706 100%)',
                            borderRadius: '6px 6px 0 0',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            boxShadow: '0 0 15px rgba(217, 119, 6, 0.2)'
                          }}>
                            <IconTrophy size={18} color="#78350f" />
                          </div>
                          <Text size="xs" mt={4} style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)', fontWeight: 700 }}>
                            3 МІСЦЕ
                          </Text>
                        </div>
                      )}
                    </div>
                  </Box>
                ) : (
                  <Text size="sm" style={{ color: 'var(--color-text-muted)', fontStyle: 'italic' }}>
                    Списки призів будуть завантажені незабаром.
                  </Text>
                )}
              </Paper>
            </Grid.Col>
          </Grid>
        </Tabs.Panel>

        {/* Tab 2: Participants Grid */}
        <Tabs.Panel value="participants">
          <ParticipantsList participants={participants ?? []} />
        </Tabs.Panel>

        {/* Tab 3: Bracket Tree (Displays Mock Preview if unstarted) */}
        <Tabs.Panel value="bracket">
          {isBracketEmpty && (
            <Paper p="md" mb="lg" style={{
              background: 'rgba(124, 58, 237, 0.08)',
              border: '1px solid var(--color-primary)',
              borderRadius: '8px'
            }}>
              <Group gap="xs" align="flex-start" wrap="nowrap">
                <IconInfoCircle size={20} color="var(--color-primary)" style={{ flexShrink: 0, marginTop: '2px' }} />
                <div>
                  <Text size="sm" style={{ fontFamily: 'Chakra Petch, sans-serif', fontWeight: 700, color: 'var(--color-text)' }}>
                    {"ПОПЕРЕДНІЙ ПРЕВ'Ю БРЕКЕТ"}
                  </Text>
                  <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)' }}>
                    {"Реєстрація триває! Нижче наведено попередній вигляд сітки турніру на основі зареєстрованих учасників. Офіційний брекет буде сформовано після старту турніру."}
                  </Text>
                </div>
              </Group>
            </Paper>
          )}

          {t.format === 'double_elimination' ? (
            <BracketDouble matches={activeBracket} />
          ) : (
            <BracketSingle matches={activeBracket} />
          )}
        </Tabs.Panel>

        {/* Tab 4: Match Schedule */}
        <Tabs.Panel value="schedule">
          <Stack gap="sm">
            {activeBracket
              .filter((m) => m.scheduledAt || m.status === 'finished' || m.id.toString().startsWith('mock'))
              .map((m) => {
                const isP1Winner = m.score1 !== null && m.score2 !== null && m.score1 > m.score2;
                const isP2Winner = m.score1 !== null && m.score2 !== null && m.score2 > m.score1;

                return (
                  <Group key={m.id} justify="space-between" style={{
                    background: 'var(--surface-secondary)',
                    border: '1px solid var(--color-border-subtle)',
                    borderRadius: 8,
                    padding: '16px 20px',
                                boxShadow: '0 2px 10px rgba(0, 0, 0, 0.2)'
                  }}>
                    <Group gap="lg">
                      <Text style={{
                        fontFamily: 'Chakra Petch, sans-serif',
                        fontWeight: 600,
                        color: isP2Winner ? 'var(--color-text-muted)' : 'var(--color-text)'
                      }}>
                        {m.participant1Name || 'Очікується (TBD)'}
                      </Text>
                      <Text size="xs" style={{ color: 'var(--color-border)' }}>vs</Text>
                      <Text style={{
                        fontFamily: 'Chakra Petch, sans-serif',
                        fontWeight: 600,
                        color: isP1Winner ? 'var(--color-text-muted)' : 'var(--color-text)'
                      }}>
                        {m.participant2Name || 'Очікується (TBD)'}
                      </Text>
                    </Group>
                    <Box style={{ textAlign: 'right' }}>
                      <Text size="sm" style={{
                        fontFamily: 'Russo One, sans-serif',
                        color: 'var(--color-accent)',
                        fontWeight: 700
                      }}>
                        {m.score1 !== null ? `${m.score1}:${m.score2}` : '0:0'}
                      </Text>
                      {m.scheduledAt && (
                        <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)' }}>
                          {new Date(m.scheduledAt).toLocaleString('uk-UA', {
                            day: '2-digit',
                            month: 'short',
                            hour: '2-digit',
                            minute: '2-digit'
                          }).replace('р.', '')}
                        </Text>
                      )}
                    </Box>
                  </Group>
                );
              })}
          </Stack>
        </Tabs.Panel>
      </Tabs>
    </Container>
  );
}
