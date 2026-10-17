using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MeaMod.Utilities
{
    /// <summary>
    /// Call for Windows Device Notifications
    /// </summary>
    public static class DeviceNotification
    {
        #region Constants & Types

        /// <summary>
        /// System detected a new device  
        /// </summary>
        public const int DbtDeviceArrival = 0x8000;
        /// <summary>
        /// System detected the Device is gone 
        /// </summary>
        public const int DbtDeviceRemoveComplete = 0x8004;
        /// <summary>
        /// Device change event
        /// </summary>
        public const int WmDeviceChange = 0x0219;

        #endregion

        #region Methods
        /// <summary>
        /// Determine if device is a monitor
        /// </summary>
        /// <param name="lParam">LParam field of the message</param>
        /// <returns>True if device is a monitor</returns>
        public static bool IsMonitor(IntPtr lParam)
        {
            return IsDeviceOfClass(lParam, GuidDeviceInterfaceMonitorDevice);
        }

        /// <summary>
        /// Determine if device is USB
        /// </summary>
        /// <param name="lParam">LParam field of the message</param>
        /// <returns>True if device is a USB device</returns>
        public static bool IsUsbDevice(IntPtr lParam)
        {
            return IsDeviceOfClass(lParam, GuidDeviceInterfaceUSBDevice);
        }

        /// <summary>
        /// Registers a window to receive notifications when Monitor devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Control HWnd</param>
        public static void RegisterMonitorDeviceNotification(IntPtr windowHandle)
        {
            var dbi = CreateBroadcastDeviceInterface(GuidDeviceInterfaceMonitorDevice);
            monitorNotificationHandle = RegisterDeviceNotification(dbi, windowHandle);
        }

        /// <summary>
        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Control HWnd</param>
        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            var dbi = CreateBroadcastDeviceInterface(GuidDeviceInterfaceUSBDevice);
            usbNotificationHandle = RegisterDeviceNotification(dbi, windowHandle);
        }

        /// <summary>
        /// UnRegisters the window for Monitor device notifications
        /// </summary>
        public static void UnRegisterMonitorDeviceNotification()
        {
            UnregisterDeviceNotification(monitorNotificationHandle);
        }

        /// <summary>
        /// UnRegisters the window for USB device notifications
        /// </summary>
        public static void UnRegisterUsbDeviceNotification()
        {
            UnregisterDeviceNotification(usbNotificationHandle);
        }

        #endregion

        #region Private or protected constants & types

        private const int DbtDeviceTypeDeviceInterface = 5;

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-usb-device
        /// <summary>
        /// USB devices
        /// </summary>
        private static readonly Guid GuidDeviceInterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-monitor
        /// <summary>
        /// Monitor devices
        /// </summary>
        private static readonly Guid GuidDeviceInterfaceMonitorDevice = new Guid("E6F07B5F-EE97-4a90-B076-33F57BF4EAA7"); 

        private static IntPtr usbNotificationHandle;

        private static IntPtr monitorNotificationHandle;

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceInterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastHdr
        {
            internal UInt32 Size;
            internal UInt32 DeviceType;
            internal UInt32 Reserved;
        }

        #endregion

        #region Private & protected methods

        private static bool IsDeviceOfClass(IntPtr lParam, Guid classGuid)
        {
            // Some WM_DEVICECHANGE events (e.g. DBT_DEVNODES_CHANGED) carry no structure at all.
            if (lParam == IntPtr.Zero)
                return false;

            DevBroadcastHdr hdr = (DevBroadcastHdr)Marshal.PtrToStructure(lParam, typeof(DevBroadcastHdr));

            if (hdr.DeviceType != DbtDeviceTypeDeviceInterface)
                return false;

            DevBroadcastDeviceInterface devIF = (DevBroadcastDeviceInterface)Marshal.PtrToStructure(lParam, typeof(DevBroadcastDeviceInterface));

            return devIF.ClassGuid == classGuid;

        }

        private static DevBroadcastDeviceInterface CreateBroadcastDeviceInterface(Guid classGuid)
        {
            var dbi = new DevBroadcastDeviceInterface
            {
                DeviceType = DbtDeviceTypeDeviceInterface,
                Reserved = 0,
                ClassGuid = classGuid,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);

            return dbi;
        }

        private static IntPtr RegisterDeviceNotification(DevBroadcastDeviceInterface dbi, IntPtr windowHandle)
        {
            var buffer = Marshal.AllocHGlobal(dbi.Size);
            IntPtr handle;

            try
            {
                Marshal.StructureToPtr(dbi, buffer, true);

                handle = RegisterDeviceNotification(windowHandle, buffer, 0);
            }
            finally
            {
                // Free buffer
                Marshal.FreeHGlobal(buffer);
            }

            return handle;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        #endregion
    }
}
