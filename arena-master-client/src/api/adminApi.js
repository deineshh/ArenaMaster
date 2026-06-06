import { baseApi } from './baseApi';

export const adminApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getUsers: builder.query({
      query: ({ page = 1 } = {}) => `/admin/users?page=${page}`,
      providesTags: ['Admin'],
    }),
    toggleBlock: builder.mutation({
      query: (id) => ({ url: `/admin/users/${id}/block`, method: 'PATCH' }),
      invalidatesTags: ['Admin'],
    }),
    changeRole: builder.mutation({
      query: ({ id, role }) => ({ url: `/admin/users/${id}/role`, method: 'PATCH', body: { role } }),
      invalidatesTags: ['Admin'],
    }),
    deleteUser: builder.mutation({
      query: (id) => ({ url: `/admin/users/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin'],
    }),
    getAdminTournaments: builder.query({
      query: () => '/admin/tournaments',
      providesTags: ['Admin'],
    }),
    deleteTournament: builder.mutation({
      query: (id) => ({ url: `/admin/tournaments/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin', 'Tournament'],
    }),
    deleteTeam: builder.mutation({
      query: (id) => ({ url: `/admin/teams/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin', 'Team'],
    }),
    createDiscipline: builder.mutation({
      query: (body) => ({ url: '/disciplines', method: 'POST', body }),
      invalidatesTags: ['Discipline'],
    }),
    deleteDiscipline: builder.mutation({
      query: (id) => ({ url: `/disciplines/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Discipline'],
    }),
  }),
});

export const {
  useGetUsersQuery,
  useToggleBlockMutation,
  useChangeRoleMutation,
  useDeleteUserMutation,
  useGetAdminTournamentsQuery,
  useDeleteTournamentMutation,
  useDeleteTeamMutation,
  useCreateDisciplineMutation,
  useDeleteDisciplineMutation,
} = adminApi;
