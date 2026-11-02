using Dapper;
using MediatR;
using Npgsql;
using OrderService.CQRS.Queries.Requests;
using OrderService.CQRS.Queries.Responses;

namespace OrderService.CQRS.Handlers.QueryHandlers;

public class GetOrdersHandler(NpgsqlDataSource dataSource)
    : IRequestHandler<GetOrdersRequest, PagedResult<OrderListItem>>
{
    // İki sorğu bir dəfəyə göndərilir: səhifədəki sətirlər və ümumi say
    private const string Sql = """
                               SELECT "Id" AS "OrderId", "Name", "OrderDate", "TotalPrice", "Discount",
                                      "TotalPrice" - "Discount" AS "FinalPrice"
                               FROM "Orders"
                               WHERE (@MinTotal::numeric IS NULL OR "TotalPrice" >= @MinTotal)
                               ORDER BY "OrderDate" DESC, "Id" DESC
                               LIMIT @PageSize OFFSET @Offset;

                               SELECT COUNT(*)
                               FROM "Orders"
                               WHERE (@MinTotal::numeric IS NULL OR "TotalPrice" >= @MinTotal);
                               """;

    public async Task<PagedResult<OrderListItem>> Handle(GetOrdersRequest request, CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            throw new ArgumentException("Page must be 1 or greater.");
        if (request.PageSize is < 1 or > 100)
            throw new ArgumentException("Page size must be between 1 and 100.");

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(Sql, new
        {
            request.MinTotal,
            request.PageSize,
            Offset = (request.Page - 1) * request.PageSize
        }, cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var items = (await results.ReadAsync<OrderListItem>()).ToList();
        var totalCount = await results.ReadSingleAsync<long>();

        return new PagedResult<OrderListItem>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}