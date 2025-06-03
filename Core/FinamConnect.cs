using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace FinamParser
{
    public class FinamConnect
    {
        private ChromeDriver driver;

        public void Initialize()
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");
            driver = new ChromeDriver(options);
        }

        public string GetFundamentalData(string ticker)
        {
            try
            {
                Initialize();
                //string url = $"https://www.finam.ru/quote/overview//";
                string url = $"https://www.finam.ru/quote/moex/{ticker}/financial/";
            
                driver.Navigate().GoToUrl(url);

                // Ждем загрузки данных
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.ClassName("fundamental-data")));

                Dispose();

                return driver.PageSource;
            }
            catch (Exception ex)
            {
                Dispose();
                throw new Exception($"Ошибка получения данных: {ex.Message}");
            }
        }

        public void Dispose()
        {
            driver?.Quit();
        }

    }
}