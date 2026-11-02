namespace OrderService.CQRS.Queries.Responses;

public sealed class OrderListItem
{
    public int OrderId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal Discount { get; init; }
    public decimal FinalPrice { get; init; }
}