using MeaMod.Utilities.Security;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class SecuritySupportTests
    {
        [TestMethod]
        public void DwordAndIntConversionsRoundTrip()
        {
            Assert.AreEqual(5u, SecuritySupport.GetDWORDFromInt(5));
            Assert.AreEqual(0xFFFFFFFFu, SecuritySupport.GetDWORDFromInt(-1));
            Assert.AreEqual(5, SecuritySupport.GetIntFromDWORD(5));
            Assert.AreEqual(unchecked((int)0x800B0100), SecuritySupport.GetIntFromDWORD(0x800B0100));
            Assert.AreEqual(-1, SecuritySupport.GetIntFromDWORD(0xFFFFFFFF));
        }

        [TestMethod]
        public void CheckIfFileExistsThrowsOnlyForMissingFiles()
        {
            string existing = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");

            SecuritySupport.CheckIfFileExists(existing);
            Assert.ThrowsExactly<FileNotFoundException>(() => SecuritySupport.CheckIfFileExists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        }

        [TestMethod]
        public void TryAddOnlyAddsNewKeys()
        {
            var dictionary = new Dictionary<string, int>();

            Assert.IsTrue(dictionary.TryAdd("a", 1));
            Assert.IsFalse(dictionary.TryAdd("a", 2));
            Assert.AreEqual(1, dictionary["a"]);
            Assert.ThrowsExactly<ArgumentNullException>(() => Utils.TryAdd((IDictionary<string, int>)null!, "a", 1));
        }

        [TestMethod]
        public void CheckArgForNullOrEmptyRejectsNullAndEmpty()
        {
            Utils.CheckArgForNullOrEmpty("x", "arg");
            Assert.ThrowsExactly<ArgumentNullException>(() => Utils.CheckArgForNullOrEmpty(null!, "arg"));
            Assert.ThrowsExactly<ArgumentNullException>(() => Utils.CheckArgForNullOrEmpty(string.Empty, "arg"));
        }

        [TestMethod]
        public void CatalogProgressExposesItsValues()
        {
            var progress = new CatalogProgress("stage", 10, 3, "item", "SHA256");

            Assert.AreEqual("stage", progress.Stage);
            Assert.AreEqual(10, progress.Total);
            Assert.AreEqual(3, progress.Processed);
            Assert.AreEqual("item", progress.CurrentItem);
            Assert.AreEqual("SHA256", progress.HashAlgorithm);
        }
    }
}
