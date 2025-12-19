using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Extensions;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasUniqueName
{
    string Name { get; set; }
}

public static class HasUniqueName
{
    public static async Task<bool> IsUniqueAsync<T>(this IQueryable<T> query, string name, CancellationToken ct = default)
        where T : IHasUniqueName
    {
        return !await query.AnyAsync(predicate: x => x.Name == name, cancellationToken: ct);
    }

    public static void AddUniqueNameRules<T>(this AbstractValidator<T> validator, string prefix = nameof(IHasUniqueName)) where T : IHasUniqueName
    {
        validator.RuleFor(expression: x => x.Name)
            .NotEmpty()
            .WithErrorCode(errorCode: CommonInput.Errors.Required(prefix: prefix, field: nameof(IHasUniqueName.Name)).Code)
            .WithMessage(errorMessage: CommonInput.Errors.Required(prefix: prefix, field: nameof(IHasUniqueName.Name)).Description)
            .MaximumLength(maximumLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .WithErrorCode(errorCode: CommonInput.Errors.TooLong(prefix: prefix, field: nameof(IHasUniqueName.Name), maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength).Code)
            .WithMessage(errorMessage: CommonInput.Errors.TooLong(prefix: prefix, field: nameof(IHasUniqueName.Name), maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength).Description);
    }

    public static void ConfigureUniqueName<T>(this EntityTypeBuilder<T> builder) where T : class, IHasUniqueName
    {
        builder.Property(propertyExpression: x => x.Name)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired() 
            .HasComment(comment: "Name: The unique name of the entity.");

        builder.HasIndex(indexExpression: x => x.Name)
            .IsUnique();
    }


    public static async Task<ErrorOr<bool>> CheckNameIsUniqueAsync<TEntity, TId>(
        this IQueryable<TEntity> queryable,
        string? name,
        string? prefix = null,
        CancellationToken cancellationToken = default,
        params object[] exclusions)
        where TEntity : class, IHasUniqueName, IEntity<TId>
        where TId : struct, IEquatable<TId>
    {
        name = name?.Trim().ToSlug() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value: name))
            return CommonInput.Errors.Required(prefix: prefix, field: "Name");

        IQueryable<TEntity> query = queryable.AsNoTracking();

        HashSet<TId> excludedIds = exclusions
            .OfType<TId>()
            .Where(predicate: id => !id.Equals(other: default))
            .ToHashSet();

        HashSet<string> excludedNames = exclusions
            .OfType<string>()
            .Where(predicate: s => !string.IsNullOrWhiteSpace(value: s))
            .Select(selector: s => s.Trim().ToLowerInvariant())
            .ToHashSet();

        if (excludedIds.Count > 0)
            query = query.Where(predicate: x => !excludedIds.Contains(x.Id));

        if (excludedNames.Count > 0)
            query = query.Where(predicate: x => !excludedNames.Contains(x.Name.ToLower()));

        bool exists = await query.AnyAsync(
            predicate: x => x.Name == name,
            cancellationToken: cancellationToken);

        return exists
            ? CommonInput.Errors.DuplicateItems(prefix: prefix)
            : true;
    }

}
