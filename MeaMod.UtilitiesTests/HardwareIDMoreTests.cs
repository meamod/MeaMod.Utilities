namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class HardwareIDMoreTests
    {
        [TestMethod]
        public void DiskSerialLookupFailureReturnsAnEmptyString()
        {
            // The WMI path is built from the SystemDrive variable; an impossible drive makes the query fail.
            string? original = Environment.GetEnvironmentVariable("SystemDrive");
            try
            {
                Environment.SetEnvironmentVariable("SystemDrive", "Z9:");

                Assert.AreEqual(string.Empty, HardwareID.GetDiskVolumeSerialNumber());
            }
            finally
            {
                Environment.SetEnvironmentVariable("SystemDrive", original);
            }
        }

        [TestMethod]
        public void DiskSerialOfTheRealSystemDriveIsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(HardwareID.GetDiskVolumeSerialNumber()));
        }
    }
}
