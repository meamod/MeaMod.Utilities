#pragma warning disable CS0618 // Hash is obsolete but still shipped
using System.Security.Cryptography;
using System.Text;
using MeaMod.Utilities.Cryptography;
using MmHash = MeaMod.Utilities.Cryptography.Hash;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class HashTests
    {
        private const string Input = "abc";
        private const string Sha256OfAbc = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

        private static byte[] Bytes => Encoding.UTF8.GetBytes(Input);

        private static string Hex(byte[] data) => Convert.ToHexString(data).ToLowerInvariant();

        [TestMethod]
        public void ObsoleteHashMd5()
        {
            Assert.AreEqual(Hex(MD5.HashData(Bytes)), MmHash.MD5(Input));
        }

        [TestMethod]
        public void ObsoleteHashSha1()
        {
            Assert.AreEqual(Hex(SHA1.HashData(Bytes)), MmHash.SHA1(Input));
        }

        [TestMethod]
        public void ObsoleteHashSha256()
        {
            Assert.AreEqual(Sha256OfAbc, MmHash.SHA256(Input));
        }

        [TestMethod]
        public void ObsoleteHashSha384()
        {
            Assert.AreEqual(Hex(SHA384.HashData(Bytes)), MmHash.SHA384(Input));
        }

        [TestMethod]
        public void ObsoleteHashSha512()
        {
            Assert.AreEqual(Hex(SHA512.HashData(Bytes)), MmHash.SHA512(Input));
        }

        [TestMethod]
        public void HashingSha256()
        {
            Assert.AreEqual(Sha256OfAbc, Hashing.GetHashSHA256(Input));
        }

        [TestMethod]
        public void HashingSha384()
        {
            Assert.AreEqual(Hex(SHA384.HashData(Bytes)), Hashing.GetHashSHA384(Input));
        }

        [TestMethod]
        public void HashingSha512()
        {
            Assert.AreEqual(Hex(SHA512.HashData(Bytes)), Hashing.GetHashSHA512(Input));
        }

        [TestMethod]
        public void HashingWithAnExplicitAlgorithm()
        {
            using var sha1 = SHA1.Create();

            Assert.AreEqual(Hex(SHA1.HashData(Bytes)), Hashing.GetHash(sha1, Input));
        }

        [TestMethod]
        public void HashingEmptyInput()
        {
            Assert.AreEqual(Hex(SHA256.HashData(Array.Empty<byte>())), Hashing.GetHashSHA256(string.Empty));
        }
    }
}
