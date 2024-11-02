using System;
using System.Collections.Generic;
using System.Linq;

namespace YourNamespace.Classes
{
    public class MeanReversionStrategy : ITradingStrategy
    {
        public string Name => "Mean Reversion";
        private readonly int _lookbackPeriod;
        private readonly double _threshold;

        public MeanReversionStrategy(int lookbackPeriod, double threshold)
        {
            _lookbackPeriod = lookbackPeriod;
            _threshold = threshold;
        }

        public bool ShouldBuy(List<double> prices)
        {
            if (prices.Count < _lookbackPeriod) return false;

            // Calculer la moyenne des prix sur la période de rétrospection
            var averagePrice = prices.TakeLast(_lookbackPeriod).Average();
            var currentPrice = prices.Last();

            // Si le prix actuel est en dessous de la moyenne de plus de _threshold, génère un signal d'achat
            return currentPrice < averagePrice * (1 - _threshold);
        }

        public bool ShouldSell(List<double> prices)
        {
            if (prices.Count < _lookbackPeriod) return false;

            // Calculer la moyenne des prix sur la période de rétrospection
            var averagePrice = prices.TakeLast(_lookbackPeriod).Average();
            var currentPrice = prices.Last();

            // Si le prix actuel est au-dessus de la moyenne de plus de _threshold, génère un signal de vente
            return currentPrice > averagePrice * (1 + _threshold);
        }
    }
}
