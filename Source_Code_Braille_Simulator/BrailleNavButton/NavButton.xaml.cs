using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BrailleNavButton
{
    public sealed partial class NavButton : UserControl
    {
        private SolidColorBrush Black = new SolidColorBrush(Color.FromArgb(255, 0x00, 0x00, 0x00));
        private SolidColorBrush Gray = new SolidColorBrush(Color.FromArgb(255, 0x50, 0x50, 0x50));
        private int BorderOver = 10;
        private int BorderNormal = 5;

        private SolidColorBrush fillDefault = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102));
        private SolidColorBrush fillOver = new SolidColorBrush(Color.FromArgb(255, 170, 170, 170));

        private BrailleNavButton bNavBtn = new BrailleNavButton();

        // Liste mit Zuweisungen der Enum Dir und Function
        private Dictionary<BrailleNavButton.direction, int> dictionary_Function = new Dictionary<BrailleNavButton.direction, int>();

        public NavButton()
        {
            this.InitializeComponent();
        }

        // Funktion zum zuweisen der ToolTips
        public void add_Tooltip(BrailleNavButton.direction dir, string tipText)
        {
            //bNavBtn.add_ToolTip(dir, tipText);

            ToolTip tooltip = new ToolTip();
            tooltip.Content = tipText;

            if (dir == BrailleNavButton.direction.left)
            {
                ToolTipService.SetToolTip(this.btn_name_left, tooltip);
            }
            if (dir == BrailleNavButton.direction.right)
            {
                ToolTipService.SetToolTip(this.btn_name_right, tooltip);
            }
            if (dir == BrailleNavButton.direction.top)
            {
                ToolTipService.SetToolTip(this.btn_name_up, tooltip);
            }
            if (dir == BrailleNavButton.direction.bottom)
            {
                ToolTipService.SetToolTip(this.btn_name_down, tooltip);
            }
            if (dir == BrailleNavButton.direction.middel)
            {
                ToolTipService.SetToolTip(this.btn_name_enter, tooltip);
            }
        }
        
        // Funktion zum zuweisen der Funktionen für eine Direction
        public void add_Function(BrailleNavButton.direction dir, int func)
        {
            // Prüfen ob Key Vorhanden ist
            if (!dictionary_Function.ContainsKey(dir))
            {
                dictionary_Function.Add(dir, func);
            }
            else
            {
                // Überschreibt den Key
                dictionary_Function[dir] = func;
            }
        }

        // Funktion zum zuweisen des Namens
        public void add_Name(BrailleNavButton.direction dir, string name)
        {
            if (dir == BrailleNavButton.direction.left)
            {
                this.btn_name_left.Text = name;                
            }
            if (dir == BrailleNavButton.direction.right)
            {
                this.btn_name_right.Text = name;
            }
            if (dir == BrailleNavButton.direction.top)
            {
                this.btn_name_up.Text = name;
            }
            if (dir == BrailleNavButton.direction.bottom)
            {
                this.btn_name_down.Text = name;
            }
            if (dir == BrailleNavButton.direction.middel)
            {
                this.btn_name_enter.Text = name;
            }
        }

        // Farbänderung, Point Over für alle
        private void Grid_Exit(object sender)
        {
            Grid parent = sender as Grid;
            DependencyObject child = VisualTreeHelper.GetChild(parent, 0);

            if (child as Path != null)
            {
                Path ell = child as Path;
                ell.Fill = fillDefault;
                ell.StrokeThickness = BorderNormal;
                ell.Stroke = Black;
            }
            else if (child as Ellipse != null)
            {
                Ellipse ell = child as Ellipse;
                ell.Fill = fillDefault;
                ell.StrokeThickness = BorderNormal;
                ell.Stroke = Black;
            }
        }

        // Farbänderung, Point Over für alle
        private void Grid_Entered(object sender)
        {
            Grid parent = sender as Grid;
            DependencyObject child = VisualTreeHelper.GetChild(parent, 0);

            if (child as Path != null)
            {
                Path ell = child as Path;
                ell.Fill = fillOver;
                ell.StrokeThickness = BorderOver;
                ell.Stroke = Gray;
            }
            else if (child as Ellipse != null)
            {
                Ellipse ell = child as Ellipse;
                ell.Fill = fillOver;
                ell.StrokeThickness = BorderOver;
                ell.Stroke = Gray;
            }
        }
        
        // Externer Eventhandler
        //public delegate void EventDelegate(object sender, PointerRoutedEventArgs e, BrailleNavButton.direction dir);
        public delegate void EventDelegate(object sender, int function);
        public event EventDelegate btn_nav_Released;
        public event EventDelegate btn_nav_Pressed;
        
        // Eventhandler
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Grid_Exit(sender);
        }
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Grid_Entered(sender);
        }         
        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Delegate_Pressed(sender,e);
        }
        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Delegate_Released(sender, e);
        }
        
        // Eventhandler Delegate, Release
        private void Delegate_Released(object sender, PointerRoutedEventArgs e)
        {
            // Farbänderung
            Grid_Exit(sender);

            // Child Element 
            Grid parent = sender as Grid;
            DependencyObject child = VisualTreeHelper.GetChild(parent, 0);
            string name = "";
            if (child as Path != null)
            {
                Path ell = child as Path;
                name = ell.Name;
            }
            else if (child as Ellipse != null)
            {
                Ellipse ell = child as Ellipse;
                name = ell.Name;
            }

            int function = 0;
            switch (name)
            {
                case "btn_left":
                    function = dictionary_Function[BrailleNavButton.direction.left];
                    break;
                case "btn_right":
                    function = dictionary_Function[BrailleNavButton.direction.right];
                    break;
                case "btn_bottom":
                    function = dictionary_Function[BrailleNavButton.direction.bottom];
                    break;
                case "btn_top":
                    function = dictionary_Function[BrailleNavButton.direction.top];
                    break;
                case "btn_middle":
                    function = dictionary_Function[BrailleNavButton.direction.middel];
                    break;
            }
            if (btn_nav_Released != null)
            {
                btn_nav_Released(sender, function);
            }
        }

        // Eventhandler Delegate, Press
        private void Delegate_Pressed(object sender, PointerRoutedEventArgs e)
        {
            // Farbänderung
            Grid_Entered(sender);

            // Child Element 
            Grid parent = sender as Grid;
            DependencyObject child = VisualTreeHelper.GetChild(parent, 0);
            string name = "";
            if (child as Path != null)
            {
                Path ell = child as Path;
                name = ell.Name;
            }
            else if (child as Ellipse != null)
            {
                Ellipse ell = child as Ellipse;
                name = ell.Name;
            }

            int function = 0;
            switch (name)
            {
                case "btn_left":
                    function = dictionary_Function[BrailleNavButton.direction.left];
                    break;
                case "btn_right":
                    function = dictionary_Function[BrailleNavButton.direction.right];
                    break;
                case "btn_bottom":
                    function = dictionary_Function[BrailleNavButton.direction.bottom];
                    break;
                case "btn_top":
                    function = dictionary_Function[BrailleNavButton.direction.top];
                    break;
                case "btn_middle":
                    function = dictionary_Function[BrailleNavButton.direction.middel];
                    break;
            }

            if (btn_nav_Pressed != null)
            {
                btn_nav_Pressed(sender, function);
            }
        }


    }
}
