namespace OrderService.Events;

public record OrderCreatedEvent(
    int OrderId,
    string Name,
    decimal TotalPrice,
    decimal Discount,
    DateTime CreatedAt);