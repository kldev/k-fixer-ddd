using System.Data;

using K.Fixer.Domain.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace K.Fixer.Infrastructure.Persistence.Extensions;

public static class AggregateRootExtensions
{
    public static void ConfigureAggregateRootFields<T>(this EntityTypeBuilder<T> builder) where T : AggregateRoot<Guid>
    {
        builder.HasKey("RecordId");
        builder.Property("RecordId").HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd(); // id = long
        builder.Property(z => z.Id).HasColumnName("gid");
        
        builder.HasIndex(x => x.Id).IsUnique();

    }
}