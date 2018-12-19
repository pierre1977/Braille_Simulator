using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace BrailleDisplay
{
    public class BrailleDot
    {
        // VARS   
        private Grid dotGrid;
        private Rectangle dotOutline;
        private Ellipse dotPoint;

        private int id = 0;                         // Laufende Nummer
        private Point position = new Point(0,0);    // Position im Grid

        private bool isOn = false;                  // wenn On ohne Mouse Over, also Anzeige vom Framework On
        private bool isEntered = false;             // wenn Mouse Over

        // Color
        private SolidColorBrush dotColorOn = new SolidColorBrush(Color.FromArgb(255, 0xc9, 0xc9, 0xc9));
        private SolidColorBrush dotColorOff = new SolidColorBrush(Color.FromArgb(50, 0xc9, 0xc9, 0xc9));
        private SolidColorBrush dotColorOver = new SolidColorBrush(Color.FromArgb(50, 0xce, 0x25, 0x87));

        private SolidColorBrush borderColor = new SolidColorBrush(Color.FromArgb(80, 0x26, 0x26, 0x26)); // Transparent


        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="dotSize">Der Durchmesser eines Punktes in PX</param>
        /// <param name="dotSpace">Der Aussenabstand von Punkten in PX</param>
        /// <param name="id">Laufende Nr des Objektes</param>
        public BrailleDot(int dotSize, int dotSpace, int id )
        {
            // Id setzten
            this.id = id;

            // Eigenschaften setzte, alle auf False
            this.isOn = false;
            this.isEntered = false;
            
            // Punkt erstellen
            this.dotPoint = new Ellipse();

            this.dotPoint.Fill = dotColorOff;
            this.dotPoint.Height = dotSize;
            this.dotPoint.Width = dotSize;


            //// Border erstellen
            this.dotOutline = new Rectangle();
            
            // eventhandler
            this.dotOutline.PointerEntered += DotPoint_PointerEntered;
            this.dotOutline.PointerExited += DotPoint_PointerExited;
            this.dotOutline.PointerMoved += DotOutline_PointerMoved;
            this.dotOutline.PointerReleased += DotOutline_PointerReleased;

            this.dotOutline.PointerCanceled += DotOutline_PointerCanceled;
            this.dotOutline.PointerCaptureLost += DotOutline_PointerCaptureLost;
            this.dotOutline.PointerPressed += DotOutline_PointerPressed;

            // Name setzten für Identifikation bei Pointer Move, Enter, Leave
            this.dotOutline.Name = id.ToString();
            this.dotPoint.Name = id.ToString();

            // Farbe muss gesetzt werden damit Point Event gehen
            this.dotOutline.Fill = borderColor;

            // Debugen zeigt linien an
            //this.dotOutline.Stroke = dotColorOn;
            //this.dotOutline.StrokeThickness = 0.1;

            this.dotOutline.Width = dotSize+(int)(dotSpace);
            this.dotOutline.Height = dotSize+(int)(dotSpace);

            // Grid erstellen nimmt punkt und rechteck auf
            this.dotGrid = new Grid();
            this.dotGrid.Width = dotOutline.Width;
            this.dotGrid.Height = dotOutline.Height;

            dotGrid.Children.Add(dotOutline);
                        
            // Punkt im grid center setzten
            this.dotPoint.Margin = new Thickness { Left = (dotSpace / 2), Top = (dotSpace / 2), Right = (dotSpace / 2), Bottom= (dotSpace / 2)  };            
            dotGrid.Children.Add(dotPoint);
        }

        public void setOn()
        {
            this.dotPoint.Fill = dotColorOn;
            this.isOn = true;
        }

        public void setOff()
        {
            this.dotPoint.Fill = dotColorOff;
            this.isOn = false;
        }


        // Eventhandler 




        private void DotOutline_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("DotOutline_PointerPressed");
        }
        private void DotOutline_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("DotOutline_PointerCanceled");
        }
        private void DotOutline_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("DotOutline_PointerCaptureLost");
        }
        private void DotPoint_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (this.isOn == true)
            {
                this.dotPoint.Fill = dotColorOn;
            }
            else
            {
                this.dotPoint.Fill = dotColorOff;
            }
            this.isEntered = false;

            // auslösen des Externen Eventhandlers
            if (PointerExit != null)
            {
                PointerExit(this, e);
            }
            Debug.WriteLine("DotPoint_PointerExited");
        }
                
        private void DotPoint_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.dotPoint.Fill = dotColorOver;
            this.isEntered = true;

            // auslösen des Externen Eventhandlers
            if (PointerEntered != null)
            {
                PointerEntered(this, e);
            }
            Debug.WriteLine("DotPoint_PointerEntered");
        }

        private void DotOutline_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // auslösen des Externen Eventhandlers
            if (PointerMoved != null)
            {
                PointerMoved(this, e);
            }
            Debug.WriteLine("DotOutline_PointerMoved");
        }

        private void DotOutline_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // auslösen des Externen Eventhandlers
            if (PointerReleased != null)
            {
                PointerReleased(this, e);
            }
            Debug.WriteLine("DotOutline_PointerReleased");
        }

        // Externer Eventhandler
        public delegate void EventDelegate(object sender, PointerRoutedEventArgs e);
        public event EventDelegate PointerEntered;
        public event EventDelegate PointerExit;
        public event EventDelegate PointerMoved;
        public event EventDelegate PointerReleased;


        // LOOK hier:
        // https://www.akadia.com/services/dotnet_delegates_and_events.html





        // Return Braille Dot als XAML Element
        public Grid getXamlDot()
        {
            return this.dotGrid;
        }



        public int getId()
        {
            return this.id;
        }

    }
}
