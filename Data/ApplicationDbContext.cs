using Microsoft.EntityFrameworkCore;
using nsia.Models;

namespace nsia.Data
{
      public class ApplicationDbContext : DbContext
      {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            public DbSet<Application> Applications { get; set; }
            public DbSet<Founder> Founders { get; set; }
            public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }
            public DbSet<AdminUser> AdminUsers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // ── APPLICATION ──
                  modelBuilder.Entity<Application>(entity =>
                  {
                        entity.HasKey(e => e.Id);

                        // MySQL stores Guid as char(36) — explicitly set for clarity
                        entity.Property(e => e.Id)
                        .HasColumnType("char(36)");

                        entity.Property(e => e.Status)
                        .HasDefaultValue("Draft")
                        .HasColumnType("varchar(30)");

                        // Owned type — maps inline to the Applications table
                        entity.OwnsOne(e => e.SocialMedia, sm =>
                  {
                        sm.Property(s => s.LinkedIn)
                    .HasColumnName("SocialMedia_LinkedIn")
                    .HasColumnType("varchar(200)");

                        sm.Property(s => s.Twitter)
                    .HasColumnName("SocialMedia_Twitter")
                    .HasColumnType("varchar(100)");

                        sm.Property(s => s.Instagram)
                    .HasColumnName("SocialMedia_Instagram")
                    .HasColumnType("varchar(100)");

                        sm.Property(s => s.Facebook)
                    .HasColumnName("SocialMedia_Facebook")
                    .HasColumnType("varchar(200)");
                  });

                        // Unique indexes — must be on bounded-length columns for MySQL
                        entity.HasIndex(e => e.Email)
                        .IsUnique()
                        .HasDatabaseName("IX_Applications_Email");

                        entity.HasIndex(e => e.ReferenceNumber)
                        .IsUnique()
                        .HasDatabaseName("IX_Applications_ReferenceNumber");

                        entity.HasIndex(e => e.Status)
                        .HasDatabaseName("IX_Applications_Status");

                        entity.HasIndex(e => e.ApplicationStep)
                        .HasDatabaseName("IX_Applications_ApplicationStep");

                        entity.HasIndex(e => e.CreatedAt)
                        .HasDatabaseName("IX_Applications_CreatedAt");

                        // Large text fields — use TEXT in MySQL instead of varchar(3000)
                        entity.Property(e => e.CompanyDescription)
                        .HasColumnType("text");

                        entity.Property(e => e.ForeignAffiliateDetails)
                        .HasColumnType("text");

                        entity.Property(e => e.ProductDescription)
                        .HasColumnType("text");

                        entity.Property(e => e.ImpactDataAndStatistics)
                        .HasColumnType("text");

                        entity.Property(e => e.MeasurableCommunityDifferences)
                        .HasColumnType("text");

                        entity.Property(e => e.TopImpactExamplesDetails)
                        .HasColumnType("text");

                        entity.Property(e => e.AdditionalInformation)
                        .HasColumnType("text");

                        // SubmittedByUserId as char(36)
                        entity.Property(e => e.SubmittedByUserId)
                        .HasColumnType("char(36)");
                  });

                  // ── FOUNDER ──
                  modelBuilder.Entity<Founder>(entity =>
                  {
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.Id)
                        .HasColumnType("char(36)");

                        entity.Property(e => e.ApplicationId)
                        .HasColumnType("char(36)");

                        entity.HasOne(f => f.Application)
                        .WithMany(a => a.Founders)
                        .HasForeignKey(f => f.ApplicationId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasIndex(f => f.ApplicationId)
                        .HasDatabaseName("IX_Founders_ApplicationId");

                        entity.HasIndex(f => new { f.ApplicationId, f.DisplayOrder })
                        .HasDatabaseName("IX_Founders_ApplicationId_DisplayOrder");
                  });
            }
      }
}