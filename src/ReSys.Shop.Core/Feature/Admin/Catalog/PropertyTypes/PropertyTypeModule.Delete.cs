using ReSys.Shop.Core.Domain.Catalog.PropertyTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.PropertyTypes;

public static partial class PropertyTypeModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                var idRequired = CommonInput.Errors.Required(prefix: nameof(PropertyType), nameof(PropertyType.Id));
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
                var property = await applicationDbContext.Set<PropertyType>()
                    .Include(navigationPropertyPath: p => p.ProductPropertyTypes)
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: cancellationToken);

                // Check: existence
                if (property == null)
                    return PropertyType.Errors.NotFound(id: command.Id);

                // Check: deletable
                var deleteResult = property.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);
                applicationDbContext.Set<PropertyType>().Remove(entity: property);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return Result.Deleted;
            }
        }
    }
}