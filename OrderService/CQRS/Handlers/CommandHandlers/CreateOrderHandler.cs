using MediatR;
using OrderService.Configurations;
using OrderService.CQRS.Commands.Requests;
using OrderService.CQRS.Commands.Responses;
using OrderService.Entities;

namespace OrderService.CQRS.Handlers.CommandHandlers;

public class CreateOrderHandler(AppDbContext context) : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
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

            return new CreateOrderResponse(order.Id, true);
        }
        catch (Exception e)
        {
            return new CreateOrderResponse(-1, false);
        }
    }
}