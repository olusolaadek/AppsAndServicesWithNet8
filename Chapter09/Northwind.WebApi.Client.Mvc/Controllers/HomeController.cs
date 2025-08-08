using Microsoft.AspNetCore.Mvc;
using Northwind.WebApi.Client.Mvc.Models;
using System.Diagnostics;
using RabbitMQ.Client;

using Northwind.EntityModels;
using System.Text.Json;
using System.Text;


namespace Northwind.WebApi.Client.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

         [Route("home/products/{name?}")]
        public async Task<IActionResult> Products(string? name = "cha")
        {
            HomeProductsViewModel model = new();
            HttpClient client = _httpClientFactory.CreateClient("Northwind.WebApi.Service");
            model.NameContains = name;
            model.BaseAddress = client.BaseAddress;
            HttpRequestMessage request = new(HttpMethod.Get, $"api/products/{name}");

            HttpResponseMessage responseMessage = await client.SendAsync(request);

            if (responseMessage.IsSuccessStatusCode)
            {
                model.Products = await responseMessage.Content.ReadFromJsonAsync<IEnumerable<Product>>();
            }
            else
            {
                model.Products = Enumerable.Empty<Product>();
                string content = await responseMessage.Content.ReadAsStringAsync();

                // Use the range operator .. to start from zero and go to the first carriage return.
                string errorMessage = content[..content.IndexOf('\n')];
                model.ErrorMessage = $"Error: {responseMessage.StatusCode} - {responseMessage.ReasonPhrase} - {errorMessage}";
                _logger.LogError(model.ErrorMessage);
            }
            return View(model);
        }

        public IActionResult SendMessage()
        {
            return View();
        }

        // POST: home/sendmessage
        // Body: message=Hello&productId=1
        [HttpPost]
        public async Task<IActionResult> SendMessage(string? message, int? productId)
        {
            HomeSendMessageViewModel model = new();
            model.Message = new();

            if(message is null || productId is null)
            {
                model.Error = "Message or Product ID is missing.";
                return View(model);
            }
            model.Message.Text = message;
            model.Message.Product = new Product { ProductId = productId.Value };

            HttpClient client = _httpClientFactory.CreateClient("Northwind.WebApi.Service");

            HttpRequestMessage request = new(HttpMethod.Get, $"api/products/{productId}");
            HttpResponseMessage responseMessage = await client.SendAsync(request);

            if (responseMessage.IsSuccessStatusCode)
            {
                Product? product = await responseMessage.Content.ReadFromJsonAsync<Product>();
                if (product is not null)
                {
                    model.Message.Product = product;
                }

                // Create a RabbitMQ factory.
                ConnectionFactory factory = new()
                {
                    HostName = "localhost"
                };

                using IConnection connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();
                string queueNameAndRoutingKey = "product";

                await channel.QueueDeclareAsync(queue: queueNameAndRoutingKey,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model.Message));

                var properties = new BasicProperties();
                await channel.BasicPublishAsync<BasicProperties>(
                    exchange: string.Empty,
                    routingKey: queueNameAndRoutingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: default);

                // After successful publish, return the view with the model (optionally, you can redirect or show a success message)
                model.Success = true;
                model.Info = $"Message sent successfully to queue '{queueNameAndRoutingKey}' for product ID {productId}.";
                return View(model);
            }
            else
            {
                model.Error = $"Error: {responseMessage.StatusCode} - {responseMessage.ReasonPhrase}";
                _logger.LogError(model.Error);
                return View(model);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
