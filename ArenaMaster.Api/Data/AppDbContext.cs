using ArenaMaster.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OAuthAccount> OAuthAccounts => Set<OAuthAccount>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentParticipant> TournamentParticipants => Set<TournamentParticipant>();
    public DbSet<TournamentMatch> Matches => Set<TournamentMatch>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.SocialLinks).HasColumnType("jsonb");
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<OAuthAccount>(e =>
        {
            e.ToTable("oauth_accounts");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.OAuthAccounts).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<Discipline>(e =>
        {
            e.ToTable("disciplines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Team>(e =>
        {
            e.ToTable("teams");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Captain).WithMany(u => u.CaptainedTeams).HasForeignKey(x => x.CaptainId);
        });

        modelBuilder.Entity<TeamMember>(e =>
        {
            e.ToTable("team_members");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TeamId, x.UserId }).IsUnique();
            e.HasOne(x => x.Team).WithMany(t => t.Members).HasForeignKey(x => x.TeamId);
            e.HasOne(x => x.User).WithMany(u => u.TeamMemberships).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<TeamInvitation>(e =>
        {
            e.ToTable("team_invitations");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Team).WithMany(t => t.Invitations).HasForeignKey(x => x.TeamId);
            e.HasOne(x => x.Invitee).WithMany().HasForeignKey(x => x.InviteeId);
        });

        modelBuilder.Entity<Tournament>(e =>
        {
            e.ToTable("tournaments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Discipline).WithMany(d => d.Tournaments).HasForeignKey(x => x.DisciplineId);
            e.HasOne(x => x.Organizer).WithMany(u => u.OrganizedTournaments).HasForeignKey(x => x.OrganizerId);
        });

        modelBuilder.Entity<TournamentParticipant>(e =>
        {
            e.ToTable("tournament_participants");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TournamentId);
            e.HasOne(x => x.Tournament).WithMany(t => t.Participants).HasForeignKey(x => x.TournamentId);
            e.HasOne(x => x.User).WithMany(u => u.TournamentParticipations).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Team).WithMany(t => t.TournamentParticipations).HasForeignKey(x => x.TeamId);
        });

        modelBuilder.Entity<TournamentMatch>(e =>
        {
            e.ToTable("matches");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TournamentId);
            e.HasOne(x => x.Tournament).WithMany(t => t.Matches).HasForeignKey(x => x.TournamentId);
            e.HasOne(x => x.Participant1).WithMany().HasForeignKey(x => x.Participant1Id).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Participant2).WithMany().HasForeignKey(x => x.Participant2Id).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Winner).WithMany().HasForeignKey(x => x.WinnerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NextMatch).WithMany().HasForeignKey(x => x.NextMatchId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.Notifications).HasForeignKey(x => x.UserId);
        });
    }
}
