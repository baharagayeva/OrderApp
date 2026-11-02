using Dapper;
using MediatR;
using Npgsql;
using OrderService.CQRS.Queries.Requests;
using OrderService.CQRS.Queries.Responses;

namespace OrderService.CQRS.Handlers.QueryHandlers;

public class GetSalesSummaryHandler(NpgsqlDataSource dataSource)
    : IRequestHandler<GetSalesSummaryRequest, IReadOnlyList<DailySalesSummaryItem>>
{
    private const string Sql = """
                               SELECT ("OrderDate" AT TIME ZONE 'UTC')::date AS "Day",
                                      COUNT(*)::int                         AS "OrderCount",
                                      SUM("TotalPrice" - "Discount")        AS "Revenue"
                               FROM "Orders"
                               WHERE "OrderDate" >= @From AND "OrderDate" < @To
                               GROUP BY 1
                               ORDER BY 1;
                               """;

    public async Task<IReadOnlyList<DailySalesSummaryItem>> Handle(
        GetSalesSummaryRequest request, CancellationToken cancellationToken)
    {
        if (request.To.Date < request.From.Date)
            throw new ArgumentException("'to' must be the same day as 'from' or later.");
        if ((request.To.Date - request.From.Date).TotalDays > 366)
            throw new ArgumentException("The date range cannot be longer than one year.");

        // PostgreSQL-in "timestamp with time zone" sütunu UTC tarix tələb edir
        var from = DateTime.SpecifyKind(request.From.Date, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(request.To.Date, DateTimeKind.Utc).AddDays(1);

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<DailySalesSummaryItem>(
            new CommandDefinition(Sql, new { From = from, To = to }, cancellationToken: cancellationToken));

        return rows.ToList();
    }
}