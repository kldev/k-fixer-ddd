using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class StatusChangeLogEntryConfiguration : IEntityTypeConfiguration<StatusChangeLogEntry>
{
    public void Configure(EntityTypeBuilder<StatusChangeLogEntry> builder)
    {
        builder.ToTable("maintenance_status_log");

        builder.HasKey("RecordId");
        builder.Property("RecordId").HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(z => z.Id).HasColumnName("gid");

        builder.HasIndex(x => x.Id).IsUnique();

        builder.Property(z => z.RequestId)
            .HasColumnName("request_id")
            .IsRequired();

        builder.Property(z => z.OldStatus)
            .HasColumnName("old_status")
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(z => z.NewStatus)
            .HasColumnName("new_status")
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(z => z.ChangedById)
            .HasColumnName("changed_by_id")
            .IsRequired();

        builder.Property(z => z.ChangedAt)
            .HasColumnName("changed_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(x => x.RequestId);
        builder.HasIndex(x => x.ChangedAt);

        // FK to MaintenanceRequest — add when aggregate is created
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(z => z.ChangedById)
            .HasPrincipalKey(z => z.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}