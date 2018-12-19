using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace BrailleApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        // Settings zum Speichern
        Services.SettingsServices.SettingsService _settings;

        // Binding DisplayName
        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        // Version
        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return DisplayName + " " + $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }


        public MainPageViewModel()
        {
            // Instanz
            _settings = Services.SettingsServices.SettingsService.Instance;

        }


        // Var zum Binding, Speichern und Laden von Einstellungen
        public string settings_metec_ip
        {
            get { return _settings.metec_ip; }
            set { _settings.metec_ip = value; base.RaisePropertyChanged(); }
        }

        public string settings_metec_port
        {
            get { return _settings.metec_port; }
            set { _settings.metec_port = value; base.RaisePropertyChanged(); }
        }


        string _Value = "Gas";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                Value = suspensionState[nameof(Value)]?.ToString();
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        // PAGE NAVIGATION

        //public void GotoDetailsPage() =>
        //    NavigationService.Navigate(typeof(Views.DetailPage), Value);

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        //public void GotoPrivacy() =>
        //    NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

    }
}
