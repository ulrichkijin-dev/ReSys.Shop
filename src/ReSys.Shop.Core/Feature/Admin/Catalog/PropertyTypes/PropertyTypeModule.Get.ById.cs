using Mapster;

using MapsterMapper;

using ReSys.Shop.Core.Domain.Catalog.PropertyTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.PropertyTypes;

public static partial class PropertyTypeModule
{
    public static partial class Get
    {
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
                    var property = await dbContext.Set<PropertyType>()
                        .Include(navigationPropertyPath: p => p.ProductPropertyTypes)
                        .ProjectToType<Result>(config: mapper.Config)
                        .FirstOrDefaultAsync(predicate: p => p.Id == request.Id, cancellationToken: cancellationToken);

                    if (property == null)
                        return PropertyType.Errors.NotFound(id: request.Id);

                    return property;
                }
            }
        }
    }
}