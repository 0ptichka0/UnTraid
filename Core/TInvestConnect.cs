using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UnTraid.DTO;
using static UnTraid.DTO.TInvestDTO;

namespace UnTraid.Core
{
    public class TInvestConnect
    {
        private readonly HttpClient _client;
        private readonly TInvestTokenDTO _token;

        /// <summary>
        /// ctor: принимает токен в виде строки
        /// </summary>
        public TInvestConnect(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token не может быть пустым", nameof(token));

            _token = new TInvestTokenDTO { Id = token };
            _client = CreateHttpClient();
        }

        /// <summary>
        /// ctor: загружает токен из файла UserDataPropertis.json
        /// </summary>
        public TInvestConnect()
        {
            _token = LoadTokenFromFile()
                ?? throw new InvalidOperationException("Токен не найден");
            _client = CreateHttpClient();
        }

        /// <summary>
        /// Настройка HttpClient с базовым адресом и заголовком Authorization
        /// </summary>
        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://invest-public-api.tinkoff.ru/rest/")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token.Id);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        /// <summary>
        /// Загрузка токена из JSON-файла UserDataPropertis.json
        /// </summary>
        private TInvestTokenDTO LoadTokenFromFile()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(appDir).FullName
                ).FullName
            ).FullName;
            string path = Path.Combine(projectRoot, "UserDataPropertis.json");

            if (!File.Exists(path))
                throw new FileNotFoundException("Файл конфигурации не найден", path);

            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<UserDataDTO>(json);
            return data?.TInvestToken;
        }

        /// <summary>
        /// Проверяет действительность токена
        /// </summary>
        public async Task<bool> CheckTokenAsync()
        {
            var requestBody = new { status = "ACCOUNT_STATUS_ALL" };

            try
            {
                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.UsersService/GetAccounts",
                    requestBody
                );

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return false;

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                // Проверяем наличие массива accounts и что он не пустой
                var accounts = json["accounts"];
                return accounts != null && accounts.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Получает список всех доступных инструментов (акции)
        /// </summary>
        public async Task<List<InstrumentDTO>> GetSharesAsync()
        {
            try
            {
                var requestBody = new
                {
                    instrumentStatus = "INSTRUMENT_STATUS_BASE"
                };

                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.InstrumentsService/Shares",
                    requestBody
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var instruments = new List<InstrumentDTO>();
                var sharesArray = json["instruments"];

                if (sharesArray != null)
                {
                    foreach (var share in sharesArray)
                    {
                        instruments.Add(new InstrumentDTO
                        {
                            Figi = share["figi"]?.ToString(),
                            Ticker = share["ticker"]?.ToString(),
                            Name = share["name"]?.ToString(),
                            Currency = share["currency"]?.ToString(),
                            Lot = share["lot"]?.ToObject<int>() ?? 1,
                            TradingStatus = share["tradingStatus"]?.ToString()
                        });
                    }
                }

                return instruments;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении списка акций: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает текущую цену инструмента по FIGI
        /// </summary>
        public async Task<decimal?> GetLastPriceAsync(string figi)
        {
            if (string.IsNullOrWhiteSpace(figi))
                throw new ArgumentException("FIGI не может быть пустым", nameof(figi));

            try
            {
                var requestBody = new
                {
                    figi = new[] { figi }
                };

                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.MarketDataService/GetLastPrices",
                    requestBody
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var lastPrices = json["lastPrices"];
                if (lastPrices != null && lastPrices.Any())
                {
                    var priceData = lastPrices.First();
                    var price = priceData["price"];

                    if (price != null)
                    {
                        var units = price["units"]?.ToObject<long>() ?? 0;
                        var nano = price["nano"]?.ToObject<int>() ?? 0;

                        // Конвертируем из формата Quotation (units + nano) в decimal
                        return units + (nano / 1_000_000_000m);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении цены для {figi}: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает исторические свечи для инструмента
        /// </summary>
        public async Task<List<CandleDTO>> GetCandlesAsync(string figi, DateTime from, DateTime to, string interval = "CANDLE_INTERVAL_DAY")
        {
            if (string.IsNullOrWhiteSpace(figi))
                throw new ArgumentException("FIGI не может быть пустым", nameof(figi));

            try
            {
                var requestBody = new
                {
                    figi = figi,
                    from = from.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    to = to.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    interval = interval
                };

                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.MarketDataService/GetCandles",
                    requestBody
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var candles = new List<CandleDTO>();
                var candlesArray = json["candles"];

                if (candlesArray != null)
                {
                    foreach (var candle in candlesArray)
                    {
                        candles.Add(new CandleDTO
                        {
                            Time = DateTime.Parse(candle["time"]?.ToString()),
                            Open = ConvertQuotationToDecimal(candle["open"]),
                            High = ConvertQuotationToDecimal(candle["high"]),
                            Low = ConvertQuotationToDecimal(candle["low"]),
                            Close = ConvertQuotationToDecimal(candle["close"]),
                            Volume = candle["volume"]?.ToObject<long>() ?? 0
                        });
                    }
                }

                return candles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении свечей для {figi}: {ex.Message}");
            }
        }

        /// <summary>
        /// Поиск инструмента по тикеру или названию
        /// </summary>
        public async Task<List<InstrumentDTO>> SearchInstrumentAsync(string query, string instrumentKind = "INSTRUMENT_TYPE_SHARE")
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Поисковый запрос не может быть пустым", nameof(query));

            try
            {
                var requestBody = new
                {
                    query = query,
                    instrumentKind = instrumentKind
                };

                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.InstrumentsService/FindInstrument",
                    requestBody
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var instruments = new List<InstrumentDTO>();
                var instrumentsArray = json["instruments"];

                if (instrumentsArray != null)
                {
                    foreach (var instrument in instrumentsArray)
                    {
                        instruments.Add(new InstrumentDTO
                        {
                            Figi = instrument["figi"]?.ToString(),
                            Ticker = instrument["ticker"]?.ToString(),
                            Name = instrument["name"]?.ToString(),
                            Currency = instrument["currency"]?.ToString(),
                            Lot = instrument["lot"]?.ToObject<int>() ?? 1,
                            TradingStatus = instrument["tradingStatus"]?.ToString()
                        });
                    }
                }

                return instruments;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при поиске инструмента '{query}': {ex.Message}");
            }
        }

        /// <summary>
        /// Получает информацию о портфеле
        /// </summary>
        public async Task<PortfolioDTO> GetPortfolioAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new ArgumentException("Account ID не может быть пустым", nameof(accountId));

            try
            {
                var requestBody = new { accountId = accountId };

                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.OperationsService/GetPortfolio",
                    requestBody
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var portfolio = new PortfolioDTO();
                var totalAmountRub = json["totalAmountShares"];

                if (totalAmountRub != null)
                {
                    portfolio.TotalAmount = ConvertMoneyValueToDecimal(totalAmountRub);
                }

                var positions = new List<PositionDTO>();
                var positionsArray = json["positions"];

                if (positionsArray != null)
                {
                    foreach (var position in positionsArray)
                    {
                        positions.Add(new PositionDTO
                        {
                            Figi = position["figi"]?.ToString(),
                            InstrumentType = position["instrumentType"]?.ToString(),
                            Quantity = ConvertQuotationToDecimal(position["quantity"]),
                            AveragePositionPrice = ConvertMoneyValueToDecimal(position["averagePositionPrice"]),
                            CurrentPrice = ConvertMoneyValueToDecimal(position["currentPrice"])
                        });
                    }
                }

                portfolio.Positions = positions;
                return portfolio;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении портфеля: {ex.Message}");
            }
        }

        /// <summary>
        /// Преобразует Quotation (units + nano) в decimal
        /// </summary>
        private decimal ConvertQuotationToDecimal(JToken quotation)
        {
            if (quotation == null) return 0;

            var units = quotation["units"]?.ToObject<long>() ?? 0;
            var nano = quotation["nano"]?.ToObject<int>() ?? 0;

            return units + (nano / 1_000_000_000m);
        }

        /// <summary>
        /// Преобразует MoneyValue (units + nano + currency) в decimal
        /// </summary>
        private decimal ConvertMoneyValueToDecimal(JToken moneyValue)
        {
            if (moneyValue == null) return 0;

            var units = moneyValue["units"]?.ToObject<long>() ?? 0;
            var nano = moneyValue["nano"]?.ToObject<int>() ?? 0;

            return units + (nano / 1_000_000_000m);
        }

        public InstrumentDTO GetFirstMatchingFigi(List<InstrumentDTO> instruments)
        {
            var figiData = FigiJsonData.GetJsonData();

            var matchingInstrument = instruments.FirstOrDefault(instrument =>
                !string.IsNullOrEmpty(instrument.Figi) &&
                figiData.Data.Any(data => data.Ticker == instrument.Figi));

            if (matchingInstrument == null)
            {
                return instruments.First();
            }
            else
            {
                return matchingInstrument;
            }
        }

        /// <summary>
        /// Получает информацию об активе по FIGI, включая Asset UID
        /// </summary>
        /// <param name="figi">FIGI инструмента</param>
        /// <returns>Информация об активе с Asset UID</returns>
        public async Task<string> GetAssetByFigiAsync(string figi)
        {
            if (string.IsNullOrWhiteSpace(figi))
                throw new ArgumentException("FIGI не может быть пустым", nameof(figi));

            try
            {
                var requestBody = new
                {
                    id = figi,
                    idType = "INSTRUMENT_ID_TYPE_FIGI"
                };

                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.InstrumentsService/GetInstrumentBy",
                    requestBody
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var instrument = json["instrument"];
                if (instrument == null)
                    return null;

                return instrument["assetUid"]?.ToString();
                //{
                //    Figi = instrument["figi"]?.ToString(),
                //    Ticker = instrument["ticker"]?.ToString(),
                //    Name = instrument["name"]?.ToString(),
                //    AssetUid = instrument["uid"]?.ToString(), // Это и есть Asset UID!
                //    Currency = instrument["currency"]?.ToString(),
                //    InstrumentType = instrument["instrumentType"]?.ToString(),
                //    TradingStatus = instrument["tradingStatus"]?.ToString()
                //};
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении актива по FIGI {figi}: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает фундаментальные показатели для списка инструментов
        /// </summary>
        /// <param name="assets">Список UID активов для получения фундаментальных данных</param>
        /// <returns>Список фундаментальных показателей</returns>
        public async Task<FundamentalDTO> GetAssetFundamentalsAsync(string figi)
        {
            var asset = await GetAssetByFigiAsync(figi);

            if (asset == null)
                throw new ArgumentException("Список активов не может быть пустым", nameof(asset));

            try
            {
                var requestBody = new
                {
                    assets = new string[] { asset }
                };

                var response = await _client.PostAsJsonAsync(
                    "tinkoff.public.invest.api.contract.v1.InstrumentsService/GetAssetFundamentals",
                    requestBody
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var fundamentals = new List<FundamentalDTO>();
                var fundamentalsArray = json["fundamentals"];

                if (fundamentalsArray != null)
                {
                    foreach (var fundamental in fundamentalsArray)
                    {
                        fundamentals.Add(new FundamentalDTO
                        {
                            AssetUid = fundamental["assetUid"]?.ToString(),
                            Currency = fundamental["currency"]?.ToString(),
                            DomicileIndicatorCode = fundamental["domicileIndicatorCode"]?.ToString(),

                            // Финансовые коэффициенты
                            PeRatioTtm = fundamental["peRatioTtm"]?.ToObject<decimal?>(),
                            PriceToBookTtm = fundamental["priceToBookTtm"]?.ToObject<decimal?>(),
                            PriceToSalesTtm = fundamental["priceToSalesTtm"]?.ToObject<decimal?>(),
                            PriceToFreeCashFlowTtm = fundamental["priceToFreeCashFlowTtm"]?.ToObject<decimal?>(),
                            EvToSales = fundamental["evToSales"]?.ToObject<decimal?>(),
                            EvToEbitdaMrq = fundamental["evToEbitdaMrq"]?.ToObject<decimal?>(),

                            // Дивиденды
                            DividendYieldDailyTtm = fundamental["dividendYieldDailyTtm"]?.ToObject<decimal?>(),
                            DividendRateTtm = fundamental["dividendRateTtm"]?.ToObject<decimal?>(),
                            DividendsPerShare = fundamental["dividendsPerShare"]?.ToObject<decimal?>(),
                            DividendPayoutRatioFy = fundamental["dividendPayoutRatioFy"]?.ToObject<decimal?>(),
                            ForwardAnnualDividendYield = fundamental["forwardAnnualDividendYield"]?.ToObject<decimal?>(),
                            FiveYearsAverageDividendYield = fundamental["fiveYearsAverageDividendYield"]?.ToObject<decimal?>(),
                            FiveYearAnnualDividendGrowthRate = fundamental["fiveYearAnnualDividendGrowthRate"]?.ToObject<decimal?>(),
                            ExDividendDate = ParseNullableDateTime(fundamental["exDividendDate"]?.ToString()),

                            // Прибыль и доходность
                            EpsTtm = fundamental["epsTtm"]?.ToObject<decimal?>(),
                            DilutedEpsTtm = fundamental["dilutedEpsTtm"]?.ToObject<decimal?>(),
                            NetIncomeTtm = fundamental["netIncomeTtm"]?.ToObject<decimal?>(),
                            NetMarginMrq = fundamental["netMarginMrq"]?.ToObject<decimal?>(),
                            Roa = fundamental["roa"]?.ToObject<decimal?>(),
                            Roe = fundamental["roe"]?.ToObject<decimal?>(),
                            Roic = fundamental["roic"]?.ToObject<decimal?>(),

                            // Выручка и рост
                            RevenueTtm = fundamental["revenueTtm"]?.ToObject<decimal?>(),
                            OneYearAnnualRevenueGrowthRate = fundamental["oneYearAnnualRevenueGrowthRate"]?.ToObject<decimal?>(),
                            ThreeYearAnnualRevenueGrowthRate = fundamental["threeYearAnnualRevenueGrowthRate"]?.ToObject<decimal?>(),
                            FiveYearAnnualRevenueGrowthRate = fundamental["fiveYearAnnualRevenueGrowthRate"]?.ToObject<decimal?>(),
                            RevenueChangeFiveYears = fundamental["revenueChangeFiveYears"]?.ToObject<decimal?>(),

                            // EBITDA
                            EbitdaTtm = fundamental["ebitdaTtm"]?.ToObject<decimal?>(),
                            EbitdaChangeFiveYears = fundamental["ebitdaChangeFiveYears"]?.ToObject<decimal?>(),

                            // Изменения за 5 лет
                            EpsChangeFiveYears = fundamental["epsChangeFiveYears"]?.ToObject<decimal?>(),
                            TotalDebtChangeFiveYears = fundamental["totalDebtChangeFiveYears"]?.ToObject<decimal?>(),

                            // Долг
                            TotalDebtMrq = fundamental["totalDebtMrq"]?.ToObject<decimal?>(),
                            TotalDebtToEquityMrq = fundamental["totalDebtToEquityMrq"]?.ToObject<decimal?>(),
                            TotalDebtToEbitdaMrq = fundamental["totalDebtToEbitdaMrq"]?.ToObject<decimal?>(),
                            NetDebtToEbitda = fundamental["netDebtToEbitda"]?.ToObject<decimal?>(),

                            // Капитализация и акции
                            MarketCapitalization = fundamental["marketCapitalization"]?.ToObject<decimal?>(),
                            SharesOutstanding = fundamental["sharesOutstanding"]?.ToObject<decimal?>(),
                            FreeFloat = fundamental["freeFloat"]?.ToObject<decimal?>(),

                            // Денежные потоки
                            FreeCashFlowTtm = fundamental["freeCashFlowTtm"]?.ToObject<decimal?>(),
                            FreeCashFlowToPrice = fundamental["freeCashFlowToPrice"]?.ToObject<decimal?>(),

                            // Цены
                            HighPriceLast52Weeks = fundamental["highPriceLast52Weeks"]?.ToObject<decimal?>(),
                            LowPriceLast52Weeks = fundamental["lowPriceLast52Weeks"]?.ToObject<decimal?>(),

                            // Торговые показатели
                            AverageDailyVolumeLast4Weeks = fundamental["averageDailyVolumeLast4Weeks"]?.ToObject<decimal?>(),
                            AverageDailyVolumeLast10Days = fundamental["averageDailyVolumeLast10Days"]?.ToObject<decimal?>(),
                            Beta = fundamental["beta"]?.ToObject<decimal?>(),

                            // Прочие показатели
                            TotalEnterpriseValueMrq = fundamental["totalEnterpriseValueMrq"]?.ToObject<decimal?>(),
                            CurrentRatioMrq = fundamental["currentRatioMrq"]?.ToObject<decimal?>(),
                            NetInterestMarginMrq = fundamental["netInterestMarginMrq"]?.ToObject<decimal?>(),
                            FixedChargeCoverageRatioFy = fundamental["fixedChargeCoverageRatioFy"]?.ToObject<decimal?>(),
                            NumberOfEmployees = fundamental["numberOfEmployees"]?.ToObject<decimal?>(),
                            AdrToCommonShareRatio = fundamental["adrToCommonShareRatio"]?.ToObject<decimal?>(),
                            BuyBackTtm = fundamental["buyBackTtm"]?.ToObject<decimal?>(),

                            // Периоды
                            FiscalPeriodStartDate = ParseNullableDateTime(fundamental["fiscalPeriodStartDate"]?.ToString()),
                            FiscalPeriodEndDate = ParseNullableDateTime(fundamental["fiscalPeriodEndDate"]?.ToString())
                        });
                    }
                }

                if (fundamentals.Count != 0)
                {
                    return fundamentals.First();
                }
                else
                {
                    return new FundamentalDTO();
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении фундаментальных показателей: {ex.Message}");
            }
        }

        /// <summary>
        /// Вспомогательный метод для парсинга nullable DateTime
        /// </summary>
        private DateTime? ParseNullableDateTime(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParse(dateString, out DateTime result))
                return result;

            return null;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}