using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Taxons;

public static partial class TaxonModule
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
            IMapper mapper,
            Services.IHierarchy hierarchyService)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var request = command.Request;
                var taxon = await applicationDbContext.Set<Taxon>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: ct);
                if (taxon == null)
                    return Taxon.Errors.NotFound(id: command.Id);

                if (request.TaxonomyId != taxon.TaxonomyId)
                {
                    var taxonomy = await applicationDbContext.Set<Taxonomy>()
                        .FindAsync(keyValues: [request.TaxonomyId], cancellationToken: ct);
                    if (taxonomy == null)
                        return Taxonomy.Errors.NotFound(id: request.TaxonomyId);
                }

                if (request.ParentId.HasValue && request.ParentId != taxon.ParentId)
                {
                    var parentTaxon = await applicationDbContext.Set<Taxon>()
                        .FindAsync(keyValues: [request.ParentId.Value], cancellationToken: ct);
                    if (parentTaxon == null)
                        return Taxon.Errors.NotFound(id: request.ParentId.Value);
                    if (parentTaxon.TaxonomyId != request.TaxonomyId)
                        return Taxon.Errors.ParentTaxonomyMismatch;
                }

                if (taxon.Name != request.Name)
                {
                    var uniqueNameCheck = await applicationDbContext.Set<Taxon>()
                        .Where(predicate: m => m.Id != taxon.Id)
                        .CheckNameIsUniqueAsync<Taxon, Guid>(name: request.Name, prefix: nameof(Taxon), cancellationToken: ct);
                    if (uniqueNameCheck.IsError)
                        return uniqueNameCheck.Errors;
                }

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var updateResult = taxon.Update(
                    name: request.Name,
                    presentation: request.Presentation,
                    parentId: request.ParentId,
                    description: request.Description,
                    position: request.Position,
                    hideFromNav: request.HideFromNav,
                    automatic: request.Automatic,
                    rulesMatchPolicy: request.RulesMatchPolicy,
                    sortOrder: request.SortOrder,
                    metaTitle: request.MetaTitle,
                    metaDescription: request.MetaDescription,
                    metaKeywords: request.MetaKeywords,
                    publicMetadata: request.PublicMetadata,
                    privateMetadata: request.PrivateMetadata);

                if (updateResult.IsError) return updateResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);

                // Rebuild hierarchy if parent changed
                if (request.ParentId != taxon.ParentId)
                {
                    var buildResult = await hierarchyService.RebuildAsync(taxonomyId: taxon.TaxonomyId, cancellationToken: ct);
                    if (buildResult.IsError) return buildResult.Errors;
                }
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);
                return mapper.Map<Result>(source: updateResult.Value);
            }
        }
    }
}