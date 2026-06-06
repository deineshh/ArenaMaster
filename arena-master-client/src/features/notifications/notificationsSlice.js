import { createSlice } from '@reduxjs/toolkit';
import { notificationsApi } from '../../api/notificationsApi';

const notificationsSlice = createSlice({
  name: 'notifications',
  initialState: { items: [], unreadCount: 0 },
  reducers: {},
  extraReducers: (builder) => {
    builder.addMatcher(
      notificationsApi.endpoints.getNotifications.matchFulfilled,
      (state, { payload }) => {
        state.items = payload.items ?? [];
        state.unreadCount = payload.unread ?? 0;
      },
    );
  },
});

export default notificationsSlice.reducer;
