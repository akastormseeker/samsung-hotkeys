using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SamsungHotkeys.Controls
{
    class PowerPlan
    {
        private ManagementObject mMgmtObj;

        public string Name { get { return (string)mMgmtObj["ElementName"]; } }
        public string ID { get { return (string)mMgmtObj["InstanceID"]; } }
        public string Description { get { return (string)mMgmtObj["Description"]; } }
        public bool IsActive { get { return (bool)mMgmtObj["IsActive"]; } }

        public void Activate() { mMgmtObj.InvokeMethod("Activate", new object[0]); }
        
        public PowerPlan(ManagementObject fromObj)
        {
            mMgmtObj = fromObj;
        }
    }
    class PowerManager
    {
        public static PowerPlan[] GetAllPowerPlans()
        {
            SelectQuery query = new SelectQuery("Win32_powerplan");
            ManagementScope scope = new ManagementScope("root/cimv2/power");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            List<PowerPlan> plans = new List<PowerPlan>();
            foreach (ManagementObject obj in searcher.Get())
            {
                plans.Add(new PowerPlan(obj));
            }

            return plans.ToArray();
        }
    }
}
