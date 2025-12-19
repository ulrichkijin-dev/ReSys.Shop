using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasSlug
{
    string Slug { get; set; }
}

public static class HasSlug
{
    public static void AddSlugRules<T>(this AbstractValidator<T> validator, string prefix) where T : IHasSlug
    {
        validator.RuleFor(expression: x => x.Slug)
            .NotEmpty()
            .WithErrorCode(errorCode: CommonInput.Errors.Required(prefix: prefix, field: nameof(IHasSlug.Slug)).Code)
            .WithMessage(errorMessage: CommonInput.Errors.Required(prefix: prefix, field: nameof(IHasSlug.Slug)).Description)
            .MaximumLength(maximumLength: CommonInput.Constraints.SlugsAndVersions.SlugMaxLength)
            .WithErrorCode(errorCode: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSlug.Slug),
                maxLength: CommonInput.Constraints.SlugsAndVersions.SlugMaxLength).Code)
            .WithMessage(errorMessage: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSlug.Slug),
                maxLength: CommonInput.Constraints.SlugsAndVersions.SlugMaxLength).Description)
            .Matches(expression: CommonInput.Constraints.SlugsAndVersions.SlugPattern)
            .WithErrorCode(errorCode: CommonInput.Errors.InvalidSlug(prefix: prefix, field: nameof(IHasSlug.Slug)).Code)
            .WithMessage(errorMessage: CommonInput.Errors.InvalidSlug(prefix: prefix, field: nameof(IHasSlug.Slug)).Description);
    }

    public static void ConfigureSlug<T>(this EntityTypeBuilder<T> builder) where T : class, IHasSlug
    {
        builder.Property(propertyExpression: x => x.Slug)
            .IsRequired()
            .HasMaxLength(maxLength: CommonInput.Constraints.SlugsAndVersions.SlugMaxLength)
            .HasComment(comment: "Slug: URL-friendly identifier, required and unique if needed.");
    }

    public static IQueryable<T> SearchBySlug<T>(this IQueryable<T> query, string? term) where T : IHasSlug
    {
        if (string.IsNullOrWhiteSpace(value: term)) return query;

        return query.Where(predicate: x => x.Slug.Contains(term));
    }
}
