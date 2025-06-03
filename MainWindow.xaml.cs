using System.Windows;
using System.Windows.Controls;
using UnTraid.ViewModels;

namespace UnTraid
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Подписываемся на изменение активной секции для обновления стилей кнопок
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.ActiveSection))
            {
                UpdateNavigationButtonStyles();
            }
        }

        private void UpdateNavigationButtonStyles()
        {
            // Сбрасываем стили всех кнопок
            HomeButton.Style = (Style)FindResource("NavButtonStyle");
            UserButton.Style = (Style)FindResource("NavButtonStyle");
            SettingsButton.Style = (Style)FindResource("NavButtonStyle");

            // Устанавливаем активный стиль для текущей кнопки
            switch (_viewModel.ActiveSection)
            {
                case "Home":
                    HomeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
                    UpdateButtonIconColor(HomeButton, "White");
                    UpdateButtonIconColor(UserButton, "#666666");
                    UpdateButtonIconColor(SettingsButton, "#666666");
                    break;
                case "User":
                    UserButton.Style = (Style)FindResource("ActiveNavButtonStyle");
                    UpdateButtonIconColor(HomeButton, "#666666");
                    UpdateButtonIconColor(UserButton, "White");
                    UpdateButtonIconColor(SettingsButton, "#666666");
                    break;
                case "Settings":
                    SettingsButton.Style = (Style)FindResource("ActiveNavButtonStyle");
                    UpdateButtonIconColor(HomeButton, "#666666");
                    UpdateButtonIconColor(UserButton, "#666666");
                    UpdateButtonIconColor(SettingsButton, "White");
                    break;
            }
        }

        private void UpdateButtonIconColor(Button button, string color)
        {
            if (button.Content is System.Windows.Shapes.Path path)
            {
                path.Fill = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            }
        }
    }
}