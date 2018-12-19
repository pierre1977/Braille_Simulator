using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236


// TODO GESTEN erkennung UWP:
// https://github.com/Microsoft/Windows-universal-samples/blob/master/Samples/BasicInput/cs/5-GestureRecognizer.xaml.cs


namespace BrailleDisplay
{
    public sealed partial class Display : UserControl
    {
        // VAR
        private float dpi = 96;             // Display DPI
        private int ScreenSizeX = 0;        // Breite der Anwendung
        private int ScreenSizeY = 0;

        private int brailleAreaX = 0;       // Breite des Braille Device in Pixel
        private int brailleAreaY = 0;       // Breite des Braille Device in Pixel
        private int brailleDotSize = 0;     // Größe des Punktes in Pixel
        private int brailleDotSpace = 0;    // Aussenabstand der Punkte in Pixel
        private int brailleDotCountX = 0;   // Anzahl der Dots für X
        private int brailleDotCountY = 0;   // Anzahl der Dots für Y

        private BrailleDot[] dotObjectList;
        //private List<BrailleDot> dotObjectList = new List<BrailleDot>(8000);            // Object Liste
        private Dictionary<int, int> dotEnteredList = new Dictionary<int, int>();   // Punkte Liste welche aktuell Berührt sind

        // Touch erfassung
        TouchCapabilities touchCapabilities = new TouchCapabilities();      // infos vom system abfragen
        Dictionary<uint, Pointer> contacts;                                 // speichert pointer infos        
        Dictionary<uint, Point> lastAreaPoint;                              // speichert den letzten Punkt auf der Area        

        // Gesture
        GestureRecognizer gr = new GestureRecognizer();


        // Übernimmt das übergeordnete Element -> PAGE
        //public delegate void EventDelegate(object sender, SizeChangedEventArgs e);
        //public event EventDelegate size_changed;

        public delegate void EventTouch(object sender, Dictionary<uint, Pointer> contactList);
        public event EventTouch contactListChanged;

        public delegate void EventTouchPoint(uint finger, bool contact, Point dots, Point pointer);
        public event EventTouchPoint dot_PointerEntered;
        public event EventTouchPoint dot_PointerExit;
        public event EventTouchPoint dot_PointerMoved;
        public event EventTouchPoint dot_PointerReleased;

        public delegate void EventGesture(string gestureType, Point start, Point end);
        public event EventGesture gestureDetected;


        public Display()
        {
            this.InitializeComponent();

            // Set Initial Size Barille Area
            this.canBackArea.Width = 0;
            this.canBackArea.Height = 0;

            // Set Screen Information
            getScreenInformation();

            // Init Dictionary mit anzahl der touchCapabilities.Contacts
            contacts = new Dictionary<uint, Pointer>((int)touchCapabilities.Contacts);

            // Init Dictianry für die letzten Punkte
            lastAreaPoint = new Dictionary<uint, Point>();


            //// Pointer Eventhandler für Finger
            this.canBackArea.PointerEntered += CanBackArea_PointerEntered;
            this.canBackArea.PointerMoved += CanBackArea_PointerMoved;
            this.canBackArea.PointerReleased += CanBackArea_PointerExited;  // gleiche Fkt für PointerReleased und Exit
            this.canBackArea.PointerExited += CanBackArea_PointerExited;

            //this.canBackArea.PointerPressed += CanBackArea_PointerPressed;
            //this.canBackArea.PointerCanceled += CanBackArea_PointerCanceled;


            // Gesture Eventhandler
            gr.GestureSettings = Windows.UI.Input.GestureSettings.ManipulationRotate | Windows.UI.Input.GestureSettings.ManipulationTranslateX | Windows.UI.Input.GestureSettings.ManipulationTranslateY |
Windows.UI.Input.GestureSettings.ManipulationScale | Windows.UI.Input.GestureSettings.ManipulationRotateInertia | Windows.UI.Input.GestureSettings.ManipulationScaleInertia |
Windows.UI.Input.GestureSettings.ManipulationTranslateInertia | Windows.UI.Input.GestureSettings.Tap;
            
            // Gesture Eventhandler - sind in den Finger Eventhandler
            //this.PointerPressed += MainPage_PointerPressed;
            //this.PointerMoved += MainPage_PointerMoved;
            //this.PointerReleased += MainPage_PointerReleased;

            gr.CrossSliding += gr_CrossSliding;
            gr.Dragging += gr_Dragging;
            gr.Holding += gr_Holding;
            gr.RightTapped += gr_RightTapped;
            gr.Tapped += gr_Tapped;

            gr.ManipulationCompleted += gr_ManipulationCompleted;

            gr.ManipulationInertiaStarting += gr_ManipulationInertiaStarting;
            gr.ManipulationStarted += gr_ManipulationStarted;
            gr.ManipulationUpdated += gr_ManipulationUpdated;

        }


        /// <summary>
        /// Erstellt die Braille Area anhand der Parameter
        /// </summary>
        /// <param name="dimentionX">Größe des Brailledisplay in mm für X</param>
        /// <param name="dimentionY">Größe des Brailledisplay in mm für Y</param>
        /// <param name="dotStroke">Punkt Stroke des Brailledisplay in mm</param>
        /// <param name="dotSpace">Punktabstand des Brailledisplay in mm</param>
        /// <returns></returns>
        ///         
        public void initBrailleArea(double dimentionX, double dimentionY, double dotSpace, double dotStroke)
        {
            // in Pixel Konvertieren
            this.brailleAreaX = CentimeterToPixel((dimentionX / 10));
            this.brailleAreaY = CentimeterToPixel((dimentionY / 10));

            this.brailleDotSize = CentimeterToPixel(((dotStroke * 2) / 10));                  // *2 um aussendurchmesser zu erhalten
            this.brailleDotSpace = CentimeterToPixel((dotSpace - (dotStroke * 2)) / 10);     // - Punktdurchmesser (*2 = Punktdruchmesser)

            this.brailleDotCountX = (int)((dimentionX / dotSpace));
            this.brailleDotCountY = (int)((dimentionY / dotSpace));

            // dotObjectList Erstellen
            dotObjectList = new BrailleDot[brailleDotCountX * brailleDotCountY];

            // Area erstellen
            drawBrailleArea();

            //return true;
        }

        /// <summary>
        /// Füllt die Braille Area mit Punkten
        /// </summary>
        private void drawBrailleArea()
        {
            // Vertikal 
            int indexCounter = 1;
            int maxCount = this.brailleDotCountY * this.brailleDotCountX;
            for (int j = 1; j <= this.brailleDotCountY; j++)
            {
                // Width auf Null
                canBackArea.Width = 0;

                // Höhe 
                canBackArea.Height = canBackArea.Height + (this.brailleDotSpace + this.brailleDotSize);

                // Horizontal                
                for (int i = 0; i < this.brailleDotCountX; i++)
                {
                    // Neues Dot Object
                    BrailleDot bd = new BrailleDot(this.brailleDotSize, this.brailleDotSpace, indexCounter);

                    // Event um in die Auswahl Liste zu nehmen
                    //bd.PointerEntered += Bd_PointerEntered;     // DOT wird angewählt also contact and move
                    //bd.PointerExit += Bd_PointerExit;
                    //bd.PointerMoved += Bd_PointerMoved;
                    //bd.PointerReleased += Bd_PointerReleased;   // DOT wird abgewählt
                    
                    // Area Width setzten
                    canBackArea.Width = canBackArea.Width + (this.brailleDotSpace + this.brailleDotSize);

                    // Position des Dots setzten
                    Canvas.SetLeft(bd.getXamlDot(), canBackArea.Width);
                    Canvas.SetTop(bd.getXamlDot(), canBackArea.Height);

                    // Dot als Kind Element einfügen
                    canBackArea.Children.Add(bd.getXamlDot());

                    // in Dot Object Liste einfügen, für späteren zugriff
                    dotObjectList[indexCounter - 1] = bd;                   

                    indexCounter++;                    
                }
            }
            // Abschluss Width setzten
            canBackArea.Width = canBackArea.Width + (this.brailleDotSpace + this.brailleDotSize + this.brailleDotSpace);
            canBackArea.Height = canBackArea.Height + (this.brailleDotSpace + this.brailleDotSize + this.brailleDotSpace);

            // Skalierung
            BrailleScaler.Width = canBackArea.Width;
            BrailleScaler.Height = canBackArea.Width;
            resizerToBrailleDevice(0,0);
        }


        // GETs und SETs
        #region GETs und SETs
        public float getScreenDpi()
        {
            return dpi;
        }

        public double getBrailleScalerX() {
            return this.BrailleScaler.Width;
        }

        public double getBrailleScalerY()
        {
            return this.BrailleScaler.Height;
        }

        public int getDotSizeX() {
            return this.brailleDotCountX;
        }
        public int getDotSizeY()
        {
            return this.brailleDotCountY;
        }

        public Point getBrailleDotCounts()
        {
            return new Point(this.brailleDotCountX, this.brailleDotCountY);
        }

        public void setDotOn(int id)
        {
            dotObjectList[id-1].setOn();
        }
        public void setDotOff(int id)
        {
            dotObjectList[id - 1].setOff();
        }


        public List<int> getEnteredDots()
        {
            List<int> returnList = new List<int>();
            foreach (KeyValuePair<int, int> entry in dotEnteredList)
            {
                returnList.Add(entry.Value);
            }
            return returnList;
        }

        #endregion
        

        // Eventhandler für dotEnteredList
        #region Eventhandler

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/uwp/design/input/handle-pointer-input
        /// Pointer auf der Area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanBackArea_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            MainPage_PointerPressed(sender, e);

            Pointer ptr = e.Pointer;
            e.Handled = true;                                   // Set handel, Punkt wird nicht erneut behandelt            
            this.canBackArea.CapturePointer(e.Pointer);         // Pointer an Display Area setzten

            // Prüfe ob Pointer bereits vorhanden ist, Move Enter oder ...
            if (!contacts.ContainsKey(ptr.PointerId))
            {
                contacts[ptr.PointerId] = ptr;                  // Add contact to dictionary.
            }

            // Nummer des Touchpoints bestimmen, bzw. Fingernummer
            uint ptrTouchNr = (uint)Array.IndexOf(contacts.Keys.ToArray(), ptr.PointerId);
            var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;               // Position auf Display Area
            var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
            Point dotPoint = getDotAtPointer(sender, e);                            // X,Y des DOT Points

            // Prüfen ob Pointer noch in der Area
            if (dotPoint.X != -1 && dotPoint.Y != -1)
            {
                // Letzten Punkt Speichern, für jeden Finger
                lastAreaPoint[ptr.PointerId] = dotPoint;

                // Eventhandler auslösen
                if (dot_PointerEntered != null)
                {
                    dot_PointerEntered(ptrTouchNr, true, dotPoint, new Point((int)dotAreaX, (int)dotAreaY)); // finger ON
                }
                Debug.WriteLine("CanBackArea_PointerEntered - Nr: " + ptrTouchNr + " ID:" + ptr.PointerId + " X/Y: " + dotPoint.X + "/" + dotPoint.Y);
            }
        }

        /// <summary>
        /// Pointer Bewegt sich auf der Area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanBackArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

            MainPage_PointerMoved(sender, e);

            Pointer ptr = e.Pointer;
            e.Handled = true;                                   // Set handel, Punkt wird nicht erneut behandelt            
            this.canBackArea.CapturePointer(e.Pointer);         // Pointer an Display Area setzten

            // Nummer des Touchpoints bestimmen, bzw. Fingernummer
            uint ptrTouchNr = (uint)Array.IndexOf(contacts.Keys.ToArray(), ptr.PointerId);

            // Touch Anzahl prüfen max 10 Finger
            if( (ptrTouchNr < 9 ) && (contacts.Count > 0))
            {
                var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;               // Position auf Display Area
                var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
                Point dotPoint = getDotAtPointer(sender, e);                            // X,Y des DOT Points

                // Prüfen ob Pointer noch in der Area
                if (dotPoint.X != -1 && dotPoint.Y != -1)
                {
                    if ((dotAreaY < canBackArea.ActualHeight) && (dotAreaX < canBackArea.ActualWidth) && (dotAreaX > 0) && (dotAreaY > 0))
                    {
                        // Letzten Punkt Speichern, für jeden Finger
                        lastAreaPoint[ptr.PointerId] = dotPoint;

                        // Eventhandler auslösen
                        if (dot_PointerMoved != null)
                        {
                            dot_PointerMoved(ptrTouchNr, true, dotPoint, new Point((int)dotAreaX, (int)dotAreaY)); // finger ON
                        }
                        Debug.WriteLine("CanBackArea_PointerMoved - Nr: " + ptrTouchNr + " ID:" + ptr.PointerId + " X/Y: " + dotPoint.X + "/" + dotPoint.Y);
                    }
                }
                else
                {
                    // Pointer löschen - ausserhalb der Area, gibt als DotPoint -1,-1 zurück !!!
                    this.canBackArea.ReleasePointerCapture(e.Pointer);  // Pointer vom Display Area entfernen
                    if (contacts.ContainsKey(ptr.PointerId))
                    {
                        contacts[ptr.PointerId] = null;                 // Remove contact to dictionary.
                        contacts.Remove(ptr.PointerId);

                        // Prüfen ob letzter Punkt bekannt
                        if (lastAreaPoint.ContainsKey(ptr.PointerId))
                        {
                            // Eventhandler auslösen, mit dem Letzten bekannten Punkt!                        
                            if (dot_PointerExit != null)
                            {
                                dot_PointerExit(ptrTouchNr, false, lastAreaPoint[ptr.PointerId], new Point((int)dotAreaX, (int)dotAreaY)); // finger OFF
                            }
                            Debug.WriteLine("CanBackArea_PointerMoveExited - Nr: " + ptrTouchNr + " ID:" + ptr.PointerId + " X/Y: " + lastAreaPoint[ptr.PointerId].X + "/" + lastAreaPoint[ptr.PointerId].Y);
                            //Letzten Pointer Löschen
                            lastAreaPoint.Remove(ptr.PointerId);
                        }
                    }
                }
            }
        }
        

        private void CanBackArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("CanBackArea_PointerReleased ");

            Pointer ptr = e.Pointer;
            e.Handled = true;                                   // Set handel, Punkt wird nicht erneut behandelt            
            this.canBackArea.ReleasePointerCapture(e.Pointer);  // Pointer vom Display Area entfernen

            // Nummer des Touchpoints bestimmen, bzw. Fingernummer
            uint ptrTouchNr = (uint)Array.IndexOf(contacts.Keys.ToArray(), ptr.PointerId);

            // Prüfe ob Pointer bereits vorhanden ist, Move Enter oder ...
            if (contacts.ContainsKey(ptr.PointerId))
            {                
                contacts[ptr.PointerId] = null;                 // Remove contact to dictionary.
                contacts.Remove(ptr.PointerId);                
            }

            var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;               // Position auf Display Area
            var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
            Point dotPoint = getDotAtPointer(sender, e);                            // X,Y des DOT Points

            // Touch Anzahl prüfen max 10 Finger
            if (ptrTouchNr < 9)
            {
                // Prüfen ob Pointer noch in der Area
                if (dotPoint.X != -1 && dotPoint.Y != -1)
                {
                    // Eventhandler auslösen
                    if (dot_PointerEntered != null)
                    {
                        dot_PointerEntered(ptrTouchNr, false, dotPoint, new Point((int)dotAreaX, (int)dotAreaY)); // finger OFF
                    }
                    Debug.WriteLine("CanBackArea_PointerReleased - Nr: " + ptrTouchNr + " ID:" + ptr.PointerId + " X/Y: " + dotPoint.X + "/" + dotPoint.Y);
                }
                else
                {
                    e = null;
                }
            }
        }


        /// <summary>
        /// Event für Area (nicht DOT) Pointer Exit
        /// </summary>
        /// <param name="sender">BrailleDot Object</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        private void CanBackArea_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            MainPage_PointerReleased(sender, e);

            Debug.WriteLine("CanBackArea_PointerExited ");

            Pointer ptr = e.Pointer;
            e.Handled = true;                                   // Set handel, Punkt wird nicht erneut behandelt            
            this.canBackArea.ReleasePointerCapture(e.Pointer);  // Pointer vom Display Area entfernen

            // Nummer des Touchpoints bestimmen, bzw. Fingernummer
            uint ptrTouchNr = (uint)Array.IndexOf(contacts.Keys.ToArray(), ptr.PointerId);

            // Prüfe ob Pointer noch vorhanden ist
            if (contacts.ContainsKey(ptr.PointerId))
            {
                var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;               // Position auf Display Area
                var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
                Point dotPoint = getDotAtPointer(sender, e);                            // X,Y des DOT Points

                //// Prüfen ob Pointer noch in der Area
                //if (dotPoint.X != -1 && dotPoint.Y != -1)
                //{
                //    // Eventhandler auslösen
                //    if (dot_PointerExit != null)
                //    {
                //        dot_PointerExit(ptrTouchNr, true, dotPoint, new Point((int)dotAreaX, (int)dotAreaY)); // finger ON
                //    }
                //    Debug.WriteLine("CanBackArea_PointerExited - Nr: " + ptrTouchNr + " ID:" + ptr.PointerId + " X/Y: " + dotPoint.X + "/" + dotPoint.Y);
                //}

                // Prüfe ob Last Point vorhanden ist
                if (lastAreaPoint.ContainsKey(ptr.PointerId))
                {
                    // Eventhandler auslösen
                    if (dot_PointerExit != null)
                    {
                        dot_PointerExit(ptrTouchNr, false, lastAreaPoint[ptr.PointerId], new Point((int)dotAreaX, (int)dotAreaY)); // finger OFF
                    }
                    Debug.WriteLine("CanBackArea_PointerExited - Nr: " + ptrTouchNr + " ID:" + ptr.PointerId + " X/Y: " + lastAreaPoint[ptr.PointerId].X + "/" + lastAreaPoint[ptr.PointerId].Y);

                    // Remove last Point
                    lastAreaPoint.Remove(ptr.PointerId);
                }
                // Remove contact to dictionary.
                contacts[ptr.PointerId] = null;                 
                contacts.Remove(ptr.PointerId);                
            }
        }
        

        /// <summary>
        /// NICHT MEHR NOTWENDIG
        /// https://docs.microsoft.com/en-us/windows/uwp/design/input/handle-pointer-input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void CanBackArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Pointer ptr = e.Pointer;

            // Prevent most handlers along the event route from handling the same event again.
            e.Handled = true;

            // Lock the pointer to the target.
            this.canBackArea.CapturePointer(e.Pointer);

            // Check if pointer already exists (for example, enter occurred prior to press).
            if (!contacts.ContainsKey(ptr.PointerId))
            {
                // Add contact to dictionary.
                contacts[ptr.PointerId] = ptr;                
            }
        }




        #endregion


        // Eventhandler für Gesture
        #region gesture

        void MainPage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var ps = e.GetIntermediatePoints(null);
            if (ps != null && ps.Count > 0)
            {
                gr.ProcessUpEvent(ps[0]);
                e.Handled = true;
                gr.CompleteGesture();
            }
        }

        void MainPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            gr.ProcessMoveEvents(e.GetIntermediatePoints(null));
            e.Handled = true;
        }

        void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var ps = e.GetIntermediatePoints(null);
            if (ps != null && ps.Count > 0)
            {
                try
                {
                    gr.ProcessDownEvent(ps[0]);
                    e.Handled = true;
                }
                catch (Exception ex) { }
            }
        }

        void gr_Tapped(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.TappedEventArgs args)
        {
            Debug.WriteLine("gr_Tapped");
        }
        void gr_RightTapped(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.RightTappedEventArgs args)
        {
            Debug.WriteLine("gr_RightTapped");
        }
        void gr_Holding(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.HoldingEventArgs args)
        {
            Debug.WriteLine("gr_Holding");
            switch (args.HoldingState)
            {
                case HoldingState.Canceled:                    
                    Debug.WriteLine("gr_Holding Cancelled");
                    break;
                case HoldingState.Completed:                    
                    Debug.WriteLine("gr_Holding Completed");
                    break;
                case HoldingState.Started:                    
                    Debug.WriteLine("gr_Holding Started");
                    break;
            }

        }
        void gr_Dragging(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.DraggingEventArgs args)
        {
            Debug.WriteLine("gr_Dragging");
        }
        void gr_CrossSliding(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.CrossSlidingEventArgs args)
        {
            Debug.WriteLine("gr_CrossSliding");
        }
        void gr_ManipulationUpdated(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.ManipulationUpdatedEventArgs args)
        {
            Debug.WriteLine("gr_ManipulationUpdated");
            
            ManipulationDelta m = args.Cumulative;
            if (m.Expansion > 0)
            {
                Debug.WriteLine("Zoom gesture detected");
                // Eventhandler auslösen
                if (gestureDetected != null)
                {
                    gestureDetected("Zoom", args.Position, args.Position);
                }
            }
            else if (m.Expansion < 0)
            {
                Debug.WriteLine("Pinch gesture detected");
                // Eventhandler auslösen
                if (gestureDetected != null)
                {
                    gestureDetected("Pinch", args.Position, args.Position);
                }
            }

            if (m.Rotation != 0.0)
            {
                Debug.WriteLine("Rotation detected");
                // Eventhandler auslösen
                if (gestureDetected != null)
                {
                    gestureDetected("Rotation", args.Position, args.Position);
                }
            }



        }
        void gr_ManipulationStarted(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.ManipulationStartedEventArgs args)
        {
            Debug.WriteLine("gr_ManipulationStarted");


        }
        void gr_ManipulationCompleted(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.ManipulationCompletedEventArgs args)
        {
            Debug.WriteLine("gr_ManipulationCompleted");


            ManipulationDelta m = args.Cumulative;
            if (m.Expansion > 0)
            {
                Debug.WriteLine("Zoom gesture detected");
            }
            else if (m.Expansion < 0)
            {
                Debug.WriteLine("Pinch gesture detected");
            }

            if (m.Rotation != 0.0)
            {
                Debug.WriteLine("Rotation detected");
            }


        }
        void gr_ManipulationInertiaStarting(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.ManipulationInertiaStartingEventArgs args)
        {
            Debug.WriteLine("gr_ManipulationInertiaStarting");

            //this.previousTransform.Matrix = this.cumulativeTransform.Value;

            //this.deltaTransform.TranslateX = this.deltaTransform.TranslateX;

        }






        #endregion


        //
        // Help Functions
        //
        #region Help Funktion

        private int CentimeterToPixel(double Centimeter)
        {
            double pixel = -1;
            pixel = Centimeter * this.dpi / 2.54d;
            return (int)pixel;
        }

        // GET Screen Information
        public void getScreenInformation()
        {
            DisplayInformation dI = DisplayInformation.GetForCurrentView();
            this.dpi = dI.LogicalDpi;

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = dI.RawPixelsPerViewPixel;
            this.ScreenSizeX = (int)(bounds.Width * scaleFactor);
            this.ScreenSizeY = (int)(bounds.Height * scaleFactor);
        }

        public void setBrailleScalerX(double x)
        {
            BrailleScaler.Width = x;
        }

        public void setBrailleScalerY(double y)
        {
            BrailleScaler.Height = y;
        }

        // Resize to Max Orignal Braille Size oder verkleinert auf Display Size
        public void resizerToBrailleDevice(int offsetX, int offsetY)
        {
            // Original Größe oder Verkleinern
            if ( (this.ScreenSizeX > (this.brailleAreaX + offsetX)) && (this.ScreenSizeY > (this.brailleAreaY + offsetY) ) )
            {
                BrailleScaler.Width = this.brailleAreaX;
                BrailleScaler.Height = this.brailleAreaY;
            }
            else if( this.ScreenSizeX < this.brailleAreaX ) //&& this.ScreenSizeY > this.brailleAreaY)
            {
                // verkleinern X
                BrailleScaler.Width = (this.ScreenSizeX - this.brailleDotSpace) - offsetX;
                BrailleScaler.Height = Double.NaN; // auto
            }
            else if (this.ScreenSizeX > this.brailleAreaX ) //&& this.ScreenSizeY < this.brailleAreaY)
            {
                // verkleinern Y
                BrailleScaler.Height = (this.ScreenSizeY - this.brailleDotSpace)- offsetY;
                BrailleScaler.Width = Double.NaN; // auto
            }
        }

        // Event wenn Size Resized, in XAML deklariert
        // übernimmt das übergeorndete Element -> PAGE
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ScreenSizeX = (int)e.NewSize.Width;
            this.ScreenSizeY = (int)e.NewSize.Height;
            resizerToBrailleDevice(0,0);

            //if (size_changed != null)
            //{
            //    size_changed(this, e);
            //}

        }

        // Position to DotId
        public int getDotIdFromXY(int x, int y)
        {
            y = (y - 1) * this.brailleDotCountX;
            return y + x;
        }

        public Point getDotPosition(int id)
        {            
            int x, y;
            id = id - 1;            

            // https://www.codeproject.com/Articles/161465/Arrays-Basics-in-CSharpDotNetTech
            // https://softwareengineering.stackexchange.com/questions/212808/treating-a-1d-data-structure-as-2d-grid

            x = id % this.brailleDotCountX;                   // % is the "modulo operator", the remainder of i / width;           
            y = (int)Math.Floor( (double)(id / this.brailleDotCountX) );    // where "/" is an integer division

            return new Point(x, y);
        }


        /// <summary>
        /// Liefert Dot Point an der Pointer Position wenn Sender: Rectangle oder Ellipse
        /// </summary>
        /// <param name="sender">Object Rectangle oder Ellipse</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        /// <returns>Point</returns>
        public Point getDotAtPointer(object sender, PointerRoutedEventArgs e)
        {
            Point retPoint = new Point(-1,-1);
            // ID des Dots im Rectangle im Namen
            int dotID = -1;
            try
            {
                if (e.OriginalSource.ToString() == "Windows.UI.Xaml.Shapes.Rectangle")
                {
                    Rectangle current = e.OriginalSource as Rectangle;
                    Int32.TryParse(current.Name, out dotID);
                    retPoint = getDotPosition(dotID);
                }
                else if (e.OriginalSource.ToString() == "Windows.UI.Xaml.Shapes.Ellipse")
                {
                    Ellipse current = e.OriginalSource as Ellipse;
                    Int32.TryParse(current.Name, out dotID);
                    retPoint = getDotPosition(dotID);
                }
            }
            catch (Exception ex) {
            }
            //Debug.WriteLine("CanBackArea_PointerMoved: " + dotID);
            //Debug.WriteLine("Other Souce " + e.OriginalSource.ToString());
            return retPoint;
        }


        // nicht mehr verwendet! einfacher mit ->  Rectangle current = e.OriginalSource as Rectangle;
        // Sucht alle Elemente an der Pointer Position und gibt anhand des Names eines Rectangle die DOT ID bzw den Punkt zurück
        /*
        public Point getDotIdOnMouseOver(object sender, PointerRoutedEventArgs e)
        {
            Point retPoint = new Point(0, 0);

            // Position auf Canvas
            var p = e.GetCurrentPoint(canBackArea);

            // Parent Objekt 
            Canvas parent = sender as Canvas;
            GeneralTransform gt = parent.TransformToVisual(null);
            Point pagePoint = gt.TransformPoint(new Point(p.Position.X, p.Position.Y));

            // Liste aller Elemente unter der Position
            IEnumerable<UIElement> elements = VisualTreeHelper.FindElementsInHostCoordinates(pagePoint, this.canBackArea).ToList();
            IEnumerable<Rectangle> rectangleList = elements.OfType<Rectangle>();
            string rectangleName = rectangleList.ElementAt(0).Name;

            int dotID = -1;
            Int32.TryParse(rectangleName, out dotID);

            retPoint = getDotPosition(dotID);

            return retPoint;
        }
        */

        #endregion
        

        // NOT USED - geht nur mit Mouse und Stift aber nicht mit Touch
        #region BD-Pointer Events
        /*
        

        /// <summary>
        /// NOT USED Event für Dot Pointer Exit
        /// </summary>
        /// <param name="sender">BrailleDot Object</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        private void Bd_PointerExit(object sender, PointerRoutedEventArgs e)
        {
            BrailleDot bd = sender as BrailleDot;
            int id = bd.getId();

            Pointer ptr = e.Pointer;
            uint ptrNumber = (uint)contacts.Values.ToList().IndexOf(ptr);

            // Remove contact from dictionary.
            if (contacts.ContainsKey(ptr.PointerId))
            {
                contacts[ptr.PointerId] = null;
                contacts.Remove(ptr.PointerId);
                --numActiveContacts;
            }

            // Release the pointer from the target.
            this.canBackArea.ReleasePointerCapture(e.Pointer);
            // e.Handled = true;

            var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;
            var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
            // Auslösen des Externen Delegate Eventhandlers
            if (dot_PointerExit != null)
            {
                dot_PointerExit(ptrNumber, false, getDotPosition(id), new Point((int)dotAreaX, (int)dotAreaY));
            }
            Debug.WriteLine("Bd_PointerExit - Nr: " + ptrNumber + " ID:" + ptr.PointerId + " X/Y: " + getDotPosition(id).X + "/" + getDotPosition(id).Y);
        }

        /// <summary>
        /// Event für Dot Pointer Release
        /// </summary>
        /// <param name="sender">BrailleDot Object</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        private void Bd_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            BrailleDot bd = sender as BrailleDot;
            int id = bd.getId();
            Pointer ptr = e.Pointer;
            uint ptrNumber = (uint)contacts.Values.ToList().IndexOf(ptr);

            // Remove contact from dictionary.
            if (contacts.ContainsKey(ptr.PointerId))
            {
                contacts[ptr.PointerId] = null;
                contacts.Remove(ptr.PointerId);
                --numActiveContacts;
            }

            // Release the pointer from the target.
            this.canBackArea.ReleasePointerCapture(e.Pointer);
            //            e.Handled = true;

            var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;
            var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
            // Auslösen des Externen Delegate Eventhandlers
            if (dot_PointerReleased != null)
            {
                dot_PointerReleased(ptrNumber, false, getDotPosition(id), new Point((int)dotAreaX, (int)dotAreaY));
            }
            Debug.WriteLine("Bd_PointerReleased - ID: " + ptrNumber + " X/Y: " + getDotPosition(id).X + "/" + getDotPosition(id).Y);
        }

        /// <summary>
        /// Event für Dot Pointer Entered
        /// </summary>
        /// <param name="sender">BrailleDot Object</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        private void Bd_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BrailleDot bd = sender as BrailleDot;
            int id = bd.getId();
            Pointer ptr = e.Pointer;

            // Set handel, Punkt wird nicht erneut behandelt
            //e.Handled = true;

            // Lock the pointer to the target. // TODO CHECK area oder nur dot !!
            this.canBackArea.CapturePointer(e.Pointer);

            // Prüfen ob Pointer bereits vorhanden ist, Move Enter oder ...
            if (!contacts.ContainsKey(ptr.PointerId))
            {
                // Add contact to dictionary.
                contacts[ptr.PointerId] = ptr;
                ++numActiveContacts;
            }

            uint ptrNumber = (uint)contacts.Values.ToList().IndexOf(ptr);
            var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;
            var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
            // auslösen des Externen Eventhandlers
            if (dot_PointerEntered != null)
            {
                dot_PointerEntered(ptrNumber, true, getDotPosition(id), new Point((int)dotAreaX, (int)dotAreaY)); // finger ON
            }
            Debug.WriteLine("Bd_PointerEntered - Nr: " + ptrNumber + " ID:" + ptr.PointerId + " X/Y: " + getDotPosition(id).X + "/" + getDotPosition(id).Y);
        }

        /// <summary>
        /// Event für Dot Pointer Moved
        /// </summary>
        /// <param name="sender">BrailleDot Object</param>
        /// <param name="e">PointerRoutedEventArgs</param>
        private void Bd_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            BrailleDot bd = sender as BrailleDot;
            int id = bd.getId();

            Pointer ptr = e.Pointer;

            // Set handel, Punkt wird nicht erneut behandelt
            // e.Handled = true;

            // Lock the pointer to the target. // TODO CHECK area oder nur dot !!
            this.canBackArea.CapturePointer(e.Pointer);

            // Prüfen ob Pointer bereits vorhanden ist, Move Enter oder ...
            if (!contacts.ContainsKey(ptr.PointerId))
            {
                // Add contact to dictionary.
                contacts[ptr.PointerId] = ptr;
                ++numActiveContacts;
            }

            uint ptrNumber = (uint)contacts.Values.ToList().IndexOf(ptr);
            var dotAreaX = e.GetCurrentPoint(canBackArea).Position.X;
            var dotAreaY = e.GetCurrentPoint(canBackArea).Position.Y;
            // auslösen des Externen Eventhandlers
            if (dot_PointerMoved != null)
            {
                dot_PointerMoved(ptrNumber, true, getDotPosition(id), new Point((int)dotAreaX, (int)dotAreaY)); // finger ON
            }
            Debug.WriteLine("Bd_PointerMoved - Nr: " + ptrNumber + " ID:" + ptr.PointerId + " X/Y: " + getDotPosition(id).X + "/" + getDotPosition(id).Y);
        }
        */
        #endregion
                
    }
}
