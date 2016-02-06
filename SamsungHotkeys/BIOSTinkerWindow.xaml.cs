using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SamsungHotkeys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BIOSTinkerWindow : Window
    {
        private Controls.SamsungBIOSInterface biosInterface;
        TextBox[] byteBoxen;

        public BIOSTinkerWindow(Controls.SamsungBIOSInterface biosInterface)
        {
            InitializeComponent();

            this.biosInterface = biosInterface;

            byteBoxen = new TextBox[] { byte0, byte1, byte2, byte3 };

            for(int i = 0; i < byteBoxen.Length; i++)
            {
                byteBoxen[i].Text = "0";
            }

            string modelName = biosInterface.BIOSModelName;
            if (modelName != null)
            {
                logTextBox.Text += "Detected laptop model: " + modelName + "\r\n";
            }
            else
            {
                logTextBox.Text += "Failed to retrieve laptop model!";
                return;
            }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int fn;
            byte[] buffer = new byte[4];

            if (!int.TryParse(biosFnTextBox.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out fn))
            {
                biosFnTextBox.SelectAll();
                biosFnTextBox.Focus();
                return;
            }

            if (inputTypeBytes.IsChecked == true)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (byteBoxen[i].Text == null || byteBoxen[i].Text == "") byteBoxen[i].Text = "0";

                    if (!byte.TryParse(byteBoxen[i].Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out buffer[i]))
                    {
                        byteBoxen[i].SelectAll();
                        byteBoxen[i].Focus();
                        return;
                    }
                }
                logTextBox.Text += string.Format("Calling BIOS interface 0x{0} with parameters [{1}, {2}, {3}, {4}]\r\n", fn.ToString("X2"), buffer[0].ToString("X2"), buffer[1].ToString("X2"), buffer[2].ToString("X2"), buffer[3].ToString("X2"));

                int rv = biosInterface.CallBIOSInterface(fn, buffer);
                
                logTextBox.Text += string.Format("Interface returned 0x{0}, parameters [{1}, {2}, {3}, {4}]\r\n", rv.ToString("X2"), buffer[0].ToString("X2"), buffer[1].ToString("X2"), buffer[2].ToString("X2"), buffer[3].ToString("X2"));
                
            }
            else if (inputTypeBuffer.IsChecked == true)
            {
                int buffSize;
                if (!int.TryParse(outputBufferSize.Text, out buffSize))
                {
                    outputBufferSize.SelectAll();
                    outputBufferSize.Focus();
                    return;
                }
                buffer = new byte[buffSize];
                logTextBox.Text += string.Format("Calling BIOS interface 0x{0} with {1}-byte buffer\r\n", fn.ToString("X2"), buffer.Length);
                int rv = biosInterface.CallBIOSInterface(fn, buffer);
                int nullPos = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    nullPos = i;
                    if (buffer[i] == 0)
                    {
                        break;
                    }
                }
                logTextBox.Text += string.Format("Interface returned 0x{0}, buffer = \"{1}\"\r\n", rv.ToString("X2"), Encoding.ASCII.GetString(buffer, 0, nullPos));
                for (int i = 0; i < buffer.Length; i++)
                {
                    logTextBox.Text += buffer[i].ToString("X2") + " ";
                    if (i > 0 && (i % 8 == 0 || i == (buffer.Length - 1)))
                    {
                        logTextBox.Text += "\r\n";
                    }
                }
            }
        }
    }
}
