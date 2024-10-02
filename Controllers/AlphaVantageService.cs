using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

public class AlphaVantageService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "EU6Q5BFFM9JQ4A8S"; // put your API key here

    public AlphaVantageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetStockDataAsync(string symbol)
    {
        var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new HttpRequestException("Failed to fetch data from Alpha Vantage.");
        }
    }
}
// to inject as a depencency in your controller.
public class StockController : Controller
{
    private readonly AlphaVantageService _alphaVantageService;

    public StockController(AlphaVantageService alphaVantageService)
    {
        _alphaVantageService = alphaVantageService;
    }

    public async Task<IActionResult> Index(string symbol = "AAPL")
    {
        var stockData = await _alphaVantageService.GetStockDataAsync(symbol);
        return View(stockData);
    }
}

