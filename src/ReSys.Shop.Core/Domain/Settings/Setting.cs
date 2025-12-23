using ReSys.Shop.Core.Common.Domain.Concerns;

namespace ReSys.Shop.Core.Domain.Settings;

/// <summary>
/// Represents a dynamic application configuration entry stored as a key-value pair.
/// This allows for flexible, runtime-adjustable settings without requiring code changes or redeployments.
/// In a single-store application context, these configurations effectively define the store's settings.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Role in Application:</strong>
/// This aggregate enables external management of application settings, serving as the central repository
/// for configurable parameters that define the behavior and characteristics of the single storefront.
/// <list type="bullet">
/// <item>
/// <term>Dynamic Settings</term>
/// <description>Adjust behaviors, feature flags, or thresholds during runtime, including all previously store-specific configurations.</description>
/// </item>
/// <item>
/// <term>Centralized Control</term>
/// <description>Manage all configurable parameters from a single source (e.g., an admin panel), consolidating what were formerly 'Store' entity properties.</description>
/// </item>
/// <item>
/// <term>Typed Values</term>
/// <description>Supports various data types (string, int, bool, etc.) ensuring data integrity for all configurations.</description>
/// </item>
/// </list>
/// </para>
///
/// <para>
/// <strong>Key Fields:</strong>
/// <list type="bullet">
/// <item>
/// <term>Key</term>
/// <description>A unique identifier for the configuration setting (e.g., "Store:DefaultCurrency", "Store:MailFromAddress", "Store:Address1").</description>
/// </item>
/// <item>
/// <term>Value</term>
/// <description>The current value of the configuration setting.</description>
/// </item>
/// <item>
/// <term>DefaultValue</term>
/// <description>The fallback value if the primary value is not set or invalid.</description>
/// </item>
/// <item>
/// <term>ValueType</term>
/// <description>The expected data type of the configuration's value, for validation and parsing.</description>
/// </item>
/// </list>
/// </para>
/// </remarks>
public sealed class Setting : Aggregate<Guid>, IHasMetadata
{
    /// <summary>
    /// Gets the unique key identifying this configuration setting.
    /// This key is used to retrieve the configuration value.
    /// </summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>
    /// Gets the current value of the configuration setting.
    /// This value can be updated dynamically.
    /// </summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>
    /// Gets a descriptive explanation of what this configuration setting controls.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets the default or fallback value for this configuration setting.
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
    /// <summary>
    /// Gets the expected data type of the configuration's value.
    /// Used for validation and proper casting/parsing at the application layer.
    /// </summary>
    public ConfigurationValueType ValueType { get; set; }

    public IDictionary<string, object?>? PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?>? PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    /// Private constructor for ORM (Entity Framework Core) materialization.
    /// </summary>
    private Setting() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Setting"/> class.
    /// This constructor is primarily used internally by the static <see cref="Create"/> factory method.
    /// </summary>
    private Setting(
        Guid id,
        string key,
        string value,
        string description,
        string defaultValue,
        ConfigurationValueType valueType,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
        : base(id: id)
    {
        Key = key;
        Value = value;
        Description = description;
        DefaultValue = defaultValue;
        ValueType = valueType;
        PublicMetadata = publicMetadata ?? new Dictionary<string, object?>();
        PrivateMetadata = privateMetadata ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Factory method to create a new <see cref="Setting"/> instance.
    /// </summary>
    public static ErrorOr<Setting> Create(
        string key,
        string value,
        string description,
        string defaultValue,
        ConfigurationValueType valueType,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
    {
        if (string.IsNullOrWhiteSpace(value: key))
        {
            return Errors.KeyRequired;
        }

        return new Setting(
            id: Guid.NewGuid(),
            key: key,
            value: value,
            description: description,
            defaultValue: defaultValue,
            valueType: valueType,
            publicMetadata: publicMetadata,
            privateMetadata: privateMetadata);
    }

    /// <summary>
    /// Updates the configuration setting.
    /// </summary>
    public ErrorOr<Updated> Update(
        string? newValue = null,
        string? description = null,
        string? defaultValue = null,
        ConfigurationValueType? valueType = null,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
    {
        bool changed = false;

        if (newValue != null && newValue != Value)
        {
            Value = newValue;
            changed = true;
        }

        if (description != null && description != Description)
        {
            Description = description;
            changed = true;
        }

        if (defaultValue != null && defaultValue != DefaultValue)
        {
            DefaultValue = defaultValue;
            changed = true;
        }

        if (valueType.HasValue && valueType.Value != ValueType)
        {
            ValueType = valueType.Value;
            changed = true;
        }

        if (publicMetadata != null && !PublicMetadata.MetadataEquals(dict2: publicMetadata))
        {
            PublicMetadata = new Dictionary<string, object?>(dictionary: publicMetadata);
            changed = true;
        }

        if (privateMetadata != null && !PrivateMetadata.MetadataEquals(dict2: privateMetadata))
        {
            PrivateMetadata = new Dictionary<string, object?>(dictionary: privateMetadata);
            changed = true;
        }

        if (changed)
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        return Result.Updated;
    }

    /// <summary>
    /// Defines constraints and limits for configuration keys and values.
    /// </summary>
    public static class Constraints
    {
        /// <summary>
        /// Maximum length for the configuration key.
        /// </summary>
        public const int KeyMaxLength = 100;
        /// <summary>
        /// Maximum length for the configuration value.
        /// </summary>
        public const int ValueMaxLength = 500;
        /// <summary>
        /// Maximum length for the configuration description.
        /// </summary>
        public const int DescriptionMaxLength = 500;
        /// <summary>
        /// Maximum length for the default value of the configuration.
        /// </summary>
        public const int DefaultValueMaxLength = 500;
    }

    /// <summary>
    /// Defines domain error scenarios specific to <see cref="Setting"/> operations.
    /// These errors are returned via the <see cref="ErrorOr"/> pattern for robust error handling.
    /// </summary>
    public static class Errors
    {
        /// <summary>
        /// Error indicating that the configuration key is missing or empty.
        /// </summary>
        public static Error KeyRequired => Error.Validation(
            code: "Configuration.KeyRequired",
            description: "Configuration key is required.");

        /// <summary>
        /// Error indicating that the configuration value is missing or empty during an update.
        /// </summary>
        public static Error ValueRequired => Error.Validation(
            code: "Configuration.ValueRequired",
            description: "Configuration value is required.");
            
        /// <summary>
        /// Error indicating that a requested configuration could not be found.
        /// </summary>
        public static Error NotFound => Error.NotFound(
            code: "Configuration.NotFound",
            description: "Configuration not found.");
    }
}
