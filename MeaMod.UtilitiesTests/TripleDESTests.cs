#pragma warning disable CS0618 // TripleDES is obsolete but still shipped, so it must still work
using MmTripleDES = MeaMod.Utilities.Cryptography.TripleDES;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class TripleDESTests
    {
        [TestMethod]
        public void ScrambleKeyReversesTheKey()
        {
            // The original VB-style loop computed a 1-based index and threw for every non-empty key.
            Assert.AreEqual("dcba", MmTripleDES.ScrambleKey("abcd"));
            Assert.AreEqual("a", MmTripleDES.ScrambleKey("a"));
            Assert.AreEqual(string.Empty, MmTripleDES.ScrambleKey(string.Empty));
        }

        [TestMethod]
        public void EncryptThenDecryptRoundTrips()
        {
            const string plainText = "The quick brown fox";
            const string key = "secret";

            string cipherText = MmTripleDES.EncryptTripleDES(plainText, key);

            Assert.AreNotEqual(plainText, cipherText);
            Assert.AreEqual(plainText, MmTripleDES.DecryptTripleDES(cipherText, key));
        }

        [TestMethod]
        public void DecryptWithWrongKeyDoesNotReturnPlainText()
        {
            const string plainText = "The quick brown fox";
            string cipherText = MmTripleDES.EncryptTripleDES(plainText, "secret");

            string decrypted;
            try
            {
                decrypted = MmTripleDES.DecryptTripleDES(cipherText, "wrong");
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Bad padding is an acceptable outcome for a wrong key.
                return;
            }

            Assert.AreNotEqual(plainText, decrypted);
        }
    }
}
