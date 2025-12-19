using System.Data;

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ReSys.Shop.Core.Common.Services.Persistence.Interfaces;

/// <summary>
/// Single entry point for database access, persistence, and transaction management.
/// </summary>
public interface IApplicationDbContext : IDisposable, IAsyncDisposable
{
    DbSet<T> Set<T>() where T : class;
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    bool HasActiveTransaction { get; }
    Guid? CurrentTransactionId { get; }

    Task<int> ExecuteSqlAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters);

    Task ExecuteInTransactionAsync(
        Func<Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}