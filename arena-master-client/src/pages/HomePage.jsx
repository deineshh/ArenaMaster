import { Container, Title, Text, Button, Group, SimpleGrid, Image, Stack, Box, Card } from '@mantine/core';
import { Link } from 'react-router';
import { useGetTournamentsQuery } from '../api/tournamentsApi';
import { useGetDisciplinesQuery } from '../api/disciplinesApi';
import { uploadUrl } from '../utils/bracketHelpers';
import { SkeletonCard } from '../components/ui/SkeletonCard';

export default function HomePage() {
  const { data, isLoading } = useGetTournamentsQuery({
    page: 1,
    pageSize: 6,
    status: 'registration',
  });
  const { data: discList } = useGetDisciplinesQuery();

  return (
    <>
      <Box
        className="bg-pattern-grid"
        style={{
          background: 'linear-gradient(135deg, #0F0F23 0%, #1A1A3E 50%, #0F0F23 100%), linear-gradient(rgba(124, 58, 237, 0.06) 1px, transparent 1px), linear-gradient(90deg, rgba(124, 58, 237, 0.06) 1px, transparent 1px)',
          backgroundBlendMode: 'normal',
          backgroundSize: 'auto, 48px 48px, 48px 48px',
          paddingTop: 100,
          paddingBottom: 100,
          position: 'relative',
          overflow: 'hidden',
        }}
      >
        <Box
          style={{
            position: 'absolute',
            top: '-50%',
            left: '-50%',
            width: '200%',
            height: '200%',
            background: 'radial-gradient(circle at 50% 50%, rgba(124, 58, 237, 0.08) 0%, transparent 50%)',
            animation: 'neonPulse 4s ease-in-out infinite',
            pointerEvents: 'none',
          }}
        />
        <Container size="lg" style={{ position: 'relative', zIndex: 1 }}>
          <Stack align="center" gap="lg" className="fade-in">
            <Title
              order={1}
              ta="center"
              className="gradient-text"
              style={{
                fontSize: 52,
                letterSpacing: '-0.02em',
                filter: 'drop-shadow(0 0 30px rgba(124, 58, 237, 0.3))',
                lineHeight: 1.1,
              }}
            >
              ArenaMaster
            </Title>
            <Text
              size="xl"
              ta="center"
              maw={600}
              style={{ color: 'var(--color-text)', opacity: 0.85 }}
            >
              Платформа для організації аматорських кіберспортивних турнірів
            </Text>
            <Group>
              <Button
                component={Link}
                to="/tournaments"
                size="lg"
                style={{
                  background: 'var(--brand-gradient)',
                  boxShadow: 'var(--glow-primary)',
                  transition: 'all var(--transition-normal)',
                }}
              >
                Переглянути турніри
              </Button>
              <Button
                component={Link}
                to="/register"
                variant="outline"
                size="lg"
                style={{
                  borderColor: 'var(--color-primary)',
                  color: 'var(--color-primary)',
                  transition: 'all var(--transition-normal)',
                }}
              >
                Зареєструватися
              </Button>
            </Group>
          </Stack>
        </Container>
      </Box>

      <Container size="lg" py={60}>
        <Title
          order={2}
          mb="md"
          className="fade-in"
          style={{ color: 'var(--color-text)' }}
        >
          Активні турніри
        </Title>
        {isLoading ? (
          <SkeletonCard count={6} type="card" />
        ) : (
          <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }}>
            {(data?.items ?? []).map((t) => (
              <Card
                key={t.id}
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
                    <Image src={uploadUrl(t.coverUrl)} h={120} alt={t.title} className="transition-all" />
                  </Card.Section>
                )}
                <Text fw={600} mt="sm" c="dark.0">
                  {t.title}
                </Text>
                <Text size="sm" c="dark.2">
                  {t.disciplineName} · {t.participantsCount}/{t.maxParticipants}
                </Text>
              </Card>
            ))}
          </SimpleGrid>
        )}
      </Container>

      <Container size="lg" pb={80}>
        <Title
          order={2}
          mb="md"
          className="fade-in"
          style={{ color: 'var(--color-text)' }}
        >
          Популярні дисципліни
        </Title>
        <SimpleGrid cols={{ base: 2, sm: 3, md: 5 }}>
          {(discList ?? []).map((d, idx) => (
            <Card
              key={d.id}
              padding="md"
              withBorder
              ta="center"
              className="fade-in"
              style={{ cursor: 'default' }}
            >
              <Text fw={500} className="gradient-text">{d.name}</Text>
            </Card>
          ))}
        </SimpleGrid>
      </Container>
    </>
  );
}
