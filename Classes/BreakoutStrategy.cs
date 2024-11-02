using System;
using System.Collections.Generic;
using System.Linq;

namespace YourNamespace.Classes
{
    public class BreakoutStrategy : ITradingStrategy
    {
        public string Name => "Breakout";
        private readonly int _lookbackPeriod;

        public BreakoutStrategy(int lookbackPeriod)
        {
            _lookbackPeriod = lookbackPeriod;
        }

        public bool ShouldBuy(List<double> prices)
        {
            if (prices.Count < _lookbackPeriod) return false;

            // Trouver le plus haut prix sur la période de rétrospection
            var highestPrice = prices.TakeLast(_lookbackPeriod).Max();
            var currentPrice = prices.Last();

            // Génère un signal d'achat si le prix actuel dépasse le plus haut prix
            return currentPrice > highestPrice;
        }

        public bool ShouldSell(List<double> prices)
        {
            if (prices.Count < _lookbackPeriod) return false;

            // Trouver le plus bas prix sur la période de rétrospection
            var lowestPrice = prices.TakeLast(_lookbackPeriod).Min();
            var currentPrice = prices.Last();

            // Génère un signal de vente si le prix actuel tombe en dessous du plus bas prix
            return currentPrice < lowestPrice;
        }
    }
}
