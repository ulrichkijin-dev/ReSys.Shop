namespace ReSys.Shop.Core.Domain.Settings.Stores;

public static class DefaultSettings
{
    public static IReadOnlyCollection<Setting> All =>
    [
        // ────────────── Store ──────────────
        Create(
            key: SettingKey.Store(key: StoreSettingKey.Name),
            value: "My Store",
            description: "Public store name",
            defaultValue: "My Store",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Store(key: StoreSettingKey.Code),
            value: "default",
            description: "Internal store identifier",
            defaultValue: "default",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Store(key: StoreSettingKey.DefaultCurrency),
            value: "USD",
            description: "Default store currency",
            defaultValue: "USD",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Store(key: StoreSettingKey.SupportedCurrencies),
            value: "USD",
            description: "Supported currencies (CSV)",
            defaultValue: "USD",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Store(key: StoreSettingKey.DefaultLocale),
            value: "en",
            description: "Default locale",
            defaultValue: "en",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Store(key: StoreSettingKey.SupportedLocales),
            value: "en",
            description: "Supported locales (CSV)",
            defaultValue: "en",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Store(key: StoreSettingKey.MailFromAddress),
            value: "noreply@example.com",
            description: "Outgoing email sender",
            defaultValue: "noreply@example.com",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Store(key: StoreSettingKey.SupportEmail),
            value: "support@example.com",
            description: "Customer support email",
            defaultValue: "support@example.com",
            type: ConfigurationValueType.String),

        // ────────────── SEO ──────────────
        Create(
            key: SettingKey.Seo(key: SeoSettingKey.Title),
            value: "My Store",
            description: "SEO title",
            defaultValue: "My Store",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Seo(key: SeoSettingKey.MetaDescription),
            value: "Online store",
            description: "SEO meta description",
            defaultValue: "Online store",
            type: ConfigurationValueType.String),

        Create(
            key: SettingKey.Seo(key: SeoSettingKey.MetaKeywords),
            value: "",
            description: "SEO meta keywords",
            defaultValue: "",
            type: ConfigurationValueType.String),

        // ────────────── Checkout ──────────────
        Create(
            key: SettingKey.Checkout(key: CheckoutSettingKey.ZoneId),
            value: "",
            description: "Checkout zone identifier",
            defaultValue: "",
            type: ConfigurationValueType.Guid),

        Create(
            key: SettingKey.Checkout(key: CheckoutSettingKey.AllowedCountries),
            value: "",
            description: "Allowed countries (CSV)",
            defaultValue: "",
            type: ConfigurationValueType.String),

        // ────────────── Email ──────────────
        Create(
            key: SettingKey.Email(key: EmailSettingKey.NewOrderNotificationEmail),
            value: "orders@example.com",
            description: "New order notification email",
            defaultValue: "orders@example.com",
            type: ConfigurationValueType.String)
    ];

    private static Setting Create(
        string key,
        string value,
        string description,
        string defaultValue,
        ConfigurationValueType type)
    {
        return Setting.Create(
            key: key,
            value: value,
            description: description,
            defaultValue: defaultValue,
            valueType: type
        ).Value;
    }
}