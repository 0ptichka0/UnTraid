using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnTraid.Core.Indicators.Calculators;
using UnTraid.Core.Indicators.Interface;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core.Indicators.Servise
{
    /// <summary>
    /// Индикатор SignalScore - оценка силы тренда и сигналов разворота
    /// </summary>
    public class IndicatorSignalScore : BaseIndicatorCalculator, IIndicatorCalculator<SignalScoreResult>
    {
        public string Name => "SignalScore";

        private readonly List<IndicatorConfig> _indicatorConfigs;
        private readonly List<string> _indicatorsName;

        // Кэш для индикаторов
        private IndicatorRSI _rsiIndicator;
        private IndicatorMACD _macdIndicator;
        private IndicatorADX _adxIndicator;
        private IndicatorStochastic _stochasticIndicator;
        private IndicatorTRIX _trixIndicator;

        public IndicatorSignalScore(List<CandleDTO> candles, List<string> indicatorsName)
            : base(candles, 1)
        {
            _indicatorsName = indicatorsName;
            InitializeIndicators();
            _indicatorConfigs = CreateExtendedConfiguration();
        }

        private void InitializeIndicators()
        {
            if (_candles.Count > 14 && _indicatorsName.Contains("RSI"))
                _rsiIndicator = new IndicatorRSI(_candles);

            if (_candles.Count > 26 && _indicatorsName.Contains("MACD"))
                _macdIndicator = new IndicatorMACD(_candles);

            if (_candles.Count > 14 && _indicatorsName.Contains("ADX"))
                _adxIndicator = new IndicatorADX(_candles);

            if (_candles.Count > 14 && _indicatorsName.Contains("Stochastic"))
                _stochasticIndicator = new IndicatorStochastic(_candles);

            if (_candles.Count > 18 && _indicatorsName.Contains("TRIX"))
                _trixIndicator = new IndicatorTRIX(_candles);
        }




// Новая конфигурация с разделением на три типа сигналов
private List<IndicatorConfig> CreateExtendedConfiguration()
        {
            var configs = new List<IndicatorConfig>();

            // RSI конфигурация (вес 25%)
            if (_rsiIndicator != null)
            {
                configs.Add(new IndicatorConfig
                {
                    Name = "RSI",
                    Weight = 0.25,
                    Conditions = new List<IndicatorCondition>
                    {
                        new IndicatorCondition
                        {
                            // RSI_Extreme_Levels - сигнал разворота при экстремальных уровнях
                            Name = "RSI_Extreme_Levels",
                            Weight = 0.4,
                            ReversalCondition = (candles, index) => IsRSIAtExtremeLevel(index)
                        },
                        new IndicatorCondition
                        {
                            // RSI_Divergence - сильный сигнал разворота
                            Name = "RSI_Divergence",
                            Weight = 0.6,
                            ReversalCondition = (candles, index) => IsRSIDivergence(index, candles)
                        },
                        new IndicatorCondition
                        {
                            // RSI_Bullish_Strength - сила роста
                            Name = "RSI_Bullish_Strength",
                            Weight = 0.3,
                            BullishCondition = (candles, index) => IsRSIShowingBullishStrength(index)
                        },
                        new IndicatorCondition
                        {
                            // RSI_Bearish_Strength - сила падения
                            Name = "RSI_Bearish_Strength",
                            Weight = 0.3,
                            BearishCondition = (candles, index) => IsRSIShowingBearishStrength(index)
                        }
                    }
                });
            }

            // MACD конфигурация (вес 30%)
            if (_macdIndicator != null)
            {
                configs.Add(new IndicatorConfig
                {
                    Name = "MACD",
                    Weight = 0.30,
                    Conditions = new List<IndicatorCondition>
                    {
                        new IndicatorCondition
                        {
                            // MACD_Signal_Crossover - сигнал разворота при пересечении
                            Name = "MACD_Signal_Crossover",
                            Weight = 0.5,
                            ReversalCondition = (candles, index) => IsMACDCrossover(index)
                        },
                        new IndicatorCondition
                        {
                            // MACD_Zero_Cross - сигнал разворота при пересечении нуля
                            Name = "MACD_Zero_Cross",
                            Weight = 0.4,
                            ReversalCondition = (candles, index) => IsMACDZeroCross(index)
                        },
                        new IndicatorCondition
                        {
                            // MACD_Histogram_Momentum - изменение импульса как сигнал разворота
                            Name = "MACD_Histogram_Momentum",
                            Weight = 0.3,
                            ReversalCondition = (candles, index) => IsMACDHistogramChangingDirection(index)
                        },
                        new IndicatorCondition
                        {
                            // MACD_Bullish_Strength - сила бычьего тренда
                            Name = "MACD_Bullish_Strength",
                            Weight = 0.4,
                            BullishCondition = (candles, index) => IsMACDShowingBullishStrength(index)
                        },
                        new IndicatorCondition
                        {
                            // MACD_Bearish_Strength - сила медвежьего тренда
                            Name = "MACD_Bearish_Strength",
                            Weight = 0.4,
                            BearishCondition = (candles, index) => IsMACDShowingBearishStrength(index)
                        }
                    }
                });
            }

            // ADX конфигурация (вес 20%)
            if (_adxIndicator != null)
            {
                configs.Add(new IndicatorConfig
                {
                    Name = "ADX",
                    Weight = 0.20,
                    Conditions = new List<IndicatorCondition>
                    {
                        new IndicatorCondition
                        {
                            // ADX_DI_Crossover - сигнал разворота при пересечении DI
                            Name = "ADX_DI_Crossover",
                            Weight = 0.4,
                            ReversalCondition = (candles, index) => IsADXDICrossover(index)
                        },
                        new IndicatorCondition
                        {
                            // ADX_Trend_Weakening - ослабление тренда как сигнал разворота
                            Name = "ADX_Trend_Weakening",
                            Weight = 0.3,
                            ReversalCondition = (candles, index) => IsADXTrendWeakening(index)
                        },
                        new IndicatorCondition
                        {
                            // ADX_Bullish_Strength - сила бычьего тренда
                            Name = "ADX_Bullish_Strength",
                            Weight = 0.5,
                            BullishCondition = (candles, index) => IsADXShowingBullishStrength(index)
                        },
                        new IndicatorCondition
                        {
                            // ADX_Bearish_Strength - сила медвежьего тренда
                            Name = "ADX_Bearish_Strength",
                            Weight = 0.5,
                            BearishCondition = (candles, index) => IsADXShowingBearishStrength(index)
                        }
                    }
                });
            }

            // Stochastic конфигурация (вес 15%)
            if (_stochasticIndicator != null)
            {
                configs.Add(new IndicatorConfig
                {
                    Name = "Stochastic",
                    Weight = 0.15,
                    Conditions = new List<IndicatorCondition>
                    {
                        new IndicatorCondition
                        {
                            // Stochastic_Crossover_Signal - пересечение %K и %D как сигнал разворота
                            Name = "Stochastic_Crossover_Signal",
                            Weight = 0.4,
                            ReversalCondition = (candles, index) => IsStochasticCrossover(index)
                        },
                        new IndicatorCondition
                        {
                            // Stochastic_Extreme_Reversal - экстремальные уровни как сигнал разворота
                            Name = "Stochastic_Extreme_Reversal",
                            Weight = 0.5,
                            ReversalCondition = (candles, index) => IsStochasticAtExtremeForReversal(index)
                        },
                        new IndicatorCondition
                        {
                            // Stochastic_Approaching_Cross - приближение к пересечению (ваш пример)
                            Name = "Stochastic_Approaching_Cross",
                            Weight = 0.3,
                            ReversalCondition = (candles, index) => IsStochasticApproachingCross(index)
                        },
                        new IndicatorCondition
                        {
                            // Stochastic_Bullish_Momentum - восходящая динамика стохастика
                            Name = "Stochastic_Bullish_Momentum",
                            Weight = 0.4,
                            BullishCondition = (candles, index) => IsStochasticShowingBullishMomentum(index)
                        },
                        new IndicatorCondition
                        {
                            // Stochastic_Bearish_Momentum - нисходящая динамика стохастика
                            Name = "Stochastic_Bearish_Momentum",
                            Weight = 0.4,
                            BearishCondition = (candles, index) => IsStochasticShowingBearishMomentum(index)
                        }
                    }
                });
            }

            // TRIX конфигурация (вес 10%)
            if (_trixIndicator != null)
            {
                configs.Add(new IndicatorConfig
                {
                    Name = "TRIX",
                    Weight = 0.10,
                    Conditions = new List<IndicatorCondition>
                    {
                        new IndicatorCondition
                        {
                            // TRIX_Zero_Cross - пересечение нуля как сигнал разворота
                            Name = "TRIX_Zero_Cross",
                            Weight = 0.4,
                            ReversalCondition = (candles, index) => IsTRIXZeroCross(index)
                        },
                        new IndicatorCondition
                        {
                            // TRIX_Signal_Crossover - пересечение сигнальной линии
                            Name = "TRIX_Signal_Crossover",
                            Weight = 0.5,
                            ReversalCondition = (candles, index) => IsTRIXSignalCrossover(index)
                        },
                        new IndicatorCondition
                        {
                            // TRIX_Bullish_Trend - устойчивый рост TRIX
                            Name = "TRIX_Bullish_Trend",
                            Weight = 0.3,
                            BullishCondition = (candles, index) => IsTRIXShowingBullishTrend(index)
                        },
                        new IndicatorCondition
                        {
                            // TRIX_Bearish_Trend - устойчивое падение TRIX
                            Name = "TRIX_Bearish_Trend",
                            Weight = 0.3,
                            BearishCondition = (candles, index) => IsTRIXShowingBearishTrend(index)
                        }
                    }
                });
            }

            return configs;
        }

        public SignalScoreResult Calculate()
        {
            return CalculateSignalScore();
        }

        public SignalScoreResult Calculate(int moment)
        {
            return CalculateSignalScore(moment);
        }

        public SignalScoreResult CalculateSignalScore(int moment = 0)
        {
            if (moment == 0)
            {
                moment = _candles.Count - 1;
            }

            double totalReversalScore = 0;
            double totalBullishScore = 0;
            double totalBearishScore = 0;

            double totalReversalWeight = 0;
            double totalBullishWeight = 0;
            double totalBearishWeight = 0;

            var reversalIndicators = new List<string>();
            var bullishIndicators = new List<string>();
            var bearishIndicators = new List<string>();

            foreach (var indicatorConfig in _indicatorConfigs)
            {
                double indicatorReversalScore = 0;
                double indicatorBullishScore = 0;
                double indicatorBearishScore = 0;

                double activeReversalWeight = 0;
                double activeBullishWeight = 0;
                double activeBearishWeight = 0;

                foreach (var condition in indicatorConfig.Conditions)
                {
                    // Проверяем сигналы разворота
                    if (condition.ReversalCondition != null && condition.ReversalCondition(_candles, moment))
                    {
                        indicatorReversalScore += condition.Weight;
                        activeReversalWeight += condition.Weight;
                    }

                    // Проверяем бычью силу
                    if (condition.BullishCondition != null && condition.BullishCondition(_candles, moment))
                    {
                        indicatorBullishScore += condition.Weight;
                        activeBullishWeight += condition.Weight;
                    }

                    // Проверяем медвежью силу
                    if (condition.BearishCondition != null && condition.BearishCondition(_candles, moment))
                    {
                        indicatorBearishScore += condition.Weight;
                        activeBearishWeight += condition.Weight;
                    }
                }

                // Получаем общий вес всех условий каждого типа для данного индикатора
                double totalReversalWeightForIndicator = indicatorConfig.Conditions
                    .Where(c => c.ReversalCondition != null)
                    .Sum(c => c.Weight);
                double totalBullishWeightForIndicator = indicatorConfig.Conditions
                    .Where(c => c.BullishCondition != null)
                    .Sum(c => c.Weight);
                double totalBearishWeightForIndicator = indicatorConfig.Conditions
                    .Where(c => c.BearishCondition != null)
                    .Sum(c => c.Weight);

                // Добавляем к общему скору с нормализацией
                if (activeReversalWeight > 0 && totalReversalWeightForIndicator > 0)
                {
                    double normalizedScore = (indicatorReversalScore / totalReversalWeightForIndicator) * indicatorConfig.Weight;
                    totalReversalScore += normalizedScore;
                    totalReversalWeight += indicatorConfig.Weight;
                    reversalIndicators.Add(indicatorConfig.Name);
                }

                if (activeBullishWeight > 0 && totalBullishWeightForIndicator > 0)
                {
                    double normalizedScore = (indicatorBullishScore / totalBullishWeightForIndicator) * indicatorConfig.Weight;
                    totalBullishScore += normalizedScore;
                    //totalBullishWeight += indicatorConfig.Weight;
                    totalBullishWeight += (activeBullishWeight / totalBullishWeightForIndicator) * indicatorConfig.Weight;
                    bullishIndicators.Add(indicatorConfig.Name);
                }

                if (activeBearishWeight > 0 && totalBearishWeightForIndicator > 0)
                {
                    double normalizedScore = (indicatorBearishScore / totalBearishWeightForIndicator) * indicatorConfig.Weight;
                    totalBearishScore += normalizedScore;
                    //totalBearishWeight += indicatorConfig.Weight;
                    totalBullishWeight += (activeBearishWeight / totalBearishWeightForIndicator) * indicatorConfig.Weight;
                    bearishIndicators.Add(indicatorConfig.Name);
                }
            }

            // Вычисляем средние значения
            double reversalAverage = totalReversalWeight > 0 ? totalReversalScore / totalReversalWeight : 0;

            double totalPossibleBullishWeight = _indicatorConfigs
                .Where(config => config.Conditions.Any(c => c.BullishCondition != null))
                .Sum(config => config.Weight);
            double bullishAverage = totalPossibleBullishWeight > 0 ? totalBullishScore / totalPossibleBullishWeight : 0;

            //double bullishAverage = totalBullishWeight > 0 ? totalBullishScore / totalBullishWeight : 0;

            double totalPossibleBearishWeight = _indicatorConfigs
                .Where(config => config.Conditions.Any(c => c.BullishCondition != null))
                .Sum(config => config.Weight);
            double bearishAverage = totalPossibleBearishWeight > 0 ? totalBearishScore / totalPossibleBearishWeight : 0;

            //double bearishAverage = totalBearishWeight > 0 ? totalBearishScore / totalBearishWeight : 0;

            // Применяем бонусы за консенсус
            double reversalBonus = CalculateConsensusBonus(reversalIndicators.Count, reversalIndicators);
            double bullishBonus = CalculateConsensusBonus(bullishIndicators.Count, bullishIndicators);
            double bearishBonus = CalculateConsensusBonus(bearishIndicators.Count, bearishIndicators);

            reversalAverage *= reversalBonus;
            bullishAverage *= bullishBonus;
            bearishAverage *= bearishBonus;

            // Ограничиваем максимальные значения
            reversalAverage = Math.Min(reversalAverage, 1.0);
            bullishAverage = Math.Min(bullishAverage, 1.0);
            bearishAverage = Math.Min(bearishAverage, 1.0);

            return new SignalScoreResult
            {
                Value = Math.Round(reversalAverage * 100, 2), // Сигнал разворота
                BullishScore = Math.Round(bullishAverage * 100, 2), // Сила роста
                BearishScore = Math.Round(bearishAverage * 100, 2)  // Сила падения
            };
        }

        private double CalculateConsensusBonus(int indicatorCount, List<string> activeIndicators)
        {
            double bonus = 1.0;

            // Базовый бонус за количество индикаторов
            if (indicatorCount >= 4)
            {
                bonus = 1.4; // +40% за 4+ индикаторов
            }
            else if (indicatorCount == 3)
            {
                bonus = 1.25; // +25% за 3 индикатора
            }
            else if (indicatorCount == 2)
            {
                bonus = 1.15; // +15% за 2 индикатора
            }

            // Дополнительный бонус за "сильные" комбинации
            bool hasMacdAndRsi = activeIndicators.Contains("MACD") && activeIndicators.Contains("RSI");
            bool hasAdxConfirmation = activeIndicators.Contains("ADX");

            if (hasMacdAndRsi)
            {
                bonus *= 1.1; // +10% за MACD + RSI
            }

            if (hasAdxConfirmation && indicatorCount >= 2)
            {
                bonus *= 1.05; // +5% за подтверждение трендовым индикатором ADX
            }

            return Math.Min(bonus, 1.6); // Максимальный бонус +60%
        }

        #region Новые методы для условий разворота и силы тренда

        // RSI методы
        private bool IsRSIAtExtremeLevel(int index)
        {
            var rsi = GetRSIValue(index);
            return rsi < 30 || rsi > 70; // Экстремальные уровни
        }

        private bool IsRSIDivergence(int index, List<CandleDTO> candles)
        {
            return IsRSIDivergenceBullish(index, candles) || IsRSIDivergenceBearish(index, candles);
        }

        private bool IsRSIShowingBullishStrength(int index)
        {
            var rsi = GetRSIValue(index);
            return rsi > 50 && rsi < 70 && IsRSIConsistentlyRising(index);
        }

        private bool IsRSIShowingBearishStrength(int index)
        {
            var rsi = GetRSIValue(index);
            return rsi < 50 && rsi > 30 && IsRSIConsistentlyFalling(index);
        }

        // MACD методы
        private bool IsMACDCrossover(int index)
        {
            return IsMACDCrossoverBullish(index) || IsMACDCrossoverBearish(index);
        }

        private bool IsMACDZeroCross(int index)
        {
            return IsMACDZeroCrossBullish(index) || IsMACDZeroCrossBearish(index);
        }

        private bool IsMACDHistogramChangingDirection(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.Histogram == null || index < 2 || index >= macdResult.Histogram.Length)
                    return false;

                // Проверяем изменение направления гистограммы
                var hist0 = macdResult.Histogram[index];
                var hist1 = macdResult.Histogram[index - 1];
                var hist2 = macdResult.Histogram[index - 2];

                // Изменение с роста на падение или наоборот
                return (hist1 > hist2 && hist0 < hist1) || (hist1 < hist2 && hist0 > hist1);
            }
            catch { return false; }
        }

        private bool IsMACDShowingBullishStrength(int index)
        {
            return IsMACDInBullishState(index) && IsMACDHistogramRising(index);
        }

        private bool IsMACDShowingBearishStrength(int index)
        {
            return IsMACDInBearishState(index) && IsMACDHistogramFalling(index);
        }

        // ADX методы
        private bool IsADXDICrossover(int index)
        {
            return IsDICrossoverBullish(index) || IsDICrossoverBearish(index);
        }

        private bool IsADXTrendWeakening(int index)
        {
            return IsADXWeakening(index);
        }

        private bool IsADXShowingBullishStrength(int index)
        {
            return IsADXShowingStrongBullTrend(index) && IsADXStrengthening(index);
        }

        private bool IsADXShowingBearishStrength(int index)
        {
            return IsADXShowingStrongBearTrend(index) && IsADXStrengthening(index);
        }

        // Stochastic методы
        private bool IsStochasticCrossover(int index)
        {
            return IsStochasticCrossoverBullish(index) || IsStochasticCrossoverBearish(index);
        }

        private bool IsStochasticAtExtremeForReversal(int index)
        {
            return IsStochasticExtremeOversold(index) || IsStochasticExtremeOverbought(index);
        }

        private bool IsStochasticApproachingCross(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || stochResult?.PercentD == null ||
                    index < 1 || index >= stochResult.PercentK.Length)
                    return false;

                var k = stochResult.PercentK[index];
                var d = stochResult.PercentD[index];
                var kPrev = stochResult.PercentK[index - 1];
                var dPrev = stochResult.PercentD[index - 1];

                // Линии сближаются (разность уменьшается)
                var currentDiff = Math.Abs(k - d);
                var prevDiff = Math.Abs(kPrev - dPrev);

                return currentDiff < prevDiff && currentDiff < 5; // Близко к пересечению
            }
            catch { return false; }
        }

        private bool IsStochasticShowingBullishMomentum(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || index < 1 || index >= stochResult.PercentK.Length)
                    return false;

                // %K растет и находится выше %D
                return stochResult.PercentK[index] > stochResult.PercentK[index - 1] &&
                       stochResult.PercentK[index] > stochResult.PercentD[index];
            }
            catch { return false; }
        }

        private bool IsStochasticShowingBearishMomentum(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || index < 1 || index >= stochResult.PercentK.Length)
                    return false;

                // %K падает и находится ниже %D
                return stochResult.PercentK[index] < stochResult.PercentK[index - 1] &&
                       stochResult.PercentK[index] < stochResult.PercentD[index];
            }
            catch { return false; }
        }

        // TRIX методы
        private bool IsTRIXZeroCross(int index)
        {
            try
            {
                var trixResult = _trixIndicator?.Calculate();
                if (trixResult?.TRIX == null || index < 1 || index >= trixResult.TRIX.Length)
                    return false;

                // Пересечение нулевой линии в любом направлении
                return (trixResult.TRIX[index - 1] <= 0 && trixResult.TRIX[index] > 0) ||
                       (trixResult.TRIX[index - 1] >= 0 && trixResult.TRIX[index] < 0);
            }
            catch { return false; }
        }

        private bool IsTRIXSignalCrossover(int index)
        {
            return IsTRIXSignalLineCrossoverBullish(index) || IsTRIXSignalLineCrossoverBearish(index);
        }

        private bool IsTRIXShowingBullishTrend(int index)
        {
            return IsTRIXBullish(index) && IsTRIXTrendStrengthening(index);
        }

        private bool IsTRIXShowingBearishTrend(int index)
        {
            return IsTRIXBearish(index) && IsTRIXTrendStrengthening(index);
        }

        #endregion


        #region Вспомогательные методы для индикаторов

        private double GetRSIValue(int index)
        {
            try
            {
                var rsiResult = _rsiIndicator?.Calculate();
                return rsiResult?.Any() != null && index < rsiResult.Length ? rsiResult[index] : 50;
            }
            catch { return 50; }
        }

        private bool IsRSIRising(int index)
        {
            try
            {
                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() == null || index < 1 || index >= rsiResult.Length)
                    return false;
                return rsiResult[index] > rsiResult[index - 1];
            }
            catch { return false; }
        }

        private bool IsRSIFalling(int index)
        {
            try
            {
                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() == null || index < 1 || index >= rsiResult.Length)
                    return false;
                return rsiResult[index] < rsiResult[index - 1];
            }
            catch { return false; }
        }

        private bool IsMACDBullish(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || macdResult?.SignalLine == null ||
                    index >= macdResult.MACDLine.Length || index >= macdResult.SignalLine.Length)
                    return false;
                return macdResult.MACDLine[index] > macdResult.SignalLine[index];
            }
            catch { return false; }
        }

        private bool IsMACDBearish(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || macdResult?.SignalLine == null ||
                    index >= macdResult.MACDLine.Length || index >= macdResult.SignalLine.Length)
                    return false;
                return macdResult.MACDLine[index] < macdResult.SignalLine[index];
            }
            catch { return false; }
        }

        private bool IsMACDHistogramRising(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.Histogram == null || index < 1 || index >= macdResult.Histogram.Length)
                    return false;
                return macdResult.Histogram[index] > macdResult.Histogram[index - 1];
            }
            catch { return false; }
        }

        private bool IsMACDHistogramFalling(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.Histogram == null || index < 1 || index >= macdResult.Histogram.Length)
                    return false;
                return macdResult.Histogram[index] < macdResult.Histogram[index - 1];
            }
            catch { return false; }
        }

        private bool IsADXBullish(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.ADX == null || index >= adxResult.ADX.Length)
                    return false;
                // ADX > 25 и растет, плюс проверяем DI+ > DI-
                return adxResult.ADX[index] > 25 && adxResult.PlusDI[index] > adxResult.MinusDI[index];
            }
            catch { return false; }
        }

        private bool IsADXBearish(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.ADX == null || index >= adxResult.ADX.Length)
                    return false;
                // ADX > 25 и растет, плюс проверяем DI- > DI+
                return adxResult.ADX[index] > 25 && adxResult.PlusDI[index] > adxResult.MinusDI[index];
            }
            catch { return false; }
        }

        private bool IsStochasticBullish(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || stochResult?.PercentD == null || index >= stochResult.PercentK.Length)
                    return false;
                // %K < 20 и %K > %D (пересечение снизу вверх)
                return stochResult.PercentK[index] < 20 && stochResult.PercentK[index] > stochResult.PercentD[index];
            }
            catch { return false; }
        }

        private bool IsStochasticBearish(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || stochResult?.PercentD == null || index >= stochResult.PercentK.Length)
                    return false;
                // %K > 80 и %K < %D (пересечение сверху вниз)
                return stochResult.PercentK[index] > 80 && stochResult.PercentK[index] < stochResult.PercentD[index];
            }
            catch { return false; }
        }

        private bool IsTRIXBullish(int index)
        {
            try
            {
                var trixResult = _trixIndicator?.Calculate();
                if (trixResult?.TRIX == null ||
                    index >= trixResult.TRIX.Length)
                    return false;
                return trixResult.TRIX[index] > 0;
            }
            catch { return false; }
        }

        private bool IsTRIXBearish(int index)
        {
            try
            {
                var trixResult = _trixIndicator?.Calculate();
                if (trixResult?.TRIX == null ||
                    index >= trixResult.TRIX.Length)
                    return false;
                return trixResult.TRIX[index] < 0;
            }
            catch { return false; }
        }


        // Дополнительные методы для RSI
        private bool IsRSIDivergenceBullish(int index, List<CandleDTO> candles)
        {
            try
            {
                if (index < 5 || candles == null || candles.Count <= index) return false;

                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() != true || index >= rsiResult.Length) return false;

                // Ищем бычью дивергенцию: цена делает новый минимум, а RSI - нет
                var priceLow1 = candles[index].Low;
                var priceLow2 = candles[index - 5].Low;
                var rsiValue1 = rsiResult[index];
                var rsiValue2 = rsiResult[index - 5];

                return priceLow1 < priceLow2 && rsiValue1 > rsiValue2 && rsiValue1 < 35;
            }
            catch { return false; }
        }

        private bool IsRSIDivergenceBearish(int index, List<CandleDTO> candles)
        {
            try
            {
                if (index < 5 || candles == null || candles.Count <= index) return false;

                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() != true || index >= rsiResult.Length) return false;

                // Ищем медвежью дивергенцию: цена делает новый максимум, а RSI - нет
                var priceHigh1 = candles[index].High;
                var priceHigh2 = candles[index - 5].High;
                var rsiValue1 = rsiResult[index];
                var rsiValue2 = rsiResult[index - 5];

                return priceHigh1 > priceHigh2 && rsiValue1 < rsiValue2 && rsiValue1 > 65;
            }
            catch { return false; }
        }

        private bool IsRSICrossover(int index)
        {
            try
            {
                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() != true || index < 1 || index >= rsiResult.Length) return false;

                // RSI пересекает 50 снизу вверх
                return rsiResult[index - 1] <= 50 && rsiResult[index] > 50;
            }
            catch { return false; }
        }

        private bool IsRSICrossunder(int index)
        {
            try
            {
                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() != true || index < 1 || index >= rsiResult.Length) return false;

                // RSI пересекает 50 сверху вниз
                return rsiResult[index - 1] >= 50 && rsiResult[index] < 50;
            }
            catch { return false; }
        }

        // Дополнительные методы для MACD
        private bool IsMACDCrossoverBullish(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || macdResult?.SignalLine == null ||
                    index < 1 || index >= macdResult.MACDLine.Length) return false;

                // MACD пересекает сигнальную линию снизу вверх
                return macdResult.MACDLine[index - 1] <= macdResult.SignalLine[index - 1] &&
                       macdResult.MACDLine[index] > macdResult.SignalLine[index];
            }
            catch { return false; }
        }

        private bool IsMACDCrossoverBearish(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || macdResult?.SignalLine == null ||
                    index < 1 || index >= macdResult.MACDLine.Length) return false;

                // MACD пересекает сигнальную линию сверху вниз
                return macdResult.MACDLine[index - 1] >= macdResult.SignalLine[index - 1] &&
                       macdResult.MACDLine[index] < macdResult.SignalLine[index];
            }
            catch { return false; }
        }

        private bool IsMACDZeroCrossBullish(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || index < 1 || index >= macdResult.MACDLine.Length)
                    return false;

                // MACD пересекает нулевую линию снизу вверх
                return macdResult.MACDLine[index - 1] <= 0 && macdResult.MACDLine[index] > 0;
            }
            catch { return false; }
        }

        private bool IsMACDZeroCrossBearish(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || index < 1 || index >= macdResult.MACDLine.Length)
                    return false;

                // MACD пересекает нулевую линию сверху вниз
                return macdResult.MACDLine[index - 1] >= 0 && macdResult.MACDLine[index] < 0;
            }
            catch { return false; }
        }

        // Дополнительные методы для ADX
        private bool IsADXStrengthening(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.ADX == null || index < 1 || index >= adxResult.ADX.Length)
                    return false;

                // ADX растет (тренд усиливается)
                return adxResult.ADX[index] > adxResult.ADX[index - 1] && adxResult.ADX[index] > 20;
            }
            catch { return false; }
        }

        private bool IsADXWeakening(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.ADX == null || index < 1 || index >= adxResult.ADX.Length)
                    return false;

                // ADX падает (тренд ослабевает)
                return adxResult.ADX[index] < adxResult.ADX[index - 1] && adxResult.ADX[index] < 25;
            }
            catch { return false; }
        }

        private bool IsDICrossoverBullish(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.PlusDI == null || adxResult?.MinusDI == null ||
                    index < 1 || index >= adxResult.PlusDI.Length)
                    return false;

                // DI+ пересекает DI- снизу вверх
                return adxResult.PlusDI[index - 1] <= adxResult.MinusDI[index - 1] &&
                       adxResult.PlusDI[index] > adxResult.MinusDI[index];
            }
            catch { return false; }
        }

        private bool IsDICrossoverBearish(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.PlusDI == null || adxResult?.MinusDI == null ||
                    index < 1 || index >= adxResult.PlusDI.Length)
                    return false;

                // DI- пересекает DI+ снизу вверх
                return adxResult.MinusDI[index - 1] <= adxResult.PlusDI[index - 1] &&
                       adxResult.MinusDI[index] > adxResult.PlusDI[index];
            }
            catch { return false; }
        }

        // Дополнительные методы для Stochastic
        private bool IsStochasticCrossoverBullish(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || stochResult?.PercentD == null ||
                    index < 1 || index >= stochResult.PercentK.Length)
                    return false;

                // %K пересекает %D снизу вверх в зоне перепроданности
                return stochResult.PercentK[index - 1] <= stochResult.PercentD[index - 1] &&
                       stochResult.PercentK[index] > stochResult.PercentD[index] &&
                       stochResult.PercentK[index] < 30;
            }
            catch { return false; }
        }

        private bool IsStochasticCrossoverBearish(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || stochResult?.PercentD == null ||
                    index < 1 || index >= stochResult.PercentK.Length)
                    return false;

                // %K пересекает %D сверху вниз в зоне перекупленности
                return stochResult.PercentK[index - 1] >= stochResult.PercentD[index - 1] &&
                       stochResult.PercentK[index] < stochResult.PercentD[index] &&
                       stochResult.PercentK[index] > 70;
            }
            catch { return false; }
        }

        private bool IsStochasticExtremeOversold(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || index >= stochResult.PercentK.Length)
                    return false;

                // Экстремальная перепроданность
                return stochResult.PercentK[index] < 10;
            }
            catch { return false; }
        }

        private bool IsStochasticExtremeOverbought(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || index >= stochResult.PercentK.Length)
                    return false;

                // Экстремальная перекупленность
                return stochResult.PercentK[index] > 90;
            }
            catch { return false; }
        }

        // Дополнительные методы для TRIX
        private bool IsTRIXSignalLineCrossoverBullish(int index)
        {
            try
            {
                var trixResult = _trixIndicator?.Calculate();
                if (trixResult?.TRIX == null || trixResult?.Signal == null ||
                    index < 1 || index >= trixResult.TRIX.Length)
                    return false;

                // TRIX пересекает сигнальную линию снизу вверх
                return trixResult.TRIX[index - 1] <= trixResult.Signal[index - 1] &&
                       trixResult.TRIX[index] > trixResult.Signal[index];
            }
            catch { return false; }
        }

        private bool IsTRIXSignalLineCrossoverBearish(int index)
        {
            try
            {
                var trixResult = _trixIndicator?.Calculate();
                if (trixResult?.TRIX == null || trixResult?.Signal == null ||
                    index < 1 || index >= trixResult.TRIX.Length)
                    return false;

                // TRIX пересекает сигнальную линию сверху вниз
                return trixResult.TRIX[index - 1] >= trixResult.Signal[index - 1] &&
                       trixResult.TRIX[index] < trixResult.Signal[index];
            }
            catch { return false; }
        }

        private bool IsTRIXTrendStrengthening(int index)
        {
            try
            {
                var trixResult = _trixIndicator?.Calculate();
                if (trixResult?.TRIX == null || index < 2 || index >= trixResult.TRIX.Length)
                    return false;

                // TRIX показывает усиление тренда (3 периода подряд в одном направлении)
                return (trixResult.TRIX[index] > trixResult.TRIX[index - 1] &&
                        trixResult.TRIX[index - 1] > trixResult.TRIX[index - 2]) ||
                       (trixResult.TRIX[index] < trixResult.TRIX[index - 1] &&
                        trixResult.TRIX[index - 1] < trixResult.TRIX[index - 2]);
            }
            catch { return false; }
        }

        private bool IsMACDInBullishState(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || macdResult?.SignalLine == null ||
                    index >= macdResult.MACDLine.Length)
                    return false;

                // MACD в бычьем состоянии: выше нуля И выше сигнальной линии
                return macdResult.MACDLine[index] > 0 && macdResult.MACDLine[index] > macdResult.SignalLine[index];
            }
            catch { return false; }
        }

        private bool IsMACDInBearishState(int index)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || macdResult?.SignalLine == null ||
                    index >= macdResult.MACDLine.Length)
                    return false;

                // MACD в медвежьем состоянии: ниже нуля И ниже сигнальной линии
                return macdResult.MACDLine[index] < 0 && macdResult.MACDLine[index] < macdResult.SignalLine[index];
            }
            catch { return false; }
        }

        private bool IsRSIConsistentlyRising(int index)
        {
            try
            {
                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() != true || index < 4 || index >= rsiResult.Length)
                    return false;

                // Проверяем, что RSI растет в большинстве случаев за последние 5 периодов
                int risingCount = 0;
                for (int i = 1; i <= 4; i++)
                {
                    if (rsiResult[index - i + 1] > rsiResult[index - i])
                        risingCount++;
                }

                return risingCount >= 3; // Минимум 3 из 4 периодов показывают рост
            }
            catch { return false; }
        }

        private bool IsRSIConsistentlyFalling(int index)
        {
            try
            {
                var rsiResult = _rsiIndicator?.Calculate();
                if (rsiResult?.Any() != true || index < 4 || index >= rsiResult.Length)
                    return false;

                // Проверяем, что RSI падает в большинстве случаев за последние 5 периодов
                int fallingCount = 0;
                for (int i = 1; i <= 4; i++)
                {
                    if (rsiResult[index - i + 1] < rsiResult[index - i])
                        fallingCount++;
                }

                return fallingCount >= 3; // Минимум 3 из 4 периодов показывают падение
            }
            catch { return false; }
        }

        private bool IsMACDMomentumStrong(int index, bool bullish)
        {
            try
            {
                var macdResult = _macdIndicator?.Calculate();
                if (macdResult?.MACDLine == null || macdResult?.SignalLine == null ||
                    index >= macdResult.MACDLine.Length)
                    return false;

                double difference = Math.Abs(macdResult.MACDLine[index] - macdResult.SignalLine[index]);

                // Считаем среднее расстояние за последние 10 периодов для нормализации
                double avgDifference = 0;
                int count = 0;
                for (int i = Math.Max(0, index - 9); i <= index; i++)
                {
                    if (i < macdResult.MACDLine.Length)
                    {
                        avgDifference += Math.Abs(macdResult.MACDLine[i] - macdResult.SignalLine[i]);
                        count++;
                    }
                }
                avgDifference = count > 0 ? avgDifference / count : 0;

                // Сильный импульс: текущая разница больше среднего в 1.5 раза
                bool isStrong = difference > avgDifference * 1.5;

                if (bullish)
                    return isStrong && macdResult.MACDLine[index] > macdResult.SignalLine[index];
                else
                    return isStrong && macdResult.MACDLine[index] < macdResult.SignalLine[index];
            }
            catch { return false; }
        }

        private bool IsADXShowingStrongBullTrend(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.ADX == null || adxResult?.PlusDI == null || adxResult?.MinusDI == null ||
                    index >= adxResult.ADX.Length)
                    return false;

                // Сильный бычий тренд: ADX > 25, DI+ > DI- с хорошим разрывом (минимум 5 пунктов)
                return adxResult.ADX[index] > 25 &&
                       adxResult.PlusDI[index] > adxResult.MinusDI[index] &&
                       (adxResult.PlusDI[index] - adxResult.MinusDI[index]) > 5;
            }
            catch { return false; }
        }

        private bool IsADXShowingStrongBearTrend(int index)
        {
            try
            {
                var adxResult = _adxIndicator?.Calculate();
                if (adxResult?.ADX == null || adxResult?.PlusDI == null || adxResult?.MinusDI == null ||
                    index >= adxResult.ADX.Length)
                    return false;

                // Сильный медвежий тренд: ADX > 25, DI- > DI+ с хорошим разрывом (минимум 5 пунктов)
                return adxResult.ADX[index] > 25 &&
                       adxResult.MinusDI[index] > adxResult.PlusDI[index] &&
                       (adxResult.MinusDI[index] - adxResult.PlusDI[index]) > 5;
            }
            catch { return false; }
        }

        private bool IsStochasticInBullishPosition(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || stochResult?.PercentD == null ||
                    index >= stochResult.PercentK.Length)
                    return false;

                // Бычья позиция: %K в нормальной зоне торговли (20-80) и выше %D
                return stochResult.PercentK[index] >= 20 &&
                       stochResult.PercentK[index] <= 80 &&
                       stochResult.PercentK[index] > stochResult.PercentD[index];
            }
            catch { return false; }
        }

        private bool IsStochasticInBearishPosition(int index)
        {
            try
            {
                var stochResult = _stochasticIndicator?.Calculate();
                if (stochResult?.PercentK == null || stochResult?.PercentD == null ||
                    index >= stochResult.PercentK.Length)
                    return false;

                // Медвежья позиция: %K в нормальной зоне торговли (20-80) и ниже %D
                return stochResult.PercentK[index] >= 20 &&
                       stochResult.PercentK[index] <= 80 &&
                       stochResult.PercentK[index] < stochResult.PercentD[index];
            }
            catch { return false; }
        }

        #endregion
    }
}
