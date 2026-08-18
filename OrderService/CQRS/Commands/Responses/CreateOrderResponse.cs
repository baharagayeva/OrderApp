namespace OrderService.CQRS.Commands.Responses;

public class CreateOrderResponse(int orderId, bool isSuccess)
{
    public int OrderId { get; set; } = orderId;
    public bool IsSuccess { get; set; } = isSuccess;
}