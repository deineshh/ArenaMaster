import { Group, TextInput, Select } from '@mantine/core';
import { useGetDisciplinesQuery } from '../../../api/disciplinesApi';

const inputStyles = {
  input: {
    background: 'var(--color-bg-card)',
    borderColor: 'var(--color-border)',
    color: 'var(--color-text)',
    transition: 'all var(--transition-fast)',
  },
  inputFocus: { borderColor: 'var(--color-primary)' },
  dropdown: { background: 'var(--color-bg-card)', borderColor: 'var(--color-border)' },
  option: { color: 'var(--color-text)', '&:hover': { background: 'var(--color-primary-light)' } },
};

export function TournamentFilters({ filters, onChange }) {
  const { data: disciplines } = useGetDisciplinesQuery();

  return (
    <Group mb="lg" wrap="wrap">
      <TextInput
        placeholder="Пошук за назвою"
        value={filters.search || ''}
        onChange={(e) => onChange({ ...filters, search: e.target.value, page: 1 })}
        w={200}
        styles={inputStyles}
      />
      <Select
        placeholder="Дисципліна"
        clearable
        data={(disciplines ?? []).map((d) => ({ value: d.id, label: d.name }))}
        value={filters.disciplineId || null}
        onChange={(v) => onChange({ ...filters, disciplineId: v, page: 1 })}
        w={180}
        styles={inputStyles}
      />
      <Select
        placeholder="Формат"
        clearable
        data={[
          { value: 'single_elimination', label: 'Single Elimination' },
          { value: 'double_elimination', label: 'Double Elimination' },
        ]}
        value={filters.format || null}
        onChange={(v) => onChange({ ...filters, format: v, page: 1 })}
        w={180}
        styles={inputStyles}
      />
      <Select
        placeholder="Статус"
        clearable
        data={[
          { value: 'registration', label: 'Реєстрація' },
          { value: 'ongoing', label: 'Триває' },
          { value: 'finished', label: 'Завершено' },
        ]}
        value={filters.status || null}
        onChange={(v) => onChange({ ...filters, status: v, page: 1 })}
        w={150}
        styles={inputStyles}
      />
      <Select
        placeholder="Сортування"
        data={[
          { value: 'date', label: 'За датою' },
          { value: 'participants', label: 'За учасниками' },
        ]}
        value={filters.sort || 'date'}
        onChange={(v) => onChange({ ...filters, sort: v, page: 1 })}
        w={160}
        styles={inputStyles}
      />
    </Group>
  );
}
