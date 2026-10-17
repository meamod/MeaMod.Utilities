using Microsoft.Win32;

namespace MeaMod.Utilities.Tests
{
    /// <summary>
    /// These tests write to a throw-away key under HKCU\SOFTWARE\MeaMod.Utilities.Tests and remove it afterwards.
    /// </summary>
    [TestClass]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [OSCondition(OperatingSystems.Windows)]
    public class RegistrySettingsTests
    {
        private const string BaseKey = @"SOFTWARE\MeaMod.Utilities.Tests\";
        private string _subKey = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            _subKey = Guid.NewGuid().ToString("N");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Registry.CurrentUser.DeleteSubKeyTree(BaseKey + _subKey, throwOnMissingSubKey: false);
            try
            {
                // Only succeeds once the last test has removed its own sub key.
                Registry.CurrentUser.DeleteSubKey(BaseKey.TrimEnd('\\'), throwOnMissingSubKey: false);
            }
            catch (InvalidOperationException)
            {
                // Still has sub keys from a test running in parallel.
            }
        }

        private void SetRawValue(string name, object value, RegistryValueKind kind)
        {
            using var key = Registry.CurrentUser.CreateSubKey(BaseKey + _subKey);
            Assert.IsNotNull(key);
            key.SetValue(name, value, kind);
        }

        [TestMethod]
        public void GetIntegerReadsAQWordValue()
        {
            // REG_QWORD comes back boxed as long; the old code unboxed it straight to int and threw.
            SetRawValue("Value", 1234L, RegistryValueKind.QWord);

            Assert.AreEqual(1234, RegistrySettings.GetInteger(BaseKey, _subKey, "Value"));
        }

        [TestMethod]
        public void GetIntegerReadsDWordAndStringValues()
        {
            SetRawValue("DWord", 42, RegistryValueKind.DWord);
            SetRawValue("String", "43", RegistryValueKind.String);

            Assert.AreEqual(42, RegistrySettings.GetInteger(BaseKey, _subKey, "DWord"));
            Assert.AreEqual(43, RegistrySettings.GetInteger(BaseKey, _subKey, "String"));
            Assert.AreEqual(0, RegistrySettings.GetInteger(BaseKey, _subKey, "Missing"));
        }

        [TestMethod]
        public void SaveIntegerThenGetIntegerRoundTrips()
        {
            RegistrySettings.SaveInteger(BaseKey, _subKey, "Value", -7);

            Assert.AreEqual(-7, RegistrySettings.GetInteger(BaseKey, _subKey, "Value"));
        }

        [TestMethod]
        public void GetBooleanSettingReturnsTheDefaultWhenTheValueIsMissing()
        {
            // Key exists, value does not: the old code returned Convert.ToBoolean(null) == false regardless of the default.
            SetRawValue("Other", "x", RegistryValueKind.String);

            Assert.IsTrue(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "Missing", defaultValue: true));
            Assert.IsFalse(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "Missing", defaultValue: false));
        }

        [TestMethod]
        public void GetBooleanSettingReturnsTheDefaultWhenTheKeyIsMissing()
        {
            Assert.IsTrue(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "Missing", defaultValue: true));
        }

        [TestMethod]
        public void SaveBooleanSettingThenGetBooleanSettingRoundTrips()
        {
            RegistrySettings.SaveBooleanSetting(BaseKey, _subKey, "On", true);
            RegistrySettings.SaveBooleanSetting(BaseKey, _subKey, "Off", false);

            Assert.IsTrue(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "On", defaultValue: false));
            Assert.IsFalse(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "Off", defaultValue: true));
        }

        [TestMethod]
        public void GetBooleanSettingAcceptsNumericValues()
        {
            SetRawValue("DWordOn", 1, RegistryValueKind.DWord);
            SetRawValue("DWordOff", 0, RegistryValueKind.DWord);
            SetRawValue("StringOn", "1", RegistryValueKind.String);

            Assert.IsTrue(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "DWordOn"));
            Assert.IsFalse(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "DWordOff", defaultValue: true));
            Assert.IsTrue(RegistrySettings.GetBooleanSetting(BaseKey, _subKey, "StringOn"));
        }

        [TestMethod]
        public void GetSettingReturnsEmptyStringWhenMissing()
        {
            // Documented contract is an empty string; the old code returned null when the key existed.
            Assert.AreEqual(string.Empty, RegistrySettings.GetSetting(BaseKey, _subKey, "Missing"));

            SetRawValue("Other", "x", RegistryValueKind.String);
            Assert.AreEqual(string.Empty, RegistrySettings.GetSetting(BaseKey, _subKey, "Missing"));
            Assert.AreEqual("x", RegistrySettings.GetSetting(BaseKey, _subKey, "Other"));
        }

        [TestMethod]
        public void DeleteSettingRemovesTheValueAndToleratesAMissingValue()
        {
            RegistrySettings.SaveSetting(BaseKey, _subKey, "Value", "x");
            RegistrySettings.DeleteSetting(BaseKey, _subKey, "Value");
            RegistrySettings.DeleteSetting(BaseKey, _subKey, "Value");

            Assert.DoesNotContain("Value", RegistrySettings.GetValueNames(BaseKey, _subKey));
        }
    }
}
