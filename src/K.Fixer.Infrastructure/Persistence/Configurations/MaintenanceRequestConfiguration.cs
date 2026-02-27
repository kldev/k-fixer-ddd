using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.ToTable("maintenance_requests");
        builder.ConfigureAggregateRootFields();
        
  

        builder.Property(z => z.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200)
            .HasConversion(p => p.Value, v => RequestTitle.Create(v).Value!);
        
        builder.Property(z => z.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(2000)
            .HasConversion(p => p.Value, v => RequestDescription.Create(v).Value!);
        
        builder.Property(z => z.ResolutionNote)
            .HasColumnName("resolution_note")
            .HasMaxLength(2000)
            .HasConversion(p => p.Value ?? null, v => ResolutionNote.Create(v).Value!);
        
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasConversion(p => p.Value, v => MaintenanceStatus.Create(v).Value!)
            .IsRequired();
        
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasMaxLength(50)
            .HasConversion(p => p.Value, v => Priority.Create(v).Value!)
            .IsRequired();
        
        builder.HasOne<Building>()
            .WithMany()
            .HasForeignKey(z => z.BuildingId)
            .HasPrincipalKey(z=>z.Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(z => z.AssignedToId)
            .HasPrincipalKey(z=>z.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(c => c.WorkPeriod, periodBuilder =>
        {
            periodBuilder.Property(z=>z.StartedAt) .HasColumnName("start_date_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            periodBuilder.Property(t => t.CompletedAt)
                .HasColumnName("completed_at_utc")
                .HasColumnType("timestamp with time zone");
            
            periodBuilder.HasIndex(t => new { t.StartedAt });
            periodBuilder.HasIndex(t => new { t.CompletedAt });
        });

        builder.Property(z=>z.BuildingId).HasColumnName("building_id");
        builder.Property(z=>z.AssignedToId).HasColumnName("assigned_to_id");
        
        builder.Property(z=>z.CreatedById).HasColumnName("created_by_id").IsRequired();
        
        builder.Property(z=>z.CreatedAt).HasColumnName("created_at");
        builder.Property(z=>z.ModifiedAt).HasColumnName("modified_at");
        builder.Property(z=>z.IsDeleted).HasColumnName("is_deleted");

    }
}