using System;
using System.Collections.Generic;
using UnTraid.Core.Indicators.Interface;
using UnTraid.Core.Indicators.Servise;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators
{
    /// <summary>
    /// Исправленный IndicatorMACD без NaN значений
    /// </summary>
    public class IndicatorMACD : BaseIndicatorCalculator, IIndicatorCalculator<MACDResult>
    {
        public string Name => "MACD";

        private readonly int _fastPeriod;
        private readonly int _slowPeriod;
        private readonly int _signalPeriod;

        public IndicatorMACD(List<CandleDTO> candles)
            : base(candles, Properties.Settings.Default.MacdSlowPeriod)
        {
            _fastPeriod = Properties.Settings.Default.MacdFastPeriod;
            _slowPeriod = Properties.Settings.Default.MacdSlowPeriod;
            _signalPeriod = Properties.Settings.Default.MacdSignalPeriod;
        }

        public IndicatorMACD(List<CandleDTO> candles, int fastPeriod, int slowPeriod, int signalPeriod)
            : base(candles, slowPeriod)
        {
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
            _signalPeriod = signalPeriod;

            if (_fastPeriod >= _slowPeriod)
                throw new ArgumentException("Fast period must be less than slow period");
        }

        public MACDResult Calculate()
        {
            return CalculateMACD();
        }

        public MACDResult CalculateMACD()
        {
            if (_candles.Count < _slowPeriod + _signalPeriod)
                throw new InvalidOperationException($"Insufficient data. Need at least {_slowPeriod + _signalPeriod} candles for full MACD calculation");

            var closes = GetCloses();

            // Вычисляем полные EMA массивы
            var fastEMAFull = IndicatorEMA.CalculateEMAFull(closes, _fastPeriod);
            var slowEMAFull = IndicatorEMA.CalculateEMAFull(closes, _slowPeriod);

            // Вычисляем MACD линию начиная с позиции _slowPeriod - 1
            var macdStartIndex = _slowPeriod - 1;
            var macdLength = closes.Length - macdStartIndex;
            var macdValues = new double[closes.Length];

            for (int i = 0; i < closes.Length; i++)
            {
                if (i < macdStartIndex)
                {
                    macdValues[i] = 0;
                }
                else
                {
                    macdValues[i] = fastEMAFull[i] - slowEMAFull[i];
                }
            }

            // Вычисляем сигнальную линию (EMA от MACD)
            double[] signalValues;
            if (macdValues.Length >= _signalPeriod)
            {
                signalValues = IndicatorEMA.CalculateEMAFull(macdValues, _signalPeriod);
            }
            else
            {
                signalValues = new double[0];
            }

            // Вычисляем гистограмму
            var histogramLength = signalValues.Length;
            var histogram = new double[histogramLength];
            var macdOffset = macdValues.Length - histogramLength;

            for (int i = 0; i < histogramLength; i++)
            {
                histogram[i] = macdValues[macdOffset + i] - signalValues[i];
            }

            return new MACDResult
            {
                MACDLine = macdValues,
                SignalLine = signalValues,
                Histogram = histogram
            };
        }

        /// <summary>
        /// Получить последние значения MACD
        /// </summary>
        public MACDValue GetLastValues()
        {
            var result = CalculateMACD();

            if (result.MACDLine.Length == 0)
                return new MACDValue { MACD = 0, Signal = 0, Histogram = 0 };

            var lastMacd = result.MACDLine[result.MACDLine.Length - 1];
            var lastSignal = result.SignalLine.Length > 0 ? result.SignalLine[result.SignalLine.Length - 1] : 0;
            var lastHistogram = result.Histogram.Length > 0 ? result.Histogram[result.Histogram.Length - 1] : 0;

            return new MACDValue
            {
                MACD = lastMacd,
                Signal = lastSignal,
                Histogram = lastHistogram
            };
        }

        public MACDSignal GetMACDSignal()
        {
            var result = CalculateMACD();

            if (result.Histogram.Length < 2)
                return MACDSignal.Unknown;

            var currentHistogram = result.Histogram[result.Histogram.Length - 1];
            var previousHistogram = result.Histogram[result.Histogram.Length - 2];

            // Бычий сигнал: гистограмма пересекает нулевую линию снизу вверх
            if (previousHistogram < 0 && currentHistogram > 0)
                return MACDSignal.Bullish;

            // Медвежий сигнал: гистограмма пересекает нулевую линию сверху вниз
            if (previousHistogram > 0 && currentHistogram < 0)
                return MACDSignal.Bearish;

            return MACDSignal.Neutral;
        }
    }
}