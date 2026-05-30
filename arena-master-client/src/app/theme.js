import { createTheme } from '@mantine/core';

const purple = [
  '#F3E8FF', '#E9D5FF', '#D8B4FE', '#C084FC', '#A855F7',
  '#8B5CF6', '#7C3AED', '#6D28D9', '#5B21B6', '#4C1D95',
];

const accent = [
  '#FFF1F2', '#FFE4E6', '#FECDD3', '#FDA4AF', '#FB7185',
  '#F43F5E', '#E11D48', '#BE123C', '#9F1239', '#881337',
];

const dark = [
  '#E2E8F0', '#C8CED8', '#AEB4C0', '#949AA8', '#7A8090',
  '#606678', '#464C60', '#2C3248', '#1A1A3E', '#0F0F23',
];

export const theme = createTheme({
  primaryColor: 'purple',
  colors: { purple, accent, dark },
  defaultRadius: 'md',
  fontFamily: "'Chakra Petch', sans-serif",
  fontFamilyMonospace: "'Chakra Petch', monospace",
  headings: {
    fontFamily: "'Russo One', sans-serif",
    fontWeight: '400',
  },
  primaryShade: 6,
  components: {
    Card: {
      defaultProps: {
        bg: 'dark.8',
      },
    },
    Table: {
      defaultProps: {
        bg: 'dark.8',
      },
    },
    Paper: {
      defaultProps: {
        bg: 'dark.8',
      },
    },
    Modal: {
      defaultProps: {
        bg: 'dark.8',
      },
    },
    Button: {
      defaultProps: {
        className: 'transition-all',
      },
    },
  },
});
