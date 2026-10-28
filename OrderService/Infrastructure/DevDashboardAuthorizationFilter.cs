using Hangfire.Dashboard;

namespace OrderService.Infrastructure;

// Yalnız Development üçün: paneli hamıya açır
public class DevDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}