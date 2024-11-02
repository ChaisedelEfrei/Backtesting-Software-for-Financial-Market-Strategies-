using System;
using System.Collections.Generic;
using System.Linq;

namespace YourNamespace.Classes
{
    public class MovingAverageCrossoverStrategy : ITradingStrategy
    {
        public string Name => "Moving Average Crossover";
        private readonly int _shortPeriod;
        private readonly int _longPeriod;

        public MovingAverageCrossoverStrategy(int shortPeriod, int longPeriod)
        {
            _shortPeriod = shortPeriod;
            _longPeriod = longPeriod;
        }

        public bool ShouldBuy(List<double> prices)
        {
            if (prices.Count < _longPeriod) return false;

            var shortMA = prices.TakeLast(_shortPeriod).Average();
            var longMA = prices.TakeLast(_longPeriod).Average();

            return shortMA > longMA;
        }

        public bool ShouldSell(List<double> prices)
        {
            if (prices.Count < _longPeriod) return false;

            var shortMA = prices.TakeLast(_shortPeriod).Average();
            var longMA = prices.TakeLast(_longPeriod).Average();

            return shortMA < longMA;
        }
    }
}
