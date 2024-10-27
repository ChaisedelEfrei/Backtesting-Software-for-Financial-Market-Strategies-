using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YourNamespace.Classes;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly AlphaVantageService _alphaVantageService;

        public StockController(AlphaVantageService alphaVantageService)
        {
            _alphaVantageService = alphaVantageService;
        }

        // Endpoint GET pour récupérer les données d'un stock par symbole
        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetStockData(string symbol)
        {
            try
            {
                var stockData = await _alphaVantageService.GetStockDataAsync(symbol);
                return Ok(stockData); // Retourne les données JSON du stock
            }
            catch (HttpRequestException ex)
            {
                return BadRequest($"Error fetching data for {symbol}: {ex.Message}");
            }
        }
    }
}
