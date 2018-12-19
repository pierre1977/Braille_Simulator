using Metec.MVBD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using BrailleDisplay;
using Windows.UI.ViewManagement;
using Newtonsoft.Json;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Diagnostics;

namespace BrailleApp.Views
{

    // Bool to Int, erstellt die ToInt Methode
    public static class BooleanExtensions
    {
        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }
    }


    /// <summary>
    /// Simulator Page
    /// </summary>
    public sealed partial class SimulatorPage : Page
    {
        #region VARs

        // Vars
        bool debug = true;
        Template10.Services.SerializationService.ISerializationService _SerializationService;
        string deviceId;

        // Button Liste
        private List<BrailleSingleButton.Button> buttonsList = new List<BrailleSingleButton.Button>();

        // Layout
        private double sizeFullHeight = -1;
        private double sizeFullWidth = -1;
        private double sizeGridHeight = -1;
        private double sizeBtnViewbox = -1;
        private double sizePercentGridHeight = 0;
        private double sizePercentBtnStackHeight = 0;

        private bool HamburgerIsOpen = true;
        private int sizeBtnViewboxClosed = 20;

        private double sizeGridHeightClosed = -1;
        private double sizePercentGridHeightClosed = 0;
        private double sizePercentBtnStackHeightClosed = 0;

        #endregion

        public SimulatorPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;     // Erzeugt immmer eine neue Instanz, eventhändler müssen nicht entfernt werden!
           
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;

            // Debug Mode von App Settings
            debug = App.settings.debugMode;

            // Braille Area Init
            double dotSpace = (double)App.settings.metec_device_pin_step / 2.0; // 5:2 = 2.5
            double with = (double)App.settings.metec_device_pin_count_h * dotSpace;
            double hight = (double)App.settings.metec_device_pin_count_v * dotSpace;
            brailleArea.initBrailleArea(with, hight, dotSpace, 0.7);

            // Button Liste Erstellen
            List<int[]> btn_rawList = new List<int[]>();
            btn_rawList = JsonConvert.DeserializeObject<List<int[]>>(App.settings.metec_device_btn);
            create_device_Btn(btn_rawList, this.smalStackPanel);

            // Bugfix - Left Mouse Click to Pointer
            //https://stackoverflow.com/questions/14767020/pointerpressed-not-working-on-left-click
            HamburgerButton.AddHandler(PointerPressedEvent, new PointerEventHandler(HamburgerButton_PointerPressed), true);
            BackButton.AddHandler(PointerPressedEvent, new PointerEventHandler(BackButton_PointerPressed), true);

            //init_mvbd();

            // Loaded Page Event
            this.Loaded += ThisPage_Loaded;

            // Socket Client Event
            App.ClientConnection.OnError += ClientConnection_OnError; ;
            App.ClientConnection.OnDataRecivedPin += ClientConnection_OnDataRecivedPin;

            // Custom Pointer Events
            brailleArea.dot_PointerEntered += BrailleArea_dot_Pointer_Send;
            brailleArea.dot_PointerMoved += BrailleArea_dot_Pointer_Send;
            brailleArea.dot_PointerReleased += BrailleArea_dot_Pointer_Send;
            brailleArea.dot_PointerExit += BrailleArea_dot_Pointer_Send;

            // Gesture Events
            brailleArea.gestureDetected += BrailleArea_gestureDetected;
        }



        // Page fertig geladen
        private void ThisPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Size und Größenverhältnis Speichern, wenn Hamburger Open
            sizeFullHeight = brailleArea.ActualHeight + mainBtnViewbox.ActualHeight;
            sizeFullWidth = brailleArea.ActualWidth;

            sizeGridHeight = brailleArea.ActualHeight;
            sizeBtnViewbox = mainBtnViewbox.ActualHeight;

            sizePercentGridHeight = (100 / sizeFullHeight) * sizeGridHeight;
            sizePercentBtnStackHeight = (100 / sizeFullHeight) * sizeBtnViewbox;

            // wenn Hamburger ist Closed,

            sizePercentGridHeightClosed = (100 / (brailleArea.ActualHeight + sizeBtnViewboxClosed)) * sizeGridHeight;
            sizePercentBtnStackHeightClosed = (100 / (brailleArea.ActualHeight + sizeBtnViewboxClosed)) * (sizeBtnViewboxClosed + 0);
            
            this.resizePage();

            // Default Framework Eventhandler
            brailleArea.LayoutUpdated += BrailleArea_LayoutUpdated;

            Views.Busy.SetBusy(false);
        }
        


        // Übergebenen Parameter lesen, Device ID
        // Nicht notwendig
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    // Parameter als Json Parsen
        //    _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
        //    this.deviceId = _SerializationService.Deserialize(e.Parameter?.ToString()).ToString();
        //    this.txt_Test.Text = this.deviceId;
        //}


        // Device bei MVBD einstellen
        private async void init_mvbd()
        {
            if (App.ClientConnection.IsConnected == true)
            {
                // Eventhandler
                try
                {
                    // Alle schliessen, wenn zurück und wieder auf die seite
                    App.ClientConnection.OnDataRecived -= ClientConnection_OnDataRecived;
                    App.ClientConnection.OnDataRecivedCmd -= ClientConnection_OnDataRecivedCmd;
                   // App.ClientConnection.OnError -= ClientConnection_OnError;
                    await System.Threading.Tasks.Task.Delay(1000); 
                }
                catch (Exception ex)
                {
                    int ii = 3;
                }
                finally
                {
                    // neu erstellen
                    App.ClientConnection.OnDataRecived += ClientConnection_OnDataRecived;
                    App.ClientConnection.OnDataRecivedCmd += ClientConnection_OnDataRecivedCmd;
                   // App.ClientConnection.OnError += ClientConnection_OnError;
                }              
            }
        }


        #region Eventhandler


        private void BrailleArea_dot_Pointer_Send(uint finger, bool contact, Point dots, Point pointer)
        {
            if (debug == true)
            { 
                if (this.txt_Test.RenderSize.Height > 500)
                {
                    this.txt_Test.Text = "";
                }
                this.txt_Test.Text += "Fid: " + finger.ToString() + " C: " + contact.ToString() + " dot X/Y: " + dots.X.ToString() + "/" + dots.Y.ToString() + " pos X/Y: " + pointer.X.ToString() + "/" + pointer.Y.ToString() + "\n";

                //// Dictionary erstellen als parameter für LAN send
                //Dictionary<string, int> dict = new Dictionary<string, int> {
                //    { "id", (int)(finger-1) },
                //    { "c", Convert.ToInt32(contact) },
                //    { "dotX", (int)dots.X },
                //    { "dotY", (int)dots.Y },
                //    { "pX", (int)pointer.X },
                //    { "pY", (int)pointer.Y }
                //};

                //// Parameter als Json 
                //string parameter = _SerializationService.Serialize(dict);
                //this.txt_Test.Text += parameter;

            }
            App.ClientConnection.SendFinger( finger,  contact,  dots,  pointer);

        }

        private void BrailleArea_gestureDetected(string gestureType, Point start, Point end)
        {
            if (debug == true)
            {
                if (this.txt_Test.RenderSize.Height > 500)
                {
                    this.txt_Test.Text = "";
                }
                this.txt_Test.Text += "Gesture: " + gestureType + " s X/Y: " + start.X.ToString() + "/" + start.Y.ToString() + " e X/Y: " + end.X.ToString() + "/" + end.Y.ToString() + "\n";
            }
        }


        // Datenempfang für gesendete Befehle bzw. Anfragen
        private void ClientConnection_OnDataRecivedCmd(byte[] data)
        { 
            // Get Header-Daten
            byte[] headerSend = App.ClientConnection.getCommandHeader;

            // 20. Device Infos Empfangen
            try
            {               
                if (headerSend[0] == (byte)Braille.Lan.Commands.getDeviceInfos)
                {

                }
            }
            catch (Exception ex)
            {
                show_Exeption("OnDataRecivedCmd 20: " + ex.Message);
            }

            // 52. Device Grafik und Buttons Empfangen
            try
            {
                if (headerSend[0] == (byte)Braille.Lan.Commands.getDeviceGraphic)
                {

                }
            }
            catch (Exception ex)
            {
                show_Exeption("OnDataRecivedCmd 52: " + ex.Message);
            }
        }

        // Datenempfang ohne Anfrage
        private void ClientConnection_OnDataRecived(byte[] data)
        {
        }

        // Datenempfang ohne Anfrage für Pins
        private void ClientConnection_OnDataRecivedPin(byte[] data)
        {
            // Übertragen wird immer ein bereich, als rechteck bzw. ausschnitt des displays
            byte startX = data[0];
            byte startY = data[1];
            byte endX = data[2];    // Width
            byte endY = data[3];    // Height

            int currentDotX = startX+1;
            int currentDotY = startY+1;

            int maxCount = endX * endY;

            // ab [4] die pin-daten, jedes byte enthält 8 bits für jeden pin
            for (int i = 4; i < data.Length; i++)
            {
                // bits des bytes lesen, LittleEndian -> links der erste Wert: 1001 110|1 <=
                // Es kann zu fehl IDs kommen, dass letzte Byte kann OutOfIndex erzeugen
                for (int bitNumber = 0; bitNumber < 8; bitNumber++)
                {
                    int id = 0;
                    var bit = (data[i] & (1 << bitNumber)) != 0;
                    if (bit == true)
                    {
                        // set ON
                        try
                        {
                            id = brailleArea.getDotIdFromXY(currentDotX, currentDotY);
                            if ((maxCount >= id) && (id > 0))
                            {
                                brailleArea.setDotOn(id);
                            }
                        }
                        catch (Exception ex) {
                            //Debug.Write("Ex - ID: " + id + " x/y:" + currentDotX + " / " + currentDotY);
                        }
                    }
                    else
                    {
                        // set OFF
                        try { 
                            id = brailleArea.getDotIdFromXY(currentDotX, currentDotY);
                            if ((maxCount >= id) && (id > 0))
                            {
                                brailleArea.setDotOff(id);
                            }
                        }
                        catch (Exception ex) {
                            //Debug.Write("Ex - ID: " + id + " x/y:" + currentDotX + " / " + currentDotY);
                        }
                    }

                    // Weite Prüfen
                    if ( (bitNumber >= endX) || (currentDotX >= endX) )
                    {
                        // Nächste Zeile
                        currentDotX = startX+1;
                        currentDotY++;
                    }
                    else {
                        currentDotX++;
                    }
                }
            }


        }

        // Fehler ausgabe
        private async void ClientConnection_OnError(string message)
        {
            Views.Busy.SetBusy(false);
            Views.Busy.SetBusy(true, "Connection Error: " + message);
            await Task.Delay(TimeSpan.FromSeconds(3));
            Views.Busy.SetBusy(false);
        }



        #endregion


        #region Layout Size
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sizeFullHeight != -1)
            {
                this.resizePage();
            }
            // Screen Infos neu laden
            brailleArea.getScreenInformation();
            if (HamburgerIsOpen == true)
            {
                double val = (nav_grid.ActualWidth - ((mainBtnStackPanel.Margin.Left + mainBtnStackPanel.Margin.Right) * 2));
                if( val < 0)
                {
                    val = 1;
                }
                mainBtnViewbox.Width = val;
            }
        }

        private void BrailleArea_LayoutUpdated(object sender, object e)
        {
            if (brailleArea.Height != nav_grid.Height)
            {
                nav_grid.Height = brailleArea.Height;
            }
            if (HamburgerIsOpen != true)
            {
                //       mainBtnViewbox.Width = (nav_grid.ActualWidth - ((mainBtnStackPanel.Margin.Left + mainBtnStackPanel.Margin.Right) * 2));
            }
        }

        private void resizePage()
        {
            // größe Abfragen
            var windowsSize = ApplicationView.GetForCurrentView().VisibleBounds;

            // 1. Weite Setzten
            if (windowsSize.Width < sizeFullWidth)
            {
                mainBtnViewbox.Width = windowsSize.Width; // nav_grid.ActualWidth;
                brailleArea.Width = windowsSize.Width; // nav_grid.ActualWidth;

                // höhe setzten für die Buttons
                brailleArea.Height = Double.NaN; // auto
                mainBtnViewbox.Height = (windowsSize.Height / 100) * sizePercentBtnStackHeight;
            }
            
            // 2. Hohe setzten
            // Prüfen ob Fenster kleiner als normale größe
            if (windowsSize.Height < sizeFullHeight)
            {
                if (HamburgerIsOpen == true)
                {
                    // verkleinern - verhältnis Berechnen
                    brailleArea.Height = (windowsSize.Height / 100) * sizePercentGridHeight;
                    mainBtnViewbox.Height = (windowsSize.Height / 100) * sizePercentBtnStackHeight;

                    double newWidth = (brailleArea.Height / sizeGridHeight) * sizeFullWidth;
                    mainBtnViewbox.Width = (int)newWidth;
                }
                else
                {
                    // Hamburger Closed
                    double newAreaHeight = (windowsSize.Height / 100) * sizePercentGridHeightClosed;

                    if (newAreaHeight < sizeGridHeight)
                    {
                        brailleArea.Height = (windowsSize.Height / 100) * sizePercentGridHeightClosed;
                        mainBtnViewbox.Height = (windowsSize.Height / 100) * sizePercentBtnStackHeightClosed;

                        double newWidth = (brailleArea.Height / sizeGridHeight) * sizeFullWidth;
                        mainBtnViewbox.Width = (int)newWidth;

                    }
                    else
                    {
                        // Normale größe wiederherstellen
                        brailleArea.Height = sizeGridHeight;
                        mainBtnViewbox.Height = sizeBtnViewbox;

                        brailleArea.Width = sizeFullWidth;

                        double newWidth = (brailleArea.Height / sizeGridHeight) * sizeFullWidth;
                        mainBtnViewbox.Width = (int)newWidth;
                    }
                }
                // mainBtnViewbox.Width = (brailleArea.Width - ((mainBtnStackPanel.Margin.Left + mainBtnStackPanel.Margin.Right) * 2));
            }

            // Normale größe setzten
            if ((windowsSize.Height >= sizeFullHeight) && (windowsSize.Width >= sizeFullWidth))
            {
                // Normale größe wiederherstellen
                brailleArea.Height = sizeGridHeight;
                mainBtnViewbox.Height = sizeBtnViewbox;

                brailleArea.Width = sizeFullWidth;
                //mainBtnViewbox.Width = (sizeFullWidth - ((mainBtnStackPanel.Margin.Left + mainBtnStackPanel.Margin.Right) * 2));
            }
        }

        // BACK Button
        private void BackButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.Current.NavigationService.Navigate(typeof(Views.MainPage) );
        }


        // Hamburger Button - Button verstecken
        private void HamburgerButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Init größen der Layoutboxen setzten
            //if (intMainBtnStackPanel == -1)
            //{
            //    intMainBtnStackPanel = mainBtnStackPanel.ActualHeight;
            //}

            if (HamburgerIsOpen == true)
            {
                HamburgerIsOpen = false;

                //mainBtnViewbox.Height = Double.NaN;
                BackButton.Visibility = Visibility.Collapsed;
                HamburgerButton.Content = "\uE70E";

                mainBtnStackPanel.Height = 10;
                mainBtnStackPanel.Margin = new Thickness(2, 0, 2, 0);
                mainBtnStackPanel.Padding = new Thickness(5, 0, 5, 0);

                HamburgerButton.Height = 8.0;
                HamburgerButton.FontSize = 5;

            }
            else
            {
                HamburgerIsOpen = true;

                //mainBtnViewbox.Height = Double.NaN;
                BackButton.Visibility = Visibility.Visible;
                HamburgerButton.Content = "\uE70D";

                mainBtnStackPanel.Height = Double.NaN; // auto;
                mainBtnStackPanel.Margin = new Thickness(2, 2, 2, 2);
                mainBtnStackPanel.Padding = new Thickness(5, 5, 5, 5);

                HamburgerButton.Height = 30.0;
                HamburgerButton.FontSize = 20;
            }
            resizePage();
        }
        
        #endregion



        #region Help Functions

        // Error ausgabe
        private async void show_Exeption(string mess)
        {
            Views.Busy.SetBusy(false);
            Views.Busy.SetBusy(true, "Error: " + mess);
            await System.Threading.Tasks.Task.Delay(3000);
            Views.Busy.SetBusy(false);
        }


        // Erstellt die Navigation und normalen Buttons, incl Name, Funktion, ToolTip und Eventhandler
        private void create_device_Btn( List<int[]> btn_rawList, VariableSizedWrapGrid gridTarget)
        {
            // TODO: Ausrichtung des Device, je Richtung ändern sich auch die Navigations Tasten - UP -> Left oder Right ...

            // RAW Liste auswerten
            for (int i = 0; i < btn_rawList.Count; i++)
            {
                int[] btnData = btn_rawList[i];

                // Navigations Buttons
                if (btnData[0] == 208 || btnData[0] == 212 || btnData[0] == 210 || btnData[0] == 209 || btnData[0] == 211 ||        // Left Side
                    btnData[0] == 216 || btnData[0] == 220 || btnData[0] == 218 || btnData[0] == 217 || btnData[0] == 219           // Right Side
                    )
                {
                    switch (btnData[0])
                    {
                        // UP
                        case 208:
                        case 216:
                            // Add Function
                            brailleNavBtn.add_Function(BrailleNavButton.BrailleNavButton.direction.top, btnData[0]);
                            // Add Name
                            brailleNavBtn.add_Name(BrailleNavButton.BrailleNavButton.direction.top, cutButtonName(Enum.GetName(typeof(DeviceKeys), btnData[0]), true)  );
                            // Add TopTip
                            brailleNavBtn.add_Tooltip(BrailleNavButton.BrailleNavButton.direction.top, Enum.GetName(typeof(DeviceKeys), btnData[0]));
                            break;
                        // Middle
                        case 212:
                        case 220:
                            // Add Function
                            brailleNavBtn.add_Function(BrailleNavButton.BrailleNavButton.direction.middel, btnData[0]);
                            // Add Name
                            brailleNavBtn.add_Name(BrailleNavButton.BrailleNavButton.direction.middel, cutButtonName(Enum.GetName(typeof(DeviceKeys), btnData[0]), true));
                            // Add TopTip
                            brailleNavBtn.add_Tooltip(BrailleNavButton.BrailleNavButton.direction.middel, Enum.GetName(typeof(DeviceKeys), btnData[0]));
                            break;
                        // Down
                        case 210:
                        case 218:
                            // Add Function
                            brailleNavBtn.add_Function(BrailleNavButton.BrailleNavButton.direction.bottom, btnData[0]);
                            // Add Name
                            brailleNavBtn.add_Name(BrailleNavButton.BrailleNavButton.direction.bottom, cutButtonName(Enum.GetName(typeof(DeviceKeys), btnData[0]), true));
                            // Add TopTip
                            brailleNavBtn.add_Tooltip(BrailleNavButton.BrailleNavButton.direction.bottom, Enum.GetName(typeof(DeviceKeys), btnData[0]));
                            break;
                        // Left
                        case 209:
                        case 217:
                            // Add Function
                            brailleNavBtn.add_Function(BrailleNavButton.BrailleNavButton.direction.left, btnData[0]);
                            // Add Name
                            brailleNavBtn.add_Name(BrailleNavButton.BrailleNavButton.direction.left, cutButtonName(Enum.GetName(typeof(DeviceKeys), btnData[0]), true));
                            // Add TopTip
                            brailleNavBtn.add_Tooltip(BrailleNavButton.BrailleNavButton.direction.left, Enum.GetName(typeof(DeviceKeys), btnData[0]));
                            break;
                        // Roght
                        case 211:
                        case 219:
                            // Add Function
                            brailleNavBtn.add_Function(BrailleNavButton.BrailleNavButton.direction.right, btnData[0]);
                            // Add Name
                            brailleNavBtn.add_Name(BrailleNavButton.BrailleNavButton.direction.right, cutButtonName(Enum.GetName(typeof(DeviceKeys), btnData[0]), true));
                            // Add TopTip
                            brailleNavBtn.add_Tooltip(BrailleNavButton.BrailleNavButton.direction.right, Enum.GetName(typeof(DeviceKeys), btnData[0]));
                            break;
                    }
                    // weiter zur nächsten taste
                    continue;
                }
                
                // Create Button und add to list
                buttonsList.Add(new BrailleSingleButton.Button());

                // Btn Item Event Handler setzten
                buttonsList[buttonsList.Count - 1].btn_Pressed += btn_Pressed;
                buttonsList[buttonsList.Count - 1].btn_Released += btn_Released;

                // Set btn ID und Name
                buttonsList[buttonsList.Count - 1].setBtn_Id( btnData[0] );
                buttonsList[buttonsList.Count - 1].setBtn_Name( cutButtonName(  Enum.GetName(typeof(DeviceKeys), btnData[0]), false) );
                buttonsList[buttonsList.Count - 1].setBtn_ToolTip( Enum.GetName(typeof(DeviceKeys), btnData[0]) );

                // Add to Target
                gridTarget.Children.Add(buttonsList[buttonsList.Count - 1]);
            }

            // Nav Button Eventhandler
            brailleNavBtn.btn_nav_Released += BrailleNavBtn_btn_nav_Released;
            brailleNavBtn.btn_nav_Pressed += BrailleNavBtn_btn_nav_Pressed;

        }


        // Kürzt das erste zeichen wenn es gross ist und das zweite auch gross ist
        private string cutButtonName(string name, bool isNavBtn)
        {            
            char[] myChar = name.ToCharArray();
            if (myChar.Length > 1)
            {
                if (isNavBtn == false)
                {
                    if ((char.IsUpper(myChar[0]) == true) && (char.IsUpper(myChar[1]) == true))
                    {
                        string s = new string(myChar, 1, myChar.Length-1);
                        return s;
                    }
                }
                else
                {   // wenn Button dann nur ein zeichen zurück
                    if ((char.IsUpper(myChar[0]) == true) && (char.IsUpper(myChar[1]) == true))
                    {
                        string s = new string(myChar, 1, 1);
                        return s;
                    }
                }
            }
            return name;
        }


        #endregion


        // Eventhandler für Einzelne Buttons
        private void btn_Released(object sender, PointerRoutedEventArgs e, int btnId)
        {
            // Send Key UP
            App.ClientConnection.Send(Braille.Lan.Commands.sendKeyUp, btnId);
        }
        // Eventhandler für Einzelne Buttons
        private void btn_Pressed(object sender, PointerRoutedEventArgs e, int btnId)
        {
            // Send Key Down
            App.ClientConnection.Send(Braille.Lan.Commands.sendKeyDown, btnId);
        }

        private void BrailleNavBtn_btn_nav_Released(object sender, int function)
        {
            // Send Key UP
            App.ClientConnection.Send(Braille.Lan.Commands.sendKeyUp, function);
        }
        private void BrailleNavBtn_btn_nav_Pressed(object sender, int function)
        {
            // Send Key Down
            App.ClientConnection.Send(Braille.Lan.Commands.sendKeyDown, function);
        }

 
    }
}
