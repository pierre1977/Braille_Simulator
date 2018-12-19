using System;
using System.Collections.Generic;
using System.IO;
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

namespace BrailleSingleButton
{
    public sealed partial class Button : UserControl
    {
        int btnId = 0;

        private SolidColorBrush Black = new SolidColorBrush(Color.FromArgb(255, 0x00, 0x00, 0x00));
        private SolidColorBrush Gray = new SolidColorBrush(Color.FromArgb(255, 0x50, 0x50, 0x50));
        private int BorderOver = 5;
        private int BorderNormal = 3;

        private SolidColorBrush fillDefault = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102));
        private SolidColorBrush fillOver = new SolidColorBrush(Color.FromArgb(255, 170, 170, 170));

        public Button()
        {
            this.InitializeComponent();
        }

        public void setBtn_Name(string name)
        {
            btn_name.Text = name;
        }

        public void setBtn_ToolTip(string name)
        {
            ToolTip tooltip = new ToolTip();
            tooltip.Content = name;
            ToolTipService.SetToolTip(this.btn_name, tooltip);
        }

        public void setBtn_Id(int id)
        {
            this.btnId = id;
        }

        // Externer Eventhandler
        public delegate void EventDelegate(object sender, PointerRoutedEventArgs e, int btnId);
        public event EventDelegate btn_Released;
        public event EventDelegate btn_Pressed;


        // Point Over Events, Farbänderung
        private void btn_PointerEntered(object sender)
        {
            Rectangle ell = btn_inn;
            //Rectangle ell = sender as Rectangle;
            ell.Fill = fillOver;
            ell.StrokeThickness = BorderOver;
            ell.Stroke = Gray;
        }

        // Point Exit Events, Farbänderung
        private void btn_PointerPressed(object sender)
        {
            Rectangle ell = btn_inn;
            //Rectangle ell = sender as Rectangle;
            ell.Fill = Black;
            ell.StrokeThickness = BorderOver;
            ell.Stroke = Gray;
        }

        // Point Exit Events, Farbänderung
        private void btn_PointerExited(object sender)
        {
            Rectangle ell = btn_inn;
            // Rectangle ell = sender as Rectangle;
            ell.Fill = fillDefault;
            ell.StrokeThickness = BorderNormal;
            ell.Stroke = Black;
        }





        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            btn_PointerEntered(sender);
        }

        private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            btn_PointerExited(sender);
        }

        private void Rectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            btn_PointerPressed(sender);

            // Event Delegate
            if (btn_Pressed != null)
            {
                btn_Pressed(this, e, this.btnId);
            }
        }

        private void Rectangle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            btn_PointerExited(sender);

            // Event Delegate
            if (btn_Released != null)
            {
                btn_Released(this, e, this.btnId);
            }
        }
    }
}
