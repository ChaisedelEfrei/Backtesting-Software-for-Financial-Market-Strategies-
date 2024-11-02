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
            return View("StockData");
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
                // Check if jsonData is null or contains an error message
                if (jsonData == null || jsonData.Contains("Error Message"))
                {
                    return BadRequest("API limit reached or symbol data is unavailable. Please try again later.");
                }

                // Parse the JSON data into a list of StockDataModel
                var stockData = ParseJsonToStockData(jsonData);

                return Ok(stockData);
            }
            catch (HttpRequestException ex)
            {
                return BadRequest($"Error fetching data for {symbol}: {ex.Message}");
            }
        }

        //endpoint for performance metric
        [HttpGet("GetPerformanceMetrics")]
        public async Task<IActionResult> GetPerformanceMetrics(string symbol, string timeframe = "daily")
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return BadRequest("Symbol is required.");
            }
            try
            {
                var metrics = await _alphaVantageService.CalculatePerformanceMetrics(symbol, timeframe);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating metrics for {symbol}: {ex.Message}");
            }
        }
        // Helper method to parse JSON data into StockDataModel objects
        private List<StockDataModel> ParseJsonToStockData(string jsonData)
        {
            var stockDataList = new List<StockDataModel>();
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonData);
            var timeSeries = jsonObject["Time Series (Daily)"];

            if (timeSeries == null)
            {
                Console.WriteLine("Time series data is missing or malformed.");
                return stockDataList;
            }

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
