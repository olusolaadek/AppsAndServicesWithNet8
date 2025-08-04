using Northwind.DataContext;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Register distributed cache services
builder.Services.AddDistributedMemoryCache();
// register an implementation for the in-memory cache, configured to store a maximum of 50 products
builder.Services.AddSingleton<IMemoryCache>( new MemoryCache(new MemoryCacheOptions
{
    TrackStatistics = true,
    SizeLimit = 50 // products
}));

// Add services to the container.

builder.Services.AddNorthWindContext();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
