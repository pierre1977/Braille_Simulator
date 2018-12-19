using System;
using BrailleApp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.Storage;

using BrailleApp;
using Braille.Lan;
using Template10.Mvvm;
using System.Threading.Tasks;
using System.Text;
using Template10.Services.NavigationService;
using System.Collections.Generic;
using Newtonsoft.Json;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using Windows.Foundation;

namespace BrailleApp.Views
{
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            
            // Load Page Settings
            // https://docs.microsoft.com/de-de/windows/uwp/design/app-settings/store-and-retrieve-app-data
            //ApplicationData.Current.LocalSettings.Values["UseShellBackButton"];
            // Object value = ApplicationData.Current.LocalSettings.Values["UseShellBackButton"];
            // Object valueip = ApplicationData.Current.LocalSettings.Values["mvbd_ip"];            
        }


        // Power Down
        private void PointerReleased_PowerDown(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Shutdown, TimeSpan.FromSeconds(1));
        }

        // Power Restart
        private void PointerReleased_PowerRestart(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));
        }

        // Lan Connection
        private async void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            // Wenn keine Connection
            if (App.ClientConnection.IsConnected == false)
            {
                if (this.tb_Ip.Text != "" && this.tb_Port.Text != "")
                {
                    int port = 0;
                    if (Int32.TryParse(tb_Port.Text, out port))
                    {
                        App.ClientConnection.Ip = this.tb_Ip.Text;
                        App.ClientConnection.Port = port;

                        // Verbindung herstellen
                        Views.Busy.SetBusy(true, "Connecting");
                        for (int i = 0; i < 10; i++)
                        {
                            App.ClientConnection.Connect();
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            if (App.ClientConnection.IsConnected == true)
                            {
                                // Connected
                                Views.Busy.SetBusy(true, "Connected");
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                this.btn_connect.Content = "Trennen";

                                // Devices Laden
                                Views.Busy.SetBusy(true, "Get Devices");
                                App.ClientConnection.Send(Commands.getDeviceList, 0);

                                // Prüfen ob Daten vorhanden sind
                                // Daten abfragen und Combobox Erstellen, Select device aus dem Speicher
                                byte[] deviceBytes = new byte[0];
                                for (int j = 0; j < 10; j++)
                                {
                                    deviceBytes = App.ClientConnection.getCommandDaten;
                                    if (deviceBytes == null ||  deviceBytes.Length == 0)
                                    {
                                        // Wait
                                        await Task.Delay(TimeSpan.FromSeconds(1));
                                    }
                                    else {
                                        // Ende
                                        break;
                                    }
                                }

                                // Combobox Items Erstellen
                                ComboBoxItem[] cbi = createDeviceCombobox(deviceBytes);
                                // Combobox löschen und füllen
                                this.cb_device.ItemsSource = null;
                                this.cb_device.ItemsSource = cbi;

                                // Selected Index suchen, Selected Device ID aus dem Speicher
                                int selIndex = findSelectedIndex(this.cb_device, App.settings.metec_device_id);
                                this.cb_device.SelectedIndex = selIndex;

                                // Applikations Button Aktivieren, Simulator und Notepad
                                //this.btn_run_notepad.IsEnabled = true;
                                if (selIndex != -1) {
                                    this.btn_run_simulator.IsEnabled = true;
                                }

                                // ende schleife
                                break;                                
                            }
                            else
                            {
                                await Task.Delay(TimeSpan.FromSeconds(3));
                            }
                        }                        
                        Views.Busy.SetBusy(false);
                        
                    }
                }
            }
            else
            // Wenn Connected, dann disconnect
            {               
                this.btn_connect.Content = "Verbinden";
                App.ClientConnection.Close();
            }


        }

        
        // Response 26. List of virtual devices received
        public ComboBoxItem[] createDeviceCombobox(byte[] ba)
        {
            // Anzahl der Devices
            int pos = 0;
            int count = ba[pos++];         // erstes Byte
            
            ComboBoxItem[] cbList = new ComboBoxItem[count];

            for (int i = 0; i < count; i++)
            {
                ComboBoxItem curr = new ComboBoxItem();
                // Device ID
                int deviceId = (ba[pos++] | ba[pos++] << 8 | ba[pos++] << 16 | ba[pos++] << 24);    // 4106
                curr.Tag = deviceId;

                // Device Name
                int len = ba[pos++];
                string s = Encoding.ASCII.GetString(ba, pos, len);
                pos += len;
               // curr.Name = s;
                curr.Content = s;

                // Add to List
                cbList[i] = curr;
            }

            return cbList;
        }

        public int findSelectedIndex(ComboBox cb, string tag)
        {
            int ret = -1;

            for (int i = 0; i < cb.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)cb.Items[i];
                if (item.Tag.ToString() == tag)
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        //
        private void cb_device_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBoxItem curr = (e.AddedItems[0] as ComboBoxItem);
                string metec_device_id = curr.Tag.ToString();
                App.settings.metec_device_id = metec_device_id;               

                // Applikations Button Aktivieren, Simulator
                if ( Int32.Parse( metec_device_id) != -1)
                {
                    this.btn_run_simulator.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                // Applikations Button Deaktiviern, Simulator
                this.btn_run_simulator.IsEnabled = false;
            }

        }

        // Simulator starten
        private async void btn_run_simulator_Click(object sender, RoutedEventArgs e)
        {
            // Device ID Prüfen
            if (App.settings.metec_device_id != "")
            {
                Views.Busy.SetBusy(true, "Prepare Simulator");

                /////////////////////////////////////////////
                // MVDB Infos Anfragen - Synchron
                /////////////////////////////////////////////

                //---------------------------------
                // Send Device
                //---------------------------------
                App.ClientConnection.Send(Braille.Lan.Commands.sendDevice, Int32.Parse(App.settings.metec_device_id) );
                await Task.Delay(TimeSpan.FromSeconds(1));

                //---------------------------------
                // Set Application  MVBD
                //---------------------------------
                App.ClientConnection.Send(Braille.Lan.Commands.sendClientId, 1);
                await Task.Delay(TimeSpan.FromSeconds(1));

                // TODO SET ROOT COMMAND


                //---------------------------------
                // Get Device Infos
                //---------------------------------
                App.ClientConnection.Send(Braille.Lan.Commands.getDeviceInfos);
                // Prüfen ob Antwort vorhanden sind
                byte[] data = new byte[0];
                for (int j = 0; j < 20; j++)
                {
                    data = App.ClientConnection.getCommandDaten;
                    if (data == null || data.Length == 0)
                    {
                        // Wait
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    else
                    {
                        try
                        {
                            // Speichern in App, Count of horizontal & vertical pins 
                            App.settings.metec_device_pin_count_h = data[0];
                            App.settings.metec_device_pin_count_v = data[1];

                            // TODO Rotation
                            // Byte 2: Working position. On witch side of the device are you working. (0=Front, 1= Right, 2=Rear, 3=Left)
                        }
                        catch (Exception ex)
                        {
                            Views.Busy.SetBusy(true, "OnDataRecived 20: " + ex.Message);
                            await Task.Delay(TimeSpan.FromSeconds(3));
                            Views.Busy.SetBusy(false);
                        }
                        // Ende
                        break;
                    }
                }



                //---------------------------------
                // Get MVBD Version
                //---------------------------------
                App.ClientConnection.Send(Braille.Lan.Commands.getMVBDVersion);
                // Prüfen ob Antwort vorhanden sind
                data = new byte[0];
                int version = 0;
                for (int j = 0; j < 5; j++)
                {
                    data = App.ClientConnection.getCommandDaten;
                    if (data == null || data.Length == 0)
                    {
                        // Wait
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    else
                    {
                        version = data[0];
                    }
                }

                //---------------------------------
                // Get DeviceGraphic und Button Infos, wird nur ab MVBD Treiber 1.7 unterstützt
                //---------------------------------
                if (version > 116)
                {
                    List<int[]> btn_rawList = new List<int[]>();        // zum Aufbereiten der Button Daten, wird in App Settings gespeichert

                    App.ClientConnection.Send(Braille.Lan.Commands.getDeviceGraphic);
                    // Prüfen ob Antwort vorhanden sind
                    data = new byte[0];
                    for (int j = 0; j < 20; j++)
                    {
                        data = App.ClientConnection.getCommandDaten;
                        if (data == null || data.Length == 0)
                        {
                            // Wait
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        else
                        {
                            try
                            {
                                // Pin Daten: pinStep / 2 = 2.5 => DOT Strocke
                                int pinStep = (data[18]);                       // Bsp: 5
                                App.settings.metec_device_pin_step = pinStep;

                                // NOT Used
                                bool isBrailleLine = (data[19] != 0);     // = 0 also keine Zeile sondern Display, d.h. andere Darstellungsform -> TODO wird nicht unterstützt

                                //int pinCountX = (data[20]);               // = 104  // gleichen Werte wie Device Infos 
                                //int pinCountY = (data[21]);               // = 60

                                // Buttons
                                int count = data[26];     // Bsp: 36
                                int datenIndex = 26 + 1;
                                for (int i = 0; i < count; i++)
                                {
                                    int keyIndex = (data[datenIndex++]);        // 00
                                    int keyIsCircle = (data[datenIndex++]);     // 01 (True false)
                                    int keyX = (data[datenIndex++] | data[datenIndex++] << 8);  // 02
                                    int keyY = (data[datenIndex++] | data[datenIndex++] << 8);  // 03
                                    int keyWidth = (data[datenIndex++] | data[datenIndex++] << 8);  // 04
                                    int keyHeight = (data[datenIndex++] | data[datenIndex++] << 8);  // 05

                                    // Add to RAW Array List
                                    int[] array = new int[] { keyIndex, keyIsCircle, keyX, keyY, keyWidth, keyHeight };
                                    btn_rawList.Add(array);
                                }
                                // Daten Speichern als JSON
                                App.settings.metec_device_btn = JsonConvert.SerializeObject(btn_rawList);
                            }
                            catch (Exception ex)
                            {
                                Views.Busy.SetBusy(true, "OnDataRecived 52: " + ex.Message);
                                await Task.Delay(TimeSpan.FromSeconds(3));
                                Views.Busy.SetBusy(false);
                            }
                            // Ende
                            break;
                        }
                    }
                    Views.Busy.SetBusy(true, "Start Simulator");
                }
                else
                {
                    Views.Busy.SetBusy(true, "MVBD unknown - error-prone ! but Start Simulator");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                
                // Page aufruf mit Parameter
                ViewModel.NavigationService.Navigate(typeof(Views.SimulatorPage), App.ClientConnection);
            }
        }
    }
}