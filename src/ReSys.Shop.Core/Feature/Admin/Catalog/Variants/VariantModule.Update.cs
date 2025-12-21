using MapsterMapper;

using ReSys.Shop.Core.Domain.Catalog.Products.Variants;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Variants;

public static partial class VariantModule
{
    public static class Update
    {
        public record Request : Models.Parameter
        {
            public List<Guid> OptionValueIds { get; init; } = new();
        }
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
            ISender sender,
            IApplicationDbContext applicationDbContext,
            IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var param = command.Request;
                var variant = await applicationDbContext.Set<Variant>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: ct);
                if (variant == null)
                    return Variant.Errors.NotFound(id: command.Id);

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var updateResult = variant.Update(
                    sku: param.Sku,
                    barcode: param.Barcode,
                    weight: param.Weight,
                    height: param.Height,
                    width: param.Width,
                    depth: param.Depth,
                    dimensionsUnit: param.DimensionsUnit,
                    weightUnit: param.WeightUnit,
                    trackInventory: param.TrackInventory,
                    costPrice: param.CostPrice,
                    costCurrency: param.CostCurrency,
                    position: param.Position,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: param.PrivateMetadata);

                if (updateResult.IsError) return updateResult.Errors;
                
                applicationDbContext.Set<Variant>().Update(updateResult.Value);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                var setOptionValuesResult = await sender.Send(new OptionValues.Manage.Command(updateResult.Value.Id,
                    new OptionValues.Manage.Request()
                    {
                        OptionValueIds = param.OptionValueIds
                    }), ct);

                if (setOptionValuesResult.IsError)
                    return setOptionValuesResult.Errors;

                return mapper.Map<Result>(source: updateResult.Value);
            }
        }
    }
}