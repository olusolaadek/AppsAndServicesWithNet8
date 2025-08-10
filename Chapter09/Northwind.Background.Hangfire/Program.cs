using Microsoft.Data.SqlClient; // To use SqlConnectionStringBuilder.
using Northwind.Background.Models; // To use WriteMessageJobDetail.
using Microsoft.AspNetCore.Mvc; // To use [FromBody].

using Hangfire;
partial class Program
{
    private static void Main(string[] args)
    {
        SqlConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "localhost",
            InitialCatalog = "Northwind.HangfireDb",
            IntegratedSecurity = true,
            Encrypt = true,
            MultipleActiveResultSets = true,
            TrustServerCertificate = true,
            ConnectTimeout = 10,
        };

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseColouredConsoleLogProvider()
                  .UseSqlServerStorage(connectionStringBuilder.ConnectionString);
        });
        builder.Services.AddHangfireServer();


        var app = builder.Build();
        app.UseHangfireDashboard();

        app.MapGet("/", () => "Navigate to /hangfire to see the Hangfire Dashboard.");

        app.MapPost("/schedulejob", ([FromBody] WriteMessageJobDetail jobDetail) =>
        {
            BackgroundJob.Schedule(methodCall: () => WriteMessage(jobDetail.Message), 
                enqueueAt: DateTimeOffset.UtcNow + TimeSpan.FromSeconds(jobDetail.Seconds));
        });
        app.MapHangfireDashboard();
        app.Run();
    }
}