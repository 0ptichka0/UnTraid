using System;
using System.Collections.Generic;
using System.Linq;
using UnTraid.Core.Indicators.Interface;
using UnTraid.Core.Indicators.Servise;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators.Calculators
{
    public class IndicatorRSI : BaseIndicatorCalculator, IIndicatorCalculator<double[]>
    {
        public string Name => "RSI";
        private readonly double _oversoldLevel;
        private readonly double _overboughtLevel;

        public IndicatorRSI(List<CandleDTO> candles)
            : base(candles, Properties.Settings.Default.RsiPeriod)
        {

            _oversoldLevel = Properties.Settings.Default.RsiOversoldLevel;
            _overboughtLevel = Properties.Settings.Default.RsiOverboughtLevel;
        }

        // Конструктор с возможностью переопределить период (для тестирования или особых случаев)
        public IndicatorRSI(List<CandleDTO> candles, int period)
            : base(candles, period)
        {
        }

        public double[] Calculate()
        {
            if (_candles.Count < _period + 1)
                throw new InvalidOperationException($"Insufficient data. Need at least {_period + 1} candles for RSI calculation");

            var closes = GetCloses();
            var rsiValues = new double[closes.Length];

            // Инициализируем массивы для хранения изменений цен
            var gains = new double[closes.Length];
            var losses = new double[closes.Length];

            // Вычисляем изменения цен
            for (int i = 1; i < closes.Length; i++)
            {
                var change = closes[i] - closes[i - 1];
                gains[i] = change > 0 ? change : 0;
                losses[i] = change < 0 ? Math.Abs(change) : 0;
            }

            // Вычисляем первое значение RSI (простое среднее)
            var initialAvgGain = gains.Skip(1).Take(_period).Average();
            var initialAvgLoss = losses.Skip(1).Take(_period).Average();

            var avgGain = (double)initialAvgGain;
            var avgLoss = (double)initialAvgLoss;

            // Заполняем первые значения NaN
            for (int i = 0; i < _period; i++)
            {
                rsiValues[i] = 0;
            }

            // Вычисляем первое значение RSI
            if (avgLoss == 0)
                rsiValues[_period] = 100;
            else
            {
                var rs = avgGain / avgLoss;
                rsiValues[_period] = 100 - (100 / (1 + rs));
            }

            // Вычисляем остальные значения RSI с использованием экспоненциального сглаживания
            for (int i = _period + 1; i < closes.Length; i++)
            {
                avgGain = ((avgGain * (_period - 1)) + (double)gains[i]) / _period;
                avgLoss = ((avgLoss * (_period - 1)) + (double)losses[i]) / _period;

                if (avgLoss == 0)
                    rsiValues[i] = 100;
                else
                {
                    var rs = avgGain / avgLoss;
                    rsiValues[i] = 100 - (100 / (1 + rs));
                }
            }

            return rsiValues;
        }

        /// <summary>
        /// Получить последнее значение RSI
        /// </summary>
        public double GetLastValue()
        {
            var values = Calculate();
            return values.LastOrDefault(x => !double.IsNaN(x));
        }

        /// <summary>
        /// Получить значения RSI без NaN
        /// </summary>
        public double[] GetValidValues()
        {
            return Calculate().Where(x => !double.IsNaN(x)).ToArray();
        }

        /// <summary>
        /// Проверить состояние RSI (перепроданность/перекупленность)
        /// </summary>
        public RSIState GetRSIState()
        {

            var lastValue = GetLastValue();

            if (double.IsNaN(lastValue))
                return RSIState.Unknown;

            if (lastValue <= _oversoldLevel)
                return RSIState.Oversold;
            else if (lastValue >= _overboughtLevel)
                return RSIState.Overbought;
            else
                return RSIState.Normal;
        }
    }
}