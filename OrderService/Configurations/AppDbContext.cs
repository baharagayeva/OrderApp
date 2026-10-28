using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Entities;

namespace OrderService.Configurations;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)  : base(options)
    {
        
    }
    
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<DailySalesReport> DailySalesReports => Set<DailySalesReport>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        builder.Entity<DailySalesReport>()
            .HasIndex(r => r.ReportDate)
            .IsUnique();
        
        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }
}