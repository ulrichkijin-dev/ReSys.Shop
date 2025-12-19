using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTimeOffset? DeletedAt { get; set; }

    string? DeletedBy { get; set; }
}

public static class SoftDeletable
{
    /// <summary>
    /// Marks the entity as deleted with UTC now and optional userId.
    /// </summary>
    public static void MarkAsDeleted(this ISoftDeletable? target, string? deletedBy = null, DateTimeOffset? deletedAt = null)
    {
        if (target == null) return;

        target.IsDeleted = true;
        target.DeletedAt = deletedAt ?? DateTimeOffset.UtcNow;
        target.DeletedBy = deletedBy;
    }

    /// <summary>
    /// Restores the entity by clearing soft-delete markers.
    /// </summary>
    public static void Restore(this ISoftDeletable? target)
    {
        if (target == null) return;

        target.IsDeleted = false;
        target.DeletedAt = null;
        target.DeletedBy = null;
    }

    public static void AddSoftDeleteRules<T>(this AbstractValidator<T> validator)
        where T : ISoftDeletable
    {
        validator.RuleFor(expression: x => x.IsDeleted)
            .NotNull()
            .WithErrorCode(errorCode: CommonInput.Errors.Required(prefix: nameof(ISoftDeletable.IsDeleted)).Code)
            .WithMessage(errorMessage: CommonInput.Errors.Required(prefix: nameof(ISoftDeletable.IsDeleted)).Description);

        validator.RuleFor(expression: x => x.DeletedBy)
            .MaximumLength(maximumLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .WithErrorCode(errorCode: CommonInput.Errors.TooLong(
                field: nameof(ISoftDeletable.DeletedBy),
                maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength).Code)
            .WithMessage(errorMessage: CommonInput.Errors.TooLong(
                field: nameof(ISoftDeletable.DeletedBy),
                maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength).Description);
    }

    public static void ConfigureSoftDelete<T>(this EntityTypeBuilder<T> builder)
        where T : class, ISoftDeletable
    {
        builder.Property(propertyExpression: m => m.IsDeleted)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment(comment: "IsDeleted: Indicates if the entity is soft-deleted.");

        builder.Property(propertyExpression: m => m.DeletedAt)
            .IsRequired(required: false)
            .HasComment(comment: "DeletedAt: Timestamp when the entity was soft-deleted.");

        builder.Property(propertyExpression: m => m.DeletedBy)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "DeletedBy: User who soft-deleted the entity.");

        builder.HasIndex(indexExpression: m => m.IsDeleted);
    }
}
