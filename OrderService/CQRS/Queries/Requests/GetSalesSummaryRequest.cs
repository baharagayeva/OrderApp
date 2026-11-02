using MediatR;
using OrderService.CQRS.Queries.Responses;

namespace OrderService.CQRS.Queries.Requests;

public class GetSalesSummaryRequest : IRequest<IReadOnlyList<DailySalesSummaryItem>>
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }   // bu gün də daxildir
}