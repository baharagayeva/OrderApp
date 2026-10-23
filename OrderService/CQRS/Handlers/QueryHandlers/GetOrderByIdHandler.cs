using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OrderService.Configurations;
using OrderService.CQRS.Queries.Requests;
using OrderService.CQRS.Queries.Responses;
using OrderService.Entities;

namespace OrderService.CQRS.Handlers.QueryHandlers;

public class GetOrderByIdHandler(
    AppDbContext context,
    IDistributedCache cache,
    ILogger<GetOrderByIdHandler> logger)
    : IRequestHandler<GetOrderByIdRequest, GetOrderByIdResponse>
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)   // TTL: 10 dəqiqədən sonra avtomatik silinir
    };
    
    public async Task<GetOrderByIdResponse> Handle(GetOrderByIdRequest request, CancellationToken cancellationToken)
    {
        var key = $"order:{request.OrderId}";
        Order? order;

        var cached = await cache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
        {
            logger.LogInformation("Cache HIT: {Key}", key);
            order = JsonSerializer.Deserialize<Order>(cached);
        }
        else
        {
            logger.LogInformation("Cache MISS: {Key}", key);
            order = await context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is not null)
                await cache.SetStringAsync(key, JsonSerializer.Serialize(order), CacheOptions, cancellationToken);
        }

        return order == null
            ? new GetOrderByIdResponse()
            : new GetOrderByIdResponse(order.Id, order.Name, order.OrderDate, order.TotalPrice, order.Discount);
    }
}