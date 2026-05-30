import { baseApi } from './baseApi';

export const disciplinesApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getDisciplines: builder.query({
      query: () => '/disciplines',
      providesTags: ['Discipline'],
    }),
  }),
});

export const { useGetDisciplinesQuery } = disciplinesApi;
