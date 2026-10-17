using System;
using System.Linq;
using System.Management;

namespace MeaMod.Utilities
{
    /// <summary>Builds a machine identifier from WMI hardware details.</summary>
    public class HardwareID
    {
        /// <summary>
        /// Returns an upper-case SHA-256 hex digest of the processor id, motherboard serial number and system
        /// drive volume serial number.
        /// </summary>
        /// <returns>A 64 character hexadecimal string.</returns>
        public static string GetHardwareID()
        {
#pragma warning disable CS0618 // Hash is obsolete but the output format must stay stable
            return Cryptography.Hash.SHA256(GetProcessorId() + GetMotherboardID() + GetDiskVolumeSerialNumber()).ToUpperInvariant();
#pragma warning restore CS0618
        }

        /// <summary>Returns the ProcessorId of the last processor reported by WMI, or an empty string.</summary>
        /// <returns>The processor id.</returns>
        public static string GetProcessorId()
        {
            return GetFirstPropertyValue("Win32_Processor", "ProcessorId");
        }

        /// <summary>Returns the MAC address, without separators, of the first IP-enabled network adapter.</summary>
        /// <returns>The MAC address, or an empty string when no adapter is IP enabled.</returns>
        public static string GetMACAddress()
        {
            string macAddress = string.Empty;
            using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            using (var moc = mc.GetInstances())
            {
                foreach (ManagementObject mo in moc.Cast<ManagementObject>())
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

        /// <summary>Returns the volume serial number of the system drive, or an empty string if it cannot be read.</summary>
        /// <returns>The volume serial number.</returns>
        public static string GetDiskVolumeSerialNumber()
        {
            try
            {
                using var disk = new ManagementObject("Win32_LogicalDisk.DeviceID=\"" + Environment.GetEnvironmentVariable("SystemDrive") + "\"");
                disk.Get();
                return disk["VolumeSerialNumber"]?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>Returns the serial number of the last base board reported by WMI, or an empty string.</summary>
        /// <returns>The motherboard serial number.</returns>
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
                foreach (ManagementObject info in results.Cast<ManagementObject>())
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
