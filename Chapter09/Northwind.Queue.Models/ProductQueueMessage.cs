using Northwind.EntityModels;

namespace Northwind.Queue.Models;

public class ProductQueueMessage
{
    public string? Text { get; set; }
    public Product Product { get; set; } = null!;
}
