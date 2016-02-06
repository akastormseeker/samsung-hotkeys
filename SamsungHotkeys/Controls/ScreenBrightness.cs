using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SamsungHotkeys.Controls
{
    class ScreenBrightness
    {
        public static int GetScreenBrightness()
        {
            byte[] levels;
            return GetScreenBrightness(out levels);
        }

        public static int GetScreenBrightness(out byte[] levels)
        {
            SelectQuery query = new SelectQuery("WmiMonitorBrightness");
            ManagementScope scope = new ManagementScope("root/wmi");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            levels = new byte[0];
            foreach (ManagementObject obj in searcher.Get())
            {
                //Debug.WriteLine("Monitor: active={0}, CurrentBrightness={1}", obj["Active"], obj["CurrentBrightness"]);
                if ((bool)obj["Active"])
                {
                    levels = (byte[])obj["Level"];
                    byte cb = byte.Parse(obj["CurrentBrightness"].ToString());
                    return cb;
                }
            }
            return -1;
        }

        public static void SetScreenBrightness(int brightness)
        {
            SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");
            ManagementScope scope = new ManagementScope("root/wmi");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject obj in searcher.Get())
            {
                obj.InvokeMethod("WmiSetBrightness", new object[] { Int32.MaxValue, (byte)brightness });

            }
        }
    }
}
