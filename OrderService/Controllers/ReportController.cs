using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.CQRS.Queries.Requests;
using OrderService.Jobs;

namespace OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReportController(IBackgroundJobClient backgroundJobs, IMediator mediator) : ControllerBase
{
    [HttpPost("daily")]
    public IActionResult GenerateDailyReport([FromQuery] DateTime date)
    {
        var jobId = backgroundJobs.Enqueue<DailySalesReportJob>(job => job.RunAsync(date));
        return Accepted(new { jobId });   // 202: iş qəbul olundu, nəticə sonra hazır olacaq
    }
    
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] GetSalesSummaryRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(request, ct));
    }
}