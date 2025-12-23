using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Products.Reviews;

namespace ReSys.Shop.Core.Feature.Admin.Catalog.Reviews;

public static partial class ReviewModule
{
    public static class Get
    {
        public sealed record PagedListQuery(QueryableParams Params) : IQuery<PaginationList<Models.ReviewItem>>;

        public sealed class PagedListHandler(IApplicationDbContext dbContext, IMapper mapper)
            : IQueryHandler<PagedListQuery, PaginationList<Models.ReviewItem>>
        {
            public async Task<ErrorOr<PaginationList<Models.ReviewItem>>> Handle(PagedListQuery request, CancellationToken ct)
            {
                var query = dbContext.Set<Review>()
                    .Include(r => r.Product)
                    .Include(r => r.User)
                    .AsNoTracking();

                query = query.ApplySearch(request.Params)
                             .ApplyFilters(request.Params)
                             .ApplySort(request.Params);

                if (string.IsNullOrWhiteSpace(request.Params.SortBy))
                {
                    query = query.OrderByDescending(r => r.CreatedAt);
                }

                return await query
                    .ProjectToType<Models.ReviewItem>(mapper.Config)
                    .ToPagedListAsync(request.Params, ct);
            }
        }

        public sealed record ByIdQuery(Guid Id) : IQuery<Models.ReviewItem>;

        public sealed class ByIdHandler(IApplicationDbContext dbContext, IMapper mapper)
            : IQueryHandler<ByIdQuery, Models.ReviewItem>
        {
            public async Task<ErrorOr<Models.ReviewItem>> Handle(ByIdQuery request, CancellationToken ct)
            {
                var review = await dbContext.Set<Review>()
                    .Include(r => r.Product)
                    .Include(r => r.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

                if (review == null) return Error.NotFound("Review.NotFound");

                return mapper.Map<Models.ReviewItem>(review);
            }
        }
    }

    public static class Actions
    {
        public static class Approve
        {
            public record Request(string? Notes = null);
            public sealed record Command(Guid Id, Request Request) : ICommand<Models.ReviewItem>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IUserContext userContext, IMapper mapper)
                : ICommandHandler<Command, Models.ReviewItem>
            {
                public async Task<ErrorOr<Models.ReviewItem>> Handle(Command command, CancellationToken ct)
                {
                    var review = await dbContext.Set<Review>()
                        .FirstOrDefaultAsync(r => r.Id == command.Id, ct);

                    if (review == null) return Error.NotFound("Review.NotFound");

                    var moderatorId = userContext.UserId ?? "System";
                    var result = review.Approve(moderatorId, command.Request.Notes);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.ReviewItem>(review);
                }
            }
        }

        public static class Reject
        {
            public record Request(string Reason);
            public sealed record Command(Guid Id, Request Request) : ICommand<Models.ReviewItem>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IUserContext userContext, IMapper mapper)
                : ICommandHandler<Command, Models.ReviewItem>
            {
                public async Task<ErrorOr<Models.ReviewItem>> Handle(Command command, CancellationToken ct)
                {
                    var review = await dbContext.Set<Review>()
                        .FirstOrDefaultAsync(r => r.Id == command.Id, ct);

                    if (review == null) return Error.NotFound("Review.NotFound");

                    var moderatorId = userContext.UserId ?? "System";
                    var result = review.Reject(moderatorId, command.Request.Reason);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.ReviewItem>(review);
                }
            }
        }

        public static class Delete
        {
            public sealed record Command(Guid Id) : ICommand<Deleted>;

            public sealed class CommandHandler(IApplicationDbContext dbContext)
                : ICommandHandler<Command, Deleted>
            {
                public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
                {
                    var review = await dbContext.Set<Review>()
                        .FirstOrDefaultAsync(r => r.Id == command.Id, ct);

                    if (review == null) return Error.NotFound("Review.NotFound");

                    dbContext.Set<Review>().Remove(review);
                    await dbContext.SaveChangesAsync(ct);

                    return Result.Deleted;
                }
            }
        }
    }
}
