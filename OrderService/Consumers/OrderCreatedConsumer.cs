using MassTransit;
using OrderService.Events;

namespace OrderService.Consumers;

public class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var msg = context.Message;
        var attempt = context.GetRetryAttempt(); // 0 = ilk cəhd, 1, 2, 3 = retry-lər

        logger.LogWarning("OrderCreated emal olunur: Id={OrderId}, cəhd={Attempt}",
            msg.OrderId, attempt);

        if (msg.Name.Contains("fail", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Sifariş {msg.OrderId} emal oluna bilmədi (test xətası)");

        logger.LogInformation("OrderCreated uğurla emal olundu: Id={OrderId}", msg.OrderId);
        return Task.CompletedTask;
    }
}