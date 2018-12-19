using System;
using Template10.Common;
using Template10.Utils;
using Windows.UI.Xaml;

namespace BrailleApp.Services.SettingsServices
{
    public class SettingsService
    {
        public static SettingsService Instance { get; } = new SettingsService();
        Template10.Services.SettingsService.ISettingsHelper _helper;
        private SettingsService()
        {
            _helper = new Template10.Services.SettingsService.SettingsHelper();
        }

        public bool UseShellBackButton
        {
            get { return _helper.Read<bool>(nameof(UseShellBackButton), true); }
            set
            {
                _helper.Write(nameof(UseShellBackButton), value);
                BootStrapper.Current.NavigationService.GetDispatcherWrapper().Dispatch(() =>
                {
                    BootStrapper.Current.ShowShellBackButton = value;
                    BootStrapper.Current.UpdateShellBackButton();
                });
            }
        }

        // IP Adresse des Metec Treibers
        public string metec_ip
        {
            get { return _helper.Read<string>(nameof(metec_ip), ""); }
            set
            {
                _helper.Write(nameof(metec_ip), value);
            }
        }

        public string metec_port
        {
            get { return _helper.Read<string>(nameof(metec_port), "2017"); }
            set
            {
                _helper.Write(nameof(metec_port), value);
            }
        }

        public string metec_device_id
        {
            get { return _helper.Read<string>(nameof(metec_device_id), ""); }
            set
            {
                _helper.Write(nameof(metec_device_id), value);
            }
        }

        // Debug Mode
        public bool debugMode
        {
            get { return _helper.Read<bool>(nameof(debugMode), false); }
            set
            {
                _helper.Write(nameof(debugMode), value);
            }
        }
        
        // Width. Count of horizontal pins
        public int metec_device_pin_count_h
        {
            get { return _helper.Read<int>(nameof(metec_device_pin_count_h), 0); }
            set
            {
                _helper.Write(nameof(metec_device_pin_count_h), value);
            }
        }

        // Height. Count of vertical pins
        public int metec_device_pin_count_v
        {
            get { return _helper.Read<int>(nameof(metec_device_pin_count_v), 0); }
            set
            {
                _helper.Write(nameof(metec_device_pin_count_v), value);
            }
        }

        // Pin Step: pinStep / 2 = 2.5 => DOT Strocke
        public int metec_device_pin_step
        {
            get { return _helper.Read<int>(nameof(metec_device_pin_step), 0); }
            set
            {
                _helper.Write(nameof(metec_device_pin_step), value);
            }
        }


        // Button from Device as JSON
        public string metec_device_btn
        {
            get { return _helper.Read<string>(nameof(metec_device_btn), ""); }
            set
            {
                _helper.Write(nameof(metec_device_btn), value);
            }
        }


        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Light;
                var value = _helper.Read<string>(nameof(AppTheme), theme.ToString());
                return Enum.TryParse<ApplicationTheme>(value, out theme) ? theme : ApplicationTheme.Dark;
            }
            set
            {
                _helper.Write(nameof(AppTheme), value.ToString());
                (Window.Current.Content as FrameworkElement).RequestedTheme = value.ToElementTheme();
            }
        }

        public TimeSpan CacheMaxDuration
        {
            get { return _helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }
    }
}
