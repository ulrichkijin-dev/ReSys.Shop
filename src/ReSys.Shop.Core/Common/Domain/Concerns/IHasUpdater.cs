using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasUpdater
{
    DateTimeOffset? UpdatedAt { get; set; }

    string? UpdatedBy { get; set; }
}

public static class HasUpdater
{
    /// <summary>
    /// Marks the entity as updated by setting UpdatedAt to UtcNow and optionally UpdatedBy.
    /// </summary>
    public static void MarkAsUpdated(this IHasUpdater? target, string? updatedBy = null)
    {
        if (target == null) return;
        target.UpdatedAt = DateTimeOffset.UtcNow;
        target.UpdatedBy = updatedBy;
    }

    public static void AddUpdaterRules<T>(this AbstractValidator<T> validator) where T : IHasUpdater
    {
        validator.RuleFor(expression: x => x.UpdatedAt)
            .Must(predicate: at => !at.HasValue || at.Value != default)
            .When(predicate: x => x.UpdatedAt.HasValue);

        validator.RuleFor(expression: x => x.UpdatedBy)
            .NotEmpty()
            .When(predicate: x => !string.IsNullOrWhiteSpace(value: x.UpdatedBy));
    }

    public static void ConfigureUpdater<T>(this EntityTypeBuilder<T> builder) where T : class, IHasUpdater
    {
        builder.Property(propertyExpression: m => m.UpdatedBy)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.UsernameMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "UpdatedBy: User who last updated this record.");

        builder.Property(propertyExpression: m => m.UpdatedAt)
            .IsRequired(required: false)
            .HasComment(comment: "UpdatedAt: Timestamp of when the record was last updated.");
    }
}
