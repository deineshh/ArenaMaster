import { Title, Table, Button } from '@mantine/core';
import { useState } from 'react';
import { useGetTeamsQuery } from '../../api/teamsApi';
import { useDeleteTeamMutation } from '../../api/adminApi';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { ConfirmModal } from '../../components/ui/ConfirmModal';

const tableHeaderStyle = { color: 'var(--color-primary)', borderBottom: '1px solid var(--color-border)' };
const tableCellStyle = { color: 'var(--color-text)', borderBottom: '1px solid var(--color-border)' };

export default function AdminTeamsPage() {
  const { data, isLoading } = useGetTeamsQuery({ page: 1, pageSize: 100 });
  const [deleteTeam] = useDeleteTeamMutation();
  const [deleteId, setDeleteId] = useState(null);

  if (isLoading) return <SkeletonCard type="table" />;

  return (
    <>
      <Title order={2} mb="lg" className="gradient-text">
        Команди
      </Title>
      <Table style={{ background: 'var(--color-bg-card)', borderRadius: 8, overflow: 'hidden' }}>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={tableHeaderStyle}>Назва</Table.Th>
            <Table.Th style={tableHeaderStyle}>Капітан</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дії</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {(data?.items ?? []).map((t) => (
            <Table.Tr key={t.id}>
              <Table.Td style={tableCellStyle}>{t.name}</Table.Td>
              <Table.Td style={tableCellStyle}>{t.captainUsername}</Table.Td>
              <Table.Td style={tableCellStyle}>
                <Button
                  size="xs"
                  style={{
                    background: 'transparent',
                    color: 'var(--color-error)',
                    border: '1px solid var(--color-error)',
                    transition: 'all var(--transition-normal)',
                  }}
                  onClick={() => setDeleteId(t.id)}
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
        title="Видалити команду?"
        message="Ви впевнені, що хочете видалити цю команду?"
        confirmLabel="Так"
        cancelLabel="Ні"
        color="red"
        onConfirm={() => deleteTeam(deleteId)}
      />
    </>
  );
}
