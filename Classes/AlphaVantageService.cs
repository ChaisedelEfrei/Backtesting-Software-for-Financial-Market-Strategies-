using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace YourNamespace.Classes
{
    public class AlphaVantageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "67CR552L2NIPD2OM"; // EZG642UW71DQQ49U NOHDA52BSLTHZCXV Replace with your Alpha Vantage API key
        // Cache in memory to store stock data
        private static readonly Dictionary<string, CacheItem> _cache = new Dictionary<string, CacheItem>();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1); // Cache lifetime (ex. 1 hour)

        public AlphaVantageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Method to retrieve stock data with cache and according to the timeframe
        public async Task<string> GetStockDataAsync(string symbol, string timeframe = "daily")
        {
            symbol = symbol.Trim();
            string function = timeframe.ToLower() switch
            {
                "daily" => "TIME_SERIES_DAILY",
                "weekly" => "TIME_SERIES_WEEKLY",
                _ => throw new ArgumentException("Invalid timeframe specified. Choose 'daily' or 'weekly'.")
            };

            // Check if data is cached and not expired
            string cacheKey = $"{symbol}_{function}";
            if (_cache.ContainsKey(cacheKey) && !_cache[cacheKey].IsExpired())
            {
                return _cache[cacheKey].Data;  // Returns the data in cache
            }
            // If not, call the API to get the data
            var url = $"https://www.alphavantage.co/query?function={function}&symbol={symbol}&outputsize=full&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();

                // Cache data with expiration date
                _cache[cacheKey] = new CacheItem(data, DateTime.UtcNow.Add(_cacheDuration));

                return data;
            }
            else
            {
                throw new HttpRequestException("Failed to fetch data from Alpha Vantage.");
            }
        }

        // Method to extract closing prices from API or cache
        public async Task<List<double>> GetClosingPricesAsync(string symbol, string timeframe = "daily")
        {
            string data = await GetStockDataAsync(symbol, timeframe);

            // Extract closing prices from JSON data according to the timeframe
            var prices = new List<double>();
            var jsonData = JObject.Parse(data);
            var timeSeriesKey = timeframe.ToLower() == "daily" ? "Time Series (Daily)" : "Weekly Time Series";
            var timeSeries = jsonData[timeSeriesKey];

            if (timeSeries == null)
            {
                throw new Exception("Invalid data format from Alpha Vantage.");
            }

            foreach (var entry in timeSeries)
            {
                var closeToken = ((JProperty)entry).Value["4. close"];
                if (closeToken == null)
                {
                    throw new Exception("Closing price data is missing.");
                }
                var closingPrice = (double)closeToken;
                prices.Add(closingPrice);
            }

            prices.Reverse(); // Reverse to get the data in chronological order

            return prices;
        }
        // metrics 
        public async Task<Dictionary<string, double>> CalculatePerformanceMetrics(string symbol, string timeframe = "daily")
        {
            var prices = await GetClosingPricesAsync(symbol, timeframe);

            if (prices.Count < 2)
            {
                throw new InvalidOperationException("Insufficient data to calculate metrics.");
            }

            // Cumulative Return
            double cumulativeReturn = (prices.Last() - prices.First()) / prices.First();

            // Daily Returns
            List<double> dailyReturns = new List<double>();
            for (int i = 1; i < prices.Count; i++)
            {
                double dailyReturn = (prices[i] - prices[i - 1]) / prices[i - 1];
                dailyReturns.Add(dailyReturn);
            }

            // Average Daily Return
            double averageDailyReturn = dailyReturns.Average();

            // Volatility (Standard deviation of daily returns)
            double volatility = Math.Sqrt(dailyReturns.Select(r => Math.Pow(r - averageDailyReturn, 2)).Sum() / dailyReturns.Count);

            // Maximum Drawdown
            double maxDrawdown = 0;
            double peak = prices.First();
            foreach (var price in prices)
            {
                if (price > peak)
                {
                    peak = price;
                }
                var drawdown = (peak - price) / peak;
                if (drawdown > maxDrawdown)
                {
                    maxDrawdown = drawdown;
                }
            }

            // Sharpe Ratio (Assuming a risk-free rate of 0 for simplicity)
            double sharpeRatio = averageDailyReturn / volatility;

            // Return metrics in a dictionary
            return new Dictionary<string, double>
            {
                { "CumulativeReturn", cumulativeReturn },
                { "AverageDailyReturn", averageDailyReturn },
                { "Volatility", volatility },
                { "MaxDrawdown", maxDrawdown },
                { "SharpeRatio", sharpeRatio }
            };
        }
    }
}
