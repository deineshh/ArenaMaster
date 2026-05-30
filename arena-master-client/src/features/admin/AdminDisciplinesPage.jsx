import { Title, Table, Button, TextInput, Group, Stack } from '@mantine/core';
import { useState } from 'react';
import { useGetDisciplinesQuery } from '../../api/disciplinesApi';
import { useCreateDisciplineMutation, useDeleteDisciplineMutation } from '../../api/adminApi';
import { SkeletonCard } from '../../components/ui/SkeletonCard';

const tableHeaderStyle = { color: 'var(--color-primary)', borderBottom: '1px solid var(--color-border)' };
const tableCellStyle = { color: 'var(--color-text)', borderBottom: '1px solid var(--color-border)' };
const inputStyle = {
  input: { background: 'var(--color-bg)', borderColor: 'var(--color-border)', color: 'var(--color-text)' },
  inputFocus: { borderColor: 'var(--color-primary)' },
};

export default function AdminDisciplinesPage() {
  const { data, isLoading } = useGetDisciplinesQuery();
  const [create] = useCreateDisciplineMutation();
  const [deleteDisc] = useDeleteDisciplineMutation();
  const [name, setName] = useState('');

  if (isLoading) return <SkeletonCard type="table" />;

  return (
    <Stack>
      <Title order={2} className="gradient-text">Дисципліни</Title>
      <Group>
        <TextInput
          placeholder="Назва гри"
          value={name}
          onChange={(e) => setName(e.target.value)}
          styles={inputStyle}
        />
        <Button
          onClick={() => { create({ name }); setName(''); }}
          style={{ background: 'var(--brand-gradient)', transition: 'all var(--transition-normal)' }}
        >
          Додати
        </Button>
      </Group>
      <Table style={{ background: 'var(--color-bg-card)', borderRadius: 8, overflow: 'hidden' }}>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={tableHeaderStyle}>Назва</Table.Th>
            <Table.Th style={tableHeaderStyle}>Slug</Table.Th>
            <Table.Th style={tableHeaderStyle}>Дії</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {(data ?? []).map((d) => (
            <Table.Tr key={d.id}>
              <Table.Td style={tableCellStyle}>{d.name}</Table.Td>
              <Table.Td style={tableCellStyle}>{d.slug}</Table.Td>
              <Table.Td style={tableCellStyle}>
                <Button
                  size="xs"
                  style={{
                    background: 'transparent',
                    color: 'var(--color-error)',
                    border: '1px solid var(--color-error)',
                    transition: 'all var(--transition-normal)',
                  }}
                  onClick={() => deleteDisc(d.id)}
                >
                  Видалити
                </Button>
              </Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>
    </Stack>
  );
}
