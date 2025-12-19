using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasSeoMetadata
{
    string? MetaTitle { get; set; }

    string? MetaDescription { get; set; }

    string? MetaKeywords { get; set; }
}

public static class HasSeoMetadata
{
    public static void AddSeoMetaSupportRules<T>(this AbstractValidator<T> validator, string prefix) where T : IHasSeoMetadata
    {
        validator.RuleFor(expression: x => x.MetaTitle)
            .MaximumLength(maximumLength: CommonInput.Constraints.Text.TitleMaxLength)
            .WithErrorCode(errorCode: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSeoMetadata.MetaTitle),
                maxLength: CommonInput.Constraints.Text.TitleMaxLength).Code)
            .WithMessage(errorMessage: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSeoMetadata.MetaTitle),
                maxLength: CommonInput.Constraints.Text.TitleMaxLength).Description);

        validator.RuleFor(expression: x => x.MetaDescription)
            .MaximumLength(maximumLength: CommonInput.Constraints.Text.DescriptionMaxLength)
            .WithErrorCode(errorCode: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSeoMetadata.MetaDescription),
                maxLength: CommonInput.Constraints.Text.DescriptionMaxLength).Code)
            .WithMessage(errorMessage: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSeoMetadata.MetaDescription),
                maxLength: CommonInput.Constraints.Text.DescriptionMaxLength).Description);

        validator.RuleFor(expression: x => x.MetaKeywords)
            .MaximumLength(maximumLength: CommonInput.Constraints.Text.ShortTextMaxLength)
            .WithErrorCode(errorCode: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSeoMetadata.MetaKeywords),
                maxLength: CommonInput.Constraints.Text.ShortTextMaxLength).Code)
            .WithMessage(errorMessage: CommonInput.Errors.TooLong(
                prefix: prefix,
                field: nameof(IHasSeoMetadata.MetaKeywords),
                maxLength: CommonInput.Constraints.Text.ShortTextMaxLength).Description);
    }

    public static void ConfigureSeoMetadata<T>(this EntityTypeBuilder<T> builder) where T : class, IHasSeoMetadata
    {
        builder.Property(propertyExpression: x => x.MetaTitle)
            .HasMaxLength(maxLength: CommonInput.Constraints.Text.TitleMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "MetaTitle: Optional SEO title for the entity.");

        builder.Property(propertyExpression: x => x.MetaDescription)
            .HasMaxLength(maxLength: CommonInput.Constraints.Text.DescriptionMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "MetaDescription: Optional SEO description for the entity.");

        builder.Property(propertyExpression: x => x.MetaKeywords)
            .HasMaxLength(maxLength: CommonInput.Constraints.Text.ShortTextMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "MetaKeywords: Optional SEO keywords (comma-separated).");
    }
}
