using MediatR;
using OrderService.CQRS.Queries.Responses;

namespace OrderService.CQRS.Queries.Requests;

public class GetOrdersRequest : IRequest<PagedResult<OrderListItem>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public decimal? MinTotal { get; set; }   // istəyə bağlı filtr
}