using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace YourNamespace.Classes
{
    public class AlphaVantageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "NOHDA52BSLTHZCXV"; // Remplacez par votre clé API Alpha Vantage

        // Cache en mémoire pour stocker les données de stock
        private readonly Dictionary<string, CacheItem> _cache = new Dictionary<string, CacheItem>();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);  // Durée de vie du cache (ex. 1 heure)

        public AlphaVantageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Méthode pour récupérer les données de stock avec cache et en fonction du timeframe
        public async Task<string> GetStockDataAsync(string symbol, string timeframe = "daily")
        {
            symbol = symbol.Trim();
            string function = timeframe.ToLower() switch
            {
                "daily" => "TIME_SERIES_DAILY",
                "weekly" => "TIME_SERIES_WEEKLY",
                _ => throw new ArgumentException("Invalid timeframe specified. Choose 'daily' or 'weekly'.")
            };

            // Vérifier si les données sont en cache et non expirées
            string cacheKey = $"{symbol}_{function}";
            if (_cache.ContainsKey(cacheKey) && !_cache[cacheKey].IsExpired())
            {
                return _cache[cacheKey].Data;  // Retourne les données en cache
            }

            // Sinon, appeler l'API pour obtenir les données
            var url = $"https://www.alphavantage.co/query?function={function}&symbol={symbol}&outputsize=full&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();

                // Mettre les données en cache avec une date d'expiration
                _cache[cacheKey] = new CacheItem(data, DateTime.UtcNow.Add(_cacheDuration));

                return data;
            }
            else
            {
                throw new HttpRequestException("Failed to fetch data from Alpha Vantage.");
            }
        }

        // Méthode pour extraire les prix de clôture de l'API ou du cache
        public async Task<List<double>> GetClosingPricesAsync(string symbol, string timeframe = "daily")
        {
            string data = await GetStockDataAsync(symbol, timeframe);

            // Extraire les prix de clôture depuis les données JSON en fonction du timeframe
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

            prices.Reverse(); // Inverser pour avoir les données dans l'ordre chronologique

            return prices;
        }
    }
}
