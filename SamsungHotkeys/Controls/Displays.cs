using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SamsungHotkeys.Controls
{
    class Displays
    {
        public static void GetDisplayDevices()
        {
            NativeMethods.DISPLAY_DEVICE dd = new NativeMethods.DISPLAY_DEVICE();
            dd.cb = Marshal.SizeOf(dd);
            uint devNum = 0;
            while (NativeMethods.EnumDisplayDevices(null, devNum, ref dd, 0))
            {
                Console.WriteLine("Found Display Device {0}: {1}; {2}; {3}; {4}; {5};",
                    devNum,
                    dd.DeviceName, dd.DeviceString, dd.StateFlags, dd.DeviceID, dd.DeviceKey);
                devNum++;
                dd.cb = Marshal.SizeOf(dd);
            }
        }

        public static void GetCurrentSettings()
        {
            NativeMethods.DEVMODE mode = new NativeMethods.DEVMODE();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);
            if(NativeMethods.EnumDisplaySettings(null, NativeMethods.ENUM_CURRENT_SETTINGS, ref mode))
            {
                Console.WriteLine("Current Display Mode: {0}x{1}, {2} bit, {3} degrees, {4} Hz",
                    mode.dmPelsWidth, mode.dmPelsHeight,
                    mode.dmBitsPerPel,
                    mode.dmDisplayOrientation * 90,
                    mode.dmDisplayFrequency);
            }
        }
        public static void GetDisplaySettings()
        {
            List<NativeMethods.DEVMODE> settings = new List<NativeMethods.DEVMODE>();
            NativeMethods.DEVMODE mode = new NativeMethods.DEVMODE();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);
            int modenum = 0;
            Console.WriteLine("Supported display modes:");
            while(NativeMethods.EnumDisplaySettings(null, modenum, ref mode))
            {
                settings.Add(mode);
                Console.WriteLine("\t{5}: {0}x{1}, {2} bpp, {3} deg., {4}Hz",
                    mode.dmPelsWidth, mode.dmPelsHeight,
                    mode.dmBitsPerPel,
                    mode.dmDisplayOrientation * 90,
                    mode.dmDisplayFrequency, mode.dmDeviceName);
                mode = new NativeMethods.DEVMODE();
                mode.dmSize = (ushort)Marshal.SizeOf(mode);
                modenum++;
            }
            Console.WriteLine("Got {0} display modes!", settings.Count);
        }
        private class NativeMethods
        {
            public const int ENUM_CURRENT_SETTINGS = -1;

            [DllImport("user32.dll")]
            public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
            [DllImport("user32.dll")]
            public static extern bool EnumDisplaySettingsEx(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode, uint dwFlags);

            [DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumDisplaySettings(
                [param: MarshalAs(UnmanagedType.LPTStr)] string lpszDeviceName,
                [param: MarshalAs(UnmanagedType.U4)] int iModeNum,
                [In, Out] ref DEVMODE lpDevMode);

            [DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.I4)]
            public static extern int ChangeDisplaySettings(
                [In, Out] ref DEVMODE lpDevMode,
                [param: MarshalAs(UnmanagedType.U4)] uint dwflags);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct DISPLAY_DEVICE
            {
                [MarshalAs(UnmanagedType.U4)]
                public int cb;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string DeviceName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceString;
                [MarshalAs(UnmanagedType.U4)]
                public DisplayDeviceStateFlags StateFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceID;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceKey;
            }

            [Flags()]
            public enum DisplayDeviceStateFlags : int
            {
                /// <summary>The device is part of the desktop.</summary>
                AttachedToDesktop = 0x1,
                MultiDriver = 0x2,
                /// <summary>The device is part of the desktop.</summary>
                PrimaryDevice = 0x4,
                /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
                MirroringDriver = 0x8,
                /// <summary>The device is VGA compatible.</summary>
                VGACompatible = 0x10,
                /// <summary>The device is removable; it cannot be the primary display.</summary>
                Removable = 0x20,
                /// <summary>The device has more display modes than its output devices support.</summary>
                ModesPruned = 0x8000000,
                Remote = 0x4000000,
                Disconnect = 0x2000000
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct DEVMODE
            {
                // You can define the following constant
                // but OUTSIDE the structure because you know
                // that size and layout of the structure
                // is very important
                // CCHDEVICENAME = 32 = 0x50
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string dmDeviceName;
                // In addition you can define the last character array
                // as following:
                //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
                //public Char[] dmDeviceName;

                // After the 32-bytes array
                [MarshalAs(UnmanagedType.U2)]
                public UInt16 dmSpecVersion;

                [MarshalAs(UnmanagedType.U2)]
                public UInt16 dmDriverVersion;

                [MarshalAs(UnmanagedType.U2)]
                public UInt16 dmSize;

                [MarshalAs(UnmanagedType.U2)]
                public UInt16 dmDriverExtra;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmFields;

                public POINTL dmPosition;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmDisplayOrientation;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmDisplayFixedOutput;

                [MarshalAs(UnmanagedType.I2)]
                public Int16 dmColor;

                [MarshalAs(UnmanagedType.I2)]
                public Int16 dmDuplex;

                [MarshalAs(UnmanagedType.I2)]
                public Int16 dmYResolution;

                [MarshalAs(UnmanagedType.I2)]
                public Int16 dmTTOption;

                [MarshalAs(UnmanagedType.I2)]
                public Int16 dmCollate;

                // CCHDEVICENAME = 32 = 0x50
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string dmFormName;
                // Also can be defined as
                //[MarshalAs(UnmanagedType.ByValArray,
                //    SizeConst = 32, ArraySubType = UnmanagedType.U1)]
                //public Byte[] dmFormName;

                [MarshalAs(UnmanagedType.U2)]
                public UInt16 dmLogPixels;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmBitsPerPel;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmPelsWidth;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmPelsHeight;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmDisplayFlags;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmDisplayFrequency;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmICMMethod;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmICMIntent;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmMediaType;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmDitherType;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmReserved1;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmReserved2;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmPanningWidth;

                [MarshalAs(UnmanagedType.U4)]
                public UInt32 dmPanningHeight;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINTL
            {
                [MarshalAs(UnmanagedType.I4)]
                public int x;
                [MarshalAs(UnmanagedType.I4)]
                public int y;
            }
        }
    }
}
