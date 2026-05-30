import { Outlet } from 'react-router';
import { AppShell as MantineAppShell } from '@mantine/core';
import { Header } from './Header';
import { Footer } from './Footer';
import styles from './AppShell.module.css';

export default function AppShellLayout() {
  return (
    <MantineAppShell
      header={{ height: 64 }}
      padding={{ base: 'sm', sm: 'md' }}
      className={styles.shell}
    >
      <MantineAppShell.Header>
        <Header />
      </MantineAppShell.Header>
      <MantineAppShell.Main className={styles.main}>
        <div className="fade-in">
          <Outlet />
        </div>
        <Footer />
      </MantineAppShell.Main>
    </MantineAppShell>
  );
}
