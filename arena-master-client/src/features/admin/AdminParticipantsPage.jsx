import { Title, Table, Button, Badge } from '@mantine/core';
import { useState } from 'react';
import { useGetAdminParticipantsQuery, useDeleteParticipantMutation } from '../../api/adminApi';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { ConfirmModal } from '../../components/ui/ConfirmModal';

const tableHeaderStyle = { color: 'var(--color-primary)', borderBottom: '1px solid var(--color-border)' };
const tableCellStyle = { color: 'var(--color-text)', borderBottom: '1px solid var(--color-border)' };

export default function AdminParticipantsPage() {
  const { data, isLoading } = useGetAdminParticipantsQuery();
  const [deleteParticipant] = useDeleteParticipantMutation();
  const [deleteId, setDeleteId] = useState(null);

  if (isLoading) return <SkeletonCard type="table" />;

  return (
    <>
      <Title order={2} mb="lg" className="gradient-text">
        Учасники
      </Title>
      <Table style={{ background: 'var(--color-bg-card)', borderRadius: 8, overflow: 'hidden' }}>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={tableHeaderStyle}>Турнір</Table.Th>
            <Table.Th style={tableHeaderStyle}>Учасник</Table.Th>
            <Table.Th style={tableHeaderStyle}>Статус</Table.Th>
            <Table.Th style={tableHeaderStyle}>Сіяний</Table.Th>
            <Table.Th style={tableHeaderStyle}>Зареєстровано</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дії</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {(data ?? []).map((p) => (
            <Table.Tr key={p.id}>
              <Table.Td style={tableCellStyle}>{p.tournamentTitle}</Table.Td>
              <Table.Td style={tableCellStyle}>{p.name}</Table.Td>
              <Table.Td style={tableCellStyle}>
                <Badge
                  color={p.status === 'confirmed' ? 'green' : p.status === 'pending' ? 'yellow' : 'red'}
                  variant="light"
                >
                  {p.status === 'confirmed' ? 'Підтверджено' : p.status === 'pending' ? 'Очікує' : 'Відхилено'}
                </Badge>
              </Table.Td>
              <Table.Td style={tableCellStyle}>{p.seed ?? '-'}</Table.Td>
              <Table.Td style={tableCellStyle}>
                {p.registeredAt ? new Date(p.registeredAt).toLocaleDateString('uk-UA') : '-'}
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
                  onClick={() => setDeleteId(p.id)}
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
        title="Видалити учасника?"
        message="Ви впевнені, що хочете видалити цього учасника?"
        confirmLabel="Так"
        cancelLabel="Ні"
        color="red"
        onConfirm={() => deleteParticipant(deleteId)}
      />
    </>
  );
}
