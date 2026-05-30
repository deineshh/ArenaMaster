import { type RouteConfig, route, layout, index } from '@react-router/dev/routes';

export default [
  layout('components/layout/AppShell.jsx', [
    index('pages/HomePage.jsx'),

    route('login', 'features/auth/LoginPage.jsx'),
    route('register', 'features/auth/RegisterPage.jsx'),
    route('confirm-email', 'features/auth/ConfirmEmailPage.jsx'),

    route('tournaments', 'features/tournaments/TournamentsPage.jsx'),
    route('tournaments/create', 'features/tournaments/CreateTournamentPage.jsx'),
    route('tournaments/:slug', 'features/tournaments/TournamentDetailPage.jsx'),

    route('teams', 'features/teams/TeamsPage.jsx'),
    route('teams/create', 'features/teams/CreateTeamPage.jsx'),
    route('teams/invitations', 'features/teams/InvitationsPage.jsx'),
    route('teams/:slug', 'features/teams/TeamDetailPage.jsx'),

    route('profile/:username', 'features/profile/ProfilePage.jsx'),
    route('profile/edit', 'features/profile/EditProfilePage.jsx'),

    layout('features/admin/AdminLayout.jsx', [
      route('admin/users', 'features/admin/AdminUsersPage.jsx'),
      route('admin/tournaments', 'features/admin/AdminTournamentsPage.jsx'),
      route('admin/teams', 'features/admin/AdminTeamsPage.jsx'),
      route('admin/disciplines', 'features/admin/AdminDisciplinesPage.jsx'),
    ]),
  ]),
] satisfies RouteConfig;
