import { baseApi } from './baseApi';

export const matchesApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getMatch: builder.query({
      query: (id) => `/matches/${id}`,
    }),
    submitResult: builder.mutation({
      query: ({ id, ...body }) => ({
        url: `/matches/${id}/result`,
        method: 'PATCH',
        body,
      }),
      invalidatesTags: ['Tournament'],
    }),
  }),
});

export const { useGetMatchQuery, useSubmitResultMutation } = matchesApi;
