using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasMetadata
{
    IDictionary<string, object?>? PublicMetadata { get; set; }

    IDictionary<string, object?>? PrivateMetadata { get; set; }
}

public static class HasMetadata
{
    public static object? GetPublic(this IHasMetadata? holder, string key)
        => holder?.PublicMetadata?.TryGetValue(key: key, value: out var v) == true ? v : null;

    public static T? GetPublic<T>(this IHasMetadata? holder, string key, T? defaultValue = default)
    {
        var value = holder?.PublicMetadata?.TryGetValue(key: key, value: out var v) == true ? v : null;
        return value is T typedValue ? typedValue : defaultValue;
    }

    public static IHasMetadata? SetPublic(this IHasMetadata? holder, string key, object? value)
    {
        if (holder == null) return holder;
        holder.PublicMetadata ??= new Dictionary<string, object?>();
        if (value == null)
            holder.PublicMetadata.Remove(key: key);
        else
            holder.PublicMetadata[key: key] = value;
        return holder;
    }

    public static object? GetPrivate(this IHasMetadata? holder, string key)
        => holder?.PrivateMetadata?.TryGetValue(key: key, value: out var v) == true ? v : null;

    public static T? GetPrivate<T>(this IHasMetadata? holder, string key, T? defaultValue = default)
    {
        var value = holder?.PrivateMetadata?.TryGetValue(key: key, value: out var v) == true ? v : null;
        return value is T typedValue ? typedValue : defaultValue;
    }

    public static IHasMetadata? SetPrivate(this IHasMetadata? holder, string key, object? value)
    {
        if (holder == null) return holder;
        holder.PrivateMetadata ??= new Dictionary<string, object?>();
        if (value == null)
            holder.PrivateMetadata.Remove(key: key);
        else
            holder.PrivateMetadata[key: key] = value;
        return holder;
    }

    public static bool MetadataEquals(
        this IDictionary<string, object?>? dict1,
        IDictionary<string, object?>? dict2,
        JsonSerializerOptions? options = null)
    {
        if ((dict1 == null || dict1.Count == 0) && (dict2 == null || dict2.Count == 0)) return true;
        if (dict1 == null || dict2 == null) return false;
        if (dict1.Count != dict2.Count) return false;

        options ??= new JsonSerializerOptions { WriteIndented = false };
        var json1 = JsonSerializer.Serialize(value: dict1, options: options);
        var json2 = JsonSerializer.Serialize(value: dict2, options: options);
        return json1 == json2;
    }

    public static void AddMetadataSupportRules<T>(
        this AbstractValidator<T> validator,
        string? prefix = null,
        string? customMessage = null,
        int? maxEntries = null,
        int? keyMinLength = null,
        int? keyMaxLength = null,
        int? valueMaxLength = null,
        Regex? keyAllowedRegex = null)
        where T : class, IHasMetadata
    {
        string codePrefix = string.IsNullOrWhiteSpace(value: prefix) ? typeof(T).Name : prefix;
        Debug.Assert(condition: validator != null, message: $"{nameof(validator)} != null");

        validator.RuleFor(expression: m => m.PublicMetadata)
            .MustBeValidDictionary(prefix: codePrefix, customMessage: customMessage,
                maxEntries: maxEntries, keyMinLength: keyMinLength, keyMaxLength: keyMaxLength,
                valueMaxLength: valueMaxLength, keyAllowedRegex: keyAllowedRegex);

        validator.RuleFor(expression: m => m.PrivateMetadata)
            .MustBeValidDictionary(prefix: codePrefix, customMessage: customMessage,
                maxEntries: maxEntries, keyMinLength: keyMinLength, keyMaxLength: keyMaxLength,
                valueMaxLength: valueMaxLength, keyAllowedRegex: keyAllowedRegex);
    }

    public static void ConfigureMetadata<T>(this EntityTypeBuilder<T> builder)
        where T : class, IHasMetadata
    {
        builder.Property(propertyExpression: x => x.PublicMetadata)
            .ConfigureDictionary(isRequired: false)
            .HasComment(comment: "Public key-value metadata for the entity, stored as JSON.");

        builder.Property(propertyExpression: x => x.PrivateMetadata)
            .ConfigureDictionary(isRequired: false)
            .HasComment(comment: "Private key-value metadata for the entity, stored as JSON.");
    }
}