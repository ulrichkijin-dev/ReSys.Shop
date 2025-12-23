using FluentAssertions;


using MapsterMapper;


using Microsoft.EntityFrameworkCore;


using NSubstitute;


using ReSys.Shop.Core.Domain.Settings.ShippingMethods;
using ReSys.Shop.Core.Feature.Admin.Settings.ShippingMethods;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class ShippingMethodModuleTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ShippingMethodModuleTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mapper = Substitute.For<IMapper>();
    }

    [Fact]
    public async Task Create_ShouldCreateShippingMethod()
    {
        // Arrange
        var handler = new ShippingMethodModule.Create.CommandHandler(_dbContext, _mapper);
        _mapper.Map<ShippingMethodModule.Create.Result>(Arg.Any<ShippingMethod>())
            .Returns(x => new ShippingMethodModule.Create.Result { Id = x.Arg<ShippingMethod>().Id });

        var request = new ShippingMethodModule.Create.Request
        {
            Name = "Standard Shipping",
            Presentation = "Standard",
            Type = ShippingMethod.ShippingType.Standard,
            BaseCost = 10.00m,
            Active = true
        };
        var command = new ShippingMethodModule.Create.Command(request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        var shippingMethod = await _dbContext.Set<ShippingMethod>().FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        shippingMethod.Should().NotBeNull();
        shippingMethod!.Name.Should().Be("standard-shipping");
    }

    [Fact]
    public async Task Activate_ShouldMarkAsActive()
    {
        // Arrange
        var shippingMethod = ShippingMethod.Create("Express", "Express", ShippingMethod.ShippingType.Express, 20m, active: false).Value;
        _dbContext.Set<ShippingMethod>().Add(shippingMethod);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ShippingMethodModule.Activate.CommandHandler(_dbContext, _mapper);
        _mapper.Map<ShippingMethodModule.Activate.Result>(Arg.Any<ShippingMethod>())
            .Returns(x => new ShippingMethodModule.Activate.Result { Id = x.Arg<ShippingMethod>().Id });

        var command = new ShippingMethodModule.Activate.Command(shippingMethod.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        var updated = await _dbContext.Set<ShippingMethod>().FindAsync(new object[] { shippingMethod.Id }, TestContext.Current.CancellationToken);
        updated!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task Deactivate_ShouldMarkAsInactive()
    {
        // Arrange
        var shippingMethod = ShippingMethod.Create("Express", "Express", ShippingMethod.ShippingType.Express, 20m, active: true).Value;
        _dbContext.Set<ShippingMethod>().Add(shippingMethod);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ShippingMethodModule.Deactivate.CommandHandler(_dbContext, _mapper);
        _mapper.Map<ShippingMethodModule.Deactivate.Result>(Arg.Any<ShippingMethod>())
            .Returns(x => new ShippingMethodModule.Deactivate.Result { Id = x.Arg<ShippingMethod>().Id });

        var command = new ShippingMethodModule.Deactivate.Command(shippingMethod.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        var updated = await _dbContext.Set<ShippingMethod>().FindAsync(new object[] { shippingMethod.Id }, TestContext.Current.CancellationToken);
        updated!.Active.Should().BeFalse();
    }
}