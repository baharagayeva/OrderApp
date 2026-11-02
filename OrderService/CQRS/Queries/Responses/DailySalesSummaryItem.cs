namespace OrderService.CQRS.Queries.Responses;

public sealed class DailySalesSummaryItem
{
    public DateTime Day { get; init; }
    public int OrderCount { get; init; }
    public decimal Revenue { get; init; }
}