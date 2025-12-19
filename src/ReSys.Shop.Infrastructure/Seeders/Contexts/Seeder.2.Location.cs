using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Domain.Location.Countries;
using ReSys.Shop.Core.Domain.Location.States;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

using Serilog;

namespace ReSys.Shop.Infrastructure.Seeders.Contexts;

public sealed class LocationDataSeeder(IServiceProvider serviceProvider) : IDataSeeder
{
    private readonly ILogger _logger = Log.ForContext<LocationDataSeeder>();

    public int Order => 2;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _logger.Information(messageTemplate: "Starting location (countries + states/provinces) seeding...");

        await SeedVietnamAsync(db: dbContext, ct: cancellationToken);
        await SeedUnitedStatesAsync(db: dbContext, ct: cancellationToken);

        _logger.Information(messageTemplate: "Location seeding completed successfully.");
    }

    private async Task SeedVietnamAsync(ApplicationDbContext db, CancellationToken ct)
    {
        const string iso = "VN";
        const string iso3 = "VNM";
        const string name = "Vietnam";

        var country = await EnsureCountryAsync(db: db, name: name, iso: iso, iso3: iso3, ct: ct);

        var existingStateNames = await db.Set<State>()
            .Where(predicate: s => s.CountryId == country.Id)
            .Select(selector: s => s.Name)
            .ToHashSetAsync(comparer: StringComparer.OrdinalIgnoreCase, cancellationToken: ct);

        var vietnamProvinces = GetVietnamProvinces();
        var newStates = vietnamProvinces
            .Where(predicate: p => !existingStateNames.Contains(item: p.Name))
            .Select(selector: p => State.Create(name: p.Name, abbr: p.Abbr, countryId: country.Id).Value)
            .ToList();

        if (newStates.Any())
        {
            await db.Set<State>().AddRangeAsync(entities: newStates, cancellationToken: ct);
            await db.SaveChangesAsync(cancellationToken: ct);
            _logger.Information(messageTemplate: "Added {Count} provinces/cities for Vietnam (post-2025 mergers)", propertyValue: newStates.Count);
        }
        else
        {
            _logger.Information(messageTemplate: "All Vietnam provinces already seeded");
        }
    }

    private async Task SeedUnitedStatesAsync(ApplicationDbContext db, CancellationToken ct)
    {
        const string iso = "US";
        const string iso3 = "USA";
        const string name = "United States";

        var country = await EnsureCountryAsync(db: db, name: name, iso: iso, iso3: iso3, ct: ct);

        var existingStateNames = await db.Set<State>()
            .Where(predicate: s => s.CountryId == country.Id)
            .Select(selector: s => s.Name)
            .ToHashSetAsync(comparer: StringComparer.OrdinalIgnoreCase, cancellationToken: ct);

        var usStates = GetUsStates();
        var newStates = usStates
            .Where(predicate: p => !existingStateNames.Contains(item: p.Name))
            .Select(selector: p => State.Create(name: p.Name, abbr: p.Abbr, countryId: country.Id).Value)
            .ToList();

        if (newStates.Any())
        {
            await db.Set<State>().AddRangeAsync(entities: newStates, cancellationToken: ct);
            await db.SaveChangesAsync(cancellationToken: ct);
            _logger.Information(messageTemplate: "Added {Count} US states", propertyValue: newStates.Count);
        }
        else
        {
            _logger.Information(messageTemplate: "All US states already seeded");
        }
    }

    private async Task<Country> EnsureCountryAsync(ApplicationDbContext db, string name, string iso, string iso3, CancellationToken ct)
    {
        var country = await db.Set<Country>()
            .FirstOrDefaultAsync(predicate: c => c.Iso == iso, cancellationToken: ct);

        if (country == null)
        {
            country = Country.Create(name: name, iso: iso, iso3: iso3).Value;
            await db.Set<Country>().AddAsync(entity: country, cancellationToken: ct);
            await db.SaveChangesAsync(cancellationToken: ct);
            _logger.Information(messageTemplate: "Created country: {CountryName} ({Iso})", propertyValue0: name, propertyValue1: iso);
        }

        return country;
    }

    private static IEnumerable<(string Name, string Abbr)> GetVietnamProvinces() =>
    [
        ("Cao Bằng", "CBG"),
        ("Lạng Sơn", "LSN"),
        ("Phú Thọ", "PTO"),
        ("Quảng Ninh", "QNH"),
        ("Thái Nguyên", "TNG"),
        ("Tuyên Quang", "TQG"),
        ("Lào Cai", "LCI"),
        ("Điện Biên", "DBN"),
        ("Lai Châu", "LCU"),
        ("Sơn La", "SLA"),
        ("Bắc Ninh", "BNH"),
        ("Hưng Yên", "HYN"),
        ("Ninh Bình", "NBH"),
        ("Hà Tĩnh", "HTH"),
        ("Nghệ An", "NAN"),
        ("Quảng Trị", "QTI"),
        ("Thanh Hóa", "THO"),
        ("Đắk Lắk", "DLA"),
        ("Gia Lai", "GLA"),
        ("Lâm Đồng", "LDG"),
        ("Khánh Hòa", "KHA"),
        ("Quảng Ngãi", "QNG"),
        ("Đồng Nai", "DNA"),
        ("Tây Ninh", "TNI"),
        ("An Giang", "AGI"),
        ("Cà Mau", "CMA"),
        ("Đồng Tháp", "DTP"),
        ("Vĩnh Long", "VLG"), ("Hà Nội", "HN"),
        ("Hải Phòng", "HPG"),
        ("Huế", "HUE"),
        ("Đà Nẵng", "DNG"),
        ("Hồ Chí Minh City", "HCM"),
        ("Cần Thơ", "CTH")
    ];

    private static IEnumerable<(string Name, string Abbr)> GetUsStates() =>
    [
        ("Alabama", "AL"),
        ("Alaska", "AK"),
        ("Arizona", "AZ"),
        ("Arkansas", "AR"),
        ("California", "CA"),
        ("Colorado", "CO"),
        ("Connecticut", "CT"),
        ("Delaware", "DE"),
        ("Florida", "FL"),
        ("Georgia", "GA"),
        ("Hawaii", "HI"),
        ("Idaho", "ID"),
        ("Illinois", "IL"),
        ("Indiana", "IN"),
        ("Iowa", "IA"),
        ("Kansas", "KS"),
        ("Kentucky", "KY"),
        ("Louisiana", "LA"),
        ("Maine", "ME"),
        ("Maryland", "MD"),
        ("Massachusetts", "MA"),
        ("Michigan", "MI"),
        ("Minnesota", "MN"),
        ("Mississippi", "MS"),
        ("Missouri", "MO"),
        ("Montana", "MT"),
        ("Nebraska", "NE"),
        ("Nevada", "NV"),
        ("New Hampshire", "NH"),
        ("New Jersey", "NJ"),
        ("New Mexico", "NM"),
        ("New York", "NY"),
        ("North Carolina", "NC"),
        ("North Dakota", "ND"),
        ("Ohio", "OH"),
        ("Oklahoma", "OK"),
        ("Oregon", "OR"),
        ("Pennsylvania", "PA"),
        ("Rhode Island", "RI"),
        ("South Carolina", "SC"),
        ("South Dakota", "SD"),
        ("Tennessee", "TN"),
        ("Texas", "TX"),
        ("Utah", "UT"),
        ("Vermont", "VT"),
        ("Virginia", "VA"),
        ("Washington", "WA"),
        ("West Virginia", "WV"),
        ("Wisconsin", "WI"),
        ("Wyoming", "WY"),
        ("District of Columbia", "DC")
    ];

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}