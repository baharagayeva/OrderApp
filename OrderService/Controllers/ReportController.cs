using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Jobs;

namespace OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReportController(IBackgroundJobClient backgroundJobs) : ControllerBase
{
    [HttpPost("daily")]
    public IActionResult GenerateDailyReport([FromQuery] DateTime date)
    {
        var jobId = backgroundJobs.Enqueue<DailySalesReportJob>(job => job.RunAsync(date));
        return Accepted(new { jobId });   // 202: iş qəbul olundu, nəticə sonra hazır olacaq
    }
}