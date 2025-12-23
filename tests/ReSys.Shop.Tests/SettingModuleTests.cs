using FluentAssertions;


using MapsterMapper;


using Microsoft.EntityFrameworkCore;


using NSubstitute;


using ReSys.Shop.Core.Domain.Settings;
using ReSys.Shop.Core.Feature.Admin.Settings.SettingModule;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class SettingModuleTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public SettingModuleTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mapper = Substitute.For<IMapper>();
    }

    [Fact]
    public async Task Create_ShouldCreateSetting()
    {
        // Arrange
        var handler = new SettingModule.Create.CommandHandler(_dbContext, _mapper);
        _mapper.Map<SettingModule.Create.Result>(Arg.Any<Setting>())
            .Returns(x => new SettingModule.Create.Result { Id = x.Arg<Setting>().Id });

        var request = new SettingModule.Create.Request
        {
            Key = "site_name",
            Value = "My Shop",
            DefaultValue = "Default Shop",
            ValueType = ConfigurationValueType.String,
            Description = "The name of the site"
        };
        var command = new SettingModule.Create.Command(request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        var setting = await _dbContext.Set<Setting>().FirstOrDefaultAsync(s => s.Key == "site_name", TestContext.Current.CancellationToken);
        setting.Should().NotBeNull();
        setting!.Value.Should().Be("My Shop");
    }

    [Fact]
    public async Task Update_ShouldUpdateValue()
    {
        // Arrange
        var setting = Setting.Create("site_name", "Old Name", "Desc", "Default", ConfigurationValueType.String).Value;
        _dbContext.Set<Setting>().Add(setting);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new SettingModule.Update.CommandHandler(_dbContext, _mapper);
        _mapper.Map<SettingModule.Update.Result>(Arg.Any<Setting>())
            .Returns(x => new SettingModule.Update.Result { Id = x.Arg<Setting>().Id });

        var request = new SettingModule.Update.Request
        {
            Key = "site_name",
            Value = "New Name",
            DefaultValue = "Default",
            ValueType = ConfigurationValueType.String,
            Description = "Updated Desc"
        };
        var command = new SettingModule.Update.Command(setting.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        var updated = await _dbContext.Set<Setting>().FindAsync(new object[] { setting.Id }, TestContext.Current.CancellationToken);
        updated!.Value.Should().Be("New Name");
    }

    [Fact]
    public async Task Delete_ShouldRemoveSetting()
    {
        // Arrange
        var setting = Setting.Create("test_key", "val", "desc", "def", ConfigurationValueType.String).Value;
        _dbContext.Set<Setting>().Add(setting);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new SettingModule.Delete.CommandHandler(_dbContext);
        var command = new SettingModule.Delete.Command(setting.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        var exists = await _dbContext.Set<Setting>().AnyAsync(s => s.Id == setting.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }
}