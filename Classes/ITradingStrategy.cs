using System.Collections.Generic;

namespace YourNamespace.Classes
{
    public interface ITradingStrategy
    {
        string Name { get; }
        bool ShouldBuy(List<double> prices);
        bool ShouldSell(List<double> prices);
    }
}
