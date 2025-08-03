using Northwind.DataContext;
using Northwind.EntityModels;

namespace Northwind.Common.EntityModels.Tests;

public class NorthwindEntityModelsTests
{
    [Fact]
    public void CanConnectIsTrue()
    {
        using var db = new NorthwindContext();
        var canConnect = db.Database.CanConnect();
        Assert.True(canConnect);
    }

    [Fact]
    public void ProviderIsSqlServer()
    {
        using var db = new NorthwindContext();

        string? providerName = db.Database.ProviderName;

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", providerName);

    }

    [Fact]
    public void ProductId1IsChai()
    {
        using var db = new NorthwindContext();
        var product = db.Products.SingleOrDefault(p => p.ProductId == 1);
        Assert.Equal("Chai", product?.ProductName);
    }

    [Fact]
    public void EmployeeHasLastRefreshedIn10sWindow()
    {
        using var db = new NorthwindContext();
        var employee1 = db.Employees.Single(e => e.EmployeeId == 1);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Assert.InRange(actual: employee1.LastRefreshed,
          low: now.Subtract(TimeSpan.FromSeconds(5)),
          high: now.AddSeconds(5));
    }

}