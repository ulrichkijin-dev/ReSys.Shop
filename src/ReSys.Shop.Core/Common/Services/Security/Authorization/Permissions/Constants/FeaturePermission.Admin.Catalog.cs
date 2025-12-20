using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

public static partial class FeaturePermission
{
    public static partial class Admin
    {
        public static partial class Catalog
        {
            public static AccessPermission[] All =>
            [
                ..Property.All,
                ..Taxonomy.All,
                ..Taxon.All,
                ..OptionType.All,
                ..OptionValue.All,
                ..Product.All, 
                ..Variant.All, 
                ..Promotion.All,
            ];
        }
    }
}