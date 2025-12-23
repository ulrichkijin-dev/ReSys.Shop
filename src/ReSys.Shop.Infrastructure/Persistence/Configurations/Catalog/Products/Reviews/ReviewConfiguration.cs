using ReSys.Shop.Core.Common.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Shop.Core.Domain.Catalog.Products.Reviews;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Catalog.Products.Reviews;

/// <summary>
/// Configures the database mapping for the <see cref="Review"/> entity.
/// </summary>
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    /// <summary>
    /// Configures the entity of type <see cref="Review"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable(name: Schema.Reviews);

        builder.HasKey(keyExpression: r => r.Id);

        builder.Property(propertyExpression: r => r.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the review. Value generated never.");

        builder.Property(propertyExpression: r => r.ProductId)
            .IsRequired()
            .HasComment(comment: "ProductId: Foreign key to the associated Product.");

        builder.Property(propertyExpression: r => r.UserId)
            .IsRequired()
            .HasComment(comment: "UserId: Foreign key to the associated ApplicationUser.");

        builder.Property(propertyExpression: r => r.Rating)
            .IsRequired()
            .HasComment(comment: "Rating: The star rating given by the user (e.g., 1-5).");

        builder.Property(propertyExpression: r => r.Title)
            .HasMaxLength(maxLength: Review.Constraints.TitleMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Title: A short title for the review.");

        builder.Property(propertyExpression: r => r.Comment)
            .HasMaxLength(maxLength: Review.Constraints.CommentMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Comment: The detailed review text provided by the user.");

        builder.Property(propertyExpression: r => r.Status)
            .IsRequired()
            .HasConversion<string>() // Store enum as string
            .HasComment(comment: "Status: The current moderation status of the review (e.g., Pending, Approved, Rejected).");

        builder.HasOne(navigationExpression: r => r.Product)
            .WithMany(navigationExpression: p => p.Reviews)
            .HasForeignKey(foreignKeyExpression: r => r.ProductId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(foreignKeyExpression: r => r.UserId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        builder.HasIndex(indexExpression: r => r.ProductId);
        builder.HasIndex(indexExpression: r => r.UserId);
        builder.HasIndex(indexExpression: r => r.Status);
    }
}