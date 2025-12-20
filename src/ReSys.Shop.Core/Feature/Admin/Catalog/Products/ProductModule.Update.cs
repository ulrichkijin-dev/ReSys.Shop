using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Products;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static class Update
    {
        public record Request : Models.Parameter;
        public record Result : Models.ListItem;
        public sealed record Command(Guid Id, Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id).NotEmpty();
                RuleFor(expression: x => x.Request).SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var request = command.Request;
                var product = await applicationDbContext.Set<Product>()
                    .Include(p => p.Variants) // Eager load variants
                    .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken: ct);
                if (product == null)
                    return Product.Errors.NotFound(id: command.Id);

                var uniqueNameCheck = await applicationDbContext.Set<Product>()
                    .Where(predicate: m => m.Id != product.Id)
                    .CheckNameIsUniqueAsync<Product, Guid>(name: request.Name, prefix: nameof(Product), cancellationToken: ct);
                if (uniqueNameCheck.IsError)
                    return uniqueNameCheck.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var updateResult = product.Update(
                    name: request.Name,
                    presentation: request.Presentation,
                    description: request.Description,
                    slug: request.Slug,
                    metaTitle: request.MetaTitle,
                    metaDescription: request.MetaDescription,
                    metaKeywords: request.MetaKeywords,
                    availableOn: request.AvailableOn,
                    makeActiveAt: request.MakeActiveAt,
                    discontinueOn: request.DiscontinueOn,
                    isDigital: request.IsDigital,
                    publicMetadata: request.PublicMetadata,
                    privateMetadata: request.PrivateMetadata);

                if (updateResult.IsError) return updateResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return mapper.Map<Result>(source: updateResult.Value);
            }
        }
    }
}