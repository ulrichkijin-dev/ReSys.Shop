using MapsterMapper;

using  ReSys.Shop.Core.Common.Domain.Events;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;


using Serilog;

using Taxonomy = ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxonomy;

namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Taxonomies;

public static partial class TaxonomyModule
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
                var idRequired = CommonInput.Errors.Required(prefix: nameof(Taxonomy), nameof(Taxonomy.Id));
                RuleFor(expression: x => x.Id)
                    .NotEmpty()
                    .WithErrorCode(idRequired.Code)
                    .WithMessage(idRequired.Description);

                RuleFor(expression: x => x.Request)
                    .SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper
        ) : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
            {
                var request = command.Request;

                Taxonomy? taxonomy = await applicationDbContext.Set<Taxonomy>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: cancellationToken);
                if (taxonomy == null)
                {
                    return Taxonomy.Errors.NotFound(id: command.Id);
                }

                if (taxonomy.Name != request.Name)
                {
                    var uniqueNameCheck = await applicationDbContext.Set<Taxonomy>()
                        .Where(predicate: m => m.Id != taxonomy.Id)
                        .CheckNameIsUniqueAsync<Taxonomy, Guid>(
                            name: request.Name,
                            prefix: nameof(Taxonomy),
                            cancellationToken: cancellationToken);
                    if (uniqueNameCheck.IsError)
                        return uniqueNameCheck.Errors;
                }

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);

                var updateResult = taxonomy.Update(
                    name: request.Name,
                    presentation: request.Presentation,
                    position: request.Position,
                    publicMetadata: request.PublicMetadata,
                    privateMetadata: request.PrivateMetadata);

                if (updateResult.IsError) return updateResult.Errors;
                applicationDbContext.Set<Taxonomy>().Update(entity: updateResult.Value);


                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return mapper.Map<Result>(source: updateResult.Value);

            }
        }

        public sealed class EventHandler(IApplicationDbContext applicationDbContext) : IDomainEventHandler<Taxonomy.Events.Updated>
        {
            public async Task Handle(Taxonomy.Events.Updated notification, CancellationToken cancellationToken)
            {

                var taxonomy = await applicationDbContext.Set<Taxonomy>()
                    .Include(m=>m.Taxons)
                    .FirstOrDefaultAsync(m => m.Id == notification.TaxonomyId, cancellationToken);
                if (taxonomy == null)
                {
                    Log.Information("Taxonomy with Id {TaxonomyId} not found for Updated event handling.", notification.TaxonomyId);
                    return;
                }

                var rootTaxon = taxonomy.Root;
                if (rootTaxon != null)
                {
                    var updateRootTaxon = rootTaxon.Update(
                        name: notification.Name,
                        presentation: notification.Presentation);

                    if (updateRootTaxon.IsError)
                    {
                        Log.Information("Failed to update root taxon for Taxonomy Id {TaxonomyId}: {Errors}", notification.TaxonomyId, updateRootTaxon.Errors);
                        return;
                    }
                    applicationDbContext.Set<Taxon>()
                        .Update(entity: updateRootTaxon.Value);
                    await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                }
                else if (rootTaxon == null)
                {
                    var newRoot = Taxon.Create(
                        taxonomyId: notification.TaxonomyId,
                        name: taxonomy.Name,
                        parentId: null,
                        presentation: taxonomy.Presentation);

                    if (newRoot.IsError)
                    {
                        Log.Information("Failed to update root taxon for Taxonomy Id {TaxonomyId}: {Errors}", notification.TaxonomyId, newRoot.Errors);
                        return;
                    }
                    applicationDbContext.Set<Taxon>()
                        .Add(entity: newRoot.Value);
                    await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                }
            }
        }
    }
}