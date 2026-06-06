import { Group, TextInput, Select } from '@mantine/core';
import { IconSearch, IconFilter, IconListDetails } from '@tabler/icons-react';
import { useGetDisciplinesQuery } from '../../../api/disciplinesApi';

const inputStyles = {
  input: {
    background: 'var(--surface-secondary)',
    borderColor: 'var(--color-border-subtle)',
    color: 'var(--color-text)',
    fontFamily: 'Chakra Petch, sans-serif',
    fontWeight: 500,
    transition: 'all var(--transition-fast)',
    '&:focus': {
      borderColor: 'var(--color-accent)',
      boxShadow: '0 0 10px rgba(244, 63, 94, 0.2)',
    }
  },
  dropdown: { 
    background: 'var(--surface-elevated)', 
    borderColor: 'var(--color-border-subtle)',
  },
  option: { 
    color: 'var(--color-text)', 
    fontFamily: 'Chakra Petch, sans-serif',
    transition: 'all var(--transition-fast)',
    '&[dataHovered]': { 
      background: 'rgba(244, 63, 94, 0.15)',
      color: 'var(--color-text)'
    },
    '&[dataSelected]': {
      background: 'var(--brand-gradient)',
      color: '#ffffff'
    }
  },
};

export function TournamentFilters({ filters, onChange }) {
  const { data: disciplines } = useGetDisciplinesQuery();

  return (
    <Group mb="xl" wrap="wrap" gap="md" style={{
      padding: '16px',
      background: 'var(--surface-secondary)',
      border: '1px solid var(--color-border-subtle)',
      borderRadius: '10px',
      boxShadow: 'var(--shadow-card)'
    }}>
      <TextInput
        placeholder="Пошук за назвою"
        leftSection={<IconSearch size={16} color="var(--color-text-muted)" />}
        value={filters.search || ''}
        onChange={(e) => onChange({ ...filters, search: e.target.value, page: 1 })}
        w={{ base: '100%', sm: 220 }}
        styles={inputStyles}
      />
      <Select
        placeholder="Дисципліна"
        clearable
        leftSection={<IconFilter size={16} color="var(--color-text-muted)" />}
        data={(disciplines ?? []).map((d) => ({ value: d.id, label: d.name }))}
        value={filters.disciplineId || null}
        onChange={(v) => onChange({ ...filters, disciplineId: v, page: 1 })}
        w={{ base: '100%', sm: 190 }}
        styles={inputStyles}
      />
      <Select
        placeholder="Формат"
        clearable
        leftSection={<IconListDetails size={16} color="var(--color-text-muted)" />}
        data={[
          { value: 'single_elimination', label: 'Single Elimination' },
          { value: 'double_elimination', label: 'Double Elimination' },
        ]}
        value={filters.format || null}
        onChange={(v) => onChange({ ...filters, format: v, page: 1 })}
        w={{ base: '100%', sm: 190 }}
        styles={inputStyles}
      />
      <Select
        placeholder="Статус"
        clearable
        leftSection={<IconFilter size={16} color="var(--color-text-muted)" />}
        data={[
          { value: 'registration', label: 'Реєстрація' },
          { value: 'ongoing', label: 'Триває' },
          { value: 'finished', label: 'Завершено' },
        ]}
        value={filters.status || null}
        onChange={(v) => onChange({ ...filters, status: v, page: 1 })}
        w={{ base: '100%', sm: 160 }}
        styles={inputStyles}
      />
      <Select
        placeholder="Сортування"
        leftSection={<IconListDetails size={16} color="var(--color-text-muted)" />}
        data={[
          { value: 'date', label: 'За датою' },
          { value: 'participants', label: 'За учасниками' },
        ]}
        value={filters.sort || 'date'}
        onChange={(v) => onChange({ ...filters, sort: v, page: 1 })}
        w={{ base: '100%', sm: 170 }}
        styles={inputStyles}
      />
    </Group>
  );
}
