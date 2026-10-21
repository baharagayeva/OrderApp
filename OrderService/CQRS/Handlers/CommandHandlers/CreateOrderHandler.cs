using MassTransit;
using MediatR;
using OrderService.Configurations;
using OrderService.CQRS.Commands.Requests;
using OrderService.CQRS.Commands.Responses;
using OrderService.Entities;
using OrderService.Events;

namespace OrderService.CQRS.Handlers.CommandHandlers;

public class CreateOrderHandler(AppDbContext context, IPublishEndpoint publishEndpoint) : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = new Order
            {
                Name = request.Name,
                Discount = request.Discount,
                TotalPrice = request.TotalPrice,
                OrderDate = DateTime.UtcNow
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync(cancellationToken);
            
            await publishEndpoint.Publish(
                new OrderCreatedEvent(order.Id, order.Name, order.TotalPrice, order.Discount, order.OrderDate),
                cancellationToken);

            return new CreateOrderResponse(order.Id, true);
        }
        catch (Exception e)
        {
            return new CreateOrderResponse(-1, false);
        }
    }
}