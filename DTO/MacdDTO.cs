using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnTraid.DTO
{

    /// <summary>
    /// Обновленный результат MACD с информацией о позиционировании
    /// </summary>
    public class MACDResult
    {
        public double[] MACDLine { get; set; }      // Основная MACD линия
        public double[] SignalLine { get; set; }    // Сигнальная линия
        public double[] Histogram { get; set; }     // Гистограмма
    }

    public class MACDValue
    {
        public double MACD { get; set; }
        public double Signal { get; set; }
        public double Histogram { get; set; }
    }

    public enum MACDSignal
    {
        Unknown,
        Bullish,    // Бычий сигнал
        Bearish,    // Медвежий сигнал
        Neutral     // Нейтральный
    }

}
