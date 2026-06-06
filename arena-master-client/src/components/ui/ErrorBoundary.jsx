import { Component } from 'react';
import { Alert } from '@mantine/core';

export class ErrorBoundary extends Component {
  state = { hasError: false };

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  render() {
    if (this.state.hasError) {
      return (
        <Alert
          color="red"
          title="Помилка"
          styles={{
            root: { background: 'var(--color-bg-card)', border: '1px solid var(--color-error)' },
            title: { color: 'var(--color-error)' },
            message: { color: 'var(--color-text-muted)' },
          }}
        >
          Щось пішло не так. Спробуйте оновити сторінку.
        </Alert>
      );
    }
    return this.props.children;
  }
}
