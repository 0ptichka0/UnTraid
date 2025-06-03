using NPOI.HSSF.Record.Chart;
using System;
using System.Collections.Generic;
using UnTraid.Core.Indicators.Interface;
using UnTraid.Core.Indicators.Servise;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators
{
    public class IndicatorTRIX : BaseIndicatorCalculator, IIndicatorCalculator<TRIXResult>
    {
        public string Name => "TRIX";
        private readonly int _period;    // Период для EMA

        public IndicatorTRIX(List<CandleDTO> candles)
            : base(candles, Properties.Settings.Default.TrixPeriod)
        {
            _period = Properties.Settings.Default.TrixPeriod;
        }

        // Конструктор с возможностью переопределить периоды
        public IndicatorTRIX(List<CandleDTO> candles, int period, int signalPeriod = 9)
            : base(candles, period)
        {
            _period = period;
            if (_period <= 0)
                throw new ArgumentException("All periods must be positive");
        }

        public TRIXResult Calculate()
        {
            var trixResult = CalculateTRIX();
            return trixResult;
        }

        /// <summary>
        /// Вычисляет полный TRIX индикатор с основной линией и сигнальной линией
        /// </summary>
        public TRIXResult CalculateTRIX()
        {
            if (_candles.Count < _period * 3 + 1)
                throw new InvalidOperationException($"Insufficient data. Need at least {_period * 3 + 1} candles for TRIX calculation");

            var closes = GetCloses();

            // Создаем результирующие массивы с нулями
            var trixValues = new double[closes.Length];
            var signalValues = new double[closes.Length];

            // Первое сглаживание EMA
            var firstEMA = IndicatorEMA.CalculateEMA(closes, _period);

            // Второе сглаживание EMA
            var secondEMA = IndicatorEMA.CalculateEMA(firstEMA, _period);

            // Третье сглаживание EMA
            var thirdEMA = IndicatorEMA.CalculateEMA(secondEMA, _period);

            // Минимальный индекс для начала расчета TRIX: 
            // (_period - 1) + (_period - 1) + (_period - 1) + 1 = _period * 3 - 2
            int trixStartIndex = _period * 3 - 2;

            // Вычисляем TRIX как процентное изменение третьего EMA
            CalculateTRIXValues(thirdEMA, trixValues, trixStartIndex);

            // Вычисляем сигнальную линию (EMA от TRIX)
            // Начинаем после того, как у нас есть достаточно значений TRIX
            //int signalStartIndex = trixStartIndex + 9 - 1; // 9 - период для сигнальной линии
            CalculateSignalLine(trixValues, signalValues, trixStartIndex);

            return new TRIXResult
            {
                TRIX = trixValues,
                Signal = signalValues
            };
        }


        /// <summary>
        /// Вычисляет значения TRIX как процентное изменение, начиная с указанного индекса
        /// </summary>
        private void CalculateTRIXValues(double[] emaValues, double[] result, int index)
        {
            // TRIX требует два соседних значения для расчета процентного изменения
            // Поэтому начинаем с startIndex + 1
            for (int i = 1; i < emaValues.Length; i++)
            {
                if (emaValues[i - 1] != 0 && emaValues[i] != 0)
                {
                    result[i + index - 1] = ((emaValues[i] - emaValues[i - 1]) / emaValues[i - 1]) * 10000;
                }
                // Остальные значения остаются нулями
            }
        }

        /// <summary>
        /// Вычисляет сигнальную линию как EMA от значений TRIX
        /// </summary>
        private void CalculateSignalLine(double[] trixValues, double[] signalValues, int trixStartIndex)
        {
            // Находим первый ненулевой индекс TRIX
            int firstTrixIndex = -1;
            for (int i = trixStartIndex; i < trixValues.Length; i++)
            {
                if (trixValues[i] != 0)
                {
                    firstTrixIndex = i;
                    break;
                }
            }

            if (firstTrixIndex == -1) return; // Нет валидных значений TRIX

            // Извлекаем только ненулевые значения TRIX для расчета EMA
            var validTrixValues = new List<double>();

            for (int i = firstTrixIndex; i < trixValues.Length; i++)
            {
                if (trixValues[i] != 0)
                {
                    validTrixValues.Add(trixValues[i]);
                }
            }

            if (validTrixValues.Count < 9) return; // Недостаточно данных для сигнальной линии

            // Вычисляем EMA от значений TRIX
            var signalEMA = IndicatorEMA.CalculateEMAFull(validTrixValues.ToArray(), 9);

            for (var i = signalEMA.Length - 1; i >= 0; i--)
            {
                signalValues[i + firstTrixIndex] = signalEMA[i];
            }

            //// Размещаем значения сигнальной линии в правильных позициях
            //int signalStartIndex = 9 - 1;
            //for (int i = signalStartIndex; i < signalEMA.Length; i++)
            //{
            //    int originalIndex = trixIndices[i];
            //    if (originalIndex < signalValues.Length)
            //    {
            //        signalValues[originalIndex] = signalEMA[i];
            //    }
            //}
        }
    }
}