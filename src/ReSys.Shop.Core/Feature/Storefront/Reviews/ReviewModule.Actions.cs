using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Products.Reviews;

namespace ReSys.Shop.Core.Feature.Storefront.Reviews;

public static partial class ReviewModule
{
    public static class Get
    {
        public sealed record PagedListQuery(Guid ProductId, QueryableParams Params) : IQuery<PaginationList<Models.ReviewItem>>;

        public sealed class PagedListHandler(IApplicationDbContext dbContext, IMapper mapper)
            : IQueryHandler<PagedListQuery, PaginationList<Models.ReviewItem>>
        {
            public async Task<ErrorOr<PaginationList<Models.ReviewItem>>> Handle(PagedListQuery request, CancellationToken ct)
            {
                var query = dbContext.Set<Review>()
                    .Include(r => r.User)
                    .Where(r => r.ProductId == request.ProductId && r.Status == Review.ReviewStatus.Approved)
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
    }

    public static class Actions
    {
        public static class Create
        {
            public record Request(int Rating, string? Title = null, string? Comment = null);
            public sealed record Command(Guid ProductId, Request Request) : ICommand<Models.ReviewItem>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IUserContext userContext, IMapper mapper)
                : ICommandHandler<Command, Models.ReviewItem>
            {
                public async Task<ErrorOr<Models.ReviewItem>> Handle(Command command, CancellationToken ct)
                {
                    if (userContext.UserId == null) return Error.Unauthorized();

                    var reviewResult = Review.Create(
                        productId: command.ProductId,
                        userId: userContext.UserId,
                        rating: command.Request.Rating,
                        title: command.Request.Title,
                        comment: command.Request.Comment);

                    if (reviewResult.IsError) return reviewResult.Errors;

                    dbContext.Set<Review>().Add(reviewResult.Value);
                    await dbContext.SaveChangesAsync(ct);

                    return mapper.Map<Models.ReviewItem>(reviewResult.Value);
                }
            }
        }

        public static class Vote
        {
            public record Request(bool Helpful);
            public sealed record Command(Guid ReviewId, Request Request) : ICommand<Models.ReviewItem>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper)
                : ICommandHandler<Command, Models.ReviewItem>
            {
                public async Task<ErrorOr<Models.ReviewItem>> Handle(Command command, CancellationToken ct)
                {
                    var review = await dbContext.Set<Review>()
                        .FirstOrDefaultAsync(r => r.Id == command.ReviewId, ct);

                    if (review == null) return Error.NotFound("Review.NotFound");

                    review.VoteHelpful(command.Request.Helpful);
                    await dbContext.SaveChangesAsync(ct);

                    return mapper.Map<Models.ReviewItem>(review);
                }
            }
        }
    }
}
