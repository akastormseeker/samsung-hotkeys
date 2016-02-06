using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamsungHotkeys
{
    class HotkeyListener : IDisposable
    {
        public string ModelName { get; private set; }
        public bool SendUnknownKeyEvents { get; set; }
        public event HotkeyEventHandler HotkeyEvent;

        private LowLevelKbHook kbHook;

        public HotkeyListener(string modelName)
        {
            ModelName = modelName;
            SendUnknownKeyEvents = false;

            kbHook = new LowLevelKbHook();
            kbHook.LowLevelKeyEvent += KbHook_LowLevelKeyEvent;
            kbHook.SetHook();
        }

        ~HotkeyListener()
        {
            Dispose();
        }

        private void KbHook_LowLevelKeyEvent(object sender, LowLevelKbHook.LowLevelKeyEventArgs e)
        {
            Hotkey hk = TranslateEventToHotkey(e);
            if (hk == Hotkey.Unknown && !SendUnknownKeyEvents) return;

            if(HotkeyEvent != null)
            {
                HotkeyEventArgs args = new HotkeyEventArgs(hk, ModelName, e);
                HotkeyEvent(this, args);
            }
        }

        private Hotkey TranslateEventToHotkey(LowLevelKbHook.LowLevelKeyEventArgs args)
        {
            if (args.Flags.HasFlag(LowLevelKbHook.KbLLHookFlags.Extended))
            {
                // this is what it is on model 700Z5C
                switch (args.ScanCode)
                {
                    case 2: return Hotkey.DisplaySwitch;
                    case 8: return Hotkey.ScreenBrightnessUp;
                    case 9: return Hotkey.ScreenBrightnessDown;
                    case 22: return Hotkey.KeyboardBacklightUp;
                    case 23: return Hotkey.KeyboardBacklightDown;
                    case 32: return Hotkey.VolumeMute;
                    case 40: return Hotkey.FnLockEnabled;
                    case 41: return Hotkey.FnLockDisabled;
                    case 46: return Hotkey.VolumeDown;
                    case 48: return Hotkey.VolumeUp;
                    case 51: return Hotkey.CoolingMode;
                    case 58: return Hotkey.EjectODD;
                    case 78: return Hotkey.EasySettings;
                    case 85: return Hotkey.ToggleWireless;
                    case 119: return Hotkey.TouchpadEnabled;
                    case 121: return Hotkey.TouchpadDisabled;
                    default: return Hotkey.Unknown;
                }
            }
            return Hotkey.Unknown;
        }

        public void Dispose()
        {
            if(kbHook.IsHooked) kbHook.ClearHook();
        }

        public delegate void HotkeyEventHandler(object sender, HotkeyEventArgs e);

        public class HotkeyEventArgs : EventArgs
        {
            public LowLevelKbHook.LowLevelKeyEventArgs LowLevelEvent { get; private set; }
            public string ModelName { get; private set; }
            public Hotkey Hotkey { get; private set; }
            public bool IsKeyRelease { get; private set; }

            public HotkeyEventArgs(Hotkey hotkey, string modelName, LowLevelKbHook.LowLevelKeyEventArgs llEvent)
            {
                Hotkey = hotkey;
                ModelName = modelName;
                LowLevelEvent = llEvent;
                IsKeyRelease = llEvent.Flags.HasFlag(LowLevelKbHook.KbLLHookFlags.Up);
            }
        }

        
    }

    public enum Hotkey
    {
        Unknown,
        EasySettings,
        ScreenBrightnessDown,
        ScreenBrightnessUp,
        DisplaySwitch,
        TouchpadEnabled,
        TouchpadDisabled,
        VolumeMute,
        VolumeDown,
        VolumeUp,
        KeyboardBacklightDown,
        KeyboardBacklightUp,
        CoolingMode,
        ToggleWireless,
        FnLockEnabled,
        FnLockDisabled,
        EjectODD,
        ToggleNumLock,
        ToggleCapsLock
    }
}
