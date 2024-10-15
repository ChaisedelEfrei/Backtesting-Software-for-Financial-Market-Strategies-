using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Backtesting_Software_for_Financial_Market_Strategies_.Classes;

class Program
{
    private static readonly string apiKey = "YOUR_API_KEY";
    private static readonly string baseUrl = "https://www.alphavantage.co/query?";

    static async Task Main(string[] args)
    {   
        AVAPI alpha_api = new AVAPI();
        
        var s_result = await alpha_api.searchStock("AAPL");

        Console.WriteLine($"Got {s_result.Length} results for AAPL :");

        for (int i = 0; i < s_result.Length; i++){
            Console.WriteLine(s_result[i]);
        }

        var returned_stock = await alpha_api.GetStockData(s_result[0].Symbol, s_result[0].Name);
        if (returned_stock != null)
        {   
            returned_stock.CalculatePriceVariations();
            returned_stock.PrintData();  // Print the historical data directly
        }
        
        
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpClient<AlphaVantageService>(); // AlphaVantageService is a custom service in Controllers folder

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();

                
    }

    /*public static async Task GetAvailableStocks(string keyword)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"{baseUrl}function=SYMBOL_SEARCH&keywords={keyword}&apikey={apiKey}";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                JObject jsonData = JObject.Parse(data);
                Console.WriteLine(jsonData.ToString());
            }
        }
    }*/
}

