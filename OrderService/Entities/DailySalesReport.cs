namespace OrderService.Entities;

public class DailySalesReport
{
    public int Id { get; set; }
    public DateOnly ReportDate { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime GeneratedAt { get; set; }
}