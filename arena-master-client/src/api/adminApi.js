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

    getAdminTeams: builder.query({
      query: () => '/admin/teams',
      providesTags: ['Admin', 'Team'],
    }),
    createTeamByAdmin: builder.mutation({
      query: ({ name, captainId }) => ({ url: '/admin/teams', method: 'POST', body: { name, captainId } }),
      invalidatesTags: ['Admin', 'Team'],
    }),
    updateTeam: builder.mutation({
      query: ({ id, name }) => ({ url: `/admin/teams/${id}`, method: 'PUT', body: { name } }),
      invalidatesTags: ['Admin', 'Team'],
    }),
    deleteTeam: builder.mutation({
      query: (id) => ({ url: `/admin/teams/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin', 'Team'],
    }),

    getAdminDisciplines: builder.query({
      query: () => '/admin/disciplines',
      providesTags: ['Admin', 'Discipline'],
    }),
    createDiscipline: builder.mutation({
      query: (body) => ({ url: '/disciplines', method: 'POST', body }),
      invalidatesTags: ['Discipline'],
    }),
    createDisciplineByAdmin: builder.mutation({
      query: ({ name }) => ({ url: '/admin/disciplines', method: 'POST', body: { name } }),
      invalidatesTags: ['Admin', 'Discipline'],
    }),
    updateDiscipline: builder.mutation({
      query: ({ id, name }) => ({ url: `/admin/disciplines/${id}`, method: 'PUT', body: { name } }),
      invalidatesTags: ['Admin', 'Discipline'],
    }),
    deleteDiscipline: builder.mutation({
      query: (id) => ({ url: `/admin/disciplines/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin', 'Discipline'],
    }),

    getAdminMatches: builder.query({
      query: () => '/admin/matches',
      providesTags: ['Admin'],
    }),
    getAdminMatch: builder.query({
      query: (id) => `/admin/matches/${id}`,
      providesTags: ['Admin'],
    }),
    createMatch: builder.mutation({
      query: (body) => ({ url: '/admin/matches', method: 'POST', body }),
      invalidatesTags: ['Admin'],
    }),
    updateMatch: builder.mutation({
      query: ({ id, ...body }) => ({ url: `/admin/matches/${id}`, method: 'PUT', body }),
      invalidatesTags: ['Admin'],
    }),
    deleteMatch: builder.mutation({
      query: (id) => ({ url: `/admin/matches/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin'],
    }),

    getAdminParticipants: builder.query({
      query: () => '/admin/participants',
      providesTags: ['Admin'],
    }),
    getAdminParticipant: builder.query({
      query: (id) => `/admin/participants/${id}`,
      providesTags: ['Admin'],
    }),
    createParticipant: builder.mutation({
      query: (body) => ({ url: '/admin/participants', method: 'POST', body }),
      invalidatesTags: ['Admin'],
    }),
    updateParticipantStatus: builder.mutation({
      query: ({ id, status }) => ({ url: `/admin/participants/${id}/status`, method: 'PATCH', body: { status } }),
      invalidatesTags: ['Admin'],
    }),
    deleteParticipant: builder.mutation({
      query: (id) => ({ url: `/admin/participants/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin'],
    }),

    getAdminMembers: builder.query({
      query: () => '/admin/members',
      providesTags: ['Admin'],
    }),
    createMember: builder.mutation({
      query: ({ teamId, userId, role }) => ({ url: '/admin/members', method: 'POST', body: { teamId, userId, role } }),
      invalidatesTags: ['Admin'],
    }),
    deleteMember: builder.mutation({
      query: (id) => ({ url: `/admin/members/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Admin'],
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
  useGetAdminTeamsQuery,
  useCreateTeamByAdminMutation,
  useUpdateTeamMutation,
  useDeleteTeamMutation,
  useGetAdminDisciplinesQuery,
  useCreateDisciplineMutation,
  useCreateDisciplineByAdminMutation,
  useUpdateDisciplineMutation,
  useDeleteDisciplineMutation,
  useGetAdminMatchesQuery,
  useGetAdminMatchQuery,
  useCreateMatchMutation,
  useUpdateMatchMutation,
  useDeleteMatchMutation,
  useGetAdminParticipantsQuery,
  useGetAdminParticipantQuery,
  useCreateParticipantMutation,
  useUpdateParticipantStatusMutation,
  useDeleteParticipantMutation,
  useGetAdminMembersQuery,
  useCreateMemberMutation,
  useDeleteMemberMutation,
} = adminApi;
