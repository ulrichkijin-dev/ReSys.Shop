using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Taxons;

public static partial class TaxonModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id).NotEmpty();
            }
        }

        public sealed class CommandHandler(IApplicationDbContext applicationDbContext, Services.IHierarchy hierarchyService)
            : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
            {
                var taxon = await applicationDbContext.Set<Taxon>()
                    .Include(navigationPropertyPath: t => t.Children)
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: ct);

                if (taxon == null)
                    return Taxon.Errors.NotFound(id: command.Id);

                var deleteResult = taxon.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);
                applicationDbContext.Set<Taxon>().Remove(entity: taxon);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);

                // Rebuild hierarchy after deletion
                var buildResult = await hierarchyService.RebuildAsync(taxonomyId: taxon.TaxonomyId, cancellationToken: ct);
                if (buildResult.IsError) return buildResult.Errors;

                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);
                return Result.Deleted;
            }
        }
    }
}