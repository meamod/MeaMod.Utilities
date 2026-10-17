using System;

namespace MeaMod.Utilities.Cryptography
{
    /// <summary>
    /// Legacy Triple DES helpers: ECB mode with a key derived from the MD5 digest of the reversed key text.
    /// Use <see cref="System.Security.Cryptography.TripleDES"/> with a proper key and IV instead.
    /// </summary>
    [Obsolete("TripleDES is deprecated, please use System.Security.Cryptography.TripleDES instead.")]
    public class TripleDES
    {
        /// <summary>Encrypts <paramref name="sIn"/> with a key derived from <paramref name="sKey"/>.</summary>
        /// <param name="sIn">The ASCII text to encrypt.</param>
        /// <param name="sKey">The key text.</param>
        /// <returns>The cipher text as Base64.</returns>
        public static string EncryptTripleDES(string sIn, string sKey)
        {
            if (sIn == null) throw new ArgumentNullException(nameof(sIn));
            if (sKey == null) throw new ArgumentNullException(nameof(sKey));

            using var des = new System.Security.Cryptography.TripleDESCryptoServiceProvider();
            using var hashMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            // scramble the key
            sKey = ScrambleKey(sKey);
            // Compute the MD5 hash.
            des.Key = hashMD5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(sKey));
            // Set the cipher mode.
            des.Mode = System.Security.Cryptography.CipherMode.ECB;
            // Create the encryptor.
            using var encryptor = des.CreateEncryptor();
            // Get a byte array of the string.
            var buffer = System.Text.Encoding.ASCII.GetBytes(sIn);
            // Transform and return the string.
            return Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
        }

        /// <summary>Decrypts Base64 cipher text produced by <see cref="EncryptTripleDES"/>.</summary>
        /// <param name="sOut">The Base64 cipher text.</param>
        /// <param name="sKey">The key text used to encrypt.</param>
        /// <returns>The decrypted ASCII text.</returns>
        public static string DecryptTripleDES(string sOut, string sKey)
        {
            if (sOut == null) throw new ArgumentNullException(nameof(sOut));
            if (sKey == null) throw new ArgumentNullException(nameof(sKey));

            using var des = new System.Security.Cryptography.TripleDESCryptoServiceProvider();
            using var hashMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            // scramble the key
            sKey = ScrambleKey(sKey);
            // Compute the MD5 hash.
            des.Key = hashMD5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(sKey));
            // Set the cipher mode.
            des.Mode = System.Security.Cryptography.CipherMode.ECB;
            // Create the decryptor.
            using var decryptor = des.CreateDecryptor();
            var buffer = Convert.FromBase64String(sOut);
            // Transform and return the string.
            return System.Text.Encoding.ASCII.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
        }

        /// <summary>
        /// Reverses <paramref name="v_strKey"/>.
        /// </summary>
        public static string ScrambleKey(string v_strKey)
        {
            if (v_strKey == null) throw new ArgumentNullException(nameof(v_strKey));

            var chars = v_strKey.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }
    }
}
