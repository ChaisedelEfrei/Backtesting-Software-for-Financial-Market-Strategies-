using System;
using System.Collections.Generic;
using YourNamespace.Classes;

namespace YourNamespace.CustomStrategies
{
    public class CustomStrategy
    {
        public string Name { get; set; }  // Nom de la stratégie personnalisée
        public List<ITradingStrategy> Strategies { get; set; } = new List<ITradingStrategy>(); // Liste des stratégies
        public string CombinationRule { get; set; }  // Règle de combinaison (ex. "AND" ou "OR")

        public CustomStrategy(string name, string combinationRule)
        {
            Name = name;
            CombinationRule = combinationRule.ToUpper();
        }

        public void AddStrategy(ITradingStrategy strategy)
        {
            Strategies.Add(strategy);
        }

        public bool ShouldBuy(List<double> prices)
        {
            foreach (var strategy in Strategies)
            {
                bool result = strategy.ShouldBuy(prices);
                if (CombinationRule == "AND" && !result) return false;
                if (CombinationRule == "OR" && result) return true;
            }
            return CombinationRule == "AND";
        }

        public bool ShouldSell(List<double> prices)
        {
            foreach (var strategy in Strategies)
            {
                bool result = strategy.ShouldSell(prices);
                if (CombinationRule == "AND" && !result) return false;
                if (CombinationRule == "OR" && result) return true;
            }
            return CombinationRule == "AND";
        }
    }
}
