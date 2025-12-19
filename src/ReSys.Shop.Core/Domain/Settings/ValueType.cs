namespace ReSys.Shop.Core.Domain.Settings;

/// <summary>
/// Defines the possible data types for a configuration setting's value.
/// This enum helps in validating and correctly parsing configuration values
/// at the application layer.
/// </summary>
public enum ConfigurationValueType
{
    String,
    Boolean,
    Integer,
    Guid,
}