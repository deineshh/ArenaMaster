import { baseApi } from './baseApi';

export const teamsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getTeams: builder.query({
      query: ({ page = 1, pageSize = 12 } = {}) =>
        `/teams?page=${page}&pageSize=${pageSize}`,
      providesTags: ['Team'],
    }),
    getTeam: builder.query({
      query: (slug) => `/teams/${slug}`,
      providesTags: ['Team'],
    }),
    createTeam: builder.mutation({
      query: (body) => ({ url: '/teams', method: 'POST', body }),
      invalidatesTags: ['Team'],
    }),
    invitePlayer: builder.mutation({
      query: ({ id, ...body }) => ({ url: `/teams/${id}/invitations`, method: 'POST', body }),
    }),
    myInvitations: builder.query({
      query: () => '/teams/invitations/my',
    }),
    acceptInvitation: builder.mutation({
      query: (id) => ({ url: `/teams/invitations/${id}/accept`, method: 'POST' }),
      invalidatesTags: ['Team'],
    }),
    declineInvitation: builder.mutation({
      query: (id) => ({ url: `/teams/invitations/${id}/decline`, method: 'POST' }),
    }),
  }),
});

export const {
  useGetTeamsQuery,
  useGetTeamQuery,
  useCreateTeamMutation,
  useInvitePlayerMutation,
  useMyInvitationsQuery,
  useAcceptInvitationMutation,
  useDeclineInvitationMutation,
} = teamsApi;
