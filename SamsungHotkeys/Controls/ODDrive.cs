using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SamsungHotkeys.Controls
{
    class ODDrive
    {
        public static void Eject()
        {
            IntPtr ptr = IntPtr.Zero;
            StringBuilder returnstring = new StringBuilder();
            NativeMethods.mciSendString("set CDAudio door open", returnstring, 127, IntPtr.Zero);
        }

        private class NativeMethods
        {
            [DllImport("winmm.dll")]
            public static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
        }
        
    }
}
