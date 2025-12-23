using ReSys.Shop.Core.Common.Models.Pagination;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Domain.Auditing;

namespace ReSys.Shop.Core.Feature.Admin.SystemAdmin;

public static partial class SystemModule
{
    public static class Get
    {
        public sealed record AuditLogsQuery(PagingParams Params) : IQuery<PaginationList<Models.AuditLogItem>>;

        public sealed class AuditLogsHandler(IApplicationDbContext dbContext)
            : IQueryHandler<AuditLogsQuery, PaginationList<Models.AuditLogItem>>
        {
            public async Task<ErrorOr<PaginationList<Models.AuditLogItem>>> Handle(AuditLogsQuery request, CancellationToken ct)
            {
                var query = dbContext.Set<AuditLog>()
                    .AsNoTracking()
                    .OrderByDescending(l => l.Timestamp)
                    .Select(l => new Models.AuditLogItem
                    {
                        Id = l.Id,
                        EntityName = l.EntityName,
                        EntityId = l.EntityId.ToString(),
                        Action = l.Action,
                        Timestamp = l.Timestamp,
                        UserId = l.UserId,
                        UserName = l.UserName,
                        UserEmail = l.UserEmail,
                        IpAddress = l.IpAddress,
                        UserAgent = l.UserAgent,
                        OldValues = l.OldValues,
                        NewValues = l.NewValues,
                        ChangedProperties = l.ChangedProperties,
                        Reason = l.Reason,
                        Severity = l.Severity
                    });

                var result = await query.ToPagedListAsync(
                    pagingParams: request.Params,
                    cancellationToken: ct);

                return result;
            }
        }
    }
}