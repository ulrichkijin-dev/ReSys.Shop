using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasVersion
{
    long Version { get; set; }
}

public static class HasVersion
{
    public static void AddVersionRules<T>(this AbstractValidator<T> validator) where T : IHasVersion
    {
        validator.RuleFor(expression: x => x.Version)
            .GreaterThanOrEqualTo(valueToCompare: 0)
            .WithErrorCode(errorCode: CommonInput.Errors.OutOfRange(
                field: nameof(IHasVersion.Version),
                minValue: 0).Code)
            .WithMessage(errorMessage: CommonInput.Errors.OutOfRange(
                field: nameof(IHasVersion.Version),
                minValue: 0).Description);
    }

    public static void ConfigureVersion<T>(this EntityTypeBuilder<T> builder) where T : class, IHasVersion
    {
        builder.Property(propertyExpression: x => x.Version)
            .IsRequired()
            .HasDefaultValue(value: 0L)
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken()
            .HasComment(comment: "Version: Optimistic concurrency token, incremented on updates.");
    }
}