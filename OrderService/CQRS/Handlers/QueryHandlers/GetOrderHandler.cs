using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Configurations;
using OrderService.CQRS.Queries.Requests;
using OrderService.CQRS.Queries.Responses;

namespace OrderService.CQRS.Handlers.QueryHandlers;

public class GetOrderHandler(AppDbContext context) : IRequestHandler<GetOrderRequest, GetOrderResponse>
{
    public async Task<GetOrderResponse> Handle(GetOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.OrderId == null)
            return new GetOrderResponse();

        var order = await context.Orders
            .AsNoTracking()
            .Where(x => x.Id == request.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        return order == null
            ? new GetOrderResponse()
            : new GetOrderResponse(order.Id, order.Name, order.OrderDate, order.TotalPrice, order.Discount);
    }
}