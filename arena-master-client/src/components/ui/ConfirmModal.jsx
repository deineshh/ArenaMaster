import { Modal, Text, Group, Button } from '@mantine/core';

export function ConfirmModal({ opened, onClose, title, message, onConfirm, confirmLabel = 'Так', cancelLabel = 'Скасувати', color = 'red' }) {
  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={title}
      styles={{
        header: { borderBottom: '1px solid var(--color-border)', paddingBottom: 12 },
        title: { color: 'var(--color-text)', fontWeight: 600 },
        body: { paddingTop: 16 },
        content: { background: 'var(--color-bg-card)' },
      }}
    >
      {message && (
        <Text mb="md" style={{ color: 'var(--color-text-muted)' }}>{message}</Text>
      )}
      <Group>
        <Button
          style={{
            background: color === 'red' ? 'var(--color-error)' : color === 'green' ? 'var(--color-success)' : 'var(--color-primary)',
          }}
          onClick={() => { onConfirm(); onClose(); }}
        >
          {confirmLabel}
        </Button>
        <Button
          variant="default"
          onClick={onClose}
          style={{
            background: 'var(--color-bg)',
            borderColor: 'var(--color-border)',
            color: 'var(--color-text)',
          }}
        >
          {cancelLabel}
        </Button>
      </Group>
    </Modal>
  );
}
