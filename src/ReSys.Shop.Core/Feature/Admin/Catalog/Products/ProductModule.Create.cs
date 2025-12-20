using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Products;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static class Create
    {
        public sealed record Request : Models.Parameter;
        public sealed record Result : Models.ListItem;
        public sealed record Command(Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Request)
                    .SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var param = command.Request;
                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var uniqueNameCheck = await applicationDbContext.Set<Product>()
                    .CheckNameIsUniqueAsync<Product, Guid>(name: param.Name, prefix: nameof(Product), cancellationToken: ct);
                if (uniqueNameCheck.IsError)
                    return uniqueNameCheck.Errors;

                var createResult = Product.Create(
                    name: param.Name,
                    description: param.Description,
                    slug: param.Slug,
                    metaTitle: param.MetaTitle,
                    metaDescription: param.MetaDescription,
                    metaKeywords: param.MetaKeywords,
                    availableOn: param.AvailableOn,
                    makeActiveAt: param.MakeActiveAt,
                    discontinueOn: param.DiscontinueOn,
                    isDigital: param.IsDigital,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: param.PrivateMetadata);

                if (createResult.IsError) return createResult.Errors;

                applicationDbContext.Set<Product>().Add(entity: createResult.Value);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return mapper.Map<Result>(source: createResult.Value);
            }
        }
    }
}