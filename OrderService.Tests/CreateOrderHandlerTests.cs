using MassTransit;
using Microsoft.EntityFrameworkCore;
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
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_AndReturnSuccessResponse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = new CreateOrderHandler(context, new Mock<IPublishEndpoint>().Object);
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
        var handler = new CreateOrderHandler(context, new Mock<IPublishEndpoint>().Object);
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
        var handler = new CreateOrderHandler(context, publishMock.Object);
        var request = new CreateOrderRequest(name: "Test Order", totalPrice: 100, discount: 10);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        publishMock.Verify(
            p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}