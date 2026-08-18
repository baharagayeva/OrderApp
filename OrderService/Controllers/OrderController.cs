using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.CQRS.Commands.Requests;
using OrderService.CQRS.Queries.Requests;

namespace OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IMediator mediator) : ControllerBase
{
   [HttpPost("CreateOrder")]
   public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
   {
      return Ok (await mediator.Send(request, ct));
   }

   [HttpGet("GetOrderById")]
   public async Task<IActionResult> GetOrderById([FromQuery] GetOrderRequest request, CancellationToken ct)
   {
      return Ok(await mediator.Send(request, ct));
   }
}