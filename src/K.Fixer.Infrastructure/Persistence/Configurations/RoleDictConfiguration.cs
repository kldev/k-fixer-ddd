using K.Fixer.Infrastructure.Persistence.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class RoleDictConfiguration : IEntityTypeConfiguration<RoleDict>
{
    public void Configure(EntityTypeBuilder<RoleDict> builder)
    {
        builder.ToTable("role_dict");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEN).HasColumnName("name_en").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NamePL).HasColumnName("name_pl").HasMaxLength(100);

        builder.HasIndex(z => z.Role).IsUnique();
    }
}