using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnTraid.DTO
{
    public class StochasticResult
    {
        public double[] PercentK { get; set; }
        public double[] PercentD { get; set; }
    }

    public class StochasticValue
    {
        public double PercentK { get; set; }
        public double PercentD { get; set; }
    }

    public enum StochasticState
    {
        Unknown,
        Oversold,    // Перепроданность (обычно < 20)
        Normal,      // Нормальное состояние
        Overbought   // Перекупленность (обычно > 80)
    }

    public enum StochasticSignal
    {
        Unknown,
        BullishCrossover,      // Сильный бычий сигнал (пересечение в зоне перепроданности)
        BearishCrossover,      // Сильный медвежий сигнал (пересечение в зоне перекупленности)
        BullishCrossoverWeak,  // Слабый бычий сигнал (простое пересечение)
        BearishCrossoverWeak,  // Слабый медвежий сигнал (простое пересечение)
        Neutral
    }

    public enum StochasticDivergence
    {
        None,
        Bullish,    // Бычья дивергенция
        Bearish     // Медвежья дивергенция
    }
}
