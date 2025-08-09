using Northwind.Background.Workers;

namespace Northwind.Background.Workers;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<QueueWorker>();
        builder.Services.AddHostedService<TimerWorker>();

        var host = builder.Build();
        host.Run();
    }
}
