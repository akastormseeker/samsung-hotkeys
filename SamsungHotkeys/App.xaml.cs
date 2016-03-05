using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SamsungHotkeys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static readonly Guid APP_GUID = new Guid("{C732A020-C10D-4350-8C41-4D3E3B946638}");

        HotkeyManager hkMgr;
        OsdWindow osdWindow = new OsdWindow();
        
        SettingsWindow settingsWindow = null;

        private readonly Brush redTextBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        private readonly Brush greenTextBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        private readonly Brush lightTextBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0xDC, 0xDC));

        private Mutex appMutex;
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            bool isNewInstance = false;
            appMutex = new Mutex(true, "SamsungHotkeys", out isNewInstance);
            if(!isNewInstance)
            {
                Debug.WriteLine("Another instance of SamsungHotkeys is already running!");
                Application.Current.Shutdown();
                return;
            }

            try
            {
                hkMgr = new HotkeyManager(Dispatcher);
            }
            catch(Controls.InterfaceNotInitializedException ex)
            {
                MessageBox.Show("Unable to initialize the Samsung BIOS interface. It would seem that SABI.DLL is not properly installed.", "Samsung Hotkeys", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            
            hkMgr.ShowOSDEvent += HkMgr_ShowOSDEvent;
            osdWindow.ShowActivated = false;
            osdWindow.Topmost = true;
            osdWindow.ShowInTaskbar = false;
            osdWindow.Opacity = 0;

            osdWindow.SizeChanged += OsdWindow_SizeChanged;

            osdWindow.Show();

            
        }

        private void OsdWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("OSD window size changed from " + e.PreviousSize.ToString() + " to " + e.NewSize.ToString());

            double bottom = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom;
            double right = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right;

            osdWindow.Top = bottom - e.NewSize.Height;
            osdWindow.Left = right - e.NewSize.Width;
        }

        private void HkMgr_ShowOSDEvent(object sender, HotkeyManager.ShowOSDEventArgs e)
        {
            Debug.WriteLine("ShowOSDEvent: " + e.HotkeyEvent + "; HasLevel=" + e.HasLevel + "; Level=" + e.Level);

            switch(e.HotkeyEvent)
            {
                case Hotkey.EasySettings:
                    ShowSettingsWindow();
                    break;

                case Hotkey.ScreenBrightnessDown:
                case Hotkey.ScreenBrightnessUp:
                    ShowOSD("Display Brightness", e.Level / 100.0);
                    break;

                case Hotkey.DisplaySwitch:
                    if (SamsungHotkeys.Properties.Settings.Default.ShowUnusedOSDNotifications)
                    {
                        ShowOSD("Display Switch");
                    }
                    break;

                case Hotkey.TouchpadDisabled:
                    ShowOSD("Touchpad", "OFF", redTextBrush);
                    break;

                case Hotkey.TouchpadEnabled:
                    ShowOSD("Touchpad", "ON", greenTextBrush);
                    break;

                case Hotkey.KeyboardBacklightDown:
                case Hotkey.KeyboardBacklightUp:
                    ShowOSD("Keyboard Backlight", e.Level / 8.0);
                    break;

                case Hotkey.CoolingMode:
                    ShowOSD("Power Plan: ", (string)e.Parameter, lightTextBrush);
                    break;

                case Hotkey.ToggleWireless:
                    ShowOSD("WiFi / BT / WWAN", e.Level == 0 ? "OFF" : "ON", e.Level == 0 ? redTextBrush : greenTextBrush);
                    break;

                case Hotkey.FnLockDisabled:
                    ShowOSD("Fn-Lock", "OFF", redTextBrush);
                    break;

                case Hotkey.FnLockEnabled:
                    ShowOSD("Fn-Lock", "ON", greenTextBrush);
                    break;

                case Hotkey.EjectODD:
                    ShowOSD("Eject ODD");
                    break;
            }
        }

        private void ShowSettingsWindow()
        {
            if (settingsWindow == null)
            {
                settingsWindow = new SettingsWindow(hkMgr.BIOSInterface);
                settingsWindow.Closed += SettingsWindow_Closed;
                settingsWindow.Show();
            }
            settingsWindow.Activate();
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            settingsWindow = null;
        }

        private void ShowOSD(string text) { ShowOSD(text, null, null, false, 0); }
        private void ShowOSD(string text, double progressBarValue) { ShowOSD(text, null, null, true, progressBarValue); }
        private void ShowOSD(string text, string colorText, Brush colorTextBrush) { ShowOSD(text, colorText, colorTextBrush, false, 0); }
        private Storyboard currentOsdStoryboard = null;
        private void ShowOSD(string text, string colorText, Brush colorTextBrush, bool showProgressBar, double progressBarValue)
        {
            osdWindow.OsdText = text;

            osdWindow.Topmost = true;

            osdWindow.OsdColorTextVisible = (colorText != null);
            osdWindow.OsdColorText = colorText;
            osdWindow.OsdColorTextBrush = colorTextBrush;

            osdWindow.OsdProgressBarVisible = showProgressBar;
            osdWindow.OsdProgressBarPercent = Math.Max(0, Math.Min(1, progressBarValue));

            if(currentOsdStoryboard != null)
            {
                currentOsdStoryboard.Stop();
            }

            currentOsdStoryboard = new Storyboard();

            DoubleAnimation aniIn = new DoubleAnimation(1.0, new Duration(TimeSpan.FromMilliseconds(70)));
            aniIn.RepeatBehavior = new RepeatBehavior(1.0);
            aniIn.DecelerationRatio = 0.2;
            Storyboard.SetTarget(aniIn, osdWindow);
            Storyboard.SetTargetProperty(aniIn, new PropertyPath(Window.OpacityProperty));
            aniIn.BeginTime = TimeSpan.FromMilliseconds(0);
            currentOsdStoryboard.Children.Add(aniIn);

            DoubleAnimation aniOut = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(1000)));
            aniOut.RepeatBehavior = new RepeatBehavior(1.0);
            aniOut.AccelerationRatio = 0.2;
            Storyboard.SetTarget(aniOut, osdWindow);
            Storyboard.SetTargetProperty(aniOut, new PropertyPath(Window.OpacityProperty));
            aniOut.BeginTime = TimeSpan.FromMilliseconds(1000);
            currentOsdStoryboard.Children.Add(aniOut);
            currentOsdStoryboard.Completed += (sender, e) =>
            {
                currentOsdStoryboard = null;
            };

            currentOsdStoryboard.Begin();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            
        }
    }
}
