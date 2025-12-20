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
        public record Parameter // Setting doesn't have ParameterizableName, Position, Filterable
        {
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string DefaultValue { get; set; } = string.Empty;
            public ConfigurationValueType ValueType { get; set; } // Changed from bool Filterable, int Position, string Presentation
        }

        // Validator:
        public sealed class ParameterValidator : AbstractValidator<Parameter>
        {
            public ParameterValidator()
            {
                const string prefix = nameof(Setting); // Changed from nameof(OptionType)
                RuleFor(x => x.Key)
                    .NotEmpty()
                    .WithErrorCode(CommonInput.Errors.Required(prefix, nameof(Setting.Key)).Code)
                    .WithMessage(CommonInput.Errors.Required(prefix, nameof(Setting.Key)).Description)
                    .MaximumLength(Setting.Constraints.KeyMaxLength) // Use Setting constraints
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

                // Assuming ConfigurationValueType is an enum, its validation might be implicit or
                // custom if there are specific valid values beyond the enum's definition.
                // For now, no explicit validation added beyond default enum behavior.
            }
        }

        // Result:
        public record SelectItem
        {
            public Guid Id { get; set; }
            public string Key { get; set; } = string.Empty; // Changed from Name
            public string Value { get; set; } = string.Empty; // New property
        }

        public record ListItem
        {
            // Properties:
            public Guid Id { get; set; }
            public string Key { get; set; } = string.Empty; // Changed from Name
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public ConfigurationValueType ValueType { get; set; } // Changed from Position, Filterable

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
                config.NewConfig<Setting, SelectItem>()
                    .Map(dest => dest.Id, src => src.Id)
                    .Map(dest => dest.Key, src => src.Key)
                    .Map(dest => dest.Value, src => src.Value);

                // Setting -> ListItem
                config.NewConfig<Setting, ListItem>()
                    .Map(dest => dest.Id, src => src.Id)
                    .Map(dest => dest.Key, src => src.Key)
                    .Map(dest => dest.Value, src => src.Value)
                    .Map(dest => dest.Description, src => src.Description)
                    .Map(dest => dest.ValueType, src => src.ValueType)
                    .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                    .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

                // Setting -> Detail
                config.NewConfig<Setting, Detail>()
                    .Map(dest => dest.Id, src => src.Id)
                    .Map(dest => dest.Key, src => src.Key)
                    .Map(dest => dest.Value, src => src.Value)
                    .Map(dest => dest.Description, src => src.Description)
                    .Map(dest => dest.DefaultValue, src => src.DefaultValue)
                    .Map(dest => dest.ValueType, src => src.ValueType)
                    .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                    .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
            }
        }
    }
}
