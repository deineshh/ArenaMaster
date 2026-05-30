import { useState } from 'react';
import { Container, SimpleGrid, Pagination, Stack } from '@mantine/core';
import { useGetTeamsQuery } from '../../api/teamsApi';
import { TeamCard } from './components/TeamCard';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { PageHeader } from '../../components/ui/PageHeader';
import { useAuth } from '../../hooks/useAuth';

export default function TeamsPage() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useGetTeamsQuery({ page });
  const { isAuthenticated } = useAuth();

  return (
    <Container size="lg" py="xl" className="fade-in">
      <PageHeader
        title="Команди"
        actionLabel={isAuthenticated ? 'Створити команду' : undefined}
        actionTo={isAuthenticated ? '/teams/create' : undefined}
      />
      {isLoading ? (
        <SkeletonCard count={6} type="team" />
      ) : (
        <Stack gap="lg">
          <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }}>
            {(data?.items ?? []).map((t) => (
              <TeamCard key={t.id} team={t} />
            ))}
          </SimpleGrid>
          <Pagination
            value={page}
            onChange={setPage}
            total={Math.ceil((data?.total || 0) / 12)}
            styles={{
              control: {
                background: 'var(--color-bg-card)',
                borderColor: 'var(--color-border)',
                color: 'var(--color-text)',
              },
              active: {
                background: 'var(--color-primary)',
                borderColor: 'var(--color-primary)',
              },
            }}
          />
        </Stack>
      )}
    </Container>
  );
}
