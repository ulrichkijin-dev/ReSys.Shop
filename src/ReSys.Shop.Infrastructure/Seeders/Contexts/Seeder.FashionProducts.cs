using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Infrastructure.Persistence.Contexts;
using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Images;
using ReSys.Shop.Core.Domain.Inventories.Locations;
using ReSys.Shop.Core.Domain.Inventories.Movements;

using Serilog;
using System.Text.Json;

namespace ReSys.Shop.Infrastructure.Seeders.Contexts;

public sealed class FashionProductDataSeeder(IServiceProvider serviceProvider) : IDataSeeder
{
    private readonly ILogger _logger = Log.ForContext<FashionProductDataSeeder>();

    public int Order => 50;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _logger.Information("Starting Fashion Product Dataset seeding...");

        var jsonPath = Environment.GetEnvironmentVariable("FASHION_JSON_PATH") ?? Path.Combine("data", "fashion", "styles.json");
        var maxItems = int.TryParse(Environment.GetEnvironmentVariable("FASHION_MAX"), out var m) ? m : 4000;
        var downloadImages = (Environment.GetEnvironmentVariable("FASHION_DOWNLOAD_IMAGES") ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
        var outputFolder = Path.Combine("data", "fashion", "processed");
        Directory.CreateDirectory(outputFolder);

        if (!File.Exists(jsonPath))
        {
            _logger.Warning("Fashion JSON file not found at {Path} - skipping fashion seeder", jsonPath);
            return;
        }

        // 1. Ensure Default Stock Location
        var stockLocation = await db.Set<StockLocation>().FirstOrDefaultAsync(l => l.Name == "default-warehouse", cancellationToken);
        if (stockLocation == null)
        {
            var locResult = StockLocation.Create(
                name: "default-warehouse",
                presentation: "Default Warehouse",
                active: true,
                isDefault: true,
                type: LocationType.Warehouse,
                shipEnabled: true
            );
            if (locResult.IsError)
            {
                _logger.Error("Failed to create default stock location: {Error}", locResult.FirstError.Description);
                return;
            }
            stockLocation = locResult.Value;
            await db.Set<StockLocation>().AddAsync(stockLocation, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            _logger.Information("Created default stock location: Default Warehouse");
        }

        var records = new List<JsonElement>();

        using (var fs = File.OpenRead(jsonPath))
        {
            using var doc = JsonDocument.Parse(fs);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                    records.Add(el);
            }
            else
            {
                // try newline-delimited JSON
                fs.Seek(0, SeekOrigin.Begin);
                using var sr = new StreamReader(fs);
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        var el = JsonDocument.Parse(line).RootElement;
                        records.Add(el);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        _logger.Information("Loaded {Count} records from JSON", records.Count);

        // Normalize to inner 'data' object if present
        var normalized = new List<JsonElement>();
        foreach (var r in records)
        {
            if (r.TryGetProperty("data", out var d)) normalized.Add(d);
            else normalized.Add(r);
        }

        // Filter to entries that have at least default or search image
        var filtered = normalized.Where(item =>
        {
            if (item.TryGetProperty("styleImages", out var si))
            {
                if (si.TryGetProperty("default", out var def) && def.TryGetProperty("imageURL", out var _)) return true;
                if (si.TryGetProperty("search", out var s) && s.TryGetProperty("imageURL", out var _)) return true;
            }
            return false;
        }).ToList();

        _logger.Information("{Count} records contain images and are eligible", filtered.Count);

        // Shuffle and take maxItems
        var rng = new Random(42);
        var sample = filtered.OrderBy(_ => rng.Next()).Take(maxItems).ToList();
        var totalSelected = sample.Count;
        var trainCount = (int)(totalSelected * 0.7);
        var valCount = (int)(totalSelected * 0.15);

        var outSearchPath = Path.Combine(outputFolder, "search_dataset.jsonl");
        var outAppPath = Path.Combine(outputFolder, "app_dataset.jsonl");
        await using var swSearch = File.CreateText(outSearchPath);
        await using var swApp = File.CreateText(outAppPath);

        int processed = 0;
        int saved = 0;
        int batch = 0;
        var http = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };
        var datasetImageDir = Path.Combine("data", "fashion", "images");
        if (downloadImages) Directory.CreateDirectory(datasetImageDir);

        foreach (var item in sample)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string splitType = "test";
            if (processed < trainCount) splitType = "train";
            else if (processed < trainCount + valCount) splitType = "val";

            processed++;

            try
            {
                var id = item.TryGetProperty("id", out var jId) ? jId.GetRawText().Trim('"') : Guid.NewGuid().ToString();
                var name = item.TryGetProperty("productDisplayName", out var pdn) ? pdn.GetString() ?? $"Product {id}" : $"Product {id}";
                var description = "";
                if (item.TryGetProperty("productDescriptors", out var pds) && pds.ValueKind == JsonValueKind.Object && pds.TryGetProperty("description", out var descEl))
                {
                    if (descEl.TryGetProperty("value", out var dv)) description = StripHtml(dv.GetString() ?? "");
                }

                var brand = item.TryGetProperty("brandName", out var b) ? b.GetString() : null;
                var baseColor = item.TryGetProperty("baseColour", out var bc) ? bc.GetString() : null;

                var masterCategory = item.TryGetProperty("masterCategory", out var mc) && mc.ValueKind == JsonValueKind.Object && mc.TryGetProperty("typeName", out var mct) ? mct.GetString() : null;
                var subCategory = item.TryGetProperty("subCategory", out var sc) && sc.ValueKind == JsonValueKind.Object && sc.TryGetProperty("typeName", out var sct) ? sct.GetString() : null;
                var articleType = item.TryGetProperty("articleType", out var at) && at.ValueKind == JsonValueKind.Object && at.TryGetProperty("typeName", out var att) ? att.GetString() : null;

                var defaultImage = item.TryGetProperty("styleImages", out var si2) && si2.TryGetProperty("default", out var def2) && def2.TryGetProperty("imageURL", out var defUrl) ? defUrl.GetString() : null;
                var searchImage = item.TryGetProperty("styleImages", out var si3) && si3.TryGetProperty("search", out var s2) && s2.TryGetProperty("imageURL", out var searchUrl) ? searchUrl.GetString() : null;

                // Create Product
                var productResult = Product.Create(name: name, description: description, publicMetadata: new Dictionary<string, object?>
                {
                    ["source"] = "myntra",
                    ["originalId"] = id,
                    ["brandName"] = brand,
                    ["baseColour"] = baseColor,
                    ["masterCategory"] = masterCategory,
                    ["subCategory"] = subCategory,
                    ["articleType"] = articleType,
                    ["split"] = splitType
                });

                if (productResult.IsError)
                {
                    _logger.Warning("Skipping product {Id} - failed to create product entity: {Error}", id, productResult.FirstError.Description);
                    continue;
                }

                var product = productResult.Value;
                product.Activate(); // Make active immediately

                // Add images (default + search if available)
                if (!string.IsNullOrWhiteSpace(defaultImage))
                {
                    var contentType = GuessContentType(defaultImage);
                    // Use 'Default' type for web app
                    var createRes = ProductImage.Create(url: defaultImage, productId: product.Id, alt: name, position: 1, type: nameof(ProductImage.ProductImageType.Default), contentType: contentType);
                    if (!createRes.IsError)
                    {
                        product.AddImage(createRes.Value);
                        
                        // write one line to app dataset
                        var obj = new Dictionary<string, object?>
                        {
                            ["id"] = id,
                            ["image_path"] = downloadImages ? SaveImageLocal(defaultImage, datasetImageDir, id, http).Result : defaultImage,
                            ["image_name"] = Path.GetFileName(defaultImage),
                            ["category"] = masterCategory,
                            ["subCategory"] = subCategory,
                            ["articleType"] = articleType,
                            ["base_color"] = baseColor,
                            ["description"] = description,
                            ["split"] = splitType
                        };
                        await swApp.WriteLineAsync(JsonSerializer.Serialize(obj));
                    }
                }

                if (!string.IsNullOrWhiteSpace(searchImage))
                {
                    var contentType = GuessContentType(searchImage);
                    // Use 'Search' type for embeddings/high-res
                    var createRes = ProductImage.Create(url: searchImage, productId: product.Id, alt: name, position: 2, type: nameof(ProductImage.ProductImageType.Search), contentType: contentType);
                    if (!createRes.IsError)
                    {
                        product.AddImage(createRes.Value);

                        var obj = new Dictionary<string, object?>
                        {
                            ["id"] = id,
                            ["image_path"] = downloadImages ? SaveImageLocal(searchImage, datasetImageDir, id + "_search", http).Result : searchImage,
                            ["image_name"] = Path.GetFileName(searchImage),
                            ["category"] = masterCategory,
                            ["subCategory"] = subCategory,
                            ["articleType"] = articleType,
                            ["base_color"] = baseColor,
                            ["description"] = description,
                            ["split"] = splitType
                        };
                        await swSearch.WriteLineAsync(JsonSerializer.Serialize(obj));
                    }
                }

                // Persist product first to generate ID for Master Variant
                await db.Set<Product>().AddAsync(product, cancellationToken);
                
                // Add Stock
                var masterVariant = product.GetMaster().Value;
                var restockResult = stockLocation.Restock(masterVariant, 100, StockMovement.MovementOriginator.Adjustment, null);
                if (restockResult.IsError)
                {
                     _logger.Warning("Failed to restock product {Id}: {Error}", id, restockResult.FirstError.Description);
                }

                batch++;
                saved++;

                if (batch >= 100)
                {
                    await db.SaveChangesAsync(cancellationToken);
                    batch = 0;
                    _logger.Information("Committed {Saved} products...", saved);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to process record #{Index}", processed);
            }
        }

        if (batch > 0) await db.SaveChangesAsync(cancellationToken);

        _logger.Information("Finished seeding {Saved}/{Processed} products. Output files: {App} {Search}", saved, processed, outAppPath, outSearchPath);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var noTags = Regex.Replace(html, "<.*?>", string.Empty);
        noTags = noTags.Replace("&nbsp;", " ");
        return noTags.Trim();
    }

    private static string GuessContentType(string url)
    {
        var ext = Path.GetExtension(url).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }

    private static async Task<string> SaveImageLocal(string url, string folder, string id, HttpClient http)
    {
        try
        {
            var resp = await http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return url;
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            var ext = Path.GetExtension(url).Split('?')[0];
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";
            var fileName = Path.Combine(folder, id + ext);
            await File.WriteAllBytesAsync(fileName, bytes);
            return fileName;
        }
        catch
        {
            return url;
        }
    }
}
