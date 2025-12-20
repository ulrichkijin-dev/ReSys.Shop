using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Taxons;

public static partial class TaxonModule
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
                RuleFor(expression: x => x.Request).SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper,
            Services.IHierarchy hierarchyService)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var param = command.Request;
                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var taxonomy = await applicationDbContext.Set<Taxonomy>()
                    .FindAsync(keyValues: [param.TaxonomyId], cancellationToken: ct);
                if (taxonomy == null)
                    return Taxonomy.Errors.NotFound(id: param.TaxonomyId);

                if (param.ParentId.HasValue)
                {
                    var parentTaxon = await applicationDbContext.Set<Taxon>()
                        .FindAsync(keyValues: [param.ParentId.Value], cancellationToken: ct);
                    if (parentTaxon == null)
                        return Taxon.Errors.NotFound(id: param.ParentId.Value);
                    if (parentTaxon.TaxonomyId != param.TaxonomyId)
                        return Taxon.Errors.ParentTaxonomyMismatch;
                }

                var uniqueNameCheck = await applicationDbContext.Set<Taxon>()
                    .CheckNameIsUniqueAsync<Taxon, Guid>(name: param.Name, prefix: nameof(Taxon), cancellationToken: ct);
                if (uniqueNameCheck.IsError)
                    return uniqueNameCheck.Errors;

                var createResult = Taxon.Create(
                    taxonomyId: param.TaxonomyId,
                    name: param.Name,
                    parentId: param.ParentId,
                    presentation: param.Presentation,
                    description: param.Description,
                    position: param.Position,
                    hideFromNav: param.HideFromNav,
                    automatic: param.Automatic,
                    rulesMatchPolicy: param.RulesMatchPolicy,
                    sortOrder: param.SortOrder,
                    metaTitle: param.MetaTitle,
                    metaDescription: param.MetaDescription,
                    metaKeywords: param.MetaKeywords,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: param.PrivateMetadata);

                if (createResult.IsError) return createResult.Errors;

                applicationDbContext.Set<Taxon>().Add(entity: createResult.Value);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);

                // Rebuild hierarchy after adding new taxon
                var buildResult = await hierarchyService.RebuildAsync(taxonomyId: createResult.Value.TaxonomyId, cancellationToken: ct);
                if (buildResult.IsError) return buildResult.Errors;

                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);
                return mapper.Map<Result>(source: createResult.Value);
            }
        }
    }
}