using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderService.Configurations;
using OrderService.CQRS.Commands.Requests;
using OrderService.CQRS.Handlers.CommandHandlers;
using OrderService.Events;

namespace OrderService.Tests;

public class CreateOrderHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // her testde teze, temiz baza
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
    
    private static CreateOrderHandler CreateHandler(AppDbContext context, IPublishEndpoint publishEndpoint) =>
        new(context, publishEndpoint, NullLogger<CreateOrderHandler>.Instance);

    [Fact]
    public async Task Handle_ShouldCreateOrder_AndReturnSuccessResponse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = CreateHandler(context, new Mock<IPublishEndpoint>().Object);
        var request = new CreateOrderRequest(name: "Test Order", totalPrice: 100, discount: 10);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.OrderId > 0);

        var savedOrder = await context.Orders.FindAsync(result.OrderId);
        Assert.NotNull(savedOrder);
        Assert.Equal("Test Order", savedOrder!.Name);
    }

    [Fact]
    public async Task Handle_ShouldStoreCorrectDiscountAndPrice()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = CreateHandler(context, new Mock<IPublishEndpoint>().Object);
        var request = new CreateOrderRequest(name: "Discounted Order", totalPrice: 200, discount: 50);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var savedOrder = await context.Orders.FindAsync(result.OrderId);
        Assert.NotNull(savedOrder);
        Assert.Equal(200, savedOrder!.TotalPrice);
        Assert.Equal(50, savedOrder.Discount);
    }
    
    [Fact]
    public async Task Handle_ShouldCreateOrder_AndPublishEvent()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var publishMock = new Mock<IPublishEndpoint>();
        var handler = CreateHandler(context, publishMock.Object);
        var request = new CreateOrderRequest(name: "Test Order", totalPrice: 100, discount: 10);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        publishMock.Verify(
            p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Theory]
    [InlineData(100, 150)]  // endirim qiymətdən böyükdür
    [InlineData(100, -5)]   // mənfi endirim
    [InlineData(0, 0)]      // qiymət sıfırdır
    public async Task Handle_ShouldThrow_AndSaveNothing_WhenRequestIsInvalid(int totalPrice, int discount)
    {
        await using var context = CreateInMemoryContext();
        var publishMock = new Mock<IPublishEndpoint>();
        var handler = CreateHandler(context, publishMock.Object);
        var request = new CreateOrderRequest(name: "Invalid Order", totalPrice: totalPrice, discount: discount);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Empty(context.Orders);
        publishMock.Verify(
            p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}