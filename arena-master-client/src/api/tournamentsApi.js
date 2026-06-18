import { baseApi } from './baseApi';

export const tournamentsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getTournaments: builder.query({
      query: (params = {}) => {
        const clean = Object.fromEntries(
          Object.entries(params).filter(([, v]) => v != null && v !== '')
        );
        const q = new URLSearchParams(clean).toString();
        return `/tournaments?${q}`;
      },
      providesTags: ['Tournament'],
    }),
    getTournament: builder.query({
      query: (slug) => `/tournaments/${slug}`,
      providesTags: ['Tournament'],
    }),
    createTournament: builder.mutation({
      query: (body) => ({ url: '/tournaments', method: 'POST', body }),
      invalidatesTags: ['Tournament'],
    }),
    register: builder.mutation({
      query: ({ id, teamId }) => ({
        url: `/tournaments/${id}/participants`,
        method: 'POST',
        body: teamId ? { teamId } : {},
      }),
      invalidatesTags: ['Tournament'],
    }),
    getParticipants: builder.query({
      query: (id) => `/tournaments/${id}/participants`,
    }),
    getBracket: builder.query({
      query: (id) => `/tournaments/${id}/bracket`,
    }),
    updateStatus: builder.mutation({
      query: ({ id, status }) => ({
        url: `/tournaments/${id}/status`,
        method: 'PATCH',
        body: { status },
      }),
      invalidatesTags: ['Tournament'],
    }),
  }),
});

export const {
  useGetTournamentsQuery,
  useGetTournamentQuery,
  useCreateTournamentMutation,
  useRegisterMutation,
  useGetParticipantsQuery,
  useGetBracketQuery,
  useUpdateStatusMutation,
} = tournamentsApi;
