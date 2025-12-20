namespace ReSys.Shop.Core.Common.Domain.Concerns;

public static class UniqueKeyExtensions
{
    /// <summary>
    /// Extension method to check if a key is unique within a DbSet.
    /// This method is intended for entities that have a unique 'Key' property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity, which must have a 'Key' string property.</typeparam>
    /// <typeparam name="TId">The type of the entity's ID.</typeparam>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <param name="key">The key value to check for uniqueness.</param>
    /// <param name="prefix">A prefix used for generating error codes (e.g., "Setting").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="exclusions">Optional: A list of entity IDs to exclude from the uniqueness check (e.g., for update scenarios).</param>
    /// <returns>An ErrorOr result indicating success (true) or an error if the key is not unique.</returns>
    public static async Task<ErrorOr<bool>> CheckKeyIsUniqueAsync<TEntity, TId>(
        this IQueryable<TEntity> dbSet,
        string key,
        string prefix,
        CancellationToken cancellationToken,
        params TId[] exclusions)
        where TEntity : class, IHasIdentity<TId>
        where TId : struct
    {
        // Check if another entity with the same key exists, excluding the provided IDs.
        bool isDuplicate = await dbSet.AnyAsync(
            predicate: e => EF.Property<string>(e, "Key") == key && !exclusions.Contains(e.Id),
            cancellationToken: cancellationToken);

        if (isDuplicate)
        {
            return CommonInput.Errors.DuplicateItems(prefix, field: "Key", customMessage: $"The key '{key}' already exists.");
        }

        return true;
    }
}