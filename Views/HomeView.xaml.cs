using FinamParser;
using NPOI.HSSF.Record.Chart;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UnTraid.Core;
using UnTraid.Core.Indicators;
using UnTraid.Core.Indicators.Calculators;
using UnTraid.Core.Indicators.Servise;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Views
{
    public partial class HomeView : UserControl
    {
        private TInvestConnect _tinkoffClient;
        private string _selectedAsset;
        private Dictionary<string, Border> _assetBorders;
        private Dictionary<string, string> _assetNames;
        private Dictionary<string, Button> _timeFrameButtons;
        private string _selectedTimeFrame = "CANDLE_INTERVAL_DAY";
        private bool _isSearchTextPlaceholder = true;
        private List<CandleDTO> _currentCandles;
        private List<CandleDTO> _candles;
        private List<ScottPlot.WpfPlot> _indicatorPlots;
        private Dictionary<string, ScottPlot.WpfPlot> _indicatorPlotMapping;
        private FundamentalDTO _fundamentalData;

        public HomeView()
        {
            InitializeComponent();

            Loaded += UserControl_Loaded;

            InitializeAssets();
            LoadTinkoffClient();
        }

        private void InitializeIndicatorPlots()
        {
            _indicatorPlots = new List<ScottPlot.WpfPlot>();
            _indicatorPlotMapping = new Dictionary<string, ScottPlot.WpfPlot>();
        }

        private void InitializeAssets()
        {
            // Связываем кнопки с их Border элементами
            _assetBorders = new Dictionary<string, Border>
            {
                { "SBER", SberBorder },
                { "GAZP", GazpBorder },
                { "LKOH", LkohBorder },
                { "NVTK", NvtkBorder },
                { "YNDX", YndxBorder }
            };

            // Названия активов
            _assetNames = new Dictionary<string, string>
            {
                { "SBER", "Сбербанк" },
                { "GAZP", "Газпром" },
                { "LKOH", "Лукойл" },
                { "NVTK", "Новатэк" },
                { "YNDX", "Яндекс" },
                { "MOEX", "Московская биржа" },
                { "ROSN", "Роснефть" },
                { "TATN", "Татнефть" },
                { "MAGN", "ММК" },
                { "NLMK", "НЛМК" },
                { "VTBR", "ВТБ" },
                { "ALRS", "Алроса" }
            };
        }

        private void InitializeTimeFrames()
        {
            _timeFrameButtons = new Dictionary<string, Button>
            {
                { "CANDLE_INTERVAL_5_MIN", TimeFrame5M },
                { "CANDLE_INTERVAL_15_MIN", TimeFrame15M },
                { "CANDLE_INTERVAL_30_MIN", TimeFrame30M },
                { "CANDLE_INTERVAL_HOUR", TimeFrame1H },
                { "CANDLE_INTERVAL_2_HOUR", TimeFrame2H },
                { "CANDLE_INTERVAL_4_HOUR", TimeFrame4H },
                { "CANDLE_INTERVAL_DAY", TimeFrame1D },
                { "CANDLE_INTERVAL_WEEK", TimeFrame1W }
            };
        }

        private async void LoadTinkoffClient()
        {
            try
            {
                _tinkoffClient = new TInvestConnect();
                bool isConnected = await _tinkoffClient.CheckTokenAsync();

                if (!isConnected)
                {
                    MessageBox.Show("Ошибка подключения к Tinkoff Invest API. Проверьте токен в настройках.",
                        "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Asset Selection

        private async void AssetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string assetCode)
            {
                await SelectAsset(assetCode);
            }
        }

        private void OtherButton_Click(object sender, RoutedEventArgs e)
        {
            OtherAssetsPopup.IsOpen = true;
        }

        private async void OtherAssetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OtherAssetsListBox.SelectedItem is ListBoxItem item && item.Tag is string assetCode)
            {
                OtherAssetsPopup.IsOpen = false;
                await SelectAsset(assetCode);
            }
        }

        private async Task SelectAsset(string assetCode)
        {
            try
            {
                // Снимаем выделение с предыдущего актива
                if (!string.IsNullOrEmpty(_selectedAsset) && _assetBorders.ContainsKey(_selectedAsset))
                {
                    _assetBorders[_selectedAsset].Style = (System.Windows.Style)FindResource("UnselectedAssetStyle");
                }

                // Выделяем новый актив
                if (_assetBorders.ContainsKey(assetCode))
                {
                    _assetBorders[assetCode].Style = (System.Windows.Style)FindResource("SelectedAssetStyle");
                }

                _selectedAsset = assetCode;

                // Обновляем заголовок
                ChartTitleTextBlock.Text = $"{assetCode} - {(_assetNames.ContainsKey(assetCode) ? _assetNames[assetCode] : assetCode)}";
                AssetNameTextBlock.Text = $"{assetCode} - {(_assetNames.ContainsKey(assetCode) ? _assetNames[assetCode] : assetCode)}";

                // Загружаем данные для актива
                await LoadAssetData(assetCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выбора актива: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Time Frame Selection

        private async void TimeFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string timeFrame)
            {
                await SelectTimeFrame(timeFrame);
            }
        }

        private async Task SelectTimeFrame(string timeFrame)
        {
            try
            {
                // Снимаем выделение с предыдущего временного диапазона
                if (_timeFrameButtons.ContainsKey(_selectedTimeFrame))
                {
                    _timeFrameButtons[_selectedTimeFrame].Style = (System.Windows.Style)FindResource("TimeFrameButtonStyle");
                }

                // Выделяем новый временной диапазон
                if (_timeFrameButtons.ContainsKey(timeFrame))
                {
                    _timeFrameButtons[timeFrame].Style = (System.Windows.Style)FindResource("SelectedTimeFrameButtonStyle");
                }

                _selectedTimeFrame = timeFrame;

                // Перезагружаем данные с новым временным диапазоном
                if (!string.IsNullOrEmpty(_selectedAsset))
                {
                    await LoadAssetData(_selectedAsset);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены временного диапазона: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Search Functionality

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isSearchTextPlaceholder)
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                _isSearchTextPlaceholder = false;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Поиск активов...";
                SearchTextBox.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                _isSearchTextPlaceholder = true;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSearchTextPlaceholder) return;

            string searchText = SearchTextBox.Text.ToLower();

            // Фильтруем элементы в ListBox
            foreach (ListBoxItem item in OtherAssetsListBox.Items)
            {
                string content = item.Content.ToString().ToLower();
                item.Visibility = content.Contains(searchText) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Data Loading

        private async Task LoadAssetData(string assetCode)
        {
            if (_tinkoffClient == null)
            {
                // Если нет подключения к API, показываем заглушечные данные
                //LoadMockData(assetCode);
                return;
            }

            try
            {
                LoadingTextBlock.Visibility = Visibility.Visible;
                LoadingTextBlock.Text = "Загрузка данных...";

                // Поиск инструмента
                //var instruments = await _tinkoffClient.SearchInstrumentAsync(assetCode, "INSTRUMENT_TYPE_CURRENCY");
                var instruments = await _tinkoffClient.SearchInstrumentAsync(assetCode); 
                var instrument = _tinkoffClient.GetFirstMatchingFigi(instruments);

                if (instrument == null)
                {
                    MessageBox.Show($"Инструмент {assetCode} не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //LoadMockData(assetCode);
                    return;
                }

                // Получение текущей цены
                //var currentPrice = await _tinkoffClient.GetLastPriceAsync(instrument.Figi);

                // Получение исторических данных с учетом временного диапазона
                var (fromDate, toDate) = GetDateRangeForTimeFrame(_selectedTimeFrame);
                var candles = await _tinkoffClient.GetCandlesAsync(
                    instrument.Figi,
                    fromDate,
                    toDate,
                    _selectedTimeFrame
                );

                _fundamentalData = await _tinkoffClient.GetAssetFundamentalsAsync(instrument.Figi);

                _currentCandles = candles;

                // Обновление UI с данными
                //UpdatePriceInfo(currentPrice.Value, _candles);
                UpdateScottPlotChart(candles);
                UpdateFundamentalData();
                //UpdateStrongScore();

                LoadingTextBlock.Text = "";
                LoadingTextBlock.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                //LoadMockData(assetCode);
            }
        }

        private void UpdateFundamentalData()
        {
            PeRatioValue.Text = _fundamentalData.PeRatioTtm.ToString();
            PriceToBookValue.Text = _fundamentalData.PriceToBookTtm.ToString();
            EvToEbitdaValue.Text = _fundamentalData.EvToEbitdaMrq.ToString();
            MarketCapValue.Text = _fundamentalData.MarketCapitalization.ToString();
            EpsValue.Text = _fundamentalData.EpsTtm.ToString();
            RoeValue.Text = _fundamentalData.Roe.ToString();
            NetMarginValue.Text = _fundamentalData.NetMarginMrq.ToString();
            RevenueValue.Text = _fundamentalData.RevenueTtm.ToString();
            RevenueGrowthValue.Text = _fundamentalData.OneYearAnnualRevenueGrowthRate.ToString();
            EpsGrowth5YValue.Text = _fundamentalData.EpsChangeFiveYears.ToString();
            FreeCashFlowValue.Text = _fundamentalData.FreeCashFlowTtm.ToString();
            DividendYieldValue.Text = _fundamentalData.DividendYieldDailyTtm.ToString();
            DividendPayoutValue.Text = _fundamentalData.DividendPayoutRatioFy.ToString();
            DebtToEquityValue.Text = _fundamentalData.TotalDebtToEquityMrq.ToString();
            NetDebtToEbitdaValue.Text = _fundamentalData.NetDebtToEbitda.ToString();
            BetaValue.Text = _fundamentalData.Beta.ToString();
            FreeFloatValue.Text = _fundamentalData.FreeFloat.ToString();
            EmployeesValue.Text = _fundamentalData.NumberOfEmployees.ToString();
        }

        private (DateTime fromDate, DateTime toDate) GetDateRangeForTimeFrame(string timeFrame)
        {
            var toDate = DateTime.Now;
            DateTime fromDate;

            switch (timeFrame)
            {
                case "CANDLE_INTERVAL_5_MIN":
                    fromDate = toDate.AddDays(-7);
                    break;
                case "CANDLE_INTERVAL_15_MIN":
                    fromDate = toDate.AddDays(-21);
                    break;
                case "CANDLE_INTERVAL_30_MIN":
                    fromDate = toDate.AddDays(-21);
                    break;
                case "CANDLE_INTERVAL_HOUR":
                    fromDate = toDate.AddMonths(-3); 
                    break;
                case "CANDLE_INTERVAL_2_HOUR":
                    fromDate = toDate.AddMonths(-3); 
                    break;
                case "CANDLE_INTERVAL_4_HOUR":
                    fromDate = toDate.AddMonths(-3); 
                    break;
                case "CANDLE_INTERVAL_DAY":
                    fromDate = toDate.AddYears(-6);
                    break;
                case "CANDLE_INTERVAL_WEEK":
                    fromDate = toDate.AddYears(-5); 
                    break;
                default:
                    fromDate = toDate.AddDays(-365);
                    break;
            }

            return (fromDate, toDate);
        }

        //private void LoadMockData(string assetCode)
        //{
        //    try
        //    {
        //        // Генерируем фиктивные данные для демонстрации
        //        var random = new Random();
        //        var mockCandles = new List<CandleDTO>();
        //        var basePrice = GetBasePriceForAsset(assetCode);
        //        var (fromDate, toDate) = GetDateRangeForTimeFrame(_selectedTimeFrame);
        //        var timeSpan = GetTimeSpanForInterval(_selectedTimeFrame);

        //        var currentDate = fromDate;
        //        while (currentDate <= toDate)
        //        {
        //            var change = (random.NextDouble() - 0.5) * 0.1; // ±5% изменение
        //            var price = basePrice * (1 + change);

        //            mockCandles.Add(new CandleDTO
        //            {
        //                Time = currentDate,
        //                Open = (decimal)(price * 0.98),
        //                High = (decimal)(price * 1.02),
        //                Low = (decimal)(price * 0.96),
        //                Close = (decimal)price,
        //                Volume = random.Next(100000, 1000000)
        //            });

        //            currentDate = currentDate.Add(timeSpan);
        //        }

        //        _currentCandles = mockCandles;

        //        // Обновление UI с фиктивными данными
        //        var lastCandle = mockCandles.Last();
        //        UpdatePriceInfo(lastCandle.Close, mockCandles);
        //        UpdateScottPlotChart(mockCandles);
        //        //UpdateStrongScore();

        //        LoadingTextBlock.Text = "";
        //        LoadingTextBlock.Visibility = Visibility.Collapsed;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка загрузки демо-данных: {ex.Message}",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private TimeSpan GetTimeSpanForInterval(string interval)
        {
            switch (interval)
            {
                case "CANDLE_INTERVAL_5_MIN": return TimeSpan.FromMinutes(5);
                case "CANDLE_INTERVAL_15_MIN": return TimeSpan.FromMinutes(15);
                case "CANDLE_INTERVAL_30_MIN": return TimeSpan.FromMinutes(30);
                case "CANDLE_INTERVAL_HOUR": return TimeSpan.FromHours(1);
                case "CANDLE_INTERVAL_2_HOUR": return TimeSpan.FromHours(2);
                case "CANDLE_INTERVAL_4_HOUR": return TimeSpan.FromHours(4);
                case "CANDLE_INTERVAL_DAY": return TimeSpan.FromDays(1);
                case "CANDLE_INTERVAL_WEEK": return TimeSpan.FromDays(7);
                default: return TimeSpan.FromDays(1);
            }
        }

        private double GetBasePriceForAsset(string assetCode)
        {
            // Примерные базовые цены для российских акций
            var basePrices = new Dictionary<string, double>
            {
                { "SBER", 250.0 },
                { "GAZP", 150.0 },
                { "LKOH", 6000.0 },
                { "NVTK", 1200.0 },
                { "YNDX", 2500.0 },
                { "MOEX", 200.0 },
                { "ROSN", 450.0 },
                { "TATN", 650.0 },
                { "MAGN", 45.0 },
                { "NLMK", 180.0 },
                { "VTBR", 0.025 },
                { "ALRS", 80.0 }
            };

            return basePrices.ContainsKey(assetCode) ? basePrices[assetCode] : 100.0;
        }

        #endregion

        #region UI Updates

        private void UpdatePriceInfo(List<CandleDTO> candles)
        {
            try
            {
                if (candles == null || candles.Count == 0) return;

                var currentPrice = candles.Last().Close;

                var lastCandle = candles.Last();
                var previousCandle = candles.Count > 1 ? candles[candles.Count - 2] : lastCandle;

                // Обновляем цену
                PriceTextBlock.Text = $"{currentPrice:F2} ₽";

                // Рассчитываем изменение
                var change = currentPrice - previousCandle.Close;
                var changePercent = (change / previousCandle.Close) * 100;

                ChangeTextBlock.Text = $"{change:F2} ({changePercent:F2}%)";
                ChangeTextBlock.Foreground = change >= 0
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80))
                    : new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));

                // Обновляем объем
                VolumeTextBlock.Text = FormatVolume(lastCandle.Volume);

                UpdateDateTimeTextBlock.Text = candles.Last().Time.ToString("dd/MM/yyyy");

                // Время обновления
                UpdateTimeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления информации о цене: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatVolume(long volume)
        {
            if (volume >= 1000000)
                return $"{volume / 1000000.0:F1}M";
            else if (volume >= 1000)
                return $"{volume / 1000.0:F1}K";
            else
                return volume.ToString();
        }

        #region ScottPlot Chart Implementation

        private void UpdateScottPlotChart(List<CandleDTO> candles)
        {
            try
            {
                if (candles == null || candles.Count == 0)
                {
                    LoadingTextBlock.Visibility = Visibility.Visible;
                    LoadingTextBlock.Text = "Нет данных для отображения";
                    return;
                }

                _candles = GetTransformCandles(candles);



                // Очищаем предыдущие графики
                MainPlot.Plot.Clear();

                // Подготавливаем данные для свечного графика
                var ohlc = _candles.Select(c => new OHLC(
                    open: (double)c.Open,
                    high: (double)c.High,
                    low: (double)c.Low,
                    close: (double)c.Close,
                    timeSpan: GetTimeSpanForInterval(_selectedTimeFrame),
                    timeStart: c.Time
                )).ToArray();

                // Добавляем свечной график
                var candlestickPlot = MainPlot.Plot.AddCandlesticks(ohlc);
                candlestickPlot.ColorUp = System.Drawing.Color.FromArgb(76, 175, 80);
                candlestickPlot.ColorDown = System.Drawing.Color.FromArgb(244, 67, 54);

                // Настройка основного графика
                //MainPlot.Plot.YLabel("Цена, ₽");
                //MainPlot.Plot.XLabel(GetTimeFrameLabel(_selectedTimeFrame));

                string dateFormat = GetDateFormatForTimeFrame(_selectedTimeFrame);
                MainPlot.Plot.XAxis.TickLabelFormat(dateFormat, dateTimeFormat: true);

                SetChartFocusToLastCandles(_candles);

                // Обновляем график
                //MainPlot.Refresh();

                // Обновляем индикаторные графики отдельно
                UpdateIndicatorPlots(_candles);
                UpdatePriceInfo(_candles);

                LoadingTextBlock.Visibility = Visibility.Collapsed;
                LoadingTextBlock.Text = "";
            }
            catch (Exception ex)
            {
                LoadingTextBlock.Visibility = Visibility.Visible;
                LoadingTextBlock.Text = $"Ошибка отрисовки графика: {ex.Message}";
            }
        }

        private void NegativeBordButton_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(TextBord.Text) - 1;
            if (Math.Abs(value) < _candles.Count - 200)
            {
                TextBord.Text = value.ToString();
            }

            UpdateScottPlotChart(_currentCandles);
        }

        private void PositiveBordButton_Click(object sender, RoutedEventArgs e)
        {
            var value = Convert.ToInt32(TextBord.Text) + 1;
            if (Math.Abs(value) < _candles.Count - 200 && value != 1)
            {
                TextBord.Text = value.ToString();
            }

            UpdateScottPlotChart(_currentCandles);
        }

        private List<CandleDTO> GetTransformCandles(List<CandleDTO> candles)
        {
            var result = candles.GetRange(0, candles.Count + Convert.ToInt32(TextBord.Text));

            return result;
        }

        private void UpdateIndicatorPlots(List<CandleDTO> candles)
        {
            try
            {
                var prices = candles.Select(c => (double)c.Close).ToArray();
                var xs = candles.Select(c => c.Time.ToOADate()).ToArray();

                // Очищаем существующие индикаторные графики
                ClearIndicatorPlots();

                var visibleIndicators = GetVisibleIndicators();

                if (visibleIndicators.Count > 0)
                {
                    IndicatorsGrid.Visibility = Visibility.Visible;

                    // Создаем нужное количество рядов в Grid
                    CreateIndicatorRows(visibleIndicators.Count);

                    for (int i = 0; i < visibleIndicators.Count; i++)
                    {
                        var indicatorPlot = CreateIndicatorPlot();
                        PlotIndicator(indicatorPlot, visibleIndicators[i], candles, prices, xs);

                        // Размещаем график в Grid
                        Grid.SetRow(indicatorPlot, i + 1); // +1 потому что 0-й ряд занят основным графиком
                        IndicatorsGrid.Children.Add(indicatorPlot);

                        _indicatorPlots.Add(indicatorPlot);
                        _indicatorPlotMapping[visibleIndicators[i]] = indicatorPlot;

                        // Синхронизируем оси X с основным графиком
                        SynchronizeXAxis(indicatorPlot);
                    }

                    UpdateStrongScore(new IndicatorSignalScore(candles, visibleIndicators).Calculate());
                }
                else
                {
                    StrongScoreProgressBar.Value = 0;
                    StrongScoreProgressBarBullishScore.Value = 0;
                    StrongScoreProgressBarBearishScore.Value = 0;

                    StrongScoreTextBlock.Text = "—";
                    StrongScoreTextBlockBullishScore.Text = "—";
                    StrongScoreTextBlockBearishScore.Text = "—";

                    StrongScoreDescriptionTextBlock.Text = "Недостаточно данных";
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения индикаторов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearIndicatorPlots()
        {
            foreach (var plot in _indicatorPlots)
            {
                if (IndicatorsGrid.Children.Contains(plot))
                {
                    IndicatorsGrid.Children.Remove(plot);
                }
            }
            _indicatorPlots.Clear();
            _indicatorPlotMapping.Clear();
        }

        private void CreateIndicatorRows(int count)
        {
            // Очищаем существующие ряды (кроме первого для основного графика)
            var rowsToRemove = IndicatorsGrid.RowDefinitions.Skip(1).ToList();
            foreach (var row in rowsToRemove)
            {
                IndicatorsGrid.RowDefinitions.Remove(row);
            }

            // Добавляем нужное количество рядов
            for (int i = 0; i < count; i++)
            {
                IndicatorsGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(50, GridUnitType.Star)
                });
            }
        }

        private ScottPlot.WpfPlot CreateIndicatorPlot()
        {
            var plot = new ScottPlot.WpfPlot();
            plot.Configuration.ScrollWheelZoom = true;
            plot.Configuration.MiddleClickDragZoom = false;
            plot.Configuration.RightClickDragZoom = false;
            plot.Margin = new Thickness(0, 0, 0, 0);

            plot.Padding = new Thickness(0);

            return plot;
        }

        private bool _isSynchronizing = false;
        // Словарь для отслеживания подписок на события (чтобы избежать дублирования)
        private readonly Dictionary<ScottPlot.WpfPlot, bool> _plotEventSubscriptions = new Dictionary<ScottPlot.WpfPlot, bool>();

        private void SynchronizeXAxis(ScottPlot.WpfPlot indicatorPlot)
        {
            if (indicatorPlot == null) return;

            // Синхронизируем оси X изначально
            var mainLimits = MainPlot.Plot.GetAxisLimits();
            indicatorPlot.Plot.SetAxisLimitsX(mainLimits.XMin, mainLimits.XMax);
            indicatorPlot.Refresh();

            // Подписываемся на изменения осей только если еще не подписаны
            if (!_plotEventSubscriptions.ContainsKey(indicatorPlot))
            {
                indicatorPlot.AxesChanged += IndicatorPlot_AxesChanged;
                _plotEventSubscriptions[indicatorPlot] = true;
            }
        }
        private void IndicatorPlot_AxesChanged(object sender, EventArgs e)
        {
            // Предотвращаем рекурсию
            if (_isSynchronizing) return;

            try
            {
                _isSynchronizing = true;

                var sourcePlot = (ScottPlot.WpfPlot)sender;
                var limits = sourcePlot.Plot.GetAxisLimits();

                // Синхронизируем основной график (только если источник не основной график)
                if (sourcePlot != MainPlot)
                {
                    MainPlot.Plot.SetAxisLimitsX(limits.XMin, limits.XMax);
                    MainPlot.Refresh();
                }

                // Синхронизируем все остальные индикаторные графики
                foreach (var otherPlot in _indicatorPlots.Where(p => p != sourcePlot))
                {
                    if (otherPlot != null)
                    {
                        otherPlot.Plot.SetAxisLimitsX(limits.XMin, limits.XMax);
                        otherPlot.Refresh();
                    }
                }
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private string GetDateFormatForTimeFrame(string timeFrame)
        {
            switch (timeFrame)
            {
                case "CANDLE_INTERVAL_5_MIN":
                case "CANDLE_INTERVAL_15_MIN":
                case "CANDLE_INTERVAL_30_MIN":
                    return "HH:mm";
                case "CANDLE_INTERVAL_HOUR":
                case "CANDLE_INTERVAL_2_HOUR":
                case "CANDLE_INTERVAL_4_HOUR":
                    return "dd.MM HH:mm";
                case "CANDLE_INTERVAL_DAY":
                    return "dd.MM";
                case "CANDLE_INTERVAL_WEEK":
                    return "MMM yyyy";
                default:
                    return "dd.MM";
            }
        }

        private void PlotIndicator(ScottPlot.WpfPlot plot, string indicator, List<CandleDTO> candles, double[] prices, double[] xs)
        {
            plot.Plot.Clear();

            switch (indicator)
            {
                case "RSI":
                    var rsiValues = new IndicatorRSI(candles).Calculate();
                    var rsiXs = candles.Select(c => c.Time.ToOADate()).ToArray();
                    

                    plot.Plot.AddScatter(rsiXs, rsiValues, color: System.Drawing.Color.Purple);
                    plot.Plot.AddHorizontalLine(70, System.Drawing.Color.Red, 1, ScottPlot.LineStyle.Dash);
                    plot.Plot.AddHorizontalLine(30, System.Drawing.Color.Green, 1, ScottPlot.LineStyle.Dash);
                    plot.Plot.AddHorizontalLine(50, System.Drawing.Color.Gray, 1, ScottPlot.LineStyle.Dot);
                    //plot.Plot.YLabel("RSI");
                    plot.Plot.SetAxisLimits(yMin: 0, yMax: 100);

                    plot.Plot.XAxis.TickLabelStyle(fontSize: 8);

                    plot.Plot.XAxis.Label(string.Empty);
                    plot.Plot.YLabel(label: "RSI");
                    break;

                case "MACD":

                    var macd = new IndicatorMACD(candles).Calculate();
                    var macdXs = candles.Select(c => c.Time.ToOADate()).ToArray();

                    plot.Plot.AddScatter(macdXs, macd.MACDLine, color: System.Drawing.Color.Blue);
                    plot.Plot.AddScatter(macdXs, macd.SignalLine, color: System.Drawing.Color.Green);
                    var barPlot = plot.Plot.AddBar(macd.Histogram, macdXs, System.Drawing.Color.Gray);
                    barPlot.BorderLineWidth = 0; // Убираем границы для более чистого вида 
                    barPlot.BarWidth = GetBarWidthMACD();

                    //plot.Plot.AddBar(histogram, color: System.Drawing.Color.Gray);
                    plot.Plot.AddHorizontalLine(0, System.Drawing.Color.Black, 1, ScottPlot.LineStyle.Solid);
                    //plot.Plot.YLabel("MACD");

                    plot.Plot.XAxis.TickLabelStyle(fontSize: 8);
                    plot.Plot.Legend(enable: false);

                    plot.Plot.XAxis.Label(string.Empty);
                    plot.Plot.YLabel(label: "MACD");
                    break;

                case "Stochastic":
                    var stochValues = new IndicatorStochastic(candles).Calculate();
                    var stochXs = candles.Select(c => c.Time.ToOADate()).ToArray();

                    plot.Plot.AddScatter(stochXs, stochValues.PercentK, color: System.Drawing.Color.Orange);
                    plot.Plot.AddScatter(stochXs, stochValues.PercentD, color: System.Drawing.Color.Blue);
                    plot.Plot.AddHorizontalLine(80, System.Drawing.Color.Red, 1, ScottPlot.LineStyle.Dash);
                    plot.Plot.AddHorizontalLine(20, System.Drawing.Color.Green, 1, ScottPlot.LineStyle.Dash);
                    plot.Plot.AddHorizontalLine(50, System.Drawing.Color.Gray, 1, ScottPlot.LineStyle.Dot);
                    //plot.Plot.YLabel("Stochastic");
                    plot.Plot.SetAxisLimits(yMin: 0, yMax: 100);

                    plot.Plot.XAxis.TickLabelStyle(fontSize: 8);
                    plot.Plot.Legend(enable: false);

                    plot.Plot.XAxis.Label(string.Empty);
                    plot.Plot.YLabel(label: "St");
                    break;

                case "ADX":
                    var adxValues = new IndicatorADX(candles).Calculate();
                    var adxXs = candles.Select(c => c.Time.ToOADate()).ToArray();

                    plot.Plot.AddScatter(adxXs, adxValues.ADX, color: System.Drawing.Color.Brown);
                    plot.Plot.AddScatter(adxXs, adxValues.PlusDI, color: System.Drawing.Color.Red);
                    plot.Plot.AddScatter(adxXs, adxValues.MinusDI, color: System.Drawing.Color.Blue);
                    plot.Plot.AddHorizontalLine(25, System.Drawing.Color.Orange, 1, ScottPlot.LineStyle.Dash);
                    //plot.Plot.YLabel("ADX");
                    plot.Plot.SetAxisLimits(yMin: 0, yMax: 100);

                    plot.Plot.XAxis.TickLabelStyle(fontSize: 8);
                    plot.Plot.Legend(enable: false);

                    plot.Plot.XAxis.Label(string.Empty);
                    plot.Plot.YLabel(label: "ADX");
                    break;

                case "TRIX":
                    var trixValues = new IndicatorTRIX(candles).Calculate();
                    var trixXs = candles.Select(c => c.Time.ToOADate()).ToArray();

                    plot.Plot.AddScatter(trixXs, trixValues.TRIX, color: System.Drawing.Color.DarkGreen);
                    plot.Plot.AddScatter(trixXs, trixValues.Signal, color: System.Drawing.Color.Blue);
                    plot.Plot.AddHorizontalLine(0, System.Drawing.Color.Black, 1, ScottPlot.LineStyle.Solid);
                    //plot.Plot.YLabel("TRIX %");

                    plot.Plot.XAxis.TickLabelStyle(fontSize: 8);
                    plot.Plot.Legend(enable: false);

                    plot.Plot.XAxis.Label(string.Empty);
                    plot.Plot.YLabel(label: "TRIX");
                    break;
            }

            // Настройка осей
            string dateFormat = GetDateFormatForTimeFrame(_selectedTimeFrame);
            plot.Plot.XAxis.TickLabelFormat(dateFormat, dateTimeFormat: true);

            plot.Plot.Legend(enable: false);

            // Отключаем цифровые метки на осях
            plot.Plot.XAxis.Ticks(false); // Убираем метки на оси X

            plot.Plot.Layout(0, 0, 0, 0);

            plot.Refresh();
        }

        private double GetBarWidthMACD()
        {
            double result = 0.8; 


            if (_selectedTimeFrame == "CANDLE_INTERVAL_5_MIN")
            {
                return result / (96 * 3);
            }
            else if (_selectedTimeFrame == "CANDLE_INTERVAL_15_MIN")
            {
                return result / 96;
            }
            else if (_selectedTimeFrame == "CANDLE_INTERVAL_30_MIN")
            {
                return result / 48;
            }
            else if (_selectedTimeFrame == "CANDLE_INTERVAL_HOUR")
            {
                return result / 24;
            }
            else if (_selectedTimeFrame == "CANDLE_INTERVAL_2_HOUR")
            {
                return result / 12;
            }
            else if (_selectedTimeFrame == "CANDLE_INTERVAL_4_HOUR")
            {
                return result / 6;
            }
            else if (_selectedTimeFrame == "CANDLE_INTERVAL_WEEK")
            {
                return result * 7;
            }
            else
            {
                return result;
            }
        }

        private List<string> GetVisibleIndicators()
        {
            var visible = new List<string>();

            if (MacdCheckBox.IsChecked == true) visible.Add("MACD");
            if (RsiCheckBox.IsChecked == true) visible.Add("RSI");
            if (StochasticCheckBox.IsChecked == true) visible.Add("Stochastic");
            if (TrixCheckBox.IsChecked == true) visible.Add("TRIX");
            if (AdxCheckBox.IsChecked == true) visible.Add("ADX");

            return visible;
        }

        #endregion

        public void UpdateStrongScore(SignalScoreResult signalScore)
        {
            if (_currentCandles == null || _currentCandles.Count < 14)
            {
                StrongScoreProgressBar.Value = 0;
                StrongScoreProgressBarBullishScore.Value = 0;
                StrongScoreProgressBarBearishScore.Value = 0;

                StrongScoreTextBlock.Text = "—";
                StrongScoreTextBlockBullishScore.Text = "—";
                StrongScoreTextBlockBearishScore.Text = "—";

                StrongScoreDescriptionTextBlock.Text = "Недостаточно данных";
                return;
            }

            StrongScoreProgressBar.Value = signalScore.Value;
            StrongScoreProgressBarBullishScore.Value = signalScore.BullishScore;
            StrongScoreProgressBarBearishScore.Value = signalScore.BearishScore;

            StrongScoreProgressBarBullishScore.Foreground = new SolidColorBrush(Colors.Green);
            StrongScoreProgressBarBearishScore.Foreground = new SolidColorBrush(Colors.Red);

            StrongScoreTextBlock.Text = $"{signalScore.Value:F0}/100";
            StrongScoreTextBlockBullishScore.Text = $"{signalScore.BullishScore:F0}/100";
            StrongScoreTextBlockBearishScore.Text = $"{signalScore.BearishScore:F0}/100";

            string description;
            if (signalScore.Value >= 70 && signalScore.BullishScore >= 50)
                description = "Сильный сигнал разворота вниз";
            else if (signalScore.Value >= 70 && signalScore.BearishScore >= 50)
                description = "Сильный сигнал разворота вверх";
            else if (signalScore.Value >= 50 && signalScore.BullishScore >= 50)
                description = "Умеренный сигнал разворота вниз";
            else if (signalScore.Value >= 50 && signalScore.BearishScore >= 50)
                description = "Умеренный сигнал разворота вверх";
            else if (signalScore.Value >= 70)
                description = "Сильный сигнал разворота";
            else if (signalScore.Value >= 50)
                description = "Умеренный сигнал разворота";
            else if (signalScore.Value >= 30)
                description = "Слабый сигнал разворота";
            else
                description = "Нет сигналов разворота";

            StrongScoreDescriptionTextBlock.Text = description;

            // Цвет прогресс-бара
            if (signalScore.Value >= 60)
                StrongScoreProgressBar.Foreground = new SolidColorBrush(Colors.Green);
            else if (signalScore.Value >= 40)
                StrongScoreProgressBar.Foreground = new SolidColorBrush(Colors.Orange);
            else
                StrongScoreProgressBar.Foreground = new SolidColorBrush(Colors.Red);
        }

        #endregion

        #region Indicator Display

        private void IndicatorCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_currentCandles != null && _currentCandles.Count > 0)
            {
                UpdateScottPlotChart(_currentCandles);
            }
        }

        #endregion

        private bool _isUpdatingAxes = false; // Флаг для предотвращения рекурсии

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainPlot.Configuration.ScrollWheelZoom = true;
            MainPlot.AxesChanged += MainPlot_AxesChanged;

            // Инициализируем кнопки временных диапазонов
            InitializeTimeFrames();

            // Инициализируем индикаторные графики
            InitializeIndicatorPlots();

            // Устанавливаем выделение для выбранного временного диапазона по умолчанию
            if (_timeFrameButtons.ContainsKey(_selectedTimeFrame))
            {
                _timeFrameButtons[_selectedTimeFrame].Style = (System.Windows.Style)FindResource("SelectedTimeFrameButtonStyle");
            }
        }

        private void MainPlot_AxesChanged(object sender, EventArgs e)
        {
            // Предотвращаем рекурсию
            if (_isUpdatingAxes || _candles == null || _candles.Count == 0)
                return;

            try
            {
                _isUpdatingAxes = true;

                // Получаем текущие границы оси X
                var limits = MainPlot.Plot.GetAxisLimits();
                double xMin = limits.XMin;
                double xMax = limits.XMax;

                // Синхронизируем все индикаторные графики по X и обновляем Y границы
                for (int i = 0; i < _indicatorPlots.Count; i++)
                {
                    var indicatorPlot = _indicatorPlots[i];
                    if (indicatorPlot == null) continue;

                    // Синхронизируем по X
                    indicatorPlot.Plot.SetAxisLimitsX(xMin, xMax);

                    // Определяем тип индикатора для данного графика
                    string indicatorType = GetIndicatorTypeForPlotOptimized(indicatorPlot);

                    // Обновляем Y границы в зависимости от типа индикатора
                    UpdateIndicatorYLimits(indicatorPlot, indicatorType, xMin, xMax);

                    indicatorPlot.Refresh();
                }

                // Вычисляем оптимальные границы Y только для видимых свечей основного графика
                var visibleCandles = _candles
                    .Where(c => c.Time.ToOADate() >= xMin && c.Time.ToOADate() <= xMax)
                    .ToList();

                if (!visibleCandles.Any())
                    return;

                double minY = (double)visibleCandles.Min(c => c.Low);
                double maxY = (double)visibleCandles.Max(c => c.High);
                double padding = (maxY - minY) * 0.1;

                // Проверяем, нужно ли обновлять Y границы основного графика
                var currentYLimits = MainPlot.Plot.GetAxisLimits();
                double newMinY = minY - padding;
                double newMaxY = maxY + padding;

                // Обновляем Y границы только если есть значительное изменение
                if (Math.Abs(currentYLimits.YMin - newMinY) > (maxY - minY) * 0.01 ||
                    Math.Abs(currentYLimits.YMax - newMaxY) > (maxY - minY) * 0.01)
                {
                    MainPlot.Plot.SetAxisLimitsY(newMinY, newMaxY);
                    MainPlot.Refresh();
                }
            }
            finally
            {
                _isUpdatingAxes = false;
            }
        }

        private void UpdateIndicatorYLimits(ScottPlot.WpfPlot indicatorPlot, string indicatorType, double xMin, double xMax)
        {
            switch (indicatorType)
            {
                case "RSI":
                case "Stochastic":
                case "ADX":
                    // Эти индикаторы должны всегда показывать диапазон 0-100
                    indicatorPlot.Plot.SetAxisLimitsY(0, 100);
                    break;

                case "MACD":
                case "TRIX":
                    // Для этих индикаторов вычисляем динамические границы на основе видимых данных
                    UpdateDynamicYLimits(indicatorPlot, indicatorType, xMin, xMax);
                    break;

                default:
                    // Для неизвестных индикаторов также используем динамические границы
                    UpdateDynamicYLimits(indicatorPlot, indicatorType, xMin, xMax);
                    break;
            }
        }

        private void UpdateDynamicYLimits(ScottPlot.WpfPlot indicatorPlot, string indicatorType, double xMin, double xMax)
        {
            // Получаем все данные с графика индикатора
            var plottables = indicatorPlot.Plot.GetPlottables();

            if (!plottables.Any())
                return;

            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var plottable in plottables)
            {
                // Проверяем разные типы графиков
                if (plottable is ScottPlot.Plottable.ScatterPlot scatter)
                {
                    var xs = scatter.Xs;
                    var ys = scatter.Ys;

                    for (int i = 0; i < xs.Length; i++)
                    {
                        if (xs[i] >= xMin && xs[i] <= xMax)
                        {
                            minY = Math.Min(minY, ys[i]);
                            maxY = Math.Max(maxY, ys[i]);
                        }
                    }
                }
                else if (plottable is ScottPlot.Plottable.BarPlot bar)
                {
                    var positions = bar.Positions;
                    var values = bar.Values;

                    for (int i = 0; i < positions.Length; i++)
                    {
                        if (positions[i] >= xMin && positions[i] <= xMax)
                        {
                            minY = Math.Min(minY, values[i]);
                            maxY = Math.Max(maxY, values[i]);
                        }
                    }
                }
                // Исправленная поддержка для SignalPlot
                else if (plottable is ScottPlot.Plottable.SignalPlot signal)
                {
                    var ys = signal.Ys;
                    double sampleRate = signal.SampleRate;
                    double xOffset = signal.OffsetX;

                    int startIndex = Math.Max(0, (int)((xMin - xOffset) * sampleRate));
                    int endIndex = Math.Min(ys.Length - 1, (int)((xMax - xOffset) * sampleRate));

                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        minY = Math.Min(minY, ys[i]);
                        maxY = Math.Max(maxY, ys[i]);
                    }
                }
            }

            // Если нашли данные, устанавливаем границы с отступами
            if (minY != double.MaxValue && maxY != double.MinValue)
            {
                double range = maxY - minY;
                double padding = range * 0.1; // 10% отступ

                // Минимальный отступ для случаев, когда все значения одинаковые
                if (padding < 0.01)
                    padding = 0.1;

                double newMinY = minY - padding;
                double newMaxY = maxY + padding;

                // Для MACD и TRIX убеждаемся, что линия 0 видна
                if (indicatorType == "MACD" || indicatorType == "TRIX")
                {
                    newMinY = Math.Min(newMinY, -padding);
                    newMaxY = Math.Max(newMaxY, padding);
                }

                indicatorPlot.Plot.SetAxisLimitsY(newMinY, newMaxY);
            }
        }

        private string GetIndicatorTypeForPlot(int plotIndex)
        {
            // Проверяем корректность индекса
            if (plotIndex < 0 || plotIndex >= _indicatorPlots.Count)
                return "Unknown";

            var plot = _indicatorPlots[plotIndex];

            // Ищем тип индикатора в маппинге по графику
            foreach (var mapping in _indicatorPlotMapping)
            {
                if (mapping.Value == plot)
                {
                    return mapping.Key; // Возвращаем название индикатора (RSI, MACD, etc.)
                }
            }

            // Если не найдено в маппинге, пытаемся определить по Y-метке
            try
            {
                string yLabel = plot.Plot.YAxis.Label();
                if (!string.IsNullOrEmpty(yLabel))
                {
                    if (yLabel.Contains("RSI")) return "RSI";
                    if (yLabel.Contains("MACD")) return "MACD";
                    if (yLabel.Contains("St")) return "Stochastic";
                    if (yLabel.Contains("ADX")) return "ADX";
                    if (yLabel.Contains("TRIX")) return "TRIX";
                }
            }
            catch
            {
                // Игнорируем ошибки получения метки
            }

            return "Unknown";
        }

        // Альтернативный метод для более эффективного поиска
        private string GetIndicatorTypeForPlotOptimized(ScottPlot.WpfPlot plot)
        {
            // Прямой поиск в маппинге по значению
            var indicatorType = _indicatorPlotMapping.FirstOrDefault(x => x.Value == plot).Key;

            if (!string.IsNullOrEmpty(indicatorType))
                return indicatorType;

            // Fallback - поиск по Y-метке
            try
            {
                string yLabel = plot.Plot.YAxis.Label();
                if (!string.IsNullOrEmpty(yLabel))
                {
                    if (yLabel.Contains("RSI")) return "RSI";
                    if (yLabel.Contains("MACD")) return "MACD";
                    if (yLabel.Contains("St")) return "Stochastic";
                    if (yLabel.Contains("ADX")) return "ADX";
                    if (yLabel.Contains("TRIX")) return "TRIX";
                }
            }
            catch
            {
                // Игнорируем ошибки получения метки
            }

            return "Unknown";
        }

        // 1. Добавьте константу для количества отображаемых свечей
        private const int DEFAULT_VISIBLE_CANDLES = 40;

        // 2. Добавьте метод для установки фокуса на последние свечи
        private void SetChartFocusToLastCandles(List<CandleDTO> candles, int visibleCandlesCount = DEFAULT_VISIBLE_CANDLES)
        {
            if (candles == null || candles.Count == 0)
                return;

            try
            {
                // Определяем количество свечей для отображения
                int candlesToShow = Math.Min(visibleCandlesCount, candles.Count);

                // Получаем последние свечи
                var lastCandles = candles.GetRange(candles.Count - visibleCandlesCount, visibleCandlesCount).ToList();

                if (lastCandles.Count < 2)
                    return;

                // Вычисляем границы по оси X (время)
                double xMin = lastCandles.First().Time.ToOADate();
                double xMax = lastCandles.Last().Time.ToOADate();

                //var a = lastCandles.Last();
                //var b = candles.Last();

                // Добавляем небольшой отступ (10% от диапазона)
                double timeRange = xMax - xMin;
                double padding = timeRange * 0.15; // 5% отступ с каждой стороны

                xMin -= padding;
                xMax += padding;

                // Устанавливаем границы для основного графика
                MainPlot.Plot.SetAxisLimitsX(xMin, xMax);

                // Вычисляем оптимальные границы Y для видимых свечей
                double minY = (double)lastCandles.Min(c => c.Low);
                double maxY = (double)lastCandles.Max(c => c.High);
                double yPadding = (maxY - minY) * 0.1; // 10% отступ по Y

                MainPlot.Plot.SetAxisLimitsY(minY - yPadding, maxY + yPadding);

                MainPlot.Refresh();
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не показываем пользователю
                System.Diagnostics.Debug.WriteLine($"Ошибка установки фокуса графика: {ex.Message}");
            }
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await LoadAssetData(SearchTextBox.Text);
            }
        }
    }
}