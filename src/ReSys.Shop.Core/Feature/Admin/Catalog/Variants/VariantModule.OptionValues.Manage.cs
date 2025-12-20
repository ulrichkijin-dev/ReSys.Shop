using ReSys.Shop.Core.Domain.Catalog.OptionTypes;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Variants;

public static partial class VariantModule
{
    public static partial class OptionValues
    {
        public static class Manage
        {
            public sealed record Request
            {
                public List<Guid> OptionValueIds { get; init; } = new();
            }

            public sealed record Command(Guid VariantId, Request Request) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(expression: x => x.VariantId).NotEmpty();
                    RuleFor(expression: x => x.Request.OptionValueIds).NotNull();
                }
            }

            public sealed class CommandHandler(IApplicationDbContext applicationDbContext)
                : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var variant = await applicationDbContext.Set<Variant>()
                        .Include(navigationPropertyPath: v => v.VariantOptionValues)
                        .ThenInclude(navigationPropertyPath: ovv => ovv.OptionValue)
                        .FirstOrDefaultAsync(predicate: v => v.Id == command.VariantId, cancellationToken: ct);

                    if (variant == null)
                        return Variant.Errors.NotFound(id: command.VariantId);

                    await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                    // Remove option values not in the new list
                    var existingIds = variant.VariantOptionValues.Select(selector: ovv => ovv.OptionValueId).ToHashSet();
                    var toRemove = existingIds.Except(second: command.Request.OptionValueIds).ToList();

                    foreach (var optionValueId in toRemove)
                    {
                        var removeResult = variant.RemoveOptionValue(optionValueId: optionValueId);
                        if (removeResult.IsError) return removeResult.FirstError;
                    }

                    // Add new option values
                    var toAdd = command.Request.OptionValueIds.Except(second: existingIds).ToList();
                    foreach (var optionValueId in toAdd)
                    {
                        var optionValue = await applicationDbContext.Set<OptionValue>()
                            .FindAsync(keyValues: [optionValueId], cancellationToken: ct);

                        if (optionValue == null)
                            return OptionValue.Errors.NotFound(id: optionValueId);

                        var addResult = variant.AddOptionValue(optionValue: optionValue);
                        if (addResult.IsError) return addResult.FirstError;
                    }

                    await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                    await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                    return Result.Success;
                }
            }
        }
    }
}