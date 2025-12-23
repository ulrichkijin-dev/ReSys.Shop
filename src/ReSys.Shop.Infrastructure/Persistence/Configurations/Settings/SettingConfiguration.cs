using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Settings;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Settings;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        // Table and Schema
        builder.ToTable(name: "Settings", Schema.Default);

        // Primary Key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(Setting.Constraints.KeyMaxLength);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(Setting.Constraints.ValueMaxLength);

        builder.Property(s => s.Description)
            .HasMaxLength(Setting.Constraints.DescriptionMaxLength);

        builder.Property(s => s.DefaultValue)
            .HasMaxLength(Setting.Constraints.DefaultValueMaxLength);

        builder.Property(s => s.ValueType)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string in DB

        // Indexes
        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.ConfigureMetadata();
        builder.ConfigureAuditable();
        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}