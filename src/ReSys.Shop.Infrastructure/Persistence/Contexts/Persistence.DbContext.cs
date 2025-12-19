using System.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Services.Persistence.Interfaces;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Catalog.OptionTypes;
using ReSys.Shop.Core.Domain.Identity.Roles;
using ReSys.Shop.Core.Domain.Identity.Roles.Claims;
using ReSys.Shop.Core.Domain.Identity.Users;
using ReSys.Shop.Core.Domain.Identity.Users.Claims;
using ReSys.Shop.Core.Domain.Identity.Users.Logins;
using ReSys.Shop.Core.Domain.Identity.Users.Roles;
using ReSys.Shop.Core.Domain.Identity.Users.Tokens;

namespace ReSys.Shop.Infrastructure.Persistence.Contexts;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<
        User, Role, string,
        UserClaim, UserRole, UserLogin,
        RoleClaim, UserToken>(options: options), IApplicationDbContext
{
    private IDbContextTransaction? _currentTransaction;
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder: builder);
        builder.HasPostgresExtension(name: "vector");
        builder.HasDefaultSchema(schema: Schema.Default);

        builder.ApplyConfigurationsFromAssembly(assembly: typeof(OptionType).Assembly);
        builder.ApplyUtcConversions();

    }
    public bool HasActiveTransaction => _currentTransaction != null;

    public Guid? CurrentTransactionId => _currentTransaction?.TransactionId;

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken: cancellationToken);
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction = await Database.BeginTransactionAsync(
            isolationLevel: isolationLevel,
            cancellationToken: cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            return;

        try
        {
            await SaveChangesAsync(cancellationToken: cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken: cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken: cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<int> ExecuteSqlAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters)
    {
        return await Database.ExecuteSqlRawAsync(
            sql: sql,
            parameters: parameters,
            cancellationToken: cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
        {
            await action();
            return;
        }

        await using var transaction =
            await Database.BeginTransactionAsync(isolationLevel: isolationLevel, cancellationToken: cancellationToken);

        try
        {
            await action();
            await SaveChangesAsync(cancellationToken: cancellationToken);
            await transaction.CommitAsync(cancellationToken: cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken: cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
        {
            return await action();
        }

        await using var transaction =
            await Database.BeginTransactionAsync(isolationLevel: isolationLevel, cancellationToken: cancellationToken);

        try
        {
            var result = await action();
            await SaveChangesAsync(cancellationToken: cancellationToken);
            await transaction.CommitAsync(cancellationToken: cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken: cancellationToken);
            throw;
        }
    }

}
