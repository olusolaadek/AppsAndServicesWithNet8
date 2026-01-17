using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Northwind.AzureFunctions.Service;

public class NumbersToWordsFunction
{
    private readonly ILogger<NumbersToWordsFunction> _logger;

    public NumbersToWordsFunction(ILogger<NumbersToWordsFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(NumbersToWordsFunction))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
    HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string? amount = req.Query["amount"];
        string result=String.Empty;
        Response response = new Response();
        if (long.TryParse(amount, out long number))
        {
           response.StatusCode = StatusCodes.Status200OK;
           response.Body = number.ToWords().Titleize();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            // await response.WriteStringAsync(number.ToWords());
        }
        else
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Body = "";
            _logger.LogError($"Failed to parse: {amount}");
            //response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            //// response.StatusCode = StatusCodes.Status400BadRequest;
            // response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            //response.WriteStringAsync($"Failed to parse: {amount}").GetAwaiter().GetResult();
        }

        return new OkObjectResult(response);
    }

    class Response
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; } = string.Empty;
    }
}