using System;
using System.Collections.Generic;

namespace UnTraid.DTO
{
    public class TInvestDTO
    {
        // DTO классы для работы с данными
        public class InstrumentDTO
        {
            public string Figi { get; set; }
            public string Ticker { get; set; }
            public string Name { get; set; }
            public string Currency { get; set; }
            public int Lot { get; set; }
            public string TradingStatus { get; set; }
        }

        public class CandleDTO
        {
            public DateTime Time { get; set; }
            public decimal Open { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal Close { get; set; }
            public long Volume { get; set; }
        }

        public class PortfolioDTO
        {
            public decimal TotalAmount { get; set; }
            public List<PositionDTO> Positions { get; set; } = new List<PositionDTO>();
        }

        public class PositionDTO
        {
            public string Figi { get; set; }
            public string InstrumentType { get; set; }
            public decimal Quantity { get; set; }
            public decimal AveragePositionPrice { get; set; }
            public decimal CurrentPrice { get; set; }
        }

        /// <summary>
        /// DTO для фундаментальных показателей финансовых инструментов
        /// </summary>
        public class FundamentalDTO
        {
            #region Базовая информация

            /// <summary>
            /// Уникальный идентификатор актива
            /// </summary>
            public string AssetUid { get; set; }

            /// <summary>
            /// Валюта, в которой торгуется инструмент
            /// </summary>
            public string Currency { get; set; }

            /// <summary>
            /// Код страны регистрации компании
            /// </summary>
            public string DomicileIndicatorCode { get; set; }

            #endregion

            #region Коэффициенты оценки (Valuation Ratios)

            /// <summary>
            /// P/E коэффициент за последние 12 месяцев (Price-to-Earnings Ratio TTM)
            /// Отношение цены акции к прибыли на акцию
            /// </summary>
            public decimal? PeRatioTtm { get; set; }

            /// <summary>
            /// P/B коэффициент за последние 12 месяцев (Price-to-Book Ratio TTM)
            /// Отношение рыночной цены акции к балансовой стоимости на акцию
            /// </summary>
            public decimal? PriceToBookTtm { get; set; }

            /// <summary>
            /// P/S коэффициент за последние 12 месяцев (Price-to-Sales Ratio TTM)
            /// Отношение рыночной капитализации к выручке
            /// </summary>
            public decimal? PriceToSalesTtm { get; set; }

            /// <summary>
            /// Отношение цены к свободному денежному потоку за последние 12 месяцев
            /// Показывает, сколько инвесторы готовы платить за каждый рубль свободного денежного потока
            /// </summary>
            public decimal? PriceToFreeCashFlowTtm { get; set; }

            /// <summary>
            /// EV/Sales - отношение стоимости предприятия к выручке
            /// Учитывает не только рыночную капитализацию, но и долг компании
            /// </summary>
            public decimal? EvToSales { get; set; }

            /// <summary>
            /// EV/EBITDA - отношение стоимости предприятия к EBITDA за последний квартал
            /// Популярный мультипликатор для оценки компаний
            /// </summary>
            public decimal? EvToEbitdaMrq { get; set; }

            #endregion

            #region Дивидендные показатели

            /// <summary>
            /// Дивидендная доходность за последние 12 месяцев (в процентах)
            /// Отношение дивидендов на акцию к цене акции
            /// </summary>
            public decimal? DividendYieldDailyTtm { get; set; }

            /// <summary>
            /// Размер дивидендов за последние 12 месяцев
            /// Общая сумма дивидендов, выплаченных за год
            /// </summary>
            public decimal? DividendRateTtm { get; set; }

            /// <summary>
            /// Дивиденды на акцию
            /// Размер дивидендов, приходящихся на одну акцию
            /// </summary>
            public decimal? DividendsPerShare { get; set; }

            /// <summary>
            /// Коэффициент выплат дивидендов за финансовый год (в долях от 0 до 1)
            /// Доля чистой прибыли, направляемая на выплату дивидендов
            /// </summary>
            public decimal? DividendPayoutRatioFy { get; set; }

            /// <summary>
            /// Форвардная годовая дивидендная доходность
            /// Прогнозируемая дивидендная доходность на следующий год
            /// </summary>
            public decimal? ForwardAnnualDividendYield { get; set; }

            /// <summary>
            /// Средняя дивидендная доходность за последние 5 лет
            /// Исторический показатель стабильности дивидендных выплат
            /// </summary>
            public decimal? FiveYearsAverageDividendYield { get; set; }

            /// <summary>
            /// Среднегодовой темп роста дивидендов за 5 лет (в процентах)
            /// Показывает, как быстро растут дивидендные выплаты
            /// </summary>
            public decimal? FiveYearAnnualDividendGrowthRate { get; set; }

            /// <summary>
            /// Дата отсечки для получения дивидендов
            /// Последняя дата, когда нужно было владеть акцией для получения дивидендов
            /// </summary>
            public DateTime? ExDividendDate { get; set; }

            #endregion

            #region Показатели прибыльности

            /// <summary>
            /// Прибыль на акцию за последние 12 месяцев (Earnings Per Share TTM)
            /// Чистая прибыль компании, деленная на количество акций в обращении
            /// </summary>
            public decimal? EpsTtm { get; set; }

            /// <summary>
            /// Разводненная прибыль на акцию за последние 12 месяцев
            /// EPS с учетом потенциального выпуска новых акций (опционы, конвертируемые облигации)
            /// </summary>
            public decimal? DilutedEpsTtm { get; set; }

            /// <summary>
            /// Чистая прибыль за последние 12 месяцев
            /// Итоговая прибыль компании после всех расходов и налогов
            /// </summary>
            public decimal? NetIncomeTtm { get; set; }

            /// <summary>
            /// Чистая маржа за последний квартал (в процентах)
            /// Доля чистой прибыли в выручке
            /// </summary>
            public decimal? NetMarginMrq { get; set; }

            /// <summary>
            /// ROA - рентабельность активов (Return on Assets)
            /// Показывает, насколько эффективно компания использует свои активы для генерации прибыли
            /// </summary>
            public decimal? Roa { get; set; }

            /// <summary>
            /// ROE - рентабельность собственного капитала (Return on Equity)
            /// Показывает доходность вложений акционеров
            /// </summary>
            public decimal? Roe { get; set; }

            /// <summary>
            /// ROIC - рентабельность инвестированного капитала (Return on Invested Capital)
            /// Эффективность использования всего инвестированного капитала
            /// </summary>
            public decimal? Roic { get; set; }

            #endregion

            #region Показатели выручки и роста

            /// <summary>
            /// Выручка за последние 12 месяцев (Revenue TTM)
            /// Общий доход компании от основной деятельности
            /// </summary>
            public decimal? RevenueTtm { get; set; }

            /// <summary>
            /// Годовой темп роста выручки за 1 год (в процентах)
            /// Сравнение выручки текущего года с предыдущим
            /// </summary>
            public decimal? OneYearAnnualRevenueGrowthRate { get; set; }

            /// <summary>
            /// Среднегодовой темп роста выручки за 3 года (в процентах)
            /// Показывает среднесрочную динамику роста бизнеса
            /// </summary>
            public decimal? ThreeYearAnnualRevenueGrowthRate { get; set; }

            /// <summary>
            /// Среднегодовой темп роста выручки за 5 лет (в процентах)
            /// Долгосрочный тренд развития компании
            /// </summary>
            public decimal? FiveYearAnnualRevenueGrowthRate { get; set; }

            /// <summary>
            /// Изменение выручки за 5 лет (в процентах)
            /// Общий рост выручки за пятилетний период
            /// </summary>
            public decimal? RevenueChangeFiveYears { get; set; }

            #endregion

            #region EBITDA показатели

            /// <summary>
            /// EBITDA за последние 12 месяцев
            /// Прибыль до вычета процентов, налогов, износа и амортизации
            /// Показывает операционную эффективность компании
            /// </summary>
            public decimal? EbitdaTtm { get; set; }

            /// <summary>
            /// Изменение EBITDA за 5 лет (в процентах)
            /// Динамика операционной прибыльности компании
            /// </summary>
            public decimal? EbitdaChangeFiveYears { get; set; }

            #endregion

            #region Изменения показателей за 5 лет

            /// <summary>
            /// Изменение прибыли на акцию за 5 лет (в процентах)
            /// Долгосрочная динамика прибыльности
            /// </summary>
            public decimal? EpsChangeFiveYears { get; set; }

            /// <summary>
            /// Изменение общего долга за 5 лет (в процентах)
            /// Показывает, как изменилась долговая нагрузка компании
            /// </summary>
            public decimal? TotalDebtChangeFiveYears { get; set; }

            #endregion

            #region Долговые показатели

            /// <summary>
            /// Общий долг за последний квартал
            /// Суммарная задолженность компании (краткосрочная + долгосрочная)
            /// </summary>
            public decimal? TotalDebtMrq { get; set; }

            /// <summary>
            /// Отношение общего долга к собственному капиталу за последний квартал
            /// Показывает степень закредитованности компании
            /// </summary>
            public decimal? TotalDebtToEquityMrq { get; set; }

            /// <summary>
            /// Отношение общего долга к EBITDA за последний квартал
            /// Показывает, за сколько лет компания сможет погасить долг из операционной прибыли
            /// </summary>
            public decimal? TotalDebtToEbitdaMrq { get; set; }

            /// <summary>
            /// Отношение чистого долга к EBITDA
            /// Чистый долг = общий долг - денежные средства
            /// </summary>
            public decimal? NetDebtToEbitda { get; set; }

            #endregion

            #region Капитализация и акции

            /// <summary>
            /// Рыночная капитализация
            /// Общая рыночная стоимость всех акций компании в обращении
            /// </summary>
            public decimal? MarketCapitalization { get; set; }

            /// <summary>
            /// Количество акций в обращении
            /// Общее количество выпущенных и находящихся в обращении акций
            /// </summary>
            public decimal? SharesOutstanding { get; set; }

            /// <summary>
            /// Свободное обращение (Free Float)
            /// Доля акций, доступных для торговли на открытом рынке
            /// </summary>
            public decimal? FreeFloat { get; set; }

            #endregion

            #region Денежные потоки

            /// <summary>
            /// Свободный денежный поток за последние 12 месяцев
            /// Денежные средства, остающиеся после капитальных затрат
            /// </summary>
            public decimal? FreeCashFlowTtm { get; set; }

            /// <summary>
            /// Отношение свободного денежного потока к цене
            /// Показывает эффективность генерации денежных средств
            /// </summary>
            public decimal? FreeCashFlowToPrice { get; set; }

            #endregion

            #region Ценовые показатели

            /// <summary>
            /// Максимальная цена за последние 52 недели
            /// Наивысшая торговая цена акции за год
            /// </summary>
            public decimal? HighPriceLast52Weeks { get; set; }

            /// <summary>
            /// Минимальная цена за последние 52 недели
            /// Наименьшая торговая цена акции за год
            /// </summary>
            public decimal? LowPriceLast52Weeks { get; set; }

            #endregion

            #region Торговые показатели

            /// <summary>
            /// Средний дневной объем торгов за последние 4 недели
            /// Ликвидность акции в краткосрочном периоде
            /// </summary>
            public decimal? AverageDailyVolumeLast4Weeks { get; set; }

            /// <summary>
            /// Средний дневной объем торгов за последние 10 дней
            /// Текущая ликвидность акции
            /// </summary>
            public decimal? AverageDailyVolumeLast10Days { get; set; }

            /// <summary>
            /// Коэффициент бета
            /// Показатель волатильности акции относительно рынка в целом
            /// Бета = 1: волатильность как у рынка, >1: более волатильна, <1: менее волатильна
            /// </summary>
            public decimal? Beta { get; set; }

            #endregion

            #region Прочие финансовые показатели

            /// <summary>
            /// Общая стоимость предприятия за последний квартал (Enterprise Value)
            /// Рыночная капитализация + общий долг - денежные средства
            /// </summary>
            public decimal? TotalEnterpriseValueMrq { get; set; }

            /// <summary>
            /// Коэффициент текущей ликвидности за последний квартал
            /// Отношение оборотных активов к краткосрочным обязательствам
            /// </summary>
            public decimal? CurrentRatioMrq { get; set; }

            /// <summary>
            /// Чистая процентная маржа за последний квартал (для банков)
            /// Разница между процентными доходами и расходами
            /// </summary>
            public decimal? NetInterestMarginMrq { get; set; }

            /// <summary>
            /// Коэффициент покрытия фиксированных расходов за финансовый год
            /// Способность компании покрывать постоянные расходы
            /// </summary>
            public decimal? FixedChargeCoverageRatioFy { get; set; }

            /// <summary>
            /// Количество сотрудников
            /// Общая численность персонала компании
            /// </summary>
            public decimal? NumberOfEmployees { get; set; }

            /// <summary>
            /// Соотношение ADR к обыкновенным акциям
            /// Для американских депозитарных расписок
            /// </summary>
            public decimal? AdrToCommonShareRatio { get; set; }

            /// <summary>
            /// Объем выкупа собственных акций за последние 12 месяцев
            /// Сумма, потраченная компанией на обратный выкуп акций
            /// </summary>
            public decimal? BuyBackTtm { get; set; }

            #endregion

            #region Отчетные периоды

            /// <summary>
            /// Дата начала финансового периода
            /// Начало отчетного периода для финансовых показателей
            /// </summary>
            public DateTime? FiscalPeriodStartDate { get; set; }

            /// <summary>
            /// Дата окончания финансового периода
            /// Конец отчетного периода для финансовых показателей
            /// </summary>
            public DateTime? FiscalPeriodEndDate { get; set; }

            #endregion
        }
    }
}
