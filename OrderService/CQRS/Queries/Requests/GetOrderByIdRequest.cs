using MediatR;
using OrderService.CQRS.Queries.Responses;

namespace OrderService.CQRS.Queries.Requests;

public class GetOrderByIdRequest(int? orderId) : IRequest<GetOrderByIdResponse>
{
    public GetOrderByIdRequest() : this(null)
    {
    }
    public int? OrderId { get; set; } = orderId;
}