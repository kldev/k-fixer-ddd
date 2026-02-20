using K.Fixer.Domain.IAM.Aggregates.AdminUser;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");
        builder.ConfigureAggregateRootFields();
        
        builder.Property(z => z.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion(f => f.Value, v => Username.Create(v).Value!);
        
        builder.Property(z => z.Password)
            .HasColumnName("password")
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(p => p.Hash, v => HashedPassword.FromHash(v));
        
        builder.Property(z => z.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(f => f.Value, v => FullName.Create(v).Value!);
       
        builder.Property(z => z.Role)
            .HasColumnName("role")
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(r => r.Value, v => UserRole.Create(v).Value!);

        builder.HasIndex(z => z.Username).IsUnique();
        
        builder.Property(z=>z.CreatedAt).HasColumnName("created_at");
    }
}
