using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SamsungHotkeys
{
    class HotkeyManager
    {
        private Controls.SamsungBIOSInterface biosIface;
        private HotkeyListener hkListener;
        private Dispatcher mDispatcher;

        public event ShowOSDEventHandler ShowOSDEvent;

        public Controls.SamsungBIOSInterface BIOSInterface
        {
            get { return biosIface; }
        }

        public HotkeyManager(Dispatcher dispatcher)
        {
            mDispatcher = dispatcher;
            biosIface = new Controls.SamsungBIOSInterface();
            hkListener = new HotkeyListener(biosIface.BIOSModelName);
            hkListener.SendUnknownKeyEvents = true;
            hkListener.HotkeyEvent += HkListener_HotkeyEvent;
        }

        private void HkListener_HotkeyEvent(object sender, HotkeyListener.HotkeyEventArgs e)
        {
            switch(e.Hotkey)
            {
                case Hotkey.EasySettings: ShowOSD(Hotkey.EasySettings); break;
                case Hotkey.ScreenBrightnessDown: ScreenBrightnessDown(); break;
                case Hotkey.ScreenBrightnessUp: ScreenBrightnessUp(); break;
                case Hotkey.DisplaySwitch: ShowOSD(Hotkey.DisplaySwitch); break;
                case Hotkey.TouchpadDisabled: ShowOSD(Hotkey.TouchpadDisabled); break;
                case Hotkey.TouchpadEnabled: ShowOSD(Hotkey.TouchpadEnabled); break;
                case Hotkey.VolumeMute:
                case Hotkey.VolumeDown:
                case Hotkey.VolumeUp: ShowVolumeOSD(e.Hotkey, e.IsKeyRelease); break;
                case Hotkey.KeyboardBacklightDown: KbBacklightDown(); break;
                case Hotkey.KeyboardBacklightUp: KbBacklightUp(); break;
                case Hotkey.CoolingMode: ShowOSD(Hotkey.CoolingMode); break;
                case Hotkey.ToggleWireless: ToggleWireless(); break;
                case Hotkey.FnLockDisabled: ShowOSD(Hotkey.FnLockDisabled); break;
                case Hotkey.FnLockEnabled: ShowOSD(Hotkey.FnLockEnabled); break;
                case Hotkey.EjectODD: if (!e.IsKeyRelease) EjectODD(); break;

                default:
                    Debug.WriteLine(e.Hotkey + " Hotkey Event: vkcode=" + e.LowLevelEvent.VkCode + "; scancode=" + e.LowLevelEvent.ScanCode + "; flags=" + e.LowLevelEvent.Flags + "; extra=" + e.LowLevelEvent.ExtraInfo + "; timestamp=" + e.LowLevelEvent.Timestamp);
                    ShowOSD(Hotkey.Unknown, (int)e.LowLevelEvent.ScanCode);
                    break;
            }
        }


        int mLastEjectTicks = 0;
        private void EjectODD()
        {
            // delay for about 2 seconds - to keep from queueing up multiple eject commands if key is presses repeatedly or held down
            if(Environment.TickCount > (mLastEjectTicks + 2000))
            {
                mLastEjectTicks = Environment.TickCount;
                ShowOSD(Hotkey.EjectODD);
                new Thread(() =>
                {
                    Controls.ODDrive.Eject();
                }).Start();
            }
        }

        private void KbBacklightDown()
        {
            try
            {
                int brightness = biosIface.GetKbBacklightBrightness();
                if (brightness > 0)
                {
                    brightness--;
                }
                biosIface.SetKbBacklightBrightness(brightness);
                ShowOSD(Hotkey.KeyboardBacklightDown, brightness);
            }
            catch (Controls.SamsungBIOSException ex)
            {
                Debug.WriteLine("SamsungBIOSException getting/setting keyboard backlight brightness: " + ex.ToString());
            }
        }

        private void KbBacklightUp()
        {
            try
            {
                int brightness = biosIface.GetKbBacklightBrightness();
                if (brightness < 8)
                {
                    brightness++;
                }
                biosIface.SetKbBacklightBrightness(brightness);
                ShowOSD(Hotkey.KeyboardBacklightUp, brightness);
            }
            catch(Controls.SamsungBIOSException ex)
            {
                Debug.WriteLine("SamsungBIOSException getting/setting keyboard backlight brightness: " + ex.ToString());
            }
        }

        private void ScreenBrightnessDown()
        {
            mDispatcher.BeginInvoke(new Action(() =>
            {
                byte[] levels;
                int brightness = Controls.ScreenBrightness.GetScreenBrightness(out levels);
                int notchSize = 95 / 8;

                int newBrightness = Math.Max(5, brightness - notchSize);
                Controls.ScreenBrightness.SetScreenBrightness(newBrightness);

                brightness = Controls.ScreenBrightness.GetScreenBrightness(out levels);

                ShowOSD(Hotkey.ScreenBrightnessDown, brightness);
            }));
        }

        private void ScreenBrightnessUp()
        {
            mDispatcher.BeginInvoke(new Action(() =>
            {
                byte[] levels;
                int brightness = Controls.ScreenBrightness.GetScreenBrightness(out levels);
                int notchSize = 95 / 8;

                int newBrightness = Math.Min(100, brightness + notchSize);
                Controls.ScreenBrightness.SetScreenBrightness(newBrightness);

                brightness = Controls.ScreenBrightness.GetScreenBrightness(out levels);

                ShowOSD(Hotkey.ScreenBrightnessUp, brightness);
                //ShowProgressOSDWindow("Brightness", brightness);
            }));
        }

        private void ToggleWireless()
        {
            byte[] status;
            if (biosIface.GetWirelessStatus(out status))
            {
                bool isOn = (status[0] == 1 || status[1] == 1 || status[2] == 1);
                if (isOn)
                {
                    if (status[0] == 1) status[0] = 0;
                    if (status[1] == 1) status[1] = 0;
                    if (status[2] == 1) status[2] = 0;
                }
                else
                {
                    if (status[0] == 0) status[0] = 1;
                    if (status[1] == 0) status[1] = 1;
                    if (status[2] == 0) status[2] = 1;
                }
                int level = (status[0] == 1 ? 1 : 0) | (status[1] == 1 ? 2 : 0) | (status[2] == 1 ? 4 : 0);
                biosIface.SetWirelessStatus(status);
                ShowOSD(Hotkey.ToggleWireless, level);
            }
        }

        private void ShowVolumeOSD(Hotkey key, bool isKeyRelease)
        {
            int volume = Controls.WindowsAudio.GetVolume();
            bool muted = Controls.WindowsAudio.IsMuted();
            biosIface.SetVolumeMuteLight(muted);
            ShowOSD(key, muted ? -1 : volume);
        }

        private void ShowOSD(Hotkey hotkey)
        {
            if(ShowOSDEvent != null)
            {
                ShowOSDEvent(this, new ShowOSDEventArgs(hotkey));
            }
        }

        private void ShowOSD(Hotkey hotkey, int level)
        {
            if (ShowOSDEvent != null)
            {
                ShowOSDEvent(this, new ShowOSDEventArgs(hotkey, level));
            }
        }

        public delegate void ShowOSDEventHandler(object sender, ShowOSDEventArgs e);
        public class ShowOSDEventArgs : EventArgs
        {
            public Hotkey HotkeyEvent { get; private set; }
            public int Level { get; private set; }
            public bool HasLevel { get; private set; }

            public ShowOSDEventArgs(Hotkey hotkey)
            {
                HotkeyEvent = hotkey;
                Level = -1;
                HasLevel = false;
            }

            public ShowOSDEventArgs(Hotkey hotkey, int level)
            {
                HotkeyEvent = hotkey;
                Level = level;
                HasLevel = true;
            }
        }
    }
}
