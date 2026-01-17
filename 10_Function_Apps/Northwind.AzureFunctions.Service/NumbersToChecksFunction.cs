using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Northwind.AzureFunctions.Service;

public class NumbersToChecksFunction
{
    private readonly ILogger<NumbersToChecksFunction> _logger;

    public NumbersToChecksFunction(ILogger<NumbersToChecksFunction> logger)
    {
        _logger = logger;
    }

    [Function("NumbersToChecksFunction")]
    [QueueOutput("checks-queue")]
    public string Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function NumbersToChecksFunction processed a request.");

        string? amount = req.Query["amount"];

        if (long.TryParse(amount, out long number))
        {
            return number.ToWords().Titleize();
        }
        else
        {
            return $"Failed to parse: {amount}";
        }

    }
}