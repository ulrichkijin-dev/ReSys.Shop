using  ReSys.Shop.Core.Common.Domain.Events;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;


using Serilog;

namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Taxonomies;

public static partial class TaxonomyModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;
        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                var idRequired = CommonInput.Errors.Required(prefix: nameof(Taxonomy), nameof(Taxonomy.Id));
                RuleFor(expression: x => x.Id)
                    .NotEmpty()
                    .WithErrorCode(idRequired.Code)
                    .WithMessage(idRequired.Description);
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext
        ) : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
            {
                // Fetch: 
                var taxonomy = await applicationDbContext.Set<Taxonomy>()
                    .Include(navigationPropertyPath: t => t.Taxons)
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: cancellationToken);

                // Check: existence
                if (taxonomy == null)
                    return Taxonomy.Errors.NotFound(id: command.Id);

                // Check: deletable
                var deleteResult = taxonomy.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);
                applicationDbContext.Set<Taxonomy>().Remove(entity: taxonomy);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return Result.Deleted;
            }
        }

        public sealed class EventHandler(IApplicationDbContext applicationDbContext) : IDomainEventHandler<Taxonomy.Events.Created>
        {
            public async Task Handle(Taxonomy.Events.Created notification, CancellationToken cancellationToken)
            {

                var taxonomy = await applicationDbContext.Set<Taxonomy>()
                    .Include(m => m.Taxons)
                    .FirstOrDefaultAsync(m => m.Id == notification.TaxonomyId, cancellationToken);
                if (taxonomy != null)
                {
                    Log.Information("Taxonomy with Id {TaxonomyId} still found for Deleted event handling.", notification.TaxonomyId);
                    return;
                }

                var taxonomyRoot = await applicationDbContext.Set<Taxon>()
                    .FirstOrDefaultAsync(m => m.TaxonomyId == notification.TaxonomyId, cancellationToken);
                if (taxonomyRoot != null)
                {
                    var deleteResult = taxonomyRoot.Delete();
                    if (deleteResult.IsError)
                    {
                        Log.Information("Failed to delete root taxon for Taxonomy Id {TaxonomyId}: {Errors}", notification.TaxonomyId, deleteResult.Errors);
                    }
                    applicationDbContext.Set<Taxon>()
                        .Remove(entity: taxonomyRoot);
                    await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                }
            }
        }
    }
}