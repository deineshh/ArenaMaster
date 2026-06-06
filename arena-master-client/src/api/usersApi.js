import { baseApi } from './baseApi';

export const usersApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getProfile: builder.query({
      query: (username) => `/users/${username}`,
    }),
    updateProfile: builder.mutation({
      query: (body) => ({ url: '/users/me', method: 'PUT', body }),
      invalidatesTags: ['User'],
    }),
    uploadAvatar: builder.mutation({
      query: (file) => {
        const formData = new FormData();
        formData.append('file', file);
        return { url: '/users/me/avatar', method: 'POST', body: formData };
      },
      invalidatesTags: ['User'],
    }),
    getTournamentHistory: builder.query({
      query: (username) => `/users/${username}/tournaments`,
    }),
  }),
});

export const {
  useGetProfileQuery,
  useUpdateProfileMutation,
  useUploadAvatarMutation,
  useGetTournamentHistoryQuery,
} = usersApi;
