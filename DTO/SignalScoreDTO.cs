using System;
using System.Collections.Generic;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.DTO
{
    /// <summary>
    /// Результат расчета SignalScore
    /// </summary>
    public class SignalScoreResult
    {
        public double Value { get; set; }
        /// <summary>
        /// Оптимистичный результат
        /// </summary>
        public double BullishScore { get; set; }
        /// <summary>
        /// писчимистический результат
        /// </summary>
        public double BearishScore { get; set; }
    }

    /// <summary>
    /// Последние значения SignalScore
    /// </summary>
    public class SignalScoreValue
    {
        public double Score { get; set; }
        public double BullishScore { get; set; }
        public double BearishScore { get; set; }
        public SignalStrength Strength { get; set; }
    }

    /// <summary>
    /// Сила сигнала
    /// </summary>
    public enum SignalStrength
    {
        VeryWeak,    // 0-20%
        Weak,        // 20-40%
        Moderate,    // 40-60%
        Strong,      // 60-80%
        VeryStrong   // 80-100%
    }

    /// <summary>
    /// Конфигурация условий для индикатора
    /// </summary>
    public class IndicatorCondition
    {
        public string Name { get; set; }
        public double Weight { get; set; }

        // Условие для сигнала разворота
        public Func<List<CandleDTO>, int, bool> ReversalCondition { get; set; }

        // Условие для бычьей силы
        public Func<List<CandleDTO>, int, bool> BullishCondition { get; set; }

        // Условие для медвежьей силы
        public Func<List<CandleDTO>, int, bool> BearishCondition { get; set; }
    }

    /// <summary>
    /// Конфигурация индикатора для SignalScore
    /// </summary>
    public class IndicatorConfig
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public List<IndicatorCondition> Conditions { get; set; } = new List<IndicatorCondition>();
    }
}
