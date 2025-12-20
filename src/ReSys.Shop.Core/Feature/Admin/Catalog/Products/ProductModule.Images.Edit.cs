using MapsterMapper;

using Microsoft.Extensions.Logging;

using ReSys.Shop.Core.Common.Services.Storage.Models;
using ReSys.Shop.Core.Common.Services.Storage.Services;
using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Images;

namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static partial class Images
    {
        public static class Edit
        {
            public sealed class Request : Models.UploadImageParameter;
            public sealed class Result : Models.ImageResult;

            public sealed record Command(
                Guid ProductId,
                Guid ImageId,
                Request Request
            ) : ICommand<Result>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.ProductId).NotEmpty();
                    RuleFor(x => x.ImageId).NotEmpty();
                    RuleFor(x => x.Request)
                        .SetValidator(new Models.UploadImageParameterValidator());
                }
            }

            public sealed class CommandHandler(
                IApplicationDbContext applicationDbContext,
                IMapper mapper,
                IStorageService storageService,
                ILogger<CommandHandler> logger
            ) : ICommandHandler<Command, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
                {
                    await applicationDbContext.BeginTransactionAsync(ct);

                    string? oldUrl = null;

                    try
                    {
                        if (!Enum.TryParse<ProductImage.ProductImageType>(
                                command.Request.Type,
                                ignoreCase: true,
                                out var imageType))
                        {
                            await applicationDbContext.RollbackTransactionAsync(ct);
                            return Error.Validation("Image.Type.Invalid", "Invalid image type.");
                        }

                        var product = await applicationDbContext.Set<Product>()
                            .Include(p => p.Variants)
                            .FirstOrDefaultAsync(p => p.Id == command.ProductId, ct);

                        if (product == null)
                        {
                            await applicationDbContext.RollbackTransactionAsync(ct);
                            return Product.Errors.NotFound(command.ProductId);
                        }

                        var image = await applicationDbContext.Set<ProductImage>()
                            .Include(i => i.Variant)
                            .FirstOrDefaultAsync(
                                i => i.Id == command.ImageId
                                     && i.ProductId == command.ProductId
                                     && (command.Request.VariantId == null ||
                                         i.VariantId == command.Request.VariantId),
                                ct);

                        if (image == null)
                        {
                            await applicationDbContext.RollbackTransactionAsync(ct);
                            return ProductImage.Errors.NotFound(command.ImageId);
                        }

                        if (command.Request.File != null)
                        {
                            var uploadOptions = UploadOptions.FromDomainSpec(
                                type: imageType,
                                productId: command.ProductId,
                                variantId: command.Request.VariantId,
                                contentType: command.Request.File.ContentType);

                            var uploadResult = await storageService.UploadFileAsync(
                                command.Request.File,
                                uploadOptions,
                                ct);

                            if (uploadResult.IsError)
                            {
                                await applicationDbContext.RollbackTransactionAsync(ct);
                                return uploadResult.Errors;
                            }

                            oldUrl = image.Url;

                            var updateResult = image.Update(
                                variantId: command.Request.VariantId,
                                url: uploadResult.Value.Url,
                                alt: command.Request.Alt ?? command.Request.File.FileName,
                                position: command.Request.Position,
                                type: command.Request.Type,
                                contentType: uploadResult.Value.ContentType,
                                width: uploadResult.Value.Width,
                                height: uploadResult.Value.Height);

                            if (updateResult.IsError)
                            {
                                await storageService.DeleteFileAsync(uploadResult.Value.Url, ct);
                                await applicationDbContext.RollbackTransactionAsync(ct);
                                return updateResult.Errors;
                            }
                        }
                        else
                        {
                            var updateResult = image.Update(
                                alt: command.Request.Alt,
                                position: command.Request.Position,
                                type: command.Request.Type);

                            if (updateResult.IsError)
                            {
                                await applicationDbContext.RollbackTransactionAsync(ct);
                                return updateResult.Errors;
                            }

                            updateResult = product.AddImage(image);

                            if (updateResult.IsError)
                            {
                                await applicationDbContext.RollbackTransactionAsync(ct);
                                return updateResult.Errors;
                            }
                        }
                        applicationDbContext.Set<ProductImage>().Update(image);
                        await applicationDbContext.SaveChangesAsync(ct);

                        await applicationDbContext.CommitTransactionAsync(ct);

                        if (!string.IsNullOrWhiteSpace(oldUrl))
                        {
                            var deleteOld = await storageService.DeleteFileAsync(oldUrl, ct);
                            if (deleteOld.IsError)
                            {
                                logger.LogWarning(
                                    "Failed to delete old product image file: {Url}",
                                    oldUrl);
                            }
                        }

                        return mapper.Map<Result>(image);
                    }
                    catch (Exception ex)
                    {
                        await applicationDbContext.RollbackTransactionAsync(ct);

                        logger.LogError(
                            ex,
                            "Failed to edit product image {ImageId}",
                            command.ImageId);

                        return Error.Failure(
                            code: "ProductImage.EditFailed",
                            description: "Failed to edit product image.");
                    }
                }

            }
        }
    }
}
