using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using UnTraid.Core;
using UnTraid.DTO;

namespace UnTraid.Views
{
    public partial class UserDataView : UserControl
    {
        public UserDataView()
        {
            InitializeComponent();
            DataPaging();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void SavePropertisButton_Click(object sender, RoutedEventArgs e)
        {
            UserDataDTO data = UserJsonData.GetJsonData();

            if (UserNameTextBox.Text != null || data.User.UserName != UserNameTextBox.Text || UserNameTextBox.Text != data.User.Defalt)
            {
                data.User.UserName = UserNameTextBox.Text;
            }

            if (TelegramIdTextBox.Text != null && data.TelegramId.Status)
            {
                data.TelegramId.Id = TelegramIdTextBox.Text;
            }
            else
            {

            }
            
            if (ApiTokenPasswordBox.Password != null && data.TInvestToken.Status)
            {
                data.TInvestToken.Id = ApiTokenPasswordBox.Password;
            }
            else
            {

            }

            UserJsonData.UpdateJsonData(data);
            MessageBox.Show("Настройки сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DataPaging()
        {
            UserDataDTO data = UserJsonData.GetJsonData();

            if (data.User.UserName != null)
            {
                UserNameTextBox.Text = data.User.UserName;
            }
            else
            {
                UserNameTextBox.Text = data.User.Defalt;
            }

            if (data.TelegramId.Id != null)
            {
                TelegramIdTextBox.Text = data.TelegramId.Id;
            }

            if (data.TInvestToken.Id != null)
            {
                ApiTokenPasswordBox.Password = data.TInvestToken.Id;
            }
        }

        private async void TInvestCheckConnectButton_Click(object sender, EventArgs e)
        {
            string token = ApiTokenPasswordBox.Password;
            var connect = new TInvestConnect(token);
            bool result = await connect.CheckTokenAsync();

            if (result)
            {
                UserDataDTO data = UserJsonData.GetJsonData();

                data.TInvestToken.Status = true;
                data.TInvestToken.Id = token;

                UserJsonData.UpdateJsonData(data);
                MessageBox.Show("Токен найдет и к нему произошло успешное подключение. Данный токен сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Данный токен не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private async void TestButton_Click(object sender, EventArgs e)
        {
            string token = ApiTokenPasswordBox.Password;
            var connect = new TInvestConnect(token);

            // Поиск акции
            var instruments = await connect.SearchInstrumentAsync("SBER");

            // Получение текущей цены
            decimal? price = await connect.GetLastPriceAsync("BBG004730N88"); // FIGI Сбербанка

            // Получение исторических данных
            var candles = await connect.GetCandlesAsync("BBG004730N88",
                DateTime.Now.AddDays(-200), DateTime.Now);
        }
    }
}