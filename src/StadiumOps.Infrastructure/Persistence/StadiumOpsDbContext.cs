using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StadiumOps.Domain.Common;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.Identity;

namespace StadiumOps.Infrastructure.Persistence;

public sealed class StadiumOpsDbContext(DbContextOptions<StadiumOpsDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Stadium> Stadiums => Set<Stadium>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<StadiumPointOfInterest> PointsOfInterest => Set<StadiumPointOfInterest>();
    public DbSet<NavigationRoute> NavigationRoutes => Set<NavigationRoute>();
    public DbSet<CrowdZone> CrowdZones => Set<CrowdZone>();
    public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();
    public DbSet<StadiumNotification> Notifications => Set<StadiumNotification>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiKnowledgeDocument> AiKnowledgeDocuments => Set<AiKnowledgeDocument>();
    public DbSet<AiAnalyticsEvent> AiAnalyticsEvents => Set<AiAnalyticsEvent>();
    public DbSet<TransportStatus> TransportStatuses => Set<TransportStatus>();
    public DbSet<SustainabilityMetric> SustainabilityMetrics => Set<SustainabilityMetric>();
    public DbSet<VolunteerTask> VolunteerTasks => Set<VolunteerTask>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<IntegrationEventOutbox> IntegrationEventOutbox => Set<IntegrationEventOutbox>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.PreferredLanguage).HasMaxLength(16);
            entity.Property(x => x.AccessibilityPreference).HasMaxLength(120);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(260);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).HasMaxLength(128);
            entity.Property(x => x.DeviceName).HasMaxLength(160);
            entity.Property(x => x.CreatedByIp).HasMaxLength(64);
            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureEntity<Stadium>(builder);
        ConfigureEntity<Match>(builder);
        ConfigureEntity<Seat>(builder);
        ConfigureEntity<StadiumPointOfInterest>(builder);
        ConfigureEntity<NavigationRoute>(builder);
        ConfigureEntity<CrowdZone>(builder);
        ConfigureEntity<IncidentReport>(builder);
        ConfigureEntity<StadiumNotification>(builder);
        ConfigureEntity<AiConversation>(builder);
        ConfigureEntity<AiKnowledgeDocument>(builder);
        ConfigureEntity<AiAnalyticsEvent>(builder);
        ConfigureEntity<TransportStatus>(builder);
        ConfigureEntity<SustainabilityMetric>(builder);
        ConfigureEntity<VolunteerTask>(builder);
        ConfigureEntity<AuditLog>(builder);
        ConfigureEntity<IntegrationEventOutbox>(builder);

        builder.Entity<Stadium>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(180);
            entity.Property(x => x.City).HasMaxLength(120);
            entity.Property(x => x.Country).HasMaxLength(120);
            entity.Property(x => x.TimeZone).HasMaxLength(80);
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
        });

        builder.Entity<Match>(entity =>
        {
            entity.Property(x => x.HomeTeam).HasMaxLength(120);
            entity.Property(x => x.AwayTeam).HasMaxLength(120);
            entity.Property(x => x.Stage).HasMaxLength(80);
            entity.Property(x => x.Status).HasMaxLength(40);
        });

        builder.Entity<Seat>(entity =>
        {
            entity.Property(x => x.Section).HasMaxLength(40);
            entity.Property(x => x.Row).HasMaxLength(20);
            entity.Property(x => x.Number).HasMaxLength(20);
        });

        builder.Entity<StadiumPointOfInterest>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.Category).HasMaxLength(80);
            entity.Property(x => x.Level).HasMaxLength(40);
            entity.Property(x => x.Zone).HasMaxLength(80);
        });

        builder.Entity<NavigationRoute>(entity =>
        {
            entity.Property(x => x.FromLocation).HasMaxLength(120);
            entity.Property(x => x.ToLocation).HasMaxLength(120);
            entity.Property(x => x.SafetyNote).HasMaxLength(500);
        });

        builder.Entity<CrowdZone>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.Status).HasMaxLength(40);
        });

        builder.Entity<IncidentReport>(entity =>
        {
            entity.Property(x => x.Category).HasMaxLength(80);
            entity.Property(x => x.Severity).HasMaxLength(40);
            entity.Property(x => x.Location).HasMaxLength(160);
            entity.Property(x => x.Description).HasMaxLength(1200);
            entity.Property(x => x.Status).HasMaxLength(40);
            entity.Property(x => x.Priority).HasMaxLength(40);
            entity.Property(x => x.AssignedTeam).HasMaxLength(120);
        });

        builder.Entity<StadiumNotification>(entity =>
        {
            entity.Property(x => x.Type).HasMaxLength(80);
            entity.Property(x => x.Title).HasMaxLength(180);
            entity.Property(x => x.Message).HasMaxLength(1200);
            entity.Property(x => x.Priority).HasMaxLength(40);
        });

        builder.Entity<AiConversation>(entity =>
        {
            entity.Property(x => x.Prompt).HasMaxLength(4000);
            entity.Property(x => x.Response).HasMaxLength(8000);
            entity.Property(x => x.Intent).HasMaxLength(80);
            entity.Property(x => x.AgentKey).HasMaxLength(80);
            entity.Property(x => x.AgentName).HasMaxLength(160);
            entity.Property(x => x.ConfidenceScore).HasPrecision(5, 2);
            entity.Property(x => x.Model).HasMaxLength(120);
            entity.Property(x => x.PromptVersion).HasMaxLength(80);
            entity.Property(x => x.GroundingSummary).HasMaxLength(1200);
            entity.Property(x => x.SourcesJson).HasMaxLength(4000);
            entity.Property(x => x.GuardrailsJson).HasMaxLength(4000);
            entity.HasIndex(x => x.Intent);
            entity.HasIndex(x => x.AgentKey);
        });

        builder.Entity<AiKnowledgeDocument>(entity =>
        {
            entity.Property(x => x.Category).HasMaxLength(120);
            entity.Property(x => x.Title).HasMaxLength(220);
            entity.Property(x => x.SourceType).HasMaxLength(80);
            entity.Property(x => x.SourceUri).HasMaxLength(600);
            entity.Property(x => x.ContentSummary).HasMaxLength(2000);
            entity.Property(x => x.Language).HasMaxLength(16);
            entity.HasIndex(x => new { x.Category, x.Language, x.IsApproved });
        });

        builder.Entity<AiAnalyticsEvent>(entity =>
        {
            entity.Property(x => x.EventName).HasMaxLength(120);
            entity.Property(x => x.Intent).HasMaxLength(80);
            entity.Property(x => x.AgentKey).HasMaxLength(80);
            entity.Property(x => x.AgentName).HasMaxLength(160);
            entity.Property(x => x.ConfidenceScore).HasPrecision(5, 2);
            entity.Property(x => x.MetadataJson).HasMaxLength(4000);
            entity.HasIndex(x => x.Intent);
            entity.HasIndex(x => x.AgentKey);
        });

        builder.Entity<TransportStatus>(entity =>
        {
            entity.Property(x => x.Mode).HasMaxLength(60);
            entity.Property(x => x.Provider).HasMaxLength(120);
            entity.Property(x => x.Status).HasMaxLength(80);
        });

        builder.Entity<SustainabilityMetric>(entity =>
        {
            entity.Property(x => x.EnergyKwh).HasPrecision(12, 2);
            entity.Property(x => x.WaterLiters).HasPrecision(12, 2);
            entity.Property(x => x.WasteKg).HasPrecision(12, 2);
            entity.Property(x => x.RecyclingRate).HasPrecision(5, 2);
            entity.Property(x => x.CarbonScore).HasPrecision(5, 2);
        });

        builder.Entity<VolunteerTask>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(180);
            entity.Property(x => x.Location).HasMaxLength(160);
            entity.Property(x => x.Status).HasMaxLength(40);
            entity.Property(x => x.Priority).HasMaxLength(40);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.Action).HasMaxLength(160);
            entity.Property(x => x.Resource).HasMaxLength(160);
            entity.Property(x => x.Details).HasMaxLength(1200);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.CorrelationId).HasMaxLength(80);
        });

        builder.Entity<IntegrationEventOutbox>(entity =>
        {
            entity.Property(x => x.EventType).HasMaxLength(160);
            entity.Property(x => x.AggregateType).HasMaxLength(120);
            entity.Property(x => x.PayloadJson).HasMaxLength(8000);
            entity.Property(x => x.PublishStatus).HasMaxLength(40);
            entity.Property(x => x.LastError).HasMaxLength(1200);
            entity.Property(x => x.CorrelationId).HasMaxLength(80);
            entity.HasIndex(x => new { x.PublishStatus, x.NextAttemptAt });
            entity.HasIndex(x => x.EventType);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureEntity<TEntity>(ModelBuilder builder) where TEntity : Entity
    {
        builder.Entity<TEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
