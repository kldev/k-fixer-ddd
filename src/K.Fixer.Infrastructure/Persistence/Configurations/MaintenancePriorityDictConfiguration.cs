using K.Fixer.Infrastructure.Persistence.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class MaintenancePriorityDictConfiguration : IEntityTypeConfiguration<MaintenancePriorityDict>
{
    public void Configure(EntityTypeBuilder<MaintenancePriorityDict> builder)
    {
        builder.ToTable("maintenance_priority_dict");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Priority).HasColumnName("priority").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEN).HasColumnName("name_en").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NamePL).HasColumnName("name_pl").HasMaxLength(100);

        builder.HasIndex(z => z.Priority).IsUnique();
    }
}
