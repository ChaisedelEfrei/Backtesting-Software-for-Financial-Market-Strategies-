    using System;
    using System.Collections.Generic;

    namespace Backtesting_Software_for_Financial_Market_Strategies_.Classes
    {
        public class Stock
        {
            public string Symbol { get; set; }
            public string Name { get; set; }
            public List<StockData> HistoricalData { get; set; }

            public Stock(string symbol, string name)
            {
                Symbol = symbol;
                Name = name;
                HistoricalData = new List<StockData>();
            }

            // Add a new data entry for a specific date
            public void AddData(DateTime date, double open, double high, double low, double close, long volume)
            {
                HistoricalData.Add(new StockData(date, open, high, low, close, volume));
            }

            // Calculate price variation over the last known stock price
            public void CalculatePriceVariations()
            {
                HistoricalData.Reverse();
                for (int i = 1; i < HistoricalData.Count; i++)  // Start from 1 to avoid out of bounds
                {
                    double previousClose = HistoricalData[i - 1].Close;
                    double currentClose = HistoricalData[i].Close;

                    // Calculate the price variation
                    HistoricalData[i].PriceVariation = ((currentClose - previousClose) / previousClose) * 100;
                }
            }

            // Print historical data
            public void PrintData()
            {
                foreach (var data in HistoricalData)
                {
                    Console.WriteLine($"Date: {data.Date.ToShortDateString()}, Close: {data.Close}, Price Variation: {data.PriceVariation:F2}%");
                }
            }
        }


        public class StockData
        {
            public DateTime Date { get; set; }
            public double Open { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public long Volume { get; set; }
            public double PriceVariation { get; set; }  // Store price variation compared to the previous day's close

            public StockData(DateTime date, double open, double high, double low, double close, long volume)
            {
                Date = date;
                Open = open;
                High = high;
                Low = low;
                Close = close;
                Volume = volume;
                PriceVariation = 0.0;  // Will be calculated later
            }
        }
    }