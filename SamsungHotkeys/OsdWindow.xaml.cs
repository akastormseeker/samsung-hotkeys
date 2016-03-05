using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SamsungHotkeys
{
    /// <summary>
    /// Interaction logic for OsdWindow.xaml
    /// </summary>
    public partial class OsdWindow : Window
    {

        public OsdWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }

        public string OsdText
        {
            get { return TitleText.Text; }
            set { TitleText.Text = value; }
        }

        public string OsdColorText
        {
            get { return ColorText.Text; }
            set { ColorText.Text = value; }
        }

        public Brush OsdColorTextBrush
        {
            get { return ColorText.Foreground; }
            set { ColorText.Foreground = value; }
        }

        public bool OsdColorTextVisible
        {
            get { return ColorText.Visibility == Visibility.Visible; }
            set { ColorText.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool OsdProgressBarVisible
        {
            get { return OsdProgressBar.Visibility == Visibility.Visible; }
            set {
                double height = ActualHeight;
                OsdProgressBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public double OsdProgressBarPercent
        {
            set { OsdProgressBar_Progress.Width = value * OsdProgressBar.ActualWidth; }
        }


    }
}
