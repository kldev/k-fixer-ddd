using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Domain.PropertyManagement.Aggregates.Company;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("building");
        builder.ConfigureAggregateRootFields();
        
        builder.Property(z => z.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(150)
            .HasConversion(p => p.Value, v => BuildingName.Create(v).Value!);
        
        builder.Property(z => z.CountryCode)
            .HasColumnName("country_code")
            .IsRequired()
            .HasMaxLength(2)
            .HasConversion(p => p.Value, v => CountryCode.Create(v).Value!);

        builder.OwnsOne(c => c.Address, addressBuilder =>
        {
            addressBuilder.Property(t => t.CountryCode)
                .HasColumnName("address_country_code")
                .HasColumnType("character varying(2)")
                .HasMaxLength(2)
                .IsRequired();

            addressBuilder.Property(t => t.City)
                .HasColumnName("address_city")
                .HasColumnType("character varying(50)")
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(t => t.Street)
                .HasColumnName("address_street")
                .HasColumnType("character varying(100)")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(t => t.PostalCode)
                .HasColumnName("address_postal_code")
                .HasColumnType("character varying(20)")
                .HasMaxLength(20)
                .IsRequired();
            
        });
        
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(z => z.CompanyId)
            .HasPrincipalKey(z=>z.Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(z=>z.CompanyId).HasColumnName("company_id");
        builder.Property(z=>z.CreatedAt).HasColumnName("created_at");
        builder.Property(z=>z.IsActive).HasColumnName("is_active");
        
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.IsActive);
    }
}