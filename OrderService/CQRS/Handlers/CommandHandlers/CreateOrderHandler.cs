using MassTransit;
using MediatR;
using OrderService.Configurations;
using OrderService.CQRS.Commands.Requests;
using OrderService.CQRS.Commands.Responses;
using OrderService.Entities;
using OrderService.Events;

namespace OrderService.CQRS.Handlers.CommandHandlers;

public class CreateOrderHandler(AppDbContext context, IPublishEndpoint publishEndpoint, ILogger<CreateOrderHandler> logger)
    : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.TotalPrice <= 0)
            throw new ArgumentException("Total price must be greater than zero.");
        if (request.Discount < 0 || request.Discount > request.TotalPrice)
            throw new ArgumentException("Discount must be between 0 and the total price.");

        // Xəta olsa, metoddan çıxanda tranzaksiya dispose olunur və avtomatik rollback edilir
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var order = new Order
        {
            Name = request.Name,
            Discount = request.Discount,
            TotalPrice = request.TotalPrice,
            OrderDate = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken); // order.Id yaranır, tranzaksiya hələ açıqdır

        await publishEndpoint.Publish(
            new OrderCreatedEvent(order.Id, order.Name, order.TotalPrice, order.Discount, order.OrderDate),
            cancellationToken); // RabbitMQ-ya yox, outbox-a əlavə olunur
        await context.SaveChangesAsync(cancellationToken); // outbox sətri bazaya yazılır

        await transaction.CommitAsync(cancellationToken); // sifariş və event birlikdə commit olunur

        logger.LogInformation("Order {OrderId} created with total {TotalPrice} and discount {Discount}",
            order.Id, order.TotalPrice, order.Discount);

        return new CreateOrderResponse(order.Id, true);
    }
}