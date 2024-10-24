using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

public class PriceData
{
    public string? date { get; set; }
    public string? close_price { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        string json = File.ReadAllText("historical_closing_prices.json");
        List<PriceData> prices = JsonConvert.DeserializeObject<List<PriceData>>(json);
        string json2 = File.ReadAllText("Coca_prices.json");
        List<PriceData> Coca_prices = JsonConvert.DeserializeObject<List<PriceData>>(json2);
        string json3 = File.ReadAllText("Pepsi_prices.json");
        List<PriceData> Pepsi_prices = JsonConvert.DeserializeObject<List<PriceData>>(json3);

        Console.WriteLine("Que voulez-vous faire ?");
        Console.WriteLine("Moving Average Crossover Strategy");
        Console.WriteLine("1. Utiliser la stratégie SMA");
        Console.WriteLine("2. Utiliser la stratégie EMA");
        Console.WriteLine("3. Utiliser la stratégie WMA");
        Console.WriteLine("4. Utiliser la stratégie SMA avec stop suiveur");
        
        Console.WriteLine("5. Utiliser la stratégie Mean Reversion");

        Console.WriteLine("6. Utiliser la stratégie Momentum");
        Console.WriteLine("7. Utiliser la stratégie de confirmation de tendance (Combine momentum signals with a trend indicator (e.g., moving averages) to confirm trends.)");

        Console.WriteLine("8. Utiliser la stratégie Breakout");
        Console.WriteLine("9. Utiliser la stratégie Pair Trading");

        Console.Write("Entrez votre choix (1-9) : ");

        int choix = int.Parse(Console.ReadLine());

        switch (choix)
        {
            case 1:
                Console.Write("Entrez la période de la moyenne mobile court terme: ");
                int shortTermPeriod = int.Parse(Console.ReadLine());
                Console.Write("Entrez la période de la moyenne mobile long terme: ");
                int longTermPeriod = int.Parse(Console.ReadLine());
                Console.WriteLine("\nRésultats avec SMA:");
                BacktestMovingAverageCrossover(prices, shortTermPeriod, longTermPeriod);
                break;
            case 2:
                Console.Write("Entrez la période de la moyenne mobile court terme: ");
                shortTermPeriod = int.Parse(Console.ReadLine());
                Console.Write("Entrez la période de la moyenne mobile long terme: ");
                longTermPeriod = int.Parse(Console.ReadLine());
                Console.WriteLine("\nRésultats avec EMA:");
                BacktestEMACrossover(prices, shortTermPeriod, longTermPeriod);
                break;
            case 3:
                Console.Write("Entrez la période de la moyenne mobile court terme: ");
                shortTermPeriod = int.Parse(Console.ReadLine());
                Console.Write("Entrez la période de la moyenne mobile long terme: ");
                longTermPeriod = int.Parse(Console.ReadLine());
                Console.WriteLine("\nRésultats avec WMA:");
                BacktestWMACrossover(prices, shortTermPeriod, longTermPeriod);
                break;
            case 4:
                Console.Write("Entrez la période de la moyenne mobile court terme: ");
                shortTermPeriod = int.Parse(Console.ReadLine());
                Console.Write("Entrez la période de la moyenne mobile long terme: ");
                longTermPeriod = int.Parse(Console.ReadLine());
                Console.Write("Entrez le pourcentage du stop suiveur (ex: 0.02 pour 2%): ");
                double trailingStopPercentage = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
                Console.WriteLine("\nRésultats avec SMA et stop suiveur:");
                BacktestWithTrailingStop(prices, shortTermPeriod, longTermPeriod, trailingStopPercentage);
                break;
            case 5:
                Console.WriteLine("\nRésultats de la stratégie Mean Reversion Strategy:");
                Console.WriteLine(MeanReversionStrategy(prices));
                break;
            case 6:
                Console.WriteLine("\nRésultats de la stratégie Momentum Strategy:");
                Console.WriteLine(MomentumStrategy(prices));
                break;
            case 7:
                Console.WriteLine("\nRésultats de la stratégie de confirmation de tendance :");
                Console.WriteLine(TrendConfirmationStrategy(prices));
                break;
            case 8:
                Console.WriteLine("\nRésultats de la stratégie Breakout Strategy:");
                Console.WriteLine(BreakoutStrategyWithSignals(prices));
                break;
            case 9:
                Console.WriteLine("\nRésultats de la stratégie Pair Trading Strategy:");
                Console.WriteLine(PairTradingStrategy(Coca_prices, Pepsi_prices));
                break;
            default:
                Console.WriteLine("Choix invalide. Veuillez choisir un nombre entre 1 et 8.");
                break;
        }

    }
    //------------------------5eme Pair Trading (Statistical Arbitrage) Strategy-----------------------//
    //Most know assets with stong correlation in trading paiirs : Coca-Cola and Pepsi.
    //Calculate the spread between the two assets.
    public static List<double> CalculateSpread(List<PriceData> asset1Prices, List<PriceData> asset2Prices)
    {
        if (asset1Prices == null || asset2Prices == null || asset1Prices.Count != asset2Prices.Count)
        {
            throw new ArgumentException("Les données de prix pour les deux actifs doivent être fournies et avoir la même longueur.");
        }

        List<double> asset1ClosePrices = asset1Prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();
        List<double> asset2ClosePrices = asset2Prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();

        List<double> spread = new List<double>();

        for (int i = 0; i < asset1ClosePrices.Count; i++)
        {
            spread.Add(asset1ClosePrices[i] - asset2ClosePrices[i]);
        }
        return spread;
    }
    // Calculate the mean and standard deviation of the spread
    public static (double mean, double stddev) CalculateMeanAndStdDev(List<double> spread)
    {
        double mean = spread.Average();
        double variance = spread.Average(v => Math.Pow(v - mean, 2));
        double stddev = Math.Sqrt(variance);

        return (mean, stddev);
    }
    // Generate signals based on the spread
    public static void GenerateSignals(List<double> spread, double threshold)
    {
        (double mean, double stddev) = CalculateMeanAndStdDev(spread);
        string filePath = "resultatsSignaux.txt";
        
        for (int i = 0; i < spread.Count; i++)
        {
            double currentSpread = spread[i];

            if (currentSpread > mean + threshold)  // Signal d'achat
            {
                Console.WriteLine($"Signal d'achat à l'index {i}: Écart = {currentSpread:F2}, Moyenne = {mean:F2}");
            }
            else if (currentSpread < mean - threshold)  // Signal de vente (réversion à la moyenne)
            {
                Console.WriteLine($"Signal de vente à l'index {i}: Écart = {currentSpread:F2}, Moyenne = {mean:F2}");
            }
        }
    }
    //Backtest the Pair Trading Strategy
    public static string PairTradingStrategy(List<PriceData> asset1Prices, List<PriceData> asset2Prices)
    {
        if (asset1Prices == null || asset2Prices == null || asset1Prices.Count != asset2Prices.Count)
        {
            return "Les données de prix pour les deux actifs doivent être fournies et avoir la même longueur.";
        }

        List<double> spread = CalculateSpread(asset1Prices, asset2Prices);
        GenerateSignals(spread, 1.5);
        return "Pair trading strategy applied successfully.";
    }


    //------------------------4eme Breakout Strategy------------------------//
    //Identify Trading Range, Use historical price data to identify key support (low price) and resistance (high price) levels.
    public static (double Support, double Resistance) IdentifyTradingRange(List<PriceData> prices)
    {
        if (prices == null || prices.Count < 20)
        {
            throw new ArgumentException("Not enough price data to identify trading range");
        }

        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();

        double highestPrice = closePrices.Max();
        double lowestPrice = closePrices.Min();

        double range = highestPrice - lowestPrice;
        double support = lowestPrice + (range * 0.3);
        double resistance = highestPrice - (range * 0.3);
        //Ces calculs créent une "zone tampon" autour des prix extrêmes. Voici pourquoi c'est important :
        //a. Support : Au lieu d'utiliser simplement le prix le plus bas comme support, vous ajoutez 30% du range. Cela place le support légèrement au-dessus du minimum absolu, ce qui peut être plus représentatif d'un niveau de support réel dans des conditions de marché typiques.
        //b. Résistance : De même, vous soustrayez 30% du range du prix le plus haut pour la résistance. Cela place la résistance un peu en dessous du maximum absolu.

        return (support, resistance);
    }
    //Define Breakout Condition: A breakout occurs when the price closes above the resistance level (bullish) or below the support level (bearish).
    public static string BreakoutStrategyWithSignals(List<PriceData> prices)
    {
        if (prices == null || prices.Count < 20)
        {
            return "Not enough price data to apply breakout strategy.";
        }

        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();

        (double support, double resistance) = IdentifyTradingRange(prices);

        for (int i = 0; i < closePrices.Count; i++)
        {
            double currentPrice = closePrices[i]; 

            if (currentPrice > resistance) 
            {
                Console.WriteLine($"Breakout: Price closed above resistance level ({resistance:F2}). Consider buying or going long.");
            }
            else if (currentPrice < support)
            {
                Console.WriteLine($"Breakout: Price closed below support level ({support:F2}). Consider selling or going short.");
            }
            else
            {
                Console.WriteLine("No breakout detected. Price remains within the trading range.");
            }
        }
        return "Breakout strategy applied successfully.";
    }
    //Generate Signals: Buy Signal: When the asset breaks above the resistance. Sell Signal: When the asset breaks below the support.
    // public static string BreakoutStrategyWithSignals(List<PriceData> prices)
    // {
    //     if (prices == null || prices.Count < 20)
    //     {
    //         return "Not enough price data to apply breakout strategy.";
    //     }

    //     List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();

    //     (double support, double resistance) = IdentifyTradingRange(prices);

    //     double currentPrice = closePrices.Last();
    //     double previousPrice = closePrices[closePrices.Count - 2];

    //     if (currentPrice > resistance && previousPrice < resistance)
    //     {
    //         return $"Buy Signal: Price broke above resistance level ({resistance:F2}). Consider buying or going long.";
    //     }
    //     else if (currentPrice < support && previousPrice > support)
    //     {
    //         return $"Sell Signal: Price broke below support level ({support:F2}). Consider selling or going short.";
    //     }
    //     else
    //     {
    //         return "No clear signal detected. Price remains within the trading range.";
    //     }
    // }

    //--------------------------3eme stratégie Momentum Strategy--------------------------
    //ROC method
    public static double CalculateROC(List<double> prices)
    {
        if (prices == null || prices.Count <= 14)
        {
            throw new ArgumentException("Not enough price data for the specified period");
        }

        double currentPrice = prices[prices.Count - 1];
        double priceNPeriodsAgo = prices[prices.Count - 1 - 14];

        double roc = ((currentPrice - priceNPeriodsAgo) / priceNPeriodsAgo) * 100;

        return roc;
    }
    //Moving Average Convergence Divergence (MACD) method
    public static double CalculateMACD(List<double> prices, int shortTermPeriod = 12, int longTermPeriod = 26)
    {
        if (prices == null || prices.Count <= longTermPeriod)
        {
            throw new ArgumentException("Not enough price data for the specified period");
        }

        double shortTermEMA = CalculateEMA(prices, prices.Count - 1, shortTermPeriod);
        double longTermEMA = CalculateEMA(prices, prices.Count - 1, longTermPeriod);

        double macd = shortTermEMA - longTermEMA;

        return macd;
    }
    //Momentum Strategy method
    public static string MomentumStrategy(List<PriceData> prices)
    {
        if (prices == null || prices.Count < 26)
        {
            return "Données insuffisantes pour appliquer la stratégie de momentum.";
        }

        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();

        double roc = CalculateROC(closePrices);
        double macd = CalculateMACD(closePrices);
        double previousROC = CalculateROC(closePrices.Take(closePrices.Count - 1).ToList());
        double previousMACD = CalculateMACD(closePrices.Take(closePrices.Count - 1).ToList());
        string signal = "";
        // Conditions pour le signal d'achat
        if ((roc > previousROC && roc > 0) || (macd > previousMACD && macd > 0))
        {
            signal = "Signal d'achat : Le momentum augmente.";
        }
        // Conditions pour le signal de vente
        else if ((roc < previousROC && roc < 0) || (macd < previousMACD && macd < 0))
        {
            signal = "Signal de vente : Le momentum s'affaiblit.";
        }
        else
        {
            signal = "Pas de signal clair. Surveillez le marché.";
        }
        return $"ROC actuel : {roc:F2}, MACD actuel : {macd:F2}\n{signal}";
    }


    
    public static double CalculateSMA(List<double> prices, int period){
        if (prices == null || prices.Count < period)
        {
            throw new ArgumentException("Données insuffisantes pour calculer la SMA");
        }
        return prices.TakeLast(period).Average();
    }

    //Potential Enhancements: Trend Confirmation
    public static string TrendConfirmationStrategy(List<PriceData> prices, int smaPeriod = 50)
    {
        if (prices == null || prices.Count < Math.Max(26, smaPeriod))
        {
            return "Données insuffisantes pour appliquer la stratégie de confirmation de tendance.";
        }

        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();

        double roc = CalculateROC(closePrices);
        double macd = CalculateMACD(closePrices);
        double sma = CalculateSMA(closePrices, smaPeriod);
        double currentPrice = closePrices.Last();

        string momentumSignal = "";
        string trendSignal = "";
        string finalSignal = "";

        if (roc > 0 && macd > 0)
        {
            momentumSignal = "Momentum haussier";
        }
        else if (roc < 0 && macd < 0)
        {
            momentumSignal = "Momentum baissier";
        }
        else{
            momentumSignal = "Momentum neutre";
        }

        // Évaluation de la tendance
        if (currentPrice > sma)
        {
            trendSignal = "Tendance haussière";
        }
        else if (currentPrice < sma)
        {
            trendSignal = "Tendance baissière";
        }
        else
        {
            trendSignal = "Pas de tendance claire";
        }

        // Combinaison des signaux
        if (momentumSignal == "Momentum haussier" && trendSignal == "Tendance haussière")
        {
            finalSignal = "Signal d'achat fort : Momentum haussier confirmé par la tendance haussière.";
        }
        else if (momentumSignal == "Momentum baissier" && trendSignal == "Tendance baissière")
        {
            finalSignal = "Signal de vente fort : Momentum baissier confirmé par la tendance baissière.";
        }
        else
        {
            finalSignal = "Pas de signal clair. Le momentum et la tendance ne sont pas alignés.";
        }
        return $"ROC : {roc:F2}, MACD : {macd:F2}, SMA({smaPeriod}) : {sma:F2}\n{momentumSignal}, {trendSignal}\n{finalSignal}";
    }
    


    //-------------------------2eme stratégie Mean Reversion Strategy (j'ai fait double confirmation seulement dans Potential Enhancements, aucun Additional Considerations)-------------------------
    static double RSI(List<PriceData> prices) {
        double rs = 0;
        int period = 14;
        int startIndex = prices.Count - period; // Utilise 'Count' pour obtenir la taille de la liste
        List<double> Average_earnings = new List<double>();
        List<double> Average_losses = new List<double>();

        for (int i = startIndex; i < prices.Count - 1; i++) { // Assurez-vous de ne pas dépasser les limites
            double currentPrice = double.Parse(prices[i].close_price.Replace(",", "."), CultureInfo.InvariantCulture);
            double nextPrice = double.Parse(prices[i + 1].close_price.Replace(",", "."), CultureInfo.InvariantCulture);

            if (nextPrice - currentPrice > 0) {
                Average_earnings.Add(nextPrice - currentPrice);
            } else {
                Average_losses.Add(currentPrice - nextPrice);
            }
        }

        double average_earnings = Average_earnings.Sum() / period;
        double average_losses = Average_losses.Sum() / period;
        double RSI = 100 - (100 / (1 + (average_earnings / average_losses)));
        if(RSI > 70) {
            Console.WriteLine("RSI > 70 -> You should sell or go short.");
        } else if(RSI < 30) {
            Console.WriteLine("RSI < 30 -> You should buy or go long.");
        }
        return RSI;
    }

    public static (double SMA, double UpperBand, double LowerBand) BollingerBands(List<PriceData> prices, int period = 20, double numStdDev = 2)
    {
        if (prices == null || prices.Count < period)
            throw new ArgumentException("Données de prix insuffisantes");
        int startIndex = prices.Count - period;
        // Calculer la SMA
        double SMA = prices.Skip(startIndex)
                           .Select(p => Convert.ToDouble(p.close_price, CultureInfo.InvariantCulture))
                           .Average();
        // Calculer l'écart-type
        double variance = prices.Skip(startIndex)
                                .Select(p => Convert.ToDouble(p.close_price, CultureInfo.InvariantCulture))
                                .Average(price => Math.Pow(price - SMA, 2));
        double standardDeviation = Math.Sqrt(variance);
        // Calculer les bandes
        double upperBand = SMA + (numStdDev * standardDeviation);
        double lowerBand = SMA - (numStdDev * standardDeviation);
        // Optionnel : Analyser le prix actuel
        double currentPrice = Convert.ToDouble(prices[prices.Count - 1].close_price, CultureInfo.InvariantCulture);
        if (currentPrice > upperBand)
        {
            Console.WriteLine("Le prix est au-dessus de la bande supérieure. Envisagez de vendre ou de prendre une position courte.");
        }
        else if (currentPrice < lowerBand)
        {
            Console.WriteLine("Le prix est en dessous de la bande inférieure. Envisagez d'acheter ou de prendre une position longue.");
        }
        return (SMA, upperBand, lowerBand);
    }
    static string MeanReversionStrategy(List<PriceData> prices) {
        double RSI_value = RSI(prices);
        double BollingerBands_value = BollingerBands(prices).SMA;
        if(RSI_value > 70 && BollingerBands_value > 0) {
            return "RSI value is greater than 70 and the price is above the upper band. You should sell or go short.";
        } else if(RSI_value < 30 && BollingerBands_value < 0) {
            return "RSI value is less than 30 and the price is below the lower band. You should buy or go long.";
        } else {
            return "No action required.";
        }
    }



    // 1ere stratgie Moving Average Crossover Strategy
    static double CalculateMovingAverage(List<double> prices, int index, int period)
    {
        double sum = 0;
        int startIndex = Math.Max(0, index - period + 1);
        for (int i = startIndex; i <= index; i++)
        {
            sum += prices[i];
        }
        return sum / (index - startIndex + 1);
    }

    static double CalculateEMA(List<double> prices, int index, int period)
    {
        if (index < period - 1)
            return CalculateMovingAverage(prices, index, index + 1);

        double multiplier = 2.0 / (period + 1);
        double ema = prices[index - period + 1];

        for (int i = index - period + 2; i <= index; i++)
        {
            ema = (prices[i] - ema) * multiplier + ema;
        }

        return ema;
    }

    static double CalculateWMA(List<double> prices, int index, int period)
    {
        double sum = 0;
        double weightSum = 0;
        int startIndex = Math.Max(0, index - period + 1);

        for (int i = startIndex, weight = 1; i <= index; i++, weight++)
        {
            sum += prices[i] * weight;
            weightSum += weight;
        }

        return sum / weightSum;
    }

    static void BacktestMovingAverageCrossover(List<PriceData> prices, int shortTermPeriod, int longTermPeriod)
    {
        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();
        
        double startingCapital = 10000; 
        double capital = startingCapital;
        double position = 0; // Nombre d'actions détenues
        double fees = 0.001; // Frais de transaction (0.1%)
        
        double? previousShortTermMA = null;
        double? previousLongTermMA = null;
        List<Trade> trades = new List<Trade>(); // Pour suivre tous les trades

        for (int i = longTermPeriod - 1; i < closePrices.Count; i++)
        {
            double shortTermMA = CalculateMovingAverage(closePrices, i, shortTermPeriod);
            double longTermMA = CalculateMovingAverage(closePrices, i, longTermPeriod);
            double currentPrice = closePrices[i];

            if (previousShortTermMA.HasValue && previousLongTermMA.HasValue)
            {
                // Signal d'achat (Golden Cross)
                if (shortTermMA > longTermMA && previousShortTermMA < previousLongTermMA)
                {
                    if (position == 0) // Si nous ne sommes pas déjà en position
                    {
                        double shareAmount = Math.Floor(capital / currentPrice); // Nombre d'actions qu'on peut acheter
                        double cost = shareAmount * currentPrice;
                        double transactionFees = cost * fees;
                        
                        if (cost + transactionFees <= capital)
                        {
                            position = shareAmount;
                            capital -= (cost + transactionFees);
                            
                            trades.Add(new Trade
                            {
                                Date = prices[i].date,
                                Type = "BUY",
                                Price = currentPrice,
                                Shares = shareAmount,
                                Cost = cost,
                                Fees = transactionFees
                            });
                            
                            Console.WriteLine($"Golden Cross (Date: {prices[i].date}) - Achat de {shareAmount} actions à {currentPrice:F2}$");
                        }
                    }
                }
                // Signal de vente (Death Cross)
                else if (shortTermMA < longTermMA && previousShortTermMA > previousLongTermMA)
                {
                    if (position > 0) // Si nous avons une position à vendre
                    {
                        double saleAmount = position * currentPrice;
                        double transactionFees = saleAmount * fees;
                        capital += (saleAmount - transactionFees);
                        
                        trades.Add(new Trade
                        {
                            Date = prices[i].date,
                            Type = "SELL",
                            Price = currentPrice,
                            Shares = position,
                            Cost = saleAmount,
                            Fees = transactionFees
                        });
                        
                        Console.WriteLine($"Death Cross (Date: {prices[i].date}) - Vente de {position} actions à {currentPrice:F2}$");
                        position = 0;
                    }
                }
            }

            previousShortTermMA = shortTermMA;
            previousLongTermMA = longTermMA;
        }
        // Liquider la position finale si nécessaire
        if (position > 0)
        {
            double finalPrice = closePrices[closePrices.Count - 1];
            double saleAmount = position * finalPrice;
            double transactionFees = saleAmount * fees;
            capital += (saleAmount - transactionFees);
        }

        // Calculer et afficher les statistiques
        double totalReturn = (capital - startingCapital) / startingCapital * 100;
        double totalFees = trades.Sum(t => t.Fees);
        int numberOfTrades = trades.Count;

        Console.WriteLine("\nRésultats du Backtesting:");
        Console.WriteLine($"Capital initial: {startingCapital:F2}$");
        Console.WriteLine($"Capital final: {capital:F2}$");
        Console.WriteLine($"Rendement total: {totalReturn:F2}%");
        Console.WriteLine($"Nombre de trades: {numberOfTrades}");
        Console.WriteLine($"Total des frais de transaction: {totalFees:F2}$");
    }
    static void BacktestEMACrossover(List<PriceData> prices, int shortTermPeriod, int longTermPeriod)
    {
        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();
        
        // Paramètres de trading
        double startingCapital = 10000; // Capital initial
        double capital = startingCapital;
        double position = 0; // Nombre d'actions détenues
        double fees = 0.001; // Frais de transaction (0.1%)
        
        double? previousShortTermEMA = null;
        double? previousLongTermEMA = null;
        List<Trade> trades = new List<Trade>();

        for (int i = longTermPeriod - 1; i < closePrices.Count; i++)
        {
            double shortTermEMA = CalculateEMA(closePrices, i, shortTermPeriod);
            double longTermEMA = CalculateEMA(closePrices, i, longTermPeriod);
            double currentPrice = closePrices[i];

            if (previousShortTermEMA.HasValue && previousLongTermEMA.HasValue)
            {
                // Signal d'achat (Golden Cross)
                if (shortTermEMA > longTermEMA && previousShortTermEMA < previousLongTermEMA)
                {
                    if (position == 0)
                    {
                        double shareAmount = Math.Floor(capital / currentPrice);
                        double cost = shareAmount * currentPrice;
                        double transactionFees = cost * fees;
                        
                        if (cost + transactionFees <= capital)
                        {
                            position = shareAmount;
                            capital -= (cost + transactionFees);
                            
                            trades.Add(new Trade
                            {
                                Date = prices[i].date,
                                Type = "BUY",
                                Price = currentPrice,
                                Shares = shareAmount,
                                Cost = cost,
                                Fees = transactionFees
                            });
                            
                            Console.WriteLine($"Golden Cross EMA (Date: {prices[i].date}) - Achat de {shareAmount} actions à {currentPrice:F2}$");
                        }
                    }
                }
                // Signal de vente (Death Cross)
                else if (shortTermEMA < longTermEMA && previousShortTermEMA > previousLongTermEMA)
                {
                    if (position > 0)
                    {
                        double saleAmount = position * currentPrice;
                        double transactionFees = saleAmount * fees;
                        capital += (saleAmount - transactionFees);
                        
                        trades.Add(new Trade
                        {
                            Date = prices[i].date,
                            Type = "SELL",
                            Price = currentPrice,
                            Shares = position,
                            Cost = saleAmount,
                            Fees = transactionFees
                        });
                        
                        Console.WriteLine($"Death Cross EMA (Date: {prices[i].date}) - Vente de {position} actions à {currentPrice:F2}$");
                        position = 0;
                    }
                }
            }

            previousShortTermEMA = shortTermEMA;
            previousLongTermEMA = longTermEMA;
        }

        // Liquider la position finale si nécessaire
        if (position > 0)
        {
            double finalPrice = closePrices[closePrices.Count - 1];
            double saleAmount = position * finalPrice;
            double transactionFees = saleAmount * fees;
            capital += (saleAmount - transactionFees);
        }

        // Calculer et afficher les statistiques
        double totalReturn = (capital - startingCapital) / startingCapital * 100;
        double totalFees = trades.Sum(t => t.Fees);
        int numberOfTrades = trades.Count;

        Console.WriteLine("\nRésultats du Backtesting EMA:");
        Console.WriteLine($"Capital initial: {startingCapital:F2}$");
        Console.WriteLine($"Capital final: {capital:F2}$");
        Console.WriteLine($"Rendement total: {totalReturn:F2}%");
        Console.WriteLine($"Nombre de trades: {numberOfTrades}");
        Console.WriteLine($"Total des frais de transaction: {totalFees:F2}$");
    }

    static void BacktestWMACrossover(List<PriceData> prices, int shortTermPeriod, int longTermPeriod)
    {
        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();
        
        // Paramètres de trading
        double startingCapital = 10000; // Capital initial
        double capital = startingCapital;
        double position = 0; // Nombre d'actions détenues
        double fees = 0.001; // Frais de transaction (0.1%)
        
        double? previousShortTermWMA = null;
        double? previousLongTermWMA = null;
        List<Trade> trades = new List<Trade>();

        for (int i = longTermPeriod - 1; i < closePrices.Count; i++)
        {
            double shortTermWMA = CalculateWMA(closePrices, i, shortTermPeriod);
            double longTermWMA = CalculateWMA(closePrices, i, longTermPeriod);
            double currentPrice = closePrices[i];

            if (previousShortTermWMA.HasValue && previousLongTermWMA.HasValue)
            {
                // Signal d'achat (Golden Cross)
                if (shortTermWMA > longTermWMA && previousShortTermWMA < previousLongTermWMA)
                {
                    if (position == 0)
                    {
                        double shareAmount = Math.Floor(capital / currentPrice);
                        double cost = shareAmount * currentPrice;
                        double transactionFees = cost * fees;
                        
                        if (cost + transactionFees <= capital)
                        {
                            position = shareAmount;
                            capital -= (cost + transactionFees);
                            
                            trades.Add(new Trade
                            {
                                Date = prices[i].date,
                                Type = "BUY",
                                Price = currentPrice,
                                Shares = shareAmount,
                                Cost = cost,
                                Fees = transactionFees
                            });
                            
                            Console.WriteLine($"Golden Cross WMA (Date: {prices[i].date}) - Achat de {shareAmount} actions à {currentPrice:F2}$");
                        }
                    }
                }
                // Signal de vente (Death Cross)
                else if (shortTermWMA < longTermWMA && previousShortTermWMA > previousLongTermWMA)
                {
                    if (position > 0)
                    {
                        double saleAmount = position * currentPrice;
                        double transactionFees = saleAmount * fees;
                        capital += (saleAmount - transactionFees);
                        
                        trades.Add(new Trade
                        {
                            Date = prices[i].date,
                            Type = "SELL",
                            Price = currentPrice,
                            Shares = position,
                            Cost = saleAmount,
                            Fees = transactionFees
                        });
                        
                        Console.WriteLine($"Death Cross WMA (Date: {prices[i].date}) - Vente de {position} actions à {currentPrice:F2}$");
                        position = 0;
                    }
                }
            }

            previousShortTermWMA = shortTermWMA;
            previousLongTermWMA = longTermWMA;
        }

        // Liquider la position finale si nécessaire
        if (position > 0)
        {
            double finalPrice = closePrices[closePrices.Count - 1];
            double saleAmount = position * finalPrice;
            double transactionFees = saleAmount * fees;
            capital += (saleAmount - transactionFees);
        }

        // Calculer et afficher les statistiques
        double totalReturn = (capital - startingCapital) / startingCapital * 100;
        double totalFees = trades.Sum(t => t.Fees);
        int numberOfTrades = trades.Count;

        Console.WriteLine("\nRésultats du Backtesting WMA:");
        Console.WriteLine($"Capital initial: {startingCapital:F2}$");
        Console.WriteLine($"Capital final: {capital:F2}$");
        Console.WriteLine($"Rendement total: {totalReturn:F2}%");
        Console.WriteLine($"Nombre de trades: {numberOfTrades}");
        Console.WriteLine($"Total des frais de transaction: {totalFees:F2}$");
    }

    static void BacktestWithTrailingStop(List<PriceData> prices, int shortTermPeriod, int longTermPeriod, double trailingStopPercentage)
    {
        List<double> closePrices = prices.Select(p => double.Parse(p.close_price.Replace(",", "."), CultureInfo.InvariantCulture)).ToList();
        
        // Paramètres de trading
        double startingCapital = 10000;
        double capital = startingCapital;
        double fees = 0.001; // 0.1% frais de transaction
        
        double? previousShortTermMA = null;
        double? previousLongTermMA = null;
        bool inPosition = false;
        double position = 0;
        double entryPrice = 0;
        double highestPrice = 0;
        List<Trade> trades = new List<Trade>();
        double maxDrawdown = 0;
        double peakCapital = startingCapital;

        for (int i = longTermPeriod - 1; i < closePrices.Count; i++)
        {
            double shortTermMA = CalculateMovingAverage(closePrices, i, shortTermPeriod);
            double longTermMA = CalculateMovingAverage(closePrices, i, longTermPeriod);
            double currentPrice = closePrices[i];

            // Mettre à jour le drawdown
            if (capital > peakCapital)
            {
                peakCapital = capital;
            }
            double currentDrawdown = (peakCapital - capital) / peakCapital;
            maxDrawdown = Math.Max(maxDrawdown, currentDrawdown);

            if (previousShortTermMA.HasValue && previousLongTermMA.HasValue)
            {
                // Signal d'achat (Golden Cross)
                if (!inPosition && shortTermMA > longTermMA && previousShortTermMA < previousLongTermMA)
                {
                    double shareAmount = Math.Floor(capital / currentPrice);
                    double cost = shareAmount * currentPrice;
                    double transactionFees = cost * fees;

                    if (cost + transactionFees <= capital)
                    {
                        position = shareAmount;
                        entryPrice = currentPrice;
                        highestPrice = currentPrice;
                        capital -= (cost + transactionFees);
                        inPosition = true;

                        trades.Add(new Trade
                        {
                            Date = prices[i].date,
                            Type = "BUY",
                            Price = currentPrice,
                            Shares = shareAmount,
                            Cost = cost,
                            Fees = transactionFees
                        });

                        Console.WriteLine($"Golden Cross (Date: {prices[i].date}) - Achat de {shareAmount} actions à {currentPrice:F2}$");
                    }
                }
                else if (inPosition)
                {
                    // Mettre à jour le prix le plus haut atteint
                    if (currentPrice > highestPrice)
                    {
                        highestPrice = currentPrice;
                    }

                    double stopPrice = highestPrice * (1 - trailingStopPercentage);
                    bool stopTriggered = currentPrice <= stopPrice;
                    bool sellSignal = shortTermMA < longTermMA && previousShortTermMA > previousLongTermMA;

                    // Vendre si le stop est déclenché ou sur un signal de vente
                    if (stopTriggered || sellSignal)
                    {
                        double saleAmount = position * currentPrice;
                        double transactionFees = saleAmount * fees;
                        capital += (saleAmount - transactionFees);

                        trades.Add(new Trade
                        {
                            Date = prices[i].date,
                            Type = "SELL",
                            Price = currentPrice,
                            Shares = position,
                            Cost = saleAmount,
                            Fees = transactionFees,
                            ProfitLoss = ((currentPrice - entryPrice) / entryPrice * 100)
                        });

                        string exitReason = stopTriggered ? "Stop suiveur déclenché" : "Death Cross";
                        Console.WriteLine($"{exitReason} (Date: {prices[i].date}) - Vente de {position} actions à {currentPrice:F2}$");
                        Console.WriteLine($"Profit/Perte sur ce trade: {((currentPrice - entryPrice) / entryPrice * 100):F2}%");

                        inPosition = false;
                        position = 0;
                    }
                }
            }

            previousShortTermMA = shortTermMA;
            previousLongTermMA = longTermMA;
        }

        // Liquider la position finale si nécessaire
        if (inPosition)
        {
            double finalPrice = closePrices[closePrices.Count - 1];
            double saleAmount = position * finalPrice;
            double transactionFees = saleAmount * fees;
            capital += (saleAmount - transactionFees);
        }

        // Calculer les statistiques
        double totalReturn = (capital - startingCapital) / startingCapital * 100;
        double totalFees = trades.Sum(t => t.Fees);
        int numberOfTrades = trades.Count;
        int profitableTrades = trades.Count(t => t.ProfitLoss > 0);
        double winRate = (double)profitableTrades / numberOfTrades * 100;
        double averageProfit = trades.Where(t => t.ProfitLoss > 0).Average(t => t.ProfitLoss);
        double averageLoss = trades.Where(t => t.ProfitLoss <= 0).Average(t => t.ProfitLoss);

        // Afficher les résultats
        Console.WriteLine("\nRésultats du Backtesting avec Stop Suiveur:");
        Console.WriteLine($"Capital initial: {startingCapital:F2}$");
        Console.WriteLine($"Capital final: {capital:F2}$");
        Console.WriteLine($"Rendement total: {totalReturn:F2}%");
        Console.WriteLine($"Nombre total de trades: {numberOfTrades}");
        Console.WriteLine($"Trades gagnants: {profitableTrades} ({winRate:F2}%)");
        Console.WriteLine($"Profit moyen sur trades gagnants: {averageProfit:F2}%");
        Console.WriteLine($"Perte moyenne sur trades perdants: {averageLoss:F2}%");
        Console.WriteLine($"Drawdown maximum: {maxDrawdown * 100:F2}%");
        Console.WriteLine($"Total des frais de transaction: {totalFees:F2}$");
    }

    // Mise à jour de la classe Trade pour inclure le profit/perte
    public class Trade
    {
        public string Date { get; set; }
        public string Type { get; set; }
        public double Price { get; set; }
        public double Shares { get; set; }
        public double Cost { get; set; }
        public double Fees { get; set; }
        public double ProfitLoss { get; set; } // En pourcentage
    }

}
