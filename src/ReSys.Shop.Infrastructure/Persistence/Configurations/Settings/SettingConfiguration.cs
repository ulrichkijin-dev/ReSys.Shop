using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Domain.Settings;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Settings;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable(name: Schema.Settings);

        builder.HasKey(keyExpression: c => c.Id);

        builder.HasIndex(indexExpression: c => c.Key)
            .IsUnique();

        builder.Property(propertyExpression: c => c.Key)
            .HasMaxLength(maxLength: Core.Domain.Settings.Setting.Constraints.KeyMaxLength)
            .IsRequired();

        builder.Property(propertyExpression: c => c.Value)
            .HasMaxLength(maxLength: Core.Domain.Settings.Setting.Constraints.ValueMaxLength)
            .IsRequired();

        builder.Property(propertyExpression: c => c.Description)
            .HasMaxLength(maxLength: Core.Domain.Settings.Setting.Constraints.DescriptionMaxLength)
            .IsRequired();
            
        builder.Property(propertyExpression: c => c.DefaultValue)
            .HasMaxLength(maxLength: Core.Domain.Settings.Setting.Constraints.DefaultValueMaxLength)
            .IsRequired();

        builder.Property(propertyExpression: c => c.ValueType)
            .IsRequired();
    }
}
