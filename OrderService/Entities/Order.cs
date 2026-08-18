namespace OrderService.Entities;

public class Order(string name, DateTime orderDate, decimal totalPrice, decimal discount)
{
    public Order() : this(string.Empty, DateTime.MinValue, -1, -1)
    {
    }

    public int Id { get; set; }
    public string Name { get; set; } = name;
    public DateTime OrderDate { get; set; } = orderDate;
    public decimal TotalPrice { get; set; } = totalPrice;
    public decimal Discount { get; set; } = discount;
}