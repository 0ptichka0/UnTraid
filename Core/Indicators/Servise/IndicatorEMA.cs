using NPOI.OpenXmlFormats.Dml.Chart;
using static UnTraid.DTO.TInvestDTO;
using System.Collections.Generic;
using System;
using UnTraid.Core.Indicators.Interface;
using UnTraid.Core.Indicators.Servise;

namespace UnTraid.Core.Indicators
{
    public class IndicatorEMA : BaseIndicatorCalculator, IIndicatorCalculator<double[]>
    {
        public string Name => "EMA";
        public IndicatorEMA(List<CandleDTO> candles, int period)
            : base(candles, period)
        {
        }

        public double[] Calculate()
        {
            if (_candles.Count < _period)
                throw new InvalidOperationException($"Insufficient data. Need at least {_period} candles for EMA calculation");

            var closes = GetCloses();
            return CalculateEMA(closes, _period);
        }

        /// <summary>
        /// Статический метод для вычисления EMA - возвращает только валидные значения
        /// </summary>
        public static double[] CalculateEMA(double[] values, int period)
        {
            if (values == null || values.Length < period)
                throw new ArgumentException("Insufficient data for EMA calculation");

            // Возвращаем массив только с валидными значениями (начиная с period-1 индекса)
            var validLength = values.Length - period + 1;
            var result = new double[validLength];
            var multiplier = 2.0 / (period + 1);

            // Первое значение EMA = простое среднее первых period значений
            var sum = 0.0;
            for (int i = 0; i < period; i++)
            {
                sum += values[i];
            }
            result[0] = sum / period;

            // Вычисляем остальные значения EMA
            for (int i = 1; i < validLength; i++)
            {
                var originalIndex = i + period - 1;
                result[i] = (values[originalIndex] * multiplier) + (result[i - 1] * (1 - multiplier));
            }

            return result;
        }

        /// <summary>
        /// Статический метод для полного массива (с учетом позиционирования)
        /// </summary>
        public static double[] CalculateEMAFull(double[] values, int period)
        {
            if (values == null || values.Length < period)
                return new double[0];

            var result = new double[values.Length];
            var multiplier = 2.0 / (period + 1);

            // Первое значение EMA на позиции period-1
            var sum = 0.0;
            for (int i = 0; i < period; i++)
            {
                sum += values[i];
            }
            result[period - 1] = sum / period;

            // Вычисляем остальные значения EMA
            for (int i = period; i < values.Length; i++)
            {
                result[i] = (values[i] * multiplier) + (result[i - 1] * (1 - multiplier));
            }

            return result;
        }

        /// <summary>
        /// Вычисляет EMA с заполнением нулями в начале
        /// </summary>
        public static double[] CalculateEMAWithZeros(double[] values, int period)
        {
            var result = new double[values.Length];

            if (values.Length < period)
                return result; // Возвращаем массив нулей

            var multiplier = 2.0 / (period + 1);

            // Первое значение EMA на позиции period-1
            var sum = 0.0;
            for (int i = 0; i < period; i++)
            {
                sum += values[i];
            }
            result[period - 1] = sum / period;

            // Вычисляем остальные значения EMA
            for (int i = period; i < values.Length; i++)
            {
                result[i] = (values[i] * multiplier) + (result[i - 1] * (1 - multiplier));
            }

            return result;
        }

        public double GetLastValue()
        {
            var values = Calculate();
            return values.Length > 0 ? values[values.Length - 1] : 0;
        }
    }
}