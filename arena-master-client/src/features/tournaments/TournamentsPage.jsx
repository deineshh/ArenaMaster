import { useState } from 'react';
import { Container, SimpleGrid, Pagination, Stack, Text } from '@mantine/core';
import { useGetTournamentsQuery } from '../../api/tournamentsApi';
import { TournamentCard } from './components/TournamentCard';
import { TournamentFilters } from './components/TournamentFilters';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { PageHeader } from '../../components/ui/PageHeader';
import { useAuth } from '../../hooks/useAuth';

export default function TournamentsPage() {
  const [filters, setFilters] = useState({ page: 1, pageSize: 12 });
  const { data, isLoading } = useGetTournamentsQuery(filters);
  const { isOrganizer } = useAuth();

  return (
    <Container size="lg" py="xl" className="fade-in">
      <PageHeader
        title="Турніри"
        actionLabel={isOrganizer ? '+ Створити турнір' : undefined}
        actionTo={isOrganizer ? '/tournaments/create' : undefined}
      />
      <TournamentFilters filters={filters} onChange={setFilters} />
      {isLoading ? (
        <SkeletonCard count={6} type="card" />
      ) : data?.items?.length ? (
        <Stack gap="lg">
          <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }}>
            {data.items.map((t) => (
              <TournamentCard key={t.id} tournament={t} />
            ))}
          </SimpleGrid>
          <Pagination
            value={filters.page}
            onChange={(page) => setFilters((f) => ({ ...f, page }))}
            total={Math.ceil((data.total || 0) / filters.pageSize)}
            styles={{
              control: {
                background: 'var(--color-bg-card)',
                borderColor: 'var(--color-border)',
                color: 'var(--color-text)',
                transition: 'all var(--transition-fast)',
              },
              active: {
                background: 'var(--color-primary)',
                borderColor: 'var(--color-primary)',
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
