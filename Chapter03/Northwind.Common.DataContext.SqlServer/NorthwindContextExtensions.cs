using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northwind.DataContext;

public static class NorthwindContextExtensions
{
    public static IServiceCollection AddNorthWindContext(
        this IServiceCollection services,
        string? connectionString = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = "localhost",
                InitialCatalog = "Northwind",
                TrustServerCertificate = true,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
                ConnectTimeout = 3
            };
            connectionString = builder.ConnectionString;
            //connectionString = Environment.GetEnvironmentVariable("NORTHWIND_CONNECTION_STRING");
            //if (string.IsNullOrEmpty(connectionString))
            //{
            //    throw new ArgumentException("Connection string is not provided and environment variable 'NORTHWIND_CONNECTION_STRING' is not set.");
            //}
        }
        services.AddDbContext<NorthwindContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.LogTo(Console.WriteLine,
                new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting })
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors();
        },
        // Register with a transient lifetime to avoid concurrency 
        // issues with Blazor Server projects.
        contextLifetime: ServiceLifetime.Transient,
        optionsLifetime: ServiceLifetime.Transient
        );

        return services;
    }
}
