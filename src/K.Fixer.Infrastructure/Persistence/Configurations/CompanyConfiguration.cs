using K.Fixer.Domain.PropertyManagement.Aggregates.Company;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("company");
  
        builder.ConfigureAggregateRootFields();
 
        builder.Property(z => z.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(p => p.Value, v => CompanyName.Create(v).Value!);
        
        builder.OwnsOne(c => c.TaxId, taxBuilder =>
        {
            taxBuilder.Property(t => t.CountryCode)
                .HasColumnName("tax_id_country")
                .HasColumnType("character varying(2)")
                .HasMaxLength(2)
                .IsRequired();

            taxBuilder.Property(t => t.Number)
                .HasColumnName("tax_id_number")
                .HasColumnType("character varying(20)")
                .HasMaxLength(20)
                .IsRequired();

            taxBuilder.HasIndex(t => new { t.CountryCode, t.Number }).IsUnique();
        });

        builder.Property(z => z.ContactEmail)
            .HasColumnName("contact_email")
            .IsRequired()
            .HasMaxLength(250)
            .HasConversion(p => p.Value, v => ContactEmail.Create(v).Value!);

        builder.Property(z => z.ContactPhone)
            .HasColumnName("contact_phone")
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(p => p.Value, v => ContactPhone.Create(v).Value!);
        
        builder.Property(z=>z.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.CreatedAt);
    }
}