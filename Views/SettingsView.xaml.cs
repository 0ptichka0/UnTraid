using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace UnTraid.Views
{
    public partial class SettingsView : UserControl
    {
        public event EventHandler<IndicatorSettingsEventArgs> IndicatorSettingsChanged;
        public event EventHandler<bool> BackgroundModeChanged;

        public SettingsView()
        {
            InitializeComponent();
            LoadSettings();
        }

        #region Button Click Handlers

        private void MacdButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            MacdSettingsPopup.IsOpen = true;
        }

        private void RsiButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            RsiSettingsPopup.IsOpen = true;
        }

        private void StochasticButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            StochasticSettingsPopup.IsOpen = true;
        }

        private void TrixButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            TrixSettingsPopup.IsOpen = true;
        }

        private void AdxButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            AdxSettingsPopup.IsOpen = true;
        }

        #endregion

        #region Apply Settings Handlers

        private void ApplyMacdSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new MacdSettings
                {
                    FastPeriod = int.Parse(MacdFastPeriod.Text),
                    SlowPeriod = int.Parse(MacdSlowPeriod.Text),
                    SignalPeriod = int.Parse(MacdSignalPeriod.Text)
                };

                if (ValidateMacdSettings(settings))
                {
                    IndicatorSettingsChanged?.Invoke(this, new IndicatorSettingsEventArgs
                    {
                        IndicatorType = IndicatorType.MACD,
                        Settings = settings
                    });

                    SaveMacdSettings(settings);
                    MacdSettingsPopup.IsOpen = false;
                    ShowSuccessMessage("Настройки MACD сохранены");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при сохранении настроек MACD: {ex.Message}");
            }
        }

        private void ApplyRsiSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new RsiSettings
                {
                    Period = int.Parse(RsiPeriod.Text),
                    OverboughtLevel = double.Parse(RsiOverboughtLevel.Text),
                    OversoldLevel = double.Parse(RsiOversoldLevel.Text)
                };

                if (ValidateRsiSettings(settings))
                {
                    IndicatorSettingsChanged?.Invoke(this, new IndicatorSettingsEventArgs
                    {
                        IndicatorType = IndicatorType.RSI,
                        Settings = settings
                    });

                    SaveRsiSettings(settings);
                    RsiSettingsPopup.IsOpen = false;
                    ShowSuccessMessage("Настройки RSI сохранены");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при сохранении настроек RSI: {ex.Message}");
            }
        }

        private void ApplyStochasticSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new StochasticSettings
                {
                    KPeriod = int.Parse(StochasticKPeriod.Text),
                    DPeriod = int.Parse(StochasticDPeriod.Text),
                    SensitivityLevel = int.Parse(StochasticSensitivity.Text)
                };

                if (ValidateStochasticSettings(settings))
                {
                    IndicatorSettingsChanged?.Invoke(this, new IndicatorSettingsEventArgs
                    {
                        IndicatorType = IndicatorType.Stochastic,
                        Settings = settings
                    });

                    SaveStochasticSettings(settings);
                    StochasticSettingsPopup.IsOpen = false;
                    ShowSuccessMessage("Настройки Stochastic сохранены");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при сохранении настроек Stochastic: {ex.Message}");
            }
        }

        private void ApplyTrixSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new TrixSettings
                {
                    Period = int.Parse(TrixPeriod.Text)
                };

                if (ValidateTrixSettings(settings))
                {
                    IndicatorSettingsChanged?.Invoke(this, new IndicatorSettingsEventArgs
                    {
                        IndicatorType = IndicatorType.TRIX,
                        Settings = settings
                    });

                    SaveTrixSettings(settings);
                    TrixSettingsPopup.IsOpen = false;
                    ShowSuccessMessage("Настройки TRIX сохранены");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при сохранении настроек TRIX: {ex.Message}");
            }
        }

        private void ApplyAdxSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new AdxSettings
                {
                    Period = int.Parse(AdxPeriod.Text),
                    StrongTrendLevel = double.Parse(AdxStrongTrend.Text),
                    VeryStrongTrendLevel = double.Parse(AdxVeryStrongTrend.Text)
                };

                if (ValidateAdxSettings(settings))
                {
                    IndicatorSettingsChanged?.Invoke(this, new IndicatorSettingsEventArgs
                    {
                        IndicatorType = IndicatorType.ADX,
                        Settings = settings
                    });

                    SaveAdxSettings(settings);
                    AdxSettingsPopup.IsOpen = false;
                    ShowSuccessMessage("Настройки ADX сохранены");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при сохранении настроек ADX: {ex.Message}");
            }
        }

        #endregion

        #region Validation Methods

        private bool ValidateMacdSettings(MacdSettings settings)
        {
            if (settings.FastPeriod <= 0 || settings.SlowPeriod <= 0 || settings.SignalPeriod <= 0)
            {
                ShowErrorMessage("Все периоды должны быть положительными числами");
                return false;
            }

            if (settings.FastPeriod >= settings.SlowPeriod)
            {
                ShowErrorMessage("Быстрый период должен быть меньше медленного");
                return false;
            }

            return true;
        }

        private bool ValidateRsiSettings(RsiSettings settings)
        {
            if (settings.Period <= 0)
            {
                ShowErrorMessage("Период RSI должен быть положительным числом");
                return false;
            }

            if (settings.OverboughtLevel <= settings.OversoldLevel)
            {
                ShowErrorMessage("Уровень перекупленности должен быть выше уровня перепроданности");
                return false;
            }

            if (settings.OverboughtLevel > 100 || settings.OversoldLevel < 0)
            {
                ShowErrorMessage("Уровни RSI должны быть в диапазоне от 0 до 100");
                return false;
            }

            return true;
        }

        private bool ValidateStochasticSettings(StochasticSettings settings)
        {
            if (settings.KPeriod <= 0 || settings.DPeriod <= 0)
            {
                ShowErrorMessage("Периоды должны быть положительными числами");
                return false;
            }

            if (settings.SensitivityLevel <= 0 || settings.SensitivityLevel >= 50)
            {
                ShowErrorMessage("Зона чувствительности должна быть в диапазоне от 0 до 50");
                return false;
            }

            return true;
        }

        private bool ValidateTrixSettings(TrixSettings settings)
        {
            if (settings.Period <= 0)
            {
                ShowErrorMessage("Период TRIX должен быть положительным числом");
                return false;
            }

            return true;
        }

        private bool ValidateAdxSettings(AdxSettings settings)
        {
            if (settings.Period <= 0)
            {
                ShowErrorMessage("Период ADX должен быть положительным числом");
                return false;
            }

            if (settings.StrongTrendLevel >= settings.VeryStrongTrendLevel)
            {
                ShowErrorMessage("Уровень сильного тренда должен быть меньше уровня очень сильного тренда");
                return false;
            }

            if (settings.StrongTrendLevel < 0 || settings.VeryStrongTrendLevel > 100)
            {
                ShowErrorMessage("Уровни тренда должны быть в диапазоне от 0 до 100");
                return false;
            }

            return true;
        }

        #endregion

        #region Settings Persistence

        private void LoadSettings()
        {
            try
            {
                // Load MACD settings
                MacdFastPeriod.Text = Properties.Settings.Default.MacdFastPeriod.ToString();
                MacdSlowPeriod.Text = Properties.Settings.Default.MacdSlowPeriod.ToString();
                MacdSignalPeriod.Text = Properties.Settings.Default.MacdSignalPeriod.ToString();

                // Load RSI settings
                RsiPeriod.Text = Properties.Settings.Default.RsiPeriod.ToString();
                RsiOverboughtLevel.Text = Properties.Settings.Default.RsiOverboughtLevel.ToString();
                RsiOversoldLevel.Text = Properties.Settings.Default.RsiOversoldLevel.ToString();

                // Load Stochastic settings
                StochasticKPeriod.Text = Properties.Settings.Default.StochasticKPeriod.ToString();
                StochasticDPeriod.Text = Properties.Settings.Default.StochasticDPeriod.ToString();
                StochasticSensitivity.Text = Properties.Settings.Default.StochasticSmoothK.ToString();

                // Load TRIX settings
                TrixPeriod.Text = Properties.Settings.Default.TrixPeriod.ToString();

                // Load ADX settings
                AdxPeriod.Text = Properties.Settings.Default.AdxPeriod.ToString();
                AdxStrongTrend.Text = Properties.Settings.Default.AdxStrongTrend.ToString();
                AdxVeryStrongTrend.Text = Properties.Settings.Default.AdxVeryStrongTrend.ToString();

                // Load background mode setting
                BackgroundModeToggle.IsChecked = Properties.Settings.Default.BackgroundMode;
                BackgroundModeToggle.Checked += BackgroundModeToggle_Changed;
                BackgroundModeToggle.Unchecked += BackgroundModeToggle_Changed;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        private void SaveMacdSettings(MacdSettings settings)
        {
            Properties.Settings.Default.MacdFastPeriod = settings.FastPeriod;
            Properties.Settings.Default.MacdSlowPeriod = settings.SlowPeriod;
            Properties.Settings.Default.MacdSignalPeriod = settings.SignalPeriod;
            Properties.Settings.Default.Save();
        }

        private void SaveRsiSettings(RsiSettings settings)
        {
            Properties.Settings.Default.RsiPeriod = settings.Period;
            Properties.Settings.Default.RsiOverboughtLevel = settings.OverboughtLevel;
            Properties.Settings.Default.RsiOversoldLevel = settings.OversoldLevel;
            Properties.Settings.Default.Save();
        }

        private void SaveStochasticSettings(StochasticSettings settings)
        {
            Properties.Settings.Default.StochasticKPeriod = settings.KPeriod;
            Properties.Settings.Default.StochasticDPeriod = settings.DPeriod;
            Properties.Settings.Default.StochasticSmoothK = settings.SensitivityLevel;
            Properties.Settings.Default.Save();
        }

        private void SaveTrixSettings(TrixSettings settings)
        {
            Properties.Settings.Default.TrixPeriod = settings.Period;
            Properties.Settings.Default.Save();
        }

        private void SaveAdxSettings(AdxSettings settings)
        {
            Properties.Settings.Default.AdxPeriod = settings.Period;
            Properties.Settings.Default.AdxStrongTrend = settings.StrongTrendLevel;
            Properties.Settings.Default.AdxVeryStrongTrend = settings.VeryStrongTrendLevel;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Helper Methods

        private void CloseAllPopups()
        {
            MacdSettingsPopup.IsOpen = false;
            RsiSettingsPopup.IsOpen = false;
            StochasticSettingsPopup.IsOpen = false;
            TrixSettingsPopup.IsOpen = false;
            AdxSettingsPopup.IsOpen = false;
        }

        private void BackgroundModeToggle_Changed(object sender, RoutedEventArgs e)
        {
            bool isEnabled = BackgroundModeToggle.IsChecked ?? false;
            Properties.Settings.Default.BackgroundMode = isEnabled;
            Properties.Settings.Default.Save();

            BackgroundModeChanged?.Invoke(this, isEnabled);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }

    #region Event Args and Settings Classes

    public class IndicatorSettingsEventArgs : EventArgs
    {
        public IndicatorType IndicatorType { get; set; }
        public object Settings { get; set; }
    }

    public enum IndicatorType
    {
        MACD,
        RSI,
        Stochastic,
        TRIX,
        ADX
    }

    public class MacdSettings
    {
        public int FastPeriod { get; set; } = 12;
        public int SlowPeriod { get; set; } = 26;
        public int SignalPeriod { get; set; } = 9;
    }

    public class RsiSettings
    {
        public int Period { get; set; } = 14;
        public double OverboughtLevel { get; set; } = 70;
        public double OversoldLevel { get; set; } = 30;
    }

    public class StochasticSettings
    {
        public int KPeriod { get; set; } = 14;
        public int DPeriod { get; set; } = 3;
        public int SensitivityLevel { get; set; } = 20;
    }

    public class TrixSettings
    {
        public int Period { get; set; } = 14;
    }

    public class AdxSettings
    {
        public int Period { get; set; } = 14;
        public double StrongTrendLevel { get; set; } = 25;
        public double VeryStrongTrendLevel { get; set; } = 50;
    }

    #endregion
}