using System;
using System.Management;

namespace MeaMod.Utilities
{
    public class HardwareID
    {
        public static string GetHardwareID()
        {
#pragma warning disable CS0618 // Hash is obsolete but the output format must stay stable
            return Cryptography.Hash.SHA256(GetProcessorId() + GetMotherboardID() + GetDiskVolumeSerialNumber()).ToUpperInvariant();
#pragma warning restore CS0618
        }

        // Get processor ID
        public static string GetProcessorId()
        {
            return GetFirstPropertyValue("Win32_Processor", "ProcessorId");
        }

        // Get MAC Address
        public static string GetMACAddress()
        {
            string macAddress = string.Empty;
            using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            using (var moc = mc.GetInstances())
            {
                foreach (ManagementObject mo in moc)
                {
                    using (mo)
                    {
                        if (!string.IsNullOrEmpty(macAddress))
                            continue;

                        // IPEnabled is a WMI boolean, not a string
                        if (mo["IPEnabled"] is bool ipEnabled && ipEnabled && mo["MacAddress"] is string mac)
                            macAddress = mac.Replace(":", string.Empty);
                    }
                }
            }

            return macAddress;
        }

        public static string GetDiskVolumeSerialNumber()
        {
            try
            {
                using (var disk = new ManagementObject("Win32_LogicalDisk.DeviceID=\"" + Environment.GetEnvironmentVariable("SystemDrive") + "\""))
                {
                    disk.Get();
                    return disk["VolumeSerialNumber"]?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        // Get Motherboard ID
        public static string GetMotherboardID()
        {
            return GetFirstPropertyValue("Win32_BaseBoard", "SerialNumber");
        }

        private static string GetFirstPropertyValue(string wmiClass, string propertyName)
        {
            string value = string.Empty;
            using (var search = new ManagementObjectSearcher(new SelectQuery(wmiClass)))
            using (var results = search.Get())
            {
                foreach (ManagementObject info in results)
                {
                    using (info)
                    {
                        // Some (virtual) machines report a null value here
                        value = info[propertyName]?.ToString() ?? string.Empty;
                    }
                }
            }

            return value;
        }
    }
}
