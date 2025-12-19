namespace ReSys.Shop.Core.Common.Constants;

public static class Schema
{
    public const string Default = "eshopdb";

    public const string OutboxMessages = "outbox_messages";
    public const string AuditLogs = "audit_logs";
    public const string Settings = "settings";

    public const string Users = "users";
    public const string Roles = "roles";
    public const string UserRoles = "user_roles";
    public const string RoleClaims = "role_claims";
    public const string UserClaims = "user_claims";
    public const string UserLogins = "user_logins";
    public const string UserTokens = "user_tokens";
    public const string RefreshTokens = "refresh_tokens";
    public const string AccessPermissions = "permissions";

    public const string Addresses = "addresses";
    public const string Countries = "countries";
    public const string States = "states";

    public const string Customers = "customers";
    public const string UserAddresses = "customer_addresses";
    public const string CustomerWishlists = "customer_wishlists";
    public const string WishlistItems = "wishlist_items";
    public const string NewsletterSubscriptions = "newsletter_subscriptions";

    public const string Taxonomies = "taxonomies";
    public const string Taxons = "taxa";
    public const string Classifications = "classification";
    public const string TaxonRules = "taxon_rules";
    public const string TaxonImages = "taxon_images";

    public static string TranslationFor(string tableName)
        => $"{tableName}_translations";

    public const string Products = "products";
    public const string Tags = "tags";
    public const string Reviews = "reviews";
    public const string ProductImages = "product_images";

    public const string PropertyTypes = "property_types";
    public const string ProductPropertyTypes = "product_property_types";

    public const string OptionTypes = "option_types";
    public const string OptionValues = "option_values";
    public const string ProductOptionTypes = "product_option_types";
    public const string ProductTags = "product_tags";

    public const string Variants = "variants";
    public const string VariantOptionValues = "variant_option_values";
    public const string Prices = "prices";

    public const string StockLocations = "stock_locations";
    public const string StockItems = "stock_items";
    public const string StockMovements = "stock_movements";
    public const string InventoryUnits = "inventory_units";
    public const string StockTransfers = "stock_transfers";

    public const string Orders = "orders";
    public const string LineItems = "line_items";
    public const string LineItemAdjustments = "line_item_adjustments";
    public const string OrderAdjustments = "order_adjustments";
    public const string OrderPromotions = "order_promotions";
    public const string Adjustments = "adjustments";

    public const string Shipments = "shipments";
    public const string ShippingMethods = "shipping_methods";
    public const string ShippingRates = "shipping_rates";
    public const string ShippingCategories = "shipping_categories";

    public const string Payments = "payments";
    public const string PaymentMethods = "payment_methods";
    public const string PaymentSources = "payment_sources";
    public const string Refunds = "refunds";

    public const string ProductFeatures = "product_features";
    public const string ProductColorPalettes = "product_color_palettes";
    public const string VisualSimilarityScores = "visual_similarity_scores";
    public const string StyleEmbeddings = "style_embeddings";

    public const string UserInteractions = "user_interactions";
    public const string UserPreferences = "user_preferences";
    public const string SimilarProducts = "similar_products";
    public const string RecommendationSets = "recommendation_sets";
    public const string RecommendationLogs = "recommendation_logs";

    public const string UserSessions = "user_sessions";
    public const string ClickstreamEvents = "clickstream_events";
    public const string SearchQueries = "search_queries";
    public const string AbandonedCarts = "abandoned_carts";

    public const string Promotions = "promotions";
    public const string PromotionUsages = "promotion_usages";
    public const string PromotionRules = "promotion_rules";
    public const string PromotionActions = "promotion_actions";
    public const string PromotionCategories = "promotion_categories";
    public const string PromotionRuleTaxons = "promotion_rule_taxons";
    public const string PromotionRuleRoles = "promotion_rule_roles";
    public const string PromotionRuleUsers = "promotion_rule_users";

    public const string EmailCampaigns = "email_campaigns";
    public const string CustomerSegments = "customer_segments";
    public const string LoyaltyPrograms = "loyalty_programs";
    public const string LoyaltyPoints = "loyalty_points";

    public const string ProductReviews = "product_reviews";
    public const string ReviewImages = "review_images";
    public const string ReviewHelpfulness = "review_helpfulness";
    public const string ReviewModerationQueue = "review_moderation_queue";

    public const string UserFollows = "user_follows";
    public const string ProductShares = "product_shares";
    public const string OutfitPosts = "outfit_posts";

    public const string SalesReports = "sales_reports";
    public const string InventoryReports = "inventory_reports";
    public const string CustomerAnalytics = "customer_analytics";
    public const string ProductPerformance = "product_performance";

    public const string ModelPerformanceMetrics = "model_performance_metrics";
    public const string RecommendationAccuracy = "recommendation_accuracy";
    public const string SearchAnalytics = "search_analytics";
    public const string ConversionTracking = "conversion_tracking";

    public const string InventoryAlerts = "inventory_alerts";
    public const string FulfillmentMetrics = "fulfillment_metrics";
    public const string CustomerServiceTickets = "customer_service_tickets";
    public const string ReturnReasons = "return_reasons";

    public const string TodoItems = "todo_items";
    public const string TodoLists = "todo_lists";
}
