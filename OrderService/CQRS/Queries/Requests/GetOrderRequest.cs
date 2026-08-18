using MediatR;
using OrderService.CQRS.Queries.Responses;

namespace OrderService.CQRS.Queries.Requests;

public class GetOrderRequest(int? orderId) : IRequest<GetOrderResponse>
{
    public GetOrderRequest() : this(null)
    {
    }
    public int? OrderId { get; set; } = orderId;
}