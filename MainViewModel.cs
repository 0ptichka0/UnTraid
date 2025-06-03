using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Controls;

namespace UnTraid.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Fields
        private UserControl _currentView;
        private string _activeSection = "Home";
        #endregion

        #region Properties
        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public string ActiveSection
        {
            get => _activeSection;
            set
            {
                _activeSection = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand NavigateCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);

            // Устанавливаем начальную страницу
            Navigate("Home");
        }
        #endregion

        #region Methods
        private void Navigate(object parameter)
        {
            if (parameter is string section)
            {
                ActiveSection = section;

                switch (section)
                {
                    case "Home":
                        CurrentView = new Views.HomeView();
                        break;
                    case "User":
                        CurrentView = new Views.UserDataView();
                        break;
                    case "Settings":
                        CurrentView = new Views.SettingsView();
                        break;
                    default:
                        CurrentView = new Views.HomeView();
                        break;
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // Простая реализация ICommand для навигации
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}