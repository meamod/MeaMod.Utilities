using Microsoft.Win32;
using System;

namespace MeaMod.Utilities
{
    /// <summary>The RegistrySettings class contains methods to access the Windows registry for getting, creating, updating and deleting registry values</summary>
    public class RegistrySettings
    {
        /// <summary>This method deletes a value from to the current user registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of registry value to delete</param>
        public static void DeleteSetting(string baseKey, string appNameSubKey, string valueName)
        {
            DeleteSetting(Registry.CurrentUser, baseKey, appNameSubKey, valueName, nameof(DeleteSetting));
        }

        /// <summary>This method deletes a value from to the local machine registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of registry value to delete</param>
        public static void DeleteSettingLM(string baseKey, string appNameSubKey, string valueName)
        {
            DeleteSetting(Registry.LocalMachine, baseKey, appNameSubKey, valueName, nameof(DeleteSettingLM));
        }

        /// <summary>This method gets <paramref name="valueName"/> as a boolean from the current user registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value to retrieve</param>
        /// <param name="defaultValue">Default value if <paramref name="valueName"/> does not exist</param>
        /// <returns>The boolean from <paramref name="valueName"/> or <paramref name="defaultValue"/> if <paramref name="valueName"/> does not exist</returns>
        public static bool GetBooleanSetting(string baseKey, string appNameSubKey, string valueName, bool defaultValue = false)
        {
            return GetBooleanSetting(Registry.CurrentUser, baseKey, appNameSubKey, valueName, defaultValue, nameof(GetBooleanSetting));
        }

        /// <summary>This method gets <paramref name="valueName"/> as a boolean from the local machine registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value to retrieve</param>
        /// <param name="defaultValue">Default value if <paramref name="valueName"/> does not exist</param>
        /// <returns>The boolean from <paramref name="valueName"/> or <paramref name="defaultValue"/> if <paramref name="valueName"/> does not exist</returns>
        public static bool GetBooleanSettingLM(string baseKey, string appNameSubKey, string valueName, bool defaultValue = false)
        {
            return GetBooleanSetting(Registry.LocalMachine, baseKey, appNameSubKey, valueName, defaultValue, nameof(GetBooleanSettingLM));
        }

        /// <summary>This method gets <paramref name="valueName"/> as a string from the current user registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value to retrieve</param>
        /// <returns>The string value from <paramref name="valueName"/> or empty if <paramref name="valueName"/> does not exist</returns>
        public static string GetSetting(string baseKey, string appNameSubKey, string valueName)
        {
            return GetSetting(Registry.CurrentUser, baseKey, appNameSubKey, valueName, nameof(GetSetting));
        }

        /// <summary>This method gets <paramref name="valueName"/> as a string from the current user registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value to retrieve</param>
        /// <returns>The string value from <paramref name="valueName"/> or empty if <paramref name="valueName"/> does not exist</returns>
        public static string GetSettingLM(string baseKey, string appNameSubKey, string valueName)
        {
            return GetSetting(Registry.LocalMachine, baseKey, appNameSubKey, valueName, nameof(GetSettingLM));
        }

        /// <summary>This method gets <paramref name="valueName"/> as an integer from the current user registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value to retrieve</param>
        /// <returns>The integer value from <paramref name="valueName"/> or 0 if <paramref name="valueName"/> does not exist</returns>
        public static int GetInteger(string baseKey, string appNameSubKey, string valueName)
        {
            return GetInteger(Registry.CurrentUser, baseKey, appNameSubKey, valueName, nameof(GetInteger));
        }

        /// <summary>This method gets <paramref name="valueName"/> as an integer from the local machine registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value to retrieve</param>
        /// <returns>The integer value from <paramref name="valueName"/> or 0 if <paramref name="valueName"/> does not exist</returns>
        public static int GetIntegerLM(string baseKey, string appNameSubKey, string valueName)
        {
            return GetInteger(Registry.LocalMachine, baseKey, appNameSubKey, valueName, nameof(GetIntegerLM));
        }

        /// <summary>This method saves <paramref name="valueName"/> as a boolean to the current user registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value</param>
        /// <param name="value">Value to be saved as boolean</param>
        public static void SaveBooleanSetting(string baseKey, string appNameSubKey, string valueName, bool value)
        {
            SaveValue(Registry.CurrentUser, baseKey, appNameSubKey, valueName, value, RegistryValueKind.String, nameof(SaveBooleanSetting));
        }

        /// <summary>This method saves <paramref name="valueName"/> as a boolean to the local machine registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value</param>
        /// <param name="value">Value to be saved as boolean</param>
        public static void SaveBooleanSettingLM(string baseKey, string appNameSubKey, string valueName, bool value)
        {
            SaveValue(Registry.LocalMachine, baseKey, appNameSubKey, valueName, value, RegistryValueKind.String, nameof(SaveBooleanSettingLM));
        }

        /// <summary>This method saves <paramref name="valueName"/> as a string to the current user registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value</param>
        /// <param name="value">Value to be saved as string</param>
        public static void SaveSetting(string baseKey, string appNameSubKey, string valueName, string value)
        {
            SaveValue(Registry.CurrentUser, baseKey, appNameSubKey, valueName, value, RegistryValueKind.String, nameof(SaveSetting));
        }

        /// <summary>This method saves <paramref name="valueName"/> as a string to the local machine registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value</param>
        /// <param name="value">Value to be saved as string</param>
        public static void SaveSettingLM(string baseKey, string appNameSubKey, string valueName, string value)
        {
            SaveValue(Registry.LocalMachine, baseKey, appNameSubKey, valueName, value, RegistryValueKind.String, nameof(SaveSettingLM));
        }

        /// <summary>This method saves <paramref name="valueName"/> as a integer to the current user registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value</param>
        /// <param name="value">Value to be saved as integer</param>
        public static void SaveInteger(string baseKey, string appNameSubKey, string valueName, int value)
        {
            SaveValue(Registry.CurrentUser, baseKey, appNameSubKey, valueName, value, RegistryValueKind.DWord, nameof(SaveInteger));
        }

        /// <summary>This method saves <paramref name="valueName"/> as a integer to the local machine registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/>\<paramref name="valueName"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey that <paramref name="valueName"/> is located in under <paramref name="baseKey"/></param>
        /// <param name="valueName">Name of value</param>
        /// <param name="value">Value to be saved as integer</param>
        public static void SaveIntegerLM(string baseKey, string appNameSubKey, string valueName, int value)
        {
            SaveValue(Registry.LocalMachine, baseKey, appNameSubKey, valueName, value, RegistryValueKind.DWord, nameof(SaveIntegerLM));
        }

        /// <summary>This method returns all value names within the local machine registry located in HKCU\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKCU</param>
        /// <param name="appNameSubKey">Name of the SubKey to return all value names is located in under <paramref name="baseKey"/></param>
        public static string[] GetValueNames(string baseKey, string appNameSubKey)
        {
            return GetValueNames(Registry.CurrentUser, baseKey, appNameSubKey);
        }

        /// <summary>This method returns all value names within the local machine registry located in HKLM\<paramref name="baseKey"/>\<paramref name="appNameSubKey"/></summary>
        /// <param name="baseKey">Base sub key and should look like SOFTWARE\MeaModGroup\ and will be located in HKLM</param>
        /// <param name="appNameSubKey">Name of the SubKey to return all value names is located in under <paramref name="baseKey"/></param>
        public static string[] GetValueNamesLM(string baseKey, string appNameSubKey)
        {
            return GetValueNames(Registry.LocalMachine, baseKey, appNameSubKey);
        }

        #region Implementation

        private static void DeleteSetting(RegistryKey hive, string baseKey, string appNameSubKey, string valueName, string caller)
        {
            try
            {
                using var registryKey = hive.CreateSubKey(string.Concat(baseKey, appNameSubKey));
                // throwOnMissingValue: false - deleting a value that is already gone is not an error
                registryKey?.DeleteValue(valueName, false);
            }
            catch (Exception exception)
            {
                Console.WriteLine("MeaMod.Utilities.RegistrySettings." + caller + " : Exception" + Environment.NewLine + exception.Message);
                throw;
            }
        }

        private static bool GetBooleanSetting(RegistryKey hive, string baseKey, string appNameSubKey, string valueName, bool defaultValue, string caller)
        {
            try
            {
                using var registryKey = hive.OpenSubKey(string.Concat(baseKey, appNameSubKey), RegistryKeyPermissionCheck.ReadSubTree);
                if (registryKey == null)
                    return defaultValue;

                return ToBoolean(registryKey.GetValue(valueName), defaultValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MeaMod.Utilities.RegistrySettings." + caller + " : Exception" + Environment.NewLine + ex.Message);
                throw;
            }
        }

        private static string GetSetting(RegistryKey hive, string baseKey, string appNameSubKey, string valueName, string caller)
        {
            try
            {
                using var registryKey = hive.OpenSubKey(string.Concat(baseKey, appNameSubKey), RegistryKeyPermissionCheck.ReadSubTree);
                // Documented contract: empty string when the key or value does not exist
                return registryKey?.GetValue(valueName)?.ToString() ?? string.Empty;
            }
            catch (Exception exception)
            {
                Console.WriteLine("MeaMod.Utilities.RegistrySettings." + caller + " : Exception" + Environment.NewLine + exception.Message);
                throw;
            }
        }

        private static int GetInteger(RegistryKey hive, string baseKey, string appNameSubKey, string valueName, string caller)
        {
            try
            {
                using var registryKey = hive.OpenSubKey(string.Concat(baseKey, appNameSubKey), RegistryKeyPermissionCheck.ReadSubTree);
                if (registryKey == null)
                    return 0;

                var rawResult = registryKey.GetValue(valueName);
                if (rawResult == null)
                    return 0;

                return registryKey.GetValueKind(valueName) switch
                {
                    RegistryValueKind.DWord => (int)rawResult,
                    RegistryValueKind.QWord => Convert.ToInt32((long)rawResult),// REG_QWORD is returned boxed as long; unboxing straight to int throws
                    RegistryValueKind.String => int.Parse((string)rawResult),
                    _ => 0,
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine("MeaMod.Utilities.RegistrySettings." + caller + ": Exception" + Environment.NewLine + exception.Message);
                throw;
            }
        }

        private static void SaveValue(RegistryKey hive, string baseKey, string appNameSubKey, string valueName, object value, RegistryValueKind kind, string caller)
        {
            try
            {
                using var registryKey = hive.CreateSubKey(string.Concat(baseKey, appNameSubKey)) ?? throw new UnauthorizedAccessException("Unable to save");
                registryKey.SetValue(valueName, value, kind);
            }
            catch (Exception exception)
            {
                Console.WriteLine("MeaMod.Utilities.RegistrySettings." + caller + " : Exception" + Environment.NewLine + exception.Message);
                throw;
            }
        }

        private static string[] GetValueNames(RegistryKey hive, string baseKey, string appNameSubKey)
        {
            using var registryKey = hive.OpenSubKey(string.Concat(baseKey, appNameSubKey));
            return registryKey != null ? registryKey.GetValueNames() : [];
        }

        /// <summary>
        /// Converts a raw registry value to a boolean. Accepts the "True"/"False" strings written by
        /// <see cref="SaveBooleanSetting"/>, numeric strings, and numeric registry kinds.
        /// </summary>
        private static bool ToBoolean(object raw, bool defaultValue)
        {
            switch (raw)
            {
                case null:
                    return defaultValue;
                case string s:
                    if (bool.TryParse(s, out var parsedBool))
                        return parsedBool;
                    if (long.TryParse(s, out var parsedNumber))
                        return parsedNumber != 0;
                    return defaultValue;
                default:
                    return Convert.ToBoolean(raw);
            }
        }

        #endregion
    }
}
