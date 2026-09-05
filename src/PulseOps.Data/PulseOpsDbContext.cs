using Microsoft.EntityFrameworkCore;

namespace PulseOps.Data;

public sealed class PulseOpsDbContext(DbContextOptions<PulseOpsDbContext> options)
    : DbContext(options)
{
    public DbSet<Incident> Incidents => Set<Incident>();

    public DbSet<IncidentStatusHistory> IncidentStatusHistory =>
        Set<IncidentStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Incident>(incident =>
        {
            incident.ToTable("incidents");
            incident.HasKey(item => item.Id);
            incident.Property(item => item.Id).HasColumnName("id");
            incident.Property(item => item.ServiceId)
                .HasColumnName("service_id")
                .HasMaxLength(100)
                .IsRequired();
            incident.Property(item => item.Summary)
                .HasColumnName("summary")
                .HasMaxLength(500)
                .IsRequired();
            incident.Property(item => item.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            incident.Property(item => item.CreatedAtUtc).HasColumnName("created_at_utc");
            incident.Property(item => item.UpdatedAtUtc).HasColumnName("updated_at_utc");
            incident.HasMany(item => item.History)
                .WithOne(item => item.Incident)
                .HasForeignKey(item => item.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IncidentStatusHistory>(history =>
        {
            history.ToTable("incident_status_history");
            history.HasKey(item => item.Id);
            history.Property(item => item.Id).HasColumnName("id");
            history.Property(item => item.IncidentId).HasColumnName("incident_id");
            history.Property(item => item.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            history.Property(item => item.ChangedAtUtc).HasColumnName("changed_at_utc");
            history.HasIndex(item => new { item.IncidentId, item.ChangedAtUtc });
        });
    }
}
