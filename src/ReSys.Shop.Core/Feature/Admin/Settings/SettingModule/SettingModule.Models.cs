using Mapster;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Settings;

// Changed from Catalog.OptionTypes

namespace ReSys.Shop.Core.Feature.Admin.Settings.SettingModule; // Changed from Catalog.OptionTypes

public static partial class SettingModule
{
    public static class Models
    {
        // Request:
        public record Parameter : IHasMetadata
        {
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string DefaultValue { get; set; } = string.Empty;
            public ConfigurationValueType ValueType { get; set; }
            public IDictionary<string, object?>? PublicMetadata { get; set; }
            public IDictionary<string, object?>? PrivateMetadata { get; set; }
        }

        // Validator:
        public sealed class ParameterValidator : AbstractValidator<Parameter>
        {
            public ParameterValidator()
            {
                const string prefix = nameof(Setting);
                RuleFor(x => x.Key)
                    .NotEmpty()
                    .WithErrorCode(CommonInput.Errors.Required(prefix, nameof(Setting.Key)).Code)
                    .WithMessage(CommonInput.Errors.Required(prefix, nameof(Setting.Key)).Description)
                    .MaximumLength(Setting.Constraints.KeyMaxLength)
                    .WithErrorCode(CommonInput.Errors.TooLong(prefix, nameof(Setting.Key), Setting.Constraints.KeyMaxLength).Code)
                    .WithMessage(CommonInput.Errors.TooLong(prefix, nameof(Setting.Key), Setting.Constraints.KeyMaxLength).Description);

                RuleFor(x => x.Value)
                    .NotEmpty()
                    .WithErrorCode(CommonInput.Errors.Required(prefix, nameof(Setting.Value)).Code)
                    .WithMessage(CommonInput.Errors.Required(prefix, nameof(Setting.Value)).Description)
                    .MaximumLength(Setting.Constraints.ValueMaxLength)
                    .WithErrorCode(CommonInput.Errors.TooLong(prefix, nameof(Setting.Value), Setting.Constraints.ValueMaxLength).Code)
                    .WithMessage(CommonInput.Errors.TooLong(prefix, nameof(Setting.Value), Setting.Constraints.ValueMaxLength).Description);
                
                RuleFor(x => x.Description)
                    .MaximumLength(Setting.Constraints.DescriptionMaxLength)
                    .WithErrorCode(CommonInput.Errors.TooLong(prefix, nameof(Setting.Description), Setting.Constraints.DescriptionMaxLength).Code)
                    .WithMessage(CommonInput.Errors.TooLong(prefix, nameof(Setting.Description), Setting.Constraints.DescriptionMaxLength).Description);

                RuleFor(x => x.DefaultValue)
                    .MaximumLength(Setting.Constraints.DefaultValueMaxLength)
                    .WithErrorCode(CommonInput.Errors.TooLong(prefix, nameof(Setting.DefaultValue), Setting.Constraints.DefaultValueMaxLength).Code)
                    .WithMessage(CommonInput.Errors.TooLong(prefix, nameof(Setting.DefaultValue), Setting.Constraints.DefaultValueMaxLength).Description);

                this.AddMetadataSupportRules(prefix: prefix);
            }
        }

        // Result:
        public record SelectItem
        {
            public Guid Id { get; set; }
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public record ListItem
        {
            // Properties:
            public Guid Id { get; set; }
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public ConfigurationValueType ValueType { get; set; }

            // Audit time:
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
        }

        public record Detail : Parameter, IHasIdentity<Guid>
        {
            public Guid Id { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
        }

        // Mapping:
        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                // Setting -> SelectItem
                config.NewConfig<Setting, SelectItem>();

                // Setting -> ListItem
                config.NewConfig<Setting, ListItem>();

                // Setting -> Detail
                config.NewConfig<Setting, Detail>();
            }
        }
    }
}
