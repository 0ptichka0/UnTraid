using System;
using System.Collections.Generic;
using System.Linq;
using UnTraid.Core.Indicators.Interface;
using UnTraid.Core.Indicators.Servise;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators.Calculators
{
    /// <summary>
    /// Simple Moving Average (SMA) - Простая скользящая средняя
    /// </summary>
    public class IndicatorSMA : BaseIndicatorCalculator, IIndicatorCalculator<double[]>
    {
        public string Name => "SMA";

        public IndicatorSMA(List<CandleDTO> candles, int period)
            : base(candles, period)
        {
        }

        public double[] Calculate()
        {
            if (_candles.Count < _period)
                throw new InvalidOperationException($"Insufficient data. Need at least {_period} candles for SMA calculation");

            var closes = GetCloses();
            var result = new double[closes.Length];

            // Заполняем первые значения NaN
            for (int i = 0; i < _period - 1; i++)
            {
                result[i] = 0;
            }

            // Вычисляем SMA
            for (int i = _period - 1; i < closes.Length; i++)
            {
                var sum = 0.0;
                for (int j = i - _period + 1; j <= i; j++)
                {
                    sum += (double)closes[j];
                }
                result[i] = sum / _period;
            }

            return result;
        }

        /// <summary>
        /// Статический метод для вычисления SMA из массива decimal
        /// </summary>
        public static double[] CalculateSMA(decimal[] values, int period)
        {
            if (values == null || values.Length < period)
                throw new ArgumentException("Insufficient data for SMA calculation");

            var result = new double[values.Length];

            // Заполняем первые значения NaN
            for (int i = 0; i < period - 1; i++)
            {
                result[i] = 0;
            }

            // Вычисляем SMA
            for (int i = period - 1; i < values.Length; i++)
            {
                var sum = 0.0;
                for (int j = i - period + 1; j <= i; j++)
                {
                    sum += (double)values[j];
                }
                result[i] = sum / period;
            }

            return result;
        }

        /// <summary>
        /// Получить последнее значение SMA
        /// </summary>
        public double GetLastValue()
        {
            var values = Calculate();
            return values.LastOrDefault(x => !double.IsNaN(x));
        }
    }
}