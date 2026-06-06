import { Skeleton, SimpleGrid, Stack } from '@mantine/core';

const skeletonBase = { background: 'var(--color-bg-elevated)' };

function SkeletonCardBase() {
  return (
    <div className="skeleton-pulse" style={{ borderRadius: 'var(--radius-card)', overflow: 'hidden' }}>
      <Skeleton height={140} radius="md" mb="sm" style={skeletonBase} />
      <Skeleton height={20} width="70%" radius="sm" mb="xs" style={skeletonBase} />
      <Skeleton height={14} width="50%" radius="sm" mb="xs" style={skeletonBase} />
      <Skeleton height={14} width="40%" radius="sm" style={skeletonBase} />
    </div>
  );
}

function SkeletonTeamCard() {
  return (
    <div className="skeleton-pulse" style={{ borderRadius: 'var(--radius-card)', padding: 16 }}>
      <Skeleton height={20} width="60%" radius="sm" mb="sm" style={skeletonBase} />
      <Skeleton height={14} width="40%" radius="sm" style={skeletonBase} />
    </div>
  );
}

function SkeletonProfile() {
  return (
    <Stack gap="md" className="skeleton-pulse">
      <Skeleton height={100} width={100} radius="md" style={skeletonBase} />
      <Skeleton height={24} width="40%" radius="sm" style={skeletonBase} />
      <Skeleton height={14} width="60%" radius="sm" style={skeletonBase} />
    </Stack>
  );
}

function SkeletonTable({ rows = 5 }) {
  return (
    <Stack gap="xs" className="skeleton-pulse">
      {Array.from({ length: rows }).map((_, i) => (
        <Skeleton key={i} height={32} radius="sm" style={skeletonBase} />
      ))}
    </Stack>
  );
}

export function SkeletonCard({ count = 1, type = 'card' }) {
  const Component = type === 'team' ? SkeletonTeamCard
    : type === 'profile' ? SkeletonProfile
    : type === 'table' ? SkeletonTable
    : SkeletonCardBase;

  if (type === 'table') return <SkeletonTable />;

  return (
    <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }}>
      {Array.from({ length: count }).map((_, i) => (
        <Component key={i} />
      ))}
    </SimpleGrid>
  );
}
