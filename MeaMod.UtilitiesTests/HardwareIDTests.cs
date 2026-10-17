namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class HardwareIDTests
    {
        [TestMethod]
        public void GetMACAddressDoesNotThrowAndStripsColons()
        {
            // The old code cast the WMI boolean IPEnabled to string and always threw InvalidCastException.
            string mac = HardwareID.GetMACAddress();

            Assert.IsNotNull(mac);
            Assert.IsFalse(mac.Contains(':'), mac);
        }

        [TestMethod]
        public void GetHardwareIDReturnsUpperCaseSha256Hex()
        {
            string id = HardwareID.GetHardwareID();

            Assert.AreEqual(64, id.Length, id);
            Assert.AreEqual(id.ToUpperInvariant(), id);
            Assert.IsTrue(id.All(Uri.IsHexDigit), id);
        }

        [TestMethod]
        public void ComponentLookupsDoNotThrow()
        {
            Assert.IsNotNull(HardwareID.GetProcessorId());
            Assert.IsNotNull(HardwareID.GetMotherboardID());
            Assert.IsNotNull(HardwareID.GetDiskVolumeSerialNumber());
        }
    }
}
