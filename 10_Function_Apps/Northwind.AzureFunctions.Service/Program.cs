using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient("Amazon", options =>
{
    options.BaseAddress = new Uri("https://www.amazon.ca");
    // Pretend to be Chrome with US English.
    options.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("text/html"));
    options.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
    options.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
    options.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("image/avif"));
    options.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("image/webp"));
    options.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("image/apng"));
    options.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("*/*", 0.8));

    options.DefaultRequestHeaders.AcceptLanguage.Add(
new StringWithQualityHeaderValue("en-US"));
    options.DefaultRequestHeaders.AcceptLanguage.Add(
new StringWithQualityHeaderValue("en", 0.8));
    options.DefaultRequestHeaders.UserAgent.Add(
new(productName: "Chrome", productVersion: "114.0.5735.91"));
});

string? storageConnection = builder.Configuration["AzureWebJobsStorage"];

builder.Services.AddAzureClients(clientBuilder =>
{
    if (!string.IsNullOrEmpty(storageConnection))
    {
        clientBuilder.AddBlobServiceClient(storageConnection);
    }
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
