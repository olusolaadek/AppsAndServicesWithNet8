using Microsoft.AspNetCore.Mvc;
using Northwind.WebApi.Client.Mvc.Models;
using System.Diagnostics;

using Northwind.EntityModels;

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
