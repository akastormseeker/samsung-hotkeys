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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SamsungHotkeys
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Controls.SamsungBIOSInterface biosInterface;
        BIOSTinkerWindow tinkerWindow = null;
        private bool isInitializing = false;
        private bool confirmedTinkerDisclaimer = false;

        public SettingsWindow(Controls.SamsungBIOSInterface biosInterface)
        {
            isInitializing = true;
            InitializeComponent();
            this.biosInterface = biosInterface;

            systemModelLabel.Content = biosInterface.BIOSModelName;
            chkBatteryLifeExtenderEnabled.IsChecked = biosInterface.GetBLEEnabled();
            chkUsbChargingEnabled.IsChecked = biosInterface.GetUSBChargingEnabled();

            isInitializing = false;
        }

        private void OpenTinkererButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTinkerWindow();
        }

        private void chkBatteryLifeExtenderEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;
            biosInterface.SetBLEEnabled(true);
        }

        private void ShowTinkerWindow()
        {
            if (!confirmedTinkerDisclaimer)
            {
                if (MessageBox.Show("Warning: This may have unintended side-effects and may cause harm to your computer, your family, and/or your mental health. PROCEED AT YOUR OWN RISK!", "Samsung Hotkeys", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
                confirmedTinkerDisclaimer = true;
            }

            if (tinkerWindow == null)
            {
                tinkerWindow = new BIOSTinkerWindow(biosInterface);
                tinkerWindow.Closed += TinkerWindow_Closed;
                tinkerWindow.Show();
            }
            tinkerWindow.Activate();
        }

        private void TinkerWindow_Closed(object sender, EventArgs e)
        {
            tinkerWindow = null;
        }

        private void chkBatteryLifeExtenderEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;
            biosInterface.SetBLEEnabled(false);
        }

        private void chkUsbChargingEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;
            biosInterface.SetUSBChargingEnabled(true);
        }

        private void chkUsbChargingEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;
            biosInterface.SetUSBChargingEnabled(false);
        }
    }
}
