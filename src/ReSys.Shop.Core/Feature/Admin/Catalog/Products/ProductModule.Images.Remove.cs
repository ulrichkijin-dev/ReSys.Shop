using Microsoft.Extensions.Logging;

using ReSys.Shop.Core.Common.Services.Storage.Services;
using ReSys.Shop.Core.Domain.Catalog.Products.Images;

namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static partial class Images
    {
        public static class Remove
        {
            public sealed record Command(Guid ProductId, Guid ImageId) : ICommand<Deleted>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(expression: x => x.ProductId).NotEmpty();
                    RuleFor(expression: x => x.ImageId).NotEmpty();
                }
            }

            public sealed class CommandHandler(
                IApplicationDbContext applicationDbContext,
                IStorageService storageService,
                ILogger<CommandHandler> logger)
                : ICommandHandler<Command, Deleted>
            {
                public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
                {
                    await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                    try
                    {
                        var image = await applicationDbContext.Set<ProductImage>()
                            .FirstOrDefaultAsync(
                                predicate: i => i.Id == command.ImageId
                                                && i.ProductId == command.ProductId
                                                && i.VariantId == null,
                                cancellationToken: ct);

                        if (image == null)
                            return Error.NotFound(
                                code: "ProductImage.NotFound",
                                description: "Image not found.");

                        var urlToDelete = image.Url;

                        applicationDbContext.Set<ProductImage>().Remove(entity: image);
                        await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                        await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                        if (!string.IsNullOrEmpty(value: urlToDelete))
                        {
                            var del = await storageService.DeleteFileAsync(fileUrl: urlToDelete, cancellationToken: ct);
                            if (del.IsError)
                                logger.LogWarning(
                                    message: "Failed to delete product image file: {Url}",
                                    args: urlToDelete);
                        }

                        return Result.Deleted;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(exception: ex,
                            message: "Failed to remove product image {ImageId}",
                            args: command.ImageId);
                        await applicationDbContext.RollbackTransactionAsync(cancellationToken: ct);
                        return Error.Failure(
                            code: "ProductImage.RemoveFailed",
                            description: "Failed to remove image.");
                    }
                }
            }
        }
    }
}