import { createSlice } from '@reduxjs/toolkit';
import { authApi } from '../../api/authApi';

const authSlice = createSlice({
  name: 'auth',
  initialState: { user: null, token: null, status: 'idle' },
  reducers: {
    setCredentials: (state, action) => {
      state.user = action.payload.user;
      state.token = action.payload.token ?? null;
      state.status = 'authenticated';
    },
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.status = 'idle';
    },
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(authApi.endpoints.login.matchFulfilled, (state, { payload }) => {
        state.user = payload;
        state.status = 'authenticated';
        try { localStorage.setItem('auth', '1'); } catch {}
      })
      .addMatcher(authApi.endpoints.getMe.matchFulfilled, (state, { payload }) => {
        state.user = payload;
        state.status = 'authenticated';
      })
      .addMatcher(authApi.endpoints.getMe.matchRejected, () => {
        try { localStorage.removeItem('auth'); } catch {}
      })
      .addMatcher(authApi.endpoints.logout.matchFulfilled, (state) => {
        state.user = null;
        state.token = null;
        state.status = 'idle';
        try { localStorage.removeItem('auth'); } catch {}
      });
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;
