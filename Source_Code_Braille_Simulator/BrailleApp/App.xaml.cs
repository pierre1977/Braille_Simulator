using System;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using BrailleApp.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Mvvm;
using Template10.Common;
using System.Linq;
using Windows.UI.Xaml.Data;

using Braille.Lan;

namespace BrailleApp
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
   

    [Bindable]
    sealed partial class App : BootStrapper
    {
        #region Globales Object        
        // https://stackoverflow.com/questions/27402797/c-sharp-xaml-how-to-define-global-object-for-all-the-pages-in-the-app
        private static ClientConnection _ClientConnection = null;
        public static ClientConnection ClientConnection
        {
            get
            {
                if (_ClientConnection == null)
                    _ClientConnection = new ClientConnection();

                return _ClientConnection;
            }
            set { }
        }

        private static SettingsService _settings = null;
        public static SettingsService settings
        {
            get
            {
                if (_settings == null)
                    _settings = SettingsService.Instance;

                return _settings;
            }
            set { }
        }


        #endregion

        public App()
        {
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region app settings

            // some settings must be set in app.constructor
            settings = SettingsService.Instance;

            RequestedTheme = settings.AppTheme;
            //CacheMaxDuration = settings.CacheMaxDuration;  // zeit für die Dauer der Speichers
            ShowShellBackButton = settings.UseShellBackButton;

            //GC.Collect(1, GCCollectionMode.Optimized);
            //GC.Collect(2, GCCollectionMode.Optimized);
            //GC.Collect(3, GCCollectionMode.Optimized);
            GC.AddMemoryPressure(100000000);
            GC.KeepAlive(ClientConnection);

            #endregion
        }


        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // TODO: add your long-running task here
            await NavigationService.NavigateAsync(typeof(Views.MainPage));
        }
    }
}
