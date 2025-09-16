using Microsoft.EntityFrameworkCore;
using FantasyArchive.Data.Models;

namespace FantasyArchive.Data
{
    public class FantasyArchiveContext : DbContext
    {
    public DbSet<Franchise> Franchises { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerWeek> PlayerWeeks { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<TeamScore> TeamScores { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionGroup> TransactionGroups { get; set; }

    public FantasyArchiveContext(DbContextOptions<FantasyArchiveContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Franchise configuration - mapping to existing GLA table
            modelBuilder.Entity<Franchise>(entity =>
            {
                entity.ToTable("Franchise");
                entity.HasKey(e => e.FranchiseId);
                entity.Property(e => e.MainName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(50);
            });

            // Season configuration - mapping to existing GLA table  
            modelBuilder.Entity<Season>(entity =>
            {
                entity.ToTable("Season");
                entity.HasKey(e => new { e.LeagueId, e.Year }); // Composite key
            });

            // Team configuration - mapping to existing GLA table
            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Team");
                entity.HasKey(e => e.TeamId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                
                entity.HasOne(d => d.Season)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => new { d.LeagueId, d.Year });
                    
                entity.HasOne(d => d.Franchise)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.FranchiseId);

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.OwnerId);
            });

            // Owner configuration - mapping to existing GLA table
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.ToTable("Owner");
                entity.HasKey(e => e.OwnerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                
                entity.HasOne(d => d.Franchice) // Note: keeping typo from original
                    .WithMany(p => p.Owners)
                    .HasForeignKey(d => d.FranchiceId);
            });

            // Player configuration - mapping to existing GLA table
            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("Player");
                entity.HasKey(e => e.PlayerID);
                entity.Property(e => e.PlayerID).HasColumnName("PlayerID");
                entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(100);
                entity.Property(e => e.Position).HasColumnName("Position").HasMaxLength(10);
                entity.Property(e => e.YahooPlayerID).HasColumnName("YahooPlayerID");
                entity.Property(e => e.PrimaryPosition).HasColumnName("PrimaryPosition").HasMaxLength(2);
                entity.Property(e => e.ShortName).HasColumnName("ShortName").HasMaxLength(100);
                entity.Property(e => e.BirthYear).HasColumnName("BirthYear");
            });

            // PlayerWeek configuration - mapping to existing GLA table
            modelBuilder.Entity<PlayerWeek>(entity =>
            {
                entity.ToTable("PlayerWeek");
                // Composite primary key to match actual database
                entity.HasKey(e => new { e.PlayerID, e.Year, e.Week });
                
                entity.HasOne(d => d.Player)
                    .WithMany(p => p.PlayerWeeks)
                    .HasForeignKey(d => d.PlayerID);
                    
                entity.HasOne(d => d.Team)
                    .WithMany()
                    .HasForeignKey(d => d.TeamId);
            });

            // Match configuration - mapping to existing GLA table
            modelBuilder.Entity<Match>(entity =>
            {
                entity.ToTable("Match");
                entity.HasKey(e => e.MatchID);
                
                entity.HasOne(d => d.WinningTeam)
                    .WithMany()
                    .HasForeignKey(d => d.WinningTeamID)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(d => d.LosingTeam)
                    .WithMany()
                    .HasForeignKey(d => d.LosingTeamID)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(d => d.Season)
                    .WithMany()
                    .HasForeignKey(d => new { d.LeagueID, d.Year });
            });

            // TeamScore configuration - mapping to existing GLA table
            modelBuilder.Entity<TeamScore>(entity =>
            {
                entity.ToTable("TeamScore");
                entity.HasKey(e => new { e.TeamID, e.Year, e.Week });
                
                entity.HasOne(d => d.Team)
                    .WithMany()
                    .HasForeignKey(d => d.TeamID);
                    
                entity.HasOne(d => d.Season)
                    .WithMany()
                    .HasForeignKey(d => new { d.LeagueID, d.Year });
            });

            // Transaction configuration - mapping to existing GLA table
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");
                entity.HasKey(e => e.TransactionId);
                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
                entity.Property(e => e.TransactionGroupId).HasColumnName("TransactionGroupID");
                entity.Property(e => e.TeamId).HasColumnName("TeamID");
                entity.Property(e => e.TransactionType).HasColumnName("TransactionType");
                entity.Property(e => e.PlayerId).HasColumnName("PlayerID");
                entity.Property(e => e.DraftPickId).HasColumnName("DraftPickID");
                entity.Property(e => e.Description).HasMaxLength(255);
                
                entity.HasOne(d => d.TransactionGroup)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.TransactionGroupId);
                    
                entity.HasOne(d => d.Team)
                    .WithMany()
                    .HasForeignKey(d => d.TeamId);
                    
                entity.HasOne(d => d.Player)
                    .WithMany()
                    .HasForeignKey(d => d.PlayerId);
            });

            // TransactionGroup configuration - mapping to existing GLA table
            modelBuilder.Entity<TransactionGroup>(entity =>
            {
                entity.ToTable("TransactionGroup");
                entity.HasKey(e => e.TransactionGroupId);
                entity.Property(e => e.TransactionGroupId).HasColumnName("TransactionGroupID");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}