using MediatR;
using OrderService.CQRS.Commands.Responses;

namespace OrderService.CQRS.Commands.Requests;

public class CreateOrderRequest(string name, decimal totalPrice, decimal discount) : IRequest<CreateOrderResponse>
{
    public string Name { get; set; } = name;
    public decimal TotalPrice { get; set; } = totalPrice;
    public decimal Discount { get; set; } = discount;
}