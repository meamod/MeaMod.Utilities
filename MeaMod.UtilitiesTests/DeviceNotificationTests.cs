namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class DeviceNotificationTests
    {
        [TestMethod]
        public void ZeroLParamIsNotADevice()
        {
            // DBT_DEVNODES_CHANGED and friends arrive with lParam == 0; the old code dereferenced it.
            Assert.IsFalse(DeviceNotification.IsUsbDevice(IntPtr.Zero));
            Assert.IsFalse(DeviceNotification.IsMonitor(IntPtr.Zero));
        }
    }
}
