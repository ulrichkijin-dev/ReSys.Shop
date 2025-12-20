using Mapster;

using MapsterMapper;

using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Settings;

// Changed from Catalog.OptionTypes

namespace ReSys.Shop.Core.Feature.Admin.Settings.SettingModule;

public static partial class SettingModule
{
    public static partial class Get
    {
        // Select List:
        public static class SelectList
        {
            public sealed class Request : QueryableParams;
            public sealed record Result : Models.SelectItem;

            public sealed record Query(Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(
                IApplicationDbContext dbContext,
                IMapper mapper
            ) : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(Query command, CancellationToken cancellationToken)
                {
                    PaginationList<Result> pagedResult = await dbContext.Set<Setting>().AsQueryable()
                        .AsNoTracking()
                        .ApplySearch(searchParams: command.Request) // Assumes search can be applied to 'Key' or 'Description'
                        .ApplyFilters(filterParams: command.Request)
                        .ApplySort(sortParams: command.Request)
                        .ProjectToType<Result>(config: mapper.Config)
                        .ToPagedListOrAllAsync(pagingParams: command.Request,
                            cancellationToken: cancellationToken);

                    return pagedResult;
                }
            }
        }

        // Paged List:
        public static class PagedList
        {
            public sealed class Request : QueryableParams;
            public sealed record Result : Models.ListItem;

            public sealed record Query(Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(
                IApplicationDbContext dbContext,
                IMapper mapper
            ) : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(Query command,
                    CancellationToken cancellationToken)
                {

                    var pagedResult = await dbContext.Set<Setting>()
                        .AsQueryable()
                        .AsNoTracking()
                        .ApplySearch(searchParams: command.Request)
                        .ApplyFilters(filterParams: command.Request)
                        .ApplySort(sortParams: command.Request)
                        .ProjectToType<Result>(config: mapper.Config)
                        .ToPagedListAsync(pagingParams: command.Request,
                            cancellationToken: cancellationToken);

                    return pagedResult;
                }
            }
        }
        // By Id:
        public static class ById
        {
            public sealed record Result : Models.Detail;
            public sealed record Query(Guid Id) : IQuery<Result>;
            public sealed class QueryHandler(
                IApplicationDbContext dbContext,
                IMapper mapper
            ) : IQueryHandler<Query, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Query request, CancellationToken cancellationToken)
                {
                    var setting = await dbContext.Set<Setting>()
                        .ProjectToType<Result>(config: mapper.Config)
                        .FirstOrDefaultAsync(predicate: p => p.Id == request.Id, cancellationToken: cancellationToken);

                    if (setting == null)
                        return Setting.Errors.NotFound; // Changed from OptionType.Errors.NotFound(id: request.Id);

                    return setting;
                }
            }
        }

    }
}
