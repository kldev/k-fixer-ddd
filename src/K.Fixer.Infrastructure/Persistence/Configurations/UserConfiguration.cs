using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.PropertyManagement.Aggregates.Company;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
  
        builder.ConfigureAggregateRootFields();

        builder.Property(z => z.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(250)
            .HasConversion(e => e.Value, v => Email.Create(v).Value!);
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
        builder.Property(z => z.IsActive).HasColumnName("is_active");
        builder.Property(z => z.IsLocked).HasColumnName("is_locked");
        builder.Property(z => z.CompanyId).HasColumnName("company_id");

        builder.HasOne<Company>()
             .WithMany()
             .HasForeignKey(z => z.CompanyId)
             .HasPrincipalKey(z=>z.Id)
             .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(z => z.Email).IsUnique(); // tylko maila firmowe unikatowe
        builder.HasIndex(z => z.Role);
        
        builder.Property(z=>z.CreatedAt).HasColumnName("created_at");
        builder.Property(z=>z.ModifiedAt).HasColumnName("modified_at");
        
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ModifiedAt);

    }
}