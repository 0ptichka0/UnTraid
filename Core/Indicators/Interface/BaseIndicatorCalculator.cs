using System;
using System.Collections.Generic;
using System.Linq;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators.Servise
{
    public abstract class BaseIndicatorCalculator
    {
        protected readonly List<CandleDTO> _candles;
        protected readonly int _period;

        protected BaseIndicatorCalculator(List<CandleDTO> candles, int period)
        {
            _candles = candles ?? throw new ArgumentNullException(nameof(candles));
            _period = period > 0 ? period : throw new ArgumentException("Period must be positive");
        }

        protected double[] GetCloses() => _candles.Select(c => (double)c.Close).ToArray();
        protected double[] GetHighs() => _candles.Select(c => (double)c.High).ToArray();
        protected double[] GetLows() => _candles.Select(c => (double)c.Low).ToArray();
    }
}
