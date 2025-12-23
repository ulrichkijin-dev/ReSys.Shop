using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace ReSys.Shop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductImageEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "embedding_fclip_model",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_fclip_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_efficientnet_model",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_efficientnet_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_dino_model",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_dino_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Vector>(
                name: "embedding_dino",
                schema: "eshopdb",
                table: "product_images",
                type: "vector(384)",
                nullable: true,
                comment: "EmbeddingDino: Embedding vector for DINOv2 ViT-S/14 (384-dim).",
                oldClrType: typeof(Vector),
                oldType: "vector(384)",
                oldNullable: true,
                oldComment: "EmbeddingDino: Embedding vector for DINO ViT-S/16 (384-dim).");

            migrationBuilder.AlterColumn<string>(
                name: "embedding_convnext_model",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_convnext_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "embedding_clip",
                schema: "eshopdb",
                table: "product_images",
                type: "vector(512)",
                nullable: true,
                comment: "EmbeddingClip: Embedding vector for CLIP ViT-B/16 (512-dim).");

            migrationBuilder.AddColumn<string>(
                name: "embedding_clip_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "embedding_clip_generated_at",
                schema: "eshopdb",
                table: "product_images",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "embedding_clip_model",
                schema: "eshopdb",
                table: "product_images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_images_embedding_clip_hnsw",
                schema: "eshopdb",
                table: "product_images",
                column: "embedding_clip")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_product_images_embedding_clip_hnsw",
                schema: "eshopdb",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "embedding_clip",
                schema: "eshopdb",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "embedding_clip_checksum",
                schema: "eshopdb",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "embedding_clip_generated_at",
                schema: "eshopdb",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "embedding_clip_model",
                schema: "eshopdb",
                table: "product_images");

            migrationBuilder.AlterColumn<string>(
                name: "embedding_fclip_model",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_fclip_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_efficientnet_model",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_efficientnet_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_dino_model",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_dino_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<Vector>(
                name: "embedding_dino",
                schema: "eshopdb",
                table: "product_images",
                type: "vector(384)",
                nullable: true,
                comment: "EmbeddingDino: Embedding vector for DINO ViT-S/16 (384-dim).",
                oldClrType: typeof(Vector),
                oldType: "vector(384)",
                oldNullable: true,
                oldComment: "EmbeddingDino: Embedding vector for DINOv2 ViT-S/14 (384-dim).");

            migrationBuilder.AlterColumn<string>(
                name: "embedding_convnext_model",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "embedding_convnext_checksum",
                schema: "eshopdb",
                table: "product_images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
