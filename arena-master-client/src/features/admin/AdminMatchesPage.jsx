import { Title, Table, Button, Badge } from '@mantine/core';
import { useState } from 'react';
import { useGetAdminMatchesQuery, useDeleteMatchMutation } from '../../api/adminApi';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { ConfirmModal } from '../../components/ui/ConfirmModal';

const tableHeaderStyle = { color: 'var(--color-primary)', borderBottom: '1px solid var(--color-border)' };
const tableCellStyle = { color: 'var(--color-text)', borderBottom: '1px solid var(--color-border)' };

export default function AdminMatchesPage() {
  const { data, isLoading } = useGetAdminMatchesQuery();
  const [deleteMatch] = useDeleteMatchMutation();
  const [deleteId, setDeleteId] = useState(null);

  if (isLoading) return <SkeletonCard type="table" />;

  return (
    <>
      <Title order={2} mb="lg" className="gradient-text">
        Матчі
      </Title>
      <Table style={{ background: 'var(--color-bg-card)', borderRadius: 8, overflow: 'hidden' }}>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={tableHeaderStyle}>Турнір</Table.Th>
            <Table.Th style={tableHeaderStyle}>Раунд</Table.Th>
            <Table.Th style={tableHeaderStyle}>Матч #</Table.Th>
            <Table.Th style={tableHeaderStyle}>Учасники</Table.Th>
            <Table.Th style={tableHeaderStyle}>Рахунок</Table.Th>
            <Table.Th style={tableHeaderStyle}>Статус</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дії</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {(data ?? []).map((m) => (
            <Table.Tr key={m.id}>
              <Table.Td style={tableCellStyle}>{m.tournamentTitle}</Table.Td>
              <Table.Td style={tableCellStyle}>{m.round}</Table.Td>
              <Table.Td style={tableCellStyle}>{m.matchNumber}</Table.Td>
              <Table.Td style={tableCellStyle}>
                {m.participant1Name} vs {m.participant2Name}
              </Table.Td>
              <Table.Td style={tableCellStyle}>{m.score1 != null ? `${m.score1} : ${m.score2 ?? '-'}` : '-'}</Table.Td>
              <Table.Td style={tableCellStyle}>
                <Badge
                  color={m.status === 'completed' ? 'green' : m.status === 'ongoing' ? 'yellow' : 'gray'}
                  variant="light"
                >
                  {m.status === 'completed' ? 'Завершено' : m.status === 'ongoing' ? 'В процесі' : 'Заплановано'}
                </Badge>
              </Table.Td>
              <Table.Td style={tableCellStyle}>
                <Button
                  size="xs"
                  style={{
                    background: 'transparent',
                    color: 'var(--color-error)',
                    border: '1px solid var(--color-error)',
                    transition: 'all var(--transition-normal)',
                  }}
                  onClick={() => setDeleteId(m.id)}
                >
                  Видалити
                </Button>
              </Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>

      <ConfirmModal
        opened={!!deleteId}
        onClose={() => setDeleteId(null)}
        title="Видалити матч?"
        message="Ви впевнені, що хочете видалити цей матч?"
        confirmLabel="Так"
        cancelLabel="Ні"
        color="red"
        onConfirm={() => deleteMatch(deleteId)}
      />
    </>
  );
}
