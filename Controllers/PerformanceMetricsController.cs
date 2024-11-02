using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Classes;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerformanceMetricsController : ControllerBase
    {
        private readonly AlphaVantageService _alphaVantageService;

        public PerformanceMetricsController(AlphaVantageService alphaVantageService)
        {
            _alphaVantageService = alphaVantageService;
        }

        [HttpGet("cumulative-return")]
        public async Task<IActionResult> GetCumulativeReturn(string symbol, string timeframe, DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = await _alphaVantageService.GetStockDataAsync(symbol, timeframe);
            var prices = ParsePrices(data, timeframe, startDate, endDate);
            var cumulativeReturn = PerformanceMetrics.CalculateCumulativeReturn(prices);
            return Ok(cumulativeReturn);
        }

        [HttpGet("average-daily-return")]
        public async Task<IActionResult> GetAverageDailyReturn(string symbol, string timeframe, DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = await _alphaVantageService.GetStockDataAsync(symbol, timeframe);
            var prices = ParsePrices(data, timeframe, startDate, endDate);
            var averageDailyReturn = PerformanceMetrics.CalculateAverageDailyReturn(prices);
            return Ok(averageDailyReturn);
        }

        [HttpGet("volatility")]
        public async Task<IActionResult> GetVolatility(string symbol, string timeframe, DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = await _alphaVantageService.GetStockDataAsync(symbol, timeframe);
            var prices = ParsePrices(data, timeframe, startDate, endDate);
            var volatility = PerformanceMetrics.CalculateVolatility(prices);
            return Ok(volatility);
        }

        [HttpGet("maximum-drawdown")]
        public async Task<IActionResult> GetMaximumDrawdown(string symbol, string timeframe, DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = await _alphaVantageService.GetStockDataAsync(symbol, timeframe);
            var prices = ParsePrices(data, timeframe, startDate, endDate);
            var maxDrawdown = PerformanceMetrics.CalculateMaximumDrawdown(prices);
            return Ok(maxDrawdown);
        }

        [HttpGet("sharpe-ratio")]
        public async Task<IActionResult> GetSharpeRatio(string symbol, string timeframe, double riskFreeRate = 0.01, DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = await _alphaVantageService.GetStockDataAsync(symbol, timeframe);
            var prices = ParsePrices(data, timeframe, startDate, endDate);
            var sharpeRatio = PerformanceMetrics.CalculateSharpeRatio(prices, riskFreeRate);
            return Ok(sharpeRatio);
        }

        private List<double> ParsePrices(string jsonData, string timeframe, DateTime? startDate, DateTime? endDate)
        {
            var prices = new List<double>();

            try
            {
                var jsonObject = JObject.Parse(jsonData);
                string timeSeriesKey = timeframe.ToLower() switch
                {
                    "daily" => "Time Series (Daily)",
                    "weekly" => "Weekly Time Series",
                    _ => throw new ArgumentException("Invalid timeframe specified.")
                };

                var timeSeries = jsonObject[timeSeriesKey] as JObject;

                if (timeSeries != null)
                {
                    foreach (var entry in timeSeries)
                    {
                        DateTime entryDate = DateTime.Parse(entry.Key);
                        if ((startDate == null || entryDate >= startDate) && (endDate == null || entryDate <= endDate))
                        {
                            var dailyData = entry.Value as JObject;
                            if (dailyData != null && dailyData.ContainsKey("4. close"))
                            {
                                double closePrice = double.Parse(dailyData["4. close"].ToString(), CultureInfo.InvariantCulture);
                                prices.Add(closePrice);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON data: {ex.Message}");
            }

            return prices.OrderByDescending(p => p).ToList(); 
        }
    }
}
