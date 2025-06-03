using System;
using System.Collections.Generic;
using System.Linq;
using UnTraid.Core.Indicators.Interface;
using UnTraid.Core.Indicators.Servise;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators.Calculators
{
    /// <summary>
    /// Stochastic Oscillator - Стохастический осциллятор
    /// </summary>
    public class IndicatorStochastic : BaseIndicatorCalculator, IIndicatorCalculator<StochasticResult>
    {
        public string Name => "Stochastic";

        private readonly int _kPeriod;    // Период для %K
        private readonly int _dPeriod;    // Период для %D (сглаживание %K)
        private readonly int _smoothK;    // Сглаживание для %K

        public IndicatorStochastic(List<CandleDTO> candles)
            : base(candles, Properties.Settings.Default.StochasticKPeriod)
        {
            _kPeriod = Properties.Settings.Default.StochasticKPeriod;
            _dPeriod = Properties.Settings.Default.StochasticDPeriod;
            _smoothK = Properties.Settings.Default.StochasticSmoothK;
        }

        // Конструктор с возможностью переопределить периоды
        public IndicatorStochastic(List<CandleDTO> candles, int kPeriod, int dPeriod, int smoothK = 3)
            : base(candles, kPeriod)
        {
            _kPeriod = kPeriod;
            _dPeriod = dPeriod;
            _smoothK = smoothK;

            if (_kPeriod <= 0 || _dPeriod <= 0 || _smoothK <= 0)
                throw new ArgumentException("All periods must be positive");
        }

        public StochasticResult Calculate()
        {
            var stochasticResult = CalculateStochastic();
            return stochasticResult;
        }

        /// <summary>
        /// Вычисляет полный стохастический осциллятор с %K и %D линиями
        /// </summary>
        public StochasticResult CalculateStochastic()
        {
            if (_candles.Count < _kPeriod)
                throw new InvalidOperationException($"Insufficient data. Need at least {_kPeriod} candles for Stochastic calculation");

            var highs = GetHighs();
            var lows = GetLows();
            var closes = GetCloses();
            var length = _candles.Count;

            // Вычисляем raw %K
            var rawPercentK = CalculateRawPercentK(highs, lows, closes);

            // Сглаживаем %K
            var percentK = IndicatorSMA.CalculateSMA(rawPercentK.Select(x => (decimal)x).ToArray(), _smoothK);

            // Вычисляем %D (SMA от %K)
            var percentD = IndicatorSMA.CalculateSMA(percentK.Select(x => (decimal)x).ToArray(), _dPeriod);

            return new StochasticResult
            {
                PercentK = percentK,
                PercentD = percentD
            };
        }

        /// <summary>
        /// Вычисляет сырой %K без сглаживания
        /// </summary>
        private double[] CalculateRawPercentK(double[] highs, double[] lows, double[] closes)
        {
            var result = new double[_candles.Count];

            // Заполняем первые значения NaN
            for (int i = 0; i < _kPeriod - 1; i++)
            {
                result[i] = 0;
            }

            // Вычисляем %K для каждого периода
            for (int i = _kPeriod - 1; i < _candles.Count; i++)
            {
                var periodHigh = double.MinValue;
                var periodLow = double.MaxValue;

                // Находим максимум и минимум за период
                for (int j = i - _kPeriod + 1; j <= i; j++)
                {
                    if (highs[j] > periodHigh)
                        periodHigh = highs[j];
                    if (lows[j] < periodLow)
                        periodLow = lows[j];
                }

                // Вычисляем %K
                if (periodHigh == periodLow)
                {
                    result[i] = 50.0; // Избегаем деления на ноль
                }
                else
                {
                    result[i] = ((double)(closes[i] - periodLow) / (double)(periodHigh - periodLow)) * 100.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Получить последние значения Stochastic
        /// </summary>
        public StochasticValue GetLastValues()
        {
            var result = CalculateStochastic();
            var lastIndex = result.PercentK.Length - 1;

            // Ищем последние валидные значения
            while (lastIndex >= 0 && (double.IsNaN(result.PercentK[lastIndex]) ||
                                     double.IsNaN(result.PercentD[lastIndex])))
            {
                lastIndex--;
            }

            if (lastIndex < 0)
                return new StochasticValue { PercentK = 0, PercentD = 0 };

            return new StochasticValue
            {
                PercentK = result.PercentK[lastIndex],
                PercentD = result.PercentD[lastIndex]
            };
        }

        /// <summary>
        /// Определить состояние стохастического осциллятора
        /// </summary>
        public StochasticState GetStochasticState(double oversoldLevel = 20, double overboughtLevel = 80)
        {
            var lastValues = GetLastValues();

            if (double.IsNaN(lastValues.PercentK) || double.IsNaN(lastValues.PercentD))
                return StochasticState.Unknown;

            var avgValue = (lastValues.PercentK + lastValues.PercentD) / 2;

            if (avgValue <= oversoldLevel)
                return StochasticState.Oversold;
            else if (avgValue >= overboughtLevel)
                return StochasticState.Overbought;
            else
                return StochasticState.Normal;
        }

        /// <summary>
        /// Определить сигнал стохастического осциллятора
        /// </summary>
        public StochasticSignal GetStochasticSignal()
        {
            var result = CalculateStochastic();
            var length = result.PercentK.Length;

            if (length < 2)
                return StochasticSignal.Unknown;

            // Ищем два последних валидных значения
            int lastIndex = -1, prevIndex = -1;

            for (int i = length - 1; i >= 0; i--)
            {
                if (!double.IsNaN(result.PercentK[i]) && !double.IsNaN(result.PercentD[i]))
                {
                    if (lastIndex == -1)
                        lastIndex = i;
                    else if (prevIndex == -1)
                    {
                        prevIndex = i;
                        break;
                    }
                }
            }

            if (lastIndex == -1 || prevIndex == -1)
                return StochasticSignal.Unknown;

            var currentK = result.PercentK[lastIndex];
            var currentD = result.PercentD[lastIndex];
            var previousK = result.PercentK[prevIndex];
            var previousD = result.PercentD[prevIndex];

            // Бычий сигнал: %K пересекает %D снизу вверх в зоне перепроданности
            if (previousK < previousD && currentK > currentD && currentK < 20)
                return StochasticSignal.BullishCrossover;

            // Медвежий сигнал: %K пересекает %D сверху вниз в зоне перекупленности
            if (previousK > previousD && currentK < currentD && currentK > 80)
                return StochasticSignal.BearishCrossover;

            // Простые пересечения без учета зон
            if (previousK < previousD && currentK > currentD)
                return StochasticSignal.BullishCrossoverWeak;

            if (previousK > previousD && currentK < currentD)
                return StochasticSignal.BearishCrossoverWeak;

            return StochasticSignal.Neutral;
        }

        /// <summary>
        /// Проверить дивергенцию между ценой и стохастическим осциллятором
        /// </summary>
        public StochasticDivergence CheckDivergence(int lookbackPeriod = 10)
        {
            if (_candles.Count < lookbackPeriod + _kPeriod)
                return StochasticDivergence.None;

            var result = CalculateStochastic();
            var closes = GetCloses();
            var length = Math.Min(result.PercentK.Length, lookbackPeriod);

            // Получаем последние N свечей и значений стохастика
            var recentCandles = _candles.Skip(_candles.Count - length).ToList();
            var recentStochK = result.PercentK.Skip(result.PercentK.Length - length).ToArray();

            // Простая проверка дивергенции (можно усложнить)
            var priceHigh = recentCandles.Max(c => (double)c.High);
            var priceLow = recentCandles.Min(c => (double)c.Low);
            var stochHigh = recentStochK.Where(x => !double.IsNaN(x)).Max();
            var stochLow = recentStochK.Where(x => !double.IsNaN(x)).Min();

            var lastPrice = closes.Last();
            var lastStoch = recentStochK.LastOrDefault(x => !double.IsNaN(x));

            // Бычья дивергенция: цена делает новый минимум, а стохастик - нет
            if (lastPrice == priceLow && lastStoch > stochLow + 10)
                return StochasticDivergence.Bullish;

            // Медвежья дивергенция: цена делает новый максимум, а стохастик - нет  
            if (lastPrice == priceHigh && lastStoch < stochHigh - 10)
                return StochasticDivergence.Bearish;

            return StochasticDivergence.None;
        }
    }

    
}