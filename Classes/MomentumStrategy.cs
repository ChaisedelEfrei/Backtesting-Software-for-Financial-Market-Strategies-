using System;
using System.Collections.Generic;

namespace YourNamespace.Classes
{
    public class MomentumStrategy : ITradingStrategy
    {
        public string Name => "Momentum Strategy";
        private readonly int _lookbackPeriod;

        public MomentumStrategy(int lookbackPeriod)
        {
            _lookbackPeriod = lookbackPeriod;
        }

        public bool ShouldBuy(List<double> prices)
        {
            if (prices.Count < _lookbackPeriod) return false;

            return prices[^1] > prices[^_lookbackPeriod]; // Prix actuel supérieur à celui d'il y a _lookbackPeriod jours
        }

        public bool ShouldSell(List<double> prices)
        {
            if (prices.Count < _lookbackPeriod) return false;

            return prices[^1] < prices[^_lookbackPeriod]; // Prix actuel inférieur à celui d'il y a _lookbackPeriod jours
        }
    }
}
