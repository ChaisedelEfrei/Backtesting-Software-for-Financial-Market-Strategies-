using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Backtesting_Software_for_Financial_Market_Strategies_.Classes
{
    class ApiResponse
    {
        public JObject Data { get; set; } 
        public string ErrorMessage { get; set; } 
        public bool Success { get; set; } 

        public long ResponseTime { get; set; }
    }

    class SearchResponse {
        public string Symbol {get; set;}
        public string Name {get; set;}
        public string Currency{get; set;}
        public string Region {get; set;}
        public decimal MatchScore {get; set;}
        public override string ToString()
        {
            return $"Symbol: {Symbol}, Name: {Name}, Currency: {Currency}, Region: {Region}, Match Score: {MatchScore:F2}";
        }
    }

    class AVAPI {
        private readonly string _apiKey = "EU6Q5BFFM9JQ4A8S";

        private async Task<ApiResponse> queryData(string query){
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine($"Querying {query}");
            ApiResponse apiResponse = new ApiResponse();
            string query_url = $"https://www.alphavantage.co/query?{query}&apikey={_apiKey}";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(query_url);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        JObject jsonData = JObject.Parse(data);
                        apiResponse.Success = true;
                        apiResponse.Data = jsonData;
                    }
                    else
                    {
                        apiResponse.ErrorMessage = "Response was empty or null.";
                        apiResponse.Success = false;
                    }
                }
                else
                {
                    string errorData = await response.Content.ReadAsStringAsync();
                    string complete_err = $"Failed to fetch data. HTTP Status Code: {response.StatusCode}\n" + "Error details: " + errorData;
                    apiResponse.ErrorMessage = complete_err;
                }

                stopwatch.Stop();
                apiResponse.ResponseTime = stopwatch.ElapsedMilliseconds;
                return apiResponse;
            }
        }

        public async Task<SearchResponse[]>? searchStock(string name){
            string query = $"function=SYMBOL_SEARCH&keywords={name}";
            ApiResponse response = await queryData(query);
            if (response.Success && response.Data != null){
                var bestMatches = response.Data["bestMatches"];
                int result_size = bestMatches.Count();

                SearchResponse[] searchResult = new SearchResponse[result_size];
                
                for (int i = 0; i < result_size; i++)
                {
                    JObject match = (JObject)bestMatches[i];  // Each match is a JObject
                    searchResult[i] = new SearchResponse
                    {
                        Symbol = match["1. symbol"]?.ToString(),
                        Name = match["2. name"]?.ToString(),
                        Currency = match["8. currency"]?.ToString(),
                        Region = match["4. region"]?.ToString(),
                        MatchScore = decimal.Parse(match["9. matchScore"]?.ToString() ?? "0", CultureInfo.InvariantCulture)
                    };
                }
                return searchResult;
            }
            else {
                Console.WriteLine($"Error while searching for {name} : {response.ErrorMessage}.");
                return null;
            }
        }

        public async Task<Stock>? GetStockData(string symbol, string stock_name)
        {
            string query = $"function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=full";
            ApiResponse response = await queryData(query);
            if (response.Success && response.Data != null)
            {
                var timeSeries = response.Data["Time Series (Daily)"];
                if (timeSeries == null)
                {
                    Console.WriteLine($"No data found for symbol: {symbol}");
                    return null;
                }

                Stock returnedStock = new Stock(symbol, stock_name);

                foreach (var entry in timeSeries)
                {
                    string dateString = ((JProperty)entry).Name;  // Extract the key as a date string
                    DateTime date = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);  // Parse the date

                    JObject dayData = (JObject)entry.First;  // The actual stock data for the date is in the Value part

                    // Add the stock data using the AddData method
                    returnedStock.AddData(
                        date,
                        double.Parse(dayData["1. open"]?.ToString() ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(dayData["2. high"]?.ToString() ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(dayData["3. low"]?.ToString() ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(dayData["4. close"]?.ToString() ?? "0", CultureInfo.InvariantCulture),
                        long.Parse(dayData["5. volume"]?.ToString() ?? "0", CultureInfo.InvariantCulture)
                    );
                }

                // Convert the list to an array and return
                return returnedStock;
            }
            else
            {
                Console.WriteLine($"Error while fetching data for symbol {symbol}: {response.ErrorMessage}");
                return null;
            }
        }

    }
    
}