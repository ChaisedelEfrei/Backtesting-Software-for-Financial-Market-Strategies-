using System;
using System.Collections.Generic;
using System.Linq;

namespace YourNamespace.Classes
{
    public class PerformanceMetrics
    {
        public static double CalculateCumulativeReturn(List<double> prices)
        {
            if (prices == null || prices.Count < 2)
                return 0; // not enough data to calculate cumulative return

            double cumulativeReturn = (prices.Last() - prices.First()) / prices.First();
            return cumulativeReturn;
        }

        public static double CalculateAverageDailyReturn(List<double> prices)
        {
            if (prices == null || prices.Count < 2)
                return 0; // not enough data to calculate average daily return

            List<double> dailyReturns = new List<double>();
            for (int i = 1; i < prices.Count; i++)
            {
                dailyReturns.Add((prices[i] - prices[i - 1]) / prices[i - 1]);
            }

            return dailyReturns.Average();
        }

        public static double CalculateVolatility(List<double> prices)
        {
            if (prices == null || prices.Count < 2)
                return 0; // Not enough data to calculate volatility

            List<double> dailyReturns = new List<double>();
            for (int i = 1; i < prices.Count; i++)
            {
                dailyReturns.Add((prices[i] - prices[i - 1]) / prices[i - 1]);
            }

            double averageReturn = dailyReturns.Average();
            double sumSquaredDifferences = dailyReturns.Sum(r => Math.Pow(r - averageReturn, 2));
            return Math.Sqrt(sumSquaredDifferences / dailyReturns.Count);
        }

        public static double CalculateMaximumDrawdown(List<double> prices)
        {
            if (prices == null || prices.Count < 2)
                return 0; // not enough data to calculate maximum drawdown

            double peak = prices[0];
            double maxDrawdown = 0;

            foreach (double price in prices)
            {
                if (price > peak)
                {
                    peak = price;
                }
                double drawdown = (peak - price) / peak;
                if (drawdown > maxDrawdown)
                {
                    maxDrawdown = drawdown;
                }
            }

            return maxDrawdown;
        }

        // Method to calculate Sharpe ratio
        public static double CalculateSharpeRatio(List<double> prices, double riskFreeRate)
        {
            if (prices == null || prices.Count < 2)
                return 0; // not enough data to calculate Sharpe ratio

            double averageReturn = CalculateAverageDailyReturn(prices);
            double volatility = CalculateVolatility(prices);

            if (volatility == 0)
                return 0; // return 0 if volatility is zero to avoid division by zero

            return (averageReturn - riskFreeRate) / volatility;
        }
    }
} 
