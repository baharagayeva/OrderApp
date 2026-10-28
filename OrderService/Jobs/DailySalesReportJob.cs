using Hangfire;
using Microsoft.EntityFrameworkCore;
using OrderService.Configurations;
using OrderService.Entities;

namespace OrderService.Jobs;

[AutomaticRetry(Attempts = 3)]   // uğursuz olsa 3 dəfə yenidən cəhd edir (defolt 10-dur)
public class DailySalesReportJob(AppDbContext context, ILogger<DailySalesReportJob> logger)
{
    // Cron hər gecə bunu çağırır: dünənin hesabatı
    public Task RunForYesterdayAsync() => RunAsync(DateTime.UtcNow.Date.AddDays(-1));

    // İstənilən gün üçün hesabat
    public async Task RunAsync(DateTime day)
    {
        var from = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);
        var to = from.AddDays(1);
        var reportDate = DateOnly.FromDateTime(from);

        var orders = context.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= from && o.OrderDate < to);

        var orderCount = await orders.CountAsync();
        var revenue = await orders.SumAsync(o => o.TotalPrice - o.Discount);

        // Eyni gün üçün hesabat varsa yenilə, yoxdursa yarat (job təkrar işləsə belə bir sətir qalır)
        var report = await context.DailySalesReports.FirstOrDefaultAsync(r => r.ReportDate == reportDate);
        if (report is null)
        {
            report = new DailySalesReport { ReportDate = reportDate };
            context.DailySalesReports.Add(report);
        }

        report.OrderCount = orderCount;
        report.TotalRevenue = revenue;
        report.GeneratedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Daily sales report for {ReportDate}: {OrderCount} orders, revenue {Revenue}",
            reportDate, orderCount, revenue);
    }
}