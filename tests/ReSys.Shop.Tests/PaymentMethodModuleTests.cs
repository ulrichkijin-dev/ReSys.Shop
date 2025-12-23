using FluentAssertions;


using MapsterMapper;


using Microsoft.EntityFrameworkCore;


using NSubstitute;


using ReSys.Shop.Core.Common.Services.Security.Encryptors.Interfaces;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;
using ReSys.Shop.Core.Feature.Admin.Settings.PaymentMethods;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class PaymentMethodModuleTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ICredentialEncryptor _encryptor;

    public PaymentMethodModuleTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mapper = Substitute.For<IMapper>();
        _encryptor = Substitute.For<ICredentialEncryptor>();
        
        // Setup default encryption behavior
        _encryptor.Encrypt(Arg.Any<string>()).Returns(x => "ENC_" + x.Arg<string>());
        _encryptor.Decrypt(Arg.Any<string>()).Returns(x => x.Arg<string>().Replace("ENC_", ""));
    }

    [Fact]
    public async Task Create_ShouldEncryptPrivateMetadata()
    {
        // Arrange
        var handler = new PaymentMethodModule.Create.CommandHandler(_dbContext, _mapper, _encryptor);
        
        // Setup mapper to return a result so we don't get NullReferenceException in some cases if it was used
        _mapper.Map<PaymentMethodModule.Create.Result>(Arg.Any<PaymentMethod>())
            .Returns(x => new PaymentMethodModule.Create.Result { Id = x.Arg<PaymentMethod>().Id });

        var request = new PaymentMethodModule.Create.Request
        {
            Name = "Stripe",
            Presentation = "Pay with Card",
            Type = PaymentMethod.PaymentType.Stripe,
            Active = true,
            PrivateMetadata = new Dictionary<string, object?>
            {
                ["ApiKey"] = "sk_test_123"
            }
        };
        var command = new PaymentMethodModule.Create.Command(request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        var paymentMethod = await _dbContext.Set<PaymentMethod>().FirstAsync(TestContext.Current.CancellationToken);
        paymentMethod.PrivateMetadata!["ApiKey"].Should().Be("ENC_sk_test_123");
        _encryptor.Received(1).Encrypt("sk_test_123");
    }

    [Fact]
    public async Task GetById_ShouldDecryptPrivateMetadata()
    {
        // Arrange
        var paymentMethod = PaymentMethod.Create(
            name: "Stripe",
            presentation: "Pay with Card",
            type: PaymentMethod.PaymentType.Stripe,
            privateMetadata: new Dictionary<string, object?> { ["ApiKey"] = "ENC_sk_test_123" }
        ).Value;
        _dbContext.Set<PaymentMethod>().Add(paymentMethod);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new PaymentMethodModule.Get.ById.QueryHandler(_dbContext, _mapper, _encryptor);
        var query = new PaymentMethodModule.Get.ById.Query(paymentMethod.Id);
        
        var detailResult = new PaymentMethodModule.Get.ById.Result { Id = paymentMethod.Id };
        _mapper.Map<PaymentMethodModule.Get.ById.Result>(paymentMethod).Returns(detailResult);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        result.Value.PrivateMetadata!["ApiKey"].Should().Be("sk_test_123");
        _encryptor.Received(1).Decrypt("ENC_sk_test_123");
    }

    [Fact]
    public async Task Update_ShouldHandleMetadataCorrectly()
    {
        // Arrange
        var paymentMethod = PaymentMethod.Create("Stripe", "Card", PaymentMethod.PaymentType.Stripe).Value;
        _dbContext.Set<PaymentMethod>().Add(paymentMethod);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new PaymentMethodModule.Update.CommandHandler(_dbContext, _mapper, _encryptor);
        
        _mapper.Map<PaymentMethodModule.Update.Result>(Arg.Any<PaymentMethod>())
            .Returns(x => new PaymentMethodModule.Update.Result { Id = x.Arg<PaymentMethod>().Id });

        var request = new PaymentMethodModule.Update.Request
        {
            Name = "Stripe Updated",
            Presentation = "Card Updated",
            Type = PaymentMethod.PaymentType.Stripe,
            PrivateMetadata = new Dictionary<string, object?> { ["Secret"] = "NewSecret" }
        };
        var command = new PaymentMethodModule.Update.Command(paymentMethod.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        var updated = await _dbContext.Set<PaymentMethod>().FindAsync(new object[] { paymentMethod.Id }, TestContext.Current.CancellationToken);
        updated!.Name.Should().Be("stripe-updated");
        updated.PrivateMetadata!["Secret"].Should().Be("ENC_NewSecret");
    }
}