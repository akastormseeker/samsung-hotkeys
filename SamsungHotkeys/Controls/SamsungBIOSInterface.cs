using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SamsungHotkeys.Controls
{
    public class SamsungBIOSInterface
    {
        public string BIOSModelName { get; private set; }

        public SamsungBIOSInterface()
        {
            BIOSModelName = null;

            int hresult = 0;
            if ((hresult = NativeMethods.InitBIOSInterface()) != 0)
            {
                throw new InterfaceNotInitializedException(hresult);
            }

            if ((hresult = NativeMethods.CheckSamsung()) != 0)
            {
                throw new NotSamsungBIOSException(hresult);
            }

            BIOSModelName = GetModelName();
        }

        public bool GetWirelessStatus(out byte[] status)
        {
            byte[] buffer = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.GetWirelessStatus, buffer);
            if (rv != 0)
            {
                Debug.WriteLine("Error retrieving wireless status!");
                status = null;
                return false;
            }
            status = buffer;
            return true;
        }

        public bool SetWirelessStatus(byte[] status)
        {
            byte[] buffer = new byte[] { status[0], status[1], status[2], 0x00 };

            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.SetWirelessStatus, buffer);
            if (rv != 0)
            {
                Debug.WriteLine("Error retrieving wireless status!");
                return false;
            }

            status[0] = buffer[0];
            status[1] = buffer[1];
            status[2] = buffer[2];

            return true;
        }

        public int GetKbBacklightBrightness()
        {
            byte[] buffer = new byte[] { 0x81, 0, 0, 0 };
            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.KbBacklight, buffer);
            if (rv != 0)
            {
                Debug.WriteLine("Error reading keyboard backlight brightness!");
                throw new MethodInvocationException("GetKbBacklightBrightness");
            }
            return buffer[0];
        }

        public void SetKbBacklightBrightness(int brightness)
        {
            byte[] buffer = new byte[] { 0x82, (byte)(brightness & 0xFF), 0, 0 };
            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.KbBacklight, buffer);
            if (rv != 0)
            {
                Debug.WriteLine("Error setting keyboard backlight brightness!");
                throw new MethodInvocationException("SetKbBacklightBrightness");
            }
        }


        public int GetKeybardALSStatus()
        {
            byte[] buffer = new byte[4];
            buffer[1] = buffer[2] = buffer[3] = 0;
            buffer[0] = 0x80;
            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.KbBacklight, buffer);
            Debug.WriteLine("CallBIOSInterface(0x78, 0x00000080) returned {0}; data[0]=[{1}, {2}, {3}, {4}]", rv, buffer[0].ToString("X2"), buffer[1].ToString("X2"), buffer[2].ToString("X2"), buffer[3].ToString("X2"));
            if (rv != 0)
            {
                Debug.WriteLine("Error calling SABI interface!");
                throw new MethodInvocationException("GetKeyboardALSStatus");
            }

            if (buffer[0] == 0xFF)
            {
                Debug.WriteLine("Unable to get Keyboard ALS Status!");
                throw new MethodNotSupportedException("GetKeyboardALSStatus");
            }

            return buffer[0];
        }

        private bool GetKeybardBacklight81(out int status)
        {
            byte[] buffer = new byte[4];
            buffer[1] = buffer[2] = buffer[3] = 0;
            buffer[0] = 0x81;
            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.KbBacklight, buffer);
            Debug.WriteLine("CallBIOSInterface(0x78, 0x00000080) returned {0}; data[0]=[{1}, {2}, {3}, {4}]", rv, buffer[0].ToString("X2"), buffer[1].ToString("X2"), buffer[2].ToString("X2"), buffer[3].ToString("X2"));
            if (rv != 0)
            {
                Debug.WriteLine("Error calling SABI interface!");
                status = 0;
                return false;
            }

            status = 0;
            return true;
        }

        private bool GetKeybardBacklight82(out int status)
        {
            byte[] buffer = new byte[4];
            buffer[1] = buffer[2] = buffer[3] = 0;
            buffer[0] = 0x82;
            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.KbBacklight, buffer);
            Debug.WriteLine("CallBIOSInterface(0x78, 0x00000080) returned {0}; data[0]=[{1}, {2}, {3}, {4}]", rv, buffer[0].ToString("X2"), buffer[1].ToString("X2"), buffer[2].ToString("X2"), buffer[3].ToString("X2"));
            if (rv != 0)
            {
                Debug.WriteLine("Error calling SABI interface!");
                status = 0;
                return false;
            }

            status = 0;
            return true;
        }

        public void SetVolumeMuteLight(bool on)
        {
            byte[] buffer = new byte[4];
            buffer[0] = 0xBB;
            buffer[1] = 0xAA;
            buffer[2] = buffer[3] = 0x00;
            int rv = NativeMethods.CallBIOSInterface((int)BIOSFunctions.SetMuteLight, buffer);
            if (rv != 0 || buffer[0] != 0xDD || buffer[1] != 0xCC)
            {
                Debug.WriteLine("Test for KB Backlight interface failed. CallBIOSInterface() returned: " + rv + "; buffer[] = [" + buffer[0].ToString("X2") + ", " + buffer[1].ToString("X2") + ", " + buffer[2].ToString("X2") + ", " + buffer[3].ToString("X2"));
                throw new MethodNotSupportedException("SetVolumeMuteLight");
            }

            buffer[0] = 0x81;
            buffer[1] = (byte)(on ? 1 : 0);
            buffer[2] = buffer[3] = 0;
            rv = CallBIOSInterface((int)BIOSFunctions.SetMuteLight, buffer);
            if (rv != 0)
            {
                Debug.WriteLine("Call KB Backlight interface failed. CallBIOSInterface() returned: " + rv + "; buffer[] = [" + buffer[0].ToString("X2") + ", " + buffer[1].ToString("X2") + ", " + buffer[2].ToString("X2") + ", " + buffer[3].ToString("X2"));
                throw new MethodInvocationException("SetVolumeMuteLight");
            }
        }

        public int CallBIOSInterface(int fn, byte[] data) {
            //IntPtr unmanagedBuffer = Marshal.AllocHGlobal(data.Length);
            //Marshal.Copy(data, 0, unmanagedBuffer, data.Length);
            //int rv = NativeMethods.CallBIOSInterface(fn, unmanagedBuffer);
            //Marshal.Copy(unmanagedBuffer, data, 0, data.Length);
            //Marshal.FreeHGlobal(unmanagedBuffer);
            //return rv;
            return NativeMethods.CallBIOSInterface(fn, data);
        }
        public int CallBIOSInterface(int fn, IntPtr buffer)
        {
            return NativeMethods.CallBIOSInterface(fn, buffer);
        }

        #region Static Methods

        private static string GetModelName()
        {
            string outStr;
            int rv = CallBIOSInterface((int)BIOSFunctions.GetModelName, out outStr);
            return (rv == 0 ? outStr : null);
        }

        private static int CallBIOSInterface(int cmd, out string str)
        {
            byte[] buffer = new byte[100];
            int rv = NativeMethods.CallBIOSInterface(cmd, buffer);
            if (rv != 0)
            {
                Debug.WriteLine("Error {0} calling BIOS function {1}.", rv, cmd);
                str = null;
                return rv;
            }
            int length = 0;
            for (length = 0; length < buffer.Length; length++)
            {
                if (buffer[length] == 0x00)
                {
                    break;
                }
            }
            str = Encoding.ASCII.GetString(buffer, 0, length);

            Debug.WriteLine("Got string from BIOS function {0}: '{1}'", cmd, str.Trim());

            return rv;
        }

        #endregion

        #region Private Enums

        private enum BIOSFunctions : int
        {
            GetModelName = 0x04,     // use with BIOSInterfaceGetString()

            GetWirelessStatus = 0x69,  // bytes 0-2 = status of wifi, wwan, bluetooth; 0 = off, 1 = on, 2 = n/a
            SetWirelessStatus = 0x6A,  // bytes 0-2 = wifi, wwan, bt; 0 = off, 1 = on, 2 = [in]leave as-is [out]n/a

            KbBacklight = 0x78,

            SetMuteLight = 0x79, // Test: BB/AA/00/00; 81/[on/off]/0/0
        }

        private enum RadioStatus : byte
        {
            Off = 0,
            On = 1,
            NotAvailable = 2,
            DontChange = 2
        }

        #endregion

        private class NativeMethods
        {
            [DllImport("Sabi3.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int InitBIOSInterface();
            [DllImport("Sabi3.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int UninitBIOSInterface();

            [DllImport("Sabi3.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int CheckSamsung();

            [DllImport("Sabi3.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int CallBIOSInterface(int cmd, byte[] pData);
            [DllImport("Sabi3.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int CallBIOSInterface(int cmd, IntPtr pData);

            [DllImport("Sabi3.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int CallBIOSInterfaceEx(int cmd, short subCmd, IntPtr pData);
        }
    }

    #region Exceptions

    public class SamsungBIOSException : IOException
    {
        public SamsungBIOSException() : base() { }
        public SamsungBIOSException(string message) : base(message) { }
        public SamsungBIOSException(string message, Exception innerException) : base(message, innerException) { }
        public SamsungBIOSException(string message, int hresult) : base(message, hresult) { }
        public SamsungBIOSException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class InterfaceNotInitializedException : SamsungBIOSException
    {
        public InterfaceNotInitializedException(int hresult) : base("Unable to initialize BIOS interface.", hresult) { }
    }

    public class NotSamsungBIOSException : SamsungBIOSException
    {
        public NotSamsungBIOSException(int hresult) : base("Unable to verify Samsung BIOS!", hresult) { }
    }

    public class MethodInvocationException : SamsungBIOSException
    {
        public MethodInvocationException(string name) : base("There was an error invoking the method '" + name + "'.") { }
    }

    public class MethodNotSupportedException : SamsungBIOSException
    {
        public MethodNotSupportedException(string name) : base("The method '" + name + "' is not supported on this device.") { }
    }

    #endregion
}
