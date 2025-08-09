using Northwind.Background.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<QueueWorker>();

var host = builder.Build();
host.Run();
