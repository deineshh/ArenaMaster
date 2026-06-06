import { baseApi } from './baseApi';

export const authApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation({
      query: (body) => ({ url: '/auth/login', method: 'POST', body }),
    }),
    register: builder.mutation({
      query: (body) => ({ url: '/auth/register', method: 'POST', body }),
    }),
    logout: builder.mutation({
      query: () => ({ url: '/auth/logout', method: 'POST' }),
    }),
    getMe: builder.query({
      query: () => '/users/me',
      providesTags: ['User'],
    }),
    confirmEmail: builder.query({
      query: (token) => `/auth/confirm-email?token=${encodeURIComponent(token)}`,
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useLogoutMutation,
  useGetMeQuery,
  useConfirmEmailQuery,
} = authApi;
