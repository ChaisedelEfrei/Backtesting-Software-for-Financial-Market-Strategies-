using System;
using System.Collections.Generic;
using YourNamespace.Classes;

namespace YourNamespace.Managers
{
    public class StrategyManager
    {
        private readonly List<ITradingStrategy> _strategies;

        public StrategyManager()
        {
            _strategies = new List<ITradingStrategy>();
        }

        public void AddStrategy(ITradingStrategy strategy)
        {
            _strategies.Add(strategy);
        }

        public void ExecuteStrategies(List<double> prices)
        {
            foreach (var strategy in _strategies)
            {
                if (strategy.ShouldBuy(prices))
                {
                    Console.WriteLine($"{strategy.Name}: Buy signal generated.");
                }
                else if (strategy.ShouldSell(prices))
                {
                    Console.WriteLine($"{strategy.Name}: Sell signal generated.");
                }
                else
                {
                    Console.WriteLine($"{strategy.Name}: No signal.");
                }
            }
        }
    }
}
