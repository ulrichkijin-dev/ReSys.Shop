using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReSys.Shop.Core.Common.Domain.Concerns;

public interface IHasAuditable : IHasCreator, IHasUpdater { }

public static class HasAuditable
{
    public static void AddAuditableRules<T>(this AbstractValidator<T> validator) where T : IHasAuditable
    {
        validator.AddCreatorRules();
        validator.AddUpdaterRules();
    }

    public static void ConfigureAuditable<T>(this EntityTypeBuilder<T> builder) where T : class, IHasAuditable
    {
        builder.ConfigureCreator();
        builder.ConfigureUpdater();
    }
}