using System.Net.Http.Headers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Configure Polly for resilience
// Create five jittered delays, starting with about 1 second.
IEnumerable<TimeSpan> delays = Backoff.DecorrelatedJitterBackoffV2(
  medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);
WriteLine("Jittered delays for Polly retries:");

foreach (TimeSpan item in delays)
{
    WriteLine($"  {item.TotalSeconds:N2} seconds.");
}
AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
  // Handle network failures, 408 and 5xx status codes.
  .HandleTransientHttpError().WaitAndRetryAsync(delays);

// Register services
builder.Services.AddHttpClient("Northwind.WebApi.Service", client =>
{
    client.BaseAddress = new Uri("https://localhost:5091/");
    // client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 1.0));
}).AddPolicyHandler(retryPolicy);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
