using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourNamespace.Classes;
using YourNamespace.Managers;
using YourNamespace.CustomStrategies;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradingController : ControllerBase
    {
        private readonly AlphaVantageService _alphaVantageService;

        public TradingController(AlphaVantageService alphaVantageService)
        {
            _alphaVantageService = alphaVantageService;
        }

        // Endpoint GET pour exécuter la stratégie de Momentum
        [HttpGet("momentum")]
        public async Task<IActionResult> ExecuteMomentumStrategy([FromQuery] string symbol, [FromQuery] int lookbackPeriod, [FromQuery] string timeframe = "daily")
        {
            try
            {
                var prices = await _alphaVantageService.GetClosingPricesAsync(symbol, timeframe);

                if (prices.Count == 0)
                {
                    return BadRequest("No price data available for the specified symbol.");
                }

                var momentumStrategy = new MomentumStrategy(lookbackPeriod);
                bool buySignal = momentumStrategy.ShouldBuy(prices);
                bool sellSignal = momentumStrategy.ShouldSell(prices);

                return Ok(new
                {
                    Strategy = "Momentum",
                    Symbol = symbol,
                    LookbackPeriod = lookbackPeriod,
                    Timeframe = timeframe,
                    BuySignal = buySignal,
                    SellSignal = sellSignal
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching data for {symbol}: {ex.Message}");
            }
        }

        // Endpoint GET pour exécuter la stratégie de Moving Average Crossover
        [HttpGet("movingAverageCrossover")]
        public async Task<IActionResult> ExecuteMovingAverageCrossoverStrategy([FromQuery] string symbol, [FromQuery] int shortPeriod, [FromQuery] int longPeriod, [FromQuery] string timeframe = "daily")
        {
            try
            {
                var prices = await _alphaVantageService.GetClosingPricesAsync(symbol, timeframe);

                if (prices.Count == 0)
                {
                    return BadRequest("No price data available for the specified symbol.");
                }

                var movingAverageCrossoverStrategy = new MovingAverageCrossoverStrategy(shortPeriod, longPeriod);
                bool buySignal = movingAverageCrossoverStrategy.ShouldBuy(prices);
                bool sellSignal = movingAverageCrossoverStrategy.ShouldSell(prices);

                return Ok(new
                {
                    Strategy = "Moving Average Crossover",
                    Symbol = symbol,
                    ShortPeriod = shortPeriod,
                    LongPeriod = longPeriod,
                    Timeframe = timeframe,
                    BuySignal = buySignal,
                    SellSignal = sellSignal
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching data for {symbol}: {ex.Message}");
            }
        }

        // Endpoint GET pour exécuter la stratégie de Mean Reversion
        [HttpGet("meanReversion")]
        public async Task<IActionResult> ExecuteMeanReversionStrategy([FromQuery] string symbol, [FromQuery] int lookbackPeriod, [FromQuery] double threshold, [FromQuery] string timeframe = "daily")
        {
            try
            {
                var prices = await _alphaVantageService.GetClosingPricesAsync(symbol, timeframe);

                if (prices.Count == 0)
                {
                    return BadRequest("No price data available for the specified symbol.");
                }

                var meanReversionStrategy = new MeanReversionStrategy(lookbackPeriod, threshold);
                bool buySignal = meanReversionStrategy.ShouldBuy(prices);
                bool sellSignal = meanReversionStrategy.ShouldSell(prices);

                return Ok(new
                {
                    Strategy = "Mean Reversion",
                    Symbol = symbol,
                    LookbackPeriod = lookbackPeriod,
                    Threshold = threshold,
                    Timeframe = timeframe,
                    BuySignal = buySignal,
                    SellSignal = sellSignal
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching data for {symbol}: {ex.Message}");
            }
        }

        // Endpoint GET pour exécuter la stratégie de Breakout
        [HttpGet("breakout")]
        public async Task<IActionResult> ExecuteBreakoutStrategy([FromQuery] string symbol, [FromQuery] int lookbackPeriod, [FromQuery] string timeframe = "daily")
        {
            try
            {
                var prices = await _alphaVantageService.GetClosingPricesAsync(symbol, timeframe);

                if (prices.Count == 0)
                {
                    return BadRequest("No price data available for the specified symbol.");
                }

                var breakoutStrategy = new BreakoutStrategy(lookbackPeriod);
                bool buySignal = breakoutStrategy.ShouldBuy(prices);
                bool sellSignal = breakoutStrategy.ShouldSell(prices);

                return Ok(new
                {
                    Strategy = "Breakout",
                    Symbol = symbol,
                    LookbackPeriod = lookbackPeriod,
                    Timeframe = timeframe,
                    BuySignal = buySignal,
                    SellSignal = sellSignal
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching data for {symbol}: {ex.Message}");
            }
        }

        // Endpoint POST pour créer et exécuter une stratégie personnalisée
        [HttpPost("createCustomStrategy")]
        public async Task<IActionResult> CreateCustomStrategy([FromBody] CustomStrategyRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.CombinationRule))
            {
                return BadRequest("Name and CombinationRule are required.");
            }

            var customStrategy = new CustomStrategy(request.Name, request.CombinationRule);

            if (request.Strategies == null)
            {
                return BadRequest("Strategies are required.");
            }

            foreach (var strategyRequest in request.Strategies)
            {
                ITradingStrategy? strategy = strategyRequest.Type switch
                {
                    "momentum" => strategyRequest.LookbackPeriod.HasValue 
                        ? new MomentumStrategy(strategyRequest.LookbackPeriod.Value) 
                        : null,
                    "movingAverageCrossover" => (strategyRequest.ShortPeriod.HasValue && strategyRequest.LongPeriod.HasValue)
                        ? new MovingAverageCrossoverStrategy(strategyRequest.ShortPeriod.Value, strategyRequest.LongPeriod.Value)
                        : null,
                    "meanReversion" => (strategyRequest.LookbackPeriod.HasValue && strategyRequest.Threshold.HasValue)
                        ? new MeanReversionStrategy(strategyRequest.LookbackPeriod.Value, strategyRequest.Threshold.Value)
                        : null,
                    "breakout" => strategyRequest.LookbackPeriod.HasValue
                        ? new BreakoutStrategy(strategyRequest.LookbackPeriod.Value)
                        : null,
                    _ => null
                };
                
                if (strategy == null)
                {
                    return BadRequest($"Invalid or missing parameters for strategy type: {strategyRequest.Type}");
                }
                
                customStrategy.AddStrategy(strategy!);
            }

            if (string.IsNullOrEmpty(request.Symbol))
            {
                return BadRequest("Symbol is required.");
            }

            var prices = await _alphaVantageService.GetClosingPricesAsync(request.Symbol, request.Timeframe ?? "daily");
            if (prices.Count == 0) return BadRequest("No price data available for the specified symbol.");

            bool buySignal = customStrategy.ShouldBuy(prices);
            bool sellSignal = customStrategy.ShouldSell(prices);

            return Ok(new
            {
                Strategy = customStrategy.Name,
                CombinationRule = customStrategy.CombinationRule,
                Symbol = request.Symbol,
                Timeframe = request.Timeframe,
                BuySignal = buySignal,
                SellSignal = sellSignal
            });
        }
    }

    // Modèle de requête pour la stratégie personnalisée
    public class CustomStrategyRequest
    {
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public string? CombinationRule { get; set; } // AND ou OR
        public string? Timeframe { get; set; } = "daily"; // Choix du timeframe
        public List<StrategyRequest>? Strategies { get; set; }
    }

    public class StrategyRequest
    {
        public string? Type { get; set; } // Type de la stratégie : momentum, movingAverageCrossover, etc.
        public int? LookbackPeriod { get; set; }
        public int? ShortPeriod { get; set; }
        public int? LongPeriod { get; set; }
        public double? Threshold { get; set; }
    }
}
