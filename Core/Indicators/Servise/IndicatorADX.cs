using System;
using System.Collections.Generic;
using UnTraid.Core.Indicators.Interface;
using UnTraid.Core.Indicators.Servise;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators
{
    public class IndicatorADX : BaseIndicatorCalculator, IIndicatorCalculator<ADXResult>
    {
        public string Name => "ADX";
        private readonly int _period;    // Период для ADX

        public IndicatorADX(List<CandleDTO> candles)
            : base(candles, Properties.Settings.Default.AdxPeriod)
        {
            _period = Properties.Settings.Default.AdxPeriod;
        }

        // Конструктор с возможностью переопределить период
        public IndicatorADX(List<CandleDTO> candles, int period)
            : base(candles, period)
        {
            _period = period;
            if (_period <= 0)
                throw new ArgumentException("Period must be positive");
        }

        public ADXResult Calculate()
        {
            var adxResult = CalculateADX();
            return adxResult; // Возвращаем основную ADX линию
        }

        /// <summary>
        /// Вычисляет полный ADX индикатор с ADX, +DI и -DI линиями
        /// </summary>
        public ADXResult CalculateADX()
        {
            if (_candles.Count < _period * 2)
                throw new InvalidOperationException($"Insufficient data. Need at least {_period * 2} candles for ADX calculation");

            var highs = GetHighs();
            var lows = GetLows();
            var closes = GetCloses();
            var length = _candles.Count;

            // Вычисляем True Range (TR)
            var trueRange = CalculateTrueRange(highs, lows, closes);

            // Вычисляем направленные движения +DM и -DM
            var directionalMovements = CalculateDirectionalMovements(highs, lows);
            var plusDM = directionalMovements.PlusDM;
            var minusDM = directionalMovements.MinusDM;

            // Вычисляем сглаженные значения TR, +DM и -DM
            var smoothedTR = CalculateSmoothedValues(trueRange, _period);
            var smoothedPlusDM = CalculateSmoothedValues(plusDM, _period);
            var smoothedMinusDM = CalculateSmoothedValues(minusDM, _period);

            // Вычисляем +DI и -DI
            var plusDI = CalculateDirectionalIndex(smoothedPlusDM, smoothedTR);
            var minusDI = CalculateDirectionalIndex(smoothedMinusDM, smoothedTR);

            // Вычисляем DX
            var dx = CalculateDX(plusDI, minusDI);

            // Вычисляем ADX (сглаженный DX)
            var adx = CalculateSmoothedValues(dx, _period);

            return new ADXResult
            {
                ADX = adx,
                PlusDI = plusDI,
                MinusDI = minusDI
            };
        }

        /// <summary>
        /// Вычисляет True Range для каждого периода
        /// </summary>
        private double[] CalculateTrueRange(double[] highs, double[] lows, double[] closes)
        {
            var tr = new double[highs.Length];
            tr[0] = highs[0] - lows[0]; // Первое значение

            for (int i = 1; i < highs.Length; i++)
            {
                var range1 = highs[i] - lows[i];
                var range2 = Math.Abs(highs[i] - closes[i - 1]);
                var range3 = Math.Abs(lows[i] - closes[i - 1]);

                tr[i] = Math.Max(range1, Math.Max(range2, range3));
            }

            return tr;
        }

        /// <summary>
        /// Вычисляет направленные движения +DM и -DM
        /// </summary>
        private DirectionalMovementResult CalculateDirectionalMovements(double[] highs, double[] lows)
        {
            var plusDM = new double[highs.Length];
            var minusDM = new double[highs.Length];

            for (int i = 1; i < highs.Length; i++)
            {
                var upMove = highs[i] - highs[i - 1];
                var downMove = lows[i - 1] - lows[i];

                if (upMove > downMove && upMove > 0)
                {
                    plusDM[i] = upMove;
                }
                else
                {
                    plusDM[i] = 0;
                }

                if (downMove > upMove && downMove > 0)
                {
                    minusDM[i] = downMove;
                }
                else
                {
                    minusDM[i] = 0;
                }
            }

            return new DirectionalMovementResult { PlusDM = plusDM, MinusDM = minusDM };
        }

        /// <summary>
        /// Вычисляет сглаженные значения по методу Wilder
        /// </summary>
        private double[] CalculateSmoothedValues(double[] values, int period)
        {
            var smoothed = new double[values.Length];

            // Первое сглаженное значение = сумма первых period значений
            var sum = 0.0;
            for (int i = 0; i < period && i < values.Length; i++)
            {
                sum += values[i];
            }

            if (period <= values.Length)
            {
                smoothed[period - 1] = sum;

                // Последующие сглаженные значения по формуле Wilder
                for (int i = period; i < values.Length; i++)
                {
                    smoothed[i] = (smoothed[i - 1] * (period - 1) + values[i]) / period;
                }
            }

            return smoothed;
        }

        /// <summary>
        /// Вычисляет индекс направленности (DI)
        /// </summary>
        private double[] CalculateDirectionalIndex(double[] smoothedDM, double[] smoothedTR)
        {
            var di = new double[smoothedDM.Length];

            for (int i = 0; i < smoothedDM.Length; i++)
            {
                if (smoothedTR[i] != 0)
                {
                    di[i] = (smoothedDM[i] / smoothedTR[i]) * 100;
                }
                else
                {
                    di[i] = 0;
                }
            }

            return di;
        }

        /// <summary>
        /// Вычисляет DX (индекс направленности)
        /// </summary>
        private double[] CalculateDX(double[] plusDI, double[] minusDI)
        {
            var dx = new double[plusDI.Length];

            for (int i = 0; i < plusDI.Length; i++)
            {
                var sum = plusDI[i] + minusDI[i];
                if (sum != 0)
                {
                    dx[i] = (Math.Abs(plusDI[i] - minusDI[i]) / sum) * 100;
                }
                else
                {
                    dx[i] = 0;
                }
            }

            return dx;
        }
    }
}
