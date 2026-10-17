using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class DeviceNotificationMoreTests
    {
        private const int DbtDevTypDeviceInterface = 5;
        private const int DbtDevTypVolume = 2;
        private static readonly Guid UsbDeviceInterface = new("A5DCBF10-6530-11D2-901F-00C04FB951ED");
        private static readonly Guid MonitorDeviceInterface = new("E6F07B5F-EE97-4a90-B076-33F57BF4EAA7");

        /// <summary>Builds a DEV_BROADCAST_DEVICEINTERFACE block the way Windows passes it in WM_DEVICECHANGE's lParam.</summary>
        private static IntPtr AllocBroadcast(int deviceType, Guid classGuid)
        {
            const int size = 64;
            IntPtr block = Marshal.AllocHGlobal(size);
            for (int i = 0; i < size; i++)
                Marshal.WriteByte(block, i, 0);

            Marshal.WriteInt32(block, 0, 32);          // dbcc_size
            Marshal.WriteInt32(block, 4, deviceType);  // dbcc_devicetype
            Marshal.WriteInt32(block, 8, 0);           // dbcc_reserved
            Marshal.Copy(classGuid.ToByteArray(), 0, block + 12, 16); // dbcc_classguid
            return block;
        }

        [TestMethod]
        public void UsbInterfaceBroadcastIsRecognised()
        {
            IntPtr block = AllocBroadcast(DbtDevTypDeviceInterface, UsbDeviceInterface);
            try
            {
                Assert.IsTrue(DeviceNotification.IsUsbDevice(block));
                Assert.IsFalse(DeviceNotification.IsMonitor(block));
            }
            finally
            {
                Marshal.FreeHGlobal(block);
            }
        }

        [TestMethod]
        public void MonitorInterfaceBroadcastIsRecognised()
        {
            IntPtr block = AllocBroadcast(DbtDevTypDeviceInterface, MonitorDeviceInterface);
            try
            {
                Assert.IsTrue(DeviceNotification.IsMonitor(block));
                Assert.IsFalse(DeviceNotification.IsUsbDevice(block));
            }
            finally
            {
                Marshal.FreeHGlobal(block);
            }
        }

        [TestMethod]
        public void OtherBroadcastTypesAreIgnored()
        {
            IntPtr block = AllocBroadcast(DbtDevTypVolume, UsbDeviceInterface);
            try
            {
                Assert.IsFalse(DeviceNotification.IsUsbDevice(block));
                Assert.IsFalse(DeviceNotification.IsMonitor(block));
            }
            finally
            {
                Marshal.FreeHGlobal(block);
            }
        }

        [TestMethod]
        public void RegistrationWithARealWindowSucceedsAndCanBeUndone()
        {
            StaThread.Run(() =>
            {
                using var control = new Control();
                IntPtr handle = control.Handle;

                DeviceNotification.RegisterUsbDeviceNotification(handle);
                DeviceNotification.RegisterMonitorDeviceNotification(handle);
                DeviceNotification.UnRegisterUsbDeviceNotification();
                DeviceNotification.UnRegisterMonitorDeviceNotification();
            });
        }

        [TestMethod]
        public void RegistrationWithoutAWindowDoesNotThrow()
        {
            DeviceNotification.RegisterUsbDeviceNotification(IntPtr.Zero);
            DeviceNotification.RegisterMonitorDeviceNotification(IntPtr.Zero);
            DeviceNotification.UnRegisterUsbDeviceNotification();
            DeviceNotification.UnRegisterMonitorDeviceNotification();
        }
    }
}
