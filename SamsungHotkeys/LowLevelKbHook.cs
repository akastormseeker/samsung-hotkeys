using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SamsungHotkeys
{
    class LowLevelKbHook
    {
        private HookProc kbCallbackDelegate = null;
        private IntPtr mHookHandle;

        public event LowLevelKeyEventHandler LowLevelKeyEvent;

        public bool IsHooked { get; private set; }

        public void SetHook()
        {
            this.kbCallbackDelegate = new HookProc(this.MyCallbackFunction);
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    IntPtr hModule = NativeMethods.GetModuleHandle(module.ModuleName);
                    mHookHandle = NativeMethods.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, kbCallbackDelegate, hModule, 0);
                    IsHooked = true;
                }
            }
        }

        public bool ClearHook()
        {
            if(mHookHandle != IntPtr.Zero)
            {
                bool rv = NativeMethods.UnhookWindowsHookEx(mHookHandle);
                mHookHandle = IntPtr.Zero;
                IsHooked = false;
                return rv;
            }
            return false;
        }

        private int MyCallbackFunction(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
            {
                //you need to call CallNextHookEx without further processing
                //and return the value returned by CallNextHookEx
                return NativeMethods.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }

            // we can convert the 2nd parameter (the key code) to a System.Windows.Forms.Keys enum constant
            KBDLLHOOKSTRUCT kbinfo = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            //Console.WriteLine("Key Pressed: code={0}, wParam=0x{1}, lParam=[{2}]", code, wParam.ToString("X2"), kbinfo.ToString());
            OnLowLevelKeyEvent(kbinfo);

            //return the value returned by CallNextHookEx
            return NativeMethods.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);
        private delegate int LowLevelKeyboardProc(int nCode, int wParam, [In] KBDLLHOOKSTRUCT lParam);

        private void OnLowLevelKeyEvent(KBDLLHOOKSTRUCT data)
        {
            if (LowLevelKeyEvent != null)
            {
                LowLevelKeyEvent(this, new LowLevelKeyEventArgs(data));
            }
        }
        public delegate void LowLevelKeyEventHandler(object sender, LowLevelKeyEventArgs e);

        public class LowLevelKeyEventArgs : EventArgs
        {
            public uint VkCode { get; set; }
            public uint ScanCode { get; set; }
            public uint Timestamp { get; set; }
            public KbLLHookFlags Flags { get; set; }
            public uint ExtraInfo { get; set; }

            public LowLevelKeyEventArgs()
            {
                VkCode = 0;
                ScanCode = 0;
                Timestamp = 0;
                Flags = 0;
                ExtraInfo = 0;
            }

            public LowLevelKeyEventArgs(KBDLLHOOKSTRUCT data)
            {
                VkCode = data.vkCode;
                ScanCode = data.scanCode;
                Timestamp = data.time;
                Flags = (KbLLHookFlags)data.flags;
                ExtraInfo = (uint)data.dwExtraInfo;
            }
        }

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;

            public override string ToString()
            {
                return string.Format("vkCode={0}, scanCode={1}, flags=[{2}], time={3}, dwExtraInfo={4}", vkCode, scanCode, flags, time, dwExtraInfo);
            }
        }

        [Flags]
        public enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }

        [Flags]
        public enum KbLLHookFlags : uint
        {
            Extended = 0x01,
            Injected = 0x10,
            AltDown = 0x20,
            Up = 0x80
        }

        private class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll")]
            public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        }
    }
}
