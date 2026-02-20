using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.PropertyManagement.Aggregates.Building;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class ResidentOccupancyConfiguration : IEntityTypeConfiguration<ResidentOccupancy>
{
    public void Configure(EntityTypeBuilder<ResidentOccupancy> builder)
    {
        builder.ToTable("resident_assignments");
        
        builder.HasKey("RecordId");
        builder.Property("RecordId").HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd(); // id = long
        builder.Property(z => z.Id).HasColumnName("gid");
        
        builder.HasIndex(x => x.Id).IsUnique();
        
        builder.HasOne<Building>()
            .WithMany("Occupancies")
            .HasForeignKey(z => z.BuildingId)
            .HasPrincipalKey(z => z.Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(z => z.ResidentId)
            .HasPrincipalKey(z=>z.Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.OwnsOne(c => c.Period, periodBuilder =>
        {
            periodBuilder.Property(t => t.From)
                .HasColumnName("period_from")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            periodBuilder.Property(t => t.To)
                .HasColumnName("period_to")
                .HasColumnType("timestamp with time zone");
            
            periodBuilder.HasIndex(t => new { t.From });
            periodBuilder.HasIndex(t => new { t.To });
        });
        
        builder.Property(z=>z.BuildingId).HasColumnName("building_id");
        builder.Property(z=>z.ResidentId).HasColumnName("resident_id");
        
    }
}