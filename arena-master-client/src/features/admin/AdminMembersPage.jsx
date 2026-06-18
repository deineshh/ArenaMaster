import { Title, Table, Button } from '@mantine/core';
import { useState } from 'react';
import { useGetAdminMembersQuery, useDeleteMemberMutation } from '../../api/adminApi';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { ConfirmModal } from '../../components/ui/ConfirmModal';

const tableHeaderStyle = { color: 'var(--color-primary)', borderBottom: '1px solid var(--color-border)' };
const tableCellStyle = { color: 'var(--color-text)', borderBottom: '1px solid var(--color-border)' };

export default function AdminMembersPage() {
  const { data, isLoading } = useGetAdminMembersQuery();
  const [deleteMember] = useDeleteMemberMutation();
  const [deleteId, setDeleteId] = useState(null);

  if (isLoading) return <SkeletonCard type="table" />;

  return (
    <>
      <Title order={2} mb="lg" className="gradient-text">
        Склад команд
      </Title>
      <Table style={{ background: 'var(--color-bg-card)', borderRadius: 8, overflow: 'hidden' }}>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={tableHeaderStyle}>Команда</Table.Th>
            <Table.Th style={tableHeaderStyle}>Користувач</Table.Th>
            <Table.Th style={tableHeaderStyle}>Роль</Table.Th>
            <Table.Th style={tableHeaderStyle}>Приєднався</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дії</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {(data ?? []).map((m) => (
            <Table.Tr key={m.id}>
              <Table.Td style={tableCellStyle}>{m.teamName}</Table.Td>
              <Table.Td style={tableCellStyle}>{m.username}</Table.Td>
              <Table.Td style={tableCellStyle}>{m.role}</Table.Td>
              <Table.Td style={tableCellStyle}>
                {m.joinedAt ? new Date(m.joinedAt).toLocaleDateString('uk-UA') : '-'}
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
        title="Видалити учасника команди?"
        message="Ви впевнені, що хочете видалити цього учасника з команди?"
        confirmLabel="Так"
        cancelLabel="Ні"
        color="red"
        onConfirm={() => deleteMember(deleteId)}
      />
    </>
  );
}
