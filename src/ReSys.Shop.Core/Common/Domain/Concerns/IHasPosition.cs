using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasPosition
{
    int Position { get; set; }
}

public static class HasPosition
{
    public static bool IsValid(int position) => position >= 1;

    public static void SetPosition(this IHasPosition? target, int position)
    {
        if (target == null || !IsValid(position: position)) return;
        target.Position = position;
    }

    public static void Increment(this IHasPosition? target)
    {
        if (target == null) return;
        target.Position++;
    }

    public static void Decrement(this IHasPosition? target)
    {
        if (target == null) return;
        target.Position = Math.Max(val1: 1, val2: target.Position - 1);
    }

    public static void AddPositionRules<T>(this AbstractValidator<T> validator, string prefix) where T : IHasPosition
    {
        validator.RuleFor(expression: x => x.Position)
            .GreaterThanOrEqualTo(valueToCompare: 1)
            .WithErrorCode(errorCode: CommonInput.Errors.OutOfRange(prefix: prefix, field: nameof(IHasPosition.Position), minValue: 1).Code)
            .WithMessage(errorMessage: CommonInput.Errors.OutOfRange(prefix: prefix, field: nameof(IHasPosition.Position), minValue: 1).Description);
    }

    public static void ConfigurePosition<T>(this EntityTypeBuilder<T> builder) where T : class, IHasPosition
    {
        builder.Property(propertyExpression: x => x.Position)
            .IsRequired()
            .HasDefaultValue(value: 1)
            .HasComment(comment: "Position: Sortable ordering of the entity, minimum value is 1.");

        builder.HasIndex(indexExpression: x => x.Position);
    }
}
