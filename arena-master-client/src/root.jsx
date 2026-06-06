import { Links, Meta, Outlet, Scripts, ScrollRestoration } from 'react-router';
import { MantineProvider } from '@mantine/core';
import { Notifications } from '@mantine/notifications';
import { Provider } from 'react-redux';
import { store } from './app/store';
import { theme } from './app/theme';
import '@mantine/core/styles.css';
import '@mantine/notifications/styles.css';
import './styles/tokens.css';
import './styles/transitions.css';

export function Layout({ children }) {
  return (
    <html lang="uk">
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <Meta />
        <Links />
        <link rel="icon" type="image/svg+xml" href="/favicon.svg" />
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
        <link
          href="https://fonts.googleapis.com/css2?family=Chakra+Petch:wght@400;500;600;700&family=Russo+One&display=swap"
          rel="stylesheet"
        />
      </head>
      <body>
        <Provider store={store}>
          <MantineProvider theme={theme} defaultColorScheme="dark">
            <Notifications position="top-right" />
            {children}
          </MantineProvider>
        </Provider>
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export default function Root() {
  return <Outlet />;
}
