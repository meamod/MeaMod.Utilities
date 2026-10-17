using System.Security;
using Microsoft.Win32;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class RegistrySettingsMoreTests
    {
        private const string TestBaseKey = @"SOFTWARE\MeaMod.Utilities.Tests\";
        private const string WindowsNtBaseKey = @"SOFTWARE\Microsoft\Windows NT\";
        private const string CurrentVersion = "CurrentVersion";
        private string _subKey = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            _subKey = Guid.NewGuid().ToString("N");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Registry.CurrentUser.DeleteSubKeyTree(TestBaseKey + _subKey, throwOnMissingSubKey: false);
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(TestBaseKey + _subKey, throwOnMissingSubKey: false);
            }
            catch (UnauthorizedAccessException)
            {
                // Not elevated: nothing was written there either.
            }
        }

        private void SetRawValue(string name, object value, RegistryValueKind kind)
        {
            using var key = Registry.CurrentUser.CreateSubKey(TestBaseKey + _subKey);
            Assert.IsNotNull(key);
            key.SetValue(name, value, kind);
        }

        [TestMethod]
        public void LocalMachineReadsWork()
        {
            Assert.IsFalse(string.IsNullOrEmpty(RegistrySettings.GetSettingLM(WindowsNtBaseKey, CurrentVersion, "ProductName")));
            Assert.IsTrue(RegistrySettings.GetIntegerLM(WindowsNtBaseKey, CurrentVersion, "CurrentMajorVersionNumber") >= 10);
            Assert.AreEqual(0, RegistrySettings.GetIntegerLM(WindowsNtBaseKey, CurrentVersion, "MeaModMissingValue"));
            Assert.IsTrue(RegistrySettings.GetBooleanSettingLM(WindowsNtBaseKey, CurrentVersion, "MeaModMissingValue", defaultValue: true));
            CollectionAssert.Contains(RegistrySettings.GetValueNamesLM(WindowsNtBaseKey, CurrentVersion), "ProductName");
        }

        [TestMethod]
        public void LocalMachineReadsOfAMissingKeyReturnDefaults()
        {
            Assert.AreEqual(string.Empty, RegistrySettings.GetSettingLM(TestBaseKey, _subKey, "Value"));
            Assert.AreEqual(0, RegistrySettings.GetIntegerLM(TestBaseKey, _subKey, "Value"));
            Assert.IsFalse(RegistrySettings.GetBooleanSettingLM(TestBaseKey, _subKey, "Value"));
            Assert.AreEqual(0, RegistrySettings.GetValueNamesLM(TestBaseKey, _subKey).Length);
            Assert.AreEqual(0, RegistrySettings.GetValueNames(TestBaseKey, _subKey).Length);
        }

        [TestMethod]
        public void LocalMachineWritesRequireElevationOrRoundTrip()
        {
            try
            {
                RegistrySettings.SaveSettingLM(TestBaseKey, _subKey, "String", "x");
            }
            catch (UnauthorizedAccessException)
            {
                // Not elevated: the write path was exercised up to the access check.
                Assert.ThrowsExactly<UnauthorizedAccessException>(() => RegistrySettings.SaveIntegerLM(TestBaseKey, _subKey, "Int", 1));
                Assert.ThrowsExactly<UnauthorizedAccessException>(() => RegistrySettings.SaveBooleanSettingLM(TestBaseKey, _subKey, "Bool", true));
                Assert.ThrowsExactly<UnauthorizedAccessException>(() => RegistrySettings.DeleteSettingLM(TestBaseKey, _subKey, "String"));
                return;
            }

            RegistrySettings.SaveIntegerLM(TestBaseKey, _subKey, "Int", 7);
            RegistrySettings.SaveBooleanSettingLM(TestBaseKey, _subKey, "Bool", true);

            Assert.AreEqual("x", RegistrySettings.GetSettingLM(TestBaseKey, _subKey, "String"));
            Assert.AreEqual(7, RegistrySettings.GetIntegerLM(TestBaseKey, _subKey, "Int"));
            Assert.IsTrue(RegistrySettings.GetBooleanSettingLM(TestBaseKey, _subKey, "Bool"));

            RegistrySettings.DeleteSettingLM(TestBaseKey, _subKey, "String");
            Assert.AreEqual(string.Empty, RegistrySettings.GetSettingLM(TestBaseKey, _subKey, "String"));
        }

        [TestMethod]
        public void ReadingAProtectedHiveLogsAndRethrows()
        {
            // HKLM\SECURITY is readable only by SYSTEM, so the read fails and the failure is logged and rethrown.
            Assert.ThrowsExactly<SecurityException>(() => RegistrySettings.GetSettingLM(@"SECURITY\", "Policy", "Value"));
        }

        [TestMethod]
        public void GetIntegerWithANonNumericStringThrowsFormatException()
        {
            SetRawValue("Value", "abc", RegistryValueKind.String);

            Assert.ThrowsExactly<FormatException>(() => RegistrySettings.GetInteger(TestBaseKey, _subKey, "Value"));
        }

        [TestMethod]
        public void GetIntegerWithAnUnsupportedKindIsZero()
        {
            SetRawValue("Value", new byte[] { 1, 2 }, RegistryValueKind.Binary);

            Assert.AreEqual(0, RegistrySettings.GetInteger(TestBaseKey, _subKey, "Value"));
        }

        [TestMethod]
        public void GetBooleanSettingWithAnUnparsableStringReturnsTheDefault()
        {
            SetRawValue("Value", "maybe", RegistryValueKind.String);

            Assert.IsTrue(RegistrySettings.GetBooleanSetting(TestBaseKey, _subKey, "Value", defaultValue: true));
            Assert.IsFalse(RegistrySettings.GetBooleanSetting(TestBaseKey, _subKey, "Value", defaultValue: false));
        }

        [TestMethod]
        public void GetBooleanSettingWithABinaryValueThrows()
        {
            SetRawValue("Value", new byte[] { 1 }, RegistryValueKind.Binary);

            Assert.ThrowsExactly<InvalidCastException>(() => RegistrySettings.GetBooleanSetting(TestBaseKey, _subKey, "Value"));
        }

        [TestMethod]
        public void SaveSettingRoundTripsAndListsValueNames()
        {
            RegistrySettings.SaveSetting(TestBaseKey, _subKey, "Name", "value");

            Assert.AreEqual("value", RegistrySettings.GetSetting(TestBaseKey, _subKey, "Name"));
            CollectionAssert.Contains(RegistrySettings.GetValueNames(TestBaseKey, _subKey), "Name");
        }
    }
}
