import { Title, Table, Button } from '@mantine/core';
import { useState } from 'react';
import { useGetAdminTournamentsQuery, useDeleteTournamentMutation } from '../../api/adminApi';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { ConfirmModal } from '../../components/ui/ConfirmModal';
import { Link } from 'react-router';

const tableHeaderStyle = { color: 'var(--color-primary)', borderBottom: '1px solid var(--color-border)' };
const tableCellStyle = { color: 'var(--color-text)', borderBottom: '1px solid var(--color-border)' };

export default function AdminTournamentsPage() {
  const { data, isLoading } = useGetAdminTournamentsQuery();
  const [deleteTournament] = useDeleteTournamentMutation();
  const [deleteId, setDeleteId] = useState(null);

  if (isLoading) return <SkeletonCard type="table" />;

  return (
    <>
      <Title order={2} mb="lg" className="gradient-text">
        Турніри
      </Title>
      <Table style={{ background: 'var(--color-bg-card)', borderRadius: 8, overflow: 'hidden' }}>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={tableHeaderStyle}>Назва</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дисципліна</Table.Th>
            <Table.Th style={tableHeaderStyle}>Статус</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дії</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {(data ?? []).map((t) => (
            <Table.Tr key={t.id}>
              <Table.Td style={tableCellStyle}>
                <Button
                  component={Link}
                  to={`/tournaments/${t.slug}`}
                  variant="subtle"
                  size="compact-sm"
                  style={{ color: 'var(--color-primary)' }}
                >
                  {t.title}
                </Button>
              </Table.Td>
              <Table.Td style={tableCellStyle}>{t.discipline}</Table.Td>
              <Table.Td style={tableCellStyle}>{t.status}</Table.Td>
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
        title="Видалити турнір?"
        message="Ви впевнені, що хочете видалити цей турнір?"
        confirmLabel="Так"
        cancelLabel="Ні"
        color="red"
        onConfirm={() => deleteTournament(deleteId)}
      />
    </>
  );
}
