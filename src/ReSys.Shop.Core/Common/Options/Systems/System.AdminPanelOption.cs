namespace ReSys.Shop.Core.Common.Options.Systems;

public sealed class AdminPanelOption : SystemOptionBase
{
    public const string Section = "AdminPanel";

    public AdminPanelOption()
    {
        SystemName = "ReSys.AdminPanel";
        DefaultPage = "/dashboard";
    }
}