using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YourNamespace.Models;
using YourNamespace.Classes;
using System.Globalization;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : Controller
    {
        private readonly AlphaVantageService _alphaVantageService;

        public StockController(AlphaVantageService alphaVantageService)
        {
            _alphaVantageService = alphaVantageService;
        }

        public ActionResult Index()
        {
            return View();
        }

        // Endpoint GET pour récupérer les données d'un stock par symbole
        [HttpGet("GetStockData")]
        public async Task<IActionResult> GetStockData(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return BadRequest("Symbol is required.");
            }
            try
            {
                var jsonData = await _alphaVantageService.GetStockDataAsync(symbol);
                // Parse the JSON data into a list of StockDataModel
                var stockData = ParseJsonToStockData(jsonData);

                return View("StockData", stockData);
            }
            catch (HttpRequestException ex)
            {
                return BadRequest($"Error fetching data for {symbol}: {ex.Message}");
            }
        }
        // Helper method to parse JSON data into StockDataModel objects
        private List<StockDataModel> ParseJsonToStockData(string jsonData)
        {
            var stockDataList = new List<StockDataModel>();
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonData);
            var timeSeries = jsonObject["Time Series (Daily)"];

            foreach (var dayData in timeSeries)
            {
                var stockData = new StockDataModel
                {
                    Symbol = jsonObject["Meta Data"]["2. Symbol"],
                    Date = DateTime.Parse(dayData.Name),
                    Open = decimal.Parse(dayData.First["1. open"].ToString(), CultureInfo.InvariantCulture),
                    High = decimal.Parse(dayData.First["2. high"].ToString(), CultureInfo.InvariantCulture),
                    Low = decimal.Parse(dayData.First["3. low"].ToString(), CultureInfo.InvariantCulture),
                    Close = decimal.Parse(dayData.First["4. close"].ToString(), CultureInfo.InvariantCulture),
                    Volume = long.Parse(dayData.First["5. volume"].ToString())
                };
                stockDataList.Add(stockData);
            }
            return stockDataList;
        }
    }
}
