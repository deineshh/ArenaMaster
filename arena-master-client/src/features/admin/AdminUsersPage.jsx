import { Title, Table, Button, Group, Select } from '@mantine/core';
import { useState } from 'react';
import {
  useGetUsersQuery,
  useToggleBlockMutation,
  useChangeRoleMutation,
  useDeleteUserMutation,
} from '../../api/adminApi';
import { SkeletonCard } from '../../components/ui/SkeletonCard';
import { ConfirmModal } from '../../components/ui/ConfirmModal';

const tableHeaderStyle = { color: 'var(--color-primary)', borderBottom: '1px solid var(--color-border)' };
const tableCellStyle = { color: 'var(--color-text)', borderBottom: '1px solid var(--color-border)' };

export default function AdminUsersPage() {
  const { data, isLoading } = useGetUsersQuery({});
  const [toggleBlock] = useToggleBlockMutation();
  const [changeRole] = useChangeRoleMutation();
  const [deleteUser] = useDeleteUserMutation();
  const [deleteId, setDeleteId] = useState(null);

  if (isLoading) return <SkeletonCard type="table" />;

  return (
    <>
      <Title order={2} mb="lg" className="gradient-text">
        Користувачі
      </Title>
      <Table highlightOnHover style={{ background: 'var(--color-bg-card)', borderRadius: 8, overflow: 'hidden' }}>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={tableHeaderStyle}>Нікнейм</Table.Th>
            <Table.Th style={tableHeaderStyle}>Email</Table.Th>
            <Table.Th style={tableHeaderStyle}>Роль</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дії</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {(data?.items ?? []).map((u) => (
            <Table.Tr key={u.id}>
              <Table.Td style={tableCellStyle}>{u.username}</Table.Td>
              <Table.Td style={tableCellStyle}>{u.email}</Table.Td>
              <Table.Td style={tableCellStyle}>
                <Select
                  size="xs"
                  value={u.role}
                  data={['player', 'organizer', 'admin']}
                  onChange={(role) => changeRole({ id: u.id, role })}
                  styles={{
                    input: { background: 'var(--color-bg)', borderColor: 'var(--color-border)', color: 'var(--color-text)' },
                  }}
                />
              </Table.Td>
              <Table.Td style={tableCellStyle}>
                <Group gap="xs">
                  <Button
                    size="xs"
                    style={{
                      background: u.isBlocked ? 'var(--color-success)' : 'var(--color-warning)',
                      color: 'white',
                      transition: 'all var(--transition-normal)',
                    }}
                    onClick={() => toggleBlock(u.id)}
                  >
                    {u.isBlocked ? 'Розблок.' : 'Блок'}
                  </Button>
                  <Button
                    size="xs"
                    style={{
                      background: 'transparent',
                      color: 'var(--color-error)',
                      border: '1px solid var(--color-error)',
                      transition: 'all var(--transition-normal)',
                    }}
                    onClick={() => setDeleteId(u.id)}
                  >
                    Видалити
                  </Button>
                </Group>
              </Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>

      <ConfirmModal
        opened={!!deleteId}
        onClose={() => setDeleteId(null)}
        title="Підтвердження"
        message="Видалити користувача?"
        confirmLabel="Так"
        cancelLabel="Скасувати"
        color="red"
        onConfirm={() => deleteUser(deleteId)}
      />
    </>
  );
}
