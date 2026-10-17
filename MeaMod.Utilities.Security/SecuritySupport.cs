// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using DWORD = uint;

namespace MeaMod.Utilities.Security
{

    /// <summary>
    /// Security Support APIs.
    /// </summary>
    public static class SecuritySupport
    {

        /// <summary>
        /// Throw if file does not exist.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns>Does not return a value.</returns>
        internal static void CheckIfFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
        }

        /// <summary>
        /// Convert an int to a DWORD.
        /// </summary>
        /// <param name="n">Signed int number.</param>
        /// <returns>DWORD.</returns>
        internal static DWORD GetDWORDFromInt(int n)
        {
            UInt32 result = BitConverter.ToUInt32(BitConverter.GetBytes(n), 0);
            return (DWORD)result;
        }

        /// <summary>
        /// Convert a DWORD to int.
        /// </summary>
        /// <param name="n">Number.</param>
        /// <returns>Int.</returns>
        internal static int GetIntFromDWORD(DWORD n)
        {
            Int64 n64 = n - 0x100000000L;
            return (int)n64;
        }
    }
}
