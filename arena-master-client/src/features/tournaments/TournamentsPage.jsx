import { useState } from 'react';
import { Container, SimpleGrid, Pagination, Stack, Text, Box, Grid, Badge, Button, Group, Title } from '@mantine/core';
import { IconTrophy, IconUsers, IconLivePhoto, IconDeviceGamepad, IconTrendingUp, IconArrowRight } from '@tabler/icons-react';
import { useGetTournamentsQuery } from '../../api/tournamentsApi';
import { TournamentCard } from './components/TournamentCard';
import { TournamentFilters } from './components/TournamentFilters';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { useAuth } from '../../hooks/useAuth';
import { uploadUrl } from '../../utils/bracketHelpers';
import { Link } from 'react-router';

export default function TournamentsPage() {
  const [filters, setFilters] = useState({ page: 1, pageSize: 12 });
  const { data, isLoading } = useGetTournamentsQuery(filters);
  const { isOrganizer } = useAuth();

  // 1. Calculate stats based on returned items for the Esports Dashboard
  const items = data?.items || [];
  const activeCount = items.filter(t => t.status === 'ongoing').length;
  const registrationCount = items.filter(t => t.status === 'registration').length;
  const totalPlayers = items.reduce((acc, t) => acc + (t.participantsCount || 0), 0);
  const maxCapacity = items.reduce((acc, t) => acc + (t.maxParticipants || 16), 0);
  const filledRatio = maxCapacity > 0 ? Math.round((totalPlayers / maxCapacity) * 100) : 0;

  // 2. Count tournaments per discipline to feed the SVG charts dynamically
  const disciplineCounts = {};
  items.forEach(t => {
    if (t.disciplineName) {
      disciplineCounts[t.disciplineName] = (disciplineCounts[t.disciplineName] || 0) + 1;
    }
  });

  const chartData = Object.entries(disciplineCounts)
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 3); // Top 3

  const totalChartCount = chartData.reduce((acc, c) => acc + c.count, 0) || 1;

  // 3. Extract a premium featured tournament (Ongoing preferred, then Registration)
  const featured = items.find(t => t.status === 'ongoing') || items.find(t => t.status === 'registration') || items[0];

  const getFormatLabel = (fmt) => {
    if (fmt === 'single_elimination') return 'Single Elimination';
    if (fmt === 'double_elimination') return 'Double Elimination';
    return fmt;
  };

  return (
    <Container size="lg" py="xl" className="fade-in" style={{ background: 'var(--color-bg)', minHeight: '100vh' }}>
      
      {/* Header Portal Title */}
      <Group justify="space-between" mb="xl" align="center">
        <div>
          <Title order={1} className="gradient-text" style={{
            fontFamily: 'Russo One, sans-serif',
            fontSize: '32px',
            letterSpacing: '1px',
            textShadow: '0 0 20px rgba(124, 58, 237, 0.2)'
          }}>
            АРЕНА ТУРНІРІВ
          </Title>
          <Text style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)', fontWeight: 500 }}>
            Елітні кіберспортивні змагання та професійні брекети
          </Text>
        </div>

        {isOrganizer && (
          <Button
            component={Link}
            to="/tournaments/create"
            style={{
              background: 'var(--brand-gradient)',
              fontFamily: 'Chakra Petch, sans-serif',
              fontWeight: 700,
              borderRadius: '8px',
              transition: 'transform var(--transition-normal), box-shadow var(--transition-normal)',
              boxShadow: 'var(--glow-primary)'
            }}
            className="hover-lift"
          >
            + Створити турнір
          </Button>
        )}
      </Group>

      {/* Esports Dashboard Hero Panel */}
      {!isLoading && items.length > 0 && (
        <Grid mb="xl" gutter="lg">
          {/* Column A: Dashboard Metrics */}
          <Grid.Col span={{ base: 12, md: 5 }}>
            <SimpleGrid cols={1} spacing="md">
              {/* Card 1: LIVE Matches */}
              <Box style={{
                background: 'var(--surface-secondary)',
                border: '1px solid var(--color-border-subtle)',
                borderRadius: '12px',
                padding: '16px 20px',
                boxShadow: '0 4px 20px rgba(0, 0, 0, 0.4)',
                position: 'relative',
                overflow: 'hidden'
              }}>
                <Group justify="space-between" align="center">
                  <Box>
                    <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', fontWeight: 600, color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
                      Матчі наживо
                    </Text>
                    <Title order={2} mt={4} style={{ fontFamily: 'Russo One, sans-serif', color: 'var(--color-text)', fontSize: '28px' }}>
                      {activeCount}
                    </Title>
                  </Box>
                  <Box style={{
                    width: '42px',
                    height: '42px',
                    borderRadius: '50%',
                    background: 'rgba(244, 63, 94, 0.1)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}>
                    {activeCount > 0 ? (
                      <span style={{
                        width: '12px',
                        height: '12px',
                        borderRadius: '50%',
                        background: 'var(--color-accent)',
                        animation: 'pulse 1.5s infinite'
                      }} />
                    ) : (
                      <IconLivePhoto color="var(--color-text-muted)" size={20} />
                    )}
                  </Box>
                </Group>
                <Text size="xs" mt="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <IconTrendingUp size={12} color="var(--color-accent)" />
                  {registrationCount} турнірів очікують набір гравців
                </Text>
              </Box>

              {/* Card 2: Competitor Engagement Ratio */}
              <Box style={{
                background: 'var(--surface-secondary)',
                border: '1px solid var(--color-border-subtle)',
                borderRadius: '12px',
                padding: '16px 20px',
                boxShadow: '0 4px 20px rgba(0, 0, 0, 0.4)',
              }}>
                <Group justify="space-between" align="center">
                  <Box style={{ flexGrow: 1 }}>
                    <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', fontWeight: 600, color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
                      Рівень заповненості арен
                    </Text>
                    <Group align="baseline" mt={4} gap="xs">
                      <Title order={2} style={{ fontFamily: 'Russo One, sans-serif', color: 'var(--color-text)', fontSize: '28px' }}>
                        {filledRatio}%
                      </Title>
                      <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)' }}>
                        ({totalPlayers} / {maxCapacity} гравців)
                      </Text>
                    </Group>
                  </Box>
                  <Box style={{
                    width: '42px',
                    height: '42px',
                    borderRadius: '50%',
                    background: 'rgba(124, 58, 237, 0.1)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}>
                    <IconUsers color="var(--color-primary)" size={20} />
                  </Box>
                </Group>
                <Box mt="xs" style={{ height: '6px', background: 'var(--surface-hover)', borderRadius: '3px', overflow: 'hidden' }}>
                  <Box style={{
                    width: `${filledRatio}%`,
                    height: '100%',
                    background: 'linear-gradient(90deg, #7C3AED 0%, #00bcff 100%)',
                    borderRadius: '3px',
                    transition: 'width 0.6s ease'
                  }} />
                </Box>
              </Box>
            </SimpleGrid>
          </Grid.Col>

          {/* Column B: Dynamic Discipline SVG chart */}
          <Grid.Col span={{ base: 12, md: 7 }}>
            <Box style={{
              background: 'var(--surface-secondary)',
              border: '1px solid var(--color-border-subtle)',
              borderRadius: '12px',
              padding: '20px 24px',
              height: '100%',
              boxShadow: '0 4px 20px rgba(0, 0, 0, 0.4)',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'center'
            }}>
              <Text size="xs" mb="md" style={{
                fontFamily: 'Chakra Petch, sans-serif',
                fontWeight: 600,
                color: 'var(--color-text-muted)',
                textTransform: 'uppercase',
                letterSpacing: '0.5px'
              }}>
                Популярні дисципліни за активністю
              </Text>
              
              {chartData.length > 0 ? (
                <Stack gap="sm">
                  {chartData.map(({ name, count }, index) => {
                    const percent = Math.round((count / totalChartCount) * 100);
                    // Cycle colors: purple, cyan, pink
                    const colors = ['#7C3AED', '#00bcff', '#F43F5E'];
                    const barColor = colors[index % colors.length];

                    return (
                      <Box key={name}>
                        <Group justify="space-between" mb={4}>
                          <Group gap={6} align="center">
                            <IconDeviceGamepad size={14} color={barColor} />
                            <Text size="sm" style={{ fontFamily: 'Chakra Petch, sans-serif', fontWeight: 600, color: 'var(--color-text)' }}>
                              {name}
                            </Text>
                          </Group>
                          <Text size="xs" style={{ fontFamily: 'Russo One, sans-serif', color: barColor }}>
                            {count} турніри ({percent}%)
                          </Text>
                        </Group>
                        {/* Horizontal dynamic SVG bars */}
                        <div style={{
                          height: '8px',
                          background: 'var(--surface-hover)',
                          borderRadius: '4px',
                          overflow: 'hidden',
                          position: 'relative'
                        }}>
                          <div style={{
                            width: `${percent}%`,
                            height: '100%',
                            background: barColor,
                            borderRadius: '4px',
                            boxShadow: `0 0 10px ${barColor}`,
                            transition: 'width 0.6s cubic-bezier(0.34, 1.56, 0.64, 1)'
                          }} />
                        </div>
                      </Box>
                    );
                  })}
                </Stack>
              ) : (
                <Text size="sm" style={{ color: 'var(--color-text-muted)', fontStyle: 'italic' }}>
                  Аналітика дисциплін буде доступна після додавання турнірів
                </Text>
              )}
            </Box>
          </Grid.Col>
        </Grid>
      )}

      {/* Featured Immersive Banner */}
      {!isLoading && featured && (
        <Box style={{
          position: 'relative',
          borderRadius: '12px',
          overflow: 'hidden',
          background: 'var(--surface-secondary)',
          border: '1px solid var(--color-border-subtle)',
          boxShadow: '0 8px 32px rgba(0, 0, 0, 0.5)',
          marginBottom: '32px'
        }}>
          <Grid gutter={0} align="stretch">
            {/* Banner Cover Cover */}
            <Grid.Col span={{ base: 12, md: 5 }} style={{ position: 'relative', minHeight: '220px' }}>
              {featured.coverUrl ? (
                <img
                  src={uploadUrl(featured.coverUrl)}
                  alt={featured.title}
                  style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }}
                />
              ) : (
                <Box style={{
                  width: '100%',
                  height: '100%',
                  background: 'linear-gradient(135deg, var(--surface-tertiary) 0%, var(--surface-secondary) 100%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  <IconTrophy size={60} color="var(--color-primary)" opacity={0.3} />
                </Box>
              )}
              {/* Scanline diagonal sweep */}
              <Box style={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                background: 'linear-gradient(90deg, rgba(21, 21, 48, 0) 60%, var(--surface-secondary) 100%), linear-gradient(rgba(0,0,0,0.1) 0%, rgba(0,0,0,0.6) 100%)',
                pointerEvents: 'none'
              }} />
              <Badge
                style={{
                  position: 'absolute',
                  top: '16px',
                  left: '16px',
                  boxShadow: '0 0 15px rgba(244, 63, 94, 0.4)'
                }}
                color="red"
                variant="filled"
                size="md"
              >
                РЕКОМЕНДОВАНИЙ
              </Badge>
            </Grid.Col>

            {/* Banner Text details */}
            <Grid.Col span={{ base: 12, md: 7 }} style={{
              padding: '24px 32px',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'center',
              gap: '12px'
            }}>
              <Group gap="xs">
                <Badge variant="outline" color="purple" size="sm">
                  {featured.disciplineName}
                </Badge>
                <Badge variant="outline" color="cyan" size="sm">
                  {getFormatLabel(featured.format)}
                </Badge>
              </Group>

              <Title order={2} style={{
                fontFamily: 'Chakra Petch, sans-serif',
                fontSize: '24px',
                fontWeight: 700,
                color: 'var(--color-text)',
                lineHeight: '1.2'
              }}>
                {featured.title}
              </Title>

              <Text size="sm" lineClamp={2} style={{ color: 'var(--color-text-muted)', fontFamily: 'Chakra Petch, sans-serif' }}>
                Приєднуйтесь до наймасштабнішої битви сезону! Більше десятка елітних команд уже зареєструвалися, щоб здолати суперників у захоплюючому турнірі брекетного формату.
              </Text>

              <Group justify="space-between" mt="xs" align="center">
                <Box>
                  <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)' }}>
                    Реєстрація заповнена на:
                  </Text>
                  <Text size="sm" style={{ fontFamily: 'Russo One, sans-serif', color: 'var(--color-text)' }}>
                    {featured.participantsCount || 0} / {featured.maxParticipants || 16} гравців
                  </Text>
                </Box>
                <Button
                  component={Link}
                  to={`/tournaments/${featured.slug}`}
                  rightSection={<IconArrowRight size={16} />}
                  style={{
                    background: 'var(--brand-gradient)',
                    borderRadius: '8px',
                    fontFamily: 'Chakra Petch, sans-serif',
                    fontWeight: 700,
                    boxShadow: 'var(--glow-primary)',
                    transition: 'all var(--transition-fast)'
                  }}
                  className="hover-lift"
                >
                  Детальніше
                </Button>
              </Group>
            </Grid.Col>
          </Grid>
        </Box>
      )}

      {/* Filters bar */}
      <TournamentFilters filters={filters} onChange={setFilters} />

      {/* Main Tournaments Grid */}
      {isLoading ? (
        <SkeletonCard count={6} type="card" />
      ) : items.length ? (
        <Stack gap="xl">
          <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="lg">
            {items.map((t) => (
              <TournamentCard key={t.id} tournament={t} />
            ))}
          </SimpleGrid>

          {/* Clean Pagination matching esports theme */}
          <Pagination
            value={filters.page}
            onChange={(page) => setFilters((f) => ({ ...f, page }))}
            total={Math.ceil((data.total || 0) / filters.pageSize)}
            justify="center"
            mt="xl"
            styles={{
              control: {
                background: 'var(--surface-secondary)',
                borderColor: 'var(--color-border-subtle)',
                color: 'var(--color-text)',
                fontFamily: 'Chakra Petch, sans-serif',
                fontWeight: 600,
                transition: 'all var(--transition-fast)',
                '&:hover': {
                  background: 'var(--surface-hover)',
                  borderColor: 'var(--color-primary)'
                }
              },
              active: {
                background: 'var(--brand-gradient)',
                border: 'none',
                boxShadow: 'var(--glow-primary)'
              },
            }}
          />
        </Stack>
      ) : (
        <EmptyState message="Турнірів не знайдено" />
      )}
    </Container>
  );
}
