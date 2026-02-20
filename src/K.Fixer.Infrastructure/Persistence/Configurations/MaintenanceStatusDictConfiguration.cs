using K.Fixer.Infrastructure.Persistence.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class MaintenanceStatusDictConfiguration : IEntityTypeConfiguration<MaintenanceStatusDict>
{
    public void Configure(EntityTypeBuilder<MaintenanceStatusDict> builder)
    {
        builder.ToTable("maintenance_status_dict");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEN).HasColumnName("name_en").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NamePL).HasColumnName("name_pl").HasMaxLength(100);

        builder.HasIndex(z => z.Status).IsUnique();
    }
}