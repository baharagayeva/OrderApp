namespace OrderService.CQRS.Queries.Responses;

public class GetOrderResponse(int orderId, string name, DateTime orderDate, decimal totalPrice, decimal discount)
{
    public GetOrderResponse() : this(-1, string.Empty, DateTime.MinValue, -1, -1)
    {
    }

    public int OrderId { get; set; } = orderId;
    public string Name { get; set; } = name;
    public DateTime OrderDate { get; set; } = orderDate;
    public decimal TotalPrice { get; set; } = totalPrice;
    public decimal Discount { get; set; } = discount;
}